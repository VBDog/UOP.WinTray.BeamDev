using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Materials;

namespace UOP.WinTray.Projects.Parts
{
    public class hdwStud : uopHardware
    {
        public override uppPartTypes BasePartType => uppPartTypes.Stud;


        #region Constructors

        public hdwStud(uppHardwareSizes aSize = uppHardwareSizes.M10, uopHardwareMaterial aMaterial = null) : base(uppHardwareTypes.Stud, aSize, aMaterial) { }


        internal hdwStud(uopHardware aParent, int aQuantity = 0) : base(uppHardwareTypes.Stud, aParent.Size, aParent.HardwareMaterial, (aQuantity > 0) ? aQuantity : aParent.Quantity) 
        {
            SubPart(aParent);
            if (aParent.Type == Type) HardwareStructure = aParent.HardwareStructure;
        }

        #endregion


        #region Subparts  

        /// <summary>
        ///returns a nut with the same material and size as the bolt
        /// </summary>
        public hdwHexNut GetNut(int aQuantity = 0) => new hdwHexNut(this,aQuantity);

        /// <summary>
        ///returns a lock washer with the same material and size as the bolt
        /// </summary>
        public hdwLockWasher GetLockWasher(int aQuantity = 0) => new hdwLockWasher(this, aQuantity);

        /// <summary>
        ///returns a washer with the same material and size as the bolt
        /// </summary>
        public hdwFlatWasher GetWasher(int aQuantity = 0) => new hdwFlatWasher(this, aQuantity);

        #endregion


        public override uopProperties CurrentProperties() { UpdatePartProperties(); return base.CurrentProperties(); }



        public double HeadDiameter
        {
            get
            {
                double _rVal = 0;
                //^the diameter of the bolt head based on the current size setting
                switch (Size)
                {
                    case uppHardwareSizes.ThreeEights:
                        _rVal = 0.844;
                        break;
                    case uppHardwareSizes.M10:
                        _rVal = 22.3 / 25.4;
                        break;
                }

                return _rVal;
            }
        }

        public double HeadHeight
        {
            get
            {
                double HeadHeight = 0;
                //^the height of the bolt head based on the current size setting
                switch (Size)
                {
                    case uppHardwareSizes.ThreeEights:
                        HeadHeight = 0.208;
                        break;
                    case uppHardwareSizes.M10:
                        HeadHeight = 5.8 / 25.4;
                        break;
                }

                return HeadHeight;
            }
        }

        public override double Length { get => base.BoltLength; set => base.BoltLength = value; }

      
        public override uopHardwareMaterial HardwareMaterial { get => base.HardwareMaterial; set => base.HardwareMaterial = value; }

      
        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();

        public hdwStud Clone(int aQuantity = 0) => new hdwStud(this, aQuantity);

        //string that describes the hardware
        //like "M10 Lock Washer" or "3/8'' x 1.000 Hex Head Bolt"
        public override string GetFriendlyName(bool AllCaps = true) => AllCaps ? base.FriendlyName.ToUpper() : base.FriendlyName;
        
        public bool IsEqual(hdwStud aPart) => uopHardware.Compare(this, aPart);
       

        public override void UpdatePartProperties() => PartNumber = GetFriendlyName(false);


        public override void UpdatePartWeight() { base.Weight = Weight; }



    }
}
