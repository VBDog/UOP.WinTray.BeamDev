using System;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Events;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;


namespace UOP.WinTray.Projects
{
    /// <summary>
    /// represents a pipe
    /// </summary>
    public class uopPipe : ICloneable
    {
        private TPIPE tStruc;

        #region Constructors

        public uopPipe() { tStruc = new TPIPE(); }

        internal uopPipe(TPIPE aStructure) { tStruc = aStructure; }
        internal uopPipe(string aDescriptor) { tStruc = new TPIPE(aDescriptor); }

        #endregion

        /// <summary>
        /// returns a new object that is an excact copy of the cloned object
        /// </summary>
        /// <returns></returns>
        public uopPipe Clone() => new uopPipe(tStruc.Clone());

        object ICloneable.Clone() => (object)this.Clone();

        /// <summary>
        /// the number of these nozzles
        /// </summary>
        public int Count  => tStruc.Count;

             
        /// <summary>
        /// the cross sectional area of the pipe ID in ft^2
        /// </summary>
        public uopProperty CrossSectionalArea
        {
            get
            {
                uopProperty _rVal = new uopProperty("CrossSectionalArea", 0, uppUnitTypes.BigArea);
                _rVal.SetValue(0.25 *  Math.PI * Math.Pow((int)IDProperty().UnitVariant(uppUnitFamilies.English) / 12.0, 2), 0);
                return _rVal;
            }
        }
        /// <summary>
        /// returns a property set to the current ID of the pipe
        /// </summary>
        /// <returns>returns a property set to the current ID of the pipe</returns>
        public uopProperty IDProperty() => new uopProperty("ID", tStruc.ID, uppUnitTypes.SmallLength);

        public string SizeDescriptor  =>tStruc.SizeDescriptor;
   
        public string Descriptor { get => tStruc.Descriptor; set => tStruc.Descriptor = value; }

        internal TPIPE Structure { get => tStruc; set => tStruc = value; }
     
        public string HTRIDatafileName
        {
            get
            {
                string _rVal = string.Empty;
                string aStr = tStruc.Spec.ToUpper().Trim();
                string bStr = aStr;
                bStr = bStr.Replace(" ", "_");
                bStr = bStr.Replace(".", "_");
                string eStr = ".table";
                switch (aStr)
                {
                    case "ANSI B36.10":
                        _rVal = "01-" + bStr;
                        break;
                    case "ANSI B36.19":
                        _rVal = "02-" + bStr;
                        break;
                    case "JIS G3452":
                        eStr = eStr.ToUpper();
                        _rVal = "03-" + bStr + "_SGP";
                        break;
                    case "JIS G3454":
                        eStr = eStr.ToUpper();
                        _rVal = "04-" + bStr + "_STGP";
                        break;
                    case "JIS G3455":
                        eStr = eStr.ToUpper();
                        _rVal = "05-" + bStr;
                        break;
                    case "JIS G3456":
                        eStr = eStr.ToUpper();
                        _rVal = "06-" + bStr;
                        break;
                    case "JIS G3459":
                        eStr = eStr.ToUpper();
                        _rVal = "07-" + bStr;
                        break;
                    case "JIS G3460":
                        eStr = eStr.ToUpper();
                        _rVal = "08-" + bStr;
                        break;
                }
                _rVal += eStr;
                return _rVal;
            }
        }
        /// <summary>
        /// the inner diameter of the pipe
        /// </summary>
        public double ID { get => tStruc.ID; set => tStruc.ID = value; }

      
        public double WallThickness => (tStruc.OD > 0 && tStruc.ID > 0) ?  (tStruc.OD - tStruc.ID) / 2: 0;
                
        /// <summary>
        ///returns True if the pipe is assigned to the inlet of its parent exchanger
        /// </summary>
        public bool IsInlet { get => tStruc.IsInlet; set => tStruc.IsInlet = value; }
        public bool IsOutlet { get => !tStruc.IsInlet; set => tStruc.IsInlet = !value; }
       
