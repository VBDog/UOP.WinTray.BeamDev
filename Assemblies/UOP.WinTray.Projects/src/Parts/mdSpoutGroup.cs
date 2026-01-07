using ClosedXML.Graphics;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Documents;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;

using UOP.WinTray.Projects.Structures;
//using static Microsoft.VisualBasic.Conversion;
//using static Microsoft.VisualBasic.DateAndTime;
//using static Microsoft.VisualBasic.ErrObject;
//using static Microsoft.VisualBasic.FileSystem;
//using static Microsoft.VisualBasic.Financial;
//using static Microsoft.VisualBasic.Information;
//using static Microsoft.VisualBasic.Interaction;
//using static Microsoft.VisualBasic.VBMath;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;
//using static Microsoft.VisualBasic.Collection;
using static System.Math;

namespace UOP.WinTray.Projects.Parts
{
    /// <summary>
    /// represents one spout group in a md tray asemblies SpoutGroups collection
    /// </summary>
    public class mdSpoutGroup : mdBoxSubPart
    {

        public override uppPartTypes BasePartType => uppPartTypes.SpoutGroup;



        #region Constructors

        public mdSpoutGroup(mdDowncomerBox aBox, mdConstraint aConstraints, mdSpoutArea aSpoutArea, int? aGroupIndex = null) : base(uppPartTypes.SpoutGroup) => Initialize(aBox: aBox, aConstraints: aConstraints, aSpoutArea: aSpoutArea, aGroupIndex: aGroupIndex);

        internal mdSpoutGroup(mdSpoutGroup aPartToCopy) : base(uppPartTypes.SpoutGroup) => Initialize(aPartToCopy: aPartToCopy);
        internal override void Initialize(mdBoxSubPart aPartToCopy, mdDowncomerBox aBox)
        {
            mdSpoutGroup copy = null;
            if (aPartToCopy != null && aPartToCopy.GetType() == typeof(mdSpoutGroup)) copy = (mdSpoutGroup)aPartToCopy;
            Initialize(copy, aBox);
        }

        bool _Init = false;
        private void Initialize(mdSpoutGroup aPartToCopy = null, mdDowncomerBox aBox = null, mdConstraint aConstraints = null, mdSpoutArea aSpoutArea = null, int? aGroupIndex = null)
        {


            if (!_Init)
            {
                _Grid = new mdSpoutGrid(this, null, null) { Invalid = true };

                SpoutArea = new mdSpoutArea();
          
                _SULines = new TSTARTUPLINES();
                _StartupBound = new USHAPE();
                _Perimeter = new USHAPE();
                _Limits = URECTANGLE.Null;

                Invalid = true;
                HasChanged = false;
                DowncomerIndex = -1;
                PanelIndex = -1;
                GroupIndex = -1;

           
         
                AddProperty("TheoreticalArea", 0, uppUnitTypes.SmallArea);
                AddProperty("PatternType", uppSpoutPatterns.Undefined, aDecodeString: "0 = ~UNDEFINED~, 1 = D, 2 = C, 3 = B, 4 = A, 5 = *A *, 6 = S3, 7 = S2, 8 = S1, 9 = *S * ");
                AddProperty("VerticalPitch", 0!, uppUnitTypes.SmallLength);
                AddProperty("HorizontalPitch", 0!, uppUnitTypes.SmallLength);
                AddProperty("Spout", "", bIsHidden: true);
                AddProperty("SpoutString", "", bIsHidden: true);
                AddProperty("Origin", "(0,0,0)", bIsHidden: true);
                AddProperty("RowCount", 0, bIsHidden: true);
                AddProperty("SpoutsPerRow", 0);
                AddProperty("GroupIndex", 0, bIsHidden: true);
                AddProperty("LimitedBounds", false, bIsHidden: true);
                AddProperty("AppliedClearance", 0!, uppUnitTypes.SmallLength);
                AddProperty("ActualClearance", 0!, uppUnitTypes.SmallLength);
                AddProperty("ActualArea", 0!, uppUnitTypes.SmallArea);
                AddProperty("PatternLength", 0!, uppUnitTypes.SmallLength);
                AddProperty("ActualMargin", 0!, uppUnitTypes.SmallLength);
                _Init = true;
            }

            SuppressEvents = true;

            if (aPartToCopy != null)
            {
                base.Copy(aPartToCopy);

                Parent = aPartToCopy.Parent;

               
                _SULines = new TSTARTUPLINES(aPartToCopy._SULines);
                _StartupBound = new USHAPE(aPartToCopy._StartupBound);
                _Perimeter = new USHAPE(aPartToCopy._Perimeter);
                _Grid = new mdSpoutGrid(aPartToCopy.Grid);
                _Limits = new URECTANGLE(aPartToCopy._Limits);


                HasChanged = aPartToCopy.HasChanged;
              
                SpoutArea = new mdSpoutArea();

                aBox ??= aPartToCopy.DowncomerBox;
                aConstraints ??= aPartToCopy.Constraints(null);
                aSpoutArea ??= new mdSpoutArea(aPartToCopy.SpoutArea);
            }

            if (aConstraints != null)
            {
                //relate the constraints to the group
                SetConstraints(aConstraints);

            }

            if (aSpoutArea != null)
            {
                SpoutArea = new mdSpoutArea(aSpoutArea);
                PanelBottom = SpoutArea.PanelBottom;
                PanelTop = SpoutArea.PanelTop;
                PanelIndex = SpoutArea.PanelIndex;
                PerimeterV = new USHAPE(SpoutArea);
                _Instances = new TINSTANCES(SpoutArea.Instances);
                OccuranceFactor = SpoutArea.OccuranceFactor;

            }

            SubPart(aBox);

            if (aGroupIndex.HasValue) GroupIndex = aGroupIndex.Value;

            _Grid = new mdSpoutGrid(this, Constraints(null), null) { Invalid = true };

            SuppressEvents = false;
        }

        #endregion Constructors

        #region Relationships

        public override bool IsVirtual { get => base.IsVirtual; set => base.IsVirtual = value; }


        private WeakReference<mdSpoutGroup> _ParentRef = null;
        private WeakReference<mdSpoutGroup> _ChildRef = null;


        public mdSpoutGroup Parent
        {
            get

            {
                IsVirtual = false;
                if (_ParentRef == null) return null;
                IsVirtual = _ParentRef.TryGetTarget(out mdSpoutGroup _rVal);
                if (!IsVirtual) _ParentRef = null;
                return _rVal;
            }

            set => _ParentRef = value == null ? null : new WeakReference<mdSpoutGroup>(value);

        }

        public mdSpoutGroup Child
        {
            get
            {
                if (_ChildRef == null) return null;

                if (!_ChildRef.TryGetTarget(out mdSpoutGroup _rVal)) _ChildRef = null;
                return _rVal;
            }
            set => _ChildRef = value == null ? null : new WeakReference<mdSpoutGroup>(value);
        }

        public void CopyParentProperties()
        {
            mdSpoutGroup myparent = Parent;
            if (myparent == null) return;
            this.PropValsCopy(myparent.ActiveProperties, aSkipList: new List<string>() { "X" });
        }

        #endregion Relationship

      
        private TSTARTUPLINES _SULines;
        private USHAPE _StartupBound;
        private USHAPE _Perimeter;
        private URECTANGLE _Limits;

        #region Properties 

