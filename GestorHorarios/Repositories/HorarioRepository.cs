using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using GestorHorarios.Models;
using GestorHorarios.Services;
using Microsoft.Data.SqlClient;

namespace GestorHorarios.Repositories
{
    public sealed record AsignacionSlotSolverDto(int IdGrupo, int IdMateria, int? IdDocente, int IdDia, int IdBloque);

    public class ClaseParaSalonDto
    {
        public int IdDetalle { get; set; }
        public int IdGrupo { get; set; }
        public int IdCarreraGrupo { get; set; }
        public int CupoAlumnos { get; set; } = 35;
        public int IdDia { get; set; }
        public int IdBloque { get; set; }
        public string NombreMateria { get; set; } = "";
        public bool RequiereLaboratorio { get; set; }
        public bool EnLinea { get; set; }
    }

    public class EspacioFisicoSolverDto
    {
        public int IdSalon { get; set; }
        public int Capacidad { get; set; }
        public int? IdCarrera { get; set; }
        public int? IdCarreraSecundaria { get; set; }
        public int? IdCarreraTerciaria { get; set; }
        public string Nombre { get; set; } = "";
        public bool EsLaboratorio { get; set; }
    }

    public interface IHorarioRepository
    {
        Task<List<HorarioAsignado>> ObtenerHorarioGrupoAsync(int idGrupo, int idProyecto);
        List<HorarioAsignado> ObtenerHorarioGrupo(int idGrupo, int idProyecto);
        Task<List<HorarioPersonal>> ObtenerHorarioPersonalDocenteAsync(int idProyecto, int idDocente);
        List<HorarioPersonal> ObtenerHorarioPersonalDocente(int idProyecto, int idDocente);
        Task<Dictionary<string, int>> CargarAsignacionesGrupoMateriaAsync(int idCarrera);
        Task<(Dictionary<int, int> HorasPrevias, Dictionary<int, HashSet<string>> BloquesOcupados)> CargarOcupacionPreviaAsync(
            int idProyecto, int idCarreraActual, List<Docente> docentes);
        Task<int> GuardarResultadosSolverAsync(int idProyecto, int idCarrera, List<AsignacionSlotSolverDto> slots);
        Task<(List<ClaseParaSalonDto> Clases, List<EspacioFisicoSolverDto> Salones)> CargarDatosAsignacionSalonesAsync(int idProyecto);
        Task GuardarAsignacionesSalonesAsync(int idProyecto, Dictionary<int, int> asignacionesPorDetalle);
    }

    public class HorarioRepository : IHorarioRepository
    {
        public async Task<List<HorarioAsignado>> ObtenerHorarioGrupoAsync(int idGrupo, int idProyecto)
        {
            var horarios = new List<HorarioAsignado>();
            await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            await using var cmd = new SqlCommand("sp_GetHorarioDelGrupo", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@idGrupo", SqlDbType.Int).Value = idGrupo;
            cmd.Parameters.Add("@idProyecto", SqlDbType.Int).Value = idProyecto;

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                horarios.Add(new HorarioAsignado
                {
                    DiaSemana = Convert.ToInt32(reader["id_dia"]),
                    BloqueHora = Convert.ToInt32(reader["id_bloque"]),
                    NombreMateria = reader["Materia"].ToString() ?? "",
                    NombreDocente = reader["Docente"]?.ToString() ?? "",
                    IdDocente = reader["id_docente"] != DBNull.Value ? Convert.ToInt32(reader["id_docente"]) : 0,
                    NombreSalon = reader["Salon"]?.ToString() ?? "",
                    NombreEdificio = reader["Edificio"]?.ToString() ?? ""
                });
            }
            return horarios;
        }

