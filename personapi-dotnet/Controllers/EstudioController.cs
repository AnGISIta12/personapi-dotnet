using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using personapi_dotnet.Interfaces;
using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Controllers
{
    // llave compuesta: se usa idProf y ccPer en vez de id
    public class EstudioController : Controller
    {
        private readonly IEstudioRepository _repo;
        private readonly IPersonaRepository _personaRepo;
        private readonly IProfesionRepository _profesionRepo;

        public EstudioController(IEstudioRepository repo, IPersonaRepository personaRepo, IProfesionRepository profesionRepo)
        {
            _repo = repo;
            _personaRepo = personaRepo;
            _profesionRepo = profesionRepo;
        }

        private async Task CargarListas(int? ccPer = null, int? idProf = null)
        {
            var personas = await _personaRepo.GetAllAsync();
            var profesiones = await _profesionRepo.GetAllAsync();
            ViewData["CcPer"] = new SelectList(
                personas.Select(p => new { p.Cc, Nombre = p.Cc + " - " + p.Nombre + " " + p.Apellido }),
                "Cc", "Nombre", ccPer);
            ViewData["IdProf"] = new SelectList(profesiones, "Id", "Nom", idProf);
        }

        public async Task<IActionResult> Index()
        {
            return View(await _repo.GetAllAsync());
        }

        public async Task<IActionResult> Details(int idProf, int ccPer)
        {
            var estudio = await _repo.GetByIdAsync(idProf, ccPer);
            if (estudio == null)
            {
                return NotFound();
            }
            return View(estudio);
        }

        public async Task<IActionResult> Create()
        {
            await CargarListas();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdProf,CcPer,Fecha,Univer")] Estudio estudio)
        {
            if (await _repo.GetByIdAsync(estudio.IdProf, estudio.CcPer) != null)
            {
                ModelState.AddModelError("", "Esa persona ya tiene registrada esa profesion");
            }
            if (ModelState.IsValid)
            {
                await _repo.AddAsync(estudio);
                return RedirectToAction(nameof(Index));
            }
            await CargarListas(estudio.CcPer, estudio.IdProf);
            return View(estudio);
        }

        public async Task<IActionResult> Edit(int idProf, int ccPer)
        {
            var estudio = await _repo.GetByIdAsync(idProf, ccPer);
            if (estudio == null)
            {
                return NotFound();
            }
            return View(estudio);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("IdProf,CcPer,Fecha,Univer")] Estudio estudio)
        {
            if (ModelState.IsValid)
            {
                await _repo.UpdateAsync(estudio);
                return RedirectToAction(nameof(Index));
            }
            return View(estudio);
        }

        public async Task<IActionResult> Delete(int idProf, int ccPer)
        {
            var estudio = await _repo.GetByIdAsync(idProf, ccPer);
            if (estudio == null)
            {
                return NotFound();
            }
            return View(estudio);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int idProf, int ccPer)
        {
            await _repo.DeleteAsync(idProf, ccPer);
            return RedirectToAction(nameof(Index));
        }
    }
}
