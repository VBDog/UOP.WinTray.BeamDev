using UOP.WinTray.UI.Logger;
using System.Windows;
using UOP.WinTray.Projects.Utilities;
using DocumentFormat.OpenXml.Drawing;

namespace UOP.WinTray.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e) 
        { 
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }

        private void Application_DispatcherUnhandledException(object sender,
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                //if (System.Diagnostics.Debugger.IsAttached)
                //{
                //     MessageBox.Show("An unhandled exception just occurred: " + e.Exception.Message, "UOP WinTray", MessageBoxButton.OK, MessageBoxImage.Warning);
                //}
                ApplicationLogger.Instance.LogError(e.Exception);
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Console.WriteLine($"Application_DispatcherUnhandledException : {e.Exception}");
                e.Handled = true;
                
            }
            catch(System.Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }
    }
}

