using UOP.DXFGraphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Events;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Tables;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;
using System.Linq;

namespace UOP.WinTray.Projects
{
    public class mdSlotZones : List<mdSlotZone>, IEnumerable<mdSlotZone>, ICloneable
    {

        #region Constructors

        public mdSlotZones() => Init();


       public mdSlotZones(mdSlotZones aZones, bool bDontCloneMembers = false) => Init(aZones, bDontCloneMembers: bDontCloneMembers);


        private void Init(mdSlotZones aSlotZones = null, mdTrayAssembly aAssy = null,bool bDontCloneMembers = false)
        {
            _AssyRef = aAssy == null ? null : new WeakReference<mdTrayAssembly>(aAssy);
            ProjectType = uppProjectTypes.Undefined;
            SlottingPercentage = 0;
            WeirHeight = 0;
            AlternateZones = null;
            if (aSlotZones != null)
            {
                ProjectType = aSlotZones.ProjectType;
                ProjectHandle = aSlotZones.ProjectHandle;
                RangeGUID = aSlotZones.RangeGUID;

                FunctionArea = aSlotZones.FunctionArea;
                Invalid = aSlotZones.Invalid;
                SlotType = aSlotZones.SlotType;
                RequiredSlotCount = aSlotZones.RequiredSlotCount;
                _SlottingPercentage = aSlotZones._SlottingPercentage;
                WeirHeight = aSlotZones.WeirHeight;


                if (!bDontCloneMembers)
                {
                    foreach (var zone in aSlotZones)
                        base.Add(new mdSlotZone(zone));
                 
                    if(aSlotZones.AlternateZones != null)
                    {
                        AlternateZones = new List<mdSlotZone>();
                        foreach (var zone in aSlotZones.AlternateZones)
                            AlternateZones.Add(new mdSlotZone(zone));

                    }
                }
            }
        }
       
        #endregion Constructors

        public void SubPart(mdTrayAssembly aAssembly)
        {
            if (aAssembly == null) return;
            _AssyRef = new WeakReference<mdTrayAssembly>(aAssembly);
            RangeGUID = aAssembly.RangeGUID;
            foreach (var item in this)
            {
                item.RangeGUID = RangeGUID;
            }
        }

        public double ActualSlottingPercentage(mdTrayAssembly aAssy)
        {

             aAssy ??= TrayAssembly();

            if (aAssy == null) return 0;


            int sCnt = TotalSlotCount(aAssy);
            double sArea = SlotArea;
            double totArea = aAssy.FunctionalActiveArea;

            return (totArea > 0) ? sArea * sCnt / totArea * 100 : 0;
        }

        #region Properties

        public mdSlot FlowSlot => new mdSlot(SlotType);

        public int RequiredSlotCount { get; internal set; }

        public double FunctionArea { get; set; }

        /// <summary>
        ///   //^the type of slots int the zones
        /// </summary>
        public uppFlowSlotTypes SlotType { get; set; }

        private double _SlottingPercentage;
        public double SlottingPercentage
        {
            get => _SlottingPercentage;
            set
            {
                if (value <= 0)
                    value = 0;

                if (_SlottingPercentage != value)
                {
                    _SlottingPercentage = value;
                    Invalid = true;
                }

            }
        }

        private bool _Invalid = false;

        public bool Invalid { get => _Invalid || Count == 0; set => _Invalid = value; }

        public uppProjectTypes ProjectType { get; set; }

        public string ProjectHandle { get; set; }

        public uppMDDesigns DesignFamily { get; private set; }

        public string RangeGUID { get; set; }

        public double SlotArea => SlotType.SlotArea();

        public bool SlotsRequired => ProjectType == uppProjectTypes.MDDraw && DesignFamily.IsEcmdDesignFamily();

        public double WeirHeight { get; private set; }
        #endregion Properties

        #region Methods
        private WeakReference<mdTrayAssembly> _AssyRef;

