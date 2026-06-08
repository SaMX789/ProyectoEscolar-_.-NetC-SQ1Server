-- ============================================================
-- SCRIPT: Actualizar docentes con horarios realistas,
--         carreras secundarias y materias secundarias
-- ============================================================
-- REGLAS:
--   TC (Tiempo Completo): 32-40h/semana, 8h/día máximo
--   MT (Medio Tiempo)   : 12-20h/semana, algunos no vienen 1-2 días
--   Carreras secundarias: docentes afines tienen carreras extra
--     pero más materias de su principal que de la secundaria
-- BLOQUES:
--   1=7:30-8:30,  2=8:30-9:30,  3=9:30-10:30, 4=10:30-11:30,
--   5=11:30-12:30,6=12:30-13:30, 7=13:30-14:30,8=14:30-15:30,
--   9=15:30-16:30,10=16:30-17:30,11=17:30-18:30,12=18:30-19:30
-- DÍAS: 1=Lun 2=Mar 3=Mié 4=Jue 5=Vie
-- CARRERAS: 1=Sistemas 2=Civil 3=Comunitario 4=Gestión 5=Industrial 6=Bioquímica
-- ============================================================

-- 1. LIMPIAR TABLAS EN ORDEN (respetando FK)
DELETE FROM DisponibilidadDocente;
DELETE FROM DocenteMateria;
DELETE FROM DocenteCarrera;
DELETE FROM Docentes;

DBCC CHECKIDENT ('Docentes', RESEED, 0);
DBCC CHECKIDENT ('DocenteCarrera', RESEED, 0);
DBCC CHECKIDENT ('DocenteMateria', RESEED, 0);
DBCC CHECKIDENT ('DisponibilidadDocente', RESEED, 0);
GO

-- ============================================================
-- ?? SISTEMAS COMPUTACIONALES (id_carrera = 1)
-- ============================================================

-- ?? 1. Dr. Jimmy Josué Peña Koo ??????????????????????????????
-- TC · 40h · Lun-Vie bloques 1-8 (8h×5=40h)
-- Principal: Sistemas | Secundaria: Industrial (simulación/investigación)
-- Materias Sist: IA(49), Simulacion(23), Graficacion(25), Met.Numericos(20),
--   Prog.Logica(43), Verif.Validacion(50)
-- Materias Ind: Simulacion Industrial(189), Invest.Operaciones II(182)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('Dr. Jimmy Josué Peña Koo', 'TIEMPO COMPLETO', 1, 40);
DECLARE @d1 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d1, 1, 1);
INSERT INTO DocenteCarrera VALUES (@d1, 5, 0);
INSERT INTO DocenteMateria VALUES (@d1,49),(@d1,23),(@d1,25),(@d1,20),(@d1,43),(@d1,50);
INSERT INTO DocenteMateria VALUES (@d1,189),(@d1,182);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d1,1,1),(@d1,1,2),(@d1,1,3),(@d1,1,4),(@d1,1,5),(@d1,1,6),(@d1,1,7),(@d1,1,8),
(@d1,2,1),(@d1,2,2),(@d1,2,3),(@d1,2,4),(@d1,2,5),(@d1,2,6),(@d1,2,7),(@d1,2,8),
(@d1,3,1),(@d1,3,2),(@d1,3,3),(@d1,3,4),(@d1,3,5),(@d1,3,6),(@d1,3,7),(@d1,3,8),
(@d1,4,1),(@d1,4,2),(@d1,4,3),(@d1,4,4),(@d1,4,5),(@d1,4,6),(@d1,4,7),(@d1,4,8),
(@d1,5,1),(@d1,5,2),(@d1,5,3),(@d1,5,4),(@d1,5,5),(@d1,5,6),(@d1,5,7),(@d1,5,8);
GO

-- ?? 2. Mdeisw. David Ariel Aviles Poot ???????????????????????
-- TC · 36h · Lun-Jue 8h + Vie 4h (bloques 1-8 L-J, 1-4 Vie)
-- Principal: Sistemas | Secundaria: Gestión (TICs empresariales)
-- Materias Sist: Ing.Software(35), Gest.Proyectos(41), FundIS(29),
--   Proceso.Desarrollo(48), Metodologias(47), Prog.Web(46)
-- Materias Gest: Fund.Gestión(208), Software.Apl(211)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('Mdeisw. David Ariel Aviles Poot', 'TIEMPO COMPLETO', 1, 36);
DECLARE @d2 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d2, 1, 1);
INSERT INTO DocenteCarrera VALUES (@d2, 4, 0);
INSERT INTO DocenteMateria VALUES (@d2,35),(@d2,41),(@d2,29),(@d2,48),(@d2,47),(@d2,46);
INSERT INTO DocenteMateria VALUES (@d2,208),(@d2,211);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d2,1,1),(@d2,1,2),(@d2,1,3),(@d2,1,4),(@d2,1,5),(@d2,1,6),(@d2,1,7),(@d2,1,8),
(@d2,2,1),(@d2,2,2),(@d2,2,3),(@d2,2,4),(@d2,2,5),(@d2,2,6),(@d2,2,7),(@d2,2,8),
(@d2,3,1),(@d2,3,2),(@d2,3,3),(@d2,3,4),(@d2,3,5),(@d2,3,6),(@d2,3,7),(@d2,3,8),
(@d2,4,1),(@d2,4,2),(@d2,4,3),(@d2,4,4),(@d2,4,5),(@d2,4,6),(@d2,4,7),(@d2,4,8),
(@d2,5,1),(@d2,5,2),(@d2,5,3),(@d2,5,4);
GO

-- ?? 3. MI Cinthia del Carmen Balam Almeida ???????????????????
-- TC · 40h · Lun-Vie bloques 3-10 (turno intermedio, 8h×5=40h)
-- Principal: Sistemas | Sin secundaria
-- Materias: Estructura.Datos(14), Fund.Prog(2), POO(8), TopAvanz(21),
--   Prog.Moviles(51), Prog.WebII(52)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('MI Cinthia del Carmen Balam Almeida', 'TIEMPO COMPLETO', 1, 40);
DECLARE @d3 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d3, 1, 1);
INSERT INTO DocenteMateria VALUES (@d3,14),(@d3,2),(@d3,8),(@d3,21),(@d3,51),(@d3,52);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d3,1,3),(@d3,1,4),(@d3,1,5),(@d3,1,6),(@d3,1,7),(@d3,1,8),(@d3,1,9),(@d3,1,10),
(@d3,2,3),(@d3,2,4),(@d3,2,5),(@d3,2,6),(@d3,2,7),(@d3,2,8),(@d3,2,9),(@d3,2,10),
(@d3,3,3),(@d3,3,4),(@d3,3,5),(@d3,3,6),(@d3,3,7),(@d3,3,8),(@d3,3,9),(@d3,3,10),
(@d3,4,3),(@d3,4,4),(@d3,4,5),(@d3,4,6),(@d3,4,7),(@d3,4,8),(@d3,4,9),(@d3,4,10),
(@d3,5,3),(@d3,5,4),(@d3,5,5),(@d3,5,6),(@d3,5,7),(@d3,5,8),(@d3,5,9),(@d3,5,10);
GO

-- ?? 4. MI Jorge Manuel Dzul Huchim ???????????????????????????
-- TC · 32h · Mar-Vie bloques 5-12 (Lun libre, 8h×4=32h)
-- Principal: Sistemas | Sin secundaria
-- Materias: Fund.BD(22), Admin.BD(34), TallerBD(28), Redes(32),
--   AdminRedes(44), Conmutacion(38)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('MI Jorge Manuel Dzul Huchim', 'TIEMPO COMPLETO', 1, 32);
DECLARE @d4 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d4, 1, 1);
INSERT INTO DocenteMateria VALUES (@d4,22),(@d4,34),(@d4,28),(@d4,32),(@d4,44),(@d4,38);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d4,2,5),(@d4,2,6),(@d4,2,7),(@d4,2,8),(@d4,2,9),(@d4,2,10),(@d4,2,11),(@d4,2,12),
(@d4,3,5),(@d4,3,6),(@d4,3,7),(@d4,3,8),(@d4,3,9),(@d4,3,10),(@d4,3,11),(@d4,3,12),
(@d4,4,5),(@d4,4,6),(@d4,4,7),(@d4,4,8),(@d4,4,9),(@d4,4,10),(@d4,4,11),(@d4,4,12),
(@d4,5,5),(@d4,5,6),(@d4,5,7),(@d4,5,8),(@d4,5,9),(@d4,5,10),(@d4,5,11),(@d4,5,12);
GO

-- ?? 5. MEE Cesar Zenet Lopez Cruz ????????????????????????????
-- TC · 40h · Lun-Vie bloques 5-12 (turno vespertino, 8h×5=40h)
-- Principal: Sistemas | Secundaria: Industrial (electrónica/control)
-- Materias Sist: PrincElect(24), LenAut1(31), LenAut2(37), SistProg(42)
-- Materias Ind: Electric.Electron(160), Propiedad.Mat(161)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('MEE Cesar Zenet Lopez Cruz', 'TIEMPO COMPLETO', 1, 40);
DECLARE @d5 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d5, 1, 1);
INSERT INTO DocenteCarrera VALUES (@d5, 5, 0);
INSERT INTO DocenteMateria VALUES (@d5,24),(@d5,31),(@d5,37),(@d5,42);
INSERT INTO DocenteMateria VALUES (@d5,160),(@d5,161);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d5,1,5),(@d5,1,6),(@d5,1,7),(@d5,1,8),(@d5,1,9),(@d5,1,10),(@d5,1,11),(@d5,1,12),
(@d5,2,5),(@d5,2,6),(@d5,2,7),(@d5,2,8),(@d5,2,9),(@d5,2,10),(@d5,2,11),(@d5,2,12),
(@d5,3,5),(@d5,3,6),(@d5,3,7),(@d5,3,8),(@d5,3,9),(@d5,3,10),(@d5,3,11),(@d5,3,12),
(@d5,4,5),(@d5,4,6),(@d5,4,7),(@d5,4,8),(@d5,4,9),(@d5,4,10),(@d5,4,11),(@d5,4,12),
(@d5,5,5),(@d5,5,6),(@d5,5,7),(@d5,5,8),(@d5,5,9),(@d5,5,10),(@d5,5,11),(@d5,5,12);
GO

