// =====================================================================
// Program.cs - Punto de entrada y CONFIGURACION de la aplicacion.
//
// Rol en la arquitectura: es la "raiz de composicion" (composition root).
// Aqui se registran en el contenedor de inyeccion de dependencias (DI)
// todas las piezas de las capas (MVC, DbContext de EF Core, DAO/repositorios,
// Swagger) y luego se arma el pipeline de middlewares que procesa cada
// peticion HTTP. Ninguna otra clase crea sus dependencias con "new": las
// recibe por constructor y es este archivo el que decide que implementacion
// concreta se entrega para cada interfaz.
//
// Se usa el modelo de "top-level statements" (.NET 6+): no hay clase
// Program ni metodo Main explicitos, el compilador los genera.
// =====================================================================

using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Interfaces;
using personapi_dotnet.Models.Entities;
using personapi_dotnet.Repositories;

// El builder carga la configuracion (appsettings.json, appsettings.{Entorno}.json,
// variables de entorno, argumentos de linea de comandos), el logging y expone
// builder.Services, que es la coleccion donde se registran los servicios de DI.
var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------
// 1. REGISTRO DE SERVICIOS (contenedor de inyeccion de dependencias)
// ---------------------------------------------------------------------

// Add services to the container.
// Registra lo necesario para MVC: controladores, vistas Razor, model binding,
// validacion de modelos, etc. Sirve tanto para los controladores con vistas
// como para los controladores de API ([ApiController]) del proyecto.
builder.Services.AddControllersWithViews();

// conexion a SQL Server, la cadena esta en appsettings.json
// AddDbContext registra PersonaDbContext con ciclo de vida Scoped (una
// instancia por peticion HTTP). UseSqlServer indica el proveedor de EF Core
// y GetConnectionString("PersonaDb") lee la seccion
// "ConnectionStrings:PersonaDb" de appsettings.json, asi la cadena no queda
// quemada en el codigo y puede cambiar segun el entorno.
builder.Services.AddDbContext<PersonaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PersonaDb")));

// repositorios (DAO)
// Cada linea asocia una interfaz DAO (contrato) con su implementacion concreta.
// Los controladores piden la interfaz (p. ej. IPersonaRepository) en su
// constructor y el contenedor les entrega la clase concreta; asi la capa de
// presentacion no depende de EF Core ni de SQL Server (principio de inversion
// de dependencias) y la implementacion se podria cambiar tocando solo aqui.
//
// AddScoped = se crea UNA instancia por peticion HTTP y se comparte dentro de
// esa peticion; al terminar la peticion se desecha. Es el ciclo de vida
// correcto porque los repositorios dependen del DbContext, que tambien es
// Scoped y NO es seguro para usarse entre hilos/peticiones concurrentes.
// (Alternativas: AddTransient = una instancia nueva cada vez que se pide;
//  AddSingleton = una sola instancia para toda la vida de la aplicacion,
//  que aqui seria un error porque retendria un DbContext compartido.)
builder.Services.AddScoped<IPersonaRepository, PersonaRepository>();
builder.Services.AddScoped<IProfesionRepository, ProfesionRepository>();
builder.Services.AddScoped<IEstudioRepository, EstudioRepository>();
builder.Services.AddScoped<ITelefonoRepository, TelefonoRepository>();

// swagger
// AddEndpointsApiExplorer descubre los endpoints de la API y AddSwaggerGen
// genera a partir de ellos el documento OpenAPI (swagger.json) que describe
// rutas, parametros y modelos de la API REST.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Build() "congela" el registro de servicios y crea la aplicacion web.
// A partir de aqui ya no se pueden registrar servicios, solo middlewares.
var app = builder.Build();

// ---------------------------------------------------------------------
// 2. PIPELINE DE MIDDLEWARES
// Cada peticion HTTP pasa por estos componentes EN EL ORDEN en que se
// agregan; cada uno puede atenderla, modificarla o pasarla al siguiente.
// Por eso el orden importa (p. ej. UseRouting debe ir antes de
// UseAuthorization y de MapControllerRoute).
// ---------------------------------------------------------------------

// swagger queda disponible en /swagger
// UseSwagger publica el JSON OpenAPI (/swagger/v1/swagger.json) y UseSwaggerUI
// la pagina interactiva para probar la API. Se dejan fuera del "if" de
// desarrollo para que siempre esten disponibles en el laboratorio.
app.UseSwagger();
app.UseSwaggerUI();

// Configure the HTTP request pipeline.
// Fuera de desarrollo, cualquier excepcion no controlada se redirige a la
// accion Home/Error, que muestra una pagina amigable (ErrorViewModel) en vez
// del detalle tecnico del error. En desarrollo ASP.NET muestra la pagina de
// excepciones detallada, util para depurar.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
// Sirve archivos estaticos de la carpeta wwwroot (css, js, librerias como
// Bootstrap). Si la peticion corresponde a un archivo, se responde aqui y no
// sigue al resto del pipeline.
app.UseStaticFiles();

// Analiza la URL de la peticion y selecciona el endpoint (controlador/accion)
// que la debe atender, segun las rutas definidas mas abajo y los atributos
// [Route] de los controladores de API.
app.UseRouting();

// Aplica las politicas de autorizacion ([Authorize]) al endpoint elegido.
// Este proyecto no define autenticacion, pero se deja por la plantilla MVC.
app.UseAuthorization();

// Ruta convencional de MVC: /{controlador}/{accion}/{id opcional}.
// Si no se indica nada se usa Home/Index. Ejemplo: /Persona/Edit/123
// ejecuta PersonaController.Edit(123).
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Arranca el servidor web (Kestrel) y se queda escuchando peticiones.
app.Run();