        public double AppliedEndPlateClearance { get { if (Invalid) UpdateSpouts(null); return _Grid.AppliedEndPlateClearance; } }

        public double ActualEndPlateClearance { get { if (Invalid) UpdateSpouts(null); return _Grid.ActualEndPlateClearance; } }

        public uopRectangle Margins { get { if (Invalid) UpdateSpouts(null); return _Grid.Margins; } }



        public double PatternY { get { if (Invalid) UpdateSpouts(null); return _Grid.SpoutCenter.Y; } }

        internal UHOLES SpoutsV => Grid._Spouts;

        /// <summary>
        /// the actual hole area of the spouts in the spout group
        /// </summary>
        public double ActualArea { get => Grid.TotalArea; }

        public double ActualClearance { get => Grid.ActualClearance; }

        /// <summary>
        /// the actual margin of the spouts to bounding downcomer
        /// </summary>
        public double ActualMargin { get => Grid.MaxMargin.HasValue ? Grid.MaxMargin.Value : 0; }

        public double AppliedClearance { get { if (Invalid) UpdateSpouts(null); return _Grid.AppliedClearance; } }

        public double AppliedMargin { get { if (Invalid) UpdateSpouts(null); return _Grid.AppliedMargin; } }


        /// <summary>
        /// the smallest fraction of the parent downcomer bottom width at this spout group
        /// </summary>
        public double BottomFraction { get { if (Invalid) UpdateSpouts(null); return _Grid.BottomFraction; } }


        /// <summary>
        /// the rectangle that encloses all of the spouts in the group
        /// </summary>
        public uopRectangle SpoutLimits { get { if (Invalid) UpdateSpouts(null); return new uopRectangle(_Grid._Spouts.BoundaryRectangle) { Name = Handle, Tag = Handle}; } }

        /// <summary>
        /// the boundary used to generate the spout group pattern
        /// </summary>
        public uopShape SpoutBoundary { get { if (Invalid) UpdateSpouts(null); return new uopShape(_Grid) { Name = "BOUNDARY", Tag = Handle }; } }

        /// <summary>
        /// the center point of the spout group
        /// </summary>
        public override dxfVector CenterDXF { get => new dxfVector(X, Y, Z, aTag: Handle); set => base.CenterDXF = value; }

        public string Descriptor
        {
            get
            {
                string _rVal;
                _rVal = SpanName(false);
                uopHoles aSpts = null;
                uopHole aHl = null;
                int i = 0;

                aSpts = Spouts;
                aHl = aSpts.Item(1);
                mzUtils.ListAdd(ref _rVal, Handle);
                mzUtils.ListAdd(ref _rVal, PatternName);
                mzUtils.ListAdd(ref _rVal, aSpts.Count);
                if (aHl == null)
                {
                    mzUtils.ListAdd(ref _rVal, "NONE");
                    mzUtils.ListAdd(ref _rVal, "FIRST CENTER=(0,0)");
                }
                else
                {
                    if (PatternType < uppSpoutPatterns.S3)
                    {
                        mzUtils.ListAdd(ref _rVal, mzUtils.Format(aHl.Diameter, "0.00##") + " HOLE");
                    }
                    else
                    {
                        if (PatternType != uppSpoutPatterns.SStar)
                        {
                            mzUtils.ListAdd(ref _rVal, mzUtils.Format(aHl.Diameter, "0.00##") + " X " + mzUtils.Format(aHl.Length, "0.00##") + " Slot");

                        }
                        else
                        {
                            for (i = 1; i <= aSpts.Count; i++)
                            {
                                aHl = aSpts.Item(i);
                                mzUtils.ListAdd(ref _rVal, mzUtils.Format(aHl.Diameter, "0.00##") + " X " + mzUtils.Format(aHl.Length, "0.00##") + " Slot");
                            }
                        }

                    }


                    aHl = aSpts.Item(1);
                    mzUtils.ListAdd(ref _rVal, "FIRST CENTER=(" + mzUtils.Format(aHl.X, "0.00##") + "," + mzUtils.Format(aHl.Y, "0.00##") + ")");
                    mzUtils.ListAdd(ref _rVal, "DX=" + mzUtils.Format(Grid.HPitch, "0.00##"));
                    mzUtils.ListAdd(ref _rVal, "DY=" + mzUtils.Format(_Grid.VPitch, "0.00##"));

                }

                return _rVal;
            }
        }

        internal USHAPE PerimeterV
        {
            get => _Perimeter;

            set
            {
                _Perimeter = value;
                _Perimeter.Update();

                _Center.X = _Perimeter.Center.X;
                _Center.Y = _Perimeter.Center.Y;
                _TriangularBounds = !_Perimeter.IsRectangular(true);

            }
        }

        public override dxfPlane Plane => new dxfPlane(CenterDXF);

        /// <summary>
        /// the number of times this spout group appears in a complete tray assembly
        /// </summary>
        public int CountPerTray
        {
            get => IsVirtual ? 0 : Instances.Count + 1;

        }


        /// <summary>
        /// the depth of the spout hole
        /// </summary
        public double Depth => Thickness;

        /// the percentage that the current spout area deviates fron the ideal
        /// </summary>
        public double ErrorPercentage { get { if (Invalid) UpdateSpouts(null); return _Grid.ErrorPercentage; } }

        /// <summary>
        /// the collection index of the spout group within its owning downcomer
        /// </summary>
        public override int GroupIndex { get => base.GroupIndex; set => base.GroupIndex = Abs(value); }

        public string GroupName => $"SG_{Handle}";

        /// <summary>
        /// a string that identifies the spout groups downcomer and panel index
        /// </summary>
        public string Handle => $"{DowncomerIndex},{PanelIndex}";

        public override uopInstances Instances { get => new uopInstances(_Instances, this); set => _Instances = new TINSTANCES(value); }

        public override string Name { get => GroupName; set { } }

        /// <summary>
        /// used by test if a change has been made to the curent project
        /// </summary>
        public bool HasChanged { get; set; }

        /// <summary>
        /// the x distance between spout rows in the spout group
        /// </summary>
        public double HorizontalPitch
        {
            get
            {
                if (Invalid) UpdateSpouts(null);
                return _Grid.HPitch;
            }
        }
        public override string INIPath => $"COLUMN({ColumnIndex}).RANGE({RangeIndex}).TRAYASSEMBLY.SPOUTGROUPS";

        public override bool Invalid
        {
            get { bool _rVal = _Spouts == null ? true : _Grid != null ? _Grid.Invalid : true; if (!_rVal && _Spouts != null && _Spouts.Count <= 0 && TheoreticalArea > 0) _rVal = true; return _rVal; }
            set { if (_Grid != null) _Grid.Invalid = value; base.Invalid = value; }
        }

        internal URECTANGLE Limits { get => _Limits; set => _Limits = value; }

        public double PanelBottom { get => _Limits.Bottom; set => _Limits.Bottom = value; }

        public double PanelTop { get => _Limits.Top; set => _Limits.Top = value; }

        public double DCLeft { get => _Limits.Left; set => _Limits.Left = value; }

        public double DCRight { get => _Limits.Right; set => _Limits.Right = value; }

        /// <summary>
        /// returns True if the spout groups spout bounds was effected by the endplate
        /// </summary>
        public bool LimitedBounds => SpoutArea == null ? false : SpoutArea.LimitedBounds;

