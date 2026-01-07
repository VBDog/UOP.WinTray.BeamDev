using UOP.WinTray.UI.CustomControls.UserControls;
using UOP.WinTray.UI.ViewModels;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace UOP.WinTray.UI.Views
{

    /// <summary>
    /// Interaction logic for Edit_SpoutGroup_View.xaml
    /// </summary>
    [ExcludeFromCodeCoverage]
    public partial class Edit_SpoutGroup_View : Window
    {
        public Edit_SpoutGroup_View()
        {
            InitializeComponent();
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight - 100;
            Edit_SpoutGroup_ViewModel VM = DataContext as Edit_SpoutGroup_ViewModel;


        }


        #region Event Handlers
        /// <summary>
        /// Handler increments the count when increment/up arrow button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumericTextBoxSpinControl_IncreaseClicked(object sender, RoutedEventArgs e)
        {
            var source = sender as NumericTextBoxSpinControl;
            var sourceVM = source.DataContext as Edit_SpoutGroup_ViewModel;
            sourceVM.IncrementValue(source.Name, 1);
        }

        /// <summary>
        /// Handler decrements the count when decrement/down arrow button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumericTextBoxSpinControl_DecreaseClicked(object sender, RoutedEventArgs e)
        {
            var source = sender as NumericTextBoxSpinControl;
            var sourceVM = source.DataContext as Edit_SpoutGroup_ViewModel;
            sourceVM.IncrementValue(source.Name, -1);
        }

        /// <summary>
        /// Handler to set the constrains when user provides constraints manually (instead of using up/down control)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>      
        private void NumericTextBoxSpinControl_TextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is System.Windows.Controls.TextBox)
            {
                var source = sender as NumericTextBoxSpinControl;
                var sourceVM = source.DataContext as Edit_SpoutGroup_ViewModel;
                var textValue = (e.OriginalSource as System.Windows.Controls.TextBox).Text;
                sourceVM.SetConstraintWhenLostFocus(source.Name.ToUpper(), ref textValue);
                (e.OriginalSource as System.Windows.Controls.TextBox).Text = textValue;
            }
        }

        #endregion



        private void NumericTextBoxSpinControl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var source = sender as NumericTextBoxSpinControl;

            if (e.Key == System.Windows.Input.Key.Delete)
            {

                var sourceVM = source.DataContext as Edit_SpoutGroup_ViewModel;
                sourceVM.ResetValue(source.Name);
            }
            else
            {

                e.Handled = true;

            }
            e.Handled = true;

        }


     
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Edit_SpoutGroup_ViewModel VM = (Edit_SpoutGroup_ViewModel)DataContext;
            if (!VM.Activated)
            {
                VM.Viewer = cadfxDxfViewerEditSpouts;
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

    }
}
