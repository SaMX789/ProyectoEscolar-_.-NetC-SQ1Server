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

        // Variable de tu compañero para saber si estamos editando o agregando
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
            ListaMaterias.Children.Clear();

            for (int semestre = 1; semestre <= semestreMaximo; semestre++)
            {
                var textoSemestre = ObtenerNombreSemestre(semestre);
                var titloSemestre = new TextBlock
                {
                    Text = textoSemestre,
                    FontSize = 16,
                    FontWeight = FontWeights.ExtraBold, // Ajuste moderno
                    Foreground = (Brush)FindResource("GuindaBajo"),
                    Margin = new Thickness(5, 25, 0, 10)
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

        // ==========================================
        // DISEÑO MODERNO DE LA TARJETA + BOTONES FUNCIONALES
        // ==========================================
        private Border CrearCardMateria(Materia materia)
        {
            var border = new Border { Style = (Style)FindResource("MateriaCardStyle") };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Nombre
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) }); // Clave
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) }); // Creditos
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Botones

            // 1. Título
            var nombreText = new TextBlock
            {
                Text = materia.Nombre,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333")),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(nombreText, 0);
            grid.Children.Add(nombreText);

            // 2. Badge de la Clave
            var claveBadge = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F1F3F5")),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(8, 4, 8, 4),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };
            claveBadge.Child = new TextBlock { Text = materia.Clave, FontSize = 12, FontWeight = FontWeights.SemiBold, Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#495057")) };
            Grid.SetColumn(claveBadge, 1);
            grid.Children.Add(claveBadge);

            // 3. Badge de los Créditos
            var creditosBadge = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F5E9")),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(8, 4, 8, 4),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };
            creditosBadge.Child = new TextBlock { Text = $"{materia.Creditos} Créditos", FontSize = 12, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E7D32")) };
            Grid.SetColumn(creditosBadge, 2);
            grid.Children.Add(creditosBadge);

            // 4. Panel de Botones Modernos
            var buttonStack = new StackPanel { Orientation = Orientation.Horizontal };

            // Ícono de Ojo para Docentes
            string pathOjo = "M12,4.5 C7,4.5 2.73,7.61 1,12 C2.73,16.39 7,19.5 12,19.5 C17,19.5 21.27,16.39 23,12 C21.27,7.61 17,4.5 12,4.5 Z M12,17 C9.24,17 7,14.76 7,12 C7,9.24 9.24,7 12,7 C14.76,7 17,9.24 17,12 C17,14.76 14.76,17 12,17 Z M12,9 C10.34,9 9,10.34 9,12 C9,13.66 10.34,15 12,15 C13.66,15 15,13.66 15,12 C15,10.34 13.66,9 12,9 Z";
            var btnDocentes = CrearBotonAccion("Docentes", pathOjo, "#0D9488"); // Turquesa
            btnDocentes.Click += (s, e) => AbrirModalDocentesMateria(materia.IdMateria, materia.Nombre);

            // Ícono de Lápiz para Editar (CONECTADO A LA LÓGICA DE TU COMPAÑERO)
            string pathLapiz = "M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25zM20.71 7.04c.39-.39.39-1.02 0-1.41l-2.34-2.34c-.39-.39-1.02-.39-1.41 0l-1.83 1.83 3.75 3.75 1.83-1.83z";
            var btnEditar = CrearBotonAccion("Editar", pathLapiz, "#475569"); // Gris Pizarra
            btnEditar.Tag = materia;
            btnEditar.Click += EditarMateria_Click;

            // Ícono de Bote de Basura para Eliminar (CONECTADO A LA LÓGICA DE TU COMPAÑERO)
            string pathBasura = "M6 19c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7H6v12zM19 4h-3.5l-1-1h-5l-1 1H5v2h14V4z";
            var btnEliminar = CrearBotonAccion("Eliminar", pathBasura, "#E11D48"); // Rojo moderno
            btnEliminar.Tag = materia.IdMateria;
            btnEliminar.Click += EliminarMateria_Click;

            buttonStack.Children.Add(btnDocentes);
            buttonStack.Children.Add(btnEditar);
            buttonStack.Children.Add(btnEliminar);

            Grid.SetColumn(buttonStack, 3);
            grid.Children.Add(buttonStack);

            border.Child = grid;
            return border;
        }

        // ==========================================
        // GENERADOR AUTOMÁTICO DE BOTONES CON ÍCONOS
        // ==========================================
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

        // ==========================================
        // LÓGICA DEL NUEVO MODAL DE DOCENTES
        // ==========================================
        private void AbrirModalDocentesMateria(int idMateria, string nombreMateria)
        {
            TxtModalNombreMateria.Text = nombreMateria;
            PanelListaDocentesMateria.Children.Clear();

            try
            {
                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                using var cmd = new SqlCommand("sp_GetDocentesPorMateria", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@id_materia", idMateria);

                conn.Open();
                using var reader = cmd.ExecuteReader();

                bool hayDocentes = false;

                while (reader.Read())
                {
                    hayDocentes = true;
                    string nombreDoc = reader["NombreDocente"].ToString()!;
                    string carreraDoc = reader["CarreraPrincipal"].ToString()!;

                    // NUEVO: Leemos la lista de carreras separadas por comas
                    string carrerasAsignadas = reader["CarrerasAsignadas"].ToString()!;

                    var border = new Border
                    {
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8F9FA")),
                        BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DEE2E6")),
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(6),
                        Padding = new Thickness(15),
                        Margin = new Thickness(0, 0, 0, 10)
                    };

                    var sp = new StackPanel();
                    sp.Children.Add(new TextBlock { Text = nombreDoc, FontWeight = FontWeights.Bold, FontSize = 14, Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333")) });
                    sp.Children.Add(new TextBlock { Text = $"Departamento Base: {carreraDoc}", FontSize = 12, Foreground = Brushes.DimGray, Margin = new Thickness(0, 4, 0, 0) });

                    // NUEVO: Agregamos el texto con las carreras en las que imparte clases
                    sp.Children.Add(new TextBlock
                    {
                        Text = $"Da clases en: {carrerasAsignadas}",
                        FontSize = 11,
                        FontStyle = FontStyles.Italic,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0D9488")), // Turquesa
                        Margin = new Thickness(0, 4, 0, 0),
                        TextWrapping = TextWrapping.Wrap
                    });

                    border.Child = sp;
                    PanelListaDocentesMateria.Children.Add(border);
                }

                if (!hayDocentes)
                {
                    PanelListaDocentesMateria.Children.Add(new TextBlock
                    {
                        Text = "No hay ningún docente registrado en el sistema habilitado para dar esta materia.",
                        FontStyle = FontStyles.Italic,
                        Foreground = Brushes.Gray,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 10, 0, 0)
                    });
                }

                ModalDocentesMateria.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar docentes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CerrarModalDocentes_Click(object sender, RoutedEventArgs e)
        {
            ModalDocentesMateria.Visibility = Visibility.Collapsed;
        }

        // ==========================================
        // LÓGICA RESTAURADA DE TU COMPAÑERO
        // ==========================================
        private void VolverCarreras_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GetFromWindow(this)?.NavigateTo(new MateriasView());
        }

        private void BotonMostrarAgregarMaterias_Click(object sender, RoutedEventArgs e)
        {
            if (PanelFormularioMateria.Visibility == Visibility.Collapsed)
            {
                PanelFormularioMateria.Visibility = Visibility.Visible;
                BotonMostrarAgregarMaterias.Content = "X Cerrar formulario";
                BotonMostrarAgregarMaterias.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#475569"));
            }
            else
            {
                LimpiarFormulario(); // Tu compañero agregó esto
                PanelFormularioMateria.Visibility = Visibility.Collapsed;
                BotonMostrarAgregarMaterias.Content = "+ Agregar materias";
                BotonMostrarAgregarMaterias.Background = (Brush)FindResource("GuindaBajo");
            }
        }

        private void LimpiarFormulario()
        {
            TextboxNombre.Clear();
            TextboxClave.Clear();
            TextboxCreditos.Clear();
            ComboBoxSemestre.SelectedIndex = -1;
            _idMateriaEnEdicion = null; // Salimos del modo edición
            BotonGuardarMaterias.Content = "Guardar"; // Restauramos el texto
        }

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
                BotonMostrarAgregarMaterias.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#475569"));
                PanelFormularioMateria.Visibility = Visibility.Visible;
            }
        }

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
                            string query = "UPDATE Materias SET id_estado = 2 WHERE id_materia = @id";
                            SqlCommand cmd = new SqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@id", idMateria);

                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("Materia dada de baja correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                        ListaMaterias.Children.Clear();
                        CargarMaterias();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al eliminar la materia: {ex.Message}", "Error de BD", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

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

                    if (_idMateriaEnEdicion == null)
                    {
                        cmd.CommandText = @"INSERT INTO Materias (nombre, clave, creditos, semestre, id_carrera, id_estado) 
                                            VALUES (@nombre, @clave, @creditos, @semestre, @id_carrera, 1)";
                    }
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
                BotonMostrarAgregarMaterias.Content = "+ Agregar materias";
                BotonMostrarAgregarMaterias.Background = (Brush)FindResource("GuindaBajo");

                ListaMaterias.Children.Clear();
                CargarMaterias();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al guardar en la base de datos:\n\n{ex.Message}", "Error de BD", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BotonCancelarMaterias_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
            PanelFormularioMateria.Visibility = Visibility.Collapsed;
            BotonMostrarAgregarMaterias.Content = "+ Agregar materias";
            BotonMostrarAgregarMaterias.Background = (Brush)FindResource("GuindaBajo");
        }
    }
}