-- ?? 6. MT. José Ildefonso Espinosa Pacho ?????????????????????
-- MT · 20h · Lun-Vie bloques 1-4 (4h×5=20h)
-- Principal: Sistemas | Secundaria: Civil, Industrial (cálculo)
-- Materias Sist: Calc.Dif(1), Calc.Int(7), Calc.Vec(13), Ec.Dif(19)
-- Materias Civil: Calc.Dif(255), Calc.Int(265), Calc.Vec(268)
-- Materias Ind: Calc.Dif(156), Calc.Int(162)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('MT. José Ildefonso Espinosa Pacho', 'MEDIO TIEMPO', 1, 20);
DECLARE @d6 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d6, 1, 1);
INSERT INTO DocenteCarrera VALUES (@d6, 2, 0);
INSERT INTO DocenteCarrera VALUES (@d6, 5, 0);
INSERT INTO DocenteMateria VALUES (@d6,1),(@d6,7),(@d6,13),(@d6,19);
INSERT INTO DocenteMateria VALUES (@d6,255),(@d6,265),(@d6,268);
INSERT INTO DocenteMateria VALUES (@d6,156),(@d6,162);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d6,1,1),(@d6,1,2),(@d6,1,3),(@d6,1,4),
(@d6,2,1),(@d6,2,2),(@d6,2,3),(@d6,2,4),
(@d6,3,1),(@d6,3,2),(@d6,3,3),(@d6,3,4),
(@d6,4,1),(@d6,4,2),(@d6,4,3),(@d6,4,4),
(@d6,5,1),(@d6,5,2),(@d6,5,3),(@d6,5,4);
GO

-- ?? 7. Ing. Jorge Angel Santamaria Magaña ????????????????????
-- MT · 16h · Mar-Vie bloques 3-6 (Lun libre, 4h×4=16h)
-- Principal: Sistemas | Sin secundaria
-- Materias: Conmutacion(38), Fund.Telecom(26), SistOp(27), TallerSistOp(33)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('Ing. Jorge Angel Santamaria Magaña', 'MEDIO TIEMPO', 1, 16);
DECLARE @d7 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d7, 1, 1);
INSERT INTO DocenteMateria VALUES (@d7,38),(@d7,26),(@d7,27),(@d7,33);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d7,2,3),(@d7,2,4),(@d7,2,5),(@d7,2,6),
(@d7,3,3),(@d7,3,4),(@d7,3,5),(@d7,3,6),
(@d7,4,3),(@d7,4,4),(@d7,4,5),(@d7,4,6),
(@d7,5,3),(@d7,5,4),(@d7,5,5),(@d7,5,6);
GO

-- ============================================================
-- ?? INGENIERÍA INDUSTRIAL (id_carrera = 5)
-- ============================================================

-- ?? 8. Mtro. Ezer Uc Colli ???????????????????????????????????
-- TC · 40h · Lun-Vie bloques 1-8 (8h×5=40h)
-- Principal: Industrial | Secundaria: Gestión (administración/proyectos)
-- Materias Ind: Admin.Proy(179), Plan.Financiera(194), Ing.Econ(187),
--   FormEval(200), Rel.Industriales(201)
-- Materias Gest: Ing.Economica(223), Form.Eval.Proy(229)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('Mtro. Ezer Uc Colli', 'TIEMPO COMPLETO', 1, 40);
DECLARE @d8 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d8, 5, 1);
INSERT INTO DocenteCarrera VALUES (@d8, 4, 0);
INSERT INTO DocenteMateria VALUES (@d8,179),(@d8,194),(@d8,187),(@d8,200),(@d8,201);
INSERT INTO DocenteMateria VALUES (@d8,223),(@d8,229);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d8,1,1),(@d8,1,2),(@d8,1,3),(@d8,1,4),(@d8,1,5),(@d8,1,6),(@d8,1,7),(@d8,1,8),
(@d8,2,1),(@d8,2,2),(@d8,2,3),(@d8,2,4),(@d8,2,5),(@d8,2,6),(@d8,2,7),(@d8,2,8),
(@d8,3,1),(@d8,3,2),(@d8,3,3),(@d8,3,4),(@d8,3,5),(@d8,3,6),(@d8,3,7),(@d8,3,8),
(@d8,4,1),(@d8,4,2),(@d8,4,3),(@d8,4,4),(@d8,4,5),(@d8,4,6),(@d8,4,7),(@d8,4,8),
(@d8,5,1),(@d8,5,2),(@d8,5,3),(@d8,5,4),(@d8,5,5),(@d8,5,6),(@d8,5,7),(@d8,5,8);
GO

-- ?? 9. Ing. Jesús Caamal Chan ????????????????????????????????
-- TC · 36h · Lun-Jue 8h + Vie 4h (bloques 1-4)
-- Principal: Industrial | Sin secundaria
-- Materias: EstTrab1(171), EstTrab2(177), Ergonomia(184),
--   Higiene(178), Manuf.Esbelta(203), ManufAsis(202)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('Ing. Jesús Caamal Chan', 'TIEMPO COMPLETO', 1, 36);
DECLARE @d9 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d9, 5, 1);
INSERT INTO DocenteMateria VALUES (@d9,171),(@d9,177),(@d9,184),(@d9,178),(@d9,203),(@d9,202);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d9,1,1),(@d9,1,2),(@d9,1,3),(@d9,1,4),(@d9,1,5),(@d9,1,6),(@d9,1,7),(@d9,1,8),
(@d9,2,1),(@d9,2,2),(@d9,2,3),(@d9,2,4),(@d9,2,5),(@d9,2,6),(@d9,2,7),(@d9,2,8),
(@d9,3,1),(@d9,3,2),(@d9,3,3),(@d9,3,4),(@d9,3,5),(@d9,3,6),(@d9,3,7),(@d9,3,8),
(@d9,4,1),(@d9,4,2),(@d9,4,3),(@d9,4,4),(@d9,4,5),(@d9,4,6),(@d9,4,7),(@d9,4,8),
(@d9,5,1),(@d9,5,2),(@d9,5,3),(@d9,5,4);
GO

-- ?? 10. MAO. Raúl Eduardo Tzab Campo ?????????????????????????
-- TC · 40h · Lun-Vie bloques 5-12 (vespertino, 8h×5=40h)
-- Principal: Industrial | Secundaria: Gestión (operaciones/producción)
-- Materias Ind: Admin.Op1(181), Admin.Op2(188), Rel.Ind(201), Topicos(204)
-- Materias Gest: Gest.Prod1(237), Gest.Prod2(243)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('MAO. Raúl Eduardo Tzab Campo', 'TIEMPO COMPLETO', 1, 40);
DECLARE @d10 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d10, 5, 1);
INSERT INTO DocenteCarrera VALUES (@d10, 4, 0);
INSERT INTO DocenteMateria VALUES (@d10,181),(@d10,188),(@d10,201),(@d10,204);
INSERT INTO DocenteMateria VALUES (@d10,237),(@d10,243);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d10,1,5),(@d10,1,6),(@d10,1,7),(@d10,1,8),(@d10,1,9),(@d10,1,10),(@d10,1,11),(@d10,1,12),
(@d10,2,5),(@d10,2,6),(@d10,2,7),(@d10,2,8),(@d10,2,9),(@d10,2,10),(@d10,2,11),(@d10,2,12),
(@d10,3,5),(@d10,3,6),(@d10,3,7),(@d10,3,8),(@d10,3,9),(@d10,3,10),(@d10,3,11),(@d10,3,12),
(@d10,4,5),(@d10,4,6),(@d10,4,7),(@d10,4,8),(@d10,4,9),(@d10,4,10),(@d10,4,11),(@d10,4,12),
(@d10,5,5),(@d10,5,6),(@d10,5,7),(@d10,5,8),(@d10,5,9),(@d10,5,10),(@d10,5,11),(@d10,5,12);
GO

-- ?? 11. MEC. Felipe de Jesús Cool Chi ????????????????????????
-- MT · 20h · Lun-Vie bloques 1-4 (4h×5=20h)
-- Principal: Industrial | Secundaria: Civil, Bioquímica (cálculo/física)
-- Materias Ind: Calc.Dif(156), Calc.Int(162), Calc.Vec(168), Ec.Dif(167)
-- Materias Civil: Calc.Dif(255), Calc.Int(265)
-- Materias Bioq: Calc.Dif(105), Calc.Int(111)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('MEC. Felipe de Jesús Cool Chi', 'MEDIO TIEMPO', 1, 20);
DECLARE @d11 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d11, 5, 1);
INSERT INTO DocenteCarrera VALUES (@d11, 2, 0);
INSERT INTO DocenteCarrera VALUES (@d11, 6, 0);
INSERT INTO DocenteMateria VALUES (@d11,156),(@d11,162),(@d11,168),(@d11,167);
INSERT INTO DocenteMateria VALUES (@d11,255),(@d11,265);
INSERT INTO DocenteMateria VALUES (@d11,105),(@d11,111);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d11,1,1),(@d11,1,2),(@d11,1,3),(@d11,1,4),
(@d11,2,1),(@d11,2,2),(@d11,2,3),(@d11,2,4),
(@d11,3,1),(@d11,3,2),(@d11,3,3),(@d11,3,4),
(@d11,4,1),(@d11,4,2),(@d11,4,3),(@d11,4,4),
(@d11,5,1),(@d11,5,2),(@d11,5,3),(@d11,5,4);
GO

-- ?? 12. MAO. Rangel Antonio Navarrete Canté ??????????????????
-- MT · 16h · Lun-Jue bloques 5-8 (Vie libre, 4h×4=16h)
-- Principal: Industrial | Secundaria: Gestión (logística)
-- Materias Ind: Gestion.Costos(180), Logistica(197), Plan.Dis.Inst(195)
-- Materias Gest: Cadena.Sum(249), Admin.Salud(235)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('MAO. Rangel Antonio Navarrete Canté', 'MEDIO TIEMPO', 1, 16);
DECLARE @d12 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d12, 5, 1);
INSERT INTO DocenteCarrera VALUES (@d12, 4, 0);
INSERT INTO DocenteMateria VALUES (@d12,180),(@d12,197),(@d12,195);
INSERT INTO DocenteMateria VALUES (@d12,249),(@d12,235);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d12,1,5),(@d12,1,6),(@d12,1,7),(@d12,1,8),
(@d12,2,5),(@d12,2,6),(@d12,2,7),(@d12,2,8),
(@d12,3,5),(@d12,3,6),(@d12,3,7),(@d12,3,8),
(@d12,4,5),(@d12,4,6),(@d12,4,7),(@d12,4,8);
GO

