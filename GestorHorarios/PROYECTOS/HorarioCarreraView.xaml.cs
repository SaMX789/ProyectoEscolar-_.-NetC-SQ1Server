using GestorHorarios.Models;
using GestorHorarios.Services;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace GestorHorarios.PROYECTOS
{
    public partial class HorarioCarreraView : UserControl
    {
        private readonly Proyecto _proyecto;
        private readonly int _idCarrera;
        private string _nombreCarrera = "";

        private class BloqueUI
        {
            public string Horario { get; set; }
            public int IdBloque { get; set; }
            public bool EsReceso { get; set; }
        }

        private static readonly List<BloqueUI> BLOQUES_MATUTINO = new List<BloqueUI>
        {
            new BloqueUI { Horario = "7:30 - 8:30", IdBloque = 1 }, new BloqueUI { Horario = "8:30 - 9:20", IdBloque = 2 },
            new BloqueUI { Horario = "9:20 - 9:40", IdBloque = 0, EsReceso = true }, new BloqueUI { Horario = "9:40 - 10:30", IdBloque = 3 },
            new BloqueUI { Horario = "10:30 - 11:30", IdBloque = 4 }, new BloqueUI { Horario = "11:30 - 12:30", IdBloque = 5 },
            new BloqueUI { Horario = "12:30 - 13:30", IdBloque = 6 }, new BloqueUI { Horario = "13:30 - 14:30", IdBloque = 7 }
        };

        private static readonly List<BloqueUI> BLOQUES_VESPERTINO = new List<BloqueUI>
        {
            new BloqueUI { Horario = "12:30 - 13:30", IdBloque = 6 }, new BloqueUI { Horario = "13:30 - 14:30", IdBloque = 7 },
            new BloqueUI { Horario = "14:30 - 15:20", IdBloque = 8 }, new BloqueUI { Horario = "15:20 - 15:40", IdBloque = 0, EsReceso = true },
            new BloqueUI { Horario = "15:40 - 16:30", IdBloque = 9 }, new BloqueUI { Horario = "16:30 - 17:30", IdBloque = 10 },
            new BloqueUI { Horario = "17:30 - 18:30", IdBloque = 11 }, new BloqueUI { Horario = "18:30 - 19:30", IdBloque = 12 }
        };

        private static readonly List<BloqueUI> BLOQUES_UNIFICADOS = new List<BloqueUI>
        {
            new BloqueUI { Horario = "7:30 - 8:30", IdBloque = 1 }, new BloqueUI { Horario = "8:30 - 9:20", IdBloque = 2 },
            new BloqueUI { Horario = "9:20 - 9:40", IdBloque = 0, EsReceso = true }, new BloqueUI { Horario = "9:40 - 10:30", IdBloque = 3 },
            new BloqueUI { Horario = "10:30 - 11:30", IdBloque = 4 }, new BloqueUI { Horario = "11:30 - 12:30", IdBloque = 5 },
            new BloqueUI { Horario = "12:30 - 13:30", IdBloque = 6 }, new BloqueUI { Horario = "13:30 - 14:30", IdBloque = 7 },
            new BloqueUI { Horario = "14:30 - 15:20", IdBloque = 8 }, new BloqueUI { Horario = "15:20 - 15:40", IdBloque = 0, EsReceso = true },
            new BloqueUI { Horario = "15:40 - 16:30", IdBloque = 9 }, new BloqueUI { Horario = "16:30 - 17:30", IdBloque = 10 },
            new BloqueUI { Horario = "17:30 - 18:30", IdBloque = 11 }, new BloqueUI { Horario = "18:30 - 19:30", IdBloque = 12 }
        };

        public HorarioCarreraView() { InitializeComponent(); _proyecto = new Proyecto(); }

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
                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                using var cmd = new SqlCommand("sp_ObtenerNombreCarrera", conn) { CommandType = CommandType.StoredProcedure };
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
            using var cmd = new SqlCommand($@"SELECT g.id_grupo, g.nombre AS NombreGrupo, g.semestre, g.turno FROM Grupos g WHERE g.id_carrera = @id AND {filtro} ORDER BY g.semestre, g.nombre", conn);
            cmd.Parameters.AddWithValue("@id", _idCarrera);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                grupos.Add(new Grupo { IdGrupo = Convert.ToInt32(reader["id_grupo"]), Nombre = reader["NombreGrupo"].ToString() ?? "", Semestre = Convert.ToInt32(reader["semestre"]), Turno = reader["turno"].ToString() ?? "" });
            }
            return grupos;
        }

        private List<Materia> ObtenerMateriasSemestre(int semestre)
        {
            var materias = new List<Materia>();
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand(@"SELECT id_materia, nombre, clave, creditos, semestre FROM Materias WHERE id_carrera = @id AND semestre = @sem AND id_estado = 1 ORDER BY nombre", conn);
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
                    Creditos = Convert.ToInt32(reader["creditos"])
                });
            }
            return materias;
        }

        private List<HorarioAsignado> ObtenerHorarioDelGrupo(int idGrupo)
        {
            var horarios = new List<HorarioAsignado>();
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());

            // SE ACTUALIZÓ PARA USAR EL PROCEDIMIENTO ALMACENADO
            using var cmd = new SqlCommand("sp_GetHorarioDelGrupo", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@idGrupo", idGrupo);
            cmd.Parameters.AddWithValue("@idProyecto", _proyecto.IdProyecto);

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                horarios.Add(new HorarioAsignado
                {
                    DiaSemana = Convert.ToInt32(reader["id_dia"]),
                    BloqueHora = Convert.ToInt32(reader["id_bloque"]),
                    NombreMateria = reader["Materia"].ToString() ?? "",
                    NombreDocente = reader["Docente"]?.ToString() ?? "",
                    IdDocente = reader["id_docente"] != DBNull.Value ? Convert.ToInt32(reader["id_docente"]) : 0,
                    NombreSalon = reader["Salon"]?.ToString() ?? "",
                    NombreEdificio = reader["Edificio"]?.ToString() ?? ""
                });
            }
            return horarios;
        }

        private void CargarGruposConTablas()
        {
            PanelGrupos.Children.Clear();
            var grupos = ObtenerGruposCiclo();
            if (grupos.Count == 0) return;
            var porSemestre = grupos.GroupBy(g => g.Semestre).OrderBy(g => g.Key);

            foreach (var semGrupo in porSemestre)
            {
                var headerBorder = new Border { Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD")), CornerRadius = new CornerRadius(8), Padding = new Thickness(16, 10, 16, 10), Margin = new Thickness(0, 15, 0, 10) };
                headerBorder.Child = new TextBlock { Text = $"Semestre {semGrupo.Key}", FontSize = 16, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1565C0")) };
                PanelGrupos.Children.Add(headerBorder);
                var materias = ObtenerMateriasSemestre(semGrupo.Key);
                foreach (var grupo in semGrupo) PanelGrupos.Children.Add(CrearSeccionGrupo(grupo, materias));
            }
        }

        private Border CrearSeccionGrupo(Grupo grupo, List<Materia> materias)
        {
            var container = new Border { Background = Brushes.White, CornerRadius = new CornerRadius(10), Padding = new Thickness(20), Margin = new Thickness(0, 0, 0, 20), Effect = new DropShadowEffect { Color = (Color)ColorConverter.ConvertFromString("#DDDDDD"), Direction = 270, ShadowDepth = 2, BlurRadius = 10, Opacity = 0.3 } };
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
            var bloques = esMatutino ? BLOQUES_MATUTINO : BLOQUES_VESPERTINO;
            var grid = new Grid { Margin = new Thickness(0, 0, 0, 15) };
            for (int c = 0; c < 6; c++) grid.ColumnDefinitions.Add(new ColumnDefinition { Width = c == 0 ? new GridLength(110) : new GridLength(1, GridUnitType.Star) });
            for (int r = 0; r <= bloques.Count; r++) grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            string[] encabezados = { "Hora", "Lunes", "Martes", "Miercoles", "Jueves", "Viernes" };
            for (int c = 0; c < encabezados.Length; c++)
            {
                var headerCell = new Border { Background = (Brush)FindResource("GuindaBajo"), BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5A002C")), BorderThickness = new Thickness(0, 0, 0.5, 0.5), Padding = new Thickness(6, 8, 6, 8) };
                headerCell.Child = new TextBlock { Text = encabezados[c], FontSize = 12, FontWeight = FontWeights.Bold, Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center };
                Grid.SetRow(headerCell, 0); Grid.SetColumn(headerCell, c); grid.Children.Add(headerCell);
            }

            for (int r = 0; r < bloques.Count; r++)
            {
                var bloque = bloques[r];
                var horaCell = new Border { Background = bloque.EsReceso ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFECB3")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3E0")), BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DDDDDD")), BorderThickness = new Thickness(0, 0, 0.5, 0.5), Padding = new Thickness(4, 6, 4, 6) };
                horaCell.Child = new TextBlock { Text = bloque.Horario, FontSize = bloque.EsReceso ? 10 : 11, FontWeight = bloque.EsReceso ? FontWeights.Bold : FontWeights.SemiBold, Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bloque.EsReceso ? "#E65100" : "#333333")), HorizontalAlignment = HorizontalAlignment.Center };
                Grid.SetRow(horaCell, r + 1); Grid.SetColumn(horaCell, 0); grid.Children.Add(horaCell);

                for (int c = 1; c <= 5; c++)
                {
                    var cell = new Border { Background = bloque.EsReceso ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFECB3")) : Brushes.White, BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DDDDDD")), BorderThickness = new Thickness(0, 0, 0.5, 0.5), Padding = new Thickness(4, 6, 4, 6), MinHeight = bloque.EsReceso ? 30 : 55 }; // MinHeight aumentado un poco para que quepan los salones

                    if (bloque.EsReceso)
                    {
                        cell.Child = new TextBlock { Text = c == 3 ? "R E C E S O" : "", FontSize = 11, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E65100")), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                    }
                    else
                    {
                        var clase = horarioAsignado.FirstOrDefault(h => h.DiaSemana == c && h.BloqueHora == bloque.IdBloque);
                        bool sinMaestro = clase == null || string.IsNullOrEmpty(clase.NombreDocente) || clase.IdDocente == -1 || clase.NombreDocente == "0";

                        if (clase != null)
                        {
                            // SE ACTUALIZÓ PARA MOSTRAR LA MATERIA, EL MAESTRO Y EL SALÓN APILADOS
                            var celdaContenido = new StackPanel { VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };

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
                                Foreground = string.IsNullOrEmpty(clase.NombreSalon) ? Brushes.Red : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E7D32")),
                                TextAlignment = TextAlignment.Center,
                                Margin = new Thickness(0, 3, 0, 0)
                            });

                            cell.Child = celdaContenido;
                            cell.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8EAF6"));
                        }
                    }
                    Grid.SetRow(cell, r + 1); Grid.SetColumn(cell, c); grid.Children.Add(cell);
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
            for (int i = 0; i <= materias.Count; i++) grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            string[] headers = { "Asignatura", "Clave", "Creditos", "Docente" };
            for (int c = 0; c < headers.Length; c++)
            {
                var cell = new Border { Background = (Brush)FindResource("GuindaBajo"), BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5A002C")), BorderThickness = new Thickness(0, 0, 0.5, 0.5), Padding = new Thickness(8, 6, 8, 6) };
                cell.Child = new TextBlock { Text = headers[c], FontSize = 12, FontWeight = FontWeights.Bold, Foreground = Brushes.White, HorizontalAlignment = c == 2 ? HorizontalAlignment.Center : HorizontalAlignment.Left };
                Grid.SetRow(cell, 0); Grid.SetColumn(cell, c); grid.Children.Add(cell);
            }

            for (int i = 0; i < materias.Count; i++)
            {
                var m = materias[i];
                var bg = new SolidColorBrush((Color)ColorConverter.ConvertFromString(i % 2 == 0 ? "#FFFFFF" : "#F9F9F9"));
                var clase = horarioAsignado.FirstOrDefault(h => h.NombreMateria == m.Nombre);

                bool sinMaestro = clase == null || string.IsNullOrEmpty(clase.NombreDocente) || clase.IdDocente == -1;
                string nombreDocente = sinMaestro ? "(Por asignar)" : clase.NombreDocente;

                AgregarCeldaTexto(grid, i + 1, 0, m.Nombre, bg);
                var cellClave = AgregarCeldaTexto(grid, i + 1, 1, m.Clave, bg, foreground: "#1976D2", esEnlace: true);
                cellClave.Cursor = Cursors.Hand;
                cellClave.ToolTip = "Ver lista de docentes y sus horas para esta materia";

                int idMateriaSeleccionada = m.IdMateria;
                string nombreMateriaSeleccionada = m.Nombre;
                cellClave.MouseLeftButtonDown += (s, e) => {
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
            var cell = new Border { Background = bg, BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DDDDDD")), BorderThickness = new Thickness(0, 0, 0.5, 0.5), Padding = new Thickness(8, 5, 8, 5) };
            var txt = new TextBlock { Text = texto, FontSize = 11, Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(foreground)), HorizontalAlignment = hAlign, VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap, FontStyle = esItalica ? FontStyles.Italic : FontStyles.Normal };
            if (esEnlace) { txt.TextDecorations = TextDecorations.Underline; txt.FontWeight = FontWeights.SemiBold; }
            cell.Child = txt; Grid.SetRow(cell, row); Grid.SetColumn(cell, col); grid.Children.Add(cell);
            return cell;
        }

        // =========================================================================================
        // LÓGICA DEL MODAL CON DISPONIBILIDAD UNIFICADA, PALETA POR GRUPO Y HFG
        // =========================================================================================
        private void AbrirModalDocente(int idDocente, string nombreDocente)
        {
            TxtModalNombreDocente.Text = nombreDocente;
            GridHorarioDocente.Children.Clear();
            GridHorarioDocente.RowDefinitions.Clear();
            GridHorarioDocente.ColumnDefinitions.Clear();

            var horarioPersonal = new List<HorarioPersonal>();
            var disponibilidad = new HashSet<string>();
            int hfgAsignadas = 0;

            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            conn.Open();

            using (var cmdHfg = new SqlCommand("SELECT horas_frente_grupo FROM Docentes WHERE id_docente = @id", conn))
            {
                cmdHfg.Parameters.AddWithValue("@id", idDocente);
                var result = cmdHfg.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    hfgAsignadas = Convert.ToInt32(result);
                }
            }

            using (var cmd = new SqlCommand("sp_GetHorarioPersonalDocente", conn) { CommandType = CommandType.StoredProcedure })
            {
                cmd.Parameters.AddWithValue("@id_proyecto", _proyecto.IdProyecto);
                cmd.Parameters.AddWithValue("@id_docente", idDocente);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    horarioPersonal.Add(new HorarioPersonal
                    {
                        Dia = Convert.ToInt32(reader["id_dia"]),
                        Bloque = Convert.ToInt32(reader["id_bloque"]),
                        Materia = reader["NombreMateria"].ToString() ?? "",
                        Grupo = reader["NombreGrupo"].ToString() ?? ""
                    });
                }
            }

            using (var cmdDisp = new SqlCommand("SELECT id_dia, id_bloque FROM DisponibilidadDocente WHERE id_docente = @id", conn))
            {
                cmdDisp.Parameters.AddWithValue("@id", idDocente);
                using var readerDisp = cmdDisp.ExecuteReader();
                while (readerDisp.Read())
                {
                    disponibilidad.Add($"{readerDisp["id_dia"]}_{readerDisp["id_bloque"]}");
                }
            }

            TxtModalTotalHoras.Text = $"HFG Asignadas: {hfgAsignadas}   |   Horas Repartidas: {horarioPersonal.Count}";

            var materiasUnicas = horarioPersonal.Select(h => h.Materia).Distinct().ToList();
            TxtMateriasLista.Text = materiasUnicas.Count > 0 ? string.Join("  •  ", materiasUnicas) : "Sin materias asignadas";

            for (int c = 0; c < 6; c++)
                GridHorarioDocente.ColumnDefinitions.Add(new ColumnDefinition { Width = c == 0 ? new GridLength(110) : new GridLength(1, GridUnitType.Star) });

            GridHorarioDocente.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            string[] encabezados = { "Hora", "Lunes", "Martes", "Miercoles", "Jueves", "Viernes" };
            for (int c = 0; c < encabezados.Length; c++)
            {
                var hCell = new Border { Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333")), BorderBrush = Brushes.LightGray, BorderThickness = new Thickness(0, 0, 1, 1), Padding = new Thickness(6, 12, 6, 12) };
                hCell.Child = new TextBlock { Text = encabezados[c], FontSize = 14, FontWeight = FontWeights.Bold, Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center };
                Grid.SetRow(hCell, 0); Grid.SetColumn(hCell, c); GridHorarioDocente.Children.Add(hCell);
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

            for (int r = 0; r < BLOQUES_UNIFICADOS.Count; r++)
            {
                var bloque = BLOQUES_UNIFICADOS[r];

                GridHorarioDocente.RowDefinitions.Add(new RowDefinition { Height = bloque.EsReceso ? new GridLength(35) : new GridLength(85) });

                var horaCell = new Border { Background = Brushes.White, BorderBrush = Brushes.LightGray, BorderThickness = new Thickness(0, 0, 1, 1), Padding = new Thickness(4, 6, 4, 6) };
                horaCell.Child = new TextBlock { Text = bloque.Horario, FontSize = bloque.EsReceso ? 11 : 12, FontWeight = FontWeights.Bold, Foreground = Brushes.Black, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                Grid.SetRow(horaCell, r + 1); Grid.SetColumn(horaCell, 0); GridHorarioDocente.Children.Add(horaCell);

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
                    Grid.SetRow(bgCell, r + 1); Grid.SetColumn(bgCell, c); GridHorarioDocente.Children.Add(bgCell);

                    if (bloque.EsReceso)
                    {
                        var txtReceso = new TextBlock { Text = c == 3 ? "R E C E S O" : "", FontSize = 12, FontWeight = FontWeights.Bold, Foreground = Brushes.Gray, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                        Grid.SetRow(txtReceso, r + 1); Grid.SetColumn(txtReceso, c); GridHorarioDocente.Children.Add(txtReceso);
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
                            Grid.SetRow(tarjetaMateria, r + 1); Grid.SetColumn(tarjetaMateria, c); GridHorarioDocente.Children.Add(tarjetaMateria);
                        }
                    }
                }
            }
            ModalDocente.Visibility = Visibility.Visible;
        }

        private void CerrarModal_Click(object sender, RoutedEventArgs e) { ModalDocente.Visibility = Visibility.Collapsed; }

        private void Volver_Click(object sender, RoutedEventArgs e) { NavigationService.GetFromWindow(this)?.NavigateTo(new ProyectoDetalleView(_proyecto)); }

        // =========================================================================================
        // LÓGICA DEL MODAL DE DOCENTES POR MATERIA (CLAVES)
        // =========================================================================================
        private void AbrirModalDocentesMateria(int idMateria, string nombreMateria)
        {
            TxtModalNombreMateria.Text = nombreMateria;
            PanelListaDocentesMateria.Children.Clear();

            try
            {
                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                using var cmd = new SqlCommand("sp_GetDocentesAsignablesAMateria", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@id_materia", idMateria);
                cmd.Parameters.AddWithValue("@id_proyecto", _proyecto.IdProyecto);

                conn.Open();
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int idDoc = Convert.ToInt32(reader["id_docente"]);
                    string nombreDoc = reader["nombre"].ToString() ?? "";
                    int hfg = Convert.ToInt32(reader["hfg"]);
                    int repartidas = Convert.ToInt32(reader["HorasRepartidas"]);
                    string carreraPrin = reader["CarreraPrincipal"].ToString() ?? "";

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

                    stackHoras.Children.Add(new TextBlock { Text = $"HFG Asignadas: {hfg}h", FontSize = 12, FontWeight = FontWeights.SemiBold, Foreground = Brushes.Gray, Margin = new Thickness(0, 0, 15, 0) });
                    stackHoras.Children.Add(new TextBlock { Text = $"Repartidas: {repartidas}h", FontSize = 12, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorRepartidas)) });

                    Grid.SetColumn(stackHoras, 1);
                    gridInfo.Children.Add(stackHoras);

                    tarjetaDocente.Child = gridInfo;
                    PanelListaDocentesMateria.Children.Add(tarjetaDocente);
                }

                if (PanelListaDocentesMateria.Children.Count == 0)
                {
                    PanelListaDocentesMateria.Children.Add(new TextBlock { Text = "Ningun docente tiene esta materia asignada en su perfil.", FontSize = 14, Foreground = Brushes.Gray, FontStyle = FontStyles.Italic, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 20, 0, 0) });
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

        public class HorarioAsignado
        {
            public int DiaSemana { get; set; }
            public int BloqueHora { get; set; }
            public string NombreMateria { get; set; } = "";
            public string NombreDocente { get; set; } = "";
            public int IdDocente { get; set; }
            public string NombreSalon { get; set; } = "";
            public string NombreEdificio { get; set; } = "";
        }

        public class HorarioPersonal
        {
            public int Dia { get; set; }
            public int Bloque { get; set; }
            public string Materia { get; set; } = "";
            public string Grupo { get; set; } = "";
        }
    }
}