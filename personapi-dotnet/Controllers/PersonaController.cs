using Microsoft.AspNetCore.Mvc;
using personapi_dotnet.Interfaces;
using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Controllers
{
    // usa el repositorio, no el DbContext directo
    public class PersonaController : Controller
    {
        private readonly IPersonaRepository _repo;

        public PersonaController(IPersonaRepository repo)
        {
            _repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _repo.GetAllAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var persona = await _repo.GetByIdAsync(id);
            if (persona == null)
            {
                return NotFound();
            }
            return View(persona);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Cc,Nombre,Apellido,Genero,Edad")] Persona persona)
        {
            if (await _repo.GetByIdAsync(persona.Cc) != null)
            {
                ModelState.AddModelError("Cc", "Ya existe una persona con esa cedula");
            }
            if (ModelState.IsValid)
            {
                await _repo.AddAsync(persona);
                return RedirectToAction(nameof(Index));
            }
            return View(persona);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var persona = await _repo.GetByIdAsync(id);
            if (persona == null)
            {
                return NotFound();
            }
            return View(persona);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Cc,Nombre,Apellido,Genero,Edad")] Persona persona)
        {
            if (id != persona.Cc)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                await _repo.UpdateAsync(persona);
                return RedirectToAction(nameof(Index));
            }
            return View(persona);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var persona = await _repo.GetByIdAsync(id);
            if (persona == null)
            {
                return NotFound();
            }
            return View(persona);
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
