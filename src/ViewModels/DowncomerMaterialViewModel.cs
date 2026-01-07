using UOP.WinTray.UI.Interfaces;
using UOP.WinTray.UI.Utilities;
using UOP.WinTray.UI.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using static UOP.WinTray.UI.Utilities.Constants.CommonConstants;
using UOP.WinTray.Projects.Materials;
using UOP.WinTray.UI.Data;

namespace UOP.WinTray.UI.ViewModels
{
    public class DowncomerMaterialViewModel : ViewModel_Base
    {
        #region Constants

        private const string FAMILY_NAME_REQUIRED = "Material Family Name Is Required";
        private const string GAGE_NUMBER_REQUIRED = "Gage Number Is Required";
        private const string FAMILY_NAME_EXISTS = "New Material Family Name Already Exists. Use Existing Family Name Or \nEnter A Unique Name.";
        private const string GAGE_NUMBER_EXISTS = "New Material Gage Already Exists. Use Existing Gage Or Enter A Unique \nGage Name.";
        private const string DENSITY_LESS_THAN_EQUAL = "Material Density Is Must Be Less Than Or Equal To 0.5 lb per cubic inch";
        private const string DENSITY_MUST_GREATER = "Material Density Is Must Greater Than 0.1 lb per cubic inch";
        private const string DENSITY_REQUIRED = "Material Density Is Required";
        private const string THICKNESS_REQUIRED = "Material Thickness Is Required";
        private const string THICKNESS_LESS_THAN_025 = "Material Thickness Is Must Be Less Than Or Equal To 0.25 in.";
        private const string THICKNESS_GREATER_THAN_0012 = "Material Thickness Is Must Greater Than Or Equal To 0.012 in.";
        private const string THICKNESS_LESS_THAN_6 = "Material Thickness Is Must Be Less Than Or Equal To 6 mm";
        private const string THICKNESS_GREATER_THAN_05 = "Material Thickness Is Must Greater Than Or Equal To 0.5 mm";
        private const string MM_UNITS = "mm";
        private const string IN_UNITS = "in.";

        #endregion

        #region Varibales

        private readonly uopMaterialFamily materialFamily;
        private readonly uopMaterialGage materialGage;
        readonly IMaterialHelper materialHelper;
      

        #endregion

        #region Constructor
        public DowncomerMaterialViewModel(IMaterialHelper materialHelper)
        {
            this.materialHelper = materialHelper;
            materialFamily = new uopMaterialFamily();
            materialGage = new uopMaterialGage();
            selectedGage = new uopMaterialGage();
            selectedFamily = new uopMaterialFamily();
            GageList = new ObservableCollection<uopMaterialGage>();
            FamilyList = new ObservableCollection<uopMaterialFamily>();
            LoadDataInCollection(this.materialHelper.MaterialGageNames, uopMaterials.GetSheetMetalFamilies());
            TxtUnits = IN_UNITS;
        }

        #region Methods
        /// <summary>
        /// Load data for material list and material gage list
        /// </summary>
        private void LoadDataInCollection(List<uopMaterialGage> materialGages, List<uopMaterialFamily> materialFamilies)
        {
            try
            {
                FamilyList.Add(new uopMaterialFamily { Name = WinTrayUIConstants.NEW_FAMILY });
                materialFamilies.ToList().ForEach(item => FamilyList.Add(item));
                GageList.Add(new uopMaterialGage { Gage = WinTrayUIConstants.NEW_GAGE });
                materialGages.ToList().ForEach(item => GageList.Add(item));
                NotifyPropertyChanged("GageList");
                NotifyPropertyChanged("FamilyList");
                SelectedGage = GageList[0];
                SelectedFamily = FamilyList[0];
            }
            catch (NullReferenceException exception)
            {
                Utilities.Logger.HandleException(exception, "LoadDataInCollection", "DowncomerMaterialViewModel");
            }
            catch (Exception exception)
            {
                Utilities.Logger.HandleException(exception, "LoadDataInCollection", "DowncomerMaterialViewModel");
            }
        }
        /// <summary>
        /// Close form
        /// </summary>
        private void CloseForm()
        {
            HostId.Close();
            SelectDowncomerMaterialViewModel viewModel = new(materialHelper);
        }
        #endregion

        #endregion

