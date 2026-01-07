using System.Collections.Generic;
using UOP.WinTray.Projects.Materials;
using UOP.WinTray.UI.Data;

namespace UOP.WinTray.UI.Interfaces
{
    /// <summary>
    /// Interface for the methods used in Material Helper functionality.
    /// </summary>
    public interface IMaterialHelper
    {
        List<uopMaterialFamily> MaterialFamilies { get; set; }
        List<uopMaterialGage> MaterialGageNames { get; set; }
        void ShowAddFrame();
        string filePath { get; }
        void UpdateMaterialGageFamiliesDate(string filePath, Material materialFamily, Gage gagesFamilies);
        string SelectedMaterialGage { get; set; }
        string SelectedMaterialFamily { get; set; }

        void AddNewMaterial(Material material, Gage materialsGages);
    }
}
