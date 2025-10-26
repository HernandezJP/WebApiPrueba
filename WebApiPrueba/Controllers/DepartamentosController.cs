using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiPrueba.Data;
using WebApiPrueba.Models.Entities;
using WebApiPrueba.Models.Dtos.Departamentos;


namespace WebApiPrueba.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartamentosController : ControllerBase
    {
        private readonly EmpresaDbContext _db;
        public DepartamentosController(EmpresaDbContext db) => _db = db;

        //GET: api/Departamentos
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var departamento = await _db.Departamentos
                .AsNoTracking()
                .Select(d => new DepartamentoResponseDto
                {
                    IdDepartamento = d.IdDepartamento,
                    NombreDepto = d.NombreDepto,
                    Presupuesto = d.Presupuesto
                })
                .ToListAsync();

            return Ok(departamento);
        }

        //GET: api/Departamentos/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var departamento = await _db.Departamentos
                .AsNoTracking()
                .Where(x => x.IdDepartamento == id)
                .Select(d => new DepartamentoResponseDto
                {
                    IdDepartamento = d.IdDepartamento,
                    NombreDepto = d.NombreDepto,
                    Presupuesto = d.Presupuesto
                })
                .FirstOrDefaultAsync();

            return departamento is null ? NotFound(new { mensaje = "No encontrado" }) : Ok(departamento);
        }

        //POST: api/Departamentos
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DepartamentoCreateDto d)
        {
            // validar duplicado por nombre
            var nameTaken = await _db.Departamentos.AnyAsync(x => x.NombreDepto == d.NombreDepto);
            if (nameTaken) return Conflict(new { mensaje = "El nombre ya existe" });

            var entity = new Departamento
            {
                IdDepartamento = d.IdDepartamento,
                NombreDepto = d.NombreDepto,
                Presupuesto = d.Presupuesto
            };

            _db.Departamentos.Add(entity);
            await _db.SaveChangesAsync();

            var resp = new DepartamentoResponseDto
            {
                IdDepartamento = entity.IdDepartamento,
                NombreDepto = entity.NombreDepto,
                Presupuesto = entity.Presupuesto
            };

            return CreatedAtAction(nameof(GetOne), new { id = entity.IdDepartamento }, resp);
        }

        //PUT: api/Departamentos/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] DepartamentoUpdateDto d)
        {
            var entity = await _db.Departamentos.FirstOrDefaultAsync(x => x.IdDepartamento == id);
            if (entity is null) return NotFound(new { mensaje = "No encontrado" });

            // validar duplicado por nombre (excluyendo el mismo id)
            var nameTaken = await _db.Departamentos
                .AnyAsync(x => x.IdDepartamento != id && x.NombreDepto == d.NombreDepto);
            if (nameTaken) return Conflict(new { mensaje = "El nombre ya existe" });

            // actualizar solo campos permitidos
            entity.NombreDepto = d.NombreDepto;
            entity.Presupuesto = d.Presupuesto;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var departamento = await _db.Departamentos.FindAsync(id);
            if (departamento is null) return NotFound(new { mensaje = "No encontrado" });

            try
            {
                _db.Departamentos.Remove(departamento);
                await _db.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException)
            {
                // FK: hay empleados relacionados
                return Conflict(new { mensaje = "No se puede eliminar: hay empleados vinculados" });
            }
        }
    }
}
