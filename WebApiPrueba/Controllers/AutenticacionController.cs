using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApiPrueba.Data;
using WebApiPrueba.Models.Dtos.Autenticacion;
using WebApiPrueba.Models.Entities;

namespace WebApiPrueba.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutenticacionController : ControllerBase
    {
        private readonly EmpresaDbContext _db;
        private readonly SymmetricSecurityKey _signingKey;

        public AutenticacionController(EmpresaDbContext db, SymmetricSecurityKey signingKey)
        {
            _db = db;
            _signingKey = signingKey;
        }

        // POST: api/autenticacion/validar
        [AllowAnonymous]
        [HttpPost("validar")]
        public async Task<ActionResult<LoginResponseDto>> Validar([FromBody] LoginRequestDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Contrasena))
                return BadRequest("Credenciales incompletas.");

            var emailNorm = dto.Email.Trim().ToLower();
            var usuario = await _db.Usuarios
                .Include(u => u.Rol)
                .Include(u => u.Empleado)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == emailNorm);

            if (usuario is null)
                return Unauthorized("Usuario o contraseña inválidos.");

            if (usuario.Contrasena != dto.Contrasena)
                return Unauthorized("Usuario o contraseña inválidos.");

            var (token, expira) = EmitirToken(usuario);

            return Ok(new LoginResponseDto
            {
                Email = usuario.Email,
                IdEmpleado = usuario.IdEmpleado,
                NombreEmpleado = usuario.Empleado is null ? null : $"{usuario.Empleado.Nombre} {usuario.Empleado.Apellidos}",
                IdRol = usuario.IdRol,
                RolNombre = usuario.Rol?.NombreRol,
                Token = token,
                Expira = expira
            });
        }

        private (string token, DateTime expira) EmitirToken(Usuario u)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, u.Email),
                new Claim(ClaimTypes.Name, u.Email),
                new Claim("uid", u.IdUsuario.ToString()),
                new Claim(ClaimTypes.Role, u.Rol?.NombreRol ?? "Operador"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var creds = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
            var expira = DateTime.UtcNow.AddHours(2);

            var jwt = new JwtSecurityToken(
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expira,
                signingCredentials: creds
            );

            return (new JwtSecurityTokenHandler().WriteToken(jwt), expira);
        }
    }
}

