using System;
using System.Windows.Controls;
using System.Windows.Input;
using GestorHorarios.Repositories;
using GestorHorarios.Services;

namespace GestorHorarios.GRUPOS
{
    public partial class GruposView : UserControl
    {
        private readonly IGrupoRepository _grupoRepository;

        public GruposView(IGrupoRepository? grupoRepository = null)
        {
            InitializeComponent();
            _grupoRepository = grupoRepository ?? new GrupoRepository();
            CargarConteoGrupos();
        }

        private void CargarConteoGrupos()
        {
            try
            {
                TextBlock[] bloques =
                {
                    TxtGruposSistemas, TxtGruposCivil, TxtGruposComunitario,
                    TxtGruposEmpresarial, TxtGruposIndustrial, TxtGruposBioquimica
                };

                var conteos = _grupoRepository.ContarPorCarrera();

                for (int i = 0; i < conteos.Count && i < bloques.Length; i++)
                {
                    int total = conteos[i].TotalGrupos;
                    bloques[i].Text = $"{total} {(total == 1 ? "Grupo" : "Grupos")}";
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
