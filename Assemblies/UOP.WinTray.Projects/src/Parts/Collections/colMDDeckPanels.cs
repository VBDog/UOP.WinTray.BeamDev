using UOP.DXFGraphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Materials;
using UOP.WinTray.Projects.Utilities;
using System.Linq;

namespace UOP.WinTray.Projects.Parts
{
    /// <summary>
    /// collection of mdDeckPanel objects
    /// 
    /// </summary>
    public class colMDDeckPanels : uopParts, IEnumerable<mdDeckPanel>
    {

        public override uppPartTypes BasePartType => uppPartTypes.DeckPanels;


        public delegate void MDPanelMemberChangedHandler(uopProperty aProperty);
        public event MDPanelMemberChangedHandler eventMDPanelMemberChanged;


        #region Constructors

        public colMDDeckPanels() : base(uppPartTypes.DeckPanels, uppProjectFamilies.uopFamMD, bBaseOne: true, bMaintainIndices: true) { }

        internal colMDDeckPanels(colMDDeckPanels aPartToCopy, uopPart aParent = null, bool bDontCopyMembers = false) : base(aPartToCopy, bDontCopyMembers, aParent) { }

        internal colMDDeckPanels(List<mdDeckPanel> aPanels, uopPart aParent = null) : base(null, true, aParent: aParent)
        {
            if (aPanels == null) return;
            foreach (mdDeckPanel item in aPanels)
            {
                Add(item, false);
            }
        }


        #endregion Constructors

        #region IEnumerable Implementation

        public new IEnumerator<mdDeckPanel> GetEnumerator() { foreach (uopPart part in Members) { yield return (mdDeckPanel)part; } }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion IEnumerable Implementation
        /// <summary>
        ///returns the panel marked as selected from the collection
        /// </summary>
        public new mdDeckPanel SelectedMember => (mdDeckPanel)base.SelectedMember;

        public new mdDeckPanel LastItem => (mdDeckPanel)base.LastItem();

        public new mdDeckPanel FirstItem => (mdDeckPanel)base.FirstItem();

        public override int Count { get => base.Count; }


        /// <summary>
        /// the number of slots required tray wide
        /// </summary>
        public int RequiredSlotCount
        {
            get
            {
                int _rVal = 0;
                mdDeckPanel aMem;
                for (int i = 1; i <= Count; i++)
                {
                    aMem = Item(i);
                    _rVal += aMem.RequiredSlotCount;
                }
                return _rVal;
            }
        }

        /// <summary>
        /// the radius of the ring that supports this panel
        /// </summary>
        public  override double RingRadius
        {
            get
            {
                if (RingID <= 0)
                {

                    mdTrayRange aRange = GetMDRange();
                    if (aRange != null) RingID = aRange.RingID;
                }
                return RingID / 2;
            }
        }
        /// <summary>
        /// the deck splices of the parent tray assembly of the deck panels collection
        /// </summary>
        public uopDeckSplices Splices
        {
            get
            {
                mdTrayAssembly aAssy = GetMDTrayAssembly();
                return aAssy?.DeckSplices;

            }
        }

        public Collection<mdDeckPanel> ToCollection
        {
            get
            {
                Collection<mdDeckPanel> _rVal = new Collection<mdDeckPanel>();
                for (int i = 1; i <= Count; i++) { _rVal.Add(Item(i)); }
                return _rVal;
            }
        }


        /// <summary>
        ///returns the total deck panel area for the entire collection
        /// </summary>
        public double TotalPanelArea
        {
            get
            {
                double _rVal = 0;
                mdDeckPanel Panel = null;
                for (int i = 1; i <= Count; i++)
                {
                    Panel = Item(i);
                    _rVal += Panel.PanelArea * Panel.OccuranceFactor;
                }
                return _rVal;
            }
        }

      

       


        /// <summary>
        /// used to add an item to the collection
        /// won't add "Nothing" (no error raised).
        /// </summary>
        /// <param name="aPanel">the item to add to the collection</param>
        /// <param name="bAddClone"></param>
        /// <returns></returns>
        public mdDeckPanel Add(mdDeckPanel aPanel, bool bAddClone = false)
        {
            return (aPanel == null) ? null : (mdDeckPanel)base.Add(aPanel, bAddClone);
        }


