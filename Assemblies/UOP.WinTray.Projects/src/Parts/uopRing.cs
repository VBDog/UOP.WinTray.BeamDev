using System;
using UOP.WinTray.Projects.Enums;
using uppPartTypes = UOP.WinTray.Projects.Enums.uppPartTypes;

namespace UOP.WinTray.Projects.Parts
{
    /// <summary>
    /// the object which represents the support ring of a tray range
    /// </summary>
    public class uopRing : uopPart
    {
        public override uppPartTypes BasePartType => uppPartTypes.Ring;
        private double _StartAngle = 0;
        private double _EndAngle = 0;
        private double _TrimLength = 0;
        


        public uopRing() : base(uppPartTypes.Ring, uppProjectFamilies.uopFamMD, "", "", true)
        {
            Suppressed = true;
            _StartAngle = 0;
            _EndAngle = 360;
     
        }


        public uopRing(double aStartAngle =0, double aEndAngle = 360, uopTrayRange aRange = null) :base(uppPartTypes.Ring, uppProjectFamilies.uopFamMD, "", "", true)
        {
            Suppressed = true;
            _StartAngle = aStartAngle;
            _EndAngle = aEndAngle;
            if (aRange != null) base.SubPart(aRange);
        }

        internal uopRing(uopRing aPartToCopy) : base(uppPartTypes.Ring, uppProjectFamilies.uopFamMD, "", "", true)
        {
            Copy(aPartToCopy);
            _StartAngle = aPartToCopy.StartAngle;
            _EndAngle = aPartToCopy.EndAngle;
            _TrimLength = aPartToCopy.TrimLength;
        }

        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        public override uopPart Clone(bool aFlag = false) => (uopPart)Clone();

        public uopRing Clone() => new uopRing(this);
     

        /// <summary>
        /// the end angle of the ring
        /// </summary>
        public double EndAngle { get => _EndAngle; set => _EndAngle = value; }
      
        /// <summary>
        /// the ring inner diameter
        /// </summary>
        public double ID
        {
            get
            {
                uopTrayRange aRange = base.GetMDRange();
                return (aRange != null) ? aRange.RingID : 0;
               
            }
        }
  
        /// <summary>
        /// the ring outer diameter
        /// </summary>
        public double OD { get => ShellID;  }

     
        

        /// <summary>
        //the start angle of the ring
        /// </summary>
        public double StartAngle { get => _StartAngle; set => _StartAngle = value; }

       
     
        /// <summary>
        /// the ring thickness
        /// </summary>
        public override double Thickness { get { uopTrayRange aRange = GetMDRange(); return (aRange != null) ? aRange.RingThk : 0; } }


        /// <summary>
        /// the length of the trimmed edge of the ring near the downcomer beam
        /// </summary>
        public double TrimLength { get => _TrimLength; set => _TrimLength = Math.Abs(value); }
        
        public override void UpdatePartWeight() => base.Weight = 0;
     


    }
}
