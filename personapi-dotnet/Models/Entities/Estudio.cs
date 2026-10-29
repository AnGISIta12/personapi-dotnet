// =====================================================================
// Estudio.cs - CAPA MODELO (entidad de EF Core).
// Representa una fila de la tabla "estudios", que es la tabla intermedia
// de la relacion muchos a muchos Persona <-> Profesion, con datos propios
// (fecha y universidad). Generada por scaffolding (Database First); el
// mapeo y la llave compuesta se definen en PersonaDbContext.OnModelCreating.
// =====================================================================

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace personapi_dotnet.Models.Entities;

/// <summary>
/// Entidad Estudio: indica que una persona estudio una profesion, en que
/// universidad y en que fecha.
/// La llave primaria es COMPUESTA por (<see cref="IdProf"/>, <see cref="CcPer"/>):
/// una misma persona no puede registrar dos veces la misma profesion. Por eso
/// los metodos del DAO de estudios reciben siempre los dos valores.
/// </summary>
public partial class Estudio
{
    /// <summary>
    /// Id de la profesion estudiada (columna "id_prof"). Parte de la llave
    /// primaria y llave foranea hacia profesion.id.
    /// </summary>
    public int IdProf { get; set; }

    /// <summary>
    /// Cedula de la persona que estudio (columna "cc_per"). Parte de la llave
    /// primaria y llave foranea hacia persona.cc.
    /// </summary>
    public int CcPer { get; set; }

    /// <summary>Fecha de grado (columna tipo date, opcional: por eso DateTime?).</summary>
    public DateTime? Fecha { get; set; }

    /// <summary>Universidad donde se realizo el estudio (varchar(50), opcional).</summary>
    public string? Univer { get; set; }

    // ----- Propiedades de navegacion (lado "muchos" de cada relacion) -----
    // Permiten llegar desde el estudio al objeto completo de la persona y de
    // la profesion. Solo se llenan si el repositorio las carga con Include().
    // "= null!" quita la advertencia de nullable: toda fila de estudios tiene
    // persona y profesion (las FK son NOT NULL), EF Core las asigna al cargar.
    //
    // ValidateNever para que el formulario no pida la persona y la profesion completas
    // (el formulario solo envia IdProf y CcPer; sin este atributo MVC
    // marcaria ModelState como invalido porque estas propiedades no anulables
    // llegan vacias).
    // [JsonIgnore] evita ciclos (estudio -> persona -> estudios -> ...) al
    // serializar en la API; el JSON solo lleva las llaves.

    /// <summary>Persona duenia del estudio (navegacion por la FK CcPer).</summary>
    [JsonIgnore]
    [ValidateNever]
    public virtual Persona CcPerNavigation { get; set; } = null!;

    /// <summary>Profesion estudiada (navegacion por la FK IdProf).</summary>
    [JsonIgnore]
    [ValidateNever]
    public virtual Profesion IdProfNavigation { get; set; } = null!;
}
