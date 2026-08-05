using GestorHorarios.Models;
using GestorHorarios.Services;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GestorHorarios.GRUPOS
{
    public partial class IngenieriaSeleccionG : UserControl
    {
        private readonly int _idCarrera;

        // NUEVO: Variable para saber si estamos editando o agregando un grupo
        private int? _idGrupoEnEdicion = null;

        public IngenieriaSeleccionG()
        {
            InitializeComponent();
        }

        public IngenieriaSeleccionG(int idCarrera) : this()
        {
            _idCarrera = idCarrera;
            CargarTituloCarrera();

            // NUEVO: Cargamos los ComboBox del formulario antes de cargar los grupos
            CargarOpcionesFormulario();
            CargarGrupos();
        }

        // NUEVO: Método para llenar los ComboBox de Semestre y Turno
        private void CargarOpcionesFormulario()
        {
            // Llenar Semestres (1 al 9)
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

            // Llenar Turnos
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
                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                using var cmd = new SqlCommand("sp_ObtenerNombreCarrera", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@id_carrera", _idCarrera);
                conn.Open();
                var resultado = cmd.ExecuteScalar();
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
                var grupos = new List<Grupo>();

                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                using var cmd = new SqlCommand("sp_GetGruposByCarrera", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@id_carrera", _idCarrera);
                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    grupos.Add(new Grupo
                    {
                        IdGrupo = Convert.ToInt32(reader["id_grupo"]),
                        Nombre = reader["NombreGrupo"]?.ToString() ?? reader["nombre"]?.ToString() ?? "",
                        Semestre = Convert.ToInt32(reader["semestre"]),
                        Turno = reader["turno"].ToString() ?? "",
                        NombreCarrera = reader["NombreCarrera"]?.ToString() ?? ""
                    });
                }

                // Separar por ciclo: impares = B, pares = A
                var cicloB = grupos.Where(g => g.Semestre % 2 != 0).OrderBy(g => g.Semestre).ToList();
                var cicloA = grupos.Where(g => g.Semestre % 2 == 0).OrderBy(g => g.Semestre).ToList();

                // Agrupar por semestre dentro de cada ciclo
                PopularPanel(PanelCicloB, cicloB);
                PopularPanel(PanelCicloA, cicloA);

                if (cicloB.Count == 0)
                    PanelCicloB.Children.Add(CrearMensajeVacio());
                if (cicloA.Count == 0)
                    PanelCicloA.Children.Add(CrearMensajeVacio());
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
                // Encabezado de semestre
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
                Tag = grupo
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Nombre + turno
            var infoPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            infoPanel.Children.Add(new TextBlock
            {
                Text = grupo.Nombre,
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)FindResource("GuindaBajo")
            });
            infoPanel.Children.Add(new TextBlock
            {
                Text = $"Turno {grupo.Turno}",
                FontSize = 12,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#888888")),
                Margin = new Thickness(0, 2, 0, 0)
            });
            Grid.SetColumn(infoPanel, 0);
            grid.Children.Add(infoPanel);

            // Botones
            var botonesPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0)
            };

            // MODIFICACIÓN: Añadir botón de Editar
            var btnEditar = new Button
            {
                Content = "Editar",
                Padding = new Thickness(12, 6, 12, 6),
                Margin = new Thickness(0, 0, 5, 0),
                Tag = grupo // Guardamos el objeto completo para poder editarlo
            };
            btnEditar.Click += EditarGrupo_Click;
            botonesPanel.Children.Add(btnEditar);

            var btnEliminar = new Button
            {
                Content = "Eliminar",
                Padding = new Thickness(12, 6, 12, 6),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E53935")),
                Tag = grupo.IdGrupo
            };
            btnEliminar.Click += EliminarGrupo_Click;
            botonesPanel.Children.Add(btnEliminar);

            Grid.SetColumn(botonesPanel, 2);
            grid.Children.Add(botonesPanel);

            border.Child = grid;
            return border;
        }

        private static TextBlock CrearMensajeVacio() => new()
        {
            Text = "No hay grupos registrados para este ciclo.",
            FontSize = 13,
            Foreground = Brushes.Gray,
            FontStyle = FontStyles.Italic,
            Margin = new Thickness(0, 4, 0, 4)
        };

        // NUEVO: Método para ocultar/mostrar formulario de grupos
        private void BotonMostrarAgregarGrupo_Click(object sender, RoutedEventArgs e)
        {
            if (PanelFormularioGrupo.Visibility == Visibility.Collapsed)
            {
                PanelFormularioGrupo.Visibility = Visibility.Visible;
                BotonMostrarAgregarGrupo.Content = "Cerrar";
            }
            else
            {
                LimpiarFormulario();
                PanelFormularioGrupo.Visibility = Visibility.Collapsed;
                BotonMostrarAgregarGrupo.Content = "Agregar Grupo";
            }
        }

        // NUEVO: Limpiar el formulario
        private void LimpiarFormulario()
        {
            TextboxNombre.Clear();
            ComboBoxSemestre.SelectedIndex = -1;
            ComboBoxTurno.SelectedIndex = -1;
            _idGrupoEnEdicion = null;
            BotonGuardarGrupo.Content = "Guardar";
        }

        // NUEVO: Lógica del botón Editar
        private void EditarGrupo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Grupo grupo)
            {
                _idGrupoEnEdicion = grupo.IdGrupo;
                TextboxNombre.Text = grupo.Nombre;

                // Seleccionar Semestre
                foreach (ComboBoxItem item in ComboBoxSemestre.Items)
                {
                    if ((int)item.Tag == grupo.Semestre)
                    {
                        ComboBoxSemestre.SelectedItem = item;
                        break;
                    }
                }

                // Seleccionar Turno
                foreach (ComboBoxItem item in ComboBoxTurno.Items)
                {
                    if (item.Tag.ToString() == grupo.Turno)
                    {
                        ComboBoxTurno.SelectedItem = item;
                        break;
                    }
                }

                BotonGuardarGrupo.Content = "Actualizar";
                BotonMostrarAgregarGrupo.Content = "Cancelar edición";
                PanelFormularioGrupo.Visibility = Visibility.Visible;
            }
        }

        // NUEVO: Guardar o Actualizar Grupo
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

            try
            {
                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                using var cmd = new SqlCommand();
                cmd.Connection = conn;

                if (_idGrupoEnEdicion == null)
                {
                    // INSERT
                    cmd.CommandText = @"INSERT INTO Grupos (nombre, semestre, turno, id_carrera) 
                                        VALUES (@nombre, @semestre, @turno, @id_carrera)";
                }
                else
                {
                    // UPDATE
                    cmd.CommandText = @"UPDATE Grupos 
                                        SET nombre = @nombre, semestre = @semestre, turno = @turno 
                                        WHERE id_grupo = @id_grupo";
                    cmd.Parameters.AddWithValue("@id_grupo", _idGrupoEnEdicion.Value);
                }

                cmd.Parameters.AddWithValue("@nombre", TextboxNombre.Text.Trim().ToUpper());
                cmd.Parameters.AddWithValue("@semestre", semestreSeleccionado);
                cmd.Parameters.AddWithValue("@turno", turnoSeleccionado);
                cmd.Parameters.AddWithValue("@id_carrera", _idCarrera);

                conn.Open();
                cmd.ExecuteNonQuery();

                MessageBox.Show(_idGrupoEnEdicion == null ? "Grupo guardado exitosamente." : "Grupo actualizado exitosamente.",
                                "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                LimpiarFormulario();
                PanelFormularioGrupo.Visibility = Visibility.Collapsed;
                BotonMostrarAgregarGrupo.Content = "Agregar Grupo";

                CargarGrupos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el grupo:\n\n{ex.Message}", "Error de BD", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // NUEVO: Lógica del botón Cancelar
        private void BotonCancelarGrupo_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
            PanelFormularioGrupo.Visibility = Visibility.Collapsed;
            BotonMostrarAgregarGrupo.Content = "Agregar Grupo";
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
                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                using var cmd = new SqlCommand(
                    "DELETE FROM Grupos WHERE id_grupo = @id", conn);
                cmd.Parameters.AddWithValue("@id", idGrupo);
                conn.Open();
                cmd.ExecuteNonQuery();

                MessageBox.Show("Grupo eliminado correctamente.",
                    "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
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