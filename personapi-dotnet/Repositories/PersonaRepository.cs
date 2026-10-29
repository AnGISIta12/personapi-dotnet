// =====================================================================
// PersonaRepository.cs - CAPA DAO (implementacion).
//
// Implementacion concreta del contrato IPersonaRepository usando EF Core.
// Es la UNICA clase que sabe como se leen y escriben personas en SQL
// Server; los controladores la usan a traves de la interfaz, y Program.cs
// la registra con AddScoped (una instancia por peticion HTTP).
// =====================================================================

using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Interfaces;
using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Repositories
{
    /// <summary>
    /// DAO de personas implementado con Entity Framework Core sobre
    /// <see cref="PersonaDbContext"/>.
    /// </summary>
    public class PersonaRepository : IPersonaRepository
    {
        // DbContext inyectado. Es readonly porque se asigna una sola vez en el
        // constructor. Al ser Scoped, este repositorio y el controlador que lo
        // usa comparten el mismo contexto durante toda la peticion.
        private readonly PersonaDbContext _context;

        /// <summary>
        /// Inyeccion de dependencias por constructor: el contenedor de DI crea
        /// el <see cref="PersonaDbContext"/> de la peticion y lo entrega aqui;
        /// el repositorio nunca lo instancia con "new".
        /// </summary>
        /// <param name="context">Contexto de EF Core de la peticion actual.</param>
        public PersonaRepository(PersonaDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Persona>> GetAllAsync()
        {
            // ToListAsync ejecuta la consulta (SELECT ... FROM persona) y
            // materializa el resultado en una lista. Hasta ese momento la
            // consulta LINQ solo estaba "armada", no ejecutada.
            return await _context.Personas.ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<Persona?> GetByIdAsync(int cc)
        {
            // FindAsync busca por LLAVE PRIMARIA: primero revisa si la entidad
            // ya esta cargada en el change tracker del contexto (sin ir a la BD)
            // y solo si no esta hace el SELECT. Devuelve null si no existe.
            return await _context.Personas.FindAsync(cc);
        }

        /// <inheritdoc/>
        public async Task AddAsync(Persona persona)
        {
            // Add solo marca la entidad como "Added" en memoria (change tracker).
            _context.Personas.Add(persona);
            // SaveChangesAsync revisa todos los cambios pendientes, genera los
            // INSERT/UPDATE/DELETE correspondientes y los ejecuta en una sola
            // transaccion contra SQL Server.
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(Persona persona)
        {
            // se busca el registro actual y se le copian los valores nuevos
            // Por que SetValues y no _context.Update(persona)?
            //  - "persona" llega del formulario/JSON y NO esta rastreada por el
            //    contexto. Update() la adjuntaria y marcaria TODAS sus columnas
            //    como modificadas, y podria fallar si ya hay otra instancia con
            //    la misma llave rastreada.
            //  - Con FindAsync se obtiene la entidad rastreada real y SetValues
            //    copia solo las propiedades escalares; EF detecta cuales
            //    cambiaron y el UPDATE incluye solo esas columnas. Ademas se
            //    puede verificar si el registro existe.
            var actual = await _context.Personas.FindAsync(persona.Cc);
            if (actual != null)
            {
                _context.Entry(actual).CurrentValues.SetValues(persona);
                await _context.SaveChangesAsync();
            }
            // Si no existe no se hace nada (no se lanza error); por eso el
            // controlador de API verifica antes la existencia para responder 404.
            // Nota: en ese caso el GetByIdAsync previo ya dejo la entidad en el
            // change tracker, asi que este FindAsync la toma de memoria sin
            // volver a consultar la BD.
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(int cc)
        {
            var persona = await _context.Personas.FindAsync(cc);
            if (persona != null)
            {
                // primero se borran los telefonos y estudios de la persona por las llaves foraneas
                // Las FK telefono_persona_fk y estudio_persona_fk NO tienen
                // ON DELETE CASCADE (DeleteBehavior.ClientSetNull en el
                // DbContext), asi que SQL Server rechazaria borrar una persona
                // que aun tenga hijos. RemoveRange marca todos los hijos para
                // borrar y EF ordena los DELETE (hijos antes que el padre).
                _context.Telefonos.RemoveRange(_context.Telefonos.Where(t => t.Duenio == cc));
                _context.Estudios.RemoveRange(_context.Estudios.Where(e => e.CcPer == cc));
                _context.Personas.Remove(persona);
                // Un solo SaveChangesAsync => una sola transaccion: o se borra
                // todo (telefonos, estudios y persona) o no se borra nada.
                await _context.SaveChangesAsync();
            }
        }
    }
}
