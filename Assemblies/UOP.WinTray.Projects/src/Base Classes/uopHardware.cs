using System;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Materials;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;

namespace UOP.WinTray.Projects
{
    public abstract class uopHardware : uopPart
    {

        private THARDWARE tStruc;



        public uopHardware(uppHardwareTypes aType, uppHardwareSizes aSize, uopHardwareMaterial aMaterial = null,int aQuantity = 1) : base(aType.PartType(), uppProjectFamilies.Undefined)
        {
            tStruc = new THARDWARE(aType, aSize);
            
          
            SparePercentage = 5;
            
            Quantity = aQuantity;
           
            if(aType == uppHardwareTypes.LargeODWasher)
            {
                base.IsSheetMetal = true;
                OD = 1.75; ID = 0.4375;
                tStruc.Material = new TMATERIAL(uppMaterialTypes.SheetMetal);
            }
            else
            {
                tStruc.Material = (aMaterial != null) ? aMaterial.Structure : new TMATERIAL(uppMaterialTypes.Hardware);
            }

         
        }

        internal THARDWARE HardwareStructure { get => tStruc; set => tStruc = value; }


        public uppHardwareTypes Type { get => tStruc.Type; } 

        public int Add(int RHS) { Quantity += RHS; return Quantity; }

        public override double Length { get => tStruc.Length; set { tStruc.Length = value; base.Length = value; } }

        public void SetThickness(double aThickness) => tStruc.Thickness = aThickness;

        public double ObscuredLength { get => tStruc.ObscuredLength; set => tStruc.ObscuredLength = value; }

        public double MinorDiameter { get => tStruc.MinorDia; set => tStruc.MinorDia = value; }

        public bool DoubleNut { get => tStruc.DoubleNut; set => tStruc.DoubleNut = value; }

        public string LengthLable
        => IsMetric ? (Length / 25.4).ToString("F0") : Length.ToString("F3");
           
        public string FriendlyName 
        {
            get

            {
                string Spcr = IsMetric ? " " : " UNC ";
                
                switch (tStruc.Type)
                {
                    case uppHardwareTypes.CarriageBolt:
                        return SizeName + " x " + LengthLable + Spcr + uopEnums.Description(tStruc.Type);
                    case uppHardwareTypes.FlatWasher:
                        return SizeName + Spcr + uopEnums.Description(tStruc.Type);
                    case uppHardwareTypes.HexBolt:
                        return SizeName + Spcr + uopEnums.Description(tStruc.Type);
                    case uppHardwareTypes.HexNut:
                        return SizeName + Spcr + uopEnums.Description(tStruc.Type);
                    case uppHardwareTypes.LockWasher:
                        return SizeName + Spcr + uopEnums.Description(tStruc.Type);
                    case uppHardwareTypes.SetScrew:
                        return SizeName + " x " + LengthLable + Spcr + uopEnums.Description(tStruc.Type);
                    case uppHardwareTypes.ShavedStud:
                        return SizeName + " x " + LengthLable + Spcr + uopEnums.Description(tStruc.Type);
                    case uppHardwareTypes.Stud:
                        return SizeName + " x " + LengthLable + Spcr + uopEnums.Description(tStruc.Type);
                    case uppHardwareTypes.LargeODWasher:
                        return SizeName + Spcr + uopEnums.Description(tStruc.Type);
                    default:
                        return uopEnums.Description(tStruc.Type);
                }
            }
        
        }

       

        public override bool IsMetric {  get => tStruc.IsMetric;  set => tStruc.IsMetric = value;  }

        public uppWasherSeries Series { get => tStruc.WasherSeries; set => tStruc.WasherSeries = value; }


        public uppWasherTypes WasherType { get => tStruc.WasherType; set => tStruc.WasherType = value; }


        /// <summary>
        /// the inner diameter of the washer
        /// </summary>
        public virtual double ID { get => tStruc.ID; set => tStruc.ID = value; }


        /// <summary>
        /// the outer diameter of the washer
        /// </summary>

