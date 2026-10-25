using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace personapi_dotnet.Models.Entities;

public partial class Profesion
{
    public int Id { get; set; }

    public string Nom { get; set; } = null!;

    public string? Des { get; set; }

    [JsonIgnore]
    [ValidateNever]
    public virtual ICollection<Estudio> Estudios { get; set; } = new List<Estudio>();
}
