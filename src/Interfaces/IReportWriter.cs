using UOP.WinTray.Projects.Documents;

namespace UOP.WinTray.UI.Interfaces
{
    /// <summary>
    /// Interface for report writer operations
    /// </summary>
    public interface IReportWriter
    {
        public bool PreValidateReportWriter(out string rBrief, out string rError);
        public string GenerateReport(uopDocReport aReport);
        public void MarkRevisions(uopDocReport aReport);

        public uopDocReport Report { get; set; }
        public void TerminateReferences();

        public string FileSpec { get;  }
        public bool FatalError { get; }
        public bool CanMarkRevision { get; }
    }
}
