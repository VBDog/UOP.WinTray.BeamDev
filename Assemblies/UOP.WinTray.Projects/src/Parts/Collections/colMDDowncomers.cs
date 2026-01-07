using UOP.DXFGraphics;
using System;
using System.Collections.Generic;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
//using Microsoft.VisualBasic.PowerPacks;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Materials;

using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.src.Utilities.ExtensionMethods;
using System.Linq;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;
using System.Data;
using System.Collections.Specialized;

namespace UOP.WinTray.Projects.Parts
{
    /// <summary>
    /// a collection of mdDowncomer objects
    /// </summary>
    public class colMDDowncomers : uopParts, IEnumerable<mdDowncomer>
    {

        public override uppPartTypes BasePartType => uppPartTypes.Downcomers;
        private bool _Initializing = false;



        public delegate void SpacingChangedHandler();
        public event SpacingChangedHandler eventSpacingChanged;
        public delegate void SpacingOptimizationBeginHandler();
        public event SpacingOptimizationBeginHandler eventSpacingOptimizationBegin;
        public delegate void SpacingOptimizationEndHandler(double StartSpace, double EndSpace);
        public event SpacingOptimizationEndHandler eventSpacingOptimizationEnd;
        public delegate void DowncomerMemberChangedHandler(uopProperty aProperty, bool isGlobalDowncomer = true);
        public event DowncomerMemberChangedHandler eventDowncomerMemberChanged;
        public delegate void DowncomersInvalidatedHandler();
        public event DowncomersInvalidatedHandler eventDowncomersInvalidated;


        #region Constructors

        public colMDDowncomers() : base(uppPartTypes.Downcomers, uppProjectFamilies.uopFamMD, bBaseOne: true, bMaintainIndices: true)
        {
            InitializeProperties();
        }

        internal colMDDowncomers(colMDDowncomers aPartToCopy, uopPart aParent = null, bool bDontCopyMembers = false) : base(aPartToCopy, bDontCopyMembers, aParent)
        {
            if (base.PropertyCount() <= 0) InitializeProperties();
        }

        private void InitializeProperties()
        {


            AddProperty("OptimumSpacing", 0, uppUnitTypes.SmallLength, bProtected: true);
            AddProperty("OverrideSpacing", 0, uppUnitTypes.SmallLength, aNullVal: 0);
            AddProperty("SpacingDeviation", 0, uppUnitTypes.SmallLength);
            AddProperty("WeightedSpacingDeviation", 0, uppUnitTypes.SmallLength);
            AddProperty("WeightedAverage", 0, uppUnitTypes.SmallLength);
            AddProperty("AverageRatio", 0, uppUnitTypes.SmallLength);
            AddProperty("Ratios", "");
            AddProperty("FBAs", "");
            AddProperty("WeirLengths", "");
            AddProperty("Offset", 0, uppUnitTypes.SmallLength);

        }

        #endregion Constructors

        #region IEnumerable Implementation

        public new IEnumerator<mdDowncomer> GetEnumerator() { foreach (uopPart part in Members) { yield return (mdDowncomer)part; } }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion IEnumerable Implementation

        #region Properties
        public override uppProjectTypes ProjectType
        {
            get => base.ProjectType;
            set
            {
                base.ProjectType = value;
                if (_SpacingData != null) _SpacingData.ProjectType = value;
            }
        }
        double CurrentSpacing
        {
            get
            {
                int cnt = Count;
                if (cnt <= 0) return 0;
                if (cnt == 1) return Item(1).X;
                return Math.Abs(Item(2).X - Item(1).X);

            }
        }

        /// <summary>
        /// a comma delimited string of the the FBA values
        /// </summary>
        public string FBAs { get => PropValS("FBAs"); set => PropValSet("FBAs", value, bSuppressEvnts: true); }
        /// <summary>
        /// returns true if the collection has a downcomer with gusseted end plates
        /// </summary>
        public bool HasGussetedEndPlates
        {
            get
            {
                mdDowncomer aMem;
                for (int i = 1; i <= Count; i++)
                {
                    aMem = Item(i);
                    if (aMem.IsVirtual) continue;
                    if (aMem.GussetedEndplates) return true;
                }

                return false;
            }
        }
        /// <summary>
        /// returns true if the collection has a downcomer with supplemental deflectors
        /// </summary>
        public bool HasSupplementalDeflectors
        {
            get
            {
                for (int i = 1; i <= Count; i++)
                {
                    mdDowncomer aMem = Item(i);
                    if (aMem.IsVirtual) continue;
                    if (aMem.HasSupplementalDeflector) return true;
                }
                return false;
            }
        }
        /// <summary>
        /// returns true if the collection has a downcomer with foldover weirs
        /// </summary>
        public bool HasFoldovers
        {
            get
            {

                for (int i = 1; i <= Count; i++)
                {
                    mdDowncomer aMem = Item(i);
                    if (aMem.IsVirtual) continue;
                    if (aMem.FoldOverHeight != 0) return true;
                }
                return false;
            }
        }
        /// <summary>
        /// returns true if the collection has a downcomer with triangular end plates
        /// </summary>
        public bool HasTriangularEndPlates
        {
            get
            {

                for (int i = 1; i <= Count; i++)
                {
                    mdDowncomer aMem = Item(i);
                    if (aMem.IsVirtual) continue;
                    if (aMem.HasTriangularEndPlate) return true;
                }

                return false;
            }
        }
        public override string INIPath => $"COLUMN({ColumnIndex}).RANGE({RangeIndex}).TRAYASSEMBLY.DOWNCOMERS";


        /// <summary>
        /// flag indicating that something has changed and the collection needs to be regenerated
        /// </summary>
        public override bool Invalid
        {
            get => base.Invalid || Count <= 0;

            set
            {
                if (base.Invalid != value && value) eventDowncomersInvalidated?.Invoke();
                base.Invalid = value;
                if (value)

                    foreach (var aMem in this) { aMem.Info = null; aMem.Boxes = null; }
            }
        }
        /// <summary>
        /// the average of the fba to weir length ratios
        /// ~returns the average  if the spacing method is standard
        /// </summary>
        public double AverageRatio { get => PropValD("AverageRatio"); set => PropValSet("AverageRatio", value, bSuppressEvnts: true); }

        public double MaxBoxWidth
        {
            get
            {
                double _rVal = 0;

                for (int i = 1; i <= Count; i++)
                {
                    mdDowncomer aMem = Item(i);
                    double wd = aMem.BoxWidth;
                    if (wd > _rVal) _rVal = wd;

                }
                return _rVal;
            }
        }

        public double FBA2WLRatio => SpacingData.FBA2WLRatio;

        /// <summary>
        /// flag indicating that something has changed and the collection needs to be reoptimized
        /// </summary>
        public bool Optimized { get; set; }

        private bool _Optimizing = false;
        /// <summary>
        /// indicates if the collection is in then process of optimizing its spacing values
        /// </summary>
        public bool Optimizing => _Optimizing;

