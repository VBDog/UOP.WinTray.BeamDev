using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows.Media.Media3D;
using UOP.DXFGraphics;
using UOP.DXFGraphics.Utilities;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Parts;

using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;


namespace UOP.WinTray.Projects.Generators
{
    public class mdSpacingData
    {

        #region Constructors
        public mdSpacingData()
        {
            WeirLengths = new List<double>();
            Ratios = new List<double>();
            WeightedRatios = new List<double>();
            FreeBubblingAreas = new List<double>();
            FBAs = null;
            SpacingMethod = uppMDSpacingMethods.Weighted;
        }

        public mdSpacingData(mdTrayAssembly aAssy, double? aSpaceValue = null, double? aOffset = null, mdDowncomer aBasis = null, uppMDSpacingMethods? aMethod = null, uppMDRoundToLimits? aRoundMethod = null)
        {

            FBAs = null;
            SpacingMethod = uppMDSpacingMethods.Weighted;

            if (aAssy == null) return;
            if (!aSpaceValue.HasValue)
                aSpaceValue = aAssy.DowncomerSpacing;
            if (!aOffset.HasValue)
                aOffset = aAssy.DowncomerOffset;

            SpacingMethod = aMethod ?? aAssy.SpacingMethod;
            Spacing = aSpaceValue.Value;
            Offset = aOffset;

            //create the free bubbling areas at this space
            FBAs = new mdFreeBubblingAreas(aAssy, Spacing, Offset, aBasis: aBasis, aRoundMethod: aRoundMethod);

            //mdDeckPanel aPnl;
            if (FBAs.Count <= 0) return;

            //collect the data from the free bubbling area
            int datacnt = FBAs.Count;
            double sumXsqr = 0;
            double std = 0;
            double wtstd = 0;
            double ratio;

            double FBA;
            double WL;
            WeirLengths = new List<double>(FBAs.WeirLengths);
            Ratios = new List<double>(FBAs.Ratios);
            _TotalFreeBubblingArea = FBAs.TotalFreeBubblingArea;
            _TotalWeirLength = FBAs.TotalWeirLength;
            FreeBubblingAreas = new List<double>(FBAs.Areas);

            //tabulate the average ratios (standard and weighted)

            WeightedRatios = new List<double>();
            WeightedAverageRatio = 0;
            AverageRatio = 0;
            for (int i = 1; i <= Ratios.Count; i++)
            {

                WL = WeirLengths[i - 1];
                ratio = Ratios[i - 1];
                AverageRatio += ratio;
                double FBAWeighted = ratio * (WL / TotalWeirLength); //* (FBA / rFBATot)
                WeightedRatios.Add(ratio * (WL / TotalWeirLength));
                WeightedAverageRatio += FBAWeighted;
            }
            AverageRatio /= Ratios.Count;
            //WeightedAverageRatio /= Ratios.Count;
            double summation = 0;

            for (int i = 1; i <= Ratios.Count; i++)
            {
                FBA = FreeBubblingAreas[i - 1];
                ratio = Ratios[i - 1];

                double val1 = (ratio - WeightedAverageRatio) * (ratio - WeightedAverageRatio) * (FBA / TotalFreeBubblingArea);
                summation += val1;
                double val2 = ratio - AverageRatio;
                sumXsqr += Math.Pow(val2, 2);
            }
            if (summation > 0) wtstd = Math.Sqrt(summation);

            if (sumXsqr > 0) std = Math.Sqrt(sumXsqr / Ratios.Count);

            //set the return argument values (the collections were already populated by the deck panels collection)
            WeightedDeviation = wtstd;
            StandardDeviation = std;

        }

        #endregion Constructors

        #region Properties

        public int PanelCount => FBAs == null ? 0 : FBAs.PanelCount;

        public uppProjectTypes ProjectType 
        {
            get
            {
                return FBAs == null ? FBAs.ProjectType : uppProjectTypes.Undefined;
            }
            set
            {
                if (FBAs == null)  return;
                FBAs.ProjectType = value;
            }
        }
        public List<double> WeirLengths { get; set; }
        public List<double> Ratios { get; set; }
        public List<double> WeightedRatios { get; set; }
        public List<double> FreeBubblingAreas { get; set; }

        public double StandardDeviation { get; set; }
        public double WeightedDeviation { get; set; }
        public double AverageRatio { get; set; }
        public double WeightedAverageRatio { get; set; }

        private double? _TotalWeirLength;
        public double TotalWeirLength { get { if (_TotalWeirLength.HasValue) return _TotalWeirLength.Value; return FBAs == null ? 0 : FBAs.TotalWeirLength; } }

        private double? _TotalFreeBubblingArea;
        public double TotalFreeBubblingArea { get { if (_TotalFreeBubblingArea.HasValue) return _TotalFreeBubblingArea.Value; return FBAs == null ? 0 : FBAs.TotalFreeBubblingArea; } }
        public double? Offset { get; set; }
        public double Spacing { get; set; }


        /// <summary>
        /// returns the total tray free bubbling area divided by the total weir length
        /// </summary>
        public double FBA2WLRatio
        {
            get
            {
                double _rVal = TotalWeirLength;
                return _rVal == 0 ? 0 : TotalFreeBubblingArea / _rVal;
            }
        }

        public uppMDSpacingMethods SpacingMethod { get; set; }


        public mdFreeBubblingAreas FBAs { get; set; }

        public DowncomerDataSet DowncomerData => FBAs == null ? null : FBAs.DowncomerData;

        public double Deviation => SpacingMethod == uppMDSpacingMethods.NonWeighted ? StandardDeviation : WeightedDeviation;

        #endregion Properties

        #region Methods

        public override string ToString() => $"mdSpacingData - {Spacing:0.0000} - Dev: {Deviation:0.000} FBAPanels: { PanelCount}";


        internal void PrintToConsole()
        {

            Console.WriteLine($"{ToString()}");

            foreach (uopFreeBubblingArea item in FBAs)
            {
                string txt = $"FBA {FBAs.IndexOf(item)}";
                txt = $"{txt} Area : {item.Area.ToString("0.00")}";
                txt = $"{txt} WL Left : {item.WeirLength_Left.ToString("0.00")}";
                txt = $"{txt} WL Right : {item.WeirLength_Right.ToString("0.00")}";
                Console.WriteLine(txt);

            }
        }
        public List<uopLinePair> GetLimitLines(int aDCIndex = 0, bool bRegenInfo = false)
        {
            List<uopLinePair> _rVal = new List<uopLinePair>();
            DowncomerDataSet data = DowncomerData;
            if (data == null) return _rVal;
            return data.GetLimitLines(aDCIndex, bRegenInfo: bRegenInfo);

        }
        public List<uopLinePair> GetBoxLines(int aDCIndex = 0, bool bRegenInfo = false)
        {
            List<uopLinePair> _rVal = new List<uopLinePair>();
            DowncomerDataSet data = DowncomerData;
            if (data == null) return _rVal;
            foreach (var item in data)
            {
                if (aDCIndex <= 0 || item.DCIndex == aDCIndex) _rVal.AddRange(item.GetBoxLines(bRegenLimits: bRegenInfo));
            }
            return _rVal;
        }
        #endregion Methods

    }

    public class mdSpacingSolutions : List<mdSpacingData>
    {

        #region Constructors
        public mdSpacingSolutions()
        {
            Clear();
            SpacingMethod = uppMDSpacingMethods.Weighted;
            RoundingMethod = uppMDRoundToLimits.Sixteenth;
        }

        public mdSpacingSolutions(uppMDSpacingMethods aMethod, uppMDRoundToLimits? aRoundMethod = null)
        {
            Clear();
            SpacingMethod = aMethod;
            RoundingMethod = aRoundMethod.HasValue ? aRoundMethod.Value : uppMDRoundToLimits.Sixteenth;
        }

        #endregion Constructors

        #region Properties

        public double OptimumSpace => OptimumSolution.Spacing;


        public mdSpacingData OptimumSolution => !IsConverged ? new mdSpacingData() : Item(SolutionIndex);

        public uppMDSpacingMethods SpacingMethod { get; set; }

        public uppMDRoundToLimits RoundingMethod { get; set; }

        public int SolutionIndex { get; private set; }

        public bool IsConverged
        {
            get
            {
                SolutionIndex = 0;
                if (Count == 1)
                {
                    SolutionIndex = 1;

                    return true;
                }


                if (Count < 3) return false;
                //find the minimum deviation member
                List<Tuple<double, mdSpacingData>> sort = new List<Tuple<double, mdSpacingData>>();
                foreach (var item in this)
                {
                    sort.Add(new Tuple<double, mdSpacingData>(item.Deviation, item));
                }
                sort = sort.OrderBy(t => t.Item1).ToList();
                mdSpacingData min = sort[0].Item2;
                int idx = IndexOf(min);
                if (idx == 1 || idx == Count) return false;
                SolutionIndex = idx;
                return true;

                //mdSpacingData data1;
                //mdSpacingData data2;
                //mdSpacingData data3;

                //for (int i = 1; i <= Count; i++)
                //{
                //    data1 = Item(i);
                //    if (i + 1 > Count)
                //        return false;
                //    else
                //        data2 = Item(i + 1);
                //    if (i + 2 > Count)
                //        return false;
                //    else
                //        data3 = Item(i + 2);

                //    if (data1.Deviation >= data2.Deviation && data3.Deviation >= data2.Deviation)
                //    {

                //        SolutionIndex = i + 1;
                //        return true;
                //    }


                //}


                //return false;
            }

        }

        public mdSpacingData Solution => SolutionIndex < 1 || SolutionIndex > Count ? null : Item(SolutionIndex);

        #endregion Properties

        #region Methods

        public new void Add(mdSpacingData aMember)
        {
            if (aMember == null) return;


            if (Count <= 0)
            {
                base.Add(aMember);
            }
            else
            {
                if (FindIndex(x => x.Spacing == aMember.Spacing && x.Offset == aMember.Offset) < 0) return;
                if (aMember.Spacing < Item(1).Spacing)
                    base.Insert(0, aMember);
                else
                    base.Add(aMember);
       
            }


        }

        public mdSpacingData Item(int aIndex)
        {
            if (aIndex < 1 || aIndex > Count) throw new ArgumentOutOfRangeException();
            mdSpacingData _rVal = base[aIndex - 1];
            _rVal.SpacingMethod = SpacingMethod;

            return _rVal;
        }

        public double Deviation(int aIndex)
        {
            if (aIndex < 1 || aIndex > Count) throw new ArgumentOutOfRangeException();
            mdSpacingData _rVal = base[aIndex - 1];
            _rVal.SpacingMethod = SpacingMethod;

            return _rVal.Deviation;
        }

        public override string ToString()
        {
            return $"mdSpacingSpolutions [{Count}]";
        }


        public void Sort(bool bByDeviation = false)
        {
            List<Tuple<double, mdSpacingData>> sort = new List<Tuple<double, mdSpacingData>>();
            foreach (var item in this)
            {
                if (bByDeviation)
                    sort.Add(new Tuple<double, mdSpacingData>(item.Deviation, item));
                else
                    sort.Add(new Tuple<double, mdSpacingData>(item.Spacing, item));
            }
            sort = sort.OrderBy(t => t.Item1).ToList();
            base.Clear();
            foreach (var tuple in sort)
                base.Add(tuple.Item2);
        
        }
        /// <summary>
        /// computes the best downcomer spacing based on the tray assembly properties and the passed input
        /// </summary>
        /// <param name="aAssy">the subject assembly</param>
        /// <param name="aStartSpace">a starting space value estimate</param>
        /// <param name="aOffset">an offset to apply</param>
        /// <param name="aBasis">the global downcomer whose base properties should match all the downcomers in the assemblies downcomer collection</param>
        /// <param name="aSpaceInterval">the step value to use in the interation</param>
        /// <returns></returns>
        public mdSpacingData SolveForOptimimumSpace(mdTrayAssembly aAssy, double aStartSpace, double aOffset, mdDowncomer aBasis = null, double aSpaceInterval = 0.0625)
        {
            if (aBasis == null && aAssy != null) aBasis = aAssy.Downcomer();
            aAssy ??= aBasis.GetMDTrayAssembly();
            if (aAssy == null || aBasis == null) return null;
            aSpaceInterval = mzUtils.LimitedValue(Math.Abs(aSpaceInterval), 0.0625,0.5, 0.0625); // 1/16th to 1/2 of an inch

            int pcnt = aBasis.Count + 1;
            try
            {
                //get the value a step less
                mdSpacingData data1 = new mdSpacingData(aAssy, aSpaceValue: aStartSpace - aSpaceInterval, aOffset: aOffset, aBasis: aBasis, aMethod: SpacingMethod, aRoundMethod: RoundingMethod);
                //get the value at the start value
                mdSpacingData data2 = new mdSpacingData(aAssy, aSpaceValue: aStartSpace, aOffset: aOffset, aBasis: aBasis, aMethod: SpacingMethod, aRoundMethod: RoundingMethod);
                //get the value a step more
                mdSpacingData data3 = new mdSpacingData(aAssy, aSpaceValue: aStartSpace + aSpaceInterval, aOffset: aOffset, aBasis: aBasis, aMethod: SpacingMethod, aRoundMethod: RoundingMethod);

                //decide which direction to loop
                //if the deviations are declining keep increasing the space (move right) otherwise decrease the space (move left)
                // the deviation curve is always V shaped !!
                int breakat = 0;
                
                double Direxion = data1.Deviation <data2.Deviation ?-1 : data3.Deviation < data2.Deviation ? 1 : -1;
                // space = Direxion == 1 ? data3.Spacing + (aSpaceInterval * Direxion) : data1.Spacing + (aSpaceInterval *Direxion);
                base.Clear();
                base.Add(data1);
                base.Add(data2);
                base.Add(data3);

                if (Direxion != 1) { base.Reverse(); data1 = this[0]; data2 = this[1]; data3 = this[2]; }
                // a solution is coverged at the point where the deviations are minimized
                if (data1.Deviation > data2.Deviation && data3.Deviation > data2.Deviation)
                {
                    SolutionIndex = 2;
                    return data2;
                }
                double space = base[Count -1].Spacing + (aSpaceInterval * Direxion);
                
                for (int i = 1; i <= 1000; i++)
                {
                    mdSpacingData data = new mdSpacingData(aAssy, aSpaceValue: space, aOffset: aOffset, aBasis: aBasis, aMethod: SpacingMethod, aRoundMethod: RoundingMethod);
                    if(data.PanelCount < pcnt) //we lost downcomers because the space was too big
                    {
                        SolutionIndex = Count - 1;
                        return this[SolutionIndex - 1];
                    }
                    base.Add(data);
                    //SolutionIndex = IndexOf(data);
                    //return true;

                    data1 = this[Count - 3];
                    data2 = this[Count - 2];
                    data3 = this[Count - 1];

                    if (data1.Deviation > data2.Deviation && data3.Deviation > data2.Deviation)
                    {
                        SolutionIndex = IndexOf(data2);
                        return data2;
                    }

                    breakat++;


                    if (breakat > 500)
                        return null;  // it shouldn't take this many times ever!

                    space += aSpaceInterval * Direxion;

                }

                
            }
            catch
            {
            }
            return null;
        }



        internal void PrintToConsole()
        {

            bool conv = IsConverged;
            int sol = IndexOf(OptimumSolution);

            Console.WriteLine("");
            Console.WriteLine($"{ToString()} - Converged = {conv}");
            if (Count <= 0) return;



            for (int i = 1; i <= Count; i++)
            {
                mdSpacingData item = Item(i);
                string suf = i == sol ? " ***" : "";
                Console.WriteLine($"{i} => DEVIATION : {item.Deviation.ToString("0.0000##")} {suf}");
                item.PrintToConsole();
                Console.WriteLine("");
            }


        }

        public new int IndexOf(mdSpacingData aMember)
        {
            if (aMember == null) return 0;
            return base.IndexOf(aMember) + 1;
        }

        public static double ComputeOptimizedSpace(mdTrayAssembly aAssy, out mdSpacingSolutions rSolutions)
        {
            rSolutions = null;
            return aAssy == null ? 0 : mdSpacingSolutions.OptimizedSpace(aAssy, out rSolutions, 0, 0, null).Spacing;

        }

        public static double OptimizedSpace(mdTrayAssembly aAssy, double aDCWidth, int aDCCnt)
        {
            if (aAssy == null || aDCWidth <= 0 || aDCCnt <= 0) return 0;
            mdDowncomer origdc = aAssy.Downcomer();

            double _rVal = aAssy.Downcomers.OptimumSpacing;
            try
            {
                mdDowncomer baseDC = origdc;
                if (aDCWidth != origdc.Width || aDCCnt != origdc.Count)
                {

                    baseDC = origdc.Clone();
                    baseDC.RangeGUID = string.Empty;
                    baseDC.SuppressEvents = true;
                    baseDC.Width = aDCWidth;
                    baseDC.Count = aDCCnt;

                    aAssy.SetDowncomer(baseDC);
                }

                mdSpacingData data = mdSpacingSolutions.OptimizedSpace(aAssy,out mdSpacingSolutions soultions,  aBasis: baseDC);

                _rVal = data != null ? data.Spacing : 0;
            }
            catch { }
            finally
            {
                aAssy.SetDowncomer(origdc);
            }

            return _rVal;

        }

        /// <summary>
        /// returns the optimized downcomer spacing based on the physical properties of the downcomers in the passed collection
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aIdealSpace"></param>
        /// <param name="aStartValue"></param>
        /// <param name="aOffset"></param>
        /// <param name="aBasis"></param>
        /// <returns></returns>
        public static mdSpacingData OptimizedSpace(mdTrayAssembly aAssy, out mdSpacingSolutions rSolutions,double aIdealSpace = 0, double aStartValue = 0, double? aOffset = 0, mdDowncomer aBasis = null, uppMDRoundToLimits? aRoundMethod = null)
        {

            rSolutions = new mdSpacingSolutions(uppMDSpacingMethods.Weighted);
            if (aAssy == null) return null;

            uppMDDesigns family = aAssy.DesignFamily;

            aBasis ??= aAssy.Downcomer();
            if (aIdealSpace < 0) aIdealSpace = 0;

            int dcCnt = aBasis.Count;
            uppMDSpacingMethods method = aAssy.SpacingMethod;
             rSolutions = new mdSpacingSolutions(method);
            
            if (!aOffset.HasValue) aOffset = 0;

            //initialize
            if (aStartValue <= 0) aStartValue = 0;

            //estimate the space based on number of downcomers
            double space_estimate = aBasis.Count switch
            {
                1 => 0,
                2 => 0.405 * aAssy.RingID,
                3 => 0.29 * aAssy.RingID,
                4 => 0.23 * aAssy.RingID,
                _ => Math.Pow(10, (0.4342945 * Math.Log(aBasis.Count) + 0.0387) / -1.0229) * aAssy.RingID
            };

            uppMDRoundToLimits roundMethod = aRoundMethod.HasValue ? aRoundMethod.Value : aAssy.DowncomerRoundToLimit;

            dxxRoundToLimits roundto = roundMethod switch
            {
                uppMDRoundToLimits.Sixteenth => dxxRoundToLimits.Sixteenth,
                uppMDRoundToLimits.Millimeter => dxxRoundToLimits.Millimeter,
                _ => dxxRoundToLimits.Undefined
            };

            space_estimate = uopUtils.RoundTo(space_estimate, roundto);

            //make sure the start value is doable
            aStartValue = uopUtils.RoundTo(aStartValue, roundto, false, true);
            if (aStartValue > 0)
            {
                double lcnt;
                double lastX = 0;
                double boundRad = aAssy.RingClipRadius;
                if (mzUtils.IsOdd(dcCnt))
                { lcnt = (dcCnt - 1) / 2; }
                else
                {
                    lcnt = dcCnt / 2;
                    lastX = 0.5 * aStartValue;
                }
                for (int i = 1; i < lcnt; i++) { lastX += aStartValue; }

                if (lastX + aBasis.Width / 2 + 2 >= boundRad) aStartValue = 0;

            }

            space_estimate = uopUtils.RoundTo(space_estimate, roundto);

            try
            {
                //if an ideal space is passed then just get the data for the passed space
                if (aIdealSpace > 0)
                {
                    rSolutions.Add(new mdSpacingData(aAssy, aIdealSpace, aOffset: aOffset, aBasis, aRoundMethod: roundMethod));
                    return rSolutions.Item(1);  // solution with just one dataset is converged by default'
                }


                //set the starting value
                if (aStartValue > 0)
                {
                    if ((aStartValue / space_estimate) >= 1.3 || (aStartValue / space_estimate) < 0.7) aStartValue = space_estimate;
                }
                else
                { aStartValue = space_estimate; }
                rSolutions = new mdSpacingSolutions(method, roundMethod);

                //this finds the best solution
                mdSpacingData _rVal = rSolutions.SolveForOptimimumSpace(aAssy, aStartValue, aOffset ?? 0, aBasis);
                return _rVal;
            }
            catch
            {

            }
            finally
            {
                // FBA2WLRatio = rSolutions.OptimumSolution.FBA2WLRatio;


            }



            return null;

        }

        #endregion Methods
    }

    public class DowncomerInfo : ICloneable
    {

        #region Constructors

        public DowncomerInfo() => Init();

        public DowncomerInfo(DowncomerInfo aInfo)
        {
            if (aInfo != null)
                Copy(aInfo);
            else
                Init();

        }
        public DowncomerInfo(mdDowncomer aDC, mdTrayAssembly aAssy, double? aXVal, uppMDRoundToLimits? aRoundMethod = null, DividerInfo aDividerInfo = null)
        {
            Init();
            if (aXVal.HasValue) X = aXVal.Value;
            aAssy ??= aDC?.GetMDTrayAssembly();
            TrayAssembly = aAssy;
            if (aDividerInfo != null)
                Divider = new DividerInfo(aDividerInfo);
            else if (aAssy != null && aAssy.DesignFamily.IsBeamDesignFamily()) Divider = new DividerInfo(aAssy.Beam);
            else Divider = new DividerInfo();
            if (aRoundMethod.HasValue) RoundingMethod = aRoundMethod.Value;
            if (aDC == null) return;

            DCIndex = aDC.Index;

            if (!aXVal.HasValue)
                X = aDC.X;
            Y = aDC.Y;
            EndPlateOverhang = aDC.EndPlateOverhang;

            InsideWidth = aDC.Width;
            Thickness = aDC.Thickness;
            DeckRadius = aDC.DeckRadius;
            HasTriangularEndPlate = aDC.HasTriangularEndPlate;
            EndplateInset = aDC.EndplateInset;
            ClipClearance = aDC.ClipClearance;
            IsVirtual = aDC.IsVirtual;
            ShelfWidth = aDC.ShelfWidth;
            Clearance = aDC.RingClearance;
            RingRadius = aDC.RingID / 2;
            ColumnRadius = aDC.ShellID / 2;
            RingClipSize = aDC.RingClipSize;
            DowncomerCount = aDC.Count;
            SpoutDiameter = aDC.SpoutDiameter;
            WeirHeight = aDC.WeirHeight;
            _AssyRef = null;


            if (aAssy != null)
            {
                _AssyRef = new WeakReference<mdTrayAssembly>(aAssy);
                DeckRadius = aAssy.DeckRadius;
                RingRadius = aAssy.RingID / 2;
                mdDowncomer globalDC = aAssy.Downcomer();
                Thickness = globalDC.Thickness;
                ShelfWidth = globalDC.ShelfWidth;
                if (globalDC.Width > 0) InsideWidth = globalDC.Width;
                Spacing = aAssy.DowncomerSpacing;
                RingClipRadius = aAssy.RingClipRadius;
                Clearance = aAssy.RingClearance;
                DesignFamily = aAssy.DesignFamily;
                ColumnRadius = aAssy.ShellID / 2;
                RingClipSize = aAssy.RingClipSize;
                if (DesignFamily.IsBeamDesignFamily()) Divider = new DividerInfo(aAssy.Beam);
                PanelClearance = aAssy.PanelClearance(true);
                DowncomerCount = aAssy.Downcomer().Count;
                WeirHeight = aAssy.Downcomer().WeirHeight;
            }

        }

        private void Init()
        {
            MetricSpouting = false;
            SpoutDiameter = 0.75 / 25.4;
            X = 0;
            TrayAssembly = null;
            Y = 0;
            PanelClearance = mdGlobals.DefaultPanelClearance;
            RoundingMethod = uppMDRoundToLimits.Sixteenth;
            ShelfWidth = mdGlobals.DefaultShelfAngleWidth;
            RingRadius = 0;
            InsideWidth = 0;
            Thickness = 0;
            Clearance = 0;
            RingClipRadius = 0;
            DeckRadius = 0;
            ClipClearance = mdGlobals.DefaultRingClipClearance;
            HasTriangularEndPlate = false;
            ShelfWidth = 0;
            Spacing = 0;
            IsVirtual = false;
            Index = 0;
            EndplateInset = mdGlobals.DefaultEndplateInset;
            WeirFraction = 0;
            PanelClearance = 0;
            ColumnRadius = 0;
            DCIndex = 0;
            Divider = new DividerInfo();
            EndPlateOverhang = mdGlobals.DefaultEndPlateOverhang;
            _RingClipSize = uppRingClipSizes.ThreeInchRC;
            _RingClipHoleDiameter = mdGlobals.gsBigHole;
            LimLines = null; ;
            BoxLns = null;
            WeirLns = null;
            DowncomerCount = 0;
            WeirHeight = 0;
            _RoundingMethod = mdGlobals.DefaultRoundingMethod;
        }
        #endregion Constructors

        #region Properties

        public double WeirHeight { get; set; }

        public bool MetricSpouting { get; set; }

        public int DowncomerCount { get; set; }
        public int PanelCount => DowncomerCount > 0 ? DowncomerCount + 1 : 0;

        public bool MultiPanel { get => Divider == null ? false : Divider.DividerType == uppTrayDividerTypes.Beam && Divider.Offset != 0; }

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
                if (value == null)
                {
                    DesignFamily = uppMDDesigns.Undefined;
                    _AssyRef = null;
                    Divider = new DividerInfo();
                    return;
                }
                _AssyRef = new WeakReference<mdTrayAssembly>(value);
                DeckRadius = value.DeckRadius;
                RingRadius = value.RingID / 2;
                Thickness = value.Downcomer().Thickness;
                ShelfWidth = value.Downcomer().ShelfWidth;
                InsideWidth = value.Downcomer().Width;
                Spacing = value.DowncomerSpacing;
                RingClipRadius = value.RingClipRadius;
                Clearance = value.RingClearance;
                DesignFamily = value.DesignFamily;
                if (DesignFamily.IsBeamDesignFamily())
                    Divider = new DividerInfo(value.Beam);
                RoundingMethod = value.DowncomerRoundToLimit;
                PanelClearance = value.PanelClearance(true);
                DowncomerCount = value.Downcomer().Count;

            }
        }


        private uppMDRoundToLimits _RoundingMethod;
        /// <summary>
        /// the rounding method to apply when calculating weir and box lengths 
        /// </summary>
        public uppMDRoundToLimits RoundingMethod
        {
            get => _RoundingMethod;
            set
            {
                if (_RoundingMethod != value)
                    LimLines = null;

                _RoundingMethod = value;

            }

        }
        /// <summary>
        ///returns then default clearance for spoutgroups in this downcomer
        /// </summary>
        /// <returns></returns>
        public double SpoutGroupClearance => mdSpoutGroup.GetDefaultClearance(Thickness);


        /// <summary>
        /// if true the downcomer is an identical child or mirror of another downcomer
        /// </summary>
        public bool IsVirtual { get; private set; }

        /// <summary>
        /// the inside width of the downcomer
        /// </summary>
        public double InsideWidth { get; set; }

        /// <summary>
        /// the outside width of the downcmer
        /// </summary>
        public double BoxWidth => InsideWidth + 2 * Thickness;

        public double PanelClearance { get; set; }

        public double SpoutDiameter { get; set; }


        public double WeirFraction { get; internal set; }

        public bool RequiresTwoEndSupportBolts
        {
            get
            {
                if (DeckRadius <= 0) return false;

                Thickness = Math.Abs(Thickness);
                if (InsideWidth >= 8) return true;
                if (InsideWidth < 5) return false;

                double wd = Math.Round(InsideWidth, 5);
                double xr = Math.Abs(X) + (wd / 2d);

                if (xr > DeckRadius) return false;

                double xl = xr - wd;
                double yR = Math.Sqrt(Math.Pow(DeckRadius, 2) - Math.Pow(xr + Thickness, 2));
                double yL = Math.Round(X, 5) != 0 ? Math.Sqrt(Math.Pow(DeckRadius, 2) - Math.Pow(xl, 2)) : yR;

                double d1 = Math.Round(Math.Sqrt(Math.Pow(xr - xl, 2) + Math.Pow(yR - yL, 2)), 5);
                return d1 >= mdGlobals.TwoEndSupportBoltLimitLineLength;
            }


        }

        public double Spacing { get; set; }

        public uppMDDesigns DesignFamily { get; set; }

        public double Thickness { get; set; }

        public double ShelfWidth { get; set; }

        public double EndplateInset { get; set; }


        public double EndPlateOverhang { get; set; }

        public double ClipClearance { get; set; }

        public double X { get; set; }

        public double X_Inside_Left => X - InsideWidth / 2;

        public double X_Inside_Right => X + InsideWidth / 2;

        public double X_Outside_Left => X - BoxWidth / 2;

        public double X_Outside_Right => X + BoxWidth / 2;

        public double Y { get; set; }

        public double RingRadius { get; set; }

        public double ColumnRadius { get; set; }

        public double DeckRadius { get; set; }

        public double DeckLap => DeckRadius - RingRadius;
        public double RingClipRadius { get; set; }
        public double Clearance { get; set; }

        public int Index { get; set; }

        public int DCIndex { get; set; }

        public virtual double BoundingRadius { get { double _rVal = RingRadius - Clearance; return (_rVal < 0) ? 0 : _rVal; } }

        public double ShellID => ColumnRadius * 2;

        private bool _HasTriangularEndPlate;
        public bool HasTriangularEndPlate { get => Math.Round(X, 5) == 0 ? false : _HasTriangularEndPlate; set => _HasTriangularEndPlate = value; }

        public double MinimumEndPlateEngagement => 0.75;

        private uppRingClipSizes? _RingClipSize;

        public uppRingClipSizes RingClipSize { get { return !_RingClipSize.HasValue ? (ShellID <= uopGlobals.RingClipSizeChangeLimit) ? uppRingClipSizes.ThreeInchRC : uppRingClipSizes.FourInchRC : _RingClipSize.Value; } set => _RingClipSize = value; }

        public virtual double DefaultRingClipClearance { get { return (RingClipSize == uppRingClipSizes.FourInchRC) ? 1.375 : 1.125; } }

        internal double? _RingClipHoleDiameter;
        /// <summary>
        /// the iameter of the ring clip holes  default is 11 mm for 3'' ring clips and 13 mm fro 4 '' ring clips
        /// </summary>
        public double RingClipHoleDiameter { get => _RingClipHoleDiameter.HasValue ? _RingClipHoleDiameter.Value : RingClipSize == uppRingClipSizes.ThreeInchRC ? mdGlobals.gsBigHole : 13 / 25.4; set => _RingClipHoleDiameter = value; }


