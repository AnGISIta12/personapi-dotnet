// =====================================================================
// IEstudioRepository.cs - CAPA DAO (contrato / interfaz).
// Define QUE operaciones de datos hay sobre estudios; la implementacion
// con EF Core esta en Repositories/EstudioRepository.cs y se enlaza en
// Program.cs con AddScoped<IEstudioRepository, EstudioRepository>().
// Los controladores solo conocen esta interfaz (patron DAO).
// =====================================================================

using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Interfaces
{
    /// <summary>
    /// DAO de estudios, la llave es compuesta (id_prof, cc_per).
    /// Por eso buscar y eliminar reciben DOS parametros: un estudio solo queda
    /// identificado por la pareja profesion + persona.
    /// </summary>
    public interface IEstudioRepository
    {
        /// <summary>Obtiene todos los estudios, con su persona y profesion cargadas.</summary>
        /// <returns>Coleccion de estudios (vacia si no hay ninguno).</returns>
        Task<IEnumerable<Estudio>> GetAllAsync();

        /// <summary>Busca un estudio por su llave compuesta.</summary>
        /// <param name="idProf">Id de la profesion.</param>
        /// <param name="ccPer">Cedula de la persona.</param>
        /// <returns>El estudio (con persona y profesion cargadas) o null si no existe.</returns>
        Task<Estudio?> GetByIdAsync(int idProf, int ccPer);

        /// <summary>Inserta un nuevo estudio.</summary>
        /// <param name="estudio">Estudio a crear; la persona y la profesion deben existir (FK).</param>
        Task AddAsync(Estudio estudio);

        /// <summary>Actualiza fecha y universidad de un estudio existente.</summary>
        /// <param name="estudio">Estudio con los valores nuevos; se ubica por (IdProf, CcPer).</param>
        Task UpdateAsync(Estudio estudio);

        /// <summary>Elimina un estudio por su llave compuesta.</summary>
        /// <param name="idProf">Id de la profesion.</param>
        /// <param name="ccPer">Cedula de la persona.</param>
        Task DeleteAsync(int idProf, int ccPer);
    }
}
