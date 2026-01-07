using System;
using System.Collections.Generic;
using System.Text;
using Honeywell.UOP.WinTray.Model;

namespace Honeywell.UOP.WinTray.ViewModel
{
    /// <summary>
    /// View Model Class for Downcomer Data Section of the MD Spout Project Form.
    /// </summary>
    public class MDSpoutProjectDowncomerDataViewModel : ViewModelBase
    {
        public MDSpoutProjectViewDowncomerData mDSpoutProjectViewDowncomerData;

        public MDSpoutProjectDowncomerDataViewModel()
        {
            mDSpoutProjectViewDowncomerData = new MDSpoutProjectViewDowncomerData();
        }

        public int NoOfDowncomers
        {
            get
            {
                return mDSpoutProjectViewDowncomerData.NoOfDowncomers;
            }
            set
            {
                mDSpoutProjectViewDowncomerData.NoOfDowncomers = value;
                NotifyPropertyChanged("NoOfDowncomers");
            }
        }

        public double TotalDCArea
        {
            get
            {
                return mDSpoutProjectViewDowncomerData.TotalDCArea;
            }
            set
            {
                mDSpoutProjectViewDowncomerData.TotalDCArea = value;
                NotifyPropertyChanged("TotalDCArea");
            }
        }

        public double DowncomerWidth
        {
            get
            {
                return mDSpoutProjectViewDowncomerData.DowncomerWidth;
            }
            set
            {
                mDSpoutProjectViewDowncomerData.DowncomerWidth = value;
                NotifyPropertyChanged("DowncomerWidth");
            }
        }

        public double CenterToCenter
        {
            get
            {
                return mDSpoutProjectViewDowncomerData.CenterToCenter;
            }
            set
            {
                mDSpoutProjectViewDowncomerData.CenterToCenter = value;
                NotifyPropertyChanged("CenterToCenter");
            }
        }

        public double TotalWeirLength
        {
            get
            {
                return mDSpoutProjectViewDowncomerData.TotalWeirLength;
            }
            set
            {
                mDSpoutProjectViewDowncomerData.TotalWeirLength = value;
                NotifyPropertyChanged("TotalWeirLength");
            }
        }

        public double InsideDCHeight
        {
            get
            {
                return mDSpoutProjectViewDowncomerData.InsideDCHeight;
            }
            set
            {
                mDSpoutProjectViewDowncomerData.InsideDCHeight = value;
                NotifyPropertyChanged("InsideDCHeight");
            }
        }

        public double DowncomerMatrl
        {
            get
            {
                return mDSpoutProjectViewDowncomerData.DowncomerMatrl;
            }
            set
            {
                mDSpoutProjectViewDowncomerData.DowncomerMatrl = value;
                NotifyPropertyChanged("DowncomerMatrl");
            }
        }

        /// Edit command
        /// </summary>
        private DelegateCommand editDowncomerDataCommand;
        public DelegateCommand EditDowncomerDataCommand
        {
            get
            {
                if (editDowncomerDataCommand is null) editDowncomerDataCommand = new DelegateCommand(param => EditDowncomerData());
                return editDowncomerDataCommand;
            }
        }
        /// <summary>
        /// Edit Deck Data
        /// </summary>
        private void EditDowncomerData()
        {

        }
    }
}
