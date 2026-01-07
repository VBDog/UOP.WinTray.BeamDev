using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Structures;


namespace UOP.WinTray.Projects.Utilities
{
    public class uopUnits
    {


        #region Variables

        static TUNITS _gsUnits;

        #endregion

        #region Properties

        internal static TUNITS gsUnits =>  _gsUnits;
          
        #endregion

        #region Constructor

        static uopUnits()
        {
            _gsUnits = new TUNITS();
          
            _gsUnits.Add(uppUnitTypes.SmallLength);
            _gsUnits.Add(uppUnitTypes.BigLength);
            _gsUnits.Add(uppUnitTypes.SmallArea);
            _gsUnits.Add(uppUnitTypes.BigArea);
            _gsUnits.Add(uppUnitTypes.Percentage);
            _gsUnits.Add(uppUnitTypes.BigPercentage);
            _gsUnits.Add(uppUnitTypes.VolumePercentage);
            _gsUnits.Add(uppUnitTypes.MassRate);
            _gsUnits.Add(uppUnitTypes.BigMassRate);
            _gsUnits.Add(uppUnitTypes.Density);
            _gsUnits.Add(uppUnitTypes.VolumeRate);
            _gsUnits.Add(uppUnitTypes.Viscosity);
            _gsUnits.Add(uppUnitTypes.SurfaceTension);
            _gsUnits.Add(uppUnitTypes.Seconds);
            _gsUnits.Add(uppUnitTypes.RhoVSqr);
            _gsUnits.Add(uppUnitTypes.Velocity);
            _gsUnits.Add(uppUnitTypes.Weight);
            _gsUnits.Add(uppUnitTypes.Temperature);
            _gsUnits.Add(uppUnitTypes.Pressure);
        }

        #endregion

        //returns the english untis to metric or SI units converion factor
        public static int UnitPrecision(uppUnitTypes aUnitType, uppUnitFamilies aFamily)
        {

            switch (aUnitType)
            {
                case uppUnitTypes.SmallLength:
                    return (aFamily != uppUnitFamilies.English) ? 1 : 4;
                case uppUnitTypes.BigLength:
                    return (aFamily != uppUnitFamilies.English) ? 1 : 3;
                case uppUnitTypes.SmallArea:
                    return (aFamily != uppUnitFamilies.English) ? 1 : 3;
                case uppUnitTypes.BigArea:
                    return (aFamily != uppUnitFamilies.English) ? 1 : 3;
                case uppUnitTypes.Percentage:
                    return (aFamily != uppUnitFamilies.English) ? 2 : 2;
                case uppUnitTypes.BigPercentage:
                    return (aFamily != uppUnitFamilies.English) ? 2 : 2;
                case uppUnitTypes.VolumePercentage:
                    return (aFamily != uppUnitFamilies.English) ? 1 : 1;
                case uppUnitTypes.MassRate:
                    return (aFamily != uppUnitFamilies.English) ? 1 : 3;
                case uppUnitTypes.BigMassRate:
                    return (aFamily != uppUnitFamilies.English) ? 1 : 3;
                case uppUnitTypes.Density:
                    return (aFamily != uppUnitFamilies.English) ? 1 : 3;
                case uppUnitTypes.VolumeRate:
                    return (aFamily != uppUnitFamilies.English) ? 1 : 3;
                case uppUnitTypes.Viscosity:
                    return (aFamily != uppUnitFamilies.English) ? 3 : 3;
                case uppUnitTypes.SurfaceTension:
                    return (aFamily != uppUnitFamilies.English) ? 3 : 3;
                case uppUnitTypes.Temperature:
                    return (aFamily != uppUnitFamilies.English) ? 1 : 1;
                case uppUnitTypes.Seconds:
                    return (aFamily != uppUnitFamilies.English) ? 1 : 1;
                case uppUnitTypes.RhoVSqr:
                    return (aFamily != uppUnitFamilies.English) ? 1 :  3;
                case uppUnitTypes.Velocity:
                    return (aFamily != uppUnitFamilies.English) ? 1 : 3;
                case uppUnitTypes.Pressure:
                    return (aFamily != uppUnitFamilies.English) ? 1 : 3;
                case uppUnitTypes.Weight:
                    return (aFamily != uppUnitFamilies.English) ? 1 : 2;
                default:
                    return (aFamily != uppUnitFamilies.English) ? 1 : 3;
            }


        }

        //returns the english untis to metric or SI units converion factor
        public static double UnitFactor(uppUnitTypes aUnitType, uppUnitFamilies aFamily)
        {

           switch (aUnitType)
            {
                case uppUnitTypes.SmallLength:
                    return (aFamily != uppUnitFamilies.English) ? 25.4 : 1;
                case uppUnitTypes.BigLength:
                    return (aFamily != uppUnitFamilies.English) ? 0.3048 : 1 ;
                case uppUnitTypes.SmallArea:
                    return (aFamily != uppUnitFamilies.English) ? 6.4516 : 1 ;
                case uppUnitTypes.BigArea:
                    return (aFamily != uppUnitFamilies.English) ? 0.09290304: 1 ;
                case uppUnitTypes.Percentage:
                 case uppUnitTypes.BigPercentage:
                case uppUnitTypes.VolumePercentage:
                    return (aFamily != uppUnitFamilies.English) ? 1 : 1;
                case uppUnitTypes.MassRate:
                    return (aFamily != uppUnitFamilies.English) ? 0.4535923 : 1 ;
                case uppUnitTypes.BigMassRate:
                    return (aFamily != uppUnitFamilies.English) ? 0.45359237 : 1 ;
                case uppUnitTypes.Density:
                    return (aFamily != uppUnitFamilies.English) ? 16.018463374 : 1 ;
                case uppUnitTypes.VolumeRate:
                    return (aFamily != uppUnitFamilies.English) ? 0.0283168466 : 1 ;
                case uppUnitTypes.Viscosity:
                 case uppUnitTypes.SurfaceTension:
                case uppUnitTypes.Seconds:
                case uppUnitTypes.Temperature:
                    return (aFamily != uppUnitFamilies.English) ? 1 : 1;
                case uppUnitTypes.RhoVSqr:
                    return (aFamily != uppUnitFamilies.English) ? (0.45359237 * 3.280839895) : 1 ;
                case uppUnitTypes.Velocity:
                    return (aFamily != uppUnitFamilies.English) ? 0.3048 : 1; 
                case uppUnitTypes.Weight:
                    return (aFamily != uppUnitFamilies.English) ? 0.45359237 : 1 ;
                case uppUnitTypes.Pressure:
                    return (aFamily != uppUnitFamilies.English) ? 0.0193368 : 1 ;
                default:
                    return 1;
            }

          
        }

