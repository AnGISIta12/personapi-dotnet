using Microsoft.AspNetCore.Mvc;
using personapi_dotnet.Interfaces;
using personapi_dotnet.Models.Entities;

// =====================================================================
// CAPA CONTROLADOR - API REST - PersonaApiController
// Expone el CRUD de personas como servicio REST: recibe y devuelve JSON
// (no vistas HTML), lo documenta Swagger (/swagger) y lo puede consumir
// cualquier cliente (Postman, otra aplicacion, un frontend JS...).
// Reutiliza el mismo DAO (IPersonaRepository) que el controlador MVC, lo que
// muestra la ventaja de la capa de acceso a datos: dos "presentaciones"
// distintas (web y API) sobre la misma logica de persistencia.
// =====================================================================

namespace personapi_dotnet.Controllers.Api
{
    /// <summary>
    /// API REST de personas. Recurso base: <c>api/personas</c>.
    /// Hereda de <see cref="ControllerBase"/> y no de Controller porque una API
    /// no necesita soporte de vistas (View, ViewData); ControllerBase trae solo
    /// lo necesario para responder datos y codigos HTTP (Ok, NotFound, ...).
    /// </summary>
    // endpoints REST de persona (salen en swagger)
    // [Route]: enrutamiento por atributos. Todas las acciones cuelgan de "api/personas"
    // y los atributos [HttpGet("{cc}")] etc. agregan segmentos a esa ruta base.
    // [ApiController] activa comportamientos propios de APIs:
    //   - validacion automatica: si el modelo recibido no es valido responde 400
    //     (ValidationProblemDetails) sin entrar a la accion, por eso aqui no se
    //     revisa ModelState.IsValid a mano.
    //   - binding inferido: los parametros complejos (Persona) se leen del body JSON
    //     ([FromBody] implicito) y los simples (cc) de la ruta.
    //   - exige rutas por atributo (no usa la ruta convencional de MVC).
    [Route("api/personas")]
    [ApiController]
    public class PersonaApiController : ControllerBase
    {
        // Mismo DAO que usa PersonaController (MVC); se inyecta por constructor.
        private readonly IPersonaRepository _repo;

        /// <summary>
        /// Constructor con inyeccion de dependencias.
        /// </summary>
        /// <param name="repo">Repositorio (DAO) de personas.</param>
        public PersonaApiController(IPersonaRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Lista todas las personas. Ejemplo: GET api/personas
        /// </summary>
        /// <returns>200 OK con el arreglo JSON de personas.</returns>
        // ActionResult<T> permite devolver el dato (T) o un codigo HTTP, y le
        // indica a Swagger el tipo de la respuesta exitosa.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Persona>>> GetAll()
        {
            return Ok(await _repo.GetAllAsync());
        }

        /// <summary>
        /// Consulta una persona por cedula. Ejemplo: GET api/personas/123
        /// </summary>
        /// <param name="cc">Cedula; se toma del segmento {cc} de la URL.</param>
        /// <returns>200 OK con la persona, o 404 Not Found si no existe.</returns>
        // "{cc}" es un parametro de ruta: el valor de la URL se asigna al
        // parametro del metodo que tiene el mismo nombre.
        [HttpGet("{cc}")]
        public async Task<ActionResult<Persona>> Get(int cc)
        {
            var persona = await _repo.GetByIdAsync(cc);
            // 404: el recurso pedido no existe.
            if (persona == null)
            {
                return NotFound();
            }
            // 200: respuesta exitosa con el recurso en el cuerpo.
            return Ok(persona);
        }

        /// <summary>
        /// Crea una persona a partir del JSON del body. Ejemplo: POST api/personas
        /// </summary>
        /// <param name="persona">Persona recibida en el cuerpo de la peticion.</param>
        /// <returns>201 Created con la persona y el header Location, o 409 Conflict si la cedula ya existe.</returns>
        [HttpPost]
        public async Task<ActionResult<Persona>> Create(Persona persona)
        {
            // 409 Conflict: la peticion esta bien formada, pero choca con el estado
            // actual del servidor (ya hay un recurso con esa llave primaria).
            if (await _repo.GetByIdAsync(persona.Cc) != null)
            {
                return Conflict("Ya existe una persona con esa cedula");
            }
            await _repo.AddAsync(persona);
            // 201 Created es la respuesta REST correcta al crear un recurso.
            // CreatedAtAction ademas agrega el header Location con la URL del nuevo
            // recurso (ej: /api/personas/123), construida a partir de la accion Get
            // y del valor de ruta { cc }; el cuerpo lleva la persona creada.
            return CreatedAtAction(nameof(Get), new { cc = persona.Cc }, persona);
        }

        /// <summary>
        /// Actualiza una persona existente. Ejemplo: PUT api/personas/123
        /// </summary>
        /// <param name="cc">Cedula de la URL (identifica el recurso).</param>
        /// <param name="persona">Datos nuevos en el body.</param>
        /// <returns>204 No Content si se actualizo, 400 si la cedula no coincide, 404 si no existe.</returns>
        [HttpPut("{cc}")]
        public async Task<IActionResult> Update(int cc, Persona persona)
        {
            // 400 Bad Request: la peticion es incoherente (la URL dice un recurso y
            // el body otro). No se permite cambiar la llave primaria por esta via.
            if (cc != persona.Cc)
            {
                return BadRequest("La cedula de la url no coincide con la del body");
            }
            // 404 si se intenta actualizar algo que no existe (PUT aqui no crea).
            if (await _repo.GetByIdAsync(cc) == null)
            {
                return NotFound();
            }
            await _repo.UpdateAsync(persona);
            // 204 No Content: la operacion salio bien y no hace falta devolver cuerpo,
            // el cliente ya tiene los datos que envio.
            return NoContent();
        }

        /// <summary>
        /// Elimina una persona (el repositorio borra antes sus telefonos y estudios
        /// por las llaves foraneas). Ejemplo: DELETE api/personas/123
        /// </summary>
        /// <param name="cc">Cedula de la persona a eliminar.</param>
        /// <returns>204 No Content si se borro, o 404 Not Found si no existe.</returns>
        [HttpDelete("{cc}")]
        public async Task<IActionResult> Delete(int cc)
        {
            // Se verifica primero para poder informar 404 en vez de un 204 enganoso.
            if (await _repo.GetByIdAsync(cc) == null)
            {
                return NotFound();
            }
            await _repo.DeleteAsync(cc);
            return NoContent();
        }
    }
}
