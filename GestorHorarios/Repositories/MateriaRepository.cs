using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using GestorHorarios.Models;
using GestorHorarios.Services;
using Microsoft.Data.SqlClient;

namespace GestorHorarios.Repositories
{
    public interface IMateriaRepository
    {
        Task<List<(int IdCarrera, string Carrera, int TotalMaterias)>> ContarPorCarreraAsync();
        List<(int IdCarrera, string Carrera, int TotalMaterias)> ContarPorCarrera();
        Task<List<Materia>> ObtenerPorCarreraAsync(int idCarrera);
        List<Materia> ObtenerPorCarrera(int idCarrera);
        Task<List<Materia>> ObtenerPorCarreraYSemestreAsync(int idCarrera, int semestre);
        List<Materia> ObtenerPorCarreraYSemestre(int idCarrera, int semestre);
        Task<int> SumarCreditosSemestreAsync(int idCarrera, int semestre);
        int SumarCreditosSemestre(int idCarrera, int semestre);
        Task GuardarAsync(Materia materia, int idCarrera);
        void Guardar(Materia materia, int idCarrera);
        Task DarDeBajaAsync(int idMateria);
        void DarDeBaja(int idMateria);
    }

    public class MateriaRepository : IMateriaRepository
    {
        public async Task<List<(int IdCarrera, string Carrera, int TotalMaterias)>> ContarPorCarreraAsync()
        {
            var lista = new List<(int, string, int)>();
            await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            await using var cmd = new SqlCommand("sp_ContarMateriasPorCarrera", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add((
                    Convert.ToInt32(reader["id_carrera"]),
                    reader["Carrera"].ToString() ?? "",
                    Convert.ToInt32(reader["TotalMaterias"])
                ));
            }
            return lista;
        }

        public List<(int IdCarrera, string Carrera, int TotalMaterias)> ContarPorCarrera()
        {
            var lista = new List<(int, string, int)>();
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand("sp_ContarMateriasPorCarrera", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add((
                    Convert.ToInt32(reader["id_carrera"]),
                    reader["Carrera"].ToString() ?? "",
                    Convert.ToInt32(reader["TotalMaterias"])
                ));
            }
            return lista;
        }

        public async Task<List<Materia>> ObtenerPorCarreraAsync(int idCarrera)
        {
            var materias = new List<Materia>();
            await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            await using var cmd = new SqlCommand("sp_ObtenerMateriasPorCarrera", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@id_carrera", SqlDbType.Int).Value = idCarrera;
            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                materias.Add(MapMateria(reader));
            }
            return materias;
        }

        public List<Materia> ObtenerPorCarrera(int idCarrera)
        {
            var materias = new List<Materia>();
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand("sp_ObtenerMateriasPorCarrera", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@id_carrera", SqlDbType.Int).Value = idCarrera;
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                materias.Add(MapMateria(reader));
            }
            return materias;
        }

        public async Task<List<Materia>> ObtenerPorCarreraYSemestreAsync(int idCarrera, int semestre)
        {
            var materias = new List<Materia>();
            await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            await using var cmd = new SqlCommand(@"
                SELECT id_materia, nombre, clave, creditos, semestre,
                       ISNULL(requiere_laboratorio, 0) AS requiere_laboratorio
                FROM Materias
                WHERE id_carrera = @id AND semestre = @sem AND id_estado = 1
                ORDER BY nombre", conn);
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = idCarrera;
            cmd.Parameters.Add("@sem", SqlDbType.Int).Value = semestre;
            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                materias.Add(MapMateria(reader));
            }
            return materias;
        }

        public List<Materia> ObtenerPorCarreraYSemestre(int idCarrera, int semestre)
        {
            var materias = new List<Materia>();
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand(@"
                SELECT id_materia, nombre, clave, creditos, semestre,
                       ISNULL(requiere_laboratorio, 0) AS requiere_laboratorio
                FROM Materias
                WHERE id_carrera = @id AND semestre = @sem AND id_estado = 1
                ORDER BY nombre", conn);
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = idCarrera;
            cmd.Parameters.Add("@sem", SqlDbType.Int).Value = semestre;
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                materias.Add(MapMateria(reader));
            }
            return materias;
        }

