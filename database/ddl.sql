-- Script DDL - persona_db
-- Crea la base de datos, le da la propiedad a sa y crea las tablas del modelo

CREATE DATABASE persona_db;
GO

ALTER AUTHORIZATION ON DATABASE::persona_db TO sa;
GO

USE persona_db;
GO

-- en SQL Server no hay ENUM ni INT(n), por eso genero queda como CHAR(1) con CHECK
-- y des (TEXT) queda como VARCHAR(MAX)
CREATE TABLE profesion (
    id  INT          NOT NULL,
    nom VARCHAR(90)  NOT NULL,
    des VARCHAR(MAX) NULL,
    CONSTRAINT PK_profesion PRIMARY KEY (id)
);
GO

CREATE TABLE persona (
    cc       INT         NOT NULL,
    nombre   VARCHAR(45) NOT NULL,
    apellido VARCHAR(45) NOT NULL,
    genero   CHAR(1)     NOT NULL,
    edad     INT         NULL,
    CONSTRAINT PK_persona PRIMARY KEY (cc),
    CONSTRAINT CK_persona_genero CHECK (genero IN ('M', 'F'))
);
GO

-- tabla intermedia entre persona y profesion
CREATE TABLE estudios (
    id_prof INT         NOT NULL,
    cc_per  INT         NOT NULL,
    fecha   DATE        NULL,
    univer  VARCHAR(50) NULL,
    CONSTRAINT PK_estudios PRIMARY KEY (id_prof, cc_per),
    CONSTRAINT estudio_persona_fk FOREIGN KEY (cc_per) REFERENCES persona (cc),
    CONSTRAINT estudio_profesion_fk FOREIGN KEY (id_prof) REFERENCES profesion (id)
);
GO

CREATE TABLE telefono (
    num    VARCHAR(15) NOT NULL,
    oper   VARCHAR(45) NOT NULL,
    duenio INT         NOT NULL,
    CONSTRAINT PK_telefono PRIMARY KEY (num),
    CONSTRAINT telefono_persona_fk FOREIGN KEY (duenio) REFERENCES persona (cc)
);
GO