-- ============================================================
-- ?? GESTIÓN EMPRESARIAL (id_carrera = 4)
-- ============================================================

-- ?? 13. M.A. Wilbert Manuel Góngora Basto ????????????????????
-- TC · 40h · Lun-Vie bloques 1-8 (8h×5=40h)
-- Principal: Gestión | Secundaria: Comunitario (administración rural)
-- Materias Gest: Gest.Estrat(244), Plan.Neg(242), Taller.Etica(215),
--   Algebra.Lineal(222), Habl.Dir1(220), Habl.Dir2(226)
-- Materias Com: Fund.Admin(72), Plan.Nuevas.Emp(98)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('M.A. Wilbert Manuel Góngora Basto', 'TIEMPO COMPLETO', 1, 40);
DECLARE @d13 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d13, 4, 1);
INSERT INTO DocenteCarrera VALUES (@d13, 3, 0);
INSERT INTO DocenteMateria VALUES (@d13,244),(@d13,242),(@d13,215),(@d13,222),(@d13,220),(@d13,226);
INSERT INTO DocenteMateria VALUES (@d13,72),(@d13,98);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d13,1,1),(@d13,1,2),(@d13,1,3),(@d13,1,4),(@d13,1,5),(@d13,1,6),(@d13,1,7),(@d13,1,8),
(@d13,2,1),(@d13,2,2),(@d13,2,3),(@d13,2,4),(@d13,2,5),(@d13,2,6),(@d13,2,7),(@d13,2,8),
(@d13,3,1),(@d13,3,2),(@d13,3,3),(@d13,3,4),(@d13,3,5),(@d13,3,6),(@d13,3,7),(@d13,3,8),
(@d13,4,1),(@d13,4,2),(@d13,4,3),(@d13,4,4),(@d13,4,5),(@d13,4,6),(@d13,4,7),(@d13,4,8),
(@d13,5,1),(@d13,5,2),(@d13,5,3),(@d13,5,4),(@d13,5,5),(@d13,5,6),(@d13,5,7),(@d13,5,8);
GO

-- ?? 14. L.P. Irlanda Beatriz Varguez Canul ???????????????????
-- TC · 32h · Mar-Vie bloques 1-8 (Lun libre, 8h×4=32h)
-- Principal: Gestión | Sin secundaria
-- Materias: Mercado.Elec(246), Mktg.Serv(250), Com.Mktg(251),
--   Sis.Info.Mktg(240), Mercadotecnia(234), El.Emprend(236)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('L.P. Irlanda Beatriz Varguez Canul', 'TIEMPO COMPLETO', 1, 32);
DECLARE @d14 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d14, 4, 1);
INSERT INTO DocenteMateria VALUES (@d14,246),(@d14,250),(@d14,251),(@d14,240),(@d14,234),(@d14,236);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d14,2,1),(@d14,2,2),(@d14,2,3),(@d14,2,4),(@d14,2,5),(@d14,2,6),(@d14,2,7),(@d14,2,8),
(@d14,3,1),(@d14,3,2),(@d14,3,3),(@d14,3,4),(@d14,3,5),(@d14,3,6),(@d14,3,7),(@d14,3,8),
(@d14,4,1),(@d14,4,2),(@d14,4,3),(@d14,4,4),(@d14,4,5),(@d14,4,6),(@d14,4,7),(@d14,4,8),
(@d14,5,1),(@d14,5,2),(@d14,5,3),(@d14,5,4),(@d14,5,5),(@d14,5,6),(@d14,5,7),(@d14,5,8);
GO

-- ?? 15. M.P. Mariela Guadalupe Buenfil Tenreiro ??????????????
-- TC · 40h · Lun-Vie bloques 3-10 (intermedio-vespertino, 8h×5=40h)
-- Principal: Gestión | Sin secundaria
-- Materias: Gest.Destinos(247), Oper.Serv(248), Des.Humano(207),
--   Fund.Quimica(210), Fund.Gestion(208), Dinamica.Social(214)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('M.P. Mariela Guadalupe Buenfil Tenreiro', 'TIEMPO COMPLETO', 1, 40);
DECLARE @d15 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d15, 4, 1);
INSERT INTO DocenteMateria VALUES (@d15,247),(@d15,248),(@d15,207),(@d15,210),(@d15,208),(@d15,214);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d15,1,3),(@d15,1,4),(@d15,1,5),(@d15,1,6),(@d15,1,7),(@d15,1,8),(@d15,1,9),(@d15,1,10),
(@d15,2,3),(@d15,2,4),(@d15,2,5),(@d15,2,6),(@d15,2,7),(@d15,2,8),(@d15,2,9),(@d15,2,10),
(@d15,3,3),(@d15,3,4),(@d15,3,5),(@d15,3,6),(@d15,3,7),(@d15,3,8),(@d15,3,9),(@d15,3,10),
(@d15,4,3),(@d15,4,4),(@d15,4,5),(@d15,4,6),(@d15,4,7),(@d15,4,8),(@d15,4,9),(@d15,4,10),
(@d15,5,3),(@d15,5,4),(@d15,5,5),(@d15,5,6),(@d15,5,7),(@d15,5,8),(@d15,5,9),(@d15,5,10);
GO

-- ?? 16. MNI. Porfirio Arturo Santamaría Fuentes ??????????????
-- TC · 40h · Lun-Vie bloques 1-8 (8h×5=40h)
-- Principal: Gestión | Secundaria: Sistemas (informática/TICs)
-- Materias Gest: Fund.Invest(205), Calc.Dif(206), Fund.Gest(208),
--   Software.Apl(211), Des.Sust(245), Taller.Invest1(233)
-- Materias Sist: Taller.Admin(5), Cultura.Empr(15)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('MNI. Porfirio Arturo Santamaría Fuentes', 'TIEMPO COMPLETO', 1, 40);
DECLARE @d16 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d16, 4, 1);
INSERT INTO DocenteCarrera VALUES (@d16, 1, 0);
INSERT INTO DocenteMateria VALUES (@d16,205),(@d16,206),(@d16,208),(@d16,211),(@d16,245),(@d16,233);
INSERT INTO DocenteMateria VALUES (@d16,5),(@d16,15);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d16,1,1),(@d16,1,2),(@d16,1,3),(@d16,1,4),(@d16,1,5),(@d16,1,6),(@d16,1,7),(@d16,1,8),
(@d16,2,1),(@d16,2,2),(@d16,2,3),(@d16,2,4),(@d16,2,5),(@d16,2,6),(@d16,2,7),(@d16,2,8),
(@d16,3,1),(@d16,3,2),(@d16,3,3),(@d16,3,4),(@d16,3,5),(@d16,3,6),(@d16,3,7),(@d16,3,8),
(@d16,4,1),(@d16,4,2),(@d16,4,3),(@d16,4,4),(@d16,4,5),(@d16,4,6),(@d16,4,7),(@d16,4,8),
(@d16,5,1),(@d16,5,2),(@d16,5,3),(@d16,5,4),(@d16,5,5),(@d16,5,6),(@d16,5,7),(@d16,5,8);
GO

-- ?? 17. M.M. René Aarón González Canché ??????????????????????
-- TC · 36h · Lun-Jue bloques 5-12 + Vie 5-8 (32+4=36h)
-- Principal: Gestión | Sin secundaria
-- Materias: Gest.Prod1(237), Gest.Prod2(243), Dis.Org(238),
--   Calidad.Gest(241), Taller.Invest2(239), Plan.Neg(242)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('M.M. René Aarón González Canché', 'TIEMPO COMPLETO', 1, 36);
DECLARE @d17 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d17, 4, 1);
INSERT INTO DocenteMateria VALUES (@d17,237),(@d17,243),(@d17,238),(@d17,241),(@d17,239),(@d17,242);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d17,1,5),(@d17,1,6),(@d17,1,7),(@d17,1,8),(@d17,1,9),(@d17,1,10),(@d17,1,11),(@d17,1,12),
(@d17,2,5),(@d17,2,6),(@d17,2,7),(@d17,2,8),(@d17,2,9),(@d17,2,10),(@d17,2,11),(@d17,2,12),
(@d17,3,5),(@d17,3,6),(@d17,3,7),(@d17,3,8),(@d17,3,9),(@d17,3,10),(@d17,3,11),(@d17,3,12),
(@d17,4,5),(@d17,4,6),(@d17,4,7),(@d17,4,8),(@d17,4,9),(@d17,4,10),(@d17,4,11),(@d17,4,12),
(@d17,5,5),(@d17,5,6),(@d17,5,7),(@d17,5,8);
GO

-- ?? 18. LAE. Nino Antonio Pacheco Escobedo ???????????????????
-- TC · 40h · Lun-Vie bloques 5-12 (8h×5=40h)
-- Principal: Gestión | Sin secundaria
-- Materias: Contab(213), Costos(219), Legislacion(216),
--   Marco.Legal(217), Prob.Est(218), Finanzas(229)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('LAE. Nino Antonio Pacheco Escobedo', 'TIEMPO COMPLETO', 1, 40);
DECLARE @d18 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d18, 4, 1);
INSERT INTO DocenteMateria VALUES (@d18,213),(@d18,219),(@d18,216),(@d18,217),(@d18,218),(@d18,229);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d18,1,5),(@d18,1,6),(@d18,1,7),(@d18,1,8),(@d18,1,9),(@d18,1,10),(@d18,1,11),(@d18,1,12),
(@d18,2,5),(@d18,2,6),(@d18,2,7),(@d18,2,8),(@d18,2,9),(@d18,2,10),(@d18,2,11),(@d18,2,12),
(@d18,3,5),(@d18,3,6),(@d18,3,7),(@d18,3,8),(@d18,3,9),(@d18,3,10),(@d18,3,11),(@d18,3,12),
(@d18,4,5),(@d18,4,6),(@d18,4,7),(@d18,4,8),(@d18,4,9),(@d18,4,10),(@d18,4,11),(@d18,4,12),
(@d18,5,5),(@d18,5,6),(@d18,5,7),(@d18,5,8),(@d18,5,9),(@d18,5,10),(@d18,5,11),(@d18,5,12);
GO

