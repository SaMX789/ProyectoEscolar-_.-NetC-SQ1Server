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

                    List<Grupo> grupos = ObtenerGruposDeDB(idCarrera);
                    List<Materia> materias = ObtenerMateriasDeDB(idCarrera);
                    List<Docente> docentes = ObtenerDocentesDeDB(idCarrera);

                    if (grupos.Count == 0) return "ERROR: No hay Grupos.";
                    if (materias.Count == 0) return "ERROR: No hay Materias.";

                    if (docentes.Count == 0)
                    {
                        docentes.Add(new Docente { IdDocente = 1, NombreCompleto = "Sin Maestro Asignado", MateriasIds = new List<int>() });
                    }

                    int[] dias = { 1, 2, 3, 4, 5 };

                    // Turno Matutino: Bloques del 1 al 6 (7:30 a 13:30)
                    int[] bloquesMatutinosBase = { 1, 2, 3, 4, 5, 6 };
                    int bloqueExtendidoMatutino = 7; // Hora prestada: Bloque 7 (13:30 a 14:30)

                    // Turno Vespertino: Bloques del 7 al 12 (13:30 a 19:30)
                    int[] bloquesVespertinosBase = { 7, 8, 9, 10, 11, 12 };
                    int bloqueExtendidoVespertino = 6; // Hora prestada: Bloque 6 (12:30 a 13:30)

                    Dictionary<int, List<int>> bloquesPorGrupo = new Dictionary<int, List<int>>();

                    foreach (var g in grupos)
                    {
                        bool esMatutino = g.Turno.ToLower().Contains("matutino") || g.Turno.ToLower().StartsWith("m");
                        List<int> bloquesDelGrupo = new List<int>(esMatutino ? bloquesMatutinosBase : bloquesVespertinosBase);

                        int creditosTotales = materias.Where(m => m.Semestre == g.Semestre).Sum(m => m.Creditos);

                        // Si el grupo exige más de 30 horas, habilitamos la hora extra
                        if (creditosTotales > 30)
                        {
                            if (esMatutino)
                                bloquesDelGrupo.Add(bloqueExtendidoMatutino);
                            else
                                bloquesDelGrupo.Insert(0, bloqueExtendidoVespertino);
                        }

                        int limiteHoras = dias.Length * bloquesDelGrupo.Count;
                        if (creditosTotales > limiteHoras)
                        {
                            return $"ERROR BD: El grupo {g.Nombre} exige {creditosTotales} créditos. Incluso con el horario extendido, el máximo posible es {limiteHoras} horas.";
                        }

                        bloquesPorGrupo[g.IdGrupo] = bloquesDelGrupo;
                    }

                    CpModel model = new CpModel();
                    Dictionary<string, BoolVar> asignaciones = new Dictionary<string, BoolVar>();
                    Dictionary<string, BoolVar> grupoHoraActiva = new Dictionary<string, BoolVar>();
                    Dictionary<string, int> materiaDocenteAsignado = new Dictionary<string, int>();
                    List<LinearExpr> penalizaciones = new List<LinearExpr>();

                    // 1. CREAR VARIABLES Y ASIGNACIÓN RELACIONAL DE DOCENTES
                    foreach (var g in grupos)
                    {
                        var materiasDelGrupo = materias.Where(m => m.Semestre == g.Semestre).ToList();
                        var bloquesPermitidos = bloquesPorGrupo[g.IdGrupo];

                        foreach (var d in dias)
                        {
                            foreach (var b in bloquesPermitidos)
                            {
                                grupoHoraActiva[$"{g.IdGrupo}_{d}_{b}"] = model.NewBoolVar($"Activa_{g.IdGrupo}_{d}_{b}");
                            }
                        }

                        foreach (var m in materiasDelGrupo)
                        {
                            var docentesCapaces = docentes.Where(doc => doc.MateriasIds.Contains(m.IdMateria)).ToList();

                            Docente docenteElegido = docentesCapaces.Count > 0
                                ? (docentesCapaces.FirstOrDefault(doc => doc.IdCarreraPrincipal == idCarrera) ?? docentesCapaces.First())
                                : docentes.First();

                            materiaDocenteAsignado[$"{g.IdGrupo}_{m.IdMateria}"] = docenteElegido.IdDocente;

                            foreach (var d in dias)
                            {
                                foreach (var b in bloquesPermitidos)
                                {
                                    var variableClase = model.NewBoolVar($"X_{g.IdGrupo}_{m.IdMateria}_{d}_{b}");
                                    asignaciones[$"{g.IdGrupo}_{m.IdMateria}_{d}_{b}"] = variableClase;

                                    if (b == bloqueExtendidoMatutino || b == bloqueExtendidoVespertino)
                                    {
                                        penalizaciones.Add(variableClase * 100);
                                    }
                                }
                            }
                        }
                    }

                    // 2. RESTRICCIONES
                    foreach (var g in grupos)
                    {
                        var materiasDelGrupo = materias.Where(m => m.Semestre == g.Semestre).ToList();
                        var bloquesPermitidos = bloquesPorGrupo[g.IdGrupo];

                        // A. Control de Huecos en Blanco
                        foreach (var d in dias)
                        {
                            foreach (var b in bloquesPermitidos)
                            {
                                var hVar = grupoHoraActiva[$"{g.IdGrupo}_{d}_{b}"];
                                List<BoolVar> clasesEnEsteBloque = new List<BoolVar>();

                                foreach (var m in materiasDelGrupo)
                                {
                                    clasesEnEsteBloque.Add(asignaciones[$"{g.IdGrupo}_{m.IdMateria}_{d}_{b}"]);
                                }

                                model.AddAtMostOne(clasesEnEsteBloque);
                                model.Add(hVar == LinearExpr.Sum(clasesEnEsteBloque));
                            }

                            for (int i = 1; i < bloquesPermitidos.Count - 1; i++)
                            {
                                var prev = grupoHoraActiva[$"{g.IdGrupo}_{d}_{bloquesPermitidos[i - 1]}"];
                                var curr = grupoHoraActiva[$"{g.IdGrupo}_{d}_{bloquesPermitidos[i]}"];
                                var next = grupoHoraActiva[$"{g.IdGrupo}_{d}_{bloquesPermitidos[i + 1]}"];

                                model.AddBoolOr(new ILiteral[] { prev.Not(), curr, next.Not() });
                            }
                        }

                        // B. JUNTAR MATERIAS (Consecutivas) y CUMPLIR CRÉDITOS
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

                                    if (i == 0)
                                    {
                                        model.Add(is_start == x_curr);
                                    }
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
                                model.Add(LinearExpr.Sum(horasEsteDia) <= 2);
                            }
                            model.Add(LinearExpr.Sum(todasHorasMateria) == m.Creditos);
                        }
                    }

                    // 3. OBJETIVO
                    model.Minimize(LinearExpr.Sum(penalizaciones));

                    CpSolver solver = new CpSolver();
                    solver.StringParameters = "max_time_in_seconds: 25.0";
                    CpSolverStatus status = solver.Solve(model);

                    if (status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible)
                    {
                        // AQUÍ PASAMOS EL idCarrera A LA FUNCIÓN DE GUARDADO
                        int insertados = GuardarResultadosEnDB(idProyecto, idCarrera, grupos, materias, dias, bloquesPorGrupo, materiaDocenteAsignado, asignaciones, solver);
                        return $"EXITO: Horario generado sin dispersión. Se insertaron {insertados} registros. Costo extendido: {solver.ObjectiveValue}";
                    }
                    else if (status == CpSolverStatus.Infeasible)
                    {
                        return "ERROR: Con las reglas actuales es imposible acomodar el horario. Revisa tu BD.";
                    }

                    return $"ERROR: {status} - Tiempo agotado.";
                }
                catch (Exception ex)
                {
                    return $"EXCEPCIÓN CRÍTICA: {ex.Message}\n\nStack: {ex.StackTrace}";
                }
            });
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

            using (var cmd = new SqlCommand("sp_GetDocentesByCarrera", conn))
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
                        Nombre = reader["NombreCompleto"].ToString() ?? "",
                        TipoTiempo = reader["TipoTiempo"].ToString() ?? "",
                        HorasMaximas = reader["HorasMaximas"] != DBNull.Value ? Convert.ToInt32(reader["HorasMaximas"]) : 20,
                        IdCarreraPrincipal = reader["IdCarreraPrincipal"] != DBNull.Value ? Convert.ToInt32(reader["IdCarreraPrincipal"]) : 0,
                        CarreraPrincipal = reader["CarreraPrincipal"].ToString() ?? "",
                        HorasAsignadas = reader["HorasAsignadas"] != DBNull.Value ? Convert.ToInt32(reader["HorasAsignadas"]) : 0,
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
                        if (dictDocentes.ContainsKey(idDoc))
                        {
                            dictDocentes[idDoc].MateriasIds.Add(idMat);
                        }
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
                        if (dictDocentes.ContainsKey(idDoc))
                        {
                            dictDocentes[idDoc].DisponibilidadBloques.Add(bloque);
                        }
                    }
                }
            }

            return dictDocentes.Values.ToList();
        }

        // AQUÍ RECIBIMOS EL idCarrera PARA BORRAR SOLO LOS DATOS PERTINENTES
        private int GuardarResultadosEnDB(int idProyecto, int idCarrera, List<Grupo> grupos, List<Materia> materias, int[] dias, Dictionary<int, List<int>> bloquesPorGrupo, Dictionary<string, int> materiaDocenteAsignado, Dictionary<string, BoolVar> asignaciones, CpSolver solver)
        {
            int contadorInsert = 0;
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            conn.Open();

            // CONSULTA CORREGIDA: Usa INNER JOIN para borrar solo los horarios de los grupos que pertenecen a la carrera actual
            using (var cmdDelete = new SqlCommand(@"
                DELETE hd 
                FROM HorarioDetalle hd
                INNER JOIN Grupos g ON hd.id_grupo = g.id_grupo
                WHERE hd.id_proyecto = @idProj AND g.id_carrera = @idCarrera", conn))
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