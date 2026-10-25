using Microsoft.AspNetCore.Mvc;
using personapi_dotnet.Interfaces;
using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Controllers.Api
{
    // endpoints REST de persona (salen en swagger)
    [Route("api/personas")]
    [ApiController]
    public class PersonaApiController : ControllerBase
    {
        private readonly IPersonaRepository _repo;

        public PersonaApiController(IPersonaRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Persona>>> GetAll()
        {
            return Ok(await _repo.GetAllAsync());
        }

        [HttpGet("{cc}")]
        public async Task<ActionResult<Persona>> Get(int cc)
        {
            var persona = await _repo.GetByIdAsync(cc);
            if (persona == null)
            {
                return NotFound();
            }
            return Ok(persona);
        }

        [HttpPost]
        public async Task<ActionResult<Persona>> Create(Persona persona)
        {
            if (await _repo.GetByIdAsync(persona.Cc) != null)
            {
                return Conflict("Ya existe una persona con esa cedula");
            }
            await _repo.AddAsync(persona);
            return CreatedAtAction(nameof(Get), new { cc = persona.Cc }, persona);
        }

        [HttpPut("{cc}")]
        public async Task<IActionResult> Update(int cc, Persona persona)
        {
            if (cc != persona.Cc)
            {
                return BadRequest("La cedula de la url no coincide con la del body");
            }
            if (await _repo.GetByIdAsync(cc) == null)
            {
                return NotFound();
            }
            await _repo.UpdateAsync(persona);
            return NoContent();
        }

        [HttpDelete("{cc}")]
        public async Task<IActionResult> Delete(int cc)
        {
            if (await _repo.GetByIdAsync(cc) == null)
            {
                return NotFound();
            }
            await _repo.DeleteAsync(cc);
            return NoContent();
        }
    }
}
