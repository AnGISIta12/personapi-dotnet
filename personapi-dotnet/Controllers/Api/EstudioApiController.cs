using Microsoft.AspNetCore.Mvc;
using personapi_dotnet.Interfaces;
using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Controllers.Api
{
    // estudios tiene llave compuesta, por eso las rutas llevan idProf y ccPer
    [Route("api/estudios")]
    [ApiController]
    public class EstudioApiController : ControllerBase
    {
        private readonly IEstudioRepository _repo;
        private readonly IPersonaRepository _personaRepo;
        private readonly IProfesionRepository _profesionRepo;

        public EstudioApiController(IEstudioRepository repo, IPersonaRepository personaRepo, IProfesionRepository profesionRepo)
        {
            _repo = repo;
            _personaRepo = personaRepo;
            _profesionRepo = profesionRepo;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Estudio>>> GetAll()
        {
            return Ok(await _repo.GetAllAsync());
        }

        [HttpGet("{idProf}/{ccPer}")]
        public async Task<ActionResult<Estudio>> Get(int idProf, int ccPer)
        {
            var estudio = await _repo.GetByIdAsync(idProf, ccPer);
            if (estudio == null)
            {
                return NotFound();
            }
            return Ok(estudio);
        }

        [HttpPost]
        public async Task<ActionResult<Estudio>> Create(Estudio estudio)
        {
            if (await _repo.GetByIdAsync(estudio.IdProf, estudio.CcPer) != null)
            {
                return Conflict("Esa persona ya tiene registrada esa profesion");
            }
            if (await _personaRepo.GetByIdAsync(estudio.CcPer) == null || await _profesionRepo.GetByIdAsync(estudio.IdProf) == null)
            {
                return BadRequest("La persona o la profesion no existen");
            }
            await _repo.AddAsync(estudio);
            return CreatedAtAction(nameof(Get), new { idProf = estudio.IdProf, ccPer = estudio.CcPer }, estudio);
        }

        [HttpPut("{idProf}/{ccPer}")]
        public async Task<IActionResult> Update(int idProf, int ccPer, Estudio estudio)
        {
            if (idProf != estudio.IdProf || ccPer != estudio.CcPer)
            {
                return BadRequest("La llave de la url no coincide con la del body");
            }
            if (await _repo.GetByIdAsync(idProf, ccPer) == null)
            {
                return NotFound();
            }
            await _repo.UpdateAsync(estudio);
            return NoContent();
        }

        [HttpDelete("{idProf}/{ccPer}")]
        public async Task<IActionResult> Delete(int idProf, int ccPer)
        {
            if (await _repo.GetByIdAsync(idProf, ccPer) == null)
            {
                return NotFound();
            }
            await _repo.DeleteAsync(idProf, ccPer);
            return NoContent();
        }
    }
}
