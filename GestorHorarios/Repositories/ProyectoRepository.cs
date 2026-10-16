using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using GestorHorarios.Models;
using GestorHorarios.Services;
using Microsoft.Data.SqlClient;

namespace GestorHorarios.Repositories
{
    public record EstadisticasDashboard(
        int TotalDocentes,
        int TotalMaterias,
        int TotalGrupos,
        int TotalSalones,
        int TotalProyectos);

    public class AsignaturaCoberturaDto
    {
        public string NombreMateria { get; set; } = "";
        public string DocenteAsignado { get; set; } = "";
        public bool EstaAsignado => !string.IsNullOrEmpty(DocenteAsignado);
    }

    public class GrupoCoberturaDto
    {
        public int IdGrupo { get; set; }
        public string NombreGrupo { get; set; } = "";
        public int Semestre { get; set; }
        public List<AsignaturaCoberturaDto> Materias { get; set; } = new();
        public int Faltantes => Materias.Count(m => !m.EstaAsignado);
    }

    public class CarreraCoberturaDto
    {
        public int IdCarrera { get; set; }
        public string NombreCarrera { get; set; } = "";
        public List<GrupoCoberturaDto> Grupos { get; set; } = new();
        public int TotalFaltantes => Grupos.Sum(g => g.Faltantes);
    }

    public interface IProyectoRepository
    {
        Task<EstadisticasDashboard> ObtenerEstadisticasDashboardAsync();
        EstadisticasDashboard ObtenerEstadisticasDashboard();
        Task<List<Proyecto>> ObtenerProyectosActivosAsync();
        List<Proyecto> ObtenerProyectosActivos();
        Task<int> CrearProyectoAsync(string nombre, int anio, string periodo, string ciclo);
        int CrearProyecto(string nombre, int anio, string periodo, string ciclo);
        Task EliminarProyectoAsync(int idProyecto);
        void EliminarProyecto(int idProyecto);
        Task<string> ObtenerCicloProyectoAsync(int idProyecto);
        Task<bool> TieneHorarioGeneradoAsync(int idProyecto, int idCarrera);
        bool TieneHorarioGenerado(int idProyecto, int idCarrera);
        Task<List<CarreraCoberturaDto>> ObtenerDiagnosticoCoberturaAsync(string ciclo);
        List<CarreraCoberturaDto> ObtenerDiagnosticoCobertura(string ciclo);
    }

    public class ProyectoRepository : IProyectoRepository
    {
        public async Task<EstadisticasDashboard> ObtenerEstadisticasDashboardAsync()
        {
            try
            {
                await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                await using var cmd = new SqlCommand("sp_EstadisticasDashboard", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                await conn.OpenAsync();
                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new EstadisticasDashboard(
                        TotalDocentes: Convert.ToInt32(reader["TotalDocentes"]),
                        TotalMaterias: Convert.ToInt32(reader["TotalMaterias"]),
                        TotalGrupos: Convert.ToInt32(reader["TotalGrupos"]),
                        TotalSalones: Convert.ToInt32(reader["TotalSalones"]),
                        TotalProyectos: Convert.ToInt32(reader["TotalProyectos"])
                    );
                }
            }
            catch
            {
                // Si falla la BD, retornar en 0
            }
            return new EstadisticasDashboard(0, 0, 0, 0, 0);
        }

        public EstadisticasDashboard ObtenerEstadisticasDashboard()
        {
            try
            {
                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                using var cmd = new SqlCommand("sp_EstadisticasDashboard", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                conn.Open();
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new EstadisticasDashboard(
                        TotalDocentes: Convert.ToInt32(reader["TotalDocentes"]),
                        TotalMaterias: Convert.ToInt32(reader["TotalMaterias"]),
                        TotalGrupos: Convert.ToInt32(reader["TotalGrupos"]),
                        TotalSalones: Convert.ToInt32(reader["TotalSalones"]),
                        TotalProyectos: Convert.ToInt32(reader["TotalProyectos"])
                    );
                }
            }
            catch
            {
                // Si falla la BD, retornar en 0
            }
            return new EstadisticasDashboard(0, 0, 0, 0, 0);
        }

