using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using GestorHorarios.Models;
using GestorHorarios.Repositories;
using GestorHorarios.Services;

namespace GestorHorarios.PROYECTOS
{
    public partial class HorarioCarreraView : UserControl
    {
        private readonly Proyecto _proyecto;
        private readonly int _idCarrera;
        private readonly ICarreraRepository _carreraRepository;
        private readonly IGrupoRepository _grupoRepository;
        private readonly IMateriaRepository _materiaRepository;
        private readonly IDocenteRepository _docenteRepository;
        private readonly IHorarioRepository _horarioRepository;
        private string _nombreCarrera = "";

        public HorarioCarreraView()
        {
            InitializeComponent();
            _proyecto = new Proyecto();
            _carreraRepository = new CarreraRepository();
            _grupoRepository = new GrupoRepository();
            _materiaRepository = new MateriaRepository();
            _docenteRepository = new DocenteRepository();
            _horarioRepository = new HorarioRepository();
        }

        public HorarioCarreraView(Proyecto proyecto, int idCarrera) : this()
        {
            _proyecto = proyecto;
            _idCarrera = idCarrera;
            CargarNombreCarrera();
            CargarEncabezado();
            CargarGruposConTablas();
        }

        private void CargarNombreCarrera()
        {
            try
            {
                _nombreCarrera = _carreraRepository.ObtenerNombre(_idCarrera);
            }
            catch
            {
                _nombreCarrera = "Carrera";
            }
        }

        private void CargarEncabezado()
        {
            TxtTitulo.Text = $"Horarios — {_nombreCarrera}";
            TxtSubtitulo.Text = $"Ciclo {_proyecto.Ciclo} · {_proyecto.Anio} · {_proyecto.Periodo}";
        }

        private List<Grupo> ObtenerGruposCiclo() =>
            _grupoRepository.ObtenerPorCicloYCarrera(_proyecto.Ciclo, _idCarrera);

        private List<Materia> ObtenerMateriasSemestre(int semestre) =>
            _materiaRepository.ObtenerPorCarreraYSemestre(_idCarrera, semestre);

        private List<HorarioAsignado> ObtenerHorarioDelGrupo(int idGrupo) =>
            _horarioRepository.ObtenerHorarioGrupo(idGrupo, _proyecto.IdProyecto);

        private void CargarGruposConTablas()
        {
            PanelGrupos.Children.Clear();
            var grupos = ObtenerGruposCiclo();
            if (grupos.Count == 0) return;
            var porSemestre = grupos.GroupBy(g => g.Semestre).OrderBy(g => g.Key);

            foreach (var semGrupo in porSemestre)
            {
                var headerBorder = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD")),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(16, 10, 16, 10),
                    Margin = new Thickness(0, 15, 0, 10)
                };
                headerBorder.Child = new TextBlock
                {
                    Text = $"Semestre {semGrupo.Key}",
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1565C0"))
                };
                PanelGrupos.Children.Add(headerBorder);

                var materias = ObtenerMateriasSemestre(semGrupo.Key);
                foreach (var grupo in semGrupo)
                {
                    PanelGrupos.Children.Add(CrearSeccionGrupo(grupo, materias));
                }
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
            var horarioAsignado = ObtenerHorarioDelGrupo(grupo.IdGrupo);

            bool esMatutino = grupo.Turno?.ToLower().Contains("matutino") == true || grupo.Turno?.ToLower().StartsWith("m") == true;
            int totalHoras = materias.Sum(m => m.Creditos);

            var lblGrupo = new TextBlock
            {
                Text = $"Grupo: {grupo.Nombre}  |  Turno: {(esMatutino ? "Matutino" : "Vespertino")}  |  Horas a la semana: {totalHoras}",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource("GuindaBajo"),
                Margin = new Thickness(0, 0, 0, 15)
            };

            sp.Children.Add(lblGrupo);
            sp.Children.Add(CrearTablaHorario(esMatutino, horarioAsignado));
            if (materias.Count > 0) sp.Children.Add(CrearTablaResumenMaterias(materias, horarioAsignado));

            container.Child = sp;
            return container;
        }

