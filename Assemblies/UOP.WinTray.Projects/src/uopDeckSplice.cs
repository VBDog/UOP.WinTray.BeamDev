using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;
using static System.Collections.Specialized.BitVector32;

namespace UOP.WinTray.Projects
{
    public class uopDeckSplice: ICloneable
    {

        #region Constructors

        public uopDeckSplice() => Init();

        public uopDeckSplice(uopDeckSplice aSplice) => Init(aSplice);

        private void Init(uopDeckSplice aSplice = null)
        {
            if (aSplice == null)
            {
                ProjectFamily = uppProjectFamilies.Undefined;
                MDDesignFamily = uppMDDesigns.Undefined;
                PanelLimits = URECTANGLE.Null;
                _FlangeLine = null;
                Vertical = false;
                SpliceIndex = -1;
                SpliceType = uppSpliceTypes.Undefined;
                BaseFlangeLength = 0;
                SpliceStyle = uppSpliceStyles.Undefined;
                LeftDowncomerClearance = 0;
                RightDowncomerClearance = 0;
                ManwayHandle = string.Empty;
                Side = uppSides.Undefined;
                ManTag = string.Empty;
             
                _TabDirection = null;
                PanelIndex = 0;
                MaxOrdinate = null;
                MinOrdinate = null;
                JoggleSpliceLimit = 0;
                Center = uopVector.Zero;
                Defined = false;
                RangeGUID = string.Empty;
                ProjectHandle = string.Empty;
                Selected = false;
                Index = -1;
                FlangeHt = 0;
                ShellRadius = 0;
                RingRadius = 0;
                DeckRadius = 0;
                _TabDirection = null;
                Depth = 0;
               
                SpliceAngleLength = 0;
                SpliceBoltCount = 0;
                TabCount = 0;
                BoltSpacing = 0;
                TabSpacing = 0;
            }
            else
            {
                Copy(aSplice);
        
            }
            
        }

        #endregion Constructors

        #region Fields
        //the line that defines the outermost edge of the formed flange in the deck panel if one is required.
      
        internal URECTANGLE PanelLimits;

        #endregion Fields

        #region Properties

        public double FastenerSpacing  => FastenerType == uppSpliceFastenerTypes.SlotAndTab? TabSpacing : BoltSpacing;


        private uopFlangeLine _FlangeLine;
        public uopFlangeLine FlangeLine { get { _FlangeLine ??= new uopFlangeLine(this); return _FlangeLine; }  set => _FlangeLine = value ; }

        public List<double> FastenerOrdinates => FlangeLine == null ? new List<double>() : FlangeLine.FastenerOrdinates;
            

        public double JoggleAngleHeight 
        { 
            get
            {
                 return (Female && (SpliceType == uppSpliceTypes.SpliceWithTabs || SpliceType == uppSpliceTypes.SpliceWithJoggle))   ? FlangeHt > 0 ? FlangeHt: 1.5 : 0.0;
            }  
        }
        public uppProjectFamilies ProjectFamily { get;  set; }

        public uppMDDesigns MDDesignFamily { get;  set; }

        public bool Vertical { get; set; }

        public int SpliceIndex { get; set; }
        public uppSpliceTypes SpliceType { get; set; }

        public uppSpliceAngleTypes SpliceAngleType { get =>  SpliceType == uppSpliceTypes.SpliceWithAngle ? SpliceType == uppSpliceTypes.SpliceManwayCenter ? uppSpliceAngleTypes.ManwaySplicePlate : SupportsManway ? uppSpliceAngleTypes.ManwayAngle :  uppSpliceAngleTypes.SpliceAngle : uppSpliceAngleTypes.Undefined; }
   
        public uppSpliceIndicators SpliceIndicator
        {
            get
            {
                uppSpliceIndicators _rVal = uppSpliceIndicators.Undefined;
                if (!Vertical)
                {
                    if (Female)
                    {
                        switch (SpliceType)
                        {
                            case uppSpliceTypes.SpliceWithAngle:
                                _rVal = uppSpliceIndicators.AngleFemale;
                                break;
                            case uppSpliceTypes.SpliceManwayCenter:
                                _rVal = uppSpliceIndicators.ManwayAngleFemale;
                                break;
                            case uppSpliceTypes.SpliceWithJoggle:
                                _rVal = uppSpliceIndicators.JoggleFemale;

                                break;
                            case uppSpliceTypes.SpliceWithTabs:
                                _rVal = uppSpliceIndicators.TabFemale;
                                break;
                        }
                    }
                    else
                    {
                        switch (SpliceType)
                        {
                            case uppSpliceTypes.SpliceWithAngle:
                                _rVal = uppSpliceIndicators.AngleMale;
                                break;
                            case uppSpliceTypes.SpliceManwayCenter:
                                _rVal = uppSpliceIndicators.ManwayAngleMale;
                                break;
                            case uppSpliceTypes.SpliceWithJoggle:
                                _rVal = uppSpliceIndicators.JoggleMale;

                                break;
                            case uppSpliceTypes.SpliceWithTabs:
                                _rVal = uppSpliceIndicators.TabMale;
                                break;
                        }

                    }
                }
                else
                {
                     return IsTop ? _rVal = uppSpliceIndicators.TabFemale : _rVal = uppSpliceIndicators.TabMale;
                    
                }


                    return _rVal;
            }
        }


