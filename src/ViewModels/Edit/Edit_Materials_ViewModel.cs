using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Materials;
using UOP.WinTray.UI.BusinessLogic;

using UOP.WinTray.UI.Events.EventAggregator;
using UOP.WinTray.UI.Messages;
using UOP.WinTray.UI.Commands;
using MvvmDialogs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;


namespace UOP.WinTray.UI.ViewModels
{
    /// <summary>
    /// Edit Materials View Model
    /// </summary>
    public class Edit_Materials_ViewModel : MDProjectViewModelBase, IModalDialogViewModel
    {

        #region Constructor

        internal Edit_Materials_ViewModel(  IEventAggregator eventAggregator,
                                            IDialogService dialogService,
                                            uopProject project,
                                            string selectedFieldName) : base(eventAggregator, dialogService:dialogService)
        {



            Project = project;
           
            IsApplyToAllTrays = true;
            SelectedFieldName = selectedFieldName;
            IsDeck = false;
            IsDowncommer = false;
            IsSheetMetal = false;
            IsHardware = false;
            BoltingMaterials = new ObservableCollection<uopHardwareMaterial>(uopMaterials.GetBoltingMaterials());
            MaterialFamilies = new ObservableCollection<uopMaterialFamily>(uopMaterials.GetSheetMetalFamilies());
            LoadProperties();
        }

        private void LoadProperties()
        {
            if (SelectedFieldName.ToUpper() == "DECK" )
            {
                IsDeck = true;
                IsDowncommer = false;
                IsHardware = false;
                MaterialTitle = "Edit Deck Material";
                IsSheetMetal = true;
                GetDeckMaterial();

            }
            else if (SelectedFieldName.ToUpper() == "DOWNCOMER")
            {
                IsDowncommer = true;
                IsDeck = false;
                IsHardware = false;
                MaterialTitle = "Edit Downcomer Material";
                IsSheetMetal = true;

                GetDowncomerMaterial();

            }
            else
            {
                IsHardware = true;
                IsDowncommer = false;
                IsDeck = false;
                MaterialTitle = "Edit Hardware Material";
                IsApplyToAllTrays = true;
                LoadBoltingMaterial();
            }

            if (!IsHardware)
            {
                uopTrayRange range = SelectedTrayRange;
                IsApplyToDeckNDowncomer = (range != null) && range.DeckMaterial.IsEqual(range.BeamMaterial);
            }
        }

        private void LoadBoltingMaterial()
        {           
            var lstBoltingMaterial = this.BoltingMaterials;

            if (null == lstBoltingMaterial || lstBoltingMaterial.Count <= 0) return;
            if (MaterialFamilies == null) return;
            uopTrayRange range = SelectedTrayRange;
           

            if (range != null)
            {
                var ind = lstBoltingMaterial.ToList().FindLastIndex(x => x.Name == range.HardwareMaterial?.FamilySelectName);
                SelectedBoltingMaterialIndex = (ind != -1) ? ind : 0;
            }                   
           
        }        

        /// <summary>
        /// Close form
        /// </summary>
        private void CloseForm()
        {
            //HostId.Close();
            //SelectDowncomerMaterialViewModel viewModel = new SelectDowncomerMaterialViewModel(materialHelper);
            this.DialogResult = false;
        }
        #endregion

        #region properties

        private string SelectedFieldName { get; set; }
      
        private string _MaterialTitle;
        public string MaterialTitle
        { 
            get=> _MaterialTitle;
            set { _MaterialTitle = value; NotifyPropertyChanged("MaterialTitle"); }
        }

        private uopMaterialFamily _SelectedMaterialFamily;
        /// <summary>
        /// Property refers to current selection of Material Family.
        /// </summary>
        public uopMaterialFamily SelectedMaterialFamily
        {
            get => _SelectedMaterialFamily;
            set
            {
                uopMaterialGage gage = SelectedMaterialGage;

                _SelectedMaterialFamily = value; 
                NotifyPropertyChanged("SelectedMaterialFamily");
                MaterialGages = new ObservableCollection<uopMaterialGage>(uopMaterials.GetSheetMetalGages(value));
               if(gage != null)
                {
                    foreach (var item in MaterialGages)
                    {
                        if(string.Compare(gage.Gage, item.Gage, true) == 0)
                        {
                            SelectedMaterialGage = item;
                            break;
                        }
                    }
                }
            }
        }

