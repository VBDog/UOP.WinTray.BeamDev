using UOP.WinTray.UI.ViewModels;
using System;
using System.Windows;

namespace UOP.WinTray.UI.Views
{
    /// <summary>
    /// Interaction logic for Edit_PropertyControlContainer_View.xaml
    /// </summary>
    public partial class Edit_PropertyControlContainer_View : Window
    {
        public Edit_PropertyControlContainer_View()
        {
            InitializeComponent();
        }

        public void Window_Activated(object sender, EventArgs e)
        {
            ViewModel_Base VM = (ViewModel_Base)DataContext;
            if (VM.Activated) return;

            VM.Activate(this);
        }
    }
}