        public double BaseFlangeLength { get;set; }

        public uppSpliceStyles SpliceStyle { get; set; }

        public string ManwayHandle { get; set; }

        public bool Female => IsTop ? MFTag.EndsWith("F") : MFTag.StartsWith("F");
        public double Depth { get; set; }

        public bool IsTop  => Vertical ? PanelX >=0 ?  Side == uppSides.Right : Side == uppSides.Left : Side == uppSides.Top   ;

        public bool IsManwayCenter => SupportsManway && string.Compare(ManTag , "CENTER", true) ==0;
        public uppSides Side { get; set; }

        private string _ManTag;
        public string ManTag
        { 
            get => _ManTag;
            set
            {
                value ??= "";
                if (value == _ManTag) return;
                if(!string.IsNullOrWhiteSpace(_ManTag) && _ManTag == "CENTER" && value.Trim().ToUpper() != "CENTER")
                {
                    Console.Beep();
                }
                _ManTag = value.Trim().ToUpper();
            } 
        }
    

        public dxxOrthoDirections Direction { get => TabDirection; set => TabDirection = value; }

        public double X { get =>Center.X; set => Center.X = value; }
        public double Y { get => Center.Y; set => Center.Y = value; }

        public double JoggleSpliceLimit { get; set; }
        public double MaxOrd { get =>MaxOrdinate.HasValue ? MaxOrdinate.Value : double.MaxValue ; set => MaxOrdinate = value; }
        public double MinOrd { get => MinOrdinate.HasValue ? MinOrdinate.Value : double.MinValue; set => MinOrdinate = value; }

        public double? MaxOrdinate { get; set; }

        public double? MinOrdinate { get; set; }

        public double RingRadius { get; set; }

        private dxxOrthoDirections? _TabDirection;
        public dxxOrthoDirections TabDirection 
        { get
            {
                return _TabDirection.HasValue ? _TabDirection.Value :dxxOrthoDirections.Up;
                
            }
            set 
            { 
                if (value == dxxOrthoDirections.Right) value = dxxOrthoDirections.Up;
                if (value == dxxOrthoDirections.Left) value = dxxOrthoDirections.Down; 
                if (value != dxxOrthoDirections.Up && value != dxxOrthoDirections.Down) 
                    return;
                if (_TabDirection.HasValue && _TabDirection.Value == value)
                    return;
                //it has changed
                _TabDirection = value; 
            } 
        }

        public string MFTag 
        { get 
            {
                if (SpliceType == uppSpliceTypes.SpliceWithAngle)
                {
                    if (SupportsManway) 
                    { 
                        return  TabDirection == dxxOrthoDirections.Up ? "FC" : "CF";
                      }
                        return "FF";
                }
                if(SpliceType == uppSpliceTypes.SpliceWithTabs) 
                {
                    return (string.Compare(ManTag, "CENTER", true) == 0)?  "FF" : TabDirection == dxxOrthoDirections.Up ? "FM" : "MF";
                    
                }
                return TabDirection == dxxOrthoDirections.Up ? "FM" : "MF";
            } 
        }

        public double DeckThickness { get; set; }

        public double DeckRadius { get; set; }
        public double TrimRadius { get => RingRadius > 0 ? RingRadius - RingClearance : 0; }
        public double ShellRadius { get; set; }

        public double RingClearance => uopUtils.BoundingClearance(2 * ShellRadius);

        public bool Defined { get; set; }

        public string RangeGUID { get; set; }

        public string ProjectHandle { get; set; }

        public bool Selected { get; set; }

        public int Index { get; set; }
        public int PanelIndex { get; set; }

        public double FlangeHt {get; set; }

        public bool SupportsManway { get => !string.IsNullOrWhiteSpace(ManwayHandle) && !string.IsNullOrWhiteSpace(ManTag); }

        public string IndicatorStyleName => IndicatorName(SpliceIndicator, SupportsManway);

        /// <summary>
        ///the X ordinate for vertical splices and the Y ordinte for horizontal splices
        /// </summary>
        public double Ordinate
        {
            get => Vertical ? X : Y;
            set
            {
                value = Math.Round(value, 6);
                if (Vertical) { X = value; } else { Y = value; }
            }
        }

        public double FlowSlotClearance
        {
            get
            {

                switch (SpliceType)
                {
                    case uppSpliceTypes.SpliceWithTabs:
                        return Female ? 0.25 : 1.1975;
                    case uppSpliceTypes.SpliceWithJoggle:
                        return Female ? 0.75 + 0.125 : 0.625 + 0.125;
                    default:
                        return 1.25;

                }

            }
        }

        public string Descriptor
        {
            get
            {
                string _rVal = IsTop ? "TOP" : "BOT";
                _rVal += $" SPLICE {Handle} ";
                _rVal +=  $"{SpliceTypeName()} MFTAG:{MFTag}";
                _rVal += Vertical ? $" X:{X:0.0###}" : $" Y:{ Y:0.0###}";
                if (SupportsManway)
                {
                    _rVal += $" MANTAG:{ManwayHandle} ~ {ManTag}";
                }
                return _rVal;
            }
        }
        /// <summary>
        ///the panel index coupled with the splice index
        ///like 1,2
        /// </summary>
        public string Handle
        { get { if (SpliceIndex == 0) SpliceIndex = 1; return $"{PanelIndex},{SpliceIndex}"; } }