        /// <summary>
        /// the optimum center to center spacing of the downcomers in the collection
        /// </summary>
        public double OptimumSpacing { get => PropValD("OptimumSpacing"); set => PropValSet("OptimumSpacing", value, bSuppressEvnts: true); }

        /// <summary>
        /// the center to center spacing to use other that the optimized value
        /// </summary>
        public double OverrideSpacing { get => PropValD("OverrideSpacing"); set => PropValSet("OverrideSpacing", value, bSuppressEvnts: true); }

        internal mdSpacingData _SpacingData;
        public mdSpacingData SpacingData
        {
            get
            {
                if (!Optimized || _SpacingData == null)
                {
                    mdTrayAssembly assy = GetMDTrayAssembly();
                    if (Optimizing)
                    {
                        return new mdSpacingData(assy, Spacing, Offset);
                    }
                    else
                    {
                        if (assy != null) OptimizeSpacing(assy);
                    }
                    
                }
                if(_SpacingData == null)
                {
                    mdTrayAssembly assy = GetMDTrayAssembly();
                    _SpacingData = new mdSpacingData(assy, Spacing, Offset);
                }
                else
                {
                    _SpacingData.ProjectType = ProjectType;
                }
          
                return _SpacingData;
            }
            set
            {
                _SpacingData = value;
                if (value != null)
                {
                    value.DowncomerData.UpdateMembers();
                    WeightedSpacingDeviation = value.WeightedDeviation;
                    SpacingDeviation = value.StandardDeviation;
                    WeightedAverageRatio = value.WeightedAverageRatio;
                    AverageRatio = value.AverageRatio;

                    Ratios = mzUtils.ListToString(value.Ratios);
                    FBAs = mzUtils.ListToString(value.FreeBubblingAreas);
                    WeirLengths = mzUtils.ListToString(value.WeirLengths);
                    ResetBoxes();
                    
                    if (DesignFamily.IsBeamDesignFamily())
                    {
                        mdTrayAssembly assy = GetMDTrayAssembly();
                        if (assy != null)
                        {
                            assy.Beam.DowncomerSpacing = value.DowncomerData.Spacing;
                          
                        }
                    }
                    
                    
                }
            }

        }

        /// <summary>
        /// a comma delimited string of the the FBA to WeirLength Ratios
        /// </summary>
        public string Ratios { get => PropValS("Ratios"); set => PropValSet("Ratios", value, bSuppressEvnts: true); }

        /// <summary>
        /// the total number of ring clips needed to attach the downcomers to the ring
        /// ~accounts for downcomers occurance factors
        /// </summary>
        public int RingClipCount
        {
            get
            {
                int _rVal = 0;
                mdDowncomer aMem;
                for (int i = 1; i <= Count; i++)
                {
                    aMem = Item(i);
                    _rVal += 2 * aMem.Boxes[0].EndSupport().GenHoles("RING CLIP").HoleCount() * aMem.OccuranceFactor;
                }
                return _rVal;
            }
        }

        /// <summary>
        ///returns the downcomer marked as selected from the collection
        /// ^if none are downcomer as selected the first group is returned
        /// </summary>
        public new mdDowncomer SelectedMember => (mdDowncomer)base.SelectedMember;

        /// <summary>
        /// the center to center distance between the downcomers in the collection
        /// ~returns either the override value or the optimized value
        /// </summary>
        public double Spacing
        {
            get
            {
                double _rVal = OverrideSpacing;
                if (_rVal <= 0)
                    _rVal = OptimumSpacing;

                return _rVal;
            }
        }

        public double Offset { get => PropValD("Offset"); set => PropValSet("Offset", value); }

        /// <summary>
        /// the standard deviation of the spacing optimization calculation results
        /// ~returns the Weighted deviation if the parent tray assembly spacingmethod = weighyed
        /// </summary>
        public double SpacingDeviation
        {
            get
            {
                double _rVal = PropValD("SpacingDeviation");
                mdTrayAssembly aAssy = GetMDTrayAssembly();

                if (aAssy == null) return _rVal;
                if (aAssy.SpacingMethod != uppMDSpacingMethods.NonWeighted)
                { _rVal = WeightedSpacingDeviation; }

                return _rVal;
            }
            set
            {
                PropValSet("SpacingDeviation", value, bSuppressEvnts: true);

            }
        }
        /// <summary>
        ///returns all the centers of startup spouts of all the downcomers in the collection
        /// </summary>
        public colDXFVectors StartupCenters
        {
            get
            {
                colDXFVectors startupCenters = new colDXFVectors();
                mdDowncomer aMem;
                mdTrayAssembly aAssy = null;
                for (int i = 1; i <= Count; i++)
                {
                    aMem = Item(i);
                    startupCenters.Append(aMem.StartupSpouts(aAssy).Slots.CentersDXF());
                }
                return startupCenters;
            }
        }


        /// <summary>
        /// the function total bottom area for all the downcomers in the collection
        /// </summary>
        public double TotalBottomArea
        {
            get
            {
                double totalBottomArea = 0;
                mdDowncomer DComer = null;
                for (int i = 1; i <= Count; i++)
                {
                    DComer = Item(i);
                    totalBottomArea += DComer.BottomArea * DComer.OccuranceFactor;
                }
                return totalBottomArea;
            }
        }

        /// <summary>
        ///returns the total weir length for the entire tray assembly
        /// ~this is the sum of all the downcomers weir lengths times their occurance factors
        /// </summary>
        public double TotalWeirLength
        {
            get
            {
                double _rVal = 0;
                foreach (mdDowncomer dc in this) if(!dc.IsVirtual) _rVal += dc.WeirLength(bTrayWide: true);
                return _rVal;
            }
        }

        /// <summary>
        /// the weighted average of the fba to weir length ratios
        /// ~returns the average if the spacing method is weighted
        /// </summary>
        public double WeightedAverageRatio
        {
            get => PropValD("WeightedAverage");
            set => PropValSet("WeightedAverage", value, bSuppressEvnts: true);
        }

        /// <summary>
        /// the weighted spacing deviation
        /// </summary>
        public double WeightedSpacingDeviation
        {
            get => PropValD("WeightedSpacingDeviation");
            set => PropValSet("WeightedSpacingDeviation", value, bSuppressEvnts: true);

        }
        /// <summary>
        /// a comma delimited string of the the weir length values
        /// </summary>
        public string WeirLengths
        {
            get => PropValS("WeirLengths");
            set => PropValSet("WeirLengths", value, bSuppressEvnts: true);
        }

