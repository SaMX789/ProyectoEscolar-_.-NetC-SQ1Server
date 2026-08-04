using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.OrTools.Sat;
using Microsoft.Data.SqlClient;
using GestorHorarios.Models;

namespace GestorHorarios.Services
{
    public class GeneradorHorariosService
    {
        public async Task<string> EjecutarDiagnosticoAsync(int idProyecto, int idCarrera)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (idProyecto <= 0) return "ERROR: ID del proyecto inválido.";

                    // 1. OBTENER EL CICLO DEL PROYECTO DESDE LA BASE DE DATOS
                    string ciclo = ObtenerCicloProyecto(idProyecto);

                    // 2. CARGAR TODOS LOS DATOS
                    List<Grupo> gruposTodos = ObtenerGruposDeDB(idCarrera);
                    List<Materia> materiasTodas = ObtenerMateriasDeDB(idCarrera);

                    // 3. FILTRAR ESTRICTAMENTE POR CICLO (A = Pares, B = Impares)
                    List<Grupo> grupos = ciclo == "A"
                        ? gruposTodos.Where(g => g.Semestre % 2 == 0).ToList()
                        : gruposTodos.Where(g => g.Semestre % 2 != 0).ToList();

                    List<Materia> materias = ciclo == "A"
                        ? materiasTodas.Where(m => m.Semestre % 2 == 0).ToList()
                        : materiasTodas.Where(m => m.Semestre % 2 != 0).ToList();

                    List<Docente> docentes = ObtenerDocentesDeDB(idCarrera);

                    // Si después de filtrar no hay grupos, abortamos rápidamente sin error
                    if (grupos.Count == 0 || materias.Count == 0) return "Omitido: Sin grupos o materias para este ciclo.";

                    if (!docentes.Any(d => d.IdDocente == -1))
                    {
                        docentes.Add(new Docente { IdDocente = -1, NombreCompleto = "Sin Maestro Asignado", MateriasIds = new List<int>(), HorasMaximas = 999 });
                    }

                    CargarOcupacionPrevia(idProyecto, idCarrera, docentes, out var horasPrevias, out var bloquesOcupadosPrevios);

                    int[] dias = { 1, 2, 3, 4, 5 };
                    int[] bloquesMatutinosBase = { 1, 2, 3, 4, 5, 6 };
                    int bloqueExtendidoMatutino = 7;
                    int[] bloquesVespertinosBase = { 7, 8, 9, 10, 11, 12 };
                    int bloqueExtendidoVespertino = 6;

                    Dictionary<int, List<int>> bloquesPorGrupo = new Dictionary<int, List<int>>();
                    Dictionary<string, int> materiaDocenteAsignado = new Dictionary<string, int>();
                    Dictionary<int, int> horasAsignadasPorMaestro = new Dictionary<int, int>(horasPrevias);

                    // 4. ASIGNACIÓN INTELIGENTE (Load Balancing)
                    foreach (var g in grupos)
                    {
                        bool esMatutino = g.Turno.ToLower().Contains("matutino") || g.Turno.ToLower().StartsWith("m");
                        List<int> bloquesDelGrupo = new List<int>(esMatutino ? bloquesMatutinosBase : bloquesVespertinosBase);

                        var materiasDelGrupo = materias.Where(m => m.Semestre == g.Semestre).ToList();
                        int creditosTotales = materiasDelGrupo.Sum(m => m.Creditos);

                        if (creditosTotales > 30)
                        {
                            if (esMatutino) bloquesDelGrupo.Add(bloqueExtendidoMatutino);
                            else bloquesDelGrupo.Insert(0, bloqueExtendidoVespertino);
                        }

                        bloquesPorGrupo[g.IdGrupo] = bloquesDelGrupo;

                        foreach (var m in materiasDelGrupo)
                        {
                            var docentesCapaces = docentes.Where(doc => doc.IdDocente != -1 && doc.MateriasIds.Contains(m.IdMateria)).ToList();

                            var docentesConCapacidad = docentesCapaces.Where(d =>
                                (horasAsignadasPorMaestro[d.IdDocente] + m.Creditos) <= d.HorasMaximas
                            ).ToList();

                            var docentesDisponibles = docentesConCapacidad.Where(d =>
                                d.DisponibilidadBloques.Count == 0 ||
                                d.DisponibilidadBloques.Any(disp => bloquesDelGrupo.Contains(int.Parse(disp.Split('_')[1])))
                            ).ToList();

                            var pool = docentesDisponibles.Count > 0 ? docentesDisponibles : docentesConCapacidad;

                            Docente docenteElegido = null;
                            if (pool.Count > 0)
                                docenteElegido = pool.OrderBy(d => horasAsignadasPorMaestro[d.IdDocente]).First();
                            else
                                docenteElegido = docentes.First(d => d.IdDocente == -1);

                            materiaDocenteAsignado[$"{g.IdGrupo}_{m.IdMateria}"] = docenteElegido.IdDocente;
                            horasAsignadasPorMaestro[docenteElegido.IdDocente] += m.Creditos;
                        }
                    }

