using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UOP.WinTray.UI.ViewModels.CADfx.Shared
{
    public class YValuesEditorViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        private ObservableCollection<string> yValues;
        public ObservableCollection<string> YValues { 
            get
            {
                return yValues;
            }

            set
            {
                yValues = value;
                OnPropertyChanged();
            }
        }

        private string selectedYValue;
        public string SelectedYValue
        {
            get
            {
                return selectedYValue;
            }

            set
            {
                selectedYValue = value;
                OnPropertyChanged();
            }
        }

        private bool addMirrorValues;
        public bool AddMirrorValues
        {
            get
            {
                return addMirrorValues;
            }

            set
            {
                addMirrorValues = value;
                OnPropertyChanged();
            }
        }

        public YValuesEditorViewModel(IEnumerable<string> inputYValues = null)
        {
            yValues = new ObservableCollection<string>();
            if (inputYValues != null)
            {
                foreach (var value in inputYValues)
                {
                    yValues.Add(value);
                }
            }
        }
    }
}
