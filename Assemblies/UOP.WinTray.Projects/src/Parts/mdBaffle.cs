using UOP.DXFGraphics;
using System.Collections.Generic;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;

using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;
using System.Linq;
using System;
using UOP.WinTray.Projects.Interfaces;
using UOP.WinTray.Projects.Constants;

namespace UOP.WinTray.Projects.Parts
{
    /// <summary>
    /// the deflector plates of a ECMD tray downcomer assembly
    /// </summary>
    public class mdBaffle : mdBoxSubPart
    {
        public override uppPartTypes BasePartType => uppPartTypes.Deflector;

        #region Constructors
        public mdBaffle() : base(uppPartTypes.Deflector)
        { Initialize(); }


        internal mdBaffle(mdBaffle aPartToCopy) : base(uppPartTypes.Deflector)
        {

            ProjectType = uppProjectTypes.MDDraw;
            base.Copy(aPartToCopy);
            Initialize(aPartToCopy);
        }
        internal mdBaffle(mdDowncomerBox aBox,  double? aHeight = null, double? aZ = null, double? aZLim= null, mzValues aDCCenters = null) : base(uppPartTypes.Deflector)
        {
            Initialize(null, aBox);

            if(aHeight.HasValue) Height = aHeight.Value;
            if(aZ.HasValue) Z = aZ.Value;
            if(aZLim.HasValue) ZLimit = aZLim.Value;


        }
        private void Initialize(mdBaffle aPartToCopy = null, mdDowncomerBox aBox = null)
        {
            Height = 1;
            BaffleMountSlot = mdGlobals.BaffleMountSlot(null);
            DCCenters = new TVALUES();
            ZLimit = 0;
            Length = 0;
            SplicedOnBottom = false;
            SplicedOnTop = false;
            _Limits = URECTANGLE.Null;
            DowncomerBoxWidth = 0;
            _Limits.Top = 5;
            _Limits.Bottom = -5;
            X = 0;
            Y = 0;
            Z = 0;
            DistributorIndex = 0;
            IsBlank = false;
            ParentPartType = uppPartTypes.Downcomer;
            BoxIndex = 0;
            _StiffenerYs = null;

            if (aPartToCopy != null)
            {
                base.Copy(aPartToCopy);
                X = aPartToCopy.X;
                Y = aPartToCopy.Y;
                Z = aPartToCopy.Z;
                Height = aPartToCopy.Height;
                DCCenters = new TVALUES(aPartToCopy.DCCenters);
                ZLimit = aPartToCopy.ZLimit;
                Length = aPartToCopy.Length;
                SplicedOnBottom = aPartToCopy.SplicedOnBottom;
                SplicedOnTop = aPartToCopy.SplicedOnTop;
                Limits = new URECTANGLE(aPartToCopy.Limits);
                DowncomerBoxWidth = aPartToCopy.DowncomerBoxWidth;
                BoxIndex = aPartToCopy.BoxIndex;
                BaffleMountSlot = new UHOLE(aPartToCopy.BaffleMountSlot);
                ParentPartType = aPartToCopy.ParentPartType;
                DistributorIndex = aPartToCopy.DistributorIndex;
                IsBlank = aPartToCopy.IsBlank;
                if(aPartToCopy._StiffenerYs != null)  _StiffenerYs = new List<double>(aPartToCopy._StiffenerYs);
                aBox ??= aPartToCopy.DowncomerBox;
            }

            if (aBox == null) return;
         
                    base.MDDowncomer = aBox.Downcomer;
            SheetMetal = aBox.SheetMetal;
            SubPart(aBox);
            X = aBox.X;
            Y = aBox.Y;
            Z = aBox.WeirHeight;
            Row = aBox.Row;
            Col = aBox.Col;
            IsVirtual = aBox.IsVirtual;
            OccuranceFactor = aBox.OccuranceFactor;
            ZLimit = aBox.RingSpacing - aBox.HeightBelowDeck - 0.5 - Z;
            DowncomerIndex = aBox.DowncomerIndex;
            RangeIndex = aBox.RangeIndex;
            ParentList = aBox.PartNumber;
            _Limits.Top = aBox.LimitLn(true).MidPt.Y;
            _Limits.Bottom = aBox.LimitLn(false).MidPt.Y;
            DowncomerBoxWidth = aBox.Width;
            ParentPartType = aBox.PartType;
            BaffleMountSlot = new UHOLE(aBox.BaffleMountSlot);
             mdTrayAssembly aAssy = aBox.GetMDTrayAssembly();
            Height = aBox.BaffleHeight;
            _StiffenerYs = new List<double>(aBox._StiffenerYs);
            if (aAssy != null)
            {
                SubPart(aAssy);
                ZLimit = aAssy.RingSpacing - aBox.HeightBelowDeck - 0.5 - Z;
                BaffleMountSlot = mdGlobals.BaffleMountSlot(aAssy);

            }
            

     
            
            
            //TotalQuantity = aDC.OccuranceFactor * aAssy.TrayCount,
            
            
            Quantity = OccuranceFactor;
            SubPart(aBox);

        }

