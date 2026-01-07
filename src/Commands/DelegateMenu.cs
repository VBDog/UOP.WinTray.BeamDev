using System.Windows.Controls;

namespace UOP.WinTray.UI.Commands
{
    public class DelegateMenu :ContextMenu
    {
        public DelegateMenu(string aName)
        {
            base.Name = aName;
        }
    }
}