        /// <summary>
        ///returns the longest downcomer in the collection
        /// </summary>
        public new mdDowncomer LongestMember
        {
            get
            {
                mdDowncomer _rVal = null;
                double lg = 0;
                double dclg = 0;
                mdDowncomer aMem;
                for (int i = 1; i <= Count; i++)
                {
                    aMem = Item(i);
                    dclg = aMem.Length;
                    if (dclg > lg)
                    {
                        _rVal = aMem;
                        lg = dclg;
                    }
                }
                return _rVal;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// returns all the boxes of the members
        /// </summary>
        /// <remarks>virtual downcomers dont have any boxes. in non-standard trays some downcomers do not have any non-virtual boxes</remarks>
        /// <returns></returns>
        public List<mdDowncomerBox> Boxes(int aDCIndex = 0)
        {
            List<mdDowncomerBox> _rVal = new List<mdDowncomerBox>();
            foreach (mdDowncomer dc in this)
            {
                if (dc.IsVirtual) continue;
                if (aDCIndex > 0 && dc.Index != aDCIndex) continue;
                _rVal.AddRange(dc.Boxes.FindAll((x) => !x.IsVirtual));
            }
            return _rVal;
        }

        public List<uopRingClipSegment> RingClipSegments(int aDCIndex = 0)
        {
            List<uopRingClipSegment> _rVal = new List<uopRingClipSegment>();
            List<mdDowncomerBox> boxes = Boxes(aDCIndex);
            foreach (var box in boxes) _rVal.AddRange(box.RingClipSegments());
            return _rVal;

        }

        public override void Clear(bool bSuppressEvents = false)
        {
            base.Clear(bSuppressEvents);
            _SpacingData = null;
            _Optimizing = false;
        }

        public new mdDowncomer LastItem() => (mdDowncomer)base.LastItem();

        public new mdDowncomer FirstItem() => (mdDowncomer)base.FirstItem();


        /// <summary>
        ///returns the centers of all the downcomers in the collection
        /// </summary>
        public colDXFVectors Centers(bool bTrayWide = false)
        {

            colDXFVectors _rVal = new colDXFVectors();

            for (int i = 1; i <= Count; i++)
            {
                mdDowncomer aMem = Item(i);
                if (!bTrayWide && aMem.IsVirtual) continue;
                _rVal.Add(aMem.CenterDXF);
            }
            return _rVal;

        }

        /// <summary>
        ///returns the centers X ordinates of all the downcomers in the collection
        /// </summary>
        public List<double> XValues(bool bTrayWide = false)
        {

            List<double> _rVal = new List<double>();
            mdDowncomer aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (!bTrayWide && aMem.IsVirtual) break;
                _rVal.Add(aMem.X);
            }
            return _rVal;

        }

        /// <summary>
        ///returns the objects properties in a collection
        /// ~signatures like "COLOR=RED"
        /// </summary>
        public override uopProperties CurrentProperties() { UpdatePartProperties(); return base.CurrentProperties(); }


        internal TVALUES CenterValues(bool bReturnNegatives = false)
        {
            TVALUES _rVal = new TVALUES("CENTER VALUES");
            mdDowncomer aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (i == 1) _rVal.BaseValue = aMem.BoxWidth;
                _rVal.Add(aMem.X);
            }

            if (!bReturnNegatives) return _rVal;
            double xVal;
            for (int i = Count; i >= 1; i--)
            {
                aMem = Item(i);
                xVal = aMem.X;
                if (Math.Round(xVal, 1) != 0) _rVal.Add(-xVal);
            }

            return _rVal;

        }


        /// <summary>
        ///a rectangle that contains the extreme limits of all the dowcomers in the collection
        /// </summary>
        public uopRectangle Bounds(bool bExcludeVirtuals = true)
        {

            if (Count <= 0) return new uopRectangle();
            uopRectangle _rVal = Item(1).Bounds;
            for (int i = 2; i <= Count; i++)
            {
                mdDowncomer mem = Item(i);
                if (!bExcludeVirtuals || (bExcludeVirtuals && !mem.IsVirtual)) _rVal.ExpandTo(mem.Bounds);
            }
            return _rVal;

        }

        public new List<mdDowncomer> ToList(bool bGetClones = false)
        {

            List<mdDowncomer> _rVal = new List<mdDowncomer>();

            foreach (var part in CollectionObj)
            {
                if (!bGetClones)
                    _rVal.Add((mdDowncomer)part);
                else
                    _rVal.Add((mdDowncomer)part.Clone());
            }
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
        UHOLEARRAY GenHolesV(mdTrayAssembly aAssy = null, string aTag = "", string aFlag = "", bool bSuppressSpouts = false, bool bTrayWide = false)
        {
            UHOLEARRAY _rVal =UHOLEARRAY.Null;;
            aAssy ??= GetMDTrayAssembly(aAssy);
            if (aAssy == null) return _rVal;

            for (int i = 1; i <= Count; i++)
            {
                mdDowncomer aMem = Item(i);
                if (aMem.IsVirtual || !aMem.Boxes.Any(b => !b.IsVirtual)) continue;
                UHOLEARRAY memHls = aMem.GenHolesV(aAssy, aTag, aFlag, bSuppressSpouts);
                if (bTrayWide && aMem.OccuranceFactor > 1)
                {
                    memHls.AppendMirrors(aX: 0, aY: null);
                }
                _rVal.Append(memHls, bAppendToExisting: true);
            }
            return _rVal;
        }
        /// <summary>
        /// all the holes of all the sections in the collection
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aTag"></param>
        /// <param name="aFlag"></param>
        /// <param name="bSuppressSpouts"></param>
        /// <param name="bTrayWide"></param>
        /// <returns></returns>
        public uopHoleArray GenHoles(mdTrayAssembly aAssy, string aTag = "", string aFlag = "", bool bSuppressSpouts = false, bool bTrayWide = false)
        {
            uopHoleArray _rVal = new uopHoleArray(GenHolesV(aAssy, aTag, aFlag, bSuppressSpouts, bTrayWide));

            return _rVal;
        }

        /// <summary>
        /// used to add an item to the collection
        /// ~won't add "Nothing" (no error raised).
        /// #1the item to add to the collection
        /// </summary>
        /// <param name="aDowncomer"></param>
        /// <param name="bAddClone"></param>
        /// <returns></returns>
        public mdDowncomer Add(mdDowncomer aDowncomer, bool bAddClone = false)
        {
            mdDowncomer _rVal = (mdDowncomer)base.Add(aDowncomer, bAddClone);

            if (_rVal == null) return null;


            return _rVal;
        }

        /// <summary>
        /// the global downcomer that this collection is modeled after
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public mdDowncomer Basis(mdTrayAssembly aAssy)
        {
            aAssy ??= GetMDTrayAssembly(aAssy);
            if (aAssy == null) return null;
            return aAssy.Downcomer();
        }


        public void SetSpacingData(mdTrayAssembly aAssy, mdSpacingData aSolution, bool bCentersOnly = false)
        {
            aAssy ??= GetMDTrayAssembly();
            SpacingData = aSolution;
            if (aSolution == null || aAssy == null) return;

            if (!bCentersOnly)
                RefreshProperties(aAssy, aAssy.Downcomer(), aSolution.DowncomerData);
            else
                SetCenters(aAssy, aSolution.DowncomerData);

        }


        /// <summary>
        ///returns the startup spouts that are active on the members of the collection
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="bReturnMirrored"></param>
        /// <param name="aDCList"></param>
        /// <param name="aSide"></param>
        /// <param name="bGetClones"></param>
        /// <returns></returns>
        public mdStartupSpouts StartupSpouts(mdTrayAssembly aAssy, bool bReturnMirrored, string aDCList = "", string aSide = "", bool bGetClones = false)
        {
            mdStartupSpouts _rVal = new mdStartupSpouts();
            aAssy ??= GetMDTrayAssembly(aAssy);
            if (aAssy == null) return _rVal;

            mdStartupSpouts aSUs = aAssy.StartupSpouts.GetBySuppressed(false);

            int cnt = GetByVirtual(aVirtualValue: false).Count;
            for (int i = 1; i <= cnt; i++)
            {
                if (mzUtils.ListContains(i, aDCList, ",", true))
                {
                    mdStartupSpouts dcSUs = aSUs.GetByDowncomerIndex(i, 0,aSide, bGetClones);
                    _rVal.Append(dcSUs);

                    if (bReturnMirrored && dcSUs != null)
                    {
                        for (int j = dcSUs.Count; j > 0; j--)
                        {
                            mdStartupSpout aSU = dcSUs.Item(j);
                            if (aSU.Mirrored)
                            {
                                aSU = aSU.Clone();
                                aSU.Y = -aSU.Y;
                                _rVal.Add(aSU);
                            }
                        }
                    }
                }
            }
            _rVal.SetObscured(aAssy, null);
            return _rVal;
        }

        public int StiffenerCount(bool bOneSide)
        {
            int StiffenerCount = 0;
            mdDowncomer aMem;
            int cnt = 0;
            string ssites = string.Empty;
            string[] sVals; // TODO - Specified Minimum Array Boundary Not Supported:     Dim sVals() As String
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                cnt = 0;
                ssites = aMem.StiffenerSites;
                if (ssites !=  string.Empty)
                {
                    sVals = ssites.Split(",".ToCharArray());
                    cnt = sVals.Length;// UBound(sVals) + 1;
                }
                if (!bOneSide)
                {
                    if (Math.Round(aMem.X, 1) > 0)
                    {
                        cnt *= 2;
                    }
                }
                StiffenerCount += cnt;
            }
            return StiffenerCount;
        }



