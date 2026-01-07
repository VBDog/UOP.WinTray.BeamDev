using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.Projects
{
    public class uopVectorProperties
    {

        #region Constructors
        public uopVectorProperties() => Init();

        public uopVectorProperties(uopVectorProperties aProperties) => Init(aProperties);
        public uopVectorProperties(uopVector aVector) => Init(null,aVector);

        public uopVectorProperties(dxfVector aVector) => Init(null, aVector);

        public uopVectorProperties(uopHole aHole) => Init(null, aHole);

        private void Init(uopVectorProperties aProperties = null, iVector aVector = null)
        {
            X = aProperties == null ? null : aProperties.X ;
            Y  = aProperties == null ? null : aProperties.Y ;
            Value  = aProperties == null ? null : aProperties.Value ;
            Row  = aProperties == null ? null : aProperties.Row ;
            Col  = aProperties == null ? null : aProperties.Col ;
            PartIndex  = aProperties == null ? null : aProperties.PartIndex ;
            Index  = aProperties == null ? null : aProperties.Index ;
            Tag  = aProperties == null ? null : aProperties.Tag ;
            Flag  = aProperties == null ? null : aProperties.Flag ;
            Suppressed  = aProperties == null ? null : aProperties.Suppressed ;
            Radius  = aProperties == null ? null : aProperties.Radius ;
            Proximity  = aProperties == null ? null : aProperties.Proximity ;
            Inset  = aProperties == null ? null : aProperties.Inset ;
            DownSet  = aProperties == null ? null : aProperties.DownSet ;
            Elevation  = aProperties == null ? null : aProperties.Elevation ;
            Virtual  = aProperties == null ? null : aProperties.Virtual ;
            if (aVector != null)
            {
                if (aVector is uopVector)
                {
                    Copy( (uopVector)aVector);
                }
                else if (aVector is dxfVector)
                {
                    Copy((dxfVector)aVector);
                }
                else if (aVector is uopHole)
                {
                    Copy((uopHole)aVector);
                }
            }

        }
        #endregion Constructors

        #region Properties

        public double? X { get; set; }
        public double? Y { get; set; }
        public double? Value { get; set; }
        public int? Row { get; set; }
        public int? Col { get; set; }
        public int? PartIndex { get; set; }
        public int? Index { get; set; }
        public string Tag { get; set; }
        public string Flag { get; set; }
        public bool? Suppressed { get; set; }
        public double? Radius { get; set; }
        public double? Proximity { get; set; }
        public double? Inset { get; set; }
        public double? DownSet { get; set; }
        public double? Elevation { get; set; }
        public bool? Mark { get; set; }
        public bool? Virtual { get; set; }

        #endregion Properties

        #region Methods

        public override string ToString()
        {
            string _rVal = "VECTOR PROPS [";
            if (X.HasValue) _rVal += $" X:{ X.Value :0.00##}";
            if (Y.HasValue) _rVal += $" Y:{Y.Value:0.00##}";
            if (Elevation.HasValue && Elevation.Value != 0) _rVal += $" ELEV:{Elevation.Value:0.00##}";
            if (Radius.HasValue) _rVal += $" RAD:{Radius.Value:0.00##}";
            if (Row.HasValue && Row.Value!=0) _rVal += $" ROW:{Row.Value}";
            if (Col.HasValue && Col.Value != 0) _rVal += $" COL:{Col.Value}";
            if (PartIndex.HasValue  && PartIndex.Value != 0) _rVal += $" PART INDEX:{PartIndex.Value}";
            if (Index.HasValue && Index.Value != 0) _rVal += $" INDEX:{PartIndex.Value}";
            if (Tag != null) Tag = _rVal += $" TAG:{Tag}";
            if (Flag != null) Flag = _rVal += $" FLAG:{Flag}";
            if (Value.HasValue && Value.Value != 0) _rVal += $" VALUE:{Value.Value:0.00##}";
            if (Suppressed.HasValue) _rVal += $" SUPPRESSED:{Suppressed.Value}";
            if (Proximity.HasValue && Proximity.Value !=0)_rVal += $" PROXIMITY:{Proximity.Value:0.00##}";
            if (Inset.HasValue && Inset.Value !=0) _rVal += $" INSET:{Inset.Value:0.00##}";
            if (DownSet.HasValue && DownSet !=0) _rVal += $" DOWNSET:{DownSet.Value:0.00##}";
            if (Virtual.HasValue) _rVal += $" VIRTUAL:{Virtual.Value}";
            return  _rVal;
        }

         public void Copy(uopVector aVector,bool bCopyAll = true)
        {
            if (aVector == null) return;

            X = aVector.X;
            Y = aVector.Y;
            if (aVector.Value != 0 || bCopyAll) Value = aVector.Value; else Value = null;
            if (aVector.Row != 0 || bCopyAll) Row = aVector.Row; else Row = null;
            if (aVector.Col != 0 || bCopyAll) Col = aVector.Col; else Col = null;
            if (aVector.PartIndex != 0 || bCopyAll) PartIndex = aVector.PartIndex; else PartIndex = null;
            if (aVector.Index != 0 || bCopyAll) Index = aVector.Index; else Index = null;
            if (!string.IsNullOrWhiteSpace( aVector.Tag) || bCopyAll) Tag = aVector.Tag; else Tag = null;
            if (!string.IsNullOrWhiteSpace(aVector.Flag) || bCopyAll) Flag = aVector.Flag; else Flag = null;
            
            if(aVector.Suppressed || bCopyAll) Suppressed = aVector.Suppressed;
            if (aVector.Radius != 0 || bCopyAll) Radius = aVector.Radius; else Radius = null;
            if (aVector.Proximity != 0 || bCopyAll) Proximity = aVector.Proximity; else Proximity = null;
            if (aVector.Inset != 0 || bCopyAll) Inset = aVector.Inset; else Inset = null;
            if (aVector.DownSet != 0 || bCopyAll) DownSet = aVector.DownSet; else DownSet = null;
            if (aVector.Elevation.HasValue || bCopyAll) Elevation = aVector.Elevation; else Elevation = null;
            if (aVector.Virtual || bCopyAll) Virtual = aVector.Virtual;
          

        }
        public void Copy(dxfVector aVector)
        {
            if (aVector == null) return;
        
            X = aVector.X;
            Y = aVector.Y;
            Value = aVector.Value;
            Row = aVector.Row;
            Col = aVector.Col;
            Index = aVector.Index;
            Tag = aVector.Tag;
            Flag = aVector.Flag;
            Suppressed = aVector.Suppressed;
            Radius = aVector.Radius;
            Elevation = aVector.Z;
     

        }

        public void Copy(uopHole aHole)
        {
            if (aHole == null) return;

            X = aHole.X;
            Y = aHole.Y;
            Value = aHole.Value;
            Index = aHole.Index;
            Tag = aHole.Tag;
            Flag = aHole.Flag;
            Suppressed = aHole.Suppressed;
            Radius = aHole.Radius;
            Elevation = aHole.Elevation;
        }

        public void Copy(uopVectorProperties aProperties)
        {
            if (aProperties == null) return;
     
            X = aProperties.X;
            Y = aProperties.Y;
            Value = aProperties.Value;
            Row = aProperties.Row;
            Col = aProperties.Col;
            PartIndex = aProperties.PartIndex;
            Index = aProperties.Index;
            Tag = aProperties.Tag;
            Flag = aProperties.Flag;
            Suppressed = aProperties.Suppressed;
            Radius = aProperties.Radius;
            Proximity = aProperties.Proximity;
            Inset = aProperties.Inset;
            DownSet = aProperties.DownSet;
            Elevation = aProperties.Elevation;
            Virtual = aProperties.Virtual;

        }
        #endregion Methods

    }
}
