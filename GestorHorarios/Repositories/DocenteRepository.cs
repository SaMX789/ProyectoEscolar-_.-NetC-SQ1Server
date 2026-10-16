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
    public class DocenteBusquedaGlobalDto
    {
        public int IdDocente { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public int IdCarreraPrincipal { get; set; }
        public string NombreCarreraPrincipal { get; set; } = string.Empty;
    }

    public class DocentePorMateriaDto
    {
        public string NombreDocente { get; set; } = string.Empty;
        public string CarreraPrincipal { get; set; } = string.Empty;
        public string CarrerasAsignadas { get; set; } = string.Empty;
    }

    public class DocenteAsignableMateriaDto
    {
        public int IdDocente { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Hfg { get; set; }
        public int HorasRepartidas { get; set; }
        public string CarreraPrincipal { get; set; } = string.Empty;
    }

    public class DocenteProyectoResumenDto
    {
        public int IdDocente { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int HorasFrenteGrupo { get; set; }
        public string CarreraPrincipal { get; set; } = string.Empty;
    }

    public class DocenteMateriaGrupoEdicionDto
    {
        public int IdMateria { get; set; }
        public int? IdGrupo { get; set; }
        public bool EnLinea { get; set; }
        public string NombreGrupo { get; set; } = string.Empty;
        public bool EsEspecial { get; set; }
    }

    public interface IDocenteRepository
    {
        Task<List<int>> ContarPorCarreraAsync();
        List<int> ContarPorCarrera();
        Task<List<DocenteBusquedaGlobalDto>> BuscarGlobalAsync(string filtro);
        List<DocenteBusquedaGlobalDto> BuscarGlobal(string filtro);
        Task<List<Docente>> ObtenerPorCarreraAsync(int idCarrera);
        List<Docente> ObtenerPorCarrera(int idCarrera);
        Task<List<Docente>> ObtenerGlobalesPorCarreraParaSolverAsync(int idCarrera, string cicloProyecto);
        List<DocentePorMateriaDto> ObtenerPorMateria(int idMateria);
        Task<List<DocenteAsignableMateriaDto>> ObtenerAsignablesAMateriaEnProyectoAsync(int idMateria, int idProyecto);
        List<DocenteAsignableMateriaDto> ObtenerAsignablesAMateriaEnProyecto(int idMateria, int idProyecto);
        Task<List<DocenteProyectoResumenDto>> ObtenerConHorarioPorProyectoAsync(int idProyecto);
        List<DocenteProyectoResumenDto> ObtenerConHorarioPorProyecto(int idProyecto);
        Task<int> ObtenerHorasFrenteGrupoAsync(int idDocente);
        int ObtenerHorasFrenteGrupo(int idDocente);
        Task<List<(int IdDia, int IdBloque)>> ObtenerDisponibilidadAsync(int idDocente);
        List<(int IdDia, int IdBloque)> ObtenerDisponibilidad(int idDocente);
        Task<List<int>> CargarBloquesIdsAsync();
        List<int> CargarBloquesIds();
        List<DocenteMateriaGrupoEdicionDto> ObtenerMateriasYGruposParaEdicion(int idDocente);
        void GuardarNuevo(
            string nombre,
            string tipoDocente,
            int horasMaximas,
            int horasFrenteGrupo,
            int idCarreraPrincipal,
            List<int> idCarrerasSecundarias,
            List<(int IdMateria, int? IdGrupo, bool EnLinea)> asignacionesMateriaGrupo,
            List<(int IdDia, int IdBloque)> disponibilidad);
        void Actualizar(
            int idDocente,
            string nombre,
            string tipoDocente,
            int horasMaximas,
            int horasFrenteGrupo,
            int idCarreraPrincipal,
            List<int> idCarrerasSecundarias,
            List<(int IdMateria, int? IdGrupo, bool EnLinea)> asignacionesMateriaGrupo,
            List<(int IdDia, int IdBloque)> disponibilidad);
        void Eliminar(int idDocente);
        bool LeerPreferenciaCicloA();
        void GuardarPreferenciaCiclo(bool esCicloA);
    }

    public class DocenteRepository : IDocenteRepository
    {
        public async Task<List<int>> ContarPorCarreraAsync()
        {
            var conteos = new List<int>();
            await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            await using var cmd = new SqlCommand("sp_ContarDocentesPorCarrera", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                conteos.Add(Convert.ToInt32(reader["TotalDocentes"]));
            }
            return conteos;
        }

        public List<int> ContarPorCarrera()
        {
            var conteos = new List<int>();
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand("sp_ContarDocentesPorCarrera", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                conteos.Add(Convert.ToInt32(reader["TotalDocentes"]));
            }
            return conteos;
        }

        public async Task<List<DocenteBusquedaGlobalDto>> BuscarGlobalAsync(string filtro)
        {
            var resultados = new List<DocenteBusquedaGlobalDto>();
            await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            await using var cmd = new SqlCommand("sp_BuscarDocentesGlobal", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@filtro", SqlDbType.NVarChar, 100).Value = filtro;
            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                resultados.Add(new DocenteBusquedaGlobalDto
                {
                    IdDocente = reader.GetInt32(0),
                    NombreCompleto = reader.GetString(1),
                    IdCarreraPrincipal = reader["IdCarreraPrincipal"] != DBNull.Value ? reader.GetInt32(4) : 0,
                    NombreCarreraPrincipal = reader["NombreCarreraPrincipal"] != DBNull.Value ? reader.GetString(5) : "Sin carrera principal"
                });
            }
            return resultados;
        }

        public List<DocenteBusquedaGlobalDto> BuscarGlobal(string filtro)
        {
            var resultados = new List<DocenteBusquedaGlobalDto>();
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand("sp_BuscarDocentesGlobal", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@filtro", SqlDbType.NVarChar, 100).Value = filtro;
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                resultados.Add(new DocenteBusquedaGlobalDto
                {
                    IdDocente = reader.GetInt32(0),
                    NombreCompleto = reader.GetString(1),
                    IdCarreraPrincipal = reader["IdCarreraPrincipal"] != DBNull.Value ? reader.GetInt32(4) : 0,
                    NombreCarreraPrincipal = reader["NombreCarreraPrincipal"] != DBNull.Value ? reader.GetString(5) : "Sin carrera principal"
                });
            }
            return resultados;
        }

        public async Task<List<Docente>> ObtenerPorCarreraAsync(int idCarrera)
        {
            return await Task.Run(() => ObtenerPorCarrera(idCarrera));
        }

        public List<Docente> ObtenerPorCarrera(int idCarrera)
        {
            var docentes = new List<Docente>();
            var idDocentesProcesados = new HashSet<int>();

            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand("sp_GetDocentesByCarrera", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@id_carrera", SqlDbType.Int).Value = idCarrera;
            conn.Open();

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int idDocente = Convert.ToInt32(reader["id_docente"]);
                    if (!idDocentesProcesados.Add(idDocente)) continue;

                    int hfg = 0;
                    try
                    {
                        if (reader["horas_frente_grupo"] != DBNull.Value)
                            hfg = Convert.ToInt32(reader["horas_frente_grupo"]);
                    }
                    catch { }

                    docentes.Add(new Docente
                    {
                        IdDocente = idDocente,
                        NombreCompleto = reader["NombreCompleto"] as string ?? "Sin nombre",
                        TipoTiempo = reader["TipoTiempo"] as string ?? string.Empty,
                        CarreraPrincipal = reader["CarreraPrincipal"] as string ?? string.Empty,
                        CarreraSecundaria = reader["CarreraSecundaria"] as string ?? "Ninguna",
                        Materias = reader["Materias"] as string ?? string.Empty,
                        HorarioLaboral = reader["HorarioLaboral"] as string ?? string.Empty,
                        HorasMaximas = reader["HorasMaximas"] != DBNull.Value ? Convert.ToInt32(reader["HorasMaximas"]) : 0,
                        HorasFrenteGrupo = hfg,
                        HorasAsignadas = reader["HorasAsignadas"] != DBNull.Value ? Convert.ToInt32(reader["HorasAsignadas"]) : 0,
                        IdCarreraPrincipal = reader["IdCarreraPrincipal"] != DBNull.Value ? Convert.ToInt32(reader["IdCarreraPrincipal"]) : 0
                    });
                }
            }

            foreach (var d in docentes)
            {
                d.MateriasIds = CargarMateriasIdsDeDocente(conn, d.IdDocente);
                d.CarrerasSecundariasIds = CargarCarrerasSecundariasIds(conn, d.IdDocente);
            }

            return docentes;
        }

        public async Task<List<Docente>> ObtenerGlobalesPorCarreraParaSolverAsync(int idCarrera, string cicloProyecto)
        {
            var dictDocentes = new Dictionary<int, Docente>();
            await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            await conn.OpenAsync();

            await using (var cmd = new SqlCommand("sp_GetDocentesGlobalesPorCarrera", conn)
            {
                CommandType = CommandType.StoredProcedure
            })
            {
                cmd.Parameters.Add("@id_carrera", SqlDbType.Int).Value = idCarrera;

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    int id = Convert.ToInt32(reader["id_docente"]);
                    int hfg = reader["horas_frente_grupo"] != DBNull.Value
                        ? Convert.ToInt32(reader["horas_frente_grupo"])
                        : 0;

                    dictDocentes[id] = new Docente
                    {
                        IdDocente = id,
                        NombreCompleto = reader["NombreCompleto"].ToString() ?? "",
                        HorasMaximas = reader["HorasMaximas"] != DBNull.Value ? Convert.ToInt32(reader["HorasMaximas"]) : 20,
                        HorasFrenteGrupo = hfg,
                        IdCarreraPrincipal = reader["IdCarreraPrincipal"] != DBNull.Value ? Convert.ToInt32(reader["IdCarreraPrincipal"]) : 0,
                        MateriasIds = new List<int>(),
                        DisponibilidadBloques = new HashSet<string>()
                    };
                }
            }

            if (dictDocentes.Count > 0)
            {
                await using (var cmdMat = new SqlCommand(@"
                    SELECT dm.id_docente, dm.id_materia,
                           CASE WHEN dm.id_grupo IS NOT NULL AND (
                               g.es_especial = 1 OR
                               (@ciclo = 'A' AND m.semestre % 2 = 0) OR
                               (@ciclo = 'B' AND m.semestre % 2 != 0)
                           ) THEN m.creditos ELSE 0 END AS creditos_ciclo
                    FROM DocenteMateria dm
                    INNER JOIN Materias m ON dm.id_materia = m.id_materia
                    LEFT JOIN Grupos g ON dm.id_grupo = g.id_grupo
                    WHERE m.id_estado = 1", conn))
                {
                    cmdMat.Parameters.Add("@ciclo", SqlDbType.Char, 1).Value = cicloProyecto;
                    var hfgPorCiclo = new Dictionary<int, int>();

                    await using var readerMat = await cmdMat.ExecuteReaderAsync();
                    while (await readerMat.ReadAsync())
                    {
                        int idDoc = Convert.ToInt32(readerMat["id_docente"]);
                        if (!dictDocentes.TryGetValue(idDoc, out var doc)) continue;

                        int idMat = Convert.ToInt32(readerMat["id_materia"]);
                        if (!doc.MateriasIds.Contains(idMat))
                            doc.MateriasIds.Add(idMat);

                        int creditosCiclo = Convert.ToInt32(readerMat["creditos_ciclo"]);
                        if (creditosCiclo > 0)
                        {
                            hfgPorCiclo[idDoc] = (hfgPorCiclo.TryGetValue(idDoc, out int acum) ? acum : 0) + creditosCiclo;
                        }
                    }

                    foreach (var kvp in hfgPorCiclo)
                    {
                        if (kvp.Value > 0 && dictDocentes.TryGetValue(kvp.Key, out var doc))
                        {
                            doc.HorasFrenteGrupo = Math.Max(doc.HorasFrenteGrupo, kvp.Value);
                        }
                    }
                }

                await using (var cmdDisp = new SqlCommand(@"
                    SELECT dd.id_docente, dd.id_dia, dd.id_bloque
                    FROM DisponibilidadDocente dd
                    INNER JOIN Docentes d ON dd.id_docente = d.id_docente
                    WHERE d.id_estado = 1", conn))
                await using (var readerDisp = await cmdDisp.ExecuteReaderAsync())
                {
                    while (await readerDisp.ReadAsync())
                    {
                        int idDoc = Convert.ToInt32(readerDisp["id_docente"]);
                        if (dictDocentes.TryGetValue(idDoc, out var doc))
                        {
                            doc.DisponibilidadBloques.Add($"{readerDisp["id_dia"]}_{readerDisp["id_bloque"]}");
                        }
                    }
                }
            }

            return dictDocentes.Values.ToList();
        }

        public List<DocentePorMateriaDto> ObtenerPorMateria(int idMateria)
        {
            var lista = new List<DocentePorMateriaDto>();
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand("sp_GetDocentesPorMateria", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@id_materia", SqlDbType.Int).Value = idMateria;

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new DocentePorMateriaDto
                {
                    NombreDocente = reader["NombreDocente"]?.ToString() ?? "",
                    CarreraPrincipal = reader["CarreraPrincipal"]?.ToString() ?? "",
                    CarrerasAsignadas = reader["CarrerasAsignadas"]?.ToString() ?? ""
                });
            }
            return lista;
        }

        public async Task<List<DocenteAsignableMateriaDto>> ObtenerAsignablesAMateriaEnProyectoAsync(int idMateria, int idProyecto)
        {
            var lista = new List<DocenteAsignableMateriaDto>();
            await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            await using var cmd = new SqlCommand("sp_GetDocentesAsignablesAMateria", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@id_materia", SqlDbType.Int).Value = idMateria;
            cmd.Parameters.Add("@id_proyecto", SqlDbType.Int).Value = idProyecto;

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new DocenteAsignableMateriaDto
                {
                    IdDocente = Convert.ToInt32(reader["id_docente"]),
                    Nombre = reader["nombre"].ToString() ?? "",
                    Hfg = Convert.ToInt32(reader["hfg"]),
                    HorasRepartidas = Convert.ToInt32(reader["HorasRepartidas"]),
                    CarreraPrincipal = reader["CarreraPrincipal"].ToString() ?? ""
                });
            }
            return lista;
        }

        public List<DocenteAsignableMateriaDto> ObtenerAsignablesAMateriaEnProyecto(int idMateria, int idProyecto)
        {
            var lista = new List<DocenteAsignableMateriaDto>();
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand("sp_GetDocentesAsignablesAMateria", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@id_materia", SqlDbType.Int).Value = idMateria;
            cmd.Parameters.Add("@id_proyecto", SqlDbType.Int).Value = idProyecto;

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new DocenteAsignableMateriaDto
                {
                    IdDocente = Convert.ToInt32(reader["id_docente"]),
                    Nombre = reader["nombre"].ToString() ?? "",
                    Hfg = Convert.ToInt32(reader["hfg"]),
                    HorasRepartidas = Convert.ToInt32(reader["HorasRepartidas"]),
                    CarreraPrincipal = reader["CarreraPrincipal"].ToString() ?? ""
                });
            }
            return lista;
        }

        public async Task<List<DocenteProyectoResumenDto>> ObtenerConHorarioPorProyectoAsync(int idProyecto)
        {
            var lista = new List<DocenteProyectoResumenDto>();
            await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            await using var cmd = new SqlCommand("sp_GetDocentesConHorarioPorProyecto", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@id_proyecto", SqlDbType.Int).Value = idProyecto;
            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new DocenteProyectoResumenDto
                {
                    IdDocente = Convert.ToInt32(reader["id_docente"]),
                    Nombre = reader["nombre"].ToString() ?? "",
                    HorasFrenteGrupo = Convert.ToInt32(reader["horas_frente_grupo"]),
                    CarreraPrincipal = reader["CarreraPrincipal"].ToString() ?? "Sin Departamento"
                });
            }
            return lista;
        }

        public List<DocenteProyectoResumenDto> ObtenerConHorarioPorProyecto(int idProyecto)
        {
            var lista = new List<DocenteProyectoResumenDto>();
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand("sp_GetDocentesConHorarioPorProyecto", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@id_proyecto", SqlDbType.Int).Value = idProyecto;
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new DocenteProyectoResumenDto
                {
                    IdDocente = Convert.ToInt32(reader["id_docente"]),
                    Nombre = reader["nombre"].ToString() ?? "",
                    HorasFrenteGrupo = Convert.ToInt32(reader["horas_frente_grupo"]),
                    CarreraPrincipal = reader["CarreraPrincipal"].ToString() ?? "Sin Departamento"
                });
            }
            return lista;
        }

        public async Task<int> ObtenerHorasFrenteGrupoAsync(int idDocente)
        {
            await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            await using var cmd = new SqlCommand("SELECT horas_frente_grupo FROM Docentes WHERE id_docente = @id", conn);
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = idDocente;
            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }

        public int ObtenerHorasFrenteGrupo(int idDocente)
        {
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand("SELECT horas_frente_grupo FROM Docentes WHERE id_docente = @id", conn);
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = idDocente;
            conn.Open();
            var result = cmd.ExecuteScalar();
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }

        public async Task<List<(int IdDia, int IdBloque)>> ObtenerDisponibilidadAsync(int idDocente)
        {
            var disponibilidad = new List<(int IdDia, int IdBloque)>();
            await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            await using var cmd = new SqlCommand(
                "SELECT id_dia, id_bloque FROM DisponibilidadDocente WHERE id_docente = @id ORDER BY id_dia, id_bloque", conn);
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = idDocente;
            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                disponibilidad.Add((Convert.ToInt32(reader["id_dia"]), Convert.ToInt32(reader["id_bloque"])));
            }
            return disponibilidad;
        }

        public List<(int IdDia, int IdBloque)> ObtenerDisponibilidad(int idDocente)
        {
            var disponibilidad = new List<(int IdDia, int IdBloque)>();
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand(
                "SELECT id_dia, id_bloque FROM DisponibilidadDocente WHERE id_docente = @id ORDER BY id_dia, id_bloque", conn);
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = idDocente;
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                disponibilidad.Add((Convert.ToInt32(reader["id_dia"]), Convert.ToInt32(reader["id_bloque"])));
            }
            return disponibilidad;
        }

        public async Task<List<int>> CargarBloquesIdsAsync()
        {
            var bloques = new List<int>();
            await using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            await using var cmd = new SqlCommand("SELECT id_bloque FROM BloquesHora ORDER BY hora_inicio", conn);
            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                bloques.Add(Convert.ToInt32(reader["id_bloque"]));
            }
            return bloques;
        }

        public List<int> CargarBloquesIds()
        {
            var bloques = new List<int>();
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand("SELECT id_bloque FROM BloquesHora ORDER BY hora_inicio", conn);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                bloques.Add(Convert.ToInt32(reader["id_bloque"]));
            }
            return bloques;
        }

        public List<DocenteMateriaGrupoEdicionDto> ObtenerMateriasYGruposParaEdicion(int idDocente)
        {
            var lista = new List<DocenteMateriaGrupoEdicionDto>();
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand(@"
                SELECT dm.id_materia, dm.id_grupo, dm.en_linea, g.nombre AS nombre_grupo, g.es_especial
                FROM DocenteMateria dm
                LEFT JOIN Grupos g ON dm.id_grupo = g.id_grupo
                WHERE dm.id_docente = @id", conn);
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = idDocente;
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new DocenteMateriaGrupoEdicionDto
                {
                    IdMateria = Convert.ToInt32(reader["id_materia"]),
                    IdGrupo = reader["id_grupo"] != DBNull.Value ? Convert.ToInt32(reader["id_grupo"]) : null,
                    EnLinea = reader["en_linea"] != DBNull.Value && Convert.ToBoolean(reader["en_linea"]),
                    NombreGrupo = reader["nombre_grupo"]?.ToString() ?? "",
                    EsEspecial = reader["es_especial"] != DBNull.Value && Convert.ToBoolean(reader["es_especial"])
                });
            }
            return lista;
        }

        public void GuardarNuevo(
            string nombre,
            string tipoDocente,
            int horasMaximas,
            int horasFrenteGrupo,
            int idCarreraPrincipal,
            List<int> idCarrerasSecundarias,
            List<(int IdMateria, int? IdGrupo, bool EnLinea)> asignacionesMateriaGrupo,
            List<(int IdDia, int IdBloque)> disponibilidad)
        {
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            conn.Open();
            using var tx = conn.BeginTransaction();
            try
            {
                int idDocente;
                using (var cmd = new SqlCommand(@"
                    INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas, horas_frente_grupo)
                    VALUES (@nombre, @tipo, 1, @horas, @hfg);
                    SELECT SCOPE_IDENTITY();", conn, tx))
                {
                    cmd.Parameters.Add("@nombre", SqlDbType.NVarChar, 150).Value = nombre;
                    cmd.Parameters.Add("@tipo", SqlDbType.NVarChar, 50).Value = tipoDocente;
                    cmd.Parameters.Add("@horas", SqlDbType.Int).Value = horasMaximas;
                    cmd.Parameters.Add("@hfg", SqlDbType.Int).Value = horasFrenteGrupo;
                    idDocente = Convert.ToInt32(cmd.ExecuteScalar());
                }

                InsertarRelacionesDocente(conn, tx, idDocente, idCarreraPrincipal, idCarrerasSecundarias, asignacionesMateriaGrupo, disponibilidad);
                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public void Actualizar(
            int idDocente,
            string nombre,
            string tipoDocente,
            int horasMaximas,
            int horasFrenteGrupo,
            int idCarreraPrincipal,
            List<int> idCarrerasSecundarias,
            List<(int IdMateria, int? IdGrupo, bool EnLinea)> asignacionesMateriaGrupo,
            List<(int IdDia, int IdBloque)> disponibilidad)
        {
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            conn.Open();
            using var tx = conn.BeginTransaction();
            try
            {
                using (var cmd = new SqlCommand(@"
                    UPDATE Docentes
                    SET nombre = @nombre, tipo_docente = @tipo, horas_maximas = @horas, horas_frente_grupo = @hfg
                    WHERE id_docente = @id", conn, tx))
                {
                    cmd.Parameters.Add("@nombre", SqlDbType.NVarChar, 150).Value = nombre;
                    cmd.Parameters.Add("@tipo", SqlDbType.NVarChar, 50).Value = tipoDocente;
                    cmd.Parameters.Add("@horas", SqlDbType.Int).Value = horasMaximas;
                    cmd.Parameters.Add("@hfg", SqlDbType.Int).Value = horasFrenteGrupo;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = idDocente;
                    cmd.ExecuteNonQuery();
                }

                EjecutarDelete(conn, tx, "DELETE FROM DisponibilidadDocente WHERE id_docente = @id", idDocente);
                EjecutarDelete(conn, tx, "DELETE FROM DocenteMateria WHERE id_docente = @id", idDocente);
                EjecutarDelete(conn, tx, "DELETE FROM DocenteCarrera WHERE id_docente = @id", idDocente);

                InsertarRelacionesDocente(conn, tx, idDocente, idCarreraPrincipal, idCarrerasSecundarias, asignacionesMateriaGrupo, disponibilidad);
                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public void Eliminar(int idDocente)
        {
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            conn.Open();
            using var tx = conn.BeginTransaction();
            try
            {
                EjecutarDelete(conn, tx, "DELETE FROM DisponibilidadDocente WHERE id_docente = @id", idDocente);
                EjecutarDelete(conn, tx, "DELETE FROM DocenteMateria WHERE id_docente = @id", idDocente);
                EjecutarDelete(conn, tx, "DELETE FROM DocenteCarrera WHERE id_docente = @id", idDocente);
                EjecutarDelete(conn, tx, "DELETE FROM Docentes WHERE id_docente = @id", idDocente);
                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public bool LeerPreferenciaCicloA()
        {
            try
            {
                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                using var cmd = new SqlCommand("SELECT valor FROM Configuraciones WHERE clave = 'CicloActivoHFG'", conn);
                conn.Open();
                var result = cmd.ExecuteScalar()?.ToString();
                return result != "B";
            }
            catch
            {
                return true;
            }
        }

        public void GuardarPreferenciaCiclo(bool esCicloA)
        {
            try
            {
                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                using var cmd = new SqlCommand(@"
                    IF EXISTS (SELECT 1 FROM Configuraciones WHERE clave = 'CicloActivoHFG')
                        UPDATE Configuraciones SET valor = @valor WHERE clave = 'CicloActivoHFG'
                    ELSE
                        INSERT INTO Configuraciones (clave, valor) VALUES ('CicloActivoHFG', @valor)", conn);
                cmd.Parameters.Add("@valor", SqlDbType.NVarChar, 10).Value = esCicloA ? "A" : "B";
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            catch { }
        }

        private static void InsertarRelacionesDocente(
            SqlConnection conn,
            SqlTransaction tx,
            int idDocente,
            int idCarreraPrincipal,
            List<int> idCarrerasSecundarias,
            List<(int IdMateria, int? IdGrupo, bool EnLinea)> asignacionesMateriaGrupo,
            List<(int IdDia, int IdBloque)> disponibilidad)
        {
            InsertarDocenteCarrera(conn, tx, idDocente, idCarreraPrincipal, esPrincipal: true);
            foreach (var idSec in idCarrerasSecundarias)
            {
                InsertarDocenteCarrera(conn, tx, idDocente, idSec, esPrincipal: false);
            }

            foreach (var (idMat, idGrupo, enLinea) in asignacionesMateriaGrupo)
            {
                using var cmdMat = new SqlCommand(@"
                    INSERT INTO DocenteMateria (id_docente, id_materia, id_grupo, en_linea)
                    VALUES (@idDocente, @idMateria, @idGrupo, @enLinea);", conn, tx);
                cmdMat.Parameters.Add("@idDocente", SqlDbType.Int).Value = idDocente;
                cmdMat.Parameters.Add("@idMateria", SqlDbType.Int).Value = idMat;
                cmdMat.Parameters.Add("@idGrupo", SqlDbType.Int).Value = (object?)idGrupo ?? DBNull.Value;
                cmdMat.Parameters.Add("@enLinea", SqlDbType.Bit).Value = enLinea;
                cmdMat.ExecuteNonQuery();
            }

            foreach (var (idDia, idBloque) in disponibilidad)
            {
                using var cmdDisp = new SqlCommand(@"
                    INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque)
                    VALUES (@idDocente, @idDia, @idBloque);", conn, tx);
                cmdDisp.Parameters.Add("@idDocente", SqlDbType.Int).Value = idDocente;
                cmdDisp.Parameters.Add("@idDia", SqlDbType.Int).Value = idDia;
                cmdDisp.Parameters.Add("@idBloque", SqlDbType.Int).Value = idBloque;
                cmdDisp.ExecuteNonQuery();
            }
        }

        private static void InsertarDocenteCarrera(SqlConnection conn, SqlTransaction tx, int idDocente, int idCarrera, bool esPrincipal)
        {
            using var cmd = new SqlCommand(@"
                INSERT INTO DocenteCarrera (id_docente, id_carrera, es_principal)
                VALUES (@idDocente, @idCarrera, @esPrincipal);", conn, tx);
            cmd.Parameters.Add("@idDocente", SqlDbType.Int).Value = idDocente;
            cmd.Parameters.Add("@idCarrera", SqlDbType.Int).Value = idCarrera;
            cmd.Parameters.Add("@esPrincipal", SqlDbType.Bit).Value = esPrincipal;
            cmd.ExecuteNonQuery();
        }

        private static void EjecutarDelete(SqlConnection conn, SqlTransaction tx, string sql, int id)
        {
            using var cmd = new SqlCommand(sql, conn, tx);
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
            cmd.ExecuteNonQuery();
        }

        private static List<int> CargarMateriasIdsDeDocente(SqlConnection conn, int idDocente)
        {
            var lista = new List<int>();
            using var cmd = new SqlCommand("SELECT id_materia FROM DocenteMateria WHERE id_docente = @id", conn);
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = idDocente;
            using var r = cmd.ExecuteReader();
            while (r.Read()) lista.Add(Convert.ToInt32(r["id_materia"]));
            return lista;
        }

        private static List<int> CargarCarrerasSecundariasIds(SqlConnection conn, int idDocente)
        {
            var lista = new List<int>();
            using var cmd = new SqlCommand("SELECT id_carrera FROM DocenteCarrera WHERE id_docente = @id AND es_principal = 0", conn);
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = idDocente;
            using var r = cmd.ExecuteReader();
            while (r.Read()) lista.Add(Convert.ToInt32(r["id_carrera"]));
            return lista;
        }
    }
}

