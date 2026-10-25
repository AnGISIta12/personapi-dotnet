using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Interfaces
{
    // DAO de estudios, la llave es compuesta (id_prof, cc_per)
    public interface IEstudioRepository
    {
        Task<IEnumerable<Estudio>> GetAllAsync();
        Task<Estudio?> GetByIdAsync(int idProf, int ccPer);
        Task AddAsync(Estudio estudio);
        Task UpdateAsync(Estudio estudio);
        Task DeleteAsync(int idProf, int ccPer);
    }
}