        public uopLine CenterLine
        {
            get
            {
                if (!Vertical)
                {
                    double y1 = Y;
                    double X1 = Math.Sqrt(Math.Pow(DeckRadius, 2) - Math.Pow(y1, 2));
                    if (MaxX() < X1) X1 = MaxX();
                    return new uopLine(new UVECTOR(MinX(), y1), new UVECTOR(X1, y1));


                }
                else
                {
                    double y1 = Math.Sqrt(Math.Pow(DeckRadius, 2) - Math.Pow(X, 2));
                    return new uopLine(new UVECTOR(X, y1), new UVECTOR(X, -y1));

                }
            }
        }

        public uopLine DefinitionLine
        {
            get
            {

                try
                {
                    uopArc aArc = null;
                    uopVector v1 = null;
                    uopVector v2 = null;
                    double x1 = 0;
                    double x2 = 0;
                    double y1 = 0;
                    double y2 = 0;

                    if (Vertical)
                    {
                        x1 = X;
                        x2 = x1;
                        y1 = Math.Sqrt(Math.Pow(DeckRadius, 2) - Math.Pow(X, 2));
                        y2 = -y1;


                    }
                    else
                    {
                        x1 = MinX(true);
                        x2 = MaxX(true);
                        y1 = Y;
                        y2 = y1;

                        aArc = new uopArc(DeckRadius);

                        if (aArc.Radius > 0)
                        {
                            uopLine aL = new uopLine(new uopVector(x1, y1), new uopVector(x2, y2));
                            uopVectors iPts = aL.Intersections(aArc, true, true);
                            v1 = iPts.GetVector(dxxPointFilters.AtMinX);
                            v2 = iPts.GetVector(dxxPointFilters.AtMaxX);
                            if (v1 != null)
                            {
                                if (v1.X > x1 || v2.X < x2)
                                {
                                    if (v1.X > x1) x1 = v1.X;

                                    if (v2.X < x2) x2 = v2.X;

                                }
                            }
                            else
                            {
                                return null;
                            }
                        }

                    }
                    return new uopLine(new uopVector(x1, y1), new uopVector(x2, y2)) { Tag = Handle, Flag = SpliceIndex.ToString() };
                }
                catch (Exception e) { throw e; }

            }
        }

        public  double Length => DefinitionLine.Length;

        public double WasherClearance
        {
            get
            {
                switch (SpliceType)
                {
                    case uppSpliceTypes.SpliceWithTabs:
                        return Female ? 0.25 : 1.1975;

                    case uppSpliceTypes.SpliceWithJoggle:
                        return Female ? 0.75 : 0.625;

                    default:
                        return 1.25;

                }

            }
        }

        public double PanelY => PanelLimits.Y;
        
        public double PanelX => PanelLimits.X;

        public double LeftDowncomerClearance { get; set; }
        public double RightDowncomerClearance { get; set; }

        private WeakReference<uopSectionShape> _SectionShapeRef;
        public uopSectionShape Section
        {
            get
            {
                if (_SectionShapeRef == null) return null;
                if(!_SectionShapeRef.TryGetTarget(out uopSectionShape _rVal))  _SectionShapeRef = null;
                return _rVal;
            }
            set
            {
                if(value == null) { _SectionShapeRef = null;  return; }
                _SectionShapeRef = new WeakReference<uopSectionShape>(value);
                DeckRadius = value.DeckRadius;
                ShellRadius = value.ShellRadius;
                RingRadius = value.RingRadius;

            }
        }


        private uopVector _Center;
        public uopVector Center
        {
            get { _Center ??= uopVector.Zero; return _Center; }
            set { _Center = value == null? uopVector.Zero: value ; }
        }
        
        public bool RequiresFlange
        {
            get
            {
                return (Female && SpliceType == uppSpliceTypes.SpliceWithTabs) || (!Female && SpliceType == uppSpliceTypes.SpliceWithJoggle);
            }
        }
        public bool RequiresTabs
        {
            get
            {
                return (!Female && SpliceType == uppSpliceTypes.SpliceWithTabs) ;
            }
        }
        #endregion Properties

        #region Methods

