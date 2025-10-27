namespace WebApiPrueba.Models.Dtos.Reportes
{
    public record SerieDiariaDto(
         DateTime Fecha,
         decimal Ingreso,
         decimal Costo,
         decimal Margen,
         int Items
    );

    public record ClienteKpisDto(
        string Nit,
        int Ventas,
        decimal Ingreso,
        decimal Costo,
        decimal Margen,
        decimal TicketPromedio
    );

    public record ClienteDetalleDto(
        ClienteKpisDto Resumen,
        IEnumerable<SerieDiariaDto> Serie
    );
}
