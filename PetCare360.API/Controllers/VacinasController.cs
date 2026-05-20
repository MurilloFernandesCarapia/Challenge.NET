using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCare360.API.Data;
using PetCare360.API.Models;

namespace PetCare360.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VacinasController : ControllerBase
    {
        private readonly AppDbContext dbContext;

        public VacinasController(AppDbContext _dbContext)
        {
            dbContext = _dbContext;
        }

      
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var vacinas = await dbContext.Vacinas.ToListAsync();
            return Ok(vacinas);
        }

       
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var vacina = await dbContext.Vacinas.FindAsync(id);
            if (vacina == null)
            {
                return NotFound("Vacina não encontrada.");
            }
            return Ok(vacina);
        }

        
        [HttpGet("pet/{petId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByPet(int petId)
        {
            var vacinas = await dbContext.Vacinas
                .Where(v => v.IdPet == petId)
                .ToListAsync();
            return Ok(vacinas);
        }

       
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] Vacina vacina)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool petExiste = await dbContext.Pets.AnyAsync(p => p.IdPet == vacina.IdPet);
            if (!petExiste)
            {
                return BadRequest("O pet informado não existe.");
            }

            dbContext.Vacinas.Add(vacina);
            await dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = vacina.IdVacina }, vacina);
        }

        
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] Vacina vacinaAtualizada)
        {
            if (id != vacinaAtualizada.IdVacina)
            {
                return BadRequest("O ID da URL não confere com o ID do corpo.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var vacinaExistente = await dbContext.Vacinas.FindAsync(id);
            if (vacinaExistente == null)
            {
                return NotFound("Vacina não encontrada.");
            }

            vacinaExistente.NmVacina = vacinaAtualizada.NmVacina;
            vacinaExistente.Fabricante = vacinaAtualizada.Fabricante;
            vacinaExistente.DtAplicacao = vacinaAtualizada.DtAplicacao;
            vacinaExistente.DtProximaDose = vacinaAtualizada.DtProximaDose;
            vacinaExistente.IdPet = vacinaAtualizada.IdPet;
            vacinaExistente.IdConsulta = vacinaAtualizada.IdConsulta;

            await dbContext.SaveChangesAsync();
            return NoContent();
        }

       
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var vacina = await dbContext.Vacinas.FindAsync(id);
            if (vacina == null)
            {
                return NotFound("Vacina não encontrada.");
            }

            dbContext.Vacinas.Remove(vacina);
            await dbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
