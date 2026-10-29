// =====================================================================
// EstudioRepository.cs - CAPA DAO (implementacion).
//
// Implementacion concreta de IEstudioRepository con EF Core. La tabla
// "estudios" tiene llave primaria compuesta (id_prof, cc_per), por eso las
// busquedas usan siempre los dos valores. Program.cs la registra con
// AddScoped y los controladores la usan a traves de la interfaz.
// =====================================================================

using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Interfaces;
using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Repositories
{
    /// <summary>
    /// DAO de estudios implementado con Entity Framework Core sobre
    /// <see cref="PersonaDbContext"/>.
    /// </summary>
    public class EstudioRepository : IEstudioRepository
    {
        // Contexto de EF Core de la peticion actual (inyectado, Scoped).
        private readonly PersonaDbContext _context;

        /// <summary>
        /// Recibe el <see cref="PersonaDbContext"/> por inyeccion de dependencias.
        /// </summary>
        /// <param name="context">Contexto de EF Core de la peticion actual.</param>
        public EstudioRepository(PersonaDbContext context)
        {
            _context = context;
        }

        // se incluyen la persona y la profesion para mostrar los nombres en las vistas
        // Include = "eager loading": EF genera un SELECT con JOIN a persona y a
        // profesion y llena CcPerNavigation e IdProfNavigation en la misma
        // consulta. Sin Include esas propiedades quedarian en null (no hay lazy
        // loading) y la vista no podria mostrar "Nombre Apellido" ni el nombre
        // de la profesion. Tambien evita el problema N+1 (una consulta extra por fila).
        /// <inheritdoc/>
        public async Task<IEnumerable<Estudio>> GetAllAsync()
        {
            return await _context.Estudios
                .Include(e => e.CcPerNavigation)
                .Include(e => e.IdProfNavigation)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<Estudio?> GetByIdAsync(int idProf, int ccPer)
        {
            // Aqui se usa FirstOrDefaultAsync y no FindAsync porque FindAsync no
            // se puede combinar con Include (solo busca por llave).
            // FirstOrDefaultAsync arma un SELECT con WHERE id_prof = @p AND
            // cc_per = @p y los JOIN del Include; siempre va a la BD y devuelve
            // el primer resultado o null si no hay coincidencia.
            return await _context.Estudios
                .Include(e => e.CcPerNavigation)
                .Include(e => e.IdProfNavigation)
                .FirstOrDefaultAsync(e => e.IdProf == idProf && e.CcPer == ccPer);
        }

        /// <inheritdoc/>
        public async Task AddAsync(Estudio estudio)
        {
            // Basta con asignar las FK (IdProf y CcPer); no hace falta cargar las
            // navegaciones. Si la persona o la profesion no existen, SQL Server
            // rechaza el INSERT por las llaves foraneas.
            _context.Estudios.Add(estudio);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(Estudio estudio)
        {
            // se busca el registro actual y se le copian los valores nuevos
            // FindAsync con llave compuesta: los valores van en el MISMO orden en
            // que se declaro HasKey en el DbContext (IdProf, CcPer).
            // SetValues copia solo las propiedades escalares (Fecha, Univer; las
            // llaves no cambian) y EF actualiza unicamente las columnas modificadas.
            var actual = await _context.Estudios.FindAsync(estudio.IdProf, estudio.CcPer);
            if (actual != null)
            {
                _context.Entry(actual).CurrentValues.SetValues(estudio);
                await _context.SaveChangesAsync();
            }
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(int idProf, int ccPer)
        {
            // Estudio no tiene tablas hijas, asi que se puede borrar directamente.
            var estudio = await _context.Estudios.FindAsync(idProf, ccPer);
            if (estudio != null)
            {
                _context.Estudios.Remove(estudio);
                await _context.SaveChangesAsync();
            }
        }
    }
}
