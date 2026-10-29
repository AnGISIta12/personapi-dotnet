using Microsoft.AspNetCore.Mvc;
using personapi_dotnet.Interfaces;
using personapi_dotnet.Models.Entities;

// =====================================================================
// CAPA CONTROLADOR - API REST - ProfesionApiController
// Expone el CRUD de profesiones como servicio REST en JSON, documentado en
// Swagger. Usa el mismo DAO (IProfesionRepository) que el controlador MVC.
// =====================================================================

namespace personapi_dotnet.Controllers.Api
{
    /// <summary>
    /// API REST de profesiones. Recurso base: <c>api/profesiones</c>.
    /// Hereda de <see cref="ControllerBase"/> (sin soporte de vistas) porque
    /// solo devuelve datos y codigos de estado HTTP.
    /// </summary>
    // [Route] fija la ruta base del recurso; [ApiController] valida el modelo
    // automaticamente (400 si es invalido), lee del body JSON los parametros
    // complejos y obliga a usar rutas por atributo.
    [Route("api/profesiones")]
    [ApiController]
    public class ProfesionApiController : ControllerBase
    {
        // DAO de profesiones inyectado por constructor.
        private readonly IProfesionRepository _repo;

        /// <summary>
        /// Constructor con inyeccion de dependencias.
        /// </summary>
        /// <param name="repo">Repositorio (DAO) de profesiones.</param>
        public ProfesionApiController(IProfesionRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Lista todas las profesiones. Ejemplo: GET api/profesiones
        /// </summary>
        /// <returns>200 OK con el arreglo JSON de profesiones.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Profesion>>> GetAll()
        {
            return Ok(await _repo.GetAllAsync());
        }

        /// <summary>
        /// Consulta una profesion por id. Ejemplo: GET api/profesiones/1
        /// </summary>
        /// <param name="id">Id de la profesion (segmento {id} de la URL).</param>
        /// <returns>200 OK con la profesion, o 404 Not Found si no existe.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Profesion>> Get(int id)
        {
            var profesion = await _repo.GetByIdAsync(id);
            if (profesion == null)
            {
                return NotFound();
            }
            return Ok(profesion);
        }

        /// <summary>
        /// Crea una profesion a partir del JSON del body. Ejemplo: POST api/profesiones
        /// </summary>
        /// <param name="profesion">Profesion recibida en el cuerpo de la peticion.</param>
        /// <returns>201 Created con header Location, o 409 Conflict si el id ya existe.</returns>
        [HttpPost]
        public async Task<ActionResult<Profesion>> Create(Profesion profesion)
        {
            // 409 Conflict: ya existe un recurso con esa llave primaria.
            if (await _repo.GetByIdAsync(profesion.Id) != null)
            {
                return Conflict("Ya existe una profesion con ese id");
            }
            await _repo.AddAsync(profesion);
            // 201 Created + header Location apuntando a GET api/profesiones/{id}.
            return CreatedAtAction(nameof(Get), new { id = profesion.Id }, profesion);
        }

        /// <summary>
        /// Actualiza una profesion existente. Ejemplo: PUT api/profesiones/1
        /// </summary>
        /// <param name="id">Id de la URL.</param>
        /// <param name="profesion">Datos nuevos en el body.</param>
        /// <returns>204 No Content, 400 si el id no coincide, o 404 si no existe.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Profesion profesion)
        {
            // 400: la URL y el body deben referirse al mismo recurso.
            if (id != profesion.Id)
            {
                return BadRequest("El id de la url no coincide con el del body");
            }
            // 404: no se puede actualizar lo que no existe.
            if (await _repo.GetByIdAsync(id) == null)
            {
                return NotFound();
            }
            await _repo.UpdateAsync(profesion);
            // 204: exito sin cuerpo de respuesta.
            return NoContent();
        }

        /// <summary>
        /// Elimina una profesion (el repositorio borra antes los estudios que la
        /// usan, por la llave foranea). Ejemplo: DELETE api/profesiones/1
        /// </summary>
        /// <param name="id">Id de la profesion a eliminar.</param>
        /// <returns>204 No Content si se borro, o 404 Not Found si no existe.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _repo.GetByIdAsync(id) == null)
            {
                return NotFound();
            }
            await _repo.DeleteAsync(id);
            return NoContent();
        }
    }
}
