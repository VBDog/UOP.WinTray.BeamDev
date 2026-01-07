using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UOP.WinTray.Projects.Generators;

using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;

namespace UOP.WinTray.Projects.Utilities
{
    public class mdFreeBubblingAreas :  List<uopFreeBubblingArea>, ICloneable, IEnumerable<uopFreeBubblingArea>
    {

        #region Constructors

        public mdFreeBubblingAreas(mdTrayAssembly aAssy, double? aDCSpace, double? aOffset,  List<mdDeckPanel> aDeckPanels = null, bool bAssignFBAs = false, mdDowncomer aBasis = null, DowncomerDataSet aDCData = null, uppMDRoundToLimits? aRoundMethod = null )
        {

            TrayAssembly = aAssy;

            if (aDCData == null)
            {
                if (aBasis != null)
                {
                    if (aBasis.Width == aAssy.Downcomer().Width && aBasis.Count == aAssy.Downcomer().Count)
                        aBasis = aAssy.Downcomer();
                }
                aBasis ??= aAssy.Downcomer();
                aDCData = new DowncomerDataSet(aAssy, aDCSpace, aOffset, aBasis, aRoundMethod: aRoundMethod);
            }
            Initialize(aAssy, aDCData, aDeckPanels, bAssignFBAs);
        }

        public mdFreeBubblingAreas(mdFreeBubblingAreas aAreas)
        {
            if (aAreas != null)
            {
                TrayAssembly = aAreas.TrayAssembly;
                     if (aAreas._Panels != null) _Panels = new uopFreeBubblingPanels(aAreas._Panels);
                if(aAreas._DowncomerData != null) _DowncomerData = new DowncomerDataSet(aAreas._DowncomerData);
                UpdateData();
            }

        }

        private void Initialize(mdTrayAssembly aAssy, DowncomerDataSet dcdata, List<mdDeckPanel> aDeckPanels = null, bool bAssignFBAs = false)
        {

            TrayAssembly = aAssy;

            dcdata ??= new DowncomerDataSet(aAssy, null, null, null);
            
            //setting the downcomer data set creates the free bubbling panels
            DowncomerData = dcdata;
  
            Clear();
            double FBA;
            double WL;
            _WeirLengths = new List<double>();
            _Ratios = new List<double>();
            double totarea = 0;
            double totweir = 0;
            _TotalWeirLength = 0;
            _TotalFreeBubblingArea = 0;
            _Areas = new List<double>();
            if (Panels == null)
                return;
            foreach (uopFreeBubblingPanel panel in Panels)
            {
                foreach (var fba in panel)
                {
                    if (fba.IsVirtual)
                        continue;
                    FBA = fba.TotalArea;
                    WL = fba.TotalWeirLength;
                    _Areas.Add(FBA);
                    _WeirLengths.Add(WL);
                    _Ratios.Add(WL <= 0 ? 0 : FBA / WL);
                    //keep running totals
                    totarea += FBA;
                    totweir += WL;

                    base.Add(fba);
                }
            }

            _TotalWeirLength = totweir;
            _TotalFreeBubblingArea = totarea;

            if (dcdata.Count <= 0) return;

            

            //double totalfba =  TotalArea();
            //double totalweir = TotalWeirLength();
            //foreach (uopFreeBubblingArea fba in this)
            //{
            //    fba.TotalTrayFreeBubblingArea = totalfba;
            //    fba.TotalTrayWeirLength = totalweir;
            //}



            if (bAssignFBAs && aDeckPanels != null)
            {
                foreach (uopFreeBubblingPanel fbp in Panels)
                {
                    mdDeckPanel dp = aDeckPanels.Find(x => x.PanelIndex == fbp.PanelIndex);
                    if (dp != null)
                        dp.SetFBP(fbp);
                }
            }
        }

        #endregion Constructors
        #region Properties


        private List<double> _WeirLengths;
        public List<double> WeirLengths 
        { 
            get
            {
                if (_WeirLengths == null && Count > 0) UpdateData();
                return _WeirLengths;
            } 
     
        }
        private List<double> _Areas;

        public List<double> Areas
        {
            get
            {
                if (_Areas == null && Count > 0) UpdateData();
                return _Areas;
            }

        }

        private List<double> _Ratios;
        public List<double> Ratios
        {
            get
            {

                if (_Ratios == null && Count > 0) UpdateData();
                return _Ratios;
            }
          
        }


