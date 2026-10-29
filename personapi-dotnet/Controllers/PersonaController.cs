using Microsoft.AspNetCore.Mvc;
using personapi_dotnet.Interfaces;
using personapi_dotnet.Models.Entities;

// =====================================================================
// CAPA CONTROLADOR (MVC) - PersonaController
// En el patron MVC el controlador recibe la peticion HTTP del navegador,
// pide los datos a la capa de acceso a datos (DAO / repositorio) y elige
// la vista Razor (Views/Persona/*.cshtml) que se le devuelve al usuario.
// Este controlador NO usa el DbContext directamente: depende solo de la
// interfaz IPersonaRepository, asi la logica web queda desacoplada de
// Entity Framework y de SQL Server.
// =====================================================================

namespace personapi_dotnet.Controllers
{
    /// <summary>
    /// Controlador MVC con el CRUD (crear, leer, actualizar, borrar) de personas.
    /// Hereda de <see cref="Controller"/> porque trabaja con vistas: Controller
    /// es ControllerBase + soporte de vistas (View, ViewData, TempData...).
    /// La llave primaria de persona es la cedula (Cc), que llega en el parametro "id"
    /// porque asi se llama el segmento opcional de la ruta por defecto.
    /// </summary>
    // controlador MVC de persona, usa el repositorio (DAO) y no el contexto directamente
    public class PersonaController : Controller
    {
        // Referencia a la abstraccion del DAO; la implementacion concreta
        // (PersonaRepository) la decide Program.cs con AddScoped.
        private readonly IPersonaRepository _repo;

        /// <summary>
        /// Constructor con inyeccion de dependencias: el contenedor de ASP.NET Core
        /// entrega una instancia de IPersonaRepository (una por peticion, Scoped).
        /// </summary>
        /// <param name="repo">Repositorio (DAO) de personas.</param>
        public PersonaController(IPersonaRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Lista todas las personas.
        /// </summary>
        /// <returns>La vista Index con la coleccion de personas como modelo.</returns>
        // GET: Persona
        public async Task<IActionResult> Index()
        {
            return View(await _repo.GetAllAsync());
        }

        /// <summary>
        /// Muestra el detalle de una persona.
        /// </summary>
        /// <param name="id">Cedula de la persona (segmento {id} de la ruta).</param>
        /// <returns>La vista Details, o 404 si la cedula no existe.</returns>
        // GET: Persona/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var persona = await _repo.GetByIdAsync(id);
            // Si no existe se responde 404 Not Found en lugar de mostrar una vista vacia.
            if (persona == null)
            {
                return NotFound();
            }
            return View(persona);
        }

        /// <summary>
        /// Primera mitad del patron GET/POST de formularios: el GET solo muestra
        /// el formulario vacio; el POST (siguiente accion) procesa lo enviado.
        /// </summary>
        /// <returns>La vista Create con el formulario vacio.</returns>
        // GET: Persona/Create
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Recibe el formulario de creacion y guarda la nueva persona.
        /// </summary>
        /// <param name="persona">Persona armada por el model binding con los campos del formulario.</param>
        /// <returns>Redireccion a Index si todo sale bien, o la misma vista con los errores.</returns>
        // POST: Persona/Create
        // [HttpPost]: esta accion solo atiende peticiones POST (envio del formulario).
        // [ValidateAntiForgeryToken]: exige el token antifalsificacion que la vista
        // incluye en un campo oculto. Protege contra CSRF, es decir, que otro sitio
        // haga que el navegador del usuario envie este formulario sin que lo sepa.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [Bind(...)]: lista blanca de propiedades que se aceptan del formulario.
        // Evita el "over-posting": que alguien agregue campos extra al POST
        // (por ejemplo propiedades de navegacion) y modifique datos no previstos.
        public async Task<IActionResult> Create([Bind("Cc,Nombre,Apellido,Genero,Edad")] Persona persona)
        {
            // Validacion de negocio que los DataAnnotations no cubren: la cedula es
            // llave primaria, asi que no puede repetirse. AddModelError asocia el
            // mensaje al campo "Cc" para que asp-validation-for lo muestre junto al input
            // y, ademas, deja ModelState.IsValid en false.
            if (await _repo.GetByIdAsync(persona.Cc) != null)
            {
                ModelState.AddModelError("Cc", "Ya existe una persona con esa cedula");
            }
            // ModelState reune los errores de conversion de tipos, de las anotaciones
            // del modelo y los agregados a mano. Solo se guarda si no hay ninguno.
            if (ModelState.IsValid)
            {
                await _repo.AddAsync(persona);
                // Patron Post-Redirect-Get (PRG): tras un POST exitoso se redirige
                // (HTTP 302) a una accion GET. Asi, si el usuario recarga la pagina,
                // el navegador repite el GET y no reenvia el formulario (no duplica el registro).
                // nameof(Index) evita escribir el nombre de la accion como texto suelto.
                return RedirectToAction(nameof(Index));
            }
            // Si hay errores se devuelve la misma vista con los datos digitados,
            // para que el usuario no pierda lo que escribio y vea los mensajes.
            return View(persona);
        }

        /// <summary>
        /// Muestra el formulario de edicion precargado con los datos actuales.
        /// </summary>
        /// <param name="id">Cedula de la persona a editar.</param>
        /// <returns>La vista Edit, o 404 si no existe.</returns>
        // GET: Persona/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var persona = await _repo.GetByIdAsync(id);
            if (persona == null)
            {
                return NotFound();
            }
            return View(persona);
        }

        /// <summary>
        /// Recibe el formulario de edicion y actualiza la persona.
        /// </summary>
        /// <param name="id">Cedula tomada de la URL.</param>
        /// <param name="persona">Datos enviados en el formulario.</param>
        /// <returns>Redireccion a Index, 404 si la cedula no cuadra, o la vista con errores.</returns>
        // POST: Persona/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Cc,Nombre,Apellido,Genero,Edad")] Persona persona)
        {
            // La cedula de la URL debe coincidir con la del formulario; si alguien
            // manipula el campo oculto se rechaza para no editar otra persona.
            if (id != persona.Cc)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                await _repo.UpdateAsync(persona);
                // Post-Redirect-Get, igual que en Create.
                return RedirectToAction(nameof(Index));
            }
            return View(persona);
        }

        /// <summary>
        /// Muestra la pantalla de confirmacion antes de borrar. El GET nunca borra:
        /// una operacion que cambia datos no debe ejecutarse con un simple enlace.
        /// </summary>
        /// <param name="id">Cedula de la persona a borrar.</param>
        /// <returns>La vista Delete de confirmacion, o 404 si no existe.</returns>
        // GET: Persona/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var persona = await _repo.GetByIdAsync(id);
            if (persona == null)
            {
                return NotFound();
            }
            return View(persona);
        }

        /// <summary>
        /// Ejecuta el borrado cuando el usuario confirma en el formulario.
        /// El repositorio tambien elimina los telefonos y estudios de la persona
        /// para no violar las llaves foraneas.
        /// </summary>
        /// <param name="id">Cedula de la persona a borrar.</param>
        /// <returns>Redireccion a Index.</returns>
        // POST: Persona/Delete/5
        // El metodo se llama DeleteConfirmed porque en C# no pueden existir dos
        // metodos con la misma firma: Delete(int id) ya existe para el GET.
        // [ActionName("Delete")] hace que para MVC esta accion siga llamandose
        // "Delete", asi el formulario de la vista Delete hace POST a /Persona/Delete/5.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _repo.DeleteAsync(id);
            // Post-Redirect-Get: se vuelve al listado.
            return RedirectToAction(nameof(Index));
        }
    }
}