        private Grid CrearTablaHorario(bool esMatutino, List<HorarioAsignado> horarioAsignado)
        {
            var bloques = esMatutino ? BloqueHorarioInfo.Matutino : BloqueHorarioInfo.Vespertino;
            var grid = new Grid { Margin = new Thickness(0, 0, 0, 15) };
            for (int c = 0; c < 6; c++)
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = c == 0 ? new GridLength(110) : new GridLength(1, GridUnitType.Star) });
            for (int r = 0; r <= bloques.Count; r++)
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            string[] encabezados = { "Hora", "Lunes", "Martes", "Miercoles", "Jueves", "Viernes" };
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
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                Grid.SetRow(headerCell, 0);
                Grid.SetColumn(headerCell, c);
                grid.Children.Add(headerCell);
            }

            for (int r = 0; r < bloques.Count; r++)
            {
                var bloque = bloques[r];
                var horaCell = new Border
                {
                    Background = bloque.EsReceso
                        ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFECB3"))
                        : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3E0")),
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DDDDDD")),
                    BorderThickness = new Thickness(0, 0, 0.5, 0.5),
                    Padding = new Thickness(4, 6, 4, 6)
                };
                horaCell.Child = new TextBlock
                {
                    Text = bloque.Horario,
                    FontSize = bloque.EsReceso ? 10 : 11,
                    FontWeight = bloque.EsReceso ? FontWeights.Bold : FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bloque.EsReceso ? "#E65100" : "#333333")),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                Grid.SetRow(horaCell, r + 1);
                Grid.SetColumn(horaCell, 0);
                grid.Children.Add(horaCell);

                for (int c = 1; c <= 5; c++)
                {
                    var cell = new Border
                    {
                        Background = bloque.EsReceso
                            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFECB3"))
                            : Brushes.White,
                        BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DDDDDD")),
                        BorderThickness = new Thickness(0, 0, 0.5, 0.5),
                        Padding = new Thickness(4, 6, 4, 6),
                        MinHeight = bloque.EsReceso ? 30 : 55
                    };

                    if (bloque.EsReceso)
                    {
                        cell.Child = new TextBlock
                        {
                            Text = c == 3 ? "R E C E S O" : "",
                            FontSize = 11,
                            FontWeight = FontWeights.Bold,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E65100")),
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                    }
                    else
                    {
                        var clase = horarioAsignado.FirstOrDefault(h => h.DiaSemana == c && h.BloqueHora == bloque.IdBloque);
                        bool sinMaestro = clase == null || string.IsNullOrEmpty(clase.NombreDocente) || clase.IdDocente <= 0 || clase.NombreDocente == "0";

                        if (clase != null)
                        {
                            var celdaContenido = new StackPanel
                            {
                                VerticalAlignment = VerticalAlignment.Center,
                                HorizontalAlignment = HorizontalAlignment.Center
                            };

                            celdaContenido.Children.Add(new TextBlock
                            {
                                Text = clase.NombreMateria,
                                FontSize = 10,
                                FontWeight = FontWeights.SemiBold,
                                TextAlignment = TextAlignment.Center,
                                TextWrapping = TextWrapping.Wrap
                            });

                            if (!sinMaestro)
                            {
                                celdaContenido.Children.Add(new TextBlock
                                {
                                    Text = $"({clase.NombreDocente})",
                                    FontSize = 9,
                                    Foreground = Brushes.DarkGray,
                                    TextAlignment = TextAlignment.Center,
                                    TextWrapping = TextWrapping.Wrap
                                });
                            }

                            string textoSalon = string.IsNullOrEmpty(clase.NombreSalon)
                                ? "[Sin Salón]"
                                : $"[{clase.NombreSalon} - {clase.NombreEdificio}]";
                            celdaContenido.Children.Add(new TextBlock
                            {
                                Text = textoSalon,
                                FontSize = 10,
                                FontWeight = FontWeights.Bold,
                                Foreground = string.IsNullOrEmpty(clase.NombreSalon)
                                    ? Brushes.Red
                                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E7D32")),
                                TextAlignment = TextAlignment.Center,
                                Margin = new Thickness(0, 3, 0, 0)
                            });

                            cell.Child = celdaContenido;
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

        private Grid CrearTablaResumenMaterias(List<Materia> materias, List<HorarioAsignado> horarioAsignado)
        {
            var grid = new Grid { Margin = new Thickness(0, 5, 0, 0) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
            for (int i = 0; i <= materias.Count; i++)
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            string[] headers = { "Asignatura", "Clave", "Creditos", "Docente" };
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
                var bg = new SolidColorBrush((Color)ColorConverter.ConvertFromString(i % 2 == 0 ? "#FFFFFF" : "#F9F9F9"));
                var clase = horarioAsignado.FirstOrDefault(h => h.NombreMateria == m.Nombre);

                bool sinMaestro = clase == null || string.IsNullOrEmpty(clase.NombreDocente) || clase.IdDocente <= 0;
                string nombreDocente = sinMaestro ? "(Por asignar)" : clase!.NombreDocente;

                AgregarCeldaTexto(grid, i + 1, 0, m.Nombre, bg);
                var cellClave = AgregarCeldaTexto(grid, i + 1, 1, m.Clave, bg, foreground: "#1976D2", esEnlace: true);
                cellClave.Cursor = Cursors.Hand;
                cellClave.ToolTip = "Ver lista de docentes y sus horas para esta materia";

                int idMateriaSeleccionada = m.IdMateria;
                string nombreMateriaSeleccionada = m.Nombre;
                cellClave.MouseLeftButtonDown += (s, e) =>
                {
                    AbrirModalDocentesMateria(idMateriaSeleccionada, nombreMateriaSeleccionada);
                };
                AgregarCeldaTexto(grid, i + 1, 2, m.Creditos.ToString(), bg, HorizontalAlignment.Center);

                var cellDocente = AgregarCeldaTexto(grid, i + 1, 3, nombreDocente, bg, foreground: sinMaestro ? "#999999" : "#1976D2", esItalica: sinMaestro, esEnlace: !sinMaestro);

                if (!sinMaestro && clase != null)
                {
                    cellDocente.Cursor = Cursors.Hand;
                    cellDocente.ToolTip = "Haz clic para ver el horario personal de este docente";
                    cellDocente.MouseLeftButtonDown += (s, e) => { AbrirModalDocente(clase.IdDocente, clase.NombreDocente); };
                }
            }
            return grid;
        }

        private Border AgregarCeldaTexto(Grid grid, int row, int col, string texto, SolidColorBrush bg, HorizontalAlignment hAlign = HorizontalAlignment.Left, string foreground = "#333333", bool esItalica = false, bool esEnlace = false)
        {
            var cell = new Border
            {
                Background = bg,
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DDDDDD")),
                BorderThickness = new Thickness(0, 0, 0.5, 0.5),
                Padding = new Thickness(8, 5, 8, 5)
            };
            var txt = new TextBlock
            {
                Text = texto,
                FontSize = 11,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(foreground)),
                HorizontalAlignment = hAlign,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                FontStyle = esItalica ? FontStyles.Italic : FontStyles.Normal
            };
            if (esEnlace)
            {
                txt.TextDecorations = TextDecorations.Underline;
                txt.FontWeight = FontWeights.SemiBold;
            }
            cell.Child = txt;
            Grid.SetRow(cell, row);
            Grid.SetColumn(cell, col);
            grid.Children.Add(cell);
            return cell;
        }

        private void AbrirModalDocente(int idDocente, string nombreDocente)
        {
            TxtModalNombreDocente.Text = nombreDocente;
            GridHorarioDocente.Children.Clear();
            GridHorarioDocente.RowDefinitions.Clear();
            GridHorarioDocente.ColumnDefinitions.Clear();

            int hfgAsignadas = _docenteRepository.ObtenerHorasFrenteGrupo(idDocente);
            var horarioPersonal = _horarioRepository.ObtenerHorarioPersonalDocente(_proyecto.IdProyecto, idDocente);
            var disponibilidad = _docenteRepository.ObtenerDisponibilidad(idDocente)
                .Select(d => $"{d.IdDia}_{d.IdBloque}")
                .ToHashSet();

            TxtModalTotalHoras.Text = $"HFG Asignadas: {hfgAsignadas}   |   Horas Repartidas: {horarioPersonal.Count}";

            var materiasUnicas = horarioPersonal.Select(h => h.Materia).Distinct().ToList();
            TxtMateriasLista.Text = materiasUnicas.Count > 0 ? string.Join("  •  ", materiasUnicas) : "Sin materias asignadas";

            for (int c = 0; c < 6; c++)
                GridHorarioDocente.ColumnDefinitions.Add(new ColumnDefinition { Width = c == 0 ? new GridLength(110) : new GridLength(1, GridUnitType.Star) });

            GridHorarioDocente.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            string[] encabezados = { "Hora", "Lunes", "Martes", "Miercoles", "Jueves", "Viernes" };
            for (int c = 0; c < encabezados.Length; c++)
            {
                var hCell = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333")),
                    BorderBrush = Brushes.LightGray,
                    BorderThickness = new Thickness(0, 0, 1, 1),
                    Padding = new Thickness(6, 12, 6, 12)
                };
                hCell.Child = new TextBlock
                {
                    Text = encabezados[c],
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                Grid.SetRow(hCell, 0);
                Grid.SetColumn(hCell, c);
                GridHorarioDocente.Children.Add(hCell);
            }

            string[] coloresGrupos = { "#FFF59D", "#E1BEE7", "#B2DFDB", "#F8BBD0", "#BBDEFB", "#C8E6C9", "#FFCC80" };
            var mapaColoresPorGrupo = new Dictionary<string, string>();

            foreach (var clase in horarioPersonal)
            {
                if (!mapaColoresPorGrupo.ContainsKey(clase.Grupo))
                {
                    int indiceColor = Math.Abs(clase.Grupo.GetHashCode()) % coloresGrupos.Length;
                    mapaColoresPorGrupo[clase.Grupo] = coloresGrupos[indiceColor];
                }
            }

            for (int r = 0; r < BloqueHorarioInfo.Unificados.Count; r++)
            {
                var bloque = BloqueHorarioInfo.Unificados[r];

                GridHorarioDocente.RowDefinitions.Add(new RowDefinition { Height = bloque.EsReceso ? new GridLength(35) : new GridLength(85) });

                var horaCell = new Border
                {
                    Background = Brushes.White,
                    BorderBrush = Brushes.LightGray,
                    BorderThickness = new Thickness(0, 0, 1, 1),
                    Padding = new Thickness(4, 6, 4, 6)
                };
                horaCell.Child = new TextBlock
                {
                    Text = bloque.Horario,
                    FontSize = bloque.EsReceso ? 11 : 12,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetRow(horaCell, r + 1);
                Grid.SetColumn(horaCell, 0);
                GridHorarioDocente.Children.Add(horaCell);

                for (int c = 1; c <= 5; c++)
                {
                    bool estaDisponible = disponibilidad.Count == 0 || disponibilidad.Contains($"{c}_{bloque.IdBloque}");
                    string colorFondo = estaDisponible ? "#32CD32" : "#FFFFFF";

                    var bgCell = new Border
                    {
                        Background = bloque.EsReceso
                            ? Brushes.White
                            : new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorFondo)),
                        BorderBrush = Brushes.LightGray,
                        BorderThickness = new Thickness(0, 0, 1, 1)
                    };
                    Grid.SetRow(bgCell, r + 1);
                    Grid.SetColumn(bgCell, c);
                    GridHorarioDocente.Children.Add(bgCell);

                    if (bloque.EsReceso)
                    {
                        var txtReceso = new TextBlock
                        {
                            Text = c == 3 ? "R E C E S O" : "",
                            FontSize = 12,
                            FontWeight = FontWeights.Bold,
                            Foreground = Brushes.Gray,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        Grid.SetRow(txtReceso, r + 1);
                        Grid.SetColumn(txtReceso, c);
                        GridHorarioDocente.Children.Add(txtReceso);
                    }
                    else
                    {
                        var clase = horarioPersonal.FirstOrDefault(h => h.Dia == c && h.Bloque == bloque.IdBloque);
                        if (clase != null)
                        {
                            string colorGrupo = mapaColoresPorGrupo.ContainsKey(clase.Grupo) ? mapaColoresPorGrupo[clase.Grupo] : "#FFF59D";

                            var tarjetaMateria = new Border
                            {
                                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorGrupo)),
                                BorderBrush = Brushes.LightGray,
                                BorderThickness = new Thickness(0, 0, 1, 1),
                                Margin = new Thickness(0),
                                VerticalAlignment = VerticalAlignment.Stretch,
                                HorizontalAlignment = HorizontalAlignment.Stretch
                            };

                            var stackTexto = new StackPanel
                            {
                                VerticalAlignment = VerticalAlignment.Center,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                Margin = new Thickness(5)
                            };

                            stackTexto.Children.Add(new TextBlock
                            {
                                Text = clase.Materia,
                                TextWrapping = TextWrapping.Wrap,
                                TextAlignment = TextAlignment.Center,
                                FontWeight = FontWeights.Bold,
                                FontSize = 11,
                                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222222"))
                            });
                            stackTexto.Children.Add(new TextBlock
                            {
                                Text = $"Grupo: {clase.Grupo}",
                                TextAlignment = TextAlignment.Center,
                                FontSize = 11,
                                Margin = new Thickness(0, 3, 0, 0),
                                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#444444"))
                            });

                            tarjetaMateria.Child = stackTexto;
                            Grid.SetRow(tarjetaMateria, r + 1);
                            Grid.SetColumn(tarjetaMateria, c);
                            GridHorarioDocente.Children.Add(tarjetaMateria);
                        }
                    }
                }
            }
            ModalDocente.Visibility = Visibility.Visible;
        }

        private void CerrarModal_Click(object sender, RoutedEventArgs e)
        {
            ModalDocente.Visibility = Visibility.Collapsed;
        }

        private void Volver_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GetFromWindow(this)?.NavigateTo(new ProyectoDetalleView(_proyecto));
        }

        private void AbrirModalDocentesMateria(int idMateria, string nombreMateria)
        {
            TxtModalNombreMateria.Text = nombreMateria;
            PanelListaDocentesMateria.Children.Clear();

            try
            {
                var docentes = _docenteRepository.ObtenerAsignablesAMateriaEnProyecto(idMateria, _proyecto.IdProyecto);

                foreach (var doc in docentes)
                {
                    int idDoc = doc.IdDocente;
                    string nombreDoc = doc.Nombre;
                    int hfg = doc.Hfg;
                    int repartidas = doc.HorasRepartidas;
                    string carreraPrin = doc.CarreraPrincipal;

                    var tarjetaDocente = new Border
                    {
                        Background = Brushes.White,
                        BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0")),
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(8),
                        Margin = new Thickness(0, 0, 0, 10),
                        Padding = new Thickness(15),
                        Cursor = Cursors.Hand,
                        ToolTip = "Ver horario personal"
                    };

                    tarjetaDocente.MouseEnter += (s, e) => tarjetaDocente.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F9FF"));
                    tarjetaDocente.MouseLeave += (s, e) => tarjetaDocente.Background = Brushes.White;
                    tarjetaDocente.MouseLeftButtonDown += (s, e) => { AbrirModalDocente(idDoc, nombreDoc); };

                    var gridInfo = new Grid();
                    gridInfo.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    gridInfo.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    var stackTexto = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
                    stackTexto.Children.Add(new TextBlock
                    {
                        Text = nombreDoc,
                        FontSize = 14,
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333"))
                    });
                    stackTexto.Children.Add(new TextBlock
                    {
                        Text = carreraPrin,
                        FontSize = 11,
                        Foreground = Brushes.Gray,
                        Margin = new Thickness(0, 2, 0, 0)
                    });

                    Grid.SetColumn(stackTexto, 0);
                    gridInfo.Children.Add(stackTexto);

                    var stackHoras = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
                    string colorRepartidas = repartidas > hfg ? "#D32F2F" : (repartidas == hfg ? "#388E3C" : "#1976D2");

                    stackHoras.Children.Add(new TextBlock
                    {
                        Text = $"HFG Asignadas: {hfg}h",
                        FontSize = 12,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = Brushes.Gray,
                        Margin = new Thickness(0, 0, 15, 0)
                    });
                    stackHoras.Children.Add(new TextBlock
                    {
                        Text = $"Repartidas: {repartidas}h",
                        FontSize = 12,
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorRepartidas))
                    });

                    Grid.SetColumn(stackHoras, 1);
                    gridInfo.Children.Add(stackHoras);

                    tarjetaDocente.Child = gridInfo;
                    PanelListaDocentesMateria.Children.Add(tarjetaDocente);
                }

                if (PanelListaDocentesMateria.Children.Count == 0)
                {
                    PanelListaDocentesMateria.Children.Add(new TextBlock
                    {
                        Text = "Ningun docente tiene esta materia asignada en su perfil.",
                        FontSize = 14,
                        Foreground = Brushes.Gray,
                        FontStyle = FontStyles.Italic,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 20, 0, 0)
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los docentes de la materia:\n{ex.Message}", "Error de consulta", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            ModalDocentesMateria.Visibility = Visibility.Visible;
        }

        private void CerrarModalMateria_Click(object sender, RoutedEventArgs e)
        {
            ModalDocentesMateria.Visibility = Visibility.Collapsed;
        }
    }
}