        private uopMaterialGage _SelectedMaterialGage;
        /// <summary>
        /// Property refers to current selection of Material Gage.
        /// </summary>
        public uopMaterialGage SelectedMaterialGage
        {
            get => _SelectedMaterialGage;
            set{ _SelectedMaterialGage = value; NotifyPropertyChanged("SelectedMaterialGage"); NotifyPropertyChanged("MaterialThickness"); }
            
        }
        private ObservableCollection<uopMaterialFamily> _MaterialFamilies;
        /// <summary>
        /// This property populates the Select Material Family ComboBox.
        /// </summary>
        public ObservableCollection<uopMaterialFamily> MaterialFamilies
        {
            get => _MaterialFamilies;
            set {_MaterialFamilies = value; NotifyPropertyChanged("MaterialFamilies"); }
            
        }

        private ObservableCollection<string> _SheetMetalFamilies;
        /// <summary>
        /// This property populates the Select Material Family ComboBox.
        /// </summary>
        public ObservableCollection<string> SheetMetalFamilies
        {
            get => _SheetMetalFamilies;
            set { _SheetMetalFamilies = value; NotifyPropertyChanged("SheetMetalFamilies"); }

        }

        private ObservableCollection<uopMaterialGage> _MaterialGages;
        /// <summary>
        /// This property populates the Select Material Gage ComboBox.
        /// </summary>
        public ObservableCollection<uopMaterialGage> MaterialGages
        {
            get => _MaterialGages;
            set { _MaterialGages = value; NotifyPropertyChanged("MaterialGages"); }
        }

        private int _SelectedBoltingMaterialIndex;

        public int SelectedBoltingMaterialIndex
        {
            get => _SelectedBoltingMaterialIndex;
            set
            {
                if (-1 != value)
                {
                    _SelectedBoltingMaterialIndex = value;
                    NotifyPropertyChanged("SelectedBoltingMaterialIndex");
                }
            }
        }

        public uopHardwareMaterial SelectedBoltingMaterial
        {
            get
            {
                if (BoltingMaterials == null) return null;

                List<uopHardwareMaterial> mtrls = BoltingMaterials.ToList();

                return (SelectedBoltingMaterialIndex >=0  && SelectedBoltingMaterialIndex < mtrls.Count)? mtrls[SelectedBoltingMaterialIndex] : null;
               
            }
        }

        private bool _IsDeck;

        public bool IsDeck
        {
            get => _IsDeck;
            set {_IsDeck = value; NotifyPropertyChanged("IsDeck"); }
            
        }
        private bool _IsDowncommer;

        public bool IsDowncommer
        {
            get => _IsDowncommer;
            set  { _IsDowncommer = value; NotifyPropertyChanged("IsDowncommer"); }
        }

        private bool _IsHardware;

        public bool IsHardware
        {
            get => _IsHardware;
            set { _IsHardware = value; NotifyPropertyChanged("IsHardware"); }
        }

        private bool _IsSheetMetal;

        public bool IsSheetMetal
        {
            get => _IsSheetMetal;
            set { _IsSheetMetal = value; NotifyPropertyChanged("IsSheetMetal"); }
        }

        private bool _IsApplyToDeckNDowncomer;

        public bool IsApplyToDeckNDowncomer
        {
            get => _IsApplyToDeckNDowncomer;
            set { _IsApplyToDeckNDowncomer = value; NotifyPropertyChanged("IsApplyToDeckNDowncomer"); }
            
        }

        private bool _IsApplyToAllTrays;

