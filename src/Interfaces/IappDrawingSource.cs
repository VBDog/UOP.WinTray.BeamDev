using System;
using System.Drawing;
using UOP.DXFGraphics;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Documents;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.UI.BusinessLogic;

namespace UOP.WinTray.UI.Interfaces
{
    public interface IappDrawingSource
    {
        public delegate void StatusChangeHandler(string StatusString);
        public event StatusChangeHandler StatusChange;

        public dxfImage GenerateImage(uopDocDrawing argDrawing, bool bSuppressErrors = false, System.Drawing.Size aImageSize = new System.Drawing.Size(), uopTrayRange preSelectedTrayRange = null, bool bUsingViewer = true, Color? BackColor = null, bool bSuppressIDEEffects = false);

        public uopDocDrawing Drawing { get; }
        public dxfImage Image { get; set; }
        public abstract uopProject Project { get; set; }
        public abstract uopTrayRange Range { get; set; }
        public abstract dxfBlockSource BlockSource { get; set; }
        public abstract bool CancelOps { get; set; }
        public bool DrawinginProgress { get; set; }

        public bool SuppressWorkStatus { get; set; }

        public bool SuppressBorder { get; set; }

        public abstract TitleBlockHelper TitleHelper { get; }
        public abstract string Status { get; set; }

        public abstract mdTrayAssembly MDAssy { get; }
        public abstract void HandleError(System.Reflection.MethodBase aMethod, Exception e);
        public abstract void TerminateObjects();

        public abstract bool DrawLayoutRectangles {get;}
    }


}
