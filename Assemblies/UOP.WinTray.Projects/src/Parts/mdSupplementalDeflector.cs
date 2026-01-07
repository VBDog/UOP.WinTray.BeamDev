using UOP.DXFGraphics;
using System;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;
using System.Linq;


namespace UOP.WinTray.Projects.Parts
{
    /// <summary>
    /// the deflector plates of a ECMD tray downcomer assembly
    /// </summary>
    public class mdSupplementalDeflector : mdBoxSubPart
    {
        public override uppPartTypes BasePartType => uppPartTypes.SupplementalDeflector;

        #region Constructors

        public mdSupplementalDeflector() : base(uppPartTypes.SupplementalDeflector) => Initialize();
   
        public mdSupplementalDeflector(mdDowncomerBox aBox) : base(uppPartTypes.SupplementalDeflector) => Initialize(null, aBox);

        internal mdSupplementalDeflector(mdSupplementalDeflector aPartToCopy) : base(uppPartTypes.SupplementalDeflector) => Initialize(aPartToCopy);


        internal override void Initialize(mdBoxSubPart aPartToCopy, mdDowncomerBox aBox)
        {
            mdSupplementalDeflector copy = null;
            if (aPartToCopy != null && aPartToCopy.GetType() == typeof(mdSupplementalDeflector)) copy = (mdSupplementalDeflector)aPartToCopy;
            Initialize(copy, aBox);
        }

        private bool _Init;
        private void Initialize(mdSupplementalDeflector aPartToCopy = null, mdDowncomerBox aBox = null)
        {
            if (!_Init)
            {
                Height = 1;
                Length = 1;
                ParentPartType = uppPartTypes.Downcomer;
                _Init = true;
            }

            if (aPartToCopy != null)
            {
                Copy(aPartToCopy);
                aBox ??= aPartToCopy.DowncomerBox;
            }

             if (aBox != null)
            {

         
                SubPart(aBox);
                OccuranceFactor = aBox.OccuranceFactor;
                ULINE aLn = aBox.LimitLn(bTop: true); // aDowncomer.LimLines().First().Line1.Value;
                ULINE bLn = aBox.LimitLn(bTop: false); // aDowncomer.LimLines().Last().Line2.Value;
                double thk = Thickness;
             
                double dkthk = aBox.DeckThickness;

                OccuranceFactor = aBox.OccuranceFactor;
               
                BoxIndex = aBox.Index;
               
                SetCoordinates(aBox.X, aBox.Y, aBox.WeirHeight);
                Length = aLn.MidPt.Y - bLn.MidPt.Y;

                Height = aBox.SupplementalDeflectorHeight;
                if (aBox.DesignFamily.IsEcmdDesignFamily()) X = X - (dkthk / 2) + (1 / 16) + (1 / 32) + thk / 2;

                if (aLn.Slope != 0 || bLn.Slope != 0)
                {
                    ULINE iln;

                    if (X >= 0)
                        iln = new ULINE(new UVECTOR(X + 0.5 * thk, 0), new UVECTOR(X + 0.5 * thk, 100));
                    else
                        iln = new ULINE(new UVECTOR(X - 0.5 * thk, 0), new UVECTOR(X - 0.5 * thk, 100));

                    UVECTOR v1 = aLn.IntersectionPt(iln);
                    UVECTOR v2 = bLn.IntersectionPt(iln);

                    Length = v1.Y - v2.Y;
                }

            }
         
        }

        #endregion Constructors


        #region Properties

        

        /// <summary>
        /// the length of the plate
        /// </summary>
        public override double Length { get => base.Length; set => base.Length = Math.Abs(value); }



        /// <summary>
        /// returns the weight of the part in english pounds
        /// </summary>
        public override double Weight => Length * Height * SheetMetalWeightMultiplier;

        #endregion Properties

        #region Methods

        /// <summary>
        /// IsEqual Method
        /// </summary>
        /// <param name="aDefl"></param>
        /// <returns></returns>
        public bool IsEqual(mdSupplementalDeflector aDefl)
        {
            if (aDefl == null) return false;

            if (!TVALUES.CompareNumeric(aDefl.Length, Length, 3)) return false;
            if (!TVALUES.CompareNumeric(aDefl.Height, Height, 3)) return false;

            if (!aDefl.Material.IsEqual(Material)) return false;

            return true;
        }


        /// <summary>
        /// Current Properties
        /// </summary>
        public override uopProperties CurrentProperties() { UpdatePartProperties(); return base.CurrentProperties(); }



        /// <summary>
        /// returns a new object that is an exact copy of this one
        /// </summary>
        /// <returns></returns>
        public mdSupplementalDeflector Clone() => new mdSupplementalDeflector(this);

        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();

        /// <summary>
        /// Update Part Properties method
        /// </summary>
        public override void UpdatePartProperties()
        {
            Quantity = OccuranceFactor;
            DescriptiveName = "Supplemental Deflector Plate (DC " + DowncomerIndex + ")";
        }

        /// <summary>
        /// Update Part Weight Method
        /// </summary>
        public override void UpdatePartWeight() { base.Weight = Weight; }


        /// <summary>
        /// returns a dxePolygon that is used to draw the elevation view of the plate
        /// </summary>
        /// <param name="aAssy_UNUSED"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public dxePolygon View_Elevation(ref mdTrayAssembly aAssy_UNUSED, UOP.DXFGraphics.dxfVector aCenter, ref double aRotation, ref string aLayerName)
        => mdPolygons.SupDef_View_Elevation(this, aCenter, aRotation, aLayerName);

        /// <summary>
        /// returns a dxePolygon that is used to draw the elevation view of the plate
        /// </summary>
        /// <param name="aAssy"></param>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public dxePolygon View_Plan( mdTrayAssembly aAssy, iVector aCenter,  double aRotation = 0,  string aLayerName = "GEOMETRY") => mdPolygons.SupDef_View_Plan(this, aAssy, aCenter, aRotation, aLayerName);

        /// <summary>
        /// returns a dxePolygon that is used to draw the elevation view of the plate
        /// </summary>
        /// <param name="aCenter"></param>
        /// <param name="aRotation"></param>
        /// <param name="aLayerName"></param>
        /// <returns></returns>
        public dxePolygon View_Profile(iVector aCenter,  double aRotation = 0,  string aLayerName ="GEOMETRY") => mdPolygons.SupDef_View_Profile(this, aCenter, aRotation, aLayerName);


        /// <summary>
        /// Current Property
        /// </summary>
        /// <param name="aPropertyName"></param>
        /// <param name="bSupressNotFoundError"></param>
        /// <returns></returns>
        public override uopProperty CurrentProperty(string aPropertyName, bool bSupressNotFoundError = false)
        {
            UpdatePartProperties();
            return base.CurrentProperty(aPropertyName, bSupressNotFoundError);
        }

        #endregion Methods

    }
}
