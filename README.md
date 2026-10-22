# personapi-dotnet

Laboratorio 1: monolito con patrón **MVC** y **DAO** (repositorios) que expone una API **REST** documentada con **Swagger** y vistas MVC para el CRUD de un modelo de personas, profesiones, estudios y teléfonos.

## Stack

| Componente | Versión enunciado | Versión procedimiento |
|---|---|---|
| Framework | .NET 7 | .NET 6.0 |
| Base de datos | MS SQL Server 2022 | SQL Server 2019 Express |
| API | REST | REST |
| Documentación | Swagger 3 (OpenAPI 3) | Swagger 3 (OpenAPI 3) |

> ⚠️ El enunciado y el procedimiento no piden las mismas versiones. Confirmen con el profesor cuál vale. Los pasos de abajo sirven para cualquiera de las dos; solo cambia lo que elijan al crear el proyecto y la versión de los paquetes NuGet (7.x para .NET 7, 6.x para .NET 6).

---

## Qué hay que entregar

- [ ] Repositorio **público** llamado `personapi-dotnet`
- [ ] Base de datos `persona_db` con las 4 tablas del modelo
- [ ] Proyecto ASP.NET Core MVC llamado `personapi-dotnet`
- [ ] Entidades generadas con `Scaffold-DbContext`
- [ ] Interfaces DAO (una por entidad)
- [ ] Repositorios que implementan esas interfaces
- [ ] Controladores **API REST** (CRUD) visibles en Swagger
- [ ] Controladores **MVC con vistas** (CRUD) para las 4 entidades
- [ ] Aplicación desplegada y funcionando
- [ ] Push al repositorio
- [ ] TAG creado (ej. `v1.0.0`)

---

## Modelo de datos

```
profesion (1) ───< estudios >─── (1) persona (1) ───< telefono
```

| Tabla | Campos | Llave |
|---|---|---|
| **profesion** | `id INT`, `nom VARCHAR(90)`, `des TEXT` | PK `id` |
| **persona** | `cc INT`, `nombre VARCHAR(45)`, `apellido VARCHAR(45)`, `genero ENUM('M','F')`, `edad INT` | PK `cc` |
| **estudios** | `id_prof INT`, `cc_per INT`, `fecha DATE`, `univer VARCHAR(50)` | PK (`id_prof`, `cc_per`); FK → profesion, persona |
| **telefono** | `num VARCHAR(15)`, `oper VARCHAR(45)`, `duenio INT` | PK `num`; FK `duenio` → persona |

`estudios` es la tabla intermedia (muchos a muchos) entre `persona` y `profesion`.

---

## Paso a paso

### 1. Crear el repositorio en GitHub

1. En GitHub: **New repository** → nombre `personapi-dotnet` → **Public**.
2. Marquen *Add a README* y *.gitignore: VisualStudio* (ese .gitignore evita subir `bin/`, `obj/` y `.vs/`).

### 2. Instalar SQL Server 2019 Express (modo Básico)

1. Descarguen *SQL Server 2019 Express* de la página de Microsoft.
2. Elijan **Básico** y terminen la instalación.
3. Anoten el nombre de la instancia; por defecto es `localhost\SQLEXPRESS`.

### 3. Instalar SQL Server Management Studio (SSMS)

Descarguen e instalen SSMS 18 (o una versión más nueva). Conéctense a `localhost\SQLEXPRESS` con *Autenticación de Windows*.

### 4. Crear la base de datos `persona_db` con dueño `sa`

En SSMS, **Nueva consulta**:

```sql
CREATE DATABASE persona_db;
GO
ALTER AUTHORIZATION ON DATABASE::persona_db TO sa;
GO
```

O con la interfaz: clic derecho en *Bases de datos* → *Nueva base de datos* → nombre `persona_db` → en *Propietario* elegir `sa`.

### 5. Crear las tablas según el modelo

SQL Server no tiene `ENUM` ni `INT(n)`, así que se traduce así: `INT(n)` → `INT`, `ENUM('M','F')` → `CHAR(1)` con `CHECK`, `TEXT` → `VARCHAR(MAX)`.

