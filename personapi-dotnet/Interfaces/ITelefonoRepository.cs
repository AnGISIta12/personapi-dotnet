// =====================================================================
// ITelefonoRepository.cs - CAPA DAO (contrato / interfaz).
// Define QUE operaciones de datos hay sobre telefonos; la implementacion
// con EF Core esta en Repositories/TelefonoRepository.cs y se enlaza en
// Program.cs con AddScoped<ITelefonoRepository, TelefonoRepository>().
// Los controladores solo conocen esta interfaz (patron DAO).
// =====================================================================

using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Interfaces
{
    /// <summary>
    /// DAO de telefono, la llave es el numero (string).
    /// Define las operaciones CRUD sobre la tabla "telefono".
    /// </summary>
    public interface ITelefonoRepository
    {
        /// <summary>Obtiene todos los telefonos, con su duenio cargado.</summary>
        /// <returns>Coleccion de telefonos (vacia si no hay ninguno).</returns>
        Task<IEnumerable<Telefono>> GetAllAsync();

        /// <summary>Busca un telefono por su numero.</summary>
        /// <param name="num">Numero telefonico (llave primaria).</param>
        /// <returns>El telefono (con su duenio cargado) o null si no existe.</returns>
        Task<Telefono?> GetByIdAsync(string num);

        /// <summary>Inserta un nuevo telefono.</summary>
        /// <param name="telefono">Telefono a crear; el duenio (cedula) debe existir (FK).</param>
        Task AddAsync(Telefono telefono);

        /// <summary>Actualiza operador y duenio de un telefono existente.</summary>
        /// <param name="telefono">Telefono con los valores nuevos; se ubica por su numero.</param>
        Task UpdateAsync(Telefono telefono);

        /// <summary>Elimina un telefono por su numero.</summary>
        /// <param name="num">Numero del telefono a eliminar; si no existe no se hace nada.</param>
        Task DeleteAsync(string num);
    }
}
