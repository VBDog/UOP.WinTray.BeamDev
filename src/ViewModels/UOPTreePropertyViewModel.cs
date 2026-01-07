using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.UI.Commands;
using UOP.WinTray.UI.Model;
using MvvmDialogs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;


namespace UOP.WinTray.UI.ViewModels
{
    /// <summary>
    /// popup window data binding for tree view nodes
    /// </summary>
    public class UOPTreePropertyViewModel : BindingObject, IModalDialogViewModel
    {
        #region Constant

        private const string EQUALVALUE = "=";
        private const string NULVALUE = "-999";
        private const string SPACE = " ";

        #endregion

        #region Variables


        #endregion

        #region Constructor
        public UOPTreePropertyViewModel(IDialogService dialogService)
        {
         
            //_DialogService = dialogService;

        }
        #endregion

        #region Properties

        /// <summary>
        /// Dialogservice result
        /// </summary>
        private bool? _DialogResult;
        public bool? DialogResult
        {
            get => _DialogResult;
            private set { _DialogResult = value; NotifyPropertyChanged(nameof(DialogResult)); }
        }


        public double Multiplier { get; set; }

        private ObservableCollection<TreeViewNode> _TreeViewNode;
        public ObservableCollection<TreeViewNode> TreeViewNodes
        {
            get => _TreeViewNode;
            
            set { value ??= new ObservableCollection<TreeViewNode>(); _TreeViewNode = value; NotifyPropertyChanged("TreeViewNodes"); }
        }

        private ObservableCollection<TreeViewNode> _CopyTreeViewNode;
        public ObservableCollection<TreeViewNode> CopyTreeViewNode
        {
            get => _CopyTreeViewNode;
            
            set { _CopyTreeViewNode = value; NotifyPropertyChanged("CopyTreeViewNode"); }
        }
        private bool _IsEnglish;
        public bool IsEnglish
        {
            get => _IsEnglish;
            set
            {
                _IsEnglish = value;
                if (value)
                {
                    IsMetric = false;
                    Multiplier = 1;
                    ShowUnits();
                }
                NotifyPropertyChanged("IsEnglish");
            }
        }

        private bool _IsMetric;
        public bool IsMetric
        {
            get => _IsMetric;
            set
            {
                _IsMetric = value;
                if (value)
                {
                    IsEnglish = false;
                    Multiplier = 25.4;
                    ShowUnits();
                }
                NotifyPropertyChanged("IsMetric");
            }
        }

        private bool _ShowHidden;

        public bool ShowHidden
        {
            get => _ShowHidden;
            set
            {
                _ShowHidden = value;
                ShowHiddenProperties();
                NotifyPropertyChanged("ShowHidden");
            }
        }

        private string _HeaderText;

        public string HeaderText
        {
            get => _HeaderText;
            set {_HeaderText = value; NotifyPropertyChanged("HeaderText"); }
        }

        private string _SelectedText;

        public string SelectedText
        {
            get => _SelectedText;
            set {_SelectedText = value; NotifyPropertyChanged("SelectedText"); }
          
        }


        #endregion

        #region Commands

        private ICommand _Cmd_Close;

        public ICommand Command_Close
        {
            get
            {
                if (_Cmd_Close == null) _Cmd_Close = new DelegateCommand(param => Close());
               
                return _Cmd_Close;
            }

        }

        

        #endregion

        #region Methods

        /// <summary>
        /// display the units 
        /// </summary>
        public void ShowUnits()
        {
            TreeViewNodes = new ObservableCollection<TreeViewNode>();

            foreach (var item in CopyTreeViewNode)
            {
                TreeViewNode nodeType = new(item.NodeName);
                nodeType.Members = new List<TreeViewNode>();
                HeaderText = item.NodeName;
                for (int i = 0; i < item.Members.Count; i++)
                {
                    var node = item.Members[i];
                    if (item.Members[i].Units != null && node.Value != NULVALUE && item.Members[i].Units.IsDefined)
                    {
                        node.NodeName = $"{node.Name}={GetValue(node.Value, node.Units)}";
                    }
                    else if (node.Name != null)
                    {
                        if (node.Value == NULVALUE)
                            node.Value = string.Empty;
                        node.NodeName = $"{node.Name}={ node.Value}";
                    }
                    if (ShowHidden)
                    {
                        nodeType.Members.Add(node);
                    }
                    else if (!node.Hidden)
                    {
                        nodeType.Members.Add(node);
                    }
                }
                TreeViewNodes.Add(nodeType);
            }
        }

        /// <summary>
        /// get the unit values
        /// </summary>
        /// <param name="value"></param>
        /// <param name="Units"></param>
        /// <returns></returns>
        private string GetValue(string value, uopUnit Units)
        {
            var aUnitFamily = IsMetric? uppUnitFamilies.Metric : uppUnitFamilies.English;
            
            if (!string.IsNullOrEmpty(value))
            {

                if (!mzUtils.IsNumeric(value))
                {
                    return value;
                }
                var aVar = Units.ConvertValue( value, Units.UnitSystem, aUnitFamily);
                var aLable = Units.Label(aUnitFamily);
                var aFmat = Units.FormatString( aUnitFamily);
                var aVal = aFmat != string.Empty ? aVar.ToString(aFmat) : aVar.ToString();
                return (aVal + SPACE + aLable).ToString().Trim();
            }
            else
            {
                return value;
            }
        }

        /// <summary>
        /// show hidden properties
        /// </summary>
        public void ShowHiddenProperties()
        {
            TreeViewNodes = new ObservableCollection<TreeViewNode>();
            foreach (var item in CopyTreeViewNode)
            {
                TreeViewNode nodeType = new(item.NodeName);
                nodeType.Members = new List<TreeViewNode>();
                for (int i = 0; i < item.Members.Count; i++)
                {
                    if (ShowHidden || !item.Members[i].Hidden)
                    {
                        var node = item.Members[i];
                        nodeType.Members.Add(node);
                    }
                }
                TreeViewNodes.Add(nodeType);
            }
        }

        /// <summary>
        /// close the popup
        /// </summary>
        private void Close()
        {
            DialogResult = true;
        }

        /// <summary>
        /// Display selected text
        /// </summary>
        /// <param name="param"></param>
        private void DisplaySelectedText(object param)
        {
            SelectedText = param.ToString();
        }

        #endregion
    }
}
