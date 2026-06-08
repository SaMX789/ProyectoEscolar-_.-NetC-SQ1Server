using System.Windows;
using System.Windows.Controls;

namespace GestorHorarios.Services
{
    public class NavigationService : INavigationService
    {
        private readonly ContentControl _contentControl;

        public NavigationService(ContentControl contentControl)
        {
            _contentControl = contentControl;
        }

        public void NavigateTo(UserControl view)
        {
            _contentControl.Content = view;
        }

        public static INavigationService? GetFromWindow(DependencyObject element)
        {
            if (Window.GetWindow(element) is MainWindow mainWindow)
            {
                return mainWindow.Navigation;
            }
            return null;
        }
    }
}
