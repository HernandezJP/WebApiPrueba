using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiPrueba.Data;
using WebApiPrueba.Models.Dtos.Roles;
using WebApiPrueba.Models.Entities;


namespace WebApiPrueba.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly EmpresaDbContext _db;
        public RolesController(EmpresaDbContext db) => _db = db;

        //GET: api/Roles
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var rol = await _db.Roles
               .AsNoTracking()
               .Select(r => new RolResponseDto
               {
                   IdRol = r.IdRol,
                   NombreRol = r.NombreRol
               })
               .ToListAsync();
            return Ok(rol);
        }

        //GET: api/Roles/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var rol = await _db.Roles
                .AsNoTracking()
                .Where(x => x.IdRol == id)
                .Select(r => new RolResponseDto
                {
                    IdRol = r.IdRol,
                    NombreRol = r.NombreRol
                })
                .FirstOrDefaultAsync();
            return rol is null ? NotFound(new { mensaje = "No encontrado" }) : Ok(rol);
        }

        //POST: api/Roles
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RolCreateDto r)
        {
            var rol = await _db.Roles.AnyAsync(x => x.NombreRol == r.NombreRol);
            if (rol) return Conflict(new { mensaje = "El nombre ya existe" });

            var Role = new Rol
            {
                NombreRol = r.NombreRol
            };

            _db.Roles.Add(Role);
            await _db.SaveChangesAsync();

            var roleResponse = new RolResponseDto
            {
                IdRol = Role.IdRol,
                NombreRol = Role.NombreRol
            };
            return CreatedAtAction(nameof(GetOne), new { id = Role.IdRol }, roleResponse);
        }

        //PUT: api/Roles/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] RolCreateDto r)
        {
            // Verificar si el rol existe
            var rol = await _db.Roles.FirstOrDefaultAsync(x => x.IdRol == id);
            if (rol is null) return NotFound(new { mensaje = "No encontrado" });

            // Verificar si el nuevo nombre ya está en uso por otro rol
            var nameTaken = await _db.Roles.AnyAsync(x => x.NombreRol == r.NombreRol && x.IdRol != id);
            if (nameTaken) return Conflict(new { mensaje = "El nombre ya existe" });

            rol.NombreRol = r.NombreRol;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        //DELETE: api/Roles/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var rol  = await _db.Roles.FindAsync(id);
            if (rol is null) return NotFound(new { mensaje = "No encontrado" });

            try
            {
                _db.Roles.Remove(rol);
                await _db.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException)
            {
                // FK: hay empleados relacionados
                return Conflict(new { mensaje = "No se puede eliminar rol" });
            }
        }
    }
}