        public void SetTrayAssembly(mdTrayAssembly aAssy)
        {
            if (aAssy != null)
            {
                _AssyRef = new WeakReference<mdTrayAssembly>(aAssy);
                DesignFamily = aAssy.DesignFamily;
                ProjectHandle = aAssy.ProjectHandle;
                RangeGUID = aAssy.RangeGUID;
                SlotType = aAssy.Deck.SlotType;
                ProjectType = aAssy.ProjectType;
                _SlottingPercentage = DesignFamily.IsEcmdDesignFamily() ? aAssy.Deck.SlottingPercentage : 0;
                WeirHeight = aAssy.WeirHeight;
            }
            else
            {
                SlotType = uppFlowSlotTypes.FullC;
                DesignFamily = uppMDDesigns.Undefined;
                ProjectHandle = string.Empty;
                RangeGUID = string.Empty;
                _AssyRef = null;
            }
        }

        public mdTrayAssembly TrayAssembly()
        {
            mdTrayAssembly _rVal = null;
            if (_AssyRef == null && !string.IsNullOrWhiteSpace(RangeGUID))
            {
                SetTrayAssembly(uopEvents.RetrieveMDTrayAssembly(RangeGUID));


            }

            if (_AssyRef == null) return _rVal;
            if (!_AssyRef.TryGetTarget(out _rVal)) _AssyRef = null;
            return _rVal;

        }

        /// <summary>
        /// the zones for the alternate ring deck sections
        /// </summary>
        public List<mdSlotZone> AlternateZones { get; set; }

        public List<string> Names()
        {
            List<string> _rVal = new List<string> ();
          
            for (int i = 1; i <= Count; i++)
            {
                mdSlotZone aMem = Item(i);
                _rVal.Add(aMem.Name);
            }
            return _rVal;
        }

        public colDXFVectors GridPointsDXF(mdTrayAssembly aAssy, bool bGreaterThanZero = false, bool bRegen = false, bool bUnsuppressedOnly = true)
        {
            colDXFVectors _rVal = new colDXFVectors();
          
             aAssy ??= TrayAssembly();
            
            if (aAssy == null) return _rVal;
            foreach (mdSlotZone zone in this)
            {
                if (bRegen)
                {
                    zone.Invalid = true;
                }
                if (zone.GridPointCount > 0)
                {
                    if (!bGreaterThanZero)
                    {
                        zone.GridPts.ToDXFVectors(true, aCollector: _rVal, aTag: zone.Name, bUnsuppressOnly: bUnsuppressedOnly);
                    }
                    else
                    {
                        zone.GridPts.ToDXFVectors(true, aMinX: 0, aCollector: _rVal, aTag: zone.Name, bUnsuppressOnly: bUnsuppressedOnly);
                    }
                }
           
            }

            return _rVal;
        }

        public uopVectors GridPoints(mdTrayAssembly aAssy, bool bBothSides = true, bool bRegen = false, bool bUnsuppressedOnly = true)
        {
            uopVectors _rVal = new uopVectors();

            aAssy ??= TrayAssembly();

            if (aAssy == null) return _rVal;
            foreach (mdSlotZone zone in this)
            {
                if (bRegen)
                    zone.Invalid = true;

                if (zone.GridPointCount <= 0) continue;
                List<uopVector> zPts = null;
                bool? supval = null;
                if (bUnsuppressedOnly) supval = false;
                   zPts = zone.GridPoints(bSuppressVal: supval, bGetClones: true);
            
                if (!bBothSides)
                {
                    if (DesignFamily.IsStandardDesignFamily())
                    {
                        zPts = zPts.FindAll((x) => x.X >= 0);
                    }
                    
                }
                _rVal.Append(zPts, bAddClone: false, aTag: zone.Name);
            }
            return _rVal;
        }

        public bool Validate(mdTrayAssembly aAssy, out string rErrorMessage, double aTotalLimitPct = 2, double aPanelLimitPct = 5)
        {
            return Validate(aAssy, out rErrorMessage, out int req, out int act, out double toterr, out int panels, aTotalLimitPct, aPanelLimitPct);
        }

