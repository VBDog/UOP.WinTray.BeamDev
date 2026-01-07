using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Parts;

namespace UOP.WinTray.Projects
{
    public class uopFreeBubblingArea : uopShape, ICloneable
    {

        #region Constructors
        public uopFreeBubblingArea()
        {
            PanelIndex = 0;
            OccuranceFactor = 1;
            Index = 0;
            WeirLines = new uopLinePair();
            BlockedAreas = new uopShapes();
        }


        public uopFreeBubblingArea(int aPanelIndex, uopLinePair aWeirLns, uopShape aFBA, double? aTrayWideWeirLength = null)
        {
            PanelIndex = aPanelIndex;
            _Area = null;
            BlockedAreas = new uopShapes();
            base.Init(null, aFBA);
            WeirLines = aWeirLns == null ? new uopLinePair() : aWeirLns;
            Index = Row;
            if (aTrayWideWeirLength.HasValue)
            {
                double wl = TotalWeirLength;
                if (aTrayWideWeirLength.Value != 0 && wl != 0) WeirFraction = wl / aTrayWideWeirLength.Value;


            }
        }
        public uopFreeBubblingArea(uopFreeBubblingArea aFBA)
        {
            _Area = null;
            PanelIndex = 0;
            Index = 0;
            OccuranceFactor = 1;
            WeirLines = new uopLinePair();
            BlockedAreas = new uopShapes();
            WeirFraction = 0;
            TrayFraction = 0;
            if (aFBA == null) return;
            base.Init(null, aFBA);  //to get the shape properties
            _Area = aFBA.Area;
            Index = aFBA.Index;
            PanelIndex = aFBA.PanelIndex;
            OccuranceFactor = aFBA.OccuranceFactor;
     
            BlockedAreas = new uopShapes(aFBA.BlockedAreas);
            WeirFraction = aFBA.WeirFraction;
            TrayFraction = aFBA.TrayFraction;
        
            base.Handle = Handle;
        }


        #endregion Constructors

        #region Properties

        private double? _Area;

        public new double Area { get { return _Area.HasValue ? _Area.Value : base.Area; }
            set { _Area = value; } }

        public double BaseArea => base.Area;

        public double TrayFraction { get; internal set; }

        public double BlockedArea() => BlockedAreas.TotalArea();
        public double ActiveArea() => Area - BlockedArea();
        private uopShapes _BlockedAreas;
        public uopShapes BlockedAreas { get { _BlockedAreas ??= new uopShapes(); return _BlockedAreas; } set => _BlockedAreas = value; }

        public int OccuranceFactor { get; set; } = 1;


        public uopLinePair WeirLines { get => base.LinePair; set =>  base.LinePair = value == null ? new uopLinePair() : value; }
        public int PanelIndex { get => base.PartIndex; set => base.PartIndex = value; }

        /// <summary>
        /// the fraction of the total tray weir length that this FBA owns
        /// </summary>
        public double WeirFraction { get; set; }

        /// <summary>
        /// the sum of the left and right weir lengths
        /// </summary>
        public double TotalWeirLength => WeirLength_Left + WeirLength_Right;

        public double WeirLength_Left => WeirLines.SideLength(uppSides.Left);
        public double WeirLength_Right => WeirLines.SideLength(uppSides.Right);

        public new string Handle => $"{PanelIndex},{Index}";

        /// <summary>
        /// returns the ratio of Area and Weir Length (FBA/WL)
        /// </summary>
        public double WeirLengthRatio => Area / TotalWeirLength;


        private WeakReference<mdTrayAssembly> _AssyRef;
        public mdTrayAssembly TrayAssembly
        {
            get
            {
                if (_AssyRef == null) return null;
                if (!_AssyRef.TryGetTarget(out mdTrayAssembly _rVal)) _AssyRef = null;
                return _rVal;
            }
            set
            {
                _AssyRef = value != null ? new WeakReference<mdTrayAssembly>(value) : null;
                if (value == null) return;

            }
        }

        #endregion Properties

        #region Methods 
        public double BlockedArea(string aTag = null)
        {
            if (string.IsNullOrWhiteSpace(aTag)) return BlockedAreas.Area;
            double _rVal = 0;
            _BlockedAreas ??= new uopShapes();
            foreach (var item in _BlockedAreas)
            {
                if (string.Compare(item.Tag, aTag, true) == 0) _rVal += item.Area;
            }
            return _rVal;
        }

        public new uopFreeBubblingArea Clone() => new uopFreeBubblingArea(this);

        object ICloneable.Clone() => (object)Clone();