        /// <summary>
        ///returns an new collection whose members are clones of the members of this collection
        /// </summary>
        /// <returns></returns>
        public colMDDowncomers Clone() => new colMDDowncomers(this, bDontCopyMembers: false);

        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();
        /// <summary>



        /// <summary>
        /// the end plates of the downcomers
        /// </summary>
        /// <param name="bSuppressNegYs"></param>
        /// <returns></returns>
        public colUOPParts EndPlates(bool bSuppressNegYs)
        {
            colUOPParts _rVal = new colUOPParts();
            mdDowncomer aMem;

            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                foreach (var box in aMem.Boxes)
                {
                    foreach (var endPlate in box.EndPlates())
                    {
                        if (endPlate.Y > 0 || !bSuppressNegYs)
                        {
                            _rVal.Add(endPlate, true);
                        }
                    }
                }
            }
            _rVal.SubPart(this);
            return _rVal;
        }

        /// <summary>
        /// the end supports of the downcomers
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aDCID"></param>
        /// <param name="aSide"></param>
        /// <returns></returns>
        public List<mdEndSupport> EndSupports(int aDCID = 0, uppSides aSide = uppSides.Undefined, uppIntersectionTypes aIntersectionType = uppIntersectionTypes.Undefined)
        {
            List<mdEndSupport> _rVal = new List<mdEndSupport>();
            if (aSide != uppSides.Top && aSide != uppSides.Bottom) aSide = uppSides.Undefined;

            for (int i = 1; i <= Count; i++)
            {
                if (aDCID <= 0 || i == aDCID)
                {
                    List<mdEndSupport> ess = Item(i).EndSupports(aSide: aSide);
                    if (aIntersectionType != uppIntersectionTypes.Undefined) ess = ess.FindAll((x) => x.IntersectionType == aIntersectionType);
                    _rVal.AddRange(ess);

                }


            }
            return _rVal;
        }

        /// <summary>
        /// the end plates of the downcomers
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aDCID"></param>
        /// <param name="aSide"></param>
        /// <returns></returns>
        public List<mdEndPlate> EndPlates(int aDCID = 0, uppSides aSide = uppSides.Undefined, uppIntersectionTypes aIntersectionType = uppIntersectionTypes.Undefined)
        {
            List<mdEndPlate> _rVal = new List<mdEndPlate>();
            if (aSide != uppSides.Top && aSide != uppSides.Bottom) aSide = uppSides.Undefined;

            for (int i = 1; i <= Count; i++)
            {
                if (aDCID <= 0 || (i == aDCID))
                {
                    List<mdEndPlate> ess = Item(i).EndPlates(aSide: aSide);

                    if (aIntersectionType != uppIntersectionTypes.Undefined) ess = ess.FindAll((x) => x.IntersectionType == aIntersectionType);
                    _rVal.AddRange(ess);
                }
            }
            return _rVal;
        }


        /// <summary>
        ///returns the data returned from the last optimization of the collections spacing
        /// </summary>
        /// <param name="aAssy"></param>
        public mdSpacingData FetchSpacingData(mdTrayAssembly aAssy)
        {

            aAssy ??= GetMDTrayAssembly(aAssy);
            mdSpacingData _rVal = new mdSpacingData(aAssy, Spacing, Offset);

            if (aAssy == null) return _rVal;

            PropValSet("SpacingDeviation", _rVal.StandardDeviation, bSuppressEvnts: true);
            PropValSet("WeightedSpacingDeviation", _rVal.WeightedDeviation, bSuppressEvnts: true);
            PropValSet("WeightedAverage", _rVal.WeightedAverageRatio, bSuppressEvnts: true);
            PropValSet("AverageRatio", _rVal.AverageRatio, bSuppressEvnts: true);
            PropValSet("FBAs", mzUtils.ListToString(_rVal.FreeBubblingAreas), bSuppressEvnts: true);
            PropValSet("Ratios", mzUtils.ListToString(_rVal.Ratios), bSuppressEvnts: true);
            PropValSet("WeirLengths", mzUtils.ListToString(_rVal.WeirLengths), bSuppressEvnts: true);

            return _rVal;
        }


