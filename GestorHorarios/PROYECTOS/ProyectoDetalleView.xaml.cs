using GestorHorarios.Models;
using GestorHorarios.Services;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GestorHorarios.PROYECTOS
{
    public partial class ProyectoDetalleView : UserControl
    {
        private readonly Proyecto _proyecto;
        private List<Carrera> _listaCarreras = new List<Carrera>();

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

        private bool TieneHorarioGenerado(int idProyecto, int idCarrera)
        {
            try
            {
                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                using var cmd = new SqlCommand(@"
                    SELECT COUNT(*) 
                    FROM HorarioDetalle hd
                    INNER JOIN Grupos g ON hd.id_grupo = g.id_grupo
                    WHERE hd.id_proyecto = @idProyecto AND g.id_carrera = @idCarrera", conn);

                cmd.Parameters.AddWithValue("@idProyecto", idProyecto);
                cmd.Parameters.AddWithValue("@idCarrera", idCarrera);

                conn.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
            catch { return false; }
        }

        private void CargarCarreras()
        {
            try
            {
                _listaCarreras.Clear();
                GridCarreras.Children.Clear();

                // NUEVO: Limpiamos las filas estáticas del diseño
                GridCarreras.RowDefinitions.Clear();

                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                using var cmd = new SqlCommand("SELECT id_carrera, nombre FROM Carreras ORDER BY id_carrera", conn);
                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _listaCarreras.Add(new Carrera
                    {
                        IdCarrera = Convert.ToInt32(reader["id_carrera"]),
                        Nombre = reader["nombre"].ToString() ?? ""
                    });
                }

                // NUEVO: Calculamos cuántas filas necesitamos automáticamente (3 tarjetas por fila)
                int totalFilas = (int)Math.Ceiling(_listaCarreras.Count / 3.0);
                for (int r = 0; r < totalFilas; r++)
                {
                    GridCarreras.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                }

                // NUEVO: Agregué más íconos a la lista (el 7mo es un maletín para Gestión Empresarial)
                string[] iconos = { "\U0001F4BB", "\U0001F3D7", "\U0001F331", "\U0001F4CA", "\u2699", "\U0001F9EA", "\U0001F4BC", "\U0001F4C8", "\U0001F4DA" };

                for (int i = 0; i < _listaCarreras.Count; i++)
                {
                    var carrera = _listaCarreras[i];
                    int row = i / 3;
                    int col = i % 3;
                    string icono = i < iconos.Length ? iconos[i] : "\U0001F4CB";

                    bool estaGenerado = TieneHorarioGenerado(_proyecto.IdProyecto, carrera.IdCarrera);

                    var border = new Border
                    {
                        Style = (Style)FindResource("CarreraCardStyle"),
                        Tag = carrera.IdCarrera
                    };
                    border.MouseLeftButtonDown += CarreraCard_Click;

                    var mainGrid = new Grid();
                    var sp = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };

                    sp.Children.Add(new TextBlock { Text = icono, FontSize = 36, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 8) });
                    sp.Children.Add(new TextBlock { Text = carrera.Nombre, FontSize = 16, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("GuindaBajo"), HorizontalAlignment = HorizontalAlignment.Center, TextAlignment = TextAlignment.Center, TextWrapping = TextWrapping.Wrap });

                    int totalGrupos = ContarGruposCiclo(carrera.IdCarrera);
                    sp.Children.Add(new TextBlock { Text = $"{totalGrupos} {(totalGrupos == 1 ? "grupo" : "grupos")}", FontSize = 12, Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#888888")), HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 4, 0, 0) });

                    var statusIcon = new TextBlock
                    {
                        Text = estaGenerado ? "\u2714" : "\u274C",
                        FontSize = 18,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(estaGenerado ? "#2E7D32" : "#D32F2F")),
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(0, -5, -5, 0),
                        ToolTip = estaGenerado ? "Horario Generado" : "Horario Pendiente"
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

        private int ContarGruposCiclo(int idCarrera)
        {
            try
            {
                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                string filtro = _proyecto.Ciclo == "B" ? "g.semestre % 2 = 1" : "g.semestre % 2 = 0";
                using var cmd = new SqlCommand($"SELECT COUNT(*) FROM Grupos g WHERE g.id_carrera = @id AND {filtro}", conn);
                cmd.Parameters.AddWithValue("@id", idCarrera);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch { return 0; }
        }

        private async void BtnGenerarTodos_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Este proceso borrará los horarios actuales y generará nuevos para TODAS las carreras.\n\n¿Deseas continuar?",
                "Generar Horarios Globales", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            BtnGenerarTodos.IsEnabled = false;
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

                    var generador = new GeneradorHorariosService();
                    string resultado = await generador.EjecutarDiagnosticoAsync(_proyecto.IdProyecto, carrera.IdCarrera);

                    if (resultado.StartsWith("EXITO"))
                    {
                        exitos++;
                    }
                    else if (!resultado.StartsWith("Omitido")) // Ignoramos las carreras que no tienen grupos (como Inglés)
                    {
                        // Si hubo un error, lo guardamos para el reporte final
                        reporteErrores += $"\n• {carrera.Nombre}: {resultado}\n";
                    }

                    completadas++;
                    BarraProgreso.Value = (completadas * 100) / total;
                }

                // MOSTRAR RESULTADOS AL FINALIZAR
                if (string.IsNullOrEmpty(reporteErrores))
                {
                    MessageBox.Show($"¡Todos los horarios han sido generados exitosamente!\n\nSe procesaron {exitos} carreras.", "Éxito Total", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"Se generaron {exitos} horarios, pero hubo problemas en las siguientes carreras:\n{reporteErrores}", "Reporte de Errores", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error crítico: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnGenerarTodos.IsEnabled = true;
                BarraProgreso.Visibility = Visibility.Collapsed;
                TxtProgreso.Visibility = Visibility.Collapsed;

                // Recargamos las tarjetas para que aparezcan las palomitas verdes
                CargarCarreras();
            }
        }

        // AHORA: Las tarjetas solo sirven para ENTRAR a ver el horario
        private void CarreraCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is int idCarrera)
            {
                bool estaGenerado = TieneHorarioGenerado(_proyecto.IdProyecto, idCarrera);

                if (estaGenerado)
                {
                    NavigationService.GetFromWindow(this)?.NavigateTo(new HorarioCarreraView(_proyecto, idCarrera));
                }
                else
                {
                    MessageBox.Show("Aún no hay un horario calculado para esta carrera.\n\nPor favor, haz clic en el botón verde de arriba 'Generar Todos los Horarios'.",
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