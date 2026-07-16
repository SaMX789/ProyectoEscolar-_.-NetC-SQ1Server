using GestorHorarios.Services;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Windows.Controls;

namespace GestorHorarios.MATERIAS
{
    /// <summary>
    /// Lógica de interacción para MateriasView.xaml
    /// </summary>
    public partial class MateriasView : UserControl
    {
        public MateriasView()
        {
            InitializeComponent();
            CargarConteos();
        }

        private void SeleccionarCarrera_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag != null)
            {
                int idCarrera = int.Parse(border.Tag.ToString()!);
                NavigationService.GetFromWindow(this)?.NavigateTo(new IngenieriaSeleccionM(idCarrera));
            }
        }

        private void CargarConteos()
        {
            string conexion = DatabaseService.GetConnectionString();

            using (SqlConnection conn = new SqlConnection(conexion))
            {
                SqlCommand cmd = new SqlCommand("sp_ContarMateriasPorCarrera", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                conn.Open();
                using SqlDataReader reader = cmd.ExecuteReader();

                TextBlock[] misTextBlocks = { CantidadMateriasSistemas, CantidadMateriasCivil, CantidadMateriasComunitario, CantidadMateriasEmpresarial, CantidadMateriasIndustrial, CantidadMateriasBioquimica, CantidadMateriasIngles, };

                int i = 0;
                while (reader.Read() && i < misTextBlocks.Length)
                {
                    misTextBlocks[i].Text = reader["TotalMaterias"].ToString();
                    i++;
                }
            }
        }
    }
}
