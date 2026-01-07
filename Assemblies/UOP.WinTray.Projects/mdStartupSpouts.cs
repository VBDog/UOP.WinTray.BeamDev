using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Parts;

using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.Projects
{
    public class mdStartupSpouts: List<mdStartupSpout>, IEnumerable<mdStartupSpout>
    {

        private int _SelID = 0;

        #region Constructors

        public mdStartupSpouts()  => Init();


        public mdStartupSpouts(mdStartupSpouts aPartToCopy,  bool bDontCopyMembers = false) 
        { Init(aPartToCopy, bDontCopyMembers); }

        private void Init(IEnumerable<mdStartupSpout> aPartToCopy = null, bool bDontCopyMembers = false)
        {
        

            TargetArea = 0;
            Height = 0;
            Length = 0;
            Invalid = false;
            Locked = false;
            Thickness = 0;
            _SelID = 0;
            Clear();
            if (aPartToCopy == null) return;
            if (aPartToCopy is mdStartupSpouts)
            {
                mdStartupSpouts mdspouts = (mdStartupSpouts)aPartToCopy;
                TargetArea = mdspouts.TargetArea;
                Height = mdspouts.Height;
                Length = mdspouts.Length;
                Invalid = mdspouts.Invalid;
                Locked = mdspouts.Locked;
                Thickness  = mdspouts.Thickness;
            }

            if (bDontCopyMembers) return;

            foreach (var mem in aPartToCopy) base.Add(new mdStartupSpout(mem) { Height = _Height, Length = _Length });


        }
        #endregion Constructors

        #region Properties
        /// <summary>
        /// the ideal total area of the collection of spouts
        /// </summary>
        public double TargetArea { get; set; }
        public uopHole StartupSpout => new uopHole(Height, aLength: Length, aDepth: this.Thickness);
        /// <summary>
        /// returns the collection of slots of all the member spouts
        /// </summary>
        public uopHoles Slots => new uopHoles(SlotsV);


        /// <summary>
        /// the hole area of the slot that is the spout
        /// </summary>
        public double SingleSpoutArea
        {
            get
            {
                if (Height <= 0 || Length <= 0) return 0;

                double rad = 0.5 * _Height;
                double _rVal = Math.PI * Math.Pow(rad, 2);
                if (_Length > _Height) _rVal += (_Length - _Height) * _Height;
                return _rVal;
            }
        }


        /// <summary>
        /// returns the collection of slots of all the member spouts
        /// </summary>
        internal UHOLES SlotsV
        {
            get
            {
                UHOLES _rVal = UHOLES.Null;

                foreach (mdStartupSpout mem in this)
                {
                    if (!mem.Suppressed)
                    {
                        UHOLES aSlts = mem.SlotsV;
                        if (_rVal.Centers.Count == 0)
                        { _rVal = aSlts; }
                        else
                        { _rVal.Centers.Append(aSlts.Centers); }
                    }
                }
                return _rVal;
            }
        }

        private bool _InvalidWhenEmpty = false;
        private bool _Invalid = false;
        public  bool Invalid { get => _InvalidWhenEmpty ? (_Invalid || Count <= 0) : _Invalid; set { if (_Invalid == value) return; _Invalid = value; } }

        private double _Height;

        /// <summary>
        /// the height of the spouts in the collection
        /// </summary>
        public  double Height
        {
            get { if (Count > 0) _Height = Item(1).Height; return _Height; }

            set
            {
                if (_Height != value)
                {

                    foreach (mdStartupSpout mem in this)
                        mem.Height = value;

                }
                _Height = value;
            
            }
        }
        public virtual mdStartupSpout SelectedMember
        {
            get
            {

                _SelID = 0;
                uopPart aMem;
                uopPart _rVal = null;

                for (int i = 1; i <= Count; i++)
                {
                    aMem = this[i - 1];
                    if (aMem.Selected)
                    {
                        if (_rVal == null)
                        {
                            _SelID = i;
                            _rVal = aMem;
                        }
                        else { aMem.Selected = false; }
                    }
                }
                if (_rVal == null && Count > 0)
                {
                    this[0].Selected = true;
                    _SelID = 1;
                }

                return (_SelID > 0) ? Item(_SelID) : null;

            }

        }


        public int SelectedIndex
        {
            get
            {
                uopPart aMem = SelectedMember;
                return _SelID;
            }

            set => SetSelected(value);

        }

        public double Thickness { get; set; }
        public bool Locked { get; set; }

        private double _Length;

        /// <summary>
        /// the length of the spouts in the 
        /// </summary>
        public  double Length
        {
            get { if (Count > 0) _Length = Item(1).Length; return _Length; }

            set
            {
                if (_Length != value)
                {

                    foreach (mdStartupSpout mem in this)
                        mem.Length = value;

                }
                _Length = value;
                
            }
        }

        public string YOrds(bool bNoMirrors = false)
        {
      
                string _rVal = string.Empty;
                mdStartupSpout mem;
                for (int i = 1; i <= Count; i++)
                {
                    mem = Item(i);
                    if (_rVal != string.Empty) _rVal += ",";
                    _rVal += string.Format("{0:0.0###}", mem.Y);
                }
                if(bNoMirrors) return _rVal;
                for (int i = Count; i >= 1; i--)
                {
                    mem = Item(i);
                    if (mem.Mirrored)
                    {
                        if (_rVal != string.Empty) _rVal += ",";
                        _rVal += string.Format("{0:0.0###}", mem.MIrrorY - mem.Y);

                    }
                }
                return _rVal;
           
        }


  
        /// <summary>
        /// the maximum length any spout can be in the collection
        /// the longest unsupressed site length
        /// </summary>
        public double MaxSiteLength
        {
            get
            {
                mdStartupLines cntrlLines = ControlLines(bUnsuppressOnly: true);
                return (cntrlLines.Count > 0) ? uopUtils.RoundTo(cntrlLines.LongestMember(false).MaxLength, dxxRoundToLimits.Sixteenth, false, true) : 0;

            }
        }

        /// <summary>
        /// the minimum length any spout can be in the collection
        /// the shortest unsupressed site length
        /// </summary>
        public double MinSiteLength
        {
            get
            {
                mdStartupLines cntrlLines = ControlLines(bUnsuppressOnly: true);
                return (cntrlLines.Count > 0) ? uopUtils.RoundTo(cntrlLines.ShortestMember(false).Length, dxxRoundToLimits.Sixteenth, false, true) : 0;

            }
        }

        /// <summary>
        /// the total count of startup spouts in the collection
        /// </summary>
        public int TotalCount
        {
            get
            {
                int _rVal = 0;
                try
                {

                    foreach( mdStartupSpout mem in this)
                    {
                        if (!mem.Suppressed) _rVal += mem.OccuranceFactor;

                    }
                    return _rVal;
                }
                catch (Exception exception)
                {
                    LoggerManager log = new LoggerManager();
                    log.LogError(exception.Message);
                    return _rVal;
                }
            }
        }
        /// <summary>
        /// the total open area formed by all the startup spouts in the collection
        /// </summary>
        public double TotalArea { get => Count <= 0 ? 0 : TotalCount * SingleSpoutArea; }

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
                if (value == null) { _AssyRef = null; return; }
                _AssyRef = new WeakReference<mdTrayAssembly>(value);
                Thickness = value.Downcomer().Thickness;
            }
        }
        #endregion Properties

        #region Methods
        /// <summary>
        /// marks the indicated member as the selected part
        /// </summary>

        public  void SetSelected(mdStartupSpout aSpout)
        {
            if (aSpout == null) return;
            SetSelected(base.IndexOf(aSpout) + 1);

        }
        public virtual void SetSelected(int aIndex)
        {
            int j = SelectedIndex;
            for (int i = 1; i <= Count; i++)
            {
                base[i - 1].SetSelected(i == aIndex);

                if (i == aIndex)
                    j = i;
            }

            if (j == 0 && Count > 0)
            {
                base[0].SetSelected(true);
                j = 1;
            }

        }
        public mdStartupSpout Item(int aIndex, bool bGetClone = false)
        {
            if(aIndex < 0 || aIndex > Count) return null;
            base[aIndex - 1].Height = _Height;
            base[aIndex - 1].Length = _Length;
            base[aIndex - 1].Depth= Thickness;

            return !bGetClone ? base[aIndex - 1] : new mdStartupSpout(base[aIndex - 1]);
        }

        /// <summary>
        /// returns a string that describes the collection
        /// like 40 - 0.50 X 1.50
        /// </summary>
        /// <param name="Units">the units to use</param>
        /// <param name="rDeviationPct"></param>
        /// <param name="AllowedDevPct">the percentage value of the ideal that is considered out of spec</param>
        /// <param name="rIsDeviant">returns True if the current collection does not satisfy the ideal</param>
        /// <param name="IncludeCount"></param>
        /// <param name="IncludeArea"></param>
        /// <param name="IncludeUnits"></param>
        /// <returns></returns>
        public string Descriptor(uppUnitFamilies Units, out double rDeviationPct, out bool rIsDeviant, double AllowedDevPct = 5, bool IncludeCount = true, bool IncludeArea = true, bool IncludeUnits = false)
        {
            rDeviationPct = 100;
            string _rVal = "0=0";
            mdStartupSpout suSpout = null;
            string wd = string.Empty;
            string lg = string.Empty;
            int cnt = 0;
            double are = 0;
            string tot = string.Empty;
            double sTarget = 0;
            string tg = string.Empty;
            sTarget = TargetArea;
            AllowedDevPct = mzUtils.LimitedValue(AllowedDevPct, 1, 10);
            rIsDeviant = true;



            if (Count > 0)
            {
                suSpout = Item(1);
                cnt = TotalCount;
                are = TotalArea;
                if (sTarget > 0)
                {
                    rDeviationPct = (are - sTarget) / sTarget * 100;
                    rIsDeviant = rDeviationPct < 0 && (Math.Abs(rDeviationPct) > AllowedDevPct);
                }
                _rVal = IncludeCount ? cnt + " - " : _rVal = string.Empty;

                if (Units == uppUnitFamilies.Metric)
                {
                    wd = string.Format("{0:0.0#}", suSpout.Height * 2.54);
                    lg = string.Format("{0:0.0#}", suSpout.Length * 2.54);
                    tot = string.Format("{0:0.0#}", are * Math.Pow(2.54, 2));
                    tg = string.Format("{0:0.0#}", Math.Pow(sTarget * 2.54, 2));
                }
                else
                {
                    wd = string.Format("{0:0.00##}", suSpout.Height);
                    lg = string.Format("{0:0.00##}", suSpout.Length);
                    tot = string.Format("{0:0.00##}", are);
                    tg = string.Format("{0:0.00##}", sTarget);
                }
                if (IncludeUnits)
                {
                    if (Units == uppUnitFamilies.English)
                    {
                        wd += " in";
                        lg += " in";
                        tot += " in" + (char)178;
                    }
                    else
                    {
                        wd += " cm";
                        lg += " cm";
                        tot += " cm" + (char)178;
                    }
                }
                _rVal = _rVal + wd + " X " + lg;
                if (IncludeArea)
                {
                    _rVal = _rVal + " = " + tot;
                }
            }
            return _rVal;
        }
        
        public void SetObscured(mdTrayAssembly aAssy, colUOPParts aAssyStiffeners = null)
        {
            if (Count <= 0) return;
            
            aAssy ??= TrayAssembly;
            if (aAssy == null) return;
            
            aAssyStiffeners ??= colUOPParts.FromPartsList(mdPartGenerator.Stiffeners_ASSY(aAssy, false));
            
             colMDDowncomers DComers = aAssy.Downcomers;
            double aAllow = 0.25;
            
         
            for (int i = 1; i <= DComers.Count; i++)
            {
                mdDowncomer DComer = DComers.Item(i);

                mdStartupSpouts DCSpts = GetByDowncomerIndex(i, 0);
                List<uopPart> dcStfs = aAssyStiffeners.GetByDowncomerIndex(i);
                for (int j = 1; j <= DCSpts.Count; j++)
                {
                   mdStartupSpout  aSU = DCSpts.Item(j);
                    aSU.Obscured = false;
                    if (!aSU.Suppressed)
                    {
                        double cY = aSU.Y;
                        double y1 = cY + (0.5 * aSU.Length + aAllow);
                        double y2 = cY - (0.5 * aSU.Length + aAllow);
                        for (int k = 0; k < dcStfs.Count ; k++)
                        {
                            mdStiffener aStf = (mdStiffener)dcStfs[k];
                            //see if the su is blocked by the stiffeners flange
                            double topY = aStf.Y + 0.75;
                            double botY = aStf.Y - 0.5;
                            if ((cY >= botY && cY <= topY) || (y1 >= botY && y1 <= topY) || (y2 >= botY && y2 <= topY))
                            {
                                aSU.Obscured = true;
                            }
                            else
                            {
                                UVECTORS fCPts = aStf.FingerClipPts;
                                for (int m = 1; m <= fCPts.Count; m++)
                                {
                                    UVECTOR aFC = fCPts.Item(m);
                                    if ((aFC.Tag == "LEFT" && aSU.LeftSide) || (aFC.Tag == "RIGHT" && !aSU.LeftSide))
                                    {
                                        topY = aFC.Y + 0.5 * mdFingerClip.DefaultLength;
                                        botY = aFC.Y - 0.5 * mdFingerClip.DefaultLength;
                                        if ((cY >= botY && cY <= topY) || (y1 >= botY && y1 <= topY) || (y2 >= botY && y2 <= topY))
                                        {
                                            aSU.Obscured = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (aSU.Obscured)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// sets the properties of the spouts specific to the passed downcomer
        /// the depth and X ordinate of the spout can vary because all downcomers may not be the same material
        /// </summary>
        /// <param name="aDowncomer"></param>
        /// <param name="aAssy"></param>
        public void SetSpoutDowncomerProperties(mdDowncomerBox aDowncomer, mdTrayAssembly aAssy)
        {
            if (aDowncomer == null) return;
            aAssy ??= aDowncomer.GetMDTrayAssembly();
            mdStartupSpouts DCSpouts = GetByDowncomerIndex(aDowncomer.Index, 0);
            mdStartupSpout mem = null;
            double thk = aDowncomer.Thickness;
            double cX = aDowncomer.X;
            double cL = cX - 0.5 * aDowncomer.Width + 0.5 * thk;
            double cR = cX + 0.5 * aDowncomer.Width - 0.5 * thk;
            double cZ = (aAssy != null) ? aAssy.Deck.Thickness : aDowncomer.DeckThickness;
            for (int i = 1; i <= DCSpouts.Count; i++)
            {
                mem = DCSpouts.Item(i);
                mem.Depth = thk;
                mem.X = mem.LeftSide ? cL : cR;
                mem.Z = cZ + 0.5 * mem.Height;
            }
        }

        public bool IsEqual(mdStartupSpouts aSpouts)
        {
            if (aSpouts == null) return false;

            if (aSpouts.Count != Count || aSpouts.Height != Height || aSpouts.Length != Length) return false;
            mdStartupSpout mine;
            mdStartupSpout hers;

            for (int i = 1; i <= Count; i++)
            {
                mine = Item(i);
                hers = aSpouts.Item(i);
                if (string.Compare(mine.Handle, hers.Handle, ignoreCase: true) != 0)
                    return false;
                if (mine.Suppressed != hers.Suppressed)
                    return false;

                if (mine.X != hers.X)
                    return false;
                if (mine.Y != hers.Y)
                    return false;



            }


            return true;


        }

        public bool IsEqual(mdStartupSpouts aSpouts, bool bIncludeSuppressed)
        {
            if (aSpouts == null) return false;


            mdStartupSpout bMem;
            List<mdStartupSpout> bSpouts = new List<mdStartupSpout>();
            int cnt1 = 0;
            int cnt2 = 0;
            for (int j = 1; j <= aSpouts.Count; j++)
            {
                bMem = aSpouts.Item(j);
                if (bIncludeSuppressed || (!bIncludeSuppressed && !bMem.Suppressed))
                {
                    bSpouts.Add(bMem);
                }
            }



            foreach (mdStartupSpout mem in this)
            {
                if (bIncludeSuppressed || (!bIncludeSuppressed && !mem.Suppressed))
                {
                    cnt1 += 1;
                    for (int j = 0; j < bSpouts.Count; j++)
                    {
                        bMem = bSpouts[j];
                        if (mem.Suppressed == bMem.Suppressed)
                        {
                            if (Math.Round(mem.Length - bMem.Length, 3) == 0)
                            {
                                if (Math.Round(mem.Height - bMem.Height, 3) == 0)
                                {
                                    if (mem.Center.DistanceTo(bMem.Center, 3) == 0)
                                    {
                                        cnt2 += 1;
                                        bSpouts.RemoveAt(j);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return (cnt1 == cnt2) && bSpouts.Count == 0;
        }
        /// <summary>
        /// Get Current Ordinates
        /// </summary>
        /// <param name="aSuppressedVal"></param>
        /// <returns></returns>
        public Dictionary<string, double> CurrentOrdinates(bool aSuppressedVal = false)
        {
            Dictionary<string, double> _rVal = new Dictionary<string, double>();
            try
            {
                mdStartupSpout mem;
                bool bSup = false;
                bool bBySup = false;
                bBySup = true;
                bSup = aSuppressedVal;
                for (int i = 1; i <= Count; i++)
                {
                    mem =Item(i);
                    if (!bBySup || mem.Suppressed == bSup) _rVal.Add(mem.Handle, mem.Y);

                }
                return _rVal;
            }
            catch (Exception exception)
            {
                LoggerManager log = new LoggerManager();
                log.LogError(exception.Message);
                return _rVal;
            }
        }
        public void UpdateYLimits(mdTrayAssembly aAssy, colMDDowncomers aDowncomers)
        {
            aAssy ??= TrayAssembly;
            if (aAssy == null) return;
            if (aDowncomers == null) aDowncomers = aAssy.Downcomers;
            if (aDowncomers == null) return;

            string sID;
            URECTANGLE aLims = URECTANGLE.Null;
            double lng = Length / 2;

            mdStartupLine aCntrlLine;
            mdStartupLine bCntrlLine;
            double YTop;
            double YBot;

            aLims.Bottom = aAssy.DeckPanels.LastPanel().Left(false) - 0.5 * aAssy.Downcomer().BoxWidth;
            for (int i = 1; i <= aDowncomers.Count; i++)
            {
                mdDowncomer aDC = aDowncomers.Item(i);
                if (aDC.IsVirtual) continue;

                List<mdDowncomerBox> boxes = aDC.Boxes.FindAll((x) => !x.IsVirtual);

                foreach (mdDowncomerBox box in boxes)
                {

                    //get the end angles to set the top limit
                    List<mdEndAngle> dcEndAngs = mdPartGenerator.EndAngles_DCBox(box);
                    for (int j = 1; j <= 2; j++)
                    {
                        mdEndAngle aEA = null;
                        mdEndAngle bEA = null;

                        //set the top limit using the end angle on the appropriate side
                        if (j == 1)
                        {
                            sID = "RIGHT";
                            aEA = dcEndAngs.Find((x) => x.Side == uppSides.Right && x.End == uppSides.Top);
                            bEA = dcEndAngs.Find((x) => x.Side == uppSides.Right && x.End == uppSides.Bottom);
                        }
                        else
                        {
                            sID = "LEFT";
                            aEA = dcEndAngs.Find((x) => x.Side == uppSides.Left && x.End == uppSides.Top);
                            bEA = dcEndAngs.Find((x) => x.Side == uppSides.Left && x.End == uppSides.Bottom);
                        }
                        if (aEA == null) continue;
                        aLims.Top = aEA.Y - 0.5 * aEA.Length - lng - 0.25;
                        aLims.Bottom = bEA.Y + 0.5 * bEA.Length + lng + 0.25;
                        //get the startups along the side of the downcomer (right or left) sorted to to bottom
                        mdStartupSpouts aSUs = GetByDowncomerIndex(i, box.Index, sID, aSortOrder: mzSortOrders.HighToLow);
                        //loop on the startups
                        for (int k = 1; k <= aSUs.Count; k++)
                        {
                            mdStartupSpout aSU = aSUs.Item(k);
                            aCntrlLine = aSU.ControlLine;
                            if (aSU.Suppressed)
                            {
                                //just keep the suppresed ones limited to their control line
                                YTop = aCntrlLine.sp.Y - lng;
                                YBot = aCntrlLine.ep.Y + lng;
                            }
                            else
                            {
                                //default to full range
                                YTop = aLims.Top;
                                YBot = aLims.Bottom;
                                //loop upward to find the first unsuppressed member
                                for (int m = k - 1; m >= 1; m--)
                                {
                                    mdStartupSpout bSU = aSUs.Item(m);
                                    bCntrlLine = bSU.ControlLine;
                                    if (!bSU.Suppressed)
                                    {
                                        //limit the su location to no closer than 0.25'' to the one above
                                        YTop = bSU.Y - (2 * lng + 0.25);
                                        break;
                                    }
                                }

                                //loop down to find the first unsuppressed member
                                for (int m = k + 1; m < aSUs.Count; m++)
                                {
                                    mdStartupSpout bSU = aSUs.Item(m);
                                    bCntrlLine = bSU.ControlLine;
                                    if (!bSU.Suppressed)
                                    {
                                        //limit the su location to no closer than 0.25'' to the below
                                        YBot = bSU.Y + (2 * lng + 0.25);
                                        break;
                                    }
                                }
                            }

                            if (YTop > aLims.Top)
                                YTop = aLims.Top;
                            if (YBot > aLims.Bottom)
                                YBot = aLims.Bottom;


                            //set the limits for the su
                            aSU.MaxY = YTop;
                            aSU.MinY = YBot;
                        }
                    }
                }


            }
        }


        /// <summary>
        /// sets the z ordinate of all the spouts in the collection to the passed value
        /// </summary>
        /// <param name="aZ"></param>
        public void SetZ(ref double aZ)
        {

            foreach (mdStartupSpout mem in this) mem.Z = aZ;
        }

        /// <summary>
        /// returns a collection of strings that are warnings about possible problems with
        /// the current tray assembly design.
        /// these warnings may or may not be fatal problems.
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aCategory"></param>
        /// <returns></returns>
        public uopDocuments GenerateWarnings(mdTrayAssembly aAssy, string aCategory = "", uopDocuments aCollector = null, bool bJustOne = false)
        {
            uopDocuments _rVal = aCollector ?? new uopDocuments();
            if (bJustOne && _rVal.Count > 0) return _rVal;
            bool done = false;
            string txt = string.Empty;
            aAssy ??= TrayAssembly;

            if (aAssy == null) return _rVal;


            aCategory = string.IsNullOrWhiteSpace(aCategory) ? aAssy.TrayName(true) : aCategory.Trim();
            if (Count == 0)
            {
                txt = "No Startup Spouts Defined";
                _rVal.AddWarning(aAssy, "Startup Spout Warning", txt, uppWarningTypes.General, "Startup Spouts", aCategory);
                if (bJustOne && _rVal.Count > 0) return _rVal;
                done = true;
            }
            if (!done)
            {
                if ((100 + Deviation()) < 95)
                {
                    txt = "Startup Spout Area Is Less Than 95% of Ideal";
                    _rVal.AddWarning(aAssy, "Startup Spout Warning", txt, uppWarningTypes.General, $"{aAssy.TrayName()} - Startup Spouts", aCategory);
                    if (bJustOne && _rVal.Count > 0) return _rVal;
                }

                if (aAssy.ProjectType == uppProjectTypes.MDDraw)
                {
                    SetObscured(aAssy);

                    foreach (mdStartupSpout mem in this)
                    {
                        if (mem.Obscured)
                        {
                            txt = "Finger Clips Obscure Some Startup Spouts";
                            _rVal.AddWarning(aAssy, "Obscured Startups Warning", txt, uppWarningTypes.General, $"{aAssy.TrayName()} - Startup Spouts", aCategory);
                            break;
                        }
                    }
                    if (bJustOne && _rVal.Count > 0) return _rVal;
                }
            }
            return _rVal;
        }


        /// <summary>
        /// returns the percentage the current total area differs from the ideal
        /// </summary>
        /// <param name="AllowedDevPct">the percentage value of the ideal that is considered out of spec</param>
        /// <param name="IsDeviant">returns True if the current collection does not satisfy the ideal</param>
        /// <returns></returns>
        public double Deviation(double AllowedDevPct = 5, bool IsDeviant = false)
        {
            double _rVal = 0;
            double sTarget = 0;
            double are = 0;
            sTarget = TargetArea;
            AllowedDevPct = mzUtils.LimitedValue(AllowedDevPct, 1, 10);
            _rVal = -100;
            IsDeviant = true;

            are = TotalArea;
            if (sTarget > 0 & are > 0)
            {
                _rVal = (are - sTarget) / sTarget * 100;
                IsDeviant = _rVal < 0 & (Math.Abs(_rVal) > AllowedDevPct);
            }
            return _rVal;
        }
        /// <summary>
        /// returns a spout from the collection whose center is the passed point
        /// the point must actually be the spouts center not just equivalent to the spouts center coordinates
        /// </summary>
        /// <param name="aCenter">the center to search for</param>
        /// <param name="SearchCol">an optional collection of spouts to search other than the current collection</param>
        /// <returns></returns>
        public mdStartupSpout GetByCenter(iVector aCenter, IEnumerable<mdStartupSpout> SearchCol = null)
        {
            if (aCenter == null) return null;

            IEnumerable<mdStartupSpout> sCol = SearchCol ?? this;


            UVECTOR u1 = new UVECTOR(aCenter.X, aCenter.Y);
            foreach (mdStartupSpout s in sCol)
            {

                if (u1.DistanceTo(s.Center, 2) == 0) return s;

            }
            return null;
        }

        /// <summary>
        /// returns an new object with the same properties as the cloned object
        /// </summary>
        /// <param name="bNoMembers"></param>
        /// <param name="bUnlock"></param>
        /// <returns></returns>
        public mdStartupSpouts Clone(bool bNoMembers = false, bool bUnlock = false)
        {
            mdStartupSpouts _rVal = new mdStartupSpouts(this, bDontCopyMembers: bNoMembers);
            if (bUnlock) _rVal.Locked = false;
            return _rVal;
        }


        /// <summary>
        /// returns all the spouts that have a downcomer index matching the passed value
        /// </summary>
        /// <param name="aDCIndex">the index to search for</param>
        /// <param name="aSide"></param>
        /// <param name="bGetClones"></param>
        /// <param name="aSortOrder"></param>
        /// <returns></returns>
        public mdStartupSpouts GetByDowncomerIndex(int aDCIndex, int aBoxIndex, string aSide = "", bool bGetClones = false, mzSortOrders aSortOrder = mzSortOrders.None)
        {
            mdStartupSpouts _rVal = Clone(true);

            bool bKeep = false;
            aSide = string.IsNullOrWhiteSpace(aSide) ? string.Empty : aSide.Trim().ToUpper();

            foreach (mdStartupSpout mem in this)
            {
                bKeep = mem.DowncomerIndex == aDCIndex;
                if (aBoxIndex > 0) bKeep = bKeep && mem.BoxIndex == aBoxIndex;
                if (aSide != string.Empty)
                {
                    if ((aSide == "LEFT" && !mem.LeftSide) || (aSide == "RIGHT" && mem.LeftSide)) bKeep = false;

                }
                if (bKeep) _rVal.Add(mem, bGetClones);


            }
            if (aSortOrder != mzSortOrders.None) _rVal =  mdStartupSpouts.Sort(_rVal, aSortOrder);


            return _rVal;
        }

        /// <summary>
        /// won't add "Nothing" (no error raised).
        /// won't add a spout that doesn't have a spout group associated to 
        /// </summary>
        /// <param name="aSpout">1the item to add to the collection</param>
        /// <param name="bAddClone"></param>
        /// <returns></returns>
        public mdStartupSpout Add(mdStartupSpout aSpout, bool bAddClone = false)
        {if(aSpout == null) return null;
            base.Add(!bAddClone ? aSpout: new mdStartupSpout(aSpout)) ;
            return Item(Count);
        }


        /// <summary>
        /// used to add the members of the passed collection to this collection
        /// </summary>
        /// <param name="aSpouts">the collection of startup spouts to append to this one</param>
        public void Append(IEnumerable<mdStartupSpout> aSpouts, bool bAddClones = false) 
        { 
        if(aSpouts == null) return;
            foreach (var aSpout in aSpouts) Add(aSpout, bAddClones);
        }

        public List<mdStartupSpout> ToList(bool bGetClones = false, bool? aSuppressedVal = null)
        {

            List<mdStartupSpout> _rVal = new List<mdStartupSpout>();

            foreach (var part in this)
            {
                if (aSuppressedVal.HasValue)
                {
                    if (part.Suppressed != aSuppressedVal.Value) continue;
                }
                 _rVal.Add(!bGetClones ?part : new mdStartupSpout(part));
            }
            return _rVal;
        }
        public mdStartupLines ControlLines(bool bUnsuppressOnly = false)
        {
            mdStartupLines _rVal = new mdStartupLines();

            foreach (mdStartupSpout mem in this)
            {
                if (!bUnsuppressOnly || (bUnsuppressOnly && !mem.Suppressed)) _rVal.Add(mem.ControlLine);

            }
            return _rVal;
        }
        public void GenerateByLines(mdTrayAssembly aAssy, mdStartupLines aControlLines, colMDSpoutGroups aSpoutGroups, bool bDontSetYVals = false, Dictionary<string, double> aLocations = null)
        {
            try
            {
                base.Clear();
                Height = 0;
                Length = 0;
                if (aControlLines == null) return;

                aAssy ??= TrayAssembly;
                if (aAssy == null) return;

                //create the collection based on the unsupressed lines

                colMDSpoutGroups sGroups = aSpoutGroups == null ? aAssy.SpoutGroups : sGroups = aSpoutGroups;
                double mirlim = aAssy.OddDowncomers ? 0 : 0.5 * aAssy.DeckSectionWidth(false);
                double aVal = 0;
                SubPart(aAssy);

                double ht = aControlLines.SpoutHeight;
                if (ht <= 0) ht = aAssy.StartupDiameter;
                if (ht <= 0) ht = aAssy.MetricSpouting ? 10 / 25.4 : 0.375;
                double lg = aControlLines.SpoutLength;
                if (lg <= 0) lg = aAssy.StartupLength;
                if (lg < ht) lg = ht;

                mdStartupSpout aSU = new mdStartupSpout(ht, lg) { Z = aAssy.Deck.Thickness + ht / 2 };

                aSU.SubPart(aAssy);
                for (int i = 1; i <= sGroups.Count; i++)
                {
                    mdSpoutGroup aSG = sGroups.Item(i);
                    if (aSG.IsVirtual)
                    {
                        aSG.StartupLines = new mdStartupLines();
                        continue;
                    }

                    List<mdStartupLine> SULines = aControlLines.FindAll((x) => string.Compare(x.SpoutGroupHandle, aSG.Handle, true) == 0);
                    foreach (mdStartupLine aLine in SULines)
                    {
                        if (aLine.MaxLength < lg) aLine.Suppressed = true;
                        mdStartupSpout bSU = new mdStartupSpout(aSU);
                        if (bDontSetYVals) bSU.Y = aLine.RefPtY;

                        bSU.SetProps(aAssy, aLine, aSG, bDontSetYVals, mirlim);
                        bSU.Index = Count + 1;
                        if (aLocations != null)
                        {
                            if (aLocations.TryGetValue(bSU.Handle, out aVal)) bSU.Y = aVal;

                        }
                        Add(bSU);
                    }
                    aSG.StartupLines = new mdStartupLines(SULines);
                }
                _Length = lg;
                _Height = ht;
                Invalid = false;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void SubPart(mdTrayAssembly aAssy)
        {
            if (aAssy == null) return;
            TrayAssembly = aAssy;
        }


        /// <summary>
        /// returns then first startup with a handle matching the passed value
        /// </summary>
        /// <param name="aHandle">the handle to search for</param>
        /// <param name="HandleSpouts">returns all then spouts with the passed handle</param>
        /// <returns></returns>
        public mdStartupSpout GetByHandle(string aHandle) => string.IsNullOrWhiteSpace(aHandle) ? null :  Find(x => string.Compare(x.Handle, aHandle, true)  == 0);
        


        /// <summary>
        /// returns then first startup with a handle matching the passed value
        /// </summary>
        /// <param name="aHandle">the handle to search for</param>
        /// <param name="rHandleSpouts">returns all then spouts with the passed handle</param>
        /// <returns></returns>
        public mdStartupSpout GetByHandle(string aHandle, out mdStartupSpouts rHandleSpouts)
        {
            mdStartupSpout _rVal = null;
            rHandleSpouts = Clone(true);

            foreach (mdStartupSpout mem in this)
            {
                if (string.Compare(mem.Handle, aHandle, ignoreCase: true) == 0)
                {
                    if (_rVal == null) _rVal = mem;

                    rHandleSpouts.Add(mem);
                }
            }
            return _rVal;
        }

        /// <summary>
        /// returns the members that have a leftside property equal to thte passed value
        /// </summary>
        /// <param name="aLeftSide"></param>
        /// <param name="bUnsuppressedOnly"></param>
        /// <returns></returns>
        public mdStartupSpouts GetBySide(bool aLeftSide, bool bUnsuppressedOnly)
        {
            mdStartupSpouts _rVal = Clone(true);

            foreach (mdStartupSpout mem in this)
            {
                if (mem.LeftSide == aLeftSide)
                {
                    if (!bUnsuppressedOnly || (bUnsuppressedOnly && !mem.Suppressed)) _rVal.Add(mem);
                }
            }
            return _rVal;
        }

        /// <summary>
        /// returns the members that have a leftside property equal to thte passed value
        /// </summary>
        /// <param name="aSuppressValue"></param>
        /// <returns></returns>
        public mdStartupSpouts GetBySuppressed(bool aSuppressValue)
        {
            mdStartupSpouts _rVal = Clone(true);

            foreach (mdStartupSpout mem in this)
            {
                if (mem.Suppressed == aSuppressValue) _rVal.Add(mem);
            }
            return _rVal;
        }

        /// <summary>
        /// returns a spout from the collection whose properties or position in the collection match the passed control flag
        /// </summary>
        /// <param name="ControlFlag">flag indicating what type of spout to search for</param>
        /// <param name="Ordinate">the ordinate to search for if the search is ordinate relative</param>
        /// <param name="SearchCol">an optional collection of spouts to search other than the current collection</param>
        /// <returns></returns>
        public mdStartupSpout GetSpout(dxxPointFilters ControlFlag, double Ordinate = 0, IEnumerable<mdStartupSpout> SearchCol = null)
        {

            uopVector ctr = mdStartupSpouts.GetCenters(SearchCol == null ? this : SearchCol).GetVector(ControlFlag, Ordinate, aPrecis: 3);
            return GetByCenter(ctr, SearchCol);
        }
        /// <summary>
        /// returns the spouts that match the passed obscured and suppressed values
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aObscValue"></param>
        /// <param name="aSuppressVal"></param>
        /// <returns></returns>
        public mdStartupSpouts GetByObscured(mdTrayAssembly aAssy, bool aObscValue, bool aSuppressVal) => GetByObscured(aAssy, aObscValue, aSuppressVal, null, out _);


        /// <summary>
        /// returns then spouts that match the passed obscured and suppressed values
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aObscValue"></param>
        /// <param name="aSuppressVal"></param>
        /// <param name="aStiffeners"></param>
        /// <param name="rTheOthers"></param>
        /// <returns></returns>
        public mdStartupSpouts GetByObscured(mdTrayAssembly aAssy, bool aObscValue, bool aSuppressVal, colUOPParts aStiffeners, out mdStartupSpouts rTheOthers)
        {
            mdStartupSpouts _rVal = Clone(true);
            rTheOthers = Clone(true);
            aAssy ??= TrayAssembly;
            if (aAssy == null) return _rVal;



            SetObscured(aAssy, aStiffeners);

            foreach (mdStartupSpout mem in this)
            {
                bool bKeep = false;
                if (mem.Suppressed == aSuppressVal)
                {
                    if (mem.Obscured == aObscValue) bKeep = true;

                }
                if (bKeep)
                { _rVal.Add(mem); }

                else
                { rTheOthers.Add(mem, true); }
            }
            return _rVal;
        }

        /// <summary>
        /// returns the centers of all the holes in the collection
        /// </summary>
        /// <param name="aXOffset"></param>
        /// <param name="aSide"></param>
        /// <returns></returns>
        public uopVectors Centers( uppSides aSide = uppSides.Undefined, double aXOffset = 0) => mdStartupSpouts.GetCenters(this,aSide,aXOffset);
  
        #endregion Methods

        #region Shared Methods

        public static mdStartupSpouts Sort(mdStartupSpouts aSpouts, mzSortOrders aSortOrder, bool? aSupVal = null)
        {
            if (aSpouts == null) return null;

            mdStartupSpouts _rVal = aSpouts.Clone(bNoMembers: true);

            mdStartupSpout mem;
            TVALUES aVals = new TVALUES();
            TVALUES aIds = new TVALUES();
            for (int i = 1; i <= aSpouts.Count; i++)
            {
                mem = aSpouts.Item(i);
                if (aSupVal.HasValue) mem.Suppressed = aSupVal.Value;

                aVals.Add(mem.Y);
                aIds.Add(i);
            }

            if (aSortOrder != mzSortOrders.None) aIds = aVals.SortWithIDs(true, aSortOrder == mzSortOrders.HighToLow, bNumeric: true);



            for (int i = 1; i <= aIds.Count; i++)
            {
                _rVal.Add(aSpouts.Item(aIds.Item(i), true));
            }
            return _rVal;
        }

        public static uopVectors GetCenters(IEnumerable<mdStartupSpout> aSpouts = null, uppSides aSide = uppSides.Undefined, double aXOffset = 0)
        {
            uopVectors _rVal =uopVectors.Zero;
            if (aSpouts == null) return _rVal;

   

            foreach (mdStartupSpout s in aSpouts)
            {
                if (aSide == uppSides.Undefined || s.Side == aSide)
                {
                    uopVector u1 = new uopVector(s.Center)
                    {
                        PartIndex = s.DowncomerIndex,
                        Suppressed = s.Suppressed,
                        Tag = s.Handle,
                        Flag = s.LeftSide ? "LEFT" : "RIGHT",
                        Mark = s.Obscured,
                        Value = s.LeftSide ? 90 : 270
                    };
                    if (aXOffset != 0)
                    {
                        u1.Move(s.LeftSide ? -1 : 1 * Math.Abs(aXOffset));
                    }
                    _rVal.Add(u1);
                }
            }
            return _rVal;
        }

        #endregion Shared Methods
    }
}