        /// <summary>
        /// property containing list of limit line pairs where the first item is the upper limit line and the second item is the lower limit line
        /// </summary>
        public List<uopLinePair> LimitLines => uopLinePair.FromList(LimLines);

        /// <summary>
        /// property containing list of limit line pairs where the first item is the upper limit line and the second item is the lower limit line
        /// </summary>
        internal List<ULINEPAIR> _LimLines;
        internal List<ULINEPAIR> LimLines
        {
            get
            {
                if (_LimLines != null && _LimLines.Count <= 0)
                    _LimLines = null;


                if (_LimLines == null)
                    CreatedDefinitionLines();
                return _LimLines;
            }
            set { _LimLines = value; if (value == null) { _BoxLns = null; _WeirLns = null; } }
        }
        /// <summary>
        /// property containing list of box line pairs where the first item is the left outer box line and the second item is the right outer box line
        /// </summary>
        internal List<ULINEPAIR> _BoxLns;
        internal List<ULINEPAIR> BoxLns
        {
            get
            {
                _BoxLns ??= new List<ULINEPAIR>();
                if (_BoxLns.Count <= 0)
                    CreatedDefinitionLines();
                return _BoxLns;
            }
            set { _BoxLns = value; if (value == null) { _LimLines = null; _WeirLns = null; } }
        }

        /// <summary>
        /// property containing list of limit line pairs where the first item is the upper limit line and the second item is the lower limit line
        /// </summary>
        public List<uopLinePair> BoxLines => uopLinePair.FromList(BoxLns);


        internal List<ULINEPAIR> _ShelfLns;
        /// <summary>
        /// property containing list of  lines that define the shelf angles on either side of the box lines
        /// </summary>
        internal List<ULINEPAIR> ShelfLns
        {
            get
            {
                if (_ShelfLns != null && _ShelfLns.Count == 0)
                    _ShelfLns = null;
                _ShelfLns ??= CreateShelfLines();
                return _ShelfLns;
            }
        }

        /// <summary>
        ///property containing list of  lines that define the shelf angles on either side of the box lines
        /// </summary>
        public List<uopLinePair> ShelfLines => uopLinePair.FromList(ShelfLns);


        internal List<ULINEPAIR> _EndSupportLns;
        /// <summary>
        /// property containing list of  lines that define the end supports on the top and bottom of the boxes
        /// </summary>
        internal List<ULINEPAIR> EndSupportLns
        {
            get
            {
                if (_EndSupportLns != null && _EndSupportLns.Count == 0)
                    _EndSupportLns = null;
                _EndSupportLns ??= CreateEndSupportLines();
                return _EndSupportLns;
            }
        }

        /// <summary>
        ///property containing list of  lines that define the end supports on the top and bottom of the boxes
        /// </summary>
        public List<uopLinePair> EndSupportLines => uopLinePair.FromList(EndSupportLns);

        internal List<ULINEPAIR> _WeirLns;
        /// <summary>
        /// property containing list of weir line pairs where the first item is the left dc weir line and the second item is the right dc weir line
        /// </summary>
        internal List<ULINEPAIR> WeirLns
        {
            get
            {
                _WeirLns ??= new List<ULINEPAIR>();
                if (_WeirLns.Count <= 0)
                    CreatedDefinitionLines();
                return _WeirLns;
            }
            set { _WeirLns = value; if (value == null) { _BoxLns = null; _LimLines = null; } }
        }

        internal List<ULINE> WeirLine_Left => WeirLines(uppSides.Left);

        internal List<ULINE> WeirLines_Right => WeirLines(uppSides.Right);

        public double WeirLength_Left
        {
            get
            {
                WeirLines(uppSides.Left, out double _rVal);
                return _rVal;
            }

        }

        public double WeirLength_Right
        {
            get
            {
                WeirLines(uppSides.Right, out double _rVal);
                return _rVal;
            }

        }

        public double TotalWeirLength
        {
            get
            {
                WeirLines(uppSides.Undefined, out double _rVal);
                return _rVal;
            }

        }

        public int DividerCount => DesignFamily.IsStandardDesignFamily() ? 0 : DesignFamily.IsBeamDesignFamily() ? Divider.Offset != 0 ? 2 : 1 : 1;
        /// <summary>
        /// carries the information about the beam or wall that divides the downcomers
        /// </summary>
        public DividerInfo Divider { get; set; }

        public int MaxRow => DesignFamily.IsStandardDesignFamily() || DesignFamily.IsDividedWallDesignFamily() ? 1 : !MultiPanel ? 2 : 3;



        #endregion Properties

        #region Methods

        public void ResetDefinitonLines() { _WeirLns = null; _BoxLns = null; _LimLines = null; }

        public override string ToString()
        {
            return $"DowncomerInfo {DCIndex} [X: {X:0.00##}";
        }

        public DowncomerInfo Clone() => new DowncomerInfo(this);

        internal List<ULINE> WeirLines(uppSides aSide = uppSides.Undefined) => WeirLines(aSide, out double _);

        internal List<ULINE> WeirLines(uppSides aSide, out double rLengthTotal)
        {
            rLengthTotal = 0;
            List<ULINE> _rVal = new List<ULINE>();
            List<ULINEPAIR> weirs = WeirLns;
            ULINE? sideleft = null;
            ULINE? sidelright = null;
            foreach (var item in weirs)
            {
                sideleft = item.GetSide(uppSides.Left);
                sidelright = item.GetSide(uppSides.Right);

                if (aSide == uppSides.Left)
                {

                    if (sideleft.HasValue) { rLengthTotal += sideleft.Value.Length; _rVal.Add(sideleft.Value); }
                }
                else if (aSide == uppSides.Right)
                {
                    if (sidelright.HasValue) { rLengthTotal += sidelright.Value.Length; _rVal.Add(sidelright.Value); }
                }
                else
                {

                    if (sideleft.HasValue) { rLengthTotal += sideleft.Value.Length; _rVal.Add(sideleft.Value); }
                    if (sidelright.HasValue) { rLengthTotal += sidelright.Value.Length; _rVal.Add(sidelright.Value); }
                }
            }
            return _rVal;

        }

        /// <summary>
        /// <returns>returns the lined which are the out side edges of the boxes</returns>
        /// </summary>
        /// <returns></returns>
        public List<uopLinePair> GetBoxLines(bool bRegenLimits = false)
        {
            List<uopLinePair> _rVal = new List<uopLinePair>();
            if (bRegenLimits) CreatedDefinitionLines();
            return uopLinePair.FromList(BoxLns);
        }

