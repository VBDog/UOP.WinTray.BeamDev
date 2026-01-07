using UOP.WinTray.UI.Converters;
using MvvmDialogs;
using System.Collections.ObjectModel;
using System.Windows;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.UI.Events.EventAggregator;
using UOP.WinTray.UI.Messages;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.UI.ViewModels
{
    /// <summary>
    /// Base class for all the classes that need to bind the data to the grid when reading function is done.
    /// </summary>
    public abstract class MDProjectViewModelBase : ViewModel_Base, IEventSubscriber<Message_UnloadProject>
    {

      

        #region Constructors

        internal MDProjectViewModelBase( IEventAggregator eventAggregator = null, IDialogService dialogService = null , uopProject project = null, ViewModel_Base parentVM = null) : base(eventAggregator, dialogService, project, parentVM)
        {
       
        }


        #endregion Constructors

        #region Properties

        private string _Caption;
        public string Caption { get => _Caption == null ? "Caption" : _Caption; set { _Caption = value; NotifyPropertyChanged("Caption"); } }

        public override uopProject Project
        {
            get => base.Project;
            set
            {
                base.Project = value;
                NotifyPropertyChanged("Project");
                UpdateProjectVisibility();

            }
        }
      

        public double Width
        {
            get
            {
                Window me = MyWindow();
                return me == null ? 800 : me.ActualWidth;
            }
            set
            {
                Window me = MyWindow();
                if (value <= 0) value = 800;
                if (me != null) me.Width = value;
            }
        }

        public double Height
        {
            get
            {
                Window me = MyWindow();
                return me == null ? 800 : me.ActualHeight;
            }
            set
            {
                Window me = MyWindow();
                if (value <= 0) value = 800;
                if (me != null) me.Height = value;
            }
        }


        // Added by CADfx
        private ObservableCollection<uopTrayRange> _RangeData;
        public virtual ObservableCollection<uopTrayRange> RangeData
        {
            get => _RangeData;
            set {
                ObservableCollection<uopTrayRange> oldData = _RangeData;

                bool doit = true;

                if (oldData != null && value != null)
                {
                    if (oldData.Count == value.Count)
                    {
                        for (int i = 1; i <= oldData.Count; i++)
                        {
                            if (string.Compare(value[i - 1].SelectName, oldData[i - 1].SelectName, ignoreCase: true) != 0)
                            {
                                doit = false;
                                break;
                            }
                        }
                    }
                }

                if (!doit) return;
                _RangeData = value;
                NotifyPropertyChanged("RangeData");
            }
        }

        public virtual uopTrayRange SelectedTrayRange => Project?.TrayRanges.SelectedRange;

        private mdTrayRange _MDRange;
        public mdTrayRange MDRange 
        { 
            get=> _MDRange == null? MDProject?.SelectedRange : _MDRange ;
            set => _MDRange = value;
        }

        public mdTrayAssembly MDAssy => MDRange?.TrayAssembly ;



        private Visibility _VisibilityMDDraw = Visibility.Collapsed;
        public virtual Visibility VisibilityMDDraw
        {
            get => _VisibilityMDDraw;
            set
            {
                _VisibilityMDDraw = value;
                NotifyPropertyChanged("VisibilityMDDraw");
                _VisibilityMDSpout = _VisibilityMDDraw == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                NotifyPropertyChanged("VisibilityMDSpout");

            }
        }

        private Visibility _Visibility_Progress = Visibility.Collapsed;
        public virtual Visibility Visibility_Progress
        {
            get => _Visibility_Progress;
            set { _Visibility_Progress = value; NotifyPropertyChanged("Visibility_Progress"); ; }
        }

        private string _StatusText = "";
        public virtual string StatusText
        {
            get => _StatusText;
            set { _StatusText = value; NotifyPropertyChanged("StatusText"); ; }
        }

        private Visibility _VisibilityMDSpout = Visibility.Collapsed;
        public Visibility VisibilityMDSpout
        {
            get => _VisibilityMDSpout;
            set
            {
                _VisibilityMDSpout = value;
                NotifyPropertyChanged("VisibilityMDSpout");
                _VisibilityMDDraw =  BooleanToVisibilityConverter.ConvertBool(_VisibilityMDSpout != Visibility.Visible);
                NotifyPropertyChanged("VisibilityMDDraw");
                VisibilityECMDDraw = (_VisibilityECMD == Visibility.Visible && _VisibilityMDDraw == Visibility.Visible) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

    

        private Visibility _VisibilityECMDDraw;
        public virtual Visibility VisibilityECMDDraw
        {
            get => _VisibilityECMDDraw;
            set { _VisibilityECMDDraw = value; NotifyPropertyChanged("VisibilityECMDDraw"); }
        }

        private Visibility _VisibilityECMD = Visibility.Collapsed;
        public virtual Visibility VisibilityECMD
        {
            get => _VisibilityECMD;
            set { _VisibilityECMD = value; NotifyPropertyChanged("VisibilityECMD"); VisibilityECMDDraw  = (_VisibilityECMD == Visibility.Visible && _VisibilityMDDraw == Visibility.Visible) ? Visibility.Visible : Visibility.Collapsed; }
        }

        private bool _ProjectIsMDSpout;
        public bool ProjectIsMDSpout { get => _ProjectIsMDSpout; set { _ProjectIsMDSpout = value; NotifyPropertyChanged("ProjectIsMDSpout"); } }

        private bool _ProjectIsMDDraw;
        public bool ProjectIsMDDraw { get => _ProjectIsMDDraw; set { _ProjectIsMDDraw = value; NotifyPropertyChanged("ProjectIsMDDraw"); } }

        #endregion Properties


        #region Methods



        public void UpdateProjectVisibility()
        {
            VisibilityProject = BooleanToVisibilityConverter.ConvertBool(Project != null);
            if (Project != null)
            {
                if(Project.ProjectFamily == uppProjectFamilies.uopFamMD)
                {
                    VisibilityMDProject = Visibility.Visible;
                    VisibilityMDDraw = (Project.ProjectType == uppProjectTypes.MDDraw) ? Visibility.Visible : Visibility.Collapsed;
                    uopTrayRange range = Project.TrayRanges.SelectedRange;
                    if (range == null)
                    {
                        
                        VisibilityECMD = Visibility.Collapsed;
                    }
                    else
                    {
                        VisibilityECMD = BooleanToVisibilityConverter.ConvertBool(Project.ProjectFamily == uppProjectFamilies.uopFamMD && range.DesignFamily.IsEcmdDesignFamily());
                    }
                    ProjectIsMDSpout = Project.ProjectType == uppProjectTypes.MDSpout;
                    ProjectIsMDDraw = !ProjectIsMDSpout;

                }
                else
                {
                    VisibilityMDProject = Visibility.Collapsed;
                    VisibilityMDDraw = Visibility.Collapsed;
                    VisibilityECMD = Visibility.Collapsed;
                    ProjectIsMDSpout = false;
                    ProjectIsMDDraw = false;

                }

            }
            else
            {
                VisibilityMDProject = Visibility.Collapsed;
                VisibilityMDDraw = Visibility.Collapsed;
                VisibilityECMD = Visibility.Collapsed;

                ProjectIsMDSpout = false;
                ProjectIsMDDraw = false;
            }

        }

        public override void ReleaseReferences()
        {
            base.ReleaseReferences();
           
            EditProps = null;
            _MDRange = null;
            Viewer = null;
          
          
        }
        #endregion Methods
    }
}
