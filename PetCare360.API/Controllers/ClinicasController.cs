using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCare360.API.Data;
using PetCare360.API.Models;

namespace PetCare360.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClinicasController : ControllerBase
    {
        private readonly AppDbContext dbContext;

        public ClinicasController(AppDbContext _dbContext)
        {
            dbContext = _dbContext;
        }

        
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var clinicas = await dbContext.Clinicas.ToListAsync();
            return Ok(clinicas);
        }

       
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var clinica = await dbContext.Clinicas.FindAsync(id);
            if (clinica == null)
            {
                return NotFound("Clínica não encontrada.");
            }
            return Ok(clinica);
        }

        
        [HttpGet("cnpj/{cnpj}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByCnpj(string cnpj)
        {
            var clinica = await dbContext.Clinicas.FirstOrDefaultAsync(c => c.Cnpj == cnpj);
            if (clinica == null)
            {
                return NotFound("Clínica não encontrada.");
            }
            return Ok(clinica);
        }

        
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] Clinica clinica)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            dbContext.Clinicas.Add(clinica);
            await dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = clinica.IdClinica }, clinica);
        }

       
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] Clinica clinicaAtualizada)
        {
            if (id != clinicaAtualizada.IdClinica)
            {
                return BadRequest("O ID da URL não confere com o ID do corpo.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var clinicaExistente = await dbContext.Clinicas.FindAsync(id);
            if (clinicaExistente == null)
            {
                return NotFound("Clínica não encontrada.");
            }

            clinicaExistente.NmClinica = clinicaAtualizada.NmClinica;
            clinicaExistente.Cnpj = clinicaAtualizada.Cnpj;
            clinicaExistente.Endereco = clinicaAtualizada.Endereco;
            clinicaExistente.Telefone = clinicaAtualizada.Telefone;
            clinicaExistente.Email = clinicaAtualizada.Email;

            await dbContext.SaveChangesAsync();
            return NoContent();
        }

        
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var clinica = await dbContext.Clinicas.FindAsync(id);
            if (clinica == null)
            {
                return NotFound("Clínica não encontrada.");
            }

            dbContext.Clinicas.Remove(clinica);
            await dbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
