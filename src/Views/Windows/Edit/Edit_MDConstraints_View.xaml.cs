using UOP.WinTray.UI.ViewModels;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace UOP.WinTray.UI.Views
{
    /// <summary>
    /// Interaction logic for Edit_MDConstraints_View.xaml
    /// </summary>
    [ExcludeFromCodeCoverage]
    public partial class Edit_MDConstraints_View : Window
    {
        public Edit_MDConstraints_View()
        {
          InitializeComponent();
        }

        private void Got_Focus(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).SelectAll();
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
