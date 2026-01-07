namespace UOP.WinTray.UI.Model
{
    /// <summary>
    /// Model class for Spout Groups Data Grid of the MD Spout Project form.
    /// </summary>
    public class SpoutGroup
    {
        public int Index { get; set; }

        public CellInfo Handle { get; set; }


        public CellInfo No { get; set; }

        public CellInfo SpoutCount { get; set; }

        public CellInfo PatType { get; set; }

        public CellInfo SpoutLength { get; set; }

        public CellInfo SPR { get; set; }

        public CellInfo VertPitch { get; set; }

        public CellInfo PatLength { get; set; }

        public CellInfo ActualMargin { get; set; }

        public CellInfo IdealArea { get; set; }

        public CellInfo  ActualArea { get; set; }
        
        public CellInfo Err { get; set; }
       
    }
}
