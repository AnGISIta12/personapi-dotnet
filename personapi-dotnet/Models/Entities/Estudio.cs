using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace personapi_dotnet.Models.Entities;

public partial class Estudio
{
    public int IdProf { get; set; }

    public int CcPer { get; set; }

    public DateTime? Fecha { get; set; }

    public string? Univer { get; set; }

    // ValidateNever: asi el formulario no pide la persona ni la profesion completas
    [JsonIgnore]
    [ValidateNever]
    public virtual Persona CcPerNavigation { get; set; } = null!;

    [JsonIgnore]
    [ValidateNever]
    public virtual Profesion IdProfNavigation { get; set; } = null!;
}
