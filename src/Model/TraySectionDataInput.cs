using System.Collections.Generic;

namespace Honeywell.UOP.WinTray.Model
{
    public class TraySectionDataInput
    {
        #region Constructors
        public TraySectionDataInput()
        {
        }

        #endregion
        public int Id { get; set; }
        public int ProjectId { get; set; }        
        public Project Project { get; set; }
        //Tray Section Name
        public string TraySectionName { get; set; }
            
         
        public int BoltingMtrlId { get; set; }       
        public BoltingMaterial BoltingMtrl { get; set; }       
        public IEnumerable<BoltingMaterial> BoltingMaterials { get; set; }
        //Shell Id
        public double ShellId { get; set; }

        //Ringh Width
        public double RingWidth { get; set; }
        //Ring Thickness
        public double RingThk { get; set; }
        //Tray Spacing
        public double TraySpacing { get; set; }
        //Ring Clearance
        public double RingClearance { get; set; }
        //Diameter of Tray
        public double TrayDiameter { get; set; }
        //If using default ring clearance (True/False)
        public bool UseDefaultRingClearance { get; set; }    
        public string ProjectTitle { get; set; }     
        public int TrayRangeFrom { get; set; }       
        public int TrayRangeTo { get; set; }
    }
}
