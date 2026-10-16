using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using GestorHorarios.Models;
using GestorHorarios.Services;
using Microsoft.Data.SqlClient;

namespace GestorHorarios.Repositories
{
    public class GrupoDisponibleMateriaDto
    {
        public int IdGrupo { get; set; }
        public string NombreGrupo { get; set; } = string.Empty;
        public string Turno { get; set; } = string.Empty;
        public bool EsEspecial { get; set; }
        public int? IdDocenteAsignado { get; set; }
        public string NombreDocenteAsignado { get; set; } = string.Empty;
    }

    public interface IGrupoRepository
    {
        Task<List<(int IdCarrera, string NombreCarrera, int TotalGrupos)>> ContarPorCarreraAsync();
        List<(int IdCarrera, string NombreCarrera, int TotalGrupos)> ContarPorCarrera();
        Task<List<Grupo>> ObtenerPorCarreraAsync(int idCarrera);
        List<Grupo> ObtenerPorCarrera(int idCarrera);
        Task<List<Grupo>> ObtenerPorCicloYCarreraAsync(string ciclo, int idCarrera);
        List<Grupo> ObtenerPorCicloYCarrera(string ciclo, int idCarrera);
        Task<int> ContarPorCicloYCarreraAsync(string ciclo, int idCarrera);
        int ContarPorCicloYCarrera(string ciclo, int idCarrera);
        List<GrupoDisponibleMateriaDto> ObtenerGruposParaMateria(int idCarrera, int semestre, int idMateria);
        Task GuardarAsync(Grupo grupo);
        void Guardar(Grupo grupo);
        Task EliminarAsync(int idGrupo);
        void Eliminar(int idGrupo);
    }

    public class GrupoRepository : IGrupoRepository
    {
        public async Task<List<(int IdCarrera, string NombreCarrera, int TotalGrupos)>> ContarPorCarreraAsync()
        {
            var resultado = new List<(int, string, int)>();
            await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            await using var cmd = new SqlCommand("sp_ContarGruposPorCarrera", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                resultado.Add((
                    Convert.ToInt32(reader["id_carrera"]),
                    reader["nombre"].ToString() ?? "",
                    Convert.ToInt32(reader["TotalGrupos"])
                ));
            }
            return resultado;
        }

        public List<(int IdCarrera, string NombreCarrera, int TotalGrupos)> ContarPorCarrera()
        {
            var resultado = new List<(int, string, int)>();
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand("sp_ContarGruposPorCarrera", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                resultado.Add((
                    Convert.ToInt32(reader["id_carrera"]),
                    reader["nombre"].ToString() ?? "",
                    Convert.ToInt32(reader["TotalGrupos"])
                ));
            }
            return resultado;
        }

        public async Task<List<Grupo>> ObtenerPorCarreraAsync(int idCarrera)
        {
            var grupos = new List<Grupo>();
            await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            await using var cmd = new SqlCommand(@"
                SELECT id_grupo, nombre, semestre, turno,
                       ISNULL(es_especial, 0) AS es_especial,
                       ISNULL(cupo_alumnos, 35) AS cupo_alumnos
                FROM Grupos
                WHERE id_carrera = @id_carrera", conn);
            cmd.Parameters.Add("@id_carrera", SqlDbType.Int).Value = idCarrera;
            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                grupos.Add(new Grupo
                {
                    IdGrupo = Convert.ToInt32(reader["id_grupo"]),
                    Nombre = reader["nombre"].ToString() ?? "",
                    Semestre = Convert.ToInt32(reader["semestre"]),
                    Turno = reader["turno"].ToString() ?? "",
                    IdCarrera = idCarrera,
                    EsEspecial = Convert.ToBoolean(reader["es_especial"]),
                    CupoAlumnos = Convert.ToInt32(reader["cupo_alumnos"])
                });
            }
            return grupos;
        }

        public List<Grupo> ObtenerPorCarrera(int idCarrera)
        {
            var grupos = new List<Grupo>();
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand(@"
                SELECT id_grupo, nombre, semestre, turno,
                       ISNULL(es_especial, 0) AS es_especial,
                       ISNULL(cupo_alumnos, 35) AS cupo_alumnos
                FROM Grupos
                WHERE id_carrera = @id_carrera", conn);
            cmd.Parameters.Add("@id_carrera", SqlDbType.Int).Value = idCarrera;
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                grupos.Add(new Grupo
                {
                    IdGrupo = Convert.ToInt32(reader["id_grupo"]),
                    Nombre = reader["nombre"].ToString() ?? "",
                    Semestre = Convert.ToInt32(reader["semestre"]),
                    Turno = reader["turno"].ToString() ?? "",
                    IdCarrera = idCarrera,
                    EsEspecial = Convert.ToBoolean(reader["es_especial"]),
                    CupoAlumnos = Convert.ToInt32(reader["cupo_alumnos"])
                });
            }
            return grupos;
        }

        public async Task<List<Grupo>> ObtenerPorCicloYCarreraAsync(string ciclo, int idCarrera)
        {
            var grupos = new List<Grupo>();
            await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            await using var cmd = new SqlCommand("sp_ObtenerGruposPorCicloYCarrera", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@ciclo", SqlDbType.Char, 1).Value = ciclo;
            cmd.Parameters.Add("@id_carrera", SqlDbType.Int).Value = idCarrera;
            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                grupos.Add(MapGrupoCiclo(reader, idCarrera));
            }
            return grupos;
        }

