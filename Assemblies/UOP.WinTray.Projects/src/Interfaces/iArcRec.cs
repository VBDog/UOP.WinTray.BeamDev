using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Enums;

namespace UOP.WinTray.Projects.Interfaces
{
    public interface iArcRec
    {
        public uppArcRecTypes Type { get; }
     
        uopArc Arc { get; }
        uopRectangle Rectangle { get; }
        uopShape Slot { get; }

        double Rotation { get; }
        double Height { get; }
        double Width { get; }
        double Radius { get; }

        double X { get; }
        double Y { get; }

        string Tag { get; }
        string Flag { get; }
        uopVector Center { get; }

        bool Suppressed { get; }

        bool ContainsVector(iVector aVector, bool bOnIsIn = true, int aPrecis = 5, bool bTreatAsInfinite = false);

        public bool Contains(iArcRec bArcRec, bool bOnIsIn = true, int aPrecis = 4, bool bMustBeCompletelyWithin = false, bool bReturnTrueByCenter = false);

        iArcRec Clone();
    }
}