        public async Task<int> SumarCreditosSemestreAsync(int idCarrera, int semestre)
        {
            await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            await using var cmd = new SqlCommand(
                "SELECT SUM(creditos) FROM Materias WHERE id_carrera = @idC AND semestre = @sem AND id_estado = 1", conn);
            cmd.Parameters.Add("@idC", SqlDbType.Int).Value = idCarrera;
            cmd.Parameters.Add("@sem", SqlDbType.Int).Value = semestre;
            await conn.OpenAsync();
            var res = await cmd.ExecuteScalarAsync();
            return res != null && res != DBNull.Value ? Convert.ToInt32(res) : 0;
        }

        public int SumarCreditosSemestre(int idCarrera, int semestre)
        {
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand(
                "SELECT SUM(creditos) FROM Materias WHERE id_carrera = @idC AND semestre = @sem AND id_estado = 1", conn);
            cmd.Parameters.Add("@idC", SqlDbType.Int).Value = idCarrera;
            cmd.Parameters.Add("@sem", SqlDbType.Int).Value = semestre;
            conn.Open();
            var res = cmd.ExecuteScalar();
            return res != null && res != DBNull.Value ? Convert.ToInt32(res) : 0;
        }

        public async Task GuardarAsync(Materia materia, int idCarrera)
        {
            await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            await using var cmd = CrearComandoGuardar(conn, materia, idCarrera);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public void Guardar(Materia materia, int idCarrera)
        {
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = CrearComandoGuardar(conn, materia, idCarrera);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public async Task DarDeBajaAsync(int idMateria)
        {
            await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            await using var cmd = new SqlCommand("UPDATE Materias SET id_estado = 2 WHERE id_materia = @id", conn);
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = idMateria;
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public void DarDeBaja(int idMateria)
        {
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand("UPDATE Materias SET id_estado = 2 WHERE id_materia = @id", conn);
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = idMateria;
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        private static SqlCommand CrearComandoGuardar(SqlConnection conn, Materia materia, int idCarrera)
        {
            var cmd = new SqlCommand { Connection = conn };
            if (materia.IdMateria <= 0)
            {
                cmd.CommandText = @"INSERT INTO Materias (nombre, clave, creditos, semestre, id_carrera, id_estado, requiere_laboratorio) 
                                    VALUES (@nombre, @clave, @creditos, @semestre, @id_carrera, 1, @reqLab)";
            }
            else
            {
                cmd.CommandText = @"UPDATE Materias 
                                    SET nombre = @nombre, clave = @clave, creditos = @creditos, semestre = @semestre, requiere_laboratorio = @reqLab
                                    WHERE id_materia = @id_materia";
                cmd.Parameters.Add("@id_materia", SqlDbType.Int).Value = materia.IdMateria;
            }

            cmd.Parameters.Add("@nombre", SqlDbType.VarChar, 150).Value = materia.Nombre.Trim();
            cmd.Parameters.Add("@clave", SqlDbType.VarChar, 20).Value = materia.Clave.Trim().ToUpperInvariant();
            cmd.Parameters.Add("@creditos", SqlDbType.Int).Value = materia.Creditos;
            cmd.Parameters.Add("@semestre", SqlDbType.Int).Value = materia.Semestre;
            cmd.Parameters.Add("@id_carrera", SqlDbType.Int).Value = idCarrera;
            cmd.Parameters.Add("@reqLab", SqlDbType.Bit).Value = materia.RequiereLaboratorio;
            return cmd;
        }

        private static Materia MapMateria(SqlDataReader reader) => new()
        {
            IdMateria = Convert.ToInt32(reader["id_materia"]),
            Nombre = reader["nombre"].ToString() ?? "",
            Clave = reader["clave"].ToString() ?? "",
            Creditos = Convert.ToInt32(reader["creditos"]),
            Semestre = Convert.ToInt32(reader["semestre"]),
            RequiereLaboratorio = Convert.ToBoolean(reader["requiere_laboratorio"])
        };
    }
}

