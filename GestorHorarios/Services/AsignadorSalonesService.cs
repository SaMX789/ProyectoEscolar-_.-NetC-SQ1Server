using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.OrTools.Sat;
using GestorHorarios.Repositories;

namespace GestorHorarios.Services
{
    public class AsignadorSalonesService
    {
        private readonly IHorarioRepository _horarioRepository;

        public AsignadorSalonesService(IHorarioRepository? horarioRepository = null)
        {
            _horarioRepository = horarioRepository ?? new HorarioRepository();
        }

        public async Task<bool> EjecutarAsignacionSalonesAsync(int idProyecto)
        {
            var (clasesTodas, salones) = await _horarioRepository.CargarDatosAsignacionSalonesAsync(idProyecto);

            var clasesPresenciales = clasesTodas.Where(c => !c.EnLinea).ToList();
            if (clasesPresenciales.Count == 0 || salones.Count == 0)
                return false;

            // Resolver el modelo matemático CP-SAT en hilo de fondo (CPU-bound)
            var asignaciones = await Task.Run(() => ResolverModeloSalonesCpSat(clasesPresenciales, salones));
            if (asignaciones == null)
                return false;

            // Persistir resultados de forma asíncrona y transaccional mediante el repositorio
            await _horarioRepository.GuardarAsignacionesSalonesAsync(idProyecto, asignaciones);
            return true;
        }

        private static Dictionary<int, int>? ResolverModeloSalonesCpSat(
            List<ClaseParaSalonDto> clasesPresenciales,
            List<EspacioFisicoSolverDto> salones)
        {
            var model = new CpModel();
            var varsSalon = new Dictionary<(int idDetalle, int idSalon), BoolVar>();
            var objetivo = LinearExpr.NewBuilder();

            foreach (var c in clasesPresenciales)
            {
                var posiblesSalonesDeEstaClase = new List<BoolVar>();
                foreach (var s in salones)
                {
                    var variable = model.NewBoolVar($"s_{c.IdDetalle}_{s.IdSalon}");
                    varsSalon[(c.IdDetalle, s.IdSalon)] = variable;
                    posiblesSalonesDeEstaClase.Add(variable);

                    // 1. Restricción de capacidad del salón frente al cupo del grupo
                    if (s.Capacidad > 0 && s.Capacidad < c.CupoAlumnos)
                    {
                        model.Add(variable == 0);
                        continue;
                    }

                    // 2. Restricción de Laboratorios parametrizada desde Base de Datos
                    if (c.RequiereLaboratorio && !s.EsLaboratorio)
                    {
                        model.Add(variable == 0);
                        continue;
                    }
                    if (!c.RequiereLaboratorio && s.EsLaboratorio)
                    {
                        model.Add(variable == 0);
                        continue;
                    }

                    // 3. Filtro estricto de carreras
                    bool salonCompartido = !s.IdCarrera.HasValue && !s.IdCarreraSecundaria.HasValue && !s.IdCarreraTerciaria.HasValue;
                    bool coincideCarrera = s.IdCarrera == c.IdCarreraGrupo ||
                                           s.IdCarreraSecundaria == c.IdCarreraGrupo ||
                                           s.IdCarreraTerciaria == c.IdCarreraGrupo;

                    if (!salonCompartido && !coincideCarrera)
                    {
                        model.Add(variable == 0);
                    }
                    else if (coincideCarrera)
                    {
                        objetivo.AddTerm(variable, 100);
                    }
                    else if (salonCompartido)
                    {
                        objetivo.AddTerm(variable, 20);
                    }
                }

                model.Add(LinearExpr.Sum(posiblesSalonesDeEstaClase) == 1);
            }

            // RESTRICCIÓN DURA: CERO COLISIONES DE ESPACIO
            var clasesPorMomento = clasesPresenciales.GroupBy(c => new { c.IdDia, c.IdBloque });
            foreach (var momento in clasesPorMomento)
            {
                foreach (var s in salones)
                {
                    var clasesAqui = momento.Select(c => varsSalon[(c.IdDetalle, s.IdSalon)]).ToList();
                    model.Add(LinearExpr.Sum(clasesAqui) <= 1);
                }
            }

            // ASIGNACIÓN DE "SALÓN BASE" (Minimizar la fragmentación de aulas por grupo)
            var grupos = clasesPresenciales.Select(c => c.IdGrupo).Distinct();
            foreach (var idGrupo in grupos)
            {
                var clasesDelGrupo = clasesPresenciales.Where(c => c.IdGrupo == idGrupo).ToList();

                foreach (var s in salones)
                {
                    var grupoUsaSalon = model.NewBoolVar($"grupo_{idGrupo}_usa_salon_{s.IdSalon}");
                    var clasesEnEsteSalon = clasesDelGrupo.Select(c => varsSalon[(c.IdDetalle, s.IdSalon)]).ToList();

                    foreach (var varClase in clasesEnEsteSalon)
                    {
                        model.AddImplication(varClase, grupoUsaSalon);
                    }

                    int castigoPorAbrirSalon = s.EsLaboratorio ? 500 : 5000;
                    objetivo.AddTerm(grupoUsaSalon, -castigoPorAbrirSalon);
                }
            }

            model.Maximize(objetivo);
            var solver = new CpSolver
            {
                StringParameters = "max_time_in_seconds: 15.0"
            };

            var status = solver.Solve(model);
            if (status != CpSolverStatus.Optimal && status != CpSolverStatus.Feasible)
                return null;

            var resultado = new Dictionary<int, int>();
            foreach (var c in clasesPresenciales)
            {
                foreach (var s in salones)
                {
                    if (solver.BooleanValue(varsSalon[(c.IdDetalle, s.IdSalon)]))
                    {
                        resultado[c.IdDetalle] = s.IdSalon;
                        break;
                    }
                }
            }

            return resultado;
        }
    }
}