-- ?? 19. Dr. David Israel Celis Euán ??????????????????????????
-- TC · 32h · Lun-Jue bloques 1-8 (Vie libre, 8h×4=32h)
-- Principal: Gestión | Sin secundaria
-- Materias: Invest.Op(228), Est.Inf1(224), Est.Inf2(230),
--   Prob.Est(218), Ing.Proc(231), Calc.Int(212)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('Dr. David Israel Celis Euán', 'TIEMPO COMPLETO', 1, 32);
DECLARE @d19 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d19, 4, 1);
INSERT INTO DocenteMateria VALUES (@d19,228),(@d19,224),(@d19,230),(@d19,218),(@d19,231),(@d19,212);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d19,1,1),(@d19,1,2),(@d19,1,3),(@d19,1,4),(@d19,1,5),(@d19,1,6),(@d19,1,7),(@d19,1,8),
(@d19,2,1),(@d19,2,2),(@d19,2,3),(@d19,2,4),(@d19,2,5),(@d19,2,6),(@d19,2,7),(@d19,2,8),
(@d19,3,1),(@d19,3,2),(@d19,3,3),(@d19,3,4),(@d19,3,5),(@d19,3,6),(@d19,3,7),(@d19,3,8),
(@d19,4,1),(@d19,4,2),(@d19,4,3),(@d19,4,4),(@d19,4,5),(@d19,4,6),(@d19,4,7),(@d19,4,8);
GO

-- ?? 20. MAN. Tatiana Navarrete Castillo ??????????????????????
-- MT · 20h · Lun-Vie bloques 1-4 (4h×5=20h)
-- Principal: Gestión | Sin secundaria
-- Materias: Rec.Hum(232), Ent.Macroecon(227), Cap.Humano(221)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('MAN. Tatiana Navarrete Castillo', 'MEDIO TIEMPO', 1, 20);
DECLARE @d20 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d20, 4, 1);
INSERT INTO DocenteMateria VALUES (@d20,232),(@d20,227),(@d20,221);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d20,1,1),(@d20,1,2),(@d20,1,3),(@d20,1,4),
(@d20,2,1),(@d20,2,2),(@d20,2,3),(@d20,2,4),
(@d20,3,1),(@d20,3,2),(@d20,3,3),(@d20,3,4),
(@d20,4,1),(@d20,4,2),(@d20,4,3),(@d20,4,4),
(@d20,5,1),(@d20,5,2),(@d20,5,3),(@d20,5,4);
GO

-- ?? 21. MAN. Óscar Alberto Estrella Sosa ?????????????????????
-- MT · 16h · Lun-Jue bloques 7-10 (Vie libre, 4h×4=16h)
-- Principal: Gestión | Sin secundaria
-- Materias: Des.Hum(207), Dinam.Social(214), Habl.Dir1(220), Habl.Dir2(226)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('MAN. Óscar Alberto Estrella Sosa', 'MEDIO TIEMPO', 1, 16);
DECLARE @d21 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d21, 4, 1);
INSERT INTO DocenteMateria VALUES (@d21,207),(@d21,214),(@d21,220),(@d21,226);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d21,1,7),(@d21,1,8),(@d21,1,9),(@d21,1,10),
(@d21,2,7),(@d21,2,8),(@d21,2,9),(@d21,2,10),
(@d21,3,7),(@d21,3,8),(@d21,3,9),(@d21,3,10),
(@d21,4,7),(@d21,4,8),(@d21,4,9),(@d21,4,10);
GO

-- ?? 22. Dra. Jacqueline Zapata Vazquez ???????????????????????
-- MT · 18h · Lun-Mié 6h (bloques 5-10) + Jue-Vie 3h (bloques 5-7)
-- Principal: Gestión | Sin secundaria
-- Materias: Marco.Legal(217), Ing.Econ(223), Inst.Presup(225)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('Dra. Jacqueline Zapata Vazquez', 'MEDIO TIEMPO', 1, 18);
DECLARE @d22 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d22, 4, 1);
INSERT INTO DocenteMateria VALUES (@d22,217),(@d22,223),(@d22,225);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d22,1,5),(@d22,1,6),(@d22,1,7),(@d22,1,8),(@d22,1,9),(@d22,1,10),
(@d22,2,5),(@d22,2,6),(@d22,2,7),(@d22,2,8),(@d22,2,9),(@d22,2,10),
(@d22,3,5),(@d22,3,6),(@d22,3,7),(@d22,3,8),(@d22,3,9),(@d22,3,10),
(@d22,4,5),(@d22,4,6),(@d22,4,7),
(@d22,5,5),(@d22,5,6),(@d22,5,7);
GO

-- ?? 23. LA. Iris Rubí Pech Ceballos ??????????????????????????
-- MT · 16h · Mar-Vie bloques 7-10 (Lun libre, 4h×4=16h)
-- Principal: Gestión | Sin secundaria
-- Materias: Negocios.Int(253), Pymes(252), Ent.Macroecon(227)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('LA. Iris Rubí Pech Ceballos', 'MEDIO TIEMPO', 1, 16);
DECLARE @d23 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d23, 4, 1);
INSERT INTO DocenteMateria VALUES (@d23,253),(@d23,252),(@d23,227);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d23,2,7),(@d23,2,8),(@d23,2,9),(@d23,2,10),
(@d23,3,7),(@d23,3,8),(@d23,3,9),(@d23,3,10),
(@d23,4,7),(@d23,4,8),(@d23,4,9),(@d23,4,10),
(@d23,5,7),(@d23,5,8),(@d23,5,9),(@d23,5,10);
GO

-- ?? 24. LAE. Roger Hernández Yam ?????????????????????????????
-- MT · 20h · Lun-Vie bloques 9-12 (4h×5=20h)
-- Principal: Gestión | Sin secundaria
-- Materias: Inst.Presup(225), Fin.Org(229), Contab(213), Admin.Salud(235)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('LAE. Roger Hernández Yam', 'MEDIO TIEMPO', 1, 20);
DECLARE @d24 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d24, 4, 1);
INSERT INTO DocenteMateria VALUES (@d24,225),(@d24,229),(@d24,213),(@d24,235);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d24,1,9),(@d24,1,10),(@d24,1,11),(@d24,1,12),
(@d24,2,9),(@d24,2,10),(@d24,2,11),(@d24,2,12),
(@d24,3,9),(@d24,3,10),(@d24,3,11),(@d24,3,12),
(@d24,4,9),(@d24,4,10),(@d24,4,11),(@d24,4,12),
(@d24,5,9),(@d24,5,10),(@d24,5,11),(@d24,5,12);
GO

-- ?? 25. Mtra. Yuliana del Rosario Parra Vazquez ??????????????
-- MT · 18h · Lun-Mié 4h (3-6) + Jue-Vie 5h (3-7) = 12+10? ? 4×3+5×? NO
-- Corrección: Lun-Mié bloques 3-6 (4h×3=12) + Jue bloques 3-6 (4h) + Vie bloques 3-4 (2h) = 18h
-- Principal: Gestión | Sin secundaria
-- Materias: Des.Sust(245), Mercadotecnia(234), El.Emprendedor(236)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('Mtra. Yuliana del Rosario Parra Vazquez', 'MEDIO TIEMPO', 1, 18);
DECLARE @d25 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d25, 4, 1);
INSERT INTO DocenteMateria VALUES (@d25,245),(@d25,234),(@d25,236);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d25,1,3),(@d25,1,4),(@d25,1,5),(@d25,1,6),
(@d25,2,3),(@d25,2,4),(@d25,2,5),(@d25,2,6),
(@d25,3,3),(@d25,3,4),(@d25,3,5),(@d25,3,6),
(@d25,4,3),(@d25,4,4),(@d25,4,5),(@d25,4,6),
(@d25,5,3),(@d25,5,4);
GO

-- ============================================================
-- ?? INGENIERÍA BIOQUÍMICA (id_carrera = 6)
-- ============================================================

-- ?? 26. MNI. Selene Magdalena Suárez Baeza ???????????????????
-- TC · 40h · Lun-Vie bloques 1-8 (8h×5=40h)
-- Principal: Bioquímica | Secundaria: Industrial (informática/control)
-- Materias Bioq: Prog.Met.Num(122), Dibujo.CAD(109), Instrum.Control(133),
--   Ing.Proyectos(146), Seg.Higiene(137)
-- Materias Ind: SistMan(196), DisSAC(192)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('MNI. Selene Magdalena Suárez Baeza', 'TIEMPO COMPLETO', 1, 40);
DECLARE @d26 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d26, 6, 1);
INSERT INTO DocenteCarrera VALUES (@d26, 5, 0);
INSERT INTO DocenteMateria VALUES (@d26,122),(@d26,109),(@d26,133),(@d26,146),(@d26,137);
INSERT INTO DocenteMateria VALUES (@d26,196),(@d26,192);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d26,1,1),(@d26,1,2),(@d26,1,3),(@d26,1,4),(@d26,1,5),(@d26,1,6),(@d26,1,7),(@d26,1,8),
(@d26,2,1),(@d26,2,2),(@d26,2,3),(@d26,2,4),(@d26,2,5),(@d26,2,6),(@d26,2,7),(@d26,2,8),
(@d26,3,1),(@d26,3,2),(@d26,3,3),(@d26,3,4),(@d26,3,5),(@d26,3,6),(@d26,3,7),(@d26,3,8),
(@d26,4,1),(@d26,4,2),(@d26,4,3),(@d26,4,4),(@d26,4,5),(@d26,4,6),(@d26,4,7),(@d26,4,8),
(@d26,5,1),(@d26,5,2),(@d26,5,3),(@d26,5,4),(@d26,5,5),(@d26,5,6),(@d26,5,7),(@d26,5,8);
GO

