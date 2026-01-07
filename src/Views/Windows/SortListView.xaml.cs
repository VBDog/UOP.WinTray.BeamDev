using UOP.WinTray.UI.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace UOP.WinTray.UI.Views
{
    /// <summary>
    /// Interaction logic for SortListView.xaml
    /// </summary>
    public partial class SortListView : Window
    {
        public SortListView()
        {
            InitializeComponent();
        }

        private void lstOutput_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SortListViewModel VM = DataContext as SortListViewModel;
            VM.MoveLeft();
        }

        private void lstInput_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SortListViewModel VM = DataContext as SortListViewModel;
            VM.MoveRight();
        }
    }
}
