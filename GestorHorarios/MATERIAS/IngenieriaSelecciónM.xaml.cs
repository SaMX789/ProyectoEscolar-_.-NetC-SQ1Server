using GestorHorarios.Models;
using GestorHorarios.Services;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Windows;
using System.Windows.Controls;


namespace GestorHorarios.MATERIAS
{

    public partial class IngenieriaSeleccionM : UserControl
    {
        private int _idCarrera;
        public IngenieriaSeleccionM(int idCarrera)
        {
            InitializeComponent();
            _idCarrera = idCarrera;
            TituloCarrera.Text = ObtenerNombreCarrera();
            CargarMaterias();
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
                    Foreground = (System.Windows.Media.Brush)FindResource("GuindaBajo"),
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
                        Foreground = System.Windows.Media.Brushes.Gray,
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
                7 => "SEPTIMO SEMESTRE",
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
                Style = (System.Windows.Style)FindResource("MateriaCardStyle")
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
                Foreground = (System.Windows.Media.Brush)FindResource("GuindaBajo")
            };
            Grid.SetColumn(nombreText, 0);
            grid.Children.Add(nombreText);

            var claveText = new TextBlock
            {
                Text = materia.Clave,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = (System.Windows.Media.Brush)FindResource("GuindaBajo")
            };
            Grid.SetColumn(claveText, 1);
            grid.Children.Add(claveText);

            var creditosText = new TextBlock
            {
                Text = materia.Creditos.ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = (System.Windows.Media.Brush)FindResource("GuindaBajo")
            };
            Grid.SetColumn(creditosText, 2);
            grid.Children.Add(creditosText);

            var buttonStack = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            var editarBtn = new Button
            {
                Content = "Editar",
                Margin = new Thickness(5, 0, 5, 0)
            };

            var eliminarBtn = new Button
            {
                Content = "Eliminar",
                Margin = new Thickness(5, 0, 0, 0)
            };

            buttonStack.Children.Add(editarBtn);
            buttonStack.Children.Add(eliminarBtn);

            Grid.SetColumn(buttonStack, 3);
            grid.Children.Add(buttonStack);

            border.Child = grid;
            return border;
        }
        //CONOCIDO
        private void VolverCarreras_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GetFromWindow(this)?.NavigateTo(new MateriasView());
        }
        private void BotonGuardarMaterias_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BotonMostrarAgregarMaterias_Click(object sender, RoutedEventArgs e)
        {
            // Verificamos si el panel está oculto
            if (PanelFormularioMateria.Visibility == Visibility.Collapsed)
            {
                // Si está oculto: lo mostramos y cambiamos el texto del botón
                PanelFormularioMateria.Visibility = Visibility.Visible;
                BotonMostrarAgregarMaterias.Content = "Cerrar";
            }
            else
            {
                // Si está visible: lo ocultamos y regresamos el texto original
                PanelFormularioMateria.Visibility = Visibility.Collapsed;
                BotonMostrarAgregarMaterias.Content = "Agregar materias";
            }
        }
    }
}
