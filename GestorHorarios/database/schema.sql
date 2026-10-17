USE [GestorHorarios]
GO
/****** Object:  Table [dbo].[BloquesHora]    Script Date: 16/10/2026 9:56:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BloquesHora](
	[id_bloque] [int] IDENTITY(1,1) NOT NULL,
	[hora_inicio] [time](7) NOT NULL,
	[hora_fin] [time](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_bloque] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Carreras]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Carreras](
	[id_carrera] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [varchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_carrera] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Configuraciones]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Configuraciones](
	[clave] [varchar](50) NOT NULL,
	[valor] [varchar](255) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[clave] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DiasSemana]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DiasSemana](
	[id_dia] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [varchar](20) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_dia] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DisponibilidadDocente]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DisponibilidadDocente](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[id_docente] [int] NOT NULL,
	[id_dia] [int] NOT NULL,
	[id_bloque] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_DisponibilidadDocente] UNIQUE NONCLUSTERED 
(
	[id_docente] ASC,
	[id_dia] ASC,
	[id_bloque] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DocenteCarrera]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DocenteCarrera](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[id_docente] [int] NOT NULL,
	[id_carrera] [int] NOT NULL,
	[es_principal] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_DocenteCarrera] UNIQUE NONCLUSTERED 
(
	[id_docente] ASC,
	[id_carrera] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DocenteMateria]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DocenteMateria](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[id_docente] [int] NOT NULL,
	[id_materia] [int] NOT NULL,
	[id_grupo] [int] NULL,
	[en_linea] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_DocenteMateria_Grupo] UNIQUE NONCLUSTERED 
(
	[id_docente] ASC,
	[id_materia] ASC,
	[id_grupo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Docentes]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Docentes](
	[id_docente] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [varchar](150) NOT NULL,
	[tipo_docente] [varchar](20) NULL,
	[id_estado] [int] NOT NULL,
	[horas_maximas] [int] NULL,
	[horas_frente_grupo] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_docente] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Edificios]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Edificios](
	[id_edificio] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [varchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[id_edificio] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Estados]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Estados](
	[id_estado] [int] NOT NULL,
	[nombre] [varchar](20) NULL,
PRIMARY KEY CLUSTERED 
(
	[id_estado] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Grupos]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Grupos](
	[id_grupo] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [varchar](20) NULL,
	[semestre] [int] NULL,
	[turno] [varchar](20) NULL,
	[id_carrera] [int] NULL,
	[es_especial] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_grupo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HorarioDetalle]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HorarioDetalle](
	[id_detalle] [int] IDENTITY(1,1) NOT NULL,
	[id_proyecto] [int] NOT NULL,
	[id_grupo] [int] NOT NULL,
	[id_materia] [int] NOT NULL,
	[id_docente] [int] NULL,
	[id_salon] [int] NULL,
	[id_dia] [int] NOT NULL,
	[id_bloque] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_detalle] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_HD_GrupoDiaBloque] UNIQUE NONCLUSTERED 
(
	[id_proyecto] ASC,
	[id_grupo] ASC,
	[id_dia] ASC,
	[id_bloque] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Horarios]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Horarios](
	[id_horario] [int] IDENTITY(1,1) NOT NULL,
	[id_proyecto] [int] NULL,
	[id_docente] [int] NULL,
	[id_materia] [int] NULL,
	[id_grupo] [int] NULL,
	[id_salon] [int] NULL,
	[id_dia] [int] NULL,
	[id_bloque] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[id_horario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_Horario_Docente] UNIQUE NONCLUSTERED 
(
	[id_proyecto] ASC,
	[id_docente] ASC,
	[id_dia] ASC,
	[id_bloque] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_Horario_Salon] UNIQUE NONCLUSTERED 
(
	[id_proyecto] ASC,
	[id_salon] ASC,
	[id_dia] ASC,
	[id_bloque] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Materias]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Materias](
	[id_materia] [int] IDENTITY(1,1) NOT NULL,
	[clave] [varchar](20) NULL,
	[nombre] [varchar](150) NULL,
	[creditos] [int] NULL,
	[semestre] [int] NULL,
	[id_carrera] [int] NULL,
	[id_estado] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_materia] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Proyectos]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Proyectos](
	[id_proyecto] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [varchar](100) NULL,
	[anio] [int] NULL,
	[periodo] [nvarchar](100) NOT NULL,
	[fecha_creacion] [datetime] NULL,
	[ciclo] [char](1) NOT NULL,
	[id_estado] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_proyecto] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Salones]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Salones](
	[id_salon] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [varchar](50) NULL,
	[capacidad] [int] NULL,
	[id_carrera] [int] NULL,
	[id_edificio] [int] NULL,
	[id_carreraSecundaria] [int] NULL,
	[id_carreraTerciaria] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[id_salon] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[DocenteMateria] ADD  DEFAULT ((0)) FOR [en_linea]
GO
ALTER TABLE [dbo].[Docentes] ADD  CONSTRAINT [DF_Docentes_Estado]  DEFAULT ((1)) FOR [id_estado]
GO
ALTER TABLE [dbo].[Docentes] ADD  DEFAULT ((20)) FOR [horas_maximas]
GO
ALTER TABLE [dbo].[Docentes] ADD  CONSTRAINT [DF_Docentes_HorasGrupo]  DEFAULT ((0)) FOR [horas_frente_grupo]
GO
ALTER TABLE [dbo].[Grupos] ADD  DEFAULT ((0)) FOR [es_especial]
GO
ALTER TABLE [dbo].[Materias] ADD  CONSTRAINT [DF_Materias_Estado]  DEFAULT ((1)) FOR [id_estado]
GO
ALTER TABLE [dbo].[Proyectos] ADD  DEFAULT (getdate()) FOR [fecha_creacion]
GO
ALTER TABLE [dbo].[Proyectos] ADD  DEFAULT ('B') FOR [ciclo]
GO
ALTER TABLE [dbo].[Proyectos] ADD  DEFAULT ((1)) FOR [id_estado]
GO
ALTER TABLE [dbo].[DisponibilidadDocente]  WITH CHECK ADD FOREIGN KEY([id_bloque])
REFERENCES [dbo].[BloquesHora] ([id_bloque])
GO
ALTER TABLE [dbo].[DisponibilidadDocente]  WITH CHECK ADD FOREIGN KEY([id_dia])
REFERENCES [dbo].[DiasSemana] ([id_dia])
GO
ALTER TABLE [dbo].[DisponibilidadDocente]  WITH CHECK ADD  CONSTRAINT [FK_Disponibilidad_Docente] FOREIGN KEY([id_docente])
REFERENCES [dbo].[Docentes] ([id_docente])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DisponibilidadDocente] CHECK CONSTRAINT [FK_Disponibilidad_Docente]
GO
ALTER TABLE [dbo].[DocenteCarrera]  WITH CHECK ADD FOREIGN KEY([id_carrera])
REFERENCES [dbo].[Carreras] ([id_carrera])
GO
ALTER TABLE [dbo].[DocenteCarrera]  WITH CHECK ADD  CONSTRAINT [FK_DocenteCarrera_Docente] FOREIGN KEY([id_docente])
REFERENCES [dbo].[Docentes] ([id_docente])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DocenteCarrera] CHECK CONSTRAINT [FK_DocenteCarrera_Docente]
GO
ALTER TABLE [dbo].[DocenteMateria]  WITH CHECK ADD FOREIGN KEY([id_materia])
REFERENCES [dbo].[Materias] ([id_materia])
GO
ALTER TABLE [dbo].[DocenteMateria]  WITH CHECK ADD  CONSTRAINT [FK_DocenteMateria_Docente] FOREIGN KEY([id_docente])
REFERENCES [dbo].[Docentes] ([id_docente])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DocenteMateria] CHECK CONSTRAINT [FK_DocenteMateria_Docente]
GO
ALTER TABLE [dbo].[DocenteMateria]  WITH CHECK ADD  CONSTRAINT [FK_DocenteMateria_Grupo] FOREIGN KEY([id_grupo])
REFERENCES [dbo].[Grupos] ([id_grupo])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DocenteMateria] CHECK CONSTRAINT [FK_DocenteMateria_Grupo]
GO
ALTER TABLE [dbo].[Docentes]  WITH CHECK ADD  CONSTRAINT [FK_Docentes_Estado] FOREIGN KEY([id_estado])
REFERENCES [dbo].[Estados] ([id_estado])
GO
ALTER TABLE [dbo].[Docentes] CHECK CONSTRAINT [FK_Docentes_Estado]
GO
ALTER TABLE [dbo].[Grupos]  WITH CHECK ADD FOREIGN KEY([id_carrera])
REFERENCES [dbo].[Carreras] ([id_carrera])
GO
ALTER TABLE [dbo].[HorarioDetalle]  WITH CHECK ADD  CONSTRAINT [FK_HD_Grupo] FOREIGN KEY([id_grupo])
REFERENCES [dbo].[Grupos] ([id_grupo])
GO
ALTER TABLE [dbo].[HorarioDetalle] CHECK CONSTRAINT [FK_HD_Grupo]
GO
ALTER TABLE [dbo].[HorarioDetalle]  WITH CHECK ADD  CONSTRAINT [FK_HD_Materia] FOREIGN KEY([id_materia])
REFERENCES [dbo].[Materias] ([id_materia])
GO
ALTER TABLE [dbo].[HorarioDetalle] CHECK CONSTRAINT [FK_HD_Materia]
GO
ALTER TABLE [dbo].[HorarioDetalle]  WITH CHECK ADD  CONSTRAINT [FK_HD_Proyecto] FOREIGN KEY([id_proyecto])
REFERENCES [dbo].[Proyectos] ([id_proyecto])
GO
ALTER TABLE [dbo].[HorarioDetalle] CHECK CONSTRAINT [FK_HD_Proyecto]
GO
ALTER TABLE [dbo].[Horarios]  WITH CHECK ADD FOREIGN KEY([id_bloque])
REFERENCES [dbo].[BloquesHora] ([id_bloque])
GO
ALTER TABLE [dbo].[Horarios]  WITH CHECK ADD FOREIGN KEY([id_dia])
REFERENCES [dbo].[DiasSemana] ([id_dia])
GO
ALTER TABLE [dbo].[Horarios]  WITH CHECK ADD FOREIGN KEY([id_docente])
REFERENCES [dbo].[Docentes] ([id_docente])
GO
ALTER TABLE [dbo].[Horarios]  WITH CHECK ADD FOREIGN KEY([id_grupo])
REFERENCES [dbo].[Grupos] ([id_grupo])
GO
ALTER TABLE [dbo].[Horarios]  WITH CHECK ADD FOREIGN KEY([id_materia])
REFERENCES [dbo].[Materias] ([id_materia])
GO
ALTER TABLE [dbo].[Horarios]  WITH CHECK ADD FOREIGN KEY([id_proyecto])
REFERENCES [dbo].[Proyectos] ([id_proyecto])
GO
ALTER TABLE [dbo].[Horarios]  WITH CHECK ADD FOREIGN KEY([id_salon])
REFERENCES [dbo].[Salones] ([id_salon])
GO
ALTER TABLE [dbo].[Materias]  WITH CHECK ADD FOREIGN KEY([id_carrera])
REFERENCES [dbo].[Carreras] ([id_carrera])
GO
ALTER TABLE [dbo].[Materias]  WITH CHECK ADD  CONSTRAINT [FK_Materias_Estado] FOREIGN KEY([id_estado])
REFERENCES [dbo].[Estados] ([id_estado])
GO
ALTER TABLE [dbo].[Materias] CHECK CONSTRAINT [FK_Materias_Estado]
GO
ALTER TABLE [dbo].[Salones]  WITH CHECK ADD FOREIGN KEY([id_carrera])
REFERENCES [dbo].[Carreras] ([id_carrera])
GO
ALTER TABLE [dbo].[Salones]  WITH CHECK ADD  CONSTRAINT [FK_Edificios_Carreras_Secundaria] FOREIGN KEY([id_carreraSecundaria])
REFERENCES [dbo].[Carreras] ([id_carrera])
GO
ALTER TABLE [dbo].[Salones] CHECK CONSTRAINT [FK_Edificios_Carreras_Secundaria]
GO
ALTER TABLE [dbo].[Salones]  WITH CHECK ADD  CONSTRAINT [FK_Edificios_Carreras_Terciaria] FOREIGN KEY([id_carreraTerciaria])
REFERENCES [dbo].[Carreras] ([id_carrera])
GO
ALTER TABLE [dbo].[Salones] CHECK CONSTRAINT [FK_Edificios_Carreras_Terciaria]
GO
ALTER TABLE [dbo].[Salones]  WITH CHECK ADD  CONSTRAINT [FK_Salones_Edificio] FOREIGN KEY([id_edificio])
REFERENCES [dbo].[Edificios] ([id_edificio])
GO
ALTER TABLE [dbo].[Salones] CHECK CONSTRAINT [FK_Salones_Edificio]
GO
ALTER TABLE [dbo].[Docentes]  WITH CHECK ADD  CONSTRAINT [CK_HorasSemanales_Rango] CHECK  (([horas_maximas]>=(1) AND [horas_maximas]<=(60)))
GO
ALTER TABLE [dbo].[Docentes] CHECK CONSTRAINT [CK_HorasSemanales_Rango]
GO
/****** Object:  StoredProcedure [dbo].[sp_BuscarDocentesGlobal]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_BuscarDocentesGlobal]
    @filtro NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        d.id_docente,
        d.nombre AS NombreCompleto,
        d.horas_maximas,
        d.horas_frente_grupo,
        dc.id_carrera AS IdCarreraPrincipal,
        c.nombre AS NombreCarreraPrincipal
    FROM Docentes d
    LEFT JOIN DocenteCarrera dc ON d.id_docente = dc.id_docente AND dc.es_principal = 1
    LEFT JOIN Carreras c ON dc.id_carrera = c.id_carrera
    WHERE d.nombre LIKE '%' + @filtro + '%'
      AND d.id_estado = 1
    ORDER BY d.nombre;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_ContarDocentesPorCarrera]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_ContarDocentesPorCarrera]
AS
BEGIN
    SET NOCOUNT ON;

    -- Se crea una tabla con los 7 IDs de las carreras (del 1 al 7) para que ninguna falte en el diseño
    WITH CarrerasCTE AS (
        SELECT 1 AS id_carrera UNION ALL
        SELECT 2 UNION ALL
        SELECT 3 UNION ALL
        SELECT 4 UNION ALL
        SELECT 5 UNION ALL
        SELECT 6 UNION ALL
        SELECT 7
    )
    -- Se cuenta el total de docentes activos que tienen esa carrera como principal
    SELECT 
        c.id_carrera,
        COUNT(d.id_docente) AS TotalDocentes
    FROM 
        CarrerasCTE c
    LEFT JOIN 
        DocenteCarrera dc ON c.id_carrera = dc.id_carrera AND dc.es_principal = 1
    LEFT JOIN 
        Docentes d ON dc.id_docente = d.id_docente AND d.id_estado = 1
    GROUP BY 
        c.id_carrera
    ORDER BY 
        c.id_carrera ASC;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_ContarGruposPorCarrera]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- STORED PROCEDURES PARA GRUPOS
-- =============================================

CREATE   PROCEDURE [dbo].[sp_ContarGruposPorCarrera]
AS
BEGIN
    SELECT c.id_carrera, c.nombre, COUNT(g.id_grupo) AS TotalGrupos
    FROM Carreras c
    LEFT JOIN Grupos g ON c.id_carrera = g.id_carrera
    GROUP BY c.id_carrera, c.nombre
    ORDER BY c.id_carrera;
END;

GO
/****** Object:  StoredProcedure [dbo].[sp_ContarMateriasPorCarrera]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_ContarMateriasPorCarrera]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        C.id_carrera,
        C.nombre AS Carrera,
        COUNT(M.id_materia) AS TotalMaterias
    FROM Carreras C
    LEFT JOIN Materias M ON C.id_carrera = M.id_carrera
    GROUP BY C.id_carrera, C.nombre
    ORDER BY C.id_carrera;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_ContarProyectos]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[sp_ContarProyectos]
AS
BEGIN
    SELECT COUNT(*) AS Total FROM Proyectos WHERE id_estado = 1;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_ContarSalones]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- STORED PROCEDURES PARA SALONES
-- =============================================

CREATE   PROCEDURE [dbo].[sp_ContarSalones]
AS
BEGIN
    SELECT COUNT(*) AS TotalSalones FROM Salones;
END;

GO
/****** Object:  StoredProcedure [dbo].[sp_DiagnosticoCobertura]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_DiagnosticoCobertura]
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
    FROM Carreras c
    INNER JOIN Grupos g ON c.id_carrera = g.id_carrera
    INNER JOIN Materias m ON c.id_carrera = m.id_carrera AND g.semestre = m.semestre
    -- Buscamos si hay un maestro asignado a ESE grupo y ESA materia específica
    LEFT JOIN DocenteMateria dm ON g.id_grupo = dm.id_grupo AND m.id_materia = dm.id_materia
    LEFT JOIN Docentes d ON dm.id_docente = d.id_docente
    WHERE m.id_estado = 1
      AND ((@Ciclo = 'A' AND g.semestre % 2 = 0) OR (@Ciclo = 'B' AND g.semestre % 2 <> 0))
    ORDER BY c.id_carrera, g.semestre, g.nombre, m.nombre;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_DiagnosticoViabilidad]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[sp_DiagnosticoViabilidad]
    @Ciclo CHAR(1)
AS
BEGIN
    SET NOCOUNT ON;

    WITH Demanda AS (
        SELECT 
            m.clave,
            MAX(m.nombre) AS nombre_materia,
            MAX(m.creditos) AS creditos_por_grupo,
            -- NUEVO: Si la materia está en más de 1 carrera, es Tronco Común
            CASE 
                WHEN COUNT(DISTINCT m.id_carrera) > 1 THEN 'Materias Compartidas (Tronco Común)'
                ELSE MAX(car.nombre) 
            END AS area_academica,
            COUNT(g.id_grupo) AS total_grupos,
            (COUNT(g.id_grupo) * MAX(m.creditos)) AS horas_demandadas
        FROM [dbo].[Grupos] g
        INNER JOIN [dbo].[Materias] m ON g.semestre = m.semestre AND g.id_carrera = m.id_carrera
        INNER JOIN [dbo].[Carreras] car ON m.id_carrera = car.id_carrera
        WHERE 
            m.id_estado = 1
            AND (
                (@Ciclo = 'A' AND g.semestre % 2 = 0) OR 
                (@Ciclo = 'B' AND g.semestre % 2 <> 0)
            )
        GROUP BY m.clave
    ),
    
    Capacidad AS (
        SELECT 
            clave,
            COUNT(id_docente) AS total_docentes,
            SUM(horas_maximas) AS horas_capacidad
        FROM (
            SELECT DISTINCT m.clave, d.id_docente, d.horas_maximas
            FROM [dbo].[Docentes] d
            INNER JOIN [dbo].[DocenteMateria] dm ON d.id_docente = dm.id_docente
            INNER JOIN [dbo].[Materias] m ON dm.id_materia = m.id_materia
            WHERE d.id_estado = 1
        ) AS DocentesUnicos
        GROUP BY clave
    )

    SELECT 
        d.clave AS ClaveMateria,
        d.nombre_materia AS NombreMateria,
        d.area_academica AS AreaAcademica,
        d.creditos_por_grupo AS Creditos,
        d.total_grupos AS Grupos,
        d.horas_demandadas AS HorasRequeridas,
        ISNULL(c.total_docentes, 0) AS DocentesHabilitados,
        ISNULL(c.horas_capacidad, 0) AS CapacidadHoras,
        CASE 
            WHEN ISNULL(c.horas_capacidad, 0) >= d.horas_demandadas + 10 THEN 'Verde'
            WHEN ISNULL(c.horas_capacidad, 0) >= d.horas_demandadas THEN 'Amarillo'
            ELSE 'Rojo'
        END AS Semaforo
    FROM Demanda d
    LEFT JOIN Capacidad c ON d.clave = c.clave
    ORDER BY 
        CASE 
            WHEN ISNULL(c.horas_capacidad, 0) >= d.horas_demandadas + 10 THEN 3 
            WHEN ISNULL(c.horas_capacidad, 0) >= d.horas_demandadas THEN 2 
            ELSE 1 
        END, 
        d.horas_demandadas DESC;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_EstadisticasDashboard]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[sp_EstadisticasDashboard]
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
/****** Object:  StoredProcedure [dbo].[sp_GetDocentesAsignablesAMateria]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_GetDocentesAsignablesAMateria]
    @id_materia INT,
    @id_proyecto INT
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. Averiguamos la CLAVE oficial de la materia
    DECLARE @ClaveMateria VARCHAR(20);
    SELECT @ClaveMateria = clave FROM [dbo].[Materias] WHERE id_materia = @id_materia;

    -- 2. Buscamos a los maestros por la CLAVE y traemos sus horas
    SELECT DISTINCT
        d.id_docente,
        d.nombre,
        ISNULL(d.horas_frente_grupo, 0) AS hfg,
        (SELECT COUNT(*) 
         FROM HorarioDetalle hd 
         WHERE hd.id_docente = d.id_docente 
           AND hd.id_proyecto = @id_proyecto) AS HorasRepartidas,
        ISNULL(c_prin.nombre, 'Sin departamento asignado') AS CarreraPrincipal
    FROM [dbo].[Docentes] d
    INNER JOIN [dbo].[DocenteMateria] dm ON d.id_docente = dm.id_docente
    INNER JOIN [dbo].[Materias] m ON dm.id_materia = m.id_materia
    LEFT JOIN [dbo].[DocenteCarrera] dc_prin ON d.id_docente = dc_prin.id_docente AND dc_prin.es_principal = 1
    LEFT JOIN [dbo].[Carreras] c_prin ON dc_prin.id_carrera = c_prin.id_carrera
    WHERE m.clave = @ClaveMateria
      AND d.id_estado = 1
    ORDER BY d.nombre;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_GetDocentesByCarrera]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_GetDocentesByCarrera]
    @id_carrera INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        d.id_docente,
        d.nombre            AS NombreCompleto,
        d.tipo_docente      AS TipoTiempo,
        d.horas_maximas     AS HorasMaximas,
        d.horas_frente_grupo,  -- ¡AQUÍ ESTÁ LA NUEVA LÍNEA!

        -- Id carrera principal (para edición)
        (SELECT dc2.id_carrera FROM DocenteCarrera dc2
         WHERE dc2.id_docente = d.id_docente AND dc2.es_principal = 1) AS IdCarreraPrincipal,

        -- Nombre carrera principal
        (SELECT c.nombre FROM Carreras c
         JOIN DocenteCarrera dc2 ON c.id_carrera = dc2.id_carrera
         WHERE dc2.id_docente = d.id_docente AND dc2.es_principal = 1) AS CarreraPrincipal,

        -- Carreras secundarias (nombres)
        ISNULL(STUFF((
            SELECT ', ' + c2.nombre
            FROM Carreras c2 JOIN DocenteCarrera dc3 ON c2.id_carrera = dc3.id_carrera
            WHERE dc3.id_docente = d.id_docente AND dc3.es_principal = 0
            FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'), 1, 2, ''), 'Ninguna') AS CarreraSecundaria,

        -- Materias (nombres)
        ISNULL(STUFF((
            SELECT ', ' + m.nombre
            FROM Materias m JOIN DocenteMateria dm ON m.id_materia = dm.id_materia
            WHERE dm.id_docente = d.id_docente
            FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'), 1, 2, ''), '') AS Materias,

        -- Horario legible: "Lunes 7:30-8:30, Martes 9:30-10:30"
        ISNULL(STUFF((
            SELECT ', ' + dia.nombre + ' ' +
                   FORMAT(bh.hora_inicio, 'hh\:mm') + '-' +
                   FORMAT(bh.hora_fin,   'hh\:mm')
            FROM DisponibilidadDocente dd
            JOIN DiasSemana dia ON dd.id_dia = dia.id_dia
            JOIN BloquesHora bh  ON dd.id_bloque = bh.id_bloque
            WHERE dd.id_docente = d.id_docente
            ORDER BY dia.id_dia, bh.hora_inicio
            FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'), 1, 2, ''), '') AS HorarioLaboral,

        -- Horario para Canvas: "Lunes:0-3\nMartes:1-4" (índices de HORAS_DISPONIBLES)
        ISNULL(STUFF((
            SELECT CHAR(10) + dia.nombre + ':' +
                   CAST(bh.id_bloque - 1 AS VARCHAR) + '-' +
                   CAST(bh.id_bloque - 1 AS VARCHAR)
            FROM DisponibilidadDocente dd
            JOIN DiasSemana dia ON dd.id_dia = dia.id_dia
            JOIN BloquesHora bh  ON dd.id_bloque = bh.id_bloque
            WHERE dd.id_docente = d.id_docente
            ORDER BY dia.id_dia, bh.hora_inicio
            FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'), 1, 1, ''), '') AS HorarioCanvas,

        -- Total horas asignadas
        (SELECT COUNT(*) FROM DisponibilidadDocente dd2
         WHERE dd2.id_docente = d.id_docente) AS HorasAsignadas

    FROM Docentes d
    JOIN DocenteCarrera dc ON d.id_docente = dc.id_docente
    WHERE dc.id_carrera = @id_carrera
      AND dc.es_principal = 1
      AND d.id_estado = 1
    ORDER BY d.nombre;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_GetDocentesConHorarioPorProyecto]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_GetDocentesConHorarioPorProyecto]
    @id_proyecto INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT DISTINCT 
        d.id_docente, 
        d.nombre, 
        d.horas_frente_grupo,
        ISNULL(c_prin.nombre, 'Sin Departamento') AS CarreraPrincipal
    FROM Docentes d
    INNER JOIN HorarioDetalle hd ON d.id_docente = hd.id_docente
    LEFT JOIN DocenteCarrera dc_prin ON d.id_docente = dc_prin.id_docente AND dc_prin.es_principal = 1
    LEFT JOIN Carreras c_prin ON dc_prin.id_carrera = c_prin.id_carrera
    WHERE hd.id_proyecto = @id_proyecto AND d.id_estado = 1
    ORDER BY d.nombre;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_GetDocentesGlobalesPorCarrera]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_GetDocentesGlobalesPorCarrera]
    @id_carrera INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT 
        d.id_docente, 
        d.nombre AS NombreCompleto, 
        d.horas_maximas AS HorasMaximas, 
        d.horas_frente_grupo, -- ¡AQUÍ ESTÁ LA NUEVA VARIABLE!
        dc.id_carrera AS IdCarreraPrincipal
    FROM [dbo].[Docentes] d
    -- Cruzamos con DocenteMateria para ver qué sabe dar
    INNER JOIN [dbo].[DocenteMateria] dm ON d.id_docente = dm.id_docente
    -- Cruzamos con Materias para saber a qué carrera pertenece esa materia
    INNER JOIN [dbo].[Materias] m ON dm.id_materia = m.id_materia
    -- Hacemos un LEFT JOIN para traer su carrera principal si es que tiene una asignada
    LEFT JOIN [dbo].[DocenteCarrera] dc ON d.id_docente = dc.id_docente AND dc.es_principal = 1
    WHERE m.id_carrera = @id_carrera;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_GetDocentesPorMateria]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_GetDocentesPorMateria]
    @id_materia INT
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. Averiguamos la CLAVE oficial de la materia
    DECLARE @ClaveMateria VARCHAR(50);
    SELECT @ClaveMateria = clave FROM [dbo].[Materias] WHERE id_materia = @id_materia;

    -- 2. Buscamos a los maestros y concatenamos las carreras en las que están dados de alta
    SELECT DISTINCT
        d.id_docente,
        d.nombre AS NombreDocente,
        ISNULL(c_prin.nombre, 'Sin departamento asignado') AS CarreraPrincipal,
        
        -- Truco SQL para concatenar múltiples filas (carreras) en un solo texto separado por comas
        ISNULL(
            STUFF((
                SELECT ', ' + c.nombre
                FROM [dbo].[DocenteCarrera] dc
                INNER JOIN [dbo].[Carreras] c ON dc.id_carrera = c.id_carrera
                WHERE dc.id_docente = d.id_docente
                FOR XML PATH('')
            ), 1, 2, ''), 'Ninguna asignada'
        ) AS CarrerasAsignadas

    FROM [dbo].[Docentes] d
    INNER JOIN [dbo].[DocenteMateria] dm ON d.id_docente = dm.id_docente
    INNER JOIN [dbo].[Materias] m ON dm.id_materia = m.id_materia
    LEFT JOIN [dbo].[DocenteCarrera] dc_prin ON d.id_docente = dc_prin.id_docente AND dc_prin.es_principal = 1
    LEFT JOIN [dbo].[Carreras] c_prin ON dc_prin.id_carrera = c_prin.id_carrera
    WHERE m.clave = @ClaveMateria
    ORDER BY d.nombre;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_GetGruposByCarrera]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[sp_GetGruposByCarrera]
    @id_carrera INT
AS
BEGIN
    SELECT 
        g.id_grupo,
        g.nombre AS NombreGrupo,
        g.semestre,
        g.turno,
        c.nombre AS NombreCarrera
    FROM Grupos g
    INNER JOIN Carreras c ON g.id_carrera = c.id_carrera
    WHERE g.id_carrera = @id_carrera
    ORDER BY g.semestre, g.nombre;
END;

GO
/****** Object:  StoredProcedure [dbo].[sp_GetHorarioDelGrupo]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_GetHorarioDelGrupo]
    @idGrupo INT,
    @idProyecto INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        h.id_dia, 
        h.id_bloque, 
        m.nombre AS Materia, 
        d.nombre AS Docente, 
        h.id_docente, 
        s.nombre AS Salon,
        e.nombre AS Edificio -- NUEVO: Traemos el edificio del salón
    FROM [dbo].[HorarioDetalle] h 
    INNER JOIN [dbo].[Materias] m ON h.id_materia = m.id_materia 
    LEFT JOIN [dbo].[Docentes] d ON h.id_docente = d.id_docente 
    LEFT JOIN [dbo].[Salones] s ON h.id_salon = s.id_salon
    LEFT JOIN [dbo].[Edificios] e ON s.id_edificio = e.id_edificio -- Asegúrate de que tu FK en Salones se llame id_edificio
    WHERE h.id_grupo = @idGrupo 
      AND h.id_proyecto = @idProyecto;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_GetHorarioPersonalDocente]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_GetHorarioPersonalDocente]
    @id_proyecto INT,
    @id_docente INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        h.id_dia,
        h.id_bloque,
        m.nombre AS NombreMateria,
        g.nombre AS NombreGrupo
    FROM [dbo].[HorarioDetalle] h
    INNER JOIN [dbo].[Materias] m ON h.id_materia = m.id_materia
    INNER JOIN [dbo].[Grupos] g ON h.id_grupo = g.id_grupo
    WHERE h.id_proyecto = @id_proyecto 
      AND h.id_docente = @id_docente;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_GetSalones]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_GetSalones]
AS
BEGIN
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
        ISNULL(c3.nombre, '') AS NombreCarreraTerciaria
    FROM Salones s
    LEFT JOIN Edificios e ON s.id_edificio = e.id_edificio
    LEFT JOIN Carreras c1 ON s.id_carrera = c1.id_carrera
    LEFT JOIN Carreras c2 ON s.id_carreraSecundaria = c2.id_carrera
    LEFT JOIN Carreras c3 ON s.id_carreraTerciaria = c3.id_carrera
    ORDER BY s.nombre;
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_ObtenerGruposParaMateria]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_ObtenerGruposParaMateria]
    @id_carrera INT,
    @semestre INT,
    @id_materia INT -- NUEVO PARÁMETRO REQUERIDO
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Devuelve los grupos, y cruza con DocenteMateria y Docentes para ver si ya tienen dueño
    SELECT 
        g.id_grupo, 
        g.nombre AS NombreGrupo, 
        g.turno,
        g.es_especial,
        dm.id_docente AS IdDocenteAsignado,
        d.nombre AS NombreDocenteAsignado
    FROM [dbo].[Grupos] g
    LEFT JOIN [dbo].[DocenteMateria] dm ON g.id_grupo = dm.id_grupo AND dm.id_materia = @id_materia
    LEFT JOIN [dbo].[Docentes] d ON dm.id_docente = d.id_docente
    WHERE g.id_carrera = @id_carrera 
      AND (g.semestre = @semestre OR g.es_especial = 1)
    ORDER BY g.es_especial ASC, g.nombre;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_ObtenerGruposPorCicloYCarrera]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[sp_ObtenerGruposPorCicloYCarrera]
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
/****** Object:  StoredProcedure [dbo].[sp_ObtenerMateriasPorCarrera]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_ObtenerMateriasPorCarrera]
    @id_carrera INT
AS
BEGIN
    SELECT
        id_materia,
        nombre,
        clave,
        creditos,
        semestre
    FROM Materias
    WHERE id_carrera = @id_carrera
      AND id_estado = 1
    ORDER BY semestre, nombre
END
GO
/****** Object:  StoredProcedure [dbo].[sp_ObtenerNombreCarrera]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_ObtenerNombreCarrera]
    @id_carrera INT
AS
BEGIN
    SELECT nombre
    FROM Carreras
    WHERE id_carrera = @id_carrera
END
GO
/****** Object:  StoredProcedure [dbo].[sp_ObtenerProyectos]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[sp_ObtenerProyectos]
AS
BEGIN
    SELECT id_proyecto, nombre, anio, periodo, ciclo, fecha_creacion
    FROM Proyectos
    WHERE id_estado = 1
    ORDER BY fecha_creacion DESC;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_ObtenerTodasLasCarreras]    Script Date: 16/10/2026 9:56:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_ObtenerTodasLasCarreras]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT id_carrera, nombre 
    FROM Carreras
    ORDER BY id_carrera;
END
GO

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

-- 5.5 sp_DiagnosticoCobertura: Soporta asignaciones por grupo y asignaciones generales por materia
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
        doc.nombre AS DocenteAsignado
    FROM dbo.Carreras c
    INNER JOIN dbo.Grupos g ON c.id_carrera = g.id_carrera
    INNER JOIN dbo.Materias m ON c.id_carrera = m.id_carrera
    OUTER APPLY (
        SELECT TOP 1 d.nombre, dm.id_docente
        FROM dbo.DocenteMateria dm
        INNER JOIN dbo.Docentes d ON dm.id_docente = d.id_docente AND d.id_estado = 1
        WHERE dm.id_materia = m.id_materia
          AND (dm.id_grupo = g.id_grupo OR dm.id_grupo IS NULL)
        ORDER BY CASE WHEN dm.id_grupo = g.id_grupo THEN 0 ELSE 1 END, d.horas_frente_grupo ASC
    ) doc
    WHERE m.id_estado = 1
      AND (
          g.es_especial = 1
          OR (@Ciclo = 'A' AND g.semestre % 2 = 0)
          OR (@Ciclo = 'B' AND g.semestre % 2 <> 0)
      )
      AND (
          (g.es_especial = 0 AND g.semestre = m.semestre)
          OR EXISTS (SELECT 1 FROM dbo.DocenteMateria dm2 WHERE dm2.id_grupo = g.id_grupo AND dm2.id_materia = m.id_materia)
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