        /// <summary>
        /// returns the requested weir length
        /// </summary>
        /// <remarks> if a side is passed  nly the requested side length is returned</remarks>
        /// <param name="aSide"></param>
        /// <returns></returns>
        public double WeirLength(uppSides? aSide = null) => WeirLines.SideLength(aSide); 

        public override string ToString() => $"FBA ({Handle})";

        public dxfBlock Block(string aBlockName, string aLayerName = "FREE BUBBLING AREA", dxxColors aBoundColor = dxxColors.Blue, dxxColors aLeftWeirColor = dxxColors.LightBlue, dxxColors aRightWeirColor = dxxColors.Green, bool bIncludeBlockedAreas = false)
        {
            if (string.IsNullOrWhiteSpace(aBlockName)) aBlockName = $"FBA_{PanelIndex}_{Index}";
            uopVector u1 = Center;
            colDXFVectors verts = new colDXFVectors(Vertices);
            verts.Move(-u1.X, -u1.Y);

            dxfBlock _rVal = new dxfBlock(aBlockName);
            _rVal.Entities.Add(new dxePolyline(verts, true, aDisplaySettings: dxfDisplaySettings.Null(aLayer: aLayerName, aColor: aBoundColor)));


            //draw.aPolyline(fba.Bounds.Vertices, true, dxfDisplaySettings.Null(aLayer: lname, aColor: dxxColors.Blue));
            uopLine l1 = WeirLines.GetSide(uppSides.Right);
            uopLine l2 = WeirLines.GetSide(uppSides.Left);
            if (l1 != null)
            {
                dxeLine dxl1 = new dxeLine(l1.Moved(-u1.X, -u1.Y), aDisplaySettings: new dxfDisplaySettings(aLayerName, l1.Suppressed ? dxxColors.Red : aRightWeirColor, "Continuous"));
                //dxl1.Move(0.5 * dcdata.ShelfWidth);
                _rVal.Entities.Add(dxl1);
            }
            if (l2 != null)
            {
                dxeLine dxl2 = new dxeLine(l2.Moved(-u1.X, -u1.Y), aDisplaySettings: new dxfDisplaySettings(aLayerName, l2.Suppressed ? dxxColors.Red : aLeftWeirColor, "Continuous"));
                //dxl2.Move(-0.5 * dcdata.ShelfWidth);
                _rVal.Entities.Add(dxl2);
            }

            if (bIncludeBlockedAreas)
            {
                _BlockedAreas ??= new uopShapes();
                foreach (var area in _BlockedAreas)
                {
                    verts = new colDXFVectors(area.Vertices);
                    verts.Move(-u1.X, -u1.Y);

                    dxePolyline bpl = new dxePolyline(verts, true, aDisplaySettings: dxfDisplaySettings.Null(aLayer: aLayerName, aColor: dxxColors.Red));
                }
            }

            return _rVal;
        }

        /// <summary>
        /// ^returns the ratio of the panels actual spout area to the total actual spout area for the tray asembly
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public double VLError(mdTrayAssembly aAssy)
        {
            double rFBA2WLTo = 0;
            return VLError(aAssy, ref rFBA2WLTo);
        }
        /// <summary>
        /// ^returns the ratio of the panels actual spout area to the total actual spout area for the tray asembly
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="rFBA2WLTot"></param>
        /// <returns></returns>
        public double VLError(mdTrayAssembly aAssy, ref double rFBA2WLTot)
        {

            double ratio = WeirLengthRatio;
            if (rFBA2WLTot <= 0)
            {
                aAssy ??= TrayAssembly;
                rFBA2WLTot = aAssy == null ? 0 : aAssy.Downcomers.FBA2WLRatio;
            }
            return (rFBA2WLTot != 0 && ratio != 0) ? (( ratio / rFBA2WLTot) - 1) * 100 : 0;

        }


        #endregion Methods

        #region Shared Methods


        #endregion Shared Methods
    }

    public class uopFreeBubblingPanel : List<uopFreeBubblingArea>, IEnumerable<uopFreeBubblingArea>, ICloneable    
    {
        #region Constructors

        public uopFreeBubblingPanel(int aPanelIndex)
        {
            Left = 0;
            Right = 0;
            X = 0;
            OccuranceFactor = 1;

            PanelIndex = aPanelIndex;
            Bounds = new uopShape() { PartIndex = PanelIndex };

        }

