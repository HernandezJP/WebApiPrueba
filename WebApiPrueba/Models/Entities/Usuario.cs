namespace WebApiPrueba.Models.Entities
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;

        public int IdEmpleado { get; set; }
        public Empleado? Empleado { get; set; }

        public int IdRol { get; set; }
        public Rol? Rol { get; set; }
    }
}
