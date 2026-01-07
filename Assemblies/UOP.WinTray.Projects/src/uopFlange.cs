using System;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Structures;

namespace UOP.WinTray.Projects
{
    public class uopFlange : ICloneable
    {

        private TFLANGE tStruc;
        
        #region Constructors

        public uopFlange() { tStruc = new TFLANGE(); }
        
        internal uopFlange(TFLANGE aStructure) { tStruc = aStructure; }

        internal TFLANGE Structure { get => tStruc; set => tStruc = value; }

        #endregion

        public uopFlange Clone() => new uopFlange(tStruc.Clone());

        object ICloneable.Clone() => (object)this.Clone();

        public double BoltCircle => tStruc.BoltCircle;
        
        public uppFlangeTypes FlangeType => tStruc.FlangeType;
        
        public string FlangeTypeName => tStruc.FlangeTypeName;

        public string BoltSize_Metric => tStruc.BoltSize_Metric;

        public string BoltSize_UNC => tStruc.BoltSize_UNC;

        public double Bore => tStruc.Bore;

        public int DBID => tStruc.DBID;
       
        public string Descriptor => tStruc.Descriptor;
 
        public string Family => tStruc.Family;

        public string GroupName => tStruc.GroupName;

        public double  H => tStruc.H;

        public int HoleCount => tStruc.HoleCount;

        public double HoleDia => tStruc.HoleDia;
         
        public double L => tStruc.L;

        public string Material { get => tStruc.Material; set => tStruc.Material = value; }

        public double MinStud_Metric => tStruc.MinStud_Metric;

        public double MinStud_UNC => tStruc.MinStud_UNC;
 
        public double OD => tStruc.OD;

        public int Quantity { get => tStruc.Quantity; set => tStruc.Quantity = value; }

        public double R => tStruc.R;

        public int Rating => tStruc.Rating;

        public double Size => tStruc.Size;

        public string Spec { get => tStruc.Spec; set => tStruc.Spec = value; }

        public double  StudLength { get => tStruc.StudLength; set => tStruc.StudLength = value; }

        public string StudSize { get => tStruc.StudSize; set => tStruc.StudSize = value; }
        
        public double Thickness => tStruc.Thickness;

        public int TotalStuds { get => tStruc.TotalStuds; set => tStruc.TotalStuds = value; }
       
        public double Weight => tStruc.Weight;
       
        public double  X => tStruc.X;

    }
}
