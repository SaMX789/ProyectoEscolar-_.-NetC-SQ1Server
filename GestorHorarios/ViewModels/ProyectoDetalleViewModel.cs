using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GestorHorarios.Models;
using GestorHorarios.Repositories;
using GestorHorarios.Services;

namespace GestorHorarios.ViewModels
{
    public class ProyectoDetalleViewModel : ViewModelBase
    {
        private readonly Proyecto _proyecto;
        private readonly IProyectoRepository _proyectoRepository;
        private readonly ICarreraRepository _carreraRepository;
        private readonly IGrupoRepository _grupoRepository;
        private readonly IMateriaRepository _materiaRepository;
        private readonly IDocenteRepository _docenteRepository;
        private readonly IHorarioRepository _horarioRepository;
        private readonly GeneradorHorariosService _generadorHorariosService;
        private readonly AsignadorSalonesService _asignadorSalonesService;
        private readonly GeneradorWordService _generadorWordService;

        public Proyecto Proyecto => _proyecto;

        public ProyectoDetalleViewModel(
            Proyecto proyecto,
            IProyectoRepository? proyectoRepository = null,
            ICarreraRepository? carreraRepository = null,
            IGrupoRepository? grupoRepository = null,
            IMateriaRepository? materiaRepository = null,
            IDocenteRepository? docenteRepository = null,
            IHorarioRepository? horarioRepository = null,
            GeneradorHorariosService? generadorHorariosService = null,
            AsignadorSalonesService? asignadorSalonesService = null,
            GeneradorWordService? generadorWordService = null)
        {
            _proyecto = proyecto;
            _proyectoRepository = proyectoRepository ?? new ProyectoRepository();
            _carreraRepository = carreraRepository ?? new CarreraRepository();
            _grupoRepository = grupoRepository ?? new GrupoRepository();
            _materiaRepository = materiaRepository ?? new MateriaRepository();
            _docenteRepository = docenteRepository ?? new DocenteRepository();
            _horarioRepository = horarioRepository ?? new HorarioRepository();
            _generadorHorariosService = generadorHorariosService ?? new GeneradorHorariosService(
                _proyectoRepository, _grupoRepository, _materiaRepository, _docenteRepository, _horarioRepository);
            _asignadorSalonesService = asignadorSalonesService ?? new AsignadorSalonesService(_horarioRepository);
            _generadorWordService = generadorWordService ?? new GeneradorWordService();
        }

        public List<Carrera> ObtenerCarreras() => _carreraRepository.ObtenerTodas();

        public bool TieneHorarioGenerado(int idCarrera) =>
            _proyectoRepository.TieneHorarioGenerado(_proyecto.IdProyecto, idCarrera);

        public int ContarGruposCiclo(int idCarrera) =>
            _grupoRepository.ContarPorCicloYCarrera(_proyecto.Ciclo, idCarrera);

        public List<CarreraCoberturaDto> ObtenerDiagnosticoCobertura() =>
            _proyectoRepository.ObtenerDiagnosticoCobertura(_proyecto.Ciclo);

        public Task<string> GenerarHorarioCarreraAsync(int idCarrera) =>
            _generadorHorariosService.EjecutarDiagnosticoAsync(_proyecto.IdProyecto, idCarrera);

        public Task<bool> AsignarSalonesAsync() =>
            _asignadorSalonesService.EjecutarAsignacionSalonesAsync(_proyecto.IdProyecto);

