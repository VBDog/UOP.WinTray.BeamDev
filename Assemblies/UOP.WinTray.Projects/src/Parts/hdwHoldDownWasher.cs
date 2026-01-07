using UOP.DXFGraphics;
using System;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Materials;
using UOP.DXFGraphics.Utilities;

namespace UOP.WinTray.Projects.Parts
{
    /// <summary>
    /// hold down washer object
    /// A large diameter fender washer used to clamp deck panels to deck beams.
    /// </summary>
    public class hdwHoldDownWasher : uopHardware
    {

        public override uppPartTypes BasePartType => uppPartTypes.HoldDownWasher;


        #region Constructors
        public hdwHoldDownWasher():base(uppHardwareTypes.LargeODWasher,uppHardwareSizes.M10)
        {

            OD = 1.75;
            ID = mdGlobals.gsBigHole;
            SparePercentage = 5;
            Quantity = 0;
        }

        public hdwHoldDownWasher(uopSheetMetal aMaterial, uopPart aParent = null) : base(uppHardwareTypes.LargeODWasher, uppHardwareSizes.M10)
        {

            OD = mdGlobals.HoldDownWasherDiameter;
            ID = mdGlobals.gsBigHole;
            SparePercentage = 5;
            Quantity = 0;
            if (aMaterial != null) base.SheetMetal = aMaterial;
            SubPart(aParent);
        }

        internal hdwHoldDownWasher(uopHardware aParent, int aQuantity = 0) : base(uppHardwareTypes.LargeODWasher, aParent.Size, aParent.HardwareMaterial, (aQuantity > 0) ? aQuantity : aParent.Quantity)
        {
            SubPart(aParent);
            if (aParent.Type == Type) HardwareStructure = aParent.HardwareStructure;
        }

        #endregion

        /// <summary>
        /// returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        public hdwHoldDownWasher Clone() => new hdwHoldDownWasher(this);


        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();

      

        //string that describes the hardware
        //like "M10 Lock Washer" or "3/8'' x 1.000 Hex Head Bolt"
        public override string GetFriendlyName(bool AllCaps = true) => AllCaps ? base.FriendlyName.ToUpper() : base.FriendlyName;


        public override void UpdatePartWeight() => base.Weight = Weight;



        /// <summary>
        /// hole in the washer
        /// diameter matches ID
        /// </summary>
        public dxeHole Hole => new dxeHole() { Diameter = ID, Depth = Thickness, Center = new dxfVector(X, Y, Z) };
      

        /// <summary>
        /// returns the polygon used to draw the washers profile (elevation) view
        /// </summary>
        public dxePolygon Profile(double ScaleFactor = 0, dxfVector InsertionPt = null, dxxOrthoDirections Direction = dxxOrthoDirections.Up)
        {

            dxePolygon _rVal = new dxePolygon();
            ScaleFactor = Math.Abs(ScaleFactor);
            double scl = (ScaleFactor <=0) ? 1 : ScaleFactor;
            dxfVector ip = (InsertionPt!= null) ? new dxfVector(InsertionPt) : new dxfVector(X,Y,Z);
            double thk = Thickness;
            dxfVector v1 = new dxfVector(ip);
            dxfVector v2 = null;
           dxfDisplaySettings dsp = new dxfDisplaySettings();
           
            
            dxfPrimatives primatives = new dxfPrimatives();
            object v = (object)v1;
            _rVal = (dxePolygon)primatives.Rectangle(v1, true, thk, OD);
            v1.Move(-0.203, 0.5 * thk);
            v2 = new dxfVector(v1);
          
            _rVal.AdditionalSegments.AddLine(v1, v2, dsp);
            v1.Move(ID);
            v2.Move(ID);
            _rVal.AdditionalSegments.AddLine(v1, v2, dsp);
            _rVal.InsertionPt = ip;
            if (Direction == dxxOrthoDirections.Down)
            {
                _rVal.RotateAbout(ip, 180);
            }
            else if (Direction == dxxOrthoDirections.Left)
            {
                _rVal.RotateAbout(ip, 90);
            }
            else if (Direction == dxxOrthoDirections.Right)
            {
                _rVal.RotateAbout(ip, -90);
            }

            if (scl != 1)  _rVal.Rescale(scl);
            
            return _rVal;
        }


        public override void UpdatePartProperties() { PartNumber = GetFriendlyName(false); }


        /// <summary>
        /// the weight of the part in english pounds
        /// the weight of the part in english pounds
        /// </summary>
        public override double Weight => Math.PI * Math.Pow(OD / 2, 2) - Math.PI * Math.Pow(ID / 2, 2) * Thickness * Material.Density;
      
    }
}