        #region properties
        /// <summary>
        /// UI side binding to make Family fields readonly
        /// </summary>
        private bool isGageReadOnly;
        /// <summary>
        /// UI side binding to make Gage fields readonly
        /// </summary>
        private bool isFamilyReadOnly;
        public bool IsStainlessSteelEnabled { get; set; }
        public bool IsMetricMaterialEnabled { get; set; }
        private bool isGageThicknessReadonly;
        public bool IsGageThicknessReadonly { get { return isGageThicknessReadonly; } set { isGageThicknessReadonly = value; NotifyPropertyChanged(nameof(IsGageThicknessReadonly)); } }
        private bool isGageNumberReadonly;
        public bool IsGageNumberReadOnly
        {
            get { return isGageNumberReadonly; }
            set { isGageNumberReadonly = value; NotifyPropertyChanged(nameof(IsGageNumberReadOnly)); }
        }
        public bool IsSaveButtonEnabled { get; set; }
        public bool IsGageReadOnly
        {
            get { return isGageReadOnly; }
            set { isGageReadOnly = value; if (isGageReadOnly && isFamilyReadOnly) IsSaveButtonEnabled = false; else IsSaveButtonEnabled = true; IsMetricMaterialEnabled = !value; NotifyPropertyChanged("IsSaveButtonEnabled"); NotifyPropertyChanged("IsMetricMaterialEnabled"); NotifyPropertyChanged("IsGageReadOnly"); }
        }
        public bool IsFamilyReadOnly
        {
            get { return isFamilyReadOnly; }
            set { isFamilyReadOnly = value; if (isGageReadOnly && isFamilyReadOnly) IsSaveButtonEnabled = false; else IsSaveButtonEnabled = true; IsStainlessSteelEnabled = !value; NotifyPropertyChanged("IsSaveButtonEnabled"); NotifyPropertyChanged("IsStainlessSteelEnabled"); NotifyPropertyChanged("IsFamilyReadOnly"); }
        }
        public Window HostId { get; set; }
        public string Name { get { return materialFamily.Name; } set { materialFamily.Name = value; NotifyPropertyChanged("Name"); } }
        public double Density { get { return materialFamily.Density; } set { materialFamily.Density = value; NotifyPropertyChanged("Density"); ValidateProperty(nameof(Density)); } }
        public bool IsStainlessSteel
        {
            get => materialFamily.IsStainlessSteel; 
            set { materialFamily.IsStainlessSteel = value; NotifyPropertyChanged("IsStainlessSteel"); }
        }

        public string Gage { get { return materialGage.Gage; } set { materialGage.Gage = value; NotifyPropertyChanged("Gage"); } }
        private bool isMetricMaterial;
        public bool IsMetricMaterial
        {
            get
            { return isMetricMaterial; }
            set
            {

                isMetricMaterial = value;
                IsGageNumberReadOnly = value;
                Gage = value ? NA : string.Empty;
                TxtUnits = value ? MM_UNITS : IN_UNITS;
                NotifyPropertyChanged("IsMetricMaterial");
            }
        }
        private string txtUnits;

        public string TxtUnits
        {
            get { return txtUnits; }
            set
            {
                txtUnits = value;
                NotifyPropertyChanged("TxtUnits");
            }
        }

        public double GageThickness { get { return materialGage.Thickness; } set { materialGage.Thickness = value; NotifyPropertyChanged("GageThickness"); } }
        private uopMaterialFamily selectedFamily;
        private uopMaterialGage selectedGage;
        public uopMaterialFamily SelectedFamily
        {
            get
            {
                return selectedFamily;
            }
            set
            {
                selectedFamily = value;
                Name = selectedFamily.Name;
                Density = selectedFamily.Density;
                IsStainlessSteel = selectedFamily.IsStainlessSteel;
                if (GageThickness == ZERO && !selectedGage.Gage.Equals(WinTrayUIConstants.NEW_GAGE))
                {
                    GageThickness = selectedGage.Thickness;
                    IsGageThicknessReadonly = true;
                }
                if (selectedFamily != null)
                {
                    if (!selectedFamily.Name.Equals(WinTrayUIConstants.NEW_FAMILY)) IsFamilyReadOnly = true;
                    else
                    {
                        Name = string.Empty;
                        IsFamilyReadOnly = false;
                        if (!selectedGage.Gage.Contains(MM_UNITS))
                        {
                            IsGageThicknessReadonly = false;
                            GageThickness = 0;
                        }
                    }
                }
                NotifyPropertyChanged("SelectedFamily");
            }
        }
        public uopMaterialGage SelectedGage
        {
            get
            {
                return selectedGage;
            }
            set
            {
                selectedGage = value;
                if (selectedGage != null)
                {
                    if (!selectedGage.Gage.Equals(WinTrayUIConstants.NEW_GAGE))
                    {
                        IsGageReadOnly = true;
                        IsMetricMaterial = selectedGage.Gage.Contains(MM_UNITS);
                        IsGageNumberReadOnly = true;
                        Gage = selectedGage.Gage.Contains(MM_UNITS) ? NA : selectedGage.Gage.Split(' ')[0];
                        if (isFamilyReadOnly)
                        {
                            GageThickness = !selectedGage.Gage.Contains(MM_UNITS) ? selectedGage.Thickness : Convert.ToDouble(selectedGage.Gage.Split(' ')[0]);
                            IsGageThicknessReadonly = true;
                        }
                        else
                        {
                            GageThickness = !selectedGage.Gage.Contains(MM_UNITS) ? ZERO : Convert.ToDouble(selectedGage.Gage.Split(' ')[0]);
                            IsGageThicknessReadonly = GageThickness != 0;
                        }
                    }
                    else
                    {
                        IsGageReadOnly = false;
                        IsGageNumberReadOnly = false;
                        IsGageThicknessReadonly = false;
                        Gage = string.Empty;
                        GageThickness = ZERO;
                        IsMetricMaterial = false;
                    }
                }
                NotifyPropertyChanged("SelectedGage");
            }
        }

