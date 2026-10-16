using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GestorHorarios.Models;
using GestorHorarios.Repositories;

namespace GestorHorarios.SALONES
{
    public partial class SalonesView : UserControl
    {
        private readonly ISalonRepository _salonRepository;
        private readonly ICarreraRepository _carreraRepository;
        private int? _idSalonEnEdicion = null;
        private readonly List<(int Id, string Nombre)> _edificios = new();
        private readonly List<(int Id, string Nombre)> _carreras = new();

        public SalonesView(ISalonRepository? salonRepository = null, ICarreraRepository? carreraRepository = null)
        {
            InitializeComponent();
            _salonRepository = salonRepository ?? new SalonRepository();
            _carreraRepository = carreraRepository ?? new CarreraRepository();
            CargarEdificios();
            CargarCarreras();
            CargarSalones();
        }

        // ── Carga de datos ──────────────────────────────────────────────

        private void CargarEdificios()
        {
            try
            {
                var edificios = _salonRepository.ObtenerEdificios();
                foreach (var (id, nombre) in edificios)
                {
                    _edificios.Add((id, nombre));
                    ComboBoxEdificio.Items.Add(new ComboBoxItem
                    {
                        Content = nombre,
                        Tag = id
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando edificios: {ex.Message}");
            }
        }

        private void CargarCarreras()
        {
            try
            {
                var carreras = _carreraRepository.ObtenerTodas();
                foreach (var c in carreras)
                {
                    _carreras.Add((c.IdCarrera, c.Nombre));

                    ComboBoxCarrera1.Items.Add(new ComboBoxItem { Content = c.Nombre, Tag = c.IdCarrera });
                    ComboBoxCarrera2.Items.Add(new ComboBoxItem { Content = c.Nombre, Tag = c.IdCarrera });
                    ComboBoxCarrera3.Items.Add(new ComboBoxItem { Content = c.Nombre, Tag = c.IdCarrera });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando carreras: {ex.Message}");
            }
        }

        private void CargarSalones()
        {
            ListaSalones.Children.Clear();

            try
            {
                var salones = _salonRepository.ObtenerTodos();

                if (salones.Count == 0)
                {
                    ListaSalones.Children.Add(new TextBlock
                    {
                        Text = "No hay salones registrados.",
                        FontSize = 14,
                        Foreground = Brushes.Gray,
                        FontStyle = FontStyles.Italic,
                        Margin = new Thickness(0, 8, 0, 8)
                    });
                    return;
                }

                TxtTotalSalones.Text = salones.Count.ToString();
                TxtHorasSemanales.Text = $"{salones.Count * 12 * 5:N0}h";

                foreach (var salon in salones)
                    ListaSalones.Children.Add(CrearCardSalon(salon));
            }
            catch (Exception ex)
            {
                ListaSalones.Children.Add(new TextBlock
                {
                    Text = $"Error al cargar salones: {ex.Message}",
                    Foreground = Brushes.Red,
                    Margin = new Thickness(0, 8, 0, 8)
                });
            }
        }

        // ── Creación de tarjeta ─────────────────────────────────────────

        private Border CrearCardSalon(Salon salon)
        {
            var border = new Border
            {
                Style = (Style)FindResource("SalonCardStyle"),
                Tag = salon
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            string carrerasTexto = salon.NombreCarrera;
            if (!string.IsNullOrWhiteSpace(salon.NombreCarreraSecundaria))
                carrerasTexto += $", {salon.NombreCarreraSecundaria}";
            if (!string.IsNullOrWhiteSpace(salon.NombreCarreraTerciaria))
                carrerasTexto += $", {salon.NombreCarreraTerciaria}";

            var info = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            info.Children.Add(new TextBlock
            {
                Text = salon.Nombre,
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)FindResource("GuindaBajo")
            });
            info.Children.Add(new TextBlock
            {
                Text = $"Capacidad: {salon.Capacidad} alumnos  •  Edificio: {salon.NombreEdificio}  •  Carrera(s): {carrerasTexto}",
                FontSize = 12,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#888888")),
                Margin = new Thickness(0, 2, 0, 0)
            });
            Grid.SetColumn(info, 0);
            grid.Children.Add(info);

            var botones = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0)
            };

            var btnEditar = new Button
            {
                Content = "Editar",
                Padding = new Thickness(12, 6, 12, 6),
                Margin = new Thickness(0, 0, 6, 0),
                Background = (Brush)FindResource("RosaOscuro"),
                Tag = salon.IdSalon
            };
            btnEditar.Click += EditarSalon_Click;

            var btnEliminar = new Button
            {
                Content = "Eliminar",
                Padding = new Thickness(12, 6, 12, 6),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E53935")),
                Tag = salon.IdSalon
            };
            btnEliminar.Click += EliminarSalon_Click;

            botones.Children.Add(btnEditar);
            botones.Children.Add(btnEliminar);
            Grid.SetColumn(botones, 2);
            grid.Children.Add(botones);

            border.Child = grid;
            return border;
        }

        // ── Eventos formulario ──────────────────────────────────────────

        private void ComboBoxNumCarreras_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PanelCarrera2 == null || PanelCarrera3 == null) return;

            if (ComboBoxNumCarreras?.SelectedItem is ComboBoxItem item && item.Tag != null)
            {
                int num = 1;
                if (item.Tag is int valInt)
                    num = valInt;
                else if (int.TryParse(item.Tag.ToString(), out int valParsed))
                    num = valParsed;

                PanelCarrera2.Visibility = num >= 2 ? Visibility.Visible : Visibility.Collapsed;
                PanelCarrera3.Visibility = num >= 3 ? Visibility.Visible : Visibility.Collapsed;

                if (num < 2) ComboBoxCarrera2.SelectedIndex = -1;
                if (num < 3) ComboBoxCarrera3.SelectedIndex = -1;
            }
        }

