using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Structures;

namespace UOP.WinTray.Projects
{
    public class uopFreeBubblingArea : uopPanelSectionShape, ICloneable
    {

        #region Constructors
        public uopFreeBubblingArea() => Init();


        public uopFreeBubblingArea(int aPanelIndex, uopLinePair aWeirLns, uopShape aFBA, double? aTrayWideWeirLength = null)
        {
            Init(aFBA, aWeirLns);
            PanelIndex = aPanelIndex;
            if (aTrayWideWeirLength.HasValue) TotalTrayWeirLength = aTrayWideWeirLength;
            
        }
        public uopFreeBubblingArea(uopFreeBubblingArea aFBA) =>Init(aFBA);
        

        private void Init(uopShape aShape = null, uopLinePair aWeirLns = null)
        {
            base.Init(null, aShape);  //to get the shape properties

            _Area = null;
            
            TotalTrayFreeBubblingArea = null;
            TotalTrayWeirLength = null;
            ProjectType = uppProjectTypes.MDSpout;
            Index = Row;

            if (aShape != null) Copy(aShape);
            
            if(aWeirLns != null) WeirLines = new uopLinePair(aWeirLns);

        }

        #endregion Constructors

        #region Properties


        private new double? _Area;

        /// <summary>
        /// returns the area of a single instance of the shape
        /// </summary>
        public new double Area { get { return _Area.HasValue ? _Area.Value : base.Area; }
            set 
            { 
                _Area = value; 
            } 
        
        }

        public uopShapes BlockedAreas(uopTrayAssembly aAssy = null, bool bRegen = false)
        {
            if (SubShapes.ShapeType == uppSubShapeTypes.BlockedAreas && !bRegen) return SubShapes;
            aAssy ??= TrayAssembly;
            if (aAssy == null) return new uopShapes(uppSubShapeTypes.BlockedAreas);
            if (aAssy.ProjectFamily == uppProjectFamilies.uopFamMD) 
            {
                if (aAssy.ProjectType == uppProjectTypes.MDSpout)
                {
                    SubShapes = new uopShapes(uppSubShapeTypes.BlockedAreas);
                    return SubShapes;
                }
                 
                if (SubShapes.ShapeType != uppSubShapeTypes.BlockedAreas || bRegen)
                {
            
                    mdTrayAssembly assy = (mdTrayAssembly)aAssy;
                  SubShapes = assy.DeckSections.BaseShapes(aPanelIndex: PanelIndex, aPanelSectionIndex: SectionIndex).GetSubShapes(uppSubShapeTypes.BlockedAreas,assy,PanelIndex,SectionIndex);
                    return SubShapes;
                }
           
            }




            return new uopShapes(uppSubShapeTypes.BlockedAreas);
        }


        public double TotalBlockedArea => OccuranceFactor * BlockedArea;
        public double BlockedArea => BlockedAreas().TotalArea(false);
        /// <summary>
        /// returns the area multiplied by the occurance factor
        /// </summary>
        public double TotalArea => Area * OccuranceFactor;

        /// <summary>
        /// returns the weir length multiplied by the occurance factor
        /// </summary>
        public double TotalWeirLength =>  WeirLength * OccuranceFactor;

        public double BaseArea => base.Area;

        /// <summary>
        /// the fraction of the total tray free bubbling area that this panel owns
        /// </summary>
        public double TrayFraction { get => TotalTrayFreeBubblingArea.HasValue && TotalTrayFreeBubblingArea.Value != 0 ? TotalArea / TotalTrayFreeBubblingArea.Value : 0; }

        /// <summary>
        /// the fraction of the total tray weir length that this FBA owns
        /// </summary>
        public double WeirFraction { get => TotalTrayWeirLength.HasValue && TotalTrayWeirLength.Value != 0 ? TotalWeirLength / TotalTrayWeirLength.Value : 0; }



        public double? TotalTrayFreeBubblingArea  { get; internal set; }

        public double? TotalTrayWeirLength { get; internal set; }



        public uopLinePair WeirLines { get => base.Weirs; set =>  base.LinePair = value ?? new uopLinePair(); }
  

        /// <summary>
        /// the sum of the left and right weir lengths
        /// </summary>
        public double WeirLength => WeirLines.TotalLength;

        public double WeirLength_Left => WeirLines.SideLength(uppSides.Left);
        public double WeirLength_Right => WeirLines.SideLength(uppSides.Right);

        public new string Handle => $"{PanelIndex},{Index}";

        /// <summary>
        /// returns the ratio of Area and Weir Length (FBA/WL)
        /// </summary>
        public double WeirLengthRatio { get { double tot = WeirLines.TotalLength; return tot > 0 ? Area / tot : 0; } }

        public uppProjectTypes ProjectType { get; set; }



        #endregion Properties

        #region Methods 

