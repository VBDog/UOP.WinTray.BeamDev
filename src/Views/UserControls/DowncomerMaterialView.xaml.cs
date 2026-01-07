using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;
using System.Windows.Input;

namespace UOP.WinTray.UI.Views.UserControls
{
    /// <summary>
    /// Interaction logic for DowncomerMaterialView.xaml
    /// </summary>
    [ExcludeFromCodeCoverage]
    public partial class DowncomerMaterialView : UserControl
    {
        public DowncomerMaterialView()
        {
            InitializeComponent();
        }
        private void OnKeyPressHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _ = Keyboard.Focus(OKButton);
            }
        }
    }
}
