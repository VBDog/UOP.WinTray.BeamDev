using MvvmDialogs;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using UOP.WinTray.UI.Commands;

namespace UOP.WinTray.UI.ViewModels
{
    public class SortListViewModel : BindingObject, IModalDialogViewModel
    {

        #region Constructors

        public SortListViewModel(List<string> aListToSort, string aTitle = "Sort List") 
        {
            aListToSort ??= new List<string>();
            List<string> input = aListToSort.FindAll(x => !string.IsNullOrWhiteSpace(x));
            InputList = input;
            OutputList = new List<string>();
            Title = aTitle;
            ListCount = InputList.Count;
            OriginalList = new List<string>();
            OriginalList.AddRange(InputList);
        }


        #endregion Constructors

        #region Properties
        private List<string> _OriginalList = new();
        public List<string> OriginalList { get => _OriginalList; set { value ??= new List<string>(); _OriginalList = value; NotifyPropertyChanged("OriginalList"); } }


        private int ListCount { get; set; }

        private bool? _DialogResult;
        public bool? DialogResult { get => _DialogResult; private set { _DialogResult = value; NotifyPropertyChanged("DialogResult"); } }

        private string _Title = "Sort List";
        public string Title { get => _Title; private set { value ??= "Sort List"; _Title = value; NotifyPropertyChanged("Title"); } }
        private List<string> _InputList = new();
        public List<string> InputList { get => _InputList; set { value ??= new List<string>(); _InputList = value; NotifyPropertyChanged("InputList"); } }

        private List<string> _OutputList = new();
        public List<string> OutputList { get => _OutputList; set { value ??= new List<string>(); _OutputList = value; NotifyPropertyChanged("OutputList"); } }


        private int _InputListSelectedIndex = -1;
        public int InputListSelectedIndex { get => _InputListSelectedIndex; set { _InputListSelectedIndex = value; NotifyPropertyChanged("InputListSelectedIndex"); } }
       
        private int _OutputListSelectedIndex = -1;
        public int OutputListSelectedIndex { get => _OutputListSelectedIndex; set { _OutputListSelectedIndex = value; NotifyPropertyChanged("OutputListSelectedIndex"); } }

        #endregion Properties

        #region Commands

        private DelegateCommand _CMD_Cancel;
        public ICommand Command_Cancel { get { _CMD_Cancel ??= new DelegateCommand(param => Execute_Cancel()); return _CMD_Cancel; } }

        private DelegateCommand _CMD_OK;
        public ICommand Command_OK { get { _CMD_OK ??= new DelegateCommand(param => Execute_Save()); return _CMD_OK; } }


        private ICommand _CMD_InputListDoubleClick;
        public ICommand Command_InputListDoubleClick { get { _CMD_InputListDoubleClick ??= new DelegateCommand(param => MoveRight()); return _CMD_InputListDoubleClick; } }

        private ICommand _CMD_OutputListDoubleClick;
        public ICommand Command_OutputListDoubleClick { get { _CMD_OutputListDoubleClick ??= new DelegateCommand(param => MoveLeft()); return _CMD_OutputListDoubleClick; } }

        #endregion Commands

        #region Methods
        /// <summary>
        /// Close the Edit form with no changes
        /// </summary>
        private void Execute_Cancel() => DialogResult = false;
        
        private void Execute_Save()
        {
            if(ListCount > 0 && OutputList.Count < ListCount)
            {
                MessageBox.Show("All Members Must Be Added To The New Order List!", "Incomplete Sort", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            bool? result = false;
            for(int i = 0; i < OutputList.Count; i++)
            {
                if(i < OriginalList.Count)
                {
                    if(string.Compare(OutputList[i], OriginalList[i],true) != 0)
                    {
                        result = true;
                    }
                }
                else
                {
                    result = true;
                }
            }
            
            DialogResult = result;

        }

        public void MoveRight()
        {
        
            List<string> input = new();
            List<string> output = new();
            output.AddRange(OutputList);
            for (int i = 0; i < InputList.Count; i++)
            {
                if(i == InputListSelectedIndex)
                {
                    output.Add(InputList[i]);
                }
                else
                {
                    input.Add(InputList[i]);
                }
            }
        
            InputList = input;
            OutputList = output;
          
        }
        public void MoveLeft()
        {

            List<string> input = new();
            List<string> output = new();
            input.AddRange(InputList);
            for (int i = 0; i < OutputList.Count; i++)
            {
                if (i == OutputListSelectedIndex)
                {
                    input.Add(OutputList[i]);
                }
                else
                {
                    output.Add(OutputList[i]);
                }

            }

            InputList = input;
            OutputList = output;
        }
        #endregion Methods
    }
}
