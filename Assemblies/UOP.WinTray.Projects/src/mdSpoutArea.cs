using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.Projects
{
    public class mdSpoutArea : uopShape, ICloneable
    {

        #region Constructors

        public mdSpoutArea() : base()
        {
        
            PanelIndex = 0;
            DowncomerIndex = 0;
            BoxIndex = 0;
            PanelLimitLines = new uopLinePair();
            _Instances = new uopInstances();
           
            DCInfo = null;
            SpoutAreaFraction = 1;
            _TargetArea = null;
            GroupIndex = 0;
       
            ZoneIndex = 1;
            OverrideSpoutArea = null;
            IdealSpoutArea = 0;
            Selected = false;
            _PanelLims = URECTANGLE.Null;


            _LimitedTop = null;
            _LimitedBottom = null;
            _Direction = null;
        }


        public mdSpoutArea(uopVectors aVerts, int aPanelIndex, int aDowcomerIndex, int aBoxIndex,uopLinePair aPanelLimitLines = null, int aOccuranceFactor = 1,  uopRectangle aPanelLimits = null, DowncomerInfo aDCInfo = null, double aSpoutAreaFraction = 1, bool bLimitedTop = false, bool bLimitedBottom = false) : base(aVerts)
        {
       
            PanelIndex = aPanelIndex;
            DowncomerIndex = aDowcomerIndex;
            BoxIndex = aBoxIndex;
            PanelLimitLines = aPanelLimitLines ?? new uopLinePair();
            _Instances = new uopInstances();
         
            DCInfo = aDCInfo;
            SpoutAreaFraction = aSpoutAreaFraction;
            _TargetArea = null;
            GroupIndex = 0;
 
            ZoneIndex = 1;
            OverrideSpoutArea = null;
            IdealSpoutArea = 0;
            Selected = false;
            _PanelLims = new URECTANGLE(aPanelLimits);

            _LimitedTop = null;
            _LimitedBottom = null;
            _Direction = null;
        }

        

        public mdSpoutArea(mdSpoutArea aSpoutArea) : base()
        {
            if(aSpoutArea == null)
            {
           
                PanelIndex = 0;
                DowncomerIndex = 0;
                BoxIndex = 0;
                PanelLimitLines = new uopLinePair();
                _Instances = new uopInstances();
        
                DCInfo = null;
                SpoutAreaFraction = 1;
                _TargetArea = null;
                GroupIndex = 0;
           
                ZoneIndex = 1;
                OverrideSpoutArea = null;
                IdealSpoutArea = 0;
                Selected = false;
                _PanelLims = URECTANGLE.Null;
                _BoxWeirs = null;
            }
            else
            {
                base.Init(null, null);

               if (aSpoutArea != null) Copy(aSpoutArea);
          
            }

        }

        #endregion Constructors

        #region Properties

     

        private URECTANGLE _PanelLims;
        internal URECTANGLE PanelLims { get => _PanelLims; set => _PanelLims = value; }


        public uopRectangle PanelLimits => new uopRectangle(PanelLims);

        public double SpoutAreaFraction { get; set; }

        public bool LimitedBounds => LimitedTop || LimitedBottom;

        private bool? _LimitedTop;
        public bool LimitedTop 
        { get 
            {
                if (_LimitedTop.HasValue) return _LimitedTop.Value;
               //return Segments.LineSegments().FindAll(x => !x.IsVertical(2) && !x.IsHorizontal(2) && x.MaxY > Y).Count > 0  ;

               // if (Segments.LineSegments().FindAll(x => !x.IsVertical(2) && !x.IsHorizontal(2) && x.MinY > Y).Count > 0) return true;
                double panelY = Math.Round(PanelLimits.Y, 3);
                double saY = Math.Round(Y, 3);
                if (saY == panelY) return false;
                return saY < panelY;

            }
            set => _LimitedTop = value; 
        }

        private bool? _LimitedBottom;
        public bool LimitedBottom
        {
            get
            {
                if (_LimitedBottom.HasValue) return _LimitedBottom.Value;
                double panelY = Math.Round(PanelLimits.Y, 3);
               // if ( Segments.LineSegments().FindAll(x => !x.IsVertical(2) && !x.IsHorizontal(2) && x.MinY < Y).Count > 0) return true;
                
                double saY = Math.Round(Y, 3);
                if (saY == panelY) return false;
                return saY > panelY;
               
            }
            set => _LimitedBottom = value;
        }
        private int _PanelIndex;
        public int PanelIndex { get => _PanelIndex; set { _PanelIndex = value; base.Row = value; } }

        private int _DowncomerIndex;
        public int DowncomerIndex { get => _DowncomerIndex; set { _DowncomerIndex = value; base.Col = value; } }

        public int BoxIndex { get; set; }
        
        public int ZoneIndex { get; set; }

        public double PanelY => PanelLims.Y;

        public double PanelWidth => PanelLims.Height;

        public double PanelTop  => PanelLims.Top;
       
        public double PanelBottom => PanelLims.Bottom;

       public bool Selected { get; set; }

        public uopLinePair PanelLimitLines { get;set; }

        private uopLinePair _BoxWeirs;
        public uopLinePair BoxWeirs { get => _BoxWeirs;
            set 
            { 
              _BoxWeirs = value;
                if(value != null)
                {
                    BoxLimitLines = new uopLinePair();
                    uopVectors corners = value.EndPoints();

                    BoxLimitLines.Line1 = new uopLine(corners.GetVector(dxxPointFilters.GetLeftTop, aPrecis: 1), corners.GetVector(dxxPointFilters.GetRightTop, aPrecis: 1)) { Side = uppSides.Top};
                    BoxLimitLines.Line2 = new uopLine(corners.GetVector(dxxPointFilters.GetLeftBottom, aPrecis: 1), corners.GetVector(dxxPointFilters.GetRightBottom, aPrecis: 1)) { Side = uppSides.Bottom};
                    //BoxY = BoxLimitLines.Line1.MidPt.Y;
                    BoxY = value.Y;
                }
                else
                {
                    BoxY = 0;
                    BoxLimitLines = null;
                }
            } 
        
        }

        public uopLinePair BoxLimitLines { get; set; }
        
        public DowncomerInfo DCInfo { get; set; }

        public double BoxY { get; private set; }

        private dxxOrthoDirections? _Direction;
        public dxxOrthoDirections Direction { get =>!_Direction.HasValue ? Y >= BoxY ? dxxOrthoDirections.Up : dxxOrthoDirections.Down : _Direction.Value; set {  _Direction = value; } }
        public int DowncomerCount => DCInfo!= null? DCInfo.DowncomerCount: 0;
        public int PanelCount => DowncomerCount > 0 ? DowncomerCount + 1 : 0;

        public int? OpposingPanelIndex { get { if (PanelCount <= 0) return null; return uopUtils.OpposingIndex(PanelIndex, PanelCount); } }
        
        public int? OpposingDowncomerIndex { get { if (DowncomerCount <= 0) return null; return uopUtils.OpposingIndex(DowncomerIndex, DowncomerCount); } }

        public override string Handle
        {
            get => $"{DowncomerIndex},{PanelIndex},{BoxIndex}";
            set {  }
        }


        public override int OccuranceFactor { get => Instances == null ? 1 : Instances.Count + 1; }

        private double _IdealSpoutArea;
        public double IdealSpoutArea { get => _IdealSpoutArea; set => _IdealSpoutArea = value; }

        private uopInstances _Instances;
        public uopInstances Instances 
        {
            get 
            {
                _Instances.BasePt = Center;
                 return _Instances; 
            }

        }

        private double? _TargetArea;
        public double TargetArea
        {
            get => _TargetArea.HasValue ? _TargetArea.Value : Area;
            set => _TargetArea = value;
            
        }

        /// <summary>
        /// the group that this area is assigned to
        /// </summary>
        public int GroupIndex { get; set; }
        public bool TreatAsIdeal { get => OverrideSpoutArea.HasValue; }

        /// <summary>
        ///flag indicating that the theoretical spout area of the parent spout group should be averaged with other members of its group during distribution calculations
        /// </summary>
        public bool TreatAsGroup { get => GroupIndex > 0; }

        public double? OverrideSpoutArea { get; set; }

        #endregion Properties

        #region Methods

        public override uopRectangle Limits(double aWidthAdder = 0, double aHeightAdder = 0, string aTag = null)
        {
            return new uopRectangle(BoundsV, aWidthAdder, aHeightAdder, aTag == null ? Handle : aTag, Row, Col) {Name= Handle };
        }

        public List<uopMatrixCell> GetMatrixCells(uppSpoutAreaMatrixDataTypes aDataType, bool bZeroInstances = false)
        {
            List<uopMatrixCell> _rVal = new List<uopMatrixCell>();
            double aVal = 0;
            switch (aDataType)
            {
                case uppSpoutAreaMatrixDataTypes.AvailableArea:
                    aVal = Area;
                    break;
                case uppSpoutAreaMatrixDataTypes.GroupIndex:
                    aVal = TreatAsGroup ? (double)GroupIndex : 0;
                    break;
                case uppSpoutAreaMatrixDataTypes.LockValue:
                    aVal = TreatAsIdeal && !TreatAsGroup && OverrideSpoutArea.HasValue ? OverrideSpoutArea.Value : -1;

                    break;

                case uppSpoutAreaMatrixDataTypes.IdealSpoutArea:
                    aVal = IdealSpoutArea;
                    break;
                default:
                    return _rVal;
            }
            _rVal.Add(new uopMatrixCell(PanelIndex, DowncomerIndex, aVal));

            if (Instances == null || bZeroInstances) return _rVal;
            foreach (var inst in Instances)
            {
                _rVal.Add(new uopMatrixCell(inst.Row, inst.Col, aVal) { IsVirtual = true});

            }

            return _rVal;
        }
        public new mdSpoutArea Clone() => new mdSpoutArea(this);

        public bool Copy(mdSpoutArea aSpoutArea) 
        {
            if (aSpoutArea == null) return false;
            base.Copy(aSpoutArea);
            bool _rVal = false;
            if(PanelIndex != aSpoutArea.PanelIndex) _rVal = true;
            if (DowncomerIndex != aSpoutArea.DowncomerIndex) _rVal = true;
            if (BoxIndex != aSpoutArea.BoxIndex) _rVal = true;
            if (PanelY != aSpoutArea.PanelY) _rVal = true;
            if (PanelWidth != aSpoutArea.PanelWidth) _rVal = true;
            if (SpoutAreaFraction != aSpoutArea.SpoutAreaFraction) _rVal = true;
            if (_TargetArea != aSpoutArea._TargetArea) _rVal = true;
            if (GroupIndex != aSpoutArea.GroupIndex) _rVal = true;
            if (LimitedTop != aSpoutArea.LimitedTop) _rVal = true;
            if (LimitedBottom != aSpoutArea.LimitedBottom) _rVal = true;
            if (ZoneIndex != aSpoutArea.ZoneIndex) _rVal = true;
            if (OverrideSpoutArea != aSpoutArea.OverrideSpoutArea) _rVal = true;
            if (IdealSpoutArea != aSpoutArea.IdealSpoutArea) _rVal = true;
    
            PanelIndex = aSpoutArea.PanelIndex;
            DowncomerIndex = aSpoutArea.DowncomerIndex;
            BoxIndex = aSpoutArea.BoxIndex;
            SpoutAreaFraction = aSpoutArea.SpoutAreaFraction;
            _TargetArea = aSpoutArea._TargetArea;
            GroupIndex = aSpoutArea.GroupIndex;
         
            ZoneIndex = aSpoutArea.ZoneIndex;
            OverrideSpoutArea = aSpoutArea.OverrideSpoutArea;
            IdealSpoutArea = aSpoutArea.IdealSpoutArea;
            Selected = aSpoutArea.Selected;

            DCInfo = DowncomerInfo.CloneCopy(aSpoutArea.DCInfo);
            PanelLimitLines = uopLinePair.CloneCopy(aSpoutArea.PanelLimitLines);
            _Instances = new uopInstances(aSpoutArea.Instances);
            _PanelLims = new URECTANGLE(aSpoutArea.PanelLims);
            _BoxWeirs = uopLinePair.CloneCopy(aSpoutArea._BoxWeirs);
            BoxLimitLines = uopLinePair.CloneCopy(aSpoutArea.BoxLimitLines);
            BoxY = aSpoutArea.BoxY;
            _LimitedTop = aSpoutArea._LimitedTop;
            _LimitedBottom = aSpoutArea._LimitedBottom;
            _Direction = aSpoutArea.Direction;
            return _rVal;
        }

        public override string ToString()
        {
            return $"SPOUT AREA DC:{DowncomerIndex} PANEL:{PanelIndex} BOX:{BoxIndex}";
        }

        public void Mirror(double? aX, double? aY)
        {
            
            if (!aX.HasValue && !aY.HasValue) return;


            double dx = aX.HasValue ? X - aX.Value : 0;
            double dy = aY.HasValue ? Y - aY.Value : 0;
            if (dx == 0 && dy == 0) return;

            Vertices.Mirror(aX, aY);
            Update();
            if (PanelLimitLines != null)
            {
                PanelLimitLines.Mirror(aX, aY);
                PanelLimitLines.SetSide(uppSides.Bottom,uppSides.Top, uppSides.Top);
            }


            _PanelLims.Mirror(aX, aY);
       
        }

        public (dxxColors, System.Drawing.Color) Color
        {
            get
            {
                if (TreatAsIdeal)
                {
                    return mdSpoutAreaMatrix.Color_Ideal;
                }
                else if (GroupIndex > 0)
                {
                    return mdSpoutAreaMatrix.GetGroupColor(GroupIndex);
                }
                return (dxxColors.BlackWhite, System.Drawing.Color.Black);
            }
        }

        public override void Update()
        {
            base.Update();

            _LimitedTop = null;
            _LimitedBottom = null;
        }
        #endregion Methods

        #region Shared Methods

        public static mdSpoutArea CloneCopy(mdSpoutArea aSpoutArea)  => aSpoutArea == null ? null : new mdSpoutArea(aSpoutArea);

        #endregion Shared Methods
    }



}
