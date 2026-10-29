-- Script DDL - persona_db
-- Crea la base de datos, le da la propiedad a sa y crea las tablas del modelo
--
-- DDL (Data Definition Language) = sentencias que definen la ESTRUCTURA
-- (bases de datos, tablas, llaves, restricciones). Los datos van en dml.sql.
-- El modelo original del laboratorio venia en MySQL; este script es su
-- traduccion a SQL Server (T-SQL), que es el motor que usa EF Core en el proyecto.
--
-- Modelo entidad-relacion resumido:
--   persona (1) ---- (N) telefono      una persona puede tener varios telefonos
--   persona (N) ---- (N) profesion     relacion muchos a muchos resuelta con
--                                      la tabla intermedia "estudios"
--
-- Orden de creacion: primero las tablas "padre" (profesion, persona) y despues
-- las "hijas" (estudios, telefono), porque una FK solo puede apuntar a una
-- tabla que ya exista.

-- Crea la base de datos vacia con la configuracion por defecto del servidor
-- (archivos .mdf/.ldf en la ruta por defecto de la instancia).
CREATE DATABASE persona_db;
-- GO no es T-SQL: es un separador de lotes (batches) que entienden SSMS y
-- sqlcmd. Cada lote se envia al servidor por separado. Se necesita aqui porque
-- la base debe existir antes de que el siguiente lote pueda referenciarla.
GO

-- Deja como duenio (owner) de la base al login "sa", como pide el enunciado.
-- Si no se hace, el duenio queda siendo el login (por ejemplo el usuario de
-- Windows) que ejecuto el script. Tener un duenio valido del servidor evita
-- problemas de permisos, por ejemplo al crear diagramas en SSMS, y deja la base
-- lista si se usa la cadena de conexion alternativa con User Id=sa.
ALTER AUTHORIZATION ON DATABASE::persona_db TO sa;
GO

-- Cambia el contexto a la nueva base; todo lo que sigue se crea dentro de ella
-- y no en master.
USE persona_db;
GO

-- Tabla profesion: catalogo de profesiones.
-- en SQL Server no hay ENUM ni INT(n), por eso genero queda como CHAR(1) con CHECK
-- y des (TEXT) queda como VARCHAR(MAX)
-- Adaptaciones de tipos MySQL -> SQL Server:
--   INT(n)  -> INT           (en SQL Server INT no lleva ancho de despliegue)
--   TEXT    -> VARCHAR(MAX)  (TEXT esta obsoleto en SQL Server; VARCHAR(MAX)
--                             admite hasta 2 GB y soporta funciones de cadena)
-- El id NO es IDENTITY: el usuario lo digita, igual que en el modelo original.
CREATE TABLE profesion (
    id  INT          NOT NULL,
    nom VARCHAR(90)  NOT NULL,
    des VARCHAR(MAX) NULL,
    -- Restriccion con nombre explicito (PK_<tabla>) en vez de dejar que SQL
    -- Server invente uno aleatorio: asi es facil identificarla en errores y
    -- en el modelo que genera EF Core con Scaffold-DbContext.
    CONSTRAINT PK_profesion PRIMARY KEY (id)
);
GO

-- Tabla persona: la llave es la cedula (cc), tambien digitada por el usuario.
-- genero era ENUM('M','F') en MySQL; SQL Server no tiene ENUM, asi que se usa
-- CHAR(1) y un CHECK que restringe los valores permitidos a 'M' o 'F'.
-- edad admite NULL porque en el modelo original no es obligatoria.
CREATE TABLE persona (
    cc       INT         NOT NULL,
    nombre   VARCHAR(45) NOT NULL,
    apellido VARCHAR(45) NOT NULL,
    genero   CHAR(1)     NOT NULL,
    edad     INT         NULL,
    CONSTRAINT PK_persona PRIMARY KEY (cc),
    -- CHECK: el motor rechaza cualquier INSERT/UPDATE con un genero distinto
    -- de 'M' o 'F', aunque la aplicacion no lo valide.
    CONSTRAINT CK_persona_genero CHECK (genero IN ('M', 'F'))
);
GO

-- tabla intermedia entre persona y profesion
-- Resuelve la relacion muchos a muchos: cada fila dice que la persona cc_per
-- estudio la profesion id_prof, en que fecha y en que universidad.
-- La PK es COMPUESTA (id_prof, cc_per): una persona no puede registrar dos veces
-- la misma profesion, pero si varias profesiones distintas. Por eso en la app
-- MVC/API un estudio se identifica con DOS valores (idProf y ccPer).
-- Cada columna de la llave es a la vez FK hacia su tabla padre, lo que garantiza
-- que no existan estudios de personas o profesiones inexistentes.
-- fecha: DATE (solo fecha, sin hora), igual que en MySQL.
CREATE TABLE estudios (
    id_prof INT         NOT NULL,
    cc_per  INT         NOT NULL,
    fecha   DATE        NULL,
    univer  VARCHAR(50) NULL,
    CONSTRAINT PK_estudios PRIMARY KEY (id_prof, cc_per),
    CONSTRAINT estudio_persona_fk FOREIGN KEY (cc_per) REFERENCES persona (cc),
    CONSTRAINT estudio_profesion_fk FOREIGN KEY (id_prof) REFERENCES profesion (id)
    -- Sin ON DELETE CASCADE: el comportamiento por defecto es NO ACTION, asi que
    -- no se puede borrar una persona o profesion que aun tenga estudios; primero
    -- hay que borrar los estudios asociados. Eso lo hace la aplicacion en
    -- PersonaRepository/ProfesionRepository antes de borrar el registro padre.
);
GO

-- Tabla telefono: relacion uno a muchos con persona (un duenio, varios telefonos).
-- num es VARCHAR y no INT porque un telefono es un identificador, no una
-- cantidad: no se opera aritmeticamente, puede tener ceros a la izquierda o
-- prefijos, y numeros de 10 digitos se salen del rango de INT.
-- duenio es FK a persona.cc; es NOT NULL, asi que todo telefono debe tener duenio.
CREATE TABLE telefono (
    num    VARCHAR(15) NOT NULL,
    oper   VARCHAR(45) NOT NULL,
    duenio INT         NOT NULL,
    CONSTRAINT PK_telefono PRIMARY KEY (num),
    -- Igual que en estudios, sin cascada: una persona con telefonos no se
    -- puede borrar hasta borrar (o reasignar) sus telefonos. PersonaRepository
    -- borra primero los telefonos de la persona por esta razon.
    CONSTRAINT telefono_persona_fk FOREIGN KEY (duenio) REFERENCES persona (cc)
);
GO
