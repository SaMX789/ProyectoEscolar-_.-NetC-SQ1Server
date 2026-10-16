using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.OrTools.Sat;
using GestorHorarios.Models;
using GestorHorarios.Repositories;

namespace GestorHorarios.Services
{
    public class GeneradorHorariosService
    {
        private readonly IProyectoRepository _proyectoRepository;
        private readonly IGrupoRepository _grupoRepository;
        private readonly IMateriaRepository _materiaRepository;
        private readonly IDocenteRepository _docenteRepository;
        private readonly IHorarioRepository _horarioRepository;

        public GeneradorHorariosService(
            IProyectoRepository? proyectoRepository = null,
            IGrupoRepository? grupoRepository = null,
            IMateriaRepository? materiaRepository = null,
            IDocenteRepository? docenteRepository = null,
            IHorarioRepository? horarioRepository = null)
        {
            _proyectoRepository = proyectoRepository ?? new ProyectoRepository();
            _grupoRepository = grupoRepository ?? new GrupoRepository();
            _materiaRepository = materiaRepository ?? new MateriaRepository();
            _docenteRepository = docenteRepository ?? new DocenteRepository();
            _horarioRepository = horarioRepository ?? new HorarioRepository();
        }

        public async Task<string> EjecutarDiagnosticoAsync(int idProyecto, int idCarrera)
        {
            try
            {
                if (idProyecto <= 0) return "ERROR: ID del proyecto inválido.";

                // 1. OBTENER EL CICLO DEL PROYECTO DESDE EL REPOSITORIO
                string ciclo = await _proyectoRepository.ObtenerCicloProyectoAsync(idProyecto);

                // 2. CARGAR TODOS LOS DATOS DESDE LOS REPOSITORIOS DE FORMA ASÍNCRONA
                List<Grupo> gruposTodos = await _grupoRepository.ObtenerPorCarreraAsync(idCarrera);
                List<Materia> materiasTodas = await _materiaRepository.ObtenerPorCarreraAsync(idCarrera);
                List<Docente> docentes = await _docenteRepository.ObtenerGlobalesPorCarreraParaSolverAsync(idCarrera, ciclo);
                Dictionary<string, int> materiaDocenteAsignado = await _horarioRepository.CargarAsignacionesGrupoMateriaAsync(idCarrera);

                // 3. FILTRAR POR CICLO PERMITIENDO EXCEPCIONES (GRUPOS ESPECIALES)
                List<Grupo> grupos = gruposTodos.Where(g =>
                    g.EsEspecial ||
                    (ciclo == "A" && g.Semestre % 2 == 0) ||
                    (ciclo == "B" && g.Semestre % 2 != 0)
                ).ToList();

                // No filtramos las materias para permitir que grupos especiales lleven materias de cualquier semestre
                List<Materia> materias = materiasTodas;

                if (grupos.Count == 0 || materias.Count == 0)
                    return "Omitido: Sin grupos o materias para este ciclo.";

                if (!docentes.Any(d => d.IdDocente == -1))
                {
                    docentes.Add(new Docente
                    {
                        IdDocente = -1,
                        NombreCompleto = "Sin Maestro Asignado",
                        MateriasIds = new List<int>(),
                        HorasMaximas = 999,
                        HorasFrenteGrupo = 999
                    });
                }

                var (horasPrevias, bloquesOcupadosPrevios) = await _horarioRepository.CargarOcupacionPreviaAsync(idProyecto, idCarrera, docentes);

                // 4. RESOLVER MODELO MATEMÁTICO CP-SAT EN HILO DE FONDO (CPU-BOUND)
                var resultadoSolver = await Task.Run(() =>
                    ResolverModeloCpSat(grupos, materias, docentes, materiaDocenteAsignado, horasPrevias, bloquesOcupadosPrevios));

                if (!resultadoSolver.Exito)
                {
                    return "ERROR: Matemáticamente imposible o tiempo agotado.";
                }

                // 5. GUARDAR RESULTADOS TRANSACCIONALMENTE A TRAVÉS DEL REPOSITORIO
                int insertados = await _horarioRepository.GuardarResultadosSolverAsync(idProyecto, idCarrera, resultadoSolver.Slots);
                return $"EXITO: Horario generado. Insertados {insertados} registros.";
            }
            catch (Exception ex)
            {
                return $"EXCEPCIÓN: {ex.Message}";
            }
        }

