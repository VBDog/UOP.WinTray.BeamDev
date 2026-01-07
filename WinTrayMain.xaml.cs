using UOP.WinTray.UI.BusinessLogic;
using UOP.WinTray.UI.ViewModels;
using UOP.WinTray.UI.Views;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UOP.WinTray.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [ExcludeFromCodeCoverage]
    public partial class WinTrayMain : Window
    {
        public WinTrayMain()
        {
            InitializeComponent();

            bool isValidUser = CheckValidUser(out string versionWarningMessage);
            if (isValidUser)
            {
              
                DataContext = WinTrayMainViewModel.WinTrayMainViewModelObj;
                Closing += WinTrayMain_Closing;
            }

            if (!string.IsNullOrEmpty(versionWarningMessage))
            {
                Task.Run(() => MessageBox.Show(versionWarningMessage, "Security Issue"));
            }

        }

        private void WinTrayMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!WinTrayMainViewModel.WinTrayMainViewModelObj.IsEnabled) { e.Cancel = true; return; }

          WinTrayMainViewModel.WinTrayMainViewModelObj.MenuItemViewModelHelper.Execute_Exit(e);
          
        }

        /// <summary>
        /// Check if the current user has access
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        private bool CheckValidUser(out string versionWarningMessage)
        {
            bool isAuthenticated = true;
            versionWarningMessage = string.Empty;

            SecurityHelper securityHelper = SecurityHelper.Instance;
            string validUser = securityHelper.CheckSecurity(out bool dontKill);


#if !CADFX
            if (!string.IsNullOrEmpty(validUser))
            {


#if DEBUG
                dontKill = true;
                appApplication.User.GlobalUser = true;
                MessageBox.Show("Unable To Connect to UOP Network" + "\n** SECURITY OVERRIDE **");
                versionWarningMessage = "";
                //return true;
#endif

                if (!dontKill)
                {
                    if (validUser.Contains("Unable To Connect to UOP Network"))
                    {
                        MessageBox.Show("Unable To Connect to UOP Network" + "\nPlease map or browse to Q drive and try again.", "Q Drive not mapped");
                    }
                    else
                    {
                        MessageBox.Show("User is not valid to access this application. Please contact the administrator.", "Invalid User");
                    }
                    Application.Current?.Shutdown();
                    isAuthenticated = false;
                }
                else
                {
                    versionWarningMessage = validUser;
                }
            }
#else
            appApplication.User.GlobalUser = true;
#endif

            return isAuthenticated;
        }

        private void PropertyList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            WinTrayMainViewModel VM = this.DataContext as WinTrayMainViewModel;
            UOPPropertyListView propList = sender as UOPPropertyListView;
            if (propList == null) return;
            string field = propList.SelectedProperty == null ? "" : propList.SelectedProperty.Name;
            VM.Edit_ProjectProperties(field);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel_Base VM = DataContext as ViewModel_Base;
            if(VM != null)
            {
                if (!VM.Activated)
                {
                    VM.Activate(this);
                    VM.Project = null;
                    VM.UpdateVisibily();
                }
            }
        }

        private void Window_Activated(object sender, System.EventArgs e)
        {
            WinTrayMainViewModel VM = DataContext as WinTrayMainViewModel;
              VM?.UpdateVisibily();
        }

        private void View_MDProject_Loaded(object sender, RoutedEventArgs e)
        {
            WinTrayMainViewModel VM = DataContext as WinTrayMainViewModel;
            VM.MDProjectViewModel = (MDProjectViewModel)View_MDProject.DataContext;
        }
        private void TreeNodeButton_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //Button control = sender as Button;
            //WinTrayMainViewModel VM = WinTrayMainViewModel.WinTrayMainViewModelObj;
            //VM.RespondToTreeNodeDoubleClick((string)control.Tag );
        }

        
        private void Grid_Expander_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Grid_Expander.RowDefinitions.Count >= 2)
            {
                Grid_Expander.RowDefinitions[0].Height = new GridLength(Grid_Expander.ActualHeight * 0.6);
                Grid_Expander.RowDefinitions[1].Height = new GridLength(Grid_Expander.ActualHeight * 0.4);
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel_Base VM = DataContext as ViewModel_Base;
            VM?.Dispose();
        }
    }


    /// <summary>
    /// SeparatorStyleSelector class allows the user to set style to 'Separator'
    /// </summary>
    public class SeparatorStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is SeparatorViewModel)
                return (Style)((FrameworkElement)container).FindResource("separatorStyle");
            return null;
        }
    }
}