```sql
USE persona_db;
GO

CREATE TABLE profesion (
    id   INT          NOT NULL PRIMARY KEY,
    nom  VARCHAR(90)  NOT NULL,
    des  VARCHAR(MAX) NULL
);

CREATE TABLE persona (
    cc       INT         NOT NULL PRIMARY KEY,
    nombre   VARCHAR(45) NOT NULL,
    apellido VARCHAR(45) NOT NULL,
    genero   CHAR(1)     NOT NULL CHECK (genero IN ('M', 'F')),
    edad     INT         NULL
);

CREATE TABLE estudios (
    id_prof INT         NOT NULL,
    cc_per  INT         NOT NULL,
    fecha   DATE        NULL,
    univer  VARCHAR(50) NULL,
    CONSTRAINT PK_estudios PRIMARY KEY (id_prof, cc_per),
    CONSTRAINT estudio_persona_fk   FOREIGN KEY (cc_per)  REFERENCES persona(cc),
    CONSTRAINT estudio_profesion_fk FOREIGN KEY (id_prof) REFERENCES profesion(id)
);

CREATE TABLE telefono (
    num    VARCHAR(15) NOT NULL PRIMARY KEY,
    oper   VARCHAR(45) NOT NULL,
    duenio INT         NOT NULL,
    CONSTRAINT telefono_persona_fk FOREIGN KEY (duenio) REFERENCES persona(cc)
);
GO
```

> `INT` llega hasta 2.147.483.647. Si van a probar con cédulas más grandes, usen `BIGINT` en `cc`, `cc_per` y `duenio`.

Guarden este script en el repo (ej. `database/schema.sql`) para que cualquiera del equipo pueda recrear la base.

### 6. Instalar Visual Studio Community 2022

En el instalador marquen estas cargas de trabajo / componentes:

1. Desarrollo de ASP.NET y web
2. Almacenamiento y procesamiento de datos
3. Plantillas de proyecto y elementos de .NET Framework
4. Características avanzadas de ASP.NET

### 7. Clonar el repositorio

En Visual Studio: **Clonar un repositorio** → peguen la URL de GitHub. O por consola:

```bash
git clone https://github.com/<usuario>/personapi-dotnet.git
```

### 8. Crear y desarrollar el proyecto en Visual Studio

#### 8.1 Crear el proyecto

1. **Crear un proyecto** → plantilla **Aplicación web de ASP.NET Core (Modelo-Vista-Controlador)**.
2. Nombre: `personapi-dotnet`. Ubicación: la carpeta del repo clonado.
3. Framework **.NET 6.0** (o .NET 7 si el profesor lo confirma), **Tipo de autenticación: Ninguno**, **desmarcar "Configurar para HTTPS"**.

#### 8.2 Conectarse a SQL Server desde Visual Studio

1. Menú **Ver → Explorador de objetos de SQL Server**.
2. **Agregar SQL Server** → *Local* → `SQLEXPRESS` → **Conectar**. Verifiquen que aparece `persona_db` con sus tablas.

#### 8.3 Instalar paquetes NuGet

Clic derecho en **Dependencias → Administrar paquetes NuGet** e instalen (misma versión mayor que el framework: 6.x o 7.x):

- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Tools`
- `Swashbuckle.AspNetCore` (para Swagger)

#### 8.4 Generar las entidades (Scaffold)

1. En el Explorador de soluciones, clic derecho en `Models` → **Agregar → Nueva carpeta** → `Entities`.
2. Abran **Herramientas → Administrador de paquetes NuGet → Consola del Administrador de paquetes** y ejecuten:

```powershell
Scaffold-DbContext "Server=localhost\SQLEXPRESS;Database=persona_db;Trusted_Connection=True;TrustServerCertificate=true" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models/Entities
```

Se generan `Persona.cs`, `Profesion.cs`, `Estudio.cs`, `Telefono.cs` y `PersonaDbContext.cs` en `Models/Entities`.

3. Muevan la cadena de conexión a `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "PersonaDb": "Server=localhost\\SQLEXPRESS;Database=persona_db;Trusted_Connection=True;TrustServerCertificate=true"
  },
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*"
}
```

4. En `PersonaDbContext.cs` borren el método `OnConfiguring` (el que tiene la cadena quemada con el `#warning`).

