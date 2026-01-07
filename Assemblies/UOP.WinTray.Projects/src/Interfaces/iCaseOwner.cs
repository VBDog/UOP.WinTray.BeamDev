using System.Collections.Generic;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Parts;


namespace UOP.WinTray.Projects.Interfaces
{
    public interface iCaseOwner
    {
        public int CaseCount { get; }
        public colUOPParts Cases { get; set; }
        public List<iCase> CaseList { get; set; }
        public uopProperties CurrentProperties();
        
        public string Description { get; set; }
        public string DescriptiveName{ get; set; }
        public int Index { get; }
        public string Name { get; }
        public string NozzleLabel { get; set; }
        public string ObjectType { get; }
      
        public string PartPath { get; }
        public string TrayAbove { get; set; }
        public string TrayBelow { get; set; }
        public iCaseOwner Clone();
        public iCase GetCase(int aCaseIndex);
        public dynamic GetPropertyValue(dynamic aPropertyNameorIndex, out bool bExists);
        public bool PropValSet(dynamic aPropertyNameorIndex, dynamic aNewValue);

        public uppCaseOwnerOwnerTypes OwnerType { get; }

        public mdProject MDProject { get; }

        public bool AddCase(iCase aCase);

        public bool ReNameCase(string aCurrentName, string aNewName);

        public bool SetCurrentProperties(uopProperties aProperties);

    

    }
}