        private static (bool Exito, List<AsignacionSlotSolverDto> Slots) ResolverModeloCpSat(
            List<Grupo> grupos,
            List<Materia> materias,
            List<Docente> docentes,
            Dictionary<string, int> materiaDocenteAsignado,
            Dictionary<int, int> horasPrevias,
            Dictionary<int, HashSet<string>> bloquesOcupadosPrevios)
        {
            int[] dias = { 1, 2, 3, 4, 5 };
            int[] bloquesMatutinosBase = { 1, 2, 3, 4, 5, 6 };
            int bloqueExtendidoMatutino = 7;
            int[] bloquesVespertinosBase = { 7, 8, 9, 10, 11, 12 };
            int bloqueExtendidoVespertino = 6;

            var bloquesPorGrupo = new Dictionary<int, List<int>>();
            var materiasPorGrupo = new Dictionary<int, List<Materia>>();

            // MAPEO EXACTO DE MATERIAS POR GRUPO (INCLUYENDO ESPECIALES)
            foreach (var g in grupos)
            {
                bool esMatutino = g.Turno.ToLower().Contains("matutino") || g.Turno.ToLower().StartsWith("m");
                var bloquesDelGrupo = new List<int>(esMatutino ? bloquesMatutinosBase : bloquesVespertinosBase);

                var materiasDelGrupo = materias.Where(m =>
                    (!g.EsEspecial && m.Semestre == g.Semestre) ||
                    materiaDocenteAsignado.ContainsKey($"{g.IdGrupo}_{m.IdMateria}")
                ).Distinct().ToList();

                materiasPorGrupo[g.IdGrupo] = materiasDelGrupo;

                int creditosTotales = materiasDelGrupo.Sum(m => m.Creditos);
                if (creditosTotales > 30)
                {
                    if (esMatutino) bloquesDelGrupo.Add(bloqueExtendidoMatutino);
                    else bloquesDelGrupo.Insert(0, bloqueExtendidoVespertino);
                }

                bloquesPorGrupo[g.IdGrupo] = bloquesDelGrupo;

                foreach (var m in materiasDelGrupo)
                {
                    string key = $"{g.IdGrupo}_{m.IdMateria}";
                    if (!materiaDocenteAsignado.ContainsKey(key))
                    {
                        materiaDocenteAsignado[key] = -1;
                    }
                }
            }

            var model = new CpModel();
            var asignaciones = new Dictionary<string, BoolVar>();
            var grupoHoraActiva = new Dictionary<string, BoolVar>();
            var penalizaciones = new List<LinearExpr>();

            // CREACIÓN DE VARIABLES
            foreach (var g in grupos)
            {
                var materiasDelGrupo = materiasPorGrupo[g.IdGrupo];
                var bloquesPermitidos = bloquesPorGrupo[g.IdGrupo];

                foreach (var d in dias)
                    foreach (var b in bloquesPermitidos)
                        grupoHoraActiva[$"{g.IdGrupo}_{d}_{b}"] = model.NewBoolVar($"Activa_{g.IdGrupo}_{d}_{b}");

                foreach (var m in materiasDelGrupo)
                {
                    int idMaestro = materiaDocenteAsignado[$"{g.IdGrupo}_{m.IdMateria}"];
                    var docente = docentes.FirstOrDefault(d => d.IdDocente == idMaestro)
                                  ?? docentes.First(d => d.IdDocente == -1);
                    bool tieneDisponibilidadConfigurada = docente.DisponibilidadBloques.Count > 0;

                    foreach (var d in dias)
                    {
                        for (int i = 0; i < bloquesPermitidos.Count; i++)
                        {
                            int b = bloquesPermitidos[i];
                            var variableClase = model.NewBoolVar($"X_{g.IdGrupo}_{m.IdMateria}_{d}_{b}");
                            asignaciones[$"{g.IdGrupo}_{m.IdMateria}_{d}_{b}"] = variableClase;

                            if (idMaestro != -1)
                            {
                                if (tieneDisponibilidadConfigurada && !docente.DisponibilidadBloques.Contains($"{d}_{b}"))
                                    model.Add(variableClase == 0);

                                if (bloquesOcupadosPrevios.TryGetValue(idMaestro, out var ocupados) && ocupados.Contains($"{d}_{b}"))
                                    model.Add(variableClase == 0);
                            }

                            if (b == bloqueExtendidoMatutino || b == bloqueExtendidoVespertino)
                                penalizaciones.Add(variableClase * 100);

                            penalizaciones.Add(variableClase * (i * 2));
                        }
                    }
                }
            }

            // RESTRICCIONES DE LOS ALUMNOS
            foreach (var g in grupos)
            {
                var materiasDelGrupo = materiasPorGrupo[g.IdGrupo];
                var bloquesPermitidos = bloquesPorGrupo[g.IdGrupo];

                foreach (var d in dias)
                {
                    foreach (var b in bloquesPermitidos)
                    {
                        var hVar = grupoHoraActiva[$"{g.IdGrupo}_{d}_{b}"];
                        var clasesEnEsteBloque = new List<BoolVar>();

                        foreach (var m in materiasDelGrupo)
                            clasesEnEsteBloque.Add(asignaciones[$"{g.IdGrupo}_{m.IdMateria}_{d}_{b}"]);

                        model.AddAtMostOne(clasesEnEsteBloque);
                        model.Add(hVar == LinearExpr.Sum(clasesEnEsteBloque));
                    }
                }

                foreach (var m in materiasDelGrupo)
                {
                    var todasHorasMateria = new List<BoolVar>();

                    foreach (var d in dias)
                    {
                        var horasEsteDia = new List<BoolVar>();
                        var starts = new List<BoolVar>();

                        for (int i = 0; i < bloquesPermitidos.Count; i++)
                        {
                            var b = bloquesPermitidos[i];
                            var x_curr = asignaciones[$"{g.IdGrupo}_{m.IdMateria}_{d}_{b}"];
                            horasEsteDia.Add(x_curr);
                            todasHorasMateria.Add(x_curr);

                            var is_start = model.NewBoolVar($"start_{g.IdGrupo}_{m.IdMateria}_{d}_{b}");
                            starts.Add(is_start);

                            if (i == 0) model.Add(is_start == x_curr);
                            else
                            {
                                var prev_b = bloquesPermitidos[i - 1];
                                var x_prev = asignaciones[$"{g.IdGrupo}_{m.IdMateria}_{d}_{prev_b}"];
                                model.AddImplication(is_start, x_curr);
                                model.AddImplication(is_start, x_prev.Not());
                                model.AddBoolOr(new ILiteral[] { is_start, x_curr.Not(), x_prev });
                            }
                        }

                        model.Add(LinearExpr.Sum(starts) <= 1);
                        model.Add(LinearExpr.Sum(horasEsteDia) <= 3);

                        var esUnaHora = model.NewBoolVar($"es_una_hora_{g.IdGrupo}_{m.IdMateria}_{d}");
                        var esTresHoras = model.NewBoolVar($"es_tres_horas_{g.IdGrupo}_{m.IdMateria}_{d}");

                        model.Add(LinearExpr.Sum(horasEsteDia) == 1).OnlyEnforceIf(esUnaHora);
                        model.Add(LinearExpr.Sum(horasEsteDia) != 1).OnlyEnforceIf(esUnaHora.Not());

                        model.Add(LinearExpr.Sum(horasEsteDia) == 3).OnlyEnforceIf(esTresHoras);
                        model.Add(LinearExpr.Sum(horasEsteDia) != 3).OnlyEnforceIf(esTresHoras.Not());

                        penalizaciones.Add(esUnaHora * 50);
                        penalizaciones.Add(esTresHoras * 500);
                    }

                    model.Add(LinearExpr.Sum(todasHorasMateria) <= m.Creditos);
                    var horasFaltantes = model.NewIntVar(0, m.Creditos, $"faltas_{g.IdGrupo}_{m.IdMateria}");
                    model.Add(horasFaltantes == m.Creditos - LinearExpr.Sum(todasHorasMateria));
                    penalizaciones.Add(horasFaltantes * 10000);
                }
            }

            // RESTRICCIONES ESTRICTAS DE PROFESORES
            foreach (var doc in docentes)
            {
                if (doc.IdDocente == -1) continue;

                var clasesSemanalesDocente = new List<BoolVar>();
                int ocupacionPrevia = horasPrevias.TryGetValue(doc.IdDocente, out int hp) ? hp : 0;
                int limiteRestante = Math.Max(0, doc.HorasFrenteGrupo - ocupacionPrevia);

                var diasConDisponibilidad = dias.Where(d =>
                    doc.DisponibilidadBloques.Count == 0 ||
                    doc.DisponibilidadBloques.Any(b => b.StartsWith($"{d}_"))
                ).ToList();

                int diasDisponibles = diasConDisponibilidad.Count > 0 ? diasConDisponibilidad.Count : 5;
                int maxHorasBalanceadas = (int)Math.Ceiling((double)limiteRestante / diasDisponibles) + 2;
                int limiteDiario = Math.Min(8, maxHorasBalanceadas);

                foreach (var d in dias)
                {
                    var clasesDiariasDocente = new List<BoolVar>();
                    var bloquesActivos = new Dictionary<int, BoolVar>();

                    for (int b = 1; b <= 12; b++)
                    {
                        var clasesEnEsteBloqueParaDocente = new List<BoolVar>();

                        foreach (var g in grupos)
                        {
                            var materiasDelGrupo = materiasPorGrupo[g.IdGrupo];
                            var bloquesPermitidos = bloquesPorGrupo[g.IdGrupo];

                            if (!bloquesPermitidos.Contains(b)) continue;

                            foreach (var m in materiasDelGrupo)
                            {
                                if (materiaDocenteAsignado[$"{g.IdGrupo}_{m.IdMateria}"] == doc.IdDocente)
                                {
                                    var varClase = asignaciones[$"{g.IdGrupo}_{m.IdMateria}_{d}_{b}"];
                                    clasesEnEsteBloqueParaDocente.Add(varClase);
                                    clasesDiariasDocente.Add(varClase);
                                    clasesSemanalesDocente.Add(varClase);
                                }
                            }
                        }

                        if (clasesEnEsteBloqueParaDocente.Count > 1)
                            model.AddAtMostOne(clasesEnEsteBloqueParaDocente);

                        if (clasesEnEsteBloqueParaDocente.Count > 0)
                        {
                            var bloqueActivo = model.NewBoolVar($"doc_{doc.IdDocente}_{d}_{b}_activo");
                            model.Add(bloqueActivo == LinearExpr.Sum(clasesEnEsteBloqueParaDocente));
                            bloquesActivos[b] = bloqueActivo;
                        }
                    }

                    if (clasesDiariasDocente.Count > 0)
                    {
                        model.Add(LinearExpr.Sum(clasesDiariasDocente) <= limiteDiario);

                        var trabajaEsteDia = model.NewBoolVar($"trabaja_{doc.IdDocente}_{d}");
                        model.Add(LinearExpr.Sum(clasesDiariasDocente) > 0).OnlyEnforceIf(trabajaEsteDia);
                        model.Add(LinearExpr.Sum(clasesDiariasDocente) == 0).OnlyEnforceIf(trabajaEsteDia.Not());

                        var startBlock = model.NewIntVar(1, 12, $"start_{doc.IdDocente}_{d}");
                        var endBlock = model.NewIntVar(1, 12, $"end_{doc.IdDocente}_{d}");

                        foreach (var kvp in bloquesActivos)
                        {
                            int b = kvp.Key;
                            var activo = kvp.Value;
                            model.Add(startBlock <= b).OnlyEnforceIf(activo);
                            model.Add(endBlock >= b).OnlyEnforceIf(activo);
                        }

                        model.Add(startBlock == 1).OnlyEnforceIf(trabajaEsteDia.Not());
                        model.Add(endBlock == 1).OnlyEnforceIf(trabajaEsteDia.Not());

                        var span = model.NewIntVar(0, 12, $"span_{doc.IdDocente}_{d}");
                        model.Add(span == endBlock - startBlock + 1).OnlyEnforceIf(trabajaEsteDia);
                        model.Add(span == 0).OnlyEnforceIf(trabajaEsteDia.Not());

                        var gaps = model.NewIntVar(0, 12, $"gaps_{doc.IdDocente}_{d}");
                        model.Add(gaps == span - LinearExpr.Sum(clasesDiariasDocente)).OnlyEnforceIf(trabajaEsteDia);
                        model.Add(gaps == 0).OnlyEnforceIf(trabajaEsteDia.Not());

                        penalizaciones.Add(gaps * 500);
                    }
                }

                if (clasesSemanalesDocente.Count > 0)
                {
                    model.Add(LinearExpr.Sum(clasesSemanalesDocente) <= limiteRestante);
                }
            }

            model.Minimize(LinearExpr.Sum(penalizaciones));

            var solver = new CpSolver
            {
                StringParameters = "max_time_in_seconds: 35.0"
            };
            CpSolverStatus status = solver.Solve(model);

            if (status != CpSolverStatus.Optimal && status != CpSolverStatus.Feasible)
            {
                return (false, new List<AsignacionSlotSolverDto>());
            }

            var slots = new List<AsignacionSlotSolverDto>();
            foreach (var g in grupos)
            {
                var materiasDelGrupo = materiasPorGrupo[g.IdGrupo];
                var bloquesPermitidos = bloquesPorGrupo[g.IdGrupo];

                foreach (var m in materiasDelGrupo)
                {
                    int maestroAsignado = materiaDocenteAsignado[$"{g.IdGrupo}_{m.IdMateria}"];
                    int? idDocenteReal = maestroAsignado > 0 ? maestroAsignado : null;

                    foreach (var d in dias)
                    {
                        foreach (var b in bloquesPermitidos)
                        {
                            string key = $"{g.IdGrupo}_{m.IdMateria}_{d}_{b}";
                            if (solver.Value(asignaciones[key]) == 1)
                            {
                                slots.Add(new AsignacionSlotSolverDto(g.IdGrupo, m.IdMateria, idDocenteReal, d, b));
                            }
                        }
                    }
                }
            }

            return (true, slots);
        }
    }
}

