-- Script DML - datos de prueba para persona_db
-- Correr despues de ddl.sql

USE persona_db;
GO

INSERT INTO profesion (id, nom, des) VALUES
(1, 'Ingenieria de Sistemas', 'Desarrollo de software y gestion de sistemas de informacion'),
(2, 'Medicina', 'Diagnostico y tratamiento de enfermedades'),
(3, 'Derecho', 'Asesoria y representacion legal'),
(4, 'Contaduria Publica', 'Manejo contable y financiero de empresas');
GO

INSERT INTO persona (cc, nombre, apellido, genero, edad) VALUES
(1010101010, 'Laura', 'Gomez', 'F', 22),
(1020202020, 'Carlos', 'Rodriguez', 'M', 30),
(1030303030, 'Valentina', 'Martinez', 'F', 27),
(1040404040, 'Andres', 'Lopez', 'M', 45);
GO

INSERT INTO estudios (id_prof, cc_per, fecha, univer) VALUES
(1, 1010101010, '2024-06-15', 'Pontificia Universidad Javeriana'),
(2, 1020202020, '2018-12-10', 'Universidad Nacional de Colombia'),
(3, 1030303030, '2020-07-01', 'Universidad del Rosario'),
(4, 1040404040, '2005-11-20', 'Universidad de los Andes'),
(1, 1040404040, '2012-05-30', 'Universidad Distrital');
GO

INSERT INTO telefono (num, oper, duenio) VALUES
('3001234567', 'Claro', 1010101010),
('3109876543', 'Movistar', 1020202020),
('3204567890', 'Tigo', 1030303030),
('3157654321', 'Claro', 1040404040),
('6015551234', 'ETB', 1040404040);
GO
