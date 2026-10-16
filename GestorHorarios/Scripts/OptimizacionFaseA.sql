-- =============================================================================
-- SCRIPT DE MIGRACIÓN Y OPTIMIZACIÓN (FASE A): NORMALIZACIÓN Y MOTOR OR-TOOLS
-- Base de Datos: GestorHorarios
-- =============================================================================
USE [GestorHorarios];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- -----------------------------------------------------------------------------
-- 1. LIMPIEZA PREVENTIVA DE DATOS INVÁLIDOS Y TABLA EN DESUSO
-- -----------------------------------------------------------------------------
-- Convertir id_docente <= 0 (como el antiguo -1 "Sin Maestro") a NULL real
UPDATE dbo.HorarioDetalle
SET id_docente = NULL
WHERE id_docente IS NOT NULL
  AND (id_docente <= 0 OR id_docente NOT IN (SELECT id_docente FROM dbo.Docentes));

-- Convertir id_salon inválidos a NULL
UPDATE dbo.HorarioDetalle
SET id_salon = NULL
WHERE id_salon IS NOT NULL
  AND id_salon NOT IN (SELECT id_salon FROM dbo.Salones);

-- Eliminar duplicados en DocenteMateria para un mismo (id_grupo, id_materia) si existieran
;WITH CTE_Duplicados AS (
    SELECT id,
           ROW_NUMBER() OVER (PARTITION BY id_grupo, id_materia ORDER BY id DESC) AS rn
    FROM dbo.DocenteMateria
    WHERE id_grupo IS NOT NULL
)
DELETE FROM CTE_Duplicados WHERE rn > 1;

-- Eliminar la tabla duplicada en desuso dbo.Horarios
IF OBJECT_ID('dbo.Horarios', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Horarios;
    PRINT 'Tabla en desuso dbo.Horarios eliminada.';
END
GO

-- -----------------------------------------------------------------------------
-- 2. METADATOS PARA EL SOLVER EN MATERIAS, SALONES Y GRUPOS
-- -----------------------------------------------------------------------------
-- 2.1 Bandera requiere_laboratorio en Materias
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Materias') AND name = 'requiere_laboratorio')
BEGIN
    ALTER TABLE dbo.Materias ADD requiere_laboratorio BIT NOT NULL CONSTRAINT DF_Materias_RequiereLab DEFAULT (0);
    PRINT 'Columna requiere_laboratorio agregada a Materias.';
END
GO

-- Inicializar requiere_laboratorio basándose en los criterios actuales para no perder comportamiento
UPDATE dbo.Materias
SET requiere_laboratorio = 1
WHERE nombre LIKE '%programacion%'
   OR nombre LIKE '%programación%'
   OR nombre LIKE '%base de datos%'
   OR nombre LIKE '%sistemas%';
GO

-- 2.2 Bandera es_laboratorio en Salones
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Salones') AND name = 'es_laboratorio')
BEGIN
    ALTER TABLE dbo.Salones ADD es_laboratorio BIT NOT NULL CONSTRAINT DF_Salones_EsLab DEFAULT (0);
    PRINT 'Columna es_laboratorio agregada a Salones.';
END
GO

UPDATE dbo.Salones
SET es_laboratorio = 1
WHERE nombre LIKE '%cc%'
   OR nombre LIKE '%lab%';
GO

-- 2.3 Columna cupo_alumnos en Grupos para validar contra Salones.capacidad
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Grupos') AND name = 'cupo_alumnos')
BEGIN
    ALTER TABLE dbo.Grupos ADD cupo_alumnos INT NOT NULL CONSTRAINT DF_Grupos_CupoAlumnos DEFAULT (35);
    PRINT 'Columna cupo_alumnos agregada a Grupos.';
END
GO

-- -----------------------------------------------------------------------------
-- 3. LLAVES FORÁNEAS FALTANTES Y BORRADO EN CASCADA EN HorarioDetalle
-- -----------------------------------------------------------------------------
-- Actualizar FK_HD_Proyecto para incluir ON DELETE CASCADE
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_HD_Proyecto' AND delete_referential_action = 0)
BEGIN
    ALTER TABLE dbo.HorarioDetalle DROP CONSTRAINT FK_HD_Proyecto;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_HD_Proyecto')
BEGIN
    ALTER TABLE dbo.HorarioDetalle WITH CHECK
    ADD CONSTRAINT FK_HD_Proyecto FOREIGN KEY (id_proyecto)
    REFERENCES dbo.Proyectos (id_proyecto) ON DELETE CASCADE;
    PRINT 'FK_HD_Proyecto con ON DELETE CASCADE configurada.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_HD_Docente')
BEGIN
    ALTER TABLE dbo.HorarioDetalle WITH CHECK
    ADD CONSTRAINT FK_HD_Docente FOREIGN KEY (id_docente)
    REFERENCES dbo.Docentes (id_docente);
    PRINT 'FK_HD_Docente agregada.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_HD_Salon')
