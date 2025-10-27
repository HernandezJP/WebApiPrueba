using WebApiPrueba.Models.Entities;

namespace WebApiPrueba.Models.Dtos.Usuarios
{
    public class UsuarioResponseDto
    {
        public int IdUsuario { get; set; }
        public string Email { get; set; } = string.Empty;
        public int IdEmpleado { get; set; }
        public int IdRol { get; set; }
        public string? NombreEmpleado { get; set; }
        public string? NombreRol { get; set; }
    }
}