        public bool IsApplyToAllTrays
        {
            get => _IsApplyToAllTrays;
            set { _IsApplyToAllTrays = value; NotifyPropertyChanged("IsApplyToAllTrays"); }
        }
        /// <summary>
        /// This property populates the Select Material Thickness ComboBox.
        /// </summary>

   
        public string MaterialThickness
        {
            get
            {
                uopMaterialGage gage = SelectedMaterialGage;

                return (gage != null) ?  Units_Linear.UnitValueString(gage.Thickness,uppUnitFamilies.English) + " [" + Units_Linear.UnitValueString(gage.Thickness, uppUnitFamilies.Metric) + "]" : "";
               
            }
        }

       
        private ObservableCollection<uopHardwareMaterial> _BoltingMaterials;
        public ObservableCollection<uopHardwareMaterial> BoltingMaterials
        {
            get => _BoltingMaterials;
            set { _BoltingMaterials = value; NotifyPropertyChanged("BoltingMaterials"); }

        }
      

        private DelegateCommand _CMD_Cancel;
         /// <summary>
        /// Cancel Command
        /// </summary>
        public ICommand Command_Cancel { get { if (_CMD_Cancel is null) _CMD_Cancel = new DelegateCommand(param => CloseForm()); return _CMD_Cancel; } }
        private DelegateCommand _CMD_OK;
        public ICommand Command_Ok { get { if (_CMD_OK == null) _CMD_OK = new DelegateCommand(param => Execute_Save()); return _CMD_OK; } }


        private DelegateCommand _AddCommand;
        public DelegateCommand AddCommand
        {
            get
            {
                if (_AddCommand is null) _AddCommand = new DelegateCommand(param => InvokeDowncomerMaterialView());
                return _AddCommand;
            }
        }
        
       private void Execute_Save()
        {
            if (Project == null) return;
            try
            {
                IsEnabled = false;
                BusyMessage = "Saving...";
                Save();

            }
            catch { }
            finally
            {
                IsEnabled = true;
                BusyMessage = "";
            }
        }