        public uopLine BaseLine(uopSectionShape aSection, out dxfPlane rPlane)
        {
            rPlane = new dxfPlane();
            uopLine _rVal = null;
            aSection ??= Section;
            if (aSection == null) throw new ArgumentNullException($"{ Descriptor}.Baseline");
            URECTANGLE limits = aSection.BoundsV;
            double offset = 0;
            double swap = IsTop ? -1 : 1;
            double gap = GapValue(SpliceType == uppSpliceTypes.SpliceWithAngle && SupportsManway ); // GapValue(SpliceType == uppSpliceTypes.SpliceWithAngle && SupportsManway );
            bool lefttoright = !IsTop;
            dxfDirection ydir = dxfDirection.WorldY;
            dxfDirection xdir = dxfDirection.WorldX;
            if (Vertical)
            {
                _rVal = new uopLine(X + (gap * swap), IsTop ?limits.Top: limits.Bottom, X + (gap * swap), IsTop ?limits.Bottom: limits.Top);
                _rVal.TrimWithShape(uopShape.Circle(null, aSection.DeckRadius));
                _rVal.Rectify(bDoX: false,bInverse: IsTop);
                xdir = new dxfDirection(_rVal.Direction);
                ydir = new dxfDirection( X >= 0 ? -1 : -1,0, 0);
            }
            else
            {
               
                if(SpliceType == uppSpliceTypes.SpliceWithAngle)
                {
                  
                    offset = -gap * 0.5;
                    
                        //if (Section.IsManway)
                        //{
                        //    if (IsTop)
                        //        offset = +0.125/2;
                        //    else
                        //        offset = +0.125 / 2;
                        //}
                    
                }

                _rVal = new uopLine(lefttoright? limits.Left : limits.Right, Y + ((gap +offset) * swap), lefttoright ? limits.Right : limits.Left, Y + ((gap + offset) * swap));

                if (!aSection.IsRectangular)
                {
                    if (aSection.LapsDivider)
                    {
                        uopLine l2 = aSection.Segments.LineSegments().Find(x => !x.IsVertical(1) && !x.IsHorizontal(1));
                        if (l2 != null)
                        {
                            l2 = new uopLine(l2);
                            l2.Offset(aSection.RingLap + 0.125, aSection.Center);
                            uopVector u1 = l2.IntersectionPt(l2, true, true);
                            if (u1 != null && u1.X > _rVal.sp.X)
                                _rVal.sp.X = u1.X;

                        }

                    }

                    if (aSection.LapsRing)
                        _rVal.TrimWithShape(uopShape.Circle(null, aSection.DeckRadius));

                    
                }

                xdir = new dxfDirection(_rVal.Direction);
                //if (IsTop) _rVal.Invert();


                if (SpliceType == uppSpliceTypes.SpliceManwayCenter)
                {
                    ydir = new dxfDirection(0, lefttoright ? 1 : -1, 0);
                
                }
                else
                {
                    ydir = new dxfDirection(0, TabDirection == dxxOrthoDirections.Right || TabDirection == dxxOrthoDirections.Up ? 1 : -1, 0);
                }
            }
            _rVal.Value = gap * swap;
           
           
           
            rPlane = new dxfPlane(_rVal.sp, aXDir: xdir, aYDir: ydir);

            return _rVal;
        }

        public uopLine SpliceLine(uopSectionShape aShapeTrimmer = null, double aOffset = 0, double aExtension = 0)
        {
            aShapeTrimmer ??= Section;
            if (aShapeTrimmer == null) return null;
            uopLine _rVal = Vertical ? new uopLine(Ordinate + aOffset, aShapeTrimmer.Top + aExtension, Ordinate + aOffset, aShapeTrimmer.Bottom- aExtension) : new uopLine( aShapeTrimmer.Left- aExtension, Ordinate + aOffset, aShapeTrimmer.Right + aExtension, Ordinate + aOffset);
            if(!aShapeTrimmer.IsRectangular)
            {
                _rVal.TrimWithShape(aShapeTrimmer,false);
            }
            return _rVal;
        }
        public uopLine GetFlangeLine()
        {
            return new uopLine(FlangeLine);
        }
      
        public override string ToString() => Descriptor;
        
        public void Copy(uopDeckSplice aSource)
        {
            if (aSource == null) return;
           
            PanelLimits = new URECTANGLE(aSource.PanelLimits);
            _FlangeLine = uopFlangeLine.CloneCopy(aSource._FlangeLine);
            Vertical = aSource.Vertical;
            SpliceIndex = aSource.SpliceIndex;
            SpliceType = aSource.SpliceType;
            BaseFlangeLength = aSource.BaseFlangeLength;
            SpliceStyle = aSource.SpliceStyle;
            ManwayHandle = aSource.ManwayHandle;
            Side = aSource.Side;
            ManTag = aSource.ManTag;
                 _TabDirection = aSource._TabDirection;
            PanelIndex = aSource.PanelIndex;
            MaxOrdinate = aSource.MaxOrdinate;
            MinOrdinate = aSource.MinOrdinate;
            JoggleSpliceLimit = aSource.JoggleSpliceLimit;
            Center = new uopVector(aSource.Center);

            Defined = aSource.Defined;
            RangeGUID = aSource.RangeGUID;
            ProjectHandle = aSource.ProjectHandle;
            Selected = aSource.Selected;
            Index = aSource.Index;
            FlangeHt = aSource.FlangeHt;
            ProjectFamily = aSource.ProjectFamily;
            MDDesignFamily = aSource.MDDesignFamily;
            ShellRadius = aSource.ShellRadius;
            RingRadius = aSource.RingRadius;
            DeckRadius = aSource.DeckRadius;

            LeftDowncomerClearance = aSource.LeftDowncomerClearance;
            RightDowncomerClearance = aSource.RightDowncomerClearance;
            _TabDirection = aSource._TabDirection;
            DeckThickness = aSource.DeckThickness;
            Depth = aSource.Depth;
            TabSpacing = aSource.TabSpacing;
            BoltSpacing = aSource.BoltSpacing;
            SpliceAngleLength = aSource.SpliceAngleLength;
            SpliceBoltCount = aSource.SpliceBoltCount;
            TabCount = aSource.TabCount;
        }