#### 8.5 Estructura de carpetas sugerida

```
personapi-dotnet/
├── Controllers/
│   ├── Api/                 ← controladores REST (Swagger)
│   │   └── PersonaApiController.cs ...
│   └── PersonaController.cs ...   ← controladores MVC con vistas
├── Interfaces/              ← contratos DAO
├── Repositories/            ← implementación DAO con EF Core
├── Models/Entities/         ← generadas por Scaffold
├── Views/
│   ├── Persona/ Profesion/ Estudio/ Telefono/
└── Program.cs
```

#### 8.6 Crear las interfaces (DAO)

Una por entidad. Ejemplo `Interfaces/IPersonaRepository.cs`:

```csharp
using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Interfaces
{
    public interface IPersonaRepository
    {
        Task<IEnumerable<Persona>> GetAllAsync();
        Task<Persona?> GetByIdAsync(int cc);
        Task AddAsync(Persona persona);
        Task UpdateAsync(Persona persona);
        Task DeleteAsync(int cc);
    }
}
```

Igual para `IProfesionRepository`, `ITelefonoRepository` (id `string num`) e `IEstudioRepository` (llave compuesta: `GetByIdAsync(int idProf, int ccPer)` y `DeleteAsync(int idProf, int ccPer)`).

#### 8.7 Crear los repositorios

Ejemplo `Repositories/PersonaRepository.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Interfaces;
using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Repositories
{
    public class PersonaRepository : IPersonaRepository
    {
        private readonly PersonaDbContext _context;

        public PersonaRepository(PersonaDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Persona>> GetAllAsync() =>
            await _context.Personas.ToListAsync();

        public async Task<Persona?> GetByIdAsync(int cc) =>
            await _context.Personas.FindAsync(cc);

        public async Task AddAsync(Persona persona)
        {
            _context.Personas.Add(persona);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Persona persona)
        {
            _context.Personas.Update(persona);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int cc)
        {
            var persona = await _context.Personas.FindAsync(cc);
            if (persona != null)
            {
                _context.Personas.Remove(persona);
                await _context.SaveChangesAsync();
            }
        }
    }
}
```

> Los nombres exactos (`PersonaDbContext`, `Personas`, `Estudios`...) dependen de lo que genere el Scaffold; revisen el contexto generado.

#### 8.8 Registrar todo en `Program.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using personapi_dotnet.Interfaces;
using personapi_dotnet.Models.Entities;
using personapi_dotnet.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<PersonaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PersonaDb")));

builder.Services.AddScoped<IPersonaRepository, PersonaRepository>();
builder.Services.AddScoped<IProfesionRepository, ProfesionRepository>();
builder.Services.AddScoped<IEstudioRepository, EstudioRepository>();
builder.Services.AddScoped<ITelefonoRepository, TelefonoRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

#### 8.9 Crear los controladores

**a) API REST** (salen en Swagger). Ejemplo `Controllers/Api/PersonaApiController.cs`:

```csharp
using Microsoft.AspNetCore.Mvc;
using personapi_dotnet.Interfaces;
using personapi_dotnet.Models.Entities;

namespace personapi_dotnet.Controllers.Api
{
    [Route("api/personas")]
    [ApiController]
    public class PersonaApiController : ControllerBase
    {
        private readonly IPersonaRepository _repo;

        public PersonaApiController(IPersonaRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _repo.GetAllAsync());

        [HttpGet("{cc}")]
        public async Task<IActionResult> Get(int cc)
        {
            var persona = await _repo.GetByIdAsync(cc);
            return persona == null ? NotFound() : Ok(persona);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Persona persona)
        {
            await _repo.AddAsync(persona);
            return CreatedAtAction(nameof(Get), new { cc = persona.Cc }, persona);
        }

        [HttpPut("{cc}")]
        public async Task<IActionResult> Update(int cc, Persona persona)
        {
            if (cc != persona.Cc) return BadRequest();
            await _repo.UpdateAsync(persona);
            return NoContent();
        }

        [HttpDelete("{cc}")]
        public async Task<IActionResult> Delete(int cc)
        {
            await _repo.DeleteAsync(cc);
            return NoContent();
        }
    }
}
```

