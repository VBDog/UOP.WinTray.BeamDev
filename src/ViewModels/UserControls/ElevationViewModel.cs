using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Enums;
using System.Collections.Generic;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.UI.ViewModels
{
    class ElevationViewModel : BindingObject
    {

        private uopElevation _Elevation = new();
        
        public ElevationViewModel()
        {
            ElevationTypes = uopEnumHelper.GetDescriptions(typeof(uppElevationTypes), true);
        }


        public uopElevation Elevation { get => _Elevation; set => _Elevation = value == null ? new uopElevation() : value; }

        public uppElevationTypes Type { get => _Elevation.Type; set { _Elevation.Type = value; NotifyPropertyChanged("Type"); } }
        public double Distance { get => _Elevation.Distance; set { _Elevation.Distance = value; NotifyPropertyChanged("Distance"); } }
        public int RingNo { get => _Elevation.RingNo; set { _Elevation.RingNo = value; NotifyPropertyChanged("RingNo"); } }

        private List<string> _ElevationTypes;
        public List<string> ElevationTypes
        {
            get => _ElevationTypes;
            set
            {
                value ??= new List<string>();
                _ElevationTypes = value;
                NotifyPropertyChanged("ElevationTypes");

            
            }
        }

        public int SelectedTypeIndex { get => (int)Type; set => Type = (uppElevationTypes)value; }
    }
}
