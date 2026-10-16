using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GestorHorarios.Models;
using GestorHorarios.Repositories;
using GestorHorarios.Services;
using GestorHorarios.ViewModels;

namespace GestorHorarios.PROYECTOS
{
    public partial class ProyectoDetalleView : UserControl
    {
        private readonly Proyecto _proyecto;
        private readonly ProyectoDetalleViewModel _viewModel;
        private List<Carrera> _listaCarreras = new();

        // VARIABLES PARA LA NAVEGACIÓN DEL MODAL DE DIAGNÓSTICO
        private readonly List<FrameworkElement> _encabezadosAreas = new();
        private int _indiceAreaActual = 0;

        public ProyectoDetalleView() : this(new Proyecto())
        {
        }

        public ProyectoDetalleView(Proyecto proyecto, ProyectoDetalleViewModel? viewModel = null)
        {
            InitializeComponent();
            _proyecto = proyecto;
            _viewModel = viewModel ?? new ProyectoDetalleViewModel(proyecto);
            DataContext = _viewModel;

            if (_proyecto.IdProyecto > 0 || !string.IsNullOrEmpty(_proyecto.Nombre))
            {
                CargarEncabezado();
                CargarCarreras();
            }
        }

        private void CargarEncabezado()
        {
            TxtTituloProyecto.Text = $"{_proyecto.Nombre} del {_proyecto.Anio}";

            var cultura = new CultureInfo("es-MX");
            string fechaFormateada = _proyecto.FechaCreacion.ToString("dd 'de' MMMM 'de' yyyy", cultura);
            TxtFechaProyecto.Text = $"Fecha de creacion: {fechaFormateada}";

            if (_proyecto.Ciclo == "A")
            {
                TxtCicloInfo.Text = "Ciclo A - Semestres: 2, 4, 6, 8";
                TxtCicloInfo.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1565C0"));
                BadgeCiclo.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD"));
            }
            else
            {
                TxtCicloInfo.Text = "Ciclo B - Semestres: 1, 3, 5, 7, 9";
                TxtCicloInfo.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E7D32"));
                BadgeCiclo.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F5E9"));
            }
        }