        /// <summary>
        /// used by objects above this object in the object model to alert a sub object that a property above it in the object model has changed.
        /// alerts the objects below it of the change
        /// </summary>
        /// <param name="aProperty"></param>
        public void Alert(uopProperty aProperty)
        {
            if (aProperty == null) return;
        }

        public uopRectangle Bounds(bool bExcludeVirtuals = true)
        {

            if (Count <= 0) return new uopRectangle();
            uopRectangle _rVal = Item(1).Bounds;

            if (bExcludeVirtuals)
            {
                List<mdDeckPanel> actives = ActivePanels(null, false);

                for (int i = 2; i <= actives.Count; i++)
                {
                    mdDeckPanel mem = actives[i - 1];
                    _rVal.ExpandTo(mem.Bounds);
                }
            }
            else
            {

                for (int i = 2; i <= Count; i++)
                {
                    mdDeckPanel mem = Item(i);
                    if (!bExcludeVirtuals || (bExcludeVirtuals && !mem.IsVirtual)) _rVal.ExpandTo(mem.Bounds);
                }
            }


            return _rVal;

        }

        public double MaxVLError(mdTrayAssembly aAssy) => MaxVLError(aAssy, out int rIndex);

        public void Populate(List<mdDeckPanel> aPanels)
        {
            Clear();

            if (aPanels == null) return;
            foreach (mdDeckPanel item in aPanels)
            {
                Add(item, false);
            }

        }
        public double MaxVLError(mdTrayAssembly aAssy, out int rIndex)
        {
            rIndex = -1;
            aAssy ??= GetMDTrayAssembly(aAssy);

            if (aAssy == null) return 0;
            double _rVal = 0;

            double aTot = 0;
            for (int i = 1; i <= Count; i++)
            {
                mdDeckPanel aMem = Item(i);
                double aErr = aMem.VLError(aAssy, ref aTot);
                if (Math.Abs(aErr) > Math.Abs(_rVal)) { rIndex = i; _rVal = aErr; }
            }
            return _rVal;
        }


        /// <summary>
        ///returns an new collection whose members are clones of the members of this collection
        /// </summary>
        /// <returns></returns>
        public colMDDeckPanels Clone() => new colMDDeckPanels(this);

        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();

        public List<mdFeedZone> FeedZones(mdTrayAssembly aAssy)
        {
            List<mdFeedZone> _rVal = new List<mdFeedZone>();


            for (int i = 1; i <= Count; i++)
            {
                mdDeckPanel aMem = Item(i);
                if (aMem.IsVirtual) break;

                _rVal.AddRange(aMem.FeedZones(aAssy));

            }
            return _rVal;
        }
   

        /// <summary>
        ///returns the panel whose center X ordinate is nearest to the passed X ordinate
        /// </summary>
        /// <param name="aXOrd"></param>
        /// <returns></returns>
        public mdDeckPanel GetByXOrd(double aXOrd)
        {

            mdDeckPanel aMem;
            double df = 0;
            double mindf = double.MaxValue;
            int idx = -1;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                df = Math.Abs(aMem.X - aXOrd);
                if (df < mindf)
                { idx = i; mindf = df; }
            }
            return (idx != -1) ? Item(idx) : null;
        }

        /// <summary>
        ///returns a collection filled with the ideal spout areas for each of the deck panels in the assembly
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aTotalWeirLength"></param>
        /// <param name="aTrayIdealSpoutArea"></param>
        /// <returns></returns>
        public List<double> GetIdealAreas(mdTrayAssembly aAssy, double aTotalWeirLength = 0, double aTrayIdealSpoutArea = 0)
        {
            List<double> _rVal = new List<double>();

            if (aTrayIdealSpoutArea <= 0 || aTotalWeirLength <= 0)
            {
                aAssy ??= GetMDTrayAssembly(aAssy);
                if (aAssy == null) return _rVal;
                if (aTrayIdealSpoutArea <= 0) aTrayIdealSpoutArea = aAssy.TheoreticalSpoutArea;
                if (aTotalWeirLength <= 0) aTotalWeirLength = aAssy.TotalWeirLength;
            }

            foreach (var item in this)
            {
                _rVal.Add(item.IdealSpoutArea(aAssy, aTotalWeirLength, aTrayIdealSpoutArea));
            }

            //for (int i = 1; i <= Count; i++)
            //{
            //    mdDeckPanel aMem = Item(i);
            //    if (aMem.IsVirtual)
            //    {
            //        aMem = aMem.Parent;
            //        aMem ??= Item(i);
            //    }

            //    _rVal.Add(aMem.IdealSpoutArea(aAssy, aTotalWeirLength));
            //}


            return _rVal;
        }