        public void InvertTabDirection() { if (_TabDirection.HasValue) { _TabDirection = _TabDirection.Value == dxxOrthoDirections.Up ? dxxOrthoDirections.Down : dxxOrthoDirections.Up; }  }

        public void Mirror(double? aX, double? aY)
        {
            if (!aX.HasValue && !aY.HasValue) return;
            double dx = aX.HasValue ? X - aX.Value : 0;
            double dy = aY.HasValue ? Y - aY.Value : 0;

            X -= 2 * dx;
            Y -= 2 * dy;
            if( FlangeLine !=null) FlangeLine.Mirror(aX, aY);
              InvertTabDirection();
            

        }

        public double MinX(bool bIncludeClearance = true) => bIncludeClearance ? PanelLimits.Left + LeftDowncomerClearance : PanelLimits.Left;

        public double MaxX(bool bIncludeClearance = false) => (bIncludeClearance && PanelIndex > 1) ? PanelLimits.Right - RightDowncomerClearance : PanelLimits.Right;


    

        public uopDeckSplice Clone() => new uopDeckSplice(this);
        object ICloneable.Clone() => (object)new uopDeckSplice(this);

        public string SpliceTypeName(bool bIncludeMFTag = false)
        {

            string _rVal = string.Empty;
            switch (SpliceType)
            {
                case uppSpliceTypes.SpliceWithAngle:
                    _rVal = SupportsManway ? "Manway Splice Angle" : "Angle";
                    break;

                case uppSpliceTypes.SpliceWithJoggle:
                    _rVal = "Joggle";
                    break;
                case uppSpliceTypes.SpliceWithTabs:
                    _rVal = "Tab";
                    break;
            }
            if (bIncludeMFTag) _rVal += ";" + MFTag;


            return _rVal;
        }
        internal URECTANGLE Limits(bool bTrimToFlange = false) => uopDeckSplice.SpliceLimits(this, bTrimToFlange);


        public uopRectangle Bounds(bool bTrimToFlange = false, double aAdder = 0) => new uopRectangle(uopDeckSplice.SpliceLimits(this, bTrimToFlange, aAdder: aAdder));
        

        public void Move(double Change)
        { 
            if (Vertical) 
            { 
                X += Change;
                FlangeLine.Move(Change);
            } 
            else 
            { 
                Y += Change;
                FlangeLine.Move(0,Change);
            } 
        }

        public bool IsEqual(uopDeckSplice aSplice) => uopDeckSplice.CompareSplices(this, aSplice);
       
        public double FasternerHoleLength
        {
            get
            {
                return SpliceType == uppSpliceTypes.SpliceWithTabs ? mdGlobals.DeckTabSlotLength : 0.5;
            }
        }
        public double SpliceAngleLength { get; set; }
        public int SpliceBoltCount { get; set; }
        public int TabCount { get; set; }
        public double TabSpacing { get; set; }
        public double BoltSpacing { get; set; }

        public int FastenerCount => SpliceType == uppSpliceTypes.SpliceWithTabs ? TabCount : SpliceBoltCount;
        public uppSpliceFastenerTypes FastenerType => IsManwayCenter ? uppSpliceFastenerTypes.Bolts : SpliceType == uppSpliceTypes.SpliceWithTabs ? uppSpliceFastenerTypes.SlotAndTab : uppSpliceFastenerTypes.Bolts;

        public double FlangeInset
        {
            get
            {
                if (SpliceType == uppSpliceTypes.SpliceWithTabs && !IsManwayCenter) return  mdGlobals.DeckTabFlangeInset;
                return 0;
            }
        }
        public double GapValue(bool bForManway = false) { double _rVal = SpliceType.GapValue(); if (bForManway) _rVal *= 2; return _rVal; }

        public void ReleaseManway()
        {
            ManwayHandle = string.Empty;
            ManTag = string.Empty;
        }

