using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using WebApiPrueba.Data;
using WebApiPrueba.Models.Dtos.Reportes;

namespace WebApiPrueba.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportesController : ControllerBase
    {
        private readonly EmpresaDbContext _db;
        public ReportesController(EmpresaDbContext db) => _db = db;

        private static (DateTime d1, DateTime d2) Rango(DateTime? desde, DateTime? hasta)
        {
            var d = (desde ?? DateTime.Today.AddMonths(-1)).Date;
            var h = (hasta ?? DateTime.Today).Date.AddDays(1).AddTicks(-1);
            return (d, h);
        }

        // GET api/reportes/kpis
        [HttpGet("kpis")]
        public async Task<ActionResult<KpisDto>> GetKpis([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            var (d1, d2) = Rango(desde, hasta);

            var q = from v in _db.Ventas.AsNoTracking()
                    join dv in _db.Detalles.AsNoTracking() on v.IdVenta equals dv.IdVenta
                    where v.Fecha >= d1 && v.Fecha <= d2
                    select new { v.IdVenta, Ingreso = (decimal)dv.Cantidad * dv.Precio, Costo = (decimal)dv.Cantidad * dv.Costo };

            var porVenta = await q.GroupBy(x => x.IdVenta)
                                  .Select(g => new { IdVenta = g.Key, Ingreso = g.Sum(y => y.Ingreso), Costo = g.Sum(y => y.Costo) })
                                  .ToListAsync();

            var totalVentas = porVenta.Count;
            var ingreso = porVenta.Sum(x => x.Ingreso);
            var costo = porVenta.Sum(x => x.Costo);
            var margen = ingreso - costo;
            var ticket = totalVentas > 0 ? ingreso / totalVentas : 0m;

            return Ok(new KpisDto(d1, d2, totalVentas, Math.Round(ingreso, 2), Math.Round(costo, 2), Math.Round(margen, 2), Math.Round(ticket, 2)));
        }

        // GET api/reportes/cliente/{nit}
        [HttpGet("cliente/{nit}")]
        public async Task<ActionResult<ClienteDetalleDto>> GetClienteDetalle(
            [FromRoute] string nit, [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            var (d1, d2) = Rango(desde, hasta);

            // 1) Agregar por VENTA del NIT (trae totales por factura + fecha y items)
            var ventasCliente =
                from v in _db.Ventas.AsNoTracking()
                join c in _db.Clientes.AsNoTracking() on v.IdCliente equals c.IdCliente
                join dv in _db.Detalles.AsNoTracking() on v.IdVenta equals dv.IdVenta
                where c.Nit == nit && v.Fecha >= d1 && v.Fecha <= d2
                group dv by new { v.IdVenta, Fecha = v.Fecha!.Value.Date } into g1
                select new
                {
                    g1.Key.IdVenta,
                    g1.Key.Fecha,
                    IngresoVenta = g1.Sum(x => (decimal)x.Cantidad * x.Precio),
                    CostoVenta = g1.Sum(x => (decimal)x.Cantidad * x.Costo),
                    Items = g1.Sum(x => x.Cantidad)
                };

            // 2) KPIs del cliente (a partir de ventas ya agregadas)
            var k = await (
                from x in ventasCliente
                group x by 1 into g
                select new
                {
                    Ventas = g.Count(),
                    Ingreso = g.Sum(y => y.IngresoVenta),
                    Costo = g.Sum(y => y.CostoVenta)
                }
            ).FirstOrDefaultAsync() ?? new { Ventas = 0, Ingreso = 0m, Costo = 0m };

            var resumen = new ClienteKpisDto(
                nit,
                k.Ventas,
                Math.Round(k.Ingreso, 2),
                Math.Round(k.Costo, 2),
                Math.Round(k.Ingreso - k.Costo, 2),
                k.Ventas > 0 ? Math.Round(k.Ingreso / k.Ventas, 2) : 0m
            );

            // 3) Serie diaria (reagrupando por fecha)
            var serie = await (
                from x in ventasCliente
                group x by x.Fecha into g
                orderby g.Key
                select new SerieDiariaDto(
                    g.Key,
                    g.Sum(y => y.IngresoVenta),
                    g.Sum(y => y.CostoVenta),
                    g.Sum(y => y.IngresoVenta) - g.Sum(y => y.CostoVenta),
                    g.Sum(y => y.Items)
                )
            ).ToListAsync();

            return Ok(new ClienteDetalleDto(resumen, serie));
        }
        
        // GET api/reportes/cliente/{nit}/ventas
        [HttpGet("cliente/{nit}/ventas")]
        public async Task<ActionResult<VentasListadoDto>> GetVentasDeCliente([FromRoute] string nit, [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var (d1, d2) = Rango(desde, hasta);
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var baseQ = from v in _db.Ventas.AsNoTracking()
                        join c in _db.Clientes.AsNoTracking() on v.IdCliente equals c.IdCliente
                        where c.Nit == nit && v.Fecha >= d1 && v.Fecha <= d2
                        orderby v.IdVenta
                        select new { v.IdVenta, v.IdCliente, v.Fecha, v.Monto };

            var total = await baseQ.CountAsync();
            var ventas = await baseQ.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            var ids = ventas.Select(x => x.IdVenta).ToList();

            var totales = await (from dv in _db.Detalles.AsNoTracking()
                                 where ids.Contains(dv.IdVenta)
                                 group dv by dv.IdVenta into g
                                 select new
                                 {
                                     IdVenta = g.Key,
                                     ingreso = g.Sum(x => (decimal)x.Cantidad * x.Precio),
                                     costo = g.Sum(x => (decimal)x.Cantidad * x.Costo)
                                 }).ToListAsync();
            var map = totales.ToDictionary(x => x.IdVenta, x => x);

            var rows = ventas.Select(v => new VentaRowDto(
                v.IdVenta, v.IdCliente, v.Fecha, v.Monto,
                Math.Round(map.TryGetValue(v.IdVenta, out var t) ? t.ingreso : 0m, 2),
                Math.Round(map.TryGetValue(v.IdVenta, out var t2) ? t2.costo : 0m, 2),
                Math.Round((map.TryGetValue(v.IdVenta, out var t3) ? t3.ingreso - t3.costo : 0m), 2)
            ));

            return Ok(new VentasListadoDto(nit, d1, d2, page, pageSize, total, rows));
        }
    }
}
