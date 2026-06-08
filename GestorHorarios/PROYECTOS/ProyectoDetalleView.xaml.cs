using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GestorHorarios.Models;
using GestorHorarios.Services;
using Microsoft.Data.SqlClient;

namespace GestorHorarios.PROYECTOS
{
    public partial class ProyectoDetalleView : UserControl
    {
        private readonly Proyecto _proyecto;

        public ProyectoDetalleView()
        {
            InitializeComponent();
            _proyecto = new Proyecto();
        }

        public ProyectoDetalleView(Proyecto proyecto) : this()
        {
            _proyecto = proyecto;
            CargarEncabezado();
            CargarCarreras();
        }

        #region Encabezado

        private void CargarEncabezado()
        {
            TxtTituloProyecto.Text = $"{_proyecto.Nombre} del {_proyecto.Anio}";

            var cultura = new CultureInfo("es-MX");
            string fechaFormateada = _proyecto.FechaCreacion.ToString("dd 'de' MMMM 'de' yyyy", cultura);
            TxtFechaProyecto.Text = $"Fecha de creaci\u00f3n: {fechaFormateada}";

            if (_proyecto.Ciclo == "A")
            {
                TxtCicloInfo.Text = "Ciclo A \u2014 Semestres: 2\u00b0, 4\u00b0, 6\u00b0, 8\u00b0";
                TxtCicloInfo.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1565C0"));
                BadgeCiclo.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD"));
            }
            else
            {
                TxtCicloInfo.Text = "Ciclo B \u2014 Semestres: 1\u00b0, 3\u00b0, 5\u00b0, 7\u00b0, 9\u00b0";
                TxtCicloInfo.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E7D32"));
                BadgeCiclo.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F5E9"));
            }
        }

        #endregion

        #region Carreras

        private void CargarCarreras()
        {
            try
            {
                var carreras = new List<Carrera>();
                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                using var cmd = new SqlCommand(
                    "SELECT id_carrera, nombre FROM Carreras ORDER BY id_carrera", conn);
                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    carreras.Add(new Carrera
                    {
                        IdCarrera = Convert.ToInt32(reader["id_carrera"]),
                        Nombre = reader["nombre"].ToString() ?? ""
                    });
                }

                string[] iconos = { "\U0001F4BB", "\U0001F3D7", "\U0001F331", "\U0001F4CA", "\u2699", "\U0001F9EA" };

                for (int i = 0; i < carreras.Count; i++)
                {
                    var carrera = carreras[i];
                    int row = i / 3;
                    int col = i % 3;
                    string icono = i < iconos.Length ? iconos[i] : "\U0001F4CB";

                    var border = new Border
                    {
                        Style = (Style)FindResource("CarreraCardStyle"),
                        Tag = carrera.IdCarrera
                    };
                    border.MouseLeftButtonDown += CarreraCard_Click;

                    var sp = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
                    sp.Children.Add(new TextBlock
                    {
                        Text = icono,
                        FontSize = 36,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 0, 0, 8)
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

                    int totalGrupos = ContarGruposCiclo(carrera.IdCarrera);
                    sp.Children.Add(new TextBlock
                    {
                        Text = $"{totalGrupos} {(totalGrupos == 1 ? "grupo" : "grupos")}",
                        FontSize = 12,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#888888")),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 4, 0, 0)
                    });

                    border.Child = sp;
                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, col);
                    GridCarreras.Children.Add(border);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cargando carreras: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int ContarGruposCiclo(int idCarrera)
        {
            try
            {
                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                string filtro = _proyecto.Ciclo == "B"
                    ? "g.semestre % 2 = 1"
                    : "g.semestre % 2 = 0";

                using var cmd = new SqlCommand(
                    $"SELECT COUNT(*) FROM Grupos g WHERE g.id_carrera = @id AND {filtro}", conn);
                cmd.Parameters.AddWithValue("@id", idCarrera);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch { return 0; }
        }

        private void CarreraCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is int idCarrera)
            {
                NavigationService.GetFromWindow(this)?.NavigateTo(
                    new HorarioCarreraView(_proyecto, idCarrera));
            }
        }

        #endregion

        private void Volver_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mw)
                mw.VolverAlDashboard();
        }
    }
}
