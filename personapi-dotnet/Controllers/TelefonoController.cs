using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using personapi_dotnet.Interfaces;
using personapi_dotnet.Models.Entities;

// =====================================================================
// CAPA CONTROLADOR (MVC) - TelefonoController
// Maneja el CRUD de telefonos desde el navegador. Cada telefono pertenece
// a una persona (columna "duenio", llave foranea hacia persona.cc), por eso
// ademas del DAO de telefonos se inyecta el de personas, que se usa para
// llenar el desplegable del duenio. No usa el DbContext directamente.
// =====================================================================

namespace personapi_dotnet.Controllers
{
    /// <summary>
    /// Controlador MVC con el CRUD de telefonos. Hereda de <see cref="Controller"/>
    /// porque devuelve vistas Razor (Views/Telefono). La llave primaria es el
    /// numero (Num), que es texto: por eso el parametro <c>id</c> es string.
    /// </summary>
    public class TelefonoController : Controller
    {
        // DAO de telefonos.
        private readonly ITelefonoRepository _repo;
        // DAO de personas, solo para armar la lista de posibles duenios.
        private readonly IPersonaRepository _personaRepo;

        /// <summary>
        /// Constructor con inyeccion de dependencias.
        /// </summary>
        /// <param name="repo">Repositorio (DAO) de telefonos.</param>
        /// <param name="personaRepo">Repositorio de personas (para el select del duenio).</param>
        public TelefonoController(ITelefonoRepository repo, IPersonaRepository personaRepo)
        {
            _repo = repo;
            _personaRepo = personaRepo;
        }

        /// <summary>
        /// Carga en ViewData["Duenio"] un <see cref="SelectList"/> con las personas,
        /// que la vista pinta con <c>asp-items="ViewBag.Duenio"</c>. Asi el usuario
        /// elige el duenio de una lista y no tiene que escribir una cedula que podria
        /// no existir (lo que romperia la llave foranea).
        /// </summary>
        /// <param name="seleccionada">Cedula que debe quedar seleccionada (null = ninguna).</param>
        // lista de personas para el select del duenio
        private async Task CargarPersonas(int? seleccionada = null)
        {
            var personas = await _personaRepo.GetAllAsync();
            // Valor del option = Cc; texto visible = "cedula - nombre apellido".
            ViewData["Duenio"] = new SelectList(
                personas.Select(p => new { p.Cc, Nombre = p.Cc + " - " + p.Nombre + " " + p.Apellido }),
                "Cc", "Nombre", seleccionada);
        }

        /// <summary>
        /// Lista todos los telefonos.
        /// </summary>
        /// <returns>La vista Index con la coleccion de telefonos.</returns>
        // GET: Telefono
        public async Task<IActionResult> Index()
        {
            return View(await _repo.GetAllAsync());
        }

        /// <summary>
        /// Muestra el detalle de un telefono.
        /// </summary>
        /// <param name="id">Numero de telefono.</param>
        /// <returns>La vista Details, o 404 si no existe.</returns>
        // GET: Telefono/Details/3001234567
        public async Task<IActionResult> Details(string id)
        {
            var telefono = await _repo.GetByIdAsync(id);
            if (telefono == null)
            {
                return NotFound();
            }
            return View(telefono);
        }

        /// <summary>
        /// GET del patron de formularios: muestra el formulario vacio con la
        /// lista de personas para escoger el duenio.
        /// </summary>
        /// <returns>La vista Create.</returns>
        // GET: Telefono/Create
        public async Task<IActionResult> Create()
        {
            await CargarPersonas();
            return View();
        }

        /// <summary>
        /// POST del patron de formularios: valida y guarda el nuevo telefono.
        /// </summary>
        /// <param name="telefono">Telefono armado con los campos del formulario.</param>
        /// <returns>Redireccion a Index, o la misma vista con los errores.</returns>
        // POST: Telefono/Create
        // [ValidateAntiForgeryToken] protege contra CSRF; [Bind] acepta solo
        // Num, Oper y Duenio (evita over-posting sobre DuenioNavigation u otros campos).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Num,Oper,Duenio")] Telefono telefono)
        {
            // El numero es llave primaria: si ya existe se agrega el error al campo "Num".
            if (await _repo.GetByIdAsync(telefono.Num) != null)
            {
                ModelState.AddModelError("Num", "Ya existe ese numero");
            }
            if (ModelState.IsValid)
            {
                await _repo.AddAsync(telefono);
                // Post-Redirect-Get: evita reenviar el formulario al recargar.
                return RedirectToAction(nameof(Index));
            }
            // ViewData se pierde entre peticiones: hay que volver a cargar el
            // desplegable, conservando el duenio que el usuario habia elegido.
            await CargarPersonas(telefono.Duenio);
            return View(telefono);
        }

        /// <summary>
        /// Muestra el formulario de edicion con el duenio actual preseleccionado.
        /// </summary>
        /// <param name="id">Numero del telefono a editar.</param>
        /// <returns>La vista Edit, o 404 si no existe.</returns>
        // GET: Telefono/Edit/3001234567
        public async Task<IActionResult> Edit(string id)
        {
            var telefono = await _repo.GetByIdAsync(id);
            if (telefono == null)
            {
                return NotFound();
            }
            // A diferencia de estudios, aqui el duenio SI se puede cambiar
            // (no es parte de la llave), por eso se carga el desplegable.
            await CargarPersonas(telefono.Duenio);
            return View(telefono);
        }

        /// <summary>
        /// Recibe el formulario de edicion y actualiza operador y duenio.
        /// </summary>
        /// <param name="id">Numero tomado de la URL.</param>
        /// <param name="telefono">Datos enviados (Num llega en un campo oculto).</param>
        /// <returns>Redireccion a Index, 404 si el numero no cuadra, o la vista con errores.</returns>
        // POST: Telefono/Edit/3001234567
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Num,Oper,Duenio")] Telefono telefono)
        {
            // El numero de la URL debe coincidir con el del formulario.
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

        /// <summary>
        /// Pantalla de confirmacion de borrado (el GET no modifica datos).
        /// </summary>
        /// <param name="id">Numero del telefono a borrar.</param>
        /// <returns>La vista Delete, o 404 si no existe.</returns>
        // GET: Telefono/Delete/3001234567
        public async Task<IActionResult> Delete(string id)
        {
            var telefono = await _repo.GetByIdAsync(id);
            if (telefono == null)
            {
                return NotFound();
            }
            return View(telefono);
        }

        /// <summary>
        /// Borra el telefono cuando el usuario confirma.
        /// </summary>
        /// <param name="id">Numero del telefono a borrar.</param>
        /// <returns>Redireccion a Index.</returns>
        // POST: Telefono/Delete/3001234567
        // Se llama DeleteConfirmed porque Delete(string id) ya existe para el GET y
        // C# no admite dos metodos con la misma firma; [ActionName("Delete")] lo
        // publica como la accion "Delete" para el POST del formulario.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _repo.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
