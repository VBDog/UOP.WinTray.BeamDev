using UOP.DXFGraphics;
using System.Collections.Generic;
using UOP.WinTray.Projects.Documents;
using UOP.WinTray.Projects.Parts;

using UOP.WinTray.Projects.Tables;
using uppSides = UOP.WinTray.Projects.Enums.uppSides;
using uppUnitFamilies = UOP.WinTray.Projects.Enums.uppUnitFamilies;
using UOP.WinTray.Projects.Interfaces;

namespace UOP.WinTray.Projects.Utilities
{
    /// <summary>
    ///A UOP Utilities object that provides various common function to members of the DLL
    ///and it's client application.
    /// </summary>
    public class mdUtilities
    {

      


   

        /// <summary>
        /// executed internally to create the finger clips collection
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aStiffeners"></param>
        /// <param name="aDCIndex"></param>
        /// <param name="aPanelIndex"></param>
        /// <param name="aSide"></param>
        /// <returns></returns>
        /// 
        public colUOPParts GenerateFingerClips(mdTrayAssembly aAssy, List<mdStiffener> aStiffeners = null, int aDCIndex = 0, int aPanelIndex = 0, uppSides aSide = uppSides.Undefined)
        => mdUtils.FingerClips(aAssy, aStiffeners, aDCIndex, aPanelIndex, aSide);

       


 

        /// <summary>
        /// creates the html for the passed calculation based on the properties of the tray assembly and the type of Calc that is passed.
        /// </summary>
        /// <param name="Calc">the calculation object that should be completed</param>
        /// <param name="aAssy">the MD Tray assembly to get the calculations for</param>
        public void GetCalculation(uopDocCalculation Calc, mdTrayAssembly aAssy)
        {
            //TODO
            //uopcalculations.md_GetCalculation(ref Calc, aAssy); //making calc as ref as its value is changing in md_GetCalculation method 
        }

        public uopTable GetTable(mdTrayAssembly aAssy, string aTableName, Enums.uppUnitFamilies aUnits, int MinRows = 0, colMDDowncomers aDowncomers = null, colMDSpoutGroups aSpoutGroups = null, colMDDeckPanels aDeckPanels = null)
        {
            return mdTables.GetTable(aAssy, aTableName, aUnits, MinRows, aDowncomers, aSpoutGroups, aDeckPanels);
        }



        /// <summary>
        /// sorts the stiffeners in the collection in top to bottom
        /// </summary>
        /// <param name="aStiffeners"></param>
        /// <param name="aAssy"></param>
        public void SortStiffeners(colUOPParts aStiffeners, mdTrayAssembly aAssy)
        => mdUtils.StiffenersSort(aStiffeners, aAssy);


        /// <summary>
        /// returns a collection of strings that are the names of the available spout patterns
        /// </summary>
        /// <returns></returns>
        /// To Do
        public List<string> SpoutPatternNames() => new List<string>() { "By Default", "D", "C", "B", "A", "*A*", "S3", "S2", "S1", "*S*" };


        public List<double> GenerateStiffenerOrdinates(double aSpace, mdDowncomer aDowncomer, mdTrayAssembly aAssy, bool bStraddle = false, bool bBestFit = false)
        {
            return mdUtils.StiffenersGenerate(aSpace, aDowncomer, aAssy, bStraddle, bBestFit);

        }






    }
}
