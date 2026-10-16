using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GestorHorarios.Models;
using GestorHorarios.Repositories;
using GestorHorarios.Services;

namespace GestorHorarios.PROYECTOS
{
    public partial class NuevoProyectoView : UserControl
    {
        private readonly IProyectoRepository _proyectoRepository;
        private string _cicloSeleccionado = "";

        public NuevoProyectoView(IProyectoRepository? proyectoRepository = null)
        {
            InitializeComponent();
            _proyectoRepository = proyectoRepository ?? new ProyectoRepository();
            TxtAnio.Text = DateTime.Now.Year.ToString();
            TxtFechaCreacion.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }

        #region Seleccion de Ciclo

        private void SeleccionarCicloA_Click(object sender, MouseButtonEventArgs e)
        {
            SeleccionarCiclo("A");
        }

        private void SeleccionarCicloB_Click(object sender, MouseButtonEventArgs e)
        {
            SeleccionarCiclo("B");
        }

        private void SeleccionarCiclo(string ciclo)
        {
            _cicloSeleccionado = ciclo;

            if (ciclo == "A")
            {
                CardCicloA.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1565C0"));
                CardCicloB.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DDDDDD"));
                TxtPeriodo.Text = "Enero-Junio";
                TxtCicloSeleccionado.Text = "Ciclo A - Enero a Junio (2°, 4°, 6°, 8°)";
                TxtCicloSeleccionado.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1565C0"));
                BadgeCicloSeleccionado.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD"));
            }
            else
            {
                CardCicloB.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E7D32"));
                CardCicloA.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DDDDDD"));
                TxtPeriodo.Text = "Agosto-Diciembre";
                TxtCicloSeleccionado.Text = "Ciclo B - Agosto a Diciembre (1°, 3°, 5°, 7°, 9°)";
                TxtCicloSeleccionado.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E7D32"));
                BadgeCicloSeleccionado.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F5E9"));
            }

            PanelDatosProyecto.Visibility = Visibility.Visible;
            PanelDatosProyecto.BringIntoView();
        }

        #endregion

        #region Guardar y Navegar

        private void Siguiente_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_cicloSeleccionado))
            {
                MessageBox.Show("Selecciona un ciclo escolar.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtNombre.Text))
            {
                MessageBox.Show("Ingresa el nombre del proyecto.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(TxtAnio.Text, out int anio) || anio < 2020 || anio > 2100)
            {
                MessageBox.Show("Ingresa un año válido.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int idProyecto = _proyectoRepository.CrearProyecto(
                    TxtNombre.Text.Trim(), anio, TxtPeriodo.Text, _cicloSeleccionado);

                var proyecto = new Proyecto
                {
                    IdProyecto = idProyecto,
                    Nombre = TxtNombre.Text.Trim(),
                    Anio = anio,
                    Periodo = TxtPeriodo.Text,
                    Ciclo = _cicloSeleccionado,
                    FechaCreacion = DateTime.Now
                };

                NavigationService.GetFromWindow(this)?.NavigateTo(
                    new ProyectoDetalleView(proyecto));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el proyecto: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        private void Volver_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mw)
                mw.VolverAlDashboard();
        }
    }
}