        /// <summary>
        /// the material of the pipe
        /// </summary>
        public string Material { get => tStruc.Material; set => tStruc.Material = value; }

        /// <summary>
        /// the reference name of the pipe
        /// contains nominal diameter, spec name and schedule
        /// </summary>
        public string SpecName { get => tStruc.SpecName ; set => tStruc.SpecName= value;  }

        
        /// <summary>
        /// the reference name of the pipe
        /// contains nominal diameter, spec name and schedule
        /// </summary>
        public string IDDescriptor  => tStruc.IDDescriptor;

        /// <summary>
        ///nominal diameter of the pipe with units
        /// </summary>
        public string NominalDia => tStruc.NominalDiameter  + "''";

        /// <summary>
        /// nominal diameter of the pipe
        /// </summary>
        public string NominalDiameter { get => tStruc.NominalDiameter; set => tStruc.NominalDiameter = value; }
       
        /// <summary>
        /// the outer diameter of the pipe
        /// </summary>
        public double OD { get => tStruc.OD; set => tStruc.OD = value; }

    
        /// <summary>
        /// the parent project of this simulation
        /// </summary>
        public uopProject Project => uopEvents.RetrieveProject(tStruc.ProjectHandle);
            

        /// <summary>
        /// the handle of the project that the object is associated to
        /// </summary>
        public string ProjectHandle { get => tStruc.ProjectHandle; set => tStruc.ProjectHandle = value; }
     
        /// <summary>
        /// the schedule of the pipe that the nozzles is made of
        /// </summary>
        public string Schedule { get => tStruc.Schedule; set => tStruc.Schedule = value; }
        /// <summary>
        /// the weight of the pipe in pounds per foot
        /// </summary>
        public double WeightPerFoot { get => tStruc.WeightPerFoot; set => tStruc.WeightPerFoot = value; }

        /// <summary>
        /// the spec which the nozzles properties conform to
        /// </summary>
        public string Spec { get => tStruc.Spec; set => tStruc.Spec = value; }
       

