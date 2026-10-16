using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GestorHorarios.Repositories;
using GestorHorarios.Services;

namespace GestorHorarios.DOCENTES
{
    public partial class DocentesView : UserControl
    {
        private readonly IDocenteRepository _docenteRepository;

        public DocentesView(IDocenteRepository? docenteRepository = null)
        {
            InitializeComponent();
            _docenteRepository = docenteRepository ?? new DocenteRepository();
            CargarConteoDocentes();
        }

        private void CargarConteoDocentes()
        {
            try
            {
                TextBlock[] bloques =
                {
                    TxtDocentesSistemas, TxtDocentesCivil, TxtDocentesComunitario,
                    TxtDocentesEmpresarial, TxtDocentesIndustrial,
                    TxtDocentesBioquimica, TxtDocentesIngles
                };

                var conteos = _docenteRepository.ContarPorCarrera();

                for (int i = 0; i < conteos.Count && i < bloques.Length; i++)
                {
                    bloques[i].Text = conteos[i].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al cargar los datos:\n\n{ex.Message}",
                                "Error de Base de Datos",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);

                System.Diagnostics.Debug.WriteLine($"Error cargando conteos: {ex.Message}");
            }
        }

        private void SeleccionarCarrera_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag != null
                && int.TryParse(border.Tag.ToString(), out int idCarrera))
            {
                NavigationService.GetFromWindow(this)?.NavigateTo(new IngenieriaSeleccionD(idCarrera));
            }
        }

        #region Lógica del Buscador de Docentes
        private void TxtBuscadorDocente_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filtro = TxtBuscadorDocente.Text.Trim();

            if (filtro.Length < 2)
            {
                PopupResultados.IsOpen = false;
                return;
            }

            List<DocenteBusquedaGlobalDto> resultados;
            try
            {
                resultados = _docenteRepository.BuscarGlobal(filtro);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en buscador: {ex.Message}");
                resultados = new List<DocenteBusquedaGlobalDto>();
            }

            if (resultados.Count > 0)
            {
                ListResultadosBusqueda.ItemsSource = resultados;
                PopupResultados.IsOpen = true;
            }
            else
            {
                PopupResultados.IsOpen = false;
            }
        }

        private void ListResultadosBusqueda_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListResultadosBusqueda.SelectedItem is not DocenteBusquedaGlobalDto seleccionado) return;

            PopupResultados.IsOpen = false;
            TxtBuscadorDocente.Clear();

            if (seleccionado.IdCarreraPrincipal <= 0)
            {
                MessageBox.Show("Este docente no tiene una carrera principal asignada, por lo que no se puede abrir su tarjeta directamente.",
                                "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var navService = NavigationService.GetFromWindow(this);
            if (navService != null)
            {
                var vistaDestino = new IngenieriaSeleccionD(seleccionado.IdCarreraPrincipal);
                navService.NavigateTo(vistaDestino);
            }
        }
        #endregion
    }
}