        public async Task<List<Proyecto>> ObtenerProyectosActivosAsync()
        {
            var lista = new List<Proyecto>();
            await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            await using var cmd = new SqlCommand("sp_ObtenerProyectos", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(MapProyecto(reader));
            }
            return lista;
        }

        public List<Proyecto> ObtenerProyectosActivos()
        {
            var lista = new List<Proyecto>();
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand("sp_ObtenerProyectos", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(MapProyecto(reader));
            }
            return lista;
        }

        public async Task<int> CrearProyectoAsync(string nombre, int anio, string periodo, string ciclo)
        {
            await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            await using var cmd = new SqlCommand(@"
                INSERT INTO Proyectos (nombre, anio, periodo, ciclo, fecha_creacion, id_estado)
                VALUES (@nombre, @anio, @periodo, @ciclo, GETDATE(), 1);
                SELECT SCOPE_IDENTITY();", conn);

            cmd.Parameters.Add("@nombre", SqlDbType.VarChar, 100).Value = nombre;
            cmd.Parameters.Add("@anio", SqlDbType.Int).Value = anio;
            cmd.Parameters.Add("@periodo", SqlDbType.NVarChar, 100).Value = periodo;
            cmd.Parameters.Add("@ciclo", SqlDbType.Char, 1).Value = ciclo;
            await conn.OpenAsync();
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public int CrearProyecto(string nombre, int anio, string periodo, string ciclo)
        {
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand(@"
                INSERT INTO Proyectos (nombre, anio, periodo, ciclo, fecha_creacion, id_estado)
                VALUES (@nombre, @anio, @periodo, @ciclo, GETDATE(), 1);
                SELECT SCOPE_IDENTITY();", conn);

            cmd.Parameters.Add("@nombre", SqlDbType.VarChar, 100).Value = nombre;
            cmd.Parameters.Add("@anio", SqlDbType.Int).Value = anio;
            cmd.Parameters.Add("@periodo", SqlDbType.NVarChar, 100).Value = periodo;
            cmd.Parameters.Add("@ciclo", SqlDbType.Char, 1).Value = ciclo;
            conn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public async Task EliminarProyectoAsync(int idProyecto)
        {
            await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            await conn.OpenAsync();
            await using var tx = (SqlTransaction)await conn.BeginTransactionAsync();
            try
            {
                await using (var cmd = new SqlCommand("DELETE FROM HorarioDetalle WHERE id_proyecto = @id", conn, tx))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = idProyecto;
                    await cmd.ExecuteNonQueryAsync();
                }
                await using (var cmd = new SqlCommand("DELETE FROM Proyectos WHERE id_proyecto = @id", conn, tx))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = idProyecto;
                    await cmd.ExecuteNonQueryAsync();
                }
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public void EliminarProyecto(int idProyecto)
        {
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            conn.Open();
            using var tx = conn.BeginTransaction();
            try
            {
                using (var cmd = new SqlCommand("DELETE FROM HorarioDetalle WHERE id_proyecto = @id", conn, tx))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = idProyecto;
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = new SqlCommand("DELETE FROM Proyectos WHERE id_proyecto = @id", conn, tx))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = idProyecto;
                    cmd.ExecuteNonQuery();
                }
                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<string> ObtenerCicloProyectoAsync(int idProyecto)
        {
            try
            {
                await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                await using var cmd = new SqlCommand("SELECT ciclo FROM Proyectos WHERE id_proyecto = @id", conn);
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idProyecto;
                await conn.OpenAsync();
                var res = await cmd.ExecuteScalarAsync();
                return res?.ToString() ?? "A";
            }
            catch
            {
                return "A";
            }
        }

        public async Task<bool> TieneHorarioGeneradoAsync(int idProyecto, int idCarrera)
        {
            try
            {
                await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                await using var cmd = new SqlCommand(@"
                    SELECT COUNT(*) 
                    FROM HorarioDetalle hd
                    INNER JOIN Grupos g ON hd.id_grupo = g.id_grupo
                    WHERE hd.id_proyecto = @idProyecto AND g.id_carrera = @idCarrera", conn);

                cmd.Parameters.Add("@idProyecto", SqlDbType.Int).Value = idProyecto;
                cmd.Parameters.Add("@idCarrera", SqlDbType.Int).Value = idCarrera;

                await conn.OpenAsync();
                int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                return count > 0;
            }
            catch
            {
                return false;
            }
        }

        public bool TieneHorarioGenerado(int idProyecto, int idCarrera)
        {
            try
            {
                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                using var cmd = new SqlCommand(@"
                    SELECT COUNT(*) 
                    FROM HorarioDetalle hd
                    INNER JOIN Grupos g ON hd.id_grupo = g.id_grupo
                    WHERE hd.id_proyecto = @idProyecto AND g.id_carrera = @idCarrera", conn);

                cmd.Parameters.Add("@idProyecto", SqlDbType.Int).Value = idProyecto;
                cmd.Parameters.Add("@idCarrera", SqlDbType.Int).Value = idCarrera;

                conn.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<CarreraCoberturaDto>> ObtenerDiagnosticoCoberturaAsync(string ciclo)
        {
            var carrerasDict = new Dictionary<int, CarreraCoberturaDto>();
            await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            await using var cmd = new SqlCommand("sp_DiagnosticoCobertura", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@Ciclo", SqlDbType.Char, 1).Value = ciclo;
            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                AcumularFilaCobertura(carrerasDict, reader);
            }
            return carrerasDict.Values.OrderBy(c => c.NombreCarrera).ToList();
        }

        public List<CarreraCoberturaDto> ObtenerDiagnosticoCobertura(string ciclo)
        {
            var carrerasDict = new Dictionary<int, CarreraCoberturaDto>();
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand("sp_DiagnosticoCobertura", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@Ciclo", SqlDbType.Char, 1).Value = ciclo;
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                AcumularFilaCobertura(carrerasDict, reader);
            }
            return carrerasDict.Values.OrderBy(c => c.NombreCarrera).ToList();
        }

        private static void AcumularFilaCobertura(Dictionary<int, CarreraCoberturaDto> carrerasDict, SqlDataReader reader)
        {
            int idCarrera = Convert.ToInt32(reader["id_carrera"]);
            if (!carrerasDict.TryGetValue(idCarrera, out var carrera))
            {
                carrera = new CarreraCoberturaDto
                {
                    IdCarrera = idCarrera,
                    NombreCarrera = reader["NombreCarrera"].ToString() ?? ""
                };
                carrerasDict[idCarrera] = carrera;
            }

            int idGrupo = Convert.ToInt32(reader["id_grupo"]);
            var grupo = carrera.Grupos.FirstOrDefault(g => g.IdGrupo == idGrupo);
            if (grupo == null)
            {
                grupo = new GrupoCoberturaDto
                {
                    IdGrupo = idGrupo,
                    NombreGrupo = reader["NombreGrupo"].ToString() ?? "",
                    Semestre = Convert.ToInt32(reader["Semestre"])
                };
                carrera.Grupos.Add(grupo);
            }

            grupo.Materias.Add(new AsignaturaCoberturaDto
            {
                NombreMateria = reader["NombreMateria"].ToString() ?? "",
                DocenteAsignado = reader["DocenteAsignado"]?.ToString() ?? ""
            });
        }

        private static Proyecto MapProyecto(SqlDataReader reader) => new()
        {
            IdProyecto = Convert.ToInt32(reader["id_proyecto"]),
            Nombre = reader["nombre"].ToString() ?? "",
            Anio = Convert.ToInt32(reader["anio"]),
            Periodo = reader["periodo"].ToString() ?? "",
            Ciclo = reader["ciclo"].ToString() ?? "",
            FechaCreacion = Convert.ToDateTime(reader["fecha_creacion"])
        };
    }
}

