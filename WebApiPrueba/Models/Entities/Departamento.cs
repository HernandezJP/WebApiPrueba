namespace WebApiPrueba.Models.Entities
{
    public class Departamento
    {
        public int IdDepartamento { get; set; }  
        public string NombreDepto { get; set; } = string.Empty;
        public decimal Presupuesto { get; set; }

        public ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
    }
}
