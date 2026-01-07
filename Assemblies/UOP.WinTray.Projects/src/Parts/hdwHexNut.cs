using UOP.DXFGraphics;
using System;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Materials;
namespace UOP.WinTray.Projects.Parts
{
    public class hdwHexNut : uopHardware
    {
        public override uppPartTypes BasePartType => uppPartTypes.HexNut;

        #region Constructors

        public hdwHexNut(uppHardwareSizes aSize = uppHardwareSizes.M10, uopHardwareMaterial aMaterial = null,int aQuantity = 1) : base(uppHardwareTypes.HexNut, aSize, aMaterial,aQuantity) { }


        internal hdwHexNut(uopHardware aParent, int aQuantity = 0) : base(uppHardwareTypes.HexNut, aParent.Size, aParent.HardwareMaterial, (aQuantity > 0) ? aQuantity : aParent.Quantity)
        {
            SubPart(aParent);
            if (aParent.Type == Type) HardwareStructure = aParent.HardwareStructure;
        }

        #endregion



        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();

        public hdwHexNut Clone(int aQuantity = 0) => new hdwHexNut(this, aQuantity);
        
         public override uopProperties CurrentProperties()  { UpdatePartProperties(); return base.CurrentProperties(); }

        //string that describes the hardware
        //like "M10 Lock Washer" or "3/8'' x 1.000 Hex Head Bolt"
        public override string GetFriendlyName(bool AllCaps = true) => AllCaps ? base.FriendlyName.ToUpper() : base.FriendlyName;


        public bool IsEqual(hdwHexNut aPart) => uopHardware.Compare(this, aPart);


        // <summary>
        //returns the polygon used to draw the nuts profile (elevation) view
        // </summary>
        // <returns></returns>
        public dxePolygon Profile(double ScaleFactor = 0.0f, object InsertionPt = null, dxxOrthoDirections Direction = dxxOrthoDirections.Up)
        {
            //dxePolygon Profile;
            //Profile = new dxePolygon();
            //double scl = 0;

            //dxfVector ip = null;

            //double thk = 0;

            //dxfVector v1 = null;

            //dxeHole aHole = null;

            //ip = (InsertionPt != null) ? dxfVector.FromObject(InsertionPt) : base.Center.ToDXFVector();
            //scl = Abs(ScaleFactor);
            //if (scl <= 0)
            //{
            //    scl = 1;
            //}
            //thk = Thickness;
            //v1 = new dxfVector();
            //v1.MoveTo(ip);
            //Profile = goPrimatives.Rectangle(v1, true, thk, OD);
            //aHole = new dxeHole();
            //aHole.Diameter = ID;
            //aHole.Plane = dxfUtils.CreatePlane(dxxStandardPlanes.XZ);
            //aHole.Depth = thk;
            //aHole.Center = v1;
            //colDXFEntities Hls = null;

            //Hls = new colDXFEntities();
            //Hls.Add(aHole);
            //utils_AddHolesToPGON(Profile, Hls, uopViewTop);
            //dynamic _WithVar_3734;
            //_WithVar_3734 = Profile;
            //_WithVar_3734.InsertionPt = ip;
            //if (Direction == Down)
            //{
            //    _WithVar_3734.RotateAbout(ip, 180);
            //}
            //else if (Direction == Left)
            //{
            //    _WithVar_3734.RotateAbout(ip, 90);
            //}
            //else if (Direction == Right)
            //{
            //    _WithVar_3734.RotateAbout(ip, -90);
            //}

            //if (scl != 1)
            //{
            //    _WithVar_3734.Rescale(scl);
            //}
            //return Profile;
            return null;
        }

        public override void UpdatePartProperties() { PartNumber = GetFriendlyName(false); }

        public override void UpdatePartWeight()
        {
            base.Weight = Weight;
        }

        /// <summary>
        ///the weight of the part in english pounds
        /// </summary>
        public override double Weight => Math.PI * Math.Pow(OD / 2, 2) - Math.PI * Math.Pow(ID / 2, 2) * Thickness * Material.Density;
    }
    
}