        internal override void Initialize(mdBoxSubPart aPartToCopy, mdDowncomerBox aBox)
        {
            mdBaffle copy = null;
            if(aPartToCopy != null && aPartToCopy.GetType() == typeof(mdBaffle)) copy = (mdBaffle)aPartToCopy;
            Initialize(copy,aBox);
        }


        #endregion Constructors

        #region Properties

        internal UHOLE BaffleMountSlot { get; set; }

        internal TVALUES DCCenters { get; set; }

        public bool ForDistributor => DistributorIndex > 0;

        public bool IsBlank { get; set; }

        public double DowncomerBoxWidth { get; set; }


        private URECTANGLE _Limits;
        internal URECTANGLE Limits { get => _Limits; set { _Limits = value; Y = _Limits.Bottom + 0.5 * _Limits.Height; } }

   
        public double Bottom { get => _Limits.Bottom; set { _Limits.Bottom = value; _Limits.Rectify(); Y = _Limits.Bottom + 0.5 * _Limits.Height; } }
        public double Top { get => _Limits.Top; set { _Limits.Top = value; _Limits.Rectify(); Y = _Limits.Bottom + 0.5 * _Limits.Height; } }

        /// <summary>
        /// the bolt used to install the part
        /// </summary>
        public hdwHexBolt Bolt { get => base.SmallBolt("Deflector Plate Bolt", "Deflector Attachment"); }


        public override uopProperties CurrentProperties() { UpdatePartProperties(); return base.CurrentProperties(); }

        public bool SplicedOnBottom { get; set; }

        public bool SplicedOnTop { get; set; }

        /// <summary>
        /// the length of the baffle
        /// </summary>

        public override double Length { get => _Limits.Height; }


        /// <summary>
        /// the lock washer used to install the part
        /// </summary>
        public hdwLockWasher LockWasher => Bolt.GetLockWasher();


        /// <summary>
        /// the nut used to install the part
        /// </summary>
        public hdwHexNut Nut => Bolt.GetNut();


        /// <summary>
        /// the flat washer used to install the part
        /// </summary>
        public hdwFlatWasher Washer => Bolt.GetWasher();


        ///// <summary>
        ///// the Y coordinate of the center of the part
        ///// </summary>
        //public override double Y => Bottom + (0.5 * Length);


        /// <summary>
        /// the Z coordinate of the bottom of the downcomer above
        /// </summary>
        public double ZLimit { get; set; }

        public override uopInstances Instances { get => base.Instances; set { base.Instances = value; Quantity = value == null ? 1 : value.Count + 1; } }

      
        #endregion Properties

        #region Methods

