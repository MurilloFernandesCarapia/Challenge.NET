using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCare360.API.Data;
using PetCare360.API.Models;

namespace PetCare360.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsultasController : ControllerBase
    {
        private readonly AppDbContext dbContext;

        public ConsultasController(AppDbContext _dbContext)
        {
            dbContext = _dbContext;
        }

        
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var consultas = await dbContext.Consultas.ToListAsync();
            return Ok(consultas);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var consulta = await dbContext.Consultas.FindAsync(id);
            if (consulta == null)
            {
                return NotFound("Consulta não encontrada.");
            }
            return Ok(consulta);
        }

        
        [HttpGet("pet/{petId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByPet(int petId)
        {
            var consultas = await dbContext.Consultas
                .Where(c => c.IdPet == petId)
                .ToListAsync();
            return Ok(consultas);
        }

       
        [HttpGet("clinica/{clinicaId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByClinica(int clinicaId)
        {
            var consultas = await dbContext.Consultas
                .Where(c => c.IdClinica == clinicaId)
                .ToListAsync();
            return Ok(consultas);
        }

        
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] Consulta consulta)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool petExiste = await dbContext.Pets.AnyAsync(p => p.IdPet == consulta.IdPet);
            if (!petExiste)
            {
                return BadRequest("O pet informado não existe.");
            }

            bool clinicaExiste = await dbContext.Clinicas.AnyAsync(c => c.IdClinica == consulta.IdClinica);
            if (!clinicaExiste)
            {
                return BadRequest("A clínica informada não existe.");
            }

            dbContext.Consultas.Add(consulta);
            await dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = consulta.IdConsulta }, consulta);
        }

        
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] Consulta consultaAtualizada)
        {
            if (id != consultaAtualizada.IdConsulta)
            {
                return BadRequest("O ID da URL não confere com o ID do corpo.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var consultaExistente = await dbContext.Consultas.FindAsync(id);
            if (consultaExistente == null)
            {
                return NotFound("Consulta não encontrada.");
            }

            consultaExistente.DtConsulta = consultaAtualizada.DtConsulta;
            consultaExistente.Descricao = consultaAtualizada.Descricao;
            consultaExistente.Diagnostico = consultaAtualizada.Diagnostico;
            consultaExistente.IdPet = consultaAtualizada.IdPet;
            consultaExistente.IdClinica = consultaAtualizada.IdClinica;

            await dbContext.SaveChangesAsync();
            return NoContent();
        }

       
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var consulta = await dbContext.Consultas.FindAsync(id);
            if (consulta == null)
            {
                return NotFound("Consulta não encontrada.");
            }

            dbContext.Consultas.Remove(consulta);
            await dbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