-- ?? 27. Ing. Pedro Rogelio Catzím Navarrete ??????????????????
-- TC · 36h · Lun-Vie bloques 1-8, Mié solo 1-4 (8+8+4+8+8=36h)
-- Principal: Bioquímica | Sin secundaria
-- Materias: Op.Unit1(134), Op.Unit2(141), Op.Unit3(142),
--   Ing.Procesos(148), Form.Eval(152)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('Ing. Pedro Rogelio Catzím Navarrete', 'TIEMPO COMPLETO', 1, 36);
DECLARE @d27 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d27, 6, 1);
INSERT INTO DocenteMateria VALUES (@d27,134),(@d27,141),(@d27,142),(@d27,148),(@d27,152);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d27,1,1),(@d27,1,2),(@d27,1,3),(@d27,1,4),(@d27,1,5),(@d27,1,6),(@d27,1,7),(@d27,1,8),
(@d27,2,1),(@d27,2,2),(@d27,2,3),(@d27,2,4),(@d27,2,5),(@d27,2,6),(@d27,2,7),(@d27,2,8),
(@d27,3,1),(@d27,3,2),(@d27,3,3),(@d27,3,4),
(@d27,4,1),(@d27,4,2),(@d27,4,3),(@d27,4,4),(@d27,4,5),(@d27,4,6),(@d27,4,7),(@d27,4,8),
(@d27,5,1),(@d27,5,2),(@d27,5,3),(@d27,5,4),(@d27,5,5),(@d27,5,6),(@d27,5,7),(@d27,5,8);
GO

-- ?? 28. Dr. Ángel Virgilio Domínguez May ?????????????????????
-- TC · 40h · Lun-Vie bloques 3-10 (8h×5=40h)
-- Principal: Bioquímica | Secundaria: Comunitario (biología/micro)
-- Materias Bioq: Bioquimica(124), Bioq.Nitrog(130), Microb(136), MicrobInd(153)
-- Materias Com: Bioquimica(68), Microbiologia(69)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('Dr. Ángel Virgilio Domínguez May', 'TIEMPO COMPLETO', 1, 40);
DECLARE @d28 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d28, 6, 1);
INSERT INTO DocenteCarrera VALUES (@d28, 3, 0);
INSERT INTO DocenteMateria VALUES (@d28,124),(@d28,130),(@d28,136),(@d28,153);
INSERT INTO DocenteMateria VALUES (@d28,68),(@d28,69);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d28,1,3),(@d28,1,4),(@d28,1,5),(@d28,1,6),(@d28,1,7),(@d28,1,8),(@d28,1,9),(@d28,1,10),
(@d28,2,3),(@d28,2,4),(@d28,2,5),(@d28,2,6),(@d28,2,7),(@d28,2,8),(@d28,2,9),(@d28,2,10),
(@d28,3,3),(@d28,3,4),(@d28,3,5),(@d28,3,6),(@d28,3,7),(@d28,3,8),(@d28,3,9),(@d28,3,10),
(@d28,4,3),(@d28,4,4),(@d28,4,5),(@d28,4,6),(@d28,4,7),(@d28,4,8),(@d28,4,9),(@d28,4,10),
(@d28,5,3),(@d28,5,4),(@d28,5,5),(@d28,5,6),(@d28,5,7),(@d28,5,8),(@d28,5,9),(@d28,5,10);
GO

-- ?? 29. Ing. Ricardo Yam Ucan ????????????????????????????????
-- MT · 20h · Lun-Vie bloques 1-4 (4h×5=20h)
-- Principal: Bioquímica | Secundaria: Comunitario (química)
-- Materias Bioq: Quim(106), QuimOrg1(112), QuimOrg2(118), QuimAnal(114)
-- Materias Com: Quimica(63)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('Ing. Ricardo Yam Ucan', 'MEDIO TIEMPO', 1, 20);
DECLARE @d29 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d29, 6, 1);
INSERT INTO DocenteCarrera VALUES (@d29, 3, 0);
INSERT INTO DocenteMateria VALUES (@d29,106),(@d29,112),(@d29,118),(@d29,114);
INSERT INTO DocenteMateria VALUES (@d29,63);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d29,1,1),(@d29,1,2),(@d29,1,3),(@d29,1,4),
(@d29,2,1),(@d29,2,2),(@d29,2,3),(@d29,2,4),
(@d29,3,1),(@d29,3,2),(@d29,3,3),(@d29,3,4),
(@d29,4,1),(@d29,4,2),(@d29,4,3),(@d29,4,4),
(@d29,5,1),(@d29,5,2),(@d29,5,3),(@d29,5,4);
GO

-- ?? 30. M.C. Miriam Noemí Burgos Jiménez ?????????????????????
-- MT · 16h · Lun-Jue bloques 3-6 (Vie libre, 4h×4=16h)
-- Principal: Bioquímica | Sin secundaria
-- Materias: Fenomenos.Trans1(129), Fenomenos.Trans2(135), Termodi(119), Fisicoquim(131)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('M.C. Miriam Noemí Burgos Jiménez', 'MEDIO TIEMPO', 1, 16);
DECLARE @d30 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d30, 6, 1);
INSERT INTO DocenteMateria VALUES (@d30,129),(@d30,135),(@d30,119),(@d30,131);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d30,1,3),(@d30,1,4),(@d30,1,5),(@d30,1,6),
(@d30,2,3),(@d30,2,4),(@d30,2,5),(@d30,2,6),
(@d30,3,3),(@d30,3,4),(@d30,3,5),(@d30,3,6),
(@d30,4,3),(@d30,4,4),(@d30,4,5),(@d30,4,6);
GO

-- ?? 31. M.C. Jacinto Alberto Loeza Peraza ????????????????????
-- MT · 18h · Lun 3h(7-9) + Mar-Jue 6h(5-10) + Vie 3h(7-9) = 3+6+6+6+3? NO ? 3+18+3=24? NO
-- Corrección: Mar-Jue 4h(5-8) + Lun,Vie 3h(7-9) = 12+6=18h
-- Principal: Bioquímica | Sin secundaria
-- Materias: Balance.Mat(125), Cinetica(138), Biorreactores(143)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('M.C. Jacinto Alberto Loeza Peraza', 'MEDIO TIEMPO', 1, 18);
DECLARE @d31 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d31, 6, 1);
INSERT INTO DocenteMateria VALUES (@d31,125),(@d31,138),(@d31,143);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d31,1,7),(@d31,1,8),(@d31,1,9),
(@d31,2,5),(@d31,2,6),(@d31,2,7),(@d31,2,8),
(@d31,3,5),(@d31,3,6),(@d31,3,7),(@d31,3,8),
(@d31,4,5),(@d31,4,6),(@d31,4,7),(@d31,4,8),
(@d31,5,7),(@d31,5,8),(@d31,5,9);
GO

-- ?? 32. MITA. Manuel Jesús Colli Us ??????????????????????????
-- MT · 16h · Lun-Jue bloques 7-10 (Vie libre, 4h×4=16h)
-- Principal: Bioquímica | Sin secundaria
-- Materias: Ing.Gest.Amb(147), Des.Sust(132), Calidad.Inocuidad(151)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('MITA. Manuel Jesús Colli Us', 'MEDIO TIEMPO', 1, 16);
DECLARE @d32 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d32, 6, 1);
INSERT INTO DocenteMateria VALUES (@d32,147),(@d32,132),(@d32,151);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d32,1,7),(@d32,1,8),(@d32,1,9),(@d32,1,10),
(@d32,2,7),(@d32,2,8),(@d32,2,9),(@d32,2,10),
(@d32,3,7),(@d32,3,8),(@d32,3,9),(@d32,3,10),
(@d32,4,7),(@d32,4,8),(@d32,4,9),(@d32,4,10);
GO

-- ?? 33. Mtro. Filogonio Chan López ???????????????????????????
-- MT · 20h · Lun-Vie bloques 9-12 (4h×5=20h)
-- Principal: Bioquímica | Sin secundaria
-- Materias: Fisicoquim(131), Analisis.Instrum(126), Tecnol.Enzim(150),
--   Fisiologia.Cultivo(149)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('Mtro. Filogonio Chan López', 'MEDIO TIEMPO', 1, 20);
DECLARE @d33 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d33, 6, 1);
INSERT INTO DocenteMateria VALUES (@d33,131),(@d33,126),(@d33,150),(@d33,149);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d33,1,9),(@d33,1,10),(@d33,1,11),(@d33,1,12),
(@d33,2,9),(@d33,2,10),(@d33,2,11),(@d33,2,12),
(@d33,3,9),(@d33,3,10),(@d33,3,11),(@d33,3,12),
(@d33,4,9),(@d33,4,10),(@d33,4,11),(@d33,4,12),
(@d33,5,9),(@d33,5,10),(@d33,5,11),(@d33,5,12);
GO

-- ============================================================
-- ?? INGENIERÍA CIVIL (id_carrera = 2)
-- ============================================================

-- ?? 34. Ing. José Eduardo Uluac May ??????????????????????????
-- TC · 40h · Lun-Vie bloques 1-8 (8h×5=40h)
-- Principal: Civil | Sin secundaria
-- Materias: Mec.Suelos(274), Mec.Suelos.Apl(280), Dis.Ciment(296),
--   Maq.Pesada(275), Topicos.Civil(298), Sup.Control(301)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('Ing. José Eduardo Uluac May', 'TIEMPO COMPLETO', 1, 40);
DECLARE @d34 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d34, 2, 1);
INSERT INTO DocenteMateria VALUES (@d34,274),(@d34,280),(@d34,296),(@d34,275),(@d34,298),(@d34,301);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d34,1,1),(@d34,1,2),(@d34,1,3),(@d34,1,4),(@d34,1,5),(@d34,1,6),(@d34,1,7),(@d34,1,8),
(@d34,2,1),(@d34,2,2),(@d34,2,3),(@d34,2,4),(@d34,2,5),(@d34,2,6),(@d34,2,7),(@d34,2,8),
(@d34,3,1),(@d34,3,2),(@d34,3,3),(@d34,3,4),(@d34,3,5),(@d34,3,6),(@d34,3,7),(@d34,3,8),
(@d34,4,1),(@d34,4,2),(@d34,4,3),(@d34,4,4),(@d34,4,5),(@d34,4,6),(@d34,4,7),(@d34,4,8),
(@d34,5,1),(@d34,5,2),(@d34,5,3),(@d34,5,4),(@d34,5,5),(@d34,5,6),(@d34,5,7),(@d34,5,8);
GO

