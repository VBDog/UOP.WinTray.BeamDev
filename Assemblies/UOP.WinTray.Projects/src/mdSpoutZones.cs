using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;
namespace UOP.WinTray.Projects
{
    public class mdSpoutZones : List<mdSpoutZone>, IEnumerable<mdSpoutZone>
    {
        public mdSpoutZones(mdTrayAssembly aAssy, colMDConstraints aConstraints = null) 
        {
            if (aAssy == null) { throw new ArgumentNullException("An MD Tray Assembly is required to create a valid mdSpoutZone list");  }

            _AssyRef = new WeakReference<mdTrayAssembly> (aAssy);
             DowncomerData = new DowncomerDataSet( aAssy.DowncomerData) { TrayAssembly = aAssy};
            ConvergenceLimit = aAssy.ConvergenceLimit;
            TrayName = (aAssy.TrayName());
            //get the weirlines
            List<ULINEPAIR>   weirlns = uopLinePairs.Copy( DowncomerData.WeirLns);
            aConstraints ??= aAssy._Constraints;
            Constraints = new colMDConstraints(aConstraints);
       
            
            List<mdSpoutArea> allareas = DowncomerData.SpoutAreas(bIncludeVirtuals: false);
            int zonecnt = DowncomerData.DesignFamily.IsBeamDesignFamily() && DowncomerData.Divider.Offset != 0 ? 2: 1;
            bool halftray =DowncomerData.DesignFamily.IsBeamDesignFamily() && !DowncomerData.MultiPanel;
            int? zfilter = null;
            if (halftray) zfilter = 1;
            DowncomerData.ResetDefinitonLines();

            List<ULINEPAIR> allweirs = DowncomerData.WeirLns;
            List<uopLinePair> weirlines1 = uopLinePairs.FromULinePairs(allweirs, null,  null, out double weirtotal,zfilter);
            _TotalWeirLength = weirtotal;

            mdSpoutZone zone1 = new mdSpoutZone(aAssy, aZoneIndex:1);
            mdSpoutZone zone2 = zonecnt == 2 ? new mdSpoutZone(aAssy, aZoneIndex: 2)  : null;

             if (zonecnt == 1)
            {
                zone1.WeirLns = uopLinePairs.ToULinePairs(weirlines1);
                zone1.TargetArea = aAssy.TheoreticalSpoutArea;
                if (halftray)
                {
                    zone1.TargetArea = aAssy.TheoreticalSpoutArea * 0.5;
                   
                }
            }
            else
            {
                zone1.WeirLns = weirlns.FindAll((x) => x.Row == 2);
                zone2.WeirLns= weirlns.FindAll((x) => x.Row == 1 || x.Row ==3 );
                zone1.WeirFraction = uopLinePairs.TotalLength(zone1.WeirLns)/ weirtotal ;
                zone2.WeirFraction = uopLinePairs.TotalLength(zone2.WeirLns)/ weirtotal ;
            
                zone1.TargetArea = aAssy.TheoreticalSpoutArea * zone1.WeirFraction;
                zone2.TargetArea = aAssy.TheoreticalSpoutArea * zone2.WeirFraction;

            }

            List<mdConstraint> constraints = Constraints.ToList();
            foreach (var sa in allareas)
            {
                sa.ZoneIndex = 1;
            
                mdConstraint constraint = constraints.Find((x) => x.PanelIndex == sa.PanelIndex && x.DowncomerIndex == sa.DowncomerIndex && x.BoxIndex == sa.BoxIndex);
                if (constraint == null)  constraint = constraints.Find((x) => x.PanelIndex == sa.PanelIndex && x.DowncomerIndex == sa.DowncomerIndex);
                if (constraint == null)
                {
                    constraint = new mdConstraint() { PanelIndex = sa.PanelIndex, DowncomerIndex = sa.DowncomerIndex, BoxIndex = sa.BoxIndex };
                    Constraints.Add(constraint);
                }

                sa.GroupIndex  =  constraint.AreaGroupIndex;
          
                if (constraint.TreatAsIdeal) 
                    sa.OverrideSpoutArea = constraint.OverrideSpoutArea;
              
                if (zonecnt == 1)
                {
                    zone1.Add(sa);
              
                }
                else
                {
                    if (sa.Row == 2)
                    {
                        zone1.Add(sa);
                    }
                    else
                    {
                        sa.ZoneIndex = 2;
                        zone2.Add(sa);
                    }
                }
            }

            Add(zone1);
            if (zonecnt == 2) Add(zone2);
            

        }

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
                TrayName = value.TrayName();
            }
        }

        public string TrayName { get; private set; }


        public colMDConstraints Constraints { get; set;}

        public DowncomerDataSet DowncomerData { get; private set; }

        public uppMDDesigns DesignFamily { get { return DowncomerData != null ? DowncomerData.DesignFamily : uppMDDesigns.Undefined; } }

        private double? _TotalWeirLength = null;

        public double TotalWeirLength
        {
            get
            {
                if (Count == 0)
                {
                    _TotalWeirLength = null;
                    return 0;
                }
                if (!_TotalWeirLength.HasValue)
                {
                    _TotalWeirLength = 0;
                    foreach (var item in this)
                    {
                        _TotalWeirLength += uopLinePairs.TotalLength(item.WeirLines);
                    }
                }
                return _TotalWeirLength.Value;
            }
        }

        public double ConvergenceLimit { get; set; } = 0.00001;

        public int DowncomerCount => DowncomerData == null ? 0 : DowncomerData.Count;

        public int PanelCount => DowncomerData == null ? 0 : DowncomerData.PanelCount;

        #region Methods 

        public  mdSpoutAreaMatrices Matrices(bool bDistributeArea = true)
        {
            mdSpoutAreaMatrices _rVal = new mdSpoutAreaMatrices() ;
            foreach (var zone in this)
            {
                mdSpoutAreaMatrix matrix = new mdSpoutAreaMatrix(zone, bDistributeArea);
                zone.SpoutAreaMatrix = matrix;
                _rVal.Add(matrix);
            }
            return _rVal;
        }

        public uopMatrix GetMatrix(uppSpoutAreaMatrixDataTypes aDataType)
        {
            if (DowncomerCount <= 0) return null;

            uopMatrix _rVal = new uopMatrix(PanelCount, DowncomerCount, 6, $"{uopEnums.Description(aDataType)} {TrayName}");
            for(int z = 1; z<= Count; z++)
            {
                mdSpoutZone zone = this[z -1];
                zone.UpdateMatrix(ref _rVal, aDataType);
            }
            //rVal.PrintToConsole();
            return _rVal;

        }
        public uopMatrix AvailableAreaMatrix(bool bAddTotalRowsAndColumns = false)
        {
            int iNd = DowncomerCount;
            if (iNd <= 0) return null;

            int iNp = PanelCount;

            uopMatrix _rVal = GetMatrix(uppSpoutAreaMatrixDataTypes.AvailableArea);
            if(_rVal == null) return null;

            if (bAddTotalRowsAndColumns)
            {
                _rVal.AddColumns(1);
                _rVal.AddRows(1);


                _rVal.TotalRows(iNd + 1, 1, iNd, 1, iNp);
                _rVal.TotalColumns(iNp + 1, 1, iNp, 1, iNd);
                _rVal.TotalRows(iNd + 1, 1, iNd, iNp + 1, iNp + 1);

            }

            return _rVal;

        }

     

        #endregion Methods
    }
}
