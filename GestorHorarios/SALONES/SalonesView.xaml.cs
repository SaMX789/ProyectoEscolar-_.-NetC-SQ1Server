using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GestorHorarios.Models;
using GestorHorarios.Services;
using Microsoft.Data.SqlClient;

namespace GestorHorarios.SALONES
{
    public partial class SalonesView : UserControl
    {
        private int? _idSalonEnEdicion = null;
        private readonly List<(int Id, string Nombre)> _edificios = new();

        public SalonesView()
        {
            InitializeComponent();
            CargarEdificios();
            CargarSalones();
        }

        // ── Carga de datos ──────────────────────────────────────────────

        private void CargarEdificios()
        {
            try
            {
                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                using var cmd = new SqlCommand(
                    "SELECT id_edificio, nombre FROM Edificios ORDER BY nombre", conn);
                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int id = Convert.ToInt32(reader["id_edificio"]);
                    string nombre = reader["nombre"].ToString() ?? "";
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

        private void CargarSalones()
        {
            ListaSalones.Children.Clear();

            try
            {
                var salones = new List<Salon>();

                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                using var cmd = new SqlCommand("sp_GetSalones", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    salones.Add(new Salon
                    {
                        IdSalon = Convert.ToInt32(reader["id_salon"]),
                        Nombre = reader["NombreSalon"].ToString() ?? "",
                        Capacidad = Convert.ToInt32(reader["capacidad"]),
                        NombreEdificio = reader["NombreEdificio"].ToString() ?? "",
                        NombreCarrera = reader["NombreCarrera"].ToString() ?? ""
                    });
                }

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

                // Actualizar resumen
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

            // Info salón
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
                Text = $"Capacidad: {salon.Capacidad} alumnos  •  Edificio: {salon.NombreEdificio}",
                FontSize = 12,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#888888")),
                Margin = new Thickness(0, 2, 0, 0)
            });
            Grid.SetColumn(info, 0);
            grid.Children.Add(info);

            // Botones
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

            int? idEdificio = null;
            if (ComboBoxEdificio.SelectedItem is ComboBoxItem cbItem && cbItem.Tag is int idEd)
                idEdificio = idEd;

            try
            {
                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                conn.Open();

                if (_idSalonEnEdicion == null)
                {
                    using var cmd = new SqlCommand(
                        "INSERT INTO Salones (nombre, capacidad, id_edificio) VALUES (@nombre, @cap, @edificio)",
                        conn);
                    cmd.Parameters.AddWithValue("@nombre", nombre);
                    cmd.Parameters.AddWithValue("@cap", capacidad);
                    cmd.Parameters.AddWithValue("@edificio", (object?)idEdificio ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show($"Salón '{nombre}' agregado correctamente.",
                        "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    using var cmd = new SqlCommand(
                        "UPDATE Salones SET nombre=@nombre, capacidad=@cap, id_edificio=@edificio WHERE id_salon=@id",
                        conn);
                    cmd.Parameters.AddWithValue("@nombre", nombre);
                    cmd.Parameters.AddWithValue("@cap", capacidad);
                    cmd.Parameters.AddWithValue("@edificio", (object?)idEdificio ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@id", _idSalonEnEdicion.Value);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show($"Salón '{nombre}' actualizado correctamente.",
                        "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }

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
                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                using var cmd = new SqlCommand(
                    "DELETE FROM Salones WHERE id_salon = @id", conn);
                cmd.Parameters.AddWithValue("@id", idSalon);
                conn.Open();
                cmd.ExecuteNonQuery();

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
        }

        private void CerrarFormulario()
        {
            PanelFormulario.Visibility = Visibility.Collapsed;
            BotonAgregarSalon.Content = "+ Agregar Salón";
        }
    }
}
