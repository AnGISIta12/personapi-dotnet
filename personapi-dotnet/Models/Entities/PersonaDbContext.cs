// =====================================================================
// PersonaDbContext.cs - CAPA MODELO / ACCESO A DATOS (EF Core).
//
// El DbContext es la puerta de entrada de Entity Framework Core a la base
// de datos persona_db: mantiene la conexion, rastrea los cambios de las
// entidades cargadas (change tracker) y traduce las consultas LINQ a SQL.
// Lo usan UNICAMENTE los repositorios (DAO); los controladores nunca lo
// tocan directamente.
//
// Se genero con scaffolding (Database First: Scaffold-DbContext), por eso
// la configuracion del modelo esta escrita con Fluent API en
// OnModelCreating, reflejando exactamente el esquema existente en SQL Server.
// =====================================================================

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace personapi_dotnet.Models.Entities;

/// <summary>
/// Contexto de EF Core para la base persona_db. Expone un <see cref="DbSet{TEntity}"/>
/// por tabla y define el mapeo objeto-relacional de las cuatro entidades.
/// Se registra en Program.cs con AddDbContext (ciclo de vida Scoped: una
/// instancia por peticion HTTP).
/// </summary>
public partial class PersonaDbContext : DbContext
{
    /// <summary>
    /// Constructor sin parametros generado por el scaffolding (lo usan
    /// herramientas de diseno). La aplicacion usa el constructor con opciones.
    /// </summary>
    public PersonaDbContext()
    {
    }

    /// <summary>
    /// Constructor usado por la inyeccion de dependencias: recibe las opciones
    /// configuradas en Program.cs (proveedor SQL Server y cadena de conexion
    /// "PersonaDb" de appsettings.json) y se las pasa a la clase base.
    /// </summary>
    /// <param name="options">Opciones del contexto (proveedor y cadena de conexion).</param>
    public PersonaDbContext(DbContextOptions<PersonaDbContext> options)
        : base(options)
    {
    }

    // Cada DbSet representa una tabla y es el punto de partida de las
    // consultas LINQ (p. ej. _context.Personas.ToListAsync() -> SELECT * FROM persona).

    /// <summary>Tabla "estudios" (relacion persona-profesion).</summary>
    public virtual DbSet<Estudio> Estudios { get; set; }

    /// <summary>Tabla "persona".</summary>
    public virtual DbSet<Persona> Personas { get; set; }

    /// <summary>Tabla "profesion" (el nombre Profesions lo pluralizo el scaffolding).</summary>
    public virtual DbSet<Profesion> Profesions { get; set; }

    /// <summary>Tabla "telefono".</summary>
    public virtual DbSet<Telefono> Telefonos { get; set; }

    /// <summary>
    /// Configura el modelo con Fluent API: tabla, llave primaria, nombre de
    /// columna, tipo/tamanio y relaciones (llaves foraneas) de cada entidad.
    /// EF Core lo ejecuta una sola vez al construir el modelo y lo reutiliza.
    /// Se usa Fluent API (y no data annotations) porque asi lo genera el
    /// scaffolding y deja las entidades limpias.
    /// </summary>
    /// <param name="modelBuilder">Constructor del modelo que provee EF Core.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // -------------------- ESTUDIO (tabla "estudios") --------------------
        modelBuilder.Entity<Estudio>(entity =>
        {
            // Llave primaria COMPUESTA (id_prof, cc_per): se define con un tipo
            // anonimo porque EF Core no puede inferirla por convencion (solo
            // detecta automaticamente propiedades "Id" o "<Entidad>Id").
            // HasName da el nombre de la restriccion en SQL Server.
            entity.HasKey(e => new { e.IdProf, e.CcPer }).HasName("PK_estudios");

            // Nombre real de la tabla (por defecto EF usaria el nombre del DbSet).
            entity.ToTable("estudios");

            // HasColumnName mapea la propiedad C# (PascalCase) a la columna SQL (snake_case).
            entity.Property(e => e.IdProf).HasColumnName("id_prof");
            entity.Property(e => e.CcPer).HasColumnName("cc_per");
            // HasColumnType("date"): solo fecha, sin hora (DateTime en C#).
            entity.Property(e => e.Fecha)
                .HasColumnType("date")
                .HasColumnName("fecha");
            // HasMaxLength(50) + IsUnicode(false) => varchar(50) (no nvarchar).
            entity.Property(e => e.Univer)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("univer");

            // Relacion N:1 estudio -> persona: cada estudio tiene UNA persona
            // (CcPerNavigation) y una persona tiene MUCHOS estudios (Estudios),
            // unidas por la FK cc_per.
            // DeleteBehavior.ClientSetNull: NO hay borrado en cascada; EF solo
            // pondria la FK en null en memoria, y como la columna es NOT NULL
            // la BD rechazaria borrar una persona con estudios. Por eso
            // PersonaRepository.DeleteAsync borra primero sus estudios.
            entity.HasOne(d => d.CcPerNavigation).WithMany(p => p.Estudios)
                .HasForeignKey(d => d.CcPer)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("estudio_persona_fk");

            // Relacion N:1 estudio -> profesion por la FK id_prof, tambien sin
            // cascada (ProfesionRepository.DeleteAsync borra antes los estudios).
            entity.HasOne(d => d.IdProfNavigation).WithMany(p => p.Estudios)
                .HasForeignKey(d => d.IdProf)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("estudio_profesion_fk");
        });

