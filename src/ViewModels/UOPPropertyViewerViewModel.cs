using MvvmDialogs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.UI.Commands;
using UOP.WinTray.UI.Model;

namespace UOP.WinTray.UI.ViewModels
{
    /// <summary>
    /// popup window data binding for tree view nodes
    /// </summary>
    public class UOPPropertyViewerViewModel : BindingObject, IModalDialogViewModel
    {
       
        #region Constructor
        public UOPPropertyViewerViewModel(uopProperties aProperties, uppUnitFamilies aDisplayUnits, string aFirstNodeName, uopProperties aSubProperties, string aSubPropLable = "", bool bSuppressNullProperties = false)
        {
            aProperties ??= new uopProperties();
           TreeViewNodes = new ObservableCollection<TreeViewNode>();
            _IsEnglishSelected = aDisplayUnits == uppUnitFamilies.English;
            FirstNodeName = aFirstNodeName;
            SubNodeName = aSubPropLable;
            SuppressNullProperties = bSuppressNullProperties;
            _Properties = aProperties;
            NotifyPropertyChanged("Properties");
            SubProperties = aSubProperties;
            ReloadProperties(true);

        }
        #endregion Constructor

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
        public bool SuppressNullProperties { get; set; }
        public string FirstNodeName { get; set; }
        public string SubNodeName { get; set; }

        private uopProperties _Properties= null ;
        public uopProperties Properties
        {
            get => _Properties;
            set
            {
                bool init = _Properties == null;
                value ??= new uopProperties("");
                _Properties = value;
                NotifyPropertyChanged("Properties");
                ReloadProperties(init);


            }
        }

        private uopProperties _SubProperties = null;
        public uopProperties SubProperties
        {
            get => _SubProperties;
            set
            {
             
                value ??= new uopProperties("");
                _SubProperties = value;
                NotifyPropertyChanged("SubProperties");
            }
        }

        private UOPPropertyTreeViewModel _TreeViewModel;
        public UOPPropertyTreeViewModel TreeViewModel{get => _TreeViewModel; set { _TreeViewModel = value; NotifyPropertyChanged("TreeViewModel"); } }

       // private ObservableCollection<TreeViewNode> _TreeViewNodes;
        public ObservableCollection<TreeViewNode> TreeViewNodes
        {
            get { TreeViewModel ??= new UOPPropertyTreeViewModel(); return TreeViewModel.TreeViewNodes; }  //_TreeViewNodes;

            set
            {
                value ??= new ObservableCollection<TreeViewNode>();
                TreeViewModel ??= new UOPPropertyTreeViewModel();
                TreeViewModel.TreeViewNodes = value;
                NotifyPropertyChanged("TreeViewNodes");


            }

                //_TreeViewNodes = value; NotifyPropertyChanged("TreeViewNodes"); }
        }

      
        private bool _IsEnglishSelected;
        public bool IsEnglishSelected
        {
            get => _IsEnglishSelected;
            set
            {
                _IsEnglishSelected = value;
                NotifyPropertyChanged("IsEnglishSelected");
                NotifyPropertyChanged("IsMetricSelected");
                ReloadProperties();
            }
        }

 
        public bool IsMetricSelected { get => !_IsEnglishSelected; set { IsEnglishSelected = !value; } }
        
       

        private bool _ShowHidden;
        public bool ShowHidden { get => _ShowHidden; set {  _ShowHidden = value; NotifyPropertyChanged("ShowHidden"); ReloadProperties(); } }


        private Visibility _Visibility_ShowHidden = Visibility.Collapsed;
        public Visibility Visibility_ShowHidden
        {
            get => _Visibility_ShowHidden;
            set { _Visibility_ShowHidden = value; NotifyPropertyChanged("Visibility_ShowHidden"); }

        }

        private Visibility _Visibility_ShowUnits = Visibility.Visible;
        public Visibility Visibility_ShowUnits
        {
            get => _Visibility_ShowUnits;
            set { _Visibility_ShowUnits = value; NotifyPropertyChanged("Visibility_ShowUnits"); }

        }

        public uppUnitFamilies DisplayUnits { get => IsEnglishSelected == true ? uppUnitFamilies.English : uppUnitFamilies.Metric; }

