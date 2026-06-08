using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GestorHorarios.Services;
using Microsoft.Data.SqlClient;

namespace GestorHorarios.GRUPOS
{
    public partial class GruposView : UserControl
    {
        public GruposView()
        {
            InitializeComponent();
            CargarConteoGrupos();
        }

        private void CargarConteoGrupos()
        {
            try
            {
                TextBlock[] bloques = {
                    TxtGruposSistemas, TxtGruposCivil, TxtGruposComunitario,
                    TxtGruposEmpresarial, TxtGruposIndustrial, TxtGruposBioquimica
                };

                using var conn = new SqlConnection(DatabaseService.GetConnectionString());
                using var cmd = new SqlCommand("sp_ContarGruposPorCarrera", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                conn.Open();
                using var reader = cmd.ExecuteReader();

                int i = 0;
                while (reader.Read() && i < bloques.Length)
                {
                    int total = Convert.ToInt32(reader["TotalGrupos"]);
                    bloques[i].Text = $"{total} {(total == 1 ? "Grupo" : "Grupos")}";
                    i++;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando conteo de grupos: {ex.Message}");
            }
        }

        private void SeleccionarCarrera_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag != null
                && int.TryParse(border.Tag.ToString(), out int idCarrera))
            {
                NavigationService.GetFromWindow(this)?.NavigateTo(new IngenieriaSeleccionG(idCarrera));
            }
        }
    }
}