         public bool Validate(mdTrayAssembly aAssy, out string rErrorMessage, out int rRequiredCount, out int rActualCount, out double rTotalErr, out int rErrPanels,  double aTotalLimitPct = 2, double aPanelLimitPct = 5)
        {

            rRequiredCount = 0;
            rActualCount = 0;

            aTotalLimitPct = Math.Abs(aTotalLimitPct);
            if (aTotalLimitPct == 0) aTotalLimitPct = 5;
            aPanelLimitPct = Math.Abs(aPanelLimitPct);
            if (aPanelLimitPct == 0) aPanelLimitPct = 2;

            mdSlotting.SetSlotCounts(aAssy, this, 0, aRaiseEvents: false);

            //mdSlotting.SetSlotCounts(aAssy, aAssy.SlotZonesV, aAssy.FunctionalActiveArea, false, false);

            rTotalErr = 100;
            rErrPanels = 0;
            rErrorMessage = string.Empty;
            aAssy ??= TrayAssembly();
            if (aAssy == null) return false;
            rRequiredCount = RequiredSlotCount;
            rActualCount = TotalSlotCount(aAssy);
            double req = (double)rRequiredCount;
            double tot = (double)rActualCount;
            double dif = tot - req;

            rTotalErr = (req > 0) ? dif / req * 100 : 100;
            if (Math.Abs(rTotalErr) > aTotalLimitPct)
            {
                rErrorMessage = $"Total Tray Slot Count Error {rTotalErr:0.00}% Exceeds {aTotalLimitPct:0.00}%.";
                return false;
            }

            mdDeckPanel dp;
            List<mdDeckPanel> dps = aAssy.DeckPanels.ActivePanels(aAssy, out bool specialcase, out List<int> occurances);
            string errlist = string.Empty;
            for (int i = 1; i <= dps.Count; i++)
            {
                dp = dps[i - 1];

                int cnt1 = dp.TotalRequiredSlotCount;
                //cnt1 *= occurances[i - 1];
                List<mdSlotZone> dpzones = GetByPanelIndex(i);
                int cnt2 = TotalSlotCount(aAssy, aWorkCol: dpzones);
                double cntErr = (cnt1 > 0) ? (double)(cnt2 - cnt1) / cnt1 * 100 : 100;


                if (Math.Abs(cntErr) > aPanelLimitPct)
                {
                    if (errlist !=  string.Empty) errlist += ",";
                    errlist += i.ToString();
                    rErrPanels++;
                }
            }
            if (errlist !=  string.Empty)
            {
                if (rErrPanels > 1)
                {
                    rErrorMessage = $"Panels {errlist} Have Slot Count Errors That Exceed {aPanelLimitPct:0.00}%.";
                }
                else
                {
                    rErrorMessage = $"Panel {errlist} Has a Slot Count Error That Exceeds {aPanelLimitPct:0.00}%.";
                }

                return false;

            }

            return true;
        }


        public int SetSlotCounts(mdTrayAssembly aAssy, double aFunctionalArea = 0, bool aRaiseEvents = false) => mdSlotting.SetSlotCounts(aAssy, this, aFunctionalArea, aRaiseEvents);