        public uopRectangle SelectionBounds(double aWidth = 0.5)
        {

            uopLine cl = CenterLine;

            cl.Rectify(bDoX: !Vertical);
            if (!Vertical)
            {
                return new uopRectangle(cl.sp.X, cl.sp.Y + 0.5 * aWidth, cl.ep.X, cl.ep.Y - 0.5 * aWidth, aTag: Handle, aName: Handle);
            }
            else
            {

                return new uopRectangle(cl.sp.X - 0.5 * aWidth, cl.sp.Y, cl.ep.X + 0.5 * aWidth, cl.ep.Y, aTag: Handle, aName: Handle);
            }

        }
        public uopLine GetCenterLine(mdTrayAssembly assembly)
        {
            if (Vertical)
            {
                double y1 = Math.Sqrt(Math.Pow(DeckRadius, 2) - Math.Pow(X, 2));
                return new uopLine(new UVECTOR(X, y1), new UVECTOR(X, -y1));
            }
            else
            {
                var panel = (mdDeckPanel)assembly.DeckPanels.GetByPanelIndex(PanelIndex).First();
                double halfDeckPanelWidth = panel.Width / 2;
                var extendedFlngLine = new dxeLine(new dxfVector(-DeckRadius - 1, Ordinate), new dxfVector(DeckRadius + 1, Ordinate));

                var panelShape = mdDeckPanel.GetValidPanelShapeForDeckSpliceOrdinate(PanelIndex, Ordinate, assembly);
                if (panelShape != null)
                {
                    var shapePolyline = panelShape.Perimeter();
                    var intersectionPoints = shapePolyline.Intersections(extendedFlngLine);
                    if (intersectionPoints.Count > 1)
                    {
                        var intersectionPoint1 = intersectionPoints[0];
                        var intersectionPoint2 = intersectionPoints[1];

                        return new uopLine(intersectionPoint1, intersectionPoint2);
                    }
                }

                return new uopLine(MinX(), Ordinate, MaxX(), Ordinate);
            }
        }


        public uopRectangle SelectionBounds(mdTrayAssembly assembly, double aWidth = 0.5)
        {
            uopLine cl = GetCenterLine(assembly);
            uopRectangle _rVal = null;
            if (!Vertical)
            {
                _rVal = new uopRectangle(cl.MinX, cl.Y() + 0.5 * aWidth, cl.MaxX, cl.Y() - 0.5 * aWidth, aTag: Handle, aName: Handle);
            }
            else
            {

                _rVal= new uopRectangle(cl.X()- 0.5 * aWidth, cl.MinY, cl.ep.X + 0.5 * aWidth, cl.MaxY, aTag: Handle, aName: Handle);
            }
            return _rVal;
        }

        private dxePolygon _SimplePerimeter;
        private dxePolygon _PerimeterPolygon;
        public dxePolygon PerimeterPolygon(bool bRegen = false, uopSectionShape aSection = null,  bool bSimple = false)
        {
            if (!bSimple)
            {
                if (_PerimeterPolygon != null && !bRegen) return _PerimeterPolygon;
                aSection ??= Section; 
                _PerimeterPolygon = uopDeckSplices.GetSplicePolygon(this, aSection);
                return _PerimeterPolygon;
            }
            else
            {
                if (_SimplePerimeter != null && !bRegen) return _SimplePerimeter;
                aSection ??= Section;
                _SimplePerimeter = uopDeckSplices.GetSplicePolygon(this, aSection,bSimple: true);
                return _SimplePerimeter;
            }
                
        }




        #endregion Methods

        #region Shared Methods


        public static double TabSlotRadius(double aDeckThickness) => aDeckThickness * 25.4 <= 2.7 ? 10 / 25.4 / 2 : 12 / 25.4 / 2;

        /// <summary>
        /// the length that the panel section height will be increased if it uses this splice
        /// </summary>
        /// <param name="aSpliceType"></param>
        /// <param name="aSpliceAtTop"></param>
        /// <param name="aForManway"></param>
        /// <param name="aFemale"></param>
        /// <param name="rOppsAdder"></param>
        /// <returns></returns>
        public static double SpliceLengthAdder(uppSpliceTypes aSpliceType, bool aSpliceAtTop, bool aForManway, bool aFemale, out double rOppsAdder)
        {
            double _rVal = 0;
            rOppsAdder = 0;
            switch (aSpliceType)
            {
                case uppSpliceTypes.SpliceWithAngle:
                    _rVal = aForManway ? -0.1875 : -0.0625;

                    rOppsAdder = -_rVal;
                    break;
                case uppSpliceTypes.SpliceManwayCenter:
                    _rVal = -0.157;
                    rOppsAdder = 0.157;
                    break;
                case uppSpliceTypes.SpliceWithJoggle:
                    if (!aFemale)
                    {
                        _rVal = 0.625;
                        rOppsAdder = -0.75;
                    }
                    else
                    {
                        _rVal = 0.75;
                        rOppsAdder = -0.625;
                    }
                    break;
                case uppSpliceTypes.SpliceWithTabs:
                    if (aFemale)
                    {
                        _rVal = 1.1975;
                        rOppsAdder = -1.0395;
                    }
                    else
                    {
                        _rVal = 1.0395;
                        rOppsAdder = -1.1975;
                    }
                    break;
            }
            if (!aSpliceAtTop)
            {
                _rVal = -_rVal;
                rOppsAdder = -rOppsAdder;
            }
            return _rVal;
        }

        internal static string IndicatorName(uppSpliceIndicators aIndicator, bool bSupportsManway = false)
        {

            switch (aIndicator)
            {
                case uppSpliceIndicators.AngleFemale:
                    return bSupportsManway ? "ManwayAngle_Female" : "Angle_Female";


                case uppSpliceIndicators.AngleMale:
                    return bSupportsManway ? "ManwayAngle_Male" : "Angle_Male";


                case uppSpliceIndicators.JoggleFemale:
                    return "Joggle_Female";

                case uppSpliceIndicators.JoggleMale:
                    return "Joggle_Male";

                case uppSpliceIndicators.ManwayAngleFemale:
                    return "Manway_Female";

                case uppSpliceIndicators.ManwayAngleMale:
                    return "Manway_Male";

                case uppSpliceIndicators.TabFemale:
                    return "Tab_Female";

                case uppSpliceIndicators.TabMale:
                    return "Tab_Male";

                case uppSpliceIndicators.ToRing:
                    return "To_Ring";

                case uppSpliceIndicators.ToDowncomer:
                    return "To_Downcomer";


            }
            return "";
        }