        public uopProperty AreaProperty()
        {
            return new uopProperty("InsideArea", Math.PI * Math.Pow(ID / 2, 2), uppUnitTypes.SmallArea);
        }
        /// <summary>
        /// returns the nominal diameter as a real number property (in Inches)
        /// </summary>
        /// <returns></returns>
        public uopProperty NominalSize()
        {
            
            double aVal;

            string aStr = tStruc.NominalDiameter;
            string wStr = aStr;
            string fStr = string.Empty;
            int i = aStr.IndexOf("-", StringComparison.OrdinalIgnoreCase);
            //InStr(1, aStr, "-");
            if (i != -1)
            {
                wStr = mzUtils.Left(aStr, i).Trim();//For left
                fStr = mzUtils.Right(aStr, i + 1);//for right
                //fStr = Right(aStr, aStr.Length - i).Trim();
            }
            else
            {
                if (aStr.IndexOf("/", StringComparison.OrdinalIgnoreCase) > -1)//   if (InStr(1, aStr, "/") > 0)
                {
                    wStr = "0";
                    fStr = aStr;
                }
            }
            aVal = Convert.ToDouble(wStr);

            if (!string.IsNullOrEmpty(fStr))
            {
                i = fStr.IndexOf("/", StringComparison.OrdinalIgnoreCase); //InStr(1, fStr, "/");
                if (i != -1)
                {
                    aStr = mzUtils.Left(fStr, i).Trim();
                    wStr = mzUtils.Right(fStr, i + 1).Trim();
                    //aStr = Left(fStr, i - 1).Trim();
                    //wStr = Right(fStr, fStr.Length - i).Trim();
                    if (mzUtils.IsNumeric(aStr) && mzUtils.IsNumeric(wStr))
                    {
                        aVal += (Convert.ToDouble(aStr) / Convert.ToDouble(wStr));
                    }
                }
            }

            return new uopProperty("NominalSize",aVal, uppUnitTypes.SmallLength);
         
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="aUnits">the unit system that the passed values are in</param>
        /// <param name="VolumeRate">the volumetric flow rate of material flowing through the pipe</param>
        /// <param name="FlowDensity">the density of the material flowing through the pipe</param>
        /// <param name="AreaMultiplier">the number of pipes carrying the flow (or other multiplier)</param>
        /// <returns>the density * velocity^2 of mass flowing through the pipe</returns>
        /// density is either lb/ft^3 or kg/m^3. VolumeRate is either ft^3/hr or m^3/hr

        public uopProperty RhoVSqr(uppUnitFamilies aUnits, double VolumeRate, dynamic FlowDensity, double AreaMultiplier = 0)
        {
            uopProperty _rVal = new uopProperty("RhoVSqr", 0, uppUnitTypes.RhoVSqr);
            try
            {
              
                if (aUnits == uppUnitFamilies.Default) aUnits = uppUnitFamilies.English;
             
                _rVal.UnitSystem = aUnits;

                double Veloc;

                Veloc = Velocity(aUnits, VolumeRate, AreaMultiplier).UnitVariant(uppUnitFamilies.English); //ft/s
                if (!mzUtils.IsNumeric(Veloc))
                {
                    throw new Exception();// goto Err;
                }
                if (!mzUtils.IsNumeric(FlowDensity))
                {
                    throw new Exception(); //  goto Err;
                }
                if (aUnits != uppUnitFamilies.English)
                {
                    FlowDensity *= (2.204622622 / 35.314666721); //kg/m^3 >> lb/ft^3
                }

                _rVal.Value = FlowDensity * Math.Pow(Veloc, 2);

                if (aUnits != uppUnitFamilies.English)
                {
                    _rVal.Value = _rVal.ConvertUnits(uppUnitFamilies.English, aUnits, _rVal.Value);
                }

                return _rVal;

            }
            catch (Exception)
            {
                _rVal = new uopProperty("RhoVSqr", "ERR");
                return _rVal;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aUnits">the unit system that the passed values are in</param>
        /// <param name="VolumeRate">the volumetric flow rate of material flowing through the pipe</param>
        /// <param name="AreaMultiplier">the number of pipes carrying the flow (or other multiplier)</param>
        /// <returns>returns the velocity of mass flowing through the pipe</returns>
        /// VolumeRate is either ft^3/hr or m^3/hr
        public uopProperty Velocity(Enums.uppUnitFamilies aUnits, double VolumeRate, double AreaMultiplier = 0)
        {
            uopProperty _rVal = new uopProperty("Velocity", 0, uppUnitTypes.Velocity);
            try
            {
               
                if (aUnits == uppUnitFamilies.Default) aUnits = uppUnitFamilies.English;
                
                _rVal.UnitSystem = aUnits;
                _rVal.Value = 0;

                if (!mzUtils.IsNumeric(VolumeRate))
                {
                    return _rVal;

                }
                if (VolumeRate <= 0)
                {
                    return _rVal;

                }
                if (AreaMultiplier <= 0)
                {
                    AreaMultiplier = Count;
                }
                if (AreaMultiplier <= 0)
                {
                    AreaMultiplier = 1;
                }

                //area in in^2
                var pArea = CrossSectionalArea.UnitVariant(uppUnitFamilies.English) * AreaMultiplier;
                if (pArea > 0)
                {
                    _rVal.Value = VolumeRate / pArea / 3600;
                    if (aUnits != uppUnitFamilies.English)
                    {
                        _rVal.Value = _rVal.ConvertUnits(uppUnitFamilies.English, aUnits, _rVal.Value);
                    }
                }
                return _rVal;
            }
            catch
            {
                _rVal = new uopProperty("Velocity", "Err");
                return _rVal;
            }
        }
    }
}