        /// <summary>
        ///returns the properties of the members that differ from the passed downcomer
        /// ~signatures like "COLOR=RED"
        /// #1the downcomer to compare to
        /// </summary>
        /// <param name="Basis"></param>
        /// <returns></returns>
        public uopProperties GetDifferences(mdDowncomer Basis)
        {
            uopProperties getDifferences = null;
            if (Basis == null)
            {
                return getDifferences;
            }
            getDifferences = new uopProperties();
            for (int i = 1; i <= Count; i++)
            {
                getDifferences.Append(Item(i).GetDifferences(Basis));
            }
            return getDifferences;
        }
        /// <summary>
        ///returns a collection filled with the ideal spout areas for each of the downcomers in the assembly
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aTotalWeirLength"></param>
        /// <param name="aTrayIdealSpoutArea"></param>
        /// <returns></returns>
        public List<double> GetIdealAreas(mdTrayAssembly aAssy, double aTotalWeirLength = 0, double aTrayIdealSpoutArea = 0)
        {
            List<double> _rVal = new List<double>();


            if (aTrayIdealSpoutArea <= 0)
            {
                aAssy ??= GetMDTrayAssembly();
                if (aAssy == null) return _rVal;
                aTrayIdealSpoutArea = aAssy.TheoreticalSpoutArea;
            }

            if (aTotalWeirLength <= 0) aTotalWeirLength = TotalWeirLength;
            foreach (var item in this)
            {
                _rVal.Add(item.IdealSpoutArea(aAssy, aTotalWeirLength, aTrayIdealSpoutArea));
            }
            //mdDowncomer dc1 = Item(1);
            //mdDowncomer dc2 = Item(8);
            //DowncomerInfo info1 = dc1.CurrentInfo();
            //DowncomerInfo info2 = dc2.CurrentInfo();

            //double d1 = dc1.WeirLength();
            //double d2 = dc2.WeirLength();

            //d1 = dc1.IdealSpoutArea(aAssy, aTotalWeirLength, aTrayIdealSpoutArea);
            //d2 = dc2.IdealSpoutArea(aAssy, aTotalWeirLength, aTrayIdealSpoutArea);


            ////get the weir _rVal for the downcomers
            //for (int i = 1; i <= Count; i++)
            //{
            //    mdDowncomer aMem = Item(i);
            //    if (aMem.IsVirtual)
            //    {
            //        aMem = aMem.Parent;
            //        aMem ??= Item(i);
            //    }

            //    _rVal.Add(aMem.IdealSpoutArea(aAssy, aTotalWeirLength));
            //}
            return _rVal;
        }
        /// <summary>
        ///returns the downcomer from the collection with the maximum error percentage.
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public mdDowncomer GetMaxError(mdTrayAssembly aAssy)
        {
            mdDowncomer _rVal = null;
            aAssy ??= GetMDTrayAssembly(aAssy);

            if (aAssy == null) return _rVal;

            double max = double.MinValue;

            for (int i = 1; i <= Count; i++)
            {
                mdDowncomer aMem = Item(i);
                if (!aMem.IsVirtual)
                {
                    double errpct = Math.Abs(aMem.ErrorPercentage(aAssy));
                    if (errpct > max)
                    {
                        _rVal = aMem;
                        max = errpct;
                    }

                }
            }
            return _rVal;
        }

        /// <summary>
        /// sets the current collection based on the basis tray assembly's downcomer
        /// #1the tray assembly to use for the configuration information
        /// #2an optional set of downcomers to extract some previous state properties
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="SetSpacing"></param>
        public void Initialize(mdTrayAssembly aAssy, bool SetSpacing)
        {
            if (_Initializing) return;

            aAssy ??= GetMDTrayAssembly(aAssy);
            if (aAssy == null) throw new Exception("The Passed Tray Assembly Is Nothing");

            try
            {
                SubPart(aAssy);
                mdDowncomer aBasis = aAssy.Downcomer();
                if (aBasis == null) return;

                _Initializing = true;
                int cnt = aBasis.Count;
                if (cnt <= 0)
                    return;

                mdDowncomer DCtoCopy = null;
                List<mdDowncomer> existingDComers = new List<mdDowncomer>();
                //_Collection.Items.Clear();
                mdDowncomer aDC;
                double spc1 = Spacing;
                bool bOddDc = uopUtils.IsOdd(cnt);
                uppMDDesigns design = aAssy.DesignFamily;
                bool symmetric = design.IsStandardDesignFamily();
                int virlim = !bOddDc ? cnt / 2 : (cnt + 1) / 2;
                int selID = 0;
                int dcCnt = symmetric ? virlim : cnt;
                if (!symmetric) { dcCnt = cnt; } // virlim = cnt; }
                bool divided = DesignFamily.IsBeamDesignFamily() || DesignFamily.IsDividedWallDesignFamily();
                //retain properties of individual members if we are regenerating
                if (Count > 0)
                {
                    for (int i = 1; i <= Count; i++)
                    {
                        aDC = (mdDowncomer)base.Remove(i);
                        if (existingDComers.Count < dcCnt) existingDComers.Add(aDC);

                        if (selID == 0 & aDC.Selected) selID = i;

                    }
                }

                base.Clear(true);
                if (selID == 0) selID = 1;


                while (Count != aBasis.Count)
                {

                    int j = Count + 1;
                    int k = j > virlim ? uopUtils.OpposingIndex(j, cnt) : j;
                    DCtoCopy = (existingDComers.Count > 0 && k <= existingDComers.Count) ? existingDComers[k - 1] : DCtoCopy = aBasis;

                    aDC = new mdDowncomer(DCtoCopy, aAssy) { IsGlobal = false };

                    if (aBasis.Count > 1 && (j == 1 || j == aBasis.Count) && existingDComers.Count <= 0)
                    {
                        aDC.PropValSet("HasTriangularEndPlate", true, bSuppressEvnts: true);
                    }
                    aDC.SubPart(aAssy);
                    if (symmetric)
                    {
                        aDC.IsVirtual = j > virlim;
                        if (aDC.IsVirtual)
                        {
                            aDC.AssociateToParent(Item(k));
                        }
                    }
                    else
                    {
                        if (j > virlim)
                        {
                            mdDowncomer bDC = Item(uopUtils.OpposingIndex(j, cnt));
                            aDC.AssociateToParent(bDC);
                            aDC.CopyParentProperties();
                            aDC.IsVirtual = false;
                        }


                        //if (virtualX.HasValue)
                        //{
                        //    aDC.IsVirtual = aDC.X - 0.5 * aDC.Width < virtualX.Value;
                        //}

                    }

                    
                    Add(aDC);

                    aDC.OccuranceFactor = 1; //  !symmetric ? 1 : (bOddDc && Count == dcCnt) ? 1 : 2;

                }
                if (SetSpacing)
                    InitializeSpacing(aAssy);
                else
                    if (Spacing > 0) SetCenters(aAssy, new DowncomerDataSet(aAssy, Spacing, Offset, aBasis));

                if (Spacing != spc1) eventSpacingChanged?.Invoke();



                SetSelected(selID);

            }
            catch
            {

            }
            finally
            {
                _Initializing = false;
            }


        }


        /// <summary>
        /// executed to optimize the FBA/Weir Length ratios of the downcomers in the collection
        /// </summary>
        /// <param name="aAssy"></param>
        private void InitializeSpacing(mdTrayAssembly aAssy)
        {
            mdDowncomer aBasis = Basis(aAssy);

            if (aBasis == null)
                return;
            bool wuz = _Initializing;
            _Initializing = true;
            try
            {
                Optimize(aAssy);
            }
            catch { }
            finally { _Initializing = wuz; }


        }


        public override uopPart Item(int aIndex, bool bSuppressIndexError = false) => (uopPart)Item(aIndex, bSuppressIndexError);