        public uopFreeBubblingPanel(uopFreeBubblingPanel aPanel)
        {
            Left = 0;
            Right = 0;
            X = 0;
            OccuranceFactor = 1;
            if (aPanel == null) return;
            Left = aPanel.Left;
            Right = aPanel.Right;
            X = aPanel.X;
            OccuranceFactor = aPanel.OccuranceFactor;
            PanelIndex = aPanel.PanelIndex;
            Bounds = new uopShape(aPanel.Bounds);
            foreach (var item in aPanel)
            {
                Add(new uopFreeBubblingArea(item));
            }
        }


        #endregion Constructors

        #region Properties

  public double TrayFraction
        {
            get
            {
                double _rVal = 0;
                foreach (var item in this) { _rVal += item.TrayFraction; }
                return _rVal;
            }
        }

        private int _PanelIndex;

        public int PanelIndex
        {
            get => _PanelIndex;

            set
            {
                _PanelIndex = value; foreach (var item in this)
                {
                    item.PanelIndex = value;
                }
            }
        }
        private int _OccuranceFactor;

        public int OccuranceFactor
        {
            get => _OccuranceFactor;

            set
            {
                _OccuranceFactor = value; foreach (var item in this)
                {
                    item.OccuranceFactor = value;
                }
            }
        }

       

        public double X { get; set; }

        public double Left { get; set; }
        public double Right { get; set; }


        public uopShape Bounds { get; internal set; }

        private double _TotalWeirLength;
        public double TotalWeirLength
        {
            get
            {
                if (Count == 0) { _TotalWeirLength = 0; return 0; }
                if(_TotalWeirLength <= 0)
                {
                    foreach (var item in this) { _TotalWeirLength += item.TotalWeirLength; }
                }
           
                return _TotalWeirLength;
            }
        }
        public double TotalBlockedArea
        {
            get
            {
                double _rVal = 0;
                foreach (var item in this) { _rVal += item.BlockedArea(); }
                return _rVal;
            }
        }

        public double TotalActiveArea
        {
            get
            {
                double _rVal = 0;
                foreach (var item in this) { _rVal += item.ActiveArea(); }
                return _rVal;
            }
        }

        public double TotalArea
        {
            get
            {
                double _rVal = 0;
                foreach (var item in this) { _rVal += item.Area; }
                return _rVal;
            }
        }

        public double WeirLength_Right
        {
            get
            {
                double _rVal = 0;
                foreach (var item in this)
                {
                    _rVal += item.WeirLength_Right;
                }
                return _rVal;
            }
        }

        public double WeirLength_Left
        {
            get
            {
                double _rVal = 0;
                foreach (var item in this)
                {
                    _rVal += item.WeirLength_Left;
                }
                return _rVal;
            }
        }
        #endregion Properties

        #region Methods
        public new void Add(uopFreeBubblingArea aFBA)
        {
            if (aFBA == null) return;
            aFBA.Index = Count + 1;
            aFBA.PanelIndex = PanelIndex;
            aFBA.Name = $"FBA_{PanelIndex}_{aFBA.Index}";

            base.Add(aFBA);

        }

        public double WeirLength(uppSides? aSide = null)
        {

            double _rVal = 0;
            foreach (var item in this)
            {
                _rVal += item.WeirLength(aSide);
            }
            return _rVal;

        }

        public uopFreeBubblingPanel Clone() => new uopFreeBubblingPanel(this);

        object ICloneable.Clone() => (object)Clone();
        #endregion Methods
    }

    public class uopFreeBubblingPanels : List<uopFreeBubblingPanel>, IEnumerable<uopFreeBubblingPanel>
    {

        #region Constructors
        public uopFreeBubblingPanels() { Clear(); }

        internal uopFreeBubblingPanels(List<uopFreeBubblingPanel> aPanels)
        {
            Clear();
            for (int i = 1; i <= aPanels.Count; i++)
            {
                Add(new uopFreeBubblingPanel(aPanels[i - 1]));
            }
        }

        #endregion Constructors

        #region Methods

        public double TotalWeirLength()
        {
            double _rVal = 0;
            foreach (var item in this) { _rVal += item.TotalWeirLength; }
            return _rVal;
        }
        public double TotalBlockedArea()
        {
            double _rVal = 0;
            foreach (var item in this) { _rVal += item.TotalBlockedArea; }
            return _rVal;
        }
        public double TotalActiveArea()
        {
            double _rVal = 0;
            foreach (var item in this) { _rVal += item.TotalActiveArea; }
            return _rVal;
        }

        public double TotalArea()
        {
            double _rVal = 0;
            foreach (var item in this) { _rVal += item.TotalArea; }
            return _rVal;
        }
        #endregion Methods
    }
}
