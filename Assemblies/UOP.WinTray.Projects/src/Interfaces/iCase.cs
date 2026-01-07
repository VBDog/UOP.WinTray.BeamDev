using UOP.WinTray.Projects.Enums;

namespace UOP.WinTray.Projects.Interfaces
{
    public interface iCase
    {
        public int Index { get; set; }
        public uopProperties CurrentProperties(); // set; }
        public string Description { get; set; }
        public string ProjectHandle { get; set; }
        public double MaximumOperatingRange { get; set; }
        public double MinimumOperatingRange { get; set; }
        public string Name { get; }
        public string Handle { get; }
        public string ObjectType { get; }
        public int OwnerIndex { get; }
     
        public string PartPath { get; }
        public iCase Clone();
        public dynamic GetPropertyValue(dynamic aPropertyNameorIndex, out bool bExists);
        public bool PropValSet(dynamic PropertyNameorIndex, dynamic NewValue);
        public void RecalculateDependantValues();
        public uopProperties ReportProperties(dynamic rTray, uppReportTypes aReportType);
        public abstract bool SetCurrentProperties(uopProperties value);
        public abstract uppCaseOwnerOwnerTypes OwnerType { get; }
        public uppPartTypes PartType { get; }
      
    }
}