        #endregion Properties

        #region Commands

        private ICommand _Cmd_Close;

        public ICommand Command_Close { get { _Cmd_Close ??= new DelegateCommand(param => Execute_Close()); return _Cmd_Close; } }

   
        #endregion Properties

        #region Methods

        private void ReloadProperties(bool Init = false)
        {
            ObservableCollection<TreeViewNode> nodes = new();
           
            if (_Properties != null) _Properties.DisplayUnits = DisplayUnits;
            if (_SubProperties != null) _SubProperties.DisplayUnits = DisplayUnits;

            var topnode = new TreeViewNode(string.IsNullOrWhiteSpace(FirstNodeName) ? "PROPERTIES" : FirstNodeName);
            var subnodes = new TreeViewNode(string.IsNullOrWhiteSpace(SubNodeName) ? $"{topnode.NodeName}.SUBPROPERTIES": SubNodeName);
            uppUnitFamilies units = DisplayUnits;
            int ucount = 0;
            nodes.Add(topnode);
            List<uopProperty> vprops = (_Properties != null) ? _Properties.GetByHidden(false) : new List<uopProperty>();
            List<uopProperty> hprops = (_Properties != null) ? _Properties.GetByHidden(true) : new List<uopProperty>();
            List<uopProperty> subvprops = (_SubProperties != null) ? _SubProperties.GetByHidden(false) : new List<uopProperty>();
            List<uopProperty> subhprops = (_SubProperties != null) ? _SubProperties.GetByHidden(true) : new List<uopProperty>();
            Visibility_ShowHidden = hprops.Count + subhprops.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            if (SuppressNullProperties) 
            {
                vprops = vprops.FindAll(x => x.IsNullValue == false);
                hprops = hprops.FindAll(x => x.IsNullValue == false);
                subvprops = subvprops.FindAll(x => x.IsNullValue == false);
                subhprops = subhprops.FindAll(x => x.IsNullValue == false);
            }

            //vprops.Sort(comparison:string);
            //hprops.Sort();
            //subvprops.Sort();
            //subhprops.Sort();

            foreach (var item in vprops)
            {
                item.DisplayUnits = units;
                if (item.HasUnits && item.UnitType != uppUnitTypes.Percentage && item.UnitType != uppUnitTypes.BigPercentage) ucount++;
                var pnode = new TreeViewNode(item.DisplaySignature, null, topnode);

            }
            if ( ShowHidden) 
            {
                foreach (var item in hprops)
                {
                    item.DisplayUnits = units;
                    
                    if (item.HasUnits && item.UnitType != uppUnitTypes.Percentage && item.UnitType != uppUnitTypes.BigPercentage) ucount++;
                    var pnode = new TreeViewNode(item.DisplaySignature, null, topnode);
                    pnode.Colour = "Gray";
                }
            }
            if(subvprops.Count + subhprops.Count > 0 )
            {

                foreach (var item in subvprops)
                {
                    item.DisplayUnits = units;
                    if (item.HasUnits && item.UnitType != uppUnitTypes.Percentage && item.UnitType != uppUnitTypes.BigPercentage) ucount++;
                    var pnode = new TreeViewNode(item.DisplaySignature, null, subnodes);

                }
                if (ShowHidden)
                {
                    foreach (var item in subhprops)
                    {
                        item.DisplayUnits = units;

                        if (item.HasUnits && item.UnitType != uppUnitTypes.Percentage && item.UnitType != uppUnitTypes.BigPercentage) ucount++;
                        var pnode = new TreeViewNode(item.DisplaySignature, null, subnodes);
                        pnode.Colour = "Gray";
                    }
                }
               
            }
        
          

           

            Visibility_ShowUnits = ucount > 0 ? Visibility.Visible : Visibility.Collapsed;

            if(subnodes.Members.Count > 0) nodes.Add(subnodes);
            TreeViewNodes = nodes;
            topnode.IsExpanded = true;
        }
    
        /// <summary>
        /// close the popup
        /// </summary>
        private void Execute_Close()
        {
            DialogResult = true;
        }

       

        #endregion Methods
    }
}
