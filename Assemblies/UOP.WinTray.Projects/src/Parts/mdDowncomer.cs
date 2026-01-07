using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Materials;

using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;

namespace UOP.WinTray.Projects.Parts
{
    public class mdDowncomer : uopPart
    {
        public override uppPartTypes BasePartType => uppPartTypes.Downcomer;


        //@raised when the downcomer data is changed

        public delegate void MaterialChange(uopSheetMetal NewMaterial);
        public event MaterialChange EventMaterialChange;
        public delegate void MDDowncomerPropertyChange(uopProperty aProperty);
        public event MDDowncomerPropertyChange PropertyChangeEvent;

        #region Constructors


        public mdDowncomer() : base(uppPartTypes.Downcomer, uppProjectFamilies.uopFamMD, "", "", true) { InitializeProperties(); PartEvent += PartEventHandler; }


        public mdDowncomer(mdDowncomer aPartToCopy, mdTrayAssembly aAssy) : base(uppPartTypes.Downcomer, uppProjectFamilies.uopFamMD, "", "", true)
        {
            ProjectType = uppProjectTypes.MDSpout;
            InitializeProperties();

            if (aPartToCopy != null)
            {
                ProjectType = aPartToCopy.ProjectType;
                base.Copy(aPartToCopy);
                RangeGUID = aPartToCopy.RangeGUID;
                SubPart(aAssy);
                DeckThickness = aAssy != null ? aAssy.Deck.Thickness : aPartToCopy.DeckThickness;
                ShelfWidth = aAssy != null ? aAssy.Downcomer().ShelfWidth : aPartToCopy.ShelfWidth;
                IsGlobal = aPartToCopy.IsGlobal;
                BaffleHeight = aAssy != null ?  aAssy.BaffleHeight :  aPartToCopy.BaffleHeight;
            }

            PartEvent += PartEventHandler;
        }
        private void InitializeProperties()
        {
            ShelfWidth = 1;
            IsGlobal = false;
            SheetMetal = uopGlobals.goSheetMetalOptions().GetByFamilyAndGauge(uppMetalFamilies.CarbonSteel, uppSheetGages.Gage12);


            AddProperty("X", 0, aUnitType: uppUnitTypes.SmallLength);
            AddProperty("Y", 0, aUnitType: uppUnitTypes.SmallLength);
            AddProperty("Count", 0, aUnitType: uppUnitTypes.Undefined);
            AddProperty("Width", 0, aUnitType: uppUnitTypes.SmallLength, bIsShared: true);
            AddProperty("InsideHeight", 0, aUnitType: uppUnitTypes.SmallLength, bIsShared: true);
            AddProperty("How", 0, aDisplayName: "Weir Height (How)", aUnitType: uppUnitTypes.SmallLength, bIsShared: true);
            AddProperty("SpoutDiameter", .75, aUnitType: uppUnitTypes.SmallLength);
            AddProperty("StartupDiameter", .375, aUnitType: uppUnitTypes.SmallLength, bIsShared: true);
            AddProperty("StartupLength", 0.5, aUnitType: uppUnitTypes.SmallLength, bIsShared: true);
            AddProperty("HasTriangularEndPlate", false);
            AddProperty("EndplateInset", -1.0, aUnitType: uppUnitTypes.SmallLength);
            AddProperty("Asp", 0!, aUnitType: uppUnitTypes.SmallArea, bIsShared: true);
            AddProperty("OverrideClipClearance", 0, aUnitType: uppUnitTypes.SmallLength);
            AddProperty("Material", SheetMetal.Descriptor, bIsShared: true);
            AddProperty("StiffenerSites", "");
            AddProperty("StartupSitesL", "");
            AddProperty("StartupSitesR", "");
            AddProperty("FoldOverHeight", 0, aUnitType: uppUnitTypes.SmallLength);
            AddProperty("GussetedEndPlates", false);
            AddProperty("SupplementalDeflectorHeight", 0, aUnitType: uppUnitTypes.SmallLength);
            AddProperty("BoltOnEndPlates", false);

            AddProperty("Spacing", 0, aDisplayName: "DC Spacing", aUnitType: uppUnitTypes.SmallLength, bIsShared: true);
            AddProperty("TotalWeir", 0, aUnitType: uppUnitTypes.SmallLength, bIsShared: true);
            AddProperty("TotalArea", 0, aUnitType: uppUnitTypes.SmallArea, bIsShared: true);
            AddProperty("SpoutArea", 0, aUnitType: uppUnitTypes.SmallArea, bIsShared: true);
            AddProperty("Thickness", 0, aUnitType: uppUnitTypes.SmallLength, bIsShared: true);


        }


        #endregion Constructors


        #region Notifications

        private void PartEventHandler(object sender, PartEventArgs e)
        {
            if (e.EventType == uppPartEventTypes.SheetMetalChange1)
            {
                PropValSet("Thickness", SheetMetalThickness, bSuppressEvnts: true);
                EventMaterialChange?.Invoke(SheetMetal);

            }
            else if (e.EventType == uppPartEventTypes.UpdateBaseProperties)
            {
                UpdatePartProperties();
            }
            else if (e.EventType == uppPartEventTypes.UpdateBaseWeight)
            {
                base.Weight = Weight(null, null, null, false, out string IICNCD);
            }

            if (e.Property != null)
            {

                Notify(e.Property);
            }
        }

        /// <summary>
        /// used by an object to respond to property changes of itself and of objects below it in the object model.
        ///also raises the changed property to the object above it it the object model.
        /// </summary>
        /// <param name="aProperty"></param>
        public void Notify(uopProperty aProperty, bool? bSuppressEvnts = null)
        {
            if (aProperty == null) return;
            string pname = aProperty.Name.ToUpper();
            bool supevnts = bSuppressEvnts.HasValue ? bSuppressEvnts.Value : SuppressEvents || Reading;
            switch (pname)
            {
                case "STIFFENERSITES":
                case "X":
                    {
                        _Boxes = null;
                        break;
                    }

                default:
                    break;
            }

            if (IsGlobal)
            {
                if (!aProperty.Protected)
                {
                    if(!supevnts)
                        PropertyChangeEvent?.Invoke(aProperty);
                    //else
                    //    if(!Reading)
                    //        PropertyChangeEvent?.Invoke(aProperty);
                }
            }

            if (!IsGlobal &&  !string.IsNullOrWhiteSpace(RangeGUID) && !supevnts)
            {
                colMDDowncomers myCol = MyCollection(null);
                myCol?.NotifyMemberChange(this, aProperty);

            }
        }

        #endregion Notifications

        #region Relationships

        public override bool IsVirtual  { get  => base.IsVirtual; set =>base.IsVirtual = value; }

        private WeakReference<mdDowncomer> _ParentRef;
        private WeakReference<mdDowncomer> _ChildRef;
        public void AssociateToParent(mdDowncomer aParent)
        {
            _ChildRef = null;
            if (aParent == null)
            {
                _ParentRef = null;
                IsVirtual = false;
                return;
            }

            _ParentRef = new WeakReference<mdDowncomer>(aParent);
            aParent._ChildRef = new WeakReference<mdDowncomer>(this);
            IsVirtual = true;
        }

        public void AssociateToChild(mdDowncomer aChild)
        {
            _ParentRef = null;
            if (aChild == null)
            {
                _ChildRef = null;
                IsVirtual = false;
                return;
            }
            _ChildRef = new WeakReference<mdDowncomer>(aChild);
            aChild._ParentRef = new WeakReference<mdDowncomer>(this);
            IsVirtual = false;
        }

        public mdDowncomer Parent
        {
            get

            {
                IsVirtual = false;
                if (_ParentRef == null) return null;
                IsVirtual = _ParentRef.TryGetTarget(out mdDowncomer _rVal);
                if (!IsVirtual) _ParentRef = null;
                return _rVal;
            }

            set => AssociateToParent(value);

        }

        public mdDowncomer Child
        {
            get
            {
                if (_ChildRef == null) return null;

                if (!_ChildRef.TryGetTarget(out mdDowncomer _rVal)) _ChildRef = null;
                return _rVal;
            }
            set => AssociateToChild(value);
        }

        public void CopyParentProperties()
        {
            mdDowncomer myparent = Parent;
            if (myparent == null) return;
            PropValsCopy(myparent.ActiveProperties, aSkipList: new List<string>() { "X" });
        }

        #endregion Relationships

        #region Properties
     

        /// <summary>
        /// the hole used to attach the downcomer to the ring
        /// </summary>
        /// <param name="bForEndSupports"></param>
        /// <returns></returns>
        internal UHOLE RingClipHoleU
        {
            get
            {
                double dia = RingClipSize == uppRingClipSizes.ThreeInchRC ? mdGlobals.gsBigHole : 13 / 25.4;
                UHOLE _rVal = new UHOLE(aTag: "RING CLIP", aFlag: "HOLE")
                {
                    X = X,
                    Depth = Thickness,
                    Elevation = 0.5 * Thickness,
                    Diameter = dia,
                };


                double rad = BoundingRadius;
                if (rad > 0 & _rVal.X <= rad)
                {
                    _rVal.Y = Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(_rVal.X, 2));
                }
                return _rVal;

            }
        }


        public double EndPlateOverhang => 0.25;


        public override string ToString() { return $"DOWNCOMER({ Index })"; }

        /// <summary>
        /// the distance form the bottom of the downcomer to the the deck below
        /// </summary>
        public double DeckClearance => RingSpacing - DeckThickness - HeightBelowDeck;

        public double MaxSupplementalDeflectorHeight => StiffenerHeight - 0.25;

        /// <summary>
        ///returns a circle that encompasses the downcomers elevation view
        /// ~used to determine if the downcomer will pass thru the manhole opening
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aXOffset"></param>
        /// <param name="aYOffset"></param>
        /// <returns></returns>
        public dxeArc PassThruCircle(mdTrayAssembly aAssy, double aXOffset = 0, double aYOffset = 0)
        {
            dxeArc _rVal = View_Elevation(aAssy, true, true, null, 0, "STIFFENERS").ExtentPoints.BoundingCircle();
            _rVal.Move(aXOffset, aYOffset);
            return _rVal;
        }

        public bool SpecialEndPlates
        {
            get
            {
                bool _rVal;
                _rVal = HasTriangularEndPlate;
                if (!_rVal) _rVal = GussetedEndplates;
                if (!_rVal) _rVal = BoltOnEndplates;
                return _rVal;
            }
        }
        public double StiffenerHeight => How + DeckThickness + 1;


        internal UVECTORS StiffenerPoints
        {
            get
            { return ProjectType == uppProjectTypes.MDSpout ? UVECTORS.Zero : mdUtils.StiffenerPoints(this, out _); }
            set
            {
                string yvals = ProjectType == uppProjectTypes.MDSpout ? string.Empty : mzUtils.ListToString(value.Ordinates(bGetY: true, 4));
                PropValSet("StiffenerSites", yvals, bSuppressEvnts: true);
            }

        }
        public List<double> StiffenerYs => mzUtils.ListToNumericCollection(StiffenerSites);



        public mdSupplementalDeflector SupplementalDeflector(int aBoxIndex = 1)
        {
            if (!HasSupplementalDeflector) return null;
            if(aBoxIndex < 1) aBoxIndex = 1;
            return aBoxIndex <= Boxes.Count ? Boxes[aBoxIndex - 1].SupplementalDeflector : null;

        }


        /// <summary>
        /// the height of the supplemental deflector
        /// </summary>
        public double SupplementalDeflectorHeight
        { get => PropValD("SupplementalDeflectorHeight"); set { if (Math.Abs(value) > 0) { PropValSet("SupplementalDeflectorHeight", Math.Abs(value)); } } }

        /// <summary>
        /// Flag indicating that supplemental deflector is required
        /// </summary>
        public bool HasSupplementalDeflector => SupplementalDeflectorHeight > 0;

        public override int OccuranceFactor
        {
            get
            {
                if (DesignFamily.IsStandardDesignFamily())
                    base.OccuranceFactor = Math.Round(X, 1) == 0 ? 1 : 2;
              

                    return base.OccuranceFactor;
            }
            set => base.OccuranceFactor = value;
        }

        /// <summary>
        /// Indicates that the crossbrace and ap pan attachments nuts should be welded to the bottom
        /// of the downcomer because it is too narrow for installation.
        /// </summary>
        public bool WeldedBottomNuts => Width <= 4 || (HasSupplementalDeflector && Width <= 8);

