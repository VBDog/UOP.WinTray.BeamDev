using UOP.DXFGraphics;
using UOP.WinTray.UI.ViewModels;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace UOP.WinTray.UI.Views
{
    /// <summary>
    /// Interaction logic for Edit_Startups_View.xaml
    /// </summary>
    /// 
    [ExcludeFromCodeCoverage]
    public partial class Edit_Startups_View : Window
    {
        public Edit_Startups_View()
        {
            InitializeComponent();
        }


        public void UdateViewerImage(dxfImage aImage)
        {
            UOP.DXFGraphicsControl.DXFViewer viewer = cadfxStartupSpoutDxfViewer;
            if (aImage != null)
            {
                viewer.SetImage(aImage);
                if (aImage.Display.HasLimits)
                { viewer.ZoomWindow(aImage.Display.LimitsRectangle); }
                else
                { viewer.ZoomExtents(); }
                viewer.RefreshDisplay();
            }
            else { viewer.Clear(); }

        }

        private void Enter_Override_Length(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            Edit_Startups_ViewModel VM = DataContext as Edit_Startups_ViewModel;
            VM.InputOverrideLength();

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Edit_Startups_ViewModel VM = DataContext as Edit_Startups_ViewModel;
            VM.Window_Closing(sender, e);
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Edit_Startups_ViewModel VM = (Edit_Startups_ViewModel)DataContext;
            if (!VM.Activated)
            {
                VM.Viewer = cadfxStartupSpoutDxfViewer;
                VM.Activate(this);
            }

        }


        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel_Base VM = (ViewModel_Base)DataContext;
            if (VM != null)
            {
                VM.Viewer = null;
                VM.Dispose();
            }
        }

        bool _Activated = false;
        private void Window_Activated(object sender, System.EventArgs e)
        {


            Edit_Startups_ViewModel VM = (Edit_Startups_ViewModel)DataContext;
            if (VM != null)
            {
                if (VM.Viewer != null)
                {

                    if (!_Activated)
                    {
                        _Activated = true;
                        VM.Viewer.ZoomExtents();
                        VM.Viewer.ZoomWindow(VM.MaxViewRectangle);
                    }
                    VM.HighlightStartUp();
                }

            }

        }
    }
}