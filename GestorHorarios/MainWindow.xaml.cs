using GestorHorarios.DOCENTES;
using GestorHorarios.GRUPOS;
using GestorHorarios.MATERIAS;
using GestorHorarios.Models;
using GestorHorarios.PROYECTOS;
using GestorHorarios.SALONES;
using GestorHorarios.Services;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GestorHorarios
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly object _dashboardContent;

        public INavigationService Navigation { get; }

        public MainWindow()
        {
            InitializeComponent();
            Navigation = new NavigationService(MainContentControl);
            _dashboardContent = MainContentControl.Content;
            CargarDashboard();
        }

        /// <summary>
        /// Restaura el dashboard y recarga datos. Llamado desde vistas de proyecto.
        /// </summary>
        public void VolverAlDashboard()
        {
            MainContentControl.Content = _dashboardContent;
            CargarDashboard();
        }

        private void CargarDashboard()
        {
            CargarEstadisticas();
            CargarProyectos();
        }

        #region Estadisticas

        private void CargarEstadisticas()
        {
            try
            {
                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                conn.Open();

                TxtStatDocentes.Text = ContarRegistros(conn, "Docentes");
                TxtStatMaterias.Text = ContarRegistros(conn, "Materias");
                TxtStatGrupos.Text = ContarRegistros(conn, "Grupos");
                TxtStatSalones.Text = ContarRegistros(conn, "Salones");

                // Proyectos puede no existir aun
                try
                {
                    TxtStatProyectos.Text = ContarRegistros(conn, "Proyectos", "id_estado = 1");
                }
                catch { TxtStatProyectos.Text = "0"; }
            }
            catch
            {
                // Si falla la BD, dejar en 0
            }
        }

        private static string ContarRegistros(SqlConnection conn, string tabla, string? where = null)
        {
            string sql = $"SELECT COUNT(*) FROM {tabla}";
            if (!string.IsNullOrEmpty(where))
                sql += $" WHERE {where}";

            using var cmd = new SqlCommand(sql, conn);
            return Convert.ToInt32(cmd.ExecuteScalar()).ToString();
        }

        #endregion

        #region Proyectos en Dashboard

        private void CargarProyectos()
        {
            PanelListaProyectos.Children.Clear();

            try
            {
                var proyectos = ObtenerProyectos();

                if (proyectos.Count == 0)
                {
                    PanelListaProyectos.Children.Add(new TextBlock
                    {
                        Text = "No hay proyectos creados. Presiona \"+ Nuevo Proyecto\" para comenzar.",
                        FontSize = 14,
                        Foreground = Brushes.Gray,
                        FontStyle = FontStyles.Italic,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 20, 0, 20)
                    });
                    return;
                }

                foreach (var p in proyectos)
                    PanelListaProyectos.Children.Add(CrearCardProyecto(p));
            }
            catch
            {
                PanelListaProyectos.Children.Add(new TextBlock
                {
                    Text = "Ejecuta el script CrearTablaProyectos.sql para habilitar los proyectos.",
                    FontSize = 13,
                    Foreground = Brushes.Gray,
                    FontStyle = FontStyles.Italic,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 20, 0, 20)
                });
            }
        }

        private static List<Proyecto> ObtenerProyectos()
        {
            var lista = new List<Proyecto>();
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand(
                @"SELECT id_proyecto, nombre, anio, periodo, ciclo, fecha_creacion
                  FROM Proyectos WHERE id_estado = 1
                  ORDER BY fecha_creacion DESC", conn);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Proyecto
                {
                    IdProyecto = Convert.ToInt32(reader["id_proyecto"]),
                    Nombre = reader["nombre"].ToString() ?? "",
                    Anio = Convert.ToInt32(reader["anio"]),
                    Periodo = reader["periodo"].ToString() ?? "",
                    Ciclo = reader["ciclo"].ToString() ?? "",
                    FechaCreacion = Convert.ToDateTime(reader["fecha_creacion"])
                });
            }
            return lista;
        }

        private Border CrearCardProyecto(Proyecto p)
        {
            var border = new Border
            {
                Style = (Style)FindResource("ProjectCardStyle"),
                Tag = p
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Icono
            var iconBorder = new Border
            {
                Background = (Brush)FindResource("GuindaBajo"),
                CornerRadius = new CornerRadius(8),
                Width = 60,
                Height = 60,
                Margin = new Thickness(0, 0, 20, 0),
                Child = new TextBlock
                {
                    Text = "\U0001F4C5",
                    FontSize = 28,
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
            Grid.SetColumn(iconBorder, 0);
            grid.Children.Add(iconBorder);

            // Info
            var infoPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

            var headerPanel = new StackPanel { Orientation = Orientation.Horizontal };
            headerPanel.Children.Add(new TextBlock
            {
                Text = $"{p.Nombre} {p.Anio}",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource("GuindaBajo"),
                VerticalAlignment = VerticalAlignment.Center
            });

            // Badge ciclo
            string cicloBg = p.Ciclo == "B" ? "#E8F5E9" : "#E3F2FD";
            string cicloFg = p.Ciclo == "B" ? "#2E7D32" : "#1565C0";
            headerPanel.Children.Add(new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(cicloBg)),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(8, 3, 8, 3),
                Margin = new Thickness(10, 0, 0, 0),
                Child = new TextBlock
                {
                    Text = $"Ciclo {p.Ciclo}",
                    FontSize = 11,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(cicloFg))
                }
            });
            infoPanel.Children.Add(headerPanel);

            var cultura = new CultureInfo("es-MX");
            string fecha = p.FechaCreacion.ToString("dd 'de' MMMM 'de' yyyy", cultura);
            infoPanel.Children.Add(new TextBlock
            {
                Text = $"{p.Periodo} \u2022 Creado el {fecha}",
                FontSize = 12,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#888888")),
                Margin = new Thickness(0, 4, 0, 0)
            });

            Grid.SetColumn(infoPanel, 1);
            grid.Children.Add(infoPanel);

            // Botones
            var botonesPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20, 0, 0, 0)
            };

            var btnVer = new Button
            {
                Content = "Ver",
                Padding = new Thickness(20, 8, 20, 8),
                Margin = new Thickness(5, 0, 5, 0),
                Tag = p
            };
            btnVer.Click += VerProyecto_Click;
            botonesPanel.Children.Add(btnVer);

            var btnEliminar = new Button
            {
                Content = "Eliminar",
                Padding = new Thickness(15, 8, 15, 8),
                Margin = new Thickness(5, 0, 0, 0),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E53935")),
                Tag = p.IdProyecto
            };
            btnEliminar.Click += EliminarProyecto_Click;
            botonesPanel.Children.Add(btnEliminar);

            Grid.SetColumn(botonesPanel, 2);
            grid.Children.Add(botonesPanel);

            border.Child = grid;
            return border;
        }

        private void VerProyecto_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Proyecto p)
            {
                Navigation.NavigateTo(new ProyectoDetalleView(p));
            }
        }

        private void EliminarProyecto_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not int idProyecto) return;

            var resultado = MessageBox.Show(
                "\u00bfEst\u00e1 seguro de eliminar este proyecto y todos sus horarios?",
                "Eliminar Proyecto", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (resultado != MessageBoxResult.Yes) return;

            try
            {
                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                conn.Open();
                using var tx = conn.BeginTransaction();
                try
                {
                    using (var cmd = new SqlCommand(
                        "DELETE FROM HorarioDetalle WHERE id_proyecto = @id", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@id", idProyecto);
                        cmd.ExecuteNonQuery();
                    }
                    using (var cmd = new SqlCommand(
                        "DELETE FROM Proyectos WHERE id_proyecto = @id", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@id", idProyecto);
                        cmd.ExecuteNonQuery();
                    }
                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }

                MessageBox.Show("Proyecto eliminado.", "\u00c9xito",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                CargarDashboard();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Navegacion

        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            VolverAlDashboard();
        }

        private void BtnDocentes_Click(object sender, RoutedEventArgs e)
        {
            Navigation.NavigateTo(new DocentesView());
        }

        private void BtnMaterias_Click(object sender, RoutedEventArgs e)
        {
            Navigation.NavigateTo(new MateriasView());
        }

        private void BtnGrupos_Click(object sender, RoutedEventArgs e)
        {
            Navigation.NavigateTo(new GruposView());
        }

        private void BtnSalones_Click(object sender, RoutedEventArgs e)
        {
            Navigation.NavigateTo(new SalonesView());
        }

        private void BtnNuevoProyecto_Click(object sender, RoutedEventArgs e)
        {
            Navigation.NavigateTo(new NuevoProyectoView());
        }

        #endregion
    }
}