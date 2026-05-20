using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCare360.API.Data;
using PetCare360.API.Models;

namespace PetCare360.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TutoresController : ControllerBase
    {
        private readonly AppDbContext dbContext;

        public TutoresController(AppDbContext _dbContext)
        {
            dbContext = _dbContext;
        }

       
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var tutores = await dbContext.Tutores.ToListAsync();
            return Ok(tutores);
        }

      
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var tutor = await dbContext.Tutores.FindAsync(id);

            if (tutor == null)
            {
                return NotFound("Tutor não encontrado.");
            }

            return Ok(tutor);
        }

       
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] Tutor tutor)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            dbContext.Tutores.Add(tutor);
            await dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = tutor.IdTutor }, tutor);
        }

       
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] Tutor tutorAtualizado)
        {
            if (id != tutorAtualizado.IdTutor)
            {
                return BadRequest("O ID da URL não confere com o ID do corpo da requisição.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tutorExistente = await dbContext.Tutores.FindAsync(id);
            if (tutorExistente == null)
            {
                return NotFound("Tutor não encontrado.");
            }


            tutorExistente.NmTutor = tutorAtualizado.NmTutor;
            tutorExistente.Cpf = tutorAtualizado.Cpf;
            tutorExistente.Email = tutorAtualizado.Email;
            tutorExistente.Telefone = tutorAtualizado.Telefone;
            tutorExistente.Endereco = tutorAtualizado.Endereco;

            await dbContext.SaveChangesAsync();

            return NoContent();
        }

        
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var tutor = await dbContext.Tutores.FindAsync(id);

            if (tutor == null)
            {
                return NotFound("Tutor não encontrado.");
            }

            dbContext.Tutores.Remove(tutor);
            await dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
