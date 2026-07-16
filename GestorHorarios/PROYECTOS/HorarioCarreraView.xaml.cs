using GestorHorarios.Models;
using GestorHorarios.Services;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace GestorHorarios.PROYECTOS
{
    public partial class HorarioCarreraView : UserControl
    {
        private readonly Proyecto _proyecto;
        private readonly int _idCarrera;
        private string _nombreCarrera = "";

        // Bloques matutino: 7:30-13:30 con receso 9:20-9:40
        private static readonly string[] BLOQUES_MATUTINO = { "7:30 - 8:30", "8:30 - 9:20", "9:20 - 9:40", "9:40 - 10:30", "10:30 - 11:30", "11:30 - 12:30", "12:30 - 13:30" };

        // Bloques vespertino: 13:30-19:30 con receso 15:20-15:40
        private static readonly string[] BLOQUES_VESPERTINO = { "13:30 - 14:30", "14:30 - 15:20", "15:20 - 15:40", "15:40 - 16:30", "16:30 - 17:30", "17:30 - 18:30", "18:30 - 19:30" };

        // Indices de receso (posicion 2 en ambos arrays)
        private const int INDICE_RECESO = 2;

        private static readonly string[] DIAS = { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes" };

        public HorarioCarreraView()
        {
            InitializeComponent();
            _proyecto = new Proyecto();
        }

        public HorarioCarreraView(Proyecto proyecto, int idCarrera) : this()
        {
            _proyecto = proyecto;
            _idCarrera = idCarrera;
            CargarNombreCarrera();
            CargarEncabezado();
            CargarGruposConTablas();
        }

        #region Carga de datos originales

        private void CargarNombreCarrera()
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
                _nombreCarrera = cmd.ExecuteScalar()?.ToString() ?? "Carrera";
            }
            catch { _nombreCarrera = "Carrera"; }
        }

        private void CargarEncabezado()
        {
            TxtTitulo.Text = $"Horarios — {_nombreCarrera}";
            TxtSubtitulo.Text = $"Ciclo {_proyecto.Ciclo} · {_proyecto.Anio} · {_proyecto.Periodo}";
        }

        private List<Grupo> ObtenerGruposCiclo()
        {
            var grupos = new List<Grupo>();
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            string filtro = _proyecto.Ciclo == "B" ? "g.semestre % 2 = 1" : "g.semestre % 2 = 0";
            using var cmd = new SqlCommand(
                $@"SELECT g.id_grupo, g.nombre AS NombreGrupo, g.semestre, g.turno,
                          c.nombre AS NombreCarrera
                   FROM Grupos g
                   JOIN Carreras c ON g.id_carrera = c.id_carrera
                   WHERE g.id_carrera = @id AND {filtro}
                   ORDER BY g.semestre, g.nombre", conn);
            cmd.Parameters.AddWithValue("@id", _idCarrera);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                grupos.Add(new Grupo
                {
                    IdGrupo = Convert.ToInt32(reader["id_grupo"]),
                    Nombre = reader["NombreGrupo"].ToString() ?? "",
                    Semestre = Convert.ToInt32(reader["semestre"]),
                    Turno = reader["turno"].ToString() ?? "",
                    NombreCarrera = reader["NombreCarrera"].ToString() ?? ""
                });
            }
            return grupos;
        }

        private List<Materia> ObtenerMateriasSemestre(int semestre)
        {
            var materias = new List<Materia>();
            try
            {
                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                using var cmd = new SqlCommand(
                    @"SELECT id_materia, nombre, clave, creditos, semestre
                      FROM Materias WHERE id_carrera = @id AND semestre = @sem
                      ORDER BY nombre", conn);
                cmd.Parameters.AddWithValue("@id", _idCarrera);
                cmd.Parameters.AddWithValue("@sem", semestre);
                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    materias.Add(new Materia
                    {
                        IdMateria = Convert.ToInt32(reader["id_materia"]),
                        Nombre = reader["nombre"].ToString() ?? "",
                        Clave = reader["clave"].ToString() ?? "",
                        Creditos = Convert.ToInt32(reader["creditos"]),
                        Semestre = Convert.ToInt32(reader["semestre"])
                    });
                }
            }
            catch { }
            return materias;
        }

        #endregion

        #region Nuevos metodos para el Horario Generado

        // METODO NUEVO: Consulta la BD para traer el horario ya generado
        private List<HorarioAsignado> ObtenerHorarioDelGrupo(int idGrupo)
        {
            var horarios = new List<HorarioAsignado>();
            try
            {
                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                using var cmd = new SqlCommand(@"
                    SELECT h.id_dia AS dia_semana, h.id_bloque AS bloque_hora, m.nombre AS Materia, d.nombre AS Docente 
                    FROM HorarioDetalle h
                    INNER JOIN Materias m ON h.id_materia = m.id_materia
                    LEFT JOIN Docentes d ON h.id_docente = d.id_docente
                    WHERE h.id_grupo = @idGrupo AND h.id_proyecto = @idProyecto", conn);

                cmd.Parameters.AddWithValue("@idGrupo", idGrupo);
                cmd.Parameters.AddWithValue("@idProyecto", _proyecto.IdProyecto);

                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    horarios.Add(new HorarioAsignado
                    {
                        DiaSemana = Convert.ToInt32(reader["dia_semana"]),
                        BloqueHora = Convert.ToInt32(reader["bloque_hora"]),
                        NombreMateria = reader["Materia"].ToString() ?? "",
                        NombreDocente = reader["Docente"]?.ToString() ?? "Sin Asignar"
                    });
                }
            }
            catch { }
            return horarios;
        }

        private void CargarGruposConTablas()
        {
            PanelGrupos.Children.Clear();

            try
            {
                var grupos = ObtenerGruposCiclo();

                if (grupos.Count == 0)
                {
                    PanelGrupos.Children.Add(new TextBlock
                    {
                        Text = $"No hay grupos registrados para el ciclo {_proyecto.Ciclo} en {_nombreCarrera}.",
                        FontSize = 14,
                        Foreground = Brushes.Gray,
                        FontStyle = FontStyles.Italic,
                        Margin = new Thickness(0, 10, 0, 10)
                    });
                    return;
                }

                var porSemestre = grupos.GroupBy(g => g.Semestre).OrderBy(g => g.Key);

                foreach (var semGrupo in porSemestre)
                {
                    // Encabezado del semestre
                    string bgColor = _proyecto.Ciclo == "B" ? "#E8F5E9" : "#E3F2FD";
                    string fgColor = _proyecto.Ciclo == "B" ? "#2E7D32" : "#1565C0";
                    var headerBorder = new Border
                    {
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bgColor)),
                        CornerRadius = new CornerRadius(8),
                        Padding = new Thickness(16, 10, 16, 10),
                        Margin = new Thickness(0, 15, 0, 10)
                    };
                    headerBorder.Child = new TextBlock
                    {
                        Text = $"📖 Semestre {semGrupo.Key}",
                        FontSize = 16,
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fgColor))
                    };
                    PanelGrupos.Children.Add(headerBorder);

                    var materias = ObtenerMateriasSemestre(semGrupo.Key);

                    foreach (var grupo in semGrupo)
                    {
                        PanelGrupos.Children.Add(CrearSeccionGrupo(grupo, materias));
                    }
                }
            }
            catch (Exception ex)
            {
                PanelGrupos.Children.Add(new TextBlock
                {
                    Text = $"Error al cargar grupos: {ex.Message}",
                    Foreground = Brushes.Red,
                    Margin = new Thickness(0, 10, 0, 10)
                });
            }
        }

        private Border CrearSeccionGrupo(Grupo grupo, List<Materia> materias)
        {
            var container = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(20),
                Margin = new Thickness(0, 0, 0, 20),
                Effect = new DropShadowEffect
                {
                    Color = (Color)ColorConverter.ConvertFromString("#DDDDDD"),
                    Direction = 270,
                    ShadowDepth = 2,
                    BlurRadius = 10,
                    Opacity = 0.3
                }
            };

            var sp = new StackPanel();

            var headerGrid = new Grid { Margin = new Thickness(0, 0, 0, 12) };
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var grupoInfo = new StackPanel();
            grupoInfo.Children.Add(new TextBlock
            {
                Text = $"Grupo: {grupo.Nombre}",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource("GuindaBajo")
            });

            bool esMatutino = grupo.Turno?.ToUpper() == "MATUTINO";
            string turnoTexto = esMatutino ? "Matutino (7:30 - 13:30)" : "Vespertino (13:30 - 19:30)";
            int totalHoras = materias.Sum(m => m.Creditos);

            grupoInfo.Children.Add(new TextBlock
            {
                Text = $"Turno: {turnoTexto}   |   Total de horas: {totalHoras}",
                FontSize = 12,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#888888")),
                Margin = new Thickness(0, 3, 0, 0)
            });

            Grid.SetColumn(grupoInfo, 0);
            headerGrid.Children.Add(grupoInfo);
            sp.Children.Add(headerGrid);

            // AQUI SE MANDAN A TRAER LOS HORARIOS CALCULADOS DE ESTE GRUPO
            var horarioAsignado = ObtenerHorarioDelGrupo(grupo.IdGrupo);

            // Tabla de horario (Le pasamos los datos a la cuadrícula)
            sp.Children.Add(CrearTablaHorario(esMatutino, horarioAsignado));

            if (materias.Count > 0)
                sp.Children.Add(CrearTablaResumenMaterias(materias));

            container.Child = sp;
            return container;
        }

        private Grid CrearTablaHorario(bool esMatutino, List<HorarioAsignado> horarioAsignado)
        {
            string[] bloques = esMatutino ? BLOQUES_MATUTINO : BLOQUES_VESPERTINO;
            var grid = new Grid { Margin = new Thickness(0, 0, 0, 15) };

            for (int c = 0; c < 6; c++)
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = c == 0 ? new GridLength(110) : new GridLength(1, GridUnitType.Star) });

            for (int r = 0; r <= bloques.Length; r++)
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            string[] encabezados = { "Hora", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes" };
            for (int c = 0; c < encabezados.Length; c++)
            {
                var headerCell = new Border
                {
                    Background = (Brush)FindResource("GuindaBajo"),
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5A002C")),
                    BorderThickness = new Thickness(0, 0, 0.5, 0.5),
                    Padding = new Thickness(6, 8, 6, 8)
                };
                headerCell.Child = new TextBlock
                {
                    Text = encabezados[c],
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center
                };
                Grid.SetRow(headerCell, 0);
                Grid.SetColumn(headerCell, c);
                grid.Children.Add(headerCell);
            }

            for (int r = 0; r < bloques.Length; r++)
            {
                bool esReceso = r == INDICE_RECESO;

                var horaCell = new Border
                {
                    Background = esReceso ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFECB3")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3E0")),
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DDDDDD")),
                    BorderThickness = new Thickness(0, 0, 0.5, 0.5),
                    Padding = new Thickness(4, 6, 4, 6)
                };
                horaCell.Child = new TextBlock
                {
                    Text = bloques[r],
                    FontSize = esReceso ? 10 : 11,
                    FontWeight = esReceso ? FontWeights.Bold : FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(esReceso ? "#E65100" : "#333333")),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                };
                Grid.SetRow(horaCell, r + 1);
                Grid.SetColumn(horaCell, 0);
                grid.Children.Add(horaCell);

                for (int c = 1; c <= 5; c++)
                {
                    var cell = new Border
                    {
                        Background = esReceso ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFECB3")) : Brushes.White,
                        BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DDDDDD")),
                        BorderThickness = new Thickness(0, 0, 0.5, 0.5),
                        Padding = new Thickness(4, 6, 4, 6),
                        MinHeight = esReceso ? 30 : 45
                    };

                    if (esReceso)
                    {
                        cell.Child = new TextBlock
                        {
                            Text = c == 3 ? "R  E  C  E  S  O" : "",
                            FontSize = 11,
                            FontWeight = FontWeights.Bold,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E65100")),
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                    }
                    else
                    {
                        // AQUÍ CRUZAMOS LA CUADRÍCULA CON LOS DATOS DE LA BASE DE DATOS
                        var claseActual = horarioAsignado.FirstOrDefault(h => h.DiaSemana == c && h.BloqueHora == r);

                        string textoCelda = claseActual != null ? $"{claseActual.NombreMateria}\n({claseActual.NombreDocente})" : "";

                        cell.Child = new TextBlock
                        {
                            Text = textoCelda,
                            FontSize = 10,
                            TextWrapping = TextWrapping.Wrap,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            TextAlignment = TextAlignment.Center,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333")),
                            FontWeight = claseActual != null ? FontWeights.SemiBold : FontWeights.Normal
                        };

                        if (claseActual != null)
                        {
                            cell.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8EAF6"));
                        }
                    }

                    Grid.SetRow(cell, r + 1);
                    Grid.SetColumn(cell, c);
                    grid.Children.Add(cell);
                }
            }

            return grid;
        }

        #endregion

        #region Tabla Resumen y Navegacion (Original)

        private Grid CrearTablaResumenMaterias(List<Materia> materias)
        {
            var grid = new Grid { Margin = new Thickness(0, 5, 0, 0) };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });

            for (int i = 0; i <= materias.Count; i++)
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            string[] headers = { "Asignatura", "Clave", "Créditos", "Docente" };
            for (int c = 0; c < headers.Length; c++)
            {
                var cell = new Border
                {
                    Background = (Brush)FindResource("GuindaBajo"),
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5A002C")),
                    BorderThickness = new Thickness(0, 0, 0.5, 0.5),
                    Padding = new Thickness(8, 6, 8, 6)
                };
                cell.Child = new TextBlock
                {
                    Text = headers[c],
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White,
                    HorizontalAlignment = c == 2 ? HorizontalAlignment.Center : HorizontalAlignment.Left
                };
                Grid.SetRow(cell, 0);
                Grid.SetColumn(cell, c);
                grid.Children.Add(cell);
            }

            for (int i = 0; i < materias.Count; i++)
            {
                var m = materias[i];
                string bgColor = i % 2 == 0 ? "#FFFFFF" : "#F9F9F9";
                var bg = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bgColor));

                AgregarCeldaTexto(grid, i + 1, 0, m.Nombre, bg);
                AgregarCeldaTexto(grid, i + 1, 1, m.Clave, bg);
                AgregarCeldaTexto(grid, i + 1, 2, m.Creditos.ToString(), bg, HorizontalAlignment.Center);
                AgregarCeldaTexto(grid, i + 1, 3, "(Por asignar)", bg, foreground: "#999999", esItalica: true);
            }

            return grid;
        }

        private static void AgregarCeldaTexto(Grid grid, int row, int col, string texto, SolidColorBrush bg, HorizontalAlignment hAlign = HorizontalAlignment.Left, string foreground = "#333333", bool esItalica = false)
        {
            var cell = new Border
            {
                Background = bg,
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DDDDDD")),
                BorderThickness = new Thickness(0, 0, 0.5, 0.5),
                Padding = new Thickness(8, 5, 8, 5)
            };
            cell.Child = new TextBlock
            {
                Text = texto,
                FontSize = 11,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(foreground)),
                HorizontalAlignment = hAlign,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                FontStyle = esItalica ? FontStyles.Italic : FontStyles.Normal
            };
            Grid.SetRow(cell, row);
            Grid.SetColumn(cell, col);
            grid.Children.Add(cell);
        }

        private void Volver_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GetFromWindow(this)?.NavigateTo(new ProyectoDetalleView(_proyecto));
        }

        #endregion

        public class HorarioAsignado
        {
            public int DiaSemana { get; set; }
            public int BloqueHora { get; set; }
            public string NombreMateria { get; set; } = "";
            public string NombreDocente { get; set; } = "";
        }
    }
}
