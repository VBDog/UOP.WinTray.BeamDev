using System.Collections.Generic;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.Projects.Events
{

    class Message_ProjectRequest
    {
        public Message_ProjectRequest(string aProjectHandle)
        {
            ProjectHandle = aProjectHandle;
        }
        public string ProjectHandle { get; set; }
       public uopProject Project { get; set; }
    }

    class Message_ColumnRequest
    {
        public Message_ColumnRequest(string aColumnHandle)
        {
            ColumnHandle = aColumnHandle;
      
        }
        public string ColumnHandle { get; set; }
        public uopColumn Column{ get; set; }
        
 

    }

    class Message_RangeRequest
    {
        public Message_RangeRequest(string aRangeGUID)
        {
            RangeGUID = aRangeGUID;
          
        }
        public string RangeGUID { get; set; }
        public uopTrayRange Range { get; set; }


    }
    class Message_PartsInvalidated
    {
        public Message_PartsInvalidated()
        {
            RangeGUID = string.Empty;
            PartTypes = new List<uppPartTypes>();
            ProjectHandle = string.Empty;
            InvalidateAll = false;
        }

        public Message_PartsInvalidated(string aProjectHandle, string aRangeGUID, bool bInvalidateAll = false,  uppPartTypes? aPartType = null, List<uppPartTypes> aPartTypes = null)
        {
            RangeGUID = aRangeGUID != null ? aRangeGUID.Trim() : string.Empty;
            PartTypes = new List<uppPartTypes>();
             ProjectHandle = aProjectHandle;
            InvalidateAll = bInvalidateAll;
            if (aPartType.HasValue)  PartTypes.Add(aPartType.Value); 
        }
        public string RangeGUID { get; set; }
        public string ProjectHandle { get; set; }
      

        public List<uppPartTypes> PartTypes { get; set; }

        public bool InvalidateAll { get; set; }

        public bool ContainsPartType(uppPartTypes aPartType) => PartTypes.Contains(aPartType);
        public bool ContainsPartType(List<uppPartTypes> aPartTypes)
        {
            return uopUtils.PartListContainsPartType(PartTypes, aPartTypes);
        }
    }

}
