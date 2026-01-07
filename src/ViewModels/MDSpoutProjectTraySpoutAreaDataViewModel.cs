using System;
using System.Collections.Generic;
using System.Text;
using Honeywell.UOP.WinTray.Model;

namespace Honeywell.UOP.WinTray.ViewModel
{
    /// <summary>
    /// View Model Class for Tray Spout Area Data Section of the MD Spout Project Form.
    /// </summary>
    public class MDSpoutProjectTraySpoutAreaDataViewModel : ViewModelBase
    {
        public MDSpoutProjectViewTraySpoutAreaData mDSpoutProjectViewTraySpoutAreaData;

        public MDSpoutProjectTraySpoutAreaDataViewModel()
        {
            mDSpoutProjectViewTraySpoutAreaData = new MDSpoutProjectViewTraySpoutAreaData();
        }
      
        public double IdealSpoutArea
        {
            get
            {
                return mDSpoutProjectViewTraySpoutAreaData.IdealSpoutArea;
            }
            set
            {
                mDSpoutProjectViewTraySpoutAreaData.IdealSpoutArea = value;
                NotifyPropertyChanged("IdealSpoutArea");
            }
        }

        public double ActualSpoutArea
        {
            get
            {
                return mDSpoutProjectViewTraySpoutAreaData.ActualSpoutArea;
            }
            set
            {
                mDSpoutProjectViewTraySpoutAreaData.ActualSpoutArea = value;
                NotifyPropertyChanged("ActualSpoutArea");
            }
        }

        public double ErrorPercentage
        {
            get
            {
                return mDSpoutProjectViewTraySpoutAreaData.ErrorPercentage;
            }
            set
            {
                mDSpoutProjectViewTraySpoutAreaData.ErrorPercentage = value;
                NotifyPropertyChanged("ErrorPercentage");
            }
        }
    }
}