        /// <summary>
        /// Save New Material Family
        /// </summary>
        /// <returns></returns>
        private void Save()
        {
            Message_Refresh refresh =  new(bSuppressPropertyLists: false, aPartTypeList: new List<uppPartTypes>() { uppPartTypes.Materials });
            bool thicknessChange = false;
            uopProject project = Project;
            colUOPTrayRanges prjRanges = project.TrayRanges;
            uopTrayRange range = SelectedTrayRange;
            int chngs = 0;
            string rngGUID = range != null ? range.GUID : "";
            try
            {

                if (IsHardware)
                {
                    
                    uopHardwareMaterial mtrl = SelectedBoltingMaterial;

                    if(mtrl != null)
                    {
                        
                        if (IsApplyToAllTrays)
                        {
                            foreach (var item in prjRanges)
                            {
                                if (!item.HardwareMaterial.IsEqual(mtrl))
                                {
                                    chngs++;
                                    item.HardwareMaterial = mtrl;
                                }
                                
                            }
                        }
                        else
                        {
                            if(range != null)
                            {

                                if (!range.HardwareMaterial.IsEqual(mtrl))
                                {
                                    chngs++;
                                    range.HardwareMaterial = mtrl;
                                }
                            }
                        }
                       
                    }
                   
                }
                else
                {


                    uopSheetMetal shtMtl = uopGlobals.goSheetMetalOptions().GetByFamilyAndGauge(SelectedMaterialFamily, SelectedMaterialGage);

                    var isToChangeDowncomerMat = IsApplyToDeckNDowncomer || SelectedFieldName.ToUpper() == "DOWNCOMER";
                    var isToChangeDeckMat = IsApplyToDeckNDowncomer || SelectedFieldName.ToUpper() == "DECK";

                   
                    
                    refresh = new Message_Refresh(bSuppressPropertyLists: false, aPartTypeList: new List<uppPartTypes>() { uppPartTypes.Materials });
                    List<uopTrayRange> ranges = new();
                    uopSheetMetal wuz;

                    if (!IsApplyToAllTrays)
                    {
                        ranges.Add(range);
                    }
                    else
                    {
                        for (int i = 1; i <= prjRanges.Count; i++) { ranges.Add(prjRanges.Item(i)); }

                    }

                    for (int i = 0; i < ranges.Count; i++) 
                    {
                        range = ranges[i];
                        //Downcomer Material change      
                        if (isToChangeDowncomerMat)
                        {
                            wuz = range.BeamMaterial;
                            if (!wuz.IsEqual(shtMtl))
                            {
                                if (string.Compare(range.GUID, rngGUID, true) == 0)
                                {
                                    if(shtMtl.Thickness != wuz.Thickness)
                                    thicknessChange = true;
                                    refresh.AddPartType(uppPartTypes.Downcomer);
                                    if (IsApplyToDeckNDowncomer) refresh.AddPartType(uppPartTypes.Deck);
                                }
                                chngs++;
                                range.BeamMaterial = shtMtl;

                            }
                        }
                        //Downcomer Material change      
                        if (isToChangeDeckMat)
                        {
                            if (!range.DeckMaterial.IsEqual(shtMtl))
                            {
                                if (string.Compare(range.GUID, rngGUID, true) == 0)
                                {
                                    refresh.AddPartType(uppPartTypes.Deck);
                                    if (IsApplyToDeckNDowncomer) refresh.AddPartType(uppPartTypes.Downcomer) ;
                                }

                                chngs++;
                                range.DeckMaterial = shtMtl;

                            }
                        }
                    }

                    
                  
                }
            }
            finally
            {
               if(chngs > 0)
                {

                    if (thicknessChange)
                    {
                        //refesh everything
                        refresh = new Message_Refresh();
                    }

                    RefreshMessage = refresh;
                    DialogResult = true;
                }
                else
                {
                    DialogResult = false;
                }
                
                
            }
        }
        private bool? _DialogResult;
        public bool? DialogResult
        {
            get => _DialogResult;
            private set { _DialogResult = value; NotifyPropertyChanged("DialogResult"); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get the Downcomer data
        /// </summary>
        private void GetDowncomerMaterial()
        {
            if (MaterialFamilies == null) return;
            uopTrayRange range = SelectedTrayRange;
           
            if (range != null)
            {
                uopSheetMetal shmt = range.BeamMaterial;
                if (shmt != null)
                {
                    SelectedMaterialFamily = MaterialFamilies.Select(x => x).FirstOrDefault(y => string.Compare(y.Name, shmt.FamilyName, true) == 0 || string.Compare(y.Name, shmt.FamilySelectName, true) == 0);
                    SelectedMaterialGage = MaterialGages.Select(x => x).FirstOrDefault(y => string.Compare(y.Gage, shmt.GageName, true) == 0);
                    return;
                }

            }
          


            SelectedMaterialGage = new uopMaterialGage();
        }

        /// <summary>
        /// Get the Deck data
        /// </summary>
        private void GetDeckMaterial()
        {
            if (MaterialFamilies == null) return;
            uopTrayRange range = SelectedTrayRange;
            
            if (range != null)
            {
                uopSheetMetal shmt = range.DeckMaterial;
                if (shmt != null)
                {
                    SelectedMaterialFamily = MaterialFamilies.Select(x => x).FirstOrDefault(y => string.Compare(y.Name, shmt.FamilyName, true) == 0 || string.Compare(y.Name, shmt.FamilySelectName, true) == 0);
                    SelectedMaterialGage = MaterialGages.Select(x => x).FirstOrDefault(y => string.Compare(y.Gage, shmt.GageName, true) == 0);
                    return;
                }

            }
            

            SelectedMaterialGage = new uopMaterialGage();

        }

      

        /// <summary>
        /// Invoke DowncomerMaterialForm
        /// </summary>
        private void InvokeDowncomerMaterialView()
        {
            Views.Windows.DowncomerMaterialView downcomerMaterialView = new();
            UOP.WinTray.UI.ViewModels.DowncomerMaterialViewModel downcomerMaterialViewModel = new(new MaterialHelper());
            downcomerMaterialView.DataContext = downcomerMaterialViewModel;
            downcomerMaterialViewModel.HostId = downcomerMaterialView;
            downcomerMaterialView.ShowDialog();
            NotifyPropertyChanged("MaterialFamilies");
            NotifyPropertyChanged("MaterialGages");
        }

        #endregion

    }
}