        /// <summary>
        ///returns the item from the collection at the requested index ! BASE 1 !
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        public mdDowncomer Item(int aIndex, bool bReturnClone = false, bool bSuppressIndexError = false)
        {

            if (aIndex < 1 || aIndex > Members.Count)
            {
                if (!bSuppressIndexError) 
                    throw new IndexOutOfRangeException();
                return null;
            }

            mdDowncomer _rVal = (mdDowncomer)base.Item(aIndex, bSuppressIndexError);

            if (bReturnClone && _rVal != null) _rVal = _rVal.Clone();
            return _rVal;
        }

        public colDXFVectors RingClipCenters(mdTrayAssembly aAssy, bool bBothSides, colDXFVectors aCollector)
        {
            colDXFVectors _rVal = aCollector ?? new colDXFVectors();
            aAssy ??= GetMDTrayAssembly(aAssy);
            if (aAssy == null) return _rVal;



            uopRingClip aRC = aAssy.RingClip(true);
            double rad = (aRC.Size == uppRingClipSizes.FourInchRC) ? aRC.Bolt.GetWasher().OD / 2 : mdGlobals.HoldDownWasherRadius;


            for (int i = 1; i <= Count; i++)
            {
                mdDowncomer aMem = Item(i);
                if (!bBothSides && aMem.IsVirtual) continue;
                uopHoles aHls;
                foreach (var box in aMem.Boxes)
                {
                    aHls = box.EndSupport(bTop: true).GenHoles("RING CLIP").Item(1);
                    for (int j = 1; j <= aHls.Count; j++)
                    {
                        uopHole aHl = aHls.Item(j);
                        dxfVector v1 = aHl.CenterDXF;
                        v1.Radius = rad;
                        _rVal.Add(v1);

                    }

                    aHls = box.EndSupport(bTop: false).GenHoles("RING CLIP").Item(1);
                    for (int j = 1; j <= aHls.Count; j++)
                    {
                        uopHole aHl = aHls.Item(j);
                        dxfVector v1 = aHl.CenterDXF;
                        v1.Radius = rad;
                        _rVal.Add(v1);

                    }
                }
            }
            return _rVal;
        }
        /// <summary>
        ///returns the last item from the collection
        /// </summary>
        /// <returns></returns>
        public mdDowncomer LastMember() => (Count > 0) ? Item(Count) : null;

        private void Optimize(mdTrayAssembly aAssy)
        {
            //if (_Optimizing)
            //    return;
            mdSpacingData solution = null;
            _Optimizing = true;
            try
            {
                OptimumSpacing = 0;
                WeightedSpacingDeviation = 0;
                SpacingDeviation = 0;
                WeightedAverageRatio = 0;
                AverageRatio = 0;
                Ratios = string.Empty;
                FBAs = string.Empty;
                WeirLengths = string.Empty;
                double curval = CurrentSpacing;
                mdSpacingSolutions solutions = null;
                if (Count == 0) return;
                
                aAssy ??= GetMDTrayAssembly();
                if (aAssy != null) aAssy.RaiseStatusChangeEvent($"Optimizing {aAssy.TrayName(false)} Downcomer Spacing");
                mdSpacingData opt_solution = mdSpacingSolutions.OptimizedSpace(aAssy, out solutions ,aStartValue: DesignFamily.IsStandardDesignFamily() ?curval : 0);
                
                if (opt_solution != null)
                {
                    solution = opt_solution;
                    double aVal = opt_solution.Spacing;
                    OptimumSpacing = aVal;

                    if (aAssy != null)
                    {
                   
                        double oride = aAssy.OverrideSpacing;
                        if (oride > 0)
                        {
                            if (oride != aVal)
                            {

                                mdSpacingData ovr_solution = mdSpacingSolutions.OptimizedSpace(aAssy, out solutions , aIdealSpace: oride);
                                solution = ovr_solution;
                                aVal = ovr_solution.Spacing;

                                //aVal = mdUtils.OptimizedSpace(aAssy, this, out dev, out aFBAs, out aWeirLengths, out aRatios, out wtdev, out wtavg, out avg, aIdealSpace: oride);
                                PropValSet("OverrideSpacing", aVal, bSuppressEvnts: true);
                                aAssy.PropValSet("OverrideSpacing", aVal, bSuppressEvnts: true);
                            }
                            else
                            {
                                aAssy.PropValSet("OverrideSpacing", 0, bSuppressEvnts: true);
                            }
                        }
                        else
                        {
                            PropValSet("OverrideSpacing", 0, bSuppressEvnts: true);
                        }
                    }
                    if(aAssy != null )
                    {
                        if (aAssy.DesignFamily.IsBeamDesignFamily())
                            aAssy.Beam.DowncomerSpacing = Spacing;
                    }
                  



               Optimized = true;
                }
            }
            catch { }
            finally
            {
                SetSpacingData(aAssy, solution, true);

                _Optimizing = false;
            }

        }

