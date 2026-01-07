using System;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Parts;


namespace UOP.WinTray.Projects.Events
{
    internal class uopEvents
    {
        static EventAggregator _Aggregator;

        internal static EventAggregator Aggregator { get { _Aggregator ??= new EventAggregator(); return _Aggregator; } }


        public static void NotifyProject(string projectHandle, uopProperty property)
        {

            if (property == null) return;
            uopProject project = uopEvents.RetrieveProject(projectHandle);
            if (project != null)
            {
                project.Notify(property);
            }

        }

        public static uopProject RetrieveProject(string aProjectHandle)
        {

            if (string.IsNullOrWhiteSpace(aProjectHandle)) return null;

            Message_ProjectRequest message = new Message_ProjectRequest(aProjectHandle);
            Aggregator.Publish<Message_ProjectRequest>(message);
            return message.Project;
        }


      

        /// <summary>
        /// Retrieve Distributor
        /// </summary>
        /// <param name="aProjectHandle"></param>
        /// <param name="aPartIndex"></param>
        /// <returns>returns the indicated distributor from the distributors collection of the indicated project</returns>
        public static uopPart RetrieveDistributor(string aProjectHandle, int aPartIndex)
        {

            if (String.IsNullOrWhiteSpace(aProjectHandle) || aPartIndex <= 0) return null;
            uopProject Proj = uopEvents.RetrieveProject(aProjectHandle);
            if (Proj == null) return null;

            uopParts Distribs = Proj.Distributors;

            if (Distribs == null) return null;
            return (aPartIndex <= Distribs.Count) ? Distribs.Item(aPartIndex) : null;


        }
        /// <summary>
        /// Retrieve Distributors
        /// </summary>
        /// <param name="aProjectHandle"></param>
        /// <returns>returns the distributors collection of the indicated project</returns>
        public static colMDDistributors RetrieveDistributors(string aProjectHandle)
        {

            uopProject Proj = uopEvents.RetrieveProject(aProjectHandle);

            return Proj?.Distributors;

        }

        public static mdProject RetrieveMDProject(string aProjectHandle)
        {
            uopProject aProj = uopEvents.RetrieveProject(aProjectHandle);
            if (aProj == null) return null;
            return (aProj.ProjectFamily == uppProjectFamilies.uopFamMD) ? (mdProject)aProj : null;
        }

        public static uopColumn RetrieveColumn(string aColumnHandle)
        {

            if (string.IsNullOrWhiteSpace(aColumnHandle)) return null;

            Message_ColumnRequest message = new Message_ColumnRequest(aColumnHandle);
            Aggregator.Publish<Message_ColumnRequest>(message);
            return message.Column;
        }

        public static colUOPTrayRanges RetrieveRanges(string aColumnHandle)
        {

            if (string.IsNullOrWhiteSpace(aColumnHandle)) return null;

            Message_ColumnRequest message = new Message_ColumnRequest(aColumnHandle);
            Aggregator.Publish<Message_ColumnRequest>(message);
            return message.Column?.TrayRanges;
        }

        public static uopTrayAssembly RetrieveTrayAssembly(string aRangeGUID)
        {
        
            uopTrayRange rRange = uopEvents.RetrieveRange(aRangeGUID);

            return rRange?.Assembly;
           
        }

      

        public static void SetProjectReadStatus(string aHandle, string aStatusString, int aIndex = 0)
        {
            uopProject aProj = uopEvents.RetrieveProject(aHandle);
            if (aProj != null) aProj.ReadStatus(aStatusString, aIndex);
        }
        public static void SetPartGenenerationStatus(string aHandle, string aStatusString, bool? bBegin = null)
        {
            uopProject aProj = uopEvents.RetrieveProject(aHandle);
            if (aProj != null) aProj.PartGenenerationStatus(aStatusString, bBegin);
    
        }
        public static uopPart RetrieveDeckPanel(string aRangeGUID, int aPanelIndex, uopTrayAssembly aAssy = null)
        {
         
            var Panels =  RetrieveDeckPanels( aRangeGUID, aAssy);
            if (Panels == null) return null;
            
            return (aPanelIndex > 0) ?  Panels.Item(aPanelIndex) : null;
            
        }

        public static uopParts RetrieveDeckPanels(string aRangeGUID, uopTrayAssembly aAssy = null)
        {
            if (aAssy == null) aAssy = uopEvents.RetrieveTrayAssembly(aRangeGUID);
            return aAssy?.DeckPanelsObj;

        }
        public static uopTrayRange RetrieveRange(string aRangeGUID)
        {

            if (string.IsNullOrWhiteSpace(aRangeGUID)) return null;

            Message_RangeRequest message = new Message_RangeRequest(aRangeGUID);
            Aggregator.Publish<Message_RangeRequest>(message);
            return message.Range;
        }

        /// <summary>
        ///#1the handle of the desired project
        ///#2the index of the desired column
        ///#3the GUID of the desired range
        /// </summary>
        /// <param name="aRangeGUID"></param>
        /// <returns>returns the tray range object of the indicated project and column</returns>
        public static mdTrayRange RetrieveMDRange(string aRangeGUID)
        {
            uopTrayRange aRange = uopEvents.RetrieveRange(aRangeGUID);
            return (aRange != null && aRange.ProjectFamily == uppProjectFamilies.uopFamMD) ? (mdTrayRange)aRange : null;
        }

        public static mdTrayAssembly RetrieveMDTrayAssembly(string aRangeGUID)
        {
         
            mdTrayRange rRange = uopEvents.RetrieveMDRange(aRangeGUID);
            return rRange?.TrayAssembly;
         
        }

        /// <summary>
        /// Retrieve Spout Groups
        /// </summary>
      
        /// <param name="aRangeGUID"></param>
        /// <param name="aMDAssy"></param>
        /// <returns>^returns the downcomers collection of the indicated project and tray range</returns>
        public static dynamic RetrieveSpoutGroups(string aRangeGUID)
        {

            mdTrayAssembly aMDAssy =  uopEvents.RetrieveMDTrayAssembly(aRangeGUID);
            return aMDAssy?.SpoutGroups;
        }

        /// <summary>
        /// Retrieve Downcomer
        /// </summary>
        /// <param name="aProjectHandle"></param>
        /// <param name="aColumnIndex"></param>
        /// <param name="aRangeGUID"></param>
        /// <param name="aDowncomerIndex"></param>
        /// <param name="aAssy"></param>
        /// <returns>returns the indicated downcomer of the indicated project and tray range</returns>
        public static mdDowncomer RetrieveDowncomer( string aRangeGUID, int aDowncomerIndex, mdTrayAssembly aAssy = null)
        {

            if (aAssy == null) aAssy = uopEvents.RetrieveMDTrayAssembly(aRangeGUID);
            return aAssy?.Downcomer(aDowncomerIndex);

        }

        /// <summary>
        /// Retrieve Downcomers
        /// </summary>
        /// <param name="aProjectHandle"></param>
        /// <param name="aColumnIndex"></param>
        /// <param name="aRangeGUID"></param>
        /// <param name="aAssy"></param>
        /// <returns>returns the downcomers collection of the indicated project and tray range</returns>
        public static colMDDowncomers RetrieveDowncomers( string aRangeGUID, mdTrayAssembly aAssy = null)
        {
            aAssy ??= uopEvents.RetrieveMDTrayAssembly(aRangeGUID);

            return aAssy?.Downcomers;
        }

    }
}
