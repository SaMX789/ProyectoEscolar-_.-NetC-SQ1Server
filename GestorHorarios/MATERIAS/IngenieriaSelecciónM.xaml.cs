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

namespace GestorHorarios.MATERIAS
{
    public partial class IngenieriaSeleccionM : UserControl
    {
        private int _idCarrera;

        // NUEVO: Variable para saber si estamos editando o agregando
        private int? _idMateriaEnEdicion = null;

        public IngenieriaSeleccionM(int idCarrera)
        {
            InitializeComponent();
            _idCarrera = idCarrera;
            TituloCarrera.Text = ObtenerNombreCarrera();

            CargarOpcionesSemestre();
            CargarMaterias();
        }

        private void CargarOpcionesSemestre()
        {
            ComboBoxSemestre.Items.Clear();
            for (int i = 1; i <= 9; i++)
            {
                ComboBoxSemestre.Items.Add(new ComboBoxItem
                {
                    Content = ObtenerNombreSemestre(i),
                    Tag = i
                });
            }
        }

        private void CargarMaterias()
        {
            List<Materia> materias = new List<Materia>();
            string conexion = DatabaseService.GetConnectionString();

            using (SqlConnection conn = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand("sp_ObtenerMateriasPorCarrera", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_carrera", _idCarrera);
                conn.Open();

                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Materia materia = new Materia
                    {
                        IdMateria = Convert.ToInt32(reader["id_materia"]),
                        Nombre = reader["nombre"].ToString()!,
                        Clave = reader["clave"].ToString()!,
                        Creditos = Convert.ToInt32(reader["creditos"]),
                        Semestre = Convert.ToInt32(reader["semestre"])
                    };
                    materias.Add(materia);
                }
            }

            int semestreMaximo = materias.Any() ? materias.Max(m => m.Semestre) : 0;

            for (int semestre = 1; semestre <= semestreMaximo; semestre++)
            {
                var textoSemestre = ObtenerNombreSemestre(semestre);
                var titloSemestre = new TextBlock
                {
                    Text = textoSemestre,
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    Foreground = (Brush)FindResource("GuindaBajo"),
                    Margin = new Thickness(0, 20, 0, 10)
                };

                ListaMaterias.Children.Add(titloSemestre);

                var materiasDelSemestre = materias.Where(m => m.Semestre == semestre).ToList();

                if (materiasDelSemestre.Count == 0)
                {
                    var noHayMaterias = new TextBlock
                    {
                        Text = "No hay materias asignadas a este semestre",
                        FontSize = 14,
                        Foreground = Brushes.Gray,
                        Margin = new Thickness(10, 5, 10, 15),
                        FontStyle = FontStyles.Italic
                    };
                    ListaMaterias.Children.Add(noHayMaterias);
                }
                else
                {
                    foreach (var materia in materiasDelSemestre)
                    {
                        var card = CrearCardMateria(materia);
                        ListaMaterias.Children.Add(card);
                    }
                }
            }
        }

        private string ObtenerNombreSemestre(int semestre)
        {
            return semestre switch
            {
                1 => "PRIMER SEMESTRE",
                2 => "SEGUNDO SEMESTRE",
                3 => "TERCER SEMESTRE",
                4 => "CUARTO SEMESTRE",
                5 => "QUINTO SEMESTRE",
                6 => "SEXTO SEMESTRE",
                7 => "SÉPTIMO SEMESTRE",
                8 => "OCTAVO SEMESTRE",
                9 => "NOVENO SEMESTRE",
                _ => $"SEMESTRE {semestre}"
            };
        }