        public List<mdDeckPanel> ActivePanels(mdTrayAssembly aAssy, bool bGetClones = false)
        {

            return ActivePanels(aAssy, out _, out _, bGetClones);
        }
        public List<mdDeckPanel> ActivePanels(mdTrayAssembly aAssy, out bool rSpecialCase, out List<int> rOccuranceFactors, bool bGetClones = false)
        {
            rOccuranceFactors = new List<int>();
            rSpecialCase = false;
            aAssy ??= GetMDTrayAssembly();
            List<mdDeckPanel> _rVal = GetByVirtual(aVirtualValue: false, bGetClones);
            if (aAssy == null) return _rVal;
            //special case odd downcomers
            rSpecialCase = aAssy.IsStandardDesign && aAssy.OddDowncomers && aAssy.Downcomer().Count > 1;



            if (rSpecialCase)
            {
                List<mdDeckPanel> dps = GetByVirtual(aVirtualValue: true, bGetClones);

                if (dps.Count > 0) _rVal.Add(dps[0]);
            }

            for (int i = 1; i <= _rVal.Count; i++)
            {
                mdDeckPanel aDP = _rVal[i - 1];
                int occr = aDP.OccuranceFactor;
                if (rSpecialCase)
                {
                    if (i >= _rVal.Count - 1) occr = 1;
                }
                rOccuranceFactors.Add(occr);
            }

            return _rVal;
        }

        public List<mdDeckPanel> GetByVirtual(bool? aVirtualValue, bool bGetClones = false)
        {
            List<mdDeckPanel> _rVal = new List<mdDeckPanel>();
            foreach (var item in this)
            {
                if (aVirtualValue.HasValue)
                {
                    if (item.IsVirtual != aVirtualValue.Value) continue;
                }
                mdDeckPanel mem = (mdDeckPanel)item;
                _rVal.Add(!bGetClones ? mem : mem.Clone());
            }
            return _rVal;
        }

