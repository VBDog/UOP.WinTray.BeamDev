using UOP.WinTray.UI.ViewModels.UserControls.CADfx.Questions;
using System.Windows.Controls;

namespace UOP.WinTray.UI.Views.UserControls.CADfx.Questions
{
    /// <summary>
    /// Interaction logic for StringValueQuestionView.xaml
    /// </summary>
    public partial class StringValueQuestionView : UserControl
    {
        public StringValueQuestionView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            StringValueQuestionViewModel VM = DataContext as StringValueQuestionViewModel;
            if (VM != null)
            {
                VM.TextBoxControl = textBox;
                VM.FocusElement = textBox;
            }
        }
    }
}