        public uopTable GetTable(mdTrayAssembly aAssy, string aTableName, uppUnitFamilies Units, int aPanelIndex = 0)
        {
            uopTable _rVal = new uopTable();

            aAssy ??= TrayAssembly();
            if (aAssy == null) return _rVal;


        
            string tname = aTableName.ToUpper().Trim();

            uopUnit lunits = uopUnits.GetUnit(uppUnitTypes.SmallLength);
            uopUnit aunits = uopUnits.GetUnit(uppUnitTypes.SmallArea);


           
            string units_Area = aunits.Label(Units, true);
            string units_Linear = lunits.Label(Units, true);
            double sAreaFactor = aunits.ConversionFactor(Units);
            double sLinFactor = lunits.ConversionFactor(Units);
            
            string fmt2 = lunits.FormatString(Units);
          
            //=============================================
            if (tname == "PANEL_ZONES")
            {
                //=============================================




                List<mdSlotZone> sZones = GetByPanelIndex(aPanelIndex);
          
                _rVal.SetDimensions(sZones.Count + 1, 7);

                _rVal.SetHeight("All", 190);
                _rVal.SetHeight("1", 600);
                _rVal.SetWidth("All", "600");
                _rVal.SetWidth("1", "450");
                _rVal.SetWidth("6,7", "420");

                List<string> rowdata = new List<string>
                {
                    "Sect.\nNo.",
                    "Panel\n%",
                     $"X\nPitch\n{units_Linear}",
                      $"Y\nPitch\n{units_Linear}",
                       "Pitch\nType",
                       "Req.\nSlots",
                       "Act.\nSlots"
                };

                _rVal.SetByCollection(1, bSetColumn: false, rowdata);
            
            

                for (int i = 1; i <= sZones.Count; i++)
                {
                    mdSlotZone aZone = sZones[i - 1];
                   
                    int cnt1 = aZone.TotalRequiredSlotCount;
                    int cnt2 = aZone.TotalSlotCount;

                    rowdata =  new List<string>
                {
                        aZone.SectionHandle,
                    string.Format("{0:0.00}", aZone.PanelFraction * 100),
                     string.Format(fmt2, aZone.HPitch * sLinFactor),
                    string.Format(fmt2, aZone.VPitch * sLinFactor),
                      $"Y\nPitch\n{units_Linear}",
                        aZone.PitchType == dxxPitchTypes.Rectangular ? "Rect." : "Tri.",
                       cnt1.ToString(),
                       cnt2.ToString()
                };
                    _rVal.SetByCollection(i + 2, bSetColumn: false, rowdata);

                    uopTableCell  aCell = _rVal.Cell(i + 1, 1);

                    aCell.FontColor = ((cnt2 < cnt1 - 1) || (cnt2 > cnt1 + 5)) ? Color.Red : Color.Black; // vbBlack
                    
                }
                _rVal.SetAlignment("All", "All", uopAlignments.MiddleCenter);
                //_rVal.FontSize = 7.5m; // does not exist!!!
                _rVal.SetLocks($"{1}", "All", true);
            }

            //=============================================
            if (tname == "PANELS")
            {
                //=============================================

                List<mdDeckPanel> panels = aAssy.DeckPanels.ActivePanels(aAssy, out bool specialcase, out List<int> occurances);

                _rVal.SetDimensions(panels.Count + 1, 7);

                _rVal.SetHeight(0, 190);
                _rVal.SetHeight(1, 600);

                _rVal.SetWidth("1", "200");
                _rVal.SetWidth("2", "495");
                _rVal.SetWidth("3,4", "645");
                //_rVal.SetWidth("5", "450");
                _rVal.SetWidth("5,6", "400");
                _rVal.SetWidth("7", "465");
                List<string> rowdata = new List<string>
                {
                    "-",
                    "Tray\n%",
                     $"X\nPitch\n{units_Linear}",
                      $"Y\nPitch\n{units_Linear}",
                       "Req\nCnt",
                       "Act\nCnt",
                        "Cnt\nErr\n%"
                };

                _rVal.SetByCollection(1, bSetColumn: false, aDataSet: rowdata);


               // double pArea = aAssy.TotalFreeBubblingArea;
              

                for (int i = 1; i <= panels.Count; i++)
                {
                    mdDeckPanel aDP = panels[i-1];

                    GetPitchs(i, 0, out dynamic pX, out dynamic pY, out dynamic pType);
               
                    string pYString = lunits.UnitValueString(pY, Units, bAddLabel:false, aPrecis: Units == uppUnitFamilies.Metric ? 1 : 3);
                    string pXString = lunits.UnitValueString(pX, Units, bAddLabel: false, aPrecis: Units == uppUnitFamilies.Metric ? 1 : 3);

                    string pTypeString = (pType is dxxPitchTypes) ? (pType == dxxPitchTypes.Rectangular) ? "Rect." :  "Tri." : pType.ToString();
                    //string panlepct = pArea > 0 ? string.Format("{0:0.0}", aDP.MechanicalArea * aDP.OccuranceFactor / pArea * 100) : "";
                    string panlepct = string.Format("{0:0.0}", aDP.TrayFraction  * 100);
                    uopFreeBubblingPanel fbp = aDP.FreeBubblingPanel(aAssy);

                    int cnt1 = aDP.TotalRequiredSlotCount; // /aDP.OccuranceFactor; // * occurances[i - 1];
                    List<mdSlotZone> dpzones = GetByPanelIndex(i);
                    int cnt2 = TotalSlotCount(aAssy, aWorkCol: dpzones);
                    double cntErr = (cnt1 > 0) ? (double)(cnt2 - cnt1) / cnt1 * 100 : 100;
                    rowdata = new List<string>
                {
                    $"{i}",
                    panlepct,
                     pXString,
                     pYString,
                     $"{cnt1}",
                      $"{cnt2}",
                      string.Format("{0:0.0#}",cntErr)
                };
                    _rVal.SetByCollection( i + 1, bSetColumn: false, rowdata);

                    _rVal.Cell(i + 1, 1).FontColor = (Math.Abs(cntErr) > 5) ? Color.Red : Color.Black;
                    _rVal.Cell(i + 1, 7).FontColor = (Math.Abs(cntErr) > 5) ? Color.Red : Color.Black;
                  
                    

                }
                _rVal.SetAlignment("All", "All", uopAlignments.MiddleCenter);
                //_rVal.FontSize = 7.5m; // does not exist
                _rVal.SetLocks("1", "All", true);
            }

            return _rVal;
        }

