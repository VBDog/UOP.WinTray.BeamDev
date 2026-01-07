using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.UI.Views.UserControls.CADfx.Questions;
using UOP.WinTray.UI.Views.Windows.CADfx.Questions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace UOP.WinTray.UI.ViewModels.UserControls.CADfx.Questions
{
    public class StringValueQuestionViewModel : QuestionViewModelBase, INotifyPropertyChanged, IQuestionViewModel
    {
        private mzQuestion question;
        private StringValueQuestionView userControl;
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
            this.userControl = userControl as StringValueQuestionView;
            this.options = options;

            question = q;

            LabelText = question.Prompt;
            if (question.Answer != null)
            {
                TextBoxText = question.Answer.ToString();
            }
        }

        public mzQuestion ReturnQuestion()
        {
            question.Answer = TextBoxText;

            return question;
        }

        public string ValidateInput()
        {
            string _rVal = "";
            if (question.AnswerRequired && string.IsNullOrWhiteSpace(TextBoxText)) _rVal = $"An answer for '{LabelText}' is required.";
            if(question.UnacceptableAnswers != null && _rVal == "")
            {
                List<string> badanswers = question.UnacceptableAnswers;
                if (badanswers.FindIndex(x => string.Compare(x, TextBoxText, true) == 0) >= 0) 
                {
                    _rVal = $"The answer for '{LabelText}' is unnacceptable.";
                    if (!string.IsNullOrWhiteSpace(question.UnacceptableAnswerMessage)) _rVal += $" {question.UnacceptableAnswerMessage}";
                }
            }

            return _rVal;


        }

        public void InformAboutInvalidInput()
        {
            string errorMessage = ValidateInput();
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                MessageBox.Show(errorMessage, "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);

                userControl.textBox.Focus();
            }
        }

        #endregion

        #region Binding Properties

        private System.WeakReference<TextBox> _TextBoxControl;
        public TextBox TextBoxControl
        {
            get
            {
                if (_TextBoxControl == null) return null;
                if (!_TextBoxControl.TryGetTarget(out TextBox _rVal))
                    _TextBoxControl = null;
                return _rVal;
            }

            set
            {
                if (value == null)
                {
                    _TextBoxControl = null;
                    return;
                };
                _TextBoxControl = new System.WeakReference<TextBox>(value);
                if (question.MaxChars > 0)
                    value.MaxLength = question.MaxChars;
            }
        }

        public int MaxChars { get => question.MaxChars; set { question.MaxChars = value; NotifyPropertyChanged("MaxChars");  } }

        public string LabelText { get; set; }

        private string textBoxText;
        public string TextBoxText { get => textBoxText;
            
            set
            {
                if (textBoxText != value)
                {
                    textBoxText = value;

                    question.Answer = value;

                    NotifyPropertyChanged();
                }
            }
        }

        #endregion
    }
}