        public async Task<(int Exportadas, string NombreCarpeta)> ExportarTodasCarrerasWordAsync()
        {
            return await Task.Run(async () =>
            {
                string rutaEscritorio = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string nombreCarpeta = $"Horarios_Grupos_Ciclo{_proyecto.Ciclo}_{_proyecto.Anio}".Replace(" ", "_");
                string rutaCarpeta = Path.Combine(rutaEscritorio, nombreCarpeta);

                if (!Directory.Exists(rutaCarpeta))
                {
                    Directory.CreateDirectory(rutaCarpeta);
                }

                string tituloCiclo = $"Ciclo {_proyecto.Ciclo} · {_proyecto.Anio} · {_proyecto.Periodo}";
                int exportadas = 0;

                var carreras = await _carreraRepository.ObtenerTodasAsync();
                foreach (var carrera in carreras)
                {
                    var gruposData = new List<GeneradorWordService.GrupoData>();
                    var gruposLista = await _grupoRepository.ObtenerPorCicloYCarreraAsync(_proyecto.Ciclo, carrera.IdCarrera);

                    foreach (var grupo in gruposLista)
                    {
                        int totalCreditos = await _materiaRepository.SumarCreditosSemestreAsync(carrera.IdCarrera, grupo.Semestre);
                        var horariosAsignados = await _horarioRepository.ObtenerHorarioGrupoAsync(grupo.IdGrupo, _proyecto.IdProyecto);

                        gruposData.Add(new GeneradorWordService.GrupoData
                        {
                            NombreGrupo = grupo.Nombre,
                            Turno = string.IsNullOrWhiteSpace(grupo.Turno) ? "Matutino" : grupo.Turno,
                            TotalHoras = totalCreditos,
                            Horarios = horariosAsignados
                        });
                    }

                    if (gruposData.Count > 0)
                    {
                        string nombreArchivoLimpio = string.Join("_", carrera.Nombre.Split(Path.GetInvalidFileNameChars()));
                        string rutaArchivo = Path.Combine(rutaCarpeta, $"Horarios_{nombreArchivoLimpio}.docx");

                        _generadorWordService.ExportarHorarioGrupos(rutaArchivo, carrera.Nombre, tituloCiclo, gruposData);
                        exportadas++;
                    }
                }

                return (exportadas, nombreCarpeta);
            });
        }

        public async Task<(int ArchivosGenerados, string NombreCarpeta)> ExportarDocentesWordAsync()
        {
            return await Task.Run(async () =>
            {
                string rutaEscritorio = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string nombreCarpeta = $"Docentes_Ciclo{_proyecto.Ciclo}_{_proyecto.Anio}".Replace(" ", "_");
                string rutaCarpeta = Path.Combine(rutaEscritorio, nombreCarpeta);

                if (!Directory.Exists(rutaCarpeta))
                {
                    Directory.CreateDirectory(rutaCarpeta);
                }

                string tituloCiclo = $"Ciclo {_proyecto.Ciclo} · {_proyecto.Anio} · {_proyecto.Periodo}";
                var docentesLista = await _docenteRepository.ObtenerConHorarioPorProyectoAsync(_proyecto.IdProyecto);
                var docentesPorCarrera = new Dictionary<string, List<GeneradorWordService.DocenteDataWord>>();

                foreach (var docente in docentesLista)
                {
                    var horarioPersonal = await _horarioRepository.ObtenerHorarioPersonalDocenteAsync(_proyecto.IdProyecto, docente.IdDocente);

                    var datosMaestro = new GeneradorWordService.DocenteDataWord
                    {
                        NombreDocente = docente.Nombre,
                        Hfg = docente.HorasFrenteGrupo,
                        Horarios = horarioPersonal
                    };

                    if (!docentesPorCarrera.TryGetValue(docente.CarreraPrincipal, out var listaMaestros))
                    {
                        listaMaestros = new List<GeneradorWordService.DocenteDataWord>();
                        docentesPorCarrera[docente.CarreraPrincipal] = listaMaestros;
                    }
                    listaMaestros.Add(datosMaestro);
                }

                int archivosGenerados = 0;
                foreach (var carreraGrupo in docentesPorCarrera)
                {
                    string nombreCarrera = carreraGrupo.Key;
                    var listaMaestros = carreraGrupo.Value;

                    if (listaMaestros.Count > 0)
                    {
                        string nombreArchivoLimpio = string.Join("_", nombreCarrera.Split(Path.GetInvalidFileNameChars()));
                        string rutaArchivo = Path.Combine(rutaCarpeta, $"Docentes_{nombreArchivoLimpio}.docx");

                        _generadorWordService.ExportarHorariosDocentes(rutaArchivo, tituloCiclo, listaMaestros);
                        archivosGenerados++;
                    }
                }

                return (archivosGenerados, nombreCarpeta);
            });
        }
    }
}

