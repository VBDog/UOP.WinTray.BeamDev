using UOP.WinTray.UI.ViewModels;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace UOP.WinTray.UI.Views
{
    /// <summary>
    /// Interaction logic for EditDownComersPropertiesView.xaml
    /// </summary>
    [ExcludeFromCodeCoverage]
    public partial class Edit_MDDowncomer_View : Window
    {
        public Edit_MDDowncomer_View()
        {
            InitializeComponent();
            
        }

        private void cadfxDXFViewer_Loaded(object sender, RoutedEventArgs e)
        {
            Edit_MDDowncomer_ViewModel VM = (Edit_MDDowncomer_ViewModel)DataContext;
            VM.Viewer = cadfxDXFViewer;
        }

        private void Window_Activated(object sender, System.EventArgs e)
        {
            ViewModel_Base VM = (ViewModel_Base)DataContext;
            if (VM.Activated) return;
            VM.Viewer = cadfxDXFViewer;
            VM.Activate(this);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var VM = (Edit_MDDowncomer_ViewModel)this.DataContext;
            VM.Window_Closing(sender, e);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel_Base VM = (ViewModel_Base)DataContext;
            if (VM != null)
            {
                if (VM.Activated) return;
                VM.Viewer = cadfxDXFViewer;
                VM.Activate(this);
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel_Base VM = (ViewModel_Base)DataContext;
            if (VM != null)
            {
                VM.Viewer = null;
                VM.Dispose();
            }
        }
    }  
}