-- ?? 35. Ing. José Román Yah Mis ??????????????????????????????
-- TC · 32h · Lun-Jue bloques 1-8 (Vie libre, 8h×4=32h)
-- Principal: Civil | Sin secundaria
-- Materias: Analisis.Est(284), An.Est.Av(290), Dis.Concreto(291),
--   Dis.Acero(297), Proy.Ejec(300), Dis.Ciment(296)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('Ing. José Román Yah Mis', 'TIEMPO COMPLETO', 1, 32);
DECLARE @d35 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d35, 2, 1);
INSERT INTO DocenteMateria VALUES (@d35,284),(@d35,290),(@d35,291),(@d35,297),(@d35,300),(@d35,296);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d35,1,1),(@d35,1,2),(@d35,1,3),(@d35,1,4),(@d35,1,5),(@d35,1,6),(@d35,1,7),(@d35,1,8),
(@d35,2,1),(@d35,2,2),(@d35,2,3),(@d35,2,4),(@d35,2,5),(@d35,2,6),(@d35,2,7),(@d35,2,8),
(@d35,3,1),(@d35,3,2),(@d35,3,3),(@d35,3,4),(@d35,3,5),(@d35,3,6),(@d35,3,7),(@d35,3,8),
(@d35,4,1),(@d35,4,2),(@d35,4,3),(@d35,4,4),(@d35,4,5),(@d35,4,6),(@d35,4,7),(@d35,4,8);
GO

-- ?? 36. MM. Lilian Viviana Catzin Navarrete ??????????????????
-- TC · 40h · Lun-Vie bloques 3-10 (8h×5=40h)
-- Principal: Civil | Sin secundaria
-- Materias: Hidraul.Bas(283), Hidraul.Can(288), Hidrologia(282),
--   Abast.Agua(293), Inst.Edif(285), Proy.Mun(289)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('MM. Lilian Viviana Catzin Navarrete', 'TIEMPO COMPLETO', 1, 40);
DECLARE @d36 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d36, 2, 1);
INSERT INTO DocenteMateria VALUES (@d36,283),(@d36,288),(@d36,282),(@d36,293),(@d36,285),(@d36,289);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d36,1,3),(@d36,1,4),(@d36,1,5),(@d36,1,6),(@d36,1,7),(@d36,1,8),(@d36,1,9),(@d36,1,10),
(@d36,2,3),(@d36,2,4),(@d36,2,5),(@d36,2,6),(@d36,2,7),(@d36,2,8),(@d36,2,9),(@d36,2,10),
(@d36,3,3),(@d36,3,4),(@d36,3,5),(@d36,3,6),(@d36,3,7),(@d36,3,8),(@d36,3,9),(@d36,3,10),
(@d36,4,3),(@d36,4,4),(@d36,4,5),(@d36,4,6),(@d36,4,7),(@d36,4,8),(@d36,4,9),(@d36,4,10),
(@d36,5,3),(@d36,5,4),(@d36,5,5),(@d36,5,6),(@d36,5,7),(@d36,5,8),(@d36,5,9),(@d36,5,10);
GO

-- ?? 37. Arq. Grecia Danae Pech Ceballos ??????????????????????
-- TC · 36h · Lun-Jue bloques 5-12 + Vie 5-8 (32+4=36h)
-- Principal: Civil | Sin secundaria
-- Materias: Dibujo.Civil(259), Dis.Urbano(299), Inst.Edif(285),
--   Proy.Ejec(300), Admon.Const(287), Proy.Mun(289)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('Arq. Grecia Danae Pech Ceballos', 'TIEMPO COMPLETO', 1, 36);
DECLARE @d37 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d37, 2, 1);
INSERT INTO DocenteMateria VALUES (@d37,259),(@d37,299),(@d37,285),(@d37,300),(@d37,287),(@d37,289);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d37,1,5),(@d37,1,6),(@d37,1,7),(@d37,1,8),(@d37,1,9),(@d37,1,10),(@d37,1,11),(@d37,1,12),
(@d37,2,5),(@d37,2,6),(@d37,2,7),(@d37,2,8),(@d37,2,9),(@d37,2,10),(@d37,2,11),(@d37,2,12),
(@d37,3,5),(@d37,3,6),(@d37,3,7),(@d37,3,8),(@d37,3,9),(@d37,3,10),(@d37,3,11),(@d37,3,12),
(@d37,4,5),(@d37,4,6),(@d37,4,7),(@d37,4,8),(@d37,4,9),(@d37,4,10),(@d37,4,11),(@d37,4,12),
(@d37,5,5),(@d37,5,6),(@d37,5,7),(@d37,5,8);
GO

-- ?? 38. Ing. Gabriel Manuel Tus Us ???????????????????????????
-- TC · 40h · Lun-Vie bloques 5-12 (8h×5=40h)
-- Principal: Civil | Sin secundaria
-- Materias: Topografia(263), Carreteras(269), Pav(286),
--   Sist.Transp(271), Form.Eval(294)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('Ing. Gabriel Manuel Tus Us', 'TIEMPO COMPLETO', 1, 40);
DECLARE @d38 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d38, 2, 1);
INSERT INTO DocenteMateria VALUES (@d38,263),(@d38,269),(@d38,286),(@d38,271),(@d38,294);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d38,1,5),(@d38,1,6),(@d38,1,7),(@d38,1,8),(@d38,1,9),(@d38,1,10),(@d38,1,11),(@d38,1,12),
(@d38,2,5),(@d38,2,6),(@d38,2,7),(@d38,2,8),(@d38,2,9),(@d38,2,10),(@d38,2,11),(@d38,2,12),
(@d38,3,5),(@d38,3,6),(@d38,3,7),(@d38,3,8),(@d38,3,9),(@d38,3,10),(@d38,3,11),(@d38,3,12),
(@d38,4,5),(@d38,4,6),(@d38,4,7),(@d38,4,8),(@d38,4,9),(@d38,4,10),(@d38,4,11),(@d38,4,12),
(@d38,5,5),(@d38,5,6),(@d38,5,7),(@d38,5,8),(@d38,5,9),(@d38,5,10),(@d38,5,11),(@d38,5,12);
GO

-- ?? 39. MINE. Alvaro José Leal Osorio ????????????????????????
-- TC · 32h · Mar-Vie bloques 1-8 (Lun libre, 8h×4=32h)
-- Principal: Civil | Secundaria: Industrial (mecánica)
-- Materias Civil: Estatica(266), Dinamica(276), Mec.Mat(278), Fund.Mec(272)
-- Materias Ind: Fisica(173), Propiedad.Mat(161)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('MINE. Alvaro José Leal Osorio', 'TIEMPO COMPLETO', 1, 32);
DECLARE @d39 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d39, 2, 1);
INSERT INTO DocenteCarrera VALUES (@d39, 5, 0);
INSERT INTO DocenteMateria VALUES (@d39,266),(@d39,276),(@d39,278),(@d39,272);
INSERT INTO DocenteMateria VALUES (@d39,173),(@d39,161);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d39,2,1),(@d39,2,2),(@d39,2,3),(@d39,2,4),(@d39,2,5),(@d39,2,6),(@d39,2,7),(@d39,2,8),
(@d39,3,1),(@d39,3,2),(@d39,3,3),(@d39,3,4),(@d39,3,5),(@d39,3,6),(@d39,3,7),(@d39,3,8),
(@d39,4,1),(@d39,4,2),(@d39,4,3),(@d39,4,4),(@d39,4,5),(@d39,4,6),(@d39,4,7),(@d39,4,8),
(@d39,5,1),(@d39,5,2),(@d39,5,3),(@d39,5,4),(@d39,5,5),(@d39,5,6),(@d39,5,7),(@d39,5,8);
GO

-- ?? 40. Ing. Juan de Dios Kantun Pech ????????????????????????
-- MT · 20h · Lun-Vie bloques 1-4 (4h×5=20h)
-- Principal: Civil | Sin secundaria
-- Materias: Tecnol.Concreto(270), Mat.Construccion(264), Sup.Obra(301), Fund.Invest(254)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('Ing. Juan de Dios Kantun Pech', 'MEDIO TIEMPO', 1, 20);
DECLARE @d40 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d40, 2, 1);
INSERT INTO DocenteMateria VALUES (@d40,270),(@d40,264),(@d40,301),(@d40,254);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d40,1,1),(@d40,1,2),(@d40,1,3),(@d40,1,4),
(@d40,2,1),(@d40,2,2),(@d40,2,3),(@d40,2,4),
(@d40,3,1),(@d40,3,2),(@d40,3,3),(@d40,3,4),
(@d40,4,1),(@d40,4,2),(@d40,4,3),(@d40,4,4),
(@d40,5,1),(@d40,5,2),(@d40,5,3),(@d40,5,4);
GO

-- ?? 41. M.C. Fidel Morales Couoh ?????????????????????????????
-- MT · 16h · Mar-Vie bloques 3-6 (Lun libre, 4h×4=16h)
-- Principal: Civil | Secundaria: Bioquímica (geología/física)
-- Materias Civil: Geologia(261), Ec.Dif(267), Met.Num(273)
-- Materias Bioq: Fisica(120), Estadistica(121)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('M.C. Fidel Morales Couoh', 'MEDIO TIEMPO', 1, 16);
DECLARE @d41 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d41, 2, 1);
INSERT INTO DocenteCarrera VALUES (@d41, 6, 0);
INSERT INTO DocenteMateria VALUES (@d41,261),(@d41,267),(@d41,273);
INSERT INTO DocenteMateria VALUES (@d41,120),(@d41,121);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d41,2,3),(@d41,2,4),(@d41,2,5),(@d41,2,6),
(@d41,3,3),(@d41,3,4),(@d41,3,5),(@d41,3,6),
(@d41,4,3),(@d41,4,4),(@d41,4,5),(@d41,4,6),
(@d41,5,3),(@d41,5,4),(@d41,5,5),(@d41,5,6);
GO

-- ?? 42. M.C. Jesús Alberto Caamal Canche ?????????????????????
-- MT · 18h · Lun-Mié bloques 5-8 (4h×3=12) + Jue-Vie bloques 5-7 (3h×2=6) = 18h
-- Principal: Civil | Sin secundaria
-- Materias: Admon.Const(287), Costos.Pres(281), Form.Eval(294), Ing.Costos(295)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('M.C. Jesús Alberto Caamal Canche', 'MEDIO TIEMPO', 1, 18);
DECLARE @d42 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d42, 2, 1);
INSERT INTO DocenteMateria VALUES (@d42,287),(@d42,281),(@d42,294),(@d42,295);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d42,1,5),(@d42,1,6),(@d42,1,7),(@d42,1,8),
(@d42,2,5),(@d42,2,6),(@d42,2,7),(@d42,2,8),
(@d42,3,5),(@d42,3,6),(@d42,3,7),(@d42,3,8),
(@d42,4,5),(@d42,4,6),(@d42,4,7),
(@d42,5,5),(@d42,5,6),(@d42,5,7);
GO

