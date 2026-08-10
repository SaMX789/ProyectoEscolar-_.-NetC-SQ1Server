using GestorHorarios.Services;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GestorHorarios.DOCENTES
{
    // Modelo ligero exclusivo para el buscador
    public class DocenteBusquedaDto
    {
        public int IdDocente { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public int IdCarreraPrincipal { get; set; }
        public string NombreCarreraPrincipal { get; set; } = string.Empty;
    }

    public partial class DocentesView : UserControl
    {
        public DocentesView()
        {
            InitializeComponent();
            CargarConteoDocentes();
        }

        private void CargarConteoDocentes()
        {
            try
            {
                // Los TextBlocks están en orden id_carrera 1..7
                TextBlock[] bloques = {
                    TxtDocentesSistemas, TxtDocentesCivil, TxtDocentesComunitario,
                    TxtDocentesEmpresarial, TxtDocentesIndustrial,
                    TxtDocentesBioquimica, TxtDocentesIngles
                };

                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                using var cmd = new SqlCommand("sp_ContarDocentesPorCarrera", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                conn.Open();
                using var reader = cmd.ExecuteReader();

                int i = 0;
                while (reader.Read() && i < bloques.Length)
                {
                    int total = Convert.ToInt32(reader["TotalDocentes"]);

                    // MODIFICACIÓN AQUÍ: Se eliminó la concatenación de la palabra "Docente/s"
                    bloques[i].Text = total.ToString();

                    i++;
                }
            }
            catch (Exception ex)
            {
                // Mostrar el error en pantalla para saber exactamente qué está fallando en SQL
                MessageBox.Show($"Ocurrió un error al cargar los datos:\n\n{ex.Message}",
                                "Error de Base de Datos",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);

                System.Diagnostics.Debug.WriteLine($"Error cargando conteos: {ex.Message}");
            }
        }

        private void SeleccionarCarrera_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag != null
                && int.TryParse(border.Tag.ToString(), out int idCarrera))
            {
                NavigationService.GetFromWindow(this)?.NavigateTo(new IngenieriaSeleccionD(idCarrera));
            }
        }

        #region Lógica del Buscador de Docentes
        private void TxtBuscadorDocente_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filtro = TxtBuscadorDocente.Text.Trim();

            // Solo buscamos si el usuario ha escrito al menos 2 letras
            if (filtro.Length < 2)
            {
                PopupResultados.IsOpen = false;
                return;
            }

            var resultados = new List<DocenteBusquedaDto>();

            try
            {
                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                using var cmd = new SqlCommand("sp_BuscarDocentesGlobal", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@filtro", filtro);
                conn.Open();

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    resultados.Add(new DocenteBusquedaDto
                    {
                        IdDocente = reader.GetInt32(0),
                        NombreCompleto = reader.GetString(1),
                        // Lectura segura manejando posibles nulos si un maestro no tiene carrera principal
                        IdCarreraPrincipal = reader["IdCarreraPrincipal"] != DBNull.Value ? reader.GetInt32(4) : 0,
                        NombreCarreraPrincipal = reader["NombreCarreraPrincipal"] != DBNull.Value ? reader.GetString(5) : "Sin carrera principal"
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en buscador: {ex.Message}");
            }

            // Mostrar u ocultar el Popup dependiendo de si hubo resultados
            if (resultados.Count > 0)
            {
                ListResultadosBusqueda.ItemsSource = resultados;
                PopupResultados.IsOpen = true;
            }
            else
            {
                PopupResultados.IsOpen = false;
            }
        }

        private void ListResultadosBusqueda_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListResultadosBusqueda.SelectedItem is not DocenteBusquedaDto seleccionado) return;

            // Cerramos el popup y limpiamos el texto
            PopupResultados.IsOpen = false;
            TxtBuscadorDocente.Clear();

            if (seleccionado.IdCarreraPrincipal <= 0)
            {
                MessageBox.Show("Este docente no tiene una carrera principal asignada, por lo que no se puede abrir su tarjeta directamente.",
                                "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Navegamos a la vista de la carrera específica del docente
            var navService = NavigationService.GetFromWindow(this);
            if (navService != null)
            {
                var vistaDestino = new IngenieriaSeleccionD(seleccionado.IdCarreraPrincipal);
                navService.NavigateTo(vistaDestino);
            }
        }
        #endregion
    }
}