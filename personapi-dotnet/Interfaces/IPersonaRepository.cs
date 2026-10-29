// =====================================================================
// IPersonaRepository.cs - CAPA DAO (contrato / interfaz).
//
// Patron DAO (Data Access Object): separa la logica de acceso a datos del
// resto de la aplicacion. Se divide en dos piezas:
//   - La INTERFAZ (este archivo): define QUE operaciones de datos existen,
//     sin decir COMO se hacen.
//   - La IMPLEMENTACION (Repositories/PersonaRepository.cs): hace el trabajo
//     real con EF Core y SQL Server.
// Los controladores dependen solo de la interfaz y Program.cs decide, con
// AddScoped, que implementacion se inyecta. Asi se podria cambiar la
// tecnologia de persistencia (o usar un DAO falso en pruebas) sin tocar
// los controladores.
//
// Todos los metodos son asincronos (Task): mientras la BD responde, el hilo
// queda libre para atender otras peticiones.
// =====================================================================

using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Interfaces
{
    /// <summary>
    /// DAO de persona, la llave es la cedula (cc).
    /// Define las operaciones CRUD sobre la tabla "persona".
    /// </summary>
    public interface IPersonaRepository
    {
        /// <summary>Obtiene todas las personas registradas.</summary>
        /// <returns>Coleccion con todas las personas (vacia si no hay ninguna).</returns>
        Task<IEnumerable<Persona>> GetAllAsync();

        /// <summary>Busca una persona por su cedula.</summary>
        /// <param name="cc">Cedula de la persona (llave primaria).</param>
        /// <returns>La persona encontrada o null si no existe.</returns>
        Task<Persona?> GetByIdAsync(int cc);

        /// <summary>Inserta una nueva persona en la base de datos.</summary>
        /// <param name="persona">Persona a crear; su cedula no debe existir.</param>
        Task AddAsync(Persona persona);

        /// <summary>Actualiza los datos de una persona existente (se identifica por su cedula).</summary>
        /// <param name="persona">Persona con los valores nuevos; si la cedula no existe no se hace nada.</param>
        Task UpdateAsync(Persona persona);

        /// <summary>
        /// Elimina una persona junto con sus telefonos y estudios
        /// (necesario porque las llaves foraneas no tienen borrado en cascada).
        /// </summary>
        /// <param name="cc">Cedula de la persona a eliminar; si no existe no se hace nada.</param>
        Task DeleteAsync(int cc);
    }
}