        public  void SubPart(mdTrayAssembly aAssy, string aCategory = null, bool? bHidden = null)
        {

            base.SubPart(aAssy, aCategory, bHidden);
            if (aAssy != null)
            {
                
                Height = aAssy.BaffleHeight;
                RangeIndex = aAssy.RangeIndex;
                DCCenters = new TVALUES(aAssy.Downcomers.XValues(true));

            }

        }

        public void AssociateToDowncomer(mdDowncomerBox aBox)
        {
            if (aBox == null) return;
                    SubPart(aBox);
            base.AssociateToParent(aBox.PartNumber, bClear: true);
            ParentPartType = aBox.PartType;
            
            X = aBox.X;
            //uopVectors instPts = new uopVectors();
            //instPts.Add(X, Y, aValue: 0);
            //if (aDC.OccuranceFactor > 1) instPts.Add(-X, -Y, aValue: 180);
            //Instances = uopInstances.FromVectors(instPts, instPts.Item(1));
            Quantity = Instances.Count + 1;

        }
        public void AssociateToDowncomerBox(mdDowncomerBox aDCBox)
        {
            if (aDCBox == null) return;
            SubPart(aDCBox);
            base.AssociateToParent(aDCBox.PartNumber, bClear: true);
            ParentPartType = aDCBox.PartType;
            X = aDCBox.X;
            BoxIndex = aDCBox.Index;
            DowncomerIndex = aDCBox.DowncomerIndex;
            //uopVectors instPts = new uopVectors();
            //instPts.Add(X, Y, aValue: 0);
            //if (aDC.OccuranceFactor > 1) instPts.Add(-X, -Y, aValue: 180);
            //Instances = uopInstances.FromVectors(instPts, instPts.Item(1));
            Quantity = Instances.Count + 1;

        }

        public colDXFVectors DefiningVectors(iVector aCenter = null, double aRotation = 0) => mdPolygons.DeflectorPlateVerts(this, out _,bLeftSide: false, aCenter: aCenter, aRotation: aRotation, bIncludeHoleCenters: true);


        /// <summary>
        /// executed internally to create the holes collection for the part
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="bJustOne"></param>
        /// <returns></returns>
        public uopHoleArray GenHoles(mdTrayAssembly aAssy, bool bJustOne = false) => new uopHoleArray(GenHolesV(aAssy, bJustOne));



        public override string ToString() { return $"DEFLECTOR PLATE({PartNumber})"; }

        public List<double> GetOffsets(bool bHoles)
        {
            List<double> _rVal = new List<double>();
            List<double> yvals = !bHoles  ? DCCenters.ToNumericList() : GenHolesV(null).GetOrdinates(dxxOrdinateDescriptors.Y);
            double cy = Y;
            foreach (var y1 in yvals)
            {
                _rVal.Add(System.Math.Abs(y1 - cy));
            }
            return _rVal;
        }

        public override bool IsEqual(uopPart aPart)
        {

            if (aPart == null) return false;
            if (aPart.PartType != PartType) return false;
            return CompareTo((mdBaffle)aPart, true);

        }