        public void GetPanelPitches(mdTrayAssembly aAssy , List<int> aPanelIds,out List<double> rXPitches, out List<double> rYPitches, bool bUniqueVals = true)
        {
            rXPitches = new List<double>();
            rYPitches = new List<double>();
             aAssy ??= TrayAssembly();
            if (aAssy == null) return;
            int pcnt = aAssy.DeckPanels.Count;
            
            dynamic xPitch = null;
            dynamic yPitch = null;
            dynamic pType = null;
            if (aPanelIds == null)
            {
                
                if (bUniqueVals)
                {
                    GetPitchs(0, 0, out xPitch, out yPitch, out pType);

                    if (mzUtils.IsNumeric(xPitch)) { rXPitches.Add(mzUtils.VarToDouble(xPitch)); }
                    if (mzUtils.IsNumeric(yPitch)) { rYPitches.Add(mzUtils.VarToDouble(yPitch)); }

                }
                else
                {
                    for (int i = 1; i <= pcnt; i++)
                    {
                        GetPitchs(i, 0, out xPitch,out yPitch, out pType);
                        if (mzUtils.IsNumeric(xPitch)) { rXPitches.Add(mzUtils.VarToDouble(xPitch)); } else { rXPitches.Add(-1); }
                        if (mzUtils.IsNumeric(yPitch)) { rYPitches.Add(mzUtils.VarToDouble(yPitch)); } else { rYPitches.Add(-1); }

                    }


                }



            }
            else
            {
                double val;
                for (int i = 1; i <= aPanelIds.Count; i++)
                {
                    if (aPanelIds[i - 1] > 0 && aPanelIds[i - 1] <= pcnt)
                    {
                        GetPitchs(aPanelIds[i - 1], 0, out xPitch,out yPitch,out  pType);
                        if (bUniqueVals)
                        {

                            if (mzUtils.IsNumeric(xPitch))
                            {
                                val = mzUtils.VarToDouble(xPitch, aPrecis: 4);
                                if (rXPitches.IndexOf(val) < 0) rXPitches.Add(val);

                            }

                            if (mzUtils.IsNumeric(yPitch))
                            {
                                val = mzUtils.VarToDouble(yPitch, aPrecis: 4);
                                if (rYPitches.IndexOf(val) < 0) rYPitches.Add(val);

                            }

                        }
                        else
                        {
                            if (mzUtils.IsNumeric(xPitch)) { rXPitches.Add(mzUtils.VarToDouble(xPitch)); } else { rXPitches.Add(-1); }
                            if (mzUtils.IsNumeric(yPitch)) { rYPitches.Add(mzUtils.VarToDouble(yPitch)); } else { rYPitches.Add(-1); }

                        }


                    }
                }
            }

        }


