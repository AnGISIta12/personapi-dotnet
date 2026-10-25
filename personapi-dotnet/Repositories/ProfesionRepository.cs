using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Interfaces;
using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Repositories
{
    public class ProfesionRepository : IProfesionRepository
    {
        private readonly PersonaDbContext _context;

        public ProfesionRepository(PersonaDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Profesion>> GetAllAsync()
        {
            return await _context.Profesions.ToListAsync();
        }

        public async Task<Profesion?> GetByIdAsync(int id)
        {
            return await _context.Profesions.FindAsync(id);
        }

        public async Task AddAsync(Profesion profesion)
        {
            _context.Profesions.Add(profesion);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Profesion profesion)
        {
            // se busca el registro actual y se le copian los valores nuevos
            var actual = await _context.Profesions.FindAsync(profesion.Id);
            if (actual != null)
            {
                _context.Entry(actual).CurrentValues.SetValues(profesion);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var profesion = await _context.Profesions.FindAsync(id);
            if (profesion != null)
            {
                // se borran los estudios que usan esta profesion
                _context.Estudios.RemoveRange(_context.Estudios.Where(e => e.IdProf == id));
                _context.Profesions.Remove(profesion);
                await _context.SaveChangesAsync();
            }
        }
    }
}
