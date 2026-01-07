using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Materials;

namespace UOP.WinTray.Projects.Parts
{
    /// <summary>
    /// '^manway stud object
    ///'~the shaved stud used to hold down the manway panel
    /// </summary>
    public class hdwSetScrew : uopHardware
    {
        public override uppPartTypes BasePartType => uppPartTypes.SetScrew;

        #region Constructors

        public hdwSetScrew(uppHardwareSizes aSize = uppHardwareSizes.M10, uopHardwareMaterial aMaterial = null) : base(uppHardwareTypes.SetScrew, aSize, aMaterial) { }

        public hdwSetScrew(uopPart aParent) : base(uppHardwareTypes.SetScrew, (aParent.Bolting == uppUnitFamilies.English)?uppHardwareSizes.ThreeEights: uppHardwareSizes.M10, aParent.HardwareMaterial) { SubPart( aParent); }


        internal hdwSetScrew( uopHardware aParent,int aQuantity = 0) : base(uppHardwareTypes.SetScrew, aParent.Size, aParent.HardwareMaterial,(aQuantity >0)? aQuantity: aParent.Quantity)
        {
            SubPart(aParent);
            if (aParent.Type == Type) HardwareStructure = aParent.HardwareStructure;
        }

        #endregion


         public override uopProperties CurrentProperties()  { UpdatePartProperties(); return base.CurrentProperties(); }


        public override double Length { get => base.BoltLength; set => base.BoltLength = value; }

        /// <summary>
        ///returns a lock washer with the same material and size as the bolt
        /// </summary>
        public hdwLockWasher GetLockWasher(int aQuantity = 0) => new hdwLockWasher(Size, HardwareMaterial, (aQuantity > 0) ? aQuantity : Quantity) { Category = Category };


        public override uopHardwareMaterial HardwareMaterial { get => base.HardwareMaterial; set => base.HardwareMaterial = value; }

        /// <summary>
        ///returns a nut with the same material and size as the bolt
        /// </summary>
        public hdwHexNut GetNut(int aQuantity = 0) => new hdwHexNut(Size, HardwareMaterial, (aQuantity > 0) ? aQuantity : Quantity) { Category = Category } ;


        /// <summary>
        ///returns a washer with the same material and size as the bolt
        /// </summary>
        public hdwFlatWasher GetWasher(int aQuantity = 0) => new hdwFlatWasher(Size, HardwareMaterial, (aQuantity > 0) ? aQuantity : Quantity) { Category = Category };


        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();

        public hdwSetScrew Clone(int aQuantity = 0) => new hdwSetScrew(this, aQuantity);
        //string that describes the hardware
        //like "M10 Lock Washer" or "3/8'' x 1.000 Hex Head Bolt"
        public override string GetFriendlyName(bool AllCaps = true) => AllCaps ? base.FriendlyName.ToUpper() : base.FriendlyName;


        public bool IsEqual(hdwSetScrew aPart) => uopHardware.Compare(this, aPart);



        public override void UpdatePartProperties() => PartNumber = GetFriendlyName(false);


        public override void UpdatePartWeight() { base.Weight = Weight; }



    }
}