        /// <summary>
        ///returns the maximum distance between any consecutive members in the collection
        /// </summary>
        /// <param name="mdDowncomer"></param>
        /// <returns></returns>
        public double MaxStiffenerSpacing => mzUtils.MaxDifference(StiffenerSites, iPrecis: 5);

        /// <summary>
        /// the bottom area for the downcomer
        /// ~simply the area of the dowcomer channel opening as seen from above.
        /// ~does not include bend clearances or end plate clearances.
        /// </summary>
        public double BottomArea
        {
            get
            {
                double _rVal = 0;
                if (GetMDTrayAssembly() == null) return _rVal;
                double WL1 = WeirLength(uppSides.Left);
                double WL2 = WeirLength(uppSides.Right);
                double wd = Width;
                _rVal = WL2 * wd;
                if (WL1 > WL2) _rVal += (WL1 - WL2) * 0.5 * wd;
                return _rVal;
            }
        }



        internal List<mdDowncomerBox> _Boxes;

        /// <summary>
        /// the boxes of the downcomer
        /// </summary>
        public List<mdDowncomerBox> Boxes
        {
            get
            {
                
                if (_Boxes != null && _Boxes.Count <= 0) _Boxes = null;
                if (_Boxes == null)
                {

                    _Boxes = new List<mdDowncomerBox>();
                    DowncomerInfo info = CurrentInfo();
                    if (!info.IsVirtual)
                    {
                        info.CreatedDefinitionLines();
                    }
                    List<uopLinePair> lims = uopLinePairs.FromULinePairs(info.LimLines, aSortOder: dxxSortOrders.BottomToTop, bVirtual: false);
                    foreach (uopLinePair pair in lims)
                    {
                        if (!pair.IsDefined()) continue;
                        if (pair.IsVirtual) continue; //skip virtual lines
                        mdDowncomerBox box = new mdDowncomerBox(this, aRow: pair.Row, aIndex: _Boxes.Count + 1) { Category = "Sub Parts (Welded)", IsVirtual = pair.IsVirtual };
                   
                        _Boxes.Add(box); //add the box to the collection
                    }

                }

                return _Boxes;
            }

            set => _Boxes = value;

        }

        public override uopInstances Instances
        {
            get
            {
                uopInstances insts = new uopInstances(ToString(), this);

                if (OccuranceFactor > 1)
                {
                    insts.Add(new uopInstance(-X * 2, -Y * 2, 180), this);

                }
                return insts;
            }
            set => base.Instances = value;
        }

        /// <summary>
        /// the overall width of the downcomer box
        /// </summary>
        public double BoxWidth => Width + 2 * Thickness;



        public override dxfPlane Plane => new dxfPlane(new dxfVector(X, Y));

        public override int Col { get { base.Col = Index; return base.Col; } set => base.Col = Index; }

        public override double Z { get => 0; set { return; } }

        public double ClipClearance { get { double clipClearance = OverrideClipClearance; return (clipClearance == 0) ? DefaultRingClipClearance : clipClearance; } }

        //~must be nonzero
        /// </summary>
        public int Count { get => PropValI("Count"); set { if (Math.Abs(value) > 0) PropValSet("Count", Math.Abs(value)); } }
      


        /// <summary>
        /// used as a erence for the creation of the downcomers sub components
        /// </summary>
        public double DeckThickness { get; set; }

        public override int DowncomerIndex { get => Index; set => Index = value; }

        /// <summary>
        /// the maximum width of the foldover weirs
        /// </summary>
        public static double FoldOverMaxWidth => 0.375;

        /// <summary>
        /// flag indicating if the downcomer has fold over weirs
        /// </summary>
        public bool FoldOverWeirs => FoldOverHeight > 0;
        /// <summary>
        /// the height of the folded over portion of the weir
        /// </summary>
        public double FoldOverHeight { get => (How < 1.75) ? 0 : PropValD("FoldOverHeight"); set { if (Math.Abs(value) > 0) PropValSet("FoldOverHeight", Math.Abs(value)); } }

        /// <summary>
        /// the hole used to attach to attach the end angles to the end support
        /// </summary>
        public uopHole EndAngleHole
       => new uopHole(mdGlobals.gsBigHole, 0, 0, 0, "END ANGLE", Thickness, aInset: 0.625, aElevation: DeckThickness + 0.625);
        /// <summary>
        /// the slot used to attach to attach the end angles at the ends of the box
        /// </summary>
        public uopHole EndAngleSlot
        {
            get
            {
                uopHole _rVal = new uopHole(mdGlobals.gsBigHole, 0, 0, aLength: 22 / 25.4, aDepth: Thickness, aTag: "END ANGLE", aZDirection: "1,0,0", aInset: 1);
                if (Math.Round(X, 1) == 0) _rVal.Inset = 0.776;
                return _rVal;
            }
        }

        /// <summary>
        /// flag indicating if the downcomer has gusseted end plates
        /// </summary>
        public bool GussetedEndplates { get => (!HasTriangularEndPlate) && PropValB("GussetedEndplates"); set => PropValSet("GussetedEndplates", value); }

        /// <summary>
        /// flag indicating if the downcomer has bolt on end plates
        /// </summary>
        public bool BoltOnEndplates { get => (PropValB("BoltOnEndPlates") || Math.Round(X, 2) == 0); set => PropValSet("BoltOnEndPlates", value); }

        /// <summary>
        /// flag indicating if the downcomer has a triangular end plate
        /// </summary>
        public bool HasTriangularEndPlate
        {
            get => PropValB("HasTriangularEndPlate");

            set
            {
                //^flag indicating if the downcomer has a triangular end plate
                if (value == false)
                { PropValSet("OverrideClipClearance", 0, bSuppressEvnts: true); }

                PropValSet("HasTriangularEndPlate", value);

            }
        }
        /// <summary>
        /// the distance between the bottom of the downcomer and the deck of the tray below for downcomers within the range
        /// </summary>
        public double HeightAboveDeck => RingSpacing - (InsideHeight + Thickness - How - DeckThickness);
        /// <summary>
        /// the distance between the deck that the downcomer supports and the bottom of the downcomer
        /// </summary>
        public double HeightBelowDeck => OutsideHeight - How - DeckThickness;
        /// <summary>
        /// the weir height of the downcomer
        /// </summary>
        public double How
        {
            get => PropValD("How");
            set { if (Math.Abs(value) > 0) PropValSet("How", Math.Abs(value)); }
        }

        public double WeirHeight { get => How; set => How = value; }
        public override string INIPath => (Index > 0) ? $"COLUMN({ColumnIndex}).RANGE({RangeIndex}).TRAYASSEMBLY.DOWNCOMERS({Index})" : $"COLUMN({ColumnIndex}).RANGE({RangeIndex}).TRAYASSEMBLY.DOWNCOMER";


        /// <summary>
        /// the inside height of the downcomer
        /// </summary>
        public double InsideHeight
        {
            get => PropValD("InsideHeight");
            set
            {
                if (Math.Abs(value) > 0)
                { PropValSet("InsideHeight", Math.Abs(value)); }
            }
        }

        /// <summary>
        /// the outside height of the downcomer
        /// </summary>
        public override double Height { get => InsideHeight - Thickness; set => InsideHeight = value - Thickness; }

        /// <summary>
        ///returns a string that includes the index of downcomer and includes the index opposing downcomer
        /// ~like "1 & 4" or "2 & 3" where ther are four downcomers in the assembly
        /// </summary>
        public string Label
        {
            get
            {
                int actualIndex = Index;
                string _rVal = actualIndex.ToString();
                if (actualIndex != 0)
                {


                    int max;
                    colMDDowncomers myCol = MyCollection(null);

                    if (myCol != null)
                    {
                        int cnt = myCol.GetByVirtual(aVirtualValue: false).Count;
                        if (cnt > 0)
                        {
                            mdDowncomer lastDC = myCol.Item(cnt, true);
                            if (Math.Round(lastDC.X, 1) > 0)
                            {
                                max = 2 * cnt;
                            }
                            else
                            {
                                if (cnt > 1)
                                {
                                    max = (2 * cnt) - 1;
                                }
                                else
                                {
                                    max = 1;
                                }
                            }

                            if (max - actualIndex + 1 > cnt)
                            {
                                _rVal = $"{actualIndex} & {max - actualIndex + 1}";
                            }
                        }
                        else
                        {
                            _rVal = actualIndex.ToString();
                        }

                    }
                    else
                    {
                        _rVal = actualIndex.ToString();
                    }
                }

                return _rVal;
            }
        }
        /// <summary>
        /// the overall length of the downcomer
        /// ~this is calculated on request and is read only
        /// this length is the box length on the short side  + 0.5 inches (includes the overhange of the end plates)
        /// </summary>
        public new double Length => ComputeBeamLength(Math.Abs(X) + 0.5 * Width, true);

        public uopRectangle Bounds => new uopRectangle(X - OutsideWidth / 2, Length / 2, X + OutsideWidth / 2, -Length / 2);

