using System.Collections.Generic;
using System.Threading.Tasks;
using GestorHorarios.Models;
using GestorHorarios.Repositories;

namespace GestorHorarios.ViewModels
{
    public class MainDashboardViewModel : ViewModelBase
    {
        private readonly IProyectoRepository _proyectoRepository;

        public MainDashboardViewModel(IProyectoRepository? proyectoRepository = null)
        {
            _proyectoRepository = proyectoRepository ?? new ProyectoRepository();
        }

        public EstadisticasDashboard ObtenerEstadisticas()
        {
            return _proyectoRepository.ObtenerEstadisticasDashboard();
        }

        public Task<EstadisticasDashboard> ObtenerEstadisticasAsync()
        {
            return _proyectoRepository.ObtenerEstadisticasDashboardAsync();
        }

        public List<Proyecto> ObtenerProyectosActivos()
        {
            return _proyectoRepository.ObtenerProyectosActivos();
        }

        public Task<List<Proyecto>> ObtenerProyectosActivosAsync()
        {
            return _proyectoRepository.ObtenerProyectosActivosAsync();
        }

        public void EliminarProyecto(int idProyecto)
        {
            _proyectoRepository.EliminarProyecto(idProyecto);
        }

        public Task EliminarProyectoAsync(int idProyecto)
        {
            return _proyectoRepository.EliminarProyectoAsync(idProyecto);
        }
    }
}

