# personapi-dotnet

Laboratorio 1 de Arquitectura de Software. Es un monolito en ASP.NET Core con patrón MVC y DAO que hace el CRUD de personas, profesiones, estudios y teléfonos. Tiene vistas MVC y también una API REST documentada con Swagger.

## Stack

- .NET 7 (ASP.NET Core MVC)
- MS SQL Server 2022 (también funciona con SQL Server 2019 Express)
- Entity Framework Core 7
- REST
- Swagger 3 / OpenAPI 3 (Swashbuckle)

## Modelo de datos

```
profesion (1) ──< estudios >── (1) persona (1) ──< telefono
```

| Tabla | Campos | Llaves |
|---|---|---|
| profesion | id, nom, des | PK id |
| persona | cc, nombre, apellido, genero, edad | PK cc |
| estudios | id_prof, cc_per, fecha, univer | PK (id_prof, cc_per), FK a profesion y persona |
| telefono | num, oper, duenio | PK num, FK duenio a persona |

Como SQL Server no tiene `ENUM` ni `INT(n)`, `genero` quedó como `CHAR(1)` con un `CHECK ('M','F')`, los `INT(n)` como `INT` y `des` (TEXT) como `VARCHAR(MAX)`.

## Estructura del proyecto

```
personapi-dotnet/
├── database/
│   ├── ddl.sql                  creación de la base y las tablas
│   └── dml.sql                  datos de prueba
├── personapi-dotnet.sln
└── personapi-dotnet/
    ├── Controllers/
    │   ├── Api/                 controladores REST (los que salen en Swagger)
    │   └── *Controller.cs       controladores MVC con vistas
    ├── Interfaces/              interfaces DAO, una por entidad
    ├── Repositories/            implementación de las interfaces con EF Core
    ├── Models/Entities/         entidades y DbContext generados con Scaffold-DbContext
    ├── Views/                   vistas de Persona, Profesion, Estudio y Telefono
    ├── appsettings.json         cadena de conexión
    └── Program.cs               registro del DbContext, repositorios y Swagger
```

El flujo es: el controlador (MVC o API) recibe la petición, llama al repositorio por medio de su interfaz y el repositorio es el único que usa el `PersonaDbContext` para hablar con la base de datos.

## Configuración del ambiente

Se necesita:

1. SQL Server 2022 (o 2019 Express, modo Básico).
2. SQL Server Management Studio 18 o superior.
3. Visual Studio Community 2022 con estas cargas de trabajo:
   - Desarrollo de ASP.NET y web
   - Almacenamiento y procesamiento de datos
   - Plantillas de proyecto y elementos de .NET Framework
   - Características avanzadas de ASP.NET
4. SDK de .NET 7 (viene con Visual Studio, o se descarga aparte si se va a usar la consola).

Paquetes NuGet que usa el proyecto (ya están en el `.csproj`, se restauran solos):

- Microsoft.EntityFrameworkCore 7.0.20
- Microsoft.EntityFrameworkCore.SqlServer 7.0.20
- Microsoft.EntityFrameworkCore.Tools 7.0.20
- Swashbuckle.AspNetCore 6.5.0

## Base de datos

1. Abrir SSMS y conectarse a `localhost\SQLEXPRESS` (o a la instancia que tengan).
2. Abrir y ejecutar `database/ddl.sql`. Este script crea `persona_db`, le asigna como dueño al usuario `sa` y crea las cuatro tablas.
3. Ejecutar `database/dml.sql` para cargar datos de prueba (opcional).

Las entidades de `Models/Entities` se generaron con este comando en la Consola del Administrador de paquetes:

```powershell
Scaffold-DbContext "Server=localhost\SQLEXPRESS;Database=persona_db;Trusted_Connection=True;TrustServerCertificate=true" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models/Entities
```

Después de generarlas, el `OnConfiguring` del contexto se quitó y la cadena de conexión se pasó a `appsettings.json`. A las propiedades de navegación se les agregó `[JsonIgnore]` (para evitar ciclos al devolver JSON) y `[ValidateNever]` (para que los formularios no las pidan).

### Cadena de conexión

Está en `personapi-dotnet/appsettings.json`:

```json
"ConnectionStrings": {
  "PersonaDb": "Server=localhost\\SQLEXPRESS;Database=persona_db;Trusted_Connection=True;TrustServerCertificate=true"
}
```

Si la instancia tiene otro nombre (por ejemplo SQL Server 2022 Developer queda como `localhost`), se cambia el `Server`. Si se quiere entrar con el usuario `sa` en vez de autenticación de Windows:

```
Server=localhost;Database=persona_db;User Id=sa;Password=<clave>;TrustServerCertificate=true
```

## Compilación

Con Visual Studio: abrir `personapi-dotnet.sln` y darle a Compilar > Compilar solución.

Por consola, desde la carpeta del repo:

```bash
dotnet restore
dotnet build
```

## Despliegue

### Local

En Visual Studio se corre con F5 usando el perfil `http`. Por consola:

```bash
cd personapi-dotnet
dotnet run
```

La aplicación queda en `http://localhost:5142` (el puerto está en `Properties/launchSettings.json`).

- Vistas: `http://localhost:5142/Persona`, `/Profesion`, `/Estudio`, `/Telefono`
- Swagger: `http://localhost:5142/swagger`

### Publicar

```bash
dotnet publish personapi-dotnet/personapi-dotnet.csproj -c Release -o ./publish
cd publish
dotnet personapi-dotnet.dll --urls http://localhost:5000
```

La carpeta `publish` también se puede montar en IIS (con el Hosting Bundle de .NET 7 instalado) creando un sitio que apunte a esa carpeta.

## Endpoints REST

| Recurso | Listar | Obtener / Actualizar / Eliminar | Crear |
|---|---|---|---|
| Persona | `GET /api/personas` | `GET, PUT, DELETE /api/personas/{cc}` | `POST /api/personas` |
| Profesión | `GET /api/profesiones` | `GET, PUT, DELETE /api/profesiones/{id}` | `POST /api/profesiones` |
| Teléfono | `GET /api/telefonos` | `GET, PUT, DELETE /api/telefonos/{num}` | `POST /api/telefonos` |
| Estudio | `GET /api/estudios` | `GET, PUT, DELETE /api/estudios/{idProf}/{ccPer}` | `POST /api/estudios` |

Respuestas: `200` al consultar, `201` al crear, `204` al actualizar o borrar, `404` si no existe, `409` si la llave ya existe y `400` si la persona o profesión referenciada no existe o la llave de la URL no coincide con la del body.

Ejemplo para crear una persona:

```json
{
  "cc": 1050505050,
  "nombre": "Juan",
  "apellido": "Perez",
  "genero": "M",
  "edad": 25
}
```

## Notas

- Para crear datos el orden es: profesión, persona y luego estudios o teléfonos, por las llaves foráneas.
- Al borrar una persona se borran también sus teléfonos y estudios. Al borrar una profesión se borran los estudios que la usan.
- La llave de cada registro (cc, id, num, o id_prof + cc_per) no se puede editar; si hay que cambiarla toca borrar y crear de nuevo.

## Problemas comunes

| Error | Solución |
|---|---|
| `A network-related or instance-specific error` | El servicio de SQL Server está detenido o el nombre de la instancia en la cadena está mal. |
| `The certificate chain was issued by an authority that is not trusted` | Falta `TrustServerCertificate=true` en la cadena. |
| `Scaffold-DbContext` no se reconoce | Falta instalar `Microsoft.EntityFrameworkCore.Tools`. |
| `Login failed for user` | Revisar usuario y clave, o usar `Trusted_Connection=True`. |
