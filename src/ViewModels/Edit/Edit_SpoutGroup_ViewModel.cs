using DocumentFormat.OpenXml.Drawing.Charts;
using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UOP.DXFGraphics;
using UOP.DXFGraphicsControl;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Documents;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.UI.BusinessLogic;
using UOP.WinTray.UI.Commands;
using UOP.WinTray.UI.Events.EventAggregator;
using UOP.WinTray.UI.Messages;
using UOP.WinTray.UI.Views;

namespace UOP.WinTray.UI.ViewModels
{
    /// <summary>
    /// Class for Edit _Spout properties.
    /// </summary>
    public class Edit_SpoutGroup_ViewModel : MDProjectViewModelBase, IModalDialogViewModel
    {
        #region Constants

        private const string BLUE = "Blue";
        private const string RED = "Red";
        private const string BLACK = "Black";

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Edit _Spout properties viewmodel constructor.
        /// </summary>
        internal Edit_SpoutGroup_ViewModel(IEventAggregator eventAggregator,
                                              IDialogService dialogService,
                                               mdProject project, int spoutgroupindex
                                              ) : base(eventAggregator, dialogService: dialogService)
        {
            //eventAggregator = eventAggregator;


            MDProject = project;

            IsEscapeEnabled = true;
            SpoutGroupIndex = spoutgroupindex;
            IsEnglishSelected = Project.DisplayUnits == uppUnitFamilies.English;

        }

        #endregion Constructors

        #region Properties


        public bool ApplyToGroup { get; set; }

        private string SpoutString { get; set; }

        private mdDowncomer Downcomer { get; set; }

        private mdSpoutGroup SpoutGroup { get; set; }

        private int SpoutGroupIndex { get; set; }

        private mdConstraint _Constraints;
        private mdConstraint Constraints { get => _Constraints; set => _Constraints = value; }

        private DowncomerInfo DCInfo => SpoutGroup == null ? new DowncomerInfo() : SpoutGroup.DCInfo;
        private mdSpoutGrid _Grid;
        private mdSpoutGrid Grid { get { if (_Grid != null) _Grid.Constraints = Constraints;  return _Grid; } set => _Grid = value; }
        private double DefaultClearance { get; set; }

        private double MinClearance { get; set; }

        private bool ConstraintChanged { get; set; }

        private bool IsOkEnabled { get => base.IsOkBtnEnable; set => base.IsOkBtnEnable = value; }

        public IEnumerable<Tuple<string, Enum>> PatternTuples { get; set; }

        public bool NoHPitch => true;

        private bool? _DialogResult;

