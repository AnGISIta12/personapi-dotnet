using Microsoft.AspNetCore.Mvc;
using personapi_dotnet.Interfaces;
using personapi_dotnet.Models.Entities;

// =====================================================================
// CAPA CONTROLADOR (MVC) - ProfesionController
// Recibe las peticiones del navegador para el CRUD de profesiones, usa el
// DAO IProfesionRepository para leer/escribir datos y devuelve las vistas
// Razor de Views/Profesion. No conoce el DbContext ni SQL Server.
// =====================================================================

namespace personapi_dotnet.Controllers
{
    /// <summary>
    /// Controlador MVC con el CRUD de profesiones. Hereda de <see cref="Controller"/>
    /// porque devuelve vistas HTML (a diferencia de los controladores de la API,
    /// que heredan de ControllerBase y devuelven JSON).
    /// La llave primaria es Id, que llega en el segmento {id} de la ruta por defecto.
    /// </summary>
    public class ProfesionController : Controller
    {
        // DAO de profesiones, inyectado por el contenedor (registrado en Program.cs).
        private readonly IProfesionRepository _repo;

        /// <summary>
        /// Constructor con inyeccion de dependencias.
        /// </summary>
        /// <param name="repo">Repositorio (DAO) de profesiones.</param>
        public ProfesionController(IProfesionRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Lista todas las profesiones.
        /// </summary>
        /// <returns>La vista Index con la coleccion de profesiones.</returns>
        // GET: Profesion
        public async Task<IActionResult> Index()
        {
            return View(await _repo.GetAllAsync());
        }

        /// <summary>
        /// Muestra el detalle de una profesion.
        /// </summary>
        /// <param name="id">Id de la profesion.</param>
        /// <returns>La vista Details, o 404 si no existe.</returns>
        // GET: Profesion/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var profesion = await _repo.GetByIdAsync(id);
            if (profesion == null)
            {
                return NotFound();
            }
            return View(profesion);
        }

        /// <summary>
        /// GET del patron de formularios: muestra el formulario vacio de creacion.
        /// </summary>
        /// <returns>La vista Create.</returns>
        // GET: Profesion/Create
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// POST del patron de formularios: valida y guarda la nueva profesion.
        /// </summary>
        /// <param name="profesion">Profesion armada con los campos del formulario.</param>
        /// <returns>Redireccion a Index, o la misma vista con los errores de validacion.</returns>
        // POST: Profesion/Create
        // [ValidateAntiForgeryToken] protege contra CSRF exigiendo el token oculto del formulario.
        // [Bind] limita las propiedades aceptadas (Id, Nom, Des) para evitar over-posting.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nom,Des")] Profesion profesion)
        {
            // El Id lo digita el usuario (no es autoincremental), por eso se
            // verifica a mano que no este repetido. El error queda en ModelState
            // asociado al campo "Id" y se muestra en la vista.
            if (await _repo.GetByIdAsync(profesion.Id) != null)
            {
                ModelState.AddModelError("Id", "Ya existe una profesion con ese id");
            }
            if (ModelState.IsValid)
            {
                await _repo.AddAsync(profesion);
                // Post-Redirect-Get: redirigir evita reenviar el formulario al recargar.
                return RedirectToAction(nameof(Index));
            }
            // Con errores se vuelve a mostrar el formulario con lo que el usuario escribio.
            return View(profesion);
        }

        /// <summary>
        /// Muestra el formulario de edicion con los datos actuales.
        /// </summary>
        /// <param name="id">Id de la profesion a editar.</param>
        /// <returns>La vista Edit, o 404 si no existe.</returns>
        // GET: Profesion/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var profesion = await _repo.GetByIdAsync(id);
            if (profesion == null)
            {
                return NotFound();
            }
            return View(profesion);
        }

        /// <summary>
        /// Recibe el formulario de edicion y actualiza la profesion.
        /// </summary>
        /// <param name="id">Id tomado de la URL.</param>
        /// <param name="profesion">Datos enviados en el formulario.</param>
        /// <returns>Redireccion a Index, 404 si el id no cuadra, o la vista con errores.</returns>
        // POST: Profesion/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nom,Des")] Profesion profesion)
        {
            // El id de la URL y el del formulario deben ser el mismo registro.
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

        /// <summary>
        /// Pantalla de confirmacion de borrado (el GET no modifica datos).
        /// </summary>
        /// <param name="id">Id de la profesion a borrar.</param>
        /// <returns>La vista Delete, o 404 si no existe.</returns>
        // GET: Profesion/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var profesion = await _repo.GetByIdAsync(id);
            if (profesion == null)
            {
                return NotFound();
            }
            return View(profesion);
        }

        /// <summary>
        /// Borra la profesion cuando el usuario confirma.
        /// </summary>
        /// <param name="id">Id de la profesion a borrar.</param>
        /// <returns>Redireccion a Index.</returns>
        // POST: Profesion/Delete/5
        // Se llama DeleteConfirmed porque ya existe Delete(int id) para el GET y C#
        // no permite dos metodos con la misma firma. [ActionName("Delete")] mantiene
        // el nombre de accion "Delete" para que el formulario haga POST a /Profesion/Delete/5.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _repo.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
