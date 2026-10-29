using Microsoft.AspNetCore.Mvc;
using personapi_dotnet.Interfaces;
using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Controllers.Api
{
    [Route("api/profesiones")]
    [ApiController]
    public class ProfesionApiController : ControllerBase
    {
        private readonly IProfesionRepository _repo;

        public ProfesionApiController(IProfesionRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Profesion>>> GetAll()
        {
            return Ok(await _repo.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Profesion>> Get(int id)
        {
            var profesion = await _repo.GetByIdAsync(id);
            if (profesion == null)
            {
                return NotFound();
            }
            return Ok(profesion);
        }

        [HttpPost]
        public async Task<ActionResult<Profesion>> Create(Profesion profesion)
        {
            if (await _repo.GetByIdAsync(profesion.Id) != null)
            {
                return Conflict("Ya existe una profesion con ese id");
            }
            await _repo.AddAsync(profesion);
            return CreatedAtAction(nameof(Get), new { id = profesion.Id }, profesion);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Profesion profesion)
        {
            if (id != profesion.Id)
            {
                return BadRequest("El id de la url no coincide con el del body");
            }
            if (await _repo.GetByIdAsync(id) == null)
            {
                return NotFound();
            }
            await _repo.UpdateAsync(profesion);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _repo.GetByIdAsync(id) == null)
            {
                return NotFound();
            }
            await _repo.DeleteAsync(id);
            return NoContent();
        }
    }
}