        public List<HorarioAsignado> ObtenerHorarioGrupo(int idGrupo, int idProyecto)
        {
            var horarios = new List<HorarioAsignado>();
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand("sp_GetHorarioDelGrupo", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@idGrupo", SqlDbType.Int).Value = idGrupo;
            cmd.Parameters.Add("@idProyecto", SqlDbType.Int).Value = idProyecto;

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                horarios.Add(new HorarioAsignado
                {
                    DiaSemana = Convert.ToInt32(reader["id_dia"]),
                    BloqueHora = Convert.ToInt32(reader["id_bloque"]),
                    NombreMateria = reader["Materia"].ToString() ?? "",
                    NombreDocente = reader["Docente"]?.ToString() ?? "",
                    IdDocente = reader["id_docente"] != DBNull.Value ? Convert.ToInt32(reader["id_docente"]) : 0,
                    NombreSalon = reader["Salon"]?.ToString() ?? "",
                    NombreEdificio = reader["Edificio"]?.ToString() ?? ""
                });
            }
            return horarios;
        }

        public async Task<List<HorarioPersonal>> ObtenerHorarioPersonalDocenteAsync(int idProyecto, int idDocente)
        {
            var horarioPersonal = new List<HorarioPersonal>();
            await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            await using var cmd = new SqlCommand("sp_GetHorarioPersonalDocente", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@id_proyecto", SqlDbType.Int).Value = idProyecto;
            cmd.Parameters.Add("@id_docente", SqlDbType.Int).Value = idDocente;

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                horarioPersonal.Add(new HorarioPersonal
                {
                    Dia = Convert.ToInt32(reader["id_dia"]),
                    Bloque = Convert.ToInt32(reader["id_bloque"]),
                    Materia = reader["NombreMateria"].ToString() ?? "",
                    Grupo = reader["NombreGrupo"].ToString() ?? ""
                });
            }
            return horarioPersonal;
        }

        public List<HorarioPersonal> ObtenerHorarioPersonalDocente(int idProyecto, int idDocente)
        {
            var horarioPersonal = new List<HorarioPersonal>();
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand("sp_GetHorarioPersonalDocente", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@id_proyecto", SqlDbType.Int).Value = idProyecto;
            cmd.Parameters.Add("@id_docente", SqlDbType.Int).Value = idDocente;

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                horarioPersonal.Add(new HorarioPersonal
                {
                    Dia = Convert.ToInt32(reader["id_dia"]),
                    Bloque = Convert.ToInt32(reader["id_bloque"]),
                    Materia = reader["NombreMateria"].ToString() ?? "",
                    Grupo = reader["NombreGrupo"].ToString() ?? ""
                });
            }
            return horarioPersonal;
        }

