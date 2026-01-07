using UOP.WinTray.UI.ViewModels;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Web.Management;

namespace UOP.WinTray.UI.Views
{
    /// <summary>
    /// Interaction logic for EditDeckProperties.xaml
    /// </summary>
    [ExcludeFromCodeCoverage]
    public partial class Edit_MDDeck_View : Window
    {
        public Edit_MDDeck_View()
        {
            InitializeComponent();
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