        public List<Grupo> ObtenerPorCicloYCarrera(string ciclo, int idCarrera)
        {
            var grupos = new List<Grupo>();
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand("sp_ObtenerGruposPorCicloYCarrera", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@ciclo", SqlDbType.Char, 1).Value = ciclo;
            cmd.Parameters.Add("@id_carrera", SqlDbType.Int).Value = idCarrera;
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                grupos.Add(MapGrupoCiclo(reader, idCarrera));
            }
            return grupos;
        }

        public async Task<int> ContarPorCicloYCarreraAsync(string ciclo, int idCarrera)
        {
            var lista = await ObtenerPorCicloYCarreraAsync(ciclo, idCarrera);
            return lista.Count;
        }

        public int ContarPorCicloYCarrera(string ciclo, int idCarrera)
        {
            var lista = ObtenerPorCicloYCarrera(ciclo, idCarrera);
            return lista.Count;
        }

        public List<GrupoDisponibleMateriaDto> ObtenerGruposParaMateria(int idCarrera, int semestre, int idMateria)
        {
            var lista = new List<GrupoDisponibleMateriaDto>();
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand("sp_ObtenerGruposParaMateria", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@id_carrera", SqlDbType.Int).Value = idCarrera;
            cmd.Parameters.Add("@semestre", SqlDbType.Int).Value = semestre;
            cmd.Parameters.Add("@id_materia", SqlDbType.Int).Value = idMateria;

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new GrupoDisponibleMateriaDto
                {
                    IdGrupo = Convert.ToInt32(reader["id_grupo"]),
                    NombreGrupo = reader["NombreGrupo"].ToString() ?? "",
                    Turno = reader["turno"].ToString() ?? "",
                    EsEspecial = Convert.ToBoolean(reader["es_especial"]),
                    IdDocenteAsignado = reader["IdDocenteAsignado"] != DBNull.Value ? Convert.ToInt32(reader["IdDocenteAsignado"]) : null,
                    NombreDocenteAsignado = reader["NombreDocenteAsignado"]?.ToString() ?? ""
                });
            }
            return lista;
        }

        public async Task GuardarAsync(Grupo grupo)
        {
            await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            await using var cmd = CrearComandoGuardar(conn, grupo);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public void Guardar(Grupo grupo)
        {
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = CrearComandoGuardar(conn, grupo);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public async Task EliminarAsync(int idGrupo)
        {
            await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            await using var cmd = new SqlCommand("DELETE FROM Grupos WHERE id_grupo = @id", conn);
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = idGrupo;
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public void Eliminar(int idGrupo)
        {
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand("DELETE FROM Grupos WHERE id_grupo = @id", conn);
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = idGrupo;
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        private static SqlCommand CrearComandoGuardar(SqlConnection conn, Grupo grupo)
        {
            var cmd = new SqlCommand { Connection = conn };
            if (grupo.IdGrupo <= 0)
            {
                cmd.CommandText = @"INSERT INTO Grupos (nombre, semestre, turno, id_carrera, es_especial, cupo_alumnos) 
                                    VALUES (@nombre, @semestre, @turno, @id_carrera, @es_especial, @cupo)";
            }
            else
            {
                cmd.CommandText = @"UPDATE Grupos 
                                    SET nombre = @nombre, semestre = @semestre, turno = @turno, es_especial = @es_especial, cupo_alumnos = @cupo
                                    WHERE id_grupo = @id_grupo";
                cmd.Parameters.Add("@id_grupo", SqlDbType.Int).Value = grupo.IdGrupo;
            }

            cmd.Parameters.Add("@nombre", SqlDbType.VarChar, 20).Value = grupo.Nombre.Trim().ToUpperInvariant();
            cmd.Parameters.Add("@semestre", SqlDbType.Int).Value = grupo.Semestre;
            cmd.Parameters.Add("@turno", SqlDbType.VarChar, 20).Value = grupo.Turno;
            cmd.Parameters.Add("@id_carrera", SqlDbType.Int).Value = grupo.IdCarrera;
            cmd.Parameters.Add("@es_especial", SqlDbType.Bit).Value = grupo.EsEspecial;
            cmd.Parameters.Add("@cupo", SqlDbType.Int).Value = grupo.CupoAlumnos > 0 ? grupo.CupoAlumnos : 35;
            return cmd;
        }

        private static Grupo MapGrupoCiclo(SqlDataReader reader, int idCarrera) => new()
        {
            IdGrupo = Convert.ToInt32(reader["id_grupo"]),
            Nombre = reader["NombreGrupo"].ToString() ?? "",
            Semestre = Convert.ToInt32(reader["semestre"]),
            Turno = reader["turno"].ToString() ?? "",
            IdCarrera = idCarrera,
            NombreCarrera = reader["NombreCarrera"]?.ToString() ?? "",
            EsEspecial = Convert.ToBoolean(reader["es_especial"]),
            CupoAlumnos = Convert.ToInt32(reader["cupo_alumnos"])
        };
    }
}
