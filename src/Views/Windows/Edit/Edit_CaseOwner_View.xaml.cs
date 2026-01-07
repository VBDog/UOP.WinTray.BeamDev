using UOP.WinTray.UI.ViewModels;
using System;
using System.Windows;

namespace UOP.WinTray.UI.Views
{
    /// <summary>
    /// Interaction logic for Edit_CaseOwner_View.xaml
    /// </summary>
    public partial class Edit_CaseOwner_View : Window
    {
        public Edit_CaseOwner_View()
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
