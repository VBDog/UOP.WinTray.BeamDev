using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UOP.WinTray.UI.ViewModels.CADfx.Shared
{
    public class DowncomerSelectorViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion INotifyPropertyChanged Implementation

        private ObservableCollection<DownComerSelectorItem> downcomers;
        public ObservableCollection<DownComerSelectorItem> Downcomers {
            get
            {
                return downcomers;
            }

            set
            {
                downcomers = value;
                OnPropertyChanged();
            }
        }

        public DowncomerSelectorViewModel(IEnumerable<DownComerSelectorItem> inputDowncomers = null)
        {
            downcomers = new ObservableCollection<DownComerSelectorItem>();
            if (inputDowncomers != null)
            {
                foreach (var downcomer in inputDowncomers)
                {
                    downcomers.Add(downcomer);
                }
            }
        }
    }

    public class DownComerSelectorItem : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        private bool isDowncomerChecked;
        public bool IsDowncomerChecked
        {
            get
            {
                return isDowncomerChecked;
            }

            set
            {
                if (isDowncomerChecked != value)
                {
                    isDowncomerChecked = value;
                    OnPropertyChanged(); 
                }
            }
        }

        private string text;
        public string Text
        {
            get
            {
                return text;
            }

            set
            {
                if (text != value)
                {
                    text = value;
                    OnPropertyChanged(); 
                }
            }
        }
    }
}