        public static string UnitLabel(uppUnitTypes aUnitType, uppUnitFamilies aFamily)
        {

            string dot = ((char)183).ToString();

            switch (aUnitType)
            {
                case uppUnitTypes.SmallLength:
                    return (aFamily != uppUnitFamilies.English) ? "mm" : "in.";
                case uppUnitTypes.BigLength:
                    return (aFamily != uppUnitFamilies.English) ? "m" : "ft.";
                case uppUnitTypes.SmallArea:
                    return (aFamily != uppUnitFamilies.English) ? "cm²" : "in²";
                case uppUnitTypes.BigArea:
                    return (aFamily != uppUnitFamilies.English) ? "m²" : "ft²";
                case uppUnitTypes.Percentage:
                case uppUnitTypes.BigPercentage:
                    return (aFamily != uppUnitFamilies.English) ? "%" : "%";
                case uppUnitTypes.VolumePercentage:
                    return (aFamily != uppUnitFamilies.English) ? "vol%" : "vol%" ;
                case uppUnitTypes.MassRate:
                    return (aFamily != uppUnitFamilies.English) ? "kg/hr": "lb/hr";
                case uppUnitTypes.BigMassRate:
                    return (aFamily != uppUnitFamilies.English) ? "kg/hr" : "lb/hr";
                case uppUnitTypes.Density:
                    return (aFamily != uppUnitFamilies.English) ? "kg/m³" : "lb/ft³";
                case uppUnitTypes.VolumeRate:
                    return (aFamily != uppUnitFamilies.English) ? "m³/hr" : "ft³/hr";
                case uppUnitTypes.Viscosity:
                    return (aFamily != uppUnitFamilies.English) ? "cP" : "cP";
                case uppUnitTypes.SurfaceTension:
                    return (aFamily != uppUnitFamilies.English) ? "dyne/cm" : "dyne/cm";
                case uppUnitTypes.Seconds:
                    return (aFamily != uppUnitFamilies.English) ? "sec" : "sec";
                case uppUnitTypes.RhoVSqr:
                    return (aFamily != uppUnitFamilies.English) ? "kg/m" + dot + "s²": "lb/ft" + dot + "s²" ;
                case uppUnitTypes.Velocity:
                    return (aFamily != uppUnitFamilies.English) ? "m/sec" : "ft/sec";
                case uppUnitTypes.Weight:
                    return (aFamily != uppUnitFamilies.English) ? "kg" : "lb";
                case uppUnitTypes.Temperature:
                    return (aFamily != uppUnitFamilies.English) ? "°C" : "°F";
                case uppUnitTypes.Pressure:
                    return (aFamily != uppUnitFamilies.English) ? "mmHg" : "psia" ;
                default:
                    return "";
            }
     
            
         }

       

        public uopUnit Item(int aIndex) => new uopUnit(_gsUnits.Item(aIndex));

        public uopUnit Unit(uppUnitTypes aUnitType) => new uopUnit(_gsUnits.GetByType(aUnitType));

        public static double ConvertUnits(double aValue, uppUnitTypes aUnitType, uppUnitFamilies aFromUnits, uppUnitFamilies aToUnits, int aPrecis)
        {
            return ConvertUnits(aValue, aUnitType, aFromUnits, aToUnits, aPrecis, out uopUnit UNITOBJ);
        }

         public static double ConvertUnits(double aValue, uppUnitTypes aUnitType, uppUnitFamilies aFromUnits, uppUnitFamilies aToUnits, int aPrecis, out uopUnit rUnitObj)
        {
            rUnitObj = null;
            if (aFromUnits <= uppUnitFamilies.Default) return aValue;

            TUNIT unit = gsUnits.GetByType(aUnitType);
            if (unit.UnitType == uppUnitTypes.Undefined) return aValue;
            rUnitObj = new uopUnit(unit);
            return  unit.ConvertValue(aValue, aFromUnits, aToUnits, aPrecis);
        }

        public static uopUnit GetUnit(uppUnitTypes aUnitType) => new uopUnit(gsUnits.GetByType(aUnitType));

        public static string GetUnitLabel(uppUnitTypes aUnitType, uppUnitFamilies aUnitFamily) => gsUnits.GetUnitLabel(aUnitType, aUnitFamily);
        public static double GetUnitFactor(uppUnitTypes aUnitType, uppUnitFamilies aUnitFamily) => gsUnits.GetUnitFactor(aUnitType, aUnitFamily);

     
    }
}