        public bool LimitedTop => SpoutArea == null ? false : SpoutArea.LimitedTop;
        public bool LimitedBottom => SpoutArea == null ? false : SpoutArea.LimitedBottom;

        /// <summary>
        /// returns True if this group is at one of the end of it's owning downcomer
        /// and its owning downcomer has a triangular end which limits the available area of the spout group
        /// </summary>
        /// 

        private bool _TriangularBounds;
        public bool TriangularBounds
        {
            get
            {
                if (Invalid) UpdateSpouts(null);
                _TriangularBounds = !_Perimeter.IsRectangular();
                return _TriangularBounds;
            }
            set => _TriangularBounds = value;
        }

        /// <summary>
        /// the number of times the spout group occurs in the entire tray
        /// </summary>
        public override int OccuranceFactor => CountPerTray;

        /// <summary>
        /// a point within the boundary used as the first hole location of the pattern
        /// </summary>
        public dxfVector Origin { get { if (Invalid) UpdateSpouts(null); return Grid.Origin.ToDXFVector(); } }

        /// <summary>
        /// the top to bottom length of the spouts in the group
        /// </summary>
        /// <returns></returns>
        public double PatternLength { get { if (Invalid) UpdateSpouts(null); return Grid.PatternLength; } }

        /// <summary>
        /// the name of the spout pattern applied within the spout group
        /// </summary>
        /// <param name="bNullForUndefined"></param>
        /// <returns></returns>
        public string PatternName { get { return (SpoutCount() > 0 && PatternType > uppSpoutPatterns.Undefined) ? uopEnums.Description(PatternType) : ""; } }

        /// <summary>
        /// the pattern type
        /// </summary>
        public uppSpoutPatterns PatternType { get { if (Invalid) UpdateSpouts(null); return Grid.Pattern; } }

        /// <summary>
        /// the shape that is the default boundary of the spout group
        /// </summary>
        public uopShape Perimeter => new uopShape(_Perimeter, aTag: Handle);

        /// <summary>
        /// the number of rows in the spouts collection
        /// </summary>
        public int RowCount
        {
            get
            {

                return Grid.RowCount();
            }
        }
        public string SPRString
        {
            get
            {


                string _rVal = SpoutsPerRow.ToString();
                if (PatternType == uppSpoutPatterns.A)
                {
                    if (Grid.SPR >= 1) _rVal = $"{Grid.SPR - 1} / {_rVal}";

                }
                return _rVal;

            }
        }

        /// <summary>
        ///returns the length of the groups slot if it is not a hole or the hole diamter if its not equal to 0.75
        /// </summary>
        public double SlotLength { get { if (Invalid) UpdateSpouts(null); return _Grid.SpoutLength; } }
        /// <summary>
        /// the spout (hole or slot) used within the spout group
        /// </summary>
        public uopHole Spout { get { if (Invalid) UpdateSpouts(null); return new uopHole(_Grid._Spout); } }

        public override uopVector Center
        {
            get
            {
                if (Invalid) UpdateSpouts(null);
                base.Center = SpoutArea != null ? SpoutArea.Center : uopVector.Zero; //  Grid.SpoutCenter;
                return base.Center;
            }
            set { } // ignore this 
        }

        public dxfVector SpoutCenter
        {
            get
            {
                return Grid.SpoutCenter.ToDXFVector(false, false, 0, Handle);
            }
        }


        /// <summary>
        /// a string that defines all the spouts in the spout group
        /// </summary>
        public string SpoutString
        {
            get
            {
                if (Invalid) UpdateSpouts(null);
                return (PatternType == uppSpoutPatterns.SStar) ? _Grid._Spouts.SimpleDescriptor : _Grid._Spouts.UniformDescriptor;
            }
        }

        /// <summary>
        /// the number of spouts in a full row of the pattern
        /// </summary>
        public int SpoutsPerRow
        {
            get
            {

                if (Invalid) UpdateSpouts(null);
                return _Grid.SPR;
            }
        }

        internal USHAPE StartupBoundV { set { _StartupBound = value; } }

      

        /// <summary>
        /// the lines that startup spouts are created with
        /// </summary>
        public mdStartupLines StartupLines
        {
            get { 
                mdStartupLines _rVal =  new mdStartupLines(_SULines, true);
                return _rVal; 
            }

            set
            {
                if (value != null)
                    _SULines = value.Structure_Get();
                else
                    _SULines.Clear();
            }
        }

        public List<mdStartupSpout> StartupSpouts { get { mdTrayAssembly aAssy = GetMDTrayAssembly(); return (aAssy != null) ? aAssy.StartupSpouts.ToList().FindAll((x) => string.Compare(x.SpoutGroupHandle,Handle,true) ==0) : new List<mdStartupSpout>(); } }

        public double PanelY { get => SpoutArea != null ? SpoutArea.PanelY : 0; }

        /// <summary>
        /// the required theoretical spout area for the spout group
        /// </summary>

        public double TheoreticalArea
        {
            get => SpoutArea != null ? SpoutArea.IdealSpoutArea : 0;
            set
            {
                if (SpoutArea == null) return;

                if (double.IsNaN(value))
                    value = 0;
                if (SpoutArea.IdealSpoutArea != value) Invalid = true;
                SpoutArea.IdealSpoutArea = value;
            }
        }

        public int ZoneIndex => SpoutArea == null ? 0 : SpoutArea.ZoneIndex;

        /// <summary>
        /// the y distance between spout rows in the spout group
        /// </summary>
        public double VerticalPitch { get { if (Invalid) UpdateSpouts(null); return _Grid.VPitch; } }
        /// <summary>
        /// the overall width of the spout group
        /// </summary>
        public override double Width
        {
            get
            {

                mdTrayAssembly aAssy = null;
                mdDowncomer aDC = Downcomer(aAssy);
                if (aDC != null)
                {
                    double clrc = Constraints(aAssy).Clearance;
                    if (clrc <= 0) clrc = mdSpoutGroup.GetDefaultClearance(this);
                    return aDC.Width - 2 * clrc;
                }

                return 0;
            }
        }


        public mdSpoutArea SpoutArea { get; set; }

        public DowncomerInfo DCInfo => SpoutArea == null ? null : SpoutArea.DCInfo;

        internal mdSpoutGrid _Grid;

        public mdSpoutGrid Grid { get { _Grid ??= new mdSpoutGrid(this, null, null) { Invalid = true }; if (Invalid && !Reading) UpdateSpouts(null); return _Grid; } set => _Grid = value == null ? new mdSpoutGrid(this, null, null) { Invalid = true } : new mdSpoutGrid(value); }

        public new dxxOrthoDirections Direction => SpoutArea == null ? dxxOrthoDirections.Up : SpoutArea.Y >= BoxY ? dxxOrthoDirections.Up : dxxOrthoDirections.Down;



        private uopHoles _Spouts;
        /// <summary>
        /// the collection of spouts (holes or slots) that define the spout group
      
        public uopHoles Spouts
        {
            get { if (Invalid && !Reading)  UpdateSpouts(null);  return _Spouts; }
            private set => _Spouts = value;
        }

#endregion Properties

#region Methods

public override void SubPart(mdDowncomerBox aBox, string aCategory = null, bool? bHidden = null)
        {
            if (aBox == null) return;
            try
            {
                base.SubPart(aBox, aCategory, bHidden);
                DCLeft = aBox.X - 0.5 * aBox.InsideWidth;
                DCRight = aBox.X + 0.5 * aBox.InsideWidth;
            }
            catch (Exception e)
            {
                Debug.Fail(e.Message);
            }
            finally
            {


            }

        }


