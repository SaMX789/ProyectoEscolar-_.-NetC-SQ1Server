using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using GestorHorarios.Models;
using GestorHorarios.Services;
using Microsoft.Data.SqlClient;

namespace GestorHorarios.Repositories
{
    public interface ICarreraRepository
    {
        Task<List<Carrera>> ObtenerTodasAsync();
        List<Carrera> ObtenerTodas();
        Task<string> ObtenerNombreCarreraAsync(int idCarrera);
        string ObtenerNombreCarrera(int idCarrera);
        string ObtenerNombre(int idCarrera);
    }

    public class CarreraRepository : ICarreraRepository
    {
        public string ObtenerNombre(int idCarrera) => ObtenerNombreCarrera(idCarrera);
        public async Task<List<Carrera>> ObtenerTodasAsync()
        {
            var lista = new List<Carrera>();
            await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            await using var cmd = new SqlCommand("SELECT id_carrera, nombre FROM Carreras ORDER BY id_carrera", conn);
            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new Carrera
                {
                    IdCarrera = Convert.ToInt32(reader["id_carrera"]),
                    Nombre = reader["nombre"].ToString() ?? ""
                });
            }
            return lista;
        }

        public List<Carrera> ObtenerTodas()
        {
            var lista = new List<Carrera>();
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand("SELECT id_carrera, nombre FROM Carreras ORDER BY id_carrera", conn);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Carrera
                {
                    IdCarrera = Convert.ToInt32(reader["id_carrera"]),
                    Nombre = reader["nombre"].ToString() ?? ""
                });
            }
            return lista;
        }

        public async Task<string> ObtenerNombreCarreraAsync(int idCarrera)
        {
            try
            {
                await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                await using var cmd = new SqlCommand("sp_ObtenerNombreCarrera", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add("@id_carrera", SqlDbType.Int).Value = idCarrera;
                await conn.OpenAsync();
                var res = await cmd.ExecuteScalarAsync();
                return res?.ToString() ?? "Carrera";
            }
            catch
            {
                return "Carrera";
            }
        }

        public string ObtenerNombreCarrera(int idCarrera)
        {
            try
            {
                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                using var cmd = new SqlCommand("sp_ObtenerNombreCarrera", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add("@id_carrera", SqlDbType.Int).Value = idCarrera;
                conn.Open();
                var res = cmd.ExecuteScalar();
                return res?.ToString() ?? "Carrera";
            }
            catch
            {
                return "Carrera";
            }
        }
    }
}

