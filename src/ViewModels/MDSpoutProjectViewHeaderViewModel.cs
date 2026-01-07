using System.Collections.ObjectModel;
using Honeywell.UOP.WinTray.Model;
using Honeywell.UOP.WinTray.Views;
using Honeywell.UOP.WinTray.Views.Windows;
using Honeywell.UOP.WinTray.Utilities;
using System.Collections.Generic;
using System;

namespace Honeywell.UOP.WinTray.ViewModel
{
    /// <summary>
    /// View Model Class for the Select Tray Section of the MD Spout Project Form.
    /// </summary>
    public class MDSpoutProjectViewHeaderViewModel : ViewModelBase
    {
        public readonly TraySectionDataInput traySectionDataInput;
        public readonly MDSpoutProjectViewModel parentVM;
        public MDSpoutProjectViewHeaderViewModel()
        {     
            traySectionDataInput = new TraySectionDataInput();
            traySections = new ObservableCollection<TraySectionDataInput>();
            //Initialize the Select Tray Section ComboBox.
            InitializeTraySection(); 
        }

        public MDSpoutProjectViewHeaderViewModel(ViewModelBase viewModelBase) : this()
        {
            parentVM = viewModelBase as MDSpoutProjectViewModel;
        }

        public double ManholeID
        {
            get
            {
                return Convert.ToDouble(WinTrayMainViewModel.WinTrayMainViewModelObj.NewProjectViewModel.ManholeID);
            }
            set
            {
                NotifyPropertyChanged("ManholeID");
            }
        }

        public double ShellID
        {
            get
            {
                return SelectedTraySection.ShellId;
            }
            set
            {
                SelectedTraySection.ShellId = value;
                NotifyPropertyChanged("ShellID");
            }
        }

        public double RingWD
        {
            get
            {
                return SelectedTraySection.RingWidth;
            }
            set
            {
                SelectedTraySection.RingWidth = value;
                NotifyPropertyChanged("RingWD");
            }
        }

        public double TraySpacing
        {
            get
            {
                return SelectedTraySection.TraySpacing;
            }
            set
            {
                SelectedTraySection.TraySpacing = value;
                NotifyPropertyChanged("TraySpacing");
            }
        }

        private TraySectionDataInput traySection;
        public TraySectionDataInput TraySection
        {
            get
            {
                return traySection;
            }
            set
            {
                traySection = value;
                NotifyPropertyChanged("TraySection");
            }
        }
        private ObservableCollection<TraySectionDataInput> traySections;

        /// <summary>
        /// This property will handle combobox in the header section of  MDSpoutProjecView.xaml
        /// </summary>
        public ObservableCollection<TraySectionDataInput> TraySections
        {
            get
            {
                return traySections;
            }
            set
            {
                traySections = value;
                NotifyPropertyChanged("TraySections");

            }
        }

        /// <summary>
        /// This property will return the selection of the tray section combobox.
        /// </summary>
        private TraySectionDataInput selectedTraySection;
        public TraySectionDataInput SelectedTraySection
        {
            get
            {
                return selectedTraySection;
            }
            set
            {
                selectedTraySection = value;
                NotifyPropertyChanged("SelectedTraySection");
                NotifyPropertyChanged("ShellID");
                NotifyPropertyChanged("RingWD");
                NotifyPropertyChanged("TraySpacing");
                if (!(value is null))
                {
                    if (value.TraySectionName == WinTrayUIConstants.NEW_SECTION)
                    {
                        /*New section launches the TraySectionDataInput form*/
                        LoadTraySectionDataInput();
                    }
                }
            }
        }

        /// <summary>
        /// If New Tray Section is not created set selected tray section as null
        /// </summary>
        public void SetSelectedTraySectionNull()
        {
            //If there is no created TraySction
            if (TraySections.Count == 1) SelectedTraySection = new TraySectionDataInput();
            else if (SelectedTraySection.TraySectionName.Equals(WinTrayUIConstants.NEW_SECTION)) SetNewlyCreatedTrayAsSelectedTraySection();
        }
        /// <summary>
        /// Set Newly created Tray Section as Selected Tray Section
        /// </summary>
        public void SetNewlyCreatedTrayAsSelectedTraySection()
        {
            SelectedTraySection = TraySections[TraySections.Count - 1];
        }

        /// <summary>
        /// This function adds the first item to the combox that allows user to create a new section.
        /// </summary>
        private void InitializeTraySection()
        {
            SelectedTraySection = new TraySectionDataInput();
            TraySections = new ObservableCollection<TraySectionDataInput>();
            TraySections.Add(new TraySectionDataInput { TraySectionName = WinTrayUIConstants.NEW_SECTION, Id = 0 });
            NotifyPropertyChanged("TraySections");
        }

        /// <summary>
        /// Add Newly created traysection to collection
        /// </summary>
        /// <param name="traySectionDataInput"></param>
        public void AddNewTraySection(TraySectionDataInput traySectionDataInput)
        {
            TraySections.Add(traySectionDataInput);
            NotifyPropertyChanged("TraySections");
            SetNewlyCreatedTrayAsSelectedTraySection();
        }
        /// <summary>
        /// Check if Traysection name already exists
        /// </summary>
        /// <param name="trayNumbers"></param>
        /// <returns></returns>
        public bool CheckIfTraySectionNameAlreadyExists(int trayRangeFrom)
        {
            if (new List<TraySectionDataInput>(TraySections).FindAll(item => item.TrayRangeTo>=trayRangeFrom).Count > 0)
            {
                return true;
            }
            else { return false; }
        }
        /// <summary>
        /// Func opens a new TraySectionDataInputView after user clicks new section. 
        /// </summary>
        private void LoadTraySectionDataInput()
        {
            TraySectionDataInputView traySectionDataInputView = new TraySectionDataInputView();
            ViewModel.TraySectionDataInputViewModel traySectionDataInputViewModel = new TraySectionDataInputViewModel();

            traySectionDataInputView.DataContext = traySectionDataInputViewModel;
            traySectionDataInputViewModel.hostID = traySectionDataInputView;

            traySectionDataInputView.Show();
        }

        //EditTray Command
        private DelegateCommand editTrayCommand;
        public DelegateCommand EditTrayCommand
        {
            get
            {
                if(editTrayCommand is null)
                {
                    editTrayCommand = new DelegateCommand(param => EditTray());
                }
                return editTrayCommand;
            }
        }

        /// <summary>
        /// Popup TraySection Datainput View with preloaded Data of selected Tray Section
        /// </summary>
        private void EditTray()
        {
            if(string.IsNullOrEmpty(SelectedTraySection.TraySectionName) || selectedTraySection.TraySectionName.Equals(WinTrayUIConstants.NEW_SECTION))
            {
                LoadTraySectionDataInput();
            }
            else
            {
                //Invoke TraySectionDataInput Form with Edit facility
                TraySectionDataInputView traySectionDataInputView = new TraySectionDataInputView();
                ViewModel.TraySectionDataInputViewModel traySectionDataInputViewModel = new TraySectionDataInputViewModel(SelectedTraySection);

                traySectionDataInputView.DataContext = traySectionDataInputViewModel;
                traySectionDataInputViewModel.hostID = traySectionDataInputView;

                traySectionDataInputView.Show();
            }
        }
    }
}