        public async Task<Dictionary<string, int>> CargarAsignacionesGrupoMateriaAsync(int idCarrera)
        {
            var materiaDocenteAsignado = new Dictionary<string, int>();
            await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            await conn.OpenAsync();

            await using var cmdAsign = new SqlCommand(@"
                SELECT dm.id_materia, dm.id_grupo, dm.id_docente
                FROM DocenteMateria dm
                INNER JOIN Grupos g ON dm.id_grupo = g.id_grupo
                INNER JOIN Docentes d ON dm.id_docente = d.id_docente
                WHERE dm.id_grupo IS NOT NULL
                  AND g.id_carrera = @id_carrera
                  AND d.id_estado = 1", conn);
            cmdAsign.Parameters.Add("@id_carrera", SqlDbType.Int).Value = idCarrera;

            await using var readerAsign = await cmdAsign.ExecuteReaderAsync();
            while (await readerAsign.ReadAsync())
            {
                string key = $"{readerAsign["id_grupo"]}_{readerAsign["id_materia"]}";
                materiaDocenteAsignado[key] = Convert.ToInt32(readerAsign["id_docente"]);
            }
            return materiaDocenteAsignado;
        }

        public async Task<(Dictionary<int, int> HorasPrevias, Dictionary<int, HashSet<string>> BloquesOcupados)> CargarOcupacionPreviaAsync(
            int idProyecto, int idCarreraActual, List<Docente> docentes)
        {
            var horasPrevias = new Dictionary<int, int>();
            var bloquesOcupados = new Dictionary<int, HashSet<string>>();
            foreach (var d in docentes)
            {
                horasPrevias[d.IdDocente] = 0;
                bloquesOcupados[d.IdDocente] = new HashSet<string>();
            }

            await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            await using var cmd = new SqlCommand(@"
                SELECT h.id_docente, h.id_dia, h.id_bloque
                FROM HorarioDetalle h
                INNER JOIN Grupos g ON h.id_grupo = g.id_grupo
                WHERE h.id_proyecto = @idProj
                  AND g.id_carrera != @idCarreraActual
                  AND h.id_docente IS NOT NULL", conn);

            cmd.Parameters.Add("@idProj", SqlDbType.Int).Value = idProyecto;
            cmd.Parameters.Add("@idCarreraActual", SqlDbType.Int).Value = idCarreraActual;
            await conn.OpenAsync();

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                int idDoc = Convert.ToInt32(reader["id_docente"]);
                if (horasPrevias.ContainsKey(idDoc))
                {
                    horasPrevias[idDoc]++;
                    string bloque = $"{reader["id_dia"]}_{reader["id_bloque"]}";
                    bloquesOcupados[idDoc].Add(bloque);
                }
            }

            return (horasPrevias, bloquesOcupados);
        }

        public async Task<int> GuardarResultadosSolverAsync(int idProyecto, int idCarrera, List<AsignacionSlotSolverDto> slots)
        {
            await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            await conn.OpenAsync();
            await using var tx = (SqlTransaction)await conn.BeginTransactionAsync();

            try
            {
                await using (var cmdDelete = new SqlCommand(@"
                    DELETE FROM HorarioDetalle 
                    WHERE id_proyecto = @idProj
                      AND id_grupo IN (SELECT id_grupo FROM Grupos WHERE id_carrera = @idCarrera)", conn, tx))
                {
                    cmdDelete.Parameters.Add("@idProj", SqlDbType.Int).Value = idProyecto;
                    cmdDelete.Parameters.Add("@idCarrera", SqlDbType.Int).Value = idCarrera;
                    await cmdDelete.ExecuteNonQueryAsync();
                }

                await using var cmdInsert = new SqlCommand(@"
                    INSERT INTO HorarioDetalle (id_proyecto, id_grupo, id_materia, id_docente, id_dia, id_bloque, id_salon) 
                    VALUES (@idProj, @idGrupo, @idMat, @idDoc, @dia, @bloque, NULL)", conn, tx);

                var pProj = cmdInsert.Parameters.Add("@idProj", SqlDbType.Int);
                var pGrupo = cmdInsert.Parameters.Add("@idGrupo", SqlDbType.Int);
                var pMat = cmdInsert.Parameters.Add("@idMat", SqlDbType.Int);
                var pDoc = cmdInsert.Parameters.Add("@idDoc", SqlDbType.Int);
                var pDia = cmdInsert.Parameters.Add("@dia", SqlDbType.Int);
                var pBloque = cmdInsert.Parameters.Add("@bloque", SqlDbType.Int);

                pProj.Value = idProyecto;

                int contadorInsert = 0;
                foreach (var slot in slots)
                {
                    pGrupo.Value = slot.IdGrupo;
                    pMat.Value = slot.IdMateria;
                    pDoc.Value = (object?)slot.IdDocente ?? DBNull.Value;
                    pDia.Value = slot.IdDia;
                    pBloque.Value = slot.IdBloque;

                    await cmdInsert.ExecuteNonQueryAsync();
                    contadorInsert++;
                }

                await tx.CommitAsync();
                return contadorInsert;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<(List<ClaseParaSalonDto> Clases, List<EspacioFisicoSolverDto> Salones)> CargarDatosAsignacionSalonesAsync(int idProyecto)
        {
            var clasesTodas = new List<ClaseParaSalonDto>();
            var salones = new List<EspacioFisicoSolverDto>();

            await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            await conn.OpenAsync();

            await using (var cmd = new SqlCommand(@"
                SELECT hd.id_detalle, hd.id_grupo, g.id_carrera,
                       ISNULL(g.cupo_alumnos, 35) AS cupo_alumnos,
                       hd.id_dia, hd.id_bloque, m.nombre,
                       ISNULL(m.requiere_laboratorio, 0) AS requiere_laboratorio,
                       ISNULL(dm.en_linea, 0) AS en_linea
                FROM HorarioDetalle hd
                INNER JOIN Grupos g ON hd.id_grupo = g.id_grupo
                INNER JOIN Materias m ON hd.id_materia = m.id_materia
                LEFT JOIN DocenteMateria dm ON hd.id_grupo = dm.id_grupo AND hd.id_materia = dm.id_materia
                WHERE hd.id_proyecto = @idProyecto", conn))
            {
                cmd.Parameters.Add("@idProyecto", SqlDbType.Int).Value = idProyecto;
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    string nombreMat = reader["nombre"].ToString() ?? "";
                    bool reqLab = Convert.ToBoolean(reader["requiere_laboratorio"]) ||
                                  nombreMat.Contains("programacion", StringComparison.OrdinalIgnoreCase) ||
                                  nombreMat.Contains("programación", StringComparison.OrdinalIgnoreCase) ||
                                  nombreMat.Contains("base de datos", StringComparison.OrdinalIgnoreCase) ||
                                  nombreMat.Contains("sistemas", StringComparison.OrdinalIgnoreCase);

                    clasesTodas.Add(new ClaseParaSalonDto
                    {
                        IdDetalle = Convert.ToInt32(reader["id_detalle"]),
                        IdGrupo = Convert.ToInt32(reader["id_grupo"]),
                        IdCarreraGrupo = Convert.ToInt32(reader["id_carrera"]),
                        CupoAlumnos = Convert.ToInt32(reader["cupo_alumnos"]),
                        IdDia = Convert.ToInt32(reader["id_dia"]),
                        IdBloque = Convert.ToInt32(reader["id_bloque"]),
                        NombreMateria = nombreMat,
                        RequiereLaboratorio = reqLab,
                        EnLinea = Convert.ToBoolean(reader["en_linea"])
                    });
                }
            }

            await using (var cmd = new SqlCommand(@"
                SELECT id_salon, capacidad, id_carrera, id_carreraSecundaria, id_carreraTerciaria, nombre,
                       ISNULL(es_laboratorio, 0) AS es_laboratorio
                FROM Salones", conn))
            {
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    string nombreSalon = reader["nombre"].ToString() ?? "";
                    bool esLab = Convert.ToBoolean(reader["es_laboratorio"]) ||
                                 nombreSalon.Contains("cc", StringComparison.OrdinalIgnoreCase) ||
                                 nombreSalon.Contains("lab", StringComparison.OrdinalIgnoreCase);

                    salones.Add(new EspacioFisicoSolverDto
                    {
                        IdSalon = Convert.ToInt32(reader["id_salon"]),
                        Capacidad = Convert.ToInt32(reader["capacidad"]),
                        IdCarrera = reader["id_carrera"] != DBNull.Value ? Convert.ToInt32(reader["id_carrera"]) : null,
                        IdCarreraSecundaria = reader["id_carreraSecundaria"] != DBNull.Value ? Convert.ToInt32(reader["id_carreraSecundaria"]) : null,
                        IdCarreraTerciaria = reader["id_carreraTerciaria"] != DBNull.Value ? Convert.ToInt32(reader["id_carreraTerciaria"]) : null,
                        Nombre = nombreSalon,
                        EsLaboratorio = esLab
                    });
                }
            }

            return (clasesTodas, salones);
        }

        public async Task GuardarAsignacionesSalonesAsync(int idProyecto, Dictionary<int, int> asignacionesPorDetalle)
        {
            await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            await conn.OpenAsync();
            await using var transaction = (SqlTransaction)await conn.BeginTransactionAsync();

            try
            {
                await using (var cmdReset = new SqlCommand(
                    "UPDATE HorarioDetalle SET id_salon = NULL WHERE id_proyecto = @idProyecto", conn, transaction))
                {
                    cmdReset.Parameters.Add("@idProyecto", SqlDbType.Int).Value = idProyecto;
                    await cmdReset.ExecuteNonQueryAsync();
                }

                await using var cmdUpdate = new SqlCommand(
                    "UPDATE HorarioDetalle SET id_salon = @idSalon WHERE id_detalle = @idDetalle", conn, transaction);

                var pSalon = cmdUpdate.Parameters.Add("@idSalon", SqlDbType.Int);
                var pDetalle = cmdUpdate.Parameters.Add("@idDetalle", SqlDbType.Int);

                foreach (var kvp in asignacionesPorDetalle)
                {
                    pDetalle.Value = kvp.Key;
                    pSalon.Value = kvp.Value;
                    await cmdUpdate.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}