                    CpModel model = new CpModel();
                    Dictionary<string, BoolVar> asignaciones = new Dictionary<string, BoolVar>();
                    Dictionary<string, BoolVar> grupoHoraActiva = new Dictionary<string, BoolVar>();
                    List<LinearExpr> penalizaciones = new List<LinearExpr>();

                    // 5. CREACIÓN DE VARIABLES
                    foreach (var g in grupos)
                    {
                        var materiasDelGrupo = materias.Where(m => m.Semestre == g.Semestre).ToList();
                        var bloquesPermitidos = bloquesPorGrupo[g.IdGrupo];

                        foreach (var d in dias)
                            foreach (var b in bloquesPermitidos)
                                grupoHoraActiva[$"{g.IdGrupo}_{d}_{b}"] = model.NewBoolVar($"Activa_{g.IdGrupo}_{d}_{b}");

                        foreach (var m in materiasDelGrupo)
                        {
                            int idMaestro = materiaDocenteAsignado[$"{g.IdGrupo}_{m.IdMateria}"];
                            var docente = docentes.First(d => d.IdDocente == idMaestro);
                            bool tieneDisponibilidadConfigurada = docente.DisponibilidadBloques.Count > 0;

                            foreach (var d in dias)
                            {
                                for (int i = 0; i < bloquesPermitidos.Count; i++)
                                {
                                    int b = bloquesPermitidos[i];
                                    var variableClase = model.NewBoolVar($"X_{g.IdGrupo}_{m.IdMateria}_{d}_{b}");
                                    asignaciones[$"{g.IdGrupo}_{m.IdMateria}_{d}_{b}"] = variableClase;

                                    if (idMaestro != -1)
                                    {
                                        if (tieneDisponibilidadConfigurada && !docente.DisponibilidadBloques.Contains($"{d}_{b}"))
                                            model.Add(variableClase == 0);

                                        if (bloquesOcupadosPrevios[idMaestro].Contains($"{d}_{b}"))
                                            model.Add(variableClase == 0);
                                    }

                                    if (b == bloqueExtendidoMatutino || b == bloqueExtendidoVespertino)
                                        penalizaciones.Add(variableClase * 100);

                                    penalizaciones.Add(variableClase * (i * 2));
                                }
                            }
                        }
                    }