        /// <summary>
        /// FamilyList
        /// </summary>
        public ObservableCollection<uopMaterialFamily> FamilyList { get; set; }

        /// <summary>
        /// GageList
        /// </summary>
        public ObservableCollection<uopMaterialGage> GageList { get; set; }

        private DelegateCommand _CancelCommand;
        private DelegateCommand _SaveCommand;
        /// <summary>
        /// Cancel Command
        /// </summary>
        public ICommand CancelCommand
        {
            get
            {
                if (_CancelCommand is null)
                    _CancelCommand = new DelegateCommand(param => CloseForm());
                return _CancelCommand;
            }
        }
        /// <summary>
        /// Save Command
        /// </summary>
        public ICommand SaveCommand
        {
            get
            {
                if (_SaveCommand is null)
                {
                    _SaveCommand = new DelegateCommand(param => SaveDowncomer());
                }
                return _SaveCommand;
            }
        }

       


        /// <summary>
        /// Save downcomer data
        /// </summary>
        /// <returns></returns>
        private void SaveDowncomer()
        {
            SaveMaterialFamily();



        }

        /// <summary>
        /// Save New Material Family
        /// </summary>
        /// <returns></returns>
        private void SaveMaterialFamily()
        {
            try
            {
                ErrorMessage = string.Empty;
                if (ValidateSheetMetal())
                {
                    Material material = new()
                    {
                        FamilyName = this.Name,
                        Density = this.Density,
                        IsStainlessSteel = this.IsStainlessSteel
                    };

                    Gage materialsGages = new()
                    {
                        GageName = Gage,
                        Thickness = GageThickness,
                        IsMetricMaterial = IsMetricMaterial
                    };
                    materialHelper.AddNewMaterial(material,materialsGages);
                    //materialHelper.UpdateMaterialGageFamiliesDate(materialHelper.filePath, material, materialsGages);
                    //LoadDataInCollection(materialHelper.MaterialGageNames, materialHelper.MaterialFamilies);
                    //Close Form
                    CloseForm();
                }
            }
            catch (Exception exception)
            {
                Utilities.Logger.HandleException(exception, "SaveMaterialFamily", "DowncomerMaterialViewModel");
                ErrorMessage = exception.Message;
            }
        }

        #endregion

        #region Validation

        /// <summary>
        /// Validate the material properties
        /// </summary>
        /// <returns></returns>
        private bool ValidateSheetMetal()
        {
            int i = 0;
            if (string.IsNullOrEmpty(Name))
            {
                ErrorMessage = FAMILY_NAME_REQUIRED;
                return false;
            }
            if (!string.IsNullOrEmpty(Name) && SelectedFamily.Name.Equals(WinTrayUIConstants.NEW_FAMILY))
            {
                for (i = 0; i < materialHelper.MaterialFamilies.Count; i++)
                {
                    if (materialHelper.MaterialFamilies[i].Name == Name)
                    {
                        ErrorMessage = FAMILY_NAME_EXISTS;
                        return false;
                    }
                }
            }
            if (!string.IsNullOrEmpty(Gage))
            {
                for (i = 0; i < materialHelper.MaterialGageNames.Count; i++)
                {
                    if (materialHelper.MaterialGageNames[i].Gage.Contains(Gage))
                    {
                        ErrorMessage = GAGE_NUMBER_EXISTS;
                        return false;
                    }
                }
            }
            else
            {
                if (!IsMetricMaterial)
                {
                    ErrorMessage = GAGE_NUMBER_REQUIRED;
                    return false;
                }
            }
            if (Density > 0)
            {
                if (Density > 0.5)
                {
                    ErrorMessage = DENSITY_LESS_THAN_EQUAL;
                    return false;
                }

                if (Density < 0.1)
                {
                    ErrorMessage = DENSITY_MUST_GREATER;
                    return false;
                }
            }
            else
            {
                ErrorMessage = DENSITY_REQUIRED;
                return false;
            }
            if (GageThickness < 0)
            {
                ErrorMessage = THICKNESS_REQUIRED;
                return false;

            }
            else
            {
                if (!IsMetricMaterial)
                {
                    if (GageThickness > 0.25)
                    {
                        ErrorMessage = THICKNESS_LESS_THAN_025;
                        return false;
                    }

                    if (GageThickness < 0.0121)
                    {
                        ErrorMessage = THICKNESS_GREATER_THAN_0012;
                        return false;
                    }
                }
                else
                {
                    if (GageThickness > 6)
                    {
                        ErrorMessage = THICKNESS_LESS_THAN_6;
                        return false;
                    }

                    if (GageThickness < 0.5)
                    {
                        ErrorMessage = THICKNESS_GREATER_THAN_05;
                        return false;
                    }
                }
                return true;
            }
        }

        #endregion
    }
}