        private void CargarCarreras()
        {
            try
            {
                GridCarreras.Children.Clear();
                GridCarreras.RowDefinitions.Clear();

                _listaCarreras = _viewModel.ObtenerCarreras();

                int totalFilas = (int)Math.Ceiling(_listaCarreras.Count / 3.0);
                for (int r = 0; r < totalFilas; r++)
                {
                    GridCarreras.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                }

                for (int i = 0; i < _listaCarreras.Count; i++)
                {
                    var carrera = _listaCarreras[i];
                    int row = i / 3;
                    int col = i % 3;

                    bool estaGenerado = _viewModel.TieneHorarioGenerado(carrera.IdCarrera);

                    string iconoGlyph = "\uE82D";
                    string nombreUpper = carrera.Nombre.ToUpperInvariant();

                    if (nombreUpper.Contains("SISTEMAS")) iconoGlyph = "\uE99A";
                    else if (nombreUpper.Contains("CIVIL")) iconoGlyph = "\uE913";
                    else if (nombreUpper.Contains("COMUNITARIO")) iconoGlyph = "\uE716";
                    else if (nombreUpper.Contains("EMPRESARIAL")) iconoGlyph = "\uE9D9";
                    else if (nombreUpper.Contains("INDUSTRIAL")) iconoGlyph = "\uE713";
                    else if (nombreUpper.Contains("BIOQUIMICA")) iconoGlyph = "\uE913";
                    else if (nombreUpper.Contains("INGLES") || nombreUpper.Contains("IDIOMA")) iconoGlyph = "\uE90A";

                    var border = new Border
                    {
                        Style = (Style)FindResource("CarreraCardStyle"),
                        Tag = carrera.IdCarrera
                    };
                    border.MouseLeftButtonDown += CarreraCard_Click;

                    var mainGrid = new Grid();
                    var sp = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };

                    sp.Children.Add(new TextBlock
                    {
                        Text = iconoGlyph,
                        FontFamily = new FontFamily("Segoe MDL2 Assets"),
                        FontSize = 42,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#757575")),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 0, 0, 12)
                    });

                    sp.Children.Add(new TextBlock
                    {
                        Text = carrera.Nombre,
                        FontSize = 16,
                        FontWeight = FontWeights.Bold,
                        Foreground = (Brush)FindResource("GuindaBajo"),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        TextAlignment = TextAlignment.Center,
                        TextWrapping = TextWrapping.Wrap
                    });

                    int totalGrupos = _viewModel.ContarGruposCiclo(carrera.IdCarrera);
                    sp.Children.Add(new TextBlock
                    {
                        Text = $"{totalGrupos} {(totalGrupos == 1 ? "grupo" : "grupos")}",
                        FontSize = 12,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#888888")),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 4, 0, 0)
                    });

                    var statusIcon = new TextBlock
                    {
                        Text = estaGenerado ? "[ OK ]" : "[ Pendiente ]",
                        FontSize = 12,
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(estaGenerado ? "#2E7D32" : "#D32F2F")),
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(0, -5, -5, 0)
                    };

                    mainGrid.Children.Add(sp);
                    mainGrid.Children.Add(statusIcon);
                    border.Child = mainGrid;

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, col);
                    GridCarreras.Children.Add(border);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cargando carreras: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAnalisisPrevio_Click(object sender, RoutedEventArgs e)
        {
            PanelResultadosDiagnostico.Children.Clear();
            _encabezadosAreas.Clear();
            _indiceAreaActual = 0;

            try
            {
                var carrerasCobertura = _viewModel.ObtenerDiagnosticoCobertura();

                if (carrerasCobertura.Count == 0)
                {
                    PanelResultadosDiagnostico.Children.Add(new TextBlock
                    {
                        Text = "No hay grupos creados para este ciclo. Crea los grupos primero.",
                        FontStyle = FontStyles.Italic,
                        Foreground = Brushes.Gray,
                        Margin = new Thickness(10)
                    });
                    ModalDiagnostico.Visibility = Visibility.Visible;
                    return;
                }

                foreach (var carrera in carrerasCobertura)
                {
                    var containerCarrera = new StackPanel
                    {
                        Visibility = _encabezadosAreas.Count == 0 ? Visibility.Visible : Visibility.Collapsed
                    };

                    var headerStack = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 15) };
                    bool faltanMaestros = carrera.TotalFaltantes > 0;

                    headerStack.Children.Add(new TextBlock
                    {
                        Text = faltanMaestros ? "\uE783" : "\uE73E",
                        FontFamily = new FontFamily("Segoe MDL2 Assets"),
                        FontSize = 20,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(faltanMaestros ? "#D32F2F" : "#2E7D32")),
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 0, 10, 0)
                    });

                    headerStack.Children.Add(new TextBlock
                    {
                        Text = $"{carrera.NombreCarrera.ToUpperInvariant()} — {(faltanMaestros ? $"Faltan {carrera.TotalFaltantes} asignaciones" : "Cobertura Completa")}",
                        FontSize = 18,
                        FontWeight = FontWeights.ExtraBold,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1565C0")),
                        VerticalAlignment = VerticalAlignment.Center
                    });

                    containerCarrera.Children.Add(headerStack);

                    var gruposPorSemestre = carrera.Grupos.GroupBy(g => g.Semestre).OrderBy(g => g.Key);
                    foreach (var semestre in gruposPorSemestre)
                    {
                        containerCarrera.Children.Add(new TextBlock
                        {
                            Text = $"Semestre {semestre.Key}",
                            FontSize = 14,
                            FontWeight = FontWeights.SemiBold,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666")),
                            Margin = new Thickness(0, 10, 0, 5)
                        });

                        foreach (var grupo in semestre.OrderBy(g => g.NombreGrupo))
                        {
                            containerCarrera.Children.Add(CrearTarjetaGrupoCobertura(grupo));
                        }
                    }

