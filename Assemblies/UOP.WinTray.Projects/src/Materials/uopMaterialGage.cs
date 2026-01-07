using UOP.WinTray.Projects.Enums;

namespace UOP.WinTray.Projects.Materials
{
    public class uopMaterialGage
    {

        public uopMaterialGage()
        {
            Id = 0; GageType = uppSheetGages.GageUnknown; Thickness = 0; Gage = string.Empty; FamilyName = string.Empty; 
        }

        public int Id { get; set; }
        public string Gage { get; set; }
        public double Thickness { get; set; }
        public string FamilyName { get; set; }

        public uppSheetGages GageType{ get; set; }
    }
}
