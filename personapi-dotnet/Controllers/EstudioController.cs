using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using personapi_dotnet.Interfaces;
using personapi_dotnet.Models.Entities;

// =====================================================================
// CAPA CONTROLADOR (MVC) - EstudioController
// Maneja el CRUD de estudios desde el navegador. La tabla "estudios" es la
// relacion muchos a muchos entre persona y profesion: cada fila dice que
// una persona (cc_per) estudio una profesion (id_prof). Por eso su llave
// primaria es COMPUESTA (id_prof + cc_per) y no hay un unico "id".
// Usa tres DAO: el de estudios y, para llenar los desplegables, los de
// persona y profesion. Nunca toca el DbContext directamente.
// =====================================================================

namespace personapi_dotnet.Controllers
{
    /// <summary>
    /// Controlador MVC con el CRUD de estudios (relacion persona - profesion).
    /// Como la llave es compuesta, las acciones reciben <c>idProf</c> y <c>ccPer</c>
    /// en lugar de un solo <c>id</c>. La ruta por defecto solo tiene un segmento {id},
    /// asi que estos dos valores viajan en la query string
    /// (ej: /Estudio/Details?idProf=1&amp;ccPer=123), generada en las vistas con
    /// asp-route-idProf y asp-route-ccPer.
    /// </summary>
    // como estudios tiene llave compuesta se manda idProf y ccPer en vez de id
    public class EstudioController : Controller
    {
        // DAO principal de estudios.
        private readonly IEstudioRepository _repo;
        // DAO de personas y de profesiones: solo se usan para armar los desplegables.
        private readonly IPersonaRepository _personaRepo;
        private readonly IProfesionRepository _profesionRepo;

        /// <summary>
        /// Constructor con inyeccion de dependencias de los tres repositorios.
        /// </summary>
        /// <param name="repo">Repositorio (DAO) de estudios.</param>
        /// <param name="personaRepo">Repositorio de personas (para el select de persona).</param>
        /// <param name="profesionRepo">Repositorio de profesiones (para el select de profesion).</param>
        public EstudioController(IEstudioRepository repo, IPersonaRepository personaRepo, IProfesionRepository profesionRepo)
        {
            _repo = repo;
            _personaRepo = personaRepo;
            _profesionRepo = profesionRepo;
        }

        /// <summary>
        /// Carga en ViewData las opciones de los desplegables (select) de persona y profesion.
        /// Un <see cref="SelectList"/> recibe: la coleccion, la propiedad que se usa como
        /// valor (value del option), la que se muestra como texto y el valor seleccionado.
        /// La vista los usa con <c>asp-items="ViewBag.CcPer"</c> / <c>ViewBag.IdProf</c>
        /// (ViewBag y ViewData son el mismo diccionario visto de dos formas).
        /// </summary>
        /// <param name="ccPer">Cedula que debe quedar seleccionada (null = ninguna).</param>
        /// <param name="idProf">Profesion que debe quedar seleccionada (null = ninguna).</param>
        // listas para los select de persona y profesion
        private async Task CargarListas(int? ccPer = null, int? idProf = null)
        {
            var personas = await _personaRepo.GetAllAsync();
            var profesiones = await _profesionRepo.GetAllAsync();
            // Para personas se proyecta un objeto anonimo con un texto mas legible
            // ("cedula - nombre apellido"); el valor enviado sigue siendo solo la cedula.
            ViewData["CcPer"] = new SelectList(
                personas.Select(p => new { p.Cc, Nombre = p.Cc + " - " + p.Nombre + " " + p.Apellido }),
                "Cc", "Nombre", ccPer);
            // Para profesiones basta con mostrar el nombre (Nom) y enviar el Id.
            ViewData["IdProf"] = new SelectList(profesiones, "Id", "Nom", idProf);
        }

        /// <summary>
        /// Lista todos los estudios (el repositorio incluye persona y profesion
        /// para mostrar nombres en vez de solo las llaves).
        /// </summary>
        /// <returns>La vista Index con la coleccion de estudios.</returns>
        // GET: Estudio
        public async Task<IActionResult> Index()
        {
            return View(await _repo.GetAllAsync());
        }

        /// <summary>
        /// Muestra el detalle de un estudio identificado por su llave compuesta.
        /// </summary>
        /// <param name="idProf">Id de la profesion (parte 1 de la llave).</param>
        /// <param name="ccPer">Cedula de la persona (parte 2 de la llave).</param>
        /// <returns>La vista Details, o 404 si no existe.</returns>
        // GET: Estudio/Details?idProf=1&ccPer=123
        public async Task<IActionResult> Details(int idProf, int ccPer)
        {
            var estudio = await _repo.GetByIdAsync(idProf, ccPer);
            if (estudio == null)
            {
                return NotFound();
            }
            return View(estudio);
        }

