// =====================================================================
// Persona.cs - CAPA MODELO (entidad de EF Core).
// Representa una fila de la tabla "persona" de la base persona_db.
// La clase se genero con scaffolding (Database First) a partir de la BD;
// el mapeo exacto a columnas (nombres, tamanios, tipos) esta en
// PersonaDbContext.OnModelCreating. Es "partial" para poder extenderla en
// otro archivo sin perder los cambios si se vuelve a generar.
// =====================================================================

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace personapi_dotnet.Models.Entities;

/// <summary>
/// Entidad Persona: datos basicos de una persona identificada por su cedula.
/// Es la entidad "padre" de <see cref="Telefono"/> (una persona tiene muchos
/// telefonos) y de <see cref="Estudio"/> (una persona tiene muchos estudios).
/// </summary>
public partial class Persona
{
    /// <summary>
    /// Cedula de ciudadania (columna "cc"). Es la llave primaria y la digita
    /// el usuario: no es autoincremental (ValueGeneratedNever en el DbContext).
    /// </summary>
    public int Cc { get; set; }

    // "= null!" : en C# con tipos de referencia no anulables (nullable
    // enabled) el compilador avisa si un string no anulable no se inicializa.
    // null! le dice "confia, EF Core lo llenara al leer de la BD o el model
    // binding al recibir el formulario", y quita la advertencia sin cambiar
    // el comportamiento. Indica ademas que la columna es NOT NULL.

    /// <summary>Nombre de la persona (varchar(45), obligatorio).</summary>
    public string Nombre { get; set; } = null!;

    /// <summary>Apellido de la persona (varchar(45), obligatorio).</summary>
    public string Apellido { get; set; } = null!;

    /// <summary>Genero de la persona: un solo caracter fijo, char(1) (p. ej. "M" o "F").</summary>
    public string Genero { get; set; } = null!;

    /// <summary>Edad en anios. Es int? porque la columna admite NULL (dato opcional).</summary>
    public int? Edad { get; set; }

    // ----- Propiedades de navegacion (relaciones 1 a N) -----
    // Son "virtual" (convencion del scaffolding) para que EF Core pudiera usar
    // proxies de lazy loading si se habilitaran; en este proyecto no se
    // habilitan, las relaciones se cargan explicitamente con Include() en los
    // repositorios. Se inicializan con una lista vacia para evitar
    // NullReferenceException al recorrerlas.
    //
    // se ignoran en el json para que no salga el ciclo persona -> telefono -> persona
    // [JsonIgnore]: al serializar una Persona en la API REST, si se incluyeran
    // sus telefonos, cada telefono volveria a incluir a su duenio y asi
    // infinitamente (ciclo de referencias) -> error del serializador. Ademas
    // la API solo debe exponer los datos propios de la persona.
    // [ValidateNever]: el model binding de MVC no valida estas colecciones al
    // recibir un formulario o JSON; de lo contrario podria marcar el modelo
    // como invalido por datos que el formulario nunca envia.

    /// <summary>Estudios (profesiones cursadas) de esta persona. Relacion por estudios.cc_per.</summary>
    [JsonIgnore]
    [ValidateNever]
    public virtual ICollection<Estudio> Estudios { get; set; } = new List<Estudio>();

    /// <summary>Telefonos cuyo duenio es esta persona. Relacion por telefono.duenio.</summary>
    [JsonIgnore]
    [ValidateNever]
    public virtual ICollection<Telefono> Telefonos { get; set; } = new List<Telefono>();
}
