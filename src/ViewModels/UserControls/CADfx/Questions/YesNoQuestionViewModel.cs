using UOP.WinTray.UI.Views.UserControls.CADfx.Questions;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.UI.Views.Windows.CADfx.Questions;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace UOP.WinTray.UI.ViewModels.UserControls.CADfx.Questions
{
    public class YesNoQuestionViewModel : QuestionViewModelBase, INotifyPropertyChanged, IQuestionViewModel
    {
        private mzQuestion question;
        private YesNoQuestionView userControl;
        private QuestionOptions options;

        #region INotigyPropertyChanged Implementation

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
            this.userControl = userControl as YesNoQuestionView;
            this.options = options;

            question = q;

            LabelText = question.Prompt;
            
            if (question.Answer == null || question.Answer is not bool)
            {
                if (options == null || !options.ShowNA)
                {
                    NoRadioButtonChecked = true;
                }
                else
                {
                    NARadioButtonChecked = true;
                }
            }
            else
            {
                bool answer = (bool)question.Answer;
                if (answer)
                {
                    YesRadioButtonChecked = true;
                }
                else
                {
                    NoRadioButtonChecked = true;
                }
            }
        }

        public mzQuestion ReturnQuestion()
        {
            bool? answer;
            if (YesRadioButtonChecked)
            {
                answer = true;
            }
            else
            {
                if (NoRadioButtonChecked)
                {
                    answer = false;
                }
                else
                {
                    answer = null;
                }
            }

            question.Answer = answer;

            return question;
        }

        public string ValidateInput()
        {
            if (question.AnswerRequired && NARadioButtonChecked)
            {
                return $"In section \"{LabelText}\", an answer is required.";
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

                userControl.yesRadioButton.Focus();
            }
        }

        #endregion

        #region Binding Properties

        public string LabelText { get; set; }

        private bool yesRadioButtonChecked;
        public bool YesRadioButtonChecked
        {
            get
            {
                return yesRadioButtonChecked;
            }

            set
            {
                if (value != yesRadioButtonChecked)
                {
                    yesRadioButtonChecked = value;

                    NotifyPropertyChanged();
                }
            }
        }

        private bool noRadioButtonChecked;
        public bool NoRadioButtonChecked
        {
            get
            {
                return noRadioButtonChecked;
            }

            set
            {
                if (value != noRadioButtonChecked)
                {
                    noRadioButtonChecked = value;

                    NotifyPropertyChanged();
                }
            }
        }

        private bool nARadioButtonChecked;
        public bool NARadioButtonChecked
        {
            get
            {
                return nARadioButtonChecked;
            }

            set
            {
                if (value != nARadioButtonChecked)
                {
                    nARadioButtonChecked = value;

                    NotifyPropertyChanged();
                }
            }
        }

        public Visibility NARadioButtonVisibility
        {
            get
            {
                if (options != null)
                {
                    return options.ShowNA ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }

        #endregion
    }
}
