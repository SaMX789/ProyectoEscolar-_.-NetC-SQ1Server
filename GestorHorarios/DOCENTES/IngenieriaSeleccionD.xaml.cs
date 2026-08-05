using GestorHorarios.Models;
using GestorHorarios.Services;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GestorHorarios.DOCENTES
{
    /// <summary>
    /// Lógica de interacción para IngenieriaSeleccionD.xaml
    /// </summary>
    public partial class IngenieriaSeleccionD : UserControl
    {
        #region Constantes
        private const double RECT_WIDTH = 50;
        private const double RECT_HEIGHT = 40;
        private const double ROW_HEIGHT = 50;
        private const double LABEL_WIDTH = 80;
        private const double SPACING = 2;

        private static readonly string[] DIAS = { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes" };
        private static readonly Color[] COLORES_DIAS =
        {
            (Color)ColorConverter.ConvertFromString("#FFE0B2"),
            (Color)ColorConverter.ConvertFromString("#C8E6C9"),
            (Color)ColorConverter.ConvertFromString("#BBDEFB"),
            (Color)ColorConverter.ConvertFromString("#F8BBD0"),
            (Color)ColorConverter.ConvertFromString("#FFE0B2")
        };
        private static readonly string[] HORAS_DISPONIBLES =
        {
            "7:30", "8:30", "9:30", "10:30", "11:30", "12:30",
            "13:30", "14:30", "15:30", "16:30", "17:30", "18:30", "19:30"
        };
        #endregion

        #region Campos Privados
        private int _idCarrera;
        private readonly List<Carrera> _carrerasDisponibles = new();
        private readonly List<int> _materiasSeleccionadasIds = new();
        private readonly Dictionary<string, (int horaInicio, int horaFin)> _horarioSeleccionado = new();
        private readonly Dictionary<string, UIElement> _secondaryPanels = new();

        // Mapeo id_materia -> nombre para el resumen
        private readonly Dictionary<int, string> _todasLasMaterias = new();

        // Total de horas seleccionadas en el Canvas (cada celda = 1 hora)
        private int _horasAsignadas = 0;
        private const int HORAS_MAXIMAS_PERMITIDAS = 40;

        // null = modo agregar, distinto de null = modo editar
        private Docente? _docenteEnEdicion = null;

        private UIElement? _mainMateriasPanel;
        #endregion

        #region Constructores
        public IngenieriaSeleccionD()
        {
            InitializeComponent();
        }

        public IngenieriaSeleccionD(int idCarrera) : this()
        {
            _idCarrera = idCarrera;
            CargarTituloCarrera();
            LoadDocentes();
        }
        #endregion

        #region Carga inicial
        private void CargarTituloCarrera()
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
                var resultado = cmd.ExecuteScalar();
                TituloCarrera.Text = $"Docentes de {resultado}";
            }
            catch
            {
                TituloCarrera.Text = "Docentes";
            }
        }
        #endregion

        #region Eventos de Botones Principales
        private void AgregarDocente_Click(object sender, RoutedEventArgs e)
        {
            bool estaCerrado = PanelFormularioDocente.Visibility == Visibility.Collapsed;

            PanelFormularioDocente.Visibility = estaCerrado ? Visibility.Visible : Visibility.Collapsed;
            BotonAgregarDocente.Content = estaCerrado ? "Cerrar" : "+ Agregar Docente";

            if (estaCerrado)
            {
                CargarCarrerasYMaterias();
                GenerarFilasHorario();
                // Disparar carga de materias de la carrera preseleccionada
                if (ComboBoxCarreraPrincipal.SelectedItem != null)
                    ComboBoxCarreraPrincipal_SelectionChanged(this,
                        new SelectionChangedEventArgs(
                            System.Windows.Controls.Primitives.Selector.SelectionChangedEvent,
                            new List<object>(), new List<object>()));
            }
        }

        private void GuardarDocente_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarFormulario()) return;

            try
            {
                if (_docenteEnEdicion == null)
                {
                    GuardarDocenteEnBD();
                    MessageBox.Show($"Docente '{TextboxNombre.Text}' guardado exitosamente.",
                        "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    ActualizarDocenteEnBD(_docenteEnEdicion.IdDocente);
                    MessageBox.Show($"Docente '{TextboxNombre.Text}' actualizado exitosamente.",
                        "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                _docenteEnEdicion = null;
                LimpiarFormulario();
                CerrarFormulario();
                LoadDocentes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelarDocente_Click(object sender, RoutedEventArgs e)
        {
            _docenteEnEdicion = null;
            LimpiarFormulario();
            CerrarFormulario();
        }

        private void VolverCarreras_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GetFromWindow(this)?.NavigateTo(new DocentesView());
        }
        #endregion

        #region Guardar en BD
        private void GuardarDocenteEnBD()
        {
            var nombre = TextboxNombre.Text.Trim();
            var tipoDocente = (ComboBoxTipoTiempo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "TIEMPO COMPLETO";
            int horasMaximas = int.TryParse(TextboxHorasMaximas.Text, out int hm) ? hm : 20;

            // Obtener id_carrera principal seleccionada
            int idCarreraPrincipal = _idCarrera;
            if (ComboBoxCarreraPrincipal.SelectedItem is ComboBoxItem cbItem
                && cbItem.Tag is int idTag)
                idCarreraPrincipal = idTag;

            // Obtener ids de carreras secundarias marcadas
            var idCarrerasSecundarias = new List<int>();
            foreach (var cb in StackPanelCarrerasSecundarias.Children.OfType<CheckBox>())
            {
                if (cb.IsChecked == true && cb.Tag is int idSec)
                    idCarrerasSecundarias.Add(idSec);
            }

            // Obtener disponibilidad del Canvas (bloques por dia)
            // Mapeamos índice de hora a id_bloque cargado de BD
            var disponibilidad = ObtenerDisponibilidadSeleccionada();

            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                // 1. Insertar Docente
                int idDocente = InsertarDocente(conn, transaction, nombre, tipoDocente, horasMaximas);

                // 2. Insertar carrera principal
                InsertarDocenteCarrera(conn, transaction, idDocente, idCarreraPrincipal, esPrincipal: true);

                // 3. Insertar carreras secundarias
                foreach (var idSec in idCarrerasSecundarias)
                    InsertarDocenteCarrera(conn, transaction, idDocente, idSec, esPrincipal: false);

                // 4. Insertar materias impartidas
                foreach (var idMateria in _materiasSeleccionadasIds.Distinct())
                    InsertarDocenteMateria(conn, transaction, idDocente, idMateria);

                // 5. Insertar disponibilidad
                foreach (var (idDia, idBloque) in disponibilidad)
                    InsertarDisponibilidad(conn, transaction, idDocente, idDia, idBloque);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private static int InsertarDocente(SqlConnection conn, SqlTransaction tx,
            string nombre, string tipoDocente, int horasMaximas)
        {
            using var cmd = new SqlCommand(@"
                INSERT INTO Docentes (nombre, tipo_docente, id_estado, horas_maximas)
                VALUES (@nombre, @tipo, 1, @horas);
                SELECT SCOPE_IDENTITY();", conn, tx);

            cmd.Parameters.AddWithValue("@nombre", nombre);
            cmd.Parameters.AddWithValue("@tipo", tipoDocente);
            cmd.Parameters.AddWithValue("@horas", horasMaximas);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private static void InsertarDocenteCarrera(SqlConnection conn, SqlTransaction tx,
            int idDocente, int idCarrera, bool esPrincipal)
        {
            using var cmd = new SqlCommand(@"
                INSERT INTO DocenteCarrera (id_docente, id_carrera, es_principal)
                VALUES (@idDocente, @idCarrera, @esPrincipal);", conn, tx);

            cmd.Parameters.AddWithValue("@idDocente", idDocente);
            cmd.Parameters.AddWithValue("@idCarrera", idCarrera);
            cmd.Parameters.AddWithValue("@esPrincipal", esPrincipal);
            cmd.ExecuteNonQuery();
        }

        private static void InsertarDocenteMateria(SqlConnection conn, SqlTransaction tx,
            int idDocente, int idMateria)
        {
            using var cmd = new SqlCommand(@"
                INSERT INTO DocenteMateria (id_docente, id_materia)
                VALUES (@idDocente, @idMateria);", conn, tx);

            cmd.Parameters.AddWithValue("@idDocente", idDocente);
            cmd.Parameters.AddWithValue("@idMateria", idMateria);
            cmd.ExecuteNonQuery();
        }

        private static void InsertarDisponibilidad(SqlConnection conn, SqlTransaction tx,
            int idDocente, int idDia, int idBloque)
        {
            using var cmd = new SqlCommand(@"
                INSERT INTO DisponibilidadDocente (id_docente, id_dia, id_bloque)
                VALUES (@idDocente, @idDia, @idBloque);", conn, tx);

            cmd.Parameters.AddWithValue("@idDocente", idDocente);
            cmd.Parameters.AddWithValue("@idDia", idDia);
            cmd.Parameters.AddWithValue("@idBloque", idBloque);
            cmd.ExecuteNonQuery();
        }

        // Devuelve lista de (id_dia, id_bloque) según las celdas seleccionadas en el Canvas
        private List<(int idDia, int idBloque)> ObtenerDisponibilidadSeleccionada()
        {
            var resultado = new List<(int, int)>();

            // DiasSemana: Lunes=1..Viernes=5 (según orden en BD)
            var diasMap = new Dictionary<string, int>
            {
                { "Lunes", 1 }, { "Martes", 2 }, { "Miércoles", 3 },
                { "Jueves", 4 }, { "Viernes", 5 }
            };

            // Cargar bloques desde BD para mapear índice → id_bloque
            var bloques = CargarBloques();

            foreach (var (dia, rango) in _horarioSeleccionado)
            {
                if (!diasMap.TryGetValue(dia, out int idDia)) continue;

                for (int i = rango.horaInicio; i <= rango.horaFin; i++)
                {
                    if (i < bloques.Count)
                        resultado.Add((idDia, bloques[i]));
                }
            }

            return resultado;
        }

        // Carga los id_bloque de BD en orden para mapear índice de Canvas → id real
        private static List<int> CargarBloques()
        {
            var bloques = new List<int>();
            try
            {
                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                using var cmd = new SqlCommand(
                    "SELECT id_bloque FROM BloquesHora ORDER BY hora_inicio", conn);
                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    bloques.Add(Convert.ToInt32(reader["id_bloque"]));
            }
            catch { /* si falla, la disponibilidad simplemente no se guarda */ }

            return bloques;
        }
        #endregion

        #region Modo Edición
        private void AbrirFormularioEdicion(Docente docente)
        {
            _docenteEnEdicion = docente;

            PanelFormularioDocente.Visibility = Visibility.Visible;
            BotonAgregarDocente.Content = "Cerrar";
            BotonGuardarDocente.Content = "ACTUALIZAR";

            // Preseleccionar la carrera principal del docente
            _idCarrera = docente.IdCarreraPrincipal;

            CargarCarrerasYMaterias();
            GenerarFilasHorario();

            // Datos básicos
            TextboxNombre.Text = docente.NombreCompleto;
            TextboxHorasMaximas.Text = docente.HorasMaximas.ToString();

            foreach (ComboBoxItem cbItem in ComboBoxTipoTiempo.Items)
                if (cbItem.Content?.ToString() == docente.TipoTiempo)
                { ComboBoxTipoTiempo.SelectedItem = cbItem; break; }

            // Marcar carreras secundarias
            foreach (var cb in StackPanelCarrerasSecundarias.Children.OfType<CheckBox>())
                if (cb.Tag is int idCb && docente.CarrerasSecundariasIds.Contains(idCb))
                    cb.IsChecked = true;

            // Marcar materias ya seleccionadas
            MarcarMateriasEnPaneles(docente.MateriasIds);

            // Restaurar horario en el Canvas directamente desde BD
            RestaurarHorarioEnCanvas(docente.IdDocente);

            PanelFormularioDocente.BringIntoView();
        }

        private void MarcarMateriasEnPaneles(List<int> ids)
        {
            // Función recursiva para encontrar y marcar CheckBoxes en modo Edición
            void Marcar(UIElementCollection children)
            {
                foreach (UIElement element in children)
                {
                    if (element is CheckBox cb && cb.Tag is int id && ids.Contains(id))
                        cb.IsChecked = true;
                    else if (element is Panel panel)
                        Marcar(panel.Children);
                    else if (element is Border border && border.Child is Panel childPanel)
                        Marcar(childPanel.Children);
                }
            }

            Marcar(StackPanelMateriasImpartidas.Children);
        }

        private void RestaurarHorarioEnCanvas(int idDocente)
        {
            // Mapeo id_dia → nombre de día en el Canvas
            var diasMap = new Dictionary<int, string>
            {
                { 1, "Lunes" }, { 2, "Martes" }, { 3, "Miércoles" },
                { 4, "Jueves" }, { 5, "Viernes" }
            };

            // Cargar bloques en orden para mapear id_bloque → índice de columna en el Canvas
            var bloquesOrdenados = CargarBloques(); // lista de id_bloque ordenada por hora_inicio
            var bloqueAIndice = new Dictionary<int, int>();
            for (int i = 0; i < bloquesOrdenados.Count; i++)
                bloqueAIndice[bloquesOrdenados[i]] = i;

            // Cargar disponibilidad real del docente desde BD
            var disponibilidad = new List<(int idDia, int idBloque)>();
            try
            {
                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                using var cmd = new SqlCommand(
                    "SELECT id_dia, id_bloque FROM DisponibilidadDocente WHERE id_docente=@id ORDER BY id_dia, id_bloque",
                    conn);
                cmd.Parameters.AddWithValue("@id", idDocente);
                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    disponibilidad.Add((Convert.ToInt32(reader["id_dia"]), Convert.ToInt32(reader["id_bloque"])));
            }
            catch { return; }

            if (disponibilidad.Count == 0) return;

            // Reconstruir _horarioSeleccionado agrupando bloques consecutivos por día
            // Un "rango" en el Canvas es (índiceInicio, índiceFin) de bloques contiguos
            var porDia = disponibilidad
                .GroupBy(x => x.idDia)
                .Where(g => diasMap.ContainsKey(g.Key));

            foreach (var grupo in porDia)
            {
                var dia = diasMap[grupo.Key];
                var indices = grupo
                    .Where(x => bloqueAIndice.ContainsKey(x.idBloque))
                    .Select(x => bloqueAIndice[x.idBloque])
                    .OrderBy(i => i)
                    .ToList();

                if (indices.Count == 0) continue;

                // Usar el rango completo: del primer al último índice del día
                int inicio = indices.First();
                int fin = indices.Last();

                _horarioSeleccionado[dia] = (inicio, fin);

                int diaIdx = Array.IndexOf(DIAS, dia);
                if (diaIdx >= 0)
                    ActualizarHorario(dia, COLORES_DIAS[diaIdx]);
            }

            ActualizarContadorHoras();
        }
        #endregion

        #region Validación
        private bool ValidarFormulario()
        {
            if (string.IsNullOrWhiteSpace(TextboxNombre.Text))
            {
                MostrarAdvertencia("Por favor, ingrese el nombre del docente.");
                return false;
            }

            if (ComboBoxCarreraPrincipal.SelectedItem == null)
            {
                MostrarAdvertencia("Por favor, seleccione una carrera principal.");
                return false;
            }

            if (ComboBoxTipoTiempo.SelectedItem == null)
            {
                MostrarAdvertencia("Por favor, seleccione el tipo de contrato.");
                return false;
            }

            if (!int.TryParse(TextboxHorasMaximas.Text, out int hm) || hm <= 0)
            {
                MostrarAdvertencia("Por favor, ingrese un número válido de horas máximas.");
                return false;
            }

            if (hm > HORAS_MAXIMAS_PERMITIDAS)
            {
                MostrarAdvertencia($"Las horas máximas no pueden superar {HORAS_MAXIMAS_PERMITIDAS}h semanales.");
                return false;
            }

            if (_horasAsignadas > hm)
            {
                MostrarAdvertencia(
                    $"Tienes {_horasAsignadas}h asignadas en el horario, " +
                    $"pero el máximo configurado es {hm}h. " +
                    $"Reduce el horario o aumenta las horas máximas.");
                return false;
            }

            if (_horasAsignadas < hm)
            {
                MostrarAdvertencia(
                    $"Debes asignar exactamente {hm}h en el horario. " +
                    $"Actualmente tienes {_horasAsignadas}h asignadas. " +
                    $"Completa el horario o reduce las horas máximas a {_horasAsignadas}.");
                return false;
            }

            return true;
        }

        private static void MostrarAdvertencia(string mensaje) =>
            MessageBox.Show(mensaje, "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
        #endregion

        #region Gestión de Carreras y Materias (desde BD)
        private void CargarCarrerasYMaterias()
        {
            LimpiarControles();

            try
            {
                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                using var cmd = new SqlCommand(
                    "SELECT id_carrera, nombre FROM Carreras ORDER BY nombre", conn);
                conn.Open();
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var carrera = new Carrera
                    {
                        IdCarrera = Convert.ToInt32(reader["id_carrera"]),
                        Nombre = reader["nombre"].ToString() ?? ""
                    };
                    _carrerasDisponibles.Add(carrera);

                    var item = new ComboBoxItem
                    {
                        Content = carrera.Nombre,
                        Tag = carrera.IdCarrera
                    };
                    ComboBoxCarreraPrincipal.Items.Add(item);
                    CrearCheckBoxCarreraSecundaria(carrera);
                }
            }
            catch (Exception ex)
            {
                MostrarMensajeError($"Error cargando carreras: {ex.Message}");
            }

            // Suscribir ANTES de asignar SelectedItem para que dispare al preseleccionar
            ComboBoxCarreraPrincipal.SelectionChanged -= ComboBoxCarreraPrincipal_SelectionChanged;
            ComboBoxCarreraPrincipal.SelectionChanged += ComboBoxCarreraPrincipal_SelectionChanged;

            // Preseleccionar la carrera principal del constructor — esto disparará el evento
            foreach (ComboBoxItem item in ComboBoxCarreraPrincipal.Items)
            {
                if (item.Tag is int id && id == _idCarrera)
                {
                    ComboBoxCarreraPrincipal.SelectedItem = item;
                    break;
                }
            }

            StackPanelCarrerasSecundarias.Visibility =
                ComboBoxCarreraPrincipal.SelectedItem != null ? Visibility.Visible : Visibility.Collapsed;
        }

        private void CrearCheckBoxCarreraSecundaria(Carrera carrera)
        {
            var checkBox = new CheckBox
            {
                Content = carrera.Nombre,
                Tag = carrera.IdCarrera,
                Margin = new Thickness(10, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center,
                IsEnabled = true  // Se gestiona dinámicamente en ActualizarCheckBoxesSecundarias
            };

            checkBox.Checked += (_, _) => AddSecondaryPanel(carrera);
            checkBox.Unchecked += (_, _) => RemoveSecondaryPanel(carrera.IdCarrera);

            StackPanelCarrerasSecundarias.Children.Add(checkBox);
        }

        private void ComboBoxCarreraPrincipal_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxCarreraPrincipal.SelectedItem is not ComboBoxItem item
                || item.Tag is not int idSeleccionada)
            {
                StackPanelCarrerasSecundarias.Visibility = Visibility.Collapsed;
                ClearMateriasPanels();
                return;
            }

            ActualizarCheckBoxesSecundarias(idSeleccionada);
            StackPanelCarrerasSecundarias.Visibility = Visibility.Visible;

            ClearMateriasPanels();
            _mainMateriasPanel = BuildMateriasPanelForCareer(idSeleccionada,
                item.Content.ToString() ?? "", isMain: true);
            StackPanelMateriasImpartidas.Children.Add(_mainMateriasPanel);
            ActualizarResumenMaterias();
        }

        private void ActualizarCheckBoxesSecundarias(int idCarreraSeleccionada)
        {
            foreach (var cb in StackPanelCarrerasSecundarias.Children.OfType<CheckBox>())
            {
                if (cb.Tag is int idCb && idCb == idCarreraSeleccionada)
                {
                    if (cb.IsChecked == true)
                        RemoveSecondaryPanel(idCb);
                    cb.IsChecked = false;
                    cb.IsEnabled = false;
                }
                else
                {
                    cb.IsEnabled = true;
                    cb.IsChecked = false;
                }
            }
        }

        private UIElement BuildMateriasPanelForCareer(int idCarrera, string nombreCarrera, bool isMain)
        {
            // Contenedor principal de toda la carrera
            var mainBorder = new Border
            {
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DDDDDD")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(0, 10, 0, 15),
                Tag = idCarrera // Importante para la lógica interna
            };

            var mainStack = new StackPanel();

            // 1. Cabecera con color de la Carrera
            var headerBorder = new Border
            {
                // Rosa bajito si es principal, gris claro si es secundaria
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isMain ? "#FCE4EC" : "#F5F5F5")),
                CornerRadius = new CornerRadius(7, 7, 0, 0),
                Padding = new Thickness(15, 10, 15, 10)
            };

            headerBorder.Child = new TextBlock
            {
                Text = $"{(isMain ? "Principal" : "Secundaria")}: {nombreCarrera}",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource("GuindaBajo")
            };
            mainStack.Children.Add(headerBorder);
            // 2. Contenedor de los Semestres
            var contentStack = new StackPanel { Margin = new Thickness(15) };

            try
            {
                var materias = CargarMateriasDeBD(idCarrera);
                var porSemestre = materias.GroupBy(m => m.Semestre).OrderBy(g => g.Key);

                foreach (var grupo in porSemestre)
                {
                    // Tarjeta individual para aislar visualmente cada semestre
                    var semesterBorder = new Border
                    {
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FAFAFA")),
                        BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EEEEEE")),
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(6),
                        Margin = new Thickness(0, 0, 0, 12),
                        Padding = new Thickness(12)
                    };

                    var semesterStack = new StackPanel();

                    semesterStack.Children.Add(new TextBlock
                    {
                        Text = $"Semestre {grupo.Key}",
                        FontSize = 13,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#444444")),
                        Margin = new Thickness(0, 0, 0, 8)
                    });

                    // WRAPPANEL: La magia para que las cajas bajen de renglón automáticamente
                    var panelMaterias = new WrapPanel { Orientation = Orientation.Horizontal };

                    foreach (var materia in grupo)
                    {
                        var cb = new CheckBox
                        {
                            Content = materia.Nombre,
                            Tag = materia.IdMateria,
                            Margin = new Thickness(0, 0, 15, 8),
                            FontSize = 12,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#555555")),
                            Cursor = Cursors.Hand
                        };
                        cb.Checked += MateriaCb_Changed;
                        cb.Unchecked += MateriaCb_Changed;
                        panelMaterias.Children.Add(cb);

                        _todasLasMaterias[materia.IdMateria] = materia.Nombre;
                    }

                    semesterStack.Children.Add(panelMaterias);
                    semesterBorder.Child = semesterStack;
                    contentStack.Children.Add(semesterBorder);
                }

                if (!materias.Any())
                {
                    contentStack.Children.Add(new TextBlock
                    {
                        Text = "No hay materias registradas para esta carrera.",
                        FontSize = 12,
                        Foreground = Brushes.Gray,
                        FontStyle = FontStyles.Italic
                    });
                }
            }
            catch (Exception ex)
            {
                contentStack.Children.Add(new TextBlock
                {
                    Text = $"Error cargando materias: {ex.Message}",
                    Foreground = Brushes.Red,
                    FontSize = 12
                });
            }

            mainStack.Children.Add(contentStack);
            mainBorder.Child = mainStack;

            return mainBorder;
        }

        private static List<Materia> CargarMateriasDeBD(int idCarrera)
        {
            var lista = new List<Materia>();
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            using var cmd = new SqlCommand("sp_ObtenerMateriasPorCarrera", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@id_carrera", idCarrera);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Materia
                {
                    IdMateria = Convert.ToInt32(reader["id_materia"]),
                    Nombre = reader["nombre"].ToString() ?? "",
                    Clave = reader["clave"].ToString() ?? "",
                    Creditos = Convert.ToInt32(reader["creditos"]),
                    Semestre = Convert.ToInt32(reader["semestre"])
                });
            }
            return lista;
        }

        private void MateriaCb_Changed(object sender, RoutedEventArgs e) =>
            ActualizarResumenMaterias();

        private void AddSecondaryPanel(Carrera carrera)
        {
            if (_secondaryPanels.ContainsKey(carrera.IdCarrera.ToString())) return;

            var panel = BuildMateriasPanelForCareer(carrera.IdCarrera, carrera.Nombre, isMain: false);
            _secondaryPanels[carrera.IdCarrera.ToString()] = panel;
            StackPanelMateriasImpartidas.Children.Add(panel);
            ActualizarResumenMaterias();
        }

        private void RemoveSecondaryPanel(int idCarrera)
        {
            var key = idCarrera.ToString();
            if (!_secondaryPanels.TryGetValue(key, out var panel)) return;

            StackPanelMateriasImpartidas.Children.Remove(panel);
            _secondaryPanels.Remove(key);
            ActualizarResumenMaterias();
        }

        private void ClearMateriasPanels()
        {
            StackPanelMateriasImpartidas.Children.Clear();
            _secondaryPanels.Clear();
            _mainMateriasPanel = null;
            ActualizarResumenMaterias();
        }
        #endregion

        #region Resumen de materias seleccionadas
        private void ActualizarResumenMaterias()
        {
            _materiasSeleccionadasIds.Clear();

            // Función recursiva para encontrar todos los CheckBoxes marcados
            void ExtraerIds(UIElementCollection children)
            {
                foreach (UIElement element in children)
                {
                    if (element is CheckBox cb && cb.IsChecked == true && cb.Tag is int idMateria)
                        _materiasSeleccionadasIds.Add(idMateria);
                    else if (element is Panel panel)
                        ExtraerIds(panel.Children);
                    else if (element is Border border && border.Child is Panel childPanel)
                        ExtraerIds(childPanel.Children);
                }
            }

            ExtraerIds(StackPanelMateriasImpartidas.Children);

            if (_materiasSeleccionadasIds.Count == 0)
            {
                TextblockMateriasSeleccionadas.Text = "(Ninguna seleccionada)";
                TextboxHorasDiarias.Text = "0";
                return;
            }

            var nombres = _materiasSeleccionadasIds
                .Distinct()
                .Where(id => _todasLasMaterias.ContainsKey(id))
                .Select(id => _todasLasMaterias[id]);

            TextblockMateriasSeleccionadas.Text = string.Join(", ", nombres);
            TextboxHorasDiarias.Text = _materiasSeleccionadasIds.Distinct().Count().ToString();
        }
        #endregion

        #region Generación de Horarios (Canvas)
        private void GenerarFilasHorario()
        {
            CanvasHorario.Children.Clear();
            _horarioSeleccionado.Clear();

            double canvasWidth = LABEL_WIDTH + (HORAS_DISPONIBLES.Length * (RECT_WIDTH + SPACING));
            CanvasHorario.Width = canvasWidth;

            GenerarEncabezadoHoras();
            GenerarFilasDias();
        }

        private void GenerarEncabezadoHoras()
        {
            double xPos = LABEL_WIDTH + SPACING;
            foreach (var hora in HORAS_DISPONIBLES)
            {
                var headerText = new TextBlock
                {
                    Text = hora,
                    FontSize = 10,
                    FontWeight = FontWeights.Bold,
                    Foreground = (Brush)FindResource("GuindaBajo"),
                    TextAlignment = TextAlignment.Center
                };
                Canvas.SetLeft(headerText, xPos);
                Canvas.SetTop(headerText, 5);
                CanvasHorario.Children.Add(headerText);
                xPos += RECT_WIDTH + SPACING;
            }
        }

        private void GenerarFilasDias()
        {
            for (int diaIdx = 0; diaIdx < DIAS.Length; diaIdx++)
                GenerarFilaDia(diaIdx);
        }

        private void GenerarFilaDia(int diaIdx)
        {
            var dia = DIAS[diaIdx];
            var colorDia = COLORES_DIAS[diaIdx];
            double yPos = 30 + (diaIdx * ROW_HEIGHT);

            var diaLabel = new Border
            {
                Background = new SolidColorBrush(colorDia),
                CornerRadius = new CornerRadius(4),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#999999")),
                BorderThickness = new Thickness(1),
                Width = LABEL_WIDTH - 10,
                Height = RECT_HEIGHT,
                Child = new TextBlock
                {
                    Text = dia,
                    FontSize = 11,
                    FontWeight = FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
            Canvas.SetLeft(diaLabel, 5);
            Canvas.SetTop(diaLabel, yPos);
            CanvasHorario.Children.Add(diaLabel);

            double xPos = LABEL_WIDTH + SPACING;
            for (int horaIdx = 0; horaIdx < HORAS_DISPONIBLES.Length; horaIdx++)
            {
                var rect = CrearRectanguloHorario(dia, horaIdx, colorDia);
                Canvas.SetLeft(rect, xPos);
                Canvas.SetTop(rect, yPos);
                CanvasHorario.Children.Add(rect);
                xPos += RECT_WIDTH + SPACING;
            }
        }

        private Rectangle CrearRectanguloHorario(string dia, int horaIdx, Color colorDia)
        {
            var rect = new Rectangle
            {
                Width = RECT_WIDTH,
                Height = RECT_HEIGHT,
                Fill = Brushes.White,
                Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DDDDDD")),
                StrokeThickness = 1,
                Cursor = Cursors.Hand,
                Tag = $"{dia}|{horaIdx}|{HORAS_DISPONIBLES[horaIdx]}"
            };

            rect.MouseLeftButtonDown += (s, e) => Rect_Click(s, e, dia, horaIdx, colorDia);
            rect.MouseRightButtonDown += (s, e) => Rect_RightClick(dia, colorDia);
            rect.MouseEnter += Rect_MouseEnter;
            rect.MouseLeave += Rect_MouseLeave;
            return rect;
        }

        private void Rect_Click(object sender, MouseButtonEventArgs e, string dia, int horaIdx, Color colorDia)
        {
            if (!_horarioSeleccionado.ContainsKey(dia))
            {
                // Sin selección: iniciar rango con una celda
                if (!ValidarHorasDisponibles(dia, (horaIdx, horaIdx))) return;
                _horarioSeleccionado[dia] = (horaIdx, horaIdx);
            }
            else
            {
                var (horaInicio, horaFin) = _horarioSeleccionado[dia];

                // Clic dentro del rango ya seleccionado → ignorar (usar clic derecho para borrar)
                if (horaIdx >= horaInicio && horaIdx <= horaFin)
                    return;

                // Clic fuera del rango → expandir hacia ese extremo
                var nuevoRango = horaIdx < horaInicio
                    ? (horaIdx, horaFin)
                    : (horaInicio, horaIdx);

                if (!ValidarHorasDisponibles(dia, nuevoRango, reemplazando: true)) return;
                _horarioSeleccionado[dia] = nuevoRango;
            }
            ActualizarHorario(dia, colorDia);
            ActualizarContadorHoras();
        }

        // Clic derecho sobre cualquier celda del día: pide confirmación y borra el día completo
        private void Rect_RightClick(string dia, Color colorDia)
        {
            if (!_horarioSeleccionado.ContainsKey(dia)) return;

            var (ini, fin) = _horarioSeleccionado[dia];
            string horaIni = HORAS_DISPONIBLES[ini];
            string horaFin = HORAS_DISPONIBLES[fin];
            int horas = fin - ini + 1;

            var res = MessageBox.Show(
                $"¿Eliminar el horario de {dia} ({horaIni} – {horaFin}, {horas}h)?\n" +
                $"Esta acción solo afecta al formulario, no se guarda hasta presionar Guardar/Actualizar.",
                $"Limpiar {dia}",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (res != MessageBoxResult.Yes) return;

            _horarioSeleccionado.Remove(dia);
            ActualizarHorario(dia, colorDia);
            ActualizarContadorHoras();
        }

        // Valida que el nuevo rango no supere el límite absoluto de 40h.
        // El límite del TextBox (horas máximas del docente) se valida solo al guardar,
        // para que bajar el TextBox no bloquee editar el Canvas.
        private bool ValidarHorasDisponibles(string dia, (int inicio, int fin) nuevoRango, bool reemplazando = false)
        {
            int horasNuevas = nuevoRango.fin - nuevoRango.inicio + 1;

            int horasTotalesOtrosDias = _horarioSeleccionado
                .Where(kv => kv.Key != dia)
                .Sum(kv => kv.Value.horaFin - kv.Value.horaInicio + 1);

            int totalProyectado = horasTotalesOtrosDias + horasNuevas;

            if (totalProyectado > HORAS_MAXIMAS_PERMITIDAS)
            {
                MostrarAdvertencia(
                    $"No puedes asignar {totalProyectado}h. " +
                    $"El máximo absoluto permitido es {HORAS_MAXIMAS_PERMITIDAS}h semanales.");
                return false;
            }
            return true;
        }

        // Recalcula el total de horas asignadas en el Canvas y actualiza el TextBox
        private void ActualizarContadorHoras()
        {
            _horasAsignadas = _horarioSeleccionado
                .Sum(kv => kv.Value.horaFin - kv.Value.horaInicio + 1);

            TextboxHorasDiarias.Text = _horasAsignadas.ToString();

            // Indicador visual: rojo si supera el máximo configurado
            bool excede = int.TryParse(TextboxHorasMaximas.Text, out int max)
                          && _horasAsignadas > Math.Min(max, HORAS_MAXIMAS_PERMITIDAS);

            TextboxHorasDiarias.Background = excede
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCDD2"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F5F5"));
        }

        private static void Rect_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Rectangle rect)
            {
                rect.StrokeThickness = 2;
                rect.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666"));
            }
        }

        private static void Rect_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Rectangle rect)
            {
                rect.StrokeThickness = 1;
                rect.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DDDDDD"));
            }
        }

        private void ActualizarHorario(string dia, Color colorDia)
        {
            foreach (var rect in CanvasHorario.Children.OfType<Rectangle>()
                .Where(r => (r.Tag as string)?.StartsWith($"{dia}|") == true))
            {
                ActualizarColorCelda(rect, dia, colorDia);
            }
        }

        private void ActualizarColorCelda(Rectangle rect, string dia, Color colorDia)
        {
            var partes = (rect.Tag as string)?.Split('|');
            if (partes?.Length != 3 || !int.TryParse(partes[1], out int horaIdx)) return;

            rect.Fill = _horarioSeleccionado.TryGetValue(dia, out var rango)
                        && horaIdx >= rango.horaInicio && horaIdx <= rango.horaFin
                ? new SolidColorBrush(colorDia)
                : Brushes.White;
        }
        #endregion

        #region Carga de Docentes desde BD
        private void LoadDocentes()
        {
            ListaDocentes.Children.Clear();

            try
            {
                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                using var cmd = new SqlCommand("sp_GetDocentesByCarrera", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@id_carrera", _idCarrera);
                conn.Open();

                using var reader = cmd.ExecuteReader();
                if (!reader.HasRows)
                {
                    MostrarMensajeSinDatos();
                    return;
                }

                while (reader.Read())
                {
                    var docente = new Docente
                    {
                        IdDocente = Convert.ToInt32(reader["id_docente"]),
                        NombreCompleto = reader["NombreCompleto"] as string ?? "Sin nombre",
                        TipoTiempo = reader["TipoTiempo"] as string ?? string.Empty,
                        CarreraPrincipal = reader["CarreraPrincipal"] as string ?? string.Empty,
                        CarreraSecundaria = reader["CarreraSecundaria"] as string ?? "Ninguna",
                        Materias = reader["Materias"] as string ?? string.Empty,
                        HorarioLaboral = reader["HorarioLaboral"] as string ?? string.Empty,
                        HorasMaximas = reader["HorasMaximas"] != DBNull.Value
                            ? Convert.ToInt32(reader["HorasMaximas"]) : 0,
                        HorasAsignadas = reader["HorasAsignadas"] != DBNull.Value
                            ? Convert.ToInt32(reader["HorasAsignadas"]) : 0,
                        IdCarreraPrincipal = reader["IdCarreraPrincipal"] != DBNull.Value
                            ? Convert.ToInt32(reader["IdCarreraPrincipal"]) : 0
                    };

                    // Cargar ids de materias y carreras secundarias
                    docente.MateriasIds = CargarMateriasIdsDeDocente(docente.IdDocente);
                    docente.CarrerasSecundariasIds = CargarCarrerasSecundariasIds(docente.IdDocente);

                    ListaDocentes.Children.Add(CrearCardDocente(docente));
                }
            }
            catch (Exception ex)
            {
                MostrarMensajeError($"Error al cargar docentes: {ex.Message}");
            }
        }

        private void MostrarMensajeSinDatos()
        {
            ListaDocentes.Children.Add(new TextBlock
            {
                Text = "No hay docentes registrados para esta carrera.",
                FontSize = 14,
                Margin = new Thickness(10),
                Foreground = (Brush)FindResource("GuindaBajo")
            });
        }

        private void MostrarMensajeError(string mensaje)
        {
            ListaDocentes.Children.Clear();
            ListaDocentes.Children.Add(new TextBlock
            {
                Text = mensaje,
                Foreground = Brushes.Red,
                Margin = new Thickness(10)
            });
        }

        private static List<int> CargarMateriasIdsDeDocente(int idDocente)
        {
            var lista = new List<int>();
            try
            {
                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                using var cmd = new SqlCommand(
                    "SELECT id_materia FROM DocenteMateria WHERE id_docente=@id", conn);
                cmd.Parameters.AddWithValue("@id", idDocente);
                conn.Open();
                using var r = cmd.ExecuteReader();
                while (r.Read()) lista.Add(Convert.ToInt32(r["id_materia"]));
            }
            catch { }
            return lista;
        }

        private static List<int> CargarCarrerasSecundariasIds(int idDocente)
        {
            var lista = new List<int>();
            try
            {
                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                using var cmd = new SqlCommand(
                    "SELECT id_carrera FROM DocenteCarrera WHERE id_docente=@id AND es_principal=0", conn);
                cmd.Parameters.AddWithValue("@id", idDocente);
                conn.Open();
                using var r = cmd.ExecuteReader();
                while (r.Read()) lista.Add(Convert.ToInt32(r["id_carrera"]));
            }
            catch { }
            return lista;
        }
        #endregion

        #region Creación de Cards de Docente
        private Border CrearCardDocente(Docente docente)
        {
            var border = new Border
            {
                Style = (Style)FindResource("DocenteCardStyle"),
                Tag = docente  // guardamos el objeto para poder editarlo
            };
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            grid.Children.Add(CrearFotoBorder());
            grid.Children.Add(CrearInformacionDocente(docente));
            grid.Children.Add(CrearBotonesAccion(docente.IdDocente));

            border.Child = grid;
            return border;
        }

        private Border CrearFotoBorder()
        {
            var border = new Border
            {
                Width = 80,
                Height = 100,
                Background = (Brush)FindResource("GuindaBajo"),
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(0, 0, 20, 0),
                Child = new TextBlock
                {
                    Text = "👤",
                    FontSize = 32,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
            Grid.SetColumn(border, 0);
            return border;
        }

        private StackPanel CrearInformacionDocente(Docente docente)
        {
            var stack = new StackPanel { VerticalAlignment = VerticalAlignment.Top };

            // Nombre + badge tipo contrato
            var header = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            header.Children.Add(new TextBlock
            {
                Text = docente.NombreCompleto,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource("GuindaBajo"),
                VerticalAlignment = VerticalAlignment.Center
            });
            if (!string.IsNullOrEmpty(docente.TipoTiempo))
                header.Children.Add(CrearBadge(docente.TipoTiempo, "#1976D2"));
            stack.Children.Add(header);

            // Badges horas: máximas + asignadas
            var filaHoras = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            filaHoras.Children.Add(CrearBadge($"Máx: {docente.HorasMaximas}h", "#455A64"));
            var colorAsignadas = docente.HorasAsignadas > docente.HorasMaximas ? "#C62828" : "#2E7D32";
            filaHoras.Children.Add(CrearBadge($"Asignadas: {docente.HorasAsignadas}h", colorAsignadas));
            stack.Children.Add(filaHoras);

            // Carreras
            stack.Children.Add(CrearLinea("Carrera principal: ", docente.CarreraPrincipal, "#7A003C"));
            if (!string.IsNullOrWhiteSpace(docente.CarreraSecundaria) && docente.CarreraSecundaria != "Ninguna")
                stack.Children.Add(CrearLinea("Secundarias: ", docente.CarreraSecundaria, "#888888"));

            // Materias
            if (!string.IsNullOrWhiteSpace(docente.Materias))
            {
                stack.Children.Add(new TextBlock
                {
                    Text = "Materias impartidas:",
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#444444")),
                    Margin = new Thickness(0, 4, 0, 2)
                });
                stack.Children.Add(new TextBlock
                {
                    Text = docente.Materias,
                    FontSize = 11,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666")),
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 6)
                });
            }

            // Tabla de horario visual por día
            if (!string.IsNullOrWhiteSpace(docente.HorarioLaboral))
                stack.Children.Add(CrearTablaHorario(docente.HorarioLaboral));

            Grid.SetColumn(stack, 1);
            return stack;
        }

        // "Lunes 7:30-8:30, Lunes 8:30-9:30, Martes 9:30-11:30" → tabla visual agrupada por día
        private static Border CrearTablaHorario(string horarioLaboral)
        {
            var sp = new StackPanel { Margin = new Thickness(0, 4, 0, 0) };
            sp.Children.Add(new TextBlock
            {
                Text = "📅 Horario de disponibilidad:",
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E65100")),
                Margin = new Thickness(0, 0, 0, 6)
            });

            // Agrupar bloques por día
            var porDia = new Dictionary<string, List<string>>();
            foreach (var bloque in horarioLaboral.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var t = bloque.Trim();
                var espacio = t.IndexOf(' ');
                if (espacio < 0) continue;
                var dia = t[..espacio];
                var rango = t[(espacio + 1)..];
                if (!porDia.ContainsKey(dia)) porDia[dia] = new List<string>();
                porDia[dia].Add(rango);
            }

            var diasOrden = new[] { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes" };
            var coloresDia = new[] { "#FFE0B2", "#C8E6C9", "#BBDEFB", "#F8BBD0", "#FFE0B2" };

            foreach (var dia in diasOrden)
            {
                if (!porDia.ContainsKey(dia)) continue;

                int diaIdx = Array.IndexOf(diasOrden, dia);
                var colorFondo = (Color)ColorConverter.ConvertFromString(coloresDia[diaIdx]);
                string horaInicio = porDia[dia].First().Split('-')[0];
                string horaFin = porDia[dia].Last().Split('-').Last();
                int totalHoras = porDia[dia].Count;

                var fila = new Border
                {
                    Background = new SolidColorBrush(colorFondo),
                    CornerRadius = new CornerRadius(4),
                    Margin = new Thickness(0, 2, 0, 2),
                    Padding = new Thickness(10, 5, 10, 5)
                };

                var rowGrid = new Grid();
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(95) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                rowGrid.Children.Add(new TextBlock
                {
                    Text = dia,
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333")),
                    VerticalAlignment = VerticalAlignment.Center
                });

                var rangoTb = new TextBlock
                {
                    Text = $"{horaInicio}  →  {horaFin}",
                    FontSize = 13,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BF360C")),
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(rangoTb, 1);
                rowGrid.Children.Add(rangoTb);

                var horasTb = new TextBlock
                {
                    Text = $"{totalHoras}h",
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E7D32")),
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(horasTb, 2);
                rowGrid.Children.Add(horasTb);

                fila.Child = rowGrid;
                sp.Children.Add(fila);
            }

            return new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF8F0")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCC80")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(10),
                Child = sp
            };
        }

        private static StackPanel CrearLinea(string etiqueta, string valor, string colorHex)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 4)
            };
            panel.Children.Add(new TextBlock
            {
                Text = etiqueta,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666"))
            });
            panel.Children.Add(new TextBlock
            {
                Text = valor,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex))
            });
            return panel;
        }

        private static Border CrearBadge(string texto, string colorHex) => new()
        {
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex)),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(8, 3, 8, 3),
            Margin = new Thickness(10, 0, 0, 0),
            Child = new TextBlock
            {
                Text = texto.ToUpperInvariant(),
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            }
        };

        private StackPanel CrearBotonesAccion(int idDocente)
        {
            var stack = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20, 0, 0, 0)
            };

            var editarBtn = new Button
            {
                Content = "Editar",
                Padding = new Thickness(15, 8, 15, 8),
                Margin = new Thickness(0, 0, 0, 8),
                Background = (Brush)FindResource("RosaOscuro"),
                Tag = idDocente
            };
            editarBtn.Click += EditarDocente_Click;

            var eliminarBtn = new Button
            {
                Content = "Eliminar",
                Padding = new Thickness(15, 8, 15, 8),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E53935")),
                Tag = idDocente
            };
            eliminarBtn.Click += EliminarDocente_Click;

            stack.Children.Add(editarBtn);
            stack.Children.Add(eliminarBtn);
            Grid.SetColumn(stack, 2);
            return stack;
        }

        private void EditarDocente_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not int idDocente) return;

            // Buscar el docente ya cargado en memoria buscando en ListaDocentes
            var card = FindDocenteCard(idDocente);
            if (card == null) return;

            // Si ya hay otro formulario abierto, cerrarlo limpio primero
            if (PanelFormularioDocente.Visibility == Visibility.Visible)
            {
                _docenteEnEdicion = null;
                LimpiarFormulario();
                CerrarFormulario();
            }

            AbrirFormularioEdicion(card);
        }

        // Busca el objeto Docente asociado a una card en ListaDocentes por su idDocente
        private Docente? FindDocenteCard(int idDocente)
        {
            foreach (UIElement el in ListaDocentes.Children)
            {
                if (el is Border b && b.Tag is Docente d && d.IdDocente == idDocente)
                    return d;
            }
            return null;
        }

        private void EliminarDocente_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not int idDocente) return;

            var resultado = MessageBox.Show(
                "¿Está seguro de eliminar este docente? Se eliminarán también sus materias y disponibilidad.",
                "Eliminar Docente", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (resultado != MessageBoxResult.Yes) return;

            try
            {
                EliminarDocenteDeBD(idDocente);
                MessageBox.Show("Docente eliminado correctamente.",
                    "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadDocentes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void EliminarDocenteDeBD(int idDocente)
        {
            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            conn.Open();
            using var tx = conn.BeginTransaction();
            try
            {
                // Borrar en cascada manual (relaciones sin CASCADE en BD)
                EjecutarDelete(conn, tx, "DELETE FROM DisponibilidadDocente WHERE id_docente=@id", idDocente);
                EjecutarDelete(conn, tx, "DELETE FROM DocenteMateria WHERE id_docente=@id", idDocente);
                EjecutarDelete(conn, tx, "DELETE FROM DocenteCarrera WHERE id_docente=@id", idDocente);
                EjecutarDelete(conn, tx, "DELETE FROM Docentes WHERE id_docente=@id", idDocente);
                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        private static void EjecutarDelete(SqlConnection conn, SqlTransaction tx, string sql, int id)
        {
            using var cmd = new SqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        // UPDATE: borra relaciones antiguas y reinserta las nuevas
        private void ActualizarDocenteEnBD(int idDocente)
        {
            var nombre = TextboxNombre.Text.Trim();
            var tipoDocente = (ComboBoxTipoTiempo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "TIEMPO COMPLETO";
            int horasMaximas = int.TryParse(TextboxHorasMaximas.Text, out int hm) ? hm : 20;

            int idCarreraPrincipal = _idCarrera;
            if (ComboBoxCarreraPrincipal.SelectedItem is ComboBoxItem cbItem && cbItem.Tag is int idTag)
                idCarreraPrincipal = idTag;

            var idCarrerasSecundarias = new List<int>();
            foreach (var cb in StackPanelCarrerasSecundarias.Children.OfType<CheckBox>())
                if (cb.IsChecked == true && cb.Tag is int idSec)
                    idCarrerasSecundarias.Add(idSec);

            var disponibilidad = ObtenerDisponibilidadSeleccionada();

            using var conn = new SqlConnection(DatabaseService.GetConnectionString());
            conn.Open();
            using var tx = conn.BeginTransaction();
            try
            {
                // Actualizar datos b\u00e1sicos
                using (var cmd = new SqlCommand(@"
                    UPDATE Docentes
                    SET nombre=@nombre, tipo_docente=@tipo, horas_maximas=@horas
                    WHERE id_docente=@id", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@nombre", nombre);
                    cmd.Parameters.AddWithValue("@tipo", tipoDocente);
                    cmd.Parameters.AddWithValue("@horas", horasMaximas);
                    cmd.Parameters.AddWithValue("@id", idDocente);
                    cmd.ExecuteNonQuery();
                }

                // Reemplazar relaciones
                EjecutarDelete(conn, tx, "DELETE FROM DisponibilidadDocente WHERE id_docente=@id", idDocente);
                EjecutarDelete(conn, tx, "DELETE FROM DocenteMateria WHERE id_docente=@id", idDocente);
                EjecutarDelete(conn, tx, "DELETE FROM DocenteCarrera WHERE id_docente=@id", idDocente);

                InsertarDocenteCarrera(conn, tx, idDocente, idCarreraPrincipal, esPrincipal: true);
                foreach (var idSec in idCarrerasSecundarias)
                    InsertarDocenteCarrera(conn, tx, idDocente, idSec, esPrincipal: false);
                foreach (var idMateria in _materiasSeleccionadasIds.Distinct())
                    InsertarDocenteMateria(conn, tx, idDocente, idMateria);
                foreach (var (idDia, idBloque) in disponibilidad)
                    InsertarDisponibilidad(conn, tx, idDocente, idDia, idBloque);

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
        #endregion

        #region Utilidades
        private void LimpiarControles()
        {
            ComboBoxCarreraPrincipal.Items.Clear();
            StackPanelCarrerasSecundarias.Children.Clear();
            StackPanelMateriasImpartidas.Children.Clear();
            _carrerasDisponibles.Clear();
            _secondaryPanels.Clear();
            _mainMateriasPanel = null;
            _todasLasMaterias.Clear();
        }

        private void LimpiarFormulario()
        {
            TextboxNombre.Clear();
            TextboxHorasMaximas.Text = "20";
            ComboBoxCarreraPrincipal.SelectedIndex = -1;
            ComboBoxTipoTiempo.SelectedIndex = 0;
            _horasAsignadas = 0;
            TextboxHorasDiarias.Text = "0";
            TextboxHorasDiarias.Background =
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F5F5"));

            foreach (var cb in StackPanelCarrerasSecundarias.Children.OfType<CheckBox>())
                cb.IsChecked = false;

            ClearMateriasPanels();
        }

        private void CerrarFormulario()
        {
            PanelFormularioDocente.Visibility = Visibility.Collapsed;
            BotonAgregarDocente.Content = "+ Agregar Docente";
            BotonGuardarDocente.Content = "Guardar";
        }
        #endregion
    }
}