        public new void Copy(uopShape aShape)
        {
            if (aShape == null) return;
            base.Copy(aShape);
            if (aShape.GetType() == typeof(uopFreeBubblingArea))
            {
                uopFreeBubblingArea fba = (uopFreeBubblingArea)aShape;
                _Area = fba.Area;
                Index = fba.Index;
                TotalTrayWeirLength = fba.TotalTrayWeirLength;
                TotalTrayFreeBubblingArea = fba.TotalTrayFreeBubblingArea;
              
            }
        }



        public new uopFreeBubblingArea Clone() => new uopFreeBubblingArea(this);

        object ICloneable.Clone() => new uopFreeBubblingArea(this);

        /// <summary>
        /// returns the requested weir length
        /// </summary>
        /// <remarks> if a side is passed  nly the requested side length is returned</remarks>
        /// <param name="aSide"></param>
        /// <returns></returns>
        public double GetWeirLength(uppSides? aSide = null) => WeirLines.SideLength(aSide); 

        public override string ToString() => $"FBA ({Handle}) x {OccuranceFactor} ~ AREA[{Area:0.00}] ~ WL[{WeirLength:0.00}]";
        public dxfBlock Block(uopTrayAssembly aAssy, string aBlockName,  out uopShapes rBlockedAreas, string aLayerName = "FREE BUBBLING AREAS", dxxColors aBoundColor = dxxColors.Blue, dxxColors aLeftWeirColor = dxxColors.LightBlue, dxxColors aRightWeirColor = dxxColors.Green, bool bSetInstances = false, bool bIncludeBlockedAreas = true, bool bRegenBlockedAreas = false)
        {
            rBlockedAreas = new uopShapes("BLOCKED AREAS");
            if (string.IsNullOrWhiteSpace(aBlockName)) aBlockName = $"FBA_{Handle.Replace(",", "_")}";
            uopVector u1 = Center;
            colDXFVectors verts = new colDXFVectors(Vertices);
            verts.Move(-u1.X, -u1.Y);
            aAssy ??= TrayAssembly;
            if (TrayAssembly == null && aAssy != null) TrayAssembly = aAssy;

            dxfBlock _rVal = new dxfBlock(aBlockName);
            _rVal.Entities.Add(new dxePolyline(verts, true, aDisplaySettings: dxfDisplaySettings.Null(aLayer: aLayerName, aColor: aBoundColor)));


            //draw.aPolyline(fba.Bounds.Vertices, true, dxfDisplaySettings.Null(aLayer: lname, aColor: dxxColors.Blue));
            uopLine l1 = WeirLines.GetSide(uppSides.Right);
            uopLine l2 = WeirLines.GetSide(uppSides.Left);
            if (l1 != null)
            {
                dxeLine dxl1 = new dxeLine(l1.Moved(-u1.X, -u1.Y), aDisplaySettings: new dxfDisplaySettings(aLayerName, l1.Suppressed ? dxxColors.Red : aRightWeirColor, dxfLinetypes.Continuous));
                //dxl1.Move(0.5 * dcdata.ShelfWidth);
                _rVal.Entities.Add(dxl1);
            }
            if (l2 != null)
            {
                dxeLine dxl2 = new dxeLine(l2.Moved(-u1.X, -u1.Y), aDisplaySettings: new dxfDisplaySettings(aLayerName, l2.Suppressed ? dxxColors.Red : aLeftWeirColor, dxfLinetypes.Continuous));
                //dxl2.Move(-0.5 * dcdata.ShelfWidth);
                _rVal.Entities.Add(dxl2);
            }

            if (bIncludeBlockedAreas)
            {
                uopShapes blockedareas = BlockedAreas(aAssy, bRegenBlockedAreas);
                if (blockedareas != null)
                {
                    foreach (var area in blockedareas)
                    {
                        rBlockedAreas.Add(area);
                        if (area.IsCircle)
                        {
                            dxeArc barea = new dxeArc(area.Center, area.Radius, aDisplaySettings: dxfDisplaySettings.Null("BLOCKED AREAS"));
                            barea.Move(-u1.X, -u1.Y);
                            _rVal.Entities.Add(barea);
                        }
                        else
                        {
                            dxePolyline barea = new dxePolyline(area, dxfDisplaySettings.Null("BLOCKED AREAS"));
                            barea.Move(-u1.X, -u1.Y);
                            _rVal.Entities.Add(barea);
                        }

                    }
                }
            }
            _rVal.Instances = Instances.ToDXFInstances();
            if (!bSetInstances) _rVal.Instances.Clear();
            return _rVal;
        }

        public dxfBlock Block(uopTrayAssembly aAssy,  string aBlockName, string aLayerName = "FREE BUBBLING AREAS", dxxColors aBoundColor = dxxColors.Blue, dxxColors aLeftWeirColor = dxxColors.LightBlue, dxxColors aRightWeirColor = dxxColors.Green, bool bSetInstances = false, bool bIncludeBlockedAreas = true, bool bRegenBlockedAreas = false)
         => Block(aAssy,aBlockName,out _,aLayerName,aBoundColor,aLeftWeirColor,aRightWeirColor,bSetInstances, bIncludeBlockedAreas, bRegenBlockedAreas);
        

