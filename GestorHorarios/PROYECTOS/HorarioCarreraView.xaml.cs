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
            _proyecto = proyecto; _idCarrera = idCarrera;
            CargarNombreCarrera(); CargarEncabezado(); CargarGruposConTablas();
        }

        private void CargarNombreCarrera()
        {
            try
            {
                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                using var cmd = new SqlCommand("sp_ObtenerNombreCarrera", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@id_carrera", _idCarrera);
                conn.Open(); _nombreCarrera = cmd.ExecuteScalar()?.ToString() ?? "Carrera";
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
            using var cmd = new SqlCommand(@"SELECT id_materia, nombre, clave, creditos, semestre FROM Materias WHERE id_carrera = @id AND semestre = @sem ORDER BY nombre", conn);
            cmd.Parameters.AddWithValue("@id", _idCarrera); cmd.Parameters.AddWithValue("@sem", semestre);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                materias.Add(new Materia { IdMateria = Convert.ToInt32(reader["id_materia"]), Nombre = reader["nombre"].ToString() ?? "", Clave = reader["clave"].ToString() ?? "", Creditos = Convert.ToInt32(reader["creditos"]) });
            }
            return materias;
        }

        private List<HorarioAsignado> ObtenerHorarioDelGrupo(int idGrupo)
        {
            var horarios = new List<HorarioAsignado>();
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand(@"SELECT h.id_dia, h.id_bloque, m.nombre AS Materia, d.nombre AS Docente, h.id_docente FROM HorarioDetalle h INNER JOIN Materias m ON h.id_materia = m.id_materia LEFT JOIN Docentes d ON h.id_docente = d.id_docente WHERE h.id_grupo = @idGrupo AND h.id_proyecto = @idProyecto", conn);
            cmd.Parameters.AddWithValue("@idGrupo", idGrupo); cmd.Parameters.AddWithValue("@idProyecto", _proyecto.IdProyecto);
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
                    IdDocente = reader["id_docente"] != DBNull.Value ? Convert.ToInt32(reader["id_docente"]) : 0
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
                headerBorder.Child = new TextBlock { Text = $"📖 Semestre {semGrupo.Key}", FontSize = 16, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1565C0")) };
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

            // Calculamos la suma total de horas (créditos) del grupo
            int totalHoras = materias.Sum(m => m.Creditos);

            // Agregamos las horas a la semana al texto del encabezado
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

            string[] encabezados = { "Hora", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes" };
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
                    var cell = new Border { Background = bloque.EsReceso ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFECB3")) : Brushes.White, BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DDDDDD")), BorderThickness = new Thickness(0, 0, 0.5, 0.5), Padding = new Thickness(4, 6, 4, 6), MinHeight = bloque.EsReceso ? 30 : 45 };
                    if (bloque.EsReceso) { cell.Child = new TextBlock { Text = c == 3 ? "R E C E S O" : "", FontSize = 11, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E65100")), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center }; }
                    else
                    {
                        var clase = horarioAsignado.FirstOrDefault(h => h.DiaSemana == c && h.BloqueHora == bloque.IdBloque);
                        bool sinMaestro = clase == null || string.IsNullOrEmpty(clase.NombreDocente) || clase.IdDocente == -1 || clase.NombreDocente == "0";
                        string textoCelda = clase != null ? (sinMaestro ? $"{clase.NombreMateria}" : $"{clase.NombreMateria}\n({clase.NombreDocente})") : "";
                        cell.Child = new TextBlock { Text = textoCelda, FontSize = 10, TextWrapping = TextWrapping.Wrap, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, TextAlignment = TextAlignment.Center, FontWeight = clase != null ? FontWeights.SemiBold : FontWeights.Normal };
                        if (clase != null) cell.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8EAF6"));
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

            string[] headers = { "Asignatura", "Clave", "Créditos", "Docente" };
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
                AgregarCeldaTexto(grid, i + 1, 1, m.Clave, bg);
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
        // LÓGICA DEL MODAL DEL DOCENTE MEJORADA CON COLORES DE DISPONIBILIDAD
        // =========================================================================================
        private void AbrirModalDocente(int idDocente, string nombreDocente)
        {
            TxtModalNombreDocente.Text = nombreDocente;
            GridHorarioDocente.Children.Clear();
            GridHorarioDocente.RowDefinitions.Clear();
            GridHorarioDocente.ColumnDefinitions.Clear();

            var horarioPersonal = new List<HorarioPersonal>();
            var disponibilidad = new HashSet<string>();

            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            conn.Open();

            // 1. Extraer las clases asignadas en el horario actual
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

            // 2. NUEVO: Extraer la matriz de disponibilidad del profesor de la base de datos
            using (var cmdDisp = new SqlCommand("SELECT id_dia, id_bloque FROM DisponibilidadDocente WHERE id_docente = @id", conn))
            {
                cmdDisp.Parameters.AddWithValue("@id", idDocente);
                using var readerDisp = cmdDisp.ExecuteReader();
                while (readerDisp.Read())
                {
                    disponibilidad.Add($"{readerDisp["id_dia"]}_{readerDisp["id_bloque"]}");
                }
            }

            TxtModalTotalHoras.Text = $"Total de Horas Asignadas: {horarioPersonal.Count}";

            // DIBUJAR CUADRÍCULA PERSONAL MÁS AMPLIA
            for (int c = 0; c < 6; c++) GridHorarioDocente.ColumnDefinitions.Add(new ColumnDefinition { Width = c == 0 ? new GridLength(110) : new GridLength(1, GridUnitType.Star) });
            for (int r = 0; r <= BLOQUES_UNIFICADOS.Count; r++) GridHorarioDocente.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            string[] encabezados = { "Hora", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes" };
            for (int c = 0; c < encabezados.Length; c++)
            {
                var hCell = new Border { Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333")), BorderBrush = Brushes.LightGray, BorderThickness = new Thickness(0, 0, 0.5, 0.5), Padding = new Thickness(6, 12, 6, 12) };
                hCell.Child = new TextBlock { Text = encabezados[c], FontSize = 14, FontWeight = FontWeights.Bold, Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center };
                Grid.SetRow(hCell, 0); Grid.SetColumn(hCell, c); GridHorarioDocente.Children.Add(hCell);
            }

            for (int r = 0; r < BLOQUES_UNIFICADOS.Count; r++)
            {
                var bloque = BLOQUES_UNIFICADOS[r];
                var horaCell = new Border { Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F5F5")), BorderBrush = Brushes.LightGray, BorderThickness = new Thickness(0, 0, 0.5, 0.5), Padding = new Thickness(4, 6, 4, 6) };
                horaCell.Child = new TextBlock { Text = bloque.Horario, FontSize = bloque.EsReceso ? 11 : 12, FontWeight = FontWeights.Bold, Foreground = Brushes.Black, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                Grid.SetRow(horaCell, r + 1); Grid.SetColumn(horaCell, 0); GridHorarioDocente.Children.Add(horaCell);

                for (int c = 1; c <= 5; c++)
                {
                    // Asignación de la paleta de colores de la primera imagen
                    string colorSombreadoDia = "#FFFFFF";
                    switch (c)
                    {
                        case 1: colorSombreadoDia = "#FDE3B2"; break; // Naranja Claro (Lunes)
                        case 2: colorSombreadoDia = "#C8E6C9"; break; // Verde Claro (Martes)
                        case 3: colorSombreadoDia = "#BBDEFB"; break; // Azul Claro (Miércoles)
                        case 4: colorSombreadoDia = "#F8BBD0"; break; // Rosa Claro (Jueves)
                        case 5: colorSombreadoDia = "#FDE3B2"; break; // Naranja Claro (Viernes)
                    }

                    // Verificamos si este bloque pertenece a las horas en que el docente configuró estar disponible
                    bool estaDisponible = disponibilidad.Count == 0 || disponibilidad.Contains($"{c}_{bloque.IdBloque}");

                    // Si está disponible pintamos pastel, si no, gris muy claro de bloque inactivo
                    string bgColor = estaDisponible ? colorSombreadoDia : "#F0F0F0";

                    var cell = new Border
                    {
                        Background = bloque.EsReceso ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString(bgColor)),
                        BorderBrush = Brushes.LightGray,
                        BorderThickness = new Thickness(0, 0, 0.5, 0.5),
                        Padding = new Thickness(8),
                        MinHeight = bloque.EsReceso ? 30 : 65 // Aumenté el alto mínimo a 65px para que se vea amplio
                    };

                    if (bloque.EsReceso)
                    {
                        cell.Child = new TextBlock { Text = c == 3 ? "R E C E S O" : "", FontSize = 12, FontWeight = FontWeights.Bold, Foreground = Brushes.Gray, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                    }
                    else
                    {
                        var clase = horarioPersonal.FirstOrDefault(h => h.Dia == c && h.Bloque == bloque.IdBloque);
                        if (clase != null)
                        {
                            // Si tiene una clase en esa hora, la marcamos con texto fuerte y nítido encima del color pastel
                            cell.Child = new TextBlock
                            {
                                Text = $"{clase.Materia}\n{clase.Grupo}",
                                FontSize = 12,
                                TextWrapping = TextWrapping.Wrap,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center,
                                TextAlignment = TextAlignment.Center,
                                FontWeight = FontWeights.Bold,
                                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222222")) // Gris muy oscuro casi negro
                            };
                        }
                    }
                    Grid.SetRow(cell, r + 1); Grid.SetColumn(cell, c); GridHorarioDocente.Children.Add(cell);
                }
            }
            ModalDocente.Visibility = Visibility.Visible;
        }

        private void CerrarModal_Click(object sender, RoutedEventArgs e) { ModalDocente.Visibility = Visibility.Collapsed; }

        private void Volver_Click(object sender, RoutedEventArgs e) { NavigationService.GetFromWindow(this)?.NavigateTo(new ProyectoDetalleView(_proyecto)); }

        public class HorarioAsignado { public int DiaSemana { get; set; } public int BloqueHora { get; set; } public string NombreMateria { get; set; } = ""; public string NombreDocente { get; set; } = ""; public int IdDocente { get; set; } }
        public class HorarioPersonal { public int Dia { get; set; } public int Bloque { get; set; } public string Materia { get; set; } = ""; public string Grupo { get; set; } = ""; }
    }
}