using Microsoft.AspNetCore.Mvc;
using personapi_dotnet.Interfaces;
using personapi_dotnet.Models.Entities;

// =====================================================================
// CAPA CONTROLADOR - API REST - TelefonoApiController
// Expone el CRUD de telefonos como servicio REST en JSON, documentado en
// Swagger. Usa el DAO de telefonos y el de personas, este ultimo para
// validar que el duenio (llave foranea hacia persona) exista.
// =====================================================================

namespace personapi_dotnet.Controllers.Api
{
    /// <summary>
    /// API REST de telefonos. Recurso base: <c>api/telefonos</c>.
    /// La llave es el numero (texto), por eso el parametro de ruta {num} es string.
    /// Hereda de <see cref="ControllerBase"/> porque no usa vistas.
    /// </summary>
    // [ApiController]: validacion automatica del modelo (400), body JSON para el
    // parametro Telefono y rutas por atributo obligatorias.
    [Route("api/telefonos")]
    [ApiController]
    public class TelefonoApiController : ControllerBase
    {
        // DAO de telefonos.
        private readonly ITelefonoRepository _repo;
        // DAO de personas, para comprobar que el duenio exista.
        private readonly IPersonaRepository _personaRepo;

        /// <summary>
        /// Constructor con inyeccion de dependencias.
        /// </summary>
        /// <param name="repo">Repositorio (DAO) de telefonos.</param>
        /// <param name="personaRepo">Repositorio de personas (validacion de FK del duenio).</param>
        public TelefonoApiController(ITelefonoRepository repo, IPersonaRepository personaRepo)
        {
            _repo = repo;
            _personaRepo = personaRepo;
        }

        /// <summary>
        /// Lista todos los telefonos. Ejemplo: GET api/telefonos
        /// </summary>
        /// <returns>200 OK con el arreglo JSON de telefonos.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Telefono>>> GetAll()
        {
            return Ok(await _repo.GetAllAsync());
        }

        /// <summary>
        /// Consulta un telefono por numero. Ejemplo: GET api/telefonos/3001234567
        /// </summary>
        /// <param name="num">Numero de telefono (segmento {num} de la URL).</param>
        /// <returns>200 OK con el telefono, o 404 Not Found si no existe.</returns>
        [HttpGet("{num}")]
        public async Task<ActionResult<Telefono>> Get(string num)
        {
            var telefono = await _repo.GetByIdAsync(num);
            if (telefono == null)
            {
                return NotFound();
            }
            return Ok(telefono);
        }

        /// <summary>
        /// Crea un telefono a partir del JSON del body. Ejemplo: POST api/telefonos
        /// </summary>
        /// <param name="telefono">Telefono recibido en el cuerpo (num, oper, duenio).</param>
        /// <returns>201 Created con header Location, 409 si el numero ya existe, o 400 si el duenio no existe.</returns>
        [HttpPost]
        public async Task<ActionResult<Telefono>> Create(Telefono telefono)
        {
            // 409 Conflict: el numero (llave primaria) ya esta registrado.
            if (await _repo.GetByIdAsync(telefono.Num) != null)
            {
                return Conflict("Ya existe ese numero");
            }
            // el duenio tiene que existir por la llave foranea
            // Si no se valida, SQL Server rechazaria el INSERT y el cliente veria un
            // error 500; con esta comprobacion recibe un 400 Bad Request explicativo.
            if (await _personaRepo.GetByIdAsync(telefono.Duenio) == null)
            {
                return BadRequest("No existe una persona con esa cedula");
            }
            await _repo.AddAsync(telefono);
            // 201 Created + header Location: /api/telefonos/{num}.
            return CreatedAtAction(nameof(Get), new { num = telefono.Num }, telefono);
        }

        /// <summary>
        /// Actualiza operador y/o duenio de un telefono. Ejemplo: PUT api/telefonos/3001234567
        /// </summary>
        /// <param name="num">Numero de la URL.</param>
        /// <param name="telefono">Datos nuevos en el body.</param>
        /// <returns>204 No Content, 400 si el numero no coincide o el duenio no existe, 404 si el telefono no existe.</returns>
        [HttpPut("{num}")]
        public async Task<IActionResult> Update(string num, Telefono telefono)
        {
            // 400: la URL y el body deben referirse al mismo telefono.
            if (num != telefono.Num)
            {
                return BadRequest("El numero de la url no coincide con el del body");
            }
            // 404: no se puede actualizar un telefono que no existe.
            if (await _repo.GetByIdAsync(num) == null)
            {
                return NotFound();
            }
            // Aqui tambien se valida la FK porque el duenio SI puede cambiar en un PUT.
            if (await _personaRepo.GetByIdAsync(telefono.Duenio) == null)
            {
                return BadRequest("No existe una persona con esa cedula");
            }
            await _repo.UpdateAsync(telefono);
            // 204: exito sin cuerpo.
            return NoContent();
        }

        /// <summary>
        /// Elimina un telefono. Ejemplo: DELETE api/telefonos/3001234567
        /// </summary>
        /// <param name="num">Numero del telefono a eliminar.</param>
        /// <returns>204 No Content si se borro, o 404 Not Found si no existe.</returns>
        [HttpDelete("{num}")]
        public async Task<IActionResult> Delete(string num)
        {
            if (await _repo.GetByIdAsync(num) == null)
            {
                return NotFound();
            }
            await _repo.DeleteAsync(num);
            return NoContent();
        }
    }
}