        public override string ToString() { return $"SPOUT GROUP({Handle})"; }


        public dxfBlock Block(string aBlockname = null, dxfDisplaySettings dsp = null, dxfImage aImage = null, bool? bSuppressCenterPts = null, bool bSuppressInstances = false)
        {
            if (string.IsNullOrWhiteSpace(aBlockname))
            {
                aBlockname = $"{TrayName(false)} SG-{DowncomerIndex}-{PanelIndex}";
                if (DesignFamily.IsBeamDesignFamily()) aBlockname += $"-{BoxIndex}";
            }

            if (dsp == null) dsp = new dxfDisplaySettings("SPOUTS");
            colDXFEntities sgents = BlockEntities(dsp.LayerName, dsp.Color, dsp.Linetype, bSuppressInstances: bSuppressInstances, aImage: aImage, bSuppressCenterPts: bSuppressCenterPts);
            sgents.Translate(-X, -Y);
            dxfBlock _rVal = new dxfBlock(sgents, aBlockname, bCloneEntities:false);
            if (!bSuppressInstances) _rVal.Instances = Instances.ToDXFInstances();
            return _rVal;

        }
        public colDXFEntities BlockEntities(string aLayerName = null, dxxColors aColor = dxxColors.Undefined, string aLinetype = null, double aHClineScale = 0, double aVClineScale = 0, double aDClineScale = 0, dxfImage aImage = null, dxxLinetypeLayerFlag aLTLSetting = dxxLinetypeLayerFlag.Undefined, bool bSuppressInstances = true, bool? bSuppressCenterPts = null)
        {
            if (Spouts.Count <= 0)
                return new colDXFEntities();
            if (!bSuppressCenterPts.HasValue) bSuppressCenterPts = !Grid.Pattern.UsesSlots(true);

            return Grid._Spouts.ToDXFEntities(aLayerName, aColor, aLinetype, aHClineScale, aVClineScale, aDClineScale, aImage, aLTLSetting, bSuppressInstances, null, bSuppressCenterPts.Value);
        }

        public void GetMirrorValues(out double? rMirrorX, out double? rMirrorY)
        {
            rMirrorX = null;
            rMirrorY = null;
            if (!DesignFamily.IsStandardDesignFamily()) return;
            uopInstance inst = Instances.Find((x) => x.YOffset == 0);
            if (inst != null) rMirrorX = 0;
            Instances.Find((x) => x.XOffset == 0);
            if (inst != null) rMirrorY =  BoxY;

            
        }

        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        public mdSpoutGroup Clone() => new mdSpoutGroup(this);

        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();

        private WeakReference<mdConstraint> _ConstraintsRef;
        public mdConstraint Constraints(mdTrayAssembly aAssy)
        {
            mdConstraint _rVal = null;
            mdSpoutGroup parent = IsVirtual ? Parent : null;
            if (parent != null)
            {
                _rVal = parent.Constraints(aAssy);
                if (_rVal != null)
                {
                    _ConstraintsRef = new WeakReference<mdConstraint>(_rVal);
                    return _rVal;
                }
            }

            if (_ConstraintsRef != null)
            {
                if (!_ConstraintsRef.TryGetTarget(out _rVal)) _ConstraintsRef = null;
                if (_rVal != null) return _rVal;
            }

            if (_rVal == null)
            {
                aAssy ??= GetMDTrayAssembly(aAssy);
                if (aAssy != null)
                {
                    if (aAssy._Constraints != null)
                    {
                        _rVal = aAssy?._Constraints?.ToList().Find((x) => x.PanelIndex == PanelIndex && x.DowncomerIndex == DowncomerIndex && x.BoxIndex == BoxIndex);
                        if (_rVal != null)
                            SetConstraints(_rVal);
                    }
                }

            }
            return _rVal; // ?? new mdConstraint();
        }

        public void SetConstraints(mdConstraint aConstraints)
        {
            if (aConstraints == null)
            {
                _ConstraintsRef = null;
                return;
            }
            _ConstraintsRef = new WeakReference<mdConstraint>(aConstraints);
            aConstraints.SetSpoutGroup(this);
        }
        /// <summary>
        ///returns the objects CurrentProperties in a collection
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="bAppendConstraints"></param>
        /// <returns></returns>
        public uopProperties CurrentProperties(mdTrayAssembly aAssy, bool bAppendConstraints = false)
        {

            UpdatePartProperties(aAssy);
            uopProperties _rVal = base.CurrentProperties();
            if (bAppendConstraints)
            {
                mdConstraint aCns = Constraints(aAssy);
                if (aCns != null)
                {
                    aCns.UpdatePartProperties();
                    TPROPERTIES aProps = aCns.ActiveProps;
                    TPROPERTIES bProps = new TPROPERTIES(_rVal);
                    for (int i = 1; i <= aProps.Count; i++)
                    {
                        TPROPERTY aProp = aProps.Item(i);
                        bProps.Add(aProp, $"Constraints.{aProp.Name}");
                    }
                    _rVal = new uopProperties(bProps);
                }
            }
            return _rVal;
        }

        public override uopProperty CurrentProperty(string aPropertyName, bool bSupressNotFoundError = false)
        {
            UpdatePartProperties();
            return base.CurrentProperty(aPropertyName, bSupressNotFoundError);
        }
        /// <summary>
        /// the parent deck panel of this spout group
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public mdDeckPanel DeckPanel(mdTrayAssembly aAssy) => GetMDPanel(aAssy, null, PanelIndex);

        public void Mirror(double? aX, double? aY)
        {
            if (!aX.HasValue && !aY.HasValue) return;
            double dx = aX.HasValue ? X - aX.Value : 0;
            double dy = aY.HasValue ? Y - aY.Value : 0;
            if (dx == 0 && dy == 0) return;

            SpoutArea.Mirror(aX, aY);
            Center.Mirror(aX, aY);
            _Limits.Mirror(aX, aY);
            _Perimeter.Mirror(aX, aY);

            _SULines.Mirror(aX, aY);
            Grid.Mirror(aX, aY);


        }

        /// <summary>
        /// the collection that this constraint is a member of
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        private colMDSpoutGroups MyCollection(mdTrayAssembly aAssy = null) { aAssy ??= GetMDTrayAssembly(aAssy); return aAssy?.SpoutGroups; }

