-- Agregar columna Dni a la tabla Usuario si no existe
ALTER TABLE Usuario ADD Dni INT NULL;
-- Si quieres que sea obligatorio y único, ejecuta también:
-- UPDATE Usuario SET Dni = IdUsuario WHERE Dni IS NULL;
-- ALTER TABLE Usuario ALTER COLUMN Dni INT NOT NULL;
-- CREATE UNIQUE INDEX IX_Usuario_Dni ON Usuario(Dni);
