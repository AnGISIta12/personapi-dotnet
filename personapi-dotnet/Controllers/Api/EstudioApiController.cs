using Microsoft.AspNetCore.Mvc;
using personapi_dotnet.Interfaces;
using personapi_dotnet.Models.Entities;

// =====================================================================
// CAPA CONTROLADOR - API REST - EstudioApiController
// Expone el CRUD de estudios (relacion persona - profesion) como servicio
// REST en JSON, documentado en Swagger. Ademas del DAO de estudios usa los
// de persona y profesion para validar las llaves foraneas antes de insertar.
// =====================================================================

namespace personapi_dotnet.Controllers.Api
{
    /// <summary>
    /// API REST de estudios. Recurso base: <c>api/estudios</c>.
    /// Como la tabla tiene llave primaria compuesta (id_prof + cc_per), un estudio
    /// concreto se identifica con DOS segmentos de ruta: <c>api/estudios/{idProf}/{ccPer}</c>.
    /// Hereda de <see cref="ControllerBase"/> porque no usa vistas.
    /// </summary>
    // estudios tiene llave compuesta, por eso las rutas llevan idProf y ccPer
    // [ApiController]: validacion automatica del modelo (400), body JSON para
    // el parametro Estudio y rutas por atributo obligatorias.
    [Route("api/estudios")]
    [ApiController]
    public class EstudioApiController : ControllerBase
    {
        // DAO de estudios.
        private readonly IEstudioRepository _repo;
        // DAO de persona y profesion: se usan para comprobar que existan las
        // filas a las que apuntan las llaves foraneas del estudio.
        private readonly IPersonaRepository _personaRepo;
        private readonly IProfesionRepository _profesionRepo;

        /// <summary>
        /// Constructor con inyeccion de dependencias de los tres repositorios.
        /// </summary>
        /// <param name="repo">Repositorio (DAO) de estudios.</param>
        /// <param name="personaRepo">Repositorio de personas (validacion de FK).</param>
        /// <param name="profesionRepo">Repositorio de profesiones (validacion de FK).</param>
        public EstudioApiController(IEstudioRepository repo, IPersonaRepository personaRepo, IProfesionRepository profesionRepo)
        {
            _repo = repo;
            _personaRepo = personaRepo;
            _profesionRepo = profesionRepo;
        }

        /// <summary>
        /// Lista todos los estudios. Ejemplo: GET api/estudios
        /// </summary>
        /// <returns>200 OK con el arreglo JSON de estudios.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Estudio>>> GetAll()
        {
            return Ok(await _repo.GetAllAsync());
        }

        /// <summary>
        /// Consulta un estudio por su llave compuesta. Ejemplo: GET api/estudios/1/123
        /// </summary>
        /// <param name="idProf">Id de la profesion (primer segmento).</param>
        /// <param name="ccPer">Cedula de la persona (segundo segmento).</param>
        /// <returns>200 OK con el estudio, o 404 Not Found si no existe.</returns>
        // Los dos parametros de ruta {idProf}/{ccPer} se enlazan por nombre
        // con los parametros del metodo.
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

        /// <summary>
        /// Crea un estudio a partir del JSON del body. Ejemplo: POST api/estudios
        /// </summary>
        /// <param name="estudio">Estudio recibido en el cuerpo (idProf, ccPer, fecha, univer).</param>
        /// <returns>201 Created con header Location, 409 si ya existe, o 400 si la persona o la profesion no existen.</returns>
        [HttpPost]
        public async Task<ActionResult<Estudio>> Create(Estudio estudio)
        {
            // 409 Conflict: esa combinacion (profesion, persona) ya esta registrada.
            if (await _repo.GetByIdAsync(estudio.IdProf, estudio.CcPer) != null)
            {
                return Conflict("Esa persona ya tiene registrada esa profesion");
            }
            // Validacion de llaves foraneas: cc_per debe existir en persona e id_prof
            // en profesion. Si no se revisara aqui, SQL Server rechazaria el INSERT
            // y EF lanzaria una excepcion que terminaria en un 500 poco claro; asi
            // el cliente recibe un 400 Bad Request con un mensaje entendible.
            if (await _personaRepo.GetByIdAsync(estudio.CcPer) == null || await _profesionRepo.GetByIdAsync(estudio.IdProf) == null)
            {
                return BadRequest("La persona o la profesion no existen");
            }
            await _repo.AddAsync(estudio);
            // 201 Created: el header Location queda como /api/estudios/{idProf}/{ccPer},
            // por eso el objeto de valores de ruta lleva las dos partes de la llave.
            return CreatedAtAction(nameof(Get), new { idProf = estudio.IdProf, ccPer = estudio.CcPer }, estudio);
        }

        /// <summary>
        /// Actualiza fecha y universidad de un estudio. Ejemplo: PUT api/estudios/1/123
        /// </summary>
        /// <param name="idProf">Id de la profesion (URL).</param>
        /// <param name="ccPer">Cedula de la persona (URL).</param>
        /// <param name="estudio">Datos nuevos en el body.</param>
        /// <returns>204 No Content, 400 si la llave no coincide, o 404 si no existe.</returns>
        [HttpPut("{idProf}/{ccPer}")]
        public async Task<IActionResult> Update(int idProf, int ccPer, Estudio estudio)
        {
            // 400: las dos partes de la llave de la URL deben coincidir con las del body;
            // la llave no se puede modificar con PUT.
            if (idProf != estudio.IdProf || ccPer != estudio.CcPer)
            {
                return BadRequest("La llave de la url no coincide con la del body");
            }
            if (await _repo.GetByIdAsync(idProf, ccPer) == null)
            {
                return NotFound();
            }
            await _repo.UpdateAsync(estudio);
            // 204: exito sin cuerpo.
            return NoContent();
        }

        /// <summary>
        /// Elimina un estudio. Ejemplo: DELETE api/estudios/1/123
        /// </summary>
        /// <param name="idProf">Id de la profesion.</param>
        /// <param name="ccPer">Cedula de la persona.</param>
        /// <returns>204 No Content si se borro, o 404 Not Found si no existe.</returns>
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
