using UOP.WinTray.UI.ViewModels.CADfx.Shared;
using System.Windows;
using System.Windows.Controls;

namespace UOP.WinTray.UI.Views.UserControls.CADfx.Shared
{
    /// <summary>
    /// Interaction logic for CancelAcceptView.xaml
    /// </summary>
    public partial class CancelAcceptView : UserControl
    {
        public CancelAcceptView()
        {
            InitializeComponent();
        }

        private ICancelAcceptHandler GetCancelAcceptHandler()
        {
            if (DataContext == null)
            {
                return null;
            }
            else
            {
                return DataContext as ICancelAcceptHandler;
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            GetCancelAcceptHandler()?.ButtonPushed(CancelAcceptButton.CANCEL);
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            GetCancelAcceptHandler()?.ButtonPushed(CancelAcceptButton.ACCEPT);
        }
    }
}
