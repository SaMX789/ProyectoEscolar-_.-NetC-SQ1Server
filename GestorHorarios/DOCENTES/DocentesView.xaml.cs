using GestorHorarios.Services;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows.Controls;
using System.Windows.Input;

namespace GestorHorarios.DOCENTES
{
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
                System.Windows.MessageBox.Show($"Ocurrió un error al cargar los datos:\n\n{ex.Message}",
                                               "Error de Base de Datos",
                                               System.Windows.MessageBoxButton.OK,
                                               System.Windows.MessageBoxImage.Error);

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
    }
}