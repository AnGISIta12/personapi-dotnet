using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Interfaces
{
    // DAO de telefono, la llave es el numero
    public interface ITelefonoRepository
    {
        Task<IEnumerable<Telefono>> GetAllAsync();
        Task<Telefono?> GetByIdAsync(string num);
        Task AddAsync(Telefono telefono);
        Task UpdateAsync(Telefono telefono);
        Task DeleteAsync(string num);
    }
}
