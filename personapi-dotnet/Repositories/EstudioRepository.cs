using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Interfaces;
using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Repositories
{
    public class EstudioRepository : IEstudioRepository
    {
        private readonly PersonaDbContext _context;

        public EstudioRepository(PersonaDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Estudio>> GetAllAsync()
        {
            return await _context.Estudios
                .Include(e => e.CcPerNavigation)
                .Include(e => e.IdProfNavigation)
                .ToListAsync();
        }

        public async Task<Estudio?> GetByIdAsync(int idProf, int ccPer)
        {
            return await _context.Estudios
                .Include(e => e.CcPerNavigation)
                .Include(e => e.IdProfNavigation)
                .FirstOrDefaultAsync(e => e.IdProf == idProf && e.CcPer == ccPer);
        }

        public async Task AddAsync(Estudio estudio)
        {
            _context.Estudios.Add(estudio);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Estudio estudio)
        {
            var actual = await _context.Estudios.FindAsync(estudio.IdProf, estudio.CcPer);
            if (actual != null)
            {
                _context.Entry(actual).CurrentValues.SetValues(estudio);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int idProf, int ccPer)
        {
            var estudio = await _context.Estudios.FindAsync(idProf, ccPer);
            if (estudio != null)
            {
                _context.Estudios.Remove(estudio);
                await _context.SaveChangesAsync();
            }
        }
    }
}