        /// <summary>
        /// the true end-to-end length of the downcomer
        /// ~this is calculated on request and is read only
        /// </summary>
        public double LongAssemblyLength
        {
            get
            {
                double rad = DeckRadius;
                if (rad <= 0) return 0;
                double x1 = Math.Abs(X) - 0.5 * Width;
                return 2 * Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(x1, 2));

            }
        }
        /// <summary>
        /// the long side length of the downcomer if HasTriangulareEndPlates = True
        /// this is calculated on request and is read only
        /// this length is the box length on the long side + 0.5 inches (includes the overhange of the end plates)
        /// </summary>
        public double LongLength => Math.Round(Math.Abs(X), 1) != 0 & HasTriangularEndPlate ? ComputeBeamLength(Math.Abs(X) - 0.5 * Width + Thickness, true) : Length;

               /// <summary>
        /// the length of the weir on the long side of the downcomer
        /// </summary>
        public double LongWeirLength => CurrentInfo().LimLines[0].LongestConnector().Length;

        /// <summary>
        /// the sheet metal used to construct the downcomer
        /// </summary>
        public new uopMaterial Material
        {
            get => SheetMetal;

            set
            {
                if (value == null) return;
                if (value.MaterialType == uppMaterialTypes.SheetMetal) SheetMetal = (uopSheetMetal)value;

            }
        }



        /// <summary>
        /// returns true if there is a odd number of total downcomers
        /// </summary>
        public bool OddDowncomers => Count % 2 != 0;

        /// <summary>
        /// the overall Height of the downcomers
        /// </summary>
        public double OutsideHeight => InsideHeight + Thickness;

        /// <summary>
        /// the overall width of the downcomers
        /// </summary>
        public double OutsideWidth => Width + 2 * Thickness;

        /// <summary>
        /// the value used to calculate the angle of the end plate
        /// ~represents the smallest allowable perpendicular distance between then end plate and the ring clip hole
        /// </summary>
        public double OverrideClipClearance
        {
            get => PropValD("OverrideClipClearance");

            set
            {
                value = Math.Abs(Math.Round(value, 5));
                if (value == DefaultRingClipClearance) { value = 0; }
                PropValSet("OverrideClipClearance", value);

            }
        }

   

        /// <summary>
        /// the distance the endplate is inset into the downcomer chanel on the short side
        /// </summary>
        public double EndplateInset { get => PropValD("EndplateInset"); set { if (value < 0) { value = -1; } PropValSet("EndplateInset", value); } }

        /// <summary>
        ///returns the collection of the two shelf angles in the downcomer assembly
        /// </summary>
        public List<colUOPParts> ShelfAngles
        {
            get
            {
                var _rVal = new List<colUOPParts>();
                List<mdDowncomerBox> boxes = Boxes;
                foreach (mdDowncomerBox box in boxes)
                {
                    _rVal.Add(new colUOPParts(box.ShelfAngle(bLeft: false), box.ShelfAngle(bLeft: true), box));
                }

                return _rVal;
            }
        }

        /// <summary>
        /// the width of the shelf angles of the downcomer assembly
        /// ~default = 1
        /// </summary>
        public double ShelfWidth { get; set; } = 1;

        /// <summary>
        /// the spout diameter to use for the downcomers spouts
        /// </summary>
        public double SpoutDiameter
        {
            get
            {
                //double _rVal = Props.ValueD("SpoutDiameter");
                //if (_rVal <= 0)
                //{

                double _rVal = (!base.PropValB("METRICSPOUTING", uppPartTypes.Project)) ? 0.75 : 19 / 25.4;
                PropValSet("SpoutDiameter", _rVal, bSuppressEvnts: true);
                //}

                return _rVal;
            }
            //set { if (Math.Abs(value) > 0) PropValSet("SpoutDiameter", Math.Abs(value)); }
        }
        
      

        /// <summary>
        /// a string describing the location of the startup spouts on the left side of the downcomer
        /// </summary>
        public string StartUpSitesL { get => PropValS("StartUpSitesL"); set => PropValSet("StartUpSitesL", value); }

        /// <summary>
        /// a string describing the location of the startup spouts on the right side of the downcomer
        /// </summary>
        public string StartUpSitesR { get => PropValS("StartUpSitesR"); set => PropValSet("StartUpSitesR", value); }

        /// <summary>
        /// the diameter (height) of the startup spouts in the downcomer
        /// </summary>
        public double StartupDiameter
        {
            get
            {
                if (IsGlobal) return PropValD("StartupDiameter");
                mdTrayAssembly aAssy = GetMDTrayAssembly();
                return (aAssy == null) ? PropValD("StartupDiameter") : aAssy.Downcomer().StartupDiameter;

            }
            set
            { if (Math.Abs(value) > 0) PropValSet("StartupDiameter", Math.Abs(value)); }
        }

        /// <summary>
        /// the length of the startup spouts in the downcomer
        /// </summary>
        public double StartupLength
        {
            get
            {
                if (Index == 0) return PropValD("StartUpLength");


                mdTrayAssembly aAssy = GetMDTrayAssembly();
                return (aAssy == null) ? PropValD("StartUpLength") : aAssy.Downcomer().StartupLength;

            }
            set
            {
                if (Math.Abs(value) > 0) { PropValSet("StartUpLength", Math.Abs(value)); }
            }
        }

        /// <summary>
        /// a line that limits the startup spouts for getting put too close to the endplate area
        /// </summary>
        public uopLine StartUpLimitLine => GetLimitLines(aClearance: 0.25 + 0.375, aReducer: 0).First().Line1;

        /// <summary>
        /// the Y ordinates of the downcomers stiffeners
        /// </summary>
        public string StiffenerSites
        {
            get => PropValS("StiffenerSites");
            set => PropValSet("StiffenerSites", value);
        }


        /// <summary>
        /// the centers of the downcomers stiffeners
        /// </summary>
        /// <returns></returns>
        public colDXFVectors StiffenerCenters => StiffenerPoints.ToDXFVectors();



        /// <summary>
        /// the length of the weir on the short side of the downcomer
        /// </summary>
        public double ShortWeirLength => CurrentInfo().LimLines[0].ShortestConnector().Length;

        /// <summary>
        /// the inside width of the downcomer box channel
        /// ~the boxwidth property is the actual outside width of the box channel
        /// </summary>
        public override double Width { get => PropValD("Width"); set { if (Math.Abs(value) > 0) { PropValSet("Width", Math.Abs(value)); } } }

        /// <summary>
        /// the X coordinate of the downcomer
        /// </summary>
        public override double X { get => PropValD("X"); set => PropValSet("X", value); }
        public override double Y { get => PropValD("Y"); set => PropValSet("Y", value); }
        /// <summary>
        /// the ideal theoretical spout area for the tray assembly
        /// </summary>
        public double ASP { get => PropValD("Asp"); set => PropValSet("Asp", Math.Abs(value)); }

        public double PanelClearance => CurrentInfo().PanelClearance;

        internal DowncomerInfo Info { get; set; }

        public override dxfVector CenterDXF { get => new dxfVector(X, Y, 0) { Index = Index }; set { value ??= new dxfVector(); X = value.X; Y = value.Y; } }


        internal UHOLE BaffleMountSlot => mdGlobals.BaffleMountSlot(GetMDTrayAssembly(), Thickness);

        public double BaffleHeight { get; set; }
        #endregion Properties

        #region Methods

        public void SubPart(mdTrayAssembly aAssy, string aCategory = null, bool? bHidden = null)
        {

            base.SubPart(aAssy, aCategory,bHidden);
            
            
        }

        /// <summary>
        /// the fraction of the total tray weir that this downcomer represents
        /// </summary>
        /// <param name="aAssy">the parent tray assembly</param>
        /// <param name="aTotalWeirLength">if the know total is passed it it used otherwise it is retrievend from the tray</param>
        /// <param name="bTrayWide">if true the downcomers occurance factor is applies</param>
        /// <returns></returns>
        public double WeirFraction(mdTrayAssembly aAssy, double aTotalWeirLength = 0, bool bTrayWide = false)
        {
            if (aTotalWeirLength <= 0)
            {
                aAssy ??= GetMDTrayAssembly();
                if (aAssy == null) return 0;
                aTotalWeirLength = aAssy.TotalWeirLength;
            }
            double mytotal = WeirLength(uppSides.Undefined, bTrayWide);
            return (aTotalWeirLength != 0) ?  mytotal / aTotalWeirLength  : 0;
        }

        public override mdDowncomer GetMDDowncomer(mdTrayAssembly aAssy = null, mdDowncomer aDC = null, int aIndex = -1) => this;

        public dxePolygon View_ManholeFit(mdTrayAssembly aAssy, iVector aCenter = null, double aRotation = 0, string aLayerName = null)
        => mdPolygons.DC_View_ManholeFit(this, aAssy, aCenter, aRotation, aLayerName);

        /// <summary>
        /// the collection of AP Pans defined for the downcomer
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public List<mdAPPan> APPans(mdTrayAssembly aAssy)
        {
            aAssy ??= GetMDTrayAssembly(aAssy);
            if (aAssy == null) return new List<mdAPPan>();

            if (!aAssy.HasAntiPenetrationPans) return new List<mdAPPan>();
            return mdPartGenerator.APPans_DC(aAssy, Index);

            //colUOPParts APs = aAssy.APPans;
            //mdAPPan aPan;
            //mdAPPan bPan;
            //UVECTORS ctrs;
            //UVECTOR u1;

            //mdAPPan cPan;
            //double cX= X;

            //for (int i = 1; i <= APs.Count; i++)
            //{
            //    aPan = (mdAPPan)APs.Item(i);

            //    ctrs = aPan.CentersV;
            //    for (int j = 1; j <= ctrs.Count; j++)
            //    {
            //        u1 = ctrs.Item(j);
            //        if (Math.Round(u1.X, 1) == Math.Round(cX, 1))
            //        {
            //            bPan = aPan.Clone();
            //            cPan = bPan.Clone();
            //            cPan.CenterIndex = 1;
            //            cPan.CentersV = UVECTORS.Zero;
            //            cPan.CentersV.Add(u1);

            //            _rVal.Add(bPan);
            //        }
            //    }
            //}
            //_rVal.SubPart(this);
            //return _rVal;
        }



        /// <summary>
        ///returns the ratio of the downcomers actual spout area to the total actual spout area for the tray asembly
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public double ActualToActualRatio(mdTrayAssembly aAssy)
        {

            aAssy ??= GetMDTrayAssembly(aAssy);
            if (aAssy == null) return 0;
            double asyTot = aAssy.TotalSpoutArea;
            return (asyTot > 0) ? TotalSpoutArea(aAssy) / asyTot : 0;

        }


        /// <summary>
        /// the actual spout area to theoretical spout area ratio for the downcomer
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public double AreaRatio(mdTrayAssembly aAssy)
        {
            aAssy ??= GetMDTrayAssembly(aAssy);

            if (aAssy == null) return 0;

            double ideal = IdealSpoutArea(aAssy);
            double actual = TotalSpoutArea(aAssy);

            if (ideal != 0) return actual / ideal;
            return (ideal == 0 & actual == 0) ? 1 : 0;

        }
        /// <summary>
        /// ^the absolute value of the difference of the area ratio (actual/theo) and 1
        /// ~represents the percentage over or under the required theoretical area
        /// #1 flag to return the ablsolute value
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="bAbsVal"></param>
        /// <returns></returns>
        public double AreaRatioDifferential(mdTrayAssembly aAssy, bool bAbsVal = true)
        {
            aAssy ??= GetMDTrayAssembly(aAssy);
            if (aAssy == null) return 0;
            return bAbsVal ? Math.Abs(AreaRatio(aAssy) - 1) : AreaRatio(aAssy) - 1;
        }



        /// <summary>
        /// the baffles that are designed for the downcomer
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public List<mdBaffle> DeflectorPlates(mdTrayAssembly aAssy)
        {

            aAssy ??= GetMDTrayAssembly(aAssy);
            return (aAssy == null) ? new List<mdBaffle>() : aAssy.DeflectorPlates(Index);

        }



        /// <summary>
        ///returns a dxePolyline that represents the inside bottom of the downcomer box trough less the passed clearance
        /// #1 a clearance to apply
        /// #2 a end plate clearance to apply
        /// </summary>
        /// <param name="aClearance"></param>
        /// <param name="aEndPlateClearance"></param>
        /// <returns></returns>
        public dxePolyline BottomPerimeter(double aClearance, double aEndPlateClearance) => BottomVerts(aClearance, aEndPlateClearance).ToDXFPolyline();

        /// <summary>
        ///returns a dxePolyline that represents the inside bottom of the downcomer box trough less the passed clearance
        /// #1 a clearance to apply
        /// #2 a end plate clearance to apply
        /// </summary>
        /// <param name="aClearance"></param>
        /// <param name="aEndPlateClearance"></param>
        /// <returns></returns>
        internal UVECTORS BottomVerts(double aClearance, double aEndPlateClearance)
        {
            UVECTORS _rVal = UVECTORS.Zero;
            double wd = Width;
            double clrnc = Math.Abs(aClearance);
            double cX = X;


            if (clrnc > (0.5 * wd)) { clrnc = 0.5 * wd; }


            ULINE l1 = GetLimLines(aEndPlateClearance).First().Line1.Value;
            ULINE l2 = new ULINE(cX - (0.5 * wd - clrnc), 0, cX - (0.5 * wd - clrnc), 100);
            UVECTOR u1 = l1.IntersectionPt(l2);
            l2 = new ULINE(cX + (0.5 * wd - clrnc), 0, cX + (0.5 * wd - clrnc), 100);
            UVECTOR u2 = l1.IntersectionPt(l2);

            _rVal.Add(u2.X, -u2.Y);
            _rVal.Add(u2.X, u2.Y);
            _rVal.Add(u1.X, u1.Y);
            _rVal.Add(u1.X, -u1.Y);
            return _rVal;
        }

        public double BoxLength(bool bLongSide) => (!bLongSide) ? Length - 0.5 : LongLength - 0.5;


        public dxePolygon View_Profile(mdTrayAssembly aAssy, bool bLongSide, bool bIncludeCrossBraces = false, bool bIncludeBoltOns = false, iVector aCenter = null, double aRotation = 0, string aBoltOnList = null, string aLayerName = null, bool bOneLayer = false)
        => mdPolygons.DC_View_Profile(this, aAssy, bLongSide, bIncludeCrossBraces, bIncludeBoltOns, aCenter, aRotation, aBoltOnList, aLayerName, bOneLayer);

        public dxePolygon View_Elevation(mdTrayAssembly aAssy, bool bCrossSection = false, bool bIncludeBoltOns = false, iVector aCenter = null, double aRotation = 0, string aBoltOnList = null, string aLayerName = null, bool bOneLayer = false)
        => mdPolygons.DC_View_Elevation(this, aAssy, bCrossSection, bIncludeBoltOns, aCenter, aRotation, aBoltOnList, aLayerName, bOneLayer);

        public dxePolygon View_Plan(mdTrayAssembly aAssy, iVector aCenter = null, double aRotation = 0, bool bObscuredShelfs = false, bool bIncludeBoltOns = false, bool bSuppressHoles = false, bool bSuppressSpouts = false, double aCenterLineLength = 0, string aBoltOnList = null, string aLayerName = "GEOMETRY", string aWeldOnList = null, bool bOneLayer = true, bool bShowVirtual = true)
       => mdPolygons.DC_View_Plan(this, aAssy, aCenter, aRotation, bObscuredShelfs, bIncludeBoltOns, bSuppressHoles, bSuppressSpouts, aCenterLineLength, aBoltOnList, aLayerName, aWeldOnList, bOneLayer, bShowVirtual: bShowVirtual);

     
     
        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        public mdDowncomer Clone() => new mdDowncomer(this, GetMDTrayAssembly());

        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();

        /// <summary>
        /// recomputes the current total spout area value
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        private double ComputeSpoutArea(mdTrayAssembly aAssy, bool bTrayWide = false)
        {

            aAssy ??= GetMDTrayAssembly();
            if (aAssy == null) return 0;
            List<mdDowncomerBox> boxes = Boxes.FindAll((x) => !x.IsVirtual);
            
            double _rVal = 0;
            foreach(var box in boxes) _rVal += box.ComputeSpoutArea(aAssy, bTrayWide);
            return _rVal;
        }

 
        /// <summary>
        // retrieves a property by name