        public void GetZonePitches(mdTrayAssembly aAssy, List<string> aSectionHandles, out List<double> rXPitches, out List<double> rYPitches, bool bUniqueVals = true)
        {
            rXPitches = new List<double>();
            rYPitches = new List<double>();
             aAssy ??= TrayAssembly();
            if (aAssy == null) return;
            int pcnt = aAssy.DeckPanels.Count;

            dynamic xPitch = null;
            dynamic yPitch = null;
            dynamic pType = null;
            if (aSectionHandles == null)
            {

                if (bUniqueVals)
                {
                    GetPitchs(0, 0, out xPitch, out yPitch, out pType);

                    if (mzUtils.IsNumeric(xPitch)) { rXPitches.Add(mzUtils.VarToDouble(xPitch)); }
                    if (mzUtils.IsNumeric(yPitch)) { rYPitches.Add(mzUtils.VarToDouble(yPitch)); }

                }
                else
                {
                    for (int i = 1; i <= pcnt; i++)
                    {
                        GetPitchs(i, 0, out xPitch, out yPitch, out pType);
                        if (mzUtils.IsNumeric(xPitch)) { rXPitches.Add(mzUtils.VarToDouble(xPitch)); } else { rXPitches.Add(-1); }
                        if (mzUtils.IsNumeric(yPitch)) { rYPitches.Add(mzUtils.VarToDouble(yPitch)); } else { rYPitches.Add(-1); }

                    }


                }



            }
            else
            {
                double val;
                mdSlotZone zone;

                for (int i = 1; i <= aSectionHandles.Count; i++)
                {

                    zone = (!string.IsNullOrWhiteSpace(aSectionHandles[i - 1])) ? GetBySectionHandle(aSectionHandles[i - 1]) : null;


                    if (zone !=null)
                    {
                        xPitch = zone.HPitch;
                        yPitch = zone.VPitch;
                        if (bUniqueVals)
                        {

                            if (mzUtils.IsNumeric(xPitch))
                            {
                                val = mzUtils.VarToDouble(xPitch, aPrecis: 4);
                                if (rXPitches.IndexOf(val) < 0) rXPitches.Add(val);

                            }

                            if (mzUtils.IsNumeric(yPitch))
                            {
                                val = mzUtils.VarToDouble(yPitch, aPrecis: 4);
                                if (rYPitches.IndexOf(val) < 0) rYPitches.Add(val);

                            }

                        }
                        else
                        {
                            if (mzUtils.IsNumeric(xPitch)) { rXPitches.Add(mzUtils.VarToDouble(xPitch)); } else { rXPitches.Add(-1); }
                            if (mzUtils.IsNumeric(yPitch)) { rYPitches.Add(mzUtils.VarToDouble(yPitch)); } else { rYPitches.Add(-1); }

                        }


                    }
                }
            }

        }

        public void GetPitchs(int aPanelIndex, int aSectionIndex, out dynamic rXPitch, out dynamic rYPitch, out dynamic rPitchType)
        {
          

            double pX = 0;

            double pY = 0;
            rXPitch = string.Empty;
            rYPitch = string.Empty;
            rPitchType = dxxPitchTypes.Triangular;
            int j = 0;
            dxxPitchTypes pType = dxxPitchTypes.Triangular;
            foreach (mdSlotZone item in this)
            {
                if (aPanelIndex <= 0 || item.PanelIndex == aPanelIndex)
                {
                    if (aSectionIndex <= 0 || item.SectionIndex == aSectionIndex)
                    {
                        j += 1;
                        if (j == 1)
                        {
                            pX = item.HPitch;
                            pY = item.VPitch;
                            rXPitch = pX;
                            rYPitch = pY;
                            pType = item.PitchType;
                            rPitchType = pType;
                        }
                        else
                        {
                            if (item.HPitch != pX) rXPitch = "Varies";

                            if (item.VPitch != pY) rYPitch = "Varies";

                            if (item.PitchType != pType) rPitchType = "Varies";

                        }
                    }
                }
            }


       
        }

        public int TotalSlotCount(mdTrayAssembly aAssy, List<mdSlotZone> aWorkCol = null)
        {
            int _rVal = 0;
            aAssy ??= TrayAssembly();
           
            if (aAssy == null) return 0;
            aWorkCol ??= this;
         
            foreach (mdSlotZone item in aWorkCol)
            {
                _rVal += item.TotalSlotCount;
            }
           

            return _rVal;
        }

        public mdSlotZones Clone(bool bSuppressMembers = false) => new mdSlotZones(this,bSuppressMembers);
        
        object ICloneable.Clone() => (object)this.Clone(false);


        public mdSlotZone GetBySectionHandle(string aHandle) => Find(x => string.Compare(x.SectionHandle, aHandle,true) == 0);
       
        public mdSlotZone GetByName(string aName)=> GetByName(aName, out int _);
            

        public mdSlotZone GetByName(string aName, out int rIndex)
        {
           
            rIndex =  FindIndex(x => string.Compare(x.Name,aName, ignoreCase:true) ==0) + 1;
            return rIndex <= 0 ? null : Item(rIndex);
            
        }

