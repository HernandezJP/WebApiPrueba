namespace WebApiPrueba.Models.Dtos.Autenticacion
{
    public class LoginResponseDto
    {
        public string Email { get; set; } = string.Empty;
        public int IdEmpleado { get; set; }
        public string? NombreEmpleado { get; set; }
        public int IdRol { get; set; }
        public string? RolNombre { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime Expira { get; set; }
    }
}
