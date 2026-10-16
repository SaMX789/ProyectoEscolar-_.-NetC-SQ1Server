using System;
using System.Windows.Controls;
using GestorHorarios.Repositories;
using GestorHorarios.Services;

namespace GestorHorarios.MATERIAS
{
    /// <summary>
    /// Lógica de interacción para MateriasView.xaml
    /// </summary>
    public partial class MateriasView : UserControl
    {
        private readonly IMateriaRepository _materiaRepository;

        public MateriasView(IMateriaRepository? materiaRepository = null)
        {
            InitializeComponent();
            _materiaRepository = materiaRepository ?? new MateriaRepository();
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
            try
            {
                TextBlock[] misTextBlocks =
                {
                    CantidadMateriasSistemas, CantidadMateriasCivil, CantidadMateriasComunitario,
                    CantidadMateriasEmpresarial, CantidadMateriasIndustrial, CantidadMateriasBioquimica,
                    CantidadMateriasIngles
                };

                var conteos = _materiaRepository.ContarPorCarrera();

                for (int i = 0; i < conteos.Count && i < misTextBlocks.Length; i++)
                {
                    misTextBlocks[i].Text = conteos[i].TotalMaterias.ToString();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando conteos de materias: {ex.Message}");
            }
        }
    }
}
