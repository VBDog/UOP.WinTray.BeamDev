using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.UI.Views.UserControls.CADfx.Questions;
using UOP.WinTray.UI.Views.Windows.CADfx.Questions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace UOP.WinTray.UI.ViewModels.UserControls.CADfx.Questions
{
    public class NumericListQuestionViewModel : QuestionViewModelBase, INotifyPropertyChanged, IQuestionViewModel
    {
        private mzQuestion question;
        private NumericListQuestionView userControl;
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
            this.userControl = userControl as NumericListQuestionView;
            this.options = options;

            question = q;

            numbers = new ObservableCollection<double>();

            LabelText = question.Prompt;
            UnitLabelText = question.Suffix;
            checkBoxIsChecked = question.AddMirrors;
            checkBoxIsVisible = !options.HideAddMirrors;
            SelectedIndex = -1;

            if (question.Answer != null)
            {
                TVALUES tvalues = TVALUES.FromDelimitedList(question.Answer, ",", false, true, true, true, question.MaxDecimals);
                tvalues.SortNumeric(true);
                for (int i = 1; i <= tvalues.Count; i++)
                {
                    Numbers.Add(mzUtils.VarToDouble(tvalues.Value(i) * question.DisplayMultiplier, aPrecis: question.MaxDecimals));
                }
            }
        }

        public mzQuestion ReturnQuestion()
        {
            List<double> enteredAnswers = GetAllNumericAnswers(CheckBoxIsChecked);
            double[] answersDouble = enteredAnswers.Select(d => mzUtils.VarToDouble(d / question.DisplayMultiplier, aPrecis: question.MaxDecimals)).ToArray();

            question.Answer = string.Join(",", answersDouble);

            return question;
        }

        public string ValidateInput()
        {
            List<double> answers = GetAllNumericAnswers(CheckBoxIsChecked);
            string stringNumberList = string.Join(",", answers);

            foreach (var answer in answers)
            {
                double temp = mzUtils.VarToDouble(answer / question.DisplayMultiplier, aPrecis: question.MaxDecimals);
                question.ValidateNumber(out string errorMsg, temp, stringNumberList);
                if (!string.IsNullOrWhiteSpace(errorMsg))
                {
                    return $"In section \"{LabelText}\" (Numeric value \"{answer}\"): {errorMsg}";
                }
            }

            return "";
        }

        public void InformAboutInvalidInput()
        {
            string errorMessage = ValidateInput();
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                MessageBox.Show(errorMessage, "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);

                userControl.listBox.Focus();
            }
        }

        #endregion

        #region Binding Properties

        public string LabelText { get; set; }
        public string UnitLabelText { get; set; }

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

        private bool checkBoxIsVisible;
        public Visibility CheckBoxVisibility
        {
            get
            {
                return checkBoxIsVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

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

        private ObservableCollection<double> numbers;
        public ObservableCollection<double> Numbers
        {
            get
            {
                return numbers;
            }

            set
            {
                numbers = value;

                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Utility Methods

        public void AddButtonClickHandler()
        {
            if (!string.IsNullOrWhiteSpace(TextBoxText) && double.TryParse(TextBoxText, out double addedInput))
            {
                double added = mzUtils.VarToDouble(addedInput, aPrecis: question.MaxDecimals);
                if (!numbers.Contains(added))
                {
                    // This section adds the new number, sorts the array, gets an ObservableCollection out of it and assign the collection to the bound property "Numbers" which is of type ObservableCollection
                    numbers.Add(added);
                    if (checkBoxIsChecked.HasValue && checkBoxIsChecked == true && !numbers.Contains( -added ))
                    {
                        numbers.Add( -added );
                    }
                    var sortedList = numbers.OrderByDescending(d => d).ToList();
                    ObservableCollection<double> newNumbers = new(sortedList);
                    Numbers = newNumbers;
                    TextBoxText = "";
                }
                else
                {
                    MessageBox.Show("Number already exists!", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                    userControl.textBox.SelectAll();
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid number.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                userControl.textBox.SelectAll();
            }

            userControl.textBox.Focus();
        }

        public void RemoveButtonClickHandler()
        {
            if (SelectedIndex >= 0)
            {
                var selectedValue = Numbers[ SelectedIndex ];
                Numbers.RemoveAt(SelectedIndex);
                if (checkBoxIsChecked.HasValue && checkBoxIsChecked == true && numbers.Contains( -selectedValue ))
                {
                    numbers.Remove( -selectedValue );
                }
            }
        }

        private List<double> GetAllNumericAnswers(bool? addMirroredValues)
        {
            if (addMirroredValues.HasValue && addMirroredValues.Value)
            {
                // The dictionary has been used to speed up the search. It contains the number as the key and its mirror as the value
                Dictionary<double, double> dictionary = new();
                foreach (var number in Numbers)
                {
                    if (!dictionary.ContainsKey(number) && !dictionary.ContainsKey(-number))
                    {
                        dictionary.Add(number, -number);
                    }
                }
                // The dictionary has been used to speed up the search. It contains the number as the key and its mirror as the value

                List<double> allAnswers = new();
                foreach (var kv in dictionary)
                {
                    allAnswers.Add(kv.Key);
                    if (kv.Key != 0) // this check is to prevent adding 0, twice.
                    {
                        allAnswers.Add(kv.Value);
                    }
                }

                return allAnswers.OrderByDescending(d => d).ToList();
            }
            else
            {
                return new List<double>(Numbers);
            }
        }

        #endregion
    }
}
