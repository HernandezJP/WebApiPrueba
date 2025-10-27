namespace WebApiPrueba.Models.Dtos.Reportes
{
    public record VentaRowDto(
        int IdVenta,
        int IdCliente,
        DateTime? Fecha,
        decimal Monto,
        decimal Ingreso,
        decimal Costo,
        decimal Margen
    );

    public record VentasListadoDto(
        string Nit,
        DateTime Desde,
        DateTime Hasta,
        int Page,
        int PageSize,
        int Total,
        IEnumerable<VentaRowDto> Rows
    );
}
