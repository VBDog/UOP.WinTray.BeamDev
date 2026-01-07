using UOP.WinTray.UI.Views.UserControls.CADfx.Questions;
using UOP.WinTray.UI.Views.Windows.CADfx.Questions;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.UI.ViewModels.UserControls.CADfx.Questions
{
    public class CheckValQuestionViewModel : QuestionViewModelBase, INotifyPropertyChanged, IQuestionViewModel
    {
        private mzQuestion question;
        private CheckValQuestionView userControl;
        private QuestionOptions options;

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion  INotifyPropertyChanged Implementation

        #region IQuestionViewModel Implementation

       public string ToolTip   {  get => question.ToolTip; set { question.ToolTip = value; NotifyPropertyChanged("ToolTip"); }  }

        public mzQuestion ReturnQuestion()
        {
            question.SetAnswer ( CheckBoxIsChecked);

            return question;
        }

        public void ConfigureDataContext(mzQuestion q, UserControl userControl, QuestionOptions options)
        {
            this.userControl = userControl as CheckValQuestionView;
            this.options = options;

            question = q;

            LabelText = question.Prompt;
           CheckBoxIsChecked = question.AnswerB;
        
        }



        public string ValidateInput()
        {
            if (!CheckBoxIsChecked.HasValue)
            {
                return $"In section \"{LabelText}\", please either select or unselect the checkbox.";
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

                userControl.checkBox.Focus();
            }
        }

        #endregion

        #region Binding Properties

        public string LabelText { get; set; }

        private bool? checkBoxIsChecked;
        public bool? CheckBoxIsChecked
        {
            get
            {
                return checkBoxIsChecked;
            }

            set
            {
                if (value != checkBoxIsChecked)
                {
                    checkBoxIsChecked = value;

                    NotifyPropertyChanged();
                }
            }
        }

        #endregion
    }
}
