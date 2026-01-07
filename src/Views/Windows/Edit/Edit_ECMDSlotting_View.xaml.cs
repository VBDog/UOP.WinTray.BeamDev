using UOP.WinTray.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace UOP.WinTray.UI.Views.Windows
{
    /// <summary>
    /// Interaction logic for Slotting.xaml
    /// </summary>
    public partial class Edit_ECMDSlotting_View : Window
    {

        public Edit_ECMDSlotting_View()
        {
            InitializeComponent();


        }

        public Edit_ECMDSlotting_View(Edit_ECMDSlotting_ViewModel slottingViewModel)
        {
            InitializeComponent();


        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var VM = (Edit_ECMDSlotting_ViewModel)this.DataContext;
            if (!VM.IsEnabled)
            {
                e.Cancel = true;
                return;
            }
            VM.Window_Closing(sender, e);
        }
        private async void pitchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            await System.Threading.Tasks.Task.Delay(60);
            await Application.Current.Dispatcher.InvokeAsync((sender as TextBox).SelectAll);
        }

        private void TargetCombo_DropDownClosed(object sender, System.EventArgs e)
        {
            Edit_ECMDSlotting_ViewModel VM = (Edit_ECMDSlotting_ViewModel)DataContext;
            VM.UpdateTargets();
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
                VM.Dispose();
            }

        }

    }
}
