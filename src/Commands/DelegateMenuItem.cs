using UOP.WinTray.UI.ViewModels;
using System;
using System.Windows.Controls;

namespace UOP.WinTray.UI.Commands
{
    public class DelegateMenuItem : MenuItem
    {
        public DelegateMenuItem(string aHeader, DelegateCommand aCommand)
        {
            base.Command = aCommand;
            base.Header = aHeader;
        }

        public DelegateMenuItem(string aHeader, DelegateCommand aCommand, UOPDocumentTab aDocTab)
        {
            base.Command = aCommand;
            base.Header = aHeader;
            DocumentTab = aDocTab;
        }

        private WeakReference<UOPDocumentTab> _DocTabRef;
        public UOPDocumentTab DocumentTab 
        {
            get 
            {
                if (_DocTabRef == null) return null;
                _DocTabRef.TryGetTarget(out UOPDocumentTab _rVal);
                if (_rVal == null) _DocTabRef = null;
                return _rVal;
            }

            set
            {
                if(value == null)
                {
                    _DocTabRef = null;
                    return;
                }
                _DocTabRef = new WeakReference<UOPDocumentTab>(value);
            }
        }

    }
}
