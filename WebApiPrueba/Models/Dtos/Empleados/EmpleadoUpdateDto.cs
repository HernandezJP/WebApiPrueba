using WebApiPrueba.Models.Entities;

namespace WebApiPrueba.Models.Dtos.Empleados
{
    public class EmpleadoUpdateDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string DocumentoCui { get; set; } = string.Empty;
        public DateTime FechaIngreso { get; set; }
        public decimal SalarioActual { get; set; }
        public DateTime? FechaUltimoAumento { get; set; }
        public string? Puesto { get; set; }
        public string? Jerarquia { get; set; }
        public int DepartamentoId { get; set; }
    }
}
