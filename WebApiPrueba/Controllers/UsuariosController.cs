using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiPrueba.Data;
using WebApiPrueba.Models.Dtos.Roles;
using WebApiPrueba.Models.Dtos.Usuarios;
using WebApiPrueba.Models.Entities;

namespace WebApiPrueba.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly EmpresaDbContext _db;
        public UsuariosController(EmpresaDbContext db) => _db = db;

        //GET: api/Usuarios
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var usuarios = await _db.Usuarios
               .AsNoTracking()
               .Select(u => new UsuarioResponseDto
               {
                   IdUsuario = u.IdUsuario,
                   Email = u.Email,
                   IdEmpleado = u.IdEmpleado,
                   IdRol = u.IdRol
               })
               .ToListAsync();
            return Ok(usuarios);
        }

        //GET: api/Usuarios/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var usuario = await _db.Usuarios
                .AsNoTracking()
                .Where(x => x.IdUsuario == id)
                .Select(u => new UsuarioResponseDto
                {
                    IdUsuario = u.IdUsuario,
                    Email = u.Email,
                    IdEmpleado = u.IdEmpleado,
                    IdRol = u.IdRol
                })
                .FirstOrDefaultAsync();
            return usuario is null ? NotFound(new { mensaje = "No encontrado" }) : Ok(usuario);
        }

        //POST: api/Usuarios    
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UsuarioCreateDto u)
        {
            var email = await _db.Usuarios.AnyAsync(x => x.Email == u.Email);
            if (email) return Conflict(new { mensaje = "El email ya existe" });

            var empleado = await _db.Empleados.AnyAsync(e => e.IdEmpleado == u.IdEmpleado);
            if (!empleado) return BadRequest(new { mensaje = "Empleado no válido" });

            var rol = await _db.Roles.AnyAsync(r => r.IdRol == u.IdRol);
            if (!rol) return BadRequest(new { mensaje = "Rol no válido" });

            var usuario = new Usuario
            {
                Email = u.Email,
                Contrasena = u.Contrasena,
                IdEmpleado = u.IdEmpleado,
                IdRol = u.IdRol
            };

            _db.Usuarios.Add(usuario);
            await _db.SaveChangesAsync();

            var usuarioResponse = new UsuarioResponseDto
            {
                Email = usuario.Email,
                IdEmpleado = usuario.IdEmpleado,
                NombreEmpleado = usuario.Empleado != null ? usuario.Empleado.Nombre + " " + usuario.Empleado.Apellidos : string.Empty,
                IdRol = usuario.IdRol,
                NombreRol = usuario.Rol != null ? usuario.Rol.NombreRol : string.Empty

            };
            return CreatedAtAction(nameof(GetOne), new { id = usuario.IdUsuario }, usuario);
        }

        //PUT: api/Usuarios/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UsuarioUpdateDto u)
        {
            var usuario = await _db.Usuarios.FindAsync(id);
            if (usuario is null) return NotFound(new { mensaje = "No encontrado" });

            // validar duplicado por email
            var email = await _db.Usuarios.AnyAsync(x => x.Email == u.Email && x.IdUsuario != id);
            if (email) return Conflict(new { mensaje = "El email ya existe" });

            usuario.Email = u.Email;
            usuario.Contrasena = u.Contrasena;
            usuario.IdRol = u.IdRol;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        //DELETE: api/Usuarios/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var usuario = await _db.Usuarios.FindAsync(id);
            if (usuario is null) return NotFound(new { mensaje = "No encontrado" });
            

            try
            {
                _db.Usuarios.Remove(usuario);
                await _db.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException)
            {
                // FK: hay empleados relacionados
                return Conflict(new { mensaje = "No se puede eliminar el usuario" });
            }
        }
    }
}
