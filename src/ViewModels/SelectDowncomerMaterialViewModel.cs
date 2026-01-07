using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using UOP.WinTray.Projects.Materials;
using UOP.WinTray.UI.Interfaces;
using UOP.WinTray.UI.Commands;
using UOP.WinTray.UI.Views.Windows;


namespace UOP.WinTray.UI.ViewModels
{
    /// <summary>
    /// ViewModel to bind data with SelectDowncomerMaterial Form in MDSpoutProject via Edit Deck Properties.
    /// </summary>
    public class SelectDowncomerMaterialViewModel : ViewModel_Base
    {
        //selected material family
        private uopMaterialFamily selectedMaterialFamily;
        //selected material gage
        private uopMaterialGage selectedMaterialGage;
        public Window HostId { get; set; }
        private readonly IMaterialHelper MaterialHelper;

        #region Constructor
        public SelectDowncomerMaterialViewModel(IMaterialHelper materialHelper)
        {
            MaterialHelper = materialHelper;
            //Async method to get data from the database.
            GetDowncomerMaterialAsync();
        }
        #endregion

        /// <summary>
        /// Get the data from Database
        /// </summary>
        private void GetDowncomerMaterialAsync()
        {
            SelectedMaterialGage = new uopMaterialGage();
            GetDowncomerMaterialsAsync();
        }

        /// <summary>
        /// This property populates the Select Material Thickness ComboBox.
        /// </summary>
        public double MaterialThickness
        {
            get
            {
                return SelectedMaterialGage.Thickness;
            }
        }

        private ObservableCollection<uopMaterialFamily> materialFamilies;
        public bool IsCancelPressed { get; set; }
        /// <summary>
        /// This property populates the Select Material Family ComboBox.
        /// </summary>
        public ObservableCollection<uopMaterialFamily> MaterialFamilies
        {
            get
            {
                return materialFamilies;
            }
            set
            {
                materialFamilies = value;
                NotifyPropertyChanged("MaterialFamilies");
            }
        }

        private ObservableCollection<uopMaterialGage> materialGages;

        /// <summary>
        /// This property populates the Select Material Gage ComboBox.
        /// </summary>
        public ObservableCollection<uopMaterialGage> MaterialGages
        {
            get
            {
                return materialGages;
            }
            set
            {
                materialGages = value;
                NotifyPropertyChanged("MaterialGages");
            }
        }

        /// <summary>
        /// Property refers to current selection of Material Family.
        /// </summary>
        public uopMaterialFamily SelectedMaterialFamily
        {
            get
            {
                return selectedMaterialFamily;
            }
            set
            {
                selectedMaterialFamily = value;
                NotifyPropertyChanged("SelectedMaterialFamily");
            }
        }

        /// <summary>
        /// Property refers to current selection of Material Gage.
        /// </summary>
        public uopMaterialGage SelectedMaterialGage
        {
            get
            {
                return selectedMaterialGage;
            }
            set
            {
                selectedMaterialGage = value;
                NotifyPropertyChanged("SelectedMaterialGage");
                NotifyPropertyChanged("MaterialThickness");
            }
        }

        /// <summary>
        /// Fetches the data via api calls and populates the local properties withrespective data
        /// </summary>
        /// <returns></returns>
        public void GetDowncomerMaterialsAsync()
        {

            try
            {
                MaterialHelper.ShowAddFrame();

                MaterialFamilies = new ObservableCollection<uopMaterialFamily>();
                for (int i = 0; i < MaterialHelper.MaterialFamilies.Count; i++)
                {

                    uopMaterialFamily materialFamily = new();
                    materialFamily.Name = MaterialHelper.MaterialFamilies[i].Name;
                    materialFamily.Id = i;
                    materialFamily.Density = MaterialHelper.MaterialFamilies[i].Density;
                    materialFamily.IsStainlessSteel = MaterialHelper.MaterialFamilies[i].IsStainlessSteel;
                    MaterialFamilies.Add(materialFamily);
                }
                SelectedMaterialFamily = MaterialFamilies.Select(x => x).FirstOrDefault(y => y.Name == MaterialHelper.SelectedMaterialFamily);
                //MaterialGages = new ObservableCollection<MaterialGage>();
                var materialsGage = new ObservableCollection<uopMaterialGage>();
                for (int i = 0; i < MaterialHelper.MaterialGageNames.Count; i++)
                {
                    uopMaterialGage materialGage = new();
                    materialGage.Gage = MaterialHelper.MaterialGageNames[i].Gage;
                    materialGage.Id = i;
                    materialGage.Thickness = MaterialHelper.MaterialGageNames[i].Thickness;
                    materialsGage.Add(materialGage);
                }
                SelectedMaterialGage = materialsGage.Select(x => x).FirstOrDefault(y => y.Gage == MaterialHelper.SelectedMaterialGage);
                MaterialGages = materialsGage;
            }

            catch (Exception exception)
            {
                Utilities.Logger.HandleException(exception, "GetDowncomerMaterialsAsync", "SelectDowncomerMaterialViewModel");
            }

        }
        #region Command Region
        //Cancel Command
        private DelegateCommand _CancelCommand;
        //Add Command
        private DelegateCommand _AddCommand;
        //Ok Command
        private DelegateCommand _OkCommand;
        public DelegateCommand CancelCommand
        {
            get
            {
                if (_CancelCommand is null) _CancelCommand = new DelegateCommand(param => CloseForm());
                return _CancelCommand;
            }
        }

        public DelegateCommand AddCommand
        {
            get
            {
                if (_AddCommand is null) _AddCommand = new DelegateCommand(param => InvokeDowncomerMaterialView());
                return _AddCommand;
            }
        }

        public DelegateCommand OkCommand
        {
            get
            {
                if (_OkCommand is null) _OkCommand = new DelegateCommand(param => ReturnSelectedMaterial());
                return _OkCommand;
            }
        }

        /// <summary>
        /// Return to parent control with Selected Material family and Gage
        /// </summary>
        private void ReturnSelectedMaterial()
        {
            IsCancelPressed = false;
            this.HostId.Close();
        }

        /// <summary>
        /// CLose the form
        /// </summary>
        private void CloseForm()
        {
            IsCancelPressed = true;
            this.HostId.Close();
        }

        /// <summary>
        /// Invoke DowncomerMaterialForm
        /// </summary>
        private void InvokeDowncomerMaterialView()
        {
            DowncomerMaterialView downcomerMaterialView = new();
            DowncomerMaterialViewModel downcomerMaterialViewModel = new(MaterialHelper);
            downcomerMaterialView.DataContext = downcomerMaterialViewModel;
            downcomerMaterialViewModel.HostId = downcomerMaterialView;
            downcomerMaterialView.ShowDialog();
            GetDowncomerMaterialsAsync();
            NotifyPropertyChanged("MaterialFamilies");
            NotifyPropertyChanged("MaterialGages");
        }

        #endregion
    }
}