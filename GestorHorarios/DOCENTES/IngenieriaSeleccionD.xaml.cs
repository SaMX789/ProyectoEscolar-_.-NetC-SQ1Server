using GestorHorarios.Models;
using GestorHorarios.Repositories;
using GestorHorarios.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GestorHorarios.DOCENTES
{
    public class GrupoSeleccionado
    {
        public int IdGrupo { get; set; }
        public string Nombre { get; set; } = "";
        public bool EnLinea { get; set; }
        public bool EsEspecial { get; set; } // Agregado para el cálculo dinámico
    }

    public class MateriaTablaDto : INotifyPropertyChanged
    {
        public int IdMateria { get; set; }
        public string Nombre { get; set; } = "";
        public string CarreraAsociada { get; set; } = "";
        public int Creditos { get; set; }
        public int Semestre { get; set; }
        public int IdCarrera { get; set; }

        public string NombreMostrar => $"{Nombre} ({CarreraAsociada})";

        // NUEVO: Propiedad para agrupar visualmente la tabla
        public string CicloAsociado => Semestre % 2 == 0 ? "🗓️ CICLO A" : "🗓️ CICLO B";

        private List<GrupoSeleccionado> _gruposSeleccionados = new();
        public List<GrupoSeleccionado> GruposSeleccionados
        {
            get => _gruposSeleccionados;
            set
            {
                _gruposSeleccionados = value;
                NotificarCambiosUI();
            }
        }

        public string GruposTexto => GruposSeleccionados.Count > 0
            ? string.Join(", ", GruposSeleccionados.Select(g => g.Nombre + (g.EnLinea ? " (En Línea)" : "")))
            : "Clic para seleccionar...";

        // REGRESAMOS EL CÁLCULO NORMAL PARA LA FILA
        public int HorasSemana => Creditos * GruposSeleccionados.Count;

        public event PropertyChangedEventHandler? PropertyChanged;

        public void NotificarCambiosUI()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GruposTexto)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HorasSemana)));
        }
    }
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
            "13:30", "14:30", "15:30", "16:30", "17:30", "18:30"
        };
        #endregion

        #region Campos Privados
        private readonly IDocenteRepository _docenteRepository;
        private readonly ICarreraRepository _carreraRepository;
        private readonly IMateriaRepository _materiaRepository;
        private readonly IGrupoRepository _grupoRepository;
        private int _idCarrera;
        private readonly List<Carrera> _carrerasDisponibles = new();
        private readonly Dictionary<string, HashSet<int>> _horarioSeleccionado = new();
        private readonly Dictionary<string, UIElement> _secondaryPanels = new();

        // Mapeo integral de materias para acceder a sus créditos y carreras fácilmente
        private readonly Dictionary<int, MateriaTablaDto> _todasLasMateriasInfo = new();

        // NUEVO: La lista observable que alimenta directamente al DataGrid
        private readonly ObservableCollection<MateriaTablaDto> _materiasEnTabla = new();

        // Variable para recordar a qué materia le estamos asignando grupos en el Modal
        private MateriaTablaDto? _materiaEditandoGrupos = null;

        private int _horasAsignadas = 0;
        private const int HORAS_MAXIMAS_PERMITIDAS = 60;

        private Docente? _docenteEnEdicion = null;
        private UIElement? _mainMateriasPanel;

        // ==========================================
        // MEMORIA GLOBAL EN SQL SERVER PARA EL CICLO
        // ==========================================
        private static bool _calcularCicloAGlobal = new DocenteRepository().LeerPreferenciaCicloA();

        // NUEVO: Lista en memoria para el buscador rápido
        private readonly List<Docente> _docentesActuales = new();
        #endregion

        #region Constructores
        public IngenieriaSeleccionD()
        {
            InitializeComponent();
            _docenteRepository = new DocenteRepository();
            _carreraRepository = new CarreraRepository();
            _materiaRepository = new MateriaRepository();
            _grupoRepository = new GrupoRepository();

            // Sincronizar botones desde SQL Server antes de cargar las tablas
            if (RbHFGCicloA != null && RbHFGCicloB != null)
            {
                RbHFGCicloA.IsChecked = _calcularCicloAGlobal;
                RbHFGCicloB.IsChecked = !_calcularCicloAGlobal;
            }

            // Vinculamos el DataGrid con nuestra lista observable
            DataGridMaterias.ItemsSource = _materiasEnTabla;
            // AGRUPAR DATAGRID POR CICLO
            var cv = System.Windows.Data.CollectionViewSource.GetDefaultView(_materiasEnTabla);
            cv.GroupDescriptions.Clear();
            cv.GroupDescriptions.Add(new System.Windows.Data.PropertyGroupDescription("CicloAsociado"));
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
                var resultado = _carreraRepository.ObtenerNombre(_idCarrera);
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
                RbHFGCicloA.IsChecked = _calcularCicloAGlobal;
                RbHFGCicloB.IsChecked = !_calcularCicloAGlobal;
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
            int horasFrenteGrupo = int.TryParse(TextboxHFG.Text, out int hfg) ? hfg : 0;

            int idCarreraPrincipal = _idCarrera;
            if (ComboBoxCarreraPrincipal.SelectedItem is ComboBoxItem cbItem && cbItem.Tag is int idTag)
                idCarreraPrincipal = idTag;

            var idCarrerasSecundarias = new List<int>();
            foreach (var cb in StackPanelCarrerasSecundarias.Children.OfType<CheckBox>())
            {
                if (cb.IsChecked == true && cb.Tag is int idSec)
                    idCarrerasSecundarias.Add(idSec);
            }

            var asignaciones = ConstruirAsignacionesMateriaGrupo();
            var disponibilidad = ObtenerDisponibilidadSeleccionada();

            _docenteRepository.GuardarNuevo(
                nombre,
                tipoDocente,
                horasMaximas,
                horasFrenteGrupo,
                idCarreraPrincipal,
                idCarrerasSecundarias,
                asignaciones,
                disponibilidad);
        }

        private List<(int IdMateria, int? IdGrupo, bool EnLinea)> ConstruirAsignacionesMateriaGrupo()
        {
            var asignaciones = new List<(int IdMateria, int? IdGrupo, bool EnLinea)>();
            foreach (var materia in _materiasEnTabla)
            {
                if (materia.GruposSeleccionados.Count == 0)
                {
                    asignaciones.Add((materia.IdMateria, null, false));
                }
                else
                {
                    foreach (var grupo in materia.GruposSeleccionados)
                    {
                        asignaciones.Add((materia.IdMateria, grupo.IdGrupo, grupo.EnLinea));
                    }
                }
            }
            return asignaciones;
        }

        private List<(int idDia, int idBloque)> ObtenerDisponibilidadSeleccionada()
        {
            var resultado = new List<(int, int)>();
            var diasMap = new Dictionary<string, int>
            {
                { "Lunes", 1 }, { "Martes", 2 }, { "Miércoles", 3 },
                { "Jueves", 4 }, { "Viernes", 5 }
            };
            var bloques = CargarBloques();

            foreach (var dia in _horarioSeleccionado.Keys)
            {
                if (!diasMap.TryGetValue(dia, out int idDia)) continue;

                foreach (var horaIdx in _horarioSeleccionado[dia])
                {
                    if (horaIdx < bloques.Count)
                        resultado.Add((idDia, bloques[horaIdx]));
                }
            }
            return resultado;
        }

        private List<int> CargarBloques()
        {
            try
            {
                return _docenteRepository.CargarBloquesIds();
            }
            catch
            {
                return new List<int>();
            }
        }
        #endregion

        #region Modo Edición
        private void AbrirFormularioEdicion(Docente docente)
        {
            _docenteEnEdicion = docente;

            PanelFormularioDocente.Visibility = Visibility.Visible;
            BotonAgregarDocente.Content = "Cerrar";
            BotonGuardarDocente.Content = "ACTUALIZAR";

            _idCarrera = docente.IdCarreraPrincipal;

            CargarCarrerasYMaterias();
            GenerarFilasHorario();

            RbHFGCicloA.IsChecked = _calcularCicloAGlobal;
            RbHFGCicloB.IsChecked = !_calcularCicloAGlobal;
            TextboxNombre.Text = docente.NombreCompleto;
            TextboxHorasMaximas.Text = docente.HorasMaximas.ToString();
            TextboxHFG.Text = docente.HorasFrenteGrupo.ToString();

            foreach (ComboBoxItem cbItem in ComboBoxTipoTiempo.Items)
                if (cbItem.Content?.ToString() == docente.TipoTiempo)
                { ComboBoxTipoTiempo.SelectedItem = cbItem; break; }

            foreach (var cb in StackPanelCarrerasSecundarias.Children.OfType<CheckBox>())
                if (cb.Tag is int idCb && docente.CarrerasSecundariasIds.Contains(idCb))
                    cb.IsChecked = true;

            CargarMateriasYGruposParaEdicion(docente.IdDocente);

            RestaurarHorarioEnCanvas(docente.IdDocente);

            PanelFormularioDocente.BringIntoView();
        }

        private void CargarMateriasYGruposParaEdicion(int idDocente)
        {
            _materiasEnTabla.Clear();
            var dictMaterias = new Dictionary<int, MateriaTablaDto>();

            var registros = _docenteRepository.ObtenerMateriasYGruposParaEdicion(idDocente);

            foreach (var reg in registros)
            {
                int idMat = reg.IdMateria;
                int? idGrup = reg.IdGrupo;
                bool enLinea = reg.EnLinea;
                string nombreGrup = reg.NombreGrupo;
                bool esEspecial = reg.EsEspecial;

                if (!dictMaterias.ContainsKey(idMat))
                {
                    if (_todasLasMateriasInfo.TryGetValue(idMat, out var info))
                    {
                        var nuevaMat = new MateriaTablaDto
                        {
                            IdMateria = info.IdMateria,
                            Nombre = info.Nombre,
                            CarreraAsociada = info.CarreraAsociada,
                            Creditos = info.Creditos,
                            Semestre = info.Semestre,
                            IdCarrera = info.IdCarrera,
                            GruposSeleccionados = new List<GrupoSeleccionado>()
                        };
                        dictMaterias[idMat] = nuevaMat;
                        _materiasEnTabla.Add(nuevaMat);

                        MarcarCheckboxMateriaVisual(StackPanelMateriasImpartidas.Children, idMat);
                    }
                }

                if (idGrup.HasValue && dictMaterias.TryGetValue(idMat, out var matDto))
                {
                    matDto.GruposSeleccionados.Add(new GrupoSeleccionado
                    {
                        IdGrupo = idGrup.Value,
                        Nombre = nombreGrup,
                        EnLinea = enLinea,
                        EsEspecial = esEspecial
                    });
                }
            }

            foreach (var mat in _materiasEnTabla) { mat.NotificarCambiosUI(); }
            ActualizarTotalHFG();
        }

        private void MarcarCheckboxMateriaVisual(UIElementCollection children, int idMateria)
        {
            foreach (UIElement element in children)
            {
                if (element is CheckBox cb && cb.Tag is int id && id == idMateria)
                {
                    cb.Checked -= MateriaCb_Changed;
                    cb.IsChecked = true;
                    cb.Checked += MateriaCb_Changed;
                }
                else if (element is Panel panel) MarcarCheckboxMateriaVisual(panel.Children, idMateria);
                else if (element is Border border && border.Child is Panel childPanel) MarcarCheckboxMateriaVisual(childPanel.Children, idMateria);
            }
        }

        private void RestaurarHorarioEnCanvas(int idDocente)
        {
            var diasMap = new Dictionary<int, string>
            {
                { 1, "Lunes" }, { 2, "Martes" }, { 3, "Miércoles" },
                { 4, "Jueves" }, { 5, "Viernes" }
            };

            var bloquesOrdenados = CargarBloques();
            var bloqueAIndice = new Dictionary<int, int>();
            for (int i = 0; i < bloquesOrdenados.Count; i++)
                bloqueAIndice[bloquesOrdenados[i]] = i;

            List<(int IdDia, int IdBloque)> disponibilidad;
            try
            {
                disponibilidad = _docenteRepository.ObtenerDisponibilidad(idDocente);
            }
            catch
            {
                return;
            }

            if (disponibilidad.Count == 0) return;

            var porDia = disponibilidad.GroupBy(x => x.IdDia).Where(g => diasMap.ContainsKey(g.Key));

            foreach (var grupo in porDia)
            {
                var dia = diasMap[grupo.Key];
                var indices = grupo
                    .Where(x => bloqueAIndice.ContainsKey(x.IdBloque))
                    .Select(x => bloqueAIndice[x.IdBloque]);

                _horarioSeleccionado[dia] = new HashSet<int>(indices);

                int diaIdx = Array.IndexOf(DIAS, dia);
                if (diaIdx >= 0)
                    ActualizarHorario(dia, COLORES_DIAS[diaIdx]);
            }

            ActualizarContadorHoras();
        }

        private void ActualizarDocenteEnBD(int idDocente)
        {
            var nombre = TextboxNombre.Text.Trim();
            var tipoDocente = (ComboBoxTipoTiempo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "TIEMPO COMPLETO";
            int horasMaximas = int.TryParse(TextboxHorasMaximas.Text, out int hm) ? hm : 20;
            int horasFrenteGrupo = int.TryParse(TextboxHFG.Text, out int hfg) ? hfg : 0;

            int idCarreraPrincipal = _idCarrera;
            if (ComboBoxCarreraPrincipal.SelectedItem is ComboBoxItem cbItem && cbItem.Tag is int idTag)
                idCarreraPrincipal = idTag;

            var idCarrerasSecundarias = new List<int>();
            foreach (var cb in StackPanelCarrerasSecundarias.Children.OfType<CheckBox>())
                if (cb.IsChecked == true && cb.Tag is int idSec)
                    idCarrerasSecundarias.Add(idSec);

            var asignaciones = ConstruirAsignacionesMateriaGrupo();
            var disponibilidad = ObtenerDisponibilidadSeleccionada();

            _docenteRepository.Actualizar(
                idDocente,
                nombre,
                tipoDocente,
                horasMaximas,
                horasFrenteGrupo,
                idCarreraPrincipal,
                idCarrerasSecundarias,
                asignaciones,
                disponibilidad);
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

            if (!int.TryParse(TextboxHFG.Text, out int hfg) || hfg < 0)
            {
                MostrarAdvertencia("Por favor, ingrese un número válido para las Horas Frente a Grupo (HFG).");
                return false;
            }

            if (hfg > hm)
            {
                MostrarAdvertencia("Las Horas Frente a Grupo no pueden ser mayores a las horas máximas del contrato.");
                return false;
            }

            if (_horasAsignadas < hfg)
            {
                MostrarAdvertencia($"Has marcado {_horasAsignadas}h de disponibilidad en el calendario, pero el maestro tiene {hfg}h Frente a Grupo. Debes pintar al menos {hfg}h disponibles.");
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
                var carreras = _carreraRepository.ObtenerTodas();
                foreach (var carrera in carreras)
                {
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
                MessageBox.Show($"Error cargando carreras: {ex.Message}");
            }

            ComboBoxCarreraPrincipal.SelectionChanged -= ComboBoxCarreraPrincipal_SelectionChanged;
            ComboBoxCarreraPrincipal.SelectionChanged += ComboBoxCarreraPrincipal_SelectionChanged;

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
                IsEnabled = true
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
            var mainBorder = new Border
            {
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DDDDDD")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(0, 10, 0, 15),
                Tag = idCarrera
            };

            var mainStack = new StackPanel();

            var headerBorder = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isMain ? "#FCE4EC" : "#F5F5F5")),
                CornerRadius = new CornerRadius(7, 7, 0, 0),
                Padding = new Thickness(15, 10, 15, 10),
                Cursor = Cursors.Hand
            };

            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var titleText = new TextBlock
            {
                Text = $"{(isMain ? "Principal" : "Secundaria")}: {nombreCarrera}",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource("GuindaBajo"),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(titleText, 0);

            var toggleIcon = new TextBlock
            {
                Text = isMain ? "▼" : "►",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource("GuindaBajo"),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(toggleIcon, 1);

            headerGrid.Children.Add(titleText);
            headerGrid.Children.Add(toggleIcon);
            headerBorder.Child = headerGrid;
            mainStack.Children.Add(headerBorder);

            var contentStack = new StackPanel
            {
                Margin = new Thickness(15),
                Visibility = isMain ? Visibility.Visible : Visibility.Collapsed
            };

            headerBorder.MouseLeftButtonDown += (s, e) =>
            {
                if (contentStack.Visibility == Visibility.Visible)
                {
                    contentStack.Visibility = Visibility.Collapsed;
                    toggleIcon.Text = "►";
                    headerBorder.CornerRadius = new CornerRadius(7);
                }
                else
                {
                    contentStack.Visibility = Visibility.Visible;
                    toggleIcon.Text = "▼";
                    headerBorder.CornerRadius = new CornerRadius(7, 7, 0, 0);
                }
            };

            try
            {
                var materias = CargarMateriasDeBD(idCarrera);

                // SEPARAMOS LAS MATERIAS EN DOS LISTAS
                var materiasCicloB = materias.Where(m => m.Semestre % 2 != 0).ToList();
                var materiasCicloA = materias.Where(m => m.Semestre % 2 == 0).ToList();

                // FUNCIÓN INTERNA PARA DIBUJAR UN CICLO COMPLETO SIN DESALINEAR LOS BOTONES
                Action<string, string, string, List<Materia>> ConstruirCiclo = (titulo, bgHex, fgHex, listaMaterias) =>
                {
                    if (!listaMaterias.Any()) return;

                    // 1. Dibujar el Encabezado del Ciclo
                    var headerCiclo = new Border
                    {
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bgHex)),
                        CornerRadius = new CornerRadius(6),
                        Padding = new Thickness(12, 8, 12, 8),
                        Margin = new Thickness(0, 10, 0, 10),
                        Child = new TextBlock
                        {
                            Text = titulo,
                            FontSize = 14,
                            FontWeight = FontWeights.Bold,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fgHex))
                        }
                    };
                    contentStack.Children.Add(headerCiclo);

                    // 2. Agrupar las materias de este ciclo por semestre y dibujarlas (ESTRUCTURA ORIGINAL INTACTA)
                    var porSemestre = listaMaterias.GroupBy(m => m.Semestre).OrderBy(g => g.Key);

                    foreach (var grupo in porSemestre)
                    {
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

                            // AQUI SE GUARDA LA INFO COMPLETA PARA EL DATAGRID
                            _todasLasMateriasInfo[materia.IdMateria] = new MateriaTablaDto
                            {
                                IdMateria = materia.IdMateria,
                                Nombre = materia.Nombre,
                                CarreraAsociada = nombreCarrera,
                                Creditos = materia.Creditos,
                                Semestre = materia.Semestre,
                                IdCarrera = idCarrera
                            };
                        }

                        semesterStack.Children.Add(panelMaterias);
                        semesterBorder.Child = semesterStack;
                        contentStack.Children.Add(semesterBorder);
                    }
                };

                // DIBUJAMOS LOS DOS CICLOS (B va primero por orden de semestres impares)
                ConstruirCiclo("🗓️ CICLO B ", "#E8F5E9", "#2E7D32", materiasCicloB);
                ConstruirCiclo("🗓️ CICLO A ", "#E3F2FD", "#1565C0", materiasCicloA);

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

        private List<Materia> CargarMateriasDeBD(int idCarrera)
        {
            return _materiaRepository.ObtenerPorCarrera(idCarrera);
        }

        private void AddSecondaryPanel(Carrera carrera)
        {
            if (_secondaryPanels.ContainsKey(carrera.IdCarrera.ToString())) return;

            var panel = BuildMateriasPanelForCareer(carrera.IdCarrera, carrera.Nombre, isMain: false);
            _secondaryPanels[carrera.IdCarrera.ToString()] = panel;
            StackPanelMateriasImpartidas.Children.Add(panel);
        }

        private void RemoveSecondaryPanel(int idCarrera)
        {
            var key = idCarrera.ToString();
            if (!_secondaryPanels.TryGetValue(key, out var panel)) return;

            StackPanelMateriasImpartidas.Children.Remove(panel);
            _secondaryPanels.Remove(key);

            // Eliminar de la tabla las materias que pertenecian a esta carrera secundaria
            var materiasABorrar = _materiasEnTabla.Where(m => m.IdCarrera == idCarrera).ToList();
            foreach (var mat in materiasABorrar)
            {
                _materiasEnTabla.Remove(mat);
            }
            ActualizarTotalHFG();
        }

        private void ClearMateriasPanels()
        {
            StackPanelMateriasImpartidas.Children.Clear();
            _secondaryPanels.Clear();
            _mainMateriasPanel = null;
            _materiasEnTabla.Clear();
            ActualizarTotalHFG();
        }
        #endregion

        #region Lógica del DataGrid y HFG (FASE 3)
        private void MateriaCb_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is not CheckBox cb || cb.Tag is not int idMateria) return;

            if (cb.IsChecked == true)
            {
                if (_todasLasMateriasInfo.TryGetValue(idMateria, out var info) && !_materiasEnTabla.Contains(info))
                {
                    _materiasEnTabla.Add(info);
                }
            }
            else
            {
                var item = _materiasEnTabla.FirstOrDefault(m => m.IdMateria == idMateria);
                if (item != null)
                {
                    // Al quitarla, limpiamos sus grupos para que no queden fantasmas en el contador
                    item.GruposSeleccionados.Clear();
                    item.NotificarCambiosUI();
                    _materiasEnTabla.Remove(item);
                }
            }
            ActualizarTotalHFG();
        }

        private void FiltroHFG_Changed(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded || RbHFGCicloA == null) return;

            // Actualizamos la variable y la guardamos en SQL Server
            _calcularCicloAGlobal = RbHFGCicloA.IsChecked == true;
            _docenteRepository.GuardarPreferenciaCiclo(_calcularCicloAGlobal);

            ActualizarTotalHFG();
        }

        private void ActualizarTotalHFG()
        {
            if (RbHFGCicloA == null) return;

            int totalHFG = 0;

            foreach (var materia in _materiasEnTabla)
            {
                bool esSemestrePar = materia.Semestre % 2 == 0;

                foreach (var g in materia.GruposSeleccionados)
                {
                    // Utilizamos la variable global _calcularCicloAGlobal para la validación
                    if (g.EsEspecial || (_calcularCicloAGlobal && esSemestrePar) || (!_calcularCicloAGlobal && !esSemestrePar))
                    {
                        totalHFG += materia.Creditos;
                    }
                }
            }

            TextboxHFG.Text = totalHFG.ToString();
        }
        #endregion

        #region Modal de Selección de Grupos (FASE 3)
        private void AbrirModalGrupos_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is MateriaTablaDto materia)
            {
                _materiaEditandoGrupos = materia;
                TxtModalMateriaNombre.Text = materia.NombreMostrar;
                PanelCheckboxesGrupos.Children.Clear();

                try
                {
                    var gruposDisponibles = _grupoRepository.ObtenerGruposParaMateria(
                        materia.IdCarrera, materia.Semestre, materia.IdMateria);

                    bool hayGrupos = false;
                    int idDocenteActual = _docenteEnEdicion != null ? _docenteEnEdicion.IdDocente : -1;

                    foreach (var g in gruposDisponibles)
                    {
                        hayGrupos = true;
                        int idGrupo = g.IdGrupo;
                        string nombreGrupo = g.NombreGrupo;
                        string turno = g.Turno;
                        bool esEspecial = g.EsEspecial;
                        string textoEspecial = esEspecial ? " [ESPECIAL]" : "";

                        int? idDocenteAsignado = g.IdDocenteAsignado;
                        string nombreDocenteAsignado = g.NombreDocenteAsignado;

                        bool ocupadoPorOtro = idDocenteAsignado.HasValue && idDocenteAsignado.Value != idDocenteActual;

                        var tbPresencial = new TextBlock { Text = $"{nombreGrupo} ({turno}){textoEspecial} — Presencial" };
                        var tbLinea = new TextBlock { Text = $"{nombreGrupo} ({turno}){textoEspecial} — En Línea" };

                        if (ocupadoPorOtro)
                        {
                            string textoOcupado = $" (Asignado a: {nombreDocenteAsignado})";

                            tbPresencial.Text += textoOcupado;
                            tbPresencial.TextDecorations = TextDecorations.Strikethrough;
                            tbPresencial.Foreground = Brushes.Gray;

                            tbLinea.Text += textoOcupado;
                            tbLinea.TextDecorations = TextDecorations.Strikethrough;
                            tbLinea.Foreground = Brushes.Gray;
                        }
                        else
                        {
                            tbLinea.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E65100"));
                        }

                        var cbPresencial = new CheckBox
                        {
                            Content = tbPresencial,
                            Tag = new GrupoSeleccionado { IdGrupo = idGrupo, Nombre = nombreGrupo, EnLinea = false, EsEspecial = esEspecial },
                            Margin = new Thickness(0, 0, 0, 5),
                            FontSize = 14,
                            Cursor = Cursors.Hand,
                            IsEnabled = !ocupadoPorOtro
                        };

                        var cbLinea = new CheckBox
                        {
                            Content = tbLinea,
                            Tag = new GrupoSeleccionado { IdGrupo = idGrupo, Nombre = nombreGrupo, EnLinea = true, EsEspecial = esEspecial },
                            Margin = new Thickness(0, 0, 0, 15),
                            FontSize = 14,
                            Cursor = Cursors.Hand,
                            IsEnabled = !ocupadoPorOtro
                        };

                        var grupoAsignado = materia.GruposSeleccionados.FirstOrDefault(gr => gr.IdGrupo == idGrupo);
                        if (grupoAsignado != null)
                        {
                            if (grupoAsignado.EnLinea) cbLinea.IsChecked = true;
                            else cbPresencial.IsChecked = true;
                        }

                        PanelCheckboxesGrupos.Children.Add(cbPresencial);
                        PanelCheckboxesGrupos.Children.Add(cbLinea);
                    }

                    if (!hayGrupos)
                    {
                        PanelCheckboxesGrupos.Children.Add(new TextBlock
                        {
                            Text = "No hay grupos registrados para este semestre y carrera en el sistema.",
                            Foreground = Brushes.Gray,
                            FontStyle = FontStyles.Italic
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar grupos: {ex.Message}");
                }

                ModalSeleccionGrupos.Visibility = Visibility.Visible;
            }
        }

        private void AceptarGrupos_Click(object sender, RoutedEventArgs e)
        {
            if (_materiaEditandoGrupos != null)
            {
                var nuevosGrupos = new List<GrupoSeleccionado>();

                foreach (var child in PanelCheckboxesGrupos.Children)
                {
                    if (child is CheckBox cb && cb.IsChecked == true && cb.Tag is GrupoSeleccionado grupo)
                    {
                        nuevosGrupos.Add(grupo);
                    }
                }

                _materiaEditandoGrupos.GruposSeleccionados = nuevosGrupos;
                ActualizarTotalHFG();
            }

            ModalSeleccionGrupos.Visibility = Visibility.Collapsed;
            _materiaEditandoGrupos = null;
        }

        private void CancelarGrupos_Click(object sender, RoutedEventArgs e)
        {
            ModalSeleccionGrupos.Visibility = Visibility.Collapsed;
            _materiaEditandoGrupos = null;
        }
        #endregion

        #region Generación de Horarios (Canvas)
        private void GenerarFilasHorario()
        {
            CanvasHorario.Children.Clear();
            _horarioSeleccionado.Clear();

            // Le sumamos 30 píxeles extra al ancho total para que el último texto ("19:30") no se corte
            double canvasWidth = LABEL_WIDTH + (HORAS_DISPONIBLES.Length * (RECT_WIDTH + SPACING)) + 30;
            CanvasHorario.Width = canvasWidth;

            GenerarEncabezadoHoras();
            GenerarFilasDias();
        }

        private void GenerarEncabezadoHoras()
        {
            double xPos = LABEL_WIDTH + SPACING;

            // 1. Dibujamos las 12 horas iniciales (7:30 a 18:30)
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

                // Avanzamos la posición X para el siguiente cuadro
                xPos += RECT_WIDTH + SPACING;
            }

            // 2. Al salir del ciclo, xPos está exactamente en el borde derecho del último cuadro.
            // Dibujamos manualmente la etiqueta "19:30" ahí para indicar la hora de salida.
            var headerFinal = new TextBlock
            {
                Text = "19:30",
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource("GuindaBajo"),
                TextAlignment = TextAlignment.Center
            };

            // Le restamos 5 píxeles a xPos para que el texto quede centrado visualmente sobre la línea final
            Canvas.SetLeft(headerFinal, xPos - 5);
            Canvas.SetTop(headerFinal, 5);
            CanvasHorario.Children.Add(headerFinal);
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
            // Si el día no existe en el diccionario, lo creamos
            if (!_horarioSeleccionado.ContainsKey(dia))
                _horarioSeleccionado[dia] = new HashSet<int>();

            var celdasDia = _horarioSeleccionado[dia];
            bool modoEspacios = CheckBoxModoEspacios.IsChecked == true;

            if (modoEspacios)
            {
                // ==========================================
                // MODO ESPACIOS: Pinta/Despinta individualmente
                // ==========================================
                if (celdasDia.Contains(horaIdx))
                {
                    celdasDia.Remove(horaIdx);
                }
                else
                {
                    if (_horasAsignadas >= HORAS_MAXIMAS_PERMITIDAS)
                    {
                        MostrarAdvertencia($"El límite máximo configurado es {HORAS_MAXIMAS_PERMITIDAS}h.");
                        return;
                    }
                    celdasDia.Add(horaIdx);
                }
            }
            else
            {
                // ==========================================
                // MODO RANGO: Estira desde el inicio al fin (Comportamiento Original)
                // ==========================================
                if (celdasDia.Count == 0)
                {
                    if (_horasAsignadas >= HORAS_MAXIMAS_PERMITIDAS)
                    {
                        MostrarAdvertencia($"El límite máximo configurado es {HORAS_MAXIMAS_PERMITIDAS}h.");
                        return;
                    }
                    celdasDia.Add(horaIdx);
                }
                else
                {
                    int min = celdasDia.Min();
                    int max = celdasDia.Max();

                    // Si da clic adentro del rango que ya está pintado, lo ignoramos (igual que el original)
                    if (horaIdx >= min && horaIdx <= max) return;

                    int nuevoMin = Math.Min(min, horaIdx);
                    int nuevoMax = Math.Max(max, horaIdx);

                    // Calculamos cuántas horas EXTRA se van a pintar
                    int horasNuevas = (nuevoMax - nuevoMin + 1) - celdasDia.Count;

                    if (_horasAsignadas + horasNuevas > HORAS_MAXIMAS_PERMITIDAS)
                    {
                        MostrarAdvertencia($"No puedes estirar el rango porque superarías las {HORAS_MAXIMAS_PERMITIDAS}h semanales permitidas.");
                        return;
                    }

                    // Rellenamos todos los huecos entre el mínimo y el máximo automáticamente
                    for (int i = nuevoMin; i <= nuevoMax; i++)
                    {
                        celdasDia.Add(i);
                    }
                }
            }

            ActualizarHorario(dia, colorDia);
            ActualizarContadorHoras();
        }

        private void Rect_RightClick(string dia, Color colorDia)
        {
            if (!_horarioSeleccionado.ContainsKey(dia) || _horarioSeleccionado[dia].Count == 0) return;

            // Extraemos los extremos para armar un mensaje bonito
            int min = _horarioSeleccionado[dia].Min();
            int max = _horarioSeleccionado[dia].Max();

            string horaIni = HORAS_DISPONIBLES[min];
            string horaFin = max == 11 ? "19:30" : HORAS_DISPONIBLES[max + 1];
            int horasTotales = _horarioSeleccionado[dia].Count;

            var res = MessageBox.Show(
                $"¿Eliminar la asignación del día {dia}?\n" +
                $"Rango afectado: {horaIni} – {horaFin} ({horasTotales}h asignadas).\n\n" +
                $"Esta acción solo afecta al formulario, no se guarda hasta presionar Guardar/Actualizar.",
                $"Limpiar {dia}",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (res != MessageBoxResult.Yes) return;

            _horarioSeleccionado[dia].Clear();
            ActualizarHorario(dia, colorDia);
            ActualizarContadorHoras();
        }

        // Valida que el nuevo rango no supere el límite absoluto de 40h.
        // El límite del TextBox (horas máximas del docente) se valida solo al guardar,
        // para que bajar el TextBox no bloquee editar el Canvas.


        // Recalcula el total de horas asignadas en el Canvas y actualiza el TextBox
        private void ActualizarContadorHoras()
        {
            // Suma la cantidad total de cuadritos pintados en todos los días
            _horasAsignadas = _horarioSeleccionado.Sum(kv => kv.Value.Count);
            TextboxHorasDiarias.Text = _horasAsignadas.ToString();

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

            // Verifica si el cuadrito específico está en nuestra lista para pintarlo
            rect.Fill = _horarioSeleccionado.ContainsKey(dia) && _horarioSeleccionado[dia].Contains(horaIdx)
                ? new SolidColorBrush(colorDia)
                : Brushes.White;
        }
        #endregion

        #region Carga de Docentes desde BD
        private void LoadDocentes()
        {
            ListaDocentes.Children.Clear();
            _docentesActuales.Clear();

            try
            {
                var docentes = _docenteRepository.ObtenerPorCarrera(_idCarrera);
                if (docentes.Count == 0)
                {
                    MostrarMensajeSinDatos();
                    return;
                }

                foreach (var docente in docentes)
                {
                    _docentesActuales.Add(docente);
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

            // Badges horas: máximas + asignadas + HFG (¡NUEVO!)
            var filaHoras = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };

            filaHoras.Children.Add(CrearBadge($"Máx: {docente.HorasMaximas}h", "#455A64"));

            var colorAsignadas = docente.HorasAsignadas > docente.HorasMaximas ? "#C62828" : "#2E7D32";
            filaHoras.Children.Add(CrearBadge($"Asignadas: {docente.HorasAsignadas}h", colorAsignadas));

            // Aquí agregamos el Badge de HFG en color Azul Claro
            filaHoras.Children.Add(CrearBadge($"HFG: {docente.HorasFrenteGrupo}h", "#0288D1"));

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

            var diasOrden = new[] { "Lunes", "Martes", "Miercoles", "Jueves", "Viernes" };
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

                string nombreVisual = dia == "Miercoles" ? "Miércoles" : dia;

                rowGrid.Children.Add(new TextBlock
                {
                    Text = nombreVisual,
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
                _docenteRepository.Eliminar(idDocente);
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
            _materiasEnTabla.Clear();
            _todasLasMateriasInfo.Clear();
        }

        private void LimpiarFormulario()
        {
            TextboxNombre.Clear();
            TextboxHorasMaximas.Text = "20";
            TextboxHFG.Text = "0";
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

        #region Buscador Dinámico
        private void TxtBuscarDocente_TextChanged(object sender, TextChangedEventArgs e)
        {
            PlaceholderBuscar.Visibility = string.IsNullOrWhiteSpace(TxtBuscarDocente.Text) ? Visibility.Visible : Visibility.Collapsed;

            string query = TxtBuscarDocente.Text.ToLower().Trim();
            if (string.IsNullOrEmpty(query))
            {
                PopupBuscador.IsOpen = false;
                return;
            }

            var resultados = _docentesActuales
                .Where(d => d.NombreCompleto.ToLower().Contains(query))
                .ToList();

            if (resultados.Any())
            {
                ListaResultadosBusqueda.ItemsSource = resultados;
                PopupBuscador.IsOpen = true;
            }
            else
            {
                PopupBuscador.IsOpen = false;
            }
        }

        private async void ListaResultadosBusqueda_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListaResultadosBusqueda.SelectedItem is Docente docenteSeleccionado)
            {
                PopupBuscador.IsOpen = false;
                TxtBuscarDocente.Text = ""; // Limpiamos la caja

                // Buscar la tarjeta física en la UI
                foreach (UIElement el in ListaDocentes.Children)
                {
                    if (el is Border card && card.Tag is Docente d && d.IdDocente == docenteSeleccionado.IdDocente)
                    {
                        // Scroll automático hacia la tarjeta
                        card.BringIntoView();

                        // Efecto visual: Resaltar en naranja por 2 segundos
                        var originalEffect = card.Effect;
                        card.Effect = new System.Windows.Media.Effects.DropShadowEffect
                        {
                            Color = Colors.DarkOrange,
                            ShadowDepth = 0,
                            BlurRadius = 25,
                            Opacity = 1
                        };

                        await System.Threading.Tasks.Task.Delay(2000);

                        // Regresar a la sombra normal
                        card.Effect = originalEffect;
                        break;
                    }
                }
            }
            ListaResultadosBusqueda.SelectedItem = null;
        }
        #endregion
    }
}