                    // 6. RESTRICCIONES DE LOS ALUMNOS
                    foreach (var g in grupos)
                    {
                        var materiasDelGrupo = materias.Where(m => m.Semestre == g.Semestre).ToList();
                        var bloquesPermitidos = bloquesPorGrupo[g.IdGrupo];

                        foreach (var d in dias)
                        {
                            foreach (var b in bloquesPermitidos)
                            {
                                var hVar = grupoHoraActiva[$"{g.IdGrupo}_{d}_{b}"];
                                List<BoolVar> clasesEnEsteBloque = new List<BoolVar>();

                                foreach (var m in materiasDelGrupo)
                                    clasesEnEsteBloque.Add(asignaciones[$"{g.IdGrupo}_{m.IdMateria}_{d}_{b}"]);

                                model.AddAtMostOne(clasesEnEsteBloque);
                                model.Add(hVar == LinearExpr.Sum(clasesEnEsteBloque));
                            }
                        }

                        foreach (var m in materiasDelGrupo)
                        {
                            List<BoolVar> todasHorasMateria = new List<BoolVar>();

                            foreach (var d in dias)
                            {
                                List<BoolVar> horasEsteDia = new List<BoolVar>();
                                List<BoolVar> starts = new List<BoolVar>();

                                for (int i = 0; i < bloquesPermitidos.Count; i++)
                                {
                                    var b = bloquesPermitidos[i];
                                    var x_curr = asignaciones[$"{g.IdGrupo}_{m.IdMateria}_{d}_{b}"];
                                    horasEsteDia.Add(x_curr);
                                    todasHorasMateria.Add(x_curr);

                                    var is_start = model.NewBoolVar($"start_{g.IdGrupo}_{m.IdMateria}_{d}_{b}");
                                    starts.Add(is_start);

                                    if (i == 0) model.Add(is_start == x_curr);
                                    else
                                    {
                                        var prev_b = bloquesPermitidos[i - 1];
                                        var x_prev = asignaciones[$"{g.IdGrupo}_{m.IdMateria}_{d}_{prev_b}"];
                                        model.AddImplication(is_start, x_curr);
                                        model.AddImplication(is_start, x_prev.Not());
                                        model.AddBoolOr(new ILiteral[] { is_start, x_curr.Not(), x_prev });
                                    }
                                }

                                model.Add(LinearExpr.Sum(starts) <= 1);
                                model.Add(LinearExpr.Sum(horasEsteDia) <= 3);

                                var esUnaHora = model.NewBoolVar($"es_una_hora_{g.IdGrupo}_{m.IdMateria}_{d}");
                                var esTresHoras = model.NewBoolVar($"es_tres_horas_{g.IdGrupo}_{m.IdMateria}_{d}");

                                model.Add(LinearExpr.Sum(horasEsteDia) == 1).OnlyEnforceIf(esUnaHora);
                                model.Add(LinearExpr.Sum(horasEsteDia) != 1).OnlyEnforceIf(esUnaHora.Not());

                                model.Add(LinearExpr.Sum(horasEsteDia) == 3).OnlyEnforceIf(esTresHoras);
                                model.Add(LinearExpr.Sum(horasEsteDia) != 3).OnlyEnforceIf(esTresHoras.Not());

                                penalizaciones.Add(esUnaHora * 50);
                                penalizaciones.Add(esTresHoras * 500);
                            }

                            model.Add(LinearExpr.Sum(todasHorasMateria) <= m.Creditos);
                            var horasFaltantes = model.NewIntVar(0, m.Creditos, $"faltas_{g.IdGrupo}_{m.IdMateria}");
                            model.Add(horasFaltantes == m.Creditos - LinearExpr.Sum(todasHorasMateria));
                            penalizaciones.Add(horasFaltantes * 10000);
                        }
                    }

                    // 7. RESTRICCIONES ESTRICTAS DE PROFESORES
                    foreach (var doc in docentes)
                    {
                        if (doc.IdDocente == -1) continue;

                        List<BoolVar> clasesSemanalesDocente = new List<BoolVar>();

                        foreach (var d in dias)
                        {
                            List<BoolVar> clasesDiariasDocente = new List<BoolVar>();

                            for (int b = 1; b <= 12; b++)
                            {
                                List<BoolVar> clasesEnEsteBloqueParaDocente = new List<BoolVar>();

                                foreach (var g in grupos)
                                {
                                    var materiasDelGrupo = materias.Where(m => m.Semestre == g.Semestre).ToList();
                                    var bloquesPermitidos = bloquesPorGrupo[g.IdGrupo];

                                    if (!bloquesPermitidos.Contains(b)) continue;

                                    foreach (var m in materiasDelGrupo)
                                    {
                                        if (materiaDocenteAsignado[$"{g.IdGrupo}_{m.IdMateria}"] == doc.IdDocente)
                                        {
                                            var varClase = asignaciones[$"{g.IdGrupo}_{m.IdMateria}_{d}_{b}"];
                                            clasesEnEsteBloqueParaDocente.Add(varClase);
                                            clasesDiariasDocente.Add(varClase);
                                            clasesSemanalesDocente.Add(varClase);
                                        }
                                    }
                                }

                                if (clasesEnEsteBloqueParaDocente.Count > 1)
                                    model.AddAtMostOne(clasesEnEsteBloqueParaDocente);
                            }

                            if (clasesDiariasDocente.Count > 0)
                                model.Add(LinearExpr.Sum(clasesDiariasDocente) <= 8);
                        }

                        if (clasesSemanalesDocente.Count > 0)
                        {
                            int ocupacionPrevia = horasPrevias[doc.IdDocente];
                            int limiteRestante = Math.Max(0, doc.HorasMaximas - ocupacionPrevia);
                            model.Add(LinearExpr.Sum(clasesSemanalesDocente) <= limiteRestante);
                        }
                    }

                    model.Minimize(LinearExpr.Sum(penalizaciones));

