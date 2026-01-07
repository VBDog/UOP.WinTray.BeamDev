using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UOP.WinTray.UI.Views
{
    /// <summary>
    /// Interaction logic for MDCaseView.xaml
    /// </summary>
    public partial class MDCaseView : UserControl
    {
        public MDCaseView()
        {
            InitializeComponent();
        }


        private void PropertyControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //PropertyControl pcontrol = sender as PropertyControl;
            //if (pcontrol == null) return;
            //PropertyControlViewModel pcVM = pcontrol.DataContext as PropertyControlViewModel;
            //if (pcVM != null) 
            //    pcVM.RespondToDoubleClick();
        }

        private void PropertyControl_GotFocus(object sender, RoutedEventArgs e)
        {
            //PropertyControl pcontrol = sender as PropertyControl;
            //if (pcontrol == null) return;
            //PropertyControlViewModel pcVM = pcontrol.DataContext as PropertyControlViewModel;
            //if (pcVM != null) 
            //    pcVM.RespondToGotFocus();
        }
    }
}
