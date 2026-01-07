using UOP.DXFGraphics;
using System;
using System.Windows.Input;
using UOP.WinTray.Projects;
using UOP.WinTray.UI.BusinessLogic;
using UOP.WinTray.UI.Commands;

namespace UOP.WinTray.UI.ViewModels
{
    // Added by CADfx
    public class UOPDocumentTab
    {
        public UOPDocumentTab(string aHeader = "")
        {
            HasBeenInitialized = false;
            Header = aHeader;
            Document = null;
        }
        public UOPDocumentTab(uopDocument aDocument, DelegateMenu aDocMenu = null , DelegateCommand aCloseTabCommand = null )
        {
            HasBeenInitialized = false;
            DocMenu = aDocMenu;
            Document = aDocument;
            Command_Close = aCloseTabCommand;
            if (aDocument != null)
            {
                Header = aDocument.TabName;
                ToolTip = aDocument.ToolTip;
            }
        }

        public UOPDocumentTab(uopDocument aDocument)
        {
            HasBeenInitialized = false;
         
            Document = aDocument;
            if (aDocument != null)
            {
                Header = aDocument.TabName;
                ToolTip = aDocument.ToolTip;
            }
        }

        public DelegateMenu DocMenu { get; set; }

        private DelegateCommand _Command_Close;
        public DelegateCommand Command_Close { get => _Command_Close; 
            set 
            {
                _Command_Close = value;
            } 
        }

        public string Header { get; set; }
        public string ToolTip { get; set; }
        public DXFViewerViewModel ViewModel { get; set; }
        public bool HasBeenInitialized { get; set; }
        public static UOP.DXFGraphicsControl.DXFViewer Viewer { get => DXFViewerViewModel.CADfxViewer; }

        private uopDocument _Document;
        public uopDocument Document 
        {
            get 
            {
                if (_Document == null) return null;

                uopTrayRange range = Range; 
                if (range != null) 
                    _Document.Range = range;

                uopPart part = Part;
                if (part != null)
                    _Document.Part = part;
                return _Document; 
            }
               
            set { _Document = value; if (value == null) return;  Range = value.Range; Part = value.Part; } 
        }

        private WeakReference<uopTrayRange> _RangeRef;
        /// <summary>
        /// returns the tray range that the tab is associated to
        /// </summary>
        public uopTrayRange Range
        {
            get
            {
                uopTrayRange _rVal = null;
                if (_RangeRef != null)
                {
                    if (!_RangeRef.TryGetTarget(out _rVal)) _RangeRef = null;
                }
                return _rVal;
            }
            set
            {
                if (value == null)
                {
                    _RangeRef = null;
                    return;
                }
                _RangeRef = new WeakReference<uopTrayRange>(value);
            }
        }


        private WeakReference<uopPart> _PartRef;
        /// <summary>
        /// returns the part that the tab is associated to
        /// </summary>
        public uopPart Part
        {
            get
            {
                if (_PartRef == null) return null;
                if (!_PartRef.TryGetTarget(out uopPart _rVal))
                    _PartRef = null;
                return _rVal;
            }
            set
            {
                if (value == null)
                {
                    _PartRef = null;
                    return;
                }
                _PartRef = new WeakReference<uopPart>(value);
            }
        }



        public override string ToString() => $"UOPDocumentTab[{ Header }]";



    }





    public class DXFViewerViewModel
    {
        public DXFViewerViewModel()
        {

        }

        private dxfImage _Image;
        public dxfImage Image
        {
            get => _Image;
            set
            {
                if (value == null && _Image != null) _Image.Dispose();
                _Image = value;
            }
        }

        public UOP.DXFGraphicsControl.CADLibModels CADModels { get; set; }
        public TitleBlockHelper titleBlockHelper { get; set; }
       
       
        public static UOP.DXFGraphicsControl.DXFViewer CADfxViewer { 
            get; 
            set;
        }
        public ICommand ZoomExtentCommand { get; set; }
        public ICommand ZoomInCommand { get; set; }
        public ICommand ZoomOutCommand { get; set; }
        public ICommand ZoomWindowCommand { get; set; }
        public ICommand ZoomPreviousCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand SaveCommandAndOpen { get; set; }

        public ICommand CloseCommand { get; set; }
        public ICommand CloseAllCommand { get; set; }
        public ICommand RegenerateCommand { get; set; }

    }

}
