using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UOP.DXFGraphics;

namespace UOP.WinTray.Projects
{
    public class uopGridLine : uopLine, ICloneable
    {

        #region Constructors

        public uopGridLine(uopGridLine aGridLine) => Init(aGridLine);
        public uopGridLine(iLine aLine)
        {
            Init();
            if (aLine != null) Copy(aLine);
        }

        public uopGridLine(bool bIsRow, double aSPX, double aSPY, double aEPX, double aEPY, int aRow = 0, int aCol = 0, uopVector aGridOrigin = null)
        {
            Init(null, bIsRow: bIsRow, aSPX: aSPX, aSPY: aSPY, aEPX: aEPX, aEPY: aEPY, aRow: aRow, aCol: aCol, aGridOrigin: aGridOrigin);
   
        }

        private void Init(uopGridLine aGridLine = null, bool bIsRow = false, double aSPX = 0, double aSPY = 0, double aEPX =0, double aEPY = 0, int aRow = 0, int aCol = 0, uopVector aGridOrigin = null)
        {
            base.Init();
            sp = new uopVector(aSPX, aSPY);
            ep = new uopVector(aEPX, aEPY);
            Row = aRow;
            Col = aCol;
            GridOrigin = null;
            IsRow = bIsRow;
            VPitch = 0;
            HPitch = 0;
            PitchType =  dxxPitchTypes.Rectangular;
            if (aGridLine != null) Copy(aGridLine);

            if(aGridOrigin !=null)
                GridOrigin =  new uopVector(aGridOrigin);

        }
        #endregion Constructors

        #region Properties

        public bool IsRow { get; set; }

        public bool IsCol => !IsRow;

        public uopVector GridOrigin {get;set;}

        public int YStep 
        { 
            get
            {
                if(!IsRow || VPitch ==0 ) return 0;
                double dY = Y() - (GridOrigin != null ? GridOrigin.Y : 0);
                return (int)Math.Round(dY / (VStepFactor * VPitch), 0) + 1; // rLn.Value; // 

            }
        
        }

        public int XStep
        {
            get
            {
                if (IsRow || HPitch == 0) return 0;
                double dX = X() - (GridOrigin != null ? GridOrigin.X : 0);
                return (int)Math.Round(dX / (HStepFactor * HPitch), 0) + 1; // rLn.Value; // 

            }

        }
        public  dxxPitchTypes PitchType { get; set;  }

        public double VPitch { get; set; }
        public double HPitch { get; set; }

        private double HStepFactor => PitchType == dxxPitchTypes.Rectangular ? 1.0 : PitchType == dxxPitchTypes.InvertedTriangular ? 1.0 : 0.5;
        private double VStepFactor => PitchType == dxxPitchTypes.Rectangular ? 1.0 : PitchType == dxxPitchTypes.InvertedTriangular ? 0.5 : 1.0;

        public bool TriangularPitch => PitchType == dxxPitchTypes.Triangular || PitchType == dxxPitchTypes.InvertedTriangular;
        #endregion Properties

        #region Methods
        object ICloneable.Clone() => Clone();

        public new  uopGridLine Clone() => new uopGridLine(this);

        public override void Copy(iLine aLine)
        {
            if (aLine == null) return;
            if (aLine is uopGridLine)
                Copy((uopGridLine)aLine);
            else
                base.Copy(aLine);
        }
        public void Copy(uopGridLine aLine)
        {
            if (aLine == null) return;
            base.Copy(aLine);
            GridOrigin = aLine.GridOrigin == null ? null : new uopVector(aLine.GridOrigin);
            IsRow = aLine.IsRow;
            PitchType = aLine.PitchType;
            VPitch = aLine.VPitch;
            HPitch = aLine.HPitch;
        }

        public override string ToString()
        {
            string _rVal = $"uopGridLine";
            if (IsRow)
            {
                _rVal += $" Row:{Row} Y:{Y():0.000} YSTEP:{YStep}";
            }
            else
            {
                _rVal += $" Col:{Col} X:{X():0.000} XSTEP:{XStep}";

            }
            return _rVal;
        }
        #endregion Methods
    }
}
