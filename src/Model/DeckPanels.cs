namespace UOP.WinTray.UI.Model
{
    /// <summary>
    /// Model Class for Deck Panels Data Grid of the MD Spout Project Form.
    /// </summary>
    public class DeckPanels
    {
        /// <summary>
        /// TODO No
        /// </summary>
        public CellInfo No { get; set; }
        /// <summary>
        /// FBA TODO
        /// </summary>
        public CellInfo FBA { get; set; }
        /// <summary>
        /// WL TODO
        /// </summary>
        public CellInfo WL { get; set; } 

        public CellInfo VolumeErr { get; set; }

        public CellInfo IdealArea { get; set; }

        public CellInfo ActualArea { get; set; }
       
        public CellInfo Err { get; set; }

        // Added by CADfx
        public CellInfo PN { get; set; }
        public CellInfo Width { get; set; }
        public CellInfo Height { get; set; }
        public CellInfo Mnwy { get; set; }
        // Added by CADfx
    }
}