        /// <summary>
        /// GET del patron de formularios: muestra el formulario de creacion
        /// con los desplegables de persona y profesion ya cargados.
        /// </summary>
        /// <returns>La vista Create.</returns>
        // GET: Estudio/Create
        public async Task<IActionResult> Create()
        {
            await CargarListas();
            return View();
        }

        /// <summary>
        /// POST del patron de formularios: valida y guarda el nuevo estudio.
        /// </summary>
        /// <param name="estudio">Estudio armado con los campos del formulario.</param>
        /// <returns>Redireccion a Index, o la misma vista con los errores.</returns>
        // POST: Estudio/Create
        // [ValidateAntiForgeryToken] protege contra CSRF; [Bind] evita over-posting
        // (solo se aceptan las llaves, la fecha y la universidad, nunca las
        // propiedades de navegacion CcPerNavigation / IdProfNavigation).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdProf,CcPer,Fecha,Univer")] Estudio estudio)
        {
            // La pareja (profesion, persona) es la llave primaria: no puede repetirse.
            // La clave "" en AddModelError marca un error del modelo completo (no de
            // un campo), porque el problema es la combinacion de los dos valores; se
            // muestra en el resumen asp-validation-summary de la vista.
            if (await _repo.GetByIdAsync(estudio.IdProf, estudio.CcPer) != null)
            {
                ModelState.AddModelError("", "Esa persona ya tiene registrada esa profesion");
            }
            if (ModelState.IsValid)
            {
                await _repo.AddAsync(estudio);
                // Post-Redirect-Get: evita que recargar la pagina reenvie el formulario.
                return RedirectToAction(nameof(Index));
            }
            // ViewData no sobrevive entre peticiones: si se vuelve a mostrar la vista
            // hay que recargar los desplegables, dejando seleccionado lo que el usuario eligio.
            await CargarListas(estudio.CcPer, estudio.IdProf);
            return View(estudio);
        }

        /// <summary>
        /// Muestra el formulario de edicion. No se cargan desplegables porque
        /// la persona y la profesion (la llave) no se pueden cambiar: viajan
        /// en campos ocultos del formulario.
        /// </summary>
        /// <param name="idProf">Id de la profesion.</param>
        /// <param name="ccPer">Cedula de la persona.</param>
        /// <returns>La vista Edit, o 404 si no existe.</returns>
        // GET: Estudio/Edit?idProf=1&ccPer=123
        public async Task<IActionResult> Edit(int idProf, int ccPer)
        {
            var estudio = await _repo.GetByIdAsync(idProf, ccPer);
            if (estudio == null)
            {
                return NotFound();
            }
            return View(estudio);
        }

        /// <summary>
        /// Recibe el formulario de edicion y actualiza la fecha y la universidad.
        /// </summary>
        /// <param name="estudio">Datos enviados; IdProf y CcPer llegan de los campos ocultos.</param>
        /// <returns>Redireccion a Index, o la vista con errores.</returns>
        // POST: Estudio/Edit
        // la llave no se cambia, solo la fecha y la universidad
        // (cambiar la llave seria en realidad borrar un estudio y crear otro).
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

        /// <summary>
        /// Pantalla de confirmacion de borrado (el GET no modifica datos).
        /// </summary>
        /// <param name="idProf">Id de la profesion.</param>
        /// <param name="ccPer">Cedula de la persona.</param>
        /// <returns>La vista Delete, o 404 si no existe.</returns>
        // GET: Estudio/Delete?idProf=1&ccPer=123
        public async Task<IActionResult> Delete(int idProf, int ccPer)
        {
            var estudio = await _repo.GetByIdAsync(idProf, ccPer);
            if (estudio == null)
            {
                return NotFound();
            }
            return View(estudio);
        }

        /// <summary>
        /// Borra el estudio cuando el usuario confirma. Las dos partes de la llave
        /// llegan desde campos ocultos (name="idProf" y name="ccPer") del formulario.
        /// </summary>
        /// <param name="idProf">Id de la profesion.</param>
        /// <param name="ccPer">Cedula de la persona.</param>
        /// <returns>Redireccion a Index.</returns>
        // POST: Estudio/Delete
        // Se llama DeleteConfirmed porque Delete(int, int) ya existe para el GET y
        // C# no admite dos metodos con la misma firma; [ActionName("Delete")] hace
        // que MVC la siga exponiendo como la accion "Delete" para el POST.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int idProf, int ccPer)
        {
            await _repo.DeleteAsync(idProf, ccPer);
            return RedirectToAction(nameof(Index));
        }
    }
}