        public bool CompareTo(mdBaffle aBaffle, bool bCompareMaterial)
        {

            if (aBaffle == null) return false;
            if (aBaffle.ForDistributor || ForDistributor) return false;
            if (aBaffle.IsBlank != IsBlank) return false;

            if (!TVALUES.CompareNumeric(aBaffle.Length, Length, 3)) return false;
            if (!TVALUES.CompareNumeric(aBaffle.Height, Height, 3)) return false;
            if (aBaffle.DCCenters.Count != DCCenters.Count) return false;
        
            if (DCCenters.Count > 0)
            {
                if (!TVALUES.CompareNumeric(aBaffle.DowncomerBoxWidth, DowncomerBoxWidth, 3)) return false;

            }


            if (bCompareMaterial)
            {
                if (!aBaffle.Material.IsEqual(Material)) return false;

            }
            if (!mzUtils.CompareNumericList(aBaffle.GetOffsets(true), GetOffsets(true), 3)) return false;
            if (!mzUtils.CompareNumericList(aBaffle.GetOffsets(false), GetOffsets(false), 3))
                return false;
            return true; // colDXFVectors.MatchPlanar(this.DefiningVectors(), aBaffle.DefiningVectors(), null, 3);

            //bool _rVal = colDXFVectors.MatchPlanar(this.DefiningVectors(), aBaffle.DefiningVectors(), null,3);

            //UVECTORS aVerts = mdPolygons.DeflectorPlateVertices(this, bLongSide:false, aCenter:dxfVector.Zero,aRotation: 0, bIncludeHoleCenters: true);

            //UVECTORS bVerts = mdPolygons.DeflectorPlateVertices(aBaffle, bLongSide: false, aCenter: dxfVector.Zero, aRotation: 0, bIncludeHoleCenters: true);

            //_rVal = UVECTORS.MatchPlanar(aVerts, bVerts, 3);
            //if (!_rVal)
            //{
            //    bVerts = mdPolygons.DeflectorPlateVertices(aBaffle, bLongSide: true, aCenter: dxfVector.Zero, aRotation: 0, bIncludeHoleCenters: true);
            //    _rVal = UVECTORS.Match(aVerts, bVerts, 3);
            //}


            //return _rVal;
        }


        /// <summary>
        ///returns a new object that is an exact copy of this one
        /// </summary>
        /// <returns></returns>
        public mdBaffle Clone() => new mdBaffle(this);


        /// <summary>
        ///returns a new object that is an exact copy of this one
        /// </summary>
        /// <returns></returns>
        public mdBaffle Copy(bool? bIsBlank = null, int? aDistribIdx = null, bool? bIsVirtual = null)
        {
            mdBaffle _rVal = new mdBaffle(this);
            _rVal.SubPart(this);
            if (bIsBlank.HasValue) _rVal.IsBlank = bIsBlank.Value;
            if (aDistribIdx.HasValue) _rVal.DistributorIndex = aDistribIdx.Value;
            if (bIsVirtual.HasValue) _rVal.IsVirtual = bIsVirtual.Value;
            _rVal.Instances_Set(new TINSTANCES(Instances));
            return _rVal;

        }

        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();

        /// <summary>
        /// executed internally to create the holes collection for the part
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="bJustOne"></param>
        /// <returns></returns>
        internal UHOLEARRAY GenHolesV(mdTrayAssembly aAssy, bool bJustOne = false)
        {
            UHOLEARRAY _rVal =UHOLEARRAY.Null;

            mdDowncomerBox box = DowncomerBox;


            if (box == null || Height <= 0) return _rVal;
            aAssy ??= box.GetMDTrayAssembly();
            if (aAssy != null && !aAssy.DesignFamily.IsEcmdDesignFamily()) return _rVal;

            SubPart(box);

            UHOLES aHls = new UHOLES("BAFFLE MOUNT");
            if (aAssy != null) BaffleMountSlot = mdGlobals.BaffleMountSlot(aAssy, Thickness);
            UHOLE slot = new UHOLE(BaffleMountSlot);
            slot.X = X;
            slot.Depth = Thickness;
            slot.Tag = "BAFFLE MOUNT";
            aHls.Member = slot;

            
            double y1 = Limits.Bottom;
            double y2 = Limits.Top;
            double x = box.X;
            List<double> yvals = StiffenerYs(box);
            foreach (double yval in yvals)
             {
                if (yval < y2 && yval > y1)
                {
                    aHls.Centers.Add(x,yval  , aValue: 0);
                    if (bJustOne) break;
                }
            }
            //slot.Elevation = Holes.Item(1).Value;



            //for (int i = 1; i <= Holes.Count; i++)
            //{
            //    u1 = Holes.Item(i);
            //    if (u1.Y < Limits.Top && u1.Y > Limits.Bottom)
            //    {
            //        if (u1.Y > y1) y1 = u1.Y;
            //        if (u1.Y < y2) y2 = u1.Y;
            //        aHls.Centers.Add(u1, aValue: 0);
            //        if (bJustOne) break;
            //    }
            //}
            _rVal.Add(aHls, "BAFFLE MOUNT");

            if (!bJustOne)
            {
                aHls.Centers.Clear();
                aHls.Member.Tag = "SPLICE";

                if (SplicedOnTop && (y1 != Limits.Top - slot.DownSet))
                    aHls.Centers.Add(new UVECTOR(0, Limits.Top - slot.DownSet), aValue: 0);

                if (SplicedOnBottom && (y2 != Limits.Bottom + slot.DownSet))
                    aHls.Centers.Add(new UVECTOR(0, Limits.Bottom + slot.DownSet), aValue: 0);

                if (aHls.Centers.Count > 0) _rVal.Add(aHls, "SPLICE");

            }
            return _rVal;
        }

