using Microsoft.AspNetCore.Mvc;
using personapi_dotnet.Interfaces;
using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Controllers
{
    public class ProfesionController : Controller
    {
        private readonly IProfesionRepository _repo;

        public ProfesionController(IProfesionRepository repo)
        {
            _repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _repo.GetAllAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var profesion = await _repo.GetByIdAsync(id);
            if (profesion == null)
            {
                return NotFound();
            }
            return View(profesion);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nom,Des")] Profesion profesion)
        {
            if (await _repo.GetByIdAsync(profesion.Id) != null)
            {
                ModelState.AddModelError("Id", "Ya existe una profesion con ese id");
            }
            if (ModelState.IsValid)
            {
                await _repo.AddAsync(profesion);
                return RedirectToAction(nameof(Index));
            }
            return View(profesion);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var profesion = await _repo.GetByIdAsync(id);
            if (profesion == null)
            {
                return NotFound();
            }
            return View(profesion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nom,Des")] Profesion profesion)
        {
            if (id != profesion.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                await _repo.UpdateAsync(profesion);
                return RedirectToAction(nameof(Index));
            }
            return View(profesion);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var profesion = await _repo.GetByIdAsync(id);
            if (profesion == null)
            {
                return NotFound();
            }
            return View(profesion);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _repo.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
