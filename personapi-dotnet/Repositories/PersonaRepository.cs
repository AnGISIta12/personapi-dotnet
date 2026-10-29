using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Interfaces;
using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Repositories
{
    public class PersonaRepository : IPersonaRepository
    {
        private readonly PersonaDbContext _context;

        public PersonaRepository(PersonaDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Persona>> GetAllAsync()
        {
            return await _context.Personas.ToListAsync();
        }

        public async Task<Persona?> GetByIdAsync(int cc)
        {
            return await _context.Personas.FindAsync(cc);
        }

        public async Task AddAsync(Persona persona)
        {
            _context.Personas.Add(persona);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Persona persona)
        {
            var actual = await _context.Personas.FindAsync(persona.Cc);
            if (actual != null)
            {
                _context.Entry(actual).CurrentValues.SetValues(persona);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int cc)
        {
            var persona = await _context.Personas.FindAsync(cc);
            if (persona != null)
            {
                // primero sus telefonos y estudios, si no la FK no deja borrar
                _context.Telefonos.RemoveRange(_context.Telefonos.Where(t => t.Duenio == cc));
                _context.Estudios.RemoveRange(_context.Estudios.Where(e => e.CcPer == cc));
                _context.Personas.Remove(persona);
                await _context.SaveChangesAsync();
            }
        }
    }
}