        /// <summary>
        /// executed to optimize the FBA/Weir Length ratios of the downcomers in the collection
        /// </summary>
        /// <param name="aAssy"></param>
        public void OptimizeSpacing(mdTrayAssembly aAssy,bool bSuppressEvents = false)
        {

            aAssy ??= GetMDTrayAssembly(aAssy);

            if (aAssy == null)
                throw new Exception("colMDDowncomers.OptimizeSpacing[Assembly Required]");
            double spcVal = 0;
            double wuz = CurrentSpacing;
            _Optimizing = true;
            if (!_Initializing && !SuppressEvents && !bSuppressEvents)
                eventSpacingOptimizationBegin?.Invoke();
            Optimize(aAssy);
            spcVal = Spacing;

            if (!_Initializing && !SuppressEvents && !bSuppressEvents)
            {
                eventSpacingOptimizationEnd?.Invoke(wuz, spcVal);
                if (wuz != spcVal)
                {
                    eventSpacingChanged?.Invoke();
                }
            }
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
                aFileSection = string.IsNullOrWhiteSpace(aFileSection) ? INIPath : aFileSection.Trim();
                if (aFileProps == null) throw new Exception("The Passed File Property Array is Undefined");
                if (string.IsNullOrWhiteSpace(aFileSection)) throw new Exception("File Section is Undefined");
                if (aProject != null) SubPart(aProject);
                if (aAssy == null) throw new Exception("Tray Assembly is Undefined");

                mdTrayAssembly myassy = (mdTrayAssembly)aAssy;
                SubPart(myassy);

                //uopProperties myfileprops = aFileProps.Item(aFileSection);
                uopPropertyArray myfileprops = aFileProps.PropertiesStartingWith(aFileSection);

                if (myfileprops == null || myfileprops.Count <= 0)
                {
                    ioWarnings?.AddWarning(this, $"{PartName} Data Not Found", $"File '{aFileProps.Name}' Does Not Contain {aFileSection} Info!");
                    return;
                }

                UpdatePartProperties();

                base.ReadProperties(aProject, myfileprops, ref ioWarnings, aFileVersion, aFileSection, bIgnoreNotFound, aSkipList: aSkipList, EqualNames: EqualNames);

                
                uopProperties memprops = aFileProps.SubPropertiesStartingWith(aFileSection, "Downcomer");
                List<mdDowncomer> parents = GetByVirtual(false);
                if (memprops.Count != parents.Count)
                {
                    ioWarnings?.AddWarning(this, $"{PartName} Data Not Found", $"File '{aFileProps.Name}' Does Not Contain Downcomer Member Info!");
                }
                for (int i = 1; i <= Count; i++)
                {
                    mdDowncomer mem = Item(i);


                    if (!mem.IsVirtual)
                    {
                        string mempropstring = memprops.ValueS($"Downcomer{i}");
                        mem.DefineDifferences(mempropstring, uopGlobals.Delim);

                    }
                    else
                    {
                        mem.CopyParentProperties();
                    }


                }
                try
                {
                    string dstr = myfileprops.ValueS(aFileSection, "OptimumSpacing");
                    if (mzUtils.IsNumeric(dstr))
                    {
                        PropValSet("OptimumSpacing", mzUtils.VarToDouble(dstr), bSuppressEvnts: true);
                        Optimized = true;
                        Invalid = false;
                    }
                    else
                    {
                        InitializeSpacing(myassy);
                    }
                    Reading = false;
                    Quantity = Count;
                }
                catch (Exception)
                {
                    InitializeSpacing(myassy);
                }
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
        /// makes sure that the properties of the collection downcomers match the assemblies global downcomer properties
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aBasis"></param>
        /// <param name="aDataset"></param>
        public void RefreshProperties(mdTrayAssembly aAssy, mdDowncomer aBasis, DowncomerDataSet aDataset = null)
        {
            if (aDataset == null) return;
            aAssy ??= aDataset.TrayAssembly;
            aAssy ??= GetMDTrayAssembly();
            aBasis ??= Basis(aAssy);

            uppMDDesigns family = aDataset.DesignFamily;

            for (int i = 1; i <= Count; i++)
            {
                mdDowncomer DComer = Item(i);

                if (aBasis != null) DComer.RefreshProperties(aBasis);
                if (aDataset != null)
                {
                    DowncomerInfo dcinfo = aDataset.Find((x) => x.DCIndex == DComer.Index);
                    if (dcinfo == null) continue;

                    DComer.PropValSet("X", dcinfo.X, bSuppressEvnts: true);
                    DComer.Info = dcinfo.Clone();
                    DComer.Boxes = null;
                    if (family.IsStandardDesignFamily())
                        DComer.OccuranceFactor = Math.Round(dcinfo.X, 1) == 0 ? 1 : 2;
                    else if (family.IsBeamDesignFamily())
                        DComer.OccuranceFactor = DComer.Boxes.Count > 0 ? Math.Round(dcinfo.X, 1) == 0 ? 1 : 2 : 0;
                    else if (DesignFamily.IsBeamDesignFamily())
                        DComer.OccuranceFactor = DComer.Boxes.Count > 0 ? 1 : 0;
                }


            }
        }
        /// <summary>
        /// removes the item from the collection at the requested index
        /// </summary>
        /// <param name="Index"></param>
        public new mdDowncomer Remove(int aIndex) => (mdDowncomer)base.Remove(aIndex);

        /// <summary>
        /// tells the downcomers to re-obtain their startup spouts from the assemblies collection
        /// </summary>
        public void ResetStartupSites()
        {
            mdDowncomer aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                aMem.SetStartupSpouts(null);
            }
        }
        
        public void ResetBoxes()
        {
            foreach (var item in this) item.Boxes = null;
        }
        public List<uopLinePair> TheoreticalWeirs(bool bIgnoreAngledEndPlates = false) 
        {
            return CurrentDataSet().CreateTheorticalWeirLines(bIgnoreAngledEndPlates);
        }

        public DowncomerDataSet CurrentDataSet(mdTrayAssembly aAssy = null) 
        {
            mdSpacingData data = SpacingData;
            aAssy ??= GetMDTrayAssembly();
            DowncomerDataSet _rVal = data == null ? new DowncomerDataSet(this, aAssy) : data.DowncomerData;
            if(_rVal== null)
            {
                _rVal = new DowncomerDataSet(this, aAssy); // this souldn't happen
                //data.DowncomerData = _rVal;
            }
            return _rVal;
        }

        /// <summary>
        /// executed internally to recenter the downcomers
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aSpaceData"></param>
        private void SetCenters(mdTrayAssembly aAssy, DowncomerDataSet aSpaceData)
        {

            aAssy ??= GetMDTrayAssembly();
            mdDowncomer aBasis = aAssy.Downcomer();
            uppMDDesigns family = aAssy.DesignFamily;
            aSpaceData ??= new DowncomerDataSet(aAssy, Spacing, Offset, aBasis);
            for (int i = 1; i <= aSpaceData.Count; i++)
            {
                DowncomerInfo info = aSpaceData.Item(i);
                mdDowncomer dc = Item(i, bSuppressIndexError: true);
               
                if (dc != null)
                {
                    dc.Boxes = null;
                    double x1 = info.X;
                    if (x1 == 0) dc.PropValSet("HasTriangularEndPlate", info.HasTriangularEndPlate, bSuppressEvnts: true);
                    dc.PropValSet("X", x1, bSuppressEvnts: true);
                    dc.Info = info.Clone();
                    if (family.IsStandardDesignFamily())
                       dc.OccuranceFactor =Math.Round(x1, 1) == 0 ? 1 : 2;
                    else if (family.IsBeamDesignFamily())
                        dc.OccuranceFactor = dc.Boxes.Count > 0 ?  Math.Round(x1, 1) == 0 ? 1 : 2 : 0;
                    else if (DesignFamily.IsBeamDesignFamily())
                        dc.OccuranceFactor = dc.Boxes.Count > 0 ? 1 : 0;
                    
                }

            }
        }


        /// <summary>
        /// sorts the dowmcomers in the collection in the requested order
        /// #1the order to sort the collection in
        /// </summary>
        /// <param name="aOrder"></param>
        public void Sort(dxxSortOrders aOrder)
        {
            if (Count <= 1) return;


            if (aOrder < 0 || (int)aOrder > 3) aOrder = dxxSortOrders.LeftToRight;

            List<uopPart> newCol = new List<uopPart>();
            mdDowncomer aMem;
            List<int> aIds = new List<int>();
            colDXFVectors ctrs = Centers(false);
            ctrs.Sort(aOrder, null, null, ref aIds);


            if (aIds != null)
            {
                for (int i = 0; i < aIds.Count; i++)
                {
                    aMem = Item(aIds[i]);
                    aMem.Index = newCol.Count + 1;
                    newCol.Add(aMem);
                }
            }
            base.CollectionObj = newCol;

            Quantity = Count;
        }

