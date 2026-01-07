using UOP.WinTray.Projects.Enums;
using UOP.WinTray.UI.ViewModels;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.UI.Views
{
    /// <summary>
    /// Interaction logic for TraySectionDataInputCtrl.xaml
    /// </summary>
    [ExcludeFromCodeCoverage]
    public partial class Edit_MDRange_View : Window
    {
        public Edit_MDRange_View()
        {
            InitializeComponent();
            //Edit_MDRange_ViewModel VM = DataContext as Edit_MDRange_ViewModel;

        }

        private void cmbTrayTypes_Loaded(object sender, RoutedEventArgs e)
        {
            var combo = sender as ComboBox;
            combo.ItemsSource = uopEnumHelper.GetDescriptions(typeof(uppMDDesigns), SkipNegatives: true);
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

        private void ShellID_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}

