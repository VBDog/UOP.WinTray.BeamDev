using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Materials;

namespace UOP.WinTray.Projects.Parts
{
    public class hdwCarriageBolt : uopHardware
    {
        public override uppPartTypes BasePartType => uppPartTypes.CarriageBolt;

        #region Constructors
        public hdwCarriageBolt(uppHardwareSizes aSize = uppHardwareSizes.M10, uopHardwareMaterial aMaterial = null) : base(uppHardwareTypes.CarriageBolt, aSize,aMaterial) { }


        internal hdwCarriageBolt(uopHardware aParent, int aQuantity = 0) : base(uppHardwareTypes.CarriageBolt, aParent.Size, aParent.HardwareMaterial, (aQuantity > 0) ? aQuantity : aParent.Quantity)
        {
            SubPart( aParent);
            if (aParent.Type == Type) HardwareStructure = aParent.HardwareStructure;
        }

        #endregion

        #region Subparts  

        /// <summary>
        ///returns a nut with the same material and size as the bolt
        /// </summary>
        public hdwHexNut GetNut(int aQuantity = 0) => new hdwHexNut(this, aQuantity);

        /// <summary>
        ///returns a lock washer with the same material and size as the bolt
        /// </summary>
        public hdwLockWasher GetLockWasher(int aQuantity = 0) => new hdwLockWasher(this, aQuantity);

        /// <summary>
        ///returns a washer with the same material and size as the bolt
        /// </summary>
        public hdwFlatWasher GetWasher(int aQuantity = 0) => new hdwFlatWasher(this, aQuantity);

        #endregion


         public override uopProperties CurrentProperties()  { UpdatePartProperties(); return base.CurrentProperties(); }


        public double HeadDiameter => uopHardware.BoltHeadDiameter(Size);

        public double HeadHeight => uopHardware.BoltHeadHeight(Size);

        public override double Length { get => base.BoltLength; set => base.BoltLength = value; }

     
        public override uopHardwareMaterial HardwareMaterial { get => base.HardwareMaterial; set => base.HardwareMaterial = value; }

        
        //^the width of the carraige bolt square based on the current size setting
        public double SquareWidth
        {
            get
            {

                switch (Size)
                {
                    case uppHardwareSizes.ThreeEights:
                        return 0.388;
                    case uppHardwareSizes.M10:
                        return 10.58 / 25.4;
                }
                return 0;
            }
        }

     
        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();

        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        public hdwCarriageBolt Clone(int aQuantity = 0) => new hdwCarriageBolt(this, aQuantity);
        //string that describes the hardware
        //like "M10 Lock Washer" or "3/8'' x 1.000 Hex Head Bolt"
        public override string GetFriendlyName(bool AllCaps = true) => AllCaps ? base.FriendlyName.ToUpper() : base.FriendlyName;


        public bool IsEqual(hdwCarriageBolt aPart) => uopHardware.Compare(this, aPart);


        public override void UpdatePartProperties() => PartNumber = GetFriendlyName(false);
        

        public override void UpdatePartWeight() { base.Weight = Weight; }



    }
}
