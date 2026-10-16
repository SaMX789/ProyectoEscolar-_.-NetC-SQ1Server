using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GestorHorarios.Models;
using GestorHorarios.Repositories;
using GestorHorarios.Services;

namespace GestorHorarios.GRUPOS
{
    public partial class IngenieriaSeleccionG : UserControl
    {
        private readonly int _idCarrera;
        private readonly ICarreraRepository _carreraRepository;
        private readonly IGrupoRepository _grupoRepository;
        private int? _idGrupoEnEdicion = null;

        public IngenieriaSeleccionG()
        {
            InitializeComponent();
            _carreraRepository = new CarreraRepository();
            _grupoRepository = new GrupoRepository();
        }

        public IngenieriaSeleccionG(int idCarrera) : this()
        {
            _idCarrera = idCarrera;
            CargarTituloCarrera();
            CargarOpcionesFormulario();
            CargarGrupos();
        }

        private void CargarOpcionesFormulario()
        {
            if (ComboBoxSemestre != null)
            {
                ComboBoxSemestre.Items.Clear();
                for (int i = 1; i <= 9; i++)
                {
                    ComboBoxSemestre.Items.Add(new ComboBoxItem
                    {
                        Content = $"Semestre {i}",
                        Tag = i
                    });
                }
            }

            if (ComboBoxTurno != null)
            {
                ComboBoxTurno.Items.Clear();
                ComboBoxTurno.Items.Add(new ComboBoxItem { Content = "Matutino", Tag = "Matutino" });
                ComboBoxTurno.Items.Add(new ComboBoxItem { Content = "Vespertino", Tag = "Vespertino" });
            }
        }

        private void CargarTituloCarrera()
        {
            try
            {
                var resultado = _carreraRepository.ObtenerNombre(_idCarrera);
                TituloCarrera.Text = $"Grupos — {resultado}";
            }
            catch
            {
                TituloCarrera.Text = "Grupos";
            }
        }

        private void CargarGrupos()
        {
            PanelCicloA.Children.Clear();
            PanelCicloB.Children.Clear();

            try
            {
                var grupos = _grupoRepository.ObtenerPorCarrera(_idCarrera);

                var cicloB = grupos.Where(g => g.Semestre % 2 != 0).OrderBy(g => g.Semestre).ToList();
                var cicloA = grupos.Where(g => g.Semestre % 2 == 0).OrderBy(g => g.Semestre).ToList();

                PopularPanel(PanelCicloB, cicloB);
                PopularPanel(PanelCicloA, cicloA);

                if (cicloB.Count == 0) PanelCicloB.Children.Add(CrearMensajeVacio());
                if (cicloA.Count == 0) PanelCicloA.Children.Add(CrearMensajeVacio());
            }
            catch (Exception ex)
            {
                PanelCicloB.Children.Add(new TextBlock
                {
                    Text = $"Error al cargar grupos: {ex.Message}",
                    Foreground = Brushes.Red,
                    Margin = new Thickness(0, 4, 0, 4)
                });
            }
        }

        private void PopularPanel(StackPanel panel, List<Grupo> grupos)
        {
            var porSemestre = grupos.GroupBy(g => g.Semestre).OrderBy(g => g.Key);

            foreach (var semGrupo in porSemestre)
            {
                panel.Children.Add(new TextBlock
                {
                    Text = $"Semestre {semGrupo.Key}",
                    FontSize = 13,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#444444")),
                    Margin = new Thickness(0, 8, 0, 4)
                });

                foreach (var grupo in semGrupo)
                    panel.Children.Add(CrearCardGrupo(grupo));
            }
        }

        private Border CrearCardGrupo(Grupo grupo)
        {
            var border = new Border
            {
                Style = (Style)FindResource("GrupoCardStyle"),
                Tag = grupo,
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DDDDDD")),
                BorderThickness = new Thickness(1)
            };

            if (grupo.EsEspecial)
            {
                border.BorderBrush = new SolidColorBrush(Colors.DarkOrange);
                border.BorderThickness = new Thickness(2);
            }

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var nombreText = new TextBlock
            {
                Text = grupo.Nombre,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333")),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(nombreText, 0);
            grid.Children.Add(nombreText);

            bool esMatutino = grupo.Turno.ToLower().Contains("matutino");
            string badgeBgColor = esMatutino ? "#E8F5E9" : "#FFF3E0";
            string badgeFgColor = esMatutino ? "#2E7D32" : "#E65100";

            var turnoBadge = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(badgeBgColor)),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(10, 4, 10, 4),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };
            turnoBadge.Child = new TextBlock
            {
                Text = $"Turno {grupo.Turno}",
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(badgeFgColor))
            };
            Grid.SetColumn(turnoBadge, 1);
            grid.Children.Add(turnoBadge);

