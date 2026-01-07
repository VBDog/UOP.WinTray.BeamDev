using UOP.WinTray.UI.ViewModels.CADfx.Windows;
using System.Windows;

namespace UOP.WinTray.UI.Views.Windows.CADfx
{
    /// <summary>
    /// Interaction logic for DrawingsBatchProcessView.xaml
    /// </summary>
    public partial class DrawingsBatchProcessView : Window
    {
        public DrawingsBatchProcessView(DrawingsBatchProcessViewModel viewModel = null)
        {
            InitializeComponent();

            this.DataContext = viewModel;
        }

        private async void processButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as DrawingsBatchProcessViewModel;
            if (viewModel != null)
            {
                viewModel.ProcessButtonIsEnabled = false;
                await viewModel.ProcessInputsAsync();
                viewModel.ProcessButtonIsEnabled = true;
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as DrawingsBatchProcessViewModel;
            if (viewModel != null)
            {
                viewModel.CancelProcess(false);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var viewModel = DataContext as DrawingsBatchProcessViewModel;
            if (viewModel != null)
            {
                if (viewModel.IsBusyProcessing())
                {
                    e.Cancel = true;
                    viewModel.CancelProcess(true);
                }
            }
        }
    }

}
