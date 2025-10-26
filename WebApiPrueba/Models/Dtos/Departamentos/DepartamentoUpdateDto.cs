namespace WebApiPrueba.Models.Dtos.Departamentos
{
    public class DepartamentoUpdateDto
    {
        public int IdDepartamento { get; set; }
        public string NombreDepto { get; set; } = string.Empty;
        public decimal Presupuesto { get; set; }
    }
}

