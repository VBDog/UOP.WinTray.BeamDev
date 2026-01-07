using System.Windows.Controls;
using UOP.WinTray.UI.ViewModels.UserControls.CADfx.Questions;

namespace UOP.WinTray.UI.Views.UserControls.CADfx.Questions
{
    /// <summary>
    /// Interaction logic for DualStringChoiceQuestionView.xaml
    /// </summary>
    public partial class DualStringChoiceQuestionView : UserControl
    {
        public DualStringChoiceQuestionView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            QuestionViewModelBase VM = (QuestionViewModelBase)DataContext;
            VM.FocusElement = leftListBox;
        }
    }
}