        /// <summary>
        /// ^returns the ratio of the panels actual spout area to the total actual spout area for the tray asembly
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public double VLError(mdTrayAssembly aAssy) { double rFBA2WLTo = 0; return VLError(aAssy, ref rFBA2WLTo);    }
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
                aAssy ??= MDTrayAssembly;
                rFBA2WLTot = aAssy == null ? 0 : aAssy.SpacingData.FBA2WLRatio;
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
            WeirFraction = 0;
            PanelIndex = aPanelIndex;
            Bounds = new uopShape() { PartIndex = PanelIndex };

        }

        public uopFreeBubblingPanel(uopFreeBubblingPanel aPanel)
        {
            Left = 0;
            Right = 0;
            X = 0;
            OccuranceFactor = 1;
            WeirFraction = 0;
            if (aPanel == null) return;
           
            Left = aPanel.Left;
            Right = aPanel.Right;
            X = aPanel.X;
            OccuranceFactor = aPanel.OccuranceFactor;
            PanelIndex = aPanel.PanelIndex;
            WeirFraction = aPanel.WeirFraction;
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
        
        public int OccuranceFactor { get; set; }
       
       

        public double X { get; set; }

        public double Left { get; set; }
        public double Right { get; set; }


        public uopShape Bounds { get; internal set; }

        public double WeirFraction { get; internal set; }

        private double _WeirLength;
        public double WeirLength
        {
            get
            {
                if (Count == 0) { _WeirLength = 0; return 0; }
                if (_WeirLength <= 0)
                {
                    foreach (var item in this) { _WeirLength += item.WeirLength; }
                }

                return _WeirLength;
            }
        }

     
   



        public double Area
        {
            get
            {
                double _rVal = 0;
                foreach (var item in this)  _rVal += item.Area; 
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

        public uopShapes BlockedAreas(uopTrayAssembly aAssy = null, bool bRegen = false)
        {
            uopShapes _rVal = new uopShapes("BLOCKED AREAS");
            foreach (var mem in this)
                _rVal.AddRange(mem.BlockedAreas(aAssy, bRegen));

            return _rVal;
        }

        /// <summary>
        /// returns the summation of the total area of all the members in the panel
        /// </summary>
        public double TotalArea()
        {
            double _rVal = 0;
            foreach (var mem in this) _rVal += mem.TotalArea;
            return _rVal;
        }

        /// <summary>
        /// returns the summation of the total weir lengths of all the members in the panel
        /// </summary>
        public double TotalWeirLength()
        {
            double _rVal = 0;
            foreach (var mem in this) _rVal += mem.TotalWeirLength;
            return _rVal;
        }

        public new void Add(uopFreeBubblingArea aFBA)
        {
            if (aFBA == null) return;
            aFBA.Index = Count + 1;
            aFBA.PanelIndex = PanelIndex;
            aFBA.Name = $"FBA_{PanelIndex}_{aFBA.Index}";

            base.Add(aFBA);

        }

        public double GetWeirLength(uppSides? aSide = null)
        {

            double _rVal = 0;
            foreach (var item in this) _rVal += item.GetWeirLength(aSide);
            
            return _rVal;

        }

        public uopFreeBubblingPanel Clone() => new uopFreeBubblingPanel(this);

        object ICloneable.Clone() => new uopFreeBubblingPanel(this);

        

        #endregion Methods
    }

    public class uopFreeBubblingPanels :  List<uopFreeBubblingPanel>, IEnumerable<uopFreeBubblingPanel>
    {

        #region Constructors
        public uopFreeBubblingPanels() { Clear(); }

        internal uopFreeBubblingPanels(IEnumerable<uopFreeBubblingPanel> aPanels)
        {
            Clear();
            if (aPanels == null) return;
            foreach (var aPanel in aPanels)
                Add(new uopFreeBubblingPanel(aPanel));
            
        }

        #endregion Constructors

        #region Methods
        /// <summary>
        /// returns the summation of the total weir length  of all the panels in the collection
        /// </summary>
        public double TotalWeirLength()
        {
            double _rVal = 0;
           foreach (var item in this) _rVal += item.TotalWeirLength();
            return _rVal;
        }


        public uopShapes BlockedAreas(uopTrayAssembly aAssy = null, bool bRegen = false)
        {
         
            uopShapes _rVal = new uopShapes("BLOCKED AREAS");
            foreach(var mem  in this)
                _rVal.AddRange(mem.BlockedAreas(aAssy, bRegen));
            
            return _rVal;

        } 
        /// <summary>
        /// returns the summation of the total area of all the panels in the collection
        /// </summary>
        public double TotalArea()
        {
            double _rVal = 0;
            foreach (var item in this)  _rVal +=  item.TotalArea(); 
            return _rVal;
        }
        #endregion Methods
    }
}