        internal UHOLES GetSpoutGroupSpouts(bool bReturnQuad2 = false, bool bReturnQuad3 = false, bool bReturnQuad4 = false, double aDepth = 0, dynamic aZ = null)
        {
            UHOLES _rVal = new UHOLES();
            UHOLES aSpts = new UHOLES(Spouts);
            if (aSpts.Count <= 0) return _rVal;
            string hndl = Handle;
            UHOLE aSpt = new UHOLE(aSpts.Member) { Name = $"SPOUTS_{hndl}" };

            double sz = mzUtils.VarToDouble(aZ, false, Z);
            aSpt.Elevation = sz;
            aSpts.Member = aSpt;
            _rVal = aSpts;
            if (!bReturnQuad2 && !bReturnQuad3 && !bReturnQuad4) return _rVal;
            uopInstances insts = Instances;
            if (insts.Count <= 0) return _rVal;
            UVECTORS basectrs = new UVECTORS(_rVal.Centers);
            aSpt = aSpts.Item(1);
            uopVector u0 = new uopVector(X, Y);
            uopVectors allpts = uopVectors.Zero;
            UVECTORS ctrs = UVECTORS.Zero;
            foreach (var item in insts)
            {
                UVECTOR ui = new UVECTOR(X + item.DX, Y + item.DY);
                bool keep = false;
                if (ui.X < 0 && ui.Y > 0 && bReturnQuad2) keep = true;

                if (ui.X < 0 && ui.Y < 0 && bReturnQuad3) keep = true;
                if (ui.X > 0 && ui.Y < BoxY && bReturnQuad4)
                    keep = true;

                if (keep)
                {
                    UVECTOR unew = new UVECTOR(ui) + new UVECTOR(item.Displacement);
                    ctrs = new UVECTORS(basectrs, aDisplacement: item.Displacement);
                    if (item.LeftHanded)
                        ctrs.Mirror(unew.X, null);
                    if (item.Inverted)
                        ctrs.Mirror(null, unew.Y);
                    if (item.Rotation != 0)
                        ctrs.Rotate(unew, item.Rotation);

                    _rVal.Centers.Append(ctrs);
                }
            }

            _rVal.Centers.Append(ctrs);
            return _rVal;
        }

        internal UHOLES GetSpoutGroupSpouts(bool bReturnInstances = false, double aDepth = 0, dynamic aZ = null)
        {
            UHOLES _rVal = new UHOLES();
            UHOLES aSpts = new UHOLES(Spouts);
            if (aSpts.Count <= 0) return _rVal;
            string hndl = Handle;
            UHOLE aSpt = new UHOLE(aSpts.Member) { Name = $"SPOUTS_{hndl}" };

            double sz = mzUtils.VarToDouble(aZ, false, Z);

            aSpt.Elevation = sz;
            aSpts.Member = aSpt;
            _rVal = aSpts;
            if (!bReturnInstances) return _rVal;

            uopInstances insts = Instances;
            UVECTORS basectrs = new UVECTORS(_rVal.Centers);
            foreach (var inst in insts)
            {
                UVECTOR ui = new UVECTOR(X + inst.DX, Y + inst.DY);
                UVECTOR unew = new UVECTOR(ui) + new UVECTOR(inst.Displacement);
                UVECTORS ctrs = new UVECTORS(basectrs, aDisplacement: inst.Displacement);
                if (inst.LeftHanded)
                    ctrs.Mirror(unew.X, null);
                if (inst.Inverted)
                    ctrs.Mirror(null, unew.Y);
                if (inst.Rotation != 0)
                    ctrs.Rotate(unew, inst.Rotation);

                _rVal.Centers.Append(ctrs);
                _rVal.Centers.Append(new UVECTORS(basectrs, aDisplacement: inst.Displacement));
            }


            return _rVal;
        }

        /// <summary>
        /// returns true if the startup edges are defined
        /// </summary>
        /// <param name="rHasBound"></param>
        /// <param name="rHasLines"></param>
        /// <returns></returns>
        public bool HasStartupObjects(out bool rHasBound, out bool rHasLines)
        {

            rHasBound = _StartupBound.IsDefined;
            rHasLines = _SULines.Count > 0;
            return rHasLines && rHasBound;
        }

        public bool IsEqual(mdSpoutGroup aSpoutGroup, mdTrayAssembly aAssy)
        {
            bool isEqual = false;
            if (aSpoutGroup == null) return false;

            //todo
            //if (!aSpoutGroup.Constraints(aAssy[_WithVar_ReportProperties.IsEqual(Constraints(aAssy)) {
            //    return IsEqual;

            //}
            //var prtColumn = uppPartTypes.Column;
            //IsEqual = TPROPERTIES.Compare(GetProps(prtColumn), aSpoutGroup.GetProps(prtColumn));
            return isEqual;
        }

        /// <summary>
        /// returns True if the spout groups constraint object has its TreatAsGroup Property = True
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public bool IsGroupMember(mdTrayAssembly aAssy) => Constraints(aAssy).TreatAsGroup;

        /// <summary>
        /// extracts the parts property values from the passed file array that was read from an INI style project file.
        /// </summary>
        /// <param name="aProject">The project requesting the read event</param>
        /// <param name="aFileProps">The property array containing the INI file properties or a subset. The Name of the array should contain the original file name.</param>
        /// <param name="ioWarnings">A collection to populate if errors or warnings are found during the property value extraction  </param>
        /// <param name="aFileVersion">The version of th efile being read. Supplied to account for backward compatibility</param>
        /// <param name="aFileSection">the INI file heading to search for the properties to extract </param>
        /// <param name="bIgnoreNotFound">A flag to ignore properties that exist on the part but were not found in the file properties</param>
        /// <param name="aAssy">An optional parent tray assembly for the part being read</param>
        /// <param name="aPartParameter">An optional parent part for the part being read</param>
        /// <param name="aSkipList">An optional list of property names to skip over during the read</param>
        public void ReadProperties(uopProject aProject, uopPropertyArray aFileProps, ref uopDocuments ioWarnings, double aFileVersion, string aFileSection = null, bool bIgnoreNotFound = false, uopTrayAssembly aAssy = null, uopPart aPartParameter = null, List<string> aSkipList = null, Dictionary<string, string> EqualNames = null, uopProperties aProperties = null, bool bRegenOnly = false)
        {

            try
            {
                ioWarnings ??= new uopDocuments();
                aFileSection = string.IsNullOrWhiteSpace(aFileSection) ? INIPath : aFileSection.Trim();
                if (aFileProps == null) throw new Exception("The Passed File Property Array is Undefined");
                if (string.IsNullOrWhiteSpace(aFileSection)) throw new Exception("File Section is Undefined");
                if (aProject != null) SubPart(aProject);
                if (aAssy == null) throw new Exception("Tray Assembly is Undefined");
          
                mdTrayAssembly myassy = (mdTrayAssembly)aAssy;
                SubPart(myassy);

                string hnd = Handle;
                int idx = Index;

                if (IsVirtual)
                {
                    mdSpoutGroup parent = Parent;
                    if (parent != null)
                    {
                        hnd = parent.Handle;
                        idx = parent.Index;
                    }
                }

                string dstr = aFileProps.ValueS(aFileSection, $"SpoutGroup{idx}", out bool found);
                if (found && (dstr.Substring(0, hnd.Length) != hnd))
                {
                    idx++;
                    dstr = aFileProps.ValueS(aFileSection, $"SpoutGroup{idx}", out found);
                    if (found) found = dstr.Substring(0, hnd.Length) == hnd;

                }
                if (found)
                {
                    dstr = mzUtils.FixGlobalDelimError(dstr);
                    found = dstr.IndexOf(uopGlobals.Delim, StringComparison.OrdinalIgnoreCase) > 0;
                }

                if (found)
                {
                    Reading = true;
                    if (aFileVersion < 3.12) bRegenOnly = true;
                    aProject?.ReadStatus($"Reading {TrayName(true)} Spout Group({hnd}) Properties");
                    mdSpoutGrid newgrid = UpdateSpouts(myassy, aFileProps, aFileSection, bRegenSpouts: bRegenOnly);
                    _Grid = newgrid; // ew USPOUTGRID(newgrid) { Invalid = !string.IsNullOrWhiteSpace(newgrid.ErrorString) };

                    if (_Grid.Invalid)
                        ioWarnings.AddWarning(this, "Spout Group Generation Error", _Grid.ErrorString);

                }
                else
                {
                    ioWarnings.AddWarning(this, "Missing Spout Group Data", $"Configuration Data For Range({RangeIndex}).SpoutGroup({Handle}) Was Not Found In File '{aFileProps.Name}'");
                    RegenerateSpouts(myassy, false);
                }
            }
            finally
            {
                Spouts = _Grid!= null? _Grid.Spouts : null;
                Reading = false;
                aProject?.ReadStatus("", 2);
            }
        }
        /// <summary>
        /// causes/forces a regeneration of the spout groups spouts colleciton
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="bNonExistantOnly"></param>
        public void RegenerateSpouts(mdTrayAssembly aAssy, bool bNonExistantOnly, bool? bSuppressEvnts = null)
        {
            if (!bSuppressEvnts.HasValue) bSuppressEvnts = SuppressEvents;
            bool wuz = SuppressEvents;
            SuppressEvents = bSuppressEvnts.Value;
            if (!bNonExistantOnly | (bNonExistantOnly & SpoutCount(aAssy) <= 0))
            {
                if (TheoreticalArea > 0)
                    UpdateSpouts(aAssy);
            }
            SuppressEvents = wuz;
        }

