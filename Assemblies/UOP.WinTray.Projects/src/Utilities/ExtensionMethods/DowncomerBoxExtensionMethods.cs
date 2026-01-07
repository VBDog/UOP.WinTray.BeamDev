using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Structures;

namespace UOP.WinTray.Projects.src.Utilities.ExtensionMethods
{
    public static class DowncomerBoxesExtensionMethods
    {
        internal static UHOLEARRAY GenHolesV(this IEnumerable<mdDowncomerBox> boxes, mdTrayAssembly aAssy, string aTag = "", string aFlag = "", bool? bSuppressSpouts = null, string aSide = "", string aTags = "")
        {
            UHOLEARRAY allHoles =UHOLEARRAY.Null;;
            UHOLEARRAY currentBoxHoles;

            foreach (var box in boxes)
            {
                currentBoxHoles = box.GenHolesV(aAssy, aTag, aFlag, bSuppressSpouts, aSide, aTags);

                allHoles.Append(currentBoxHoles, true);
            }

            return allHoles;
        }

        public static uopHoleArray GenHoles(this IEnumerable<mdDowncomerBox> boxes, mdTrayAssembly aAssy, string aTag = "", string aFlag = "", bool? bSuppressSpouts = null, string aSide = "", string aTags = "")
        {
            return new uopHoleArray(boxes.GenHolesV(aAssy, aTag, aFlag, bSuppressSpouts, aSide, aTags));
        }

        public static double Weight(this IEnumerable<mdDowncomerBox> boxes, mdDowncomer aDC, mdTrayAssembly aAssy)
        {
            double totalWeight = 0;

            foreach (var box in boxes)
            {
                totalWeight += box.Weight(aDC, aAssy);
            }

            return totalWeight;
        }

        internal static ULINE EndLn(this IEnumerable<mdDowncomerBox> boxes, bool bBottom = false, double aOffset = 0)
        {
            if (boxes == null || boxes.Count() == 0)
            {
                throw new ArgumentException($"EndLn received {(boxes == null ? "null" : "empty")} \"boxes\" argument!");
            }

            mdDowncomerBox box = bBottom ? boxes.Last() : boxes.First();
            
            return box.EndLn(bBottom, aOffset);
        }
    }
}
