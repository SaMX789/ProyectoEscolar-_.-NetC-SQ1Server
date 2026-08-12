using GestorHorarios.Models;
using GestorHorarios.Services;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
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

        // VARIABLES GLOBALES PARA LA NAVEGACIÓN DEL MODAL
        private List<FrameworkElement> _encabezadosAreas = new List<FrameworkElement>();
        private int _indiceAreaActual = 0;

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
            TxtFechaProyecto.Text = $"Fecha de creacion: {fechaFormateada}";

            if (_proyecto.Ciclo == "A")
            {
                TxtCicloInfo.Text = "Ciclo A - Semestres: 2, 4, 6, 8";
                TxtCicloInfo.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1565C0"));
                BadgeCiclo.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD"));
            }
            else
            {
                TxtCicloInfo.Text = "Ciclo B - Semestres: 1, 3, 5, 7, 9";
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

                int totalFilas = (int)Math.Ceiling(_listaCarreras.Count / 3.0);
                for (int r = 0; r < totalFilas; r++)
                {
                    GridCarreras.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                }

                for (int i = 0; i < _listaCarreras.Count; i++)
                {
                    var carrera = _listaCarreras[i];
                    int row = i / 3;
                    int col = i % 3;

                    bool estaGenerado = TieneHorarioGenerado(_proyecto.IdProyecto, carrera.IdCarrera);

                    // --- MAGIA DE ICONOS PROFESIONALES (Segoe MDL2 Assets) ---
                    string iconoGlyph = "\uE82D"; // Default: Un cuadrito generico
                    string nombreUpper = carrera.Nombre.ToUpper();

                    if (nombreUpper.Contains("SISTEMAS")) iconoGlyph = "\uE99A"; // Monitor de PC
                    else if (nombreUpper.Contains("CIVIL")) iconoGlyph = "\uE913"; // Edificios / Construccion
                    else if (nombreUpper.Contains("COMUNITARIO")) iconoGlyph = "\uE716"; // Grupo de personas
                    else if (nombreUpper.Contains("EMPRESARIAL")) iconoGlyph = "\uE9D9"; // Maletin de negocios
                    else if (nombreUpper.Contains("INDUSTRIAL")) iconoGlyph = "\uE713"; // Engrane
                    else if (nombreUpper.Contains("BIOQUIMICA")) iconoGlyph = "\uE913"; // Matraz de laboratorio (Ciencia)
                    else if (nombreUpper.Contains("INGLES") || nombreUpper.Contains("IDIOMA")) iconoGlyph = "\uE90A"; // Globo terraqueo

                    var border = new Border
                    {
                        Style = (Style)FindResource("CarreraCardStyle"),
                        Tag = carrera.IdCarrera
                    };
                    border.MouseLeftButtonDown += CarreraCard_Click;

                    var mainGrid = new Grid();
                    var sp = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };

                    // AQUI APLICAMOS LA FUENTE DE ICONOS DE WINDOWS
                    sp.Children.Add(new TextBlock
                    {
                        Text = iconoGlyph,
                        FontFamily = new FontFamily("Segoe MDL2 Assets"), // Fuente nativa de iconos
                        FontSize = 42,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#757575")), // Gris elegante
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 0, 0, 12)
                    });

                    sp.Children.Add(new TextBlock { Text = carrera.Nombre, FontSize = 16, FontWeight = FontWeights.Bold, Foreground = (Brush)FindResource("GuindaBajo"), HorizontalAlignment = HorizontalAlignment.Center, TextAlignment = TextAlignment.Center, TextWrapping = TextWrapping.Wrap });

                    int totalGrupos = ContarGruposCiclo(carrera.IdCarrera);
                    sp.Children.Add(new TextBlock { Text = $"{totalGrupos} {(totalGrupos == 1 ? "grupo" : "grupos")}", FontSize = 12, Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#888888")), HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 4, 0, 0) });

                    var statusIcon = new TextBlock
                    {
                        Text = estaGenerado ? "[ OK ]" : "[ Pendiente ]",
                        FontSize = 12,
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(estaGenerado ? "#2E7D32" : "#D32F2F")),
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(0, -5, -5, 0)
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

        // ==========================================
        // LÓGICA DEL ANÁLISIS PREVIO (DIAGNÓSTICO)
        // ==========================================

        private class DiagnosticoItem
        {
            public string Clave { get; set; } = "";
            public string Nombre { get; set; } = "";
            public int Grupos { get; set; }
            public int Requeridas { get; set; }
            public int Docentes { get; set; }
            public int Capacidad { get; set; }
            public string Semaforo { get; set; } = "";
            public int Creditos { get; set; }
            public string Area { get; set; } = "";
        }

        private void BtnAnalisisPrevio_Click(object sender, RoutedEventArgs e)
        {
            PanelResultadosDiagnostico.Children.Clear();
            _encabezadosAreas.Clear();
            _indiceAreaActual = 0;

            try
            {
                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                using var cmd = new SqlCommand("sp_DiagnosticoViabilidad", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@Ciclo", _proyecto.Ciclo);

                conn.Open();
                using var reader = cmd.ExecuteReader();

                var listaResultados = new List<DiagnosticoItem>();

                while (reader.Read())
                {
                    listaResultados.Add(new DiagnosticoItem
                    {
                        Clave = reader["ClaveMateria"].ToString()!,
                        Nombre = reader["NombreMateria"].ToString()!,
                        Grupos = Convert.ToInt32(reader["Grupos"]),
                        Requeridas = Convert.ToInt32(reader["HorasRequeridas"]),
                        Docentes = Convert.ToInt32(reader["DocentesHabilitados"]),
                        Capacidad = Convert.ToInt32(reader["CapacidadHoras"]),
                        Semaforo = reader["Semaforo"].ToString()!,
                        Creditos = Convert.ToInt32(reader["Creditos"]),
                        Area = reader["AreaAcademica"].ToString()!
                    });
                }

                if (listaResultados.Count == 0)
                {
                    PanelResultadosDiagnostico.Children.Add(new TextBlock
                    {
                        Text = "No hay datos suficientes para analizar. Asegurate de tener grupos creados para este ciclo.",
                        FontStyle = FontStyles.Italic,
                        Foreground = Brushes.Gray,
                        Margin = new Thickness(10)
                    });
                    ModalDiagnostico.Visibility = Visibility.Visible;
                    return;
                }

                var gruposPorArea = listaResultados
                    .GroupBy(x => x.Area)
                    .OrderBy(g => g.Key == "Materias Compartidas (Tronco Común)" ? 0 : 1)
                    .ThenBy(g => g.Key);

                foreach (var grupoArea in gruposPorArea)
                {
                    // 1. ENCABEZADO
                    var headerArea = new Border
                    {
                        BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0")),
                        BorderThickness = new Thickness(0, 0, 0, 2),
                        Margin = new Thickness(0, 20, 0, 15),
                        Padding = new Thickness(0, 0, 0, 5),
                        Child = new TextBlock
                        {
                            Text = grupoArea.Key.ToUpper(),
                            FontSize = 18,
                            FontWeight = FontWeights.ExtraBold,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1565C0"))
                        }
                    };

                    PanelResultadosDiagnostico.Children.Add(headerArea);
                    _encabezadosAreas.Add(headerArea); // Lo guardamos para la navegación

                    // 2. CUADRICULA
                    var gridTarjetas = new System.Windows.Controls.Primitives.UniformGrid
                    {
                        Columns = 2,
                        VerticalAlignment = VerticalAlignment.Top
                    };

                    foreach (var item in grupoArea)
                    {
                        var card = CrearTarjetaDiagnostico(item.Clave, item.Nombre, item.Grupos, item.Requeridas, item.Docentes, item.Capacidad, item.Semaforo, item.Creditos, item.Area);
                        gridTarjetas.Children.Add(card);
                    }

                    PanelResultadosDiagnostico.Children.Add(gridTarjetas);
                }

                ModalDiagnostico.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar el diagnostico: {ex.Message}", "Error de BD", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExportarTodasCarreras_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Mostrar estado en la UI
                TxtProgreso.Text = "Exportando horarios de todas las carreras...";
                TxtProgreso.Visibility = Visibility.Visible;
                BarraProgreso.Visibility = Visibility.Visible;
                BarraProgreso.IsIndeterminate = true;

                string rutaCarpeta = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string tituloCiclo = $"Ciclo {_proyecto.Ciclo} · {_proyecto.Anio} · {_proyecto.Periodo}";
                var generador = new GestorHorarios.Services.GeneradorWordService();
                int exportadas = 0;

                using var conn = new Microsoft.Data.SqlClient.SqlConnection(GestorHorarios.Services.DatabaseService.GetConnectionString());
                conn.Open();

                // 2. USO DEL NUEVO SP: Obtener TODAS las carreras del sistema
                var carreras = new List<(int Id, string Nombre)>();
                using (var cmdCarreras = new Microsoft.Data.SqlClient.SqlCommand("sp_ObtenerTodasLasCarreras", conn))
                {
                    cmdCarreras.CommandType = System.Data.CommandType.StoredProcedure;
                    using var readerCarreras = cmdCarreras.ExecuteReader();
                    while (readerCarreras.Read())
                    {
                        carreras.Add((Convert.ToInt32(readerCarreras["id_carrera"]), readerCarreras["nombre"].ToString() ?? "Carrera"));
                    }
                }

                // 3. Iterar carrera por carrera para armar sus grupos
                foreach (var carrera in carreras)
                {
                    var gruposData = new List<GestorHorarios.Services.GeneradorWordService.GrupoData>();

                    // USO DE SP EXISTENTE: Obtener los grupos de ESTA carrera filtrados por ciclo A/B
                    var gruposLista = new List<(int IdGrupo, string Nombre, int Semestre, string Turno)>();
                    using (var cmdGrupos = new Microsoft.Data.SqlClient.SqlCommand("sp_ObtenerGruposPorCicloYCarrera", conn))
                    {
                        cmdGrupos.CommandType = System.Data.CommandType.StoredProcedure;
                        cmdGrupos.Parameters.AddWithValue("@ciclo", _proyecto.Ciclo);
                        cmdGrupos.Parameters.AddWithValue("@id_carrera", carrera.Id);

                        using var readerGrupos = cmdGrupos.ExecuteReader();
                        while (readerGrupos.Read())
                        {
                            gruposLista.Add((
                                Convert.ToInt32(readerGrupos["id_grupo"]),
                                readerGrupos["NombreGrupo"].ToString() ?? "",
                                Convert.ToInt32(readerGrupos["semestre"]),
                                readerGrupos["turno"].ToString() ?? "Matutino"
                            ));
                        }
                    }

                    // 4. Llenar los datos de cada grupo
                    foreach (var grupo in gruposLista)
                    {
                        int totalCreditos = 0;

                        // Sumar horas/créditos de la materia (La tabla Materias sí tiene id_estado)
                        using (var cmdMat = new Microsoft.Data.SqlClient.SqlCommand("SELECT SUM(creditos) FROM Materias WHERE id_carrera = @idC AND semestre = @sem AND id_estado = 1", conn))
                        {
                            cmdMat.Parameters.AddWithValue("@idC", carrera.Id);
                            cmdMat.Parameters.AddWithValue("@sem", grupo.Semestre);
                            var res = cmdMat.ExecuteScalar();
                            if (res != DBNull.Value && res != null) totalCreditos = Convert.ToInt32(res);
                        }

                        // USO DE SP EXISTENTE: Obtener el horario asignado al grupo
                        var horariosAsignados = new List<GestorHorarios.PROYECTOS.HorarioCarreraView.HorarioAsignado>();
                        using (var cmdHor = new Microsoft.Data.SqlClient.SqlCommand("sp_GetHorarioDelGrupo", conn))
                        {
                            cmdHor.CommandType = System.Data.CommandType.StoredProcedure;
                            cmdHor.Parameters.AddWithValue("@idGrupo", grupo.IdGrupo);
                            cmdHor.Parameters.AddWithValue("@idProyecto", _proyecto.IdProyecto);

                            using var readerHor = cmdHor.ExecuteReader();
                            while (readerHor.Read())
                            {
                                horariosAsignados.Add(new GestorHorarios.PROYECTOS.HorarioCarreraView.HorarioAsignado
                                {
                                    DiaSemana = Convert.ToInt32(readerHor["id_dia"]),
                                    BloqueHora = Convert.ToInt32(readerHor["id_bloque"]),
                                    NombreMateria = readerHor["Materia"].ToString() ?? "",
                                    NombreDocente = readerHor["Docente"]?.ToString() ?? "",
                                    NombreSalon = readerHor["Salon"]?.ToString() ?? "",
                                    NombreEdificio = readerHor["Edificio"]?.ToString() ?? ""
                                });
                            }
                        }

                        gruposData.Add(new GestorHorarios.Services.GeneradorWordService.GrupoData
                        {
                            NombreGrupo = grupo.Nombre,
                            Turno = grupo.Turno,
                            TotalHoras = totalCreditos,
                            Horarios = horariosAsignados
                        });
                    }

                    // 5. Si la carrera tuvo grupos activos en este ciclo, generamos su archivo Word
                    if (gruposData.Count > 0)
                    {
                        // Limpiamos el nombre de la carrera por si tiene espacios raros para el nombre del archivo
                        string nombreArchivoLimpio = string.Join("_", carrera.Nombre.Split(System.IO.Path.GetInvalidFileNameChars()));
                        string rutaArchivo = System.IO.Path.Combine(rutaCarpeta, $"Horarios_{nombreArchivoLimpio}.docx");

                        generador.ExportarHorarioGrupos(rutaArchivo, carrera.Nombre, tituloCiclo, gruposData);
                        exportadas++;
                    }
                }

                // Apagar UI de carga
                BarraProgreso.Visibility = Visibility.Collapsed;
                TxtProgreso.Visibility = Visibility.Collapsed;

                MessageBox.Show($"¡Se generaron exitosamente {exportadas} documentos de Word (uno por cada carrera)!\n\nRevisa tu Escritorio.", "Exportación Masiva Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                BarraProgreso.Visibility = Visibility.Collapsed;
                TxtProgreso.Visibility = Visibility.Collapsed;
                MessageBox.Show($"Ocurrió un error en la exportación masiva:\n{ex.Message}", "Error de Exportación", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExportarDocentes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TxtProgreso.Text = "Exportando horarios de docentes...";
                TxtProgreso.Visibility = Visibility.Visible;
                BarraProgreso.Visibility = Visibility.Visible;
                BarraProgreso.IsIndeterminate = true;

                var docentesData = new List<GestorHorarios.Services.GeneradorWordService.DocenteDataWord>();
                string tituloCiclo = $"Ciclo {_proyecto.Ciclo} · {_proyecto.Anio} · {_proyecto.Periodo}";

                using var conn = new Microsoft.Data.SqlClient.SqlConnection(GestorHorarios.Services.DatabaseService.GetConnectionString());
                conn.Open();

                // 1. Obtener la lista de docentes que TIENEN clases en este proyecto
                var docentesLista = new List<(int Id, string Nombre, int Hfg)>();
                using (var cmdDocentes = new Microsoft.Data.SqlClient.SqlCommand("sp_GetDocentesConHorarioPorProyecto", conn))
                {
                    cmdDocentes.CommandType = System.Data.CommandType.StoredProcedure;
                    cmdDocentes.Parameters.AddWithValue("@id_proyecto", _proyecto.IdProyecto);

                    using var reader = cmdDocentes.ExecuteReader();
                    while (reader.Read())
                    {
                        docentesLista.Add((
                            Convert.ToInt32(reader["id_docente"]),
                            reader["nombre"].ToString() ?? "",
                            Convert.ToInt32(reader["horas_frente_grupo"])
                        ));
                    }
                }

                // 2. Por cada docente, jalar su horario
                foreach (var docente in docentesLista)
                {
                    var horarioPersonal = new List<GestorHorarios.PROYECTOS.HorarioCarreraView.HorarioPersonal>();

                    using (var cmdHorario = new Microsoft.Data.SqlClient.SqlCommand("sp_GetHorarioPersonalDocente", conn))
                    {
                        cmdHorario.CommandType = System.Data.CommandType.StoredProcedure;
                        cmdHorario.Parameters.AddWithValue("@id_proyecto", _proyecto.IdProyecto);
                        cmdHorario.Parameters.AddWithValue("@id_docente", docente.Id);

                        using var readerHorario = cmdHorario.ExecuteReader();
                        while (readerHorario.Read())
                        {
                            horarioPersonal.Add(new GestorHorarios.PROYECTOS.HorarioCarreraView.HorarioPersonal
                            {
                                Dia = Convert.ToInt32(readerHorario["id_dia"]),
                                Bloque = Convert.ToInt32(readerHorario["id_bloque"]),
                                Materia = readerHorario["NombreMateria"].ToString() ?? "",
                                Grupo = readerHorario["NombreGrupo"].ToString() ?? ""
                            });
                        }
                    }

                    docentesData.Add(new GestorHorarios.Services.GeneradorWordService.DocenteDataWord
                    {
                        NombreDocente = docente.Nombre,
                        Hfg = docente.Hfg,
                        Horarios = horarioPersonal
                    });
                }

                // 3. Mandar al Generador de Word si hay datos
                if (docentesData.Count > 0)
                {
                    string rutaCarpeta = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    string rutaArchivo = System.IO.Path.Combine(rutaCarpeta, "Horarios_Docentes.docx");

                    var generador = new GestorHorarios.Services.GeneradorWordService();
                    generador.ExportarHorariosDocentes(rutaArchivo, tituloCiclo, docentesData);

                    BarraProgreso.Visibility = Visibility.Collapsed;
                    TxtProgreso.Visibility = Visibility.Collapsed;

                    MessageBox.Show($"¡Se generó exitosamente el archivo con {docentesData.Count} maestros!\n\nRevisa tu Escritorio el archivo: Horarios_Docentes.docx", "Exportación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    BarraProgreso.Visibility = Visibility.Collapsed;
                    TxtProgreso.Visibility = Visibility.Collapsed;
                    MessageBox.Show("No se encontraron docentes con horarios asignados en este ciclo.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                BarraProgreso.Visibility = Visibility.Collapsed;
                TxtProgreso.Visibility = Visibility.Collapsed;
                MessageBox.Show($"Ocurrió un error:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Border CrearTarjetaDiagnostico(string clave, string nombre, int grupos, int requeridas, int docentes, int capacidad, string semaforo, int creditos, string area)
        {
            string bgColor = "#FFFFFF";
            string borderColor = "#CCCCCC";
            string textColor = "#333333";
            string estatus = "";
            string consejo = "";

            if (semaforo == "Rojo")
            {
                bgColor = "#FFEBEE";
                borderColor = "#D32F2F";
                textColor = "#C62828";
                estatus = "CRITICO: Faltan horas";

                int faltantes = requeridas - capacidad;
                int gruposFaltantes = (int)Math.Ceiling((double)faltantes / (creditos == 0 ? 1 : creditos));

                consejo = $"CONSEJO: Te faltan cubrir {faltantes} horas (aprox. {gruposFaltantes} grupos). El algoritmo dejara bloques vacios si no intervienes. " +
                          $"Necesitas habilitar/contratar urgentemente maestros de '{area}'.";
            }
            else if (semaforo == "Amarillo")
            {
                bgColor = "#FFFDE7";
                borderColor = "#FBC02D";
                textColor = "#F57F17";
                estatus = "AL LIMITE";

                consejo = $"CONSEJO: Tienes las horas exactas, pero dependes de {docentes} maestro(s) para cubrir {grupos} grupos. " +
                          $"Existe riesgo de cuello de botella si se empalman horarios. Considera habilitar a otro docente de '{area}'.";
            }
            else
            {
                bgColor = "#E8F5E9";
                borderColor = "#388E3C";
                textColor = "#2E7D32";
                estatus = "OPTIMO";

                consejo = $"CONSEJO: Capacidad holgada. Puedes asignar esto a 1 maestro a tiempo completo ({requeridas} hrs), " +
                          $"o distribuirlo entre {grupos} maestros ({creditos} hrs c/u) del area de '{area}'.";
            }

            var border = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bgColor)),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(borderColor)),
                BorderThickness = new Thickness(4, 1, 1, 1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(15),
                Margin = new Thickness(5, 5, 15, 15),
                VerticalAlignment = VerticalAlignment.Top
            };

            var mainStack = new StackPanel();

            var gridTop = new Grid();
            gridTop.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            gridTop.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var infoStack = new StackPanel();
            infoStack.Children.Add(new TextBlock { Text = $"[{clave}] {nombre}", FontSize = 15, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333")), Margin = new Thickness(0, 0, 0, 6), TextWrapping = TextWrapping.Wrap });
            infoStack.Children.Add(new TextBlock { Text = $"Demanda: {requeridas} horas (Para {grupos} grupos de {creditos} creditos)", FontSize = 13, Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#555555")), TextWrapping = TextWrapping.Wrap });
            infoStack.Children.Add(new TextBlock { Text = $"Capacidad: {capacidad} horas (De {docentes} docentes)", FontSize = 13, FontWeight = FontWeights.SemiBold, Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#555555")), TextWrapping = TextWrapping.Wrap });

            Grid.SetColumn(infoStack, 0);
            gridTop.Children.Add(infoStack);

            var txtEstatus = new TextBlock
            {
                Text = estatus,
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(textColor)),
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(10, 0, 0, 0)
            };
            Grid.SetColumn(txtEstatus, 1);
            gridTop.Children.Add(txtEstatus);

            mainStack.Children.Add(gridTop);

            var borderConsejo = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F5F5")),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 12, 0, 0)
            };

            var txtConsejo = new TextBlock
            {
                Text = consejo,
                FontSize = 13,
                FontStyle = FontStyles.Italic,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#424242")),
                TextWrapping = TextWrapping.Wrap
            };

            borderConsejo.Child = txtConsejo;
            mainStack.Children.Add(borderConsejo);

            border.Child = mainStack;
            return border;
        }

        private async void BtnAsignarSalones_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Este proceso asignará salones a los horarios ya generados, minimizando los cambios de aula.\n\n¿Deseas continuar?",
                "Asignar Salones", MessageBoxButton.YesNo, MessageBoxImage.Information);

            if (result != MessageBoxResult.Yes) return;

            BtnGenerarTodos.IsEnabled = false;
            BtnAnalisisPrevio.IsEnabled = false;
            BtnAsignarSalones.IsEnabled = false;
            BarraProgreso.Visibility = Visibility.Visible;
            TxtProgreso.Visibility = Visibility.Visible;
            BarraProgreso.IsIndeterminate = true;
            TxtProgreso.Text = "Calculando la distribución óptima de salones...";

            try
            {
                var asignador = new AsignadorSalonesService();
                bool exito = await asignador.EjecutarAsignacionSalonesAsync(_proyecto.IdProyecto);

                if (exito)
                {
                    MessageBox.Show("¡Los salones han sido asignados exitosamente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("No se encontró una distribución viable. Revisa si hay más clases simultáneas que salones.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al asignar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnGenerarTodos.IsEnabled = true;
                BtnAnalisisPrevio.IsEnabled = true;
                BtnAsignarSalones.IsEnabled = true;
                BarraProgreso.IsIndeterminate = false;
                BarraProgreso.Visibility = Visibility.Collapsed;
                TxtProgreso.Visibility = Visibility.Collapsed;
            }
        }

        private void CerrarModalDiagnostico_Click(object sender, RoutedEventArgs e)
        {
            ModalDiagnostico.Visibility = Visibility.Collapsed;
        }

        // ==========================================
        // NAVEGACIÓN DEL MODAL
        // ==========================================
        private void BtnAreaAnterior_Click(object sender, RoutedEventArgs e)
        {
            if (_encabezadosAreas.Count == 0) return;

            if (_indiceAreaActual > 0)
            {
                _indiceAreaActual--;
                _encabezadosAreas[_indiceAreaActual].BringIntoView();
            }
        }

        private void BtnAreaSiguiente_Click(object sender, RoutedEventArgs e)
        {
            if (_encabezadosAreas.Count == 0) return;

            if (_indiceAreaActual < _encabezadosAreas.Count - 1)
            {
                _indiceAreaActual++;
                _encabezadosAreas[_indiceAreaActual].BringIntoView();
            }
        }

        // ==========================================
        // LÓGICA DE GENERACIÓN
        // ==========================================
        private async void BtnGenerarTodos_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Este proceso borrara los horarios actuales y generara nuevos para TODAS las carreras.\n\n¿Deseas continuar?",
                "Generar Horarios Globales", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            BtnGenerarTodos.IsEnabled = false;
            BtnAnalisisPrevio.IsEnabled = false;
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
                    else if (!resultado.StartsWith("Omitido"))
                    {
                        reporteErrores += $"\n- {carrera.Nombre}: {resultado}\n";
                    }

                    completadas++;
                    BarraProgreso.Value = (completadas * 100) / total;
                }

                if (string.IsNullOrEmpty(reporteErrores))
                {
                    MessageBox.Show($"¡Todos los horarios han sido generados exitosamente!\n\nSe procesaron {exitos} carreras.", "Exito Total", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"Se generaron {exitos} horarios, pero hubo problemas en las siguientes carreras:\n{reporteErrores}", "Reporte de Errores", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrio un error critico: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnGenerarTodos.IsEnabled = true;
                BtnAnalisisPrevio.IsEnabled = true;
                BarraProgreso.Visibility = Visibility.Collapsed;
                TxtProgreso.Visibility = Visibility.Collapsed;

                CargarCarreras();
            }
        }

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
                    MessageBox.Show("Aun no hay un horario calculado para esta carrera.\n\nPor favor, haz clic en el boton verde de arriba 'Generar Todos los Horarios'.",
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