        /// <summary>
        /// Get Report Properties
        /// </summary>
        /// <param name="aTableIndex"></param>
        /// <returns></returns>
        public uopProperties ReportProperties(int aTableIndex, List<string> spoutLengthNotes)
        {
            uopProperties _rVal = new uopProperties();

            mdTrayAssembly aAssy = GetMDTrayAssembly();

            if (aAssy == null || IsVirtual) return _rVal;
            if (Invalid)
            {
                UpdateSpouts(aAssy);
            }


            mdDeckPanel aDP = GetMDPanel(aAssy, null, PanelIndex);
            mdDowncomer aDC = GetMDDowncomer(aAssy, null, DowncomerIndex);

            if (aTableIndex == 2)
            {
                _rVal = DowncomerReportProperties(out mdConstraint _, out uopProperties _, out uopProperty _, aAssy, aDP, aDC);
            }
            else if (aTableIndex == 1)
            {
                DeckPanelReportProperties(out _rVal, aAssy, Spout, aDP, aDC, spoutLengthNotes);
            }
            else
            {
                return null;
            }
            return _rVal;

        }

        /// <summary>
        /// Deck Panel Report Properties
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="aAssy"></param>
        /// <param name="aSpout"></param>
        /// <param name="aDP"></param>
        /// <param name="aDC"></param>
        /// <returns></returns>
        private uopProperties DeckPanelReportProperties(out uopProperties _rVal, mdTrayAssembly aAssy, uopHole aSpout, mdDeckPanel aDP, mdDowncomer aDC, List<string> spoutLengthNotes)
        {
            _rVal = new uopProperties
            {
                { "DC", aDC.Label },
                { "Panel", aDP.Label },
                { "No. of" + "\n" + "Spouts", SpoutCount(aAssy) },
                { "Spout" + "\n" + "Pattern", PatternName }
            };
            spoutLengthNotes ??= new List<string>();
            if (PatternName == "*S*")
            {
                string lengths = string.Join(", ", SpoutsV.Centers.ToList.Select(x => Math.Round(x.Value, 4)));

                spoutLengthNotes.Add("Note " + (spoutLengthNotes.Count + 1) + ": " + string.Join(", ", SpoutsV.Centers.ToList.Select(x => Math.Round(x.Value, 4))));
                _rVal.Add("Slot\nLength", "Note " + spoutLengthNotes.Count);
            }
            else if (aSpout != null && aSpout.Length > aSpout.Diameter)
            {
                _rVal.Add("Slot\nLength", aSpout.Length, uppUnitTypes.SmallLength);
            }
            else
            {
                _rVal.Add("Slot\nLength", string.Empty, uppUnitTypes.SmallLength);
            }
            _rVal.Add("Spouts/" + "\n" + "Row", SPRString);
            _rVal.Add("Pitch", Grid.VPitch, uppUnitTypes.SmallLength);
            _rVal.Add("Pattern" + "\n" + "Length", Grid.PatternLength, uppUnitTypes.SmallLength);
            _rVal.Add("Margin", ActualMargin, uppUnitTypes.SmallLength);
            _rVal.Add("\n" + "Ideal", Grid.TargetArea, uppUnitTypes.SmallArea);
            _rVal.Add("\n" + "Actual", Grid.TotalArea, uppUnitTypes.SmallArea);
            _rVal.Add("Spout Area" + "\n" + "Error", Grid.ErrorPercentage, uppUnitTypes.Percentage);
            _rVal.SetProtected(true);
            return _rVal;
        }

        /// <summary>
        /// Downcomer Report Properties
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="Cnstr"></param>
        /// <param name="cProps"></param>
        /// <param name="cProp"></param>
        /// <param name="aAssy"></param>
        /// <param name="aDP"></param>
        /// <param name="aDC"></param>
        private uopProperties DowncomerReportProperties(out mdConstraint Cnstr, out uopProperties cProps, out uopProperty cProp, mdTrayAssembly aAssy, mdDeckPanel aDP, mdDowncomer aDC)
        {
            Cnstr = Constraints(aAssy);
            Cnstr.SetPropertyLimits(aAssy, this);
            cProps = Cnstr.CurrentProperties();

            uopProperties _rVal = new uopProperties
            {
                { "Downcomer", aDC.Label },
                { "Panel", aDP.Label },
                { "Pattern Type", Cnstr.PatternType == uppSpoutPatterns.Undefined ? string.Empty : Cnstr.PatternName }
            };

            cProp = cProps.Item("SpoutCount");
            _rVal.Add(cProp.DisplayName, cProp.IsDefault ? string.Empty : cProp.Value);

            cProp = cProps.Item("SpoutLength");
            _rVal.Add(cProp.DisplayName, cProp.IsDefault ? string.Empty : cProp.Value, uppUnitTypes.SmallLength);

            cProp = cProps.Item("VerticalPitch");
            _rVal.Add(cProp.DisplayName, cProp.IsDefault ? string.Empty : cProp.Value, uppUnitTypes.SmallLength);

            cProp = cProps.Item("Margin");
            _rVal.Add(cProp.DisplayName, cProp.IsDefault ? string.Empty : cProp.Value, uppUnitTypes.SmallLength);

            cProp = cProps.Item("SpoutDiameter");
            _rVal.Add(cProp.DisplayName, cProp.IsDefault ? string.Empty : cProp.Value, uppUnitTypes.SmallLength);

            cProp = cProps.Item("Clearance");
            _rVal.Add(cProp.DisplayName, cProp.IsDefault ? string.Empty : cProp.Value, uppUnitTypes.SmallLength);

            cProp = cProps.Item("HorizontalPitch");
            _rVal.Add(cProp.DisplayName, cProp.IsDefault ? string.Empty : cProp.Value, uppUnitTypes.SmallLength);

            cProp = cProps.Item("EndplateClearance");
            _rVal.Add(cProp.DisplayName, cProp.IsDefault ? string.Empty : cProp.Value, uppUnitTypes.SmallLength);

            cProp = cProps.Item("YOffset");
            _rVal.Add(cProp.DisplayName, cProp.IsDefault ? string.Empty : cProp.Value, uppUnitTypes.SmallLength);

            _rVal.Add("Treat as Ideal", Cnstr.TreatAsIdeal ? "X" : string.Empty);
            _rVal.Add("Treat as Group", Cnstr.TreatAsGroup ? "X" : string.Empty);

            _rVal.SetProtected(true);
            return _rVal;

        }

