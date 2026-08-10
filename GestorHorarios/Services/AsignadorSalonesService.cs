using Google.OrTools.Sat;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GestorHorarios.Services
{
    public class AsignadorSalonesService
    {
        // Clases de transferencia de datos internas
        private class ClaseAsignada
        {
            public int IdDetalle { get; set; }
            public int IdGrupo { get; set; }
            public int IdCarreraGrupo { get; set; }
            public int IdDia { get; set; }
            public int IdBloque { get; set; }
            public string NombreMateria { get; set; } = "";
        }

        private class EspacioFisico
        {
            public int IdSalon { get; set; }
            public int Capacidad { get; set; }
            public int? IdCarrera { get; set; }
            public string Nombre { get; set; } = "";
        }

        public async Task<bool> EjecutarAsignacionSalonesAsync(int idProyecto)
        {
            return await Task.Run(() =>
            {
                var clases = new List<ClaseAsignada>();
                var salones = new List<EspacioFisico>();

                // 1. CARGAR DATOS DESDE SQL SERVER
                using (var conn = new SqlConnection(DatabaseService.GetConnectionString()))
                {
                    conn.Open();

                    // Cargar Clases Activas (Fase 1 ya completada)
                    using (var cmd = new SqlCommand(@"
                        SELECT hd.id_detalle, hd.id_grupo, g.id_carrera, hd.id_dia, hd.id_bloque, m.nombre
                        FROM HorarioDetalle hd
                        INNER JOIN Grupos g ON hd.id_grupo = g.id_grupo
                        INNER JOIN Materias m ON hd.id_materia = m.id_materia
                        WHERE hd.id_proyecto = @idProyecto", conn))
                    {
                        cmd.Parameters.AddWithValue("@idProyecto", idProyecto);
                        using var reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            clases.Add(new ClaseAsignada
                            {
                                IdDetalle = Convert.ToInt32(reader["id_detalle"]),
                                IdGrupo = Convert.ToInt32(reader["id_grupo"]),
                                IdCarreraGrupo = Convert.ToInt32(reader["id_carrera"]),
                                IdDia = Convert.ToInt32(reader["id_dia"]),
                                IdBloque = Convert.ToInt32(reader["id_bloque"]),
                                NombreMateria = reader["nombre"].ToString() ?? ""
                            });
                        }
                    }

                    // Cargar Salones
                    using (var cmd = new SqlCommand("SELECT id_salon, capacidad, id_carrera, nombre FROM Salones", conn))
                    {
                        using var reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            salones.Add(new EspacioFisico
                            {
                                IdSalon = Convert.ToInt32(reader["id_salon"]),
                                Capacidad = Convert.ToInt32(reader["capacidad"]),
                                IdCarrera = reader["id_carrera"] != DBNull.Value ? Convert.ToInt32(reader["id_carrera"]) : null,
                                Nombre = reader["nombre"].ToString() ?? ""
                            });
                        }
                    }
                }

                if (clases.Count == 0 || salones.Count == 0) return false;

                // 2. INICIALIZAR EL MODELO DE OR-TOOLS
                var model = new CpModel();
                var varsSalon = new Dictionary<(int idDetalle, int idSalon), BoolVar>();
                var objetivo = LinearExpr.NewBuilder();

                // Crear matriz de variables booleanas X[clase, salon]
                foreach (var c in clases)
                {
                    var posiblesSalonesDeEstaClase = new List<BoolVar>();
                    foreach (var s in salones)
                    {
                        var variable = model.NewBoolVar($"s_{c.IdDetalle}_{s.IdSalon}");
                        varsSalon[(c.IdDetalle, s.IdSalon)] = variable;
                        posiblesSalonesDeEstaClase.Add(variable);

                        // -- LÓGICA DE LABORATORIOS (Centros de Cómputo) --
                        // Si la materia es técnica, forzamos a que solo use salones con 'cc' o 'lab' en el nombre
                        bool esMateriaPractica = c.NombreMateria.ToLower().Contains("programacion") ||
                                                 c.NombreMateria.ToLower().Contains("base de datos") ||
                                                 c.NombreMateria.ToLower().Contains("sistemas");

                        bool esLaboratorio = s.Nombre.ToLower().Contains("cc") || s.Nombre.ToLower().Contains("lab");

                        if (esMateriaPractica && !esLaboratorio) model.Add(variable == 0);
                        if (!esMateriaPractica && esLaboratorio) model.Add(variable == 0); // Opcional: proteger labs de clases teóricas

                        // -- PREMIOS POR EDIFICIO BASE --
                        if (s.IdCarrera.HasValue && s.IdCarrera.Value == c.IdCarreraGrupo)
                        {
                            // +50 puntos si Sistemas queda en el Edificio J, Civil en K, etc.
                            objetivo.AddTerm(variable, 50);
                        }
                        else if (!s.IdCarrera.HasValue)
                        {
                            // +20 puntos si usan un edificio compartido (Comodín)
                            objetivo.AddTerm(variable, 20);
                        }
                    }

                    // RESTRICCIÓN DURA: Cada clase DEBE tener asignado exactamente 1 salón
                    model.Add(LinearExpr.Sum(posiblesSalonesDeEstaClase) == 1);
                }

                // 3. RESTRICCIÓN DURA: CERO COLISIONES DE ESPACIO
                var clasesPorMomento = clases.GroupBy(c => new { c.IdDia, c.IdBloque });
                foreach (var momento in clasesPorMomento)
                {
                    foreach (var s in salones)
                    {
                        var clasesAqui = momento.Select(c => varsSalon[(c.IdDetalle, s.IdSalon)]).ToList();
                        // En este día, en este bloque, la suma de clases en este salón debe ser <= 1
                        model.Add(LinearExpr.Sum(clasesAqui) <= 1);
                    }
                }

                // 4. PENALIZACIÓN DINÁMICA POR MIGRACIÓN (El algoritmo anti-rezagados)
                var clasesPorGrupoYDia = clases
                    .GroupBy(c => new { c.IdGrupo, c.IdDia })
                    .Select(g => g.OrderBy(c => c.IdBloque).ToList());

                foreach (var rutaDiaria in clasesPorGrupoYDia)
                {
                    for (int i = 0; i < rutaDiaria.Count - 1; i++)
                    {
                        var claseActual = rutaDiaria[i];
                        var claseSiguiente = rutaDiaria[i + 1];

                        // Si las clases son bloques consecutivos
                        if (claseSiguiente.IdBloque == claseActual.IdBloque + 1)
                        {
                            int penalizacion = ObtenerPenalizacionPorCambio(claseSiguiente.IdBloque);
                            var variablesMantenimientoSalon = new List<BoolVar>();

                            foreach (var s in salones)
                            {
                                // Esta variable mágica (ambasEnS) solo será 1 si el grupo estuvo en el salón 's' a las 8am Y a las 9am
                                var ambasEnS = model.NewBoolVar($"ambas_{claseActual.IdDetalle}_{claseSiguiente.IdDetalle}_s{s.IdSalon}");

                                model.AddBoolAnd(new[] { varsSalon[(claseActual.IdDetalle, s.IdSalon)], varsSalon[(claseSiguiente.IdDetalle, s.IdSalon)] }).OnlyEnforceIf(ambasEnS);
                                model.AddBoolOr(new[] { varsSalon[(claseActual.IdDetalle, s.IdSalon)].Not(), varsSalon[(claseSiguiente.IdDetalle, s.IdSalon)].Not() }).OnlyEnforceIf(ambasEnS.Not());

                                variablesMantenimientoSalon.Add(ambasEnS);
                            }

                            // Variable booleana final que indica si el grupo cambió de salón
                            var cambioSalon = model.NewBoolVar($"cambio_salon_{claseActual.IdDetalle}");
                            var seQuedoEnMismoSalon = model.NewBoolVar($"mismo_salon_{claseActual.IdDetalle}");

                            model.Add(LinearExpr.Sum(variablesMantenimientoSalon) == seQuedoEnMismoSalon);
                            model.Add(cambioSalon + seQuedoEnMismoSalon == 1);

                            // Restar los puntos a la función objetivo si el solver decide hacerlos caminar a otro salón
                            objetivo.AddTerm(cambioSalon, -penalizacion);
                        }
                    }
                }

                // 5. RESOLVER EL MODELO
                model.Maximize(objetivo);
                var solver = new CpSolver();
                solver.StringParameters = "max_time_in_seconds: 15.0"; // Tiempo máximo de búsqueda

                var status = solver.Solve(model);

                if (status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible)
                {
                    // 6. GUARDAR RESULTADOS EN LA BASE DE DATOS
                    using (var conn = new SqlConnection(DatabaseService.GetConnectionString()))
                    {
                        conn.Open();
                        using var transaction = conn.BeginTransaction();
                        try
                        {
                            foreach (var c in clases)
                            {
                                foreach (var s in salones)
                                {
                                    if (solver.BooleanValue(varsSalon[(c.IdDetalle, s.IdSalon)]))
                                    {
                                        using var cmdUpdate = new SqlCommand(
                                            "UPDATE HorarioDetalle SET id_salon = @idSalon WHERE id_detalle = @idDetalle", conn, transaction);
                                        cmdUpdate.Parameters.AddWithValue("@idSalon", s.IdSalon);
                                        cmdUpdate.Parameters.AddWithValue("@idDetalle", c.IdDetalle);
                                        cmdUpdate.ExecuteNonQuery();
                                        break; // Encontramos el salón asignado, pasamos a la siguiente clase
                                    }
                                }
                            }
                            transaction.Commit();
                            return true;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
                return false;
            });
        }

        // Multiplicador de castigo matemático basado en la hora del día
        private int ObtenerPenalizacionPorCambio(int idBloqueSiguiente)
        {
            // Bloques de la mañana
            if (idBloqueSiguiente <= 2) return 5;    // Cambiar temprano (ej. 8:30) molesta poco
            if (idBloqueSiguiente <= 4) return 20;   // Media mañana, penalización moderada
            if (idBloqueSiguiente == 5) return 200;  // 11:30 AM (Prohibido casi por completo)
            if (idBloqueSiguiente >= 6 && idBloqueSiguiente <= 7) return 500; // Final del matutino (Muro matemático)

            // Bloques de la tarde
            if (idBloqueSiguiente <= 9) return 5;    // Entrando a la tarde, molesta poco
            if (idBloqueSiguiente <= 11) return 20;  // Media tarde
            if (idBloqueSiguiente >= 12) return 500; // 6:30 PM en adelante, castigo severo

            return 10;
        }
    }
}