        public bool IsEqual(mdSlotZones aZones)
        {
            bool _rVal = false;
            if (aZones == null) return _rVal;
            if (aZones.Count != Count) return _rVal;
            if (aZones.SlotType != SlotType) return _rVal;
            
            if (aZones.SlottingPercentage != SlottingPercentage) return _rVal;
            _rVal = true;
            foreach (mdSlotZone aMem in this)
            {
                mdSlotZone bMem = aZones.GetBySectionHandle(aMem.SectionHandle);
                if (bMem == null)
                {
                    _rVal = false;
                    break;

                }
                else
                {
                    if (!aMem.IsEqual(bMem))
                    {
                        _rVal = false;
                        break;
                    }

                }

            }

            return _rVal;
        }

        public List<mdSlotZone> GetByPanelIndex(int aIndex)
        {
            return FindAll((x) => x.PanelIndex == aIndex);
        }

        public List<mdSlotZone> GetByPanelIDs(List<int> aPanelIDs, bool bReturnAllForEmptyList = true)
        {
            List<mdSlotZone> _rVal = new List<mdSlotZone>();
            if (aPanelIDs == null && bReturnAllForEmptyList) aPanelIDs = new List<int>();
            if (aPanelIDs == null) return _rVal;

            for (int i = 1; i <= Count; i++)
            {
                mdSlotZone aMem = Item(i);
                if(bReturnAllForEmptyList && aPanelIDs.Count <= 0)
                {
                    _rVal.Add(aMem);
                }
                else
                {
                    if (aPanelIDs.IndexOf(aMem.PanelIndex) >= 0) _rVal.Add(aMem);
                }
            }
            return _rVal;
        }

        public List<mdSlotZone> GetBySectionHandles(List<string> aSectionHandles, bool bReturnAllForEmptyList = true)
        {
            List<mdSlotZone> _rVal = new List<mdSlotZone>();
            if (aSectionHandles == null && bReturnAllForEmptyList) aSectionHandles = new List<string>();

            if (aSectionHandles == null) return _rVal;


            mdSlotZone aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (bReturnAllForEmptyList && aSectionHandles.Count <= 0)
                {
                    _rVal.Add(aMem);
                }
                else
                {
                    if (aSectionHandles.IndexOf(aMem.SectionHandle) >= 0) _rVal.Add(aMem);
                }


            }
            return _rVal;
        }

        public void ResetSlots(int aPanelIndex)
        {

            foreach (mdSlotZone item in this)
            {
                if (aPanelIndex <= 0 || item.PanelIndex == aPanelIndex)
                {
                    item.RegenSlots = true;
                    item.Clear();
               
                }

            }

           
        }

        public void SetPitches(double aXPitch, double aYPitch, dxxPitchTypes aPitchType, int aPanelIndex, bool bInvalidate)
        {


            foreach (mdSlotZone item in this)
            {
                if (aPanelIndex <= 0 || item.PanelIndex == aPanelIndex)
                {
                    if (bInvalidate) item.RegenSlots = true;


                    if (item.HPitch != aXPitch) item.RegenSlots = true;

                    if (item.HPitch != aYPitch) item.RegenSlots = true;

                    item.HPitch = aXPitch;
                    item.HPitch = aYPitch;
                    if (aXPitch == 0 && aYPitch == 0) item.TotalRequiredSlotCount = 0;
                }

         

            }
        }

   
        public void SetPitches(double aXPitch, double aYPitch, List<string> aSectionHandles, bool bInvalidate)
        {



            foreach (mdSlotZone item in this)
            {
                if (aSectionHandles.IndexOf(item.SectionHandle) >= 0)
                {
                    if (item.HPitch != aXPitch || bInvalidate) item.RegenSlots = true;
                    if (item.VPitch != aYPitch || bInvalidate) item.RegenSlots = true;

                    item.HPitch = aXPitch;
                    item.VPitch = aYPitch;
                    if (aXPitch == 0 && aYPitch == 0) item.TotalRequiredSlotCount = 0;
                }


            }

       
        }


