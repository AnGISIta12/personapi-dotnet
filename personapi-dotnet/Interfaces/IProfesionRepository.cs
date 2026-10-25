using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Interfaces
{
    // DAO de profesion
    public interface IProfesionRepository
    {
        Task<IEnumerable<Profesion>> GetAllAsync();
        Task<Profesion?> GetByIdAsync(int id);
        Task AddAsync(Profesion profesion);
        Task UpdateAsync(Profesion profesion);
        Task DeleteAsync(int id);
    }
}