        public uopShapes BlockedAreas(uopSectionShape aSection = null, bool bAddTabs = false, bool bInverted = false, double aTabWidthAdder = 0, double aTabHeightAdder = 0)
        {
            aSection ??= Section;
            uopShapes _rVal = null;
            if (SpliceType != uppSpliceTypes.SpliceWithTabs) bAddTabs = false;
            if (ProjectFamily == uppProjectFamilies.uopFamMD)
            {
                 _rVal = uopDeckSplices.GenMDBlockedAreas(this, aSection, bInverted, aTabWidthAdder, aTabHeightAdder);
                if (bAddTabs && !Female) 
                {
                    uopLine baseline = BaseLine(aSection, out dxfPlane plane);
                    FlangeLine ??= new uopFlangeLine(this, false, aSection);

                    uopFlangeLine flnLn = new uopFlangeLine(FlangeLine);
                    uopFlangeLine flnLnmale = new uopFlangeLine(this, false, aSection);
                    flnLnmale.Invert();

                    List<double> tords = flnLn.FastenerOrdinates;
                    uopVector sp = !Vertical ? new uopVector(flnLn.sp.X, Y) : new uopVector(X, flnLn.sp.Y);
                    uopVector ep = !Vertical ? new uopVector(flnLn.ep.X, Y) : new uopVector(X, flnLn.ep.Y);
                    double f1 = 1;

                    tords.Sort();
                    if (!Vertical)
                    {
                        if (plane.XDirection.X < 0)
                        {
                            tords.Reverse();
                            f1 = -1;
                        }
                        else
                        {
                            f1 = 1;
                        }
                        
                    }
                    else
                    {
                        if (plane.XDirection.Y < 0)
                        {
                            tords.Reverse();
                            f1 = -1;
                            flnLnmale.Invert();
                        }
                    }


                      
          
                    double gap = GapValue();
                    double ht = mdGlobals.DeckTabHeight - gap;
                    double wd = !Vertical ? Math.Abs(sp.Y - flnLnmale.Y()) : Math.Abs(sp.X - flnLnmale.X());
                        plane.SetCoordinates(sp.X, sp.Y);
                    //plane.SetCoordinates(sp.X, sp.Y);
                    //plane.RotateAboutLine(flnLn, 180);
                    colDXFVectors verts = new colDXFVectors() { plane.Vector(-0.5 * mdGlobals.DeckTabWidth, 0), plane.Vector(-0.5 * mdGlobals.DeckTabWidth, ht), plane.Vector(0.5 * mdGlobals.DeckTabWidth, ht), plane.Vector(0.5 * mdGlobals.DeckTabWidth, 0) };
                          
                    uopVectors shapeverts = new uopVectors() { sp };


                    foreach (var ord in tords)
                    {
                        double delta = !Vertical ? ord - sp.X : ord - sp.Y;
                        colDXFVectors tabverts = new colDXFVectors(verts);
                        tabverts.Project(plane.XDirection, delta * f1);
                        foreach (var v in tabverts)
                            shapeverts.Add(v.X, v.Y);

                        //if(Vertical) break;
                    }
                    shapeverts.Add(ep);
                    shapeverts.Add(new uopVector(flnLnmale.sp));
                    shapeverts.Add(new uopVector(flnLnmale.ep));
                    uopShape baseshape = _rVal[0];

                    _rVal.Clear();
                    baseshape.Vertices.Populate(shapeverts);
                    baseshape.Update();
                    baseshape.Area = (tords.Count * ht * mdGlobals.DeckTabWidth) + (flnLn.Length * wd);

                    baseshape.Tag = !Vertical ? $"FLANGE & {tords.Count} TABS" : $"VFLANGE & {tords.Count} TABS";

                    if (Vertical || flnLn.IsTruncated) baseshape.Tag += $" {Handle.Replace(",","_")}";
                    _rVal.Add(baseshape);
                }
            }

             if(_rVal == null)_rVal = new uopShapes("BLOCKED AREAS");
            _rVal.Name = "BLOCKED AREAS";
            return _rVal;
        }

