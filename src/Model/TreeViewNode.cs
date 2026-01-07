using UOP.WinTray.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.UI.Utilities.Constants;

namespace UOP.WinTray.UI.Model
{
    /// <summary>
    /// Tree view model class
    /// </summary>
    public class TreeViewNode : BindingObject
    {


        #region Constructor

        public TreeViewNode(string aNodeName, uopPart aPart, TreeViewNode aParent = null, int aPartIndex = 0, List<TreeViewNode> aCollector = null, uopDocument aDocument = null, string aColour = "Black")
        {
            
            NodeName = aNodeName;
            IsExpanded = false;
            Colour = aColour;
            Hidden = false;
            Members = new List<TreeViewNode>();
            PartType = aPart == null ? uppPartTypes.Undefined : aPart.PartType;
            ParentNode = aParent == null ? "" : aParent.Path;
            Pid = aPartIndex;
            Part = aPart;
            Document = aDocument;
            if (aParent != null) aParent.Members.Add(this);
            if (aCollector != null)
                aCollector.Add(this);
        }

        public TreeViewNode(string aNodeName, uppPartTypes aPartType = uppPartTypes.Undefined, string aParentNode = "", int aPartIndex = 0, List<TreeViewNode> aCollector = null, uopDocument aDocument = null, string aColour = "Black")
        {
            NodeName = aNodeName;
            IsExpanded = false;
            Colour = aColour;
            Hidden = false;
            Members = new List<TreeViewNode>();
            PartType = aPartType;
            ParentNode = aParentNode;
            Document = aDocument;
            Pid = aPartIndex;
            if (aCollector != null)
                aCollector.Add(this);
        }


        public TreeViewNode(uopProperty aProperty, string aParentNode = "", List<TreeViewNode> aCollector = null, uopDocument aDocument = null, string aColour = "Black")
        {
            if (aProperty == null) aProperty = new uopProperty("NullPropery", "");
            NodeName = aProperty.DisplayName + "=" + aProperty.ValueS;
            IsExpanded = false;
            Colour = aColour;
            Hidden = aProperty.Hidden;
            Members = new List<TreeViewNode>();
            PartType = aProperty.PartType;
            ParentNode = aParentNode;
            Pid = aProperty.PartIndex;
            Value = aProperty.UnitValueString();
            Name = aProperty.Name;
            Units = aProperty.Units.Clone();
            Document = aDocument;
            if (!string.IsNullOrEmpty(aProperty.DecodeString))
            {
                Value = aProperty.DecodedValue;

            }
            else
            {
                Value = aProperty.ValueS;
            }

            NodeName = $"{ Name } = { Value}";
            if (Hidden)
                Colour = CommonConstants.GRAY;
            else
                Colour = CommonConstants.BLACK;
            if (aCollector != null)
                aCollector.Add(this);
        }

        #endregion Constructors

        #region Properties

      

        private string _NodeName;
        public string NodeName { get => _NodeName; set { _NodeName = value; NotifyPropertyChanged("NodeName"); NotifyPropertyChanged("ToolTip"); } }

        private string _ToolTip;
        public string ToolTip { get => string.IsNullOrWhiteSpace(_ToolTip) ? NodeName : _ToolTip; set { _ToolTip = value; NotifyPropertyChanged("ToolTip"); NotifyPropertyChanged("NodeName"); } }



        private string _Colour;

        public string Colour { get => _Colour; set { value ??= "Black"; _Colour = value; NotifyPropertyChanged("Colour"); } }

        public string Path { get; set; }
      
        public int ClickCount { get; set; }

        public ObservableCollection<ProjectTreeViewModel> ProjectCollection { get; set; }
        private bool _IsExpanded;
        public bool IsExpanded { get => _IsExpanded; set { _IsExpanded = value; NotifyPropertyChanged("IsExpanded"); } }
        private bool _IsBold;
        public bool IsBold { get => _IsBold; set { _IsBold = value; NotifyPropertyChanged("IsBold"); NotifyPropertyChanged("FontWeight"); } }



      
        public string FontWeight { get => IsBold ? "Bold" : "Normal"; }

        private bool _IsSelected;
        public bool IsSelected { get => _IsSelected; set { _IsSelected = value; NotifyPropertyChanged("IsSelected"); } }

        public bool Hidden { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public uopUnit Units { get; set; }
        public string ParentNode { get; set; }
        public int Pid { get; set; }
        public uppPartTypes PartType { get; set; }
        public uppDocumentTypes DocumentType { get; set; }
        public string DocumentName { get; set; }

        public List<TreeViewNode> Members { get; set; }
        public uopDocument CADfxCorrespondingDrawing { get; set; }

        public List<uopProperty> SourceProps { get; set; }
        private WeakReference<uopPart> _PartRef;
        public uopPart Part
        {
            get
            {
                if (_PartRef == null) return null;
                if(!_PartRef.TryGetTarget(out uopPart _rVal)) _PartRef = null;
               return _rVal;
                
            }
            set
            {
                if(value == null)
                {
                    _PartRef = null;
                    return;
                }
                _PartRef = new WeakReference<uopPart>(value);
            }
        }

        private WeakReference<uopDocument> _DocRef;
        public uopDocument Document
        {
            get
            {
                if (_DocRef == null) return null;
                if (!_DocRef.TryGetTarget(out uopDocument _rVal)) _DocRef = null;
                return _rVal;

            }
             set{
                if (value == null)
                {
                    _DocRef = null;
                    return;
                }
                _DocRef = new WeakReference<uopDocument>(value);
            }
        }
        #endregion Properties


        #region Methods

        public override string ToString() => $"TreeViewNode[{ NodeName }]";
        


        #endregion Methods
    }
}
