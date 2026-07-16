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
                        docentes.Add(new Docente { IdDocente = 1, Nombre = "Sin Maestro Asignado" });
                    }

                    int[] dias = { 1, 2, 3, 4, 5 };
                    int[] bloques = { 0, 1, 3, 4, 5, 6 }; // Son 6 bloques diarios = 30 semanales

                    // ESCÁNER DE EXCESO DE CRÉDITOS (Te dirá el error exacto en Civil/Industrial)
                    foreach (var g in grupos)
                    {
                        var mats = materias.Where(m => m.Semestre == g.Semestre).ToList();
                        int creditosTotales = mats.Sum(m => m.Creditos);
                        int limiteHoras = dias.Length * bloques.Length; // 30 horas

                        if (creditosTotales > limiteHoras)
                        {
                            return $"ERROR BD: El grupo {g.Nombre} exige {creditosTotales} créditos, pero el turno matutino solo tiene {limiteHoras} horas en total. ¡Debes bajar los créditos en la BD o agregar una hora más de clase (ej. 13:30 - 14:30)!";
                        }
                    }

                    CpModel model = new CpModel();
                    Dictionary<string, BoolVar> asignaciones = new Dictionary<string, BoolVar>();
                    Dictionary<string, BoolVar> grupoHoraActiva = new Dictionary<string, BoolVar>();
                    Dictionary<string, int> materiaDocenteAsignado = new Dictionary<string, int>();

                    // 1. CREAR VARIABLES
                    foreach (var g in grupos)
                    {
                        var materiasDelGrupo = materias.Where(m => m.Semestre == g.Semestre).ToList();

                        foreach (var d in dias)
                        {
                            foreach (var b in bloques)
                            {
                                grupoHoraActiva[$"{g.IdGrupo}_{d}_{b}"] = model.NewBoolVar($"Activa_{g.IdGrupo}_{d}_{b}");
                            }
                        }

                        foreach (var m in materiasDelGrupo)
                        {
                            int indexDocente = Math.Abs(m.IdMateria + g.IdGrupo) % docentes.Count;
                            materiaDocenteAsignado[$"{g.IdGrupo}_{m.IdMateria}"] = docentes[indexDocente].IdDocente;

                            foreach (var d in dias)
                            {
                                foreach (var b in bloques)
                                {
                                    asignaciones[$"{g.IdGrupo}_{m.IdMateria}_{d}_{b}"] = model.NewBoolVar($"X_{g.IdGrupo}_{m.IdMateria}_{d}_{b}");
                                }
                            }
                        }
                    }

                    // 2. RESTRICCIONES
                    foreach (var g in grupos)
                    {
                        var materiasDelGrupo = materias.Where(m => m.Semestre == g.Semestre).ToList();

                        // A. Control de Huecos en Blanco
                        foreach (var d in dias)
                        {
                            foreach (var b in bloques)
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

                            var h0 = grupoHoraActiva[$"{g.IdGrupo}_{d}_0"];
                            var h1 = grupoHoraActiva[$"{g.IdGrupo}_{d}_1"];
                            var h3 = grupoHoraActiva[$"{g.IdGrupo}_{d}_3"];
                            var h4 = grupoHoraActiva[$"{g.IdGrupo}_{d}_4"];
                            var h5 = grupoHoraActiva[$"{g.IdGrupo}_{d}_5"];
                            var h6 = grupoHoraActiva[$"{g.IdGrupo}_{d}_6"];

                            model.AddBoolOr(new ILiteral[] { h0.Not(), h1, h3.Not() });
                            model.AddBoolOr(new ILiteral[] { h1.Not(), h3, h4.Not() });
                            model.AddBoolOr(new ILiteral[] { h3.Not(), h4, h5.Not() });
                            model.AddBoolOr(new ILiteral[] { h4.Not(), h5, h6.Not() });
                        }

                        // B. JUNTAR MATERIAS (Consecutivas) y CUMPLIR CRÉDITOS
                        foreach (var m in materiasDelGrupo)
                        {
                            List<BoolVar> todasHorasMateria = new List<BoolVar>();

                            foreach (var d in dias)
                            {
                                List<BoolVar> horasEsteDia = new List<BoolVar>();
                                List<BoolVar> starts = new List<BoolVar>();

                                for (int i = 0; i < bloques.Length; i++)
                                {
                                    var b = bloques[i];
                                    var x_curr = asignaciones[$"{g.IdGrupo}_{m.IdMateria}_{d}_{b}"];
                                    horasEsteDia.Add(x_curr);
                                    todasHorasMateria.Add(x_curr);

                                    // Lógica para saber si es el "inicio" de un bloque de clases
                                    var is_start = model.NewBoolVar($"start_{g.IdGrupo}_{m.IdMateria}_{d}_{b}");
                                    starts.Add(is_start);

                                    if (i == 0)
                                    {
                                        model.Add(is_start == x_curr);
                                    }
                                    else
                                    {
                                        var prev_b = bloques[i - 1];
                                        var x_prev = asignaciones[$"{g.IdGrupo}_{m.IdMateria}_{d}_{prev_b}"];

                                        model.AddImplication(is_start, x_curr);
                                        model.AddImplication(is_start, x_prev.Not());
                                        model.AddBoolOr(new ILiteral[] { is_start, x_curr.Not(), x_prev });
                                    }
                                }

                                // 1. Máximo 1 "inicio" por día (Obliga a que si hay 2 horas, estén pegadas)
                                model.Add(LinearExpr.Sum(starts) <= 1);

                                // 2. Máximo 2 horas al día de la misma materia (para que no aburra a los alumnos)
                                model.Add(LinearExpr.Sum(horasEsteDia) <= 2);
                            }

                            // 3. Cumplir la cuota de créditos semanal
                            model.Add(LinearExpr.Sum(todasHorasMateria) == m.Creditos);
                        }
                    }

                    CpSolver solver = new CpSolver();
                    solver.StringParameters = "max_time_in_seconds: 20.0"; // Un poco más de tiempo por la regla de juntas
                    CpSolverStatus status = solver.Solve(model);

                    if (status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible)
                    {
                        int insertados = GuardarResultadosEnDB(idProyecto, grupos, materias, dias, bloques, materiaDocenteAsignado, asignaciones, solver);
                        return $"EXITO: Horario generado sin dispersión. Se insertaron {insertados} registros.";
                    }
                    else if (status == CpSolverStatus.Infeasible)
                    {
                        return "ERROR: Con las reglas actuales es imposible acomodar el horario. Revisa tu BD.";
                    }

                    return $"ERROR: {status} - Tiempo agotado.";
                }
                catch (Exception ex)
                {
                    return $"EXCEPCIÓN CRÍTICA: {ex.Message}";
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
            var docentes = new List<Docente>();
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand("sp_GetDocentesByCarrera", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@id_carrera", idCarrera);

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                docentes.Add(new Docente
                {
                    IdDocente = Convert.ToInt32(reader["id_docente"]),
                    Nombre = reader["NombreCompleto"].ToString() ?? ""
                });
            }
            return docentes;
        }

        private int GuardarResultadosEnDB(int idProyecto, List<Grupo> grupos, List<Materia> materias, int[] dias, int[] bloques, Dictionary<string, int> materiaDocenteAsignado, Dictionary<string, BoolVar> asignaciones, CpSolver solver)
        {
            int contadorInsert = 0;
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            conn.Open();

            using (var cmdDelete = new SqlCommand("DELETE FROM HorarioDetalle WHERE id_proyecto = @id", conn))
            {
                cmdDelete.Parameters.AddWithValue("@id", idProyecto);
                cmdDelete.ExecuteNonQuery();
            }

            foreach (var g in grupos)
            {
                var materiasDelGrupo = materias.Where(m => m.Semestre == g.Semestre).ToList();
                foreach (var m in materiasDelGrupo)
                {
                    foreach (var d in dias)
                    {
                        foreach (var b in bloques)
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