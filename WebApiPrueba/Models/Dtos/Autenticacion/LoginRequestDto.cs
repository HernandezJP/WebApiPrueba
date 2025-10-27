namespace WebApiPrueba.Models.Dtos.Autenticacion
{
    public class LoginRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
    }
}
