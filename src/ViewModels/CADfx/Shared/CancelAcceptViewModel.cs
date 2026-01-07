using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UOP.WinTray.UI.ViewModels.CADfx.Shared
{
    public class CancelAcceptViewModel : INotifyPropertyChanged, ICancelAcceptHandler
    {
        #region INotifyPropertyChanged Implamentation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        public ICancelAcceptHandler ParentCancelAcceptHandler { get; set; }

        public void ButtonPushed(CancelAcceptButton button)
        {
            ParentCancelAcceptHandler?.ButtonPushed(button);
        }
    }
}