        public void GetBoxTops(out double rTopLeft, out double rTopRight)
        {
            rTopRight = 0;
            rTopLeft = 0;

            double rad = BoundingRadius;

            if (rad <= 0) return;

            double xright;
            double xleft;
            if (Math.Round(X, 5) >= 0)
            {
                xright = X_Inside_Right;
                xleft = X_Inside_Left + Thickness;

            }
            else
            {
                xright = X_Inside_Right - Thickness;
                xleft = X_Inside_Left;

            }


            //"BeamLength" is the chord length of the assemblies bounding circle at X
            if (RoundingMethod.Units() != dxxRoundToLimits.Undefined)
            {
                rTopRight = uopUtils.ComputeBeamLength(xright, rad, bRoundDown: true, aReducer: 2 * EndPlateOverhang, aRoundTo: RoundingMethod.Units()) / 2; //X at endplate end .endplate overhange is 0.25 
                rTopLeft = X == 0 ? rTopRight : uopUtils.ComputeBeamLength(xleft, rad, bRoundDown: true, aReducer: 2 * EndPlateOverhang, aRoundTo: RoundingMethod.Units()) / 2; //X at endplate end .endplate overhange is 0.25 
            }
            else
            {
                rTopRight = Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(xright, 2)) - EndPlateOverhang;
                rTopLeft = X == 0 ? rTopRight : Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(xleft, 2)) - EndPlateOverhang;


            }
            if (!HasTriangularEndPlate)
            {
                rTopLeft = Math.Min(rTopLeft, rTopRight);
                rTopRight = rTopLeft;
            }
        }


        public void GetEndSupportTops(out double rTopLeft, out double rTopRight, out double rXLeft, out double rXRight)
        {
            rTopRight = 0;
            rTopLeft = 0;
            rXLeft = 0;
            rXRight = 0;
            double rad = DeckRadius;

            if (rad <= 0) return;


            if (Math.Round(X, 5) >= 0)
            {
                rXRight = X_Outside_Right;
                rXLeft = X_Inside_Left;

            }
            else
            {
                rXRight = X_Inside_Right;
                rXLeft = X_Outside_Left;

            }

            rTopRight = Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(rXRight, 2));
            rTopLeft = X == 0 ? rTopRight : Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(rXLeft, 2));


        }

        /// <summary>
        /// <returns>returns the lines which are the faces of the downcomers end plate</returns>
        /// </summary>
        /// <param name="aClearance" and offset to move the returned line orthoganly from the default limit line></param>
        /// <param name="aReducer"  a distance to to reduce the inside the edges of the downcomer</param>
        /// <param name="aScale"   ></param>
        /// <returns></returns>

        public List<uopLinePair> GenerateLimitLines(double aClearance = 0, double aReducer = 0, bool bRegenLimits = false)
        {
            List<uopLinePair> _rVal = new List<uopLinePair>();
            List<ULINEPAIR> limlines = GenerateLimLines(aClearance, aReducer, bRegenLimits: bRegenLimits);
            foreach (var pair in limlines)
            {
                _rVal.Add(new uopLinePair(pair));
            }
            return _rVal;

        }

        internal List<ULINEPAIR> GenerateLimLines(double aClearance = 0, double aReducer = 0, bool bRegenLimits = false)
        {

            if (bRegenLimits) CreatedDefinitionLines();
            List<ULINEPAIR> lims = LimLines;
            List<ULINEPAIR> _rVal = new List<ULINEPAIR>();
            foreach (var pair in lims)
            {
                ULINEPAIR keep = new ULINEPAIR(pair);
                if (aClearance != 0 || aReducer != 0)
                {
                    keep.Line1 = ApplyOptions(pair.Line1.Value, aClearance, aReducer);
                    keep.Line2 = ApplyOptions(pair.Line2.Value, -aClearance, aReducer);

                }

                _rVal.Add(keep);


            }

            return _rVal;


        }

        /// <summary>
        ///executed to rebuild the persistent line pairs of limit, box and weir
        /// </summary>
        internal void CreatedDefinitionLines()
        {
            _BoxLns = CreateBoxLines();
            _LimLines = CreateLimLines(aBoxLines: _BoxLns);
            _WeirLns = CreateWeirLines(aLimLines: _LimLines);

            if (_BoxLns.Count > 1)
            {
                List<ULINEPAIR> smallweirs = _WeirLns.FindAll((x) => x.MinLength < mdGlobals.MinWeirLength);
                foreach (var item in smallweirs)
                {

                    _WeirLns.RemoveAll(x => x.Row == item.Row && x.Col == item.Col);
                    _BoxLns.RemoveAll(x => x.Row == item.Row && x.Col == item.Col);
                    _LimLines.RemoveAll(x => x.Row == item.Row && x.Col == item.Col);


                }
            }

        }

        private ULINE ApplyOptions(ULINE aLimitLine, double aClearance = 0, double aReducer = 0)
        {
            var _rVal = new ULINE(aLimitLine);
            double aDCInsideWith = InsideWidth;
            bool triang = Math.Round(aLimitLine.Y(true) - aLimitLine.Y(false), 2) != 0;
            double aX = X; //center of downcomer

            aDCInsideWith = Math.Abs(Math.Round(aDCInsideWith, 8));

            double wd = aDCInsideWith / 2;
            double xl = aX - wd;
            double xr = aX + wd;

            //apply the passed clearance
            bool bTrimIt = false;
            if (aClearance != 0)
            {
                if (aLimitLine.X(false) < aLimitLine.X(true))
                    _rVal.MoveOrtho(-aClearance);
                else
                    _rVal.MoveOrtho(aClearance);
                bTrimIt = true;
            }

            //move the bounds
            if (aReducer > 0)
            {
                if (aReducer > wd - 0.125)
                    aReducer = wd - 0.125;
                xl += aReducer;
                xr -= aReducer;
                bTrimIt = true;

            }

            bool isBeamLimitLine = aLimitLine.Tag.Contains("DIVIDER");
            if (bTrimIt)
            {
                if (triang || isBeamLimitLine)
                { _rVal.sp = _rVal.IntersectionPt(new ULINE(xl, 0, xl, 100)); }
                else
                { _rVal.sp.X = xl; }
                if (triang || isBeamLimitLine)
                { _rVal.ep = _rVal.IntersectionPt(new ULINE(xr, 0, xr, 100)); }
                else
                { _rVal.ep.X = xr; }

            }

            return _rVal;
        }

        internal List<ULINEPAIR> CreateDividerLns(double? aTrimRadius = null, double? aOffset = null)
        {

            List<ULINEPAIR> _rVal = new List<ULINEPAIR>();
            if (DesignFamily.IsStandardDesignFamily()) return _rVal;



            double rad = aTrimRadius.HasValue ? aTrimRadius.Value : RingRadius;
            double offset = !aOffset.HasValue ? 0 : aOffset.Value;

            double wd = BoxWidth / 2;

            if (rad <= 0) return _rVal;

            UARC trimmer = new UARC(UVECTOR.Zero, rad);
            ULINE? l1;
            ULINE? l2;
            dxfPlane dvplane = Divider.Plane;

            if (DesignFamily.IsBeamDesignFamily())
            {
                l1 = new ULINE(dvplane, 2 * rad, 0, 0.5 * Divider.Width + offset, aTrimArc: trimmer) { Side = uppSides.Top }; // the top of the top divider
                l2 = new ULINE(dvplane, 2 * rad, 0, -0.5 * Divider.Width - offset, aTrimArc: trimmer) { Side = uppSides.Bottom }; // the bottom of the top divider
                _rVal.Add(new ULINEPAIR(l1, l2, aTag: $"DIVIDER_1"));
                if (Divider.Offset != 0)
                {
                    dvplane.Project(dvplane.YDirection, -2 * Divider.Offset);
                    l1 = new ULINE(dvplane, 2 * rad, 0, 0.5 * Divider.Width + offset, aTrimArc: trimmer) { Side = uppSides.Top }; // the top of the top divider
                    l2 = new ULINE(dvplane, 2 * rad, 0, -0.5 * Divider.Width - offset, aTrimArc: trimmer) { Side = uppSides.Bottom }; // the bottom of the top divider
                    _rVal.Add(new ULINEPAIR(l1, l2, aTag: $"DIVIDER_2"));
                }

            }
            else if (DesignFamily.IsDividedWallDesignFamily())
            {
                l1 = new ULINE(dvplane, 2 * rad, 0, 0.5 * Divider.Width + offset, aTrimArc: trimmer) { Side = uppSides.Top }; // the top of the top divider
                l2 = new ULINE(dvplane, 2 * rad, 0, -0.5 * Divider.Width - offset, aTrimArc: trimmer) { Side = uppSides.Bottom }; // the bottom of the top divider
                _rVal.Add(new ULINEPAIR(l1, l2, aTag: $"DIVIDER_1"));

            }
            return _rVal;
        }

        internal List<ULINEPAIR> CreateWeirLines(bool bIncludeSuppressed = false, double aOffset = 0, List<ULINEPAIR> aLimLines = null)
        {

            List<ULINEPAIR> _rVal = new List<ULINEPAIR>();
            List<ULINEPAIR> lims = bIncludeSuppressed ? CreateLimLines(true) : aLimLines == null ? LimLines : aLimLines;
            foreach (var pair in lims)
            {
                ULINEPAIR weirs = pair.Connectors();
                weirs.ZoneIndex =  pair.ZoneIndex;
                ULINE lweir = weirs.Line1.Value;
                ULINE rweir = weirs.Line2.Value;
                weirs.Row = pair.Row;
                weirs.Col = DCIndex;
                lweir.Side = lweir.sp.X < rweir.sp.X ? uppSides.Left : uppSides.Right;
                rweir.Side = lweir.Side == uppSides.Right ? uppSides.Left : uppSides.Right;

                if (aOffset != 0)
                {
                    lweir.Move(aOffset);
                    rweir.Move(-aOffset);
                }
                weirs.Line1 = lweir;
                weirs.Line2 = rweir;
                weirs.IsVirtual = pair.IsVirtual;

                _rVal.Add(weirs);

            }
            return _rVal;
        }

        /// <summary>
        ///returns the lines which are the limit lines for the end support
        /// </summary>
        internal List<ULINEPAIR> CreateEndSupportLines()
        {
            List<ULINEPAIR> _rVal = new List<ULINEPAIR>();

            int maxrow = MaxRow;
            GetEndSupportTops(out double yTopL, out double yTopR, out double _, out double _);

            double yBotL = -yTopL;
            double yBotR = -yTopR;

            double xright = X_Outside_Right;
            double xleft = X_Outside_Left;
            double thk = Thickness;
            double limrad = BoundingRadius + 0.001;
            double lap = DeckLap;
            double rad = DeckRadius;
            UVECTOR sp1 = new UVECTOR(xleft + thk, yTopL);
            UVECTOR sp2 = new UVECTOR(xright - thk, yTopR);
            UVECTOR ep1 = new UVECTOR(xleft + thk, yBotL);
            UVECTOR ep2 = new UVECTOR(xright - thk, yBotR);

            UARC trimarc = new UARC(UVECTOR.Zero, rad);
            uppIntersectionTypes IntTypeT = uppIntersectionTypes.ToRing;
            uppIntersectionTypes IntTypeB = uppIntersectionTypes.ToRing;

            List<ULINEPAIR> allLims = LimLines.ToList();
            List<ULINEPAIR> allBxs = BoxLns.ToList();
            ULINEPAIR boxedges = new ULINEPAIR(new ULINE(aSPX: X_Outside_Left + thk, aSPY: 2 * limrad, aEPX: X_Outside_Left + thk, aEPY: -2 * limrad, aSide: uppSides.Left), new ULINE(aSPX: X_Outside_Right - thk, aSPY: 2 * limrad, aEPX: X_Outside_Right - thk, aEPY: -2 * limrad, aSide: uppSides.Right));

            ULINEPAIR trimmers = new ULINEPAIR(new ULINE(aSPX: X_Outside_Left + 2 * thk, aSPY: 2 * limrad, aEPX: X_Outside_Left + 2 * thk, aEPY: -2 * limrad, aSide: uppSides.Left), new ULINE(aSPX: X_Outside_Right - 2 * thk, aSPY: 2 * limrad, aEPX: X_Outside_Right - 2 * thk, aEPY: -2 * limrad, aSide: uppSides.Right));
            List<ULINEPAIR> dividers = CreateDividerLns(BoundingRadius, -lap);
            foreach (ULINEPAIR limpair in allLims)
            {
                int row = limpair.Row;
                ULINEPAIR bxlns = allBxs.Find((x) => x.Row == row);


                IntTypeT = limpair.IntersectionType1;
                IntTypeB = limpair.IntersectionType2;
                ULINE limT = limpair.GetSideValue(uppSides.Top);
                ULINE limB = limpair.GetSideValue(uppSides.Bottom);

                ULINE lineT = ULINE.Null;
                ULINE lineB = ULINE.Null;
                if (IntTypeT == uppIntersectionTypes.ToRing)
                {
                    lineB = new ULINE(limT, aSide: uppSides.Bottom) { Tag = "LIMIT" };
                    lineB.MoveOrtho(thk);
                    lineB.TrimToPair(trimmers);
                    lineT = new ULINE(sp1, sp2, aSide: uppSides.Top) { Tag = "LAP" };
                    ULINEPAIR pair = new ULINEPAIR(lineT, lineB) { Value = X, PartIndex = DCIndex, Col = DCIndex, Row = row, IsVirtual = limpair.IsVirtual, IntersectionType1 = uppIntersectionTypes.ToRing, IntersectionType2 = uppIntersectionTypes.ToRing };
                    _rVal.Add(pair);
                }
                else if (IntTypeT == uppIntersectionTypes.ToDivider)
                {
                    lineB = new ULINE(limT, aSide: uppSides.Bottom) { Tag = "LIMIT" };
                    lineB.MoveOrtho(thk);
                    lineB.TrimToPair(trimmers);
                    ULINEPAIR divider = row < maxrow ? dividers.First() : dividers.Last();
                    ULINE l1 = divider.GetSideValue(uppSides.Bottom);
                    ULINE l2 = boxedges.GetSideValue(uppSides.Left);
                    ULINE l3 = boxedges.GetSideValue(uppSides.Right);


                    lineT = new ULINE(l1.IntersectionPt(l2), l1.IntersectionPt(l3), aSide: uppSides.Top) { Tag = "LAP" };
                    ULINEPAIR pair = new ULINEPAIR(lineT, lineB) { Value = X, PartIndex = DCIndex, Col = DCIndex, Row = row, IsVirtual = limpair.IsVirtual, IntersectionType1 = uppIntersectionTypes.ToDivider, IntersectionType2 = uppIntersectionTypes.ToDivider };
                    _rVal.Add(pair);
                }
                else if (IntTypeT == uppIntersectionTypes.StraddlesRingToDivider)
                {
                    lineB = new ULINE(limT, aSide: uppSides.Bottom) { Tag = "LIMIT" };
                    lineB.MoveOrtho(thk);
                    lineB.TrimToPair(trimmers);

                    ULINE l1;
                    ULINE l2 = boxedges.GetSideValue(uppSides.Left);
                    ULINE l3 = boxedges.GetSideValue(uppSides.Right);

                    List<UVECTOR> dividerIntersections = new List<UVECTOR>();
                    foreach (var divider in dividers)
                    {
                        l1 = divider.GetSideValue(uppSides.Bottom);
                        dividerIntersections.Add(l1.IntersectionPt(l2));
                    }
                    UVECTOR dividerIntersection = dividerIntersections.OrderBy(di => di.Y).First();

                    UVECTOR ip = l3.Intersections(trimarc, true, true).GetVector(dxxPointFilters.AtMaxY);
                    //ip.Y = Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(ip.X, 2));
                    lineT = new ULINE(dividerIntersection, ip, aSide: uppSides.Top) { Tag = "LAP" };
                    ULINEPAIR pair = new ULINEPAIR(lineT, lineB) { Value = X, PartIndex = DCIndex, Col = DCIndex, Row = row, IsVirtual = limpair.IsVirtual, IntersectionType1 = uppIntersectionTypes.ToDivider, IntersectionType2 = uppIntersectionTypes.ToRing };
                    _rVal.Add(pair);
                }

                if (IntTypeB == uppIntersectionTypes.ToRing)
                {
                    lineT = new ULINE(limB, aSide: uppSides.Top) { Tag = "LIMIT" };
                    lineT.MoveOrtho(-thk);
                    lineT.TrimToPair(trimmers);
                    lineB = new ULINE(ep1, ep2, aSide: uppSides.Bottom) { Tag = "LAP" };
                    ULINEPAIR pair = new ULINEPAIR(lineT, lineB) { Value = X, PartIndex = DCIndex, Col = DCIndex, Row = row, IsVirtual = limpair.IsVirtual, IntersectionType1 = uppIntersectionTypes.ToRing, IntersectionType2 = uppIntersectionTypes.ToRing };
                    _rVal.Add(pair);
                }
                else if (IntTypeB == uppIntersectionTypes.ToDivider)
                {
                    lineT = new ULINE(limB, aSide: uppSides.Top) { Tag = "LIMIT" };
                    lineT.MoveOrtho(-thk);
                    lineT.TrimToPair(trimmers);
                    ULINEPAIR divider = row > 1 ? dividers.Last() : dividers.First();
                    ULINE l1 = divider.GetSideValue(uppSides.Top);
                    ULINE l2 = boxedges.GetSideValue(uppSides.Left);
                    ULINE l3 = boxedges.GetSideValue(uppSides.Right);


                    lineB = new ULINE(l1.IntersectionPt(l2), l1.IntersectionPt(l3), aSide: uppSides.Top) { Tag = "LAP" };
                    ULINEPAIR pair = new ULINEPAIR(lineT, lineB) { Value = X, PartIndex = DCIndex, Col = DCIndex, Row = row, IsVirtual = limpair.IsVirtual, IntersectionType1 = uppIntersectionTypes.ToDivider, IntersectionType2 = uppIntersectionTypes.ToRing };
                    _rVal.Add(pair);
                }

            }
            return _rVal;
        }

        /// <summary>
        ///returns the lines which are the outside edges of the boxes support shelf angles
        /// </summary>
        internal List<ULINEPAIR> CreateShelfLines()
        {
            List<ULINEPAIR> _rVal = new List<ULINEPAIR>();
            List<ULINE> trimmers = Divider != null ? uopLinePair.ToULineList(CreateDividerLns(BoundingRadius, Divider.RingClipOffset)) : new List<ULINE>();
            UARC trimarc = new UARC(BoundingRadius);
            foreach (var bxpair in BoxLns)
            {
                ULINE bxL = bxpair.GetSideValue(uppSides.Left);
                ULINE bxR = bxpair.GetSideValue(uppSides.Right);
                UVECTOR cp = bxpair.Center;
                ULINE shelfL = bxL.Moved(-ShelfWidth);
                ULINE shelfR = bxR.Moved(ShelfWidth);
                UVECTORS ptsL = UVECTORS.Zero;
                UVECTORS ptsR = UVECTORS.Zero;
                UVECTOR up = new UVECTOR(0, 1);
                UVECTOR down = new UVECTOR(0, -1);

                if (X >= 0)
                {
                    ptsL = trimarc.Intersections(bxL, aArcIsInfinite: true, aLineIsInfinite: true);

                    for (int i = 1; i <= ptsL.Count; i++)
                    {
                        ptsL.SetItem(i, new UVECTOR(shelfL.sp.X, ptsL.Item(i).Y));
                    }
                    ptsR = trimarc.Intersections(shelfR, aArcIsInfinite: true, aLineIsInfinite: true);

                }
                else
                {
                    ptsL = trimarc.Intersections(shelfL, aArcIsInfinite: true, aLineIsInfinite: true);
                    ptsR = trimarc.Intersections(bxR, aArcIsInfinite: true, aLineIsInfinite: true);

                    for (int i = 1; i <= ptsR.Count; i++)
                    {
                        ptsR.SetItem(i, new UVECTOR(shelfR.sp.X, ptsR.Item(i).Y));
                    }

                }

                if (trimmers.Count > 0)
                {

                    foreach (ULINE trimmer in trimmers)
                    {
                        UVECTOR u1 = cp.ProjectedTo(trimmer, out UVECTOR dir, out double _);
                        ULINE interceptor = dir.X < 0 ? bxR : shelfR;
                        u1 = interceptor.IntersectionPt(trimmer);
                        ptsR.Add(new UVECTOR(shelfR.sp.X, u1.Y));

                        interceptor = dir.X < 0 ? shelfL : bxL;
                        u1 = interceptor.IntersectionPt(trimmer);
                        ptsL.Add(new UVECTOR(shelfL.sp.X, u1.Y));


                    }

                }

                UVECTOR mp = shelfL.MidPt;
                shelfL.sp = ptsL.Nearest(mp, aDir: up);
                shelfL.ep = ptsL.Nearest(mp, aDir: down);

                mp = shelfR.MidPt;
                shelfR.sp = ptsR.Nearest(mp, aDir: up);
                shelfR.ep = ptsR.Nearest(mp, aDir: down);
                _rVal.Add(new ULINEPAIR(shelfL, shelfR, aTag: "SHELF") { Value = X, Col = DCIndex, Row = bxpair.Row });
            }
            return _rVal;



        }

        /// <summary>
        ///returns the lines which are the outside edges of the boxes
        /// </summary>
        internal List<ULINEPAIR> CreateBoxLines()
        {

           
            GetBoxTops(out double yTopL, out double yTopR);

            double yBotL = -yTopL;
            double yBotR = -yTopR;
            List<ULINEPAIR> _rVal = new List<ULINEPAIR>();
            double xright = X_Outside_Right;
            double xleft = X_Outside_Left;
            double thk = Thickness;
            double limrad = BoundingRadius + 0.001;
            UVECTOR sp1 = new UVECTOR(xleft, yTopL);
            UVECTOR sp2 = new UVECTOR(xright, yTopR);
            UVECTOR ep1 = new UVECTOR(xleft, yBotL);
            UVECTOR ep2 = new UVECTOR(xright, yBotR);

            int maxrow = MaxRow;
            uppIntersectionTypes IntTypeT = uppIntersectionTypes.ToRing;
            uppIntersectionTypes IntTypeB = uppIntersectionTypes.ToRing;

            ULINEPAIR pair = new ULINEPAIR(new ULINE(sp1, ep1, aSide: uppSides.Left), new ULINE(sp2, ep2, aSide: uppSides.Right), aTag: "BOX") { Value = X, PartIndex = DCIndex, Col = DCIndex, Row = maxrow, IntersectionType1 = IntTypeT, IntersectionType2 = IntTypeB, ZoneIndex = 1 };

            if (DesignFamily.IsStandardDesignFamily())
            {
                pair.IsVirtual = IsVirtual;
                pair.ZoneIndex = IsVirtual ? -1 : 1;
                _rVal.Add(pair);
                return _rVal;
            }
            ULINE left = new ULINE(sp1, ep1, aSide: uppSides.Left, aTrimArc: new UARC(RingRadius));
            ULINE right = new ULINE(sp2, ep2, aSide: uppSides.Right, aTrimArc: new UARC(RingRadius));
            List<ULINEPAIR> trimmers = CreateDividerLns(BoundingRadius, Divider.RingClipOffset);
            left.Points = left.Intersections(trimmers, aLineIsInfinite: false, aLinesAreInfinite: true);
            right.Points = right.Intersections(trimmers, aLineIsInfinite: false, aLinesAreInfinite: true);

            if (right.Points.Count == 0 && left.Points.Count == 0)  // the dc does not intersect the divider at all
            {
                pair.Row = X <= 0 ? 1 : maxrow;
                pair.IsVirtual = pair.Row == 1;
                pair.ZoneIndex = pair.Row == maxrow ? 1 : pair.IsVirtual ? -1 : 2;
                if (DesignFamily.IsBeamDesignFamily() || (DesignFamily.IsDividedWallDesignFamily() && pair.Row == maxrow)) _rVal.Add(pair);
                return _rVal;
            }

            //create the default lineset
            List<ULINEPAIR?> potentials = maxrow == 2 ? new List<ULINEPAIR?>() { null, null } : new List<ULINEPAIR?>() { null, null, null };

            UVECTOR u1 = UVECTOR.Zero;
            UVECTOR u2 = UVECTOR.Zero;
            UVECTOR u3 = UVECTOR.Zero;
            UVECTOR u4 = UVECTOR.Zero;
            double overhang = EndPlateOverhang;
            double rad = RingRadius;
            double xl1 = X_Inside_Left;
            double xl2 = xl1 + thk;
            double xr1 = X_Inside_Right - thk;
            double xr2 = xr1 + thk;
            int row = maxrow;
            ULINE l1 = new ULINE(xl1, yTopL, xl1, yBotL);
            ULINE l2 = new ULINE(xl2, yTopL, xl2, yBotL);
            ULINE r1 = new ULINE(xr1, yTopR, xr1, yBotR);
            ULINE r2 = new ULINE(xr2, yTopR, xr2, yBotR);
            ULINE bxL = left;
            ULINE bxR = right;

            // below the bottom divider
            row = maxrow;
            ULINE trimmer = trimmers[trimmers.Count - 1].GetSide(uppSides.Bottom).Value;
            bool hitsonleft = left.Intersects(trimmer, out _, bMustBeOn1: false, bMustBeOn2: true);
            bool hitsonright = right.Intersects(trimmer, out _, bMustBeOn1: false, bMustBeOn2: true);

            if (hitsonleft)
            {
                u1 = l1.IntersectionPt(trimmer); u1.Y -= overhang;
                IntTypeT = uppIntersectionTypes.ToDivider;
                IntTypeB = uppIntersectionTypes.ToRing;
                if (hitsonright)
                {
                    u2 = r1.IntersectionPt(trimmer);
                    u2.Y -= overhang;
                }
                else
                {
                    IntTypeT = uppIntersectionTypes.StraddlesRingToDivider;
                    u2 = sp2;
                }

                bxL = new ULINE(xleft, u1.Y, xleft, yBotL, aCol: DCIndex, aRow: row, aSide: uppSides.Left);
                bxR = new ULINE(xright, u2.Y, xright, yBotR, aCol: DCIndex, aRow: row, aSide: uppSides.Right);
                pair = new ULINEPAIR(bxL, bxR, aTag: "BOX") { Value = X, PartIndex = DCIndex, Col = DCIndex, Row = row, IntersectionType1 = IntTypeT, IntersectionType2 = IntTypeB };
                potentials[row - 1] = pair;
            }

            if (DesignFamily.IsBeamDesignFamily())
            {
                row = 1;
                trimmer = trimmers[0].GetSide(uppSides.Top).Value;
                hitsonleft = left.Intersects(trimmer, out _, bMustBeOn1: false, bMustBeOn2: true);
                hitsonright = right.Intersects(trimmer, out _, bMustBeOn1: false, bMustBeOn2: true);

                if (hitsonright)
                {
                    u1 = l2.IntersectionPt(trimmer); u1.Y += overhang;
                    u2 = r2.IntersectionPt(trimmer); u2.Y += overhang;
                    IntTypeT = uppIntersectionTypes.ToRing;
                    IntTypeB = uppIntersectionTypes.ToDivider;
                    if (!hitsonleft)
                    {
                        IntTypeB = uppIntersectionTypes.StraddlesRingToDivider;
                        u1 = ep1;
                    }

                    bxL = new ULINE(xleft, yTopL, xleft, u1.Y, aCol: DCIndex, aRow: row, aSide: uppSides.Left);
                    bxR = new ULINE(xright, yTopR, xright, u2.Y, aCol: DCIndex, aRow: row, aSide: uppSides.Right);
                    pair = new ULINEPAIR(bxL, bxR, aTag: "BOX") { Value = X, PartIndex = DCIndex, Col = DCIndex, Row = row, IntersectionType1 = IntTypeT, IntersectionType2 = IntTypeB, IsVirtual = true };
                    potentials[row - 1] = pair;
                }

                if (Divider.Offset != 0)
                {
                    trimmer = trimmers[0].GetSide(uppSides.Bottom).Value;
                    ULINE trimmer2 = trimmers[1].GetSide(uppSides.Top).Value;
                    hitsonleft = left.Intersects(trimmer, out _, bMustBeOn1: false, bMustBeOn2: true);
                    hitsonright = right.Intersects(trimmer, out _, bMustBeOn1: false, bMustBeOn2: true);
                    bool hitsonleftB = left.Intersects(trimmer2, out _, bMustBeOn1: false, bMustBeOn2: true);
                    bool hitsonrightB = right.Intersects(trimmer2, out _, bMustBeOn1: false, bMustBeOn2: true);
                    row = 2;
                    // intersects top and bottom divider fully
                    if (hitsonleft && hitsonright && hitsonleftB && hitsonrightB)
                    {
                        IntTypeT = uppIntersectionTypes.ToDivider;
                        IntTypeB = uppIntersectionTypes.ToDivider;
                        u1 = l1.IntersectionPt(trimmer); u1.Y -= overhang;
                        u3 = r1.IntersectionPt(trimmer); u3.Y -= overhang;
                        u2 = l2.IntersectionPt(trimmer2); u2.Y += overhang;
                        u4 = r2.IntersectionPt(trimmer2); u4.Y += overhang;

                        bxL = new ULINE(xleft, u1.Y, xleft, u2.Y, aCol: DCIndex, aRow: row, aSide: uppSides.Left);
                        bxR = new ULINE(xright, u3.Y, xright, u4.Y, aCol: DCIndex, aRow: row, aSide: uppSides.Right);
                        pair = new ULINEPAIR(bxL, bxR, aTag: "BOX") { Value = X, PartIndex = DCIndex, Col = DCIndex, Row = row, IntersectionType1 = IntTypeT, IntersectionType2 = IntTypeB, IsVirtual = X < 0 };
                        potentials[row - 1] = pair;
                    }
                    // intersects top divider fully and the bottom divider only on the right
                    else if (hitsonleft && hitsonright && !hitsonleftB && hitsonrightB)
                    {
                        u1 = l1.IntersectionPt(trimmer); u1.Y -= overhang;
                        u3 = r1.IntersectionPt(trimmer); u3.Y -= overhang;
                        IntTypeT = uppIntersectionTypes.ToDivider;
                        IntTypeB = uppIntersectionTypes.StraddlesRingToDivider;
                        u2 = ep1;
                        u4 = r2.IntersectionPt(trimmer2); u4.Y += overhang;

                        bxL = new ULINE(xleft, u1.Y, xleft, u2.Y, aCol: DCIndex, aRow: row, aSide: uppSides.Left);
                        bxR = new ULINE(xright, u3.Y, xright, u4.Y, aCol: DCIndex, aRow: row, aSide: uppSides.Right);
                        pair = new ULINEPAIR(bxL, bxR, aTag: "BOX") { Value = X, PartIndex = DCIndex, Col = DCIndex, Row = row, IntersectionType1 = IntTypeT, IntersectionType2 = IntTypeB, IsVirtual = X < 0 };
                        potentials[row - 1] = pair;
                    }
                    // intersects top divider fully and the ring below
                    else if (hitsonleft && hitsonright && !hitsonleftB && !hitsonrightB)
                    {
                        IntTypeT = uppIntersectionTypes.ToDivider;
                        IntTypeB = uppIntersectionTypes.ToRing;
                        u1 = l1.IntersectionPt(trimmer); u1.Y -= overhang;
                        u3 = r1.IntersectionPt(trimmer); u3.Y -= overhang;
                        u2 = ep1;
                        u4 = ep2;

                        bxL = new ULINE(xleft, u1.Y, xleft, u2.Y, aCol: DCIndex, aRow: row, aSide: uppSides.Left);
                        bxR = new ULINE(xright, u3.Y, xright, u4.Y, aCol: DCIndex, aRow: row, aSide: uppSides.Right);
                        pair = new ULINEPAIR(bxL, bxR, aTag: "BOX") { Value = X, PartIndex = DCIndex, Col = DCIndex, Row = row, IntersectionType1 = IntTypeT, IntersectionType2 = IntTypeB, IsVirtual = X < 0 };
                        potentials[row - 1] = pair;
                    }
                    // intersects the bottom divider fully but only on the left at the top
                    else if (hitsonleft && !hitsonright && hitsonleftB && hitsonrightB)
                    {
                        u2 = l2.IntersectionPt(trimmer2); u2.Y += overhang;
                        u4 = r2.IntersectionPt(trimmer2); u4.Y += overhang;

                        IntTypeT = uppIntersectionTypes.StraddlesRingToDivider;
                        IntTypeB = uppIntersectionTypes.ToDivider;
                        u1 = l2.IntersectionPt(trimmer); u1.Y -= overhang;
                        u3 = sp2;
                        bxL = new ULINE(xleft, u1.Y, xleft, u2.Y, aCol: DCIndex, aRow: row, aSide: uppSides.Left);
                        bxR = new ULINE(xright, u3.Y, xright, u4.Y, aCol: DCIndex, aRow: row, aSide: uppSides.Right);
                        pair = new ULINEPAIR(bxL, bxR, aTag: "BOX") { Value = X, PartIndex = DCIndex, Col = DCIndex, Row = row, IntersectionType1 = IntTypeT, IntersectionType2 = IntTypeB, IsVirtual = X < 0 };
                        potentials[row - 1] = pair;
                    }
                    // intersects the bottom divider fully and the ring at the top
                    else if (!hitsonleft && !hitsonright && hitsonleftB && hitsonrightB)
                    {
                        u2 = l2.IntersectionPt(trimmer2); u2.Y += overhang;
                        u4 = r2.IntersectionPt(trimmer2); u4.Y += overhang;
                        u1 = sp1;
                        u3 = sp2;
                        IntTypeT = uppIntersectionTypes.ToRing;
                        IntTypeB = uppIntersectionTypes.ToDivider;
                        bxL = new ULINE(xleft, u1.Y, xleft, u2.Y, aCol: DCIndex, aRow: row, aSide: uppSides.Left);
                        bxR = new ULINE(xright, u3.Y, xright, u4.Y, aCol: DCIndex, aRow: row, aSide: uppSides.Right);
                        pair = new ULINEPAIR(bxL, bxR, aTag: "BOX") { Value = X, PartIndex = DCIndex, Col = DCIndex, Row = row, IntersectionType1 = IntTypeT, IntersectionType2 = IntTypeB, IsVirtual = X < 0 };
                        potentials[row - 1] = pair;
                    }
                    // intersects ring on the top but the bottom divider only on the left
                    else if (!hitsonleft && !hitsonright && hitsonleftB && !hitsonrightB)
                    {
                        u1 = sp1;
                        u3 = sp2;
                        u2 = l2.IntersectionPt(trimmer2); u2.Y += overhang;
                        u4 = r2.IntersectionPt(trimmer2); u2.Y += overhang;
                        IntTypeT = uppIntersectionTypes.ToRing;
                        IntTypeB = uppIntersectionTypes.ToDivider;
                        bxL = new ULINE(xleft, u1.Y, xleft, u2.Y, aCol: DCIndex, aRow: row, aSide: uppSides.Left);
                        bxR = new ULINE(xright, u3.Y, xright, u4.Y, aCol: DCIndex, aRow: row, aSide: uppSides.Right);
                        pair = new ULINEPAIR(bxL, bxR, aTag: "BOX") { Value = X, PartIndex = DCIndex, Col = DCIndex, Row = row, IntersectionType1 = IntTypeT, IntersectionType2 = IntTypeB, IsVirtual = X < 0 };
                        if (u4.Y < u3.Y - 2)
                            potentials[row - 1] = pair;
                    }
                    // intersects top ring on the right and the ring below
                    else if (!hitsonleft && hitsonright && !hitsonleftB && !hitsonrightB)
                    {
                        u1 = l1.IntersectionPt(trimmer); u1.Y -= overhang;
                        u3 = r2.IntersectionPt(trimmer); u3.Y -= overhang;
                        u2 = ep1;
                        u4 = ep2;
                        IntTypeT = uppIntersectionTypes.ToDivider;
                        IntTypeB = uppIntersectionTypes.ToRing;
                        bxL = new ULINE(xleft, u1.Y, xleft, u2.Y, aCol: DCIndex, aRow: row, aSide: uppSides.Left);
                        bxR = new ULINE(xright, u3.Y, xright, u4.Y, aCol: DCIndex, aRow: row, aSide: uppSides.Right);
                        pair = new ULINEPAIR(bxL, bxR, aTag: "BOX") { Value = X, PartIndex = DCIndex, Col = DCIndex, Row = row, IntersectionType1 = IntTypeT, IntersectionType2 = IntTypeB, IsVirtual = X < 0 };
                        if (u2.Y < u1.Y - 2)
                            potentials[row - 1] = pair;
                    }
                }
            }

            double roundfactorDiv = RoundingMethod == uppMDRoundToLimits.Sixteenth ? 0.03125 : 0.019685;
            double roundfactorRng = RoundingMethod == uppMDRoundToLimits.Sixteenth ? 0.0625 : 0.03937;

            foreach (var item in potentials)
            {
                if (!item.HasValue) continue;
             
                pair = item.Value;
                bxL = pair.GetSideValue(uppSides.Left);
                bxR = pair.GetSideValue(uppSides.Right);
                if (bxL.sp.Y < bxL.ep.Y) bxL.Invert();
                if (bxR.sp.Y < bxR.ep.Y) bxR.Invert();
                IntTypeT = pair.IntersectionType1;
                IntTypeB = pair.IntersectionType2;
                
                pair.ZoneIndex = -1;
                // round box lengths
                if (RoundingMethod != uppMDRoundToLimits.None)
                {


                    if (IntTypeT == uppIntersectionTypes.ToDivider)
                    {
                        bxL.sp.Y = uopUtils.RoundIt(bxL.sp.Y, roundfactorDiv, aStyle: dxxRoundingTypes.Down);
                        bxR.sp.Y = uopUtils.RoundIt(bxR.sp.Y, roundfactorDiv, aStyle: dxxRoundingTypes.Down);
                    }
                    else if (IntTypeT == uppIntersectionTypes.StraddlesRingToDivider)
                    {
                        if (X >= 0)
                        {
                            bxL.sp.Y = uopUtils.RoundIt(bxL.sp.Y, roundfactorDiv, aStyle: dxxRoundingTypes.Down);
                            bxR.sp.Y = yTopR; // uopUtils.RoundIt(bxR.sp.Y, roundfactorRng, bRoundDown: true);
                        }
                        else
                        {
                            bxR.sp.Y = uopUtils.RoundIt(bxR.sp.Y, roundfactorDiv, aStyle: dxxRoundingTypes.Down);
                            bxL.sp.Y = yTopL; // uopUtils.RoundIt(bxL.sp.Y, roundfactorRng, bRoundDown: true);
                        }
                    }
                    else if (IntTypeT == uppIntersectionTypes.ToRing)
                    {
                        bxL.sp.Y = yTopL; // uopUtils.RoundIt(bxL.sp.Y, roundfactorRng, bRoundDown: true);
                        bxR.sp.Y = yTopR; // uopUtils.RoundIt(bxR.sp.Y, roundfactorRng, bRoundDown: true);
                    }

                    if (IntTypeB == uppIntersectionTypes.ToDivider)
                    {
                        bxL.ep.Y = uopUtils.RoundIt(bxL.ep.Y, roundfactorDiv, aStyle: dxxRoundingTypes.Up);
                        bxR.ep.Y = uopUtils.RoundIt(bxR.ep.Y, roundfactorDiv, aStyle: dxxRoundingTypes.Up);
                    }
                    else if (IntTypeB == uppIntersectionTypes.StraddlesRingToDivider)
                    {
                        if (X >= 0)
                        {
                            bxL.ep.Y = uopUtils.RoundIt(bxL.ep.Y, roundfactorDiv, aStyle: dxxRoundingTypes.Up);
                            bxR.ep.Y = yBotR; //uopUtils.RoundIt(bxR.ep.Y, roundfactorRng, bRoundDown: true);
                        }
                        else
                        {
                            bxR.ep.Y = uopUtils.RoundIt(bxR.ep.Y, roundfactorDiv, aStyle: dxxRoundingTypes.Up);
                            bxL.ep.Y = yBotL; // uopUtils.RoundIt(bxL.ep.Y, roundfactorRng, bRoundDown: true);
                        }
                    }
                    else if (IntTypeT == uppIntersectionTypes.ToRing)
                    {
                        bxL.ep.Y = yBotL; // uopUtils.RoundIt(bxL.ep.Y, roundfactorRng);
                        bxR.ep.Y = yBotR; // uopUtils.RoundIt(bxR.ep.Y, roundfactorRng);
                    }
                    //uppSegmentPoints lbase = uppSegmentPoints.MidPt;

                    //bxL.Resize(uopUtils.RoundTo(pair.Line1.Value.Length, RoundUnits, bRoundDown: true), lbase);
                    //bxR.Resize(uopUtils.RoundTo(pair.Line2.Value.Length, RoundUnits, bRoundDown: true), lbase);
                }
                pair.SetSideValue(uppSides.Left, bxL);
                pair.SetSideValue(uppSides.Right, bxR);

                //filter out and box line sets that have a box length one either side less than the minimum (8)
                if (pair.MinLength >= mdGlobals.MinBoxLength)
                {
                 
                    pair.ZoneIndex = pair.Row ==maxrow ? 1 : pair.IsVirtual ? -1 : 2 ;
                    _rVal.Add(pair);
                }
                else
                {
                    if (pair.Row == 3 && !pair.IsDefined)
                        _rVal.Add(pair);
                }


            }

            return _rVal;

        }

        /// <summary>
        ///returns the lines which are the inner faces of the downcomers end plates
        /// </summary>
        internal List<ULINEPAIR> CreateLimLines(bool bIncludeSuppressed = false, List<ULINEPAIR> aBoxLines = null)
        {

            int maxrow = MaxRow;
            ULINEPAIR ringlines = CreateRingLimLine();
            if (!ringlines.IsDefined) return new List<ULINEPAIR>();   //  if the lines are not return insufficent data was detected or an error occured

            if (maxrow <= 1) return new List<ULINEPAIR>() { ringlines }; // standard design so just one set


            aBoxLines ??= _BoxLns;
            if (aBoxLines == null || aBoxLines.Count <= 0)
            {
                _BoxLns = CreateBoxLines();
                aBoxLines = _BoxLns;
            }

            //first get the lines at the ring on both ends
            double xl = X_Inside_Left;
            double xr = X_Inside_Right;
            List<ULINEPAIR> _rVal = new List<ULINEPAIR>();

            ULINEPAIR keeper;
            int row = 0;

            try
            {

                List<ULINE> dvidercls = DividerCenterLines();

                foreach (ULINEPAIR bxlns in _BoxLns)
                {

                    ULINE leftbx = bxlns.GetSide(uppSides.Left).Value;
                    ULINE rightbx = bxlns.GetSide(uppSides.Right).Value;
                    row = bxlns.Row;
                    int col = DCIndex;
                    double inset = (EndplateInset < 0) ? mdGlobals.DefaultEndplateInset : EndplateInset;
                    double iset1 = bxlns.IntersectionType1 == uppIntersectionTypes.ToRing ? inset : mdGlobals.DefaultEndplateInset;
                    double iset2 = iset1;
                    if (bxlns.IntersectionType1 == uppIntersectionTypes.StraddlesRingToDivider) iset2 = inset;
                    if (X < 0) mzUtils.SwapTwoValues(ref iset1, ref iset2);
                    double y1 = leftbx.MaxY - iset1;
                    double y2 = rightbx.MaxY - iset2;

                    ULINE toplim = new ULINE(xl, y1, xr, y2) { Row = row, Side = uppSides.Top, Col = col };

                    iset1 = bxlns.IntersectionType2 == uppIntersectionTypes.ToRing ? inset : mdGlobals.DefaultEndplateInset;
                    iset2 = iset1;
                    if (bxlns.IntersectionType2 == uppIntersectionTypes.StraddlesRingToDivider) iset2 = inset;
                    if (X < 0) mzUtils.SwapTwoValues(ref iset1, ref iset2);
                    y1 = leftbx.MinY + iset1;
                    y2 = rightbx.MinY + iset2;

                    ULINE botlim = new ULINE(xl, y1, xr, y2) { Row = row, Side = uppSides.Bottom, Col = col };


                    // create the ring clip holes
                    toplim.Points = RingClipCenters(toplim, bxlns.IntersectionType1, dvidercls);
                    botlim.Points = RingClipCenters(botlim, bxlns.IntersectionType2, dvidercls);
                    //make sure there is enough clip clearamce
                    EnforceRingClipClearance(ref toplim, bBottom: false);
                    EnforceRingClipClearance(ref botlim, bBottom: true);

                    //round insets up to the nearest 1/16th if round is on and make sure they are equal on both sides
                    RoundInsets(ref toplim, leftbx, rightbx, bBottom: false);
                    RoundInsets(ref botlim, leftbx, rightbx, bBottom: true);

                    double wl_L = toplim.sp.Y - botlim.sp.Y;
                    double wl_R = toplim.ep.Y - botlim.ep.Y;
                    bool suppressit = false; // (wl_L < mdGlobals.MinWeirLength || wl_R < mdGlobals.MinWeirLength);
                    bool keepit = !suppressit || bIncludeSuppressed;
                    if (keepit)
                    {
                        keeper = new ULINEPAIR(toplim, botlim)
                        {
                            IsVirtual = bxlns.IsVirtual,
                            IntersectionType1 = bxlns.IntersectionType1,
                            IntersectionType2 = bxlns.IntersectionType2,
                            Value = X,
                            PartIndex = DCIndex,
                            Col = DCIndex,
                            Row = row,
                            ZoneIndex = bxlns.ZoneIndex,
                            Suppressed = (wl_L < mdGlobals.MinWeirLength || wl_R < mdGlobals.MinWeirLength)
                        };
                        _rVal.Add(keeper);
                    }
                }


            }
            catch (Exception)
            {

                throw;
            }

            return _rVal;
        }


        /// <summary>
        ///returns the lines which are the inner faces of the downcomers end plates at the ring
        /// </summary>
        private ULINEPAIR CreateRingLimLine()
        {

            ULINEPAIR _rVal = new ULINEPAIR("TO RING") { Col = DCIndex, Row = 1, IntersectionType1 = uppIntersectionTypes.ToRing, IntersectionType2 = uppIntersectionTypes.ToRing };


            //calculate the end of the box on the left and right
            GetBoxTops(out double yTopL, out double yTopR);

            InsideWidth = Math.Abs(InsideWidth);
            //no width no lines
            if (InsideWidth <= 0)
                return _rVal;

            double thk = Math.Abs(Thickness);

            DeckRadius = Math.Abs(DeckRadius);
            ClipClearance = Math.Abs(ClipClearance);

            //negative passed values means the default position is used and the constraints are applied
            //postives passed values means the user is forcing the position of the line
            double inset = (EndplateInset < 0) ? mdGlobals.DefaultEndplateInset : EndplateInset;


            //trap for dc outside edge beyond the radius
            if (X_Inside_Right >= BoundingRadius) return _rVal;

            //uninterupted box length is the chord length of the assemblies bounding circle at X  (rounded if Rounding is on)

            ULINE leftbox = new ULINE(X_Inside_Left, yTopL, X_Inside_Left, -yTopL);
            ULINE rightbox = new ULINE(X_Inside_Right, yTopR, X_Inside_Right, -yTopR);

            //define the line by applying the inset to both sides from the box ends
            //line is oriented left to right!
            ULINE limline = new ULINE(X_Inside_Left, yTopL - inset, X_Inside_Right, yTopR - inset, aSide: uppSides.Top);
            //set the ring clip hole centers
            limline.Points = RingClipCenters_Ring(limline, bBottom: false);

            //make sure there is enough clip clearamce
            EnforceRingClipClearance(ref limline, bBottom: false);


            //round insets up to the nearest 1/16th (why? tradition?) and make sure they are equal on both sides
            RoundInsets(ref limline, leftbox, rightbox, bBottom: false);

            //the current line is the end plate inside face line with the end points
            //at the inside edges of the box. this line is used to determine
            //weir lengths so it is very, very important
            limline.Tag = "TO_RING_TOP";
            limline.Side = uppSides.Top;
            //limline.sp = limline.IntersectionPt(leftbox);
            //limline.ep = limline.IntersectionPt(rightbox);

            ULINE limline2 = limline.Mirrored(null, 0);
            limline2.Tag = "TO_RING_BOT";
            limline2.Side = uppSides.Bottom;
            _rVal.Line1 = limline;
            _rVal.Line2 = limline2;
            _rVal.Value = X;
            _rVal.PartIndex = DCIndex;
            _rVal.Row = 1;
            _rVal.Col = DCIndex;

            _rVal.IntersectionType1 = uppIntersectionTypes.ToRing;
            _rVal.IntersectionType2 = uppIntersectionTypes.ToRing;
            return _rVal;

        }


        object ICloneable.Clone() => (object)Clone();

        internal UVECTORS RingClipCenters(ULINE aLimLine, uppIntersectionTypes aIntersectType, List<ULINE> aDividerCLs = null)
        {
            switch (aIntersectType)
            {
                case uppIntersectionTypes.ToRing:
                    return RingClipCenters_Ring(aLimLine, aLimLine.Side == uppSides.Bottom);
                case uppIntersectionTypes.ToDivider:
                    return RingClipCenters_Divider(aLimLine, aLimLine.Side == uppSides.Bottom, aDividerCLs);
                case uppIntersectionTypes.StraddlesRingToDivider:
                    return RingClipCenters_Divider(aLimLine, aLimLine.Side == uppSides.Bottom, aDividerCLs, bStraddle: true);
            }
            return UVECTORS.Zero;
        }

        internal UVECTORS RingClipCenters_Ring(ULINE aLimLine, bool bBottom)
        {
            UVECTORS _rVal = UVECTORS.Zero;

            if (DeckRadius <= 0) return _rVal;
            Thickness = Math.Abs(Thickness);

            double rad = BoundingRadius;
            double holerad = RingClipHoleDiameter / 2;

            UVECTOR u1 = aLimLine.MidPt;
            double y1 = u1.Y;
            if (aLimLine.Length <= mdGlobals.TwoEndSupportBoltLimitLineLength)
            {

                y1 = Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(X, 2));
                if (bBottom) y1 *= -1;
                _rVal.Add(new UVECTOR(X, y1) { Radius = holerad });

            }
            else
            {
                double hx = X - InsideWidth / 2 + mdGlobals.DCRingClipHoleInset + Thickness;
                y1 = Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(hx, 2));
                if (bBottom) y1 *= -1;
                _rVal.Add(new UVECTOR(hx, y1) { Radius = holerad });
                hx = X + InsideWidth / 2 - mdGlobals.DCRingClipHoleInset - Thickness;
                y1 = Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(hx, 2));
                if (bBottom) y1 *= -1;
                _rVal.Add(new UVECTOR(hx, y1) { Radius = holerad });

            }

            return _rVal;
        }

        internal UVECTORS RingClipCenters_Divider(ULINE aLimLine, bool? bBottom = null, List<ULINE> aDividerCLs = null, bool bStraddle = false)
        {


            UVECTORS _rVal = UVECTORS.Zero;
            aDividerCLs ??= DividerCenterLines();
            if (!bBottom.HasValue) bBottom = aLimLine.Side == uppSides.Bottom;
            if (aDividerCLs.Count <= 0) return _rVal;
            Thickness = Math.Abs(Thickness);

            //get the divider center line
            ULINE dvcl = aDividerCLs[0];
            double delta = Divider.Width / 2 + Divider.RingClipOffset;
            double f1 = 1;
            switch (aLimLine.Row)
            {
                case 1:
                    {
                        break;
                    }
                case 2:
                    {
                        if (aDividerCLs.Count > 1)
                        {
                            if (bBottom.Value)
                                dvcl = aDividerCLs[1];
                            else
                                f1 = -1;
                        }
                        else
                        {
                            if (!bBottom.Value)
                                f1 = -1;

                        }

                        break;
                    }
                case 3:
                    {
                        dvcl = aDividerCLs[1];
                        f1 = -1;

                        break;
                    }
            }



            if (dvcl.sp.X > dvcl.ep.X) dvcl = dvcl.Inverse();
            dvcl.MoveOrtho(f1 * delta);

            if (aLimLine.Length <= mdGlobals.TwoEndSupportBoltLimitLineLength && !bStraddle)
            {
                _rVal.Add(dvcl.IntersectionPt(new ULINE(X, 1000, X, -1000)));
            }
            else
            {

                if (!bStraddle)
                {
                    double x1 = X_Inside_Left + Thickness + mdGlobals.DCRingClipHoleInset;
                    double x2 = X_Inside_Right - Thickness - mdGlobals.DCRingClipHoleInset;
                    UVECTOR u1 = dvcl.IntersectionPt(new ULINE(x1, 1000, x1, -1000));
                    UVECTOR u2 = dvcl.IntersectionPt(new ULINE(x2, 1000, x2, -1000)); _rVal.Add(u1);
                    _rVal.Add(u2);

                }
                else
                {
                    // use a reduced hole inset from the sides
                    double x1 = X_Inside_Left + Thickness + 1;
                    double x2 = X_Inside_Right - Thickness - 1;
                    UVECTOR u1 = dvcl.IntersectionPt(new ULINE(x1, 1000, x1, -1000));
                    UVECTOR u2 = dvcl.IntersectionPt(new ULINE(x2, 1000, x2, -1000));

                    double y1 = Math.Sqrt(Math.Pow(BoundingRadius, 2) - Math.Pow(x1, 2));
                    double y2 = Math.Sqrt(Math.Pow(BoundingRadius, 2) - Math.Pow(x2, 2));
                    if (X > 0)
                    {
                        if (u1.Length() < BoundingRadius) _rVal.Add(u1);
                        if (u2.Length() < BoundingRadius)
                            _rVal.Add(u2);
                        else
                            _rVal.Add(new UVECTOR(x2, bBottom.Value ? -y2 : y2));

                    }
                    else
                    {

                        if (u1.Length() < BoundingRadius)
                            _rVal.Add(u1);
                        else
                            _rVal.Add(new UVECTOR(x1, bBottom.Value ? -y1 : y1));

                        if (u2.Length() < BoundingRadius) _rVal.Add(u2);
                    }
                }
            }

            return _rVal;
        }

        internal void EnforceRingClipClearance(ref ULINE aLimLine, bool bBottom)
        {


            //enforce minumum clip clearance
            //by looping on the hole centers (1 or 2 at most)
            double dcang = Math.Atan(aLimLine.DeltaY / InsideWidth);
            for (int i = 1; i <= aLimLine.Points.Count; i++)
            {
                UVECTOR u1 = aLimLine.Points.Item(i);
                ULINE vline = new ULINE(u1.X, u1.Y, u1.X, u1.Y - 2000);
                UVECTOR ip = vline.IntersectionPt(aLimLine);
                double h = (ClipClearance + Thickness) / Math.Cos(dcang);

                double d1 = Math.Abs(u1.Y - ip.Y);
                //if the line is to close to the hole move it
                //vertically towards or away from the shell center (no change to slope)
                if (d1 < h)
                {
                    double d2 = h - d1;
                    aLimLine.Move(0, !bBottom ? -d2 : d2, bDontMovePoints: true);
                    //ip = vline.IntersectionPt(aLimLine);
                    //d1 = Math.Abs(u1.Y - ip.Y);
                }
                else
                {
                    return;
                }

            }


            //trim to the vertical bounds
            //if (aLimLine.sp.X != X_Inside_Left)
            //{
            //    if (!aLimLine.IsHorizontal(3))
            //        aLimLine.sp = aLimLine.IntersectionPt(new ULINE(X_Inside_Left, 0, X_Inside_Left, 100));
            //    else
            //        aLimLine.sp.X = X_Inside_Left;
            //}


            //if (aLimLine.ep.X != X_Inside_Right)
            //{
            //    if (!aLimLine.IsHorizontal(3))
            //        aLimLine.ep = aLimLine.IntersectionPt(new ULINE(X_Inside_Right, 0, X_Inside_Right, 100));
            //    else
            //        aLimLine.ep.X = X_Inside_Right;
            //}

        }

        /// <summary>
        /// round insets up to the nearest 1/16th or millimeter (if rounding is on) and make sure they are equal on both sides
        /// </summary>
        /// <param name="aLimLine"></param>
        /// <param name="aLeftBoxLine"></param>
        /// <param name="aRightBoxLine"></param>
        /// <param name="bBottom"></param>
        internal void RoundInsets(ref ULINE aLimLine, ULINE aLeftBoxLine, ULINE aRightBoxLine, bool bBottom)
        {


            double y1 = !bBottom ? aLeftBoxLine.MaxY : aLeftBoxLine.MinY;
            UVECTOR ip1 = aLeftBoxLine.Moved(Thickness).IntersectionPt(aLimLine);
            UVECTOR sp = aLimLine.EndPoints.Nearest(ip1);
            double d1 = y1 - ip1.Y;
            double isetleft = Math.Abs(uopUtils.RoundTo(d1, RoundingMethod.Units(), bRoundUp: true));

            double y2 = !bBottom ? aRightBoxLine.MaxY : aRightBoxLine.MinY;
            UVECTOR ip2 = aRightBoxLine.Moved(-Thickness).IntersectionPt(aLimLine);
            UVECTOR ep = aLimLine.EndPoints.Nearest(ip2);
            d1 = y2 - ip2.Y;
            double isetright = Math.Abs(uopUtils.RoundTo(d1, RoundingMethod.Units(), bRoundUp: true));

            double f1 = !bBottom ? -1 : 1;
            double iset = Math.Max(isetleft, isetright);
            sp.Y = y1 + iset * f1;
            ep.Y = y2 + iset * f1;
            aLimLine.ep.SetOrdinates(ep.X, ep.Y);
            aLimLine.sp.SetOrdinates(sp.X, sp.Y);

        }
        /// <summary>
        /// enforce minumum endplate engagement (3/4'') ??
        ///calculate the contact point of the end plate to the box accounting for
        ///the bend radius if less than 3/4'' move the line down vertical by the difference
        /// </summary>
        /// <param name="aLimitLines"></param>
        /// <returns></returns>
        internal double SmallestEndplateContactDistance(ULINEPAIR aLimitLines)
        {
            double _rVal = 0;


            List<ULINEPAIR> limlines = LimLines;
            List<ULINEPAIR> bxlines = BoxLns;

            if (limlines.Count < 1 || bxlines.Count < 1) return 0;

            int bxindex = BoxLns.FindIndex((x) => x.Row == aLimitLines.Row);
            if (bxindex < 0) return 0;

            double thk = Thickness;
            ULINEPAIR bxlns = bxlines[bxindex];


            ULINE? leftline = bxlns.GetSide(uppSides.Left);
            ULINE? rightline = bxlns.GetSide(uppSides.Right);
            if (!leftline.HasValue || !leftline.HasValue) return -1;
            ULINE? topline = aLimitLines.GetSide(uppSides.Top);
            ULINE? botline = aLimitLines.GetSide(uppSides.Bottom);
            List<double> physicalinsets = new List<double>();

            if (topline.HasValue)
            {
                ULINE limline = topline.Value;
                UVECTOR lp = limline.EndPts.Find((x) => x.X < X);
                UVECTOR rp = limline.EndPts.Find((x) => x.X > X);
                double delta = leftline.Value.MaxY - lp.Y;
                if (limline.IsHorizontal(3))
                {
                    physicalinsets.Add(delta - 2 * thk);
                }
                else
                {

                    //right side first

                    //fillet the two lines together with a radius of 2 * the material thickness
                    dxeArc aR = dxfPrimatives.CreateFilletArc(2 * thk, new uopVector(lp), new uopVector(rp), new uopVector(rp.X, rp.Y + 2000));

                    //get the y of the contact of the arc with the box wall
                    double y1 = aR.EndPoints.GetOrdinate(dxxOrdinateTypes.MaxY);



                    //see if the distance from the box end to the violates the rule
                    physicalinsets.Add(Math.Round(rightline.Value.MaxY - y1, 6));


                    //left side same way
                    dxfPrimatives.CreateFilletArc(2 * thk, new uopVector(rp.X, rp.Y + 2000), new uopVector(rp), new uopVector(lp.X, lp.Y));



                    y1 = aR.EndPoints.GetOrdinate(dxxOrdinateTypes.MaxY);
                    physicalinsets.Add(Math.Round(leftline.Value.MaxY - y1, 6));

                }


            }


            if (botline.HasValue)
            {
                ULINE limline = botline.Value;
                UVECTOR lp = limline.EndPts.Find((x) => x.X < X);
                UVECTOR rp = limline.EndPts.Find((x) => x.X > X);
                double delta = lp.Y - leftline.Value.MinY;
                if (limline.IsHorizontal(3))
                {
                    physicalinsets.Add(delta - 2 * thk);
                }
                else
                {

                    //right side first

                    //fillet the two lines together with a radius of 2 * the material thickness
                    dxeArc aR = dxfPrimatives.CreateFilletArc(2 * thk, new uopVector(lp), new uopVector(rp), new uopVector(rp.X, rp.Y + 2000));

                    //get the y of the contact of the arc with the box wall
                    double y1 = aR.EndPoints.GetOrdinate(dxxOrdinateTypes.MaxY);



                    //see if the distance from the box end to the violates the rule
                    physicalinsets.Add(Math.Round(rightline.Value.MaxY - y1, 6));


                    //left side same way
                    dxfPrimatives.CreateFilletArc(2 * thk, new uopVector(rp.X, rp.Y + 2000), new uopVector(rp), new uopVector(lp.X, lp.Y));



                    y1 = aR.EndPoints.GetOrdinate(dxxOrdinateTypes.MaxY);
                    physicalinsets.Add(Math.Round(leftline.Value.MaxY - y1, 6));

                }


            }

            _rVal = -100000000;
            foreach (var item in physicalinsets)
            {
                if (Math.Abs(item) > _rVal) _rVal = Math.Abs(item);
            }
            return _rVal;
        }

        internal List<ULINE> DividerCenterLines()
        {
            List<ULINE> _rVal = new List<ULINE>();
            if (DesignFamily.IsStandardDesignFamily()) return _rVal;
            dxfPlane myplane = Divider.Plane;
            _rVal.Add(new ULINE(myplane, Divider.Length, aTrimArc: new UARC(BoundingRadius)));
            if (Divider.Offset != 0 && DesignFamily.IsBeamDesignFamily())
            {
                myplane.Project(myplane.YDirection, -2 * Divider.Offset);
                _rVal.Add(new ULINE(myplane, Divider.Length, aTrimArc: new UARC(BoundingRadius)));
            }

            return _rVal;
        }
        public void Copy(DowncomerInfo aInfo)
        {
            if (aInfo == null) return;
            TrayAssembly = aInfo.TrayAssembly;
            X = aInfo.X;
            Y = aInfo.Y;
            RingRadius = aInfo.RingRadius;
            InsideWidth = aInfo.InsideWidth;
            Thickness = aInfo.Thickness;
            Clearance = aInfo.Clearance;
            RingClipRadius = aInfo.RingClipRadius;
            DeckRadius = aInfo.DeckRadius;
            ClipClearance = aInfo.ClipClearance;
            HasTriangularEndPlate = aInfo.HasTriangularEndPlate;
            ShelfWidth = aInfo.ShelfWidth;
            Spacing = aInfo.Spacing;
            IsVirtual = aInfo.IsVirtual;
            Index = aInfo.Index;
            EndplateInset = aInfo.EndplateInset;
            WeirFraction = aInfo.WeirFraction;
            PanelClearance = aInfo.PanelClearance;
            ColumnRadius = aInfo.ColumnRadius;
            Index = aInfo.Index;
            DCIndex = aInfo.DCIndex;
            EndPlateOverhang = aInfo.EndPlateOverhang;
            _RingClipSize = aInfo._RingClipSize;
            _RingClipHoleDiameter = aInfo._RingClipHoleDiameter;

            Divider = new DividerInfo(aInfo.Divider);
            LimLines = uopLinePairs.Copy(aInfo._LimLines);
            BoxLns = uopLinePairs.Copy(aInfo._BoxLns);
            WeirLns = uopLinePairs.Copy(aInfo._WeirLns);
            DowncomerCount = aInfo.DowncomerCount;
            SpoutDiameter = aInfo.SpoutDiameter;
            MetricSpouting = aInfo.MetricSpouting;
            WeirHeight = aInfo.WeirHeight;
        }
        #endregion Methods

        #region Shared Methods

        public static DowncomerInfo CloneCopy(DowncomerInfo aInfo) => aInfo == null ? null : new DowncomerInfo(aInfo);

        #endregion Shared Methods

    }

    public class DowncomerDataSet : List<DowncomerInfo>, IEnumerable<DowncomerInfo>, ICloneable
    {

        #region Constructors
        public DowncomerDataSet(DowncomerDataSet aDataSet)
        {
            ProjectType = uppProjectTypes.MDSpout;
            DesignFamily = uppMDDesigns.Undefined;
          //  _PanelShapes = null;
            if (aDataSet == null) return;

            Spacing = aDataSet.Spacing;
            Offset = aDataSet.Offset;
            ShelfWidth = aDataSet.ShelfWidth;
            InsideWidth = aDataSet.InsideWidth;
        
            Thickness = aDataSet.Thickness;
            DeckRadius = aDataSet.DeckRadius;
            RingClipRadius = aDataSet.RingClipRadius;
            RingRadius = aDataSet.RingRadius;
            DesignFamily = aDataSet.DesignFamily;
            IsSymmetric = aDataSet.IsSymmetric;
            Spacing = aDataSet.Spacing;
            Offset = aDataSet.Offset;
            Clearance = aDataSet.Clearance;
            Divider = new DividerInfo(aDataSet.Divider);
            RoundingMethod = aDataSet.RoundingMethod;
            EndPlateOverhang = aDataSet.EndPlateOverhang;
            ShellRadius = aDataSet.ShellRadius;
            _RingClipHoleDiameter = aDataSet._RingClipHoleDiameter;
            _RingClipSize = aDataSet._RingClipSize;
            DowncomerCount = aDataSet.DowncomerCount;
            ProjectType = aDataSet.ProjectType;
            HasFoldedWeirs = aDataSet.HasFoldedWeirs;
            TrayAssembly = aDataSet.TrayAssembly;

            foreach (var item in aDataSet)
            {
                Add(new DowncomerInfo(item));

            }


        }
        public DowncomerDataSet(colMDDowncomers aDowncomers, mdTrayAssembly aAssy = null)
        {
            ProjectType = uppProjectTypes.MDSpout;
            DesignFamily = uppMDDesigns.Undefined;
            //_PanelShapes = null;
            HasBubblePromoters = false;
            HasFoldedWeirs = false;
            if (aDowncomers == null) return;
            aAssy ??= TrayAssembly;
            if (aAssy == null) return;
            TrayAssembly = aAssy;
            Spacing = aAssy.DowncomerSpacing;
            Offset = aAssy.DowncomerOffset;
            mdDowncomer aBasis = aAssy.Downcomer();
            UpdateMembers(aAssy, aBasis);
        }

        public DowncomerDataSet(mdTrayAssembly aAssy, double? aSpace, double? aOffset, mdDowncomer aBasis = null, uppMDRoundToLimits? aRoundMethod = null)
        {

            ProjectType = uppProjectTypes.MDSpout;
            DesignFamily = uppMDDesigns.Undefined;
            //_PanelShapes = null;
            HasBubblePromoters = false;
            HasFoldedWeirs = false;

            TrayAssembly = aAssy;

            if (aAssy == null) return;
            RoundingMethod = aRoundMethod.HasValue ? aRoundMethod.Value : aAssy.DowncomerRoundToLimit;
            if (!aSpace.HasValue)
                aSpace = aAssy.DowncomerSpacing;
            if (!aOffset.HasValue)
                aOffset = aAssy.DowncomerOffset;


            if (aBasis != null)
            {
                if (aBasis.Width == aAssy.Downcomer().Width && aBasis.Count == aAssy.Downcomer().Count)
                    aBasis = aAssy.Downcomer();
            }
            Spacing = aSpace.Value;
            Offset = aOffset.Value;

            UpdateMembers(aAssy, aBasis);

        }
        public void ResetDefinitonLines() { foreach (var mem in this) mem.ResetDefinitonLines(); }
        internal void UpdateMembers(mdTrayAssembly aAssy = null, mdDowncomer aBasis = null)
        {

            Clear();
            aAssy ??= TrayAssembly;
            if (aAssy == null) return;
            aBasis ??= aAssy.Downcomer();

            if (Spacing < 0 || (Spacing == 0 && aBasis.Count > 1) || aBasis.Count < 1) return;


            List<double> xvals = mdUtils.DowncomerXValues(aAssy, Spacing, Offset, aBasis);

            HasFoldedWeirs = false;
            int i = 0;
            colMDDowncomers dcs = aAssy._Downcomers;
            _TotalWeirLength = 0;
            DowncomerInfo info;
            foreach (mdDowncomer dc in dcs)
            {
                if (dc.IsVirtual)
                    dc.CopyParentProperties();
                i++;

                if (i > xvals.Count) 
                    break;
                
                if(!HasFoldedWeirs && dc.FoldOverWeirs) HasFoldedWeirs = true;

                info = new DowncomerInfo(dc, aAssy, aXVal: xvals[i - 1], aRoundMethod: RoundingMethod, Divider);
                if(info.X_Outside_Right <= RingRadius - mdGlobals.DCRingProximityClearance && info.X_Outside_Left >= -RingRadius + mdGlobals.DCRingProximityClearance)
                {
                    Add(info);
                    info.CreatedDefinitionLines();
                    _TotalWeirLength += info.TotalWeirLength;

                }
                
            }

            if (_TotalWeirLength > 0)
            {
                foreach (var item in this)
                    item.WeirFraction = item.TotalWeirLength / _TotalWeirLength;
          
            }

        }



        #endregion Constructors

        #region Properties

        public double DeckSectionWidth
        {
            get
            {

                if (Count <= 0) return DeckRadius * 2;
                if (Count == 1) return DeckRadius - Item(1).X_Outside_Right - DeckSectionClearance;
                return Spacing - BoxWidth - 2 * DeckSectionClearance;

            }
        }

        /// <summary>
        /// the unsupported width of deck sections created with this downcomer information
        /// </summary>
        public double DeckSectionSpan
        {
            get
            {

                if (Count <= 0) return RingRadius;
                if (Count == 1) return RingRadius - Item(1).X_Outside_Right - ShelfWidth;
                return Spacing - BoxWidth - 2 * ShelfWidth;

            }
        }


        private WeakReference<mdTrayAssembly> _AssyRef;
        public mdTrayAssembly TrayAssembly
        {
            get
            {
                if (_AssyRef == null) return null;
                if (!_AssyRef.TryGetTarget(out mdTrayAssembly _rVal)) _AssyRef = null;
                if (_rVal != null) ProjectType = _rVal.ProjectType;
                return _rVal;
            }
            set
            {
                Divider = new DividerInfo();
                _AssyRef = value != null ? new WeakReference<mdTrayAssembly>(value) : null;
                if (value == null)
                {
                    DeckRadius = 0;
                    RingRadius = 0;
                    ShellRadius = 0;
                    Thickness = 0;
                    ShelfWidth = mdGlobals.DefaultShelfAngleWidth;
                    Spacing = 0;
                    Offset = 0;
                    RingClipRadius = 0;
                    Clearance = 0;
                    HasBubblePromoters = false;

                    DesignFamily = uppMDDesigns.Undefined;
                    Divider = new DividerInfo();
                    HasFoldedWeirs = false;
                    return;
                }

                DeckRadius = value.DeckRadius;
                RingRadius = value.RingID / 2;
                Thickness = value.Downcomer().Thickness;
                ShelfWidth = value.Downcomer().ShelfWidth;
                InsideWidth = value.Downcomer().Width;
                Spacing = value.DowncomerSpacing;
                RingClipRadius = value.RingClipRadius;
                Clearance = value.RingClearance;
                HasBubblePromoters = value.DesignOptions.HasBubblePromoters;
                HasFoldedWeirs = value.Downcomers.HasFoldovers;
                RingClipSize = value.RingClipSize;
                RingClipHoleDiameter = value.Downcomer().RingClipHoleU.Diameter;
                DesignFamily = value.DesignFamily;
                IsSymmetric = value.IsSymmetric;
                ShellRadius = value.ShellID / 2;
                Spacing = value.DowncomerSpacing;
                Divider = value.Divider;
                DowncomerCount = value.Downcomer().Count;
                ProjectType = value.ProjectType;

            }
        }

        public bool IsSymmetric { get; private set; }

        public bool IsStandardDesign => DesignFamily.IsStandardDesignFamily();


        internal double? _RingClipHoleDiameter;

        public double RingClipHoleDiameter { get => _RingClipHoleDiameter.HasValue ? _RingClipHoleDiameter.Value : RingClipSize == uppRingClipSizes.ThreeInchRC ? mdGlobals.gsBigHole : 13 / 25.4; set => _RingClipHoleDiameter = value; }

        internal uppRingClipSizes? _RingClipSize;

        public uppRingClipSizes RingClipSize { get { return !_RingClipSize.HasValue ? (ShellID <= uopGlobals.RingClipSizeChangeLimit) ? uppRingClipSizes.ThreeInchRC : uppRingClipSizes.FourInchRC : _RingClipSize.Value; } set => _RingClipSize = value; }

        public virtual double DefaultRingClipClearance { get { return (RingClipSize == uppRingClipSizes.FourInchRC) ? 1.375 : 1.125; } }

        public double ShellID => 2 * ShellRadius;
        public double ShellRadius { get; set; }
        public double RingRadius { get; set; }
        public double RingClipRadius { get; set; }

        public double BeamOffsetFactor { get => DesignFamily.IsBeamDesignFamily() ? Divider.BeamOffsetFactor : 0; }

        public double PanelClearance => ProjectType == uppProjectTypes.MDSpout ? 0 : mdGlobals.DefaultPanelClearance;
        public double Clearance { get; set; }
        public double DeckRadius { get; set; }
        public double DeckLap => DeckRadius - RingRadius;
        public double Thickness { get; set; }
        public double ShelfWidth { get; set; } = 1;
        public double InsideWidth { get; set; }

        public double Spacing { get; set; }

        public double DeckSectionClearance => ProjectType == uppProjectTypes.MDSpout ? PanelClearance : HasFoldedWeirs ? PanelClearance + mdGlobals.FolderWeirPanelClearanceAdder : PanelClearance;
        public double Offset { get; set; }
        public List<double> XValues_Downcomers
        {
            get
            {
                List<double> _rVal = new List<double>();
                foreach (DowncomerInfo item in this)
                {
                    _rVal.Add(item.X);
                }
                return _rVal;
            }
        }

        public bool HasFoldedWeirs { get; set; }
        private double _TotalWeirLength;
        public double TotalWeirLength
        {
            get
            {
                if (_TotalWeirLength <= 0)
                {

                    foreach (var item in this)
                    {
                        _TotalWeirLength += item.TotalWeirLength;
                    }
                }
                return _TotalWeirLength;
            }
        }

        public double EndPlateOverhang { get; set; } = 0.25;

        public uppMDRoundToLimits RoundingMethod { get; set; }

        public double BoxWidth => InsideWidth + 2 * Thickness;
        public uppMDDesigns DesignFamily { get; private set; }

        public bool HasBubblePromoters { get; private set; }
        public uppProjectTypes ProjectType { get;  set; }
        public int DowncomerCount { get; set; }
        public int PanelCount => DowncomerCount > 0 ? DowncomerCount + 1 : 0;

        public bool OddDowncomers => Count % 2 != 0;

        private DividerInfo _Divider;

        public DividerInfo Divider { get { _Divider ??= new DividerInfo(); _Divider.RingRadius = RingRadius; _Divider.BoundingRadius = RingClipRadius; _Divider.DowncomerSpacing = Spacing; return _Divider; } set { value ??= new DividerInfo(); _Divider = value; } }
        public bool MultiPanel { get => _Divider == null? false : _Divider.DividerType == uppTrayDividerTypes.Beam && _Divider.Offset != 0; }

        public List<uopLinePair> LimitLines => uopLinePair.FromList(LimLines);
        internal List<ULINEPAIR> LimLines
        {
            get
            {
                List<ULINEPAIR> _rVal = new List<ULINEPAIR>();
                foreach (var item in this)
                {
                    _rVal.AddRange(uopLinePairs.Copy(item.LimLines));
                }
                return _rVal;
            }
        }


        public List<uopLinePair> BoxLines => uopLinePair.FromList(BoxLns);
        internal List<ULINEPAIR> BoxLns
        {
            get
            {
                List<ULINEPAIR> _rVal = new List<ULINEPAIR>();
                foreach (var item in this)
                {
                    _rVal.AddRange(uopLinePairs.Copy(item.BoxLns));
                }
                return _rVal;
            }
        }

        public List<uopLinePair> InternalBoxLines => uopLinePair.FromList(InternalBoxLns);
        internal List<ULINEPAIR> InternalBoxLns
        {
            get
            {
                double thk = Thickness;
                List<ULINEPAIR> _rVal = new List<ULINEPAIR>();
                foreach (var item in this)
                {
                    List<ULINEPAIR> outers = item.BoxLns;
                    foreach (var outer in outers)
                    {
                        ULINEPAIR inner = outer.Clone();
                        inner.Line1 = inner.Line1.Value.Moved(thk);
                        inner.Line2 = inner.Line2.Value.Moved(-thk);
                        _rVal.Add(inner);

                    }
                }
                return _rVal;
            }
        }

        public List<uopLinePair> WeirLines => uopLinePair.FromList(WeirLns);

        public List<uopLinePair> ShelfLines => uopLinePair.FromList(ShelfLns);

        internal List<ULINEPAIR> WeirLns
        {
            get
            {
                List<ULINEPAIR> _rVal = new List<ULINEPAIR>();
                foreach (var item in this)
                {
                    _rVal.AddRange(uopLinePairs.Copy(item.WeirLns));
                }
                return _rVal;
            }
        }

       
        internal List<ULINEPAIR> ShelfLns
        {
            get
            {
                List<ULINEPAIR> _rVal = new List<ULINEPAIR>();
                foreach (var item in this)
                {
                    _rVal.AddRange(uopLinePairs.Copy(item.CreateShelfLines()));
                }
                return _rVal;
            }
        }


        public int MaxRow => DesignFamily.IsStandardDesignFamily() || DesignFamily.IsDividedWallDesignFamily() ? 1 : !MultiPanel ? 2 : 3;
        

        #endregion Properties

        #region Methods

        public uopLines GetLines(uppDefinitionLineTypes aType, bool bGetVirtuals = false)
        {
            uopLines _rVal = new uopLines(aType.Description());
            List<ULINEPAIR> source = null;

            switch (aType)
            {
                case uppDefinitionLineTypes.LimitLine:
                    {
                        source = LimLines;
                        break;
                    }
                case uppDefinitionLineTypes.BoxWeirLine:
                        {
                            source = WeirLns;
                            break;
                        }
                case uppDefinitionLineTypes.BoxLine:
                        {
                            source = BoxLns;
                            break;
                        }

                case uppDefinitionLineTypes.PanelWeirLine:
                    {
                        source = CreateFBAWeirLines();
                        break;
                    }
            }

            if (source == null) return _rVal;
            foreach(var pair in source)
            {
                if (!bGetVirtuals && pair.IsVirtual) continue;
                if (pair.Line1.HasValue) _rVal.Add(new uopLine(pair.Line1.Value) { Tag = $"{_rVal.Name} {1}" });
                if (pair.Line2.HasValue) _rVal.Add(new uopLine(pair.Line2.Value) { Tag = $"{_rVal.Name} {2}" });
            }

            return _rVal;
        }


        /// <summary>
        /// returns bubble promoters for all the panels in the tray
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public uopVectors BPSites(mdTrayAssembly aAssy = null, List<uopDeckSplice> aSplices = null)
        {
            uopVectors _rVal = uopVectors.Zero;
            if (Count <= 0) return _rVal;

            aAssy ??= TrayAssembly;
            if (aAssy == null) return _rVal;



            _rVal.Suppressed = !aAssy.DesignOptions.HasBubblePromoters;
            List<mdSpoutGroup> sGroups = aAssy.SpoutGroups.GetByVirtual(aVirtualValue: false, bNonZeroOnly: true);

            double rad = mdGlobals.BPRadius;
            bool symmetric = DesignFamily.IsStandardDesignFamily();
            bool specialcase = symmetric && OddDowncomers && Count > 1;
            int lastpd = 0;
            if (specialcase)
            {
                lastpd = DowncomerCount - 1;

                }

            int pcnt = DowncomerCount + 1;
            double minrad = RingRadius - 3;
            
            foreach (mdSpoutGroup sGroup in sGroups)
            {

                int dcidx = sGroup.DowncomerIndex;
                int dpidx = sGroup.PanelIndex;
                DowncomerInfo dcinfo = Find(x => x.DCIndex == dcidx);

                if (dcinfo == null) continue;

                double x1 = sGroup.PatternY;
                double y1 = dcinfo.X;

                if (Math.Sqrt(Math.Pow(x1, 2) + Math.Pow(y1, 2)) > minrad) continue;
                _rVal.Add(x1, y1, aRow: dpidx, aCol: dcidx, rad);

                if (!symmetric) continue;

                if (Math.Round(Math.Abs(y1), 2) > 0.01)
                {
                    _rVal.Add(x1, -y1, dpidx, dcidx, rad);

                }
                if (specialcase && dpidx == lastpd)
                {
                    _rVal.Add(-x1, y1, dpidx+1, dcidx, rad);
                    if (Math.Round(Math.Abs(y1), 2) > 0.01)
                    {
                        _rVal.Add(-x1, -y1, dpidx + 1, dcidx, rad);

                    }
                }

            }
            if (aSplices != null)
            {
                SuppressBPSites(_rVal, aSplices);
            }

            _rVal.Invalid = false;
            return _rVal;
        }

        public void SuppressBPSites(uopVectors aSites, List<uopDeckSplice> aSplices)
        {
            uopPanelSectionShapes panels = PanelShapes();
            if (aSites == null || aSites.Count <= 0 || aSplices == null || aSplices.Count <= 0 || panels.Count <=0) return;

           
            //suppress the sites to close to the downcomers or to close to the ring
            double minrad =RingRadius - 3;
            double rad = mdGlobals.BPRadius;
           

            aSites.SetSuppressed(aIndex:null, aSuppressionVal:false, aMark: false, aRadius: rad);
            uopVectors.SetSuppressedValue(aSites.FindAll(x => x.Length(4) > minrad), aIndex: null, aSuppressionVal: true, aMark: true);
            uopRectangles splicelims = uopDeckSplices.GetSpliceLimits(aSplices, 0.9 * rad);

            int maxpanel = panels.MaxPanelIndex;
            for(int p = 1; p <= maxpanel; p ++)
            {
                List<uopPanelSectionShape> panelshapes = panels.FindAll(x => x != null && x.PanelIndex == p);
                List<uopVector> psites = aSites.FindAll(x => x != null && !x.Suppressed && x.Mark == false && x.Row == p);
           
                List<uopRectangle> panelsplices = splicelims.FindAll(x => x.Row == p);
                foreach (var pshape in panelshapes)
                {
                    List<uopVector> subsites = psites.FindAll(x => pshape.Bounds.Contains(x, bOnIsOut: true));
                    foreach (uopVector ps in subsites)
                    {
                        if (ps.X < pshape.Left + (1 + rad))
                            ps.Suppressed = true;


                        if (ps.X > pshape.Right - (1 + rad))
                            ps.Suppressed = true;

                        if (!ps.Suppressed)
                        {

                            foreach (var psplicelims in panelsplices)
                            {
                                if (psplicelims.Contains(ps, bOnIsOut: false))
                                {
                                    ps.Suppressed = true;
                                    break;
                                }

                            }
                        }

                        ps.Mark = true;


                    }
                }
            }


            //unsuppress all the ones that werent touched (mark = false)
            uopVectors.SetSuppressedValue(aSites.FindAll(x => x.Mark == false), aIndex: null, aSuppressionVal: true, aMark: true);

        }

        /// <summary>
        /// the centers of the free rectangles
        /// </summary>
        public uopVectors FreeCenters() { FreeRectangles(out  uopVectors _rVal ); return _rVal; }

        /// <summary>
        /// the rectangles formed as the intersection of a deck panels polygon and the polygons of the panels below
        /// used as the potential sites for manways
        /// </summary>
        public uopRectangles FreeRectangles( out uopVectors rFreeCenters)
        {
            rFreeCenters = uopVectors.Zero;
            uopRectangles _rVal = new uopRectangles();
         
            List<DowncomerInfo> DComers = FindAll(x => !x.IsVirtual);
            if (DComers.Count <= 0) return _rVal;

            double wd = 0;
            uopLines vLines = new uopLines();
            uopLines hLines = new uopLines();
            double x1;
            double x2;
            double x3 = 0;
            double y1;
            double y2;
            double y3 = 0;
            uopLine cLine;
            uopLine dLine;
            double rad = RingRadius;


            int pidx1;
            int pidx2;
            double ht = 0;
            int cnt;
            int cnt2;
               bool symmetric = DesignFamily.IsStandardDesignFamily();
            bool nooneg = symmetric;
            int panelcount = PanelCount;
       
            if (!OddDowncomers || !symmetric)
            {
                cnt2 = 2 * DComers.Count + 1;
                cnt = DComers.Count + 1;
                DowncomerInfo aDC = DComers[DComers.Count - 1];
                wd = 0.5 * aDC.BoxWidth;
                x1 = -aDC.X - wd;
                x2 = -aDC.X + wd;
                y1 = Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(x1, 2));
                y2 = Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(x2, 2));
              
                vLines.Add(new uopLine(new uopVector(x1, y1), new uopVector(x1, x1)));
                vLines.Add(new uopLine(new uopVector(x2, y2), new uopVector(x2, x1)));

                hLines.Add(new uopLine(new uopVector(x1, x1), new uopVector(y1, x1)));
                hLines.Add(new uopLine(new uopVector(x1, x2), new uopVector(y2, x2)));

                x3 = x1;
                y3 = x3;
            }
            else
            {
                cnt2 = 2 * DComers.Count;
                cnt = DComers.Count;
            }
            for (int i = DComers.Count; i >= 1; i--)
            {
                DowncomerInfo aDC = DComers[i - 1];
                wd = 0.5 * aDC.BoxWidth;
                x1 = aDC.X - wd;
                x2 = aDC.X + wd;
                y1 = Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(x1, 2));
                y2 = Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(x2, 2));
                vLines.Add(new uopLine(new uopVector(x1, y1), new uopVector(x1, y3)));
                vLines.Add(new uopLine(new uopVector(x2, y2), new uopVector(x2, y3)));

                hLines.Add(new uopLine(new uopVector(x3, x1), new uopVector(y1, x1)));
                hLines.Add(new uopLine(new uopVector(x3, x2), new uopVector(y2, x2)));

            }
            pidx2 = cnt;
            //loop start index specific logic
            for (int i = 2; i <= hLines.Count; i += 2)
            {
                uopLine aLine = hLines[i - 1];
                uopLine bLine = i + 1 <= hLines.Count ? hLines.Item(i + 1) : null;
                pidx1 = cnt;
                for (int j = 2; j <= vLines.Count; j += 2)
                {
                    cLine = vLines.Item(j);

                    dLine = j + 1 <= vLines.Count ? vLines.Item(j + 1) : null;
                    uopVector v1 = aLine.IntersectionPt(cLine, false, false);
                    if (v1 != null && bLine != null)
                    {
                        uopVector v2 = bLine.IntersectionPt(dLine, false, false);
                        if (v2 != null)
                        {
                            wd = v2.X - v1.X;
                            ht = v2.Y - v1.Y;
                            uopVector v3 = new uopVector(v1.X + wd / 2.0, v1.Y + ht / 2.0) { Tag = pidx1.ToString(), Flag = pidx2.ToString() };

                            _rVal.Add(v3, wd, ht, pidx1.ToString(), pidx2.ToString());
                        }
                    }

                    pidx1 -= 1;
                }
                pidx2 -= 1;
            }
            for (int i = 1; i <= _rVal.Count; i++)
            {

                uopRectangle aRect = _rVal.Item(i);
                uopVector v3 = aRect.Center;
                rFreeCenters.Add(v3, bAddClone: false);

                if (Math.Round(v3.Y, 2) > 0)
                {
                    uopRectangle bRect = new uopRectangle(aRect);
                    bRect.Move(0, -2 * aRect.Y);
                    pidx1 = mzUtils.VarToInteger(bRect.Flag);
                    pidx2 = (int)(cnt + (Math.Truncate(Convert.ToDouble(panelcount / 2)) - pidx1) + 1);
                    bRect.Flag = Convert.ToString(pidx2);
                    rFreeCenters.Add(bRect.Center, bAddClone: false);

                    _rVal.Add(bRect);
                }
            }

            if (!nooneg)
            {
                for (int i = 1; i <= _rVal.Count; i++)
                {
                    uopRectangle aRect = _rVal.Item(i);
                    uopVector v3 = aRect.Center;
                    if (Math.Round(v3.X, 2) > 0)
                    {
                        uopRectangle bRect = new uopRectangle(aRect);
                        bRect.Move(-2 * aRect.X, -2 * aRect.Y);
                        _rVal.Add(bRect);
                        rFreeCenters.Add(bRect.Center, bAddClone: false);
                    }
                }
            }

            return _rVal;
        }

        /// <summary>
        /// the X or Y ordinates that are the center of the splice bolts when jogles or splice angles are used.
        /// </summary>

        public double DeckBoltSpacing(out int rBoltCount, double? aWidth = null)
        {
            double _rVal = 0;
            rBoltCount = 0;
            try
            {
               
                double span= aWidth == null ? DeckSectionSpan :aWidth.Value ;
                if (span <= 0) return 0;

                mdTrayAssembly aAssy = TrayAssembly;
                double target =  aAssy == null ? 6 : aAssy.DesignOptions.JoggleBoltSpacing;

                rBoltCount = uopUtils.LayoutPointsOnLine(new uopLine(uopVector.Zero, new uopVector(0, span)),  out  double space, aTargetSpace: aAssy == null ? 6 : aAssy.DesignOptions.JoggleBoltSpacing, bCenterOnLine: true, aEndBuffer: 0,  bAtLeastOne: true, aMinSpace: mdGlobals.gsSmallHole + 0.0625,false).Count;
                _rVal = space;

                
            }
            catch 
            {
                
            }
            return _rVal;
        }

        /// <summary>
        /// returns the tab spacing used to lay out tabs on the passed assembly
        /// </summary>
        /// <param name="bVertical"></param>
        /// <returns></returns>
        public double TabSpacing(bool bVertical) => TabSpacing(bVertical, out double _, out int _);


        /// <summary>
        /// returns the tab spacing used to lay out tabs on the assembly
        /// </summary>
        /// <param name="bVertical"></param>
        /// <param name="rWidth"></param>
        /// <param name="rCount"></param>
        /// <returns></returns>
        public  double TabSpacing(bool bVertical, out double rWidth, out int rCount)
        {
            double _rVal = 0;
            try
            {
                rWidth = 0;
                rCount = 0;
                double span = 0;
                if (!bVertical)
                {
                    rWidth = DeckSectionWidth;
                    span = rWidth - 2 * (mdGlobals.DeckTabFlangeInset + mdGlobals.DeckTabSlotInset);
                }
                else
                {
                    double trimrad = RingClipRadius;

                    if (Count > 0)
                    {
                        double aX = Item(1).X_Outside_Right + PanelClearance;
                        aX += (trimrad - aX) / 2 ;
                        aX += +mdGlobals.DeckTabFlangeHeight;
                        rWidth = Math.Sqrt(Math.Pow(trimrad, 2) - Math.Pow(aX, 2)) * 2;
                        span = rWidth - 2 * (mdGlobals.DeckTabFlangeInset + mdGlobals.DeckTabSlotInset);
                    }
                }

                
                rCount = 1;
                if (rWidth > 11.25 && rWidth <= 16.5)
                {
                    rCount = 2;
                }
                else if (rWidth > 16.5 && rWidth <= 27)
                {
                    rCount = 3;
                }
                else if (rWidth > 27 && rWidth <= 37.5)
                {
                    rCount = 4;
                }
                else if (rWidth > 37.5)
                {
                    rCount = (int)Math.Truncate(span / 8) + 1;
                    if (rCount < 5) rCount = 5;

                }


                if (rCount > 1) _rVal = span / (rCount - 1);

               
            }
            catch (Exception ex)
            {

                //todo
                throw ex;
                
            }

            return _rVal;
        }

     
        /// <summary>
        /// returns the shapes that are the boundaries of the deck panel sub shapes
        /// </summary>
        /// <returns></returns>
        public uopPanelSectionShapes PanelShapes(bool bIncludeClearance = true, int? aPanelIndex = null, bool bUpdatePanels = false, bool bRegen = false, bool bVerbose = false,bool bReturnVirtuals = false)
        {
       
            uopPanelSectionShapes _rVal = null;

           
            mdTrayAssembly assy = TrayAssembly;
            bool gendefaults = ProjectType == uppProjectTypes.MDSpout ? !bIncludeClearance : bIncludeClearance;



            if (!gendefaults)
                bUpdatePanels = false;
          

            //if (_rVal == null)
            //{
                if (gendefaults && assy != null)
                   if(bVerbose) assy.RaiseStatusChangeEvent($"Creating {assy.TrayName()} Panel Sub-Shapes");

            _rVal = CreatePanelShapes(bIncludeClearance ? DeckSectionClearance : 0, DeckLap, out _, out _, bReturnVirtuals: bReturnVirtuals, bSetBPSites: true);

            //}

            //if (bIncludeClearance && _PanelShapes == null)
            //{
            //    _PanelShapes = _rVal;
            //    bUpdatePanels = true;
                
            //}
            if (bUpdatePanels) UpdateAssemblyPanelShapes(_rVal);
            
            return !aPanelIndex.HasValue ? _rVal : _rVal.PanelShapes(aPanelIndex.Value);
        }

        private void UpdateAssemblyPanelShapes(uopPanelSectionShapes aPanelShapes)
        {
            mdTrayAssembly assy = TrayAssembly;
            if (assy == null || aPanelShapes == null) return;

            List<mdDeckPanel> panels = assy.DeckPanels.ToList();
            if (panels != null) return;

            for (int p = 1; p <= panels.Count; p++)
            {
                mdDeckPanel panel = panels[p - 1];
                panel.SetFBP(null, aPanelShapes.PanelShapes(p, bGetClones: true), bClear: false);
            }




        }

        /// <summary>
        ///returns the lines which are the outside edges of the boxes trimed to the ring clip circle and 0.5 inches away from any beams
        /// </summary>
        public List<uopLinePair> CreateTheorticalWeirLines(bool bIgnoreAngledEndPlates = false)
        {
            List<double> aXVals = XValues_Downcomers.ToList();
            if (aXVals == null || aXVals.Count < 1 || RingRadius <= 0) return new List<uopLinePair>();

            bool stddesign = DesignFamily.IsStandardDesignFamily();
            int rows = MaxRow;
            uopArc ring = new uopArc(RingRadius - Clearance);
            double thk = Thickness;

            double rad = Math.Round(ring.Radius, 4);
            uopSegments interceptors = new uopSegments() { ring };
            uopLine left = null;
            uopLine right = null;
            List<ULINEPAIR> dividers = CreateDividerLns(ring.Radius, Divider.RingClipOffset);
            List<uopLine> trimers = new List<uopLine>();
            foreach (var item in dividers)
            {
                trimers.Add(new uopLine(item.GetSide(uppSides.Top).Value));
                trimers.Add(new uopLine(item.GetSide(uppSides.Bottom).Value));
            }
            interceptors.AddRange(trimers);

            List<uopLinePair> _rVal = new List<uopLinePair>();

            aXVals.Sort(); aXVals.Reverse();
            double aXOffset = InsideWidth / 2;
            //march from right to left creating vertical lines and intersecting them with the divider lines and the ring
            foreach (var item in aXVals)
            {
                DowncomerInfo member = Find((x) => x.X == item);
                double x = Math.Round(item, 4);
                //  if (x < 0) continue;
                double xL = item - aXOffset - thk;
                double xR = item + aXOffset + thk;

                left = new uopLine(xL, 1000, xL, -1000);
                right = new uopLine(xR, 1000, xR, -1000);
                uopVectors ipsL = left.Intersections(interceptors, false, true);
                uopVectors ipsR = right.Intersections(interceptors, false, true);
                if (ipsL.Count <= 1 && ipsR.Count <= 1) continue;
                ipsR.Sort(dxxSortOrders.TopToBottom);
                ipsL.Sort(dxxSortOrders.TopToBottom);
                int l = ipsL.Count % 2 != 0 ? x >= 0 ? 1 : 0 : 0;
                int r = ipsR.Count % 2 != 0 ? x >= 0 ? 1 : 0 : 0;

                List<uopLinePair> set = new List<uopLinePair>();
                int col = Count;
                for (int i = 1; i <= Math.Max(ipsL.Count - 1, ipsR.Count - 1); i++)
                {
                    l++; r++;
                    if (l >= ipsL.Count || r >= ipsR.Count) break;

                    uopVector spL = ipsL[l - 1];
                    uopVector epL = ipsL[l];
                    uopVector spR = ipsR[r - 1];
                    uopVector epR = ipsR[r];
                    uppIntersectionTypes IntTypeT_L = uppIntersectionTypes.ToRing;
                    uppIntersectionTypes IntTypeT_R = uppIntersectionTypes.ToRing;
                    uppIntersectionTypes IntTypeB_L = uppIntersectionTypes.ToRing;
                    uppIntersectionTypes IntTypeB_R = uppIntersectionTypes.ToRing;
                    uppIntersectionTypes IntTypeT = uppIntersectionTypes.ToRing;
                    uppIntersectionTypes IntTypeB = uppIntersectionTypes.ToRing;
                    if (!stddesign)
                    {

                        IntTypeT_L = Math.Round(spL.Length(), 4) == rad ? uppIntersectionTypes.ToRing : uppIntersectionTypes.ToDivider;
                        IntTypeT_R = Math.Round(spR.Length(), 4) == rad ? uppIntersectionTypes.ToRing : uppIntersectionTypes.ToDivider;
                        IntTypeB_L = Math.Round(epL.Length(), 4) == rad ? uppIntersectionTypes.ToRing : uppIntersectionTypes.ToDivider;
                        IntTypeB_R = Math.Round(epR.Length(), 4) == rad ? uppIntersectionTypes.ToRing : uppIntersectionTypes.ToDivider;

                        if (IntTypeT_L == uppIntersectionTypes.ToDivider && IntTypeT_R == uppIntersectionTypes.ToDivider)
                            IntTypeT = uppIntersectionTypes.ToDivider;
                        else if (IntTypeT_L == uppIntersectionTypes.ToRing && IntTypeT_R == uppIntersectionTypes.ToDivider)
                            IntTypeT = uppIntersectionTypes.StraddlesRingToDivider;

                        if (IntTypeB_L == uppIntersectionTypes.ToDivider && IntTypeB_R == uppIntersectionTypes.ToDivider)
                            IntTypeB = uppIntersectionTypes.ToDivider;
                        else if (IntTypeB_L == uppIntersectionTypes.ToRing && IntTypeB_R == uppIntersectionTypes.ToDivider)
                            IntTypeB = uppIntersectionTypes.StraddlesRingToDivider;

                    }

                    if (IntTypeT_L == uppIntersectionTypes.ToRing && x > 0)
                        spL.Y = left.Moved(thk).Intersections(ring, true, true).GetExtremeOrd(bMin: false, bGetY: true);

                    if (IntTypeT_R == uppIntersectionTypes.ToRing && x < 0)
                        spR.Y = right.Moved(-thk).Intersections(ring, true, true).GetExtremeOrd(bMin: false, bGetY: true);

                    if (IntTypeB_L == uppIntersectionTypes.ToRing && x > 0)
                        epL.Y = left.Moved(thk).Intersections(ring, true, true).GetExtremeOrd(bMin: true, bGetY: true);

                    if (IntTypeB_R == uppIntersectionTypes.ToRing && x < 0)
                        epR.Y = right.Moved(-thk).Intersections(ring, true, true).GetExtremeOrd(bMin: true, bGetY: true);

                    if (IntTypeT_R == uppIntersectionTypes.ToDivider)
                        spR.Y = right.Moved(-thk).Intersections(trimers, false, true).Nearest(spR).Y;

                    if (IntTypeB_L == uppIntersectionTypes.ToDivider)
                        epL.Y = left.Moved(thk).Intersections(trimers, false, true).Nearest(epL).Y;


                    int row = rows;
                    if (!stddesign)
                    {
                        if (DesignFamily.IsBeamDesignFamily())
                        {
                            if (IntTypeT == uppIntersectionTypes.ToRing && IntTypeB == uppIntersectionTypes.ToRing)
                            {
                                row = x < rows - 1 ? 1 : rows;
                            }
                            else if (IntTypeT == uppIntersectionTypes.ToRing && IntTypeB != uppIntersectionTypes.ToRing)
                            {
                                row = 1;
                            }
                            else if (IntTypeT != uppIntersectionTypes.ToRing && IntTypeB != uppIntersectionTypes.ToRing)
                            {
                                row = rows - 1;
                            }
                        }
                    }

                    if (!member.HasTriangularEndPlate || bIgnoreAngledEndPlates)
                    {
                        if (IntTypeT == uppIntersectionTypes.ToRing)
                        {
                            if (x >= 0) { spL.Y = spR.Y; } else { spR.Y = spL.Y; }
                        }
                        if (IntTypeB == uppIntersectionTypes.ToRing)
                        {
                            if (x >= 0) { epL.Y = epR.Y; } else { epR.Y = epL.Y; }
                        }
                    }

                    left = new uopLine(spL, epL) { Side = uppSides.Left };
                    right = new uopLine(spR, epR) { Side = uppSides.Right };



                    uopLinePair pair = new uopLinePair(left, right) { IntersectionType1 = IntTypeT, IntersectionType2 = IntTypeB, Col = col, Row = row };
                    set.Add(pair);

                    l++; r++;
                }
                if (DesignFamily == uppMDDesigns.MDDividedWall)
                {
                    while (set.Count > 1) { set.RemoveAt(0); }
                }

                _rVal.AddRange(set);
                col--;

            }

            return _rVal;

        }

        public uopFreeBubblingPanels FreeBubblingPanels() =>CreateFreeBubblingPanels(TrayAssembly);
        

        public DowncomerDataSet Clone() => new DowncomerDataSet(this);

        public List<uopLinePair> GetLimitLines(int aDCIndex, bool bRegenInfo = false)
        {
            List<uopLinePair> _rVal = new List<uopLinePair>();

            foreach (DowncomerInfo item in this)
            {
                if (aDCIndex <= 0 || item.DCIndex == aDCIndex)
                {
                    if (bRegenInfo) item.CreatedDefinitionLines();
                    _rVal.AddRange(uopLinePair.FromList(item.LimLines));

                }
            }
            return _rVal;
        }

        public List<uopLinePair> GetEndSupportLines(int aDCIndex = 0, bool bRegenInfo = false)
        {
            List<uopLinePair> _rVal = new List<uopLinePair>();


            foreach (DowncomerInfo item in this)
            {
                if (bRegenInfo) item.CreatedDefinitionLines();
                if (aDCIndex <= 0 || item.DCIndex == aDCIndex)
                    _rVal.AddRange(uopLinePair.FromList(item.CreateEndSupportLines()));

            }
            return _rVal;
        }

        public List<uopLinePair> GetShelfLines(int aDCIndex = 0, bool bRegenInfo = false)
        {
            List<uopLinePair> _rVal = new List<uopLinePair>();


            foreach (DowncomerInfo item in this)
            {
                if (bRegenInfo) item.CreatedDefinitionLines();
                if (aDCIndex <= 0 || item.DCIndex == aDCIndex)
                    _rVal.AddRange(uopLinePair.FromList(item.ShelfLns));

            }
            return _rVal;
        }

        public List<uopLinePair> GetBoxLines(int aDCIndex = 0, bool bRegenInfo = false)
        {
            List<uopLinePair> _rVal = new List<uopLinePair>();


            foreach (DowncomerInfo item in this)
            {
                if (aDCIndex <= 0 || item.DCIndex == aDCIndex) _rVal.AddRange(item.GetBoxLines(bRegenLimits: bRegenInfo));
            }
            return _rVal;
        }


        public List<uopLinePair> GetWeirLines(bool bForFreeBubbleAreas = false) => bForFreeBubbleAreas ? uopLinePair.FromList(CreateFBAWeirLines()) : WeirLines;
        public DowncomerInfo Item(int aIndex)
        {
            if (aIndex < 1 || aIndex > Count) return null;
            DowncomerInfo _rVal = base[aIndex - 1];
            _rVal.DowncomerCount = DowncomerCount;
            _rVal.Index = aIndex;
            return _rVal;
        }

        public new void Add(DowncomerInfo aInfo)
        {
            if (aInfo == null) return;
            aInfo.RingRadius = RingRadius;
            aInfo.DeckRadius = DeckRadius;
            aInfo.Thickness = Thickness;
            aInfo.ShelfWidth = ShelfWidth;
            aInfo.InsideWidth = InsideWidth;
            aInfo.RingClipRadius = RingClipRadius;
            aInfo._RingClipHoleDiameter = _RingClipHoleDiameter;
            aInfo.RingClipSize = RingClipSize;
            aInfo.Clearance = Clearance;
            aInfo.Divider = new DividerInfo(Divider);
            aInfo.RoundingMethod = RoundingMethod;
            aInfo.EndPlateOverhang = EndPlateOverhang;
            aInfo.Index = Count + 1;

            base.Add(aInfo);
        }

        public override string ToString()
        {
            return $"DowncomerDataSet [Space: {Spacing} Count: {Count} ]";
        }

        public new int IndexOf(DowncomerInfo aMember)
        {
            if (aMember == null) return 0;
            return base.IndexOf(aMember) + 1;
        }

        object ICloneable.Clone() => new DowncomerDataSet(this);

        /// <summary>
        /// the lines on the left and right of each panel with points defined on the intersections of the divider(s)
        /// </summary>
        /// <param name="bForFreeBubbleAreas"></param>
        /// <param name="aInterceptors"></param>
        /// <param name="bIncludeClearance"></param>
        /// <returns></returns>
        public List<uopLinePair> PanelLines(bool bForFreeBubbleAreas = false, List<uopLinePair> aInterceptors = null, bool bIncludeClearance = true, bool bIncludeMoonEdges = false)
        {
            double rad = bForFreeBubbleAreas ? RingRadius : DeckRadius;
            aInterceptors ??= uopLinePair.FromList(CreateDividerLns(rad, DeckLap));
            double clrc = bForFreeBubbleAreas ? ShelfWidth : bIncludeClearance ? PanelClearance : 0;
            return uopLinePair.FromList(CreatePanelLns(aRadius: rad, aInterceptors, aClearance: clrc, bIncludeMoonEdges: bIncludeMoonEdges));
        }

        public List<mdStartupZone> StartUpAreas()
        {
            List<mdStartupZone> _rVal = new List<mdStartupZone>();
            mdTrayAssembly assy = TrayAssembly;
            List<mdSpoutArea> spoutareas = SpoutAreas(false);
            foreach (var sa in spoutareas) _rVal.Add(new mdStartupZone(sa, assy));

            return _rVal;
        }

        public List<mdSpoutArea> SpoutAreas(bool bIncludeVirtuals = false)
        {
            List<mdSpoutArea> _rVal = new List<mdSpoutArea>();

            List<uopLinePair> panelpairs =  PanelLines(bIncludeClearance: false, bIncludeMoonEdges:true);
            //get the weirs for the entire tray
            List<uopLinePair> weirs = WeirLines;


            uopLinePairs.Rotate(panelpairs, null, -90);
            panelpairs = uopLinePairs.Sort(panelpairs, dxxSortOrders.TopToBottom);
            uopLinePairs.SetSide(panelpairs, uppSides.Left, uppSides.Top, uppSides.Right, uppSides.Bottom);
            uppMDDesigns design = DesignFamily;
            mdSpoutArea spoutArea = null;
            double panelrad = DeckRadius;
            uopLinePair localLimits;
            uopLine top;
            uopLine bot;
            int iNd = Count;
            int iNp = iNd + 1;

            List<mdSpoutArea> boxareas = null;
            double minarea = 1.5 * Math.PI * Math.Pow(0.375, 2);
            double minheight = 0.75;

            // loop on downcomers
            for (int dcindex = 1; dcindex <= iNd; dcindex++)
            {
                DowncomerInfo member = Item(dcindex);

                //if (member.IsVirtual) 
                //    continue;
                //get the weirs for the downcomer
                List<uopLinePair> dcpairs = bIncludeVirtuals ? weirs.FindAll((x) => x.PartIndex == dcindex) : uopLinePairs.FromULinePairs(member.WeirLns, dxxSortOrders.BottomToTop, false);

                //sort the weirs lowest to highest so the return will be sorted in the proper box order
                if (dcpairs.Count > 1 && bIncludeVirtuals)
                    dcpairs = uopLinePairs.Sort(dcpairs, dxxSortOrders.BottomToTop);

                int occr = 1;
                if (design.IsStandardDesignFamily())
                    occr = Math.Round(member.X, 2) == 0 ? 1 : 2;
                else if (design.IsBeamDesignFamily())
                    occr = 2;


                uopLinePair boxweirs = null;
                for (int boxindex = 1; boxindex <= dcpairs.Count; boxindex++)
                {
                    boxareas = new List<mdSpoutArea>();
                    boxweirs = dcpairs[boxindex - 1];
                    uopRectangle limits = boxweirs.Bounds();

                    uopLine left = boxweirs.GetSide(uppSides.Left);
                    uopLine right = boxweirs.GetSide(uppSides.Right);
                    uopVectors epts = right.EndPoints();
                    uopVectors ipts = left.EndPoints();
                    uopLine limitTop = new uopLine(epts.GetVector(dxxPointFilters.AtMaxY), ipts.GetVector(dxxPointFilters.AtMaxY)) { Side = uppSides.Top };
                    uopLine limitBot = new uopLine(ipts.GetVector(dxxPointFilters.AtMinY), epts.GetVector(dxxPointFilters.AtMinY)) { Side = uppSides.Bottom };
                    epts = limitTop.EndPoints(true) + limitBot.EndPoints(true);
                    uopLines bsegs = epts.LineSegments(bClosed: true);
                    bool virtualarea = boxweirs.IsVirtual || member.IsVirtual;
                    double topang = limitTop.AngleOfInclination;
                    double botang = limitBot.AngleOfInclination;

                  
                    for (int panelindex = 1; panelindex <= panelpairs.Count; panelindex++)
                    {
                        double panelwidth = Spacing - BoxWidth;
                        uopLinePair panelpair = panelpairs[panelindex - 1];
                        top = panelpair.GetSide(uppSides.Top);
                        bot = panelpair.GetSide(uppSides.Bottom);
                        double panelY = panelpair.Y;
                        URECTANGLE panelLimits = new URECTANGLE(boxweirs.GetSide(uppSides.Left).X(), top == null ? panelrad : top.Y(), boxweirs.GetSide(uppSides.Right).X(), bot == null ? -panelrad : bot.Y());

                        panelY = panelLimits.Center.Y;
                        panelwidth = panelLimits.Height;


                        if (top == null) top = new uopLine(limits.Left - 0.1, limits.Top, limits.Right + 0.1, limits.Top) { Side = uppSides.Top };
                        if (bot == null) bot = new uopLine(limits.Left - 0.1, limits.Bottom, limits.Right + 0.1, limits.Bottom) { Side = uppSides.Bottom };
                        top.sp.X = limits.Left - 0.1;
                        top.ep.X = limits.Right + 0.1;
                        bot.sp.X = top.sp.X;
                        bot.ep.X = top.ep.X;
                        ipts = uopVectors.Zero;

                        if (design.IsStandardDesignFamily() && !virtualarea)
                        {
                            if (Math.Round(panelpair.Y, 2) < 0) virtualarea = true;
                        }
                        if (!bIncludeVirtuals && virtualarea) continue;
                        if (top.Y() >= limits.Top && bot.Y() >= limits.Top) continue;  // both panel lines are above the dc area
                        if (top.Y() <= limits.Bottom && bot.Y() <= limits.Bottom)
                            continue; // both panel lines are below the dc area

                        //if (dcindex == 2 && panelindex == 2)
                        //{
                        //    Console.Beep();
                        //}

                        localLimits = new uopLinePair(uopLine.CloneCopy( top), uopLine.CloneCopy(bot));
                        bool intercepts_top = limits.Contains(top.Y(), dxx2DOrdinateDescriptors.Y, bOnIsIn: false);
                        bool intercepts_bot = limits.Contains(bot.Y(), dxx2DOrdinateDescriptors.Y, bOnIsIn: false);
                        bool limited = intercepts_top || intercepts_bot;
                        uopVectors tpoints = uopVectors.Zero;
                        uopVectors bpoints = uopVectors.Zero;
                        uopRectangle lims = null;
                        if (!limited)  // full size box bottom area
                        {
                            ipts = new uopVectors(epts, bCloneMembers: false);
                            lims = ipts.BoundingRectangle;
                        }
                        else
                        {
                            limited = true;
                            uopLine lim_Top = new uopLine(limitTop);
                            uopLine lim_Bot = new uopLine(limitBot);
                            // trim the limit lines with the panel lines
                            if (lim_Top.DeltaY != 0)
                            {

                                uopVector ip = null;
                                if (lim_Top.MinYr(aPrecis: 4) < top.Y(aPrecis: 4) && lim_Top.MaxYr(aPrecis: 4) > top.Y(aPrecis: 4))
                                {
                                    ip = lim_Top.IntersectionPt(top);
                                    if (ip != null)
                                        lim_Top.EndPoints().GetVector(dxxPointFilters.AtMaxY).SetCoordinates(ip.X, ip.Y);
                                }
                                if (lim_Top.MinYr(aPrecis: 4) < bot.Y(aPrecis: 4) && lim_Top.MaxYr(aPrecis: 4) > bot.Y(aPrecis: 4))
                                {
                                    ip = lim_Top.IntersectionPt(bot);
                                    if (ip != null)
                                        lim_Top.EndPoints().GetVector(dxxPointFilters.AtMinY).SetCoordinates(ip.X, ip.Y);
                                }



                            }
                            if (lim_Bot.DeltaY != 0)
                            {

                                uopVector ip = null;
                                if (lim_Bot.MinYr(aPrecis: 4) < top.Y(aPrecis: 4) && lim_Bot.MaxYr(aPrecis: 4) > top.Y(aPrecis: 4))
                                {
                                    ip = lim_Bot.IntersectionPt(top);
                                    if (ip != null)
                                        lim_Bot.EndPoints().GetVector(dxxPointFilters.AtMaxY).SetCoordinates(ip.X, ip.Y);
                                }
                                if (lim_Bot.MinYr(aPrecis: 4) < bot.Y(aPrecis: 4) && lim_Bot.MaxYr(aPrecis: 4) > bot.Y(aPrecis: 4))
                                {
                                    ip = lim_Bot.IntersectionPt(bot);
                                    if (ip != null)
                                        lim_Bot.EndPoints().GetVector(dxxPointFilters.AtMinY).SetCoordinates(ip.X, ip.Y);
                                }


                            }

                            tpoints = intercepts_top ? top.Intersections(bsegs, false, true) : lim_Top.EndPoints(bGetClones: true);
                            bpoints = intercepts_bot ? bot.Intersections(bsegs, false, true) : limitBot.EndPoints(bGetClones: true);
                            if (tpoints.Count == 2 && intercepts_top && lim_Top.MinYr(aPrecis: 4) < top.Y(aPrecis: 4))
                            {
                                if (member.X > 0)
                                {
                                    if (tpoints.GetExtremeOrd(false, false, aPrecis: 4) < right.X(aPrecis: 4))
                                        tpoints.Add(lim_Top.EndPoints().GetVector(dxxPointFilters.AtMaxX), bAddClone: true);
                                }
                                else
                                {
                                    if (tpoints.GetExtremeOrd(true, false, aPrecis: 4) > left.X(aPrecis: 4))
                                        tpoints.Add(lim_Top.EndPoints().GetVector(dxxPointFilters.AtMinX), bAddClone: true);
                                }

                            }
                            if (bpoints.Count == 2 && intercepts_bot && lim_Bot.MaxYr(aPrecis: 4) > bot.Y(aPrecis: 4))
                            {
                                if (member.X > 0)
                                {
                                    if (bpoints.GetExtremeOrd(false, false, aPrecis: 4) < right.X(aPrecis: 4))
                                    {
                                        uopVector u1 = lim_Bot.EndPoints().GetVector(dxxPointFilters.AtMaxX);
                                        if (u1.Y > bot.Y()) bpoints.Add(u1, bAddClone: true);
                                    }

                                }
                                else
                                {
                                    if (bpoints.GetExtremeOrd(true, false, aPrecis: 4) > left.X(aPrecis: 4))
                                        bpoints.Add(lim_Bot.EndPoints().GetVector(dxxPointFilters.AtMinX), bAddClone: true);
                                }

                            }


                            ipts = tpoints.Sorted(dxxSortOrders.RightToLeft) + bpoints.Sorted(dxxSortOrders.LeftToRight);
                            lims = ipts.BoundingRectangle;
                            limited = Math.Round(lims.Height, 4) < Math.Round(panelwidth, 4);
                            if (!limited)
                            {
                                // Console.Beep();
                                intercepts_top = false;
                                intercepts_bot = false;
                            }
                            else
                            {
                                //Console.Beep();


                            }
                        }

                        ipts.RemoveCoincidentVectors(2);
                        if (ipts.Count > 0)
                        {

                            spoutArea = new mdSpoutArea(ipts, panelindex, dcindex, boxindex, localLimits, occr, new uopRectangle(panelLimits), aDCInfo: new DowncomerInfo(member))
                            {
                                IsVirtual = virtualarea,
                                PartIndex = boxindex,
                                Col = boxindex,
                                Row = boxweirs.Row,
                                Index = _rVal.Count + 1,
                                BoxWeirs = new uopLinePair(boxweirs)

                            };
                            if (!spoutArea.IsRectangular())
                            {
                                uopLines tsegs = spoutArea.Segments.LineSegments(bGetClones: true);
                                tsegs.RemoveAll((x) => x.IsVertical(2));
                                List<uopLine> hsegs = tsegs.FindAll((x) => x.IsHorizontal(2));
                                List<uopLine> angsegs = tsegs.FindAll((x) => !x.IsHorizontal(2));
                                foreach (uopLine angseg in angsegs)
                                {
                                    if (angseg.MidPt.Y >= spoutArea.Y)
                                        if (angseg.MaxY >= spoutArea.Y) spoutArea.LimitedTop = true;
                                        else
                                        if (angseg.MinY < spoutArea.Y) spoutArea.LimitedBottom = true;
                                }

                            }
                            //if (intercepts_top) spoutArea.LimitedTop = true;
                            //if (intercepts_bot) spoutArea.LimitedBottom = true;


                            if (spoutArea.LimitedBottom && boxweirs.Row == 2)
                            {
                                if (spoutArea.LimitedTop)
                                {
                                    spoutArea.Direction = dxxOrthoDirections.Up;
                                }
                                else
                                {
                                    spoutArea.Direction = dxxOrthoDirections.Down;
                                }

                                //if (boxweirs.IntersectionType1 == uppIntersectionTypes.ToRing && (boxweirs.IntersectionType2 == uppIntersectionTypes.ToDivider || boxweirs.IntersectionType2 == uppIntersectionTypes.StraddlesRingToDivider))
                                //{ spoutArea.Direction = dxxOrthoDirections.Up; }
                                //else if(boxweirs.IntersectionType2 == uppIntersectionTypes.ToRing && (boxweirs.IntersectionType1 == uppIntersectionTypes.ToDivider || boxweirs.IntersectionType1 == uppIntersectionTypes.StraddlesRingToDivider))
                                //{ spoutArea.Direction = dxxOrthoDirections.Down; }
                            }
                            boxareas.Add(spoutArea);
                        }

                    }

                    for (int g = 1; g <= boxareas.Count; g++)
                    {
                        spoutArea = boxareas[g - 1];

                        if (spoutArea.Area < minarea)
                            continue;
                        uopRectangle lims = spoutArea.Limits();
                        if (lims.Height <= minheight)
                            continue;

                    
                        double panelY = Math.Round(spoutArea.PanelLimits.Center.Y,3);
                        double saY = Math.Round(lims.Center.Y, 3);
                        if(saY != panelY)
                        {
                            if (saY < panelY) spoutArea.LimitedTop = true; else spoutArea.LimitedBottom = true;
                        }
                        
                        // set the instances

                        double area = spoutArea.Area;

                        if (design.IsStandardDesignFamily())
                        {
                            if (Math.Round(spoutArea.Y, 0) > 0)
                            {
                                spoutArea.Instances.Add(0, -2 * spoutArea.Y, aRotation: 180, bInverted: false, bLeftHanded: true, aRow: spoutArea.OpposingPanelIndex.Value, aCol: spoutArea.DowncomerIndex);
                                if (Math.Round(member.X, 2) != 0)
                                {
                                    spoutArea.Instances.Add(-2 * spoutArea.X, 0, aRotation: 0, bInverted: false, bLeftHanded: true, aRow: spoutArea.PanelIndex, aCol: spoutArea.OpposingDowncomerIndex.Value);
                                    spoutArea.Instances.Add(-2 * spoutArea.X, -2 * spoutArea.Y, aRotation: 180, bInverted: false, bLeftHanded: false, aRow: spoutArea.OpposingPanelIndex.Value, aCol: spoutArea.OpposingDowncomerIndex.Value);
                                }
                            }
                            else
                            {
                                if (Math.Round(member.X, 2) != 0)
                                {
                                    spoutArea.Instances.Add(-2 * spoutArea.X, 0, aRotation: 0, bInverted: false, bLeftHanded: true, aRow: spoutArea.PanelIndex, aCol: spoutArea.OpposingDowncomerIndex.Value);
                                }
                            }
                        }
                        else if (design.IsBeamDesignFamily())
                        {
                            spoutArea.Instances.Add(-2 * spoutArea.X, -2 * spoutArea.Y, aRotation: 180, bInverted: false, bLeftHanded: false, aRow: spoutArea.OpposingPanelIndex.HasValue ? spoutArea.OpposingPanelIndex.Value : spoutArea.PanelIndex, aCol: spoutArea.OpposingDowncomerIndex.HasValue ? spoutArea.OpposingDowncomerIndex.Value : spoutArea.DowncomerIndex);
                        }




                        spoutArea.Index = _rVal.Count + 1;

                        _rVal.Add(spoutArea);
                    }

                }


            }

            //special case for two beams. there may be two regions with the same panel index and the same downcomer index where the group straddles the beam
            if (DesignFamily.IsBeamDesignFamily() && Divider.Offset != 0)
            {
                for (int dcindex = 1; dcindex <= iNd; dcindex++)
                {
                    for (int paneindex = 1; paneindex <= iNp; paneindex++)
                    {
                        List<mdSpoutArea> subareas = _rVal.FindAll((x) => x.DowncomerIndex == dcindex && x.PanelIndex == paneindex);
                        if (subareas.Count > 1)
                        {
                            double totalArea = 0;
                            foreach (var subarea in subareas) totalArea += subarea.Area;

                            if (totalArea > 0)
                            {
                                foreach (var subarea in subareas) subarea.SpoutAreaFraction = subarea.Area / totalArea;

                            }


                        }
                    }
                }


            }

            return _rVal;

        }




        /// <summary>
        /// the lines on the left and right of each panel with points defined on the intersections of the divider(s)
        /// </summary>
        /// <param name="aRadius">the deck radius to apply</param>
        /// <param name="aInterceptors"></param>
        /// <param name="aClearance"></param>
        /// <returns></returns>
        internal List<ULINEPAIR> CreatePanelLns(double aRadius, List<uopLinePair> aInterceptors = null, double aClearance = 0, bool bIncludeMoonEdges = false)
        {

            List<ULINEPAIR> _rVal = new List<ULINEPAIR>();
            double rad = aRadius <= 0 ? DeckRadius : aRadius;
            double clrc = aClearance; // !bForFreeBubbleAreas && bIncludeClearance ? PanelClearance : 0;
            double offset = clrc;
            double space = Spacing;
            double wd = BoxWidth / 2;
            double reducer = aClearance == DeckSectionClearance && HasFoldedWeirs ? mdGlobals.FolderWeirPanelClearanceAdder : 0;
            if (rad <= 0 || space <= 0) return _rVal;

            DowncomerInfo leftDC = null;
            DowncomerInfo rightDC = null;
            double xRight;
            double xLeft;
            double y1;
            double y2;
            ULINE? l1;
            ULINE? l2;
            foreach (var dcInfo in this)
            {


                leftDC = dcInfo;
                xLeft = leftDC.X + wd + offset;
                if (dcInfo.DCIndex == 1) xLeft -= reducer;

                rightDC = Find((x) => Math.Round(x.X, 3) == Math.Round(leftDC.X + space, 3));
                xRight = (rightDC == null) ? rad : rightDC.X - wd - offset;
                y1 = Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(xLeft, 2));
                y2 = (rightDC == null) ? rad : Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(xRight, 2));

                l1 = new ULINE(xLeft, y1, xLeft, -y1) { Side = uppSides.Left, Points = UVECTORS.Zero };
                l2 = null;
                if (rightDC != null) l2 = new ULINE(xRight, y2, xRight, -y2) { Side = uppSides.Right, Points = UVECTORS.Zero };

                if(!l2.HasValue && bIncludeMoonEdges)
                {
                    l2 = new ULINE(xRight, y2, xRight, -y2) { Side = uppSides.Right, Points = UVECTORS.Zero };
                }

                _rVal.Add(new ULINEPAIR(l1, l2, aTag: $"PANEL_{leftDC.Index}") { Value = xLeft + (xRight - xLeft) / 2, Col = leftDC.DCIndex });

            }

            // the last moon
            if (Count > 0)
            {
                xLeft = -rad;
                leftDC = this.Last();
                xRight = leftDC.X - wd - offset + reducer;
                y2 = Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(xRight, 2));
                l2 = new ULINE(xRight, y2, xRight, -y2) { Side = uppSides.Right, Points = UVECTORS.Zero };
                l1 = null;

                if (!l1.HasValue && bIncludeMoonEdges)
                {
                    l1 = new ULINE(xLeft, y2, xLeft, -y2) { Side = uppSides.Left, Points = UVECTORS.Zero };
                }

                _rVal.Add(new ULINEPAIR(l1, l2, aTag: $"PANEL_{leftDC.Index + 1}") { Value = xLeft + (xRight - xLeft) / 2, Col = leftDC.DCIndex + 1 });

            }

            if (aInterceptors == null) return _rVal;
            List<ULINEPAIR> Intercepted = new List<ULINEPAIR>();
            foreach (ULINEPAIR pair in _rVal)
            {

                ULINEPAIR basepair = pair;
                ULINE baseln = ULINE.Null;
                for (int j = 1; j <= 2; j++)
                {

                    ULINE? i2 = j == 1 ? basepair.Line1 : basepair.Line2;
                    if (i2.HasValue)
                    {
                        baseln = i2.Value;
                        baseln.Points.Add(baseln.sp);

                        foreach (uopLinePair ipair in aInterceptors)
                        {
                            for (int i = 1; i <= 2; i++)
                            {

                                uopLine icep = i == 1 ? ipair.Line1 : ipair.Line2;
                                if (icep == null) continue;
                                ULINE i1 = new ULINE(icep);
                                if (i1.Intersects(baseln, out UVECTOR ip, bMustBeOn1: true, bMustBeOn2: true))
                                    baseln.Points.Add(ip);

                            }
                        }

                        
                        baseln.Points.Add(baseln.ep);

                        baseln.Points.Sort(dxxSortOrders.TopToBottom);

                        UVECTORS pts = baseln.Points; // UVECTORS.Zero;
                                                      //for(int i = 1; i <= baseln.Points.Count; i++)
                                                      //{

                        //    UVECTOR u1 = baseln.Points.Item(i);
                        //    pts.Add(u1);
                        //    continue;

                        //    if (i + 1 > baseln.Points.Count)
                        //    {
                        //        pts.Add(u1);
                        //        break;
                        //    }
                        //    UVECTOR u2 = baseln.Points.Item(i + 1);
                        //    double d1 = u1.Y - u2.Y;
                        //    if(d1 >= 12)
                        //    {

                        //        pts.Add(u1);
                        //        pts.Add(u2);
                        //        i++;
                        //    }


                        //}
                        baseln.sp = pts.Item(1);
                        baseln.ep = pts.Item(pts.Count);

                        baseln.Points = pts;

                        if (j == 1) { basepair.Line1 = baseln; } else { basepair.Line2 = baseln; }
                    }

                    if (j == 2) Intercepted.Add(basepair);

                }



            }



            return Intercepted;
        }

        /// <summary>
        /// creates the shapes that are the boundaries of the free bubbling areas 
        /// </summary>
        /// <returns></returns>
        public uopPanelSectionShapes FreeBubblingShapes() => FreeBubblingShapes(out _); // (aClearance: ShelfWidth, aLap: 0, out _, out _);

        /// <summary>
        /// creates the shapes that are the boundaries of the free bubbling areas 
        /// </summary>
        /// <returns></returns>
        public uopPanelSectionShapes FreeBubblingShapes(out List<uopLine> rPanelLines) => CreatePanelShapes(aClearance: ShelfWidth, aLap: 0, out rPanelLines, out _);


        /// <summary>
        /// creates the deck section shapes that are the panel shapes divided by the current deck splices
        /// </summary>
        /// <returns></returns>
        public uopSectionShapes CreateSectionShapes(uopDeckSplices aSplices = null, uopPanelSectionShapes aPanelSectionShapes = null, bool bUpdatePanels = false, bool bVerbose = true,bool bSetInstances = true)
        {
            mdTrayAssembly assy = TrayAssembly;
            uopSectionShapes _rVal = new uopSectionShapes(assy, this);
            if (assy == null) return _rVal;
            ProjectType = assy.ProjectType;
            
            if (aPanelSectionShapes == null || bUpdatePanels)
            {
                //_PanelShapes = null;
                aPanelSectionShapes = PanelShapes( bUpdatePanels: bUpdatePanels,  bVerbose:bVerbose);
            }
            if (bVerbose) assy.RaiseStatusChangeEvent($"Creating {assy.TrayName()} Deck Section Shapes");
            if (ProjectType != uppProjectTypes.MDDraw) 
            {
                foreach (var panel in aPanelSectionShapes) _rVal.Add(new uopSectionShape(panel));
                return _rVal;
            }
            
            double rad = DeckRadius;
            double ringrad = RingRadius;
            uopDeckSplices splices = aSplices == null ? assy.DeckSplices : aSplices;
           
           
            //List<mdDeckPanel> mdpanels = assy.DeckPanels.ToList(bGetClones: false, bGetVirtuals: false);
            int panelcount = aPanelSectionShapes.MaxPanelIndex;
            for (int panelindex = 1; panelindex <= panelcount; panelindex++)
            {

                List<uopPanelSectionShape> panelsections = aPanelSectionShapes.FindAll(x => x.PanelIndex == panelindex);
                for (int panelsectionindex = 1; panelsectionindex <= panelsections.Count; panelsectionindex++)
                {
                    int secidx = 1;

                    uopPanelSectionShape psectionshape = panelsections[panelsectionindex - 1];
                    URECTANGLE plimits = psectionshape.BoundsV;
                    if (psectionshape.IsVirtual) continue;

                    if (bVerbose) assy.RaiseStatusChangeEvent($"Creating {assy.TrayName()} Panel {panelindex},{panelsectionindex} Deck Section Shapes");

                    double panelY = psectionshape.Y;
                    double panelX = psectionshape.PanelX;
                    uopShape panel = new uopShape(psectionshape);
                    uopSegments segs = new uopSegments(panel.Segments);
                    List<uopDeckSplice> psplices = new List<uopDeckSplice>();
            
                    try
                    {
                        psplices = splices.FindAll(s => s != null &&  s.PanelIndex == panelindex);
                        if (psplices.Count > 1)
                            psplices = uopDeckSplices.SortSet(psplices, bLowToHigh: psectionshape.IsHalfMoon & panelX > 0);

                    }
                    catch (Exception ex)
                    {
                        Debug.Fail(ex.Message);
                        psplices = new List<uopDeckSplice>();
                    }
                    
                    uopLinePair pweirs = panel.LinePair;
                    bool moon = psectionshape.IsHalfMoon; 

                    if (DesignFamily.IsBeamDesignFamily() && psplices.Count > 0)
                    {
                        psplices = !moon ? splices.FindAll(s => s != null && s.PanelIndex == panelindex && s.Ordinate < psectionshape.Top && s.Ordinate > psectionshape.Bottom) : splices.FindAll(s => s != null && s.PanelIndex == panelindex && s.Ordinate < psectionshape.Right && s.Ordinate > psectionshape.Left); 
                    }

                    uopSectionShape section = null;
                    if (psplices.Count <= 0)
                    {
                        //there are no splices so just clone the panel section shape
                        section = new uopSectionShape(psectionshape, aAssy: assy)
                        {
                            PanelIndex = panelindex,
                            PanelSectionIndex = panelsectionindex,
                            PanelSectionCount = panelsections.Count,
                            SectionIndex = secidx,
                            PanelX = panelX,
                            PanelY = panelY,
                            LinePair = new uopLinePair(pweirs),                        
                            LeftDowncomerInfo = pweirs.SideIsIsDefined(uppSides.Left) ? DowncomerInfo.CloneCopy(psectionshape.LeftDowncomerInfo): null,
                            RightDowncomerInfo = pweirs.SideIsIsDefined(uppSides.Right) ?DowncomerInfo.CloneCopy(psectionshape.RightDowncomerInfo) : null,

                        };

                        section.TopSpliceType =  uppSpliceIndicators.ToRing ;
                        section.BottomSpliceType = uppSpliceIndicators.ToRing;

                        _rVal.Add(section);

                    }
                    else
                    {

                        for (int s = 0; s <= psplices.Count; s++)
                        {

                            uopDeckSplice topsplice = s > 0 ? psplices[s - 1] : null;
                            uopDeckSplice botsplice = s < psplices.Count ? psplices[s] : null;
                            uopVectors verts = uopVectors.Zero;
                            if (botsplice != null && topsplice == null)
                            {
                                botsplice.PanelLimits = new URECTANGLE(plimits);
                                verts.Append(panel.Vertices.FindAll(v => v.Y > botsplice.Y), true);
                                uopLine trimmer = new uopLine(panel.Left, botsplice.Ordinate, panel.Right, botsplice.Ordinate);
                                uopVectors ipts = trimmer.Intersections(panel.Segments, aSegsAreInfinite: false, aLineIsInfinite: true);
                                verts.Append(ipts);
                            }
                            else if (botsplice == null && topsplice != null)
                            {
                                topsplice.PanelLimits = new URECTANGLE(plimits);
                                uopLine trimmer = new uopLine(panel.Left, topsplice.Ordinate, panel.Right, topsplice.Ordinate);
                                uopVectors ipts = trimmer.Intersections(panel.Segments, aSegsAreInfinite: false, aLineIsInfinite: true);
                                verts.AddRange(ipts);

                                verts.Append(panel.Vertices.FindAll(v => v.Y < topsplice.Y), true);
                            }
                            else
                            {
                                botsplice.PanelLimits = new URECTANGLE(plimits);
                                topsplice.PanelLimits = new URECTANGLE(plimits);

                                uopLine trimmer = new uopLine(panel.Left, topsplice.Ordinate, panel.Right, topsplice.Ordinate);
                                uopVectors ipts = trimmer.Intersections(panel.Segments, aSegsAreInfinite: false, aLineIsInfinite: true);
                                verts.AddRange(ipts);

                                trimmer = new uopLine(panel.Left, botsplice.Ordinate, panel.Right, botsplice.Ordinate);
                                ipts = trimmer.Intersections(panel.Segments, aSegsAreInfinite: false, aLineIsInfinite: true);
                                verts.AddRange(ipts);


                            }

                            if (verts.Count > 0)
                            {

                                if(Math.Round(panelX,2) == 0 )
                                {
                                    if(topsplice == null)
                                    {
                                        verts.Add(panelX, rad, aRadius: rad);
                                    }
                                    if (botsplice == null)
                                    {
                                        verts.Add(panelX, -rad, aRadius: rad);
                                    }

                                }
                                uopRectangle bounds = new uopRectangle(verts);
                                foreach (uopVector ip in panel.Vertices)
                                {
                                    if (bounds.Contains(ip))
                                    {
                                        if (verts.Find(x => x.IsEqual(ip, 4)) == null)
                                            verts.Add(ip, bAddClone: true);
                                    }
                                }
                                
                                //set the radius vertices
                                verts.Circularize(rad, 4, null);
                                

                                uopLine weirl = pweirs.GetSide(uppSides.Left, bGetClone: true);
                                uopLine weirr = pweirs.GetSide(uppSides.Right, bGetClone: true);
                                if (weirl != null)
                                {
                                    weirl.Rectify();
                                    if (weirl.MaxY > bounds.Top) weirl.ep.Y = bounds.Top;
                                    if (weirl.MinY < bounds.Bottom) weirl.sp.Y = bounds.Bottom;
                                }
                                if (weirr != null)
                                {
                                    weirr.Rectify();
                                    if (weirr.MaxY > bounds.Top) weirr.ep.Y = bounds.Top;
                                    if (weirr.MinY < bounds.Bottom) weirr.sp.Y = bounds.Bottom;
                                }

                                section = new uopSectionShape(psectionshape, topsplice, botsplice, aAssy: assy, bCloneSplices: false, aVertices: new uopVectors(verts))
                                {

                                    PanelIndex = panelindex,
                                    PanelSectionIndex = panelsectionindex,
                                    PanelSectionCount = panelsections.Count,
                                    SectionIndex = secidx,
                                    LinePair = pweirs,
                                    LeftDowncomerInfo = pweirs.SideIsIsDefined(uppSides.Left) ? DowncomerInfo.CloneCopy(psectionshape.LeftDowncomerInfo) : null,
                                    RightDowncomerInfo = pweirs.SideIsIsDefined(uppSides.Right) ? DowncomerInfo.CloneCopy(psectionshape.RightDowncomerInfo) : null,

                                };

                                if (section.IsManway)
                                {
                                    if (!HasFoldedWeirs)
                                    {
                                        //reduce the section width (the shape is a rectangle)
                                        verts.GetVector(dxxPointFilters.GetTopLeft).Move(mdGlobals.FolderWeirPanelClearanceAdder);
                                        verts.GetVector(dxxPointFilters.GetBottomLeft).Move(mdGlobals.FolderWeirPanelClearanceAdder);
                                        verts.GetVector(dxxPointFilters.GetTopRight).Move(-mdGlobals.FolderWeirPanelClearanceAdder);
                                        verts.GetVector(dxxPointFilters.GetBottomRight).Move(-mdGlobals.FolderWeirPanelClearanceAdder);
                                        section.Vertices.Clear();
                                        section.Vertices.AddRange(verts);

                                    }
                                  
                                }

                                if (moon)
                                {
                                    
                                     if (topsplice != null)
                                    {
                                        topsplice.Vertical = true;
                                        if (topsplice.Vertical)
                                        {
                                            section.TopSpliceType = uppSpliceIndicators.TabFemale;
                                            topsplice.TabDirection = section.X > 0 ? dxxOrthoDirections.Left : dxxOrthoDirections.Right;
                                            uopShape circsec = topsplice.X > 0 ? circsec = uopShape.CircleSection(0, 0, section.DeckRadius, 0,section.Left,  topsplice.X) : circsec = uopShape.CircleSection(0, 0, section.DeckRadius, 0,  topsplice.X, section.Right ); ;

                                            section.Vertices.Clear();
                                            section.Vertices.AddRange(circsec.Vertices);

                                        }
                                        else
                                        {
                                            section.Vertices.Clear();
                                            section.Vertices.AddRange(verts);

                                        }
                                    }
                                    if (botsplice != null)
                                    {
                                        botsplice.Vertical = true;
                                        if (botsplice.Vertical)
                                        {
                                            section.BottomSpliceType = uppSpliceIndicators.TabMale;
                                            botsplice.TabDirection = section.X > 0 ? dxxOrthoDirections.Left : dxxOrthoDirections.Right;
                                            uopShape circsec = botsplice.X > 0 ? circsec = uopShape.CircleSection(0, 0, section.DeckRadius, 0, botsplice.X) : circsec = uopShape.CircleSection(0, 0, section.DeckRadius, 0,null, botsplice.X); ;

                                            section.Vertices.Clear();
                                            section.Vertices.AddRange(circsec.Vertices);
                                        }
                                        else
                                        {
                                            section.BottomSpliceType = uppSpliceIndicators.AngleFemale;
                                        }
                                        
                                    }
                                }

                                section.Update();
                                _rVal.Add(section);
                                secidx++;
                            }

                        }

                    }

                }



            }
          
            uopSectionShapes.SetMDSpliceProperties(_rVal, assy, bVerbose:bVerbose);
            uopSectionShapes.SetMDMechanicalProperties(_rVal, assy, bVerbose: bVerbose);
            if (bSetInstances) uopSectionShapes.SetMDSectionInstances(_rVal, assy, bVerbose: bVerbose);

            
            return _rVal;
        }

        /// <summary>
        /// creates the shapes that are the boundaries of circle sections divided by the current Divider(s)
        /// </summary>
        /// <param name="aClearance">the downcomer clearance to apply</param>
        /// <param name="aLap">the ring/beam lap to apply</param>
        /// <param name="rPanelLines">eturns the lines that are the left and right edges of the circle section based on the passed clearance</param>
        /// <param name="rWeirs">returns the weir lines for the panels</param>
        /// <param name="bReturnVirtuals">flag to return virtual panel shapes</param>
        /// <param name="bSetBPSites">flag to asssign bubble promoters sites to the return shapes  </param>
         /// <returns></returns>
        public uopPanelSectionShapes CreatePanelShapes(double aClearance, double aLap, out List<uopLine> rPanelLines, out List<uopLinePair> rWeirs, bool bReturnVirtuals = false, bool bSetBPSites = false)
        {

            mdTrayAssembly assy = TrayAssembly;

            uopPanelSectionShapes _rVal = new uopPanelSectionShapes(assy, this);

            bool specialcase = ProjectType == uppProjectTypes.MDDraw && DesignFamily.IsStandardDesignFamily() && OddDowncomers && DowncomerCount > 1;
            bool specialcasesatisfied = false;

            rWeirs = uopLinePairs.FromULinePairs(CreateFBAWeirLines());
            double lap = aLap;
            double rad = RingRadius + lap;
            bool isvirtual = false;

            uopArc deckcircle = new uopArc(rad);
            List<ULINEPAIR> trimmers = CreateDividerLns(rad, -lap);
            List<ULINEPAIR> panellines = CreatePanelLns(aRadius: rad, aInterceptors: uopLinePair.FromList(trimmers), aClearance: aClearance);
            rPanelLines = new List<uopLine>();

            ULINE lineL = ULINE.Null;
            ULINE lineR = ULINE.Null;
            int maxrow = MaxRow;

            ULINEPAIR? trimmer1 = null;
            ULINEPAIR? trimmer2 = null;
            if (trimmers.Count == 1)
            {
                trimmer2 = trimmers[0];
            }
            else if (trimmers.Count == 2)
            {
                trimmer1 = trimmers[0];
                trimmer2 = trimmers[1];
            }

            int panelcount = panellines.Count;

          

            foreach (var pair in panellines)
            {
                int pid = panellines.IndexOf(pair) + 1;
                ULINE? left = pair.GetSide(uppSides.Left);
                ULINE? right = pair.GetSide(uppSides.Right);
                DowncomerInfo leftDC = null;
                DowncomerInfo rightDC = null;
                uopVectors assybpsites = bSetBPSites && assy != null ? BPSites(assy, null) : uopVectors.Zero;
                double panelX = Math.Round(pair.Value, 3);
             
                List<uopPanelSectionShape> panelsections = new List<uopPanelSectionShape>();

                if (left.HasValue)
                {
                    lineL = left.Value;
                    leftDC = Item(pid);
                }
                if (right.HasValue)
                {
                    lineR = right.Value;
                    rightDC = Item(pid - 1);
                }

                //create the full circle strip for the panel
                uopRectangle limits = new uopRectangle(aLeft :left.HasValue ? left.Value.X() : -rad -1, aTop: rad + 1, aRight: right.HasValue ? right.Value.X() : rad +1,aBottom:-rad -1);
                uopPanelSectionShape circlesec = new uopPanelSectionShape(uopShape.CircleSectionVertices(deckcircle, limits,aBisector:null, bBisector:null,bIncludeQuadrantPts:true),assy)
                {
                    LinePair = new uopLinePair(pair),
                    PanelIndex = pid,
                    PanelX = panelX,
                    PanelY = 0,
                    SectionIndex = 1,
                    BPSites = new uopVectors(assybpsites.FindAll(x => x.Row == pid)),
                    LeftDowncomerInfo = DowncomerInfo.CloneCopy(leftDC),
                    RightDowncomerInfo = DowncomerInfo.CloneCopy(rightDC),
                };
                if (DesignFamily.IsStandardDesignFamily())
                {
                    isvirtual = Math.Round(panelX, 2) < 0;

                    if (specialcase && !specialcasesatisfied && isvirtual)
                    {
                        isvirtual = false;
                        specialcasesatisfied = true;
                    }
                    if (isvirtual) continue;
                  
                    uopPanelSectionShape sectionshape = new uopPanelSectionShape(new uopVectors(circlesec.Vertices), assy)
                    {
                        LinePair = new uopLinePair(pair),
                        PanelIndex = pid,
                        SectionIndex = 1,
                        ParentShape = circlesec,
                        PanelX = panelX,
                        PanelY = 0,
                        IsVirtual = isvirtual,
                        Row =maxrow, 
                        Col =pid,
                        LeftDowncomerInfo = DowncomerInfo.CloneCopy(leftDC),
                        RightDowncomerInfo = DowncomerInfo.CloneCopy(rightDC),
                        BPSites = uopVectors.CloneCopy(circlesec.BPSites)
                            
                    };
                    _rVal.Add(sectionshape);

                }
                else if (DesignFamily.IsBeamDesignFamily())
                {
                    uppSides side = Math.Round(panelX, 2) < 0 ? uppSides.Left : uppSides.Right;
                    List<ULINEPAIR> leftboxpairs = leftDC != null  ? leftDC.BoxLns : new List<ULINEPAIR>();
                    List<ULINEPAIR> rightboxpairs = rightDC != null ? rightDC.BoxLns : new List<ULINEPAIR>();
                    

                    ULINEPAIR trimlines = trimmers[0];
                    uopPanelSectionShape subsec = null;
               
                    for (int r = maxrow; r >= 1; r--)
                    {

                        double xleft = left.HasValue ? left.Value.X() : -rad - 1;
                        double xright = right.HasValue ? right.Value.X() : rad + 1;
                        double ytop = rad + 1;
                        double ybot = -rad - 1;
                   
                        bool keep = true;
                     
                        bool moon = pid == 1 || pid == panellines.Count;
                        uopLine trimmertop = null;
                        uopLine trimmerbot = null;
                        isvirtual = r != maxrow;
                        uopVector bisector1testdir = null;
                        uopVector bisector2testdir = null;

                        ULINEPAIR leftboxpair = leftboxpairs.Find(x => x.Row == r);
                        ULINEPAIR rightboxpair = rightboxpairs.Find(x => x.Row == r);
                        ULINE? leftbox = leftboxpair.GetSide(uppSides.Right);
                        ULINE? rightbox = rightboxpair.GetSide(uppSides.Left);
                        if(!leftbox.HasValue && !rightbox.HasValue)
                                keep = false;

                        //to skip the short boxes
                        if (!leftbox.HasValue || (leftbox.HasValue && leftbox.Value.Length < mdGlobals.MinBoxLength))
                        {
                            xleft = -rad - 1;
                            leftbox = null;
                        }
                           
                        if (!rightbox.HasValue || (rightbox.HasValue && rightbox.Value.Length < mdGlobals.MinBoxLength))
                        {
                            xright = rad + 1;
                            rightbox = null;
                        }

                        if (r == maxrow) 
                            trimmertop = new uopLine(trimmers.Last().GetSideValue(uppSides.Bottom));
                        else if (r == maxrow -1)
                        {
                            if (maxrow == 3)
                            {
                                trimmertop = new uopLine(trimmers.First().GetSideValue(uppSides.Bottom));
                                trimmerbot = new uopLine(trimmers.Last().GetSideValue(uppSides.Top));
                            }
                            else
                            {
                                trimmerbot = new uopLine(trimmers.First().GetSideValue(uppSides.Top));
                            }

                        }
                        else if (r == 1)
                            trimmerbot = new uopLine(trimmers.First().GetSideValue(uppSides.Top));


                        if ((xleft == -rad - 1 && xright == rad + 1) || xleft >= xright)
                            keep = false;
                        bool divided = false;
          
                        if (trimmertop != null)
                        {
                            bisector1testdir = trimmertop.GetDirection(out _, -90);
                            uopVectors ips = trimmertop.Intersections(circlesec.Segments, bNoDupes: true);
                            if (ips.Count > 0)
                            {
                                ytop = ips.GetExtremeOrd(bMin: false, bGetY: true);
                                ips = trimmertop.Intersections(deckcircle);

                                if (side == uppSides.Left && xleft == -rad -1 && !moon)
                                {
                                   
                                  var lim = ips.GetExtremeOrd(bMin: true, bGetY: false);
                                    if (lim > xleft) xleft = lim;
                                }
                                else if (side == uppSides.Right && xright == rad + 1 && !moon)
                                {
                                    var lim = ips.GetExtremeOrd(bMin: false, bGetY: false);
                                    if (lim < xright) xright = lim;
                                }
                                    divided = true;
                            }
                        }
                        if (trimmerbot != null)
                        {
                            bisector2testdir = trimmerbot.GetDirection(out _, 90);
                            uopVectors ips = trimmerbot.Intersections(circlesec.Segments, bNoDupes: true);
                            if (ips.Count > 0)
                            {
                                ybot = ips.GetExtremeOrd(bMin: true, bGetY: true);
                                ips = trimmerbot.Intersections(deckcircle);

                                if (side == uppSides.Left && xleft == -rad - 1 && !moon)
                                {

                                    var lim = ips.GetExtremeOrd(bMin: true, bGetY: false);
                                    if (lim > xleft) xleft = lim;
                                }
                                else if (side == uppSides.Right && xright == rad + 1 && !moon)
                                {
                                    var lim = ips.GetExtremeOrd(bMin: false, bGetY: false);
                                    if (lim < xright) xright = lim;
                                }
                                divided = true;
                            }
                        }
                        if (maxrow > 2)
                        {
                            if (r == 1) isvirtual = true;
                            if (r == 2) isvirtual = Math.Round(panelX, 2) < 0;
                        }
                        if(!keep || isvirtual) 
                            continue;

                        uopRectangle sublimits = new uopRectangle(aLeft: xleft, aTop: ytop, aRight: xright, aBottom: ybot);
                        uopVectors verts = divided ? uopShape.CircleSectionVertices(deckcircle, sublimits, aBisector: trimmertop, bBisector: trimmerbot, bIncludeQuadrantPts: true, aBisector1TestDir: bisector1testdir, aBisector2TestDir: bisector2testdir) : new uopVectors(circlesec.Vertices);
                        uopRectangle panellimits = new uopRectangle(verts);

                        subsec = new uopPanelSectionShape(verts, assy)
                        {
                            LinePair = new uopLinePair(pair),
                            PanelIndex = pid,
                            ParentShape = circlesec,
                            PanelX = panelX,
                            PanelY = panellimits.Y,
                            SectionIndex =  r,
                            Row = r,
                            Col = pid,
                            BPSites = new uopVectors(assybpsites.FindAll(x => panellimits.ContainsVector(x))),
                            LeftDowncomerInfo = DowncomerInfo.CloneCopy(leftDC),
                            RightDowncomerInfo = DowncomerInfo.CloneCopy(rightDC),
                            IsVirtual = isvirtual
                        };
                         
                        panelsections.Add(subsec);
                        
                        
                    }  // loop on rows

                    int secindex = 0;
                    // so they are top down
                    panelsections.Reverse();
                    //set the indices and save to the return
                    foreach (var section in panelsections)
                    {
                        secindex++;
                        section.SectionIndex = secindex;
                        _rVal.Add(section);
                        
                    }


                } // end beam family

                
              
            } //panel loop
   
         uopPanelSectionShapes.SetMDPanelInstances(_rVal);

            if (bReturnVirtuals)
            {
                int cnt = _rVal.Count;
                for (int i = 1;  i <= cnt; i++)
                {
                    var panel = _rVal[i -1];
                    if(panel.OccuranceFactor > 1)
                    {
                        uopVectors verts = panel.Instances[0].ApplyTo(panel.Vertices, panel.Center);
                       panel = new uopPanelSectionShape(panel) { IsVirtual = true, Vertices = verts};
                        
                        _rVal.Add(panel);
                    }
                }
            }
         return _rVal;

        }

        /// <summary>
        /// creates the shapes that are the boundaries of circle sections divided by the current Divider(s)
        /// </summary>
        /// <param name="aClearance">the downcomer clearance to apply</param>
        /// <param name="aLap">the ring/beam lap to apply</param>
        /// <param name="rPanelLines">eturns the lines that are the left and right edges of the circle section based on the passed clearance</param>
        /// <param name="rWeirs">returns the weir lines for the panels</param>
        /// <param name="bReturnVirtuals">flag to return virtual panel shapes</param>
        /// <param name="bSetBPSites">flag to asssign bubble promoters sites to the return shapes  </param>
        /// <returns></returns>
        public uopPanelSectionShapes CreatePanelShapesOLD(double aClearance, double aLap, out List<uopLine> rPanelLines, out List<uopLinePair> rWeirs, bool bReturnVirtuals = false, bool bSetBPSites = false)
        {

            mdTrayAssembly assy = TrayAssembly;

            uopPanelSectionShapes _rVal = new uopPanelSectionShapes(assy,this);
          
            bool specialcase = ProjectType == uppProjectTypes.MDDraw && DesignFamily.IsStandardDesignFamily() && OddDowncomers && DowncomerCount > 1;
            bool specialcasesatisfied = false;

            rWeirs = uopLinePairs.FromULinePairs(CreateFBAWeirLines());
            double lap = aLap;
            double rad = RingRadius + lap;

         

            List<ULINEPAIR> trimmers = CreateDividerLns(rad, -lap);
            List<ULINEPAIR> panellines = CreatePanelLns(aRadius: rad, aInterceptors: uopLinePair.FromList(trimmers), aClearance: aClearance);
            rPanelLines = new List<uopLine>();

            ULINE lineL = ULINE.Null;
            ULINE lineR = ULINE.Null;
            int maxrow = MaxRow;

            ULINEPAIR? trimmer1 = null;
            ULINEPAIR? trimmer2 = null;
            if (trimmers.Count == 1)
            {
                trimmer2 = trimmers[0];
            }
            else if (trimmers.Count == 2)
            {
                trimmer1 = trimmers[0];
                trimmer2 = trimmers[1];
            }

      
            foreach (var pair in panellines)
            {
                bool vir1 = false;

                UVECTORS verts1 = UVECTORS.Zero;
                UVECTORS verts2 = UVECTORS.Zero;
                UVECTORS verts3 = UVECTORS.Zero;
                int pid = panellines.IndexOf(pair) + 1;

                double panelX = Math.Round(pair.Value, 3);
                ULINE? left = pair.GetSide(uppSides.Left);
                ULINE? right = pair.GetSide(uppSides.Right);
                ULINE? trimer = null;
                UVECTOR? u1 = null;
                UVECTOR? u2 = null;
                UVECTOR? u3 = null;
                UVECTOR? u4 = null;
                UVECTOR? u5 = null;
                int hitsL = 0;
                int hitsR = 0;
                DowncomerInfo leftDC = null;
                DowncomerInfo rightDC = null;
                uopVectors assybpsites =bSetBPSites && assy!=null ? BPSites(assy, null) : uopVectors.Zero ;
                   
                if (left.HasValue)
                {
                    lineL = left.Value;
                    lineL.Points.Sort(dxxSortOrders.TopToBottom);
                    hitsL = lineL.Points.Count;
                    leftDC = Item(pid);
                }
                if (right.HasValue)
                {
                    lineR = right.Value;
                    lineR.Points.Sort(dxxSortOrders.BottomToTop);
                    hitsR = lineR.Points.Count;
                    rightDC = Item(pid - 1);
                }
                uopRectangle limits = new uopRectangle(aLeft: left.HasValue ? left.Value.X() : -rad - 1, aTop: rad + 1, aRight: right.HasValue ? right.Value.X() : rad + 1, aBottom: -rad - 1);
                uopPanelSectionShape circlesec = new uopPanelSectionShape(uopShape.CircleSectionVertices(null, rad, limits), assy)
                {
                    LinePair = new uopLinePair(pair),
                    PanelIndex = pid,
                    PanelX = panelX,
                    PanelY = 0,
                    SectionIndex = 1,
                    BPSites = new uopVectors(assybpsites.FindAll(x => x.Row == pid)),
                    LeftDowncomerInfo = DowncomerInfo.CloneCopy(leftDC),
                    RightDowncomerInfo = DowncomerInfo.CloneCopy(rightDC),
                };

                if (panelX >= 0)
                {
                    if (hitsR == 2)
                    {
                        if (hitsL == 4)
                        {
                            lineL.Points.Remove(1);
                            lineL.Points.Remove(1);
                            hitsL -= 2;
                            lineL.sp = lineL.Points.Item(1);

                        }
                        else if (hitsL == 3)
                        {
                            lineL.Points.Remove(1);
                            hitsL -= 1;
                            lineL.sp = lineL.Points.Item(1);
                        }

                    }
                    else
                    {
                        if (hitsL == 2)
                        {
                            if (hitsR == 4)
                            {
                                lineR.Points.Remove(1);
                                lineR.Points.Remove(1);
                                hitsR -= 2;
                                lineL.sp = lineR.Points.Item(1);

                            }
                            else if (hitsR == 3)
                            {
                                lineR.Points.Remove(1);
                                hitsR -= 1;
                                lineR.sp = lineR.Points.Item(1);
                            }
                        }
                    }
                }

                if (left.HasValue && !right.HasValue) // a right side moon
                {
                    // a right side moon

                    rPanelLines.Add(new uopLine(lineL));
                    if (hitsL == 2)
                    {
                        u1 = new UVECTOR(lineL.Points.Item(1), aRadius: 0);
                        u2 = new UVECTOR(lineL.Points.Item(2), aRadius: rad);
                        u3 = new UVECTOR(rad, 0, aRadius: rad);
                        verts1 = new UVECTORS(u1, u2, u3, u4);

                    }
                    else if (hitsL == 3)
                    {
                        u1 = new UVECTOR(lineL.Points.Item(2), aRadius: 0);
                        u2 = new UVECTOR(lineL.Points.Item(3), aRadius: rad);
                        bool quadpt = u1.Value.Y > 0 && u2.Value.Y < 0;
                        if (quadpt) u3 = new UVECTOR(rad, 0, aRadius: rad);

                        if (trimmer2.HasValue)
                        {
                            trimer = trimmer2.Value.GetSide(uppSides.Bottom);

                            if (trimer.HasValue)
                            {
                                if (quadpt)
                                    u4 = new UVECTOR(trimer.Value.EndPts.Find((x) => x.X > 0), aRadius: 0);
                                else
                                    u3 = new UVECTOR(trimer.Value.EndPts.Find((x) => x.X > 0), aRadius: 0);
                            }
                        }

                        verts1 = new UVECTORS(u1, u2, u3, u4);

                    }
                    else if (hitsL == 4)
                    {
                        u1 = new UVECTOR(lineL.Points.Item(1), aRadius: 0);
                        u2 = new UVECTOR(lineL.Points.Item(2), aRadius: 0);
                        trimer = trimmer2.HasValue ? trimmer2.Value.GetSide(uppSides.Top) : null;
                        u3 = (trimer.HasValue) ? new UVECTOR(trimer.Value.EndPts.Find((x) => x.X > 0), aRadius: rad) : new UVECTOR(Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(u2.Value.Y, 2)), u2.Value.Y, aRadius: rad);
                        bool quadpt = u3.Value.Y < 0 && u1.Value.Y > 0;
                        if (quadpt)
                        {
                            u4 = u3;
                            u3 = new UVECTOR(rad, 0, aRadius: rad);
                        }

                        verts1 = new UVECTORS(u1, u2, u3, u4);

                        u1 = new UVECTOR(lineL.Points.Item(3), aRadius: 0);
                        u2 = new UVECTOR(lineL.Points.Item(4), aRadius: rad);
                        u3 = null;
                        u4 = null;
                        trimer = trimmer2.HasValue ? trimmer2.Value.GetSide(uppSides.Bottom) : null;
                        u3 = (trimer.HasValue) ? new UVECTOR(trimer.Value.EndPts.Find((x) => x.X > 0), aRadius: 0) : new UVECTOR(Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(u2.Value.Y, 2)), u2.Value.Y, aRadius: rad);
                        quadpt = u3.Value.Y > 0 && u2.Value.Y < 0;
                        if (quadpt)
                        {
                            u4 = u3;
                            u3 = new UVECTOR(rad, 0, aRadius: rad);
                        }

                        verts2 = new UVECTORS(u1, u2, u3, u4);

                    }
                }// a right side moon
                else if (!left.HasValue && right.HasValue)  // a left side moon
                {


                    rPanelLines.Add(new uopLine(lineR));
                    if (hitsR == 2)
                    {
                        u1 = new UVECTOR(lineR.Points.Item(2), aRadius: rad);
                        u2 = new UVECTOR(-rad, 0, aRadius: rad);
                        u3 = new UVECTOR(lineR.Points.Item(1), aRadius: 0);
                        verts1 = new UVECTORS(u1, u2, u3, u4);

                    }
                    else if (hitsR == 3)
                    {
                        u1 = new UVECTOR(lineR.Points.Item(2), aRadius: 0);
                        u2 = new UVECTOR(lineR.Points.Item(1), aRadius: rad);
                        bool quadpt = u1.Value.Y > 0 && u2.Value.Y < 0;
                        if (quadpt) u3 = new UVECTOR(-rad, 0, aRadius: rad);
                        trimer = trimmer1.Value.GetSide(uppSides.Top);

                        if (trimer.HasValue)
                        {
                            if (quadpt)
                                u4 = new UVECTOR(trimer.Value.EndPts.Find((x) => x.X < 0), aRadius: 0);
                            else
                                u3 = new UVECTOR(trimer.Value.EndPts.Find((x) => x.X < 0), aRadius: 0);
                        }
                        verts1 = new UVECTORS(u1, u2, u3, u4);

                    }
                    else if (hitsR == 4)
                    {
                        u1 = new UVECTOR(lineL.Points.Item(2), aRadius: 0);
                        u2 = new UVECTOR(lineL.Points.Item(1), aRadius: rad);
                        trimer = trimmer1.Value.GetSide(uppSides.Top);

                        u3 = (trimer.HasValue) ? new UVECTOR(trimer.Value.EndPts.Find((x) => x.X < 0), aRadius: 0) : new UVECTOR(-Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(u1.Value.Y, 2)), u1.Value.Y, aRadius: rad);
                        bool quadpt = u3.Value.Y < 0 && u2.Value.Y > 0;
                        if (quadpt)
                        {
                            u4 = u3;
                            u3 = new UVECTOR(-rad, 0, aRadius: rad);
                        }

                        verts1 = new UVECTORS(u1, u2, u3, u4);

                        u1 = new UVECTOR(lineL.Points.Item(4), aRadius: 0);
                        u2 = new UVECTOR(lineL.Points.Item(3), aRadius: 0);
                        u3 = null;
                        u4 = null;
                        trimer = trimmer1.Value.GetSide(uppSides.Bottom);
                        u3 = (trimer.HasValue) ? new UVECTOR(trimer.Value.EndPts.Find((x) => x.X < 0), aRadius: rad) : new UVECTOR(-Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(u2.Value.Y, 2)), u2.Value.Y, aRadius: rad);
                        quadpt = u3.Value.Y > 0 && u1.Value.Y < 0;
                        if (quadpt)
                        {
                            u4 = u3;
                            u3 = new UVECTOR(-rad, 0, aRadius: rad);
                        }
                        verts2 = new UVECTORS(u1, u2, u3, u4);

                    }
                } // a left side moon
                else if (left.HasValue && right.HasValue) // a central panel
                {


                    rPanelLines.Add(new uopLine(lineL));
                    rPanelLines.Add(new uopLine(lineR));
                    if (hitsL <= 2 && lineR.Points.Count <= 2) // a full panel
                    {
                        bool trimmed = false;

                        if (panelX >= 0)
                        {
                            u1 = new UVECTOR(lineL.Points.Item(1), aRadius: 0);
                            if (u1.Value.Length() < Math.Round(rad, 4))
                            {

                                u2 = new UVECTOR(lineL.Points.Item(2), aRadius: rad);
                                u3 = new UVECTOR(lineR.Points.Item(1), aRadius: 0);
                                u4 = new UVECTOR(lineR.Points.Item(2), aRadius: rad);
                                if (trimmer2.HasValue)
                                {
                                    trimer = trimmer2.Value.GetSide(uppSides.Bottom);
                                    trimmed = trimer.HasValue;
                                    if (trimmed) u5 = new UVECTOR(trimer.Value.EndPts.Find((x) => x.X > 0), aRadius: 0);

                                }

                            }
                            if (!trimmed)
                            {
                                u2 = new UVECTOR(lineL.Points.Item(2), aRadius: rad);
                                u3 = new UVECTOR(lineR.Points.Item(1), aRadius: 0);
                                u4 = new UVECTOR(lineR.Points.Item(2), aRadius: rad);
                                u5 = null;
                            }
                        }
                        else
                        {
                            u1 = new UVECTOR(lineR.Points.Item(1), aRadius: 0);

                            if (u1.Value.Length() < Math.Round(rad, 4))
                            {

                                u2 = new UVECTOR(lineR.Points.Item(2), aRadius: rad);
                                u3 = new UVECTOR(lineL.Points.Item(1), aRadius: 0);
                                u4 = new UVECTOR(lineL.Points.Item(2), aRadius: rad);
                                if (trimmer2.HasValue)
                                {
                                    trimer = trimmer2.Value.GetSide(uppSides.Top);
                                    trimmed = trimer.HasValue;

                                }
                                if (trimmed) u5 = new UVECTOR(trimer.Value.EndPts.Find((x) => x.X < 0), aRadius: 0);
                            }
                            if (!trimmed)
                            {

                                u2 = new UVECTOR(lineR.Points.Item(2), aRadius: rad);
                                u3 = new UVECTOR(lineL.Points.Item(1), aRadius: 0);
                                u4 = new UVECTOR(lineL.Points.Item(2), aRadius: rad);
                                u5 = null;
                            }
                        }



                        verts1 = new UVECTORS(u1, u2, u3, u4, u5);

                    }// a full panel
                    else if (hitsL == 4 && hitsR == 4) // a full panel divided once
                    {
                        u1 = new UVECTOR(lineL.Points.Item(1), aRadius: 0);
                        u2 = new UVECTOR(lineL.Points.Item(2), aRadius: 0);
                        u3 = new UVECTOR(lineR.Points.Item(3), aRadius: 0);
                        u4 = new UVECTOR(lineR.Points.Item(4), aRadius: rad);
                        bool quadpt = u1.Value.X < 0 && u3.Value.X > 0;
                        if (quadpt) u5 = new UVECTOR(0, rad, aRadius: rad);

                        verts1 = new UVECTORS(u1, u2, u3, u4, u5);

                        u1 = new UVECTOR(lineL.Points.Item(3), aRadius: 0);
                        u2 = new UVECTOR(lineL.Points.Item(4), aRadius: rad);
                        if (quadpt)
                        {
                            u3 = new UVECTOR(0, -rad, aRadius: rad);
                            u4 = new UVECTOR(lineR.Points.Item(1), aRadius: 0);
                            u5 = new UVECTOR(lineR.Points.Item(2), aRadius: 0);
                        }
                        else
                        {
                            u3 = new UVECTOR(lineR.Points.Item(1), aRadius: 0);
                            u4 = new UVECTOR(lineR.Points.Item(2), aRadius: 0);
                        }


                        verts2 = new UVECTORS(u1, u2, u3, u4, u5);
                    }// a full panel divided once
                    else if (hitsL == 6 && hitsR == 6) // a full panel divided twice
                    {
                        u1 = new UVECTOR(lineL.Points.Item(1), aRadius: 0);
                        u2 = new UVECTOR(lineL.Points.Item(2), aRadius: 0);
                        u3 = new UVECTOR(lineR.Points.Item(5), aRadius: 0);
                        u4 = new UVECTOR(lineR.Points.Item(6), aRadius: rad);
                        bool quadpt = u1.Value.X < 0 && u3.Value.X > 0;
                        if (quadpt) u5 = new UVECTOR(0, rad, aRadius: rad);

                        verts1 = new UVECTORS(u1, u2, u3, u4, u5);

                        u1 = new UVECTOR(lineL.Points.Item(3), aRadius: 0);
                        u2 = new UVECTOR(lineL.Points.Item(4), aRadius: 0);
                        u3 = new UVECTOR(lineR.Points.Item(3), aRadius: 0);
                        u4 = new UVECTOR(lineR.Points.Item(4), aRadius: 0);
                        u5 = null;


                        verts2 = new UVECTORS(u1, u2, u3, u4, u5);


                        u1 = new UVECTOR(lineL.Points.Item(5), aRadius: 0);
                        u2 = new UVECTOR(lineL.Points.Item(6), aRadius: rad);
                        if (quadpt)
                        {
                            u3 = new UVECTOR(0, -rad, aRadius: rad);
                            u4 = new UVECTOR(lineR.Points.Item(1), aRadius: 0);
                            u5 = new UVECTOR(lineR.Points.Item(2), aRadius: 0);
                        }
                        else
                        {
                            u3 = new UVECTOR(lineR.Points.Item(1), aRadius: 0);
                            u4 = new UVECTOR(lineR.Points.Item(2), aRadius: 0);
                        }


                        verts3 = new UVECTORS(u1, u2, u3, u4, u5);
                    }/// a full panel divided twice
                    else if (hitsL == 3 && hitsR == 2) // single panel intersect the bottom divider only on the left
                    {

                        u1 = new UVECTOR(lineL.Points.Item(2), aRadius: 0);
                        u2 = new UVECTOR(lineL.Points.Item(3), aRadius: rad);
                        u3 = new UVECTOR(lineR.Points.Item(1), aRadius: 0);
                        u4 = new UVECTOR(lineR.Points.Item(2), aRadius: rad);
                        u5 = new UVECTOR(Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(u1.Value.X, 2)), u1.Value.Y, aRadius: 0);

                        if (trimmer2.HasValue)
                        {
                            trimer = trimmer2.Value.GetSide(uppSides.Bottom);
                            if (trimer.HasValue) u5 = new UVECTOR(trimer.Value.EndPts.Find((x) => x.X > 0), aRadius: 0);
                        }
                        verts1 = new UVECTORS(u1, u2, u3, u4, u5);

                    } // single panel intersect the bottom divider only on the left
                    else if (hitsL == 2 && hitsR == 3) // single panel intersect the bottom divider only on the right
                    {

                        u1 = new UVECTOR(lineL.Points.Item(1), aRadius: 0);
                        u2 = new UVECTOR(lineL.Points.Item(2), aRadius: rad);
                        u4 = new UVECTOR(lineR.Points.Item(2), aRadius: 0);
                        trimer = trimmer2.HasValue ? trimmer2.Value.GetSide(uppSides.Top) : null;
                        u3 = new UVECTOR(Math.Sqrt(Math.Pow(rad, 2) - Math.Pow(u4.Value.X, 2)), u4.Value.Y, aRadius: 0);
                        if (trimer.HasValue) u3 = new UVECTOR(trimer.Value.EndPts.Find((x) => x.X < 0), aRadius: 0);
                        u5 = new UVECTOR(lineR.Points.Item(3), aRadius: rad);
                        verts1 = new UVECTORS(u1, u2, u3, u4, u5);

                    } // single panel intersect the bottom divider only on the right
                    else if (hitsL == 6 && hitsR == 5) // double panel intersect the top divider twice  on the left, once on the right & the bottom divider twice
                    {

                        u1 = new UVECTOR(lineL.Points.Item(1), aRadius: 0);
                        u2 = new UVECTOR(lineL.Points.Item(2), aRadius: 0);
                        trimer = trimmer1.Value.GetSide(uppSides.Top);
                        if (trimer.HasValue) u3 = new UVECTOR(trimer.Value.EndPts.Find((x) => x.X > 0), aRadius: rad);
                        verts1 = new UVECTORS(u1, u2, u3, u4, u5);

                        u1 = new UVECTOR(lineL.Points.Item(3), aRadius: 0);
                        u2 = new UVECTOR(lineL.Points.Item(4), aRadius: 0);
                        u3 = new UVECTOR(lineR.Points.Item(3), aRadius: 0);
                        u4 = new UVECTOR(lineR.Points.Item(4), aRadius: 0);
                        verts2 = new UVECTORS(u1, u2, u3, u4, u5);

                        u1 = new UVECTOR(lineL.Points.Item(5), aRadius: 0);
                        u2 = new UVECTOR(lineL.Points.Item(6), aRadius: rad);
                        u3 = new UVECTOR(lineR.Points.Item(1), aRadius: 0);
                        u4 = new UVECTOR(lineR.Points.Item(2), aRadius: 0);
                        verts3 = new UVECTORS(u1, u2, u3, u4, u5);


                    }// double panel intersect the top divider twice  on the left, once on the right & the bottom divider twice
                    else if (hitsL == 5 && hitsR == 6) // double panel intersect the bottom divider twice  on the right, once on the left & the top divider twice
                    {

                        u1 = new UVECTOR(lineL.Points.Item(1), aRadius: 0);
                        u2 = new UVECTOR(lineL.Points.Item(2), aRadius: 0);
                        u3 = new UVECTOR(lineR.Points.Item(5), aRadius: 0);
                        u4 = new UVECTOR(lineR.Points.Item(6), aRadius: rad);
                        verts1 = new UVECTORS(u1, u2, u3, u4, u5);

                        u1 = new UVECTOR(lineL.Points.Item(3), aRadius: 0);
                        u2 = new UVECTOR(lineL.Points.Item(4), aRadius: 0);
                        u3 = new UVECTOR(lineR.Points.Item(3), aRadius: 0);
                        u4 = new UVECTOR(lineR.Points.Item(4), aRadius: 0);
                        verts2 = new UVECTORS(u1, u2, u3, u4, u5);


                        u1 = new UVECTOR(lineR.Points.Item(1), aRadius: 0);
                        u2 = new UVECTOR(lineR.Points.Item(2), aRadius: 0);
                        u3 = null;
                        u4 = null;
                        if (trimmer2.HasValue)
                        {
                            trimer = trimmer2.Value.GetSide(uppSides.Bottom);
                            if (trimer.HasValue) u3 = new UVECTOR(trimer.Value.EndPts.Find((x) => x.X < 0), aRadius: rad);

                        }

                        verts3 = new UVECTORS(u1, u2, u3, u4, u5);


                    }// double panel intersect the bottom divider twice  on the right, once on the left & the top divider twice
                    else if (hitsL == 6 && hitsR == 4) // double panel intersect the top divider twice  on the left & the bottom divider twice on the right
                    {

                        u1 = new UVECTOR(lineL.Points.Item(1), aRadius: 0);
                        u2 = new UVECTOR(lineL.Points.Item(2), aRadius: 0);
                        trimer = trimmer1.Value.GetSide(uppSides.Top);
                        if (trimer.HasValue) u3 = new UVECTOR(trimer.Value.EndPts.Find((x) => x.X > 0), aRadius: rad);
                        verts1 = new UVECTORS(u1, u2, u3, u4, u5);

                        u1 = new UVECTOR(lineL.Points.Item(3), aRadius: 0);
                        u2 = new UVECTOR(lineL.Points.Item(4), aRadius: 0);
                        u3 = new UVECTOR(lineR.Points.Item(3), aRadius: 0);
                        u4 = new UVECTOR(lineR.Points.Item(4), aRadius: rad);
                        trimer = trimmer1.Value.GetSide(uppSides.Bottom);
                        if (trimer.HasValue) u5 = new UVECTOR(trimer.Value.EndPts.Find((x) => x.X > 0), aRadius: 0);
                        verts2 = new UVECTORS(u1, u2, u3, u4, u5);

                        u1 = new UVECTOR(lineL.Points.Item(5), aRadius: 0);
                        u2 = new UVECTOR(lineL.Points.Item(6), aRadius: rad);
                        u3 = new UVECTOR(lineR.Points.Item(1), aRadius: 0);
                        u4 = new UVECTOR(lineR.Points.Item(2), aRadius: 0);
                        u5 = null;
                        verts3 = new UVECTORS(u1, u2, u3, u4, u5);


                    }// double panel intersect the top divider twice  on the left & the bottom divider twice on the right
                    else if (hitsL == 4 && hitsR == 6) // double panel intersect the bottom divider once  on the right & the top divider twice on the left
                    {

                        u1 = new UVECTOR(lineL.Points.Item(1), aRadius: 0);
                        u2 = new UVECTOR(lineL.Points.Item(2), aRadius: 0);
                        u3 = new UVECTOR(lineR.Points.Item(5), aRadius: 0);
                        u4 = new UVECTOR(lineR.Points.Item(6), aRadius: rad);
                        verts1 = new UVECTORS(u1, u2, u3, u4, u5);

                        u1 = new UVECTOR(lineL.Points.Item(3), aRadius: 0);
                        u2 = new UVECTOR(lineL.Points.Item(4), aRadius: 0);
                        u3 = null;
                        u4 = new UVECTOR(lineR.Points.Item(3), aRadius: 0);
                        u5 = new UVECTOR(lineR.Points.Item(4), aRadius: 0);
                        trimer = trimmer2.HasValue ? trimmer2.Value.GetSide(uppSides.Top) : null;
                        if (trimer.HasValue) u3 = new UVECTOR(trimer.Value.EndPts.Find((x) => x.X < 0), aRadius: 0);

                        verts2 = new UVECTORS(u1, u2, u3, u4, u5);

                        u1 = new UVECTOR(lineR.Points.Item(1), aRadius: 0);
                        u2 = new UVECTOR(lineR.Points.Item(2), aRadius: 0);
                        u3 = null;
                        trimer = trimmer2.HasValue ? trimmer2.Value.GetSide(uppSides.Bottom) : null;
                        if (trimer.HasValue) u3 = new UVECTOR(trimer.Value.EndPts.Find((x) => x.X < 0), aRadius: 0);
                        u4 = null;
                        u5 = null;
                        verts3 = new UVECTORS(u1, u2, u3, u4, u5);


                    }
                    else if (hitsL == 5 && hitsR == 4) // double panel intersect the bottom divider twice  on the left & the top divider once on the left
                    {

                        u1 = new UVECTOR(lineL.Points.Item(1), aRadius: 0);
                        u2 = new UVECTOR(lineL.Points.Item(3), aRadius: 0);
                        u3 = new UVECTOR(lineR.Points.Item(3), aRadius: 0);
                        u4 = new UVECTOR(lineR.Points.Item(4), aRadius: rad);
                        //u5 = null;
                        //trimer = trimmer1.HasValue ? trimmer1.Value.GetSide(uppSides.Bottom) : null;
                        //if (trimer.HasValue) u5 = new UVECTOR(trimer.Value.EndPts.Find((x) => x.X > 0), aRadius: 0);
                        verts1 = new UVECTORS(u1, u2, u3, u4, u5);

                        u1 = new UVECTOR(lineL.Points.Item(4), aRadius: 0);
                        u2 = new UVECTOR(lineL.Points.Item(5), aRadius: rad);
                        u3 = null;
                        u4 = new UVECTOR(lineR.Points.Item(1), aRadius: 0);
                        u5 = new UVECTOR(lineR.Points.Item(2), aRadius: 0);

                        verts2 = new UVECTORS(u1, u2, u3, u4, u5);

                        //u1 = new UVECTOR(lineR.Points.Item(1), aRadius: 0);
                        //u2 = new UVECTOR(lineR.Points.Item(2), aRadius: 0);
                        //u3 = null;
                        //trimer = trimmer2.HasValue ? trimmer2.Value.GetSide(uppSides.Bottom) : null;
                        //if (trimer.HasValue) u3 = new UVECTOR(trimer.Value.EndPts.Find((x) => x.X < 0), aRadius: 0);
                        //u4 = null;
                        //u5 = null;
                        //verts3 = new UVECTORS(u1, u2, u3, u4, u5);


                    }
                }// a central panel


                // create 1,2 or 3 shapes
                int segid = 0;
                USHAPE? shape1 = null;
                USHAPE? shape2 = null;
                USHAPE? shape3 = null;

                int col = pid;
                if (verts1.Count > 2)
                {
                    segid++;
                    shape1 = new USHAPE(verts1, aName: $"PANEL_{pid}_{segid}", bIsVirtual: vir1) { Row = maxrow, Col = col, PartIndex = pid, Value = (double)segid };
                }
                if (verts2.Count > 2)
                {
                    segid++;
                    shape2 = new USHAPE(verts2, aName: $"PANEL_{pid}_{segid}", bIsVirtual: vir1) { Row = maxrow, Col = col, PartIndex = pid, Value = (double)segid };
                }

                if (verts3.Count > 2)
                {
                    segid++;
                    shape3 = new USHAPE(verts3, aName: $"PANEL_{pid}_{segid}", bIsVirtual: vir1) { Row = maxrow, Col = col, PartIndex = pid, Value = (double)segid };
                }

                USHAPE shp;
                if (maxrow == 1) // no divider
                {
                    shp = shape1.Value; shp.Row = 1; shp.IsVirtual = panelX < 0; shp.Value = panelX; shape1 = shp;

                }
                else if (maxrow == 2) // one beam or a wall
                {

                    if (shape1.HasValue && shape2.HasValue)
                    {
                        shp = shape1.Value; shp.Row = 1; shp.IsVirtual = true; shp.Value = panelX; shape1 = shp;
                        shp = shape2.Value; shp.Row = 2; shp.IsVirtual = false; shp.Value = panelX; shape2 = shp;
                    }
                    else if (shape1.HasValue && !shape2.HasValue)
                    {
                        shp = shape1.Value; shp.Row = panelX > 0 ? 2 : 1; shp.IsVirtual = panelX < 0; shp.Value = panelX; shape1 = shp;

                    }

                }
                else if (maxrow == 3) //two beams
                {

                    if (shape1.HasValue && shape2.HasValue && shape3.HasValue)
                    {
                        shp = shape1.Value; shp.Row = 1; shp.IsVirtual = true; shp.Value = panelX; shape1 = shp;
                        shp = shape2.Value; shp.Row = 2; shp.IsVirtual = panelX < 0; shp.Value = panelX; shape2 = shp;
                        shp = shape3.Value; shp.Row = 3; shp.IsVirtual = false; shp.Value = panelX; shape3 = shp;
                    }
                    else if (shape1.HasValue && shape2.HasValue && !shape3.HasValue)
                    {
                        shp = shape1.Value; shp.Row = panelX >= 0 ? 2 : 1; shp.IsVirtual = panelX < 0; shp.Value = panelX; shape1 = shp;
                        shp = shape2.Value; shp.Row = panelX >= 0 ? 3 : 2; shp.IsVirtual = panelX < 0; shp.Value = panelX; shape2 = shp;
                    }
                    else if (shape1.HasValue && !shape2.HasValue && !shape3.HasValue)
                    {
                        shp = shape1.Value; shp.Row = panelX >= 0 ? 3 : 1; shp.IsVirtual = panelX < 0; shp.Value = panelX; shape1 = shp;
                    }

                }

                if (shape1.HasValue)
                {
                    shp = shape1.Value;
                    if (specialcase && !specialcasesatisfied && shp.IsVirtual)
                    {
                        shp.IsVirtual = false;
                        specialcasesatisfied = true;
                        shape1 = shp;
                    }
                }
                else
                {
                    continue;
                }

                uopPanelSectionShapes panelsubshapes = new uopPanelSectionShapes(assy, this);
                //save the target shapes
                if (shape1.HasValue && (bReturnVirtuals || (!bReturnVirtuals && !shape1.Value.IsVirtual)))
                {
                    USHAPE shape = shape1.Value;
                    int weirid = rWeirs.FindIndex((x) => x.Col == shape.Col && x.Row == shape.Row);
                    if (weirid >= 0)
                    {
                        uopLinePair weirs = new uopLinePair(rWeirs.Find((x) => x.Col == shape.Col && x.Row == shape.Row));
                        shape.LinePair = new ULINEPAIR(weirs);
                        if (!DesignFamily.IsStandardDesignFamily())
                        {
                            //fix shapes that have only one weir and are not moons
                            shape = FixPanelShapeWeirs(shape, rad, Count + 1, weirs, maxrow, trimmers);
                        }

                        uopPanelSectionShape newshape = new uopPanelSectionShape(new uopShape(shape),assy)
                        {
                            PanelIndex = pid,
                            SectionIndex = shape.Row,
                            ParentShape = circlesec,
                            PanelX = panelX,
                            PanelY = shape.Y,
                            LeftDowncomerInfo = DowncomerInfo.CloneCopy(leftDC),
                            RightDowncomerInfo = DowncomerInfo.CloneCopy(rightDC)
                        };
                        newshape.BPSites = new uopVectors(assybpsites.FindAll(x => x.Row == pid && newshape.BoundsV.ContainsOrd(x.Y,bOrdIsY:true, bOnIsIn:false)));
                        _rVal.Add(newshape);
                        panelsubshapes.Add(newshape);
                    }


                }
                if (shape2.HasValue && (bReturnVirtuals || (!bReturnVirtuals && !shape2.Value.IsVirtual)))
                {
                    USHAPE shape = shape2.Value;
                    int weirid = rWeirs.FindIndex((x) => x.Col == shape.Col && x.Row == shape.Row);
                    if (weirid >= 0)
                    {
                        uopLinePair weirs = new uopLinePair(rWeirs.Find((x) => x.Col == shape.Col && x.Row == shape.Row));
                        shape.LinePair = new ULINEPAIR(weirs);
                        if (!DesignFamily.IsStandardDesignFamily())
                        {
                            //fix shapes that have only one weir and are not moons
                            shape = FixPanelShapeWeirs(shape, rad, Count + 1, weirs, maxrow, trimmers);
                        }

                        uopPanelSectionShape newshape = new uopPanelSectionShape(new uopShape(shape), assy)
                        {
                            PanelIndex = shape.PartIndex,
                            SectionIndex = (int)shape.Value,
                            ParentShape = circlesec,
                            PanelX = panelX,
                            PanelY = shape.Y,
                            LeftDowncomerInfo = DowncomerInfo.CloneCopy(leftDC),
                            RightDowncomerInfo = DowncomerInfo.CloneCopy(rightDC)
                        };
                        newshape.BPSites = new uopVectors(assybpsites.FindAll(x => x.Row == pid && newshape.BoundsV.ContainsOrd(x.Y, bOrdIsY: true, bOnIsIn: false)));
                        _rVal.Add(newshape);
                        panelsubshapes.Add(newshape);
                    }

                }
                if (shape3.HasValue && (bReturnVirtuals || (!bReturnVirtuals && !shape3.Value.IsVirtual)))
                {
                    USHAPE shape = shape3.Value;
                    int weirid = rWeirs.FindIndex((x) => x.Col == shape.Col && x.Row == shape.Row);
                    if (weirid >= 0)
                    {
                        uopLinePair weirs = new uopLinePair(rWeirs.Find((x) => x.Col == shape.Col && x.Row == shape.Row));
                        shape.LinePair = new ULINEPAIR(weirs);
                        if (!DesignFamily.IsStandardDesignFamily())
                        {
                            //fix shapes that have only one weir and are not moons
                            shape = FixPanelShapeWeirs(shape, rad, Count + 1, weirs, maxrow, trimmers);
                        }

                        uopPanelSectionShape newshape = new uopPanelSectionShape(new uopShape(shape), assy)
                        {
                            PanelIndex = shape.PartIndex,
                            SectionIndex = (int)shape.Value,
                            ParentShape = circlesec,
                            PanelX = panelX,
                            PanelY = shape.Y,
                            LeftDowncomerInfo = DowncomerInfo.CloneCopy(leftDC),
                            RightDowncomerInfo = DowncomerInfo.CloneCopy(rightDC)
                        };
                        newshape.BPSites = new uopVectors(assybpsites.FindAll(x => x.Row == pid && newshape.BoundsV.ContainsOrd(x.Y, bOrdIsY: true, bOnIsIn: false)));
                        _rVal.Add(newshape);
                        panelsubshapes.Add(newshape);
                    }

                }

            } //panel loop

            uopPanelSectionShapes.SetMDPanelInstances(_rVal);


            return _rVal;

        }

        private USHAPE FixPanelShapeWeirs(USHAPE shape, double rad, int pcnt, uopLinePair weirs, int maxrow, List<ULINEPAIR> trimmers)
        {
            int pid = shape.PartIndex;
            double panelX = shape.Value;

            if (pid < pcnt && pid > 1)
            {

                uopLinePair trimmer;
                uopLine trimL;
                uopVector u1;
                uopVector u2;
                uopVector u3;
                uopVectors bverts;

                if (panelX < 0)
                {
                    if (weirs.SideIsSuppressed(uppSides.Left, aUndefinedValue: true))
                    {
                        // panel x less than zero and no weir on the left
                        trimmer = shape.Row == maxrow ? new uopLinePair(trimmers.Last()) : new uopLinePair(trimmers.First());
                        trimL = trimmer.GetSide(uppSides.Bottom);
                        u1 = new uopVector(trimL.EndPoints().GetVector(dxxPointFilters.AtMinX)) { Radius = rad };
                        bverts = shape.Vertexes;
                        u2 = bverts.GetVector(dxxPointFilters.GetRightBottom);
                        u3 = bverts.GetVector(dxxPointFilters.GetRightTop);
                        shape.Vertexes = new uopVectors(u1, u2, u3);
                        shape.LinePair.Line1 = null;
                    }

                }
                else
                {

                    if (weirs.SideIsSuppressed(uppSides.Right, aUndefinedValue: true))
                    {
                        // panel x greater than zero and no weir on the right
                        trimmer = shape.Row == maxrow - 1 ? new uopLinePair(trimmers.Last()) : new uopLinePair(trimmers.First());
                        trimL = trimmer.GetSide(uppSides.Top);
                        u1 = new uopVector(trimL.EndPoints().GetVector(dxxPointFilters.AtMaxX)) { Radius = rad };
                        bverts = shape.Vertexes;
                        u2 = bverts.GetVector(dxxPointFilters.GetLeftTop);
                        u3 = bverts.GetVector(dxxPointFilters.GetLeftBottom);
                        shape.Vertexes = new uopVectors(u2, u3, u1);
                        shape.LinePair.Line2 = null;
                    }
                }
                shape.LinePair = new ULINEPAIR(weirs);
            }
            return shape;
        }
        /// <summary>
        /// creates the shapes that are the boundaries of circle sections divided by the current Divider(s)
        /// </summary>
        /// <returns></returns>

        public List<uopLinePair> DividerLines(double? aTrimRadius = null, double? aOffset = null) => uopLinePair.FromList(CreateDividerLns(RingRadius, aOffset));

        internal List<ULINEPAIR> CreateDividerLns(double? aTrimRadius = null, double? aOffset = null)
        {
            if (DesignFamily.IsStandardDesignFamily()) return new List<ULINEPAIR>();

            List<ULINEPAIR> _rVal = new List<ULINEPAIR>();
   
            double rad = aTrimRadius.HasValue ? aTrimRadius.Value : RingRadius;
            double offset = !aOffset.HasValue ? 0 : aOffset.Value;
            double space = Spacing;
            double wd = BoxWidth / 2;

            if (rad <= 0 || space <= 0) return _rVal;

            UARC trimmer = new UARC(UVECTOR.Zero, rad);
            ULINE? l1;
            ULINE? l2;
            dxfPlane dvplane = Divider.Plane;
            double oset = Divider.Offset;
            double setoff = 0.5 * Divider.Width;

            if (DesignFamily.IsBeamDesignFamily())
            {
                l1 = new ULINE(dvplane, 2 * rad, 0, setoff + offset, aTrimArc: trimmer) { Side = uppSides.Top }; // the top of the top divider
                l2 = new ULINE(dvplane, 2 * rad, 0, -(setoff + offset), aTrimArc: trimmer) { Side = uppSides.Bottom }; // the bottom of the top divider

                _rVal.Add(new ULINEPAIR(l1, l2, aTag: $"DIVIDER_1"));
                if (Divider.Offset != 0)
                {
                    dvplane.Project(dvplane.YDirection, -2 * Divider.Offset);
                    l1 = new ULINE(dvplane, 2 * rad, 0, setoff + offset, aTrimArc: trimmer) { Side = uppSides.Top }; // the top of the bottom divider
                    l2 = new ULINE(dvplane, 2 * rad, 0, -(setoff + offset), aTrimArc: trimmer) { Side = uppSides.Bottom }; // the bottom of the bottom divider
                    _rVal.Add(new ULINEPAIR(l1, l2, aTag: $"DIVIDER_2"));
                }

            }
            return _rVal;
        }

        internal List<ULINEPAIR> CreateFBAWeirLines()
        {

            List<ULINEPAIR> _rVal = new List<ULINEPAIR>();
            List<ULINEPAIR> allweirs = new List<ULINEPAIR>();
            foreach (var dcinfo in this)
            {
                List<ULINEPAIR> dcweirs = dcinfo.CreateWeirLines(bIncludeSuppressed: false, aOffset: 0);
                allweirs.AddRange(uopLinePairs.Copy(dcweirs));
            }
           
            double offset = (Thickness + 0.25);

            int maxrow = MaxRow;

            for (int dpi = Count + 1; dpi >= 1; dpi--)
            {

                for (int row = 1; row <= maxrow; row++)
                {

                    ULINEPAIR leftPair;
                    ULINE? weirL = null;
                    ULINE? weirR = null;
                    ULINEPAIR rightPair;

                 
                    bool virtualleft = true;
                    bool virtualright = true;

                    int leftID = allweirs.FindIndex((x) => x.Col == dpi && x.Row == row);
                    int rightID = allweirs.FindIndex((x) => x.Col == dpi - 1 && x.Row == row);
                    
                    if (leftID < 0 && rightID < 0)
                        continue;
                    if (leftID >= 0)
                    {
                        
                        leftPair = allweirs[leftID];
                        virtualleft = leftPair.IsVirtual;
                        weirL = leftPair.GetSide(uppSides.Right, aNewSide: uppSides.Left);
                        if (weirL.HasValue)
                        {
                          
                            ULINE l1 = weirL.Value;
                            l1.Move(offset);
                            l1.Suppressed = leftPair.Suppressed;
                            weirL = l1;
                        }
                    }
                    if (rightID >= 0)
                    {
                        rightPair = allweirs[rightID];
                        virtualright = rightPair.IsVirtual;

                        weirR = rightPair.GetSide(uppSides.Left, aNewSide: uppSides.Right);
                        if (weirR.HasValue)
                        {
                            
                            ULINE l1 = weirR.Value;
                            l1.Move(-offset);
                            l1.Suppressed = rightPair.Suppressed;
                            weirR = l1;
                        }
                    }
                    ULINEPAIR panelPair = new ULINEPAIR(weirL, weirR, aLine1Side: uppSides.Left, aLine2Side: uppSides.Right, bSuppressed: false) { Col = dpi, Row = row, IsVirtual = virtualleft & virtualright };
                    _rVal.Add(panelPair);



                } // row loop

                //_rVal.AddRange(panelweirs);

            } // panel loop


            //    for (int i = 1; i <= panelweirs.Count; i++)
            //    {
            //        ULINEPAIR panelweir = panelweirs[i-1];
            //        panelweir.Row = maxrow;
            //        panelweir.Col = dpi;
            //        panelweir.PartIndex = dpi;
            //        if (maxrow == 1)
            //        {
            //            _rVal.Add(panelweir);
            //            continue;
            //        }
            //        if(i==1) pair1 = panelweir;
            //        if (i == 2) pair2 = panelweir;
            //        if (i == 3) pair3 = panelweir;




            //    }

            //    //set row numbers
            //    ULINEPAIR pair;
            //    double panelX = dcleft != null ?  dcleft.X + 0.5 * Spacing : dcright.X - 0.5 * Spacing;
            //    if (maxrow == 2) // one beam or a wall
            //    {

            //        if (pair1.HasValue && pair2.HasValue)
            //        {
            //            pair = pair1.Value; pair.Row = 1; pair.IsVirtual = true; pair1 = pair;
            //            pair = pair2.Value; pair.Row = 2; pair.IsVirtual = false; pair2 = pair;
            //        }
            //        else
            //        {
            //            pair = pair1.Value; pair.Row = panelX > 0 ? 2 : 1; pair.IsVirtual = panelX < 0; pair1 = pair;

            //        }

            //    }
            //    else if (maxrow == 3) //two beams
            //    {

            //        if (pair1.HasValue && pair2.HasValue && pair3.HasValue)
            //        {
            //            pair = pair1.Value; pair.Row = 1; pair.IsVirtual = true; pair1 = pair;
            //            pair = pair2.Value; pair.Row = 2; pair.IsVirtual = panelX < 0; pair2 = pair;
            //            pair = pair3.Value; pair.Row = 3; pair.IsVirtual = false; pair3 = pair;
            //        }
            //        else if (pair1.HasValue && pair2.HasValue && !pair3.HasValue)
            //        {
            //            pair = pair1.Value; pair.Row = panelX >= 0 ? 2 : 1; pair.IsVirtual = panelX < 0; pair1 = pair;
            //            pair = pair2.Value; pair.Row = panelX >= 0 ? 3 : 2; pair.IsVirtual = panelX < 0; pair2 = pair;
            //        }
            //        else
            //        {
            //            pair = pair1.Value; pair.Row = panelX >= 0 ? 3 : 1; pair.IsVirtual = panelX < 0; pair1 = pair;
            //        }

            //    }
            //    if (pair1.HasValue) _rVal.Add(pair1.Value);
            //    if (pair2.HasValue) _rVal.Add(pair2.Value);
            //    if (pair3.HasValue) _rVal.Add(pair3.Value);



            //}


            return _rVal;

        }

        public uopFreeBubblingPanels CreateFreeBubblingPanels(mdTrayAssembly aAssy = null)
        {
    
            if (Count <= 0)
                return null;
            aAssy ??= TrayAssembly;
            uopFreeBubblingPanels _rVal = new uopFreeBubblingPanels();
            if(aAssy == null) return _rVal;

            
            //get the free bubbling shapes
            uopPanelSectionShapes allfbashapes = CreatePanelShapes(aClearance: ShelfWidth, aLap: 0, out List<uopLine> _, out List<uopLinePair> allWeirs);
            int maxrow = MaxRow;
            double totweir = 0;
            double rad = RingRadius;
            int pcnt = Count + 1;
            double pspace = this.Spacing;
            double totalfba = 0;
       

            List<uopFreeBubblingArea> allfbas = new List<uopFreeBubblingArea>();
            //working right to left
            for (int pid = pcnt; pid >= 1; pid--)
            {
                //DowncomerInfo leftdc = Find((x) => x.DCIndex == pid);
                //DowncomerInfo rightdc = Find((x) => x.DCIndex == pid - 1);
                //if (leftdc == null && rightdc == null) continue;
                //double xleft = leftdc != null ? leftdc.X : rightdc != null? rightdc.X - pspace : rad;
                //double xrignt = xleft + pspace;
                //double panelX = xleft + (xrignt - xleft) / 2;
                uopFreeBubblingPanel panel = new uopFreeBubblingPanel(aPanelIndex: pid);
                List<uopPanelSectionShape> fbashapes = allfbashapes.FindAll((x) => x.PanelIndex == pid);
                
                //int occurs = 1;
                foreach (var fbashape in fbashapes)
                {
                    if (fbashape.IsVirtual)
                        continue;
                    // get the weir lines
                    int weirid = allWeirs.FindIndex((x) => x.Col == fbashape.Col && x.Row == fbashape.Row);
                    if (weirid < 0)
                        continue;

                    //uopShape bounds = new uopShape(fbashape);
                    uopLinePair weirs = weirid < 0 ? new uopLinePair() : new uopLinePair(allWeirs[weirid]);

     
                    uopFreeBubblingArea fba = new uopFreeBubblingArea(pid, weirs, fbashape) { TrayAssembly = aAssy };
                    fba.Area = fbashape.Area;
                    totalfba += fba.TotalArea;
                    totweir += fba.TotalWeirLength;
                    allfbas.Add(fba);
                    panel.Add(fba);
                   
                } // loop on shapes
                if (panel.Count == 1)
                {
                    panel.OccuranceFactor = panel[0].OccuranceFactor;
                }
                    
                else if(panel.Count >= 2)
                {
                    panel.OccuranceFactor = panel[1].OccuranceFactor;
                
                }
                    _rVal.Add(panel);

            } //loop on panels

            //set the fractions
            foreach (var item in allfbas)
            {
                item.TrayAssembly = aAssy;
                item.TotalTrayFreeBubblingArea = totalfba;
                item.TotalTrayWeirLength = totweir;
            }

            _rVal.Reverse();

            foreach(var panel in _rVal)
            {
                panel.WeirFraction = panel.TotalWeirLength() / totweir;
            }

            return _rVal;

        }



        #endregion Methods

        #region Shared Methods 

        public static DowncomerDataSet CloneCopy(DowncomerDataSet aSet) => aSet == null ? null : new DowncomerDataSet(aSet);

        #endregion Shared Methods
    }
    public class DividerInfo
    {
       
        #region Constructors

        public DividerInfo()
        {
            Width = 0;
            Rotation = 45;
            Offset = 0;
            Clearance = 0;
            BoundingRadius = 0;
            Length = 0;
            RingRadius = 0;
            RingClipOffset = 0.5;
            BeamOffsetFactor = 0;
            DividerType = uppTrayDividerTypes.Undefined;
        }

        public DividerInfo(DividerInfo aInfo)
        {
            DividerType = aInfo == null ? uppTrayDividerTypes.Undefined : aInfo.DividerType;
            DowncomerSpacing = aInfo == null ? 0 : aInfo.DowncomerSpacing;
            Width = aInfo == null ? 0 : aInfo.Width;
            Rotation = aInfo == null ? 0 : aInfo.Rotation;
            Offset = aInfo == null ? 0 : aInfo.Offset;
            Clearance = aInfo == null ? 0 : aInfo.Clearance;
            BoundingRadius = aInfo == null ? 0 : aInfo.BoundingRadius;
            Length = aInfo == null ? 0 : aInfo.Length;
            RingRadius = aInfo == null ? 0 : aInfo.RingRadius;
            RingClipOffset = aInfo == null ? 0.5 : aInfo.RingClipOffset;
            BeamOffsetFactor = aInfo == null ? 0 : aInfo.BeamOffsetFactor;  
        }


        public DividerInfo(mdBeam aBeam)
        {
            BeamOffsetFactor = aBeam == null ? 0 : aBeam.OffsetFactor;
            DowncomerSpacing = aBeam == null ? 0 : aBeam.DowncomerSpacing;
            Width = aBeam == null ? 0 : aBeam.Width;
            Rotation = aBeam == null ? 0 : aBeam.Rotation;
            Offset = aBeam == null ? 0 : aBeam.Offset;
            Clearance = aBeam == null ? 0 : aBeam.Clearance;
            BoundingRadius = aBeam == null ? 0 : aBeam.BoundingRadius;
            Length = aBeam == null ? 0 : aBeam.Length;
            RingRadius = aBeam == null ? 0 : aBeam.RingID / 2;
            RingClipOffset = 0.5;
            DividerType = uppTrayDividerTypes.Beam;
         
        }
        #endregion Constructors

        #region Properties

        public uppTrayDividerTypes DividerType { get; set; }     
        public double BeamOffsetFactor { get; set; }
        public double DowncomerSpacing { get; set; }
        public double RingRadius { get; set; }
        public double BoundingRadius { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public double Rotation { get; set; }
        public double Clearance { get; set; }
        public double EndPlateClearance { get; set; }
        public double Y => (Offset == 0) ? 0 : Math.Sin(Rotation * Math.PI / 180) * Offset;
        public double X => (Offset == 0) ? 0 : -Math.Cos(Rotation * Math.PI / 180) * Offset;
        public dxfPlane Plane => new dxfPlane(new dxfVector(X, Y), aRotation: Rotation);
        public double RingClipOffset { get; set; }

        private double _Offset = 0;
        public double Offset 
        {
            get 
            {
                return DividerType == uppTrayDividerTypes.Beam? mdBeam.OffsetValue(Rotation,DowncomerSpacing, BeamOffsetFactor) : _Offset ;
                
            }
            set => _Offset = value ; 
        }

        #endregion Properties


        #region Methods

        //Get the long edges of the beam(s) in the tray assembly
        internal List<ULINEPAIR> TrimEdges(out dxfPlane rPlane, double aBeamOffset = 0)
        {
            rPlane = Plane;
            var _rVal = new List<ULINEPAIR>();
            if (Length <= 0 || Width <= 0) return _rVal;


            double lng = Length / 2;
            double wd = (Width / 2) + aBeamOffset;
            ULINE l1 = new ULINE(rPlane.Vector(-lng, wd), rPlane.Vector(lng, wd), aTag: "TOP_EDGE", aSide: uppSides.Top) { Row = 1 };
            ULINE l2 = new ULINE(rPlane.Vector(-lng, -wd), rPlane.Vector(lng, -wd), aTag: "BOTTOM_EDGE", aSide: uppSides.Bottom) { Row = 1 };
            _rVal.Add(new ULINEPAIR(l1, l2, "TOP"));
            if (Offset > 0)
            {
                dxfPlane p2 = new dxfPlane(rPlane);
                p2.Project(rPlane.YDirection, -2 * Offset);
                l1 = new ULINE(p2.Vector(-lng, wd), p2.Vector(lng, wd), aTag: "TOP_EDGE", aSide: uppSides.Top) { Row = 2 };
                l2 = new ULINE(p2.Vector(-lng, -wd), p2.Vector(lng, -wd), aTag: "BOTTOM_EDGE", aSide: uppSides.Bottom) { Row = 2 };
                _rVal.Add(new ULINEPAIR(l1, l2, "BOTTOM"));
            }
            return _rVal;
        }

        //upper and lower are relative to the beam
        internal List<ULINEPAIR> GetDowncomerIntersections(double aLeftX, double aRightX, out List<ULINEPAIR> rEdges, double? aBoundingRadius = null, double? aLineOffset = null)
        {

            List<ULINEPAIR> _rVal = new List<ULINEPAIR>();

            BoundingRadius = (aBoundingRadius.HasValue) ? Math.Abs(aBoundingRadius.Value) : Math.Abs(BoundingRadius);

            double ringrad = Math.Round(RingRadius, 6);
            if (ringrad <= 0) ringrad = BoundingRadius;
            rEdges = TrimEdges(out dxfPlane dividerplane, 0);

            double iset = aLineOffset.HasValue ? Math.Abs(aLineOffset.Value) : 0;
            ULINE le = new ULINE(new UVECTOR(aLeftX), new UVECTOR(aLeftX, 100));
            ULINE re = new ULINE(new UVECTOR(aRightX), new UVECTOR(aRightX, 100));

            int i = 0;
            //loop through two possible beams
            foreach (ULINEPAIR longEdges in rEdges)
            {

                i++;
                ULINE topEdge = longEdges.Line1.Value;
                ULINE bottomEdge = longEdges.Line2.Value;


                if (iset != 0)
                {
                    topEdge.MoveOrtho(iset);
                    bottomEdge.MoveOrtho(-iset);
                }

                UVECTOR lt = le.IntersectionPt(topEdge, out _, out _, out _, out bool on1, out bool exists1);
                UVECTOR rt = re.IntersectionPt(topEdge, out _, out _, out _, out bool on2, out bool exists2);
                UVECTOR lb = le.IntersectionPt(bottomEdge, out _, out _, out _, out bool on3, out bool exists3);
                UVECTOR rb = re.IntersectionPt(bottomEdge, out _, out _, out _, out bool on4, out bool exists4);
                if (!exists1 && !exists2 && !exists3 && !exists4) continue;
                ULINE l1 = new ULINE(lt, rt, aTag: $"DIVIDER_{topEdge.Tag}", aSide: uppSides.Top) { Row = i };
                ULINE l2 = new ULINE(lb, rb, aTag: $"DIVIDER_{bottomEdge.Tag}", aSide: uppSides.Bottom) { Row = i };
                l1.Points = UVECTORS.Zero;
                l2.Points = UVECTORS.Zero;

                //if only one side has an intersection, there is no limit line.
                bool hasTop = exists1 && exists2 && (lt.Length(6) < ringrad && rt.Length(6) < ringrad); //  on1 && on2;
                bool hasBottom = exists3 && exists4 && (lb.Length(6) < ringrad && rb.Length(6) < ringrad); // on3 && on4;

                if (!hasTop && !hasBottom)
                    continue;



                l1.Side = uppSides.Top;
                l2.Side = uppSides.Bottom;



                _rVal.Add(new ULINEPAIR(l1, l2));

            }

            return _rVal;
        }

        public uopVectors Vertices(bool bGetMirror = false)
        {

            dxfPlane myplane = Plane;

            if (bGetMirror && Offset != 0)
            {
                myplane.Project(myplane.YDirection, -2 * Offset);
            }
            uopVectors _rVal = new uopVectors();
            double lng = Length / 2;
            double wd = Width / 2;
            _rVal.Add(myplane.Vector(-lng, wd));
            _rVal.Add(myplane.Vector(-lng, -wd));
            _rVal.Add(myplane.Vector(lng, -wd));
            _rVal.Add(myplane.Vector(lng, wd));
            return _rVal;

        }

        public uopLinePair GetEdges(double aOffset = 0)
        {
            dxfPlane myplane = Plane;
            double lg = Length / 2;
            double wd = Width / 2 + aOffset;
            return new uopLinePair(new uopLine(myplane.Vector(-lg, wd), myplane.Vector(lg, wd), aSide: uppSides.Top), new uopLine(myplane.Vector(-lg, -wd), myplane.Vector(lg, -wd), aSide: uppSides.Bottom));


        }

        #endregion Methods


        #region Shared Methods

        public static DividerInfo CloneCopy(DividerInfo aInfo) => aInfo == null ? null : new DividerInfo(aInfo);
        #endregion Shared Methods
    }




}