        //returns the objects properties in a collection
        //signatures like "Name=Project1"
        public override uopProperties CurrentProperties()
        {
            UpdatePartProperties();

            return new uopProperties(ActiveProps);
            //}
            //set { if (value != null) SetProps(value.Structure); }
        }

        public void ResetSpouts()
        {
            Invalid = true;
            _SULines.Clear();
        }

        /// <summary>
        ///returns the properties required to save the spout group to file
        /// </summary>
        public override uopPropertyArray SaveProperties(string aHeading = null)
        {
            aHeading = string.IsNullOrWhiteSpace(aHeading) ? INIPath : aHeading;
            uopProperties props = new uopProperties();
            if (Invalid) UpdateSpouts(null);
            dxfVector aOrigin = Origin;
            uppSpoutPatterns pType = Grid.Pattern;


            props.Add("Handle", Handle);
            props.Add("PatternName", mdUtils.PatternName(pType));
            //substracted 1 to make it competible with new uppSpoutPatterns enum seq
            props.Add("PatternType", (int)pType - 1);
            props.Add("TheoreticalArea", TheoreticalArea);
            props.Add("VerticalPitch", _Grid.VPitch);
            props.Add("HorizontalPitch", _Grid.HPitch);

            props.Add("MaximizedBounds", Convert.ToInt32(false));
            uopProperties _rVal = new uopProperties();
            string aStr = props.DeliminatedString(uopGlobals.Delim);
            _rVal.Add($"SpoutGroup{Index}", aStr);
            aStr = SpoutString;
            aStr = $"({Math.Round(aOrigin.X, 6)},{Math.Round(aOrigin.Y, 6)},0,False){uopGlobals.Delim + aStr}";
            _rVal.Add("SpoutGroup" + Index.ToString() + ".Spouts", aStr);

            return new uopPropertyArray(_rVal, aName: aHeading, aHeading: aHeading);

        }

        public int SpoutCount(mdTrayAssembly aAssy = null)
        {
            if (Invalid) UpdateSpouts(aAssy);
            return Spouts.Count;
        }

        /// <summary>
        /// the diameter of spouts in the group
        /// </summary>
        /// <returns></returns>
        public double SpoutDiameter()
        => Grid.SpoutRadius * 2;

   
            

        /// <summary>
        /// the polygon that is internal to the perimeter polygon and is the absolute max limits of the area
        /// where startup spouts can be placed
        /// </summary>
        /// <param name="bRegen"></param>
        /// <returns></returns>
        public uopShape StartupBound(bool bRegen)
        {
            if (bRegen) 
                mdStartUps.UpdateSpoutObjects(this, null, true); 
            _StartupBound.Value = Thickness;
            return new uopShape(_StartupBound, aTag: Handle) { Value = Thickness };
        }

        /// <summary>
        /// the target area is the area the spout generator tried to achieve on the last run
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public double TargetArea(mdTrayAssembly aAssy = null) { if (Invalid) { UpdateSpouts(aAssy); } return _Grid.TargetArea; }

        public override void UpdatePartProperties() { UpdatePartProperties(null, null); }

        public void UpdatePartProperties(mdTrayAssembly aAssy = null, mdConstraint aCnstr = null)
        {
            if (Invalid) { UpdateSpouts(aAssy); }

            PropValSet("TheoreticalArea", Grid.TargetArea, bSuppressEvnts: true);
            PropValSet("Spout", _Grid._Spout.SimpleDescriptor, bSuppressEvnts: true);
            PropValSet("PatternType", _Grid.Pattern, bSuppressEvnts: true);
            PropValSet("ActualArea", _Grid.TotalArea, bSuppressEvnts: true);
            PropValSet("AppliedClearance", _Grid.AppliedClearance, bSuppressEvnts: true);
            PropValSet("ActualClearance", ActualClearance, bSuppressEvnts: true);
            PropValSet("ActualMargin", ActualMargin, bSuppressEvnts: true);
            PropValSet("RowCount", _Grid.RowCount(), bSuppressEvnts: true);
            PropValSet("Origin", _Grid.Origin.Coords(6), bSuppressEvnts: true);
            PropValSet("SpoutString", SpoutString, bSuppressEvnts: true);
            PropValSet("HorizontalPitch", _Grid.HPitch, bSuppressEvnts: true);
            PropValSet("PatternLength", _Grid.PatternLength, bSuppressEvnts: true);
            PropValSet("VerticalPitch", _Grid.VPitch, bSuppressEvnts: true);
            PropValSet("LimitedBounds", _Grid.SpoutArea.LimitedBounds, bSuppressEvnts: true);
            PropValSet("GroupIndex", GroupIndex, bSuppressEvnts: true);
            PropValSet("SpoutsPerRow", _Grid.SPR, bSuppressEvnts: true);
        }

        public override void UpdatePartWeight() { base.Weight = 0; }

        /// <summary>
        /// the collection of spouts (holes or slots) that define the spout group
        /// </summary>
        /// <param name="aAssy"></param>
        public void UpdateSpouts(mdTrayAssembly aAssy)
        {
            if (!_Perimeter.IsDefined) return;
            //if (string.IsNullOrWhiteSpace(aFileSpec))
            //{

            colMDSpoutGroups aSGCol = null;
            aAssy ??= GetMDTrayAssembly(aAssy);
            //raise the generation event to the assembly and listeners
            if (!SuppressEvents)
                aSGCol = MyCollection(aAssy);
            if (aSGCol != null) aSGCol.NotifyGeneration(this, true);
            if (aAssy != null) aAssy.RaiseStatusChangeEvent($"Generating {aAssy.TrayName()}  Spout Group {Handle} Spouts");

            mdSpoutGrid newgrid = mdSpoutGrid.GenSpouts(aAssy, this, Constraints(aAssy));
            _Grid = newgrid;
            Spouts = _Grid.Spouts;
            if (aSGCol != null) aSGCol.NotifyGeneration(this, false);

            
        }

        /// <summary>
        /// this will update the assembly level constraints
        /// </summary>
        public void UpdateConstraints(mdTrayAssembly aAssy)
        {
            aAssy ??= GetMDTrayAssembly();
            if (aAssy == null) return;

            SubPart(aAssy);

            if (_ConstraintsRef == null) return;
            if (!_ConstraintsRef.TryGetTarget(out mdConstraint constraints)) _ConstraintsRef = null;
                if (constraints== null) return;
            
            mdConstraint assyconstraints = aAssy.Constraints.GetByHandle(Handle);
            if(assyconstraints== null)
            {
                aAssy.Constraints.Add(constraints);
            }
            else 
            {
                if (constraints != assyconstraints)
                {
                    assyconstraints.PropValsCopy(  constraints.ActiveProps);
                    _ConstraintsRef = new WeakReference<mdConstraint>(assyconstraints);
                }
                
            }
        }


