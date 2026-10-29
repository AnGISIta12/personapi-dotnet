// =====================================================================
// ErrorViewModel.cs - CAPA MODELO (modelo de vista, no es entidad de BD).
// Lo genera la plantilla MVC. Transporta los datos que la vista
// Views/Shared/Error.cshtml necesita para mostrar la pagina de error a la
// que redirige UseExceptionHandler("/Home/Error") en Program.cs.
// =====================================================================

namespace personapi_dotnet.Models;

/// <summary>
/// Modelo de vista para la pagina de error. No se guarda en la base de datos.
/// </summary>
public class ErrorViewModel
{
    /// <summary>
    /// Identificador de la peticion que fallo (Activity.Id o TraceIdentifier).
    /// Sirve para buscar el error en los logs.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Propiedad calculada (solo lectura): indica a la vista si hay un
    /// RequestId que mostrar.
    /// </summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