        private double? _TotalWeirLength;
        /// <summary>
        /// returns the tray wide total weir length of the members of the collection including instance
        /// </summary>
        /// <returns></returns>
        public double TotalWeirLength
        {
            get
            {
                if (!_TotalWeirLength.HasValue) UpdateData();
                return _TotalWeirLength.Value;
            }

        }

        private double? _TotalFreeBubblingArea;
        /// <summary>
        /// returns the tray wide total free bubbling area of the members of the collection including instance
        /// </summary>
        /// <returns></returns>
        public double TotalFreeBubblingArea
        {
            get
            {
                if (!_TotalFreeBubblingArea.HasValue) UpdateData();
                return _TotalFreeBubblingArea.Value;
            }

        }

        private void UpdateData() 
        {
            {
                _WeirLengths = new List<double>();
                _Ratios = new List<double>();
                _Areas = new List<double>();
                _TotalWeirLength = 0;
                _TotalFreeBubblingArea = 0;

                if (_Panels == null) return;

                double totarea = 0;
                double totweir = 0;
                foreach (var fba in this)
                {
                    if (fba.IsVirtual)
                        continue;
                    double FBA = fba.TotalArea;
                    double WL = fba.TotalWeirLength;
                    _Areas.Add(FBA);
                    _WeirLengths.Add(WL);
                    _Ratios.Add(WL <= 0 ? 0 : FBA / WL);
                     totarea += FBA;
                    totweir += WL;

                }
                _TotalWeirLength = totweir;
                _TotalFreeBubblingArea = totarea;

            }
        }

        public int PanelCount => _Panels == null ? 0 : _Panels.Count;

        private uppProjectTypes _ProjectType;
        public uppProjectTypes ProjectType { get => _ProjectType;
            set { _ProjectType = value; if(_DowncomerData != null)_DowncomerData.ProjectType = value; } }
     
        private uopFreeBubblingPanels _Panels;
        public uopFreeBubblingPanels Panels => _Panels ;

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
                ProjectType = value.ProjectType;
                IsSymmetric = value.IsSymmetric;
            }
        }

        internal DowncomerDataSet _DowncomerData;

        /// <summary>
        /// the downcomer data set that is used to determine the FBAs in this collection
        /// </summary>
        public DowncomerDataSet DowncomerData
        {
            get => _DowncomerData;
            private set
            {
                _DowncomerData = value;
                if (value != null)
                {
                    XVals_Downcomer = value.XValues_Downcomers;
                    IsSymmetric =  value.IsSymmetric;
                    _Panels = value.CreateFreeBubblingPanels(TrayAssembly);
                }
                else
                {
                    XVals_Downcomer = new List<double>();
                    IsSymmetric = false;
                    _Panels = null;
                }
             
            
               

               
            }

        }

        public double MaxVLError(mdTrayAssembly aAssy)
        {
            aAssy ??= TrayAssembly;
            double d1 = aAssy == null ? 0 : aAssy.Downcomers.FBA2WLRatio;
            double _rVal = 0;
            foreach (var m in Panels)
            {
                foreach (var item in m)
                {
                    double err = item.VLError(aAssy, ref d1);
                    if (Math.Abs(err) > Math.Abs(_rVal)) _rVal = err;
                }
            }
            return _rVal;
        }

        public List<double> XVals_Downcomer { get; private set; }


        public bool IsSymmetric { get; private set; }

       

        #endregion Properties

        #region Methods

        public double MechanicalActiveArea()
        {
            return TotalFreeBubblingArea - TotalBlockedArea();
        }

        public double TotalBlockedArea()
        {
            double _rVal = 0;
            foreach (uopFreeBubblingArea item in this)
                _rVal += item.TotalBlockedArea;

            return _rVal;
        }




        public override string ToString() => $"mdFreeBubblingAreas[{Count}]";

        object ICloneable.Clone() => new mdFreeBubblingAreas(this);

        public mdFreeBubblingAreas Clone() => new mdFreeBubblingAreas(this);
           
        public uopFreeBubblingArea Item(int aIndex)
        {
            if (aIndex < 1 || aIndex > Count) throw new IndexOutOfRangeException();
            return this[aIndex - 1];
        }

        public new int IndexOf(uopFreeBubblingArea aMember)
        {
            if (aMember == null) return 0;
            return base.IndexOf(aMember) + 1;
        }

   


        #endregion Methods

    }
}
