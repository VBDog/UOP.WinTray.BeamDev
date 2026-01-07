using UOP.DXFGraphics;
using System;
using UOP.WinTray.Projects.Events;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Structures;


namespace UOP.WinTray.Projects
{
    public class mdFeedZone : ICloneable
    {
        // Option Explicit
        //^used by a md deck panel to report information about its feed areas
        private TMDFEEDZONE _Struc;

        #region Constructors
        public mdFeedZone() { _Struc = new TMDFEEDZONE(-1,-1); }

        internal mdFeedZone(TMDFEEDZONE aStructure) => _Struc = new TMDFEEDZONE(aStructure);

        public mdFeedZone(mdFeedZone aZone)
        {
            _Struc = aZone == null ? new TMDFEEDZONE(-1, -1): new TMDFEEDZONE(aZone.Structure);

        }
        #endregion

        /// <summary>
        /// ^returns a new object that is an exact copy of this one
        /// </summary>
        /// <returns></returns>
        public mdFeedZone Clone() => new mdFeedZone(this);

        object ICloneable.Clone() => (object)this.Clone();


        internal USHAPE BoundsV {  get => _Struc.Bounds;  set => _Struc.Bounds = value; }

        public dxfVector Center => _Struc.Bounds.Center.ToDXFVector();


        //^the parent deck panel of this feed zone
        public mdDeckPanel DeckPanel
        {
            get
            {
                mdTrayAssembly aAssy = TrayAssembly();
                return aAssy?.DeckPanels.Item(PanelIndex) ;

            }
            set
            {
                if (value != null)
                {
                    PanelIndex = value.Index;
                    _Struc.RangeGUID = value.RangeGUID;
                    _Struc.ProjectHandle = value.ProjectHandle;
                }

            }
        }
        public mdDowncomer Downcomer
        {
            get
            {
                mdTrayAssembly aAssy = TrayAssembly();
                return aAssy?.Downcomers.Item(DowncomerIndex);
            }
        }

        //^the index of the downcmer that feeds this zone
        public int DowncomerIndex { get => _Struc.DowncomerIndex; set => _Struc.DowncomerIndex = value; }

        //^the percentage of the trays entire FBA that this zone acounts for
        public double FBAPercentage
        {
            get
            {
               
                double tot = TrayFBA;
                double FBA = ZoneFBA;
                return (tot > 0) ?   FBA / tot * 100 : 0; 
                ;
            }
        }
        //^a string that identifies the spout group that feeds this zone
        //~like 1,2 etc.
        public string Handle => $"{DowncomerIndex },{ PanelIndex}";

        //^the number of times the zone occurs in the entire tray
        public int OccuranceFactor
        {
            get
            {
                int _rVal = 0;
                 dxfVector v1 = Center;

                double x1 = Math.Round(Math.Abs(v1.Y), 1);
                double y1 = Math.Round(v1.X, 1);
                if (x1 >= 0 & y1 >= 0)
                {
                    if (x1 > 0 & y1 > 0)
                    {
                        _rVal = 4;
                    }
                    else if (x1 > 0 & y1 == 0)
                    {
                        _rVal = 2;
                    }
                    else if (x1 == 0 & y1 > 0)
                    {
                        _rVal = 2;
                    }
                    else if (x1 == 0 & y1 == 0)
                    {
                        _rVal = 1;
                    }
                }

                return _rVal;
            }
        }
        //^the index of the parent deck panel of the zone
        public int PanelIndex { get => _Struc.PanelIndex; set => _Struc.PanelIndex = value; }

        //^the dxePolyline that is is the zone which is a portion of the parent deck panels open area
        public uopShape Bounds => new uopShape(_Struc.Bounds, aTag: Handle);
        
        //^the handle of the project that owns this object
        public string ProjectHandle { get => _Struc.ProjectHandle; set => _Struc.ProjectHandle = value; }

        //^the guid of the tray range that this part belongs to
        public string RangeGUID { get => _Struc.RangeGUID; set => _Struc.RangeGUID = value; }

        //^the spout group that supplies liquid to this zone
        public mdSpoutGroup SpoutGroup(mdTrayAssembly aAssy = null)
        { 
            aAssy ??= TrayAssembly();
                return aAssy?.SpoutGroups.GetByHandle(Handle);
        }
        internal TMDFEEDZONE Structure { get => _Struc; set => _Struc = value; }


        public mdTrayAssembly TrayAssembly()=> uopEvents.RetrieveMDTrayAssembly(_Struc.RangeGUID);

        //^the free bubbling are of the entire tray assembly
        public double TrayFBA
        {
            get
            {
                mdTrayAssembly aAssy = TrayAssembly();
                return (aAssy == null) ? 0 : aAssy.TotalFreeBubblingArea;
            }
        }

        //^the area of the zones perimeter polygon
        public double ZoneArea => _Struc.Bounds.Area;

        //^the area of the zones perimeter polygon multiplied by the number of times it ocurs in the whole tray
        public double ZoneFBA =>  ZoneArea * OccuranceFactor;

    }
}
