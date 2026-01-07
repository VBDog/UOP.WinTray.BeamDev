using System;
using System.Collections.Generic;
using System.Text;
using Honeywell.UOP.WinTray.Model;
namespace Honeywell.UOP.WinTray.ViewModel
{
    /// <summary>
    /// View Model Class for Deck Data Section of the MD Spout Project Form.
    /// </summary>
    public class MDSpoutProjectDeckDataViewModel : ViewModelBase
    {
        public MDSpoutProjectViewDeckData mDSpoutProjectViewDeckData;

        public MDSpoutProjectDeckDataViewModel()
        {
            mDSpoutProjectViewDeckData = new MDSpoutProjectViewDeckData();
        }
        
        public double TotalFBA
        {
            get
            {
                return mDSpoutProjectViewDeckData.TotalFBA;
            }
            set
            {
                mDSpoutProjectViewDeckData.TotalFBA = value;
                NotifyPropertyChanged("TotalFBA");
            }
        }

        public double ActiveArea
        {
            get
            {
                return mDSpoutProjectViewDeckData.ActiveArea;
            }
            set
            {
                mDSpoutProjectViewDeckData.ActiveArea = value;
                NotifyPropertyChanged("ActiveArea");
            }
        }

        public double PanelWidth
        {
            get
            {
                return mDSpoutProjectViewDeckData.PanelWidth;
            }
            set
            {
                mDSpoutProjectViewDeckData.PanelWidth = value;
                NotifyPropertyChanged("PanelWidth");
            }
        }

        public double FpPercent
        {
            get
            {
                return mDSpoutProjectViewDeckData.FpPercent;
            }
            set
            {
                mDSpoutProjectViewDeckData.FpPercent = value;
                NotifyPropertyChanged("FpPercent");
            }
        }

        public double PerfDia
        {
            get
            {
                return mDSpoutProjectViewDeckData.PerfDia;
            }
            set
            {
                mDSpoutProjectViewDeckData.PerfDia = value;
                NotifyPropertyChanged("PerfDia");
            }
        }

        public double Material
        {
            get
            {
                return mDSpoutProjectViewDeckData.Material;
            }
            set
            {
                mDSpoutProjectViewDeckData.Material = value;
                NotifyPropertyChanged("Material");
            }
        }
        /// Edit command
        /// </summary>
        private DelegateCommand editDeckDataCommand;
        public DelegateCommand EditDeckDataCommand
        {
            get
            {
                if (editDeckDataCommand is null) editDeckDataCommand = new DelegateCommand(param => EditDeckData());
                return editDeckDataCommand;
            }
        }
        /// <summary>
        /// Edit Deck Data
        /// </summary>
        private void EditDeckData()
        {
            
        }
    }
}