                    CpSolver solver = new CpSolver();
                    solver.StringParameters = "max_time_in_seconds: 35.0";
                    CpSolverStatus status = solver.Solve(model);

                    if (status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible)
                    {
                        int insertados = GuardarResultadosEnDB(idProyecto, idCarrera, grupos, materias, dias, bloquesPorGrupo, materiaDocenteAsignado, asignaciones, solver);
                        return $"EXITO: Horario generado. Insertados {insertados} registros.";
                    }
                    return $"ERROR: Matemáticamente imposible o tiempo agotado.";
                }
                catch (Exception ex)
                {
                    return $"EXCEPCIÓN: {ex.Message}";
                }
            });
        }

        // ====================================================================
        // NUEVO MÉTODO: Obtiene el ciclo ('A' o 'B') para saber qué semestres ignorar
        // ====================================================================
        private string ObtenerCicloProyecto(int idProyecto)
        {
            try
            {
                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                using var cmd = new SqlCommand("SELECT ciclo FROM Proyectos WHERE id_proyecto = @id", conn);
                cmd.Parameters.AddWithValue("@id", idProyecto);
                conn.Open();
                return cmd.ExecuteScalar()?.ToString() ?? "A";
            }
            catch { return "A"; }
        }

        private void CargarOcupacionPrevia(int idProyecto, int idCarreraActual, List<Docente> docentes, out Dictionary<int, int> horasPrevias, out Dictionary<int, HashSet<string>> bloquesOcupados)
        {
            horasPrevias = new Dictionary<int, int>();
            bloquesOcupados = new Dictionary<int, HashSet<string>>();
            foreach (var d in docentes)
            {
                horasPrevias[d.IdDocente] = 0;
                bloquesOcupados[d.IdDocente] = new HashSet<string>();
            }

            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand(@"
                SELECT h.id_docente, h.id_dia, h.id_bloque
                FROM HorarioDetalle h
                INNER JOIN Grupos g ON h.id_grupo = g.id_grupo
                WHERE h.id_proyecto = @idProj AND g.id_carrera != @idCarreraActual", conn);

            cmd.Parameters.AddWithValue("@idProj", idProyecto);
            cmd.Parameters.AddWithValue("@idCarreraActual", idCarreraActual);
            conn.Open();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int idDoc = Convert.ToInt32(reader["id_docente"]);
                if (horasPrevias.ContainsKey(idDoc))
                {
                    horasPrevias[idDoc]++;
                    string bloque = $"{reader["id_dia"]}_{reader["id_bloque"]}";
                    bloquesOcupados[idDoc].Add(bloque);
                }
            }
        }

        private List<Grupo> ObtenerGruposDeDB(int idCarrera)
        {
            var grupos = new List<Grupo>();
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand("sp_GetGruposByCarrera", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@id_carrera", idCarrera);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                grupos.Add(new Grupo
                {
                    IdGrupo = Convert.ToInt32(reader["id_grupo"]),
                    Nombre = reader["NombreGrupo"].ToString() ?? "",
                    Semestre = Convert.ToInt32(reader["semestre"]),
                    Turno = reader["turno"].ToString() ?? ""
                });
            }
            return grupos;
        }

        private List<Materia> ObtenerMateriasDeDB(int idCarrera)
        {
            var materias = new List<Materia>();
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand("sp_ObtenerMateriasPorCarrera", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@id_carrera", idCarrera);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                materias.Add(new Materia
                {
                    IdMateria = Convert.ToInt32(reader["id_materia"]),
                    Nombre = reader["nombre"].ToString() ?? "",
                    Clave = reader["Clave"].ToString() ?? "",
                    Creditos = Convert.ToInt32(reader["creditos"]),
                    Semestre = Convert.ToInt32(reader["semestre"])
                });
            }
            return materias;
        }

        private List<Docente> ObtenerDocentesDeDB(int idCarrera)
        {
            var dictDocentes = new Dictionary<int, Docente>();
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            conn.Open();

            using (var cmd = new SqlCommand("sp_GetDocentesGlobalesPorCarrera", conn))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_carrera", idCarrera);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int id = Convert.ToInt32(reader["id_docente"]);
                    dictDocentes[id] = new Docente
                    {
                        IdDocente = id,
                        NombreCompleto = reader["NombreCompleto"].ToString() ?? "",
                        HorasMaximas = reader["HorasMaximas"] != DBNull.Value ? Convert.ToInt32(reader["HorasMaximas"]) : 20,
                        IdCarreraPrincipal = reader["IdCarreraPrincipal"] != DBNull.Value ? Convert.ToInt32(reader["IdCarreraPrincipal"]) : 0,
                        MateriasIds = new List<int>(),
                        DisponibilidadBloques = new HashSet<string>()
                    };
                }
            }

            if (dictDocentes.Count > 0)
            {
                string idsDocentes = string.Join(",", dictDocentes.Keys);

                string queryMaterias = $"SELECT id_docente, id_materia FROM DocenteMateria WHERE id_docente IN ({idsDocentes})";
                using (var cmdMat = new SqlCommand(queryMaterias, conn))
                using (var readerMat = cmdMat.ExecuteReader())
                {
                    while (readerMat.Read())
                    {
                        int idDoc = Convert.ToInt32(readerMat["id_docente"]);
                        int idMat = Convert.ToInt32(readerMat["id_materia"]);
                        if (dictDocentes.ContainsKey(idDoc)) dictDocentes[idDoc].MateriasIds.Add(idMat);
                    }
                }

                string queryDisp = $"SELECT id_docente, id_dia, id_bloque FROM DisponibilidadDocente WHERE id_docente IN ({idsDocentes})";
                using (var cmdDisp = new SqlCommand(queryDisp, conn))
                using (var readerDisp = cmdDisp.ExecuteReader())
                {
                    while (readerDisp.Read())
                    {
                        int idDoc = Convert.ToInt32(readerDisp["id_docente"]);
                        string bloque = $"{readerDisp["id_dia"]}_{readerDisp["id_bloque"]}";
                        if (dictDocentes.ContainsKey(idDoc)) dictDocentes[idDoc].DisponibilidadBloques.Add(bloque);
                    }
                }
            }
            return dictDocentes.Values.ToList();
        }

        private int GuardarResultadosEnDB(int idProyecto, int idCarrera, List<Grupo> grupos, List<Materia> materias, int[] dias, Dictionary<int, List<int>> bloquesPorGrupo, Dictionary<string, int> materiaDocenteAsignado, Dictionary<string, BoolVar> asignaciones, CpSolver solver)
        {
            int contadorInsert = 0;
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            conn.Open();

            using (var cmdDelete = new SqlCommand(@"
                DELETE FROM HorarioDetalle 
                WHERE id_proyecto = @idProj AND id_grupo IN (SELECT id_grupo FROM Grupos WHERE id_carrera = @idCarrera)", conn))
            {
                cmdDelete.Parameters.AddWithValue("@idProj", idProyecto);
                cmdDelete.Parameters.AddWithValue("@idCarrera", idCarrera);
                cmdDelete.ExecuteNonQuery();
            }

            foreach (var g in grupos)
            {
                var materiasDelGrupo = materias.Where(m => m.Semestre == g.Semestre).ToList();
                var bloquesPermitidos = bloquesPorGrupo[g.IdGrupo];

                foreach (var m in materiasDelGrupo)
                {
                    foreach (var d in dias)
                    {
                        foreach (var b in bloquesPermitidos)
                        {
                            string key = $"{g.IdGrupo}_{m.IdMateria}_{d}_{b}";

                            if (solver.Value(asignaciones[key]) == 1)
                            {
                                int maestroAsignado = materiaDocenteAsignado[$"{g.IdGrupo}_{m.IdMateria}"];

                                using var cmdInsert = new SqlCommand(@"
                                    INSERT INTO HorarioDetalle (id_proyecto, id_grupo, id_materia, id_docente, id_dia, id_bloque, id_salon) 
                                    VALUES (@idProj, @idGrupo, @idMat, @idDoc, @dia, @bloque, 1)", conn);

                                cmdInsert.Parameters.AddWithValue("@idProj", idProyecto);
                                cmdInsert.Parameters.AddWithValue("@idGrupo", g.IdGrupo);
                                cmdInsert.Parameters.AddWithValue("@idMat", m.IdMateria);
                                cmdInsert.Parameters.AddWithValue("@idDoc", maestroAsignado);
                                cmdInsert.Parameters.AddWithValue("@dia", d);
                                cmdInsert.Parameters.AddWithValue("@bloque", b);

                                cmdInsert.ExecuteNonQuery();
                                contadorInsert++;
                            }
                        }
                    }
                }
            }
            return contadorInsert;
        }
    }
}