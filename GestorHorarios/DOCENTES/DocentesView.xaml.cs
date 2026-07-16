using GestorHorarios.Services;
using Microsoft.Data.SqlClient;
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
                    bloques[i].Text = $"{total} {(total == 1 ? "Docente" : "Docentes")}";
                    i++;
                }
            }
            catch (Exception ex)
            {
                // Si falla la BD los conteos quedan en "..."
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