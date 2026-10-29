-- Script DML - datos de prueba para persona_db
-- Correr despues de ddl.sql
--
-- DML (Data Manipulation Language) = sentencias que trabajan sobre los DATOS
-- (INSERT, UPDATE, DELETE, SELECT). Aqui solo se insertan datos de ejemplo
-- para poder probar el CRUD web y la API REST (Swagger) desde el primer momento.
--
-- ORDEN DE INSERCION (importante por las llaves foraneas):
--   1. profesion y 2. persona  -> tablas padre, no dependen de nadie.
--   3. estudios                -> sus FK apuntan a profesion.id y persona.cc,
--                                 por eso esas filas ya deben existir.
--   4. telefono                -> su FK duenio apunta a persona.cc.
-- Si se invierte el orden, SQL Server rechaza el INSERT con un error de
-- conflicto con la restriccion FOREIGN KEY. Para borrar los datos el orden es
-- el inverso (primero hijas, luego padres).
--
-- Se usa INSERT con varias filas en un solo VALUES (sintaxis disponible desde
-- SQL Server 2008) para que cada tabla quede en una sola sentencia.

-- Asegura que los INSERT se ejecuten en persona_db y no en master.
USE persona_db;
GO

-- 1. Profesiones (catalogo). Los id se dan explicitamente porque la columna no
-- es IDENTITY.
INSERT INTO profesion (id, nom, des) VALUES
(1, 'Ingenieria de Sistemas', 'Desarrollo de software y gestion de sistemas de informacion'),
(2, 'Medicina', 'Diagnostico y tratamiento de enfermedades'),
(3, 'Derecho', 'Asesoria y representacion legal'),
(4, 'Contaduria Publica', 'Manejo contable y financiero de empresas');
GO

-- 2. Personas. genero solo admite 'M' o 'F' por el CHECK CK_persona_genero.
-- Las cedulas de 10 digitos caben en INT (maximo 2147483647).
INSERT INTO persona (cc, nombre, apellido, genero, edad) VALUES
(1010101010, 'Laura', 'Gomez', 'F', 22),
(1020202020, 'Carlos', 'Rodriguez', 'M', 30),
(1030303030, 'Valentina', 'Martinez', 'F', 27),
(1040404040, 'Andres', 'Lopez', 'M', 45);
GO

-- 3. Estudios (tabla intermedia). Cada par (id_prof, cc_per) debe existir en
-- profesion y persona. La persona 1040404040 tiene dos estudios con profesiones
-- distintas, lo que muestra la relacion muchos a muchos y que la PK compuesta
-- solo impide repetir el mismo par. Las fechas van en formato 'AAAA-MM-DD'.
INSERT INTO estudios (id_prof, cc_per, fecha, univer) VALUES
(1, 1010101010, '2024-06-15', 'Pontificia Universidad Javeriana'),
(2, 1020202020, '2018-12-10', 'Universidad Nacional de Colombia'),
(3, 1030303030, '2020-07-01', 'Universidad del Rosario'),
(4, 1040404040, '2005-11-20', 'Universidad de los Andes'),
(1, 1040404040, '2012-05-30', 'Universidad Distrital');
GO

-- 4. Telefonos. num va entre comillas porque es VARCHAR. duenio debe ser una
-- cedula existente; la persona 1040404040 tiene dos telefonos (relacion 1 a N).
INSERT INTO telefono (num, oper, duenio) VALUES
('3001234567', 'Claro', 1010101010),
('3109876543', 'Movistar', 1020202020),
('3204567890', 'Tigo', 1030303030),
('3157654321', 'Claro', 1040404040),
('6015551234', 'ETB', 1040404040);
GO
