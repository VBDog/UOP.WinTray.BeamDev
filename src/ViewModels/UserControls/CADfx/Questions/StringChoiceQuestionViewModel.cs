using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.UI.Views.UserControls.CADfx.Questions;
using UOP.WinTray.UI.Views.Windows.CADfx.Questions;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace UOP.WinTray.UI.ViewModels.UserControls.CADfx.Questions
{
    public class StringChoiceQuestionViewModel : QuestionViewModelBase, INotifyPropertyChanged, IQuestionViewModel
    {
        private mzQuestion question;
        private StringChoiceQuestionView userControl;
        private QuestionOptions options;

        #region INotigyPropertyChanged 

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region IQuestionViewModel Implementation

        public string ToolTip { get => question.ToolTip; set { question.ToolTip = value; NotifyPropertyChanged("ToolTip"); } }

        public void ConfigureDataContext(mzQuestion q, UserControl userControl, QuestionOptions options)
        {
            this.userControl = userControl as StringChoiceQuestionView;
            this.options = options;

            question = q;

            strings = new ObservableCollection<string>();

            LabelText = question.Prompt;
            SelectedIndex = -1;

            if (question.Answer != null && question.Choices != null)
            {
                for (int i = 1; i <= question.Choices.Count; i++)
                {
                    string currentString = question.Choices.Item(i);
                    strings.Add(currentString);
                    if (currentString == question.Answer.ToString())
                    {
                        SelectedIndex = i - 1;
                    }
                }
            }
        }

        public mzQuestion ReturnQuestion()
        {
            question.Answer = SelectedIndex >= 0 ? Strings[SelectedIndex] : null;

            return question;
        }

        public string ValidateInput()
        {
            if (question.AnswerRequired && selectedIndex < 0)
            {
                return $"In section \"{LabelText}\", you need to select one of the options.";
            }
            else
            {
                return "";
            }
        }

        public void InformAboutInvalidInput()
        {
            string errorMessage = ValidateInput();
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                MessageBox.Show(errorMessage, "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);

                userControl.comboBox.Focus();
            }
        }

        #endregion

        #region Binding Properties

        public string LabelText { get; set; }

        private int selectedIndex;
        public int SelectedIndex
        {
            get
            {
                return selectedIndex;
            }

            set
            {
                if (value != selectedIndex)
                {
                    selectedIndex = value;

                    NotifyPropertyChanged();
                }
            }
        }

        private ObservableCollection<string> strings;
        public ObservableCollection<string> Strings
        {
            get
            {
                return strings;
            }

            set
            {
                strings = value;

                NotifyPropertyChanged();
            }
        }

        #endregion
    }
}
