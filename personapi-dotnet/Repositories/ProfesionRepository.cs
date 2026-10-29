// =====================================================================
// ProfesionRepository.cs - CAPA DAO (implementacion).
//
// Implementacion concreta de IProfesionRepository con EF Core. Encapsula
// todo el acceso a la tabla "profesion"; los controladores la usan a
// traves de la interfaz y Program.cs la registra con AddScoped.
// =====================================================================

using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Interfaces;
using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Repositories
{
    /// <summary>
    /// DAO de profesiones implementado con Entity Framework Core sobre
    /// <see cref="PersonaDbContext"/>.
    /// </summary>
    public class ProfesionRepository : IProfesionRepository
    {
        // Contexto de EF Core de la peticion actual (inyectado, Scoped).
        private readonly PersonaDbContext _context;

        /// <summary>
        /// Recibe el <see cref="PersonaDbContext"/> por inyeccion de dependencias.
        /// </summary>
        /// <param name="context">Contexto de EF Core de la peticion actual.</param>
        public ProfesionRepository(PersonaDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Profesion>> GetAllAsync()
        {
            // ToListAsync ejecuta el SELECT y trae todas las filas a memoria.
            // No se necesita Include: las vistas de profesion no muestran estudios.
            return await _context.Profesions.ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<Profesion?> GetByIdAsync(int id)
        {
            // FindAsync busca por llave primaria y aprovecha la cache del
            // change tracker (si ya esta cargada no consulta la BD).
            return await _context.Profesions.FindAsync(id);
        }

        /// <inheritdoc/>
        public async Task AddAsync(Profesion profesion)
        {
            // Add marca la entidad como nueva; SaveChangesAsync ejecuta el INSERT.
            _context.Profesions.Add(profesion);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(Profesion profesion)
        {
            // se busca el registro actual y se le copian los valores nuevos
            // (SetValues sobre la entidad rastreada en lugar de Update() sobre el
            // objeto recibido: evita conflictos de rastreo y EF solo actualiza
            // las columnas que realmente cambiaron. Ver PersonaRepository.)
            var actual = await _context.Profesions.FindAsync(profesion.Id);
            if (actual != null)
            {
                _context.Entry(actual).CurrentValues.SetValues(profesion);
                await _context.SaveChangesAsync();
            }
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(int id)
        {
            var profesion = await _context.Profesions.FindAsync(id);
            if (profesion != null)
            {
                // se borran los estudios que usan esta profesion
                // La FK estudio_profesion_fk no tiene borrado en cascada, asi
                // que si quedaran estudios apuntando a esta profesion SQL
                // Server rechazaria el DELETE. Las personas NO se tocan.
                _context.Estudios.RemoveRange(_context.Estudios.Where(e => e.IdProf == id));
                _context.Profesions.Remove(profesion);
                // Todo se confirma en una sola transaccion.
                await _context.SaveChangesAsync();
            }
        }
    }
}