-- ?? 43. Ing. Milvia Josefina Serralta Peralta ????????????????
-- MT · 16h · Lun-Jue bloques 7-10 (Vie libre, 4h×4=16h)
-- Principal: Civil | Secundaria: Industrial (matemáticas)
-- Materias Civil: Algebra.Lin(260), Calc.Vec(268), Prob.Est(262)
-- Materias Ind: Algebra.Lin(167), Prob.Est(164)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('Ing. Milvia Josefina Serralta Peralta', 'MEDIO TIEMPO', 1, 16);
DECLARE @d43 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d43, 2, 1);
INSERT INTO DocenteCarrera VALUES (@d43, 5, 0);
INSERT INTO DocenteMateria VALUES (@d43,260),(@d43,268),(@d43,262);
INSERT INTO DocenteMateria VALUES (@d43,167),(@d43,164);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d43,1,7),(@d43,1,8),(@d43,1,9),(@d43,1,10),
(@d43,2,7),(@d43,2,8),(@d43,2,9),(@d43,2,10),
(@d43,3,7),(@d43,3,8),(@d43,3,9),(@d43,3,10),
(@d43,4,7),(@d43,4,8),(@d43,4,9),(@d43,4,10);
GO

-- ?? 44. Ing. Caleb Yair Palacios Huicab ??????????????????????
-- MT · 20h · Lun-Vie bloques 9-12 (4h×5=20h)
-- Principal: Civil | Sin secundaria
-- Materias: Proy.Mun(289), Des.Sust(279), Topicos(298), Taller.Invest2(292)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('Ing. Caleb Yair Palacios Huicab', 'MEDIO TIEMPO', 1, 20);
DECLARE @d44 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d44, 2, 1);
INSERT INTO DocenteMateria VALUES (@d44,289),(@d44,279),(@d44,298),(@d44,292);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d44,1,9),(@d44,1,10),(@d44,1,11),(@d44,1,12),
(@d44,2,9),(@d44,2,10),(@d44,2,11),(@d44,2,12),
(@d44,3,9),(@d44,3,10),(@d44,3,11),(@d44,3,12),
(@d44,4,9),(@d44,4,10),(@d44,4,11),(@d44,4,12),
(@d44,5,9),(@d44,5,10),(@d44,5,11),(@d44,5,12);
GO

-- ?? 45. Dr. Arturo Antonio Alvarado Segura ???????????????????
-- MT · 16h · Lun-Mar bloques 3-8 (6h×2=12) + Jue bloques 3-6 (4h) = 16h
-- Mié y Vie libres
-- Principal: Civil | Sin secundaria
-- Materias: Modelos.Opt(277), Software.Civil(258), Fund.Invest(254), Prob.Est(262)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('Dr. Arturo Antonio Alvarado Segura', 'MEDIO TIEMPO', 1, 16);
DECLARE @d45 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d45, 2, 1);
INSERT INTO DocenteMateria VALUES (@d45,277),(@d45,258),(@d45,254),(@d45,262);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d45,1,3),(@d45,1,4),(@d45,1,5),(@d45,1,6),(@d45,1,7),(@d45,1,8),
(@d45,2,3),(@d45,2,4),(@d45,2,5),(@d45,2,6),(@d45,2,7),(@d45,2,8),
(@d45,4,3),(@d45,4,4),(@d45,4,5),(@d45,4,6);
GO

-- ============================================================
-- ?? DESARROLLO COMUNITARIO (id_carrera = 3)
-- ============================================================

-- ?? 46. M.C. Ileana Evelina Carrillo Segura ??????????????????
-- TC · 40h · Lun-Vie bloques 1-8 (8h×5=40h)
-- Principal: Comunitario | Secundaria: Bioquímica (ciencias básicas)
-- Materias Com: Microb(69), Bioq(68), Fisiologia(74), Agroecologia(89),
--   Biotecnologia(95), Ecologia(83)
-- Materias Bioq: Comportamiento.Org(108), Administracion(110)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('M.C. Ileana Evelina Carrillo Segura', 'TIEMPO COMPLETO', 1, 40);
DECLARE @d46 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d46, 3, 1);
INSERT INTO DocenteCarrera VALUES (@d46, 6, 0);
INSERT INTO DocenteMateria VALUES (@d46,69),(@d46,68),(@d46,74),(@d46,89),(@d46,95),(@d46,83);
INSERT INTO DocenteMateria VALUES (@d46,108),(@d46,110);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d46,1,1),(@d46,1,2),(@d46,1,3),(@d46,1,4),(@d46,1,5),(@d46,1,6),(@d46,1,7),(@d46,1,8),
(@d46,2,1),(@d46,2,2),(@d46,2,3),(@d46,2,4),(@d46,2,5),(@d46,2,6),(@d46,2,7),(@d46,2,8),
(@d46,3,1),(@d46,3,2),(@d46,3,3),(@d46,3,4),(@d46,3,5),(@d46,3,6),(@d46,3,7),(@d46,3,8),
(@d46,4,1),(@d46,4,2),(@d46,4,3),(@d46,4,4),(@d46,4,5),(@d46,4,6),(@d46,4,7),(@d46,4,8),
(@d46,5,1),(@d46,5,2),(@d46,5,3),(@d46,5,4),(@d46,5,5),(@d46,5,6),(@d46,5,7),(@d46,5,8);
GO

-- ?? 47. Ing. Georgina Chi González ???????????????????????????
-- TC · 36h · Lun-Jue 8h + Vie 4h (bloques 1-4)
-- Principal: Comunitario | Sin secundaria
-- Materias: Botanica(62), Zoologia(75), Nutr.Sanidad(102),
--   Sist.Prod.Agr(97), Fisiologia(74), Prod.Agroindustrial(100)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('Ing. Georgina Chi González', 'TIEMPO COMPLETO', 1, 36);
DECLARE @d47 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d47, 3, 1);
INSERT INTO DocenteMateria VALUES (@d47,62),(@d47,75),(@d47,102),(@d47,97),(@d47,74),(@d47,100);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d47,1,1),(@d47,1,2),(@d47,1,3),(@d47,1,4),(@d47,1,5),(@d47,1,6),(@d47,1,7),(@d47,1,8),
(@d47,2,1),(@d47,2,2),(@d47,2,3),(@d47,2,4),(@d47,2,5),(@d47,2,6),(@d47,2,7),(@d47,2,8),
(@d47,3,1),(@d47,3,2),(@d47,3,3),(@d47,3,4),(@d47,3,5),(@d47,3,6),(@d47,3,7),(@d47,3,8),
(@d47,4,1),(@d47,4,2),(@d47,4,3),(@d47,4,4),(@d47,4,5),(@d47,4,6),(@d47,4,7),(@d47,4,8),
(@d47,5,1),(@d47,5,2),(@d47,5,3),(@d47,5,4);
GO

-- ?? 48. Lic. Mario Alberto León Palomo ???????????????????????
-- TC · 40h · Lun-Vie bloques 5-12 (8h×5=40h)
-- Principal: Comunitario | Sin secundaria
-- Materias: Sociologia.Rural(54), Fund.Des.Com(55), Cultura.Com(60),
--   Socioeconomia(67), Org.Grupos(61), Politicas(84)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('Lic. Mario Alberto León Palomo', 'TIEMPO COMPLETO', 1, 40);
DECLARE @d48 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d48, 3, 1);
INSERT INTO DocenteMateria VALUES (@d48,54),(@d48,55),(@d48,60),(@d48,67),(@d48,61),(@d48,84);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d48,1,5),(@d48,1,6),(@d48,1,7),(@d48,1,8),(@d48,1,9),(@d48,1,10),(@d48,1,11),(@d48,1,12),
(@d48,2,5),(@d48,2,6),(@d48,2,7),(@d48,2,8),(@d48,2,9),(@d48,2,10),(@d48,2,11),(@d48,2,12),
(@d48,3,5),(@d48,3,6),(@d48,3,7),(@d48,3,8),(@d48,3,9),(@d48,3,10),(@d48,3,11),(@d48,3,12),
(@d48,4,5),(@d48,4,6),(@d48,4,7),(@d48,4,8),(@d48,4,9),(@d48,4,10),(@d48,4,11),(@d48,4,12),
(@d48,5,5),(@d48,5,6),(@d48,5,7),(@d48,5,8),(@d48,5,9),(@d48,5,10),(@d48,5,11),(@d48,5,12);
GO

-- ?? 49. Lic. Carlos Manuel Domínguez Castro ??????????????????
-- TC · 32h · Mar-Vie bloques 1-8 (Lun libre, 8h×4=32h)
-- Principal: Comunitario | Sin secundaria
-- Materias: Org.Grupos(61), Taller.Diag(76), Politicas(84),
--   Taller.Des.Com(87), Ext.Rural(101), Gest.Innov(103)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('Lic. Carlos Manuel Domínguez Castro', 'TIEMPO COMPLETO', 1, 32);
DECLARE @d49 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d49, 3, 1);
INSERT INTO DocenteMateria VALUES (@d49,61),(@d49,76),(@d49,84),(@d49,87),(@d49,101),(@d49,103);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d49,2,1),(@d49,2,2),(@d49,2,3),(@d49,2,4),(@d49,2,5),(@d49,2,6),(@d49,2,7),(@d49,2,8),
(@d49,3,1),(@d49,3,2),(@d49,3,3),(@d49,3,4),(@d49,3,5),(@d49,3,6),(@d49,3,7),(@d49,3,8),
(@d49,4,1),(@d49,4,2),(@d49,4,3),(@d49,4,4),(@d49,4,5),(@d49,4,6),(@d49,4,7),(@d49,4,8),
(@d49,5,1),(@d49,5,2),(@d49,5,3),(@d49,5,4),(@d49,5,5),(@d49,5,6),(@d49,5,7),(@d49,5,8);
GO