        private string ObtenerNombreCarrera()
        {
            string nombreCarrera = "";
            string conexion = DatabaseService.GetConnectionString();

            using (SqlConnection conn = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand("sp_ObtenerNombreCarrera", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_carrera", _idCarrera);
                conn.Open();

                var resultado = cmd.ExecuteScalar();
                if (resultado != null)
                {
                    nombreCarrera = resultado.ToString()!;
                }
            }
            return nombreCarrera;
        }

        private Border CrearCardMateria(Materia materia)
        {
            var border = new Border
            {
                Style = (Style)FindResource("MateriaCardStyle")
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var nombreText = new TextBlock
            {
                Text = materia.Nombre,
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)FindResource("GuindaBajo")
            };
            Grid.SetColumn(nombreText, 0);
            grid.Children.Add(nombreText);

            var claveText = new TextBlock
            {
                Text = materia.Clave,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = (Brush)FindResource("GuindaBajo")
            };
            Grid.SetColumn(claveText, 1);
            grid.Children.Add(claveText);

            var creditosText = new TextBlock
            {
                Text = materia.Creditos.ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = (Brush)FindResource("GuindaBajo")
            };
            Grid.SetColumn(creditosText, 2);
            grid.Children.Add(creditosText);

            var buttonStack = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            // MODIFICACIÓN: Asignar Tag y Evento Click al botón Editar
            var editarBtn = new Button
            {
                Content = "Editar",
                Margin = new Thickness(5, 0, 5, 0),
                Tag = materia // Guardamos el objeto materia completo aquí
            };
            editarBtn.Click += EditarMateria_Click;

            // MODIFICACIÓN: Asignar Tag y Evento Click al botón Eliminar
            var eliminarBtn = new Button
            {
                Content = "Eliminar",
                Margin = new Thickness(5, 0, 0, 0),
                Tag = materia.IdMateria // Guardamos solo el ID para eliminar
            };
            eliminarBtn.Click += EliminarMateria_Click;

            buttonStack.Children.Add(editarBtn);
            buttonStack.Children.Add(eliminarBtn);
            Grid.SetColumn(buttonStack, 3);
            grid.Children.Add(buttonStack);

            border.Child = grid;
            return border;
        }

        private void VolverCarreras_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GetFromWindow(this)?.NavigateTo(new MateriasView());
        }

        private void BotonMostrarAgregarMaterias_Click(object sender, RoutedEventArgs e)
        {
            if (PanelFormularioMateria.Visibility == Visibility.Collapsed)
            {
                PanelFormularioMateria.Visibility = Visibility.Visible;
                BotonMostrarAgregarMaterias.Content = "Cerrar";
            }
            else
            {
                LimpiarFormulario(); // Limpiamos al cerrar para que no queden datos pegados
                PanelFormularioMateria.Visibility = Visibility.Collapsed;
                BotonMostrarAgregarMaterias.Content = "Agregar materias";
            }
        }

        // NUEVO: Método para limpiar el formulario y resetear el modo edición
        private void LimpiarFormulario()
        {
            TextboxNombre.Clear();
            TextboxClave.Clear();
            TextboxCreditos.Clear();
            ComboBoxSemestre.SelectedIndex = -1;
            _idMateriaEnEdicion = null; // Salimos del modo edición
            BotonGuardarMaterias.Content = "Guardar"; // Restauramos el texto
        }

