using Microsoft.AspNetCore.Mvc;
using personapi_dotnet.Interfaces;
using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Controllers.Api
{
    [Route("api/telefonos")]
    [ApiController]
    public class TelefonoApiController : ControllerBase
    {
        private readonly ITelefonoRepository _repo;
        private readonly IPersonaRepository _personaRepo;

        public TelefonoApiController(ITelefonoRepository repo, IPersonaRepository personaRepo)
        {
            _repo = repo;
            _personaRepo = personaRepo;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Telefono>>> GetAll()
        {
            return Ok(await _repo.GetAllAsync());
        }

        [HttpGet("{num}")]
        public async Task<ActionResult<Telefono>> Get(string num)
        {
            var telefono = await _repo.GetByIdAsync(num);
            if (telefono == null)
            {
                return NotFound();
            }
            return Ok(telefono);
        }

        [HttpPost]
        public async Task<ActionResult<Telefono>> Create(Telefono telefono)
        {
            if (await _repo.GetByIdAsync(telefono.Num) != null)
            {
                return Conflict("Ya existe ese numero");
            }
            // el duenio tiene que existir (FK)
            if (await _personaRepo.GetByIdAsync(telefono.Duenio) == null)
            {
                return BadRequest("No existe una persona con esa cedula");
            }
            await _repo.AddAsync(telefono);
            return CreatedAtAction(nameof(Get), new { num = telefono.Num }, telefono);
        }

        [HttpPut("{num}")]
        public async Task<IActionResult> Update(string num, Telefono telefono)
        {
            if (num != telefono.Num)
            {
                return BadRequest("El numero de la url no coincide con el del body");
            }
            if (await _repo.GetByIdAsync(num) == null)
            {
                return NotFound();
            }
            if (await _personaRepo.GetByIdAsync(telefono.Duenio) == null)
            {
                return BadRequest("No existe una persona con esa cedula");
            }
            await _repo.UpdateAsync(telefono);
            return NoContent();
        }

        [HttpDelete("{num}")]
        public async Task<IActionResult> Delete(string num)
        {
            if (await _repo.GetByIdAsync(num) == null)
            {
                return NotFound();
            }
            await _repo.DeleteAsync(num);
            return NoContent();
        }
    }
}