        public override double Diameter { get => OD; set => OD = value; }
        public virtual double OD { get => tStruc.OD; set { tStruc.OD = value; base.Diameter = value; } }
        
        //distance across flats
        public virtual double f { get => tStruc.f; set => tStruc.f = value; }

            //distance across corners
        public virtual double G { get => tStruc.G; set => tStruc.G = value; }

        //<Summary>then height of the bolt head</Summary>
        public virtual double H { get => tStruc.H; set => tStruc.H = value; }


        public abstract string GetFriendlyName(bool AllCaps = true);

        public override uopMaterial Material { get => HardwareMaterial; }

        public override uopHardwareMaterial HardwareMaterial { get => new uopHardwareMaterial(tStruc.Material); set { if (value == null) { return; } tStruc.Material = value.Structure; base.HardwareMaterial = value; } }
        
        public uppHardwareSizes Size { get => tStruc.Size; set => tStruc.Size = value; }

        public uppSpecTypes SpecType 
        {
            get
            {
                switch (tStruc.Type)
                {
                    case uppHardwareTypes.CarriageBolt:
                        return uppSpecTypes.Bolt;
                    case uppHardwareTypes.FlatWasher:
                        return uppSpecTypes.FlatWasher;
                    case uppHardwareTypes.HexBolt:
                        return uppSpecTypes.Bolt;
                    case uppHardwareTypes.HexNut:
                        return uppSpecTypes.Nut;
                    case uppHardwareTypes.LockWasher:
                        return uppSpecTypes.LockWasher;
                    case uppHardwareTypes.SetScrew:
                        return uppSpecTypes.SetScrew;
                    case uppHardwareTypes.ShavedStud:
                        return uppSpecTypes.Bolt;
                    case uppHardwareTypes.Stud:
                        return uppSpecTypes.ThreadedStud;
                    case uppHardwareTypes.LargeODWasher:
                        return uppSpecTypes.SheetMetal;
                    default:
                        return uppSpecTypes.Undefined;
                }
            }  
        }

        public string SizeName
        {
            get
            {
                switch (Size)
                {
                    case uppHardwareSizes.ThreeEights:
                        return "3/8";
                    case uppHardwareSizes.M10:
                        return "M10";
                    case uppHardwareSizes.M12:
                        return "M12";
                    case uppHardwareSizes.OneHalf:
                        return "1/2";
                    default:
                        return "";
                }
                
            }
        }