        public mdSlotZone Item(int aIndex)
        {
            if (aIndex < 1 || aIndex > Count) return null;
            mdSlotZone _rVal = base[aIndex - 1];
            _rVal.Index = aIndex;
           _rVal.SlotType = SlotType;
            _rVal.ProjectHandle = ProjectHandle;
            _rVal.RangeGUID = RangeGUID;

            return _rVal;

        }

        public new int IndexOf(mdSlotZone  aMember)
        {
            return aMember == null ? 0 : base.IndexOf(aMember) + 1;
        }




        public string INIPath(mdTrayAssembly aAssy)
        {
             aAssy ??= TrayAssembly();

            return (aAssy == null)? "SLOTZONES" : aAssy.INIPath + ".SLOTZONES";
            
        }

        /// <summary>
        /// returns the properties required to save the slot zones to file
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public  uopPropertyArray SaveProperties(mdTrayAssembly aAssy)
        {
            uopProperties _rVal = new uopProperties();
             aAssy ??= TrayAssembly();
            if (aAssy == null) return new uopPropertyArray();
            

            for (int i = 1; i <= Count; i++)
            {
                mdSlotZone aMem = Item(i);
                _rVal.Add($"ZONE({aMem.SectionHandle})", aMem.SaveProperties.DeliminatedString(",", bIncludePropertyNames: true));
                aMem.GridPointProperties(aPrecis:5, aCollector:_rVal, aMaxLength: 500);
              
            }
            string heading = INIPath(aAssy);
            
            return new uopPropertyArray( _rVal , aName: heading, aHeading: heading);
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
        public void  ReadProperties(uopProject aProject, uopPropertyArray aFileProps, ref uopDocuments ioWarnings, double aFileVersion, string aFileSection = null, bool bIgnoreNotFound = false, uopTrayAssembly aAssy = null, uopPart aPartParameter = null, List<string> aSkipList = null, Dictionary<string, string> EqualNames = null)
        {
            bool memnotfound = false;
            mdTrayAssembly myassy = null;
            try
            {
                ioWarnings ??= new uopDocuments();
                if (aFileProps == null) throw new Exception("The Passed File Property Array is Undefined");
                if (string.IsNullOrWhiteSpace(aFileSection)) throw new Exception("File Section is Undefined");
              
                if (aAssy == null) throw new Exception("Tray Assembly is Undefined");

               myassy = (mdTrayAssembly)aAssy;
                aFileSection = string.IsNullOrWhiteSpace(aFileSection) ? INIPath(myassy) : aFileSection.Trim();
                SetTrayAssembly(myassy);

                uopProperties myprops = aFileProps.Item(aFileSection);
                if (myprops == null || myprops.Count <= 0)
                {
                    ioWarnings?.AddWarning(aAssy, $"{aAssy.TrayName(true)} Slot Zone Data Not Found", $"File '{ aFileProps.Name }' Does Not Contain {aFileSection} Info!");
                    return;
                }

                aProject?.ReadStatus($"Reading { myassy.TrayName(true) } Slot Zone Data");

                foreach (mdSlotZone zone in this)
                {
                    if (!zone.ReadProperties(aFileProps, aFileSection, myassy))
                    {
                       memnotfound = true;
                       if(memnotfound && IndexOf(zone) -1 > 0)
                        {
                            mdSlotZone last = Item(IndexOf(zone) - 1);
                            zone.HPitch = last.HPitch;
                            zone.VPitch = last.VPitch;
                        }
                        if (aFileVersion < 4.1) memnotfound = false; 
                    }
                }
               
                
                Invalid = false;
                

            }
            catch (Exception e)
            {
                ioWarnings?.AddWarning(aAssy, "Read Properties Error", e.Message);
            }
            finally
            {
                if (memnotfound)
                {
                    myassy.Invalidate(uppPartTypes.FlowSlotZone);
                    ioWarnings.AddWarning(myassy, "Incomplete Slot Zone Data", $"Incomplete/Invalid Slot Zone Data For Tray { myassy.TrayName(true) } Detected");
                }
              
                aProject?.ReadStatus("", 2);
            }
        }


       

        /// <summary>
        /// //^flag indicating that something has changed and the zones need to be regenerated
        /// </summary>
        /// 

        public new void Add(mdSlotZone aZone)
        {
            if (aZone == null) return;
            aZone.Index = Count + 1;
             base.Add(aZone);
            
        }

        #endregion Methods
    }
}

