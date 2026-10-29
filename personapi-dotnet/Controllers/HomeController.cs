using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using personapi_dotnet.Models;

// =====================================================================
// CAPA CONTROLADOR (MVC) - HomeController
// Controlador generado por la plantilla de ASP.NET Core MVC. No accede a
// datos: solo devuelve las vistas generales del sitio (inicio, privacidad
// y pagina de error). Es el controlador por defecto de la ruta
// "{controller=Home}/{action=Index}/{id?}" definida en Program.cs.
// =====================================================================

namespace personapi_dotnet.Controllers;

/// <summary>
/// Controlador MVC de las paginas generales de la aplicacion.
/// Hereda de <see cref="Controller"/> (y no de ControllerBase) porque
/// necesita soporte de vistas Razor: <c>View()</c>, ViewData, etc.
/// </summary>
public class HomeController : Controller
{
    // Logger inyectado por el contenedor de dependencias; permite escribir
    // mensajes de diagnostico en la consola/registro de la aplicacion.
    private readonly ILogger<HomeController> _logger;

    /// <summary>
    /// Constructor. ASP.NET Core crea el controlador en cada peticion e
    /// inyecta automaticamente las dependencias que pide (inyeccion por constructor).
    /// </summary>
    /// <param name="logger">Logger especifico de este controlador.</param>
    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Pagina de inicio. Ruta de ejemplo: GET / o GET /Home/Index
    /// </summary>
    /// <returns>La vista Views/Home/Index.cshtml.</returns>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Pagina de politica de privacidad. Ruta de ejemplo: GET /Home/Privacy
    /// </summary>
    /// <returns>La vista Views/Home/Privacy.cshtml.</returns>
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// Pagina de error a la que redirige el manejador de excepciones
    /// (<c>UseExceptionHandler("/Home/Error")</c>) fuera del entorno de desarrollo.
    /// Ruta de ejemplo: GET /Home/Error
    /// </summary>
    /// <returns>La vista de error con el identificador de la peticion fallida.</returns>
    // ResponseCache con NoStore: la pagina de error nunca se guarda en cache,
    // asi cada error muestra su propio RequestId y no uno viejo.
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        // Se usa el Id de la actividad actual (trazas distribuidas) o, si no hay,
        // el identificador interno de la peticion, para poder rastrear el error en los logs.
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
