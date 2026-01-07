using UOP.WinTray.UI.ViewModels;
using System;
using System.Windows;

namespace UOP.WinTray.UI.Views
{
    /// <summary>
    /// Interaction logic for Edit_Case_View.xaml
    /// </summary>
    public partial class Edit_Case_View : Window
    {
        public Edit_Case_View()
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