-- ?? 50. Ing. Leticia Natali Góngora Cáceres ??????????????????
-- MT · 16h · Lun-Jue bloques 1-4 (Vie libre, 4h×4=16h)
-- Principal: Comunitario | Sin secundaria
-- Materias: Introd.Prod.Agrop(82), Sist.Prod.Pec(99), Prod.Agroindustrial(100)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('Ing. Leticia Natali Góngora Cáceres', 'MEDIO TIEMPO', 1, 16);
DECLARE @d50 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d50, 3, 1);
INSERT INTO DocenteMateria VALUES (@d50,82),(@d50,99),(@d50,100);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d50,1,1),(@d50,1,2),(@d50,1,3),(@d50,1,4),
(@d50,2,1),(@d50,2,2),(@d50,2,3),(@d50,2,4),
(@d50,3,1),(@d50,3,2),(@d50,3,3),(@d50,3,4),
(@d50,4,1),(@d50,4,2),(@d50,4,3),(@d50,4,4);
GO

-- ?? 51. M.C. Damaris Guadalupe Ortegón Rivero ????????????????
-- MT · 20h · Lun-Vie bloques 3-6 (4h×5=20h)
-- Principal: Comunitario | Sin secundaria
-- Materias: Edafologia(78), Manejo.Agua(90), Agroclimatologia(91), Agroecologia(89)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('M.C. Damaris Guadalupe Ortegón Rivero', 'MEDIO TIEMPO', 1, 20);
DECLARE @d51 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d51, 3, 1);
INSERT INTO DocenteMateria VALUES (@d51,78),(@d51,90),(@d51,91),(@d51,89);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d51,1,3),(@d51,1,4),(@d51,1,5),(@d51,1,6),
(@d51,2,3),(@d51,2,4),(@d51,2,5),(@d51,2,6),
(@d51,3,3),(@d51,3,4),(@d51,3,5),(@d51,3,6),
(@d51,4,3),(@d51,4,4),(@d51,4,5),(@d51,4,6),
(@d51,5,3),(@d51,5,4),(@d51,5,5),(@d51,5,6);
GO

-- ?? 52. Ing. Hernán José Aranda Suarez ???????????????????????
-- MT · 16h · Mar-Vie bloques 5-8 (Lun libre, 4h×4=16h)
-- Principal: Comunitario | Sin secundaria
-- Materias: Plan.Regional(80), Sist.Info.Geog(81), Eval.Tecnol(93), Biotecnologia(95)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('Ing. Hernán José Aranda Suarez', 'MEDIO TIEMPO', 1, 16);
DECLARE @d52 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d52, 3, 1);
INSERT INTO DocenteMateria VALUES (@d52,80),(@d52,81),(@d52,93),(@d52,95);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d52,2,5),(@d52,2,6),(@d52,2,7),(@d52,2,8),
(@d52,3,5),(@d52,3,6),(@d52,3,7),(@d52,3,8),
(@d52,4,5),(@d52,4,6),(@d52,4,7),(@d52,4,8),
(@d52,5,5),(@d52,5,6),(@d52,5,7),(@d52,5,8);
GO

-- ?? 53. Dr. Marcos Alberto Briceño Méndez ????????????????????
-- MT · 18h · Lun,Mié,Vie bloques 7-12 (6h×3=18h) — Mar,Jue libres
-- Principal: Comunitario | Sin secundaria
-- Materias: Fund.Invest(57), Biotecnologia(95), Ecologia(83), Des.Sust(86)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('Dr. Marcos Alberto Briceño Méndez', 'MEDIO TIEMPO', 1, 18);
DECLARE @d53 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d53, 3, 1);
INSERT INTO DocenteMateria VALUES (@d53,57),(@d53,95),(@d53,83),(@d53,86);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d53,1,7),(@d53,1,8),(@d53,1,9),(@d53,1,10),(@d53,1,11),(@d53,1,12),
(@d53,3,7),(@d53,3,8),(@d53,3,9),(@d53,3,10),(@d53,3,11),(@d53,3,12),
(@d53,5,7),(@d53,5,8),(@d53,5,9),(@d53,5,10),(@d53,5,11),(@d53,5,12);
GO

-- ?? 54. Ing. Carlos Gabriel Cocom Poot ???????????????????????
-- MT · 20h · Lun-Vie bloques 1-4 (4h×5=20h)
-- Principal: Comunitario | Sin secundaria
-- Materias: Form.Eval.Proy(96), Plan.Nuevas.Emp(98), Calidad(92), Anal.Econ(85)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('Ing. Carlos Gabriel Cocom Poot', 'MEDIO TIEMPO', 1, 20);
DECLARE @d54 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d54, 3, 1);
INSERT INTO DocenteMateria VALUES (@d54,96),(@d54,98),(@d54,92),(@d54,85);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d54,1,1),(@d54,1,2),(@d54,1,3),(@d54,1,4),
(@d54,2,1),(@d54,2,2),(@d54,2,3),(@d54,2,4),
(@d54,3,1),(@d54,3,2),(@d54,3,3),(@d54,3,4),
(@d54,4,1),(@d54,4,2),(@d54,4,3),(@d54,4,4),
(@d54,5,1),(@d54,5,2),(@d54,5,3),(@d54,5,4);
GO

-- ?? 55. MC. Cesar David Lara Colli ???????????????????????????
-- MT · 16h · Mar-Vie bloques 3-6 (Lun libre, 4h×4=16h)
-- Principal: Comunitario | Sin secundaria
-- Materias: Anal.Econ(85), Fund.Contab(79), Ext.Rural(101)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('MC. Cesar David Lara Colli', 'MEDIO TIEMPO', 1, 16);
DECLARE @d55 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d55, 3, 1);
INSERT INTO DocenteMateria VALUES (@d55,85),(@d55,79),(@d55,101);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d55,2,3),(@d55,2,4),(@d55,2,5),(@d55,2,6),
(@d55,3,3),(@d55,3,4),(@d55,3,5),(@d55,3,6),
(@d55,4,3),(@d55,4,4),(@d55,4,5),(@d55,4,6),
(@d55,5,3),(@d55,5,4),(@d55,5,5),(@d55,5,6);
GO

-- ?? 56. Dr. Cesar Jacier Tucuch Haas ?????????????????????????
-- MT · 18h · Lun-Jue bloques 7-10 (4h×4=16) + Vie 7-8 (2h) = 18h
-- Principal: Comunitario | Sin secundaria
-- Materias: Gest.Innov(103), Fisiologia.Veg(74), Des.Sust(86)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('Dr. Cesar Jacier Tucuch Haas', 'MEDIO TIEMPO', 1, 18);
DECLARE @d56 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d56, 3, 1);
INSERT INTO DocenteMateria VALUES (@d56,103),(@d56,74),(@d56,86);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d56,1,7),(@d56,1,8),(@d56,1,9),(@d56,1,10),
(@d56,2,7),(@d56,2,8),(@d56,2,9),(@d56,2,10),
(@d56,3,7),(@d56,3,8),(@d56,3,9),(@d56,3,10),
(@d56,4,7),(@d56,4,8),(@d56,4,9),(@d56,4,10),
(@d56,5,7),(@d56,5,8);
GO

-- ?? 57. CP. Carlos William Falcón Plascencia ?????????????????
-- MT · 16h · Mar-Jue bloques 7-10 (4h×3=12) + Vie 9-12 (4h) = 16h
-- Lun libre
-- Principal: Comunitario | Secundaria: Gestión (administración/contab)
-- Materias Com: Fund.Admin(72), TIC(65), Socioeconomia(67)
-- Materias Gest: Fund.Invest(205), Dinamica.Social(214)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('CP. Carlos William Falcón Plascencia', 'MEDIO TIEMPO', 1, 16);
DECLARE @d57 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d57, 3, 1);
INSERT INTO DocenteCarrera VALUES (@d57, 4, 0);
INSERT INTO DocenteMateria VALUES (@d57,72),(@d57,65),(@d57,67);
INSERT INTO DocenteMateria VALUES (@d57,205),(@d57,214);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d57,2,7),(@d57,2,8),(@d57,2,9),(@d57,2,10),
(@d57,3,7),(@d57,3,8),(@d57,3,9),(@d57,3,10),
(@d57,4,7),(@d57,4,8),(@d57,4,9),(@d57,4,10),
(@d57,5,9),(@d57,5,10),(@d57,5,11),(@d57,5,12);
GO

-- ?? 58. Ing. Gustavo Andrés Murillo Peralta ??????????????????
-- MT · 20h · Lun-Vie bloques 1-4 (4h×5=20h)
-- Principal: Comunitario | Secundaria: Civil, Bioquímica (física/química)
-- Materias Com: Quimica(63), Fisica1(70), Fisica2(77)
-- Materias Civil: Quimica(257)
-- Materias Bioq: Fisica(120)
INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
VALUES ('Ing. Gustavo Andrés Murillo Peralta', 'MEDIO TIEMPO', 1, 20);
DECLARE @d58 INT = SCOPE_IDENTITY();
INSERT INTO DocenteCarrera VALUES (@d58, 3, 1);
INSERT INTO DocenteCarrera VALUES (@d58, 2, 0);
INSERT INTO DocenteCarrera VALUES (@d58, 6, 0);
INSERT INTO DocenteMateria VALUES (@d58,63),(@d58,70),(@d58,77);
INSERT INTO DocenteMateria VALUES (@d58,257);
INSERT INTO DocenteMateria VALUES (@d58,120);
INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque) VALUES
(@d58,1,1),(@d58,1,2),(@d58,1,3),(@d58,1,4),
(@d58,2,1),(@d58,2,2),(@d58,2,3),(@d58,2,4),
(@d58,3,1),(@d58,3,2),(@d58,3,3),(@d58,3,4),
(@d58,4,1),(@d58,4,2),(@d58,4,3),(@d58,4,4),
(@d58,5,1),(@d58,5,2),(@d58,5,3),(@d58,5,4);
GO

-- ============================================================
-- VERIFICACIÓN FINAL
-- ============================================================
SELECT
    d.id_docente,
    d.nombre,
    d.tipo_docente,
    d.horas_maximas,
    c.nombre AS carrera_principal,
    (SELECT COUNT(*) FROM DocenteCarrera dc2 WHERE dc2.id_docente = d.id_docente) AS num_carreras,
    (SELECT COUNT(*) FROM DocenteMateria dm WHERE dm.id_docente = d.id_docente) AS total_materias,
    (SELECT COUNT(*) FROM DisponibilidadDocente dd WHERE dd.id_docente = d.id_docente) AS horas_disponibles
FROM Docentes d
JOIN DocenteCarrera dc ON d.id_docente = dc.id_docente AND dc.es_principal = 1
JOIN Carreras c ON dc.id_carrera = c.id_carrera
ORDER BY c.id_carrera, d.id_docente;