       public void UpdateDimensions()
        {
            THARDWARE struc = tStruc;
            if (IsWasher)
            {
                if (struc.Type == uppHardwareTypes.LargeODWasher)
                {
                    if (struc.IsMetric)
                    {
                        struc.OD = 44 / 25.4;
                        struc.ID = 11 / 25.4;

                    }
                    else
                    {
                        struc.OD = 1.75;
                        struc.ID = mdGlobals.gsSmallHole;
                    }

                }

                if (struc.Size == uppHardwareSizes.ThreeEights)
                {
                    if (struc.WasherType == uppWasherTypes.TypeA)
                    {
                        if (struc.WasherSeries == uppWasherSeries.Regular)
                        {
                            struc.WasherSeries = uppWasherSeries.Narrow;
                        }
                        if (struc.WasherSeries == uppWasherSeries.Narrow)
                        {
                            struc.ID = mdGlobals.gsSmallHole;
                            struc.OD = 0.812;
                            struc.Material.Thickness = 0.065;
                        }
                        else
                        {
                            struc.ID = mdGlobals.gsBigHole;
                            struc.OD = 1;
                            struc.Material.Thickness = 0.083;
                        }
                    }
                    if (struc.WasherType == uppWasherTypes.TypeB)
                    {
                        if (struc.WasherSeries == uppWasherSeries.Narrow)
                        {
                            struc.ID = mdGlobals.gsSmallHole;
                            struc.OD = 0.734;
                            struc.Material.Thickness = 0.063;
                        }

                        if (struc.WasherSeries == uppWasherSeries.Regular)
                        {
                            struc.ID = mdGlobals.gsSmallHole;
                            struc.OD = 1;
                            struc.Material.Thickness = 0.063;
                        }

                        if (struc.WasherSeries == uppWasherSeries.Wide)
                        {
                            struc.ID = mdGlobals.gsSmallHole;
                            struc.OD = 1.25;
                            struc.Material.Thickness = 0.1;
                        }

                    }

                }

                if (struc.Size == uppHardwareSizes.OneHalf)
                {
                    if (struc.WasherType == uppWasherTypes.TypeA)
                    {
                        if (struc.WasherSeries == uppWasherSeries.Regular)
                        {
                            struc.WasherSeries = uppWasherSeries.Narrow;
                        }

                        if (struc.WasherSeries == uppWasherSeries.Narrow)
                        {
                            struc.ID = 0.531;
                            struc.OD = 1.062;
                            struc.Material.Thickness = 0.095;
                        }
                        else
                        {
                            struc.ID = 0.561;
                            struc.OD = 1.375;
                            struc.Material.Thickness = 0.109;
                        }
                    }
                    if (struc.WasherType == uppWasherTypes.TypeB)
                    {
                        if (struc.WasherSeries == uppWasherSeries.Narrow)
                        {
                            struc.ID = 0.531;
                            struc.OD = 1.062;
                            struc.Material.Thickness = 0.095;
                        }

                        if (struc.WasherSeries == uppWasherSeries.Regular)
                        {
                            struc.ID = 0.531;
                            struc.OD = 1.062;
                            struc.Material.Thickness = 0.095;
                        }

                        if (struc.WasherSeries == uppWasherSeries.Wide)
                        {
                            struc.ID = 0.561;
                            struc.OD = 1.375;
                            struc.Material.Thickness = 0.109;
                        }

                    }

                }

                if (struc.Size == uppHardwareSizes.M10)
                {
                    if (struc.WasherType == uppWasherTypes.TypeB)
                    {
                        struc.WasherType = uppWasherTypes.TypeA;
                    }
                    if (struc.WasherSeries == uppWasherSeries.Narrow)
                    {
                        struc.ID = (((11.12 - 10.85) / 2.0) + 10.85) / 25.4;
                        struc.OD = (((20.0 - 19.48) / 2.0) + 19.48) / 25.4;
                        struc.Material.Thickness = (((2.3 - 1.6) / 2.0) + 1.6) / 25.4;
                    }

                    if (struc.WasherSeries == uppWasherSeries.Regular)
                    {
                        struc.ID = (((11.12 - 10.85) / 2.0) + 10.85) / 25.4;
                        struc.OD = (((28.0 - 27.48) / 2.0) + 27.48) / 25.4;
                        struc.Material.Thickness = (((2.8 - 2.0) / 2.0) + 2.0) / 25.4;
                    }

                    if (struc.WasherSeries == uppWasherSeries.Wide)
                    {
                        struc.ID = (((11.12 - 10.85) / 2.0) + 10.85) / 25.4;
                        struc.OD = (((39.0 - 38.38) / 2.0) + 38.38) / 25.4;
                        struc.Material.Thickness = (((3.5 - 2.5) / 2.0) + 2.5) / 25.4;
                    }

                }

                if (struc.Size == uppHardwareSizes.M12)
                {
                    if (struc.WasherType == uppWasherTypes.TypeB)
                    {
                        struc.WasherType = uppWasherTypes.TypeA;
                    }

                    struc.ID = 13.0 / 25.4;
                    struc.OD = 24.0 / 25.4;
                    struc.Material.Thickness = 3.0 / 25.4;
                    if (struc.WasherSeries == uppWasherSeries.Wide)
                    {
                        struc.OD = 40.0 / 25.4;

                    }


                }
            }
            else
            {
                if (struc.Size == uppHardwareSizes.ThreeEights)
                {
                    struc.H = 21.0 / 64.0;
                    struc.dia = 0.375;
                    struc.f = 9.0 / 16.0;
                    struc.G = 0.639;
                }
                else if (struc.Size == uppHardwareSizes.M10)
                {
                    struc.H = 8.9 / 25.4;
                    struc.dia = 10.0 / 25.4;
                    struc.f = 17.0 / 25.4;
                    struc.G = 19.63 / 25.4;
                }
                else if (struc.Size == uppHardwareSizes.OneHalf)
                {
                    struc.H = 7.0 / 16.0;
                    struc.dia = 0.5;
                    struc.f = 0.75;
                    struc.G = 0.846;
                }
                else if (struc.Size == uppHardwareSizes.M12)
                {
                    struc.H = 9.5 / 25.4;
                    struc.dia = 12.0 / 25.4;
                    struc.f = 19.0 / 25.4;
                    struc.G = 21.94 / 25.4;
                }

            }
            struc.OD = struc.dia;
            tStruc = struc;
            
        }
        protected virtual double BoltLength { get => tStruc.Length; set => tStruc.Length = value; }

