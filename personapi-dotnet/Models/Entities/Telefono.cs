// =====================================================================
// Telefono.cs - CAPA MODELO (entidad de EF Core).
// Representa una fila de la tabla "telefono". Generada por scaffolding
// (Database First); el mapeo a columnas esta en
// PersonaDbContext.OnModelCreating.
// =====================================================================

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace personapi_dotnet.Models.Entities;

/// <summary>
/// Entidad Telefono: numero telefonico que pertenece a una <see cref="Persona"/>
/// (relacion muchos a uno: muchos telefonos, un duenio).
/// </summary>
public partial class Telefono
{
    /// <summary>
    /// Numero telefonico (columna "num", varchar(15)). Es la llave primaria;
    /// se guarda como texto para conservar ceros a la izquierda, prefijos, etc.
    /// "= null!" quita la advertencia de nullable (valor obligatorio).
    /// </summary>
    public string Num { get; set; } = null!;

    /// <summary>Operador o compania telefonica (varchar(45), obligatorio).</summary>
    public string Oper { get; set; } = null!;

    /// <summary>
    /// Cedula de la persona duenia del telefono (columna "duenio").
    /// Llave foranea hacia persona.cc.
    /// </summary>
    public int Duenio { get; set; }

    // [JsonIgnore]: evita el ciclo telefono -> persona -> telefonos -> ... al
    // serializar en la API; el JSON solo lleva la cedula del duenio.
    // [ValidateNever]: el formulario solo envia la cedula (Duenio), no la
    // persona completa, asi que no se debe validar esta navegacion.

    /// <summary>
    /// Persona duenia del telefono (navegacion por la FK Duenio). Se llena
    /// solo cuando el repositorio hace Include(t => t.DuenioNavigation).
    /// </summary>
    [JsonIgnore]
    [ValidateNever]
    public virtual Persona DuenioNavigation { get; set; } = null!;
}