        private void AgregarSalon_Click(object sender, RoutedEventArgs e)
        {
            bool estaCerrado = PanelFormulario.Visibility == Visibility.Collapsed;
            PanelFormulario.Visibility = estaCerrado ? Visibility.Visible : Visibility.Collapsed;
            BotonAgregarSalon.Content = estaCerrado ? "Cerrar" : "+ Agregar Salón";

            if (estaCerrado)
            {
                _idSalonEnEdicion = null;
                LimpiarFormulario();
                TituloFormulario.Text = "AGREGAR SALÓN";
                BotonGuardarSalon.Content = "Guardar";
            }
        }

        private void GuardarSalon_Click(object sender, RoutedEventArgs e)
        {
            string nombre = TextboxNombreSalon.Text.Trim();
            if (string.IsNullOrWhiteSpace(nombre))
            {
                MessageBox.Show("Ingrese el nombre del salón.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(TextboxCapacidad.Text, out int capacidad) || capacidad <= 0)
            {
                MessageBox.Show("Ingrese una capacidad válida.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int idEdificio = 0;
            if (ComboBoxEdificio.SelectedItem is ComboBoxItem cbEdItem && cbEdItem.Tag is int idEd)
                idEdificio = idEd;

            int idCarrera = 0;
            if (ComboBoxCarrera1?.SelectedItem is ComboBoxItem cbCar1 && cbCar1.Tag is int idC1)
                idCarrera = idC1;

            int numCarreras = 1;
            if (ComboBoxNumCarreras?.SelectedItem is ComboBoxItem cbNum && cbNum.Tag != null)
            {
                if (cbNum.Tag is int valInt)
                    numCarreras = valInt;
                else if (int.TryParse(cbNum.Tag.ToString(), out int valParsed))
                    numCarreras = valParsed;
            }

            int? idCarreraSecundaria = null;
            if (numCarreras >= 2 && ComboBoxCarrera2?.SelectedItem is ComboBoxItem cbCar2 && cbCar2.Tag is int idC2)
                idCarreraSecundaria = idC2;

            int? idCarreraTerciaria = null;
            if (numCarreras >= 3 && ComboBoxCarrera3?.SelectedItem is ComboBoxItem cbCar3 && cbCar3.Tag is int idC3)
                idCarreraTerciaria = idC3;

            try
            {
                var salon = new Salon
                {
                    IdSalon = _idSalonEnEdicion ?? 0,
                    Nombre = nombre,
                    Capacidad = capacidad,
                    IdEdificio = idEdificio,
                    IdCarrera = idCarrera,
                    IdCarreraSecundaria = idCarreraSecundaria,
                    IdCarreraTerciaria = idCarreraTerciaria
                };

                _salonRepository.Guardar(salon);

                MessageBox.Show(
                    _idSalonEnEdicion == null
                        ? $"Salón '{nombre}' agregado correctamente."
                        : $"Salón '{nombre}' actualizado correctamente.",
                    "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                _idSalonEnEdicion = null;
                LimpiarFormulario();
                CerrarFormulario();
                CargarSalones();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelarSalon_Click(object sender, RoutedEventArgs e)
        {
            _idSalonEnEdicion = null;
            LimpiarFormulario();
            CerrarFormulario();
        }

        private void EditarSalon_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not int idSalon) return;

            var salon = BuscarSalon(idSalon);
            if (salon == null) return;

            _idSalonEnEdicion = idSalon;
            TextboxNombreSalon.Text = salon.Nombre;
            TextboxCapacidad.Text = salon.Capacidad.ToString();

            foreach (ComboBoxItem item in ComboBoxEdificio.Items)
                if (item.Tag is int idEd && idEd == salon.IdEdificio)
                { ComboBoxEdificio.SelectedItem = item; break; }

            foreach (ComboBoxItem item in ComboBoxCarrera1.Items)
                if (item.Tag is int idCar && idCar == salon.IdCarrera)
                { ComboBoxCarrera1.SelectedItem = item; break; }

            int totalCarreras = 1;
            if (salon.IdCarreraTerciaria.HasValue && salon.IdCarreraTerciaria > 0)
                totalCarreras = 3;
            else if (salon.IdCarreraSecundaria.HasValue && salon.IdCarreraSecundaria > 0)
                totalCarreras = 2;

            foreach (ComboBoxItem item in ComboBoxNumCarreras.Items)
            {
                int numVal = 1;
                if (item.Tag is int vi) numVal = vi;
                else if (int.TryParse(item.Tag?.ToString(), out int vp)) numVal = vp;

                if (numVal == totalCarreras)
                {
                    ComboBoxNumCarreras.SelectedItem = item;
                    break;
                }
            }

            if (totalCarreras >= 2 && salon.IdCarreraSecundaria.HasValue)
            {
                foreach (ComboBoxItem item in ComboBoxCarrera2.Items)
                    if (item.Tag is int idCar && idCar == salon.IdCarreraSecundaria)
                    { ComboBoxCarrera2.SelectedItem = item; break; }
            }

            if (totalCarreras >= 3 && salon.IdCarreraTerciaria.HasValue)
            {
                foreach (ComboBoxItem item in ComboBoxCarrera3.Items)
                    if (item.Tag is int idCar && idCar == salon.IdCarreraTerciaria)
                    { ComboBoxCarrera3.SelectedItem = item; break; }
            }

            TituloFormulario.Text = "EDITAR SALÓN";
            BotonGuardarSalon.Content = "Actualizar";
            PanelFormulario.Visibility = Visibility.Visible;
            BotonAgregarSalon.Content = "Cerrar";
            PanelFormulario.BringIntoView();
        }

        private void EliminarSalon_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not int idSalon) return;

            var resultado = MessageBox.Show(
                "¿Está seguro de eliminar este salón?",
                "Eliminar Salón", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (resultado != MessageBoxResult.Yes) return;

            try
            {
                _salonRepository.Eliminar(idSalon);

                MessageBox.Show("Salón eliminado correctamente.",
                    "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CargarSalones();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Utilidades ──────────────────────────────────────────────────

        private Salon? BuscarSalon(int idSalon)
        {
            foreach (var el in ListaSalones.Children)
                if (el is Border b && b.Tag is Salon s && s.IdSalon == idSalon)
                    return s;
            return null;
        }

        private void LimpiarFormulario()
        {
            TextboxNombreSalon.Clear();
            TextboxCapacidad.Text = "40";
            ComboBoxEdificio.SelectedIndex = -1;
            ComboBoxNumCarreras.SelectedIndex = 0;
            ComboBoxCarrera1.SelectedIndex = -1;
            ComboBoxCarrera2.SelectedIndex = -1;
            ComboBoxCarrera3.SelectedIndex = -1;
            if (PanelCarrera2 != null) PanelCarrera2.Visibility = Visibility.Collapsed;
            if (PanelCarrera3 != null) PanelCarrera3.Visibility = Visibility.Collapsed;
        }

        private void CerrarFormulario()
        {
            PanelFormulario.Visibility = Visibility.Collapsed;
            BotonAgregarSalon.Content = "+ Agregar Salón";
        }
    }
}