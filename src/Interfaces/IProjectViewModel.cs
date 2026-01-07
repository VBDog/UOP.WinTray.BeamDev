using UOP.DXFGraphicsControl;
using UOP.DXFGraphics;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.UI.Model;
using UOP.WinTray.UI.Messages;

namespace UOP.WinTray.UI.Interfaces
{
    public interface IProjectViewModel
    {

        public abstract void ReleaseGraphics();

  
        public abstract DXFViewer DXFViewer { get; set; }

        public abstract dxfImage InputSketch { set; }

        public abstract uopProject Project { get; set; }

        public abstract mdProject MDProject { get; set; }

        public abstract bool Disabled {get; set;}

    public abstract void CloseAllDocuments();
        public abstract bool UpdateTrayList(string aSelectedGUID);

        public abstract void ToggleViewerVisibility(bool bVisible, bool? bShowWorking = null);

        public abstract uppProjectFamilies ProjectFamily { get; }

        public abstract string WarningColor { get; set; }

        public abstract void RespondToNodeClick(TreeViewNode selectedNode);

    
        public abstract void Dispose();
    }
}
