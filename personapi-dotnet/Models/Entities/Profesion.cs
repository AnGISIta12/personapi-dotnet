// =====================================================================
// Profesion.cs - CAPA MODELO (entidad de EF Core).
// Representa una fila de la tabla "profesion". Generada por scaffolding
// (Database First); el mapeo a columnas esta en
// PersonaDbContext.OnModelCreating.
// =====================================================================

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace personapi_dotnet.Models.Entities;

/// <summary>
/// Entidad Profesion: catalogo de profesiones que una persona puede estudiar.
/// Se relaciona con <see cref="Persona"/> de forma muchos a muchos a traves
/// de la tabla intermedia <see cref="Estudio"/>.
/// </summary>
public partial class Profesion
{
    /// <summary>
    /// Identificador de la profesion (columna "id", llave primaria).
    /// No es autoincremental: el valor lo asigna el usuario (ValueGeneratedNever).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nombre de la profesion (varchar(90), obligatorio). "= null!" quita la
    /// advertencia de nullable: el valor lo llena EF Core o el model binding.
    /// </summary>
    public string Nom { get; set; } = null!;

    /// <summary>Descripcion de la profesion (texto largo, opcional: por eso string?).</summary>
    public string? Des { get; set; }

    // [JsonIgnore] evita el ciclo profesion -> estudio -> profesion al
    // serializar a JSON en la API; [ValidateNever] evita que MVC valide esta
    // coleccion cuando llega un formulario de profesion.

    /// <summary>Estudios (personas) asociados a esta profesion. Relacion por estudios.id_prof.</summary>
    [JsonIgnore]
    [ValidateNever]
    public virtual ICollection<Estudio> Estudios { get; set; } = new List<Estudio>();
}