        internal static URECTANGLE SpliceLimits(uopDeckSplice aSplice, bool bTrimToFlange = false, bool bMechanical = false, double aAdder = 0)
        {
            URECTANGLE _rVal = URECTANGLE.Null;
            if (aSplice == null) return _rVal;
            _rVal.Row = aSplice.PanelIndex;
            if (aSplice.Vertical)
            {
                switch (aSplice.SpliceType)
                {
                    case uppSpliceTypes.SpliceWithAngle:
                        _rVal.Left = aSplice.X -  mdGlobals.SpliceAngleWidth / 2;
                        _rVal.Right = aSplice.X + mdGlobals.SpliceAngleWidth / 2;

                        break;
                    case uppSpliceTypes.SpliceWithJoggle:
                        _rVal.Left = aSplice.X - 0.625;
                        _rVal.Right = aSplice.X + 0.75;

                        break;
                    case uppSpliceTypes.SpliceWithTabs:
                        _rVal.Left = aSplice.X - 1.1975;
                        _rVal.Right = aSplice.X + 1.0395;

                        break;
                }

                if (!bTrimToFlange)
                {
                    _rVal.Top = Math.Sqrt(Math.Pow(aSplice.TrimRadius, 2) - Math.Pow(aSplice.X, 2));
                    _rVal.Bottom = -_rVal.Top;
                }
                else
                {
                    _rVal.Top =aSplice.FlangeLine.MaxY;
                    _rVal.Bottom = aSplice.FlangeLine.MinY;
                }
                
            }
            else
            {
                switch (aSplice.SpliceType)
                {
                    case uppSpliceTypes.SpliceWithAngle:
                        if (!bMechanical)
                        {
                            _rVal.Bottom = aSplice.Y - mdGlobals.SpliceAngleWidth / 2;
                            _rVal.Top = aSplice.Y + mdGlobals.SpliceAngleWidth / 2;
                        }
                        else
                        {
                            if (aSplice.ManTag ==  string.Empty)
                            {
                                _rVal.Top = aSplice.Y - 1 / 16;
                                _rVal.Bottom = aSplice.Y + 1 / 16;
                            }
                            else
                            {
                                if (aSplice.ManTag == "TOP")
                                {
                                    _rVal.Top = aSplice.Y - 3 / 16;
                                    _rVal.Bottom = aSplice.Y + 1 / 16;
                                }
                                else
                                {
                                    _rVal.Top = aSplice.Y - 1 / 16;
                                    _rVal.Bottom = aSplice.Y + 3 / 16;
                                }
                            }
                        }
                        break;
                    case uppSpliceTypes.SpliceManwayCenter:
                        if (!bMechanical)
                        {
                            _rVal.Bottom = aSplice.Y - mdGlobals.SpliceAngleWidth / 2;
                            _rVal.Top = aSplice.Y + mdGlobals.SpliceAngleWidth / 2;
                        }
                        else
                        {
                            _rVal.Top = aSplice.Y - 0.314 / 2;
                            _rVal.Bottom = aSplice.Y + 0.314 / 2;
                        }
                        break;
                    case uppSpliceTypes.SpliceWithJoggle:
                        if (aSplice.Direction == dxxOrthoDirections.Down)
                        {
                            _rVal.Top = aSplice.Y + 0.75;
                            _rVal.Bottom = aSplice.Y - 0.625;
                        }
                        else
                        {
                            _rVal.Top = aSplice.Y + 0.625;
                            _rVal.Bottom = aSplice.Y - 0.75;
                        }
                        break;
                    case uppSpliceTypes.SpliceWithTabs:
                        if (aSplice.TabDirection == dxxOrthoDirections.Down)
                        {
                            _rVal.Top = aSplice.Y + 1.0395;
                            _rVal.Bottom = aSplice.Y - 1.1975;
                        }
                        else
                        {
                            _rVal.Top = aSplice.Y + 1.1975;
                            _rVal.Bottom = aSplice.Y - 1.0395;
                        }
                        break;
                }

                if (!bTrimToFlange)
                {
                    _rVal.Left = aSplice.MinX(false);
                    _rVal.Right = aSplice.MaxX(false);
                }
                else
                {
                    _rVal.Left = aSplice.FlangeLine.MinX;
                    _rVal.Right = aSplice.FlangeLine.MaxX;
                }
                if (aSplice.SpliceType == uppSpliceTypes.SpliceWithAngle && aSplice.SupportsManway)
                {
                    if (!bMechanical)
                    {
                        if (aSplice.Direction == dxxOrthoDirections.Down)
                        {
                            _rVal.Top += 0.1875;
                            _rVal.Bottom += 0.1875;
                        }
                        else
                        {
                            _rVal.Bottom -= 0.1875;
                            _rVal.Top -= 0.1875;
                        }
                    }
                }
            }
            _rVal.Tag = aSplice.Handle; //  $"{aSplice.PanelIndex},{aSplice.SpliceIndex}";
            if (!bMechanical) _rVal.Rectify();
            if(aAdder != 0) _rVal.Stretch(aAdder);

            return _rVal;
        }

        public static bool CompareSplices(uopDeckSplice A, uopDeckSplice B)
        {
            if (A == null && B == null) return true;
            if (A == null || B == null) return false;
            if (A == B) return true;

            if (A.PanelIndex != B.PanelIndex) return false;
            if (A.SpliceType != B.SpliceType) return false;
            if (A.Vertical != B.Vertical) return false;
            if (Math.Round(A.Ordinate - B.Ordinate, 3) != 0) return false;


            return true;
        }
        public static uopDeckSplice CloneCopy(uopDeckSplice aSplice) => aSplice == null ? null : new uopDeckSplice(aSplice);
        #endregion Shared Methods
    }
}
