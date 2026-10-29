using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Interfaces;
using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Repositories
{
    public class TelefonoRepository : ITelefonoRepository
    {
        private readonly PersonaDbContext _context;

        public TelefonoRepository(PersonaDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Telefono>> GetAllAsync()
        {
            return await _context.Telefonos
                .Include(t => t.DuenioNavigation)
                .ToListAsync();
        }

        public async Task<Telefono?> GetByIdAsync(string num)
        {
            return await _context.Telefonos
                .Include(t => t.DuenioNavigation)
                .FirstOrDefaultAsync(t => t.Num == num);
        }

        public async Task AddAsync(Telefono telefono)
        {
            _context.Telefonos.Add(telefono);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Telefono telefono)
        {
            var actual = await _context.Telefonos.FindAsync(telefono.Num);
            if (actual != null)
            {
                _context.Entry(actual).CurrentValues.SetValues(telefono);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(string num)
        {
            var telefono = await _context.Telefonos.FindAsync(num);
            if (telefono != null)
            {
                _context.Telefonos.Remove(telefono);
                await _context.SaveChangesAsync();
            }
        }
    }
}
