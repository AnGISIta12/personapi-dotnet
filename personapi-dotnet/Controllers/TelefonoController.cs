using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using personapi_dotnet.Interfaces;
using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Controllers
{
    public class TelefonoController : Controller
    {
        private readonly ITelefonoRepository _repo;
        private readonly IPersonaRepository _personaRepo;

        public TelefonoController(ITelefonoRepository repo, IPersonaRepository personaRepo)
        {
            _repo = repo;
            _personaRepo = personaRepo;
        }

        private async Task CargarPersonas(int? seleccionada = null)
        {
            var personas = await _personaRepo.GetAllAsync();
            ViewData["Duenio"] = new SelectList(
                personas.Select(p => new { p.Cc, Nombre = p.Cc + " - " + p.Nombre + " " + p.Apellido }),
                "Cc", "Nombre", seleccionada);
        }

        public async Task<IActionResult> Index()
        {
            return View(await _repo.GetAllAsync());
        }

        public async Task<IActionResult> Details(string id)
        {
            var telefono = await _repo.GetByIdAsync(id);
            if (telefono == null)
            {
                return NotFound();
            }
            return View(telefono);
        }

        public async Task<IActionResult> Create()
        {
            await CargarPersonas();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Num,Oper,Duenio")] Telefono telefono)
        {
            if (await _repo.GetByIdAsync(telefono.Num) != null)
            {
                ModelState.AddModelError("Num", "Ya existe ese numero");
            }
            if (ModelState.IsValid)
            {
                await _repo.AddAsync(telefono);
                return RedirectToAction(nameof(Index));
            }
            await CargarPersonas(telefono.Duenio);
            return View(telefono);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var telefono = await _repo.GetByIdAsync(id);
            if (telefono == null)
            {
                return NotFound();
            }
            await CargarPersonas(telefono.Duenio);
            return View(telefono);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Num,Oper,Duenio")] Telefono telefono)
        {
            if (id != telefono.Num)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                await _repo.UpdateAsync(telefono);
                return RedirectToAction(nameof(Index));
            }
            await CargarPersonas(telefono.Duenio);
            return View(telefono);
        }

        public async Task<IActionResult> Delete(string id)
        {
            var telefono = await _repo.GetByIdAsync(id);
            if (telefono == null)
            {
                return NotFound();
            }
            return View(telefono);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _repo.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