        /// <summary>
        ///returns all the startup spouts of all the downcomers in the collection
        /// </summary>
        /// <param name="newobj"></param>
        public void SetStartupSpouts(mdStartupSpouts newobj)
        {

            newobj ??= new mdStartupSpouts();


            for (int i = 1; i <= Count; i++)
            {
                mdDowncomer aMem = Item(i);
                if (aMem.IsVirtual) continue;
                mdStartupSpouts spouts = newobj.GetByDowncomerIndex(i, 0);
                    aMem.SetStartupSpouts(spouts);

            }
        }

     


        public void UpdateMaterial(uopSheetMetal aSheetMetal = null)
        {
            if (aSheetMetal == null) return;
            mdDowncomer aMem;
            TMATERIAL newMat = aSheetMetal.Structure;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                aMem.SheetMetalStructure = newMat;
             
            }
        }

        public override void UpdatePartProperties()
        {
            Quantity = Count;
            base.PropSetAttributes("Offset", bOptional: (int)DesignFamily < (int)uppMDDesigns.MDDividedWall);
            base.SubPartProperties();
        }

        public override void UpdatePartWeight()
        {
            base.Weight = 0;
            double wt = 0;
            mdTrayAssembly myAssy = GetMDTrayAssembly();

            mdDowncomer aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                aMem.UpdatePartWeight();
                wt += aMem.Weight(myAssy);
            }
            base.Weight = wt;
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
            if (bJustOne && _rVal.Count > 0) return _rVal;
            if (Count <= 0) return _rVal;
            aAssy ??= GetMDTrayAssembly(aAssy);

            if (aAssy == null) return _rVal;
            aCategory = string.IsNullOrWhiteSpace(aCategory) ? TrayName(true) : aCategory.Trim();

            mdDowncomer aMem;
            string txt = string.Empty;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (!aMem.IsVirtual)
                {
                    aMem.GenerateWarnings(aAssy, aCategory, _rVal);
                    if (bJustOne && _rVal.Count > 0) return _rVal;

                }
            }

            aMem = FirstItem();

            dxePolygon aPg = aMem.View_ManholeFit(aAssy);
            dxeArc aArc = aPg.ExtentPoints.BoundingCircle();

            double ManID = aAssy.ManholeID;
            double clrc = Math.Round(ManID - aArc.Diameter, 3);
            if (clrc <= 0)
            {
                if (aAssy.ProjectType == uppProjectTypes.MDSpout)
                {
                    txt = "Downcomers Will Not Fit Through The Manhole";
                }
                else
                {
                    if (aPg.AdditionalSegments.GetTagged("STIFFENER") == null)
                    {
                        txt = "Downcomers Will Not Fit Through The Manhole";
                    }
                    else
                    {
                        txt = "Downcomers Will Not Fit Through The Manhole With Welded Stiffeners";
                    }
                }
            }
            else if (clrc < 0.5)
            {
                if (aAssy.ProjectType == uppProjectTypes.MDSpout)
                {
                    txt = "Downcomers May Not Fit Through The Manhole (Clearance = " + string.Format("{0:0.0##}", clrc.ToString()) + "'') 0.5'' Required.";
                }
                else
                {
                    if (aPg.AdditionalSegments.GetTagged("STIFFENER") == null)
                    {
                        txt = "Downcomers May Not Fit Through The Manhole (Clearance = " + string.Format("0:0.0##", clrc.ToString()) + "'') 0.5'' Required.";
                    }
                    else
                    {
                        txt = "Downcomers May Not Fit Through The Manhole With Welded Stiffeners (Clearance = " + string.Format("0:0.0##", clrc.ToString()) + "'') 0.5'' Required. Deselect 'Welded Stiffeners' in Design Options";
                    }
                }
            }
            if (txt !=  string.Empty)
            {
                _rVal.AddWarning(aMem, "Downcomer Manhole Access Warning", txt, uppWarningTypes.ReportFatal);
                if (bJustOne && _rVal.Count > 0) return _rVal;
            }
            if (aMem.HeightAboveDeck <= 0)
            {
                _rVal.AddWarning(aMem, "Downcomer Height Warning", "Downcomers Don't Fit Between Rings", uppWarningTypes.ReportFatal);
                if (bJustOne && _rVal.Count > 0) return _rVal;
            }
            return _rVal;
        }

        public virtual List<mdDowncomer> GetByVirtual(bool? aVirtualValue, bool bGetClones = false)
        {
            List<mdDowncomer> _rVal = new List<mdDowncomer>();
            foreach (var item in this)
            {
                if (aVirtualValue.HasValue)
                {
                    if (item.IsVirtual != aVirtualValue.Value) continue;
                }
                mdDowncomer mem = (mdDowncomer)item;
                _rVal.Add(!bGetClones ? mem : mem.Clone());
            }
            return _rVal;
        }

        /// <summary>
        ///returns the properties required to save the downcomers to file
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public override uopPropertyArray SaveProperties(string aHeading = null)
        {
            //^returns the objects properties in a collection
            //~signatures like "COLOR=RED"
            aHeading = string.IsNullOrWhiteSpace(aHeading) ? INIPath : aHeading.Trim();
            UpdatePartProperties();
            mdTrayAssembly aAssy = GetMDTrayAssembly();
            uopProperties props = CurrentProperties();
            props.Add("Count", Count);
            uopProperties dcprops;



            for (int i = 1; i <= Count; i++)
            {
                mdDowncomer mem = Item(i);
                if (mem.IsVirtual) continue;
                dcprops = mem.SaveProperties(aAssy).Item(1);
                props.Add($"Downcomer{i}", dcprops.DeliminatedString(uopGlobals.Delim, bIncludePropertyNames: true), aHeading: aHeading);
            }

            return new uopPropertyArray(props, aName: aHeading, aHeading: aHeading);
        }
        /// <summary>
        /// '^used by members of the collection to inform the collection of change
        /// also raises the changed property to the object above it it the object model.
        /// </summary>
        /// <param name="aMember"></param>
        /// <param name="aProperty"></param>
        public void NotifyMemberChange(mdDowncomer aMember, uopProperty aProperty)
        {
            if (aMember == null) return;
            if (aProperty == null) return;
            if (_Initializing) return;
            if (!SuppressEvents) eventDowncomerMemberChanged?.Invoke(aProperty, aMember.IsGlobal);
        }

        /// <summary>
        /// Updates the property with the given value
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="newValue"></param>
        /// <param name="raiseChange"></param>
        public void UpdateProperty(string propertyName, dynamic newValue, bool raiseChange = false)
        {
            //#1the name of the property to change
            //#2the new value to assign to the property
            //#3flag to allow the property to raise its change event
            //makes sure that a properties of the collection downcomers match the passed value

            mdDowncomer dComer;

            for (int i = 1; i <= Count; i++)
            {
                dComer = Item(i);
                dComer.PropValSet(propertyName, newValue, bSuppressEvnts: !raiseChange);
            }
        }

        public  void SubPart(mdTrayAssembly aAssy, string aCategory = null, bool? bHidden = null)
        {

            base.SubPart(aAssy);
            if (aAssy != null)
            {
               ProjectType = aAssy.ProjectType;

            }

        }
        #endregion Methods
    }
}