        public override uopHoleArray HoleArray(uopTrayAssembly aAssy = null, string aTag = null)
        => GenHoles(aAssy != null && aAssy.ProjectFamily == uppProjectFamilies.uopFamMD ? (mdTrayAssembly)aAssy: null);
        

        public override void UpdatePartProperties()
        {

            //Quantity = OccuranceFactor;
            DescriptiveName = $"Deflector Plate (DC {DowncomerIndex})";
        }

        public override void UpdatePartWeight() => base.Weight = Weight();

        /// <summary>
        ///returns a dxePolygon that is used to draw the elevation view of the baffle
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public dxePolygon View_Elevation(mdTrayAssembly aAssy, iVector aCenter, double aRotation, string aLayerName)
      => mdPolygons.Baffle_View_Elevation(this, aAssy, aCenter, aRotation, aLayerName);


        /// <summary>
        ///returns a dxePolygon that is used to draw the layout view of the angle
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="bSuppressHoles"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public dxePolygon View_Plan(mdTrayAssembly aAssy, bool bSuppressHoles, iVector aCenter, double aRotation, string aLayerName)
         => mdPolygons.Baffle_View_Plan(this, aAssy, bSuppressHoles, aCenter, aRotation, aLayerName);


        public dxePolygon View_Profile(bool bLongSide = false, bool bHiddenSot = false, iVector aCenter = null, double aRotation = 0, string aLayerName = "", bool bSuppressHoles = false, bool bAddSlotPoints = false, bool aCenterElevationToMounts = false, bool bOmmitNotches = false)
        => mdPolygons.Baffle_View_Profile(this, bLongSide, bHiddenSot, aCenter, aRotation, aLayerName, bSuppressHoles, bAddSlotPoints, aCenterElevationToMounts, bOmmitNotches);

        /// <summary>
        ///returns the points that are used to determine equality beteen baffles
        /// </summary>
        /// <returns></returns>
        public colDXFVectors Vertices() => mdPolygons.DeflectorPlateVerts(this, out _, bIncludeHoleCenters: false);


        /// <summary>
        ///returns the weight of the part in english pounds
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public new double Weight(mdTrayAssembly aAssy = null)
        {

            double sArea = View_Profile(bSuppressHoles: true).Area;
            sArea -= GenHolesV(aAssy).TotalArea();
            return sArea * SheetMetalWeightMultiplier;
        }

        #endregion Methods

        #region Shared Methods

        /// <summary>
        /// the length limit of the baffle
        /// ~120 inches by default
        /// </summary>
        public static double LengthLimit => 120;

