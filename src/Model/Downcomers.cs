namespace UOP.WinTray.UI.Model
{
    /// <summary>
    /// Model Class for the Downcomers Data Grid of the MD Spout Project form.
    /// </summary>
    public class Downcomers
    {
        public CellInfo No { get; set; }
        public CellInfo WeirLength { get; set; }
        public CellInfo IdealArea { get; set; }
        public CellInfo ActualArea { get; set; }
        public CellInfo Err { get; set; }
      
        public int Index { get; set; }

        // Added by CADfx
        public CellInfo PN { get; set; }
        public CellInfo BoxLength { get; set; }
        public CellInfo FldWeir { get; set; }
        public CellInfo SuplDefl { get; set; }
        public CellInfo Gussets { get; set; }
        // Added by CADfx
    }
}