BEGIN
    ALTER TABLE dbo.HorarioDetalle WITH CHECK
    ADD CONSTRAINT FK_HD_Salon FOREIGN KEY (id_salon)
    REFERENCES dbo.Salones (id_salon);
    PRINT 'FK_HD_Salon agregada.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_HD_Dia')
BEGIN
    ALTER TABLE dbo.HorarioDetalle WITH CHECK
    ADD CONSTRAINT FK_HD_Dia FOREIGN KEY (id_dia)
    REFERENCES dbo.DiasSemana (id_dia);
    PRINT 'FK_HD_Dia agregada.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_HD_Bloque')
BEGIN
    ALTER TABLE dbo.HorarioDetalle WITH CHECK
    ADD CONSTRAINT FK_HD_Bloque FOREIGN KEY (id_bloque)
    REFERENCES dbo.BloquesHora (id_bloque);
    PRINT 'FK_HD_Bloque agregada.';
END
GO

-- -----------------------------------------------------------------------------
-- 4. ÍNDICES ÚNICOS FILTRADOS E ÍNDICES DE RENDIMIENTO PARA EL SOLVER
-- -----------------------------------------------------------------------------
-- 4.1 Garantizar que un docente no tenga dos clases al mismo tiempo en el mismo proyecto
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_HD_ProyectoDocenteDiaBloque' AND object_id = OBJECT_ID('dbo.HorarioDetalle'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_HD_ProyectoDocenteDiaBloque
    ON dbo.HorarioDetalle (id_proyecto, id_docente, id_dia, id_bloque)
    WHERE id_docente IS NOT NULL;
    PRINT 'Índice único filtrado UQ_HD_ProyectoDocenteDiaBloque creado.';
END
GO

-- 4.2 Garantizar que un salón físico no tenga dos clases al mismo tiempo en el mismo proyecto
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_HD_ProyectoSalonDiaBloque' AND object_id = OBJECT_ID('dbo.HorarioDetalle'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_HD_ProyectoSalonDiaBloque
    ON dbo.HorarioDetalle (id_proyecto, id_salon, id_dia, id_bloque)
    WHERE id_salon IS NOT NULL;
    PRINT 'Índice único filtrado UQ_HD_ProyectoSalonDiaBloque creado.';
END
GO

-- 4.3 Garantizar un único docente titular por (id_grupo, id_materia)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_DocenteMateria_GrupoMateriaUnico' AND object_id = OBJECT_ID('dbo.DocenteMateria'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_DocenteMateria_GrupoMateriaUnico
    ON dbo.DocenteMateria (id_grupo, id_materia)
     INCLUDE (id_docente, en_linea)
    WHERE id_grupo IS NOT NULL;
    PRINT 'Índice único filtrado UQ_DocenteMateria_GrupoMateriaUnico creado.';
END
GO

