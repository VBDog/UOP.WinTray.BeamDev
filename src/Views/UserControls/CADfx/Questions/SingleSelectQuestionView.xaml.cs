using System.Windows.Controls;
using UOP.WinTray.UI.ViewModels.UserControls.CADfx.Questions;

namespace UOP.WinTray.UI.Views.UserControls.CADfx.Questions
{
    /// <summary>
    /// Interaction logic for SingleSelectQuestionView.xaml
    /// </summary>
    public partial class SingleSelectQuestionView : UserControl
    {
        public SingleSelectQuestionView()
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
