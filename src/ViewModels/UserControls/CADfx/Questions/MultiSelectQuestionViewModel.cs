using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.UI.Views.UserControls.CADfx.Questions;
using UOP.WinTray.UI.Views.Windows.CADfx.Questions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace UOP.WinTray.UI.ViewModels.UserControls.CADfx.Questions
{
    public class MultiSelectQuestionViewModel : QuestionViewModelBase, INotifyPropertyChanged, IQuestionViewModel
    {
        private mzQuestion question;
        private MultiSelectQuestionView userControl;
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
            this.userControl = userControl as MultiSelectQuestionView;
            this.options = options;

            question = q;

            LabelText = question.Prompt;

            if (question.Answer != null)
            {
                string choiceDelimiter = string.IsNullOrWhiteSpace(question.ChoiceDelimiter) ? "," : question.ChoiceDelimiter;

                TVALUES defaultAnswer = TVALUES.FromDelimitedList(question.Answer, choiceDelimiter);

                AddMissingChoicesUsingAnswer(question, defaultAnswer, choiceDelimiter);
                AdjustHeadersAndColumnCount(question);

                GridView gridView = BuildGridView(question);

                ObservableCollection<ExpandoObject> allItems = GetItemsFromInput(question, defaultAnswer);

               
                Items = GetMultiSelectItems(question, defaultAnswer);
                GridViewElement = gridView;
            }
        }

        private System.WeakReference<ListBox> _ListControl;
        public ListBox ListControl
        {
            get
            {
                if (_ListControl == null) return null;
                if (!_ListControl.TryGetTarget(out ListBox _rVal))
                    _ListControl = null;
                return _rVal;
            }

            set
            {
                if(value == null)
                {
                    _ListControl = null;
                    return;
                };
                _ListControl = new System.WeakReference<ListBox>(value);
            }
        }

        public mzQuestion ReturnQuestion()
        {
            var selectedItems = GetSelectedItems();

            if (question.AnswerRequired && selectedItems.Count < question.MinChoiceCount)
            {
                question.SetAnswer( null);
            }
            else
            {
                //// This line reads the Key property of the selected items and return them as an array of strings
                //string[] selectedKeys = selectedItems.Select(o => ReadPropertyAsString(o, "Key")).Where(t => t.Item2).Select(t => t.Item1).ToArray();
                string answer = "";
                string answers = "";
                foreach (var item in selectedItems)
                {
                    answer = item.Text;

                    if (!string.IsNullOrWhiteSpace(answer))
                    {
                        if (!string.IsNullOrWhiteSpace(answers)) answers += question.ChoiceDelimiter;
                        answers += answer;
                    }
                }
                question.SetAnswer(answers);
                //question.SetAnswer( $"{string.Join(question.ChoiceDelimiter, selectedKeys)}");
            }

            return question;
        }

        public string ValidateInput()
        {
            var selectedItems = GetSelectedItems();
            int selcount = selectedItems.Count ;

            if (question.AnswerRequired && selcount <=0 ) return $"In section \"{LabelText}\": Requires at Least {1} Selections.";
         
            
            if (selcount < question.MinChoiceCount) return $"In section \"{LabelText}\": Requires at Least {question.MinChoiceCount} Selections.";
            if (selcount > question.MaxChoiceCount && question.MaxChoiceCount > 0) return $"In section \"{LabelText}\": Only {question.MaxChoiceCount} Selections Are Allowed.";
           
            return "";


        }

        public void InformAboutInvalidInput()
        {
            string errorMessage = ValidateInput();
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                MessageBox.Show(errorMessage, "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);

                userControl.ListViewInstance.Focus();
            }
        }

        #endregion

        #region Binding Properties

        public string LabelText { get; set; }

        private GridView gridViewElement;
        public GridView GridViewElement
        {
            get
            {
                return gridViewElement;
            }

            set
            {
                gridViewElement = value;

                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<MultiSelectItem> _NewItems;
        public ObservableCollection<MultiSelectItem> Items
        {
            get => _NewItems;
            set { _NewItems = value; NotifyPropertyChanged(); }
        }

      

        #endregion

        #region Utility Methods

        public List<MultiSelectItem> GetSelectedItems()
        {
            List<MultiSelectItem> _rVal = new();
            foreach (var item in Items)
            {
                if(item.IsChecked) _rVal.Add(item);
            }
           return _rVal;

        }

       
        private void AddProperty(ExpandoObject expando, string propertyName, object propertyValue)
        {
            var expandoDict = expando as IDictionary<string, object>;

            if (expandoDict.ContainsKey(propertyName))
            {
                expandoDict[propertyName] = propertyValue;
            }
            else
            {
                expandoDict.Add(propertyName, propertyValue);
            }
        }

        private (string, bool) ReadPropertyAsString(ExpandoObject expando, string propertyName)
        {
            var expandoDict = expando as IDictionary<string, object>;

            if (expandoDict.ContainsKey(propertyName))
            {
                return (expandoDict[propertyName].ToString(), true);
            }
            else
            {
                return (null, false);
            }
        }

        private void AddMissingChoicesUsingAnswer(mzQuestion q, TVALUES answer, string delimiter)
        {
            int idx;
            for (int i = 1; i <= answer.Count; i++)
            {
                idx = question.Choices.FindStringValue(answer.Value(i), delimiter);
                if (idx <=0)
                {
                    q.Choices.Add(answer.Value(i));
                }
            }
        }

        private void AdjustHeadersAndColumnCount(mzQuestion q)
        {
            if (q.Choices.Count > 0 && q.ColCount > 0)
            {
                while (q.Headers.Count < q.ColCount)
                {
                    q.Headers.Add("");
                }

                if (q.Headers.Count > q.ColCount)
                {
                    q.ColCount = q.Headers.Count;
                }
            }
        }

        private string GetPropertyName(int index)
        {
            if (index == 0)
            {
                return "Key";
            }
            else
            {
                return $"_p{index}";
            }
        }

        private GridView BuildGridView(mzQuestion q)
        {
            GridView gridView = new();

            string header;
            string propertyName;

            for (int i = 1; i <= q.Headers.Count; i++)
            {
                header = q.Headers.Item(i).ToString().Trim();
                propertyName = GetPropertyName(i - 1);
                gridView.Columns.Add(CreateGridViewColumn(header, propertyName));
            }

            return gridView;
        }

        private ObservableCollection<ExpandoObject> GetItemsFromInput(mzQuestion q, TVALUES defaultAnswer)
        {
            ObservableCollection<ExpandoObject> allItems = new();
            ExpandoObject currentRow;
            string choice;
            TVALUES aVals;

            for (int i = 1; i <= q.Choices.Count; i++)
            {
                currentRow = new ExpandoObject();

                // Making sure the row has as many values as the number of columns
                choice = q.Choices.Item(i)?.ToString();
                if (q.ColCount > 1 && !string.IsNullOrWhiteSpace(q.ChoiceSubDelimeter))
                {
                    aVals = TVALUES.FromDelimitedList(choice, q.ChoiceSubDelimeter, true);
                    while (aVals.Count < q.ColCount)
                    {
                        aVals.Add("");
                    }
                }
                else
                {
                    aVals = new TVALUES(new List<dynamic>() { choice });
                }
                // Making sure the row has as many values as the number of columns

                // Add column values to the ExpandoObject as its properties
                for (int k = 1; k <= aVals.Count; k++)
                {
                    AddProperty(currentRow, GetPropertyName(k), aVals.Value(k));
                }
                // Add column values to the ExpandoObject as its properties

                // Check if the row is selected
                if (defaultAnswer.FindStringValue(aVals.Value(1)) > 0)
                {
                    AddProperty(currentRow, $"IsSelected", true);
                }
                else
                {
                    AddProperty(currentRow, $"IsSelected", false);
                }
                // Check if the row is selected

                allItems.Add(currentRow);
            }

            return allItems;
        }

        private ObservableCollection<MultiSelectItem> GetMultiSelectItems(mzQuestion q, TVALUES defaultAnswer)
        {
            ObservableCollection<MultiSelectItem> _rVal = new();
            //MultiSelectItem currentRow;
            string choice;
            //TVALUES aVals;
            List<string> choices = q.Choices.ToStringList();
            mzValues answers = q.Answers;
            for (int i = 1; i <= choices.Count; i++)
            {


                // Making sure the row has as many values as the number of columns
                choice = choices[i - 1];
                _rVal.Add(new MultiSelectItem(choice, answers.FindStringValue(choice) > 0));
                //if (!string.IsNullOrWhiteSpace(q.ChoiceSubDelimeter))
                //{
                //    aVals = TVALUES.FromDelimitedList(choice, q.ChoiceSubDelimeter, true);
                //    while (aVals.Count < q.ColCount)
                //    {
                //        aVals.Add("");
                //    }
                //}
                //else
                //{
                //    aVals = new TVALUES(new List<dynamic>() { choice });
                //}
                // Making sure the row has as many values as the number of columns

                // Add column values to the ExpandoObject as its properties
                //for (int k = 1; k <= aVals.Count; k++)
                //{
                //    AddProperty(currentRow, GetPropertyName(k), aVals.Value(k));
                //}
                //// Add column values to the ExpandoObject as its properties

                //// Check if the row is selected
                //if (defaultAnswer.FindStringValue(aVals.Value(1)) > 0)
                //{
                //    AddProperty(currentRow, $"IsSelected", true);
                //}
                //else
                //{
                //    AddProperty(currentRow, $"IsSelected", false);
                //}
                // Check if the row is selected

                //_rVal.Add(currentRow);
            }

            return _rVal;
        }

        private GridViewColumn CreateGridViewColumn(string headerName, string propertyToBind)
        {
            Binding binding = CreateBindingUsingPath(propertyToBind);

            GridViewColumn column = new()
            {
                Header = headerName,
                DisplayMemberBinding = binding
            };

            return column;
        }

        private Binding CreateBindingUsingPath(string propertyPath)
        {
            return new Binding() { Path = new PropertyPath(propertyPath) };
        }

        #endregion
    }
}