        public virtual double ThreadedLength { get => tStruc.ThreadedLength; set => tStruc.ThreadedLength = value; }

        public override double X { get => tStruc.X; set => tStruc.X = value; }

        public override double Y { get => tStruc.Y; set => tStruc.Y = value; }

        public override double Z { get => tStruc.Z ; set => tStruc.Z = value; }

        internal new UVECTOR Center { get => tStruc.Center; set => tStruc.Center = value; }

        public bool HasLength => HardwareStructure.HasLength;


        public bool IsWasher => HardwareStructure.IsWasher;


        public bool IsEqual(uopHardware aHardware) => uopHardware.Compare(this, aHardware);
        

        #region Shared Methods

       

        public static bool Compare(uopHardware A, uopHardware B)
        {
            if (A == null && B == null) return true;
            if (A == null || B == null) return false;
            if (A.Type != B.Type) { return false; }

            if (A.Size != B.Size ) return false;
            if (A.IsSheetMetal != B.IsSheetMetal) return false;
            if(A.HasLength != B.HasLength) return false;
            if (A.HasLength)
            {
                if (Math.Round(A.Length, 1) != Math.Round(B.Length, 1)) return false;
            }
            if (A.IsSheetMetal)
            {
                if (!A.SheetMetal.IsEqual(B.SheetMetal)) { return false; }
            } else
            {
                if (!A.HardwareMaterial.IsEqual(B.HardwareMaterial)) { return false; }
                if(A.IsWasher)
                {
                    if (A.WasherType != B.WasherType) return false;
                    if (Math.Round(A.Thickness, 1) != Math.Round(B.Thickness, 1)) return false;
                }

            }
           
            return true;
        }

        //the diameter of the bolt head based on the passed size setting
        public static double BoltHeadDiameter(uppHardwareSizes aSize)
        {
            double _rVal = 0;
            switch (aSize)
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
        //^the height of the bolt head based on the passed size setting
        public static double BoltHeadHeight(uppHardwareSizes aSize)
        {
          
            double _rVal = 0;
           
            switch (aSize)
            {
                case uppHardwareSizes.ThreeEights:
                    _rVal = 0.208;
                    break;
                case uppHardwareSizes.M10:
                    _rVal = 5.8 / 25.4;
                    break;
            }

            return _rVal;
            
        }

     


        public static uppSpecTypes GetSpecType(uppPartTypes aPartType)
        {
            uppSpecTypes _rVal;
            switch (aPartType)
            {
                case uppPartTypes.HexBolt:
                    _rVal = uppSpecTypes.Bolt;
                    break;
                case uppPartTypes.HexNut:
                    _rVal = uppSpecTypes.Nut;
                    break;
                case uppPartTypes.LockWasher:
                    _rVal = uppSpecTypes.LockWasher;
                    break;
                case uppPartTypes.Stud:
                    _rVal = uppSpecTypes.ThreadedStud;
                    break;
                case uppPartTypes.FlatWasher:
                    _rVal = uppSpecTypes.FlatWasher;
                    break;
                case uppPartTypes.SetScrew:
                    _rVal = uppSpecTypes.SetScrew;
                    break;
                default:
                    _rVal = uppSpecTypes.Undefined;
                    break;
            }

            return _rVal;
        }
        #endregion

    }

}