/// <summary>
        /// Part Current property
        /// </summary>
        /// <param name="aPropertyName"></param>
        /// <param name="bSupressNotFoundError"></param>
        /// <returns></returns>
        public override uopProperty CurrentProperty(string aPropertyName, bool bSupressNotFoundError = false)
        {
            UpdatePartProperties();
            return base.CurrentProperty(aPropertyName, bSupressNotFoundError);
        }

        /// <summary>
        ///returns the overall length of the downcomer assembly
        /// #1the parent tray assembly for this part
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="bShortSide"></param>
        /// <returns></returns>
        public double AssemblyLength(mdTrayAssembly aAssy = null, bool bShortSide = false)
        {

            aAssy ??= GetMDTrayAssembly(aAssy);
            if (aAssy == null) { return 0; }
            double x1 = (!bShortSide) ? (X - (Width * 0.5)) : (X + (Width * 0.5) + Thickness);

            return 2 * Math.Sqrt(Math.Pow(aAssy.DeckRadius, 2) - Math.Pow(x1, 2));


        }


        /// <summary>
        /// defines the properties of the downcomer based on the passed string
        /// ~strings like X = 32.1, Inset = 1.00
        /// #1a property string to parse and extract property values from
        /// </summary>
        /// <param name="aDifString"></param>
        /// <param name="aDelimitor"></param>
        public void DefineDifferences(string aDifString, string aDelimitor = ",")
        {

            aDifString ??= string.Empty;
            aDifString = aDifString.Trim();
            if (aDifString ==  string.Empty) return;

            TVALUES aVals = TVALUES.FromDelimitedList(aDifString, aDelimitor, bReturnNulls: false, bTrim: true);
            string pStr = string.Empty;
            string pname = string.Empty;
            string pval = string.Empty;
            int eq = 0;
            int i = 0;
            bool wuz = Reading;
            uopMaterial aSMT = null;
            uopSheetMetal bSMT = null;
            bool bBltEPS = false;
            bool bGust = false;
            bool bTria = false;
            Reading = true;

            try
            {
                for (i = 1; i <= aVals.Count; i++)
                {
                    pStr = (string)aVals.Item(i);
                    if (pStr !=  string.Empty)
                    {
                        if (pStr.Substring(0, 1) == "(")
                        {
                            pStr = pStr.Substring(pStr.Length - 1).Trim();//Right(pStr, Len(pStr) - 1));
                        }
                        if (pStr.Substring(pStr.Length - 1) == ")")
                        {
                            pStr = pStr.Substring(0, pStr.Length - 1).Trim();
                        }

                        eq = pStr.IndexOf("=");
                        pname = pStr.Substring(0, eq).Trim();
                        pval = pStr.Substring(eq + 1).Trim();

                        switch (pname.ToUpper())
                        {
                            case "X":
                                PropValSet("X", mzUtils.VarToDouble(pval), bSuppressEvnts: true);
                                break;

                            case "STIFFENERSITES":
                                PropValSet("StiffenerSites", pval, bSuppressEvnts: true);
                                break;
                            case "STARTUPSITESL":
                                PropValSet("StartUpSitesL", pval, bSuppressEvnts: true);
                                break;
                            case "STARTUPSITESR":
                                PropValSet("StartUpSitesR", pval, bSuppressEvnts: true);

                                break;
                            case "ENDPLATEINSET":
                                PropValSet("EndplateInset", mzUtils.VarToDouble(pval), bSuppressEvnts: true);
                                break;
                            case "OVERRIDECLIPCLEARANCE":
                                PropValSet("OverrideClipClearance", mzUtils.VarToDouble(pval), bSuppressEvnts: true);
                                break;
                            case "HASTRIANGULARENDPLATE":
                                bTria = mzUtils.VarToBoolean(pval);
                                PropValSet("HasTriangularEndPlate", bTria, bSuppressEvnts: true);
                                break;
                            case "FOLDOVERHEIGHT":
                                PropValSet("FoldOverHeight", mzUtils.VarToDouble(pval, true), bSuppressEvnts: true);
                                break;
                            case "SUPPLEMENTALDEFLECTORHEIGHT":
                                PropValSet("SupplementalDeflectorHeight", mzUtils.VarToDouble(pval, true), bSuppressEvnts: true);
                                break;
                            case "BOLTONENDPLATES":
                                bBltEPS = Convert.ToBoolean(pval);
                                break;
                            case "GUSSETEDENDPLATES":
                                bGust = Convert.ToBoolean(pval);

                                break;
                            case "GAGENAME":
                                aSMT = SheetMetal;
                                if (pval !=  string.Empty)
                                {
                                    bSMT = uopGlobals.goSheetMetalOptions().GetSheetMetalByStringVals(aSMT.FamilyName, pval);
                                    if (bSMT.Index <= 0) SheetMetal = bSMT;

                                }
                                break;
                        }
                    }
                }
            }
            catch
            {
                return;//to execute finally statement
            }
            finally
            {
                if (bTria)
                {
                    bBltEPS = false;
                    bGust = false;
                }
                if (Math.Round(X, 1) == 0)
                {
                    PropValSet("BoltOnEndPlates", bBltEPS);
                }

                PropValSet("GussetedEndplates", bGust);

                Reading = wuz;
            }
        }



        /// <summary>
        /// the drawings that are available for this part
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aCollector"></param>
        /// <param name="aSheetIndex"></param>
        public uopDocuments GenerateDrawings(mdTrayAssembly aAssy, uopDocuments aCollector, int aSheetIndex = 0, uppUnitFamilies aUnits = uppUnitFamilies.Metric)
        {
            uopDocuments _rVal = aCollector ?? new uopDocuments();


            //aAssy ??= GetMDTrayAssembly(aAssy);
            //if (aAssy == null) return _rVal;
            //int i = Index;
            //mdTrayRange range = aAssy.GetMDRange(null);

            //string txt;
            //   string pn = PartNumber;
            //string tname = $"{aAssy.TrayName(false)} - ";
            //if (pn ==  string.Empty)  pn = i.ToString();
            //uopDocDrawing aDWG;
            //string nm = $"Downcomer { pn } Box";

            //if (SupplementalDeflectorHeight > 0)
            //{
            //    nm = $"Downcomer { pn } Supl. Def.";
            //    aDWG = _rVal.AddDrawing(uppDrawingFamily.Manufacturing, SupplementalDeflector, nm, nm, uppDrawingTypes.SupplDeflector, uppBorderSizes.BSize_Landscape, aPartIndex: i, aUnits: aUnits);
            //    aDWG.Range = range;
            //    if (aSheetIndex > 0)
            //    {
            //        aDWG.SheetNumber = aSheetIndex;
            //        aSheetIndex++;
            //    }

            //}

            //aDWG = _rVal.AddDrawing(uppDrawingFamily.Manufacturing, this, tname + nm, nm, uppDrawingTypes.DowncomerBox, uppBorderSizes.BSize_Landscape,bCancelable: true, aUnits:aUnits );
            //aDWG.Range = range;

            //if (aSheetIndex > 0)
            //{
            //    aDWG.SheetNumber = aSheetIndex;
            //   aSheetIndex ++;
            //}
            //uopPart aPrt = EndSupport();
            //if (Math.Round(X, 1) != 0)
            //{
            //    nm = $"Downcomer { pn } End Support 1";
            //    aDWG = _rVal.AddDrawing(uppDrawingFamily.Manufacturing, aPrt, tname + nm, nm, uppDrawingTypes.EndSupport, uppBorderSizes.BSize_Landscape, aPartIndex: i, aUnits: aUnits);
            //    aDWG.Range = range;
            //    if (aSheetIndex > 0)
            //    {
            //        aDWG.SheetNumber = aSheetIndex;
            //       aSheetIndex ++;
            //    }
            //    nm = $"Downcomer { pn } End Support 2";
            //    aDWG = _rVal.AddDrawing(uppDrawingFamily.Manufacturing, aPrt, tname + nm, nm, uppDrawingTypes.EndSupport, uppBorderSizes.BSize_Landscape,aPartIndex: i, aPartType: uppPartTypes.BeamSupport,bQuestionsAreTrayPrompt: true, aUnits: aUnits);
            //    if (aSheetIndex > 0)
            //    {
            //        aDWG.SheetNumber = aSheetIndex;
            //       aSheetIndex ++;
            //    }
            //}
            //else
            //{
            //    nm = $"Downcomer { pn } End Support";
            //    aDWG = _rVal.AddDrawing(uppDrawingFamily.Manufacturing, aPrt, tname + nm, nm, uppDrawingTypes.EndSupport, uppBorderSizes.BSize_Landscape, aPartIndex: i, aUnits: aUnits);
            //    aDWG.Range = range;
            //    if (aSheetIndex > 0)
            //    {
            //        aDWG.SheetNumber = aSheetIndex;
            //       aSheetIndex ++;
            //    }

            //}

            //mdEndPlate eplate1 = EndPlate();
            //mdEndPlate eplate2 = EndPlate(true);

            //if (SpecialEndPlates)
            //{

            //    if (BoltOnEndplates)
            //    {
            //        nm = $"Downcomer { pn } End Plate 1 (Bolt On)";
            //        aDWG = _rVal.AddDrawing(uppDrawingFamily.Manufacturing, eplate1, nm, nm, uppDrawingTypes.EndPlate, uppBorderSizes.BSize_Landscape, aPartIndex: i, aUnits: aUnits);
            //        aDWG.Range = range;
            //        if (aSheetIndex > 0)
            //        {
            //            aDWG.SheetNumber = aSheetIndex;
            //           aSheetIndex ++;
            //        }
            //        nm = $"Downcomer { pn } End Plate 2";
            //        aDWG = _rVal.AddDrawing(uppDrawingFamily.Manufacturing, eplate1, tname + nm, nm, uppDrawingTypes.EndPlate, uppBorderSizes.BSize_Landscape, aPartIndex: i, aUnits: aUnits);
            //        aDWG.Range = range;
            //        if (aSheetIndex > 0)
            //        {
            //            aDWG.SheetNumber = aSheetIndex;
            //           aSheetIndex ++;
            //        }

            //    }
            //    else
            //    {
            //        txt = GussetedEndplates ? " (Gussets)" : " (Trapz.)";

            //        nm = $"Downcomer { pn } End Plate 1{ txt }";
            //        aDWG = _rVal.AddDrawing(uppDrawingFamily.Manufacturing, aPrt, tname + nm, nm, uppDrawingTypes.EndPlate, uppBorderSizes.BSize_Landscape, aPartIndex: i, aUnits: aUnits);
            //        aDWG.Range = range;
            //        if (aSheetIndex > 0)
            //        {
            //            aDWG.SheetNumber = aSheetIndex;
            //           aSheetIndex ++;
            //        }
            //        nm = $"Downcomer { pn } End Plate 2{ txt }";

            //        aDWG = _rVal.AddDrawing(uppDrawingFamily.Manufacturing, aPrt, tname + nm, nm, uppDrawingTypes.EndPlate, uppBorderSizes.BSize_Landscape, aPartIndex: i, false, null, false, uppPartTypes.BeamSupport, null, Math.Round(X, 1) != 0, aUnits: aUnits);
            //        aDWG.Range = range;
            //        if (aSheetIndex > 0)
            //        {
            //            aDWG.SheetNumber = aSheetIndex;
            //           aSheetIndex ++;
            //        }
            //    }
            //}



            return _rVal;
        }


        /// <summary>
        /// the distance the end angle holes are inset from the end of the angle
        /// varies from 1'' to 0.5'' to clear the endsupport weld
        /// </summary>
        /// <param name="bLeftSide"></param>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public double EndAngleHoleInset(bool bLeftSide, mdTrayAssembly aAssy = null)
        {

            aAssy ??= GetMDTrayAssembly(aAssy);


            double rad = (aAssy != null) ? aAssy.DeckRadius : DeckRadius;
            if (rad <= 0) return 0;


            double x1 = bLeftSide ? X - 0.5 * Width : X + 0.5 * Width + Thickness;

            double yb = bLeftSide ? 0.5 * BoxLength(true) : 0.5 * BoxLength(false);
            double y1 = Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(x1, 2));
            yb += mdEndSupport.WeldGap + mdEndSupport.NotchDepth;

            double clrc = Math.Abs(y1) - Math.Abs(yb);

            double hRad = EndAngleHole.Radius;

            //make sure the hole clears the weld below by at least 0.25''
            if (clrc - 1 - hRad >= 0.25)
            { return 1; }
            else if (clrc - 0.75 - hRad >= 0.25)
            { return 0.75; }
            else if (clrc - 0.625 - hRad >= 0.25)
            { return 0.625; }
            else
            { return 0.5; }


        }

        public List<mdEndAngle> EndAngles(bool bTrayWide = false) => mdPartGenerator.EndAngles_DC(this, null, bTrayWide: bTrayWide);


        /// <summary>
        /// the end plates of all the boxes of the downcomer
        /// </summary>
        /// <param name="aBoxIndex"></param>
        /// <param name="aSide"></param>
        /// <returns></returns>
        public List<mdEndPlate> EndPlates(int aBoxIndex = 0, uppSides aSide = uppSides.Undefined)
        {

            List<mdEndPlate> _rVal = new List<mdEndPlate>();
            if (aSide != uppSides.Top && aSide != uppSides.Bottom) aSide = uppSides.Undefined;

            List<mdDowncomerBox> boxes = Boxes;
            for (int i = 1; i <= boxes.Count; i++)
            {
                if (aBoxIndex <= 0 || aBoxIndex == i)
                {
                    mdDowncomerBox box = boxes[i - 1];
                    if (aSide == uppSides.Top || aSide == uppSides.Undefined) _rVal.Add(box.EndPlate(bTop: true));


                    if (aSide == uppSides.Bottom || aSide == uppSides.Undefined) _rVal.Add(box.EndPlate(bTop: false));

                }
            }




            return _rVal;
        }

        /// <summary>
        /// retuens the end supports from all the downcomers boxes
        /// </summary>
        public List<mdEndSupport> EndSupports(int aBoxIndex = 0, uppSides aSide = uppSides.Undefined)
        {
            List<mdEndSupport> _rVal = new List<mdEndSupport>();
            if (aSide != uppSides.Top && aSide != uppSides.Bottom) aSide = uppSides.Undefined;

            List<mdDowncomerBox> boxes = Boxes;
            for (int i = 1; i <= boxes.Count; i++)
            {
                if (aBoxIndex <= 0 || aBoxIndex == i)
                {
                    mdDowncomerBox box = boxes[i - 1];
                    if (aSide == uppSides.Top || aSide == uppSides.Undefined) _rVal.Add(box.EndSupport(bTop: true));


                    if (aSide == uppSides.Bottom || aSide == uppSides.Undefined) _rVal.Add(box.EndSupport(bTop: false));

                }
            }
            return _rVal;

        }
        /// <summary>
        /// the percentage that the current spout area deviates fron the ideal
        /// </summary>
        public double ErrorPercentage(mdTrayAssembly aAssy) => AreaRatioDifferential(aAssy, false) * 100;

        /// <summary>
        /// the collection of finger clips defined for the downcomer
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aClips"></param>
        /// <returns></returns>
        public mdFingerClip FingerClip(mdTrayAssembly aAssy)
        {
            colUOPParts aClips = FingerClips(aAssy);
            mdFingerClip _rVal = new mdFingerClip(this);
            if (aClips.Count > 0)
            {
                _rVal = (mdFingerClip)aClips.Item(1, true);
                _rVal.Category = "Sub Parts";
                _rVal.Quantity = aClips.Count * OccuranceFactor;
            }
            return _rVal;
        }

       
        /// <summary>
        /// the collection of finger clips defined for the downcomer
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aSide"></param>
        /// <returns></returns>
        public colUOPParts FingerClips(mdTrayAssembly aAssy, uppSides aSide = uppSides.Undefined)
        {
            aAssy ??= GetMDTrayAssembly(aAssy);
            return mdUtils.FingerClips(aAssy, null, Index, 0, aSide);
        }

        /// <summary>
        /// executed internally to create the ap pans collection
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        private colUOPParts GenerateAPPans(mdTrayAssembly aAssy = null)
        {
            colUOPParts _rVal = new colUOPParts();
            _rVal.SubPart(this);
            aAssy ??= GetMDTrayAssembly(aAssy);
            if (aAssy == null) return _rVal;

            if (!aAssy.HasAntiPenetrationPans) return _rVal;
            List<mdAPPan> pans = mdPartGenerator.APPans_DC(aAssy, Index);
            foreach (mdAPPan pan in pans)
            {
                _rVal.Add(pan);
            }
            return _rVal;

            //colUOPParts APs = aAssy.APPans;
            //mdAPPan aPan;
            //mdAPPan bPan;
            //UVECTORS ctrs;
            //UVECTOR u1;

            //mdAPPan cPan;
            //double cX= X;

            //for (int i = 1; i <= APs.Count; i++)
            //{
            //    aPan = (mdAPPan)APs.Item(i);

            //    ctrs = aPan.CentersV;
            //    for (int j = 1; j <= ctrs.Count; j++)
            //    {
            //        u1 = ctrs.Item(j);
            //        if (Math.Round(u1.X, 1) == Math.Round(cX, 1))
            //        {
            //            bPan = aPan.Clone();
            //            cPan = bPan.Clone();
            //            cPan.CenterIndex = 1;
            //            cPan.CentersV = UVECTORS.Zero;
            //            cPan.CentersV.Add(u1);

            //            _rVal.Add(bPan);
            //        }
            //    }
            //}
            //_rVal.SubPart(this);
            //return _rVal;
        }

   

        /// <summary>
        ///returns the properties of downcomer that differ from the passed downcomer properties
        /// ~signatures like "COLOR=RED"
        /// #1 the properties collection of a downcomer to compare to
        /// </summary>
        /// <param name="aBasis"></param>
        /// <returns></returns>
        public uopProperties GetDifferences(mdDowncomer aBasis)
        {
            uopProperties _rVal = new uopProperties();
            if (aBasis == null)
            {
                _rVal = CurrentProperties();
                return _rVal;
            }
            uopProperties myProps = CurrentProperties();
            uopProperties herProps = aBasis.CurrentProperties();
            uopProperty myProp;
            uopProperty herProp;


            _rVal.Add(myProps.Item("X"));
            _rVal.Add(myProps.Item("StiffenerSites"));
            _rVal.Add(myProps.Item("StartUpSitesL"));
            _rVal.Add(myProps.Item("StartUpSitesR"));

            myProp = myProps.Item("HasTriangularEndPlate");
            herProp = herProps.Item("HasTriangularEndPlate");
            if (myProp.Value != herProp.Value)
            {
                _rVal.Add(myProp);
            }

            //"EndplateInset"
            myProp = myProps.Item("EndplateInset");
            herProp = herProps.Item("EndplateInset");
            if (myProp.Value != herProp.Value)
            {
                _rVal.Add(myProp);
            }
            //"OverrideClipClearance"
            myProp = myProps.Item("OverrideClipClearance");
            herProp = herProps.Item("OverrideClipClearance");
            if (myProp.Value != herProp.Value)
            {
                _rVal.Add(myProp);
            }
            //"Material"Gage
            myProp = new uopProperty("GageName", Material.GageName);
            herProp = new uopProperty("GageName", aBasis.Material.GageName);
            if (myProp.Value != herProp.Value)
            {
                _rVal.Add(myProp);
            }

            //"FoldOverHeight"
            myProp = myProps.Item("FoldOverHeight");
            herProp = herProps.Item("FoldOverHeight");
            if (myProp.Value != herProp.Value)
            {
                _rVal.Add(myProp);
            }

            //"GussetedEndplates"
            myProp = myProps.Item("GussetedEndplates");
            herProp = herProps.Item("GussetedEndplates");
            if (myProp.Value != herProp.Value)
            {
                _rVal.Add(myProp);
            }

            //"SupplementalDeflectorHeight"
            myProp = myProps.Item("SupplementalDeflectorHeight");
            herProp = herProps.Item("SupplementalDeflectorHeight");
            if (myProp.Value != herProp.Value)
            {
                _rVal.Add(myProp);
            }

            //"BoltOnEndPlates"
            myProp = myProps.Item("BoltOnEndPlates");
            herProp = herProps.Item("BoltOnEndPlates");
            if (myProp.Value != herProp.Value)
            {
                _rVal.Add(myProp);
            }
            _rVal.SubPart(this, "Properties");
            return _rVal;
        }

        public DowncomerInfo CurrentInfo(double? aXVal = null, mdTrayAssembly aAssy = null, bool bRegenInfo = false)
        {
            if (!aXVal.HasValue)
                aXVal = X;


            aAssy ??= GetMDTrayAssembly();
            if (X != aXVal.Value || Info == null) return new DowncomerInfo(this, aAssy, aXVal);
            if (bRegenInfo || Info == null)
            {
                Info = new DowncomerInfo(this, aAssy, X);
                _Boxes = null;
            }
            Info.DCIndex = Index;
            return Info;
        }


        public double GetEndPlateInset(bool bLongSide)
        => bLongSide ? (LongLength - 0.5 - LongWeirLength) / 2 : (Length - 0.5 - ShortWeirLength) / 2;


        /// <summary>
        /// the ideal spout area for the downcomer
        /// equal to the weir length of the downcomer divided by the total weir length of the tray times the spout area of the tray
        /// </summary>
        public double IdealSpoutArea(mdTrayAssembly aAssy, double aTotalWeirLength = 0, double aTrayIdealSpoutArea = 0, bool bTrayWide = false)
        {
            if (aTrayIdealSpoutArea <= 0)
            {
                aAssy ??= GetMDTrayAssembly();
                if (aAssy == null) return 0;
                aTrayIdealSpoutArea = aAssy.TheoreticalSpoutArea;

            }
            if (aTrayIdealSpoutArea <= 0) return 0;
            if (aTotalWeirLength <= 0)
                aTotalWeirLength = aAssy.TotalWeirLength;


            aAssy ??= GetMDTrayAssembly();
            if (aAssy == null) return 0;
            List<mdDowncomerBox> boxes = Boxes;

            double _rVal = 0;
            foreach (var box in boxes) _rVal += box.IdealSpoutArea(aAssy, aTotalWeirLength,aTrayIdealSpoutArea, bTrayWide);
            return _rVal;


        }


        /// <summary>
        ///returns the ratio of the downcomers ideal spout area to the total ideal spout area for the tray asembly
        /// </summary>
        public double IdealToIdealRatio(mdTrayAssembly aAssy)
        {
            aAssy ??= GetMDTrayAssembly(aAssy);
            if (aAssy == null) return 0;
            return (aAssy.TheoreticalSpoutArea > 0) ? IdealSpoutArea(aAssy) / aAssy.TheoreticalSpoutArea : 0;
        }


        public List<uopLinePair> LimitLines => CurrentInfo().LimitLines;

        public List<uopLinePair> GetLimitLines(double aClearance = 0, double aReducer = 0, bool bRegenInfo = false) => CurrentInfo(bRegenInfo: bRegenInfo).GenerateLimitLines(aClearance: aClearance, aReducer: aReducer, bRegenLimits: bRegenInfo);

        internal List<ULINEPAIR> LimLines(bool bRegenInfo = false) => CurrentInfo(bRegenInfo: bRegenInfo).LimLines;

        internal List<ULINEPAIR> GetLimLines(double aClearance = 0, double aReducer = 0, bool bRegenInfo = false) => CurrentInfo(bRegenInfo: bRegenInfo).GenerateLimLines(aClearance, aReducer);

        /// <summary>
        /// the greatest Y value for a stiffener on the downcomer
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>

        public double MaxStiffenerOrdinate(mdTrayAssembly aAssy)
        { aAssy ??= GetMDTrayAssembly(aAssy); return 0.5 * ShortWeirLength - 2 - EndAngleHoleInset(false, aAssy) - 0.25 - 1.5; }

        /// <summary>
        /// the smallest Y value for a stiffener on the downcomer
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public double MinStiffenerOrdinate(mdTrayAssembly aAssy)
        {

            double mx = MaxStiffenerOrdinate(aAssy);

            ULINE aLn = LimLines().First().Line1.Value;

            double _rVal = aLn.ep.Y + (aLn.sp.Y - aLn.ep.Y) / 2;
            if (DesignFamily.IsEcmdDesignFamily())
            { _rVal -= 9; }
            else
            { _rVal -= 18; }
            if (_rVal < 0) _rVal = 0;
            if (_rVal > mx) _rVal = mx;

            return _rVal;
        }

        /// <summary>
        /// the collection that this downcomer is a member of
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public colMDDowncomers MyCollection(mdTrayAssembly aAssy)
        {
            aAssy ??= GetMDTrayAssembly(aAssy);
            return aAssy?.Downcomers;
        }


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
        public override void ReadProperties(uopProject aProject, uopPropertyArray aFileProps, ref uopDocuments ioWarnings, double aFileVersion, string aFileSection = null, bool bIgnoreNotFound = false, uopTrayAssembly aAssy = null, uopPart aPartParameter = null, List<string> aSkipList = null, Dictionary<string, string> EqualNames = null, uopProperties aProperties = null)
        {

            try
            {
                ioWarnings ??= new uopDocuments();
                if (IsVirtual)
                {
                    mdDowncomer parent = Parent;
                    if (parent != null)
                    {
                        aFileSection = string.IsNullOrWhiteSpace(aFileSection) ? parent.INIPath : aFileSection.Trim();

                    }
                }
                aFileSection = string.IsNullOrWhiteSpace(aFileSection) ? INIPath : aFileSection.Trim();


                if (aFileProps == null) throw new Exception("The Passed File Property Array is Undefined");
                if (string.IsNullOrWhiteSpace(aFileSection)) throw new Exception("File Section is Undefined");
                if (aProject != null) SubPart(aProject);


                uopProperties myprops = aFileProps.Item(aFileSection);
                if (myprops == null || myprops.Count <= 0)
                {
                    ioWarnings?.AddWarning(this, $"{PartName} Data Not Found", $"File '{aFileProps.Name}' Does Not Contain {aFileSection} Info!");
                    return;
                }

                if (!myprops.Contains("InsideHeight"))
                    myprops.Add(new uopProperty("InsideHeight", myprops.ValueD("Height"), uppUnitTypes.SmallLength));

                if (!myprops.Contains("OverrideClipClearance"))
                    myprops.Add(new uopProperty("OverrideClipClearance", myprops.ValueD("MinimumClipClearance"), uppUnitTypes.SmallLength));

                double aVal = myprops.ValueD("OverrideClipClearance", 0);
                if (aVal < 0) aVal = 0;
                if (aVal > 0 && aVal == DefaultRingClipClearance) aVal = 0;

                myprops.SetValueD("OverrideClipClearance", aVal);

                if (myprops.Contains("EndPlateInset"))
                {
                    aVal = myprops.ValueD("EndPlateInset", -1d);
                    if (aVal < 0) aVal = -1;
                    if (aFileVersion < 2.5 && aVal == 1) aVal = -1;
                    myprops.SetValueD("EndplateInset", aVal);
                }
                uopDocuments warnings = new uopDocuments();
                List<string> skippers = HiddenPropertyNames();
                base.ReadProperties(aProject, aFileProps, ref warnings, aFileVersion, aFileSection, bIgnoreNotFound, aSkipList: skippers, EqualNames: null);


                ioWarnings.Append(warnings);

            }
            catch (Exception e)
            {
                ioWarnings?.AddWarning(this, "Read Properties Error", e.Message);
            }
            finally
            {
                Reading = false;
                aProject?.ReadStatus("", 2);
            }
        }

        /// <summary>
        /// makes sure that the properties of a collection downcomer matches its assemblies global downcomer properties
        /// </summary>
        /// <param name="aBasis"></param>
        public void RefreshProperties(mdDowncomer aBasis)
        {
            if (Index < 0) return;

            if (aBasis == null)
            {
                mdTrayAssembly aAssy = GetMDTrayAssembly();
                if (aAssy != null) aBasis = aAssy.Downcomer();

            }

            if (aBasis == null) return;


            TPROPERTIES aProps = ActiveProps;
            aProps.CopyShared(aBasis.ActiveProps);
            ActiveProps = aProps;
            DeckThickness = aBasis.DeckThickness;

            if (IsVirtual)
                CopyParentProperties();
        }

        public uopProperties ReportProperties(mdTrayAssembly aAssy, double aTableIndex)
        {

            uopProperties _rVal = new uopProperties();

            aAssy ??= GetMDTrayAssembly(aAssy);

            if (aTableIndex == 1)
            {

                _rVal.Add("DC", Label);
                _rVal.Add("Side 1", BoxLength(true), uppUnitTypes.SmallLength, aDisplayName: "Side 1");
                _rVal.Add("Side 2", BoxLength(false), uppUnitTypes.SmallLength, aDisplayName: "Side 2");
                _rVal.Add("Side 1", WeirLength(uppSides.Left), uppUnitTypes.SmallLength, aDisplayName: "Side 1");
                _rVal.Add("Side 2", WeirLength(uppSides.Right), uppUnitTypes.SmallLength, aDisplayName: "Side 2");
                _rVal.Add("Ideal", IdealSpoutArea(aAssy), uppUnitTypes.SmallArea, aDisplayName: "Ideal");
                _rVal.Add("Actual", TotalSpoutArea(aAssy), uppUnitTypes.SmallArea, aDisplayName: "Actual");
                _rVal.Add("Error", ErrorPercentage(aAssy), uppUnitTypes.Percentage, aDisplayName: "Error");
                _rVal.Add("Ideal", IdealToIdealRatio(aAssy), aDisplayName: "Ideal");
                _rVal.Add("Actual", ActualToActualRatio(aAssy), aDisplayName: "Actual");
                //todo
                _rVal.SetFormatString("0.0000", _rVal.Count - 1, _rVal.Count);

                //.SetProtected True

            }
            if (aTableIndex == 2)
            {

                _rVal = new uopProperties
                {
                    { "DC", Label },
                    { "DC Area", BottomArea / 144, uppUnitTypes.BigArea },
                    { "Thick", Material.Thickness, uppUnitTypes.SmallLength },
                    { "Length", LongAssemblyLength, uppUnitTypes.SmallLength },
                    { "Inset", GetEndPlateInset(false), uppUnitTypes.SmallLength }
                };
                Boxes[0].GetClipClearances(out double d1, out double d2, out int cnt);
                if (cnt == 0)
                {
                    _rVal.Add("Clip_Cl1", "-", aDisplayName: "Clip Cl.");
                    _rVal.Add("Clip_Cl2", "-", aDisplayName: "Clip Cl.");
                }
                else if (cnt == 1)
                {

                    _rVal.Add("Clip_Cl1", d1, uppUnitTypes.SmallLength, aDisplayName: "Clip Cl.");
                    _rVal.Add("Clip_Cl2", "-", aDisplayName: "Clip Cl.");
                }
                else
                {

                    _rVal.Add("Clip_Cl1", d1, uppUnitTypes.SmallLength, aDisplayName: "Clip Cl.");
                    _rVal.Add("Clip_Cl2", d2, uppUnitTypes.SmallLength, aDisplayName: "Clip Cl.");
                }


                _rVal.Add("Weight", Weight(aAssy, null, null, bSuppressBoltOns: false, out string rInclude), uppUnitTypes.Weight);
                _rVal.SetProtected(true);

            }
            return _rVal;
        }

        /// <summary>
        /// the collection of finger clips defined for the downcomer
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aClips"></param>
        /// <returns></returns>
        uopRingClip RingClip(mdTrayAssembly aAssy)
        {
            uopRingClip _rVal = new uopRingClip(aAssy);
            colUOPParts aClips = RingClips(aAssy);
            if (aClips.Count > 0)
            {
                _rVal = (uopRingClip)aClips.Item(1, true);
                _rVal.Category = "Sub Parts";
                _rVal.Quantity = aClips.Count * OccuranceFactor;

            }
            return _rVal;
        }

        /// <summary>
        /// the hole used to attach the downcomer to the ring
        /// </summary>
        /// <param name="aTrayAssy"></param>
        /// <returns></returns>
        public uopHole RingClipHole() => new uopHole(RingClipHoleU);


        public colUOPParts RingClips(mdTrayAssembly aAssy)
        {
            colUOPParts _rVal = new colUOPParts();


            _rVal.SubPart(this);

            aAssy ??= GetMDTrayAssembly(aAssy);
            if (aAssy == null) return _rVal;

            uopRingClip aClip = aAssy.RingClip(true);
            uopHoles sHoles = null;

            uopHole aHl = null;
            mdEndSupport aES = Boxes[0].EndSupport();


            sHoles = aES.GenHoles("RING CLIP").Item("RING CLIP");
            for (int i = 1; i <= sHoles.Count; i++)
            {
                aHl = sHoles.Item(i);
                aClip.CenterDXF = aHl.CenterDXF;
                _rVal.Add(aClip, true);
                aClip.Y = -aClip.Y;
                _rVal.Add(aClip, true);
            }
            _rVal.SubPart(this);
            _rVal.SetVisibility(true);
            return _rVal;
        }
        /// <summary>
        /// executed internally to create the holes in the section
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aTag"></param>
        /// <param name="aFlag"></param>
        /// <param name="bSuppressSpouts"></param>
        /// <param name="bTrayWide"></param>
        /// <returns></returns>
        internal UHOLEARRAY GenHolesV(mdTrayAssembly aAssy = null, string aTag = "", string aFlag = "", bool bSuppressSpouts = false)
        {
            UHOLEARRAY _rVal =UHOLEARRAY.Null;;

            aAssy ??= GetMDTrayAssembly(aAssy);
            if (aAssy == null) return _rVal;
            List<mdDowncomerBox> myboxes = Boxes;
            foreach (mdDowncomerBox box in myboxes)
            {
                _rVal.Append(box.GenHolesV(aAssy, aTag, aFlag, bSuppressSpouts), bAppendToExisting: true);
            }

            if ((myboxes?.Count ?? 0) > 0)
            {
                UHOLEARRAY EsHoles = Boxes[0].EndSupport(bTop: true).GenHolesV(aTag, aFlag);
                if (aAssy.IsSymmetric)
                    EsHoles.AppendMirrors(null, Y);
                else
                    EsHoles.Append(Boxes.Last().EndSupport(bTop: false).GenHolesV(aTag, aFlag));

                _rVal.Append(EsHoles);

                if (aAssy.DesignFamily.IsEcmdDesignFamily())
                {
                    List<mdBaffle> aBafs = aAssy.DeflectorPlates(Index);
                    foreach (mdBaffle aBaf in aBafs)
                    {
                        _rVal.Append(aBaf.GenHolesV(aAssy));
                    }
                } 
            }
            //if (bTrayWide && OccuranceFactor > 1)
            //{
            //   UHOLEARRAY rval = Instances.ApplyTo(_rVal, true);
            //    _rVal = rval;

            //}
            return _rVal;
        }
        /// <summary>
        /// executed internally to create the holes in the section
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aTag"></param>
        /// <param name="aFlag"></param>
        /// <param name="bSuppressSpouts"></param>
        /// <param name="bTrayWide"></param>
        /// <returns></returns>
        public uopHoleArray GenHoles(mdTrayAssembly aAssy = null, string aTag = "", string aFlag = "", bool bSuppressSpouts = false)
        => new uopHoleArray(GenHolesV(aAssy, aTag, aFlag, bSuppressSpouts));


        public override uopHoleArray HoleArray(uopTrayAssembly aAssy = null, string aTag = null) => GenHoles(aAssy != null && aAssy.ProjectFamily == uppProjectFamilies.uopFamMD ? (mdTrayAssembly)aAssy : null, aTag);
        public override uopPropertyArray SaveProperties(string aHeading = null)
        {

            UpdatePartProperties();
            aHeading = string.IsNullOrWhiteSpace(aHeading) ? INIPath : aHeading.Trim();
            uopProperties _rVal = SaveProperties(GetMDTrayAssembly(), IsGlobal).Item(1);

            return new uopPropertyArray(_rVal, aName: aHeading, aHeading: aHeading);

        }
        /// <summary>
        ///returns the properties required to save the downcomer object to file
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public uopPropertyArray SaveProperties(mdTrayAssembly aAssy = null, bool isBaseDownComer = false)
        {

            aAssy ??= GetMDTrayAssembly(aAssy);

            UpdatePartProperties(aAssy);
            uopProperties props = CurrentProperties();
            if (Index <= 0 && isBaseDownComer)
            {

                props.SetHidden("EndplateInset", true);
                props.SetHidden("OverrideClipClearance", true);
                props.SetHidden("SupplementalDeflectorHeight", true);
                props.SetHidden("BoltOnEndPlates", true);
                props.SetHidden("HasTriangularEndPlate", true);

                //props.PrintToConsole("HIIDEN", bIndexed: true, bHiddenVal: true);

                props = props.GetHiddenProperties(false);
                //props.Add("Thickness", Thickness, uppUnitTypes.SmallLength );
                // props.PrintToConsole("NOT HIDDEN", bIndexed: true, bHiddenVal: false);

            }
            else
            {
                mdDowncomer aBasis = null;
                if (aAssy != null) aBasis = aAssy.Downcomer();

                props = GetDifferences(aBasis);
            }
            return new uopPropertyArray(props, aName: INIPath, aHeading: INIPath);
        }

        public List<string> HiddenPropertyNames(bool bReturnInverse = false)
        {
            bool isglobal = Index <= 0 && IsGlobal;


            //AddProperty("X", 0, aUnitType: uppUnitTypes.SmallLength);
            //AddProperty("Count", 0, aUnitType: uppUnitTypes.Undefined);
            //AddProperty("Width", 0, aUnitType: uppUnitTypes.SmallLength, bIsShared: true);
            //AddProperty("InsideHeight", 0, aUnitType: uppUnitTypes.SmallLength, bIsShared: true);
            //AddProperty("How", 0, aDisplayName: "Weir Height (How)", aUnitType: uppUnitTypes.SmallLength, bIsShared: true);
            //AddProperty("SpoutDiameter", .75, aUnitType: uppUnitTypes.SmallLength);
            //AddProperty("StartupDiameter", .375, aUnitType: uppUnitTypes.SmallLength, bIsShared: true);
            //AddProperty("StartupLength", 0.5, aUnitType: uppUnitTypes.SmallLength, bIsShared: true);
            //AddProperty("HasTriangularEndPlate", false);
            //AddProperty("EndplateInset", -1.0, aUnitType: uppUnitTypes.SmallLength);
            //AddProperty("Asp", 0!, aUnitType: uppUnitTypes.SmallArea, bIsShared: true);
            //AddProperty("OverrideClipClearance", 0, aUnitType: uppUnitTypes.SmallLength);
            //AddProperty("Material", "");
            //AddProperty("StiffenerSites", "");
            //AddProperty("StartupSitesL", "");
            //AddProperty("StartupSitesR", "");
            //AddProperty("FoldOverHeight", 0, aUnitType: uppUnitTypes.SmallLength);
            //AddProperty("GussetedEndPlates", false);
            //AddProperty("SupplementalDeflectorHeight", 0, aUnitType: uppUnitTypes.SmallLength);
            //AddProperty("BoltOnEndPlates", false);
            //AddProperty("Y", 0, aUnitType: uppUnitTypes.SmallLength);
            //AddProperty("Spacing", 0, aDisplayName: "DC Spacing", aUnitType: uppUnitTypes.SmallLength, bIsShared: true);
            //AddProperty("TotalWeir", 0, aUnitType: uppUnitTypes.SmallLength, bIsShared: true);
            //AddProperty("TotalArea", 0, aUnitType: uppUnitTypes.SmallArea, bIsShared: true);
            //AddProperty("SpoutArea", 0, aUnitType: uppUnitTypes.SmallArea, bIsShared: true);
            //AddProperty("Thickness", 0, aUnitType: uppUnitTypes.SmallLength);
            //PropValSet("Material", SheetMetal.Descriptor, bSuppressEvnts: true);

            uopProperties props = ActiveProperties;
            List<string> globalhidden = new List<string> {
                "X",
                "FoldOverHeight",
                "GussetedEndPlates",
                "SpoutArea",
                "Spacing",
                "TotalWeir",
                "TotalArea",
                "Thickness",
                "EndplateInset" ,
                "OverrideClipClearance",
                "SupplementalDeflectorHeight",
                "BoltOnEndPlates",
                "HasTriangularEndPlate",
                "StiffenerSites",
                "StartupSitesL",
                "StartupSitesR", "Y" };


            List<string> realhidden = new List<string> {
                "Count",
                "Width",
                "InsideHeight",
                "How",
                "SpoutDiameter",
                 "StartupDiameter",
                "StartupLength",
                "Asp",
                "Material",
                 "Spacing",
                "TotalArea",
                "TotalWeir",
                };


            if (ProjectType == uppProjectTypes.MDSpout)
            {

                realhidden.Add("FoldOverHeight");
                realhidden.Add("GussetedEndPlates");
                realhidden.Add("SupplementalDeflectorHeight");
                realhidden.Add("BoltOnEndPlates");
                realhidden.Add("StiffenerSites");

            }



            if (isglobal)
            {
                return !bReturnInverse ? globalhidden : realhidden;


            }
            else
            {
                return !bReturnInverse ? realhidden : globalhidden;
            }
        }


        /// <summary>
        /// the assembly startup spouts that lie on this downcomer
        /// </summary>
        /// <param name="newobj"></param>
        public void SetStartupSpouts(mdStartupSpouts newobj)
        {
            bool nomirrors = !DesignFamily.IsStandardDesignFamily();
            if (newobj == null)
            {
                PropValSet("StartUpSitesL", "", bSuppressEvnts: true);
                PropValSet("StartUpSitesR", "", bSuppressEvnts: true);
            }
            else
            {
                mdStartupSpouts spouts = newobj.GetBySide(aLeftSide:true, bUnsuppressedOnly: true);
                string ords = spouts.YOrds(nomirrors);
                PropValSet("StartUpSitesL", ords, bSuppressEvnts: true);
                spouts = newobj.GetBySide(false, true);
                ords = spouts.YOrds(nomirrors);
                PropValSet("StartUpSitesR", ords, bSuppressEvnts: true);
            }
        }

        /// <summary>
        /// a line at the top of the downcomer aligned with the face of the end plate and offet by the downcomers spout clearance.
        /// </summary>
        /// <param name="aSpoutGroup"></param>
        /// <param name="aConstraints"></param>
        /// <param name="bIncludeSpoutRadius"></param>
        /// <returns></returns>
        public dxeLine SpoutGroupLimitLine(mdSpoutGroup aSpoutGroup, mdConstraint aConstraints, bool bIncludeSpoutRadius) => SpoutGroupLimLine(aSpoutGroup, aConstraints, bIncludeSpoutRadius).ToDXFLine();

        /// <summary>
        /// a line at the top of the downcomer aligned with the face of the end plate and offet by the passed clearance.
        /// </summary>
        /// <param name="aSpoutGroup"></param>
        /// <param name="aConstraints"></param>
        /// <param name="bIncludeSpoutRadius"></param>
        /// <param name="aSpoutRadius"></param>
        /// <param name="aEPClearance"></param>
        /// <returns></returns>
        internal ULINE SpoutGroupLimLine(mdSpoutGroup aSpoutGroup, mdConstraint aConstraints, bool bIncludeSpoutRadius, double aSpoutRadius = 0, double aEPClearance = 0)
        {
            ULINE _rVal;
            double aclrc = 0;
            double cnclrc = 0;
            double arad = 0;
            if (aSpoutGroup != null)
            {
                aConstraints ??= aSpoutGroup.Constraints(null);

                arad = aSpoutRadius;
                if (arad <= 0)
                {
                    arad = aConstraints.SpoutDiameter / 2;
                    if (arad <= 0) arad = SpoutDiameter / 2;
                    if (arad <= 0) arad = 0.375;
                    aSpoutRadius = arad;
                }

                aclrc = arad;
                if (!bIncludeSpoutRadius) aclrc = 0;

                cnclrc = aEPClearance;
                if (cnclrc <= 0)
                {
                    if (aSpoutGroup.GroupIndex == 1) cnclrc = aConstraints.EndPlateClearance;
                    if (cnclrc <= 0) cnclrc = 0.25;
                }

            }

            _rVal = GetLimLines(aClearance: (aclrc + cnclrc)).First().Line1.Value;
            return _rVal;
        }
        /// <summary>
        /// the collection of spout groups defined for the downcomer
        /// ~a downcomer retrieves its spout group from its parent tray assemblies collection of defined spout groups.
        /// ~see mdTrayAssembly.SpoutGroups
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public colMDSpoutGroups SpoutGroups(mdTrayAssembly aAssy)
        {
            aAssy ??= GetMDTrayAssembly();
            return (aAssy != null) ? aAssy.SpoutGroups.GetByDowncomerIndex(Index, bNonZero: true, bParentsOnly: true) : new colMDSpoutGroups();

        }
        /// <summary>
        /// executed internally to create the spout holes collection for the box
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="bReturnMirroredGroups"></param>
        /// <returns></returns>
        public uopHoleArray Spouts(mdTrayAssembly aAssy, bool bReturnMirroredGroups = true)
        {
            aAssy ??= GetMDTrayAssembly(aAssy); return (aAssy != null) ? new uopHoleArray(mdUtils.GetSpoutGroupsSpouts(SpoutGroups(aAssy), false, false, bReturnMirroredGroups, Thickness, aAssy.Deck.Thickness + How - InsideHeight - 0.5 * SheetMetalThickness)) : new uopHoleArray();

        }


        /// <summary>
        /// the assembly startup spouts that lie on this downcomer
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aSide"></param>
        /// <returns></returns>
        public mdStartupSpouts StartupSpouts(mdTrayAssembly aAssy, string aSide = "")
        {
            aAssy ??= GetMDTrayAssembly(aAssy);

            mdStartupSpouts _rVal = (aAssy != null) ? aAssy.StartupSpouts.GetByDowncomerIndex(Index, 0,aSide) : new mdStartupSpouts();
            _rVal.SetSpoutDowncomerProperties(Boxes.FirstOrDefault(), aAssy);
            return _rVal;
        }


        public mdStiffener Stiffener(double aY = 0, mdTrayAssembly aAssy = null) => new mdStiffener(Boxes.FirstOrDefault(), aY);

        /// <summary>
        /// the collection of stiffeners defined for the downcomer
        /// </summary>
     
        /// <returns></returns>
        public List<mdStiffener> Stiffeners()
        {

            List<mdStiffener> _rVal = new List<mdStiffener>();
            if ( IsGlobal || ProjectType == uppProjectTypes.MDSpout) return _rVal;

            List<mdDowncomerBox> boxes = Boxes.FindAll((x) => !x.IsVirtual); 
            foreach(mdDowncomerBox box in boxes)  _rVal.AddRange(box.Stiffeners()); 
            return _rVal;

        }




       
        public double TotalSpoutArea(mdTrayAssembly aAssy, bool bTrayWide = false) => ComputeSpoutArea(aAssy, bTrayWide);


     

        /// <summary>
        /// the total weir length for the downcomer
        /// ~equal to WeirLength + LongWeirLength
        /// </summary>

        public override void UpdatePartProperties() => UpdatePartProperties(GetMDTrayAssembly());

        public void UpdatePartProperties(mdTrayAssembly aAssy = null)
        {
            bool bIsGlobal = PartIndex <= 0 && IsGlobal;
            if (!bIsGlobal)
            {
                Suppressed = false;
                DescriptiveName = $"Downcomer {Index} (X={string.Format("{0:0.000}", X.ToString())})";
                Quantity = OccuranceFactor;
            }
            else
            {
                Suppressed = true;
            }
            if (Index == 0)
            {
                aAssy ??= GetMDTrayAssembly(aAssy);
                if (aAssy != null)
                {
                    mdStartupSpouts aSUs = aAssy.StartupSpouts;
                    if (aSUs != null)
                    {
                        PropValSet("StartupDiameter", aSUs.Height, bSuppressEvnts: true);
                        PropValSet("StartUpLength", aSUs.Length, bSuppressEvnts: true);
                    }
                }
            }
            PropValSet("Material", Material.Descriptor, bSuppressEvnts: true);
            PropValSet("Thickness", Material.Thickness, bSuppressEvnts: true);

            List<string> hiddemprops = HiddenPropertyNames();

            TPROPERTIES actprops = ActiveProps;

            actprops.SetHidden(hiddemprops, true);
            actprops.SetHidden("TotalArea", true);
            actprops.SetHidden("TotalWeir", true);
            actprops.SetHidden("SpoutArea", true);
            actprops.SetHidden("Thickness", true);


            ActiveProps = actprops;


        }

        public override void UpdatePartWeight()
        {
            base.Weight = Weight(null, null, null, false, out string rINcl);
        }

        /// <summary>
        ///returns a collection of strings that are warnings about possible problems with
        /// ^the current tray assembly design.
        /// ~these warnings may or may not be fatal problems.
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aCategory"></param>
        /// <returns></returns>
        public uopDocuments GenerateWarnings(mdTrayAssembly aAssy, string aCategory = "", uopDocuments aCollector = null, bool bJustOne = false)
        {
            uopDocuments _rVal = aCollector ?? new uopDocuments();
            if (bJustOne && _rVal.Count > 0 || IsVirtual) return _rVal;
            aAssy ??= GetMDTrayAssembly(aAssy);
            if (aAssy == null) return _rVal;

            ULINE epLine;
            int idx = Index;
            string txt;
            double d1;
            double d2;
            string oname = PartPath(true);

            aCategory = string.IsNullOrWhiteSpace(aCategory) ? TrayName(true) : aCategory.Trim();


            if (idx != 0 && !IsGlobal)
            {
                DowncomerInfo myinfo = CurrentInfo();

                // this check is deprecated and no longer eemed necessary for WinTray 4.1
                //foreach (ULINEPAIR item in myinfo.LimLines)
                //{
                //    d1 = myinfo.SmallestEndplateContactDistance(item);
                //    if (d1 < myinfo.MinimumEndPlateEngagement)
                //    {
                //        txt = $"Downcomer {idx} Has a Physical Endplate Engagement That Exceeds The Project Limit of {myinfo.MinimumEndPlateEngagement:0.00} ";
                //        _rVal.AddWarning(this, "Endplate Minimum Inset Warning", txt, uppWarningTypes.ReportFatal, aOwnerName: oname);
                //        if (bJustOne && _rVal.Count > 0) return _rVal;
                //    }
                //}

                epLine = myinfo.LimLines.First().GetSide(uppSides.Top).Value;


                if (Math.Abs(ErrorPercentage(aAssy)) > aAssy.ErrorLimit && aAssy.ProjectType == uppProjectTypes.MDSpout)
                {
                    txt = $"Downcomer {idx} Has a Spout Area Deviation {Math.Abs(ErrorPercentage(aAssy)):0.00}) That Exceeds The Project Limit of {aAssy.ErrorLimit:0.00} ";
                    _rVal.AddWarning(this, "Spout Area Warning", txt, uppWarningTypes.ReportFatal, aOwnerName: oname);
                    if (bJustOne && _rVal.Count > 0) return _rVal;
                }
                if (aAssy.ProjectType == uppProjectTypes.MDDraw)
                {
                    if (MaxStiffenerSpacing > 18)
                    {
                        txt = $"Downcomer {idx} Has Stiffeners That Are Further Apart Than The Assembly Limit of 18 in.";
                        _rVal.AddWarning(this, "Stiffener Spacing Warning", txt, uppWarningTypes.ReportFatal, aOwnerName: oname);
                        if (bJustOne && _rVal.Count > 0) return _rVal;
                    }

                    List<double> svals = mzUtils.ListToNumericCollection(StiffenerSites, ",");


                    if (svals.Count > 0)
                    {
                        svals.Sort(); // low to high

                        d1 = epLine.MidPt.Y;
                        d2 = Math.Round(d1 - svals[svals.Count - 1], 3);

                        if (aAssy.DesignFamily.IsEcmdDesignFamily())
                        {
                            //top of baffle
                            if (d2 < 9) d2 = Math.Round(svals[0] - (-1 * d1), 3);

                            if (d2 > 9)
                            {
                                _rVal.AddWarning(this, "Baffle Mount Warning", "Stiffeners Are Over 9'' From End Of Baffle", uppWarningTypes.General, aOwnerName: oname);
                                if (bJustOne && _rVal.Count > 0) return _rVal;
                            }

                        }
                        else
                        {
                            //bottom of endplate
                            if (d2 < 18) d2 = Math.Round(svals[0] - (-1 * d1), 3);


                            if (d2 > 18)
                            {
                                _rVal.AddWarning(this, "Stiffener Location Warning", "Stiffeners Are Over 18'' From End Plate", uppWarningTypes.General, aOwnerName: oname);
                                if (bJustOne && _rVal.Count > 0) return _rVal;
                            }

                        }

                    }
                    else
                    {
                        _rVal.AddWarning(this, "Stiffener Warning", $"Downcomer {Index} Has No Stiffeners", uppWarningTypes.General, aOwnerName: oname);
                        if (bJustOne && _rVal.Count > 0) return _rVal;
                    }
                }
            }
            else
            {
                if (aAssy.ProjectType == uppProjectTypes.MDDraw)
                {
                    if (aAssy.DesignFamily.IsEcmdDesignFamily() && IsGlobal)
                    {
                        if (aAssy.DesignOptions.CDP > 0)
                        {
                            d1 = aAssy.Downcomer().Stiffener().TopZ;
                            d2 = aAssy.RingSpacing - (HeightBelowDeck + 0.5);
                            if (d1 > d2)
                            {
                                _rVal.AddWarning(this, "Baffle Mount Warning", "Stiffener Baffle Mounting Flanges MIGHT Extend Into Baffle Downcomer Cutouts. Check Downcomer Assemblies!", uppWarningTypes.General, aOwnerName: oname);
                                if (bJustOne && _rVal.Count > 0) return _rVal;
                            }

                        }
                    }
                }
            }
            return _rVal;
        }
        public override uopDocuments Warnings() => GenerateWarnings(GetMDTrayAssembly(), "", null);


        
        /// <summary>
        ///returns the weight of the downcomer in english pounds
        /// ~includes end plates, end supports, shelf angles, box.
        /// ~if the project type is not MD Spout then stiffeners,supplemental deflectors, AP Pans and baffles are included
        /// ~all holes and spouts are accounted for.
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aStiffeners"></param>
        /// <param name="aAssyBaffles"></param>
        /// <param name="bSuppressBoltOns"></param>
        /// <param name="rIncludesList"></param>
        /// <returns></returns>
        public new double Weight(mdTrayAssembly aAssy, colUOPParts aStiffeners = null, List<mdBaffle> aAssyBaffles = null, bool bSuppressBoltOns = false)
        {
            return Weight(aAssy, aStiffeners, aAssyBaffles, bSuppressBoltOns, out string rIncludesList);
        }

        /// <summary>
        ///returns the weight of the downcomer in english pounds
        /// ~includes end plates, end supports, shelf angles, box.
        /// ~if the project type is not MD Spout then stiffeners,supplemental deflectors, AP Pans and baffles are included
        /// ~all holes and spouts are accounted for.
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aStiffeners"></param>
        /// <param name="aAssyBaffles"></param>
        /// <param name="bSuppressBoltOns"></param>
        /// <param name="rIncludesList"></param>
        /// <returns></returns>
        public new double Weight(mdTrayAssembly aAssy, colUOPParts aStiffeners, List<mdBaffle> aAssyBaffles, bool bSuppressBoltOns, out string rIncludesList)
        {
            double _rVal = 0;
            double wt = 0;
       
            int i = 0;
            uppProjectTypes pType = new uppProjectTypes();
            rIncludesList = string.Empty;
            aAssy ??= GetMDTrayAssembly(aAssy);
            pType = (aAssy != null) ? aAssy.ProjectType : uppProjectTypes.MDSpout;

            rIncludesList = string.IsNullOrEmpty(rIncludesList) ? "Box,Shelf Angles,End Plates,End Supports" : rIncludesList.Trim();

            List<mdDowncomerBox> boxes = Boxes;

            foreach (mdDowncomerBox box in boxes)
            {
                wt = box.Weight(this, aAssy);
                _rVal += wt;
            }

            if (pType != uppProjectTypes.MDSpout)
            {
                List<mdBaffle> aBafs = null;
                mdBaffle aBaf = null;
                
                
                
                if (!bSuppressBoltOns)
                {
                    if (aAssy.DesignFamily.IsEcmdDesignFamily())
                    {
                        if (aAssyBaffles == null)
                        {
                            aBafs = DeflectorPlates(aAssy);
                        }
                        else
                        {
                            aBafs = aAssyBaffles.FindAll(x => x.DowncomerIndex == Index);
                        }
                        if (aBafs.Count > 0)
                        {
                            mzUtils.ListAdd(ref rIncludesList, "Baffles");
                            for (i = 0; i < aBafs.Count; i++)
                            {
                                aBaf = aBafs[i];
                                if (aBaf.IsAssociatedToRange(RangeGUID))
                                {
                                    wt = aBaf.Weight(aAssy);
                                    _rVal += wt;
                                }
                            }
                        }
                    }
                    List<mdAPPan> aAPs = APPans(aAssy);


                    if (aAPs.Count > 0)
                    {
                        mzUtils.ListAdd(ref rIncludesList, "AP Pans");
                        foreach (mdAPPan pan in aAPs)
                        {
                            _rVal += pan.Weight;
                        }

                    }
                }
            }
            return _rVal;
        }

        public override bool IsEqual(uopPart aPart)
        {
            if (aPart == null) return false;
            if (aPart.PartType != PartType) return false;
            return IsEqual((mdDowncomer)aPart);

        }

        /// <summary>
        /// returns True if the passed downcomer has the same properties as the dowcomer object
        /// </summary>
        /// <param name="aDowncomer"></param>
        /// <returns></returns>
        public bool IsEqual(mdDowncomer aDowncomer)
        {
            if (aDowncomer == null) return false;
            if (aDowncomer.IsGlobal != IsGlobal) return false;
            if (!aDowncomer.CurrentProperties().IsEqual(CurrentProperties())) return false;
            return true;
        }

        public List<double> StartupOrdinates(bool bLeft) => bLeft ? mzUtils.ListToNumericCollection(StartUpSitesL) : mzUtils.ListToNumericCollection(StartUpSitesR);

        // <summary>
        // sets the parts property with the passed name to the passed value
        //returns the property if the property value actually changes.
        // </summary>
        public override uopProperty PropValSet(string aName, object aPropVal, int aOccur = 0, bool? bSuppressEvnts = null, bool? bHiddenVal = null)
        {

            bool supevnts = bSuppressEvnts.HasValue ? bSuppressEvnts.Value : SuppressEvents || Reading;
            uopProperty _rVal = base.PropValSet(aName, aPropVal, aOccur, supevnts, bHiddenVal);
            if (_rVal != null)
            {
                string pname = _rVal.Name.ToUpper();
            }
           
            Notify(_rVal, supevnts);
            return _rVal;
        }

        /// <summary>
        /// returns the weir length of the downcomers box(s).
        /// </summary>
        /// <param name="aSide"> if passed and is left or right, only the weir length of the indicated side is returned</param>
        /// <returns></returns>
        public double WeirLength(uppSides aSide = uppSides.Undefined, bool bTrayWide = false)
        {

            List<mdDowncomerBox> boxes = Boxes.FindAll((x) => !x.IsVirtual);
            double _rVal = 0;
            foreach (var box in boxes) _rVal += box.WeirLength(aSide, bTrayWide);
           
            return _rVal;


        }



        public mdDowncomerBox FindDowncomerBoxContainingStiffenerOrdinate(double y, bool omitVirtualBoxes = true)
        {
            IEnumerable<mdDowncomerBox> boxes;
            if (omitVirtualBoxes)
            {
                boxes = Boxes.Where(b => !b.IsVirtual);
            }
            else
            {
                boxes = Boxes;
            }

            mdDowncomerBox box = boxes.FirstOrDefault(b => b.IsValidStiffenerOrdinate(y));

            return box;
        }
        /// <summary>
        /// For the selected stiffener ordinate, this method finds the box with the minimum distance to the stiffener ordinate.
        /// </summary>
        /// <param name="y"></param>
        /// <param name="omitVirtualBoxes"></param>
        /// <returns></returns>
        public mdDowncomerBox FindClosestBoxToStiffenerOrdinate(double y, bool omitVirtualBoxes = true)
        {
            IEnumerable<mdDowncomerBox> boxes;
            if (omitVirtualBoxes)
            {
                boxes = Boxes.Where(b => !b.IsVirtual);
            }
            else
            {
                boxes = Boxes;
            }

            mdDowncomerBox nearestBox = null;
            double minDistance = double.MaxValue;

            foreach (mdDowncomerBox box in boxes)
            {
                double distance = box.MinimumStiffenerDistanceFromBox(y);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestBox = box;
                }
            }

            return nearestBox;
        }
        #endregion Methods


    }
}