        // -------------------- PERSONA (tabla "persona") --------------------
        modelBuilder.Entity<Persona>(entity =>
        {
            // Llave primaria simple: la cedula.
            entity.HasKey(e => e.Cc).HasName("PK_persona");

            entity.ToTable("persona");

            // ValueGeneratedNever: la BD NO genera la cedula (no es IDENTITY);
            // EF debe enviar en el INSERT el valor que digita el usuario.
            entity.Property(e => e.Cc)
                .ValueGeneratedNever()
                .HasColumnName("cc");
            // varchar(45)
            entity.Property(e => e.Apellido)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("apellido");
            entity.Property(e => e.Edad).HasColumnName("edad");
            // HasMaxLength(1) + IsFixedLength() => char(1) (longitud fija).
            entity.Property(e => e.Genero)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("genero");
            // varchar(45)
            entity.Property(e => e.Nombre)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        // -------------------- PROFESION (tabla "profesion") --------------------
        modelBuilder.Entity<Profesion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_profesion");

            entity.ToTable("profesion");

            // El id lo asigna el usuario, no es autoincremental.
            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            // Sin HasMaxLength => varchar(max) / texto largo.
            entity.Property(e => e.Des)
                .IsUnicode(false)
                .HasColumnName("des");
            // varchar(90)
            entity.Property(e => e.Nom)
                .HasMaxLength(90)
                .IsUnicode(false)
                .HasColumnName("nom");
        });

        // -------------------- TELEFONO (tabla "telefono") --------------------
        modelBuilder.Entity<Telefono>(entity =>
        {
            // La llave primaria es el propio numero (string).
            entity.HasKey(e => e.Num).HasName("PK_telefono");

            entity.ToTable("telefono");

            // varchar(15)
            entity.Property(e => e.Num)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("num");
            entity.Property(e => e.Duenio).HasColumnName("duenio");
            // varchar(45)
            entity.Property(e => e.Oper)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("oper");

            // Relacion N:1 telefono -> persona (FK duenio), sin borrado en
            // cascada: PersonaRepository.DeleteAsync borra primero los
            // telefonos de la persona antes de borrarla.
            entity.HasOne(d => d.DuenioNavigation).WithMany(p => p.Telefonos)
                .HasForeignKey(d => d.Duenio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("telefono_persona_fk");
        });

        // Gancho para agregar configuracion extra en otro archivo "partial"
        // sin modificar este (que podria regenerarse con el scaffolding).
        OnModelCreatingPartial(modelBuilder);
    }

    /// <summary>
    /// Metodo parcial opcional: si ningun otro archivo lo implementa, el
    /// compilador elimina la llamada. Permite extender el modelo sin editar
    /// el codigo generado.
    /// </summary>
    /// <param name="modelBuilder">Constructor del modelo que provee EF Core.</param>
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
