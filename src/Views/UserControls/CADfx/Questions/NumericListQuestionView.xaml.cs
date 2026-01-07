using UOP.WinTray.UI.ViewModels.UserControls.CADfx.Questions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UOP.WinTray.UI.Views.UserControls.CADfx.Questions
{
    /// <summary>
    /// Interaction logic for NumericListQuestionView.xaml
    /// </summary>
    public partial class NumericListQuestionView : UserControl
    {
        public NumericListQuestionView()
        {
            InitializeComponent();
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as NumericListQuestionViewModel)?.AddButtonClickHandler();
        }

        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as NumericListQuestionViewModel)?.RemoveButtonClickHandler();
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Tab || e.Key == Key.LineFeed || e.Key == Key.Enter || e.Key == Key.Return)
            {
                
                (DataContext as NumericListQuestionViewModel)?.AddButtonClickHandler();
                e.Handled = true;
            }
        }

        private void listBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete || e.Key == Key.Back )
            {

                (DataContext as NumericListQuestionViewModel)?.RemoveButtonClickHandler();
                e.Handled = true;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            QuestionViewModelBase VM = (QuestionViewModelBase)DataContext;
            VM.FocusElement = textBox;
        }
    }
}
