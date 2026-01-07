using System.Windows.Controls;
using UOP.WinTray.UI.ViewModels.UserControls.CADfx.Questions;

namespace UOP.WinTray.UI.Views.UserControls.CADfx.Questions
{
    /// <summary>
    /// Interaction logic for StringChoiceQuestionView.xaml
    /// </summary>
    public partial class StringChoiceQuestionView : UserControl
    {
        public StringChoiceQuestionView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            QuestionViewModelBase VM = (QuestionViewModelBase)DataContext;
            VM.FocusElement = comboBox;
        }
    }
}
