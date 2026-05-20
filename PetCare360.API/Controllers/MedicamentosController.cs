using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCare360.API.Data;
using PetCare360.API.Models;

namespace PetCare360.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicamentosController : ControllerBase
    {
        private readonly AppDbContext dbContext;

        public MedicamentosController(AppDbContext _dbContext)
        {
            dbContext = _dbContext;
        }

       
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var medicamentos = await dbContext.Medicamentos.ToListAsync();
            return Ok(medicamentos);
        }

       
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var medicamento = await dbContext.Medicamentos.FindAsync(id);
            if (medicamento == null)
            {
                return NotFound("Medicamento não encontrado.");
            }
            return Ok(medicamento);
        }

       
        [HttpGet("pet/{petId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByPet(int petId)
        {
            var medicamentos = await dbContext.Medicamentos
                .Where(m => m.IdPet == petId)
                .ToListAsync();
            return Ok(medicamentos);
        }

        
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] Medicamento medicamento)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool petExiste = await dbContext.Pets.AnyAsync(p => p.IdPet == medicamento.IdPet);
            if (!petExiste)
            {
                return BadRequest("O pet informado não existe.");
            }

            dbContext.Medicamentos.Add(medicamento);
            await dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = medicamento.IdMedicamento }, medicamento);
        }

        
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] Medicamento medicamentoAtualizado)
        {
            if (id != medicamentoAtualizado.IdMedicamento)
            {
                return BadRequest("O ID da URL não confere com o ID do corpo.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var medicamentoExistente = await dbContext.Medicamentos.FindAsync(id);
            if (medicamentoExistente == null)
            {
                return NotFound("Medicamento não encontrado.");
            }

            medicamentoExistente.NmMedicamento = medicamentoAtualizado.NmMedicamento;
            medicamentoExistente.Dosagem = medicamentoAtualizado.Dosagem;
            medicamentoExistente.Frequencia = medicamentoAtualizado.Frequencia;
            medicamentoExistente.DtInicio = medicamentoAtualizado.DtInicio;
            medicamentoExistente.DtFim = medicamentoAtualizado.DtFim;
            medicamentoExistente.IdPet = medicamentoAtualizado.IdPet;
            medicamentoExistente.IdConsulta = medicamentoAtualizado.IdConsulta;

            await dbContext.SaveChangesAsync();
            return NoContent();
        }

        
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var medicamento = await dbContext.Medicamentos.FindAsync(id);
            if (medicamento == null)
            {
                return NotFound("Medicamento não encontrado.");
            }

            dbContext.Medicamentos.Remove(medicamento);
            await dbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
