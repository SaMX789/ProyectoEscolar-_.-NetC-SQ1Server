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
            public int? IdCarreraSecundaria { get; set; }
            public int? IdCarreraTerciaria { get; set; }
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

                    // Cargar Salones (AHORA INCLUYE CARRERA SECUNDARIA Y TERCIARIA)
                    using (var cmd = new SqlCommand(@"
                        SELECT id_salon, capacidad, id_carrera, id_carreraSecundaria, id_carreraTerciaria, nombre 
                        FROM Salones", conn))
                    {
                        using var reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            salones.Add(new EspacioFisico
                            {
                                IdSalon = Convert.ToInt32(reader["id_salon"]),
                                Capacidad = Convert.ToInt32(reader["capacidad"]),
                                IdCarrera = reader["id_carrera"] != DBNull.Value ? Convert.ToInt32(reader["id_carrera"]) : null,
                                IdCarreraSecundaria = reader["id_carreraSecundaria"] != DBNull.Value ? Convert.ToInt32(reader["id_carreraSecundaria"]) : null,
                                IdCarreraTerciaria = reader["id_carreraTerciaria"] != DBNull.Value ? Convert.ToInt32(reader["id_carreraTerciaria"]) : null,
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
                        bool esMateriaPractica = c.NombreMateria.ToLower().Contains("programacion") ||
                                                 c.NombreMateria.ToLower().Contains("base de datos") ||
                                                 c.NombreMateria.ToLower().Contains("sistemas");

                        bool esLaboratorio = s.Nombre.ToLower().Contains("cc") || s.Nombre.ToLower().Contains("lab");

                        if (esMateriaPractica && !esLaboratorio) model.Add(variable == 0);
                        if (!esMateriaPractica && esLaboratorio) model.Add(variable == 0);

                        // -- NUEVA LÓGICA: FILTRO ESTRICTO DE CARRERAS --
                        bool salonCompartido = !s.IdCarrera.HasValue && !s.IdCarreraSecundaria.HasValue && !s.IdCarreraTerciaria.HasValue;
                        bool coincideCarrera = s.IdCarrera == c.IdCarreraGrupo ||
                                               s.IdCarreraSecundaria == c.IdCarreraGrupo ||
                                               s.IdCarreraTerciaria == c.IdCarreraGrupo;

                        // Si el salón NO es compartido y NO coincide con la carrera del grupo, bloqueamos el acceso
                        if (!salonCompartido && !coincideCarrera)
                        {
                            model.Add(variable == 0);
                        }
                        else if (coincideCarrera)
                        {
                            // Premio fuerte si logra meterlos en un salón específico de su carrera
                            objetivo.AddTerm(variable, 100);
                        }
                        else if (salonCompartido)
                        {
                            // Premio menor si usan un salón compartido
                            objetivo.AddTerm(variable, 20);
                        }
                    }

                    // RESTRICCIÓN DURA: Cada clase DEBE tener asignado exactamente 1 salón
                    model.Add(LinearExpr.Sum(posiblesSalonesDeEstaClase) == 1);
                }

                // 3. RESTRICCIÓN DURA: CERO COLISIONES DE ESPACIO (Ningún grupo comparte salón al mismo tiempo)
                var clasesPorMomento = clases.GroupBy(c => new { c.IdDia, c.IdBloque });
                foreach (var momento in clasesPorMomento)
                {
                    foreach (var s in salones)
                    {
                        var clasesAqui = momento.Select(c => varsSalon[(c.IdDetalle, s.IdSalon)]).ToList();
                        model.Add(LinearExpr.Sum(clasesAqui) <= 1);
                    }
                }

                // 4. NUEVA LÓGICA: ASIGNACIÓN DE "SALÓN BASE" (Minimizar la fragmentación)
                var grupos = clases.Select(c => c.IdGrupo).Distinct();
                foreach (var idGrupo in grupos)
                {
                    var clasesDelGrupo = clases.Where(c => c.IdGrupo == idGrupo).ToList();

                    foreach (var s in salones)
                    {
                        // Variable booleana: ¿El grupo usa este salón al menos una vez?
                        var grupoUsaSalon = model.NewBoolVar($"grupo_{idGrupo}_usa_salon_{s.IdSalon}");
                        var clasesEnEsteSalon = clasesDelGrupo.Select(c => varsSalon[(c.IdDetalle, s.IdSalon)]).ToList();

                        // Si alguna clase del grupo se da en este salón, grupoUsaSalon se vuelve TRUE (1)
                        foreach (var varClase in clasesEnEsteSalon)
                        {
                            model.AddImplication(varClase, grupoUsaSalon);
                        }

                        // Castigamos fuertemente al solver por cada salón DIFERENTE que le abra a un grupo.
                        // Esto fuerza a la IA a meter todas las clases del grupo en el MISMO salón.
                        // (Nota: Castigamos menos a los CC/Labs para permitir que migren ahí sin romper el solver)
                        bool esLaboratorio = s.Nombre.ToLower().Contains("cc") || s.Nombre.ToLower().Contains("lab");
                        int castigoPorAbrirSalon = esLaboratorio ? 500 : 5000;

                        objetivo.AddTerm(grupoUsaSalon, -castigoPorAbrirSalon);
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
                                        break;
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
    }
}