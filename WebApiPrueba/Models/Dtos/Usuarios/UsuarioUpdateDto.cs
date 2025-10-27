namespace WebApiPrueba.Models.Dtos.Usuarios
{
    public class UsuarioUpdateDto
    {
        public string Email { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public int IdRol { get; set; }
    }
}
