namespace WebApiPrueba.Models.Dtos.Reportes
{
    public record KpisDto(
        DateTime Desde, 
        DateTime Hasta, 
        int TotalVentas, 
        decimal Ingreso, 
        decimal Costo,
        decimal Margen, 
        decimal TicketPromedio
    );
}
