using UOP.WinTray.UI.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using UOP.DXFGraphics;

namespace UOP.WinTray.UI.Views.Windows
{
    /// <summary>
    /// Interaction logic for Edit_MDDeckSplices_View.xaml
    /// </summary>
    public partial class Edit_MDDeckSplices_View : Window
    {

        public Edit_MDDeckSplices_View()
        {
            InitializeComponent();


        }

        public bool showProgress { get; set; }
        public Edit_MDDeckSplices_View(Edit_MDDeckSplices_ViewModel splicingViewModel)
        {
            InitializeComponent();

            this.DataContext = splicingViewModel;
            splicingViewModel.Viewer = cadfxDXFViewer;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Edit_MDDeckSplices_ViewModel VM = this.DataContext as Edit_MDDeckSplices_ViewModel;


            if (!VM.Activated)
            {
                VM.FormLoad(cadfxDXFViewer);
                VM.PropertyChanged -= PropertyChanged;
                VM.PropertyChanged += PropertyChanged;
                VM.Viewer = cadfxDXFViewer;
                VM.Activate(this);

            }


            //if (VM.Viewer != null)
            //{
            //    VM.Viewer.OnViewerMouseDown -= DfxViewer_OnViewerMouseDown;
            //    VM.Viewer.OnViewerMouseDown += DfxViewer_OnViewerMouseDown;
            //}
        }
        private bool _shown;
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            if (_shown) return;
            _shown = true;
            showProgress = true;
            Edit_MDDeckSplices_ViewModel VM = this.DataContext as Edit_MDDeckSplices_ViewModel;
            VM.Viewer = cadfxDXFViewer;

        }



        private void SpliceDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (SplicesGrid.SelectedItem == null) return;

            DataGrid dg = sender as DataGrid;
            if (dg != null)
                dg.ScrollIntoView(SplicesGrid.SelectedItem);
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var VM = (Edit_MDDeckSplices_ViewModel)this.DataContext;
            if (!VM.IsEnabled)
            {
                e.Cancel = true;
                return;
            }
            VM.Window_Closing(sender, e);
        }



        private void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.Compare(e.PropertyName, "DialogResult", true) == 0)
            {

                Edit_MDDeckSplices_ViewModel VM = (Edit_MDDeckSplices_ViewModel)DataContext;

                if (VM.DialogResult.HasValue)
                {
                    VM.PropertyChanged -= PropertyChanged;
                    Close();
                }

            }
            else if (string.Compare(e.PropertyName, "Hidden", true) == 0)
            {
                Edit_MDDeckSplices_ViewModel VM = (Edit_MDDeckSplices_ViewModel)DataContext;
                //Opacity = VM.Hidden ? 0 : 1 ;

                WindowState = VM.Hidden ? WindowState.Minimized : WindowState.Normal;
            }
            //else if (String.Compare(e.PropertyName, "RightStatusBarLabelText",true) == 0) {
            //    TextBlock textBlock
            //    Control control =rightStatusBarLabel;
            //if(control.InvokeRequired)
            //    //Edit_MDDeckSplices_ViewModel VM = (Edit_MDDeckSplices_ViewModel)DataContext;
            //    //rightStatusBarLabel.Content = VM.RightStatusBarLabelText;


            //}

        }



        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            Edit_MDDeckSplices_ViewModel VM = (Edit_MDDeckSplices_ViewModel)DataContext;
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
                Edit_MDDeckSplices_ViewModel VM = (Edit_MDDeckSplices_ViewModel)DataContext;
                if (VM != null)
                {
                    VM.Execute_Delete();
                }
            }

        }
    }


}
