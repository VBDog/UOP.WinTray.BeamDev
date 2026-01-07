using UOP.WinTray.UI.Views.UserControls.CADfx.Questions;
using UOP.WinTray.UI.Views.Windows.CADfx.Questions;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.UI.ViewModels.UserControls.CADfx.Questions
{
    public class NumericValueQuestionViewModel : QuestionViewModelBase, INotifyPropertyChanged, IQuestionViewModel
    {
        private mzQuestion question;
        private NumericValueQuestionView userControl;
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
            this.userControl = userControl as NumericValueQuestionView;
            this.options = options;

            question = q;

            LabelText = question.Prompt;
            if (question.Answer != null)
            {
                double textAsDouble = double.Parse(question.Answer.ToString());
                TextBoxText = mzUtils.VarToDouble(textAsDouble * question.DisplayMultiplier, aPrecis: question.MaxDecimals).ToString();
            }
        }

        public mzQuestion ReturnQuestion()
        {
            if (string.IsNullOrWhiteSpace(TextBoxText) || !double.TryParse(TextBoxText, out double textAsDouble))
            {
                question.Answer = null;
            }
            else
            {
                question.Answer = mzUtils.VarToDouble(textAsDouble / question.DisplayMultiplier, aPrecis: question.MaxDecimals);
            }

            return question;
        }

        public string ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(TextBoxText))
            {
                return question.AnswerRequired ? $"In section \"{LabelText}\", the answer is required." : "";
            }
            else
            {
                if (double.TryParse(TextBoxText, out double textAsDouble))
                {
                    double temp = mzUtils.VarToDouble(textAsDouble / question.DisplayMultiplier, aPrecis: question.MaxDecimals);
                    question.ValidateNumber(out string errorMsg, temp);
                    if (string.IsNullOrWhiteSpace(errorMsg))
                    {
                        return "";
                    }
                    else
                    {
                        return $"In section \"{LabelText}\": {errorMsg}";
                    }
                }
                else
                {
                    return $"In section \"{LabelText}\", please enter a valid number.";
                }
            }
        }

        public void InformAboutInvalidInput()
        {
            string errorMessage = ValidateInput();
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                MessageBox.Show(errorMessage, "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);

                userControl.textBox.SelectAll();
                userControl.textBox.Focus();
            }
        }

        #endregion

        #region Binding Properties

        public string LabelText { get; set; }

        private string textBoxText;
        public string TextBoxText
        {
            get
            {
                return textBoxText;
            }

            set
            {
                if (textBoxText != value)
                {
                    textBoxText = value;

                    NotifyPropertyChanged();
                }
            }
        }

        #endregion
    }
}
