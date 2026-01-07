using System.Collections.Generic;

namespace UOP.WinTray.UI.Data
{
    public class MaterialModel
    {
        public List<Material> Material { get; set; }
        public List<Gage> Gage { get; set; }
    }

    public class Material
    {
        public string FamilyName { get; set; }
        public double Density { get; set; }
        public bool IsStainlessSteel { get; set; }
    }

    public class Gage
    {
        public string GageName { get; set; }
        public double Thickness { get; set; }
        public bool IsMetricMaterial { get; set; }
    }
}