Endpoints esperados:

| Recurso | GET (todos) | GET (uno) | POST | PUT | DELETE |
|---|---|---|---|---|---|
| Persona | `/api/personas` | `/api/personas/{cc}` | ✔ | `/{cc}` | `/{cc}` |
| Profesión | `/api/profesiones` | `/api/profesiones/{id}` | ✔ | `/{id}` | `/{id}` |
| Teléfono | `/api/telefonos` | `/api/telefonos/{num}` | ✔ | `/{num}` | `/{num}` |
| Estudio | `/api/estudios` | `/api/estudios/{idProf}/{ccPer}` | ✔ | `/{idProf}/{ccPer}` | `/{idProf}/{ccPer}` |

> Las entidades generadas tienen propiedades de navegación (`Persona.Telefonos`, `Persona.Estudios`...). Si al serializar sale error de **ciclo**, agreguen `[JsonIgnore]` a esas propiedades o configuren `ReferenceHandler.IgnoreCycles` en `AddControllersWithViews().AddJsonOptions(...)`.

**b) Controladores MVC con vistas.** La forma rápida:

1. Clic derecho en `Controllers` → **Agregar → Controlador → Controlador de MVC con vistas que usan Entity Framework**.
2. Modelo: `Persona`, contexto: `PersonaDbContext`. Esto genera `PersonaController` y las vistas `Index`, `Details`, `Create`, `Edit`, `Delete`.
3. **Importante para cumplir DAO:** el controlador generado usa el `DbContext` directamente. Cámbienlo para que reciba `IPersonaRepository` en el constructor y llame a sus métodos en lugar de `_context`.
4. Repitan para `Profesion`, `Estudio` y `Telefono`.
5. Agreguen enlaces en `Views/Shared/_Layout.cshtml` hacia `/Persona`, `/Profesion`, `/Estudio`, `/Telefono`.

#### 8.10 Desplegar / probar

1. Ejecuten con **F5** (o `dotnet run`).
2. Vistas: `http://localhost:<puerto>/Persona` (y las demás).
3. Swagger: `http://localhost:<puerto>/swagger`.
4. Prueben el CRUD completo de las 4 entidades en ambos lados. Orden recomendado para crear datos: **profesion → persona → estudios / telefono** (por las llaves foráneas).

### 9. Hacer push al repositorio

```bash
git add .
git commit -m "Laboratorio 1: CRUD MVC + DAO + API REST con Swagger"
git push origin main
```

### 10. Crear el TAG

```bash
git tag -a v1.0.0 -m "Entrega laboratorio 1"
git push origin v1.0.0
```

O desde GitHub: **Releases → Draft a new release → Choose a tag → `v1.0.0`**.

---

## Cómo correr este proyecto (para quien lo clone)

1. Tener SQL Server Express corriendo en `localhost\SQLEXPRESS`.
2. Ejecutar `database/schema.sql` en SSMS.
3. Revisar la cadena `ConnectionStrings:PersonaDb` en `appsettings.json`.
4. Abrir `personapi-dotnet.sln` en Visual Studio y presionar **F5**.
5. Entrar a `/swagger` para la API o a `/Persona` para las vistas.

## Problemas comunes

| Error | Solución |
|---|---|
| `A network-related or instance-specific error` | El servicio *SQL Server (SQLEXPRESS)* está detenido o el nombre de la instancia es otro. |
| `The certificate chain was issued by an authority that is not trusted` | Falta `TrustServerCertificate=true` en la cadena. |
| `Scaffold-DbContext` no se reconoce | Falta instalar `Microsoft.EntityFrameworkCore.Tools`. |
| Versión de paquete incompatible | Usen paquetes 6.x con .NET 6 o 7.x con .NET 7. |
| `A possible object cycle was detected` | Ver la nota de `[JsonIgnore]` / `IgnoreCycles` en el paso 8.9. |
| Error de FK al borrar persona | Primero borren sus `telefono` y `estudios`, o configuren borrado en cascada. |