        /// <summary>
        ///returns the panel from the collection with the maximum error percentage.
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public mdDeckPanel GetMaxError(mdTrayAssembly aAssy)
        {
            aAssy ??= GetMDTrayAssembly(aAssy);
            if (aAssy == null) return null;
            mdDeckPanel _rVal = null;
            double Max = double.MinValue;

            mdDeckPanel aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                double Val = Math.Abs(aMem.ErrorPercentage(aAssy));
                if (Val > Max)
                {
                    _rVal = aMem;
                    Max = Val;
                }
            }
            return _rVal;
        }

        public List<mdDeckPanel> ToList(bool bGetClones = false, bool bGetVirtuals = false)
        {

            List<mdDeckPanel> _rVal = new List<mdDeckPanel>();

            foreach (var part in CollectionObj)
            {
                if (part.IsVirtual && !bGetVirtuals) continue;
                if (!bGetClones)
                    _rVal.Add((mdDeckPanel)part);
                else
                    _rVal.Add((mdDeckPanel)part.Clone());
            }
            return _rVal;
        }


        /// <summary>
        ///returns the item from the collection at the requested index !base 1 !
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        public new mdDeckPanel Item(int aIndex) => (mdDeckPanel)base.Item(aIndex);

        /// <summary>
        ///returns the item from the collection at the requested index !base 1 !
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        public mdDeckPanel Item(dynamic aIndex) { int idx = mzUtils.VarToInteger(aIndex); return (mdDeckPanel)base.Item(idx); }

        /// <summary>
        /// the last panel in the collection
        /// </summary>
        public mdDeckPanel LastPanel(bool? aVirtualValue = false)
        {

            List<mdDeckPanel> panels = GetByVirtual(aVirtualValue);
            return panels.Count <= 0 ? null : panels[panels.Count - 1];
        }

        /// <summary>
        ///returns the greatest panel width of all the panels
        /// </summary>
        /// <param name="IncludeMoon"></param>
        /// <returns></returns>
        public double MaxPanelWidth(bool IncludeMoon = true)
        {
            double _rVal = 0;

            for (int i = 1; i <= Count; i++)
            {
                mdDeckPanel aMem = Item(i);
                if (aMem.IsVirtual) continue;
                if (IncludeMoon || (!IncludeMoon && !aMem.IsHalfMoon))
                {
                    if (aMem.Width > _rVal) _rVal = aMem.Width;

                }
            }
            return _rVal;
        }
        /// <summary>
        /// used by members of the collection to inform the collection of change
        /// also raises the changed property to the object above it it the object model.
        /// </summary>
        /// <param name="aMember"></param>
        /// <param name="aProperty"></param>
        public void NotifyMemberChange(mdDeckPanel aMember, uopProperty aProperty)
        {

            if (aMember == null) return;
            if (aProperty == null) return;
            uopProperty colProperty;

            if (!SuppressEvents)
            {
                colProperty = aProperty.Clone();
                colProperty.SubPart(this);
                eventMDPanelMemberChanged?.Invoke(colProperty);
            }
        }


        /// <summary>
        /// removes the item from the collection at the requested index ! base 1 !
        /// </summary>
        /// <param name="Index"></param>
        public new mdDeckPanel Remove(int aIndex) => (mdDeckPanel)base.Remove(aIndex);

        /// <summary>
        /// returns True if any of the panels are wider than the manhole ID less 0.5 inches
        /// </summary>
        /// <param name="ManID"></param>
        /// <returns></returns>
        public bool RequiresSplicing(double ManID = 0)
        {
            bool _rVal = false;
            if (ManID <= 0) ManID = base.ManholeID;
            if (ManID <= 0)
            {
                mdTrayAssembly aAssy = GetMDTrayAssembly();
                if (aAssy != null) ManID = aAssy.ManholeID;

            }
            _rVal = MaxPanelWidth() > (ManID - 0.5);
            return _rVal;
        }
        /// <summary>
        ///returns the count of all the slots on all the panels in the entire tray
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public int TotalSlotCount(mdTrayAssembly aAssy)
        {
            int _rVal = 0;
            aAssy ??= GetMDTrayAssembly(aAssy);
            if (aAssy == null) return 0;

            mdDeckPanel aMem;
            mdSlotZones aZones = aAssy.SlotZones;


            int ocf = 0;

            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                ocf = aMem.OccuranceFactor;
                List<mdSlotZone> pZones = aZones.GetByPanelIndex(i);
                foreach (mdSlotZone aZone in pZones)
                {
                    _rVal += aZone.GridPoints().Count * ocf;
                }

            }
            return _rVal;
        }



        /// <summary>
        /// changes the materials of the members and their sections
        /// </summary>
        /// <param name="aSheetMetal"></param>
        public void UpdateMaterial(uopSheetMetal aSheetMetal)
        {
            if (aSheetMetal == null) return;

            try
            {
                mdDeckPanel mem;
                for (int i = 1; i <= Count; i++)
                {
                    mem = Item(i);
                    mem.UpdateMaterial(aSheetMetal);
                    SetItem(i, mem);
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }



        public override uopDocuments Warnings() => GenerateWarnings(null);

        /// <summary>
        ///returns a collection of strings that are warnings about possible problems with
        /// the current tray assembly design.
        /// these warnings may or may not be fatal problems.
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aCategory"></param>
        /// <returns></returns>
        public uopDocuments GenerateWarnings(mdTrayAssembly aAssy, string aCategory = null, uopDocuments aCollector = null, bool bJustOne = false)
        {
            uopDocuments _rVal = aCollector ?? new uopDocuments();
            if (bJustOne && _rVal.Count > 0) return _rVal;

            aAssy ??= GetMDTrayAssembly(aAssy);

            if (aAssy == null) return _rVal;

            aCategory = string.IsNullOrWhiteSpace(aCategory) ? TrayName(true) : aCategory.Trim();


            string txt;

            string plist1 = string.Empty;
            string plist2 = string.Empty;
            int wdViolate1 = 0;
            int wdViolate2 = 0;
            double wd;
            double ovrlVL = 0;
            double vlErr;
            try
            {
                bool isTiled = aAssy.DesignOptions.HasTiledDecks;
                double mhid = aAssy.ManholeID;
                int iNp = Count;
                if (!aAssy.IsSymmetric && aAssy.IsStandardDesign) iNp--;
                for (int i = 1; i <= iNp; i++)
                {
                    mdDeckPanel Panel = Item(i);
                    if (!Panel.IsVirtual)
                    {
                        if (aAssy.ProjectType == uppProjectTypes.MDSpout)
                        {
                            if (Math.Abs(Panel.ErrorPercentage(aAssy)) > aAssy.ErrorLimit)
                            {
                                txt = $"Deck Panel {Panel.Index} Has a Spout Area Deviation ({string.Format("{0:0.00}", Math.Abs(Panel.ErrorPercentage(aAssy)))}) That Exceeds The Project Limit of {string.Format("{0:0.00}", aAssy.ErrorLimit)} %";
                                _rVal.AddWarning(aAssy, "Spout Area Warning", txt, uppWarningTypes.ReportFatal, "Deck Panels");
                                if (bJustOne && _rVal.Count > 0) return _rVal;
                            }
                            vlErr = Panel.VLError(aAssy, ref ovrlVL);
                            if (Math.Abs(vlErr) > 2.5)
                            {
                                txt = $"Deck Panel {Panel.Index} Has A V/L Error ({string.Format("{0:0.000}", vlErr)}%) That Exceeds 2.5%";
                                _rVal.AddWarning(aAssy, "VL Error Warning", txt, uppWarningTypes.ReportFatal, "Deck Panels");
                                if (bJustOne && _rVal.Count > 0) return _rVal;
                            }
                        }
                        if (!Panel.IsHalfMoon)
                        {
                            wd = Panel.Width - 2 * aAssy.PanelClearance(true);
                            if (!isTiled)
                            {
                                if (wd >= mhid)
                                {
                                    wdViolate1 += 1;
                                    mzUtils.ListAdd(ref plist1, i);
                                }
                                else if (wd >= mhid - 0.5)
                                {
                                    wdViolate2 += 1;
                                    mzUtils.ListAdd(ref plist2, i);
                                }
                            }
                        }
                        else
                        {
                            if (aAssy.ProjectType == uppProjectTypes.MDDraw)
                            {
                                if (Panel.Width > mhid)
                                {
                                    if (Panel.SpliceLocations ==  string.Empty)
                                    {
                                        txt = "Half Moon Panel Requires Splicing To Fit Through The Manhole";
                                        _rVal.AddWarning(aAssy, "Panel Width Warning", txt, uppWarningTypes.ReportFatal);
                                        if (bJustOne && _rVal.Count > 0) return _rVal;
                                    }
                                }
                                else
                                {
                                    if (Panel.SpliceLocations !=  string.Empty)
                                    {
                                        txt = "Half Moon Panel Has Splicing But Splicing Is Not Required";
                                        _rVal.AddWarning(aAssy, "Unnecessary Splice Warning", txt, uppWarningTypes.ReportFatal);
                                        if (bJustOne && _rVal.Count > 0) return _rVal;
                                    }
                                }
                            }
                        }

                    }
                }
                if (wdViolate1 > 0)
                {
                    txt = $"{wdViolate1} Deck Panels {plist1} Will Not Fit Through The Manhole. Tiled Decks Should Be Requested.";
                    _rVal.AddWarning(aAssy, "Panel Width Warning", txt, uppWarningTypes.ReportFatal);
                    if (bJustOne && _rVal.Count > 0) return _rVal;
                }

                if (wdViolate2 > 0)
                {
                    txt = $"{wdViolate2} Deck Panels {plist2} May Not Fit Through The Manhole With The Required Mechanical Clearance of 0.5 Inches";
                    _rVal.AddWarning(aAssy, "Panel Width Warning", txt, uppWarningTypes.ReportFatal);
                    if (bJustOne && _rVal.Count > 0) return _rVal;
                }
                return _rVal;
            }
            catch
            {
                return _rVal;
            }
        }

        public  List<uopPanelSectionShape> SubShapes( bool bGetClones = false) => colMDDeckPanels.GetSubShapes(this,bGetClones);

        #region Shared Methods

        public static List<uopPanelSectionShape> GetSubShapes(IEnumerable<mdDeckPanel> aPanels, bool bGetClones = false)
        {
            List<uopPanelSectionShape> _rVal = new List<uopPanelSectionShape>();
            if(aPanels == null)  return _rVal;
       
            foreach (var panel in aPanels)
            {
                if (panel == null) continue;
                if (panel.SubShapes != null) continue;
                  
                _rVal.AddRange(bGetClones ? panel.SubShapes.Clone() : panel.SubShapes);
               
            }
            return _rVal;
        }
        #endregion Shared Methods
    }


}
