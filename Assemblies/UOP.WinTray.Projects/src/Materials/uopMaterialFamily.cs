using UOP.WinTray.Projects.Enums;

namespace UOP.WinTray.Projects.Materials
{

    public class uopMaterialFamily
    {
        public uopMaterialFamily()
        {
            Id = 0; Name = string.Empty; Density = 0; IsStainlessSteel = false; Family = uppMetalFamilies.CarbonSteel;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public double Density { get; set; }
        public bool IsStainlessSteel { get; set; }

        public uppMetalFamilies Family { get; set; }
    }
}