                    PanelResultadosDiagnostico.Children.Add(containerCarrera);
                    _encabezadosAreas.Add(containerCarrera);
                }

                ModalDiagnostico.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar el diagnóstico: {ex.Message}", "Error de BD", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Border CrearTarjetaGrupoCobertura(GrupoCoberturaDto grupo)
        {
            var borderPrincipal = new Border
            {
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(0, 5, 0, 10),
                Padding = new Thickness(0),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    Opacity = 0.05,
                    BlurRadius = 5,
                    ShadowDepth = 1,
                    Direction = 270
                }
            };

            var mainStack = new StackPanel();

            var headerBorder = new Border
            {
                Background = Brushes.Transparent,
                Padding = new Thickness(15, 12, 15, 12),
                Cursor = Cursors.Hand
            };

            var gridHeader = new Grid();
            gridHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            gridHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            gridHeader.Children.Add(new TextBlock
            {
                Text = grupo.NombreGrupo,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333")),
                VerticalAlignment = VerticalAlignment.Center
            });

            var tbEstatus = new TextBlock
            {
                Text = grupo.Faltantes > 0 ? grupo.Faltantes.ToString() : "\uE73E",
                FontFamily = grupo.Faltantes > 0 ? new FontFamily("Segoe UI") : new FontFamily("Segoe MDL2 Assets"),
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(grupo.Faltantes > 0 ? "#D32F2F" : "#2E7D32")),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(tbEstatus, 1);
            gridHeader.Children.Add(tbEstatus);
            headerBorder.Child = gridHeader;
            mainStack.Children.Add(headerBorder);

            var contentBorder = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F9F9F9")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EEEEEE")),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Padding = new Thickness(15, 10, 15, 10),
                Visibility = Visibility.Collapsed
            };

            var stackMaterias = new StackPanel();
            foreach (var materia in grupo.Materias.OrderBy(m => m.NombreMateria))
            {
                var gridMateria = new Grid { Margin = new Thickness(0, 0, 0, 8) };
                gridMateria.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                gridMateria.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                gridMateria.Children.Add(new TextBlock
                {
                    Text = materia.NombreMateria,
                    FontSize = 13,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#424242")),
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 10, 0)
                });

                var tbMaestro = new TextBlock
                {
                    Text = materia.EstaAsignado ? materia.DocenteAsignado : "Sin Asignar",
                    FontSize = 13,
                    FontWeight = materia.EstaAsignado ? FontWeights.SemiBold : FontWeights.Bold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(materia.EstaAsignado ? "#1565C0" : "#D32F2F")),
                    TextAlignment = TextAlignment.Right
                };
                Grid.SetColumn(tbMaestro, 1);
                gridMateria.Children.Add(tbMaestro);

                stackMaterias.Children.Add(gridMateria);
            }

            contentBorder.Child = stackMaterias;
            mainStack.Children.Add(contentBorder);

            headerBorder.MouseLeftButtonDown += (s, ev) =>
            {
                contentBorder.Visibility = contentBorder.Visibility == Visibility.Visible
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            };

            borderPrincipal.Child = mainStack;
            return borderPrincipal;
        }

        private async void BtnExportarTodasCarreras_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TxtProgreso.Text = "Exportando horarios de todas las carreras...";
                TxtProgreso.Visibility = Visibility.Visible;
                BarraProgreso.Visibility = Visibility.Visible;
                BarraProgreso.IsIndeterminate = true;

                var (exportadas, nombreCarpeta) = await _viewModel.ExportarTodasCarrerasWordAsync();

                MessageBox.Show(
                    $"¡Se generaron {exportadas} documentos de Word!\n\nEstán dentro de la carpeta:\n{nombreCarpeta}\nen tu Escritorio.",
                    "Exportación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error en la exportación:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BarraProgreso.IsIndeterminate = false;
                BarraProgreso.Visibility = Visibility.Collapsed;
                TxtProgreso.Visibility = Visibility.Collapsed;
            }
        }

        private async void BtnExportarDocentes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TxtProgreso.Text = "Organizando y exportando docentes por carrera...";
                TxtProgreso.Visibility = Visibility.Visible;
                BarraProgreso.Visibility = Visibility.Visible;
                BarraProgreso.IsIndeterminate = true;

                var (archivosGenerados, nombreCarpeta) = await _viewModel.ExportarDocentesWordAsync();

                MessageBox.Show(
                    $"¡Se generaron {archivosGenerados} archivos de Word divididos por departamento!\n\nEstán en la carpeta:\n{nombreCarpeta}\nen tu Escritorio.",
                    "Exportación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BarraProgreso.IsIndeterminate = false;
                BarraProgreso.Visibility = Visibility.Collapsed;
                TxtProgreso.Visibility = Visibility.Collapsed;
            }
        }

        private async void BtnAsignarSalones_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Este proceso asignará salones a los horarios ya generados, minimizando los cambios de aula.\n\n¿Deseas continuar?",
                "Asignar Salones", MessageBoxButton.YesNo, MessageBoxImage.Information);

            if (result != MessageBoxResult.Yes) return;

            BtnGenerarTodos.IsEnabled = false;
            BtnAnalisisPrevio.IsEnabled = false;
            BtnAsignarSalones.IsEnabled = false;
            BarraProgreso.Visibility = Visibility.Visible;
            TxtProgreso.Visibility = Visibility.Visible;
            BarraProgreso.IsIndeterminate = true;
            TxtProgreso.Text = "Calculando la distribución óptima de salones...";

            try
            {
                bool exito = await _viewModel.AsignarSalonesAsync();

                if (exito)
                {
                    MessageBox.Show("¡Los salones han sido asignados exitosamente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("No se encontró una distribución viable. Revisa si hay más clases simultáneas que salones.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al asignar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnGenerarTodos.IsEnabled = true;
                BtnAnalisisPrevio.IsEnabled = true;
                BtnAsignarSalones.IsEnabled = true;
                BarraProgreso.IsIndeterminate = false;
                BarraProgreso.Visibility = Visibility.Collapsed;
                TxtProgreso.Visibility = Visibility.Collapsed;
            }
        }

        private void CerrarModalDiagnostico_Click(object sender, RoutedEventArgs e)
        {
            ModalDiagnostico.Visibility = Visibility.Collapsed;
        }

        private void BtnAreaAnterior_Click(object sender, RoutedEventArgs e)
        {
            if (_encabezadosAreas.Count == 0 || _indiceAreaActual <= 0) return;

            _encabezadosAreas[_indiceAreaActual].Visibility = Visibility.Collapsed;
            _indiceAreaActual--;
            _encabezadosAreas[_indiceAreaActual].Visibility = Visibility.Visible;
        }

        private void BtnAreaSiguiente_Click(object sender, RoutedEventArgs e)
        {
            if (_encabezadosAreas.Count == 0 || _indiceAreaActual >= _encabezadosAreas.Count - 1) return;

            _encabezadosAreas[_indiceAreaActual].Visibility = Visibility.Collapsed;
            _indiceAreaActual++;
            _encabezadosAreas[_indiceAreaActual].Visibility = Visibility.Visible;
        }

        private async void BtnGenerarTodos_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Este proceso borrara los horarios actuales y generara nuevos para TODAS las carreras.\n\n¿Deseas continuar?",
                "Generar Horarios Globales", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            BtnGenerarTodos.IsEnabled = false;
            BtnAnalisisPrevio.IsEnabled = false;
            BarraProgreso.Visibility = Visibility.Visible;
            TxtProgreso.Visibility = Visibility.Visible;
            BarraProgreso.Value = 0;

            int total = _listaCarreras.Count;
            int completadas = 0;
            int exitos = 0;
            string reporteErrores = "";

            try
            {
                foreach (var carrera in _listaCarreras)
                {
                    TxtProgreso.Text = $"Calculando {carrera.Nombre}... ({completadas}/{total})";

                    string resultado = await _viewModel.GenerarHorarioCarreraAsync(carrera.IdCarrera);

                    if (resultado.StartsWith("EXITO"))
                    {
                        exitos++;
                    }
                    else if (!resultado.StartsWith("Omitido"))
                    {
                        reporteErrores += $"\n- {carrera.Nombre}: {resultado}\n";
                    }

                    completadas++;
                    if (total > 0)
                        BarraProgreso.Value = (completadas * 100) / total;
                }

                if (string.IsNullOrEmpty(reporteErrores))
                {
                    MessageBox.Show($"¡Todos los horarios han sido generados exitosamente!\n\nSe procesaron {exitos} carreras.", "Exito Total", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"Se generaron {exitos} horarios, pero hubo problemas en las siguientes carreras:\n{reporteErrores}", "Reporte de Errores", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrio un error critico: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnGenerarTodos.IsEnabled = true;
                BtnAnalisisPrevio.IsEnabled = true;
                BarraProgreso.Visibility = Visibility.Collapsed;
                TxtProgreso.Visibility = Visibility.Collapsed;

                CargarCarreras();
            }
        }

        private void CarreraCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is int idCarrera)
            {
                bool estaGenerado = _viewModel.TieneHorarioGenerado(idCarrera);

                if (estaGenerado)
                {
                    NavigationService.GetFromWindow(this)?.NavigateTo(new HorarioCarreraView(_proyecto, idCarrera));
                }
                else
                {
                    MessageBox.Show(
                        "Aun no hay un horario calculado para esta carrera.\n\nPor favor, haz clic en el boton verde de arriba 'Generar Todos los Horarios'.",
                        "Horario Pendiente", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void Volver_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mw)
                mw.VolverAlDashboard();
        }
    }
}