            var buttonStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0)
            };

            string pathLapiz = "M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25zM20.71 7.04c.39-.39.39-1.02 0-1.41l-2.34-2.34c-.39-.39-1.02-.39-1.41 0l-1.83 1.83 3.75 3.75 1.83-1.83z";
            var btnEditar = CrearBotonAccion("Editar", pathLapiz, "#475569");
            btnEditar.Tag = grupo;
            btnEditar.Click += EditarGrupo_Click;

            string pathBasura = "M6 19c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7H6v12zM19 4h-3.5l-1-1h-5l-1 1H5v2h14V4z";
            var btnEliminar = CrearBotonAccion("Eliminar", pathBasura, "#E11D48");
            btnEliminar.Tag = grupo.IdGrupo;
            btnEliminar.Click += EliminarGrupo_Click;

            buttonStack.Children.Add(btnEditar);
            buttonStack.Children.Add(btnEliminar);

            Grid.SetColumn(buttonStack, 2);
            grid.Children.Add(buttonStack);

            border.Child = grid;
            return border;
        }

        private Button CrearBotonAccion(string texto, string pathData, string bgColor)
        {
            var btn = new Button
            {
                ToolTip = texto,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bgColor)),
                Style = (Style)FindResource("ModernButtonStyle"),
                Margin = new Thickness(6, 0, 0, 0)
            };

            var path = new System.Windows.Shapes.Path
            {
                Data = Geometry.Parse(pathData),
                Fill = Brushes.White,
                Stretch = Stretch.Uniform,
                Width = 13,
                Height = 13
            };

            var sp = new StackPanel { Orientation = Orientation.Horizontal };
            sp.Children.Add(path);
            sp.Children.Add(new TextBlock { Text = texto, Margin = new Thickness(6, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center, FontSize = 12, FontWeight = FontWeights.SemiBold });

            btn.Content = sp;
            return btn;
        }

        private static TextBlock CrearMensajeVacio() => new()
        {
            Text = "No hay grupos registrados para este ciclo.",
            FontSize = 13,
            Foreground = Brushes.Gray,
            FontStyle = FontStyles.Italic,
            Margin = new Thickness(0, 4, 0, 4)
        };

        private void BotonMostrarAgregarGrupo_Click(object sender, RoutedEventArgs e)
        {
            if (PanelFormularioGrupo.Visibility == Visibility.Collapsed)
            {
                PanelFormularioGrupo.Visibility = Visibility.Visible;
                BotonMostrarAgregarGrupo.Content = "X Cerrar formulario";
                BotonMostrarAgregarGrupo.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#475569"));
            }
            else
            {
                LimpiarFormulario();
                PanelFormularioGrupo.Visibility = Visibility.Collapsed;
                BotonMostrarAgregarGrupo.Content = "+ Agregar Grupo";
                BotonMostrarAgregarGrupo.Background = (Brush)FindResource("GuindaBajo");
            }
        }

        private void LimpiarFormulario()
        {
            TextboxNombre.Clear();
            ComboBoxSemestre.SelectedIndex = -1;
            ComboBoxTurno.SelectedIndex = -1;
            CheckBoxEsEspecial.IsChecked = false;
            _idGrupoEnEdicion = null;
            BotonGuardarGrupo.Content = "Guardar";
        }

        private void EditarGrupo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Grupo grupo)
            {
                _idGrupoEnEdicion = grupo.IdGrupo;
                TextboxNombre.Text = grupo.Nombre;

                foreach (ComboBoxItem item in ComboBoxSemestre.Items)
                {
                    if ((int)item.Tag == grupo.Semestre)
                    {
                        ComboBoxSemestre.SelectedItem = item;
                        break;
                    }
                }

                foreach (ComboBoxItem item in ComboBoxTurno.Items)
                {
                    if (item.Tag.ToString() == grupo.Turno)
                    {
                        ComboBoxTurno.SelectedItem = item;
                        break;
                    }
                }

                CheckBoxEsEspecial.IsChecked = grupo.EsEspecial;

                BotonGuardarGrupo.Content = "Actualizar";
                BotonMostrarAgregarGrupo.Content = "Cancelar edición";
                BotonMostrarAgregarGrupo.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#475569"));
                PanelFormularioGrupo.Visibility = Visibility.Visible;
            }
        }

        private void BotonGuardarGrupo_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TextboxNombre.Text) ||
                ComboBoxSemestre.SelectedItem == null ||
                ComboBoxTurno.SelectedItem == null)
            {
                MessageBox.Show("Por favor, llena todos los campos.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int semestreSeleccionado = (int)((ComboBoxItem)ComboBoxSemestre.SelectedItem).Tag;
            string turnoSeleccionado = ((ComboBoxItem)ComboBoxTurno.SelectedItem).Tag.ToString()!;
            bool esEspecial = CheckBoxEsEspecial.IsChecked == true;

            try
            {
                var grupo = new Grupo
                {
                    IdGrupo = _idGrupoEnEdicion ?? 0,
                    Nombre = TextboxNombre.Text.Trim().ToUpperInvariant(),
                    Semestre = semestreSeleccionado,
                    Turno = turnoSeleccionado,
                    IdCarrera = _idCarrera,
                    EsEspecial = esEspecial
                };

                _grupoRepository.Guardar(grupo);

                MessageBox.Show(_idGrupoEnEdicion == null ? "Grupo guardado exitosamente." : "Grupo actualizado exitosamente.",
                                "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                LimpiarFormulario();
                PanelFormularioGrupo.Visibility = Visibility.Collapsed;
                BotonMostrarAgregarGrupo.Content = "+ Agregar Grupo";
                BotonMostrarAgregarGrupo.Background = (Brush)FindResource("GuindaBajo");

                CargarGrupos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el grupo:\n\n{ex.Message}", "Error de BD", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BotonCancelarGrupo_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
            PanelFormularioGrupo.Visibility = Visibility.Collapsed;
            BotonMostrarAgregarGrupo.Content = "+ Agregar Grupo";
            BotonMostrarAgregarGrupo.Background = (Brush)FindResource("GuindaBajo");
        }

        private void EliminarGrupo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not int idGrupo) return;

            var resultado = MessageBox.Show(
                "¿Está seguro de eliminar este grupo de forma permanente?",
                "Eliminar Grupo", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (resultado != MessageBoxResult.Yes) return;

            try
            {
                _grupoRepository.Eliminar(idGrupo);

                MessageBox.Show("Grupo eliminado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CargarGrupos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar (Revisa si el grupo ya tiene un horario asignado): {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void VolverCarreras_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GetFromWindow(this)?.NavigateTo(new GruposView());
        }
    }
}