// =====================================================================
// TelefonoRepository.cs - CAPA DAO (implementacion).
//
// Implementacion concreta de ITelefonoRepository con EF Core. Encapsula
// todo el acceso a la tabla "telefono"; Program.cs la registra con
// AddScoped y los controladores la usan a traves de la interfaz.
// =====================================================================

using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Interfaces;
using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Repositories
{
    /// <summary>
    /// DAO de telefonos implementado con Entity Framework Core sobre
    /// <see cref="PersonaDbContext"/>.
    /// </summary>
    public class TelefonoRepository : ITelefonoRepository
    {
        // Contexto de EF Core de la peticion actual (inyectado, Scoped).
        private readonly PersonaDbContext _context;

        /// <summary>
        /// Recibe el <see cref="PersonaDbContext"/> por inyeccion de dependencias.
        /// </summary>
        /// <param name="context">Contexto de EF Core de la peticion actual.</param>
        public TelefonoRepository(PersonaDbContext context)
        {
            _context = context;
        }

        // se incluye el duenio para mostrar el nombre en las vistas
        // Include hace un JOIN con persona y llena DuenioNavigation en la misma
        // consulta (eager loading); sin el, la navegacion quedaria en null.
        /// <inheritdoc/>
        public async Task<IEnumerable<Telefono>> GetAllAsync()
        {
            return await _context.Telefonos
                .Include(t => t.DuenioNavigation)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<Telefono?> GetByIdAsync(string num)
        {
            // FirstOrDefaultAsync en vez de FindAsync porque FindAsync no admite
            // Include. Devuelve el telefono con su duenio, o null si no existe.
            return await _context.Telefonos
                .Include(t => t.DuenioNavigation)
                .FirstOrDefaultAsync(t => t.Num == num);
        }

        /// <inheritdoc/>
        public async Task AddAsync(Telefono telefono)
        {
            // Solo se necesita la FK Duenio; si esa cedula no existe la BD
            // rechaza el INSERT por la llave foranea telefono_persona_fk.
            _context.Telefonos.Add(telefono);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(Telefono telefono)
        {
            // se busca el registro actual y se le copian los valores nuevos
            // FindAsync por llave primaria (el numero) + SetValues: se actualizan
            // solo Oper y/o Duenio si cambiaron. El numero, al ser la llave, no
            // se puede modificar con este metodo.
            var actual = await _context.Telefonos.FindAsync(telefono.Num);
            if (actual != null)
            {
                _context.Entry(actual).CurrentValues.SetValues(telefono);
                await _context.SaveChangesAsync();
            }
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(string num)
        {
            // Telefono no tiene tablas hijas, se borra directamente.
            var telefono = await _context.Telefonos.FindAsync(num);
            if (telefono != null)
            {
                _context.Telefonos.Remove(telefono);
                await _context.SaveChangesAsync();
            }
        }
    }
}
