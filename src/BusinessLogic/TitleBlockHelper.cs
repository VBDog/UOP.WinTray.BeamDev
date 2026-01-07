using UOP.DXFGraphicsControl;
using UOP.DXFGraphics;
using System.Collections.Generic;
using System.IO;
using UOP.WinTray.Projects;

namespace UOP.WinTray.UI.BusinessLogic
{
    public class TitleBlockHelper
    {
        public TitleBlockHelper()
        {
            IsValid = false;
            Attributes = new Dictionary<string, string>();
            InsertionPoint = dxfVector.Zero;
        }
        public TitleBlockHelper (string pSizeLetter, dxfAttributes pAttributes, dxfVector pInsertionPoint, double pScale)
        {
            IsValid = false;
            Attributes = new Dictionary<string, string>();
            InsertionPoint = dxfVector.Zero;
            uopPropertyArray appprops = appApplication.PropertyArray;
            string fname;
            string folder = appApplication.BordersFolder;
            switch (pSizeLetter)
            {
                case "D":
                    fname = $"{appApplication.DBorder}";
                    break;
                case "B":
                    fname = $"{appApplication.BBorder}";
                    
                    break;
                default:
                    fname =  $"{pSizeLetter}-Border.dwg";
                    break;
            }
            FileName = Path.Combine(folder, fname);

            if (!File.Exists( FileName )) return;
            IsValid = true;
            if (pAttributes != null)
            {
                for (int i = 1; i < pAttributes.Count; i++)
                {
                    string tag = pAttributes.Tag(i );
                    string val = pAttributes.GetValue(  tag );
                    Attributes[ tag ] = val;
                }
            }
            if (pInsertionPoint != null) InsertionPoint = pInsertionPoint.Clone();
            Scale = pScale;
            if (Scale == 0) Scale = 1;
        }

        public TitleBlockHelper(TitleBlockHelper source)
        {
            IsValid = false;
            Attributes = new Dictionary<string, string>();
            if (source == null) return;
            IsValid = source.IsValid;
            Attributes = new Dictionary<string, string>( source.Attributes );
            FileName = source.FileName;
            InsertionPoint = (source.InsertionPoint != null) ? source.InsertionPoint.Clone() : dxfVector.Zero ;
            Scale = source.Scale;
        }

        private Dictionary<string, string> Attributes { get; set; }
        private string FileName { get; set; }
        private double Scale { get; set; }
        private dxfVector InsertionPoint { get; set; }
        public bool IsValid { get; set; }

        public void Insert(DXFViewer viewer)
        {
            viewer?.SetTitleBlock( FileName, Scale, InsertionPoint.X, InsertionPoint.Y, Attributes );
        }

        public TitleBlockHelper Clone()
        {
            return new TitleBlockHelper( this );
        }
    }
}
