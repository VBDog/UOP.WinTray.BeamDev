using UOP.WinTray.UI.ViewModels;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;

namespace UOP.WinTray.UI.Views.Windows
{
    /// <summary>
    /// Interaction logic for Edit_MDProject_View.xaml
    /// </summary>
    [ExcludeFromCodeCoverage]
    public partial class Edit_MDProject_View : Window
    {
        public Edit_MDProject_View()
        {
            InitializeComponent();
        }
        private void OnKeyPressHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && 
                ((DataContext as NewMDProjectViewModelBase).MessageBoxResult != MessageBoxResult.OK))
            {
                _ = Keyboard.Focus(OKButton);
            }
            (DataContext as NewMDProjectViewModelBase).MessageBoxResult = MessageBoxResult.None;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel_Base VM = (ViewModel_Base)DataContext;
            if (VM != null)
            {
                if (VM.Activated) return;

                VM.Activate(this);
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel_Base VM = (ViewModel_Base)DataContext;
            if (VM != null)
            {
                VM.Dispose();
            }
        }
    }
}
