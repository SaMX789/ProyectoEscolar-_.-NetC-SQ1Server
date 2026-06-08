using System.Windows;
using System.Windows.Controls;
using GestorHorarios.Services;

namespace GestorHorarios.SALONES
{
    public partial class IngenieriaSeleccionS : UserControl
    {
        public IngenieriaSeleccionS()
        {
            InitializeComponent();
        }

        private void VolverCarreras_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GetFromWindow(this)?.NavigateTo(new SalonesView());
        }

        private void AgregarSalon_Click(object sender, RoutedEventArgs e) { }
        private void EditarSalon_Click(object sender, RoutedEventArgs e) { }
        private void EliminarSalon_Click(object sender, RoutedEventArgs e) { }
    }
}