        // NUEVO: Lógica del botón Editar
        private void EditarMateria_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Materia materia)
            {
                // 1. Activar el modo edición guardando el ID
                _idMateriaEnEdicion = materia.IdMateria;

                // 2. Llenar los campos con la información de la materia
                TextboxNombre.Text = materia.Nombre;
                TextboxClave.Text = materia.Clave;
                TextboxCreditos.Text = materia.Creditos.ToString();

                // 3. Buscar y seleccionar el semestre correcto en el ComboBox
                foreach (ComboBoxItem item in ComboBoxSemestre.Items)
                {
                    if ((int)item.Tag == materia.Semestre)
                    {
                        ComboBoxSemestre.SelectedItem = item;
                        break;
                    }
                }

                // 4. Cambiar el aspecto de la interfaz
                BotonGuardarMaterias.Content = "Actualizar";
                BotonMostrarAgregarMaterias.Content = "Cancelar edición";
                PanelFormularioMateria.Visibility = Visibility.Visible;
            }
        }

        // NUEVO: Lógica del botón Eliminar
        // NUEVO: Lógica del botón Eliminar (Borrado Lógico)
        private void EliminarMateria_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int idMateria)
            {
                var result = MessageBox.Show(
                    "¿Estás seguro de que deseas dar de baja esta materia?",
                    "Confirmar Eliminación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (SqlConnection conn = new SqlConnection(DatabaseService.GetConnectionString()))
                        {
                            // MODIFICACIÓN: Hacemos un UPDATE en lugar de un DELETE
                            string query = "UPDATE Materias SET id_estado = 2 WHERE id_materia = @id";
                            SqlCommand cmd = new SqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@id", idMateria);

                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("Materia dada de baja correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Refrescar la lista
                        ListaMaterias.Children.Clear();
                        CargarMaterias();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al eliminar la materia: {ex.Message}",
                                        "Error de BD",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                    }
                }
            }
        }

        // MODIFICADO: Adaptado para Guardar o Actualizar según el modo
        private void BotonGuardarMaterias_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TextboxNombre.Text) ||
                string.IsNullOrWhiteSpace(TextboxClave.Text) ||
                string.IsNullOrWhiteSpace(TextboxCreditos.Text) ||
                ComboBoxSemestre.SelectedItem == null)
            {
                MessageBox.Show("Por favor, llena todos los campos y selecciona un semestre.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(TextboxCreditos.Text, out int creditos))
            {
                MessageBox.Show("El total de créditos debe ser un número válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int semestreSeleccionado = (int)((ComboBoxItem)ComboBoxSemestre.SelectedItem).Tag;

            try
            {
                string conexion = DatabaseService.GetConnectionString();
                using (SqlConnection conn = new SqlConnection(conexion))
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;

                    // Si NO estamos editando, hacemos un INSERT
                    if (_idMateriaEnEdicion == null)
                    {
                        cmd.CommandText = @"INSERT INTO Materias (nombre, clave, creditos, semestre, id_carrera, id_estado) 
                                            VALUES (@nombre, @clave, @creditos, @semestre, @id_carrera, 1)";
                    }
                    // Si SÍ estamos editando, hacemos un UPDATE
                    else
                    {
                        cmd.CommandText = @"UPDATE Materias 
                                            SET nombre = @nombre, clave = @clave, creditos = @creditos, semestre = @semestre 
                                            WHERE id_materia = @id_materia";
                        cmd.Parameters.AddWithValue("@id_materia", _idMateriaEnEdicion.Value);
                    }

                    cmd.Parameters.AddWithValue("@nombre", TextboxNombre.Text.Trim());
                    cmd.Parameters.AddWithValue("@clave", TextboxClave.Text.Trim().ToUpper());
                    cmd.Parameters.AddWithValue("@creditos", creditos);
                    cmd.Parameters.AddWithValue("@semestre", semestreSeleccionado);
                    cmd.Parameters.AddWithValue("@id_carrera", _idCarrera);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                string mensajeExito = _idMateriaEnEdicion == null ? "Materia guardada exitosamente." : "Materia actualizada exitosamente.";
                MessageBox.Show(mensajeExito, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                LimpiarFormulario();
                PanelFormularioMateria.Visibility = Visibility.Collapsed;
                BotonMostrarAgregarMaterias.Content = "Agregar materias";

                ListaMaterias.Children.Clear();
                CargarMaterias();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al guardar en la base de datos:\n\n{ex.Message}", "Error de BD", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // NUEVO: Lógica del botón Cancelar
        private void BotonCancelarMaterias_Click(object sender, RoutedEventArgs e)
        {
            // 1. Limpiamos los campos y salimos del modo edición
            LimpiarFormulario();

            // 2. Ocultamos la ventana del formulario
            PanelFormularioMateria.Visibility = Visibility.Collapsed;

            // 3. Restauramos el texto del botón principal de la esquina superior derecha
            BotonMostrarAgregarMaterias.Content = "Agregar materias";
        }

    }
}