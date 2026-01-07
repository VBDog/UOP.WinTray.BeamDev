using UOP.WinTray.UI.ViewModels;
using System;
using System.Windows;
using MvvmDialogs;
using System.ComponentModel;

namespace UOP.WinTray.UI.Views.Windows
{
    /// <summary>
    /// Interaction logic for Edit_MDStiffeners_View.xaml
    /// </summary>
    public partial class Edit_MDStiffeners_View : Window
    {
    
        public Edit_MDStiffeners_View()
        {
           
            InitializeComponent();
    }

        public Edit_MDStiffeners_View(Edit_MDStiffeners_ViewModel VM)
        {
            InitializeComponent();
        }
       

        private void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.Compare(e.PropertyName , "DialogResult",true) == 0)
            {

                Edit_MDStiffeners_ViewModel VM = (Edit_MDStiffeners_ViewModel)DataContext;

                if (VM.DialogResult.HasValue)
                {
                    VM.PropertyChanged -= PropertyChanged;
                    Close();
                }
            }
            else if (string.Compare(e.PropertyName, "Hidden", true) == 0)
            {
                Edit_MDStiffeners_ViewModel VM = (Edit_MDStiffeners_ViewModel)DataContext;
                //Opacity = VM.Hidden ? 0 : 1;
                WindowState = VM.Hidden ? WindowState.Minimized : WindowState.Normal;
            }
        }

      

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
     
            Edit_MDStiffeners_ViewModel VM = (Edit_MDStiffeners_ViewModel)DataContext;
            if (!VM.Activated)
            {
                VM.PropertyChanged -= PropertyChanged;
                VM.PropertyChanged += PropertyChanged;
                VM.Viewer = cadfxDXFViewer;
                VM.Activate(this);
    
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ViewModel_Base VM = (ViewModel_Base)DataContext;
            if (VM != null)
            {
                if (!VM.IsEnabled)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            Edit_MDStiffeners_ViewModel VM = (Edit_MDStiffeners_ViewModel)DataContext;
            if (VM != null)
            {
                VM.Viewer = null;
                VM.PropertyChanged -= PropertyChanged;
                VM.Dispose();
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Delete)
            {
                Edit_MDStiffeners_ViewModel VM = (Edit_MDStiffeners_ViewModel)DataContext;
                if (VM != null)
                {
                    VM.Execute_Delete();
                }
            }

        }

      
    }
}

