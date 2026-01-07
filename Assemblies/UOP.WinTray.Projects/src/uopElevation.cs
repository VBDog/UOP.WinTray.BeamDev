using UOP.WinTray.Projects.Enums;

namespace UOP.WinTray.Projects
{
    public class uopElevation
    {
        public uopElevation() { Type = uppElevationTypes.AboveTangent; }
        public uopElevation(uopElevation aElevation)
        { 
            Type = uppElevationTypes.AboveTangent;
            if (aElevation == null) return;
            Type = aElevation.Type;    
            Distance = aElevation.Distance;
            RingNo = aElevation.RingNo;
        }
        public uppElevationTypes Type { get; set; }
        public double Distance { get; set; }
        public int RingNo { get; set; }

        public uopElevation Clone() => new uopElevation(this );
        
    }
}