        public static List<mdBaffle> DivideBaffles_CenterOut( mdDowncomerBox aBox, mdBaffle aBaffle, mdTrayAssembly aAssy = null, bool bSuppressInstances = false)
        {
       
            List<mdBaffle > _rVal = new List<mdBaffle> ();
            if (aBox == null)
            {
                if(aBaffle != null)
                    aBox ??= aBaffle.DowncomerBox;
                if (aBox == null)
                {
                    if(aBaffle != null) _rVal.Add(aBaffle);
                    return _rVal;
                }
            }
            aAssy ??= aBox.GetMDTrayAssembly(aAssy);

            if (aAssy == null) return _rVal;
            if (aAssy.ProjectType == uppProjectTypes.MDSpout) return _rVal;
            aBaffle ??= new mdBaffle(aBox);
       
                    //divide the baffles
            mdBaffle aBaf = null;
            mdBaffle bBaf = null;
            bool bsnap = false;
            List<double> stfvals;
            double lenlim = mdBaffle.LengthLimit;
            List<double> yvals = aBox.StiffenerYs();
            double lg = aBaffle.Length;
            double y1;
            double y2;


            try
            {

                yvals.Sort();

                if (Math.Round(lg / 2, 2) < Math.Round(lenlim, 2))
                {
                    //Segs = 2;
                    // the bottom half
                    aBaf = new mdBaffle(aBaffle) { Top = aBaffle.Bottom + 0.5 * lg + 1, SplicedOnBottom = false, SplicedOnTop = true };
                    aBaf.AssociateToDowncomer(aBox);
                    _rVal.Add(aBaf);

                    // the top half
                    bBaf = new mdBaffle(aBaffle) { Bottom = aBaf.Top - 2, SplicedOnBottom = true, SplicedOnTop = false };

                    if (!bSuppressInstances && bBaf.CompareTo(aBaf, false))
                    {
                        aBaf.Instances.Add(0, bBaf.Y - aBaf.Y, bInverted: true);
                    }
                    else
                    {

                        bBaf.AssociateToDowncomer(aBox);
                        //if (aDowncomer.OccuranceFactor == 2) bBaf.Instances.Add(-2 * aDowncomer.X, 0, bInverted: true);
                        _rVal.Add(bBaf);
                    }



                }
                else if (Math.Round(lg / 3, 2) < Math.Round(lenlim, 2))
                {
                    //Segs = 3;
                    y1 = aBaffle.Y - 0.5 * (lenlim - 2);
                    y2 = aBaffle.Y + 0.5 * (lenlim - 2);


                    stfvals = yvals.FindAll(x => x > y1 && x < y2); // mzUtils.ValuesBetween(yvals, y1, y2);

                    bsnap = false;
                    if (stfvals.Count > 1)
                    {

                        stfvals.Sort();
                        bsnap = stfvals[0] + 1 - aBaffle.Bottom <= lenlim;

                        if (bsnap)
                        {
                            y1 = stfvals[0] - 1;
                            y2 = stfvals[stfvals.Count - 1] + 1;
                        }


                    }
                    // the center third
                    mdBaffle cBaf = new mdBaffle(aBaffle)
                    {
                        Top = y2,
                        Bottom = y1,
                        SplicedOnBottom = !bsnap,
                        SplicedOnTop = !bsnap,
                    };
                    cBaf.AssociateToDowncomer(aBox);
                    _rVal.Add(cBaf);

                    // the botton third
                    bBaf = new mdBaffle(aBaffle)
                    {
                        Top = cBaf.Bottom + 2,
                        SplicedOnBottom = false,
                        SplicedOnTop = cBaf.SplicedOnBottom,
                    };

                    bBaf.AssociateToDowncomer(aBox);
                    _rVal.Add(bBaf);


                    // the top third
                    aBaf = new mdBaffle(aBaffle)
                    {
                        Bottom = cBaf.Top - 2,
                        SplicedOnBottom = false,
                        SplicedOnTop = cBaf.SplicedOnBottom,
                    };



                    if (!bSuppressInstances && aBaf.CompareTo(bBaf, false))
                    {
                        bBaf.Instances.Add(0, aBaf.Y - bBaf.Y, bInverted: true);
                    }
                    else
                    {

                        aBaf.AssociateToDowncomer(aBox);
                        _rVal.Add(aBaf);
                    }

                    // Console.WriteLine(bBaf.CompareTo(cBaf, false).ToString());

                }
                else
                {
                    //Segs = 4;

                    y1 = aBaffle.Y - (lenlim - 1);
                    y2 = aBaffle.Y + 1;

                    stfvals = mzUtils.ValuesBetween(yvals, y1, y2);
                    bsnap = false;
                    if (stfvals.Count > 1)
                    {
                        stfvals.Sort();

                        bsnap = stfvals[0] + 1 - aBaffle.Bottom <= lenlim;


                        if (bsnap)
                        {
                            y1 = stfvals[0] - 1;
                            y2 = stfvals[stfvals.Count - 1] + 1;
                        }

                    }
                    //the bottom center
                    aBaf = new mdBaffle(aBaffle) { Top = y2, Bottom = y1, SplicedOnBottom = !bsnap, SplicedOnTop = !bsnap };
                    aBaf.AssociateToDowncomer(aBox);

                    //the bottom bottom
                    bBaf = new mdBaffle(aBaffle) { Top = aBaf.Bottom + 2, SplicedOnBottom = false, SplicedOnTop = aBaf.SplicedOnBottom };
                    _rVal.Add(bBaf);
                    _rVal.Add(aBaf);


                    //bBaf.AssociateToDowncomer(aDowncomer);


                    y1 = aBaffle.Y - 1;
                    y2 = aBaffle.Y + (lenlim - 1);

                    stfvals = mzUtils.ValuesBetween(yvals, y1, y2);
                    bsnap = false;
                    if (stfvals.Count > 1)
                    {
                        stfvals.Sort();

                        bsnap = aBaffle.Top - (stfvals[stfvals.Count - 1] + 1) <= lenlim;


                        if (bsnap)
                        {
                            y1 = stfvals[0] - 1;
                            y2 = stfvals[stfvals.Count - 1] + 1;
                        }

                    }

                    // the top center
                    mdBaffle cBaf = new mdBaffle(aBaffle) { Top = y2, Bottom = y1, SplicedOnBottom = !bsnap, SplicedOnTop = !bsnap };

                    if (!bSuppressInstances && cBaf.CompareTo(aBaf, false))
                    {

                        aBaf.Instances.Add(0, cBaf.Y - aBaf.Y, bInverted: true);
                    }
                    else
                    {

                        cBaf.AssociateToDowncomer(aBox);
                        _rVal.Add(cBaf);
                    }

                    // the top top
                    mdBaffle dBaf = new mdBaffle(aBaffle) { Bottom = aBaf.Top - 2, SplicedOnTop = false, SplicedOnBottom = cBaf.SplicedOnTop };
                    if (!bSuppressInstances && dBaf.CompareTo(bBaf, false))
                    {

                        bBaf.Instances.Add(0, dBaf.Y - bBaf.Y, bInverted: true);

                    }
                    else
                    {

                        dBaf.AssociateToDowncomer(aBox);

                        _rVal.Add(dBaf);
                    }

                }
            }
            catch  { _rVal.Clear();  if (aBaffle != null) _rVal.Add(aBaffle);  }

            return _rVal;
        }

        internal List<double> _StiffenerYs = null;
        public List<double> StiffenerYs(mdDowncomerBox aDowncomerBox = null)
        {
            if (_StiffenerYs == null)
            {
                aDowncomerBox ??= DowncomerBox;
                if(aDowncomerBox == null) return new List<double>();
                _StiffenerYs = new List<double>(aDowncomerBox.StiffenerYs());  
            }

            return _StiffenerYs;
        }

     

        #endregion Shared Methods
    }
}