        /// <summary>
        /// Dialogservice result
        /// </summary>
        public bool? DialogResult
        {
            get => _DialogResult;
            private set { _DialogResult = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private bool _CanApplyChangesToAllGroups;

        /// <summary>
        /// Apply changes to all groups.
        /// </summary>
        public bool ApplyChangesToAllGroupMembers
        {
            get => _CanApplyChangesToAllGroups;
            set { _CanApplyChangesToAllGroups = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private bool _IsTextEnabled = true;
        public bool IsTextEnabled
        {
            // removing IsTextEnabled, IsFocus or Value causes Binding Failures but
            // none of the view elements are bound to them.  They are here to prevent the failures.
            get => _IsTextEnabled;
            set { _IsTextEnabled = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private bool _IsFocus;
        public bool IsFocus
        {
            // removing IsTextEnabled, IsFocus or Value causes Binding Failures but
            // none of the view elements are bound to them.  They are here to prevent the failures.
            get => _IsFocus;
            set { _IsFocus = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private string _Value;
        public string Value
        {
            // removing IsTextEnabled, IsFocus or Value causes Binding Failures but
            // none of the view elements are bound to them.  They are here to prevent the failures.
            get => _Value;
            set { _Value = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private bool _IsHorizontalPitchEnabled;
        /// <summary>
        /// Is Horizontal pitch enabled.
        /// </summary>
        public bool IsHorizontalPitchEnabled
        {
            get => _IsHorizontalPitchEnabled;
            set { _IsHorizontalPitchEnabled = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }


        private bool _IsSpoutLengthEnabled;
        /// <summary>
        /// Is _Spout length enabled.
        /// </summary>
        public bool IsSpoutLengthEnabled
        {
            get => _IsSpoutLengthEnabled;
            set { _IsSpoutLengthEnabled = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }



        private Visibility _Visibility_ApplyToGoupMember = Visibility.Visible;
        public Visibility Visibility_ApplyToGoupMember
        {
            get => _Visibility_ApplyToGoupMember;
            set { _Visibility_ApplyToGoupMember = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private Visibility _Visibility_EndPlateClearance = Visibility.Visible;
        public Visibility Visibility_EndPlateClearance
        {
            get => _Visibility_EndPlateClearance;
            set { _Visibility_EndPlateClearance = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }
        private Visibility _Visibility_AppliedEndPlateClearance = Visibility.Visible;
        public Visibility Visibility_AppliedEndPlateClearance
        {
            get => _Visibility_AppliedEndPlateClearance;
            set { _Visibility_AppliedEndPlateClearance = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private Visibility _Visibility_ActualHPitch = Visibility.Visible;
        public Visibility Visibility_ActualHPitch
        {
            get => _Visibility_ActualHPitch;
            set { _Visibility_ActualHPitch = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private Visibility _Visibility_ActualVPitch = Visibility.Visible;
        public Visibility Visibility_ActualVPitch
        {
            get => _Visibility_ActualVPitch;
            set { _Visibility_ActualVPitch = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private Visibility _Visibility_VerticalPitch = Visibility.Visible;
        public Visibility Visibility_VerticalPitch
        {
            get => _Visibility_VerticalPitch;
            set { _Visibility_VerticalPitch = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private Visibility _Visibility_SlotLength = Visibility.Visible;
        public Visibility Visibility_SlotLength
        {
            get => _Visibility_SlotLength;
            set { _Visibility_SlotLength = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private Visibility _Visibility_SpoutCount = Visibility.Visible;
        public Visibility Visibility_SpoutCount
        {
            get => _Visibility_SpoutCount;
            set { _Visibility_SpoutCount = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private Visibility _Visibility_YOffset = Visibility.Visible;
        public Visibility Visibility_YOffset
        {
            get => _Visibility_YOffset;
            set { _Visibility_YOffset = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }


        private Visibility _Visibility_ConstraintsFrame = Visibility.Visible;
        public Visibility Visibility_ConstraintsFrame
        {
            get => _Visibility_ConstraintsFrame;
            set
            {
                _Visibility_ConstraintsFrame = value;
                NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod());
                Visibility_ConstraintsButton = (value == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
                Visibility_ControlBox = (value == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private Visibility _Visibility_ConstraintsButton = Visibility.Visible;
        public Visibility Visibility_ConstraintsButton
        {
            get => _Visibility_ConstraintsButton;
            set { _Visibility_ConstraintsButton = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private Visibility _Visibility_ControlBox = Visibility.Visible;
        public Visibility Visibility_ControlBox
        {
            get => _Visibility_ControlBox;
            set { _Visibility_ControlBox = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private bool _IsEscapeEnabled;
        public bool IsEscapeEnabled
        {
            get => _IsEscapeEnabled;
            set { _IsEscapeEnabled = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); IsEscapeDisabled = !value; }
        }

        private bool _IsEscapeDisabled;
        public bool IsEscapeDisabled
        {
            get => _IsEscapeDisabled;
            set { _IsEscapeDisabled = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private uppSpoutPatterns _SelectedPatternType = uppSpoutPatterns.Undefined;
        /// <summary>
        /// Selected pattern type
        /// </summary>
        public uppSpoutPatterns SelectedPatternType
        {
            get => _SelectedPatternType;

            set
            {
                if (value != _SelectedPatternType)
                {
                    if (EditProps.SetProperty("PatternType", value))
                    {
                        EditProps = mdUtils.SetConstraintPropertyLimits(EditProps, SpoutGroup, Grid);

                        EditProps.SetToDefault("SpoutCount");
                        if (value != uppSpoutPatterns.S1 && value != uppSpoutPatterns.S2 && value != uppSpoutPatterns.S3)
                        {
                            EditProps.SetToDefault("SpoutLength");
                        }
                        else
                        {
                            var aProp = EditProps.Item("SpoutLength");
                            if (aProp.Value > aProp.MaxValue)
                                EditProps.SetToDefault("SpoutLength");
                        }
                    }
                    ShowConstraints(true);
                    _SelectedPatternType = value;
                    NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod());
                }
            }
        }

        private int _PatternNameSelectedIndex;
        /// <summary>
        /// Pattern name selected index.
        /// </summary>
        public int PatternNameSelectedIndex
        {
            get => _PatternNameSelectedIndex;
            set { _PatternNameSelectedIndex = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }


        private double? _ConstraintVerticalPitch;
        /// <summary>
        /// Constraint vertical pitch.
        /// </summary>
        public double? ConstraintVerticalPitch
        {
            get => _ConstraintVerticalPitch;
            set { _ConstraintVerticalPitch = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private double? _ConstraintHorizontalPitch;
        /// <summary>
        /// Constraint _Horizontal pitch.
        /// </summary>
        public double? ConstraintHorizontalPitch
        {
            get => _ConstraintHorizontalPitch;
            set { _ConstraintHorizontalPitch = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private int? _ConstraintSpoutCount;
        /// <summary>
        /// Constraint _Spout count.
        /// </summary>
        public int? ConstraintSpoutCount
        {
            get => _ConstraintSpoutCount;
            set { _ConstraintSpoutCount = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private double? _ConstraintSpoutLength;
        /// <summary>
        /// Constraint _Spout length.
        /// </summary>
        public double? ConstraintSpoutLength
        {
            get => _ConstraintSpoutLength;
            set { _ConstraintSpoutLength = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private double? _ConstraintClearance;
        /// <summary>
        /// Constraint clearance.
        /// </summary>
        public double? ConstraintClearance
        {
            get => _ConstraintClearance;
            set { _ConstraintClearance = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private double? _ConstraintEndPlateClearance;
        /// <summary>
        /// Constraint end plate clearance.
        /// </summary>
        public double? ConstraintEndPlateClearance
        {
            get => _ConstraintEndPlateClearance;
            set { _ConstraintEndPlateClearance = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private double? _ConstraintYOffset;
        /// <summary>
        /// Constraint YOffset.
        /// </summary>
        public double? ConstraintYOffset
        {
            get => _ConstraintYOffset;
            set { _ConstraintYOffset = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private double? _ConstraintMargin;
        /// <summary>
        /// Constraint margin.
        /// </summary>
        public double? ConstraintMargin
        {
            get => _ConstraintMargin;
            set { _ConstraintMargin = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private double? _ConstraintSpoutDiameter;
        /// <summary>
        /// Constraint _Spout diameter.
        /// </summary>
        public double? ConstraintSpoutDiameter
        {
            get => _ConstraintSpoutDiameter;
            set { _ConstraintSpoutDiameter = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        /// <summary>
        /// Set To Ideal
        /// </summary>
        public bool IsSetToIdeal => (Constraints != null) && Constraints.TreatAsIdeal;

        /// <summary>
        /// Group Member
        /// </summary>

        public bool IsGroupMember => (Constraints != null) && Constraints.TreatAsGroup;


        /// <summary>
        /// Metric Spouts
        /// </summary>
        public bool IsMetricSpouts => MDAssy == null || MDAssy.MetricSpouting;

        private string _PatternTypeName;
        /// <summary>
        /// Pattern type.
        /// </summary>
        public string PatternType
        {
            get => _PatternTypeName;
            set { _PatternTypeName = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private string _PatternTypeForeColor;
        public string PatternTypeForeColor
        {
            get => _PatternTypeForeColor;
            set { _PatternTypeForeColor = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }


        private string _SpoutLength;
        /// <summary>
        /// Spout length.
        /// </summary>
        public string SpoutLength
        {
            get => _SpoutLength;
            set { _SpoutLength = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }


        private string _SpoutLengthForeColor;
        /// <summary>
        /// SpoutLength Fore color
        /// </summary>
        public string SpoutLengthForeColor
        {
            get => _SpoutLengthForeColor;
            set { _SpoutLengthForeColor = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private string _VerticalPitch;
        /// <summary>
        /// Vertical pitch.
        /// </summary>
        public string VerticalPitch
        {
            get => _VerticalPitch;
            set { _VerticalPitch = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private string _VerticalPitchForeColor;
        /// <summary>
        /// VerticalPitch Fore color
        /// </summary>
        public string VerticalPitchForeColor
        {
            get => _VerticalPitchForeColor;
            set { _VerticalPitchForeColor = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private string _SpoutCount;
        /// <summary>
        /// Spout count.
        /// </summary>
        public string SpoutCount
        {
            get => _SpoutCount;
            set { _SpoutCount = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private string _SpoutCountForeColor;
        /// <summary>
        /// SpoutCount Fore color
        /// </summary>
        public string SpoutCountForeColor
        {
            get => _SpoutCountForeColor;
            set { _SpoutCountForeColor = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }


        private string _ActualMargin;
        /// <summary>
        /// Actual margin.
        /// </summary>
        public string ActualMargin
        {
            get => _ActualMargin;
            set { _ActualMargin = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private string _ActualMarginForeColor;
        /// <summary>
        /// ActualMargin Fore color
        /// </summary>
        public string ActualMarginForeColor
        {
            get => _ActualMarginForeColor;
            set { _ActualMarginForeColor = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        public double InitialArea { get; set; }

        public double CurrentArea { get; set; }

        public double TargetIdealArea { get; set; }


        private string _IdealArea;
        /// <summary>
        /// Ideal area.
        /// </summary>
        public string IdealArea
        {
            get => _IdealArea;
            set { _IdealArea = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private string _IdealAreaForeColor;
        /// <summary>
        /// Ideal Area Fore color
        /// </summary>
        public string IdealAreaForeColor
        {
            get => _IdealAreaForeColor;
            set { _IdealAreaForeColor = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }


        private string _ActualArea;
        /// <summary>
        /// Actual area.
        /// </summary>
        public string ActualArea
        {
            get => _ActualArea;
            set { _ActualArea = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private string _ActualAreaForeColor;
        /// <summary>
        /// ActualArea Fore color
        /// </summary>
        public string ActualAreaForeColor
        {
            get => _ActualAreaForeColor;
            set { _ActualAreaForeColor = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private string _SpoutDiameter;
        /// <summary>
        /// Spout diameter.
        /// </summary>
        public string SpoutDiameter
        {
            get => _SpoutDiameter;
            set { _SpoutDiameter = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private string _SpoutDiameterForeColor;
        /// <summary>
        /// ActualArea Fore color
        /// </summary>
        public string SpoutDiameterForeColor
        {
            get => _SpoutDiameterForeColor;
            set { _SpoutDiameterForeColor = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private string _HorizontalPitch;
        /// <summary>
        /// Horizontal pitch.
        /// </summary>
        public string HorizontalPitch
        {
            get => _HorizontalPitch;
            set { _HorizontalPitch = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private string _HorizontalPitchForeColor;
        /// <summary>
        /// ActualArea Fore color
        /// </summary>
        public string HorizontalPitchForeColor
        {
            get => _HorizontalPitchForeColor;
            set { _HorizontalPitchForeColor = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private string _AppliedClearance;
        /// <summary>
        /// Applied clearance.
        /// </summary>
        public string AppliedClearance
        {
            get => _AppliedClearance;
            set { _AppliedClearance = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private string _AppliedClearanceForeColor;
        /// <summary>
        /// AppliedClearance Fore color
        /// </summary>
        public string AppliedClearanceForeColor
        {
            get => _AppliedClearanceForeColor;
            set { _AppliedClearanceForeColor = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private string _ActualClearance;
        /// <summary>
        /// Actual clearance.
        /// </summary>
        public string ActualClearance
        {
            get => _ActualClearance;
            set { _ActualClearance = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private string _ActualClearanceForeColor;
        /// <summary>
        /// ActualClearance Fore color
        /// </summary>
        public string ActualClearanceForeColor
        {
            get => _ActualClearanceForeColor;
            set { _ActualClearanceForeColor = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private string _AppliedMargin;
        /// <summary>
        /// Applied margin.
        /// </summary>
        public string AppliedMargin
        {
            get => _AppliedMargin;
            set { _AppliedMargin = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private string _AppliedMarginForeColor;
        /// <summary>
        /// Applied Margin Fore color
        /// </summary>
        public string AppliedMarginForeColor
        {
            get => _AppliedMarginForeColor;
            set { _AppliedMarginForeColor = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }



        private string _AppliedEPClrc;
        /// <summary>
        /// Appled Clearnace
        /// </summary>
        public string AppliedEPClearance
        {
            get => _AppliedEPClrc;
            set { _AppliedEPClrc = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private string _AppliedEPClearanceForeColor;
        public string AppliedEPClearanceForeColor
        {
            get => _AppliedEPClearanceForeColor;
            set { _AppliedEPClearanceForeColor = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private string _ActualEPClrc;
        public string ActualEPClearance
        {
            get => _ActualEPClrc;
            set { _ActualEPClrc = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private string _ActualEPClearanceForeColor;
        public string ActualEPClearanceForeColor
        {
            get => _ActualEPClearanceForeColor;
            set { _ActualEPClearanceForeColor = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private string _ErrorPercent;
        public string ErrorPercent
        {
            get => _ErrorPercent;
            set { _ErrorPercent = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        private string _ErrorPercentForeColor;
        public string ErrorPercentForeColor
        {
            get => _ErrorPercentForeColor;
            set { _ErrorPercentForeColor = value; NotifyPropertyChanged(System.Reflection.MethodBase.GetCurrentMethod()); }
        }

        public override bool IsEnglishSelected
        {
            get => base.IsEnglishSelected;
            set { DisplayUnits = (!value) ? uppUnitFamilies.Metric : uppUnitFamilies.English; Execute_ShowSpoutGroup(); ShowConstraints(true); NotifyPropertyChanges(); }
        }

        //Added by CADfx


        public override DXFViewer Viewer
        {
            get => base.Viewer;
            set
            {


                if (base.Viewer != null)
                {
                    base.Viewer.OnViewerMouseDown -= Viewer_OnViewerMouseDown;
                     }
                base.Viewer = value;
                if (base.Viewer != null)
                {
                    base.Viewer.OnViewerMouseDown += Viewer_OnViewerMouseDown;
                }

                //if(_Viewer != null) _Viewer.SizeChanged += ViewerSizeChangeHandler();

                if (base.Viewer != null) base.Viewer.EnableRightMouseZoom = true;

            }
        }
        //Added by CADfx

        #endregion

        #region Commands

        private DelegateCommand _CMD_Refresh;
        /// <summary>
        /// Show Constraints Command.
        /// </summary>
        public ICommand Command_Refresh { get { _CMD_Refresh ??= new DelegateCommand(param => Execute_Refresh()); return _CMD_Refresh; } }


        private DelegateCommand _CMD_ShowConstraints;
        /// <summary>
        /// Show Constraints Command.
        /// </summary>
        public ICommand Command_ShowConstraints { get {  _CMD_ShowConstraints ??= new DelegateCommand(param => Execute_ShowConstraints()); return _CMD_ShowConstraints; } }

        /// <summary>
        /// Units Toggle command.
        /// </summary>

        private DelegateCommand _CMD_RefreshConstraints;
        /// <summary>
        /// Refresh Constraints Command.
        /// </summary>
        public ICommand Command_RefreshConstraints { get { _CMD_RefreshConstraints ??= new DelegateCommand(param => Execute_RefreshConstraints()); return _CMD_RefreshConstraints; } }

        private DelegateCommand _CMD_ResetConstraints;
        /// <summary>
        /// Reset Constraints Command.
        /// </summary>
        public ICommand Command_ResetConstraints { get { _CMD_ResetConstraints ??= new DelegateCommand(param => Execute_ResetConstraints()); return _CMD_ResetConstraints; } }

        private DelegateCommand _CMD_ClearConstraints;
        /// <summary>
        /// Clear Constraints Command.
        /// </summary>
        public ICommand Command_ClearConstraints { get { _CMD_ClearConstraints ??= new DelegateCommand(param => Execute_ClearConstraints()); return _CMD_ClearConstraints; } }

        private DelegateCommand _CMD_ApplyConstraints;
        /// <summary>
        /// Cancel Constraints command.
        /// </summary>
        public ICommand Command_ApplyConstraints { get { _CMD_ApplyConstraints ??= new DelegateCommand(param => Execute_ApplyConstraints()); return _CMD_ApplyConstraints; } }

        private DelegateCommand _CMD_CancelConstraints;
        /// <summary>
        /// Cancel Constraints command.
        /// </summary>
        public ICommand Command_CancelConstraints { get { _CMD_CancelConstraints ??= new DelegateCommand(param => Execute_CancelConstraints()); return _CMD_CancelConstraints; } }

        private DelegateCommand _OkCommand;
        /// <summary>
        /// Cancel Constraints command.
        /// </summary>
        public ICommand Command_Ok
        {
            get
            {
                if (_OkCommand == null) _OkCommand = new DelegateCommand(param => Execute_Save());
                return _OkCommand;
            }
        }


        private DelegateCommand _CMD_Cancel;
        /// <summary>
        /// Cancel command.
        /// </summary>
        public ICommand Command_Cancel { get { if (_CMD_Cancel == null) _CMD_Cancel = new DelegateCommand(param => Execute_Cancel()); return _CMD_Cancel; } }


        #endregion


        #region Private Methods

        private void Viewer_OnViewerMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 1)
            {
                mdTrayRange range = MDRange;
                DXFViewer viewer = Viewer;

            }
            else if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 2)
            {
                var mouseCoordinate = e.GetPosition((System.Windows.IInputElement)sender);


            }
            else if (e.RightButton == MouseButtonState.Pressed && e.ClickCount == 2)
            {
                Execute_Refresh();

            }
        }


        public string Precision => DisplayUnits == uppUnitFamilies.Metric ? "F1" : "F4";

        /// <summary>
        /// Manages Constraint frame
        /// </summary>
        private void Execute_ShowConstraints()
        {
            try
            {
                IsEnabled = false;
                EditProps = Constraints.CurrentProperties();
                EditProps = mdUtils.SetConstraintPropertyLimits(EditProps,  SpoutGroup, Grid );
                SetNonZero();
                SetConstraintValues();

                ShowConstraints(true);
                Visibility_ConstraintsFrame = Visibility.Visible;


                IsEscapeEnabled = false;
            }
            finally
            {
                IsEnabled = true;
            }
        }

        /// <summary>
        /// Sets constraints values.
        /// </summary>
        private void SetConstraintValues()
        {
            double result;
            ConstraintClearance = double.TryParse(AppliedClearance, out result) ? result : null;
            //ConstraintSpoutDiameter = (double.TryParse(SpoutDiameter, out result)) ? result : null;
            ConstraintMargin = double.TryParse(AppliedMargin, out result) ? result : null;
        }

        /// <summary>
        /// Sets constraints data to its underlying objects.
        /// </summary>
        private void ShowConstraints(bool getValueFromProperty = false)
        {
            if (EditProps == null) return;

            try
            {
                IsEnabled = false;
                uppSpoutPatterns _SpoutPatternType;
                uopProperty prop;
                bool patternLock;
                uppUnitFamilies units = DisplayUnits;

                _SpoutPatternType = EditProps.Value("PatternType") == null ? uppSpoutPatterns.D :
                                        (uppSpoutPatterns)Enum.Parse(typeof(uppSpoutPatterns), EditProps.Value("PatternType").ToString());
                _SelectedPatternType = _SpoutPatternType;
                NotifyPropertyChanged("SelectedPatternType");

                patternLock = _SpoutPatternType > 0;

                for (int i = 1; i <= EditProps.Count; i++)
                {
                    prop = EditProps.Item(i);
                    var propertyValue = prop.Value;
                    var defaultValue = prop.DefaultValue;
                    if (prop.IsNamed("PatternType"))
                    {
                        if (prop.Value is Enum)
                        {
                            PatternNameSelectedIndex = (int)prop.Value;
                        }
                        else if (mzUtils.IsNumeric(prop.Value))
                        {
                            int index = 0;
                            int.TryParse(prop.Value, out index);
                            PatternNameSelectedIndex = index;
                        }

                        SelectedPatternType = (uppSpoutPatterns)PatternNameSelectedIndex;
                    }
                    else
                    {
                        string consIntermediateVal;

                        if (propertyValue is string)
                        {
                            propertyValue = double.TryParse(propertyValue, out double parsedPropertyValue);
                        }

                        consIntermediateVal = string.Empty;

                        switch (prop.Name.ToUpper())
                        {
                            case "VERTICALPITCH":
                                Visibility_VerticalPitch = Visibility.Visible;
                                if (!patternLock)
                                {
                                    EditProps.SetProperty("VERTICALPITCH", 0);
                                    ConstraintVerticalPitch = null;
                                    Visibility_VerticalPitch = Visibility.Collapsed;
                                }
                                else
                                {
                                    propertyValue = CheckPropertyValue(prop, propertyValue, defaultValue, Multiplier_Linear);
                                    consIntermediateVal = prop.UnitValueString(aUnitFamily: units, bZerosAsNullString: true, bDefaultAsNullString: true, aValue: propertyValue);

                                    if (string.IsNullOrEmpty(consIntermediateVal))
                                    {
                                        ConstraintVerticalPitch = null;
                                    }
                                    else
                                    {
                                        ConstraintVerticalPitch = double.Parse(consIntermediateVal);
                                    }
                                }
                                break;

                            case "HORIZONTALPITCH":
                                IsHorizontalPitchEnabled = true;
                                if (!patternLock || _SpoutPatternType == uppSpoutPatterns.SStar || _SpoutPatternType == uppSpoutPatterns.S1 || NoHPitch)
                                {
                                    IsHorizontalPitchEnabled = false;

                                    //if (NoHPitch)
                                    //ConstraintHorizontalPitch = prop.UnitValueString(units, true, true, aValue: defaultValue);
                                }
                                else
                                {
                                    propertyValue = CheckPropertyValue(prop, propertyValue, defaultValue, Multiplier_Linear);
                                    consIntermediateVal = prop.UnitValueString(aUnitFamily: units, bZerosAsNullString: true, bDefaultAsNullString: true, aValue: propertyValue);
                                    if (string.IsNullOrEmpty(consIntermediateVal))
                                    {
                                        ConstraintHorizontalPitch = null;
                                    }
                                    else
                                    {
                                        ConstraintHorizontalPitch = double.Parse(consIntermediateVal);
                                    }
                                }
                                break;

                            case "SPOUTCOUNT":
                                Visibility_SpoutCount = patternLock ? Visibility.Visible : Visibility.Collapsed;
                                if (!patternLock)
                                {
                                    EditProps.SetProperty("SPOUTCOUNT", -1);
                                    ConstraintSpoutCount = null;
                                }
                                else
                                {
                                    propertyValue = CheckPropertyValue(prop, propertyValue, defaultValue, Multiplier_Linear);

                                    if (propertyValue == -1)
                                    {
                                        ConstraintSpoutCount = null;
                                    }
                                    else
                                    {
                                        int? _ActualSpoutCount = (int)propertyValue;
                                        ConstraintSpoutCount = _ActualSpoutCount == -1 ? (int?)null : _ActualSpoutCount;
                                    }
                                }
                                break;

                            case "SPOUTDIAMETER":

                                propertyValue = CheckPropertyValue(prop, getValueFromProperty ? propertyValue : (ConstraintSpoutDiameter ?? 0), defaultValue, Multiplier_Linear);
                                consIntermediateVal = prop.UnitValueString(aUnitFamily: units, bZerosAsNullString: true, bDefaultAsNullString: true, aValue: propertyValue);
                                if (string.IsNullOrEmpty(consIntermediateVal))
                                {
                                    ConstraintSpoutDiameter = null;
                                }
                                else
                                {
                                    ConstraintSpoutDiameter = double.Parse(consIntermediateVal);
                                }
                                break;

                            case "SPOUTLENGTH":
                                Visibility_SlotLength = Visibility.Visible;
                                if (_SpoutPatternType != uppSpoutPatterns.S1 && _SpoutPatternType != uppSpoutPatterns.S2 && _SpoutPatternType != uppSpoutPatterns.S3)
                                {
                                    EditProps.SetProperty("SPOUTLENGTH", 0);
                                    ConstraintSpoutLength = null;
                                    Visibility_SlotLength = Visibility.Collapsed;
                                }
                                else
                                {
                                    propertyValue = CheckPropertyValue(prop, propertyValue, defaultValue, Multiplier_Linear);
                                    consIntermediateVal = prop.UnitValueString(aUnitFamily: units, bZerosAsNullString: true, bDefaultAsNullString: true, aValue: propertyValue);
                                    if (string.IsNullOrEmpty(consIntermediateVal))
                                    {
                                        ConstraintSpoutLength = null;
                                    }
                                    else
                                    {
                                        ConstraintSpoutLength = double.Parse(consIntermediateVal);
                                    }
                                }
                                break;

                            case "CLEARANCE":

                                propertyValue = CheckPropertyValue(prop, getValueFromProperty ? propertyValue : (ConstraintClearance ?? 0), defaultValue, Multiplier_Linear);
                                consIntermediateVal = prop.UnitValueString(aUnitFamily: units, bZerosAsNullString: true, bDefaultAsNullString: true, aValue: propertyValue);
                                if (string.IsNullOrEmpty(consIntermediateVal))
                                {
                                    ConstraintClearance = null;
                                }
                                else
                                {
                                    ConstraintClearance = double.Parse(consIntermediateVal);
                                }
                                break;

                            case "MARGIN":
                                propertyValue = CheckPropertyValue(prop, getValueFromProperty ? propertyValue : (ConstraintMargin ?? 0), defaultValue, Multiplier_Linear);
                                consIntermediateVal = prop.UnitValueString(aUnitFamily: units, bZerosAsNullString: true, bDefaultAsNullString: true, aValue: propertyValue);
                                if (string.IsNullOrEmpty(consIntermediateVal))
                                {
                                    ConstraintMargin = null;
                                }
                                else
                                {
                                    ConstraintMargin = double.Parse(consIntermediateVal);
                                }
                                break;

                            case "ENDPLATECLEARANCE":
                                if (SpoutGroup.GroupIndex != 1)
                                {
                                    EditProps.SetProperty("ENDPLATECLEARANCE", 0);
                                    ConstraintEndPlateClearance = null;


                                }
                                else
                                {

                                    propertyValue = CheckPropertyValue(prop, propertyValue, defaultValue, Multiplier_Linear);
                                    consIntermediateVal = prop.UnitValueString(aUnitFamily: units, bZerosAsNullString: true, bDefaultAsNullString: true, aValue: propertyValue);
                                    if (string.IsNullOrEmpty(consIntermediateVal))
                                    {
                                        ConstraintEndPlateClearance = null;
                                    }
                                    else
                                    {
                                        ConstraintEndPlateClearance = double.Parse(consIntermediateVal);
                                    }
                                }
                                break;

                            case "YOFFSET":
                            case "XOFFSET":
                                if (_SpoutPatternType != uppSpoutPatterns.SStar)
                                {

                                    Visibility_YOffset = Visibility.Collapsed;
                                }
                                else
                                {
                                    Visibility_YOffset = Visibility.Visible;
                                    propertyValue = CheckPropertyValue(prop, propertyValue, defaultValue, Multiplier_Linear);
                                    consIntermediateVal = prop.UnitValueString(aUnitFamily: units, bZerosAsNullString: true, bDefaultAsNullString: true, aValue: propertyValue);
                                    if (string.IsNullOrEmpty(consIntermediateVal))
                                    {
                                        ConstraintYOffset = null;
                                    }
                                    else
                                    {
                                        ConstraintYOffset = double.Parse(consIntermediateVal);
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            finally
            {
                IsEnabled = true;
            }
        }

        /// <summary>
        /// Gets the value of the property
        /// </summary>
        /// <param name="property"></param>
        /// <param name="propertyValue"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private double CheckPropertyValue(uopProperty property, double propertyValue, double defaultValue, double multi)
        {
            var minValue = property.MinValue;
            var maxValue = property.MaxValue;
            if (propertyValue != defaultValue)
            {
                if (propertyValue < minValue)
                {
                    propertyValue = minValue;
                }
                if (propertyValue > maxValue)
                {
                    propertyValue = maxValue;
                }
            }
            else
            {
                propertyValue = property.Name?.ToUpper() == "SPOUTCOUNT" ? propertyValue : 0.0;
            }

            return propertyValue;
        }

        /// <summary>
        /// Resotres and hides UI elements when cancel button is tapped.
        /// </summary>
        private void Execute_CancelConstraints()
        {
            EditProps.RestoreValues();
            EditProps.StoredValuesClear();
            Execute_HideConstraints();
        }

        /// <summary>
        /// Saves the constraints data, hide the constraint panel and refresh the display
        /// </summary>
        private void Execute_ApplyConstraints()
        {
     

   
            try
            {
                Warnings = new uopDocuments();
                IsEnabled = false;
                Execute_SaveConstraints();

                Grid =  mdSpoutGrid.GenSpouts(MDAssy, SpoutGroup, Constraints);

                if (string.IsNullOrWhiteSpace(Grid.ErrorString))
                {
                    SpoutGroup.Invalid = false;
                    Constraints.Invalid = false;
                    EditProps = Constraints.CurrentProperties();

                }
                else
                {

                    EditProps.RestoreValues();
                    Constraints.PropValsCopy(EditProps);
                    SaveWarning(new Exception($"Spout Generation Error Detected. ERROR: {Grid.ErrorString}"),System.Reflection.MethodBase.GetCurrentMethod(), "Spout Generation Error");

                }

            }
            catch (Exception e)
            {
                SaveWarning(new Exception($"Spout Generation Error Detected. ERROR: {e.Message}"), System.Reflection.MethodBase.GetCurrentMethod(), "Spout Generation Error");

                EditProps.RestoreValues();
                Constraints.PropValsCopy(EditProps);
            }
            finally
            {
                Execute_HideConstraints();
                Execute_Refresh();
               // Execute_ShowConstraints();
               // Viewer?.RefreshDisplay();
                IsEnabled = true;
            }
        }


        /// <summary>
        /// hide the constraints panel and refresh the display
        /// </summary>
        private void Execute_Refresh()
        {

            bool wuz = IsEnabled;
            Warnings ??= new uopDocuments();
            try
            {
                IsEnabled = true;
                Execute_ShowSpoutGroup();
                Execute_RefreshImage();

            }
            catch (Exception e)
            {
                
                SaveWarning(new Exception($"Spout Generation Error Detected. ERROR: {e.Message}"), System.Reflection.MethodBase.GetCurrentMethod(), "Spout Generation Error");
            }
            finally
            {

                IsEnabled = wuz;
            }
        }


        /// <summary>
        /// draw the Edit Spout Graphics Sketch
        /// VB6 function name :DrawInput_SpoutGroupEdit
        /// </summary>
        private void Execute_RefreshImage()
        {
            if (Viewer == null || MDAssy == null || SpoutGroup == null) return;
            Viewer.Clear();
            

            dxfImage img = null;  // new(appApplication.SketchColor);
            uopDocDrawing dwg = new uopDocDrawing(uppDrawingFamily.Design, $"Spout Group {SpoutGroup.Handle}", uppDrawingTypes.SGInputSketch, MDRange, SpoutGroup)
            {
                PartIndex = SpoutGroup.Index,
                Grid = Grid
            };
            AppDrawingMDSHelper source = new AppDrawingMDSHelper();

            mdTrayAssembly assy = MDAssy;


            try
            {
                CurrentArea = SpoutGroup.ActualArea;
                img = source.GenerateImage(dwg, true, Viewer.Size, MDRange);
            
            }
            catch (Exception e)
            {
                throw new Exception("[EditSpoutGroupHelper.Execute_RefreshImage]" + e.Message);
            }
            finally
            {
                source.TerminateObjects();
                Viewer.SetImage(img);
                Viewer.ZoomExtents();
            }

        }

        /// <summary>
        /// Refreshes constraints data
        /// </summary>
        private void Execute_RefreshConstraints()
        {
            try
            {
                IsEnabled = false;
                EditProps = Constraints.CurrentProperties();
                EditProps = mdUtils.SetConstraintPropertyLimits(EditProps, SpoutGroup, Grid);
                SetNonZero();
                SetConstraintValues();

                ShowConstraints(true);

            }
            finally
            {
                IsEnabled = true;
            }
        }
        /// <summary>
        /// Refreshes constraints data
        /// </summary>
        private void Execute_ResetConstraints()
        {
            try
            {
                IsEnabled = false;

                EditProps = SpoutGroup.Constraints(MDAssy).CurrentProperties();
                EditProps = mdUtils.SetConstraintPropertyLimits(EditProps, SpoutGroup, Grid);
                SetNonZero();
                SetConstraintValues();

                ShowConstraints(true);

            }
            finally
            {
                IsEnabled = true;
            }
        }
        /// <summary>
        /// Clears the constraints data
        /// </summary>
        private void Execute_ClearConstraints()
        {
            SelectedPatternType = uppSpoutPatterns.Undefined;
            ConstraintSpoutDiameter = null;
            ConstraintClearance = null;
            ConstraintMargin = null;
            ConstraintEndPlateClearance = null;
            ConstraintSpoutLength = null;
            ConstraintVerticalPitch = null;
            ConstraintHorizontalPitch = null;
            ConstraintSpoutLength = null;
            ConstraintYOffset = null;
            ConstraintSpoutCount = null;
            uopProperty selUopProp = null;

            if (null != EditProps)
            {
                for (int i = 1; i <= EditProps.Count; i++)
                {
                    selUopProp = EditProps.Item(i);


                    if (selUopProp.Name.ToUpper() != "PATTERNTYPE")
                    {
                        EditProps.SetProperty(i, 0);
                    }
                    else
                    {
                        EditProps.SetProperty(i, uppSpoutPatterns.Undefined);
                    }
                }
                EditProps.SetProperty("SPOUTCOUNT", -1);
            }
        }

        private void Execute_Cancel() => DialogResult = false;


        /// <summary>
        /// Invokes on notify property changed for range properties.
        /// </summary>
        private void NotifyPropertyChanges()
        {
            NotifyPropertyChanged("ConstraintVerticalPitch");
            NotifyPropertyChanged("ConstraintHorizontalPitch");
            NotifyPropertyChanged("ConstraintSpoutDiameter");
            NotifyPropertyChanged("ConstraintSpoutLength");
            NotifyPropertyChanged("ConstraintClearance");
            NotifyPropertyChanged("ConstraintMargin");
            NotifyPropertyChanged("ConstraintEndPlateClearance");
            NotifyPropertyChanged("ConstraintYOffset");

            NotifyPropertyChanged("IdealArea");
            NotifyPropertyChanged("ActualArea");
            NotifyPropertyChanged("SpoutDiameter");
            NotifyPropertyChanged("SpoutLength");
            NotifyPropertyChanged("VerticalPitch");
            //NotifyPropertyChanged("HorizontalPitch");
            NotifyPropertyChanged("AppliedClearance");
            NotifyPropertyChanged("ActualClearance");
            NotifyPropertyChanged("AppliedMargin");
            NotifyPropertyChanged("ActualMargin");
            NotifyPropertyChanged("AppliedEPClearance");
            NotifyPropertyChanged("ActualEPClearance");


        }

        /// <summary>
        /// Sets non-zero to clearnace values
        /// </summary>
        private void SetNonZero()
        {
            int idx = 0;
            EditProps.ThisThenThis("Clearance", DefaultClearance, 0, false, 4, null, out idx);

            if (idx <= 0)
                EditProps.Add("Clearance", 0, uppUnitTypes.SmallLength, aNullVal: DefaultClearance);

            EditProps.ThisThenThis("EndPlateClearance", 0.25, 0, false, 4, null, out idx);

            if (idx <= 0)
                EditProps.Add("EndPlateClearance", 0, uppUnitTypes.SmallLength, aNullVal: 0.25);
        }

        /// <summary>
        /// Increments the value of the given property
        /// </summary>
        /// <param name="pname"></param>
        /// <param name="factr"></param>
        public void IncrementValue(string pname, double factr)
        {
            uopProperty aProp;
            double nextVal;
            double aVal;
            double dVal;
            double minval;
            double maxval;
            double aStep;
            bool isMarginOrOffSet = false;

            if (string.Compare(pname, "SpoutDiameter", ignoreCase: true) == 0)
            {
                DialogService.ShowMessageBox(this, "Changing Spout Diameter is currently not allowed!", "Locked Constraint");
                return;
            }
            if (string.Compare(pname, "HorizontalPitch", ignoreCase: true) == 0)
            {
                DialogService.ShowMessageBox(this, "Changing Horizontal Pitch is currently not allowed!", "Locked Constraint");
                return;
            }

            aProp = EditProps.Item(pname);
            if (aProp == null) return;
            aVal = aProp.Value;
            dVal = aProp.DefaultValue;
            minval = aProp.MinValue ?? double.MinValue;
            maxval = aProp.MaxValue ?? double.MaxValue;
            aStep = aProp.Increment;

            if (aStep <= 0)
                aStep = 0;

            pname = pname.ToUpper();

            if (aStep <= 0)
            {
                if (pname == "CLEARANCE")
                {
                    aStep = 0.05;
                }
                else
                {
                    aStep = (aProp.UnitType == uppUnitTypes.SmallLength) ? 0.0625 : 1;
                }
            }

            bool isInitialValue = true;

            switch (pname)
            {
                case "YOFFSET":
                    if (Visibility_YOffset != Visibility.Visible) return;
                    isInitialValue = ConstraintYOffset == null;
                    break;

                case "CLEARANCE":

                    isInitialValue = ConstraintClearance == null;
                    break;

                case "MARGIN":
                    isInitialValue = ConstraintMargin == null;
                    break;

                case "ENDPLATECLEARANCE":
                    if (Visibility_EndPlateClearance != Visibility.Visible) return;
                    isInitialValue = ConstraintEndPlateClearance == null;
                    break;

                case "SPOUTCOUNT":
                    if (Visibility_SpoutCount != Visibility.Visible) return;
                    isInitialValue = ConstraintSpoutCount == null;
                    break;

                case "SPOUTLENGTH":
                    if (Visibility_SlotLength != Visibility.Visible) return;
                    if (aProp.MinValue == null)
                    {
                        minval = 1;
                        maxval = 0.5 * DCInfo.InsideWidth;
                    }
                    isInitialValue = ConstraintSpoutLength == null;
                    break;

                case "VERTICALPITCH":
                    if (Visibility_VerticalPitch != Visibility.Visible) return;

                    isInitialValue = ConstraintVerticalPitch == null;
                    break;
            }

            if (isInitialValue)
            {
                switch (pname)
                {
                    case "SPOUTCOUNT":
                        factr = 0;
                        aVal = Grid.SpoutCount;

                        break;

                    case "SPOUTLENGTH":

                        factr = 0;
                        aVal = Grid.Spouts.Member.Length;

                        if (IsMetricSpouts)
                        {
                            aStep = 1 / 25.4;
                            minval = uopUtils.RoundTo(minval, dxxRoundToLimits.Millimeter, true, false);
                            maxval = uopUtils.RoundTo(maxval, dxxRoundToLimits.Millimeter, false, true);
                        }
                        break;

                    case "SPOUTDIAMETER":

                        factr = 0;
                        aVal = Grid.Spouts.Member.Diameter;
                        if (IsMetricSpouts)
                        {
                            aStep = 1 / 25.4;
                            dVal = uopUtils.RoundTo(dVal, dxxRoundToLimits.Millimeter, true, false);
                            minval = uopUtils.RoundTo(minval, dxxRoundToLimits.Millimeter, true, false);
                            maxval = uopUtils.RoundTo(maxval, dxxRoundToLimits.Millimeter, false, true);
                        }
                        break;

                    case "MARGIN":
                    case "XOFFSET":
                    case "YOFFSET":
                        isMarginOrOffSet = true;
                        aStep = aStep * factr;
                        nextVal = aVal + aStep;

                        if ((aVal < dVal && nextVal > dVal) || (aVal > dVal && nextVal < dVal))
                        {
                            nextVal = dVal;
                        }

                        if (nextVal < minval)
                            nextVal = minval;

                        if (nextVal > maxval)
                            nextVal = maxval;

                        EditProps.SetProperty(pname, nextVal);

                        if (pname == "MARGIN")
                        {
                            if (nextVal != 0)
                                EditProps.SetProperty("VERTICALPITCH", 0);
                        }
                        else if (pname == "VERTICALPITCH")
                        {
                            if (nextVal != 0)
                                EditProps.SetProperty("MARGIN", 0);
                        }

                        EditProps = mdUtils.SetConstraintPropertyLimits(EditProps, SpoutGroup, Grid);
                        ShowConstraints(true);
                        break;

                    case "CLEARANCE":
                        factr = 0;
                        aVal = DefaultClearance;
                        break;

                    case "ENDPLATECLEARANCE":
                        factr = 0;
                        aVal = 0.25;
                        break;

                    case "VERTICALPITCH":
                        factr = 0;
                        aVal = SpoutGroup.VerticalPitch;
                        dVal = aVal;
                        break;

                    case "HORIZONTALPITCH":
                        factr = 0;
                        aVal = SpoutGroup.HorizontalPitch;
                        dVal = aVal;
                        break;

                    default:
                        aVal = EditProps.Value(aProp.Name);
                        break;
                }
            }
            else
            {
                switch (pname)
                {
                    case "VERTICALPITCH":
                        minval = mdSpoutGroup.GetDefaultPitch_V(SelectedPatternType, EditProps.Value("SpoutDiameter") / 2);
                        break;
                    //TODO- for now we disable this code due to it is return this method if pname is  "HORIZONTALPITCH" & "SPOUTDIAMETER"
                    //case  "HORIZONTALPITCH":
                    //    minval = mdSpoutGroup.GetDefaultPitch_H(SelectedPatternType, EditProps.Value( "SpoutDiameter") / 2);
                    //    break;
                    //case "SPOUTDIAMETER":
                    //    ConstraintVerticalPitch = 0.0;
                    //    ConstraintHorizontalPitch = 0.0;

                    //    EditProps.SetProperty("VerticalPitch", 0);
                    //    EditProps.SetProperty("HorizontalPitch", 0);

                    //    if (IsMetricSpouts)
                    //    {
                    //        aStep = 1 / 25.4;
                    //        dVal = uopUtils.RoundTo(dVal, dxxRoundToLimits.Millimeter, true);
                    //        minval = uopUtils.RoundTo(minval, dxxRoundToLimits.Millimeter, true);
                    //        maxval = uopUtils.RoundTo(maxval, dxxRoundToLimits.Millimeter, false, true);
                    //    }
                    //    break;

                    case "SPOUTLENGTH":

                        if (IsMetricSpouts)
                        {
                            aStep = 1 / 25.4;
                            minval = uopUtils.RoundTo(minval, dxxRoundToLimits.Millimeter, true);
                            maxval = uopUtils.RoundTo(maxval, dxxRoundToLimits.Millimeter, false, true);
                        }
                        break;
                }
            }

            if (!isMarginOrOffSet)
            {
                aStep *= factr;
                nextVal = aVal + aStep;

                if ((aVal < dVal && nextVal > dVal) || (aVal > dVal && nextVal < dVal))
                {
                    nextVal = dVal;
                }

                if (nextVal < minval)
                    nextVal = minval;

                if (nextVal > maxval)
                    nextVal = maxval;

                EditProps.SetProperty(pname, nextVal);

                if (pname == "MARGIN")
                {
                    if (nextVal != 0)
                        EditProps.SetProperty("VERTICALPITCH", 0);
                }
                else if (pname == "VERTICALPITCH")
                {
                    if (nextVal != 0)
                        EditProps.SetProperty("MARGIN", 0);
                }

                //EditProps = mdUtils.SetConstraintPropertyLimits(EditProps, MDAssy, SpoutGroup, Downcomer);
                ShowConstraints(true);
            }
        }

        public override void ReleaseReferences() 
        {
            if (base.Viewer != null) base.Viewer.OnViewerMouseDown -= Viewer_OnViewerMouseDown;
            base.ReleaseReferences();
            
                if(_Constraints != null)_Constraints.eventMDConstraintPropertyChange -= ConstraintChange;
            _Constraints = null;
            _Grid = null;
        }

        /// <summary>
        /// Increments the value of the given property
        /// </summary>
        /// <param name="pname"></param>
        /// <param name="factr"></param>
        public void ResetValue(string pname)
        {
            uopProperty aProp = EditProps.Item(pname);
            if (aProp == null) return;

            double dVal = aProp.DefaultValue;

            aProp.ResetValue();

            pname = pname.ToUpper();

            switch (pname)
            {
                case "YOFFSET":
                    ConstraintYOffset = null;
                    break;

                case "CLEARANCE":
                    ConstraintClearance = null;
                    break;

                case "MARGIN":
                    ConstraintMargin = null;
                    break;

                case "ENDPLATECLEARANCE":
                    ConstraintEndPlateClearance = null;
                    break;

                case "SPOUTCOUNT":
                    ConstraintSpoutCount = null;
                    break;

                case "SPOUTLENGTH":
                    ConstraintSpoutLength = null;
                    break;

                case "VERTICALPITCH":
                    ConstraintVerticalPitch = null;
                    break;
            }


        }


        /// <summary>
        /// Sets the constrains when user provides constraints manually (instead of using up/down control)
        /// </summary>
        /// <param name="pname"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string SetConstraintWhenLostFocus(string pname, ref string value)
        {
            double aVal;
            double convVal;
            uopProperty aProp;

            bool isConverted = double.TryParse(value, out convVal);

            if (pname == "SPOUTCOUNT")
            {
                convVal = !isConverted ? -1 : convVal;
            }
            else
            {
                if (convVal <= 0 && pname != "YOFFSET")
                {
                    convVal = 0.0;
                }
                else if (convVal < 0)
                {
                    convVal = 0.0;
                }
            }

            if (null != EditProps)
            {
                aProp = EditProps.Item(pname);

                if (null != aProp)
                {
                    if (convVal == 0 && pname != "SPOUTCOUNT")
                    {
                        aVal = aProp.DefaultValue;
                    }
                    else
                    {
                        aVal = convVal;
                        aVal = pname == "SPOUTCOUNT" ? (int)aVal : aVal / Multiplier_Linear;
                    }

                    if (EditProps.SetProperty(pname, aVal))
                    {
                        if (pname == "VERTICALPITCH")
                        {
                            EditProps.SetProperty("MARGIN", 0);
                        }
                        else if (pname == "MARGIN")
                        {
                            EditProps.SetProperty("VERTICALPITCH", 0);
                        }

                        EditProps = mdUtils.SetConstraintPropertyLimits(EditProps, SpoutGroup,Grid );
                        ShowConstraints(true);

                        switch (pname)
                        {
                            case "HORIZONTALPITCH":
                                value = ConstraintHorizontalPitch.HasValue ? ConstraintHorizontalPitch.Value.ToString(Precision) : string.Empty;
                                break;
                            case "VERTICALPITCH":
                                value = ConstraintVerticalPitch.HasValue ? ConstraintVerticalPitch.Value.ToString(Precision) : string.Empty;
                                break;
                            case "SPOUTCOUNT":
                                value = ConstraintSpoutCount.HasValue ? ConstraintSpoutCount.Value.ToString("F0") : string.Empty;
                                break;
                            case "SPOUTDIAMETER":
                                value = ConstraintSpoutDiameter.HasValue ? ConstraintSpoutDiameter.Value.ToString(Precision) : string.Empty;
                                break;
                            case "SPOUTLENGTH":
                                value = ConstraintSpoutLength.HasValue ? ConstraintSpoutLength.Value.ToString(Precision) : string.Empty;
                                break;
                            case "CLEARANCE":
                                value = ConstraintClearance.HasValue ? ConstraintClearance.Value.ToString(Precision) : string.Empty;
                                break;
                            case "MARGIN":
                                value = ConstraintMargin.HasValue ? ConstraintMargin.Value.ToString(Precision) : string.Empty;
                                break;
                            case "ENDPLATECLEARANCE":
                                value = ConstraintEndPlateClearance.HasValue ? ConstraintEndPlateClearance.Value.ToString(Precision) : string.Empty;
                                break;

                            default:
                                value = string.Empty;
                                break;
                        }
                    }
                }
            }

            return convVal == 0 ? string.Empty : value;
        }

        /// <summary>
        /// Saves constraints data
        /// </summary>
        private void Execute_SaveConstraints()
        {
            ConstraintChanged = false;
            uopProperty aProp;
            double vPitch;
            double aDia;
            mdConstraint cons = Constraints;
            if (cons == null) return;
            if (mzUtils.GetEnumValue<uppSpoutPatterns>(EditProps.GetProperty("PatternType").Value) != _SelectedPatternType)
            {
                ConstraintVerticalPitch = 0.0;
                ConstraintHorizontalPitch = 0.0;
                ConstraintSpoutCount = -1;
                ConstraintYOffset = 0.0;
            }

            EditProps.SetProperty("PatternType", SelectedPatternType);

            EditProps.SetProperty("SpoutDiameter", GetConstraintValue("SpoutDiameter", ConstraintSpoutDiameter / Multiplier_Linear));
            EditProps.SetProperty("Margin", GetConstraintValue("Margin", ConstraintMargin / Multiplier_Linear));
            EditProps.SetProperty("Clearance", GetConstraintValue("Clearance", ConstraintClearance / Multiplier_Linear));
            EditProps.SetProperty("SpoutLength", GetConstraintValue("SpoutLength", ConstraintSpoutLength / Multiplier_Linear));
            EditProps.SetProperty("SpoutCount", GetConstraintValue("SpoutCount", ConstraintSpoutCount));
            EditProps.SetProperty("YOffset", GetConstraintValue("YOffset", ConstraintYOffset / Multiplier_Linear));
            EditProps.SetProperty("EndPlateClearance", GetConstraintValue("EndPlateClearance", ConstraintEndPlateClearance / Multiplier_Linear));

            vPitch = EditProps.Value("VerticalPitch");
            if (vPitch > 0)
            {
                aDia = EditProps.Value("SpoutDiameter");
                if (aDia <= 0)
                    aDia = 0.75;
                if (string.Compare(PatternType, "By Default", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    ConstraintVerticalPitch = 0.0;
                    EditProps.SetProperty("VerticalPitch", 0);
                }
            }

            EditProps = mdUtils.SetConstraintPropertyLimits(EditProps, SpoutGroup, Grid);
            SetNonZero();
            ConstraintChanged = false;
            foreach (uopProperty prop in EditProps)
            {
                if (cons.PropValSet(prop.Name, prop.Value, bSuppressEvnts: true) != null) ConstraintChanged = true;
            }

         

        }


        private void Execute_Save()
        {
            if (SpoutGroup == null || MDAssy == null)
            {
                Canceled = true;
                Edited = false;
                return;
            }
            mdConstraint aConstraints = SpoutGroup.Constraints(MDAssy);
            try
            {
                Canceled = false;
                Edited = false;

                bool bFullRefresh = false;

                ApplyToGroup = IsGroupMember && ApplyChangesToAllGroupMembers;
                Edited = !aConstraints.IsEqual(Constraints) || InitialArea != CurrentArea;
                if (!Edited) Edited = SpoutGroup.SpoutString != SpoutGroup.SpoutString;
                if (Edited)
                {

                    uopProperties aNewProps = SpoutGroup.Constraints(MDAssy).CurrentProperties();

                    if (Constraints.TreatAsIdeal)
                    {
                        if (Math.Round(aNewProps.Value("OverrideSpoutArea") - SpoutGroup.ActualArea, 4) != 0)
                        {

                            string SPOUT_AREA_DISTRIBUTION_WARNING = "WARNING : Requested Changes Will Effect Tray Wide Spout Area Distribution" +
                                                                 "\n\n Because this spout group is marked for special treatment in the tray-wide distribution of spout area, " +
                                                                 "applying the requested changes will cause a redistribution of spout area.  " +
                                                                 "This change may effect the spout patterns of all other spout groups in the tray." +
                                                                 "\n\n Press Cancel To Discard Changes";

                            if (MessageBox.Show(SPOUT_AREA_DISTRIBUTION_WARNING, "Spout Area Distribution", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                            {

                                bFullRefresh = true;

                            }
                            else
                            {
                                Constraints.PropValSet("OverrideSpoutArea", SpoutGroup.ActualArea);
                                Canceled = true;
                                Edited = false;
                            }
                        }
                    }
                }


                if (!Canceled)
                {
                    BusyMessage = "Calculation in progress..";
                    IsEnabled = false;
                    Edited = Save(bFullRefresh, aConstraints);

                }
            }
            catch { }
            finally
            {
                if (!Canceled)
                {
                    DialogResult = Edited;
                }
                BusyMessage = "";
                IsEnabled = true;


            }
        }

        /// <summary>
        /// Bound edited spout group properties back to the Spout properties grid
        /// </summary>
        private bool Save(bool bFullRefresh, mdConstraint aConstraints)
        {

            try
            {


                List<mdSpoutGroup> applyCol;
                aConstraints ??= SpoutGroup.Constraints(MDAssy);
                Edited = false;


                //get the spout groups to apply the new comstaints to
                if (ApplyToGroup)
                {
                    applyCol = MDAssy.SpoutGroups.GetGroupedSpoutGroups(Constraints.AreaGroupIndex);
                }
                else
                {
                    applyCol = new List<mdSpoutGroup>() { MDAssy.SpoutGroups.GetByHandle(SpoutGroup.Handle) };

                }

                RefreshMessage = new Message_Refresh();
                uopProperties aNewProps = Constraints.CurrentProperties();


                MDAssy.Invalidate(uppPartTypes.StartupSpout);
                foreach (mdSpoutGroup sg in applyCol)
                {
                    double oldarea = sg.ActualArea;
                    mdConstraint constraint = sg.Constraints(MDAssy);
                    if (constraint.SetCurrentProperties(aNewProps)) Edited = true;

                    sg.Invalid = false;
                    sg.UpdateSpouts(MDAssy);

                    if (oldarea != sg.ActualArea)
                    {
                        Edited = true;
                    }
                }

                if (InitialArea != CurrentArea) Edited = true;

                if (Edited)
                {
                    if (bFullRefresh) MDAssy.Invalidate(uppPartTypes.SpoutGroups);
                    if (ProjectType == uppProjectTypes.MDSpout) MDAssy.RegenerateStartupSpouts();

                }

            }
            finally
            {



            }

            return Edited;
        }


        /// <summary>
        /// Enables/Disables constraints buttons in the UI
        /// </summary>
        private void Execute_HideConstraints()
        {
            Visibility_ConstraintsFrame = Visibility.Collapsed;
            IsEscapeEnabled = true;
            IsCancelBtnEnable = true;
            IsOkBtnEnable = true;
        }

        /// <summary>
        /// Binds curent constraint values back to the visible control properties.
        /// </summary>
        private void Execute_ShowSpoutGroup()
        {

            if (Grid == null) return;
           // uopHole iSpout;
            string conProps = string.Empty;
            double aVal;

            uopUnit lunits = uopUnits.GetUnit(uppUnitTypes.SmallLength);
            uopUnit aunits = uopUnits.GetUnit(uppUnitTypes.SmallArea);
            uppUnitFamilies units = DisplayUnits;


            //iSpout = Grid.Spout;

            SpoutGroup.IsConstrainedIn(MDAssy, Constraints, "", out conProps);
            conProps = $",{ conProps },";
            uopHoles spouts = Grid.Spouts;

            PatternType =  Grid.Pattern.Description();
            PatternTypeForeColor = conProps.IndexOf("PatternType") > 0 ? "Blue" : "Black";


            VerticalPitch = lunits.UnitValueString(Grid.VPitch, units);
            VerticalPitchForeColor = conProps.IndexOf("VerticalPitch") > 0 ? "Blue" : "Black";


            HorizontalPitch = lunits.UnitValueString(Grid.HPitch, units);
            HorizontalPitchForeColor = conProps.IndexOf("HorizontalPitch") > 0 ? "Blue" : "Black";

            SpoutCount =  spouts.Count.ToString();
            SpoutCountForeColor = conProps.IndexOf("SpoutCount") > 0 ? "Blue" : "Black";

            SpoutDiameter = lunits.UnitValueString(spouts.Member.Diameter, units);
            SpoutDiameterForeColor = conProps.IndexOf("SpoutDiameter") > 0 ? "Blue" : "Black";

            AppliedClearance = lunits.UnitValueString(Grid.AppliedClearance, units);
            AppliedClearanceForeColor = conProps.IndexOf("Clearance") > 0 ? "Blue" : "Black";

            aVal = Grid.ActualClearance;
            ActualClearance = lunits.UnitValueString(Grid.ActualClearance, units);

            ActualClearanceForeColor = Math.Round(aVal, 4) < Math.Round(MinClearance, 4) ? "Red" : "Black";

            AppliedMargin = lunits.UnitValueString(Grid.AppliedMargin, units);


            aVal = Grid.AppliedEndPlateClearance;
            if (aVal == 0) aVal = 0.25;

            AppliedEPClearance = lunits.UnitValueString(aVal, units);
            AppliedEPClearanceForeColor = conProps.IndexOf("EndPlateClearance") > 0 ? "Blue" : "Black";

            aVal = Grid.ActualEndPlateClearance;
            ActualEPClearance = lunits.UnitValueString(aVal, units);
            ActualEPClearanceForeColor = Math.Round(aVal, 3) < 0.25 ? "Red" : "Black";

            if (Grid.MaxMargin.HasValue)
            {
                aVal = Grid.MaxMargin.Value;
                ActualMargin = lunits.UnitValueString(aVal, units);
                ActualMarginForeColor =  aVal <mdSpoutGrid.MinSafeMargin ? "Red" : conProps.IndexOf("Margin") > 0 ? "Blue" : "Black";


            }
            else
            {
                ActualMargin = "N/A";
            }
                  aVal = spouts.Member.Length;
            SpoutLength = lunits.UnitValueString(aVal, units);
            SpoutLengthForeColor = conProps.IndexOf("SpoutLength") > 0 ? "Blue" : "Black";

            aVal = TargetIdealArea;
            IdealArea = aunits.UnitValueString(aVal, units);

            aVal = Grid.TotalArea;
            ActualArea = aunits.UnitValueString(aVal, units);
            aVal = Grid.ErrorPercentage;
            ErrorPercent = aVal.ToString("F2");


            ErrorPercentForeColor = Math.Abs(aVal) > MDAssy.ErrorLimit ? "Red" : "Lime";

            Visibility_ActualHPitch = Grid.SPR> 1 ? Visibility.Visible : Visibility.Collapsed;
            Visibility_ActualVPitch = Grid.RowCount() > 1 ? Visibility.Visible : Visibility.Collapsed;


        }



        /// <summary>
        /// Gets the value og given constraint
        /// </summary>
        /// <param name="aName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private dynamic GetConstraintValue(string aName, dynamic value)
        {
            dynamic result;
            string pname = aName.ToUpper();

            if (pname == "SPOUTCOUNT")
            {
                if (value == null)
                {
                    result = -1;
                }
                else
                {
                    result = Convert.ToInt32(value);
                }
            }
            else if (pname == "CLEARANCE")
            {
                if (value == null || value == 0)
                {
                    result = 0;
                }
                else
                {
                    result = Math.Round(value, 5);
                    if (result == DefaultClearance)
                        result = 0;
                }
            }
            else if (pname == "ENDPLATECLEARANCE")
            {
                if (value == null || value == 0)
                {
                    result = 0;
                }
                else
                {
                    result = Math.Round(value, 5);
                    if (result == 0.25)
                        result = 0;
                }
            }
            else
            {
                if (value == null || value == 0)
                    result = 0;
                else
                    result = Math.Round(value, 5);
            }

            return result;
        }

        private void ConstraintChange(uopProperty aProperty)
        {
            if (aProperty == null) return;
            ConstraintChanged = true;
        }

        #endregion Private Methods

        #region Public Methods


        public override void Activate(Window myWindow)
        {
            if (Activated) return;

            base.Activate(myWindow);

            Edit_SpoutGroup_View myview = myWindow == null ? null : (Edit_SpoutGroup_View)myWindow;
            try
            {
                IsEnabled = false;

                if (MDProject == null) return;
                if (MDAssy == null) return;

                Visibility_ConstraintsFrame = Visibility.Collapsed;
                SpoutGroupIndex = MDAssy.SpoutGroups.SelectedGroupIndex;

                SpoutGroup = MDAssy.SpoutGroups.Item(SpoutGroupIndex);
                if (SpoutGroup == null) return;
                InitialArea = SpoutGroup.ActualArea;
                CurrentArea = InitialArea;
                TargetIdealArea = SpoutGroup.TheoreticalArea;
                
                _Grid = new mdSpoutGrid( SpoutGroup.Grid); // this is a clone of the spout groups current grid
                _Constraints = new mdConstraint(SpoutGroup.Constraints(MDAssy));
                _Constraints.eventMDConstraintPropertyChange += ConstraintChange;
                _Grid.SpoutGroup = SpoutGroup.Clone();
                _Grid.Constraints = _Constraints;

                VisibilityDeveloper = uopUtils.RunningInIDE ? Visibility.Visible : Visibility.Collapsed ;

                DefaultClearance = mdSpoutGroup.GetDefaultClearance(SpoutGroup);
                SpoutString = SpoutGroup.SpoutString;

                
                Visibility_ApplyToGoupMember = Constraints.TreatAsGroup ? Visibility.Visible : Visibility.Collapsed;
                NotifyPropertyChanged("IsSetToIdeal");
                NotifyPropertyChanged("IsGroupMember");
                NotifyPropertyChanged("IsMetricSpouts");


                //SpoutGroup.SuppressEvents = true;
                //Constraints.SuppressEvents = true;

                EditProps = Constraints.CurrentProperties();
                //Caption = "Edit Spout Group Properties"


                MinClearance =  mdSpoutGroup.GetDefaultClearance(SpoutGroup);

                Visibility_EndPlateClearance = (Grid.SpoutArea.LimitedBounds) ? Visibility.Visible : Visibility.Collapsed;

                GlobalProjectTitle = $"{MDAssy.TrayName() } - { SpoutGroup.Name}";
                _PatternNameSelectedIndex = 0;


                if (!EditProps.IsLocked("YOFFSET"))
                {
                    ConstraintYOffset = 0.0;
                    Visibility_YOffset = Visibility.Collapsed;
                }

                if (!IsGroupMember)
                {
                    ApplyChangesToAllGroupMembers = false;

                }
                else
                {
                    ApplyChangesToAllGroupMembers = true;
                }

                List<string> patternTypes = new();
                string pname = null;
                var pNames = mdUtils.SpoutPatternNames();
                bool allowSStar = !SpoutGroup.Perimeter.IsRectangular(true);

                for (int i = 0; i < pNames.Count; i++)
                {
                    pname = pNames[i];
                    if (allowSStar || !allowSStar && pname != "*S*")
                        patternTypes.Add(pname);
                }
                PatternTuples = Enum.GetValues(typeof(uppSpoutPatterns)).Cast<Enum>().Where(x => !string.IsNullOrEmpty(x.Description())).Select((e) => new Tuple<string, Enum>(e.Description(), e)).ToList();
                if (!allowSStar) PatternTuples = PatternTuples.Where(i => !i.Item1.Equals("*S*")).Select((e) => new Tuple<string, Enum>(e.Item1, e.Item2)).ToList();
                NotifyPropertyChanged("PatternTuples");
                Visibility_AppliedEndPlateClearance = (!SpoutGroup.LimitedBounds) ? Visibility.Collapsed : Visibility.Visible;


                Execute_Refresh();
              
              


            }
            catch(Exception e)
            {
                SaveWarning(e, System.Reflection.MethodBase.GetCurrentMethod());

            }
            finally
            {
                IsCancelBtnEnable = true;
                IsOkBtnEnable = true;


                IsEnabled = true;
            }




        }
        #endregion Public Methods
    }
}