        /// <summary>
        /// defines the spouts based on the descriptor strings read from the passed file data
        /// </summary>
        /// <param name="aAssy">the parent assembly</param>
        /// <param name="aFileData">a property array containind the file data</param>
        /// <param name="aFileSection">the file section to read the spout group info from</param>
        /// <param name="bRegenSpouts">aflag to read the pattern info but to regenerate the spouts using the current generation rule</param>
        public mdSpoutGrid UpdateSpouts(mdTrayAssembly aAssy, uopPropertyArray aFileData, string aFileSection = null, bool bRegenSpouts = false)
        {
            if (!_Perimeter.IsDefined || aFileData == null || string.IsNullOrWhiteSpace(aFileSection)) return new mdSpoutGrid(this, Constraints(null), GetMDTrayAssembly()) { ErrorString = "$Read File Data Error" };

            //silently cause we are reading
            bool wuz = Reading;
            Reading = true;
            mdSpoutGrid _rVal = mdSpoutGrid.GenerateSpouts(this, null, aAssy, out string ERR, aFileData, aFileSection, out string aDescriptor, bRegenSpouts);

            _rVal.Invalid = !string.IsNullOrWhiteSpace(ERR) || !string.IsNullOrWhiteSpace(_rVal.ErrorString);

            if (_rVal.Invalid && string.IsNullOrWhiteSpace(_rVal.ErrorString)) _rVal.ErrorString = ERR;
            _rVal.Invalid = !string.IsNullOrWhiteSpace(_rVal.ErrorString); 
          
            Reading = wuz;
            return _rVal;
        }

        // <summary>
        /// sets the property in the collection with the passed name to the passed value
        //returns the property if the property value actually changes.
        /// </summary>
        public override uopProperty PropValSet(string aName, object aPropVal, int aOccur = 0, bool? bSuppressEvnts = null, bool? bHiddenVal = null)
        {

            bool supevnts = bSuppressEvnts.HasValue ? bSuppressEvnts.Value : SuppressEvents || Reading;
            uopProperty _rVal = base.PropValSet(aName, aPropVal, aOccur, supevnts, bHiddenVal);
            _Grid ??= new mdSpoutGrid(this, null, null) { Invalid = true };
            double dval = mzUtils.VarToDouble(aPropVal);


            switch (aName.ToLower())

            {

              
                case "theoreticalarea":

                    TheoreticalArea = dval;
                    break;
                case "targetarea":
                    if (_Grid != null && _Grid.TargetArea != dval ) Invalid = true;
                    _Grid.TargetArea = mzUtils.VarToDouble(aPropVal);
                    break;
                case "patterntype":
                    uppSpoutPatterns ptype = (uppSpoutPatterns)mzUtils.VarToInteger(aPropVal);
                    if(_Grid.Pattern != ptype) Invalid = true;
                    _Grid.Pattern = ptype; 
                    break;
                case "verticalpitch":
                    if (_Grid.VPitch != dval) Invalid = true;
                    _Grid.VPitch = dval;
                    break;
                case "horizontalpitch":
                    if (_Grid.HPitch != dval) Invalid = true;
                    _Grid.HPitch = dval;
                    break;
                case "spoutdiameter":
                    if (_Grid.SpoutRadius != dval/2) Invalid = true;
                    _Grid.SpoutRadius = dval / 2;
                    break;
                case "spoutlength":
                    if (_Grid._Spout.Length != dval / 2) Invalid = true;
                    _Grid._Spout.Length = dval;
                    break;
                case "spoutsperrow":
                    int ival = mzUtils.VarToInteger(aPropVal); 
                    if (_Grid.SPR != ival) Invalid = true;
                    _Grid.SPR = mzUtils.VarToInteger(aPropVal);
                    break;
                case "groupindex":
                    GroupIndex = mzUtils.VarToInteger(aPropVal);
                    break;
                case "downcomerindex":
                    DowncomerIndex = mzUtils.VarToInteger(aPropVal);
                    break;
                case "panelindex":
                    PanelIndex = mzUtils.VarToInteger(aPropVal);
                    break;

                case "z":
                case "zord":
                    Z = dval;
                    break;


            }
            if (_rVal == null || supevnts) return _rVal;

            return _rVal;
        }

  
        /// <summary>
        /// returns True is there is a constraint that affects the indicated property
        /// 1 the name of the property to test for constraint
        /// 2 a comma delimited string of property names to test
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aConstraints"></param>
        /// <param name="aPropertyNameList"></param>
        /// <param name="rConstrProps"></param>
        /// <returns></returns>
        public bool IsConstrainedIn(mdTrayAssembly aAssy, mdConstraint aCN, string aPropertyList, out string rConstrProps)
        {
            rConstrProps = string.Empty;

            aAssy ??= GetMDTrayAssembly(aAssy);
            if (aAssy == null) return false;
            aCN ??= Constraints(aAssy);
            if (aCN == null) return false;
            TPROPERTIES cProps = aCN.ActiveProps.GetByDefaultStatus(false, aPropertyList);
            rConstrProps = cProps.NameList();
            return cProps.Count > 0;


        }

        #endregion Methods

        #region Shared Methods
        public static double GetDefaultClearance(uopPart aPart) => GetDefaultClearance(aPart == null ? 0 : aPart.Thickness);

        public static  double GetDefaultClearance(double aThickness) => aThickness > 0 ? Math.Round(2 * aThickness + (1.0 / 16.0), 6) : 0.25;
        /// <summary>
        /// Sets the intitial vertical pitch depending on the current pattern type
        /// </summary>
        /// <param name="aPattern"></param>
        /// <param name="aRadius"></param>
        /// <returns></returns>
        public static double GetDefaultPitch_H(uppSpoutPatterns aPattern, double aRadius)
        {
            if (aRadius <= 0)
                aRadius = 0.75 / 2;

            double result = 0.0;
            switch (aPattern)
            {
                case uppSpoutPatterns.A:
                case uppSpoutPatterns.Astar:
                case uppSpoutPatterns.B:
                    result = uopUtils.RoundTo((2 * aRadius) + (3.0 / 16), dxxRoundToLimits.Sixteenth);
                    break;
                case uppSpoutPatterns.C:
                    result = uopUtils.RoundTo((2 * aRadius) + (3.0 / 8), dxxRoundToLimits.Sixteenth);
                    break;
                case uppSpoutPatterns.D:
                    result = uopUtils.RoundTo((2 * aRadius) + 1, dxxRoundToLimits.Sixteenth);
                    break;
                default:
                    break;
            }

            return result;
        }

        /// <summary>
        /// Sets the intitial vertical pitch depending on the current pattern type
        /// </summary>
        /// <param name="aPattern"></param>
        /// <param name="aRadius"></param>
        /// <returns></returns>
        public static double GetDefaultPitch_V(uppSpoutPatterns aPattern, double aRadius)
        {
            if (aRadius <= 0)
                aRadius = 0.75 / 2;

            double result = 0.0;
            switch (aPattern)
            {
                case uppSpoutPatterns.A:
                case uppSpoutPatterns.Astar:
                    result = uopUtils.RoundTo((2 * aRadius) + (1.0 / 16), dxxRoundToLimits.Sixteenth);
                    break;
                default:
                    result = uopUtils.RoundTo((2 * aRadius) + (3.0 / 16), dxxRoundToLimits.Sixteenth);
                    break;
            }

            return result;
        }
        
        #endregion Shared Methods
    }
}