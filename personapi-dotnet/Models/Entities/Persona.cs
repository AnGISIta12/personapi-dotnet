using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace personapi_dotnet.Models.Entities;

public partial class Persona
{
    public int Cc { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public string Genero { get; set; } = null!;

    public int? Edad { get; set; }

    // JsonIgnore evita el ciclo persona -> telefono -> persona
    [JsonIgnore]
    [ValidateNever]
    public virtual ICollection<Estudio> Estudios { get; set; } = new List<Estudio>();

    [JsonIgnore]
    [ValidateNever]
    public virtual ICollection<Telefono> Telefonos { get; set; } = new List<Telefono>();
}
