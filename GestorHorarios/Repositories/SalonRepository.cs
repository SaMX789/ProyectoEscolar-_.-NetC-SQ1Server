using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using GestorHorarios.Models;
using GestorHorarios.Services;
using Microsoft.Data.SqlClient;

namespace GestorHorarios.Repositories
{
    public interface ISalonRepository
    {
        Task<List<(int Id, string Nombre)>> ObtenerEdificiosAsync();
        List<(int Id, string Nombre)> ObtenerEdificios();
        Task<List<Salon>> ObtenerTodosAsync();
        List<Salon> ObtenerTodos();
        Task GuardarAsync(Salon salon);
        void Guardar(Salon salon);
        Task EliminarAsync(int idSalon);
        void Eliminar(int idSalon);
    }

    public class SalonRepository : ISalonRepository
    {
        public async Task<List<(int Id, string Nombre)>> ObtenerEdificiosAsync()
        {
            var edificios = new List<(int Id, string Nombre)>();
            await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            await using var cmd = new SqlCommand("SELECT id_edificio, nombre FROM Edificios ORDER BY nombre", conn);
            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                edificios.Add((Convert.ToInt32(reader["id_edificio"]), reader["nombre"].ToString() ?? ""));
            }
            return edificios;
        }

        public List<(int Id, string Nombre)> ObtenerEdificios()
        {
            var edificios = new List<(int Id, string Nombre)>();
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand("SELECT id_edificio, nombre FROM Edificios ORDER BY nombre", conn);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                edificios.Add((Convert.ToInt32(reader["id_edificio"]), reader["nombre"].ToString() ?? ""));
            }
            return edificios;
        }

        public async Task<List<Salon>> ObtenerTodosAsync()
        {
            var salones = new List<Salon>();
            await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            await using var cmd = new SqlCommand("sp_GetSalones", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                salones.Add(MapearSalon(reader));
            }
            return salones;
        }

        public List<Salon> ObtenerTodos()
        {
            var salones = new List<Salon>();
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand("sp_GetSalones", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                salones.Add(MapearSalon(reader));
            }
            return salones;
        }

        public async Task GuardarAsync(Salon salon)
        {
            await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            await conn.OpenAsync();

            if (salon.IdSalon <= 0)
            {
                await using var cmd = new SqlCommand(@"
                    INSERT INTO Salones (nombre, capacidad, id_edificio, id_carrera, id_carreraSecundaria, id_carreraTerciaria, es_laboratorio) 
                    VALUES (@nombre, @cap, @edificio, @carrera, @carreraSec, @carreraTer, @esLab)", conn);
                ConfigurarParametrosSalon(cmd, salon);
                await cmd.ExecuteNonQueryAsync();
            }
            else
            {
                await using var cmd = new SqlCommand(@"
                    UPDATE Salones 
                    SET nombre = @nombre, capacidad = @cap, id_edificio = @edificio, 
                        id_carrera = @carrera, id_carreraSecundaria = @carreraSec, id_carreraTerciaria = @carreraTer,
                        es_laboratorio = @esLab
                    WHERE id_salon = @id", conn);
                ConfigurarParametrosSalon(cmd, salon);
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = salon.IdSalon;
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public void Guardar(Salon salon)
        {
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            conn.Open();

            if (salon.IdSalon <= 0)
            {
                using var cmd = new SqlCommand(@"
                    INSERT INTO Salones (nombre, capacidad, id_edificio, id_carrera, id_carreraSecundaria, id_carreraTerciaria, es_laboratorio) 
                    VALUES (@nombre, @cap, @edificio, @carrera, @carreraSec, @carreraTer, @esLab)", conn);
                ConfigurarParametrosSalon(cmd, salon);
                cmd.ExecuteNonQuery();
            }
            else
            {
                using var cmd = new SqlCommand(@"
                    UPDATE Salones 
                    SET nombre = @nombre, capacidad = @cap, id_edificio = @edificio, 
                        id_carrera = @carrera, id_carreraSecundaria = @carreraSec, id_carreraTerciaria = @carreraTer,
                        es_laboratorio = @esLab
                    WHERE id_salon = @id", conn);
                ConfigurarParametrosSalon(cmd, salon);
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = salon.IdSalon;
                cmd.ExecuteNonQuery();
            }
        }

        public async Task EliminarAsync(int idSalon)
        {
            await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            await using var cmd = new SqlCommand("DELETE FROM Salones WHERE id_salon = @id", conn);
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = idSalon;
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public void Eliminar(int idSalon)
        {
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand("DELETE FROM Salones WHERE id_salon = @id", conn);
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = idSalon;
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        private static Salon MapearSalon(SqlDataReader reader)
        {
            var salon = new Salon
            {
                IdSalon = Convert.ToInt32(reader["id_salon"]),
                Nombre = reader["NombreSalon"].ToString() ?? "",
                Capacidad = Convert.ToInt32(reader["capacidad"]),
                NombreEdificio = reader["NombreEdificio"].ToString() ?? "",
                NombreCarrera = reader["NombreCarrera"].ToString() ?? ""
            };

            if (HasColumn(reader, "id_edificio") && reader["id_edificio"] != DBNull.Value)
                salon.IdEdificio = Convert.ToInt32(reader["id_edificio"]);

            if (reader["id_carrera"] != DBNull.Value)
                salon.IdCarrera = Convert.ToInt32(reader["id_carrera"]);

            if (HasColumn(reader, "id_carreraSecundaria") && reader["id_carreraSecundaria"] != DBNull.Value)
            {
                salon.IdCarreraSecundaria = Convert.ToInt32(reader["id_carreraSecundaria"]);
                salon.NombreCarreraSecundaria = reader["NombreCarreraSecundaria"]?.ToString() ?? "";
            }

            if (HasColumn(reader, "id_carreraTerciaria") && reader["id_carreraTerciaria"] != DBNull.Value)
            {
                salon.IdCarreraTerciaria = Convert.ToInt32(reader["id_carreraTerciaria"]);
                salon.NombreCarreraTerciaria = reader["NombreCarreraTerciaria"]?.ToString() ?? "";
            }

            if (HasColumn(reader, "es_laboratorio") && reader["es_laboratorio"] != DBNull.Value)
            {
                salon.EsLaboratorio = Convert.ToBoolean(reader["es_laboratorio"]);
            }

            return salon;
        }

        private static void ConfigurarParametrosSalon(SqlCommand cmd, Salon salon)
        {
            bool esLab = salon.EsLaboratorio ||
                         salon.Nombre.Contains("CC", StringComparison.OrdinalIgnoreCase) ||
                         salon.Nombre.Contains("Lab", StringComparison.OrdinalIgnoreCase);

            cmd.Parameters.Add("@nombre", SqlDbType.NVarChar, 100).Value = salon.Nombre;
            cmd.Parameters.Add("@cap", SqlDbType.Int).Value = salon.Capacidad;
            cmd.Parameters.Add("@edificio", SqlDbType.Int).Value = salon.IdEdificio > 0 ? salon.IdEdificio : DBNull.Value;
            cmd.Parameters.Add("@carrera", SqlDbType.Int).Value = salon.IdCarrera > 0 ? salon.IdCarrera : DBNull.Value;
            cmd.Parameters.Add("@carreraSec", SqlDbType.Int).Value = (object?)salon.IdCarreraSecundaria ?? DBNull.Value;
            cmd.Parameters.Add("@carreraTer", SqlDbType.Int).Value = (object?)salon.IdCarreraTerciaria ?? DBNull.Value;
            cmd.Parameters.Add("@esLab", SqlDbType.Bit).Value = esLab;
        }

        private static bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}

