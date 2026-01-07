using System.Windows.Forms;

namespace UOP.WinTray.UI.Interfaces
{
    /// <summary>
    ///  Handles context menu for the panel.
    /// </summary>
    public interface IContextMenu
    {
        ContextMenuStrip SetContextMenu();
        ContextMenuStrip SetContextMenuForEditSpoutAreaProperties();
        ContextMenu AddRightClickOptionForStartupSpout();
    }
}