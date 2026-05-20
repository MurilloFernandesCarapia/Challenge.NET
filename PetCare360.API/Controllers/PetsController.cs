using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCare360.API.Data;
using PetCare360.API.Models;

namespace PetCare360.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PetsController : ControllerBase
    {
        private readonly AppDbContext dbContext;

        public PetsController(AppDbContext _dbContext)
        {
            dbContext = _dbContext;
        }

       
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var pets = await dbContext.Pets.ToListAsync();
            return Ok(pets);
        }

        
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var pet = await dbContext.Pets.FindAsync(id);
            if (pet == null)
            {
                return NotFound("Pet não encontrado.");
            }
            return Ok(pet);
        }

       
        [HttpGet("tutor/{tutorId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByTutor(int tutorId)
        {
            var pets = await dbContext.Pets
                .Where(p => p.IdTutor == tutorId)
                .ToListAsync();
            return Ok(pets);
        }

       
        [HttpGet("especie/{especie}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByEspecie(string especie)
        {
            var pets = await dbContext.Pets
                .Where(p => p.Especie.ToLower() == especie.ToLower())
                .ToListAsync();
            return Ok(pets);
        }

       
        [HttpGet("{id}/historico")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetHistorico(int id)
        {
            var pet = await dbContext.Pets
                .Include(p => p.Consultas)
                .Include(p => p.Vacinas)
                .Include(p => p.Medicamentos)
                .FirstOrDefaultAsync(p => p.IdPet == id);

            if (pet == null)
            {
                return NotFound("Pet não encontrado.");
            }

            return Ok(pet);
        }

       
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] Pet pet)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            bool tutorExiste = await dbContext.Tutores.AnyAsync(t => t.IdTutor == pet.IdTutor);
            if (!tutorExiste)
            {
                return BadRequest("O tutor informado não existe.");
            }

            dbContext.Pets.Add(pet);
            await dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = pet.IdPet }, pet);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] Pet petAtualizado)
        {
            if (id != petAtualizado.IdPet)
            {
                return BadRequest("O ID da URL não confere com o ID do corpo.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var petExistente = await dbContext.Pets.FindAsync(id);
            if (petExistente == null)
            {
                return NotFound("Pet não encontrado.");
            }

            petExistente.NmPet = petAtualizado.NmPet;
            petExistente.Especie = petAtualizado.Especie;
            petExistente.Raca = petAtualizado.Raca;
            petExistente.DtNascimento = petAtualizado.DtNascimento;
            petExistente.Peso = petAtualizado.Peso;
            petExistente.IdTutor = petAtualizado.IdTutor;

            await dbContext.SaveChangesAsync();
            return NoContent();
        }

      
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var pet = await dbContext.Pets.FindAsync(id);
            if (pet == null)
            {
                return NotFound("Pet não encontrado.");
            }

            dbContext.Pets.Remove(pet);
            await dbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
