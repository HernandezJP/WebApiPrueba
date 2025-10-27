using WebApiPrueba.Models.Entities;

namespace WebApiPrueba.Models.Dtos.Usuarios
{
    public class UsuarioCreateDto
    {
        public string Email { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;

        public int IdEmpleado { get; set; }

        public int IdRol { get; set; }
    }
}
