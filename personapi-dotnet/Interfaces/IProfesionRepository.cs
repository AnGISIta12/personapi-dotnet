// =====================================================================
// IProfesionRepository.cs - CAPA DAO (contrato / interfaz).
// Define QUE operaciones de datos hay sobre profesiones; la implementacion
// con EF Core esta en Repositories/ProfesionRepository.cs y se enlaza en
// Program.cs con AddScoped<IProfesionRepository, ProfesionRepository>().
// Los controladores solo conocen esta interfaz (patron DAO).
// =====================================================================

using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Interfaces
{
    /// <summary>
    /// DAO de profesion. Define las operaciones CRUD sobre la tabla "profesion";
    /// la llave es el id de la profesion.
    /// </summary>
    public interface IProfesionRepository
    {
        /// <summary>Obtiene todas las profesiones registradas.</summary>
        /// <returns>Coleccion con todas las profesiones (vacia si no hay ninguna).</returns>
        Task<IEnumerable<Profesion>> GetAllAsync();

        /// <summary>Busca una profesion por su id.</summary>
        /// <param name="id">Id de la profesion (llave primaria).</param>
        /// <returns>La profesion encontrada o null si no existe.</returns>
        Task<Profesion?> GetByIdAsync(int id);

        /// <summary>Inserta una nueva profesion.</summary>
        /// <param name="profesion">Profesion a crear; su id no debe existir.</param>
        Task AddAsync(Profesion profesion);

        /// <summary>Actualiza nombre y descripcion de una profesion existente.</summary>
        /// <param name="profesion">Profesion con los valores nuevos; si el id no existe no se hace nada.</param>
        Task UpdateAsync(Profesion profesion);

        /// <summary>
        /// Elimina una profesion y los estudios que la referencian
        /// (la FK de estudios no tiene borrado en cascada).
        /// </summary>
        /// <param name="id">Id de la profesion a eliminar; si no existe no se hace nada.</param>
        Task DeleteAsync(int id);
    }
}
