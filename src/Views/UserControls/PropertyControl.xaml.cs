using UOP.WinTray.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UOP.WinTray.UI.Views
{
    /// <summary>
    /// Interaction logic for PropertyControl.xaml
    /// </summary>
    public partial class PropertyControl : UserControl
    {
        public PropertyControl()
        {
            InitializeComponent();
        }


        private void Property_GotFocus(object sender, RoutedEventArgs e)
        {
            {
                PropertyControlViewModel VM = DataContext as PropertyControlViewModel;
                if (VM != null) VM.RespondToGotFocus();

            }
        }
       

     
        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            PropertyControlViewModel VM = DataContext as PropertyControlViewModel;
            if(VM != null)
            {
                VM.InputBox = InputBox;
                VM.ReadOnlyBox = ReadOnlyBox;
                VM.InputChoiceList = InputChoiceList;
                VM.YesNoCheckBox = PropertyCheckBox;
            }
            
        }

        private void InputBox_LostFocus(object sender, RoutedEventArgs e)
        {
            PropertyControlViewModel VM = DataContext as PropertyControlViewModel;
                if(VM !=null & !VM.IsReadOnly) VM.DisplayUnitValueString = InputBox.Text;

        }


        private void Property_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PropertyControlViewModel VM = DataContext as PropertyControlViewModel;
            if (VM != null) VM.RespondToDoubleClick();
        }

     

        private void InputChoiceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            PropertyControlViewModel VM = DataContext as PropertyControlViewModel;
            if (VM != null) 
                VM.RespondToChoiceValueChange();
        }

        private void PropertyCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            PropertyControlViewModel VM = DataContext as PropertyControlViewModel;
            if (VM != null)
                VM.RespondToYesNoValueChange();
        }

        public Control InputControl()
        {
                PropertyControlViewModel VM = DataContext as PropertyControlViewModel;
                return (VM != null) ? VM.InputControl(this) : InputBox; 
        }
    }
}
