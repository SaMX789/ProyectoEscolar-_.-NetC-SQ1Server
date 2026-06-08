-- ============================================================
-- SCRIPT: Crear/Actualizar tablas y SPs para Proyectos
-- ============================================================

-- 1. TABLA PROYECTOS: Crear si no existe
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Proyectos')
BEGIN
    CREATE TABLE Proyectos (
        id_proyecto     INT IDENTITY(1,1) PRIMARY KEY,
        nombre          NVARCHAR(200)   NOT NULL,
        anio            INT             NOT NULL,
        periodo         NVARCHAR(100)   NOT NULL,
        ciclo           CHAR(1)         NOT NULL DEFAULT 'B',
        fecha_creacion  DATE            NOT NULL DEFAULT GETDATE(),
        id_estado       INT             NOT NULL DEFAULT 1
    );
    PRINT 'Tabla Proyectos creada.';
END
GO

-- Si la tabla ya existia pero le faltan columnas, agregarlas:
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Proyectos') AND name = 'ciclo')
BEGIN
    ALTER TABLE Proyectos ADD ciclo CHAR(1) NOT NULL DEFAULT 'B';
    PRINT 'Columna ciclo agregada a Proyectos.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Proyectos') AND name = 'id_estado')
BEGIN
    ALTER TABLE Proyectos ADD id_estado INT NOT NULL DEFAULT 1;
    PRINT 'Columna id_estado agregada a Proyectos.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Proyectos') AND name = 'anio')
BEGIN
    ALTER TABLE Proyectos ADD anio INT NOT NULL DEFAULT 2026;
    PRINT 'Columna anio agregada a Proyectos.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Proyectos') AND name = 'periodo')
BEGIN
    ALTER TABLE Proyectos ADD periodo NVARCHAR(100) NOT NULL DEFAULT 'Agosto-Diciembre';
    PRINT 'Columna periodo agregada a Proyectos.';
END
ELSE
BEGIN
    -- Si ya existe pero es demasiado corta (ej. CHAR(1)), ampliarla
    IF EXISTS (
        SELECT * FROM sys.columns
        WHERE object_id = OBJECT_ID('Proyectos')
          AND name = 'periodo'
          AND max_length < 20
    )
    BEGIN
        ALTER TABLE Proyectos ALTER COLUMN periodo NVARCHAR(100) NOT NULL;
        PRINT 'Columna periodo ampliada a NVARCHAR(100).';
    END
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Proyectos') AND name = 'fecha_creacion')
BEGIN
    ALTER TABLE Proyectos ADD fecha_creacion DATE NOT NULL DEFAULT GETDATE();
    PRINT 'Columna fecha_creacion agregada a Proyectos.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Proyectos') AND name = 'nombre')
BEGIN
    ALTER TABLE Proyectos ADD nombre NVARCHAR(200) NOT NULL DEFAULT 'Horarios';
    PRINT 'Columna nombre agregada a Proyectos.';
END
GO

-- 2. TABLA DETALLE DE HORARIO (cada celda asignada)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'HorarioDetalle')
BEGIN
    CREATE TABLE HorarioDetalle (
        id_detalle      INT IDENTITY(1,1) PRIMARY KEY,
        id_proyecto     INT NOT NULL,
        id_grupo        INT NOT NULL,
        id_materia      INT NOT NULL,
        id_docente      INT NULL,
        id_salon        INT NULL,
        id_dia          INT NOT NULL,
        id_bloque       INT NOT NULL,
        CONSTRAINT FK_HD_Proyecto FOREIGN KEY (id_proyecto) REFERENCES Proyectos(id_proyecto),
        CONSTRAINT FK_HD_Grupo    FOREIGN KEY (id_grupo)    REFERENCES Grupos(id_grupo),
        CONSTRAINT FK_HD_Materia  FOREIGN KEY (id_materia)  REFERENCES Materias(id_materia),
        CONSTRAINT UQ_HD_GrupoDiaBloque UNIQUE (id_proyecto, id_grupo, id_dia, id_bloque)
    );
END
GO

-- 3. SP: Obtener todos los proyectos activos
IF OBJECT_ID('sp_ObtenerProyectos', 'P') IS NOT NULL DROP PROCEDURE sp_ObtenerProyectos;
GO
CREATE PROCEDURE sp_ObtenerProyectos
AS
BEGIN
    SELECT id_proyecto, nombre, anio, periodo, ciclo, fecha_creacion
    FROM Proyectos
    WHERE id_estado = 1
    ORDER BY fecha_creacion DESC;
END
GO

-- 4. SP: Contar proyectos activos
IF OBJECT_ID('sp_ContarProyectos', 'P') IS NOT NULL DROP PROCEDURE sp_ContarProyectos;
GO
CREATE PROCEDURE sp_ContarProyectos
AS
BEGIN
    SELECT COUNT(*) AS Total FROM Proyectos WHERE id_estado = 1;
END
GO

-- 5. SP: Obtener grupos por ciclo y carrera
IF OBJECT_ID('sp_ObtenerGruposPorCicloYCarrera', 'P') IS NOT NULL DROP PROCEDURE sp_ObtenerGruposPorCicloYCarrera;
GO
CREATE PROCEDURE sp_ObtenerGruposPorCicloYCarrera
    @ciclo CHAR(1),
    @id_carrera INT
AS
BEGIN
    IF @ciclo = 'B'
        SELECT g.id_grupo, g.nombre AS NombreGrupo, g.semestre, g.turno,
               c.nombre AS NombreCarrera
        FROM Grupos g
        JOIN Carreras c ON g.id_carrera = c.id_carrera
        WHERE g.id_carrera = @id_carrera AND g.semestre % 2 = 1
        ORDER BY g.semestre, g.nombre;
    ELSE
        SELECT g.id_grupo, g.nombre AS NombreGrupo, g.semestre, g.turno,
               c.nombre AS NombreCarrera
        FROM Grupos g
        JOIN Carreras c ON g.id_carrera = c.id_carrera
        WHERE g.id_carrera = @id_carrera AND g.semestre % 2 = 0
        ORDER BY g.semestre, g.nombre;
END
GO

-- 6. SP: Contar registros generales para dashboard
IF OBJECT_ID('sp_EstadisticasDashboard', 'P') IS NOT NULL DROP PROCEDURE sp_EstadisticasDashboard;
GO
CREATE PROCEDURE sp_EstadisticasDashboard
AS
BEGIN
    SELECT
        (SELECT COUNT(*) FROM Proyectos WHERE id_estado = 1) AS TotalProyectos,
        (SELECT COUNT(*) FROM Docentes) AS TotalDocentes,
        (SELECT COUNT(*) FROM Materias) AS TotalMaterias,
        (SELECT COUNT(*) FROM Grupos) AS TotalGrupos,
        (SELECT COUNT(*) FROM Salones) AS TotalSalones;
END
GO
