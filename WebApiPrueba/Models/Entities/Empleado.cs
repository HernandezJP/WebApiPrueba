namespace WebApiPrueba.Models.Entities
{
    public class Empleado
    {
        public int IdEmpleado { get; set; }                 
        public string Nombre { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string DocumentoCui { get; set; } = string.Empty;
        public DateTime FechaIngreso { get; set; }
        public decimal SalarioActual { get; set; }
        public DateTime? FechaUltimoAumento { get; set; }
        public string? Puesto { get; set; }
        public string? Jerarquia { get; set; }

        public int DepartamentoId { get; set; }
        public Departamento? Departamento { get; set; }

        public ICollection<Usuario>? Usuarios { get; set; }
    }
}
