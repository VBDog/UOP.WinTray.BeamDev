using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UOP.WinTray.UI
{
    /// <summary>
    /// Base class that provide chanage notification.
    /// </summary>
    public abstract class BindableBase : INotifyPropertyChanged
    {
        public virtual event PropertyChangedEventHandler PropertyChanged = delegate { };
        protected virtual void SetProperty<T>(ref T member, T val,
           [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(member, val)) return;

            member = val;
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
