using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiPrueba.Data;
using WebApiPrueba.Models.Dtos.Empleados;
using WebApiPrueba.Models.Entities;

namespace WebApiPrueba.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmpleadosController : ControllerBase
    {
        private readonly EmpresaDbContext _db;
        public EmpleadosController(EmpresaDbContext db) => _db = db;

        //GET: api/Empleados
        [Authorize(Policy = "AdminUOperador")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var empleado = await _db.Empleados
               .AsNoTracking()
               .Select(e => new EmpleadoResponseDto
               {
                   IdEmpleado = e.IdEmpleado,
                   Nombre = e.Nombre,
                   Apellidos = e.Apellidos,
                   DocumentoCui = e.DocumentoCui,
                   FechaIngreso = e.FechaIngreso,
                   SalarioActual = e.SalarioActual,
                   FechaUltimoAumento = e.FechaUltimoAumento,
                   Puesto = e.Puesto,
                   Jerarquia = e.Jerarquia,
                   NombreDepartamento = e.Departamento != null ? e.Departamento.NombreDepto : null
               })
               .ToListAsync();

            return Ok(empleado);

        }

        //GET: api/Empleados/buscar-por-cui/{cui}
        [Authorize(Policy = "AdminUOperador")]
        [HttpGet("buscar-por-cui/{cui}")]
        public async Task<IActionResult> GetOne(string cui)
        {
            var empleado = await _db.Empleados
                .AsNoTracking()
                .Where(x => x.DocumentoCui == cui)
                .Select(e => new EmpleadoResponseDto
                {
                    IdEmpleado = e.IdEmpleado,
                    Nombre = e.Nombre,
                    Apellidos = e.Apellidos,
                    DocumentoCui = e.DocumentoCui,
                    FechaIngreso = e.FechaIngreso,
                    SalarioActual = e.SalarioActual,
                    FechaUltimoAumento = e.FechaUltimoAumento,
                    Puesto = e.Puesto,
                    Jerarquia = e.Jerarquia,
                    DepartamentoId = e.DepartamentoId,
                    NombreDepartamento = e.Departamento != null ? e.Departamento.NombreDepto : null
                })
                .FirstOrDefaultAsync();

            return empleado is null ? NotFound(new { mensaje = "No encontrado" }) : Ok(empleado);
        }

        //POST: api/Empleados
        [Authorize(Policy = "AdminUOperador")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EmpleadoCreateDto e)
        {
            //Validar que el CUI no esté duplicado
            var existeCui = await _db.Empleados.AnyAsync(x => x.DocumentoCui == e.DocumentoCui);
            if (existeCui)
                return Conflict(new { mensaje = "Ya existe un empleado con ese CUI" });

            //Validar que el departamento exista
            var deptoExiste = await _db.Departamentos.AnyAsync(x => x.IdDepartamento == e.DepartamentoId);
            if (!deptoExiste)
                return BadRequest(new { mensaje = "Departamento no válido" });

            var empleado = new Empleado
            {
                Nombre = e.Nombre,
                Apellidos = e.Apellidos,
                DocumentoCui = e.DocumentoCui,
                FechaIngreso = e.FechaIngreso,
                SalarioActual = e.SalarioActual,
                FechaUltimoAumento = e.FechaUltimoAumento,
                Puesto = e.Puesto,
                Jerarquia = e.Jerarquia,
                DepartamentoId = e.DepartamentoId
            };

            _db.Empleados.Add(empleado);
            await _db.SaveChangesAsync();

            var resp = new EmpleadoResponseDto
            {
                IdEmpleado = empleado.IdEmpleado,
                Nombre = empleado.Nombre,
                Apellidos = empleado.Apellidos,
                DocumentoCui = empleado.DocumentoCui,
                FechaIngreso = empleado.FechaIngreso,
                SalarioActual = empleado.SalarioActual,
                FechaUltimoAumento = empleado.FechaUltimoAumento,
                Puesto = empleado.Puesto,
                Jerarquia = empleado.Jerarquia,
                DepartamentoId = empleado.DepartamentoId,
                NombreDepartamento = await _db.Departamentos
                    .Where(d => d.IdDepartamento == empleado.DepartamentoId)
                    .Select(d => d.NombreDepto)
                    .FirstOrDefaultAsync()
            };

            
            return CreatedAtAction(nameof(GetOne), new { cui = empleado.DocumentoCui }, resp);
        }

        //PUT: api/Empleados/5
        [Authorize(Policy = "AdminUOperador")]
        [HttpPut("{cui}")]
        public async Task<IActionResult> Update(string cui, [FromBody] EmpleadoUpdateDto e)
        {
            //validar que el empleado exista
            var empleado = await _db.Empleados.FirstOrDefaultAsync(x => x.DocumentoCui == cui);
            if (empleado is null)
                return NotFound(new { mensaje = "Empleado no encontrado" });

            //si se quiere cambiar el cui
            var existeCui = await _db.Empleados.AnyAsync(x => x.IdEmpleado != empleado.IdEmpleado && x.DocumentoCui == e.DocumentoCui);
            if (existeCui)
                return Conflict(new { mensaje = "Ya existe otro empleado con ese CUI" });

            //Validar que el departamento exista
            var deptoExiste = await _db.Departamentos.AnyAsync(x => x.IdDepartamento == e.DepartamentoId);
            if (!deptoExiste)
                return BadRequest(new { mensaje = "Departamento no válido" });

            //Actualizar solo los campos permitidos
            empleado.Nombre = e.Nombre;
            empleado.Apellidos = e.Apellidos;
            empleado.DocumentoCui = e.DocumentoCui;
            empleado.FechaIngreso = e.FechaIngreso;
            empleado.SalarioActual = e.SalarioActual;
            empleado.FechaUltimoAumento = e.FechaUltimoAumento;
            empleado.Puesto = e.Puesto;
            empleado.Jerarquia = e.Jerarquia;
            empleado.DepartamentoId = e.DepartamentoId;

            await  _db.SaveChangesAsync();
            return NoContent();
        }

        //DELETE: api/Empleados/5
        [Authorize(Policy = "SoloAdmin")]
        [HttpDelete("{cui}")]
        public async Task<IActionResult> Delete(string cui)
        {
            var empleado = await _db.Empleados.FirstOrDefaultAsync(x => x.DocumentoCui == cui);
            if (empleado is null) return NotFound(new { mensaje = "Empleado no encontrado" });
            try
            {
                _db.Empleados.Remove(empleado);
                await _db.SaveChangesAsync();
                return NoContent();
            }catch (DbUpdateException)
            {
                return Conflict(new { mensaje = "No se puede eliminar el empleado" });
            }
        }


    }
}
