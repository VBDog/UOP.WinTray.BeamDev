using UOP.WinTray.UI.ViewModels.UserControls.CADfx.Questions;
using System.Windows.Controls;

namespace UOP.WinTray.UI.Views.UserControls.CADfx.Questions
{
    /// <summary>
    /// Interaction logic for MultiSelectQuestionView.xaml
    /// </summary>
    public partial class MultiSelectQuestionView : UserControl
    {
        public MultiSelectQuestionView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            MultiSelectQuestionViewModel VM = DataContext as MultiSelectQuestionViewModel;
            if (VM == null) return;
            VM.ListControl = ListViewInstance;
            VM.FocusElement = ListViewInstance;
        }
    }
}
