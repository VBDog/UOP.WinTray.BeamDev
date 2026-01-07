using System.Windows.Controls;
using UOP.WinTray.UI.ViewModels.UserControls.CADfx.Questions;

namespace UOP.WinTray.UI.Views.UserControls.CADfx.Questions
{
    /// <summary>
    /// Interaction logic for NumericValueQuestionView.xaml
    /// </summary>
    public partial class NumericValueQuestionView : UserControl
    {
        public NumericValueQuestionView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            QuestionViewModelBase VM = (QuestionViewModelBase)DataContext;
            VM.FocusElement = textBox;
        }
    }
    
}
