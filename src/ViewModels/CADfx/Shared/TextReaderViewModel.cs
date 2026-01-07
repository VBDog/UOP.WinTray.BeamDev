using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UOP.WinTray.UI.ViewModels.CADfx.Shared
{
    public class TextReaderViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        private string unit;
        public string Unit
        {
            get
            {
                return unit;
            }

            set
            {
                if (unit != value)
                {
                    unit = value;
                    OnPropertyChanged();
                }
            }
        }

        private string label;
        public string Label
        {
            get
            {
                return label;
            }

            set
            {
                if (label != value)
                {
                    label = value;
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
