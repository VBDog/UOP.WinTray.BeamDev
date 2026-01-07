using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.UI.Views.UserControls.CADfx.Questions;
using UOP.WinTray.UI.Views.Windows.CADfx.Questions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace UOP.WinTray.UI.ViewModels.UserControls.CADfx.Questions
{
    public class DualStringChoiceQuestionViewModel : QuestionViewModelBase, INotifyPropertyChanged, IQuestionViewModel
    {
        private mzQuestion question;
        private DualStringChoiceQuestionView userControl;
        private QuestionOptions options;
        private Dictionary<string, ObservableCollection<string>> parentChild;

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
            this.userControl = userControl as DualStringChoiceQuestionView;
            this.options = options;

            question = q;

            parentChild = new Dictionary<string, ObservableCollection<string>>();
            leftList = new ObservableCollection<string>();
            rightList = new ObservableCollection<string>();
            LeftListSelectedIndex = -1;
            RightListSelectedIndex = -1;

            if (question.Choices != null)
            {
                // Extracting lists data from the input
                string choice;
                string key;
                ObservableCollection<string> value;
                mzValues choices = question.Choices;

                for (int i = 1; i <= choices.Count; i++)
                {
                    choice = choices.Item(i)?.ToString();
                    (key, value) = ExtractParentChildRelationFromInput(choice, question.ChoiceSubDelimeter);
                    if (!string.IsNullOrWhiteSpace(key))
                    {
                        parentChild.Add(key, value);
                        leftList.Add(key);
                    }
                }
                // Extracting lists data from the input

                // Extracting labels text from the input
                (LeftLabelText, RightLabelText) = ExtractLeftAndRightListLabelsFromInput(question.Prompt, question.ChoiceSubDelimeter);
                // Extracting labels text from the input

                string aStr = question.Answer.ToString();

                // Extracting selected indicies from the input
                (LeftListSelectedIndex, RightListSelectedIndex) = ExtractSelectedIndicesFromInput(question.AnswerS, question.ChoiceSubDelimeter, parentChild, leftList);
                // Extracting selected indicies from the input
            }
        }

        public mzQuestion ReturnQuestion()
        {
            string selectedKey = "", selectedValue = "";

            if (LeftListSelectedIndex >= 0)
            {
                selectedKey = LeftList[LeftListSelectedIndex];

                if (RightListSelectedIndex >= 0)
                {
                    selectedValue = RightList[RightListSelectedIndex];
                }
            }

            string answer = selectedKey;
            if (!string.IsNullOrWhiteSpace(selectedValue))
            {
                answer += $"|{selectedValue}";
            }

            question.Answer = answer;

            return question;
        }

        public string ValidateInput()
        {
            if (question.AnswerRequired && (LeftListSelectedIndex < 0 || RightListSelectedIndex < 0))
            {
                return $"In section \"{LeftLabelText}\", please select a row and its corresponding value.";
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

                if (LeftListSelectedIndex < 0)
                {
                    userControl.leftListBox.Focus();
                }
                else // Implies that: RightListSelectedIndex < 0
                {
                    userControl.rightListBox.Focus();
                }
            }
        }

        #endregion

        #region Binding Properties

        public string LeftLabelText { get; set; }
        public string RightLabelText { get; set; }

        private int leftListSelectedIndex;
        public int LeftListSelectedIndex
        {
            get
            {
                return leftListSelectedIndex;
            }

            set
            {
                if (value != leftListSelectedIndex)
                {
                    leftListSelectedIndex = value;

                    NotifyPropertyChanged();

                    if (value >= 0)
                    {
                        RightList = parentChild[leftList[value]]; // It finds the corresponding right list for the selected row in left list. 
                    }
                    RightListSelectedIndex = -1;
                }
            }
        }

        private int rightListSelectedIndex;
        public int RightListSelectedIndex
        {
            get
            {
                return rightListSelectedIndex;
            }

            set
            {
                if (value != rightListSelectedIndex)
                {
                    rightListSelectedIndex = value;

                    NotifyPropertyChanged();
                }
            }
        }

        private ObservableCollection<string> leftList;
        public ObservableCollection<string> LeftList
        {
            get
            {
                return leftList;
            }

            set
            {
                leftList = value;

                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<string> rightList;
        public ObservableCollection<string> RightList
        {
            get
            {
                return rightList;
            }

            set
            {
                rightList = value;

                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Utility Methods

        private (string, ObservableCollection<string>) ExtractParentChildRelationFromInput(string choice, string delimiter)
        {
            string key;
            ObservableCollection<string> value = new();

            TVALUES aVals = TVALUES.FromDelimitedList(choice, delimiter);
            if (aVals.Count == 0)
            {
                return (null, null);
            }

            key = aVals.Value(1)?.ToString();
            if (string.IsNullOrWhiteSpace(key)) // Extracting the key
            {
                return (null, null);
            }

            string tempString;
            for (int i = 2; i <= aVals.Count; i++) // It assumes that element is like this: le1|re1-1|re1-2|re1-3, le2|re2-1|re2-2|re2-3, ...
            {
                tempString = aVals.Value(i)?.ToString();
                if (!string.IsNullOrWhiteSpace(tempString))
                {
                    value.Add(tempString);
                }
            }

            return (key, value);
        }

        private (string, string) ExtractLeftAndRightListLabelsFromInput(string prompt, string delimiter)
        {
            string leftLabel, rightLabel;

            TVALUES aVals = TVALUES.FromDelimitedList(question.Prompt, question.ChoiceSubDelimeter, false, true);

            leftLabel = aVals.Value(1)?.ToString() ?? "";
            rightLabel = aVals.Value(2)?.ToString() ?? "";

            return (leftLabel, rightLabel);
        }

        private (int, int) ExtractSelectedIndicesFromInput(string answer, string delimiter, Dictionary<string, ObservableCollection<string>> parentChildDictionary, ObservableCollection<string> parents)
        {
            if (string.IsNullOrWhiteSpace(answer))
            {
                return (-1, -1);
            }

            string selectedParent, selectedChild;

            TVALUES aVals = TVALUES.FromDelimitedList(answer, delimiter, false, true);

            selectedParent = aVals.Value(1)?.ToString() ?? "";
            selectedChild = aVals.Value(2)?.ToString() ?? "";

            if (parentChildDictionary.TryGetValue(selectedParent, out ObservableCollection<string> children))
            {
                int selectedParentIndex = parents.IndexOf(selectedParent);
                if (selectedParentIndex == -1) // We don't expect this to happen because the dictionary keys and the strings in parent list should be the same
                {
                    return (-1, -1);
                }

                int selectedChildIndex = -1;
                if (!string.IsNullOrWhiteSpace(selectedChild))
                {
                    selectedChildIndex = children.IndexOf(selectedChild);
                }

                return (selectedParentIndex, selectedChildIndex);
            }
            else
            {
                return (-1, -1);
            }
        }

        #endregion
    }
}