-- 4.4 Índices de cobertura para acelerar consultas de Materias y Grupos
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Materias_CarreraSemestreEstado' AND object_id = OBJECT_ID('dbo.Materias'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Materias_CarreraSemestreEstado
    ON dbo.Materias (id_carrera, semestre, id_estado)
    INCLUDE (nombre, clave, creditos, requiere_laboratorio);
    PRINT 'Índice IX_Materias_CarreraSemestreEstado creado.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Materias_Clave' AND object_id = OBJECT_ID('dbo.Materias'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Materias_Clave
    ON dbo.Materias (clave)
    INCLUDE (id_materia, id_carrera, nombre, creditos, id_estado);
    PRINT 'Índice IX_Materias_Clave creado.';
END
GO

-- -----------------------------------------------------------------------------
-- 5. ACTUALIZACIÓN DE PROCEDIMIENTOS ALMACENADOS PARA EL SOLVER Y VISTAS
-- -----------------------------------------------------------------------------

-- 5.1 sp_ObtenerMateriasPorCarrera: Incluye requiere_laboratorio
CREATE OR ALTER PROCEDURE [dbo].[sp_ObtenerMateriasPorCarrera]
    @id_carrera INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        id_materia,
        nombre,
        clave,
        creditos,
        semestre,
        ISNULL(requiere_laboratorio, 0) AS requiere_laboratorio
    FROM dbo.Materias
    WHERE id_carrera = @id_carrera
      AND id_estado = 1
    ORDER BY semestre, nombre;
END
GO

-- 5.2 sp_GetSalones: Incluye es_laboratorio
CREATE OR ALTER PROCEDURE [dbo].[sp_GetSalones]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        s.id_salon,
        s.nombre AS NombreSalon,
        s.capacidad,
        s.id_edificio,
        ISNULL(e.nombre, 'Sin edificio') AS NombreEdificio,
        s.id_carrera,
        ISNULL(c1.nombre, 'Sin asignar') AS NombreCarrera,
        s.id_carreraSecundaria,
        ISNULL(c2.nombre, '') AS NombreCarreraSecundaria,
        s.id_carreraTerciaria,
        ISNULL(c3.nombre, '') AS NombreCarreraTerciaria,
        ISNULL(s.es_laboratorio, 0) AS es_laboratorio
    FROM dbo.Salones s
    LEFT JOIN dbo.Edificios e ON s.id_edificio = e.id_edificio
    LEFT JOIN dbo.Carreras c1 ON s.id_carrera = c1.id_carrera
    LEFT JOIN dbo.Carreras c2 ON s.id_carreraSecundaria = c2.id_carrera
    LEFT JOIN dbo.Carreras c3 ON s.id_carreraTerciaria = c3.id_carrera
    ORDER BY s.nombre;
END
GO

-- 5.3 sp_GetDocentesGlobalesPorCarrera: Filtra docentes activos (d.id_estado = 1)
CREATE OR ALTER PROCEDURE [dbo].[sp_GetDocentesGlobalesPorCarrera]
    @id_carrera INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT 
        d.id_docente, 
        d.nombre AS NombreCompleto, 
        d.horas_maximas AS HorasMaximas, 
        d.horas_frente_grupo,
        dc.id_carrera AS IdCarreraPrincipal
    FROM [dbo].[Docentes] d
    INNER JOIN [dbo].[DocenteMateria] dm ON d.id_docente = dm.id_docente
    INNER JOIN [dbo].[Materias] m ON dm.id_materia = m.id_materia
    LEFT JOIN [dbo].[DocenteCarrera] dc ON d.id_docente = dc.id_docente AND dc.es_principal = 1
    WHERE m.id_carrera = @id_carrera
      AND d.id_estado = 1;
END
GO

-- 5.4 sp_ObtenerGruposPorCicloYCarrera: Incluye grupos especiales (OR g.es_especial = 1)
CREATE OR ALTER PROCEDURE [dbo].[sp_ObtenerGruposPorCicloYCarrera]
    @ciclo CHAR(1),
    @id_carrera INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        g.id_grupo, 
        g.nombre AS NombreGrupo, 
        g.semestre, 
        g.turno,
        ISNULL(g.es_especial, 0) AS es_especial,
        ISNULL(g.cupo_alumnos, 35) AS cupo_alumnos,
        c.nombre AS NombreCarrera
    FROM dbo.Grupos g
    INNER JOIN dbo.Carreras c ON g.id_carrera = c.id_carrera
    WHERE g.id_carrera = @id_carrera
      AND (
          g.es_especial = 1
          OR (@ciclo = 'B' AND g.semestre % 2 = 1)
          OR (@ciclo = 'A' AND g.semestre % 2 = 0)
      )
    ORDER BY g.semestre, g.nombre;
END
GO

-- 5.5 sp_DiagnosticoCobertura: Soporta tanto materias del semestre como materias asignadas a grupos especiales
CREATE OR ALTER PROCEDURE [dbo].[sp_DiagnosticoCobertura]
    @Ciclo CHAR(1)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        c.id_carrera,
        c.nombre AS NombreCarrera,
        g.semestre AS Semestre,
        g.id_grupo,
        g.nombre AS NombreGrupo,
        m.id_materia,
        m.nombre AS NombreMateria,
        d.nombre AS DocenteAsignado
    FROM dbo.Carreras c
    INNER JOIN dbo.Grupos g ON c.id_carrera = g.id_carrera
    INNER JOIN dbo.Materias m ON c.id_carrera = m.id_carrera
    LEFT JOIN dbo.DocenteMateria dm ON g.id_grupo = dm.id_grupo AND m.id_materia = dm.id_materia
    LEFT JOIN dbo.Docentes d ON dm.id_docente = d.id_docente AND d.id_estado = 1
    WHERE m.id_estado = 1
      AND (
          g.es_especial = 1
          OR (@Ciclo = 'A' AND g.semestre % 2 = 0)
          OR (@Ciclo = 'B' AND g.semestre % 2 <> 0)
      )
      AND (
          (g.es_especial = 0 AND g.semestre = m.semestre)
          OR dm.id_docente IS NOT NULL
      )
    ORDER BY c.id_carrera, g.semestre, g.nombre, m.nombre;
END
GO

-- 5.6 sp_ContarDocentesPorCarrera: Basado en la tabla Carreras en vez de CTE fijo 1..7
CREATE OR ALTER PROCEDURE [dbo].[sp_ContarDocentesPorCarrera]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        c.id_carrera,
        COUNT(d.id_docente) AS TotalDocentes
    FROM dbo.Carreras c
    LEFT JOIN dbo.DocenteCarrera dc ON c.id_carrera = dc.id_carrera AND dc.es_principal = 1
    LEFT JOIN dbo.Docentes d ON dc.id_docente = d.id_docente AND d.id_estado = 1
    GROUP BY c.id_carrera
    ORDER BY c.id_carrera ASC;
END
GO

