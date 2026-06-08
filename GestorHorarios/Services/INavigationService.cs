using System.Windows.Controls;

namespace GestorHorarios.Services
{
    public interface INavigationService
    {
        void NavigateTo(UserControl view);
    }
}
