using UOP.WinTray.Projects.Materials;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;

using UOP.WinTray.UI.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;


using static UOP.WinTray.UI.Utilities.Constants.CommonConstants;
using UOP.WinTray.UI.Data;

namespace UOP.WinTray.UI.BusinessLogic
{
    /// <summary>
    /// Helper class to fetch the data required Material Helper
    /// </summary>
    public class MaterialHelper : BindingObject, IMaterialHelper
    {
        #region Constants
        private const string MATERIALSPATH = @"\Data\Materials.json";
        private const string MATERIAL_FAMILIES = "MaterialFamilies";
        private const string MATERIAL_GAGE_NAMES = "MaterialGageNames";

        #endregion

        #region Varibales   

        public List<uopSheetMetal> oSelected = new();
        private string selectedMaterialFamily;
        private string selectedMaterialGage;
        private List<uopMaterialFamily> materialFamilies;
        private List<uopMaterialGage> materialGageNames;

        #endregion

        #region Constructor

        public MaterialHelper()
        {
            materialFamilies = new List<uopMaterialFamily>();
            filePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + MATERIALSPATH;
        }

        #endregion

        #region Properties

        public string filePath { get; }
        public string SelectedMaterialFamily
        {
            get { return selectedMaterialFamily; }
            set { selectedMaterialFamily = value; }
        }

        public string SelectedMaterialGage
        {
            get { return selectedMaterialGage; }
            set { selectedMaterialGage = value; }
        }

        public List<uopMaterialFamily> MaterialFamilies
        {
            get { return materialFamilies; }
            set { materialFamilies = value; }
        }

        public List<uopMaterialGage> MaterialGageNames
        {
            get { return materialGageNames; }
            set { materialGageNames = value; }
        }

        #endregion

      

        #region Methods

        /// <summary>
        /// SHow the Materials
        /// </summary>
        public void ShowAddFrame()
        {
            ReadingMaterialGageFamiliesDate(filePath);
        }
        private const int MATERIALSDEFCOUNT = 56;
        /// <summary>
        /// reading the Material families and Gage Families data from JSON
        /// </summary>
        public void ReadingMaterialGageFamiliesDate(string filePath)
        {
            string materialText = File.ReadAllText(filePath);
            MaterialModel materialobj = JsonConvert.DeserializeObject<MaterialModel>(materialText);
            MaterialFamilies = new List<uopMaterialFamily>();
            MaterialGageNames = new List<uopMaterialGage>();
            uopSheetMetal mem;
               if (uopGlobals.goSheetMetalOptions().Count > MATERIALSDEFCOUNT)
            {
                for (int i = MATERIALSDEFCOUNT; i <= uopGlobals.goSheetMetalOptions().Count; i++)
                {
                    mem = (uopSheetMetal)uopGlobals.goSheetMetalOptions().Item(i);
                    if (mem.MaterialType ==uppMaterialTypes.Undefined) MaterialFamilies.Add(new uopMaterialFamily { Name = mem.FamilyName, Density = mem.Density, IsStainlessSteel = mem.IsStainless });
                    MaterialGageNames.Add(new uopMaterialGage { Gage = mem.GageName, Thickness = mem.Thickness });
                }
            }
            for (int i = 0; i < materialobj.Material.Count; i++)
            {
                uopMaterialFamily material = new();
                material.Name = materialobj.Material[i].FamilyName;
                material.Density = Convert.ToDouble(materialobj.Material[i].Density);
                material.IsStainlessSteel = materialobj.Material[i].IsStainlessSteel;
                material.Name = materialobj.Material[i].FamilyName;
                MaterialFamilies.Add(material);
            }
            NotifyPropertyChanged(MATERIAL_FAMILIES);
            for (int i = 0; i < materialobj.Gage.Count; i++)
            {
                uopMaterialGage gage = new();
                gage.Gage = materialobj.Gage[i].GageName;
                gage.Thickness = materialobj.Gage[i].Thickness;
                MaterialGageNames.Add(gage);
            }
            NotifyPropertyChanged(MATERIAL_GAGE_NAMES);
        }

        /// <summary>
        /// Update the JSON data with material families and gages families
        /// </summary>
        /// <param name="materialFamily"></param>
        /// <param name="gagesFamilies"></param>
        public void UpdateMaterialGageFamiliesDate(string filePath, Material materialFamily, Gage gagesFamilies)
        {
            string materialText = File.ReadAllText(filePath);
            MaterialModel materialobj = JsonConvert.DeserializeObject<MaterialModel>(materialText);

            if (materialFamily != null)
                materialobj.Material.Add(materialFamily);
            if (gagesFamilies != null)
                materialobj.Gage.Add(gagesFamilies);

            materialText = JsonConvert.SerializeObject(materialobj);
            File.WriteAllText(filePath, materialText);
            ReadingMaterialGageFamiliesDate(filePath);
        }
        /// <summary>
        /// This method adds newly created material to Global Static list of materials
        /// </summary>
        /// <param name="material"></param>
        /// <param name="materialsGages"></param>
        public void AddNewMaterial(Material material, Gage materialsGages)
        {
            string gName = materialsGages.IsMetricMaterial ? materialsGages.Thickness + " " + MM : materialsGages.GageName + " " + GA;
            uopSheetMetal aMat = uopGlobals.goSheetMetalOptions().GetSheetMetalByStringVals(material.FamilyName, gName);
            if (aMat is null)
            {
                aMat = new uopSheetMetal();
                var existingMat = uopGlobals.goSheetMetalOptions().GetByFamilyName(uppMaterialTypes.SheetMetal, material.FamilyName);
                if (existingMat is null)
                {
                    aMat.Family = uppMetalFamilies.Unknown;
                    aMat.FamilyName = material.FamilyName;
                    aMat.Density = material.Density;
                    aMat.IsStainless = material.IsStainlessSteel;
                }
                else
                {
                    aMat.Family = existingMat.Family;
                    aMat.FamilyName = existingMat.FamilyName;
                    aMat.Density = existingMat.Density;
                    aMat.IsStainless = existingMat.IsStainless;
                }
                aMat.SheetGage = uppSheetGages.GageUnknown;
                aMat.GageName = gName;
                aMat.IsMetric = materialsGages.IsMetricMaterial;
                aMat.Thickness = materialsGages.IsMetricMaterial ? Math.Round(materialsGages.Thickness / INCHTOMM, 3) : materialsGages.Thickness;
                uopGlobals.goSheetMetalOptions().Add(aMat);
            }
        }

        void IMaterialHelper.UpdateMaterialGageFamiliesDate(string filePath, Material materialFamily, Gage gagesFamilies)
        {
            UpdateMaterialGageFamiliesDate(filePath, materialFamily, gagesFamilies);
        }

        void IMaterialHelper.AddNewMaterial(Material material, Gage materialsGages)
        {
            AddNewMaterial(material, materialsGages);
        }

        #endregion
    }
}
