using System;
using System.Collections.Generic;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Interfaces;

using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.Projects.Parts
{
    /// <summary>
    /// represents as Distributor in an MD project
    /// </summary>
    public class mdDistributor :  uopPart, iCaseOwner
    {
        public override uppPartTypes BasePartType => uppPartTypes.Distributor;

        #region Variables

        private colUOPParts _Cases = null;

        #endregion Variables

        #region Constructors

        public mdDistributor() : base(uppPartTypes.Distributor, uppProjectFamilies.uopFamMD, "", "", true)
        {
            InitializeProperties();
        }


        internal mdDistributor(mdDistributor aPartToCopy, colUOPParts aCases) : base(uppPartTypes.Distributor, uppProjectFamilies.uopFamMD, "", "", true)
        {
            if (aPartToCopy == null) { InitializeProperties(); } else { base.Copy(aPartToCopy); }
            _Cases = (aCases == null) ? new colUOPParts() : aCases.Clone();
            _Cases.MaintainIndices = true;
        }
        private void InitializeProperties()
        {
               _Cases = new colUOPParts(this) { MaintainIndices = true } ;
      

            AddProperty("Description", "");
            AddProperty("NozzleLabel", "");
            AddProperty("TrayAbove", "", aNullVal: "");
            AddProperty("TrayBelow", "", aNullVal: "");
            AddProperty("NozzleCount", -999, aNullVal: -999, aCategory:"MECHANICAL");
            AddProperty("NozzlePipe", "", aDisplayName: "Nozzle Pipe", aCategory: "MECHANICAL");
            AddProperty("PipeConfiguration", uppPipeConfigurations.I, aHeading: "Pipe Configuration", aDecodeString: "0=I,1=T,2=H", aCategory: "MECHANICAL");
            AddProperty("Pipe", "", aDisplayName: "Pipe", aCategory: "MECHANICAL");
            AddProperty("PipeHoleCount", -999, aNullVal: -999, aCategory: "MECHANICAL");
            AddProperty("PipeHoleSize", -999, uppUnitTypes.SmallLength, aNullVal: -999, aCategory: "MECHANICAL");
            AddProperty("Tiers", -999, aNullVal: -999, aCategory: "MECHANICAL");
            AddProperty("Troughs", -999, aNullVal: -999, aCategory: "MECHANICAL");
            AddProperty("TroughHoleCount", -999, aNullVal: -999, aCategory: "MECHANICAL");
            AddProperty("TroughHoleSize", -999, uppUnitTypes.SmallLength, aNullVal: -999, aCategory: "MECHANICAL");
            AddProperty("TroughHeight", -999, uppUnitTypes.SmallLength, aNullVal: -999, aCategory: "MECHANICAL");
            AddProperty("DispersionDevice", 0, aDisplayName: "Dispersion Device", aDecodeString: "0=None,1=Flat Plate,2=V-Plate", aCategory: "MECHANICAL");
            AddProperty("DistributorRequired", true);
            AddProperty("MinimizePressureDrop", false);
            AddProperty("ContainsHFTubes", @"N\A");
            PropsLockTypes(true);
        }

        #endregion

        uppCaseOwnerOwnerTypes iCaseOwner.OwnerType => uppCaseOwnerOwnerTypes.Distributor;

        public override string ToString() => $"DISTRIBUTOR({ Index })";

        #region Public Properties

        public string ContainsHFTubes { get => PropValS("ContainsHFTubes"); set => Notify(PropValSet("ContainsHFTubes", value)); }
         

        /// <summary>
        /// the number of cases defined for this object
        /// </summary>
        public int CaseCount => Cases.Count;

        /// <summary>
        /// the flow cases defined for this distributor
        /// </summary>
        public colUOPParts Cases
        {
            get { _Cases.SubPart(this); return _Cases; }
            
            set
            {
                if (value == null) _Cases.Clear(); 
                _Cases.SubPart(this);
               if (_Cases.Count<= 0) AddCase(new mdDistributorCase());
             
            }
        }

        /// <summary>
        ///returns the objects properties in a collection
        /// signatures like "COLOR=RED"
        /// </summary>
         public override uopProperties CurrentProperties()  { UpdatePartProperties(); return base.CurrentProperties(); }


        public override string Description
        {
            get
            {
                string _rVal = PropValS("Description");
                if (_rVal ==  string.Empty)
                {
                    _rVal = "Distrib " + Index;
                    PropValSet("Description", _rVal, bSuppressEvnts:true);
                }

                return _rVal;
            }
            set => Notify(PropValSet("Description", value));
        }

        public override string DescriptiveName
        {
            get
            {
                string _rVal = Description.Trim();
                if (_rVal ==  string.Empty) return _rVal;
                string tb = TrayBelow.Trim();
                string ta = TrayAbove.Trim();
                if (!string.IsNullOrWhiteSpace(tb) && string.Compare(tb, "-") != 0)
                {
                    _rVal += $" Above Tray {tb}";
                }
                else if (!string.IsNullOrWhiteSpace(ta) && string.Compare(ta, "-") != 0)
                {
                    _rVal += $" Below Tray {ta}";
                }
                return _rVal;
            }
        }
        public int DispersionDevice
        {
            get => PropValI("DispersionDevice");
            set
            {
                if (value >= 0 && value <= 2)
                { Notify(PropValSet("DispersionDevice", value)); }

            }
        }

       

        public override string INIPath => $"DISTRIBUTORS({ Index })";

        /// <summary>
        /// the collection index of the distributor
        /// </summary>
        public override int Index { get => PartIndex; set => PartIndex = Math.Abs(value); }

        public bool DistributorRequired { get => PropValB("DistributorRequired");set => PropValSet("DistributorRequired", value); }

        public bool MinimizePressureDrop { get => PropValB("MinimizePressureDrop"); set => PropValSet("MinimizePressureDrop", value); }

        public override string Name => $"Distributor - {Description}" ;

        /// <summary>
        ///the number of the distributors nozzle
        /// </summary>
        public int NozzleCount { get => PropValI("NozzleCount"); set => Notify(PropValSet("NozzleCount", value)); }

        /// <summary>
        ///the label of the nozzle associated to the distributor
        /// </summary>
        public string NozzleLabel { get => PropValS("NozzleLabel"); set => Notify(PropValSet("NozzleLabel", value));  }
            
        public uopPipe NozzleObject { get { string sPipe = NozzlePipe; return (!string.IsNullOrWhiteSpace(sPipe))? new uopPipe(sPipe) : null; } }

        /// <summary>
        /// the name of the distributors nozzle
        /// </summary>
        public string NozzlePipe { get => PropValS("NozzlePipe"); set => Notify(PropValSet("NozzlePipe", value)); }
 
 
        /// <summary>
        /// the name of the distributors pipe
        /// </summary>
        public string Pipe { get => PropValS("Pipe"); set => Notify(PropValSet("Pipe", value)); }

        public uppPipeConfigurations PipeConfiguration { get => (uppPipeConfigurations)PropValI("PipeConfiguration"); set => Notify(PropValSet("PipeConfiguration", value)); }

        /// <summary>
        ///returns the diameter of the holes in the pipes
        /// </summary>
        public uopProperty PipeHoleDiameter => new uopProperty(new TPROPERTY("PipeHoleDiameter", PropValD("PipeHoleSize", 0),uppUnitTypes.SmallLength));

        public uopPipe PipeObject {  get { string sPipe = Pipe; return (!string.IsNullOrEmpty(sPipe)) ? new uopPipe(sPipe) : null; } }



        /// <summary>
        ///returns the properties required to save the object to file
        ///signatures like "COLOR=RED"
        /// </summary>
        public override  uopPropertyArray SaveProperties(string aHeading = null)
        {
           
                UpdatePartProperties();
            aHeading = string.IsNullOrWhiteSpace(aHeading) ? INIPath : aHeading.Trim();
                return base.SaveProperties(aHeading);
           
        }

        public new string SpanName => TrayAbove + "/" + TrayBelow;

      
        /// <summary>
        /// the number of tiers in the distributor
        /// </summary>
        public int Tiers { get => PropValI("Tiers"); set => Notify(PropValSet("Tiers", value)); }

        /// <summary>
        /// the tray number of the tray above this distributor
        /// </summary>
        public string TrayAbove { get => PropValS("TrayAbove"); set => Notify(PropValSet("TrayAbove", value)); }

        /// <summary>
        ///the tray number of the tray below this distributor
        /// </summary>
        public string TrayBelow { get => PropValS("TrayBelow"); set => Notify(PropValSet("TrayBelow", value)); }

        /// <summary>
        ///the tray number of the tray below this distributor
        /// </summary>
        public int TrayNumberBelow { get => mzUtils.VarToInteger( TrayBelow); set => Notify(PropValSet("TrayBelow", value.ToString())); }

        /// <summary>
        /// the inside height of troughs in the distributor in inches
        /// </summary>
        public double TroughHeight { get => PropValD("TroughHeight"); set => Notify(PropValSet("TroughHeight", value)); }

        /// <summary>
        /// the number of holes in the troughs in the distributor
        /// </summary>
        public int TroughHoleCount { get => PropValI("TroughHoleCount"); set => Notify(PropValSet("TroughHoleCount", value)); }

        /// <summary>
        ///returns the diameter of the holes in the throughs
        /// </summary>
        public double TroughHoleSize  { get => PropValD("TroughHoleSize"); set => Notify(PropValSet("TroughHoleSize", value)); }

        /// <summary>
        ///the number of troughs in the distributor
        /// </summary>
        public int Troughs { get => PropValI("Troughs"); set => Notify(PropValSet("Troughs", value)); }

        #endregion

        #region Private Properties

        /// <summary>
        /// the collection that this distributor is a member of
        /// </summary>
        public new colMDDistributors Collection => (colMDDistributors)base.Collection();

        int iCaseOwner.CaseCount => CaseCount;

        colUOPParts iCaseOwner.Cases { get => Cases; set => Cases = value; }

        /// <summary>
        /// Cases
        /// </summary>
        List<iCase> iCaseOwner.CaseList
        {
            get
            {
                colUOPParts cases = Cases;
                List<iCase> _rVal = new List<iCase>();
                foreach (var item in cases) { _rVal.Add((iCase)item); }
                return _rVal;
            }

            set
            {
                colUOPParts cases = Cases;
                cases.Clear();
                if (value == null) return;
                foreach (var item in value) { cases.Add((uopPart)item); }
            }
        }

        uopProperties iCaseOwner.CurrentProperties () => CurrentProperties(); 

        string iCaseOwner.Description { get => Description; set => Description = value; }

        int iCaseOwner.Index => Index;

        string iCaseOwner.Name => Name;
        
        string iCaseOwner.NozzleLabel { get => NozzleLabel; set => NozzleLabel = value; }

        string iCaseOwner.ObjectType => "Distributor";
     
        string iCaseOwner.PartPath => PartPath();
        
        string iCaseOwner.TrayAbove { get => TrayAbove; set => TrayAbove = value; 
        }
        string iCaseOwner.TrayBelow { get => TrayBelow; set => TrayBelow = value; }
        public bool AddCase(iCase aCase)
        {
            if (aCase == null) return false;
            if (aCase.OwnerType != uppCaseOwnerOwnerTypes.Distributor || string.IsNullOrWhiteSpace(aCase.Description)) return false;
            if (Cases.FindIndex(x => string.Compare(aCase.Description, x.Description, true) == 0) > 0) return false;
            Cases.Add((uopPart)aCase);
            return true;

        }
        public bool ReNameCase(string aCurrentName, string aNewName)
        {
            if (Cases.Count <= 0 || string.IsNullOrWhiteSpace(aCurrentName) || string.IsNullOrWhiteSpace(aNewName)) return false;
            aNewName = aNewName.Trim();
            bool _rVal = false;
            int idx = Cases.FindIndex(x => string.Compare(aCurrentName, x.Description, true) == 0);
            if (idx <= 0) return false;
            uopPart acase = Cases.Item(idx);
            _rVal = string.Compare(acase.Description, aNewName, false) != 0;
            acase.Description = aNewName;
            return _rVal;

        }
        #endregion


        #region Private Methods


        /// <summary>
        /// used by an object to respond to property changes of itself and of objects below it in the object model.
        /// also raises the changed property to the object above it it the object model.
        /// </summary>
        /// <param name="aProperty"></param>
        internal void Notify(uopProperty aProperty)
        {
            if (aProperty == null || aProperty.Protected) return;

            //TODO 
            //RaiseEvent(DistributorPropertyChange(aProperty));
            colMDDistributors MyCollection = Collection;
            if (MyCollection != null) MyCollection.NotifyMemberChange(this, aProperty);
         
        }
          
        iCaseOwner iCaseOwner.Clone() => (iCaseOwner)Clone();

        iCase iCaseOwner.GetCase(int aCaseIndex) => (iCase)GetCase(aCaseIndex);
        
        dynamic iCaseOwner.GetPropertyValue(dynamic aPropertyNameorIndex, out bool bExists) {  bExists = false; return PropValGet(aPropertyNameorIndex,out  bExists, aPartType: uppPartTypes.Undefined); }

        bool iCaseOwner.PropValSet(dynamic aPropertyNameorIndex, dynamic aNewValue) { return PropValSet(aPropertyNameorIndex, aNewValue) != null; }

        public override bool IsEqual(uopPart aPart)
        {
            if (aPart == null) return false;
            if (aPart.PartType != PartType)  return false;

          return IsEqual((mdDistributor)aPart);
        }


        #endregion

        #region Public Methods

        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        public mdDistributor Clone() => new mdDistributor(this, _Cases);

        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();


        public override uopProperty CurrentProperty(string aPropertyName, bool bSupressNotFoundError = false) { UpdatePartProperties(); return base.CurrentProperty(aPropertyName, bSupressNotFoundError); }

        public mdDistributorCase GetCase(int aCaseIndex) =>  (mdDistributorCase)Cases.Item(aCaseIndex);
        

        /// <summary>
        /// returns true if the passed part is equal to this one
        /// </summary>
        /// <param name="aDistrib"></param>
        /// <returns></returns>
        public bool IsEqual(mdDistributor aDistrib)
        {
        
            try
            {
                if (aDistrib == null) return false;

                if (aDistrib.Cases.Count != Cases.Count)  return false;
            if (TPROPERTIES.Compare(aDistrib.ActiveProps, ActiveProps)) { return !aDistrib.Cases.IsEqual(Cases);}
                
                return true;
            }
            catch { return false; }

        }
        public uopProperty NozzleArea(uopPipe aPipe = null)
        {
            uopProperty _rVal = new uopProperty("NozzleArea", "N/A");
            _rVal.SubPart(this);

            if (aPipe == null)   aPipe = uopGlobals.UopGlobals.Pipes.GetByDescriptor(NozzlePipe);
            
            if (aPipe != null) _rVal = aPipe.CrossSectionalArea;
           

            _rVal.Name = "NozzleArea";
            return _rVal;
        }

        /// <summary>
        ///returns the velocity of fluid in the feed pipe based on the fluid properties defined in the passed case
        /// </summary>
        /// <param name="aCase"></param>
        /// <param name="aPipe"></param>
        /// <returns></returns>
        public uopProperty NozzleRhoVsqr(mdDistributorCase aCase, uopPipe aPipe = null)
        {
            uopProperty _rVal = new uopProperty("NozzleRhoVsqr", "N/A") ;
            _rVal.SubPart(this);
            int cnt = 0;

       

            if (aPipe == null)  aPipe = NozzleObject;
        
            if (aPipe == null) return _rVal;

         
            if (aCase == null)  return _rVal; 

            cnt = NozzleCount;
            if (cnt <= 0) return _rVal;

            _rVal = new uopProperty("NozzleRhoVsqr", aPipe.RhoVSqr(uppUnitFamilies.English, aCase.TotalFeedVolume / cnt, aCase.FeedDensity.Value).Value, uppUnitTypes.RhoVSqr)  ;
            _rVal.SubPart(this);
            return _rVal;
        }

        /// <summary>
        ///returns the velocity of fluid in the feed pipe based on the fluid properties defined in the passed case
        /// </summary>
        /// <param name="aCase"></param>
        /// <param name="aPipe"></param>
        /// <returns></returns>
        public uopProperty NozzleVelocity(mdDistributorCase aCase, uopPipe aPipe = null)
        {
            uopProperty _rVal = new uopProperty("NozzleVelocity", "N/A");
            _rVal.SubPart(this);



            if (aPipe == null) aPipe = NozzleObject;
            
            if (aPipe == null) return _rVal;

            if (aCase == null) return _rVal;

            dynamic cnt = null;

            cnt = NozzleCount;
            if (cnt <= 0) return _rVal;
            uopProperty pipevel  = aPipe.Velocity(uppUnitFamilies.English, aCase.FeedVolumeRate.UnitVariant(uppUnitFamilies.English) / cnt, 1);

            aPipe.Velocity(uppUnitFamilies.English, aCase.FeedVolumeRate.UnitVariant(uppUnitFamilies.English) / cnt, 1);
            if (pipevel.HasUnits)
            {
                _rVal = new uopProperty("NozzleVelocity", pipevel.Value,uppUnitTypes.Velocity) ;
            }
            else
            {
                _rVal.Value = "N/A";
            }
            _rVal.SubPart(this);
            return _rVal;
        }

        public uopProperty PipeArea(uopPipe aPipe = null)
        {
            uopProperty _rVal = null;
            _rVal = new uopProperty("PipeArea", "N/A");


            if (aPipe == null)
            {
                //TODO
                //aPipe = uopGlobals.goPipes.Item(Pipe);
            }
            if (aPipe != null)
            {
                _rVal = aPipe.CrossSectionalArea;
            }
            else
            {
                _rVal.SetValue("N/A");
            }
            _rVal.SubPart(this);
            _rVal.Name = "PipeArea";
            return _rVal;
        }

        /// <summary>
        ///returns the the total pipe hole are in ft^2
        /// </summary>
        /// <returns></returns>
        public uopProperty PipeHoleArea()
        {
            uopProperty _rVal = new uopProperty("PipeHoleArea", "N/A");
            int cnt =  PropValI("PipeHoleCount") ;
            if (cnt <= 0) { return _rVal; }
            double ID  = PropUnitValue("PipeHoleSize", uppUnitFamilies.English);
            double aR = 0;

            if (ID <= 0) { return _rVal; }


            //hole area in ft^2
            aR = Math.Pow( cnt * Math.PI * (0.5 * ID / 12), 2);

            if (aR > 0)
            {
                _rVal = new uopProperty("PipeHoleArea", aR, uppUnitTypes.BigArea) ;
               
            }
            _rVal.SubPart(this);
            return _rVal;
        }

        /// <summary>
       ///returns the ratio of the total pipe hole area to the total pipe area
        /// </summary>
        /// <param name="aPipe"></param>
        /// <returns></returns>
        public uopProperty PipeHoleAreaRatio(uopPipe aPipe = null)
        {
            uopProperty _rVal = new uopProperty("PipeHoleAreaRatio", "N/A");


            dynamic aVal = null;

            dynamic bVal = null;

            if (aPipe == null) aPipe = PipeObject;
         
            if (aPipe == null) return _rVal;

           
            if (PipeConfiguration == uppPipeConfigurations.H)
            {
                aVal = aPipe.CrossSectionalArea.Value * 4;
            }
            else if (PipeConfiguration == uppPipeConfigurations.T)
            {
                aVal = aPipe.CrossSectionalArea.Value * 2;
            }
            else
            {
                aVal = aPipe.CrossSectionalArea.Value;
            }
            if (aVal > 0)
            {
                bVal = PipeHoleArea().Value;
                if (mzUtils.IsNumeric(bVal) && bVal > 0)
                {
                    int nzs = 0;

                    nzs = NozzleCount;
                    if (nzs > 0)
                    {
                        bVal *= nzs;
                        _rVal.Value = aVal / bVal;
                        _rVal.FormatString = "0.00";
                    }

                }
            }
            _rVal.SubPart(this);
            return _rVal;
        }

        /// <summary>
        /// ///returns the Rho V ^2 of fluid passing through the pipe holes based on the fluid properties defined in the passed case
        /// </summary>
        /// <param name="aCase"></param>
        /// <returns></returns>
        public uopProperty PipeHoleRhoVSqr(mdDistributorCase aCase)
        {
            uopProperty _rVal = new uopProperty("PipeHoleRhoVSqr","N/A");
            if (aCase == null) {return _rVal;}

            int cnt = PropValI("PipeHoleCount");
            double vrate = aCase.TotalFeedVolume;
            double ID = PropUnitValue("PipeHoleSize", uppUnitFamilies.English);
      
            if (cnt <= 0 || vrate <=0 || ID <= 0 ) { return _rVal; }
            
            dynamic vel = null;

            int nzs  = NozzleCount;


            vel = PipeHoleVelocity(aCase).Value;
            _rVal = new uopProperty("PipeHoleRhoVSqr", aCase.FeedDensity.Value * Math.Pow(vel, 2), uppUnitTypes.RhoVSqr) ;
            _rVal.SubPart(this);
            return _rVal;
        }

        /// <summary>
        ///returns the velocity of fluid passing through the pipe holes based on the fluid properties defined in the passed case
        /// </summary>
        /// <param name="aCase"></param>
        /// <param name="bAtTurnDown"></param>
        /// <returns></returns>
        public uopProperty PipeHoleVelocity(mdDistributorCase aCase, bool bAtTurnDown = false)
        {
            uopProperty _rVal = new uopProperty("PipeHoleVelocity", "N/A");
            if (aCase == null) { return _rVal; }
            double vrate = (double)aCase.FeedVolumeRate.UnitVariant(uppUnitFamilies.English);
            if (bAtTurnDown) { vrate *= (aCase.MinimumOperatingRange / 100); }

            int cnt = PropValI("PipeHoleCount");

            double ID = PropUnitValue("PipeHoleSize", uppUnitFamilies.English);

            if (vrate <= 0 || cnt <= 0 || ID <=0 ) { return _rVal; }



            dynamic aR = cnt * Math.PI * Math.Pow(0.5 * ID / 12, 2);

        

            if (aR > 0)
            {
                _rVal = new uopProperty("PipeHoleVelocity", vrate / aR / 3600, uppUnitTypes.Velocity) ;
           
            }
            _rVal.SubPart(this);
            return _rVal;
        }

        /// <summary>
        ///returns the velocity of fluid in the distributor pipe based on the fluid properties defined in the passed case
        /// </summary>
        /// <param name="aCase"></param>
        /// <param name="aPipe"></param>
        /// <returns></returns>
        public uopProperty PipeRhoVsqr(mdDistributorCase aCase, uopPipe aPipe = null)
        {
            uopProperty _rVal = new uopProperty("PipeRhoVsqr","N/A");

            int nzs = 0;


            nzs = NozzleCount;
        
            if (aPipe == null) aPipe = PipeObject;
            if (aPipe == null || aCase == null) return _rVal;
       
            dynamic vel = null;
            double vrate = 0;
            dynamic fdens = null;
            vel = PipeVelocity(aCase).Value;
            if (!mzUtils.IsNumeric(vel)) return _rVal;

        

            vrate = aCase.TotalFeedVolume;

            fdens = aCase.FeedDensity.Value;
            if (!mzUtils.IsNumeric(fdens)) return _rVal;

            _rVal = new uopProperty("PipeRhoVsqr", fdens * Math.Pow(vel, 2), uppUnitTypes.RhoVSqr);

            _rVal.SubPart(this);
            return _rVal;
        }

        /// <summary>
        /// ///returns the velocity of fluid in the pipe based on the fluid properties defined in the passed case
        /// </summary>
        /// <param name="aCase"></param>
        /// <param name="aPipe"></param>
        /// <returns></returns>
        public uopProperty PipeVelocity(mdDistributorCase aCase, uopPipe aPipe = null)
        {
            uopProperty _rVal = new uopProperty("PipeVelocity","N/A");

            dynamic fctr = null;

            dynamic vrate = null;

            int nzs =  NozzleCount;
          
            if (aPipe == null) aPipe = PipeObject;
         
            if (aPipe == null || aCase == null || nzs <= 0)  return _rVal;

          
            if (PipeConfiguration == uppPipeConfigurations.T)
            { fctr = 0.5; }
            else if (PipeConfiguration == uppPipeConfigurations.H)
            { fctr = 0.25; }
            else
            { fctr = 1; }

            vrate = aCase.FeedVolumeRate.UnitVariant(uppUnitFamilies.English) * fctr;
            _rVal = new uopProperty("PipeVelocity", aPipe.Velocity(uppUnitFamilies.English, vrate, 1).Value / nzs, uppUnitTypes.Velocity);
            _rVal.SubPart(this);
            return _rVal;
        }



        /// <summary>
        /// extracts the parts property values from the passed file array that was read from an INI style project file.
        /// </summary>
        /// <param name="aProject">The project requesting the read event</param>
        /// <param name="aFileProps">The property array containing the INI file properties or a subset. The Name of the array should contain the original file name.</param>
        /// <param name="ioWarnings">A collection to populate if errors or warnings are found during the property value extraction  </param>
        /// <param name="aFileVersion">The version of th efile being read. Supplied to account for backward compatibility</param>
        /// <param name="aFileSection">the INI file heading to search for the properties to extract </param>
        /// <param name="bIgnoreNotFound">A flag to ignore properties that exist on the part but were not found in the file properties</param>
        /// <param name="aAssy">An optional parent tray assembly for the part being read</param>
        /// <param name="aPartParameter">An optional parent part for the part being read</param>
        /// <param name="aSkipList">An optional list of property names to skip over during the read</param>
        public override void  ReadProperties(uopProject aProject, uopPropertyArray aFileProps, ref uopDocuments ioWarnings, double aFileVersion, string aFileSection = null, bool bIgnoreNotFound = false, uopTrayAssembly aAssy = null, uopPart aPartParameter = null, List<string> aSkipList = null, Dictionary<string, string> EqualNames = null, uopProperties aProperties = null)
        {

            try
            {
                ioWarnings ??= new uopDocuments();
                aFileSection = string.IsNullOrWhiteSpace(aFileSection) ? INIPath : aFileSection.Trim();
                if (aFileProps == null) throw new Exception("The Passed File Property Array is Undefined");
                if (string.IsNullOrWhiteSpace(aFileSection)) throw new Exception("File Section is Undefined");

                if (aProject != null) SubPart(aProject);
                Reading = true;


                uopPropertyArray myprops = aFileProps.PropertiesStartingWith(aFileSection);


                if (myprops == null || myprops.Count <= 0)
                {
                    ioWarnings?.AddWarning(this, $"{PartName} Data Not Found", $"File '{ aFileProps.Name }' Does Not Contain {aFileSection} Info!");
                    return;
                }
                base.ReadProperties(aProject, myprops, ref ioWarnings, aFileVersion, aFileSection, bIgnoreNotFound, aSkipList: aSkipList, EqualNames: EqualNames);
                Reading = true;
                UpdatePartProperties();

                List<string> skippers = new List<string>() { "DistributorType" };
          
                Cases.CollectionObj.Clear();
                for(int i = 1; i <= 1000; i++)
                {
                    string fsec = $"{aFileSection}.CASE({i})";
                    if (!myprops.Contains(fsec)) break;

                    mdDistributorCase newcase = new mdDistributorCase();
                    uopPropertyArray caseprops = aFileProps.PropertiesStartingWith(fsec);
                    newcase.Index = i;
                    newcase.ReadProperties(aProject, caseprops, ref ioWarnings, aFileVersion, fsec, bIgnoreNotFound, aSkipList: skippers, EqualNames: EqualNames);
                    newcase.Reading = true;
                    newcase.UpdatePartProperties();
                    newcase.SubPart(this);
                    newcase.Reading = false;
                    Cases.CollectionObj.Add(newcase);
                }


            }
            catch (Exception e)
            {
                ioWarnings?.AddWarning(this, "Read Properties Error", e.Message);
            }
            finally
            {
               
                aProject?.ReadStatus("", 2);
                Reading = false;
            }
        }

        public uopProperty TroughHead(mdDistributorCase aCase, double OpRange = 100)
        {
            uopProperty _rVal = null;
            _rVal = new uopProperty("TroughHead_" + OpRange,"N/A");
            _rVal.SubPart(this);
            if (aCase == null){ return _rVal; }
            if (OpRange <= 0) { return _rVal; }
            if (Troughs <= 0) { return _rVal; }
            uopProperty p1 = TroughHoleArea();
            if (!p1.HasUnits) { return _rVal; }

            double aR = p1.UnitVariant();
            if (aR <= 0 ) { return _rVal; }

            double Vl = 0;
            double MRate1 = aCase.LiquidRate;
            double Dens1 = aCase.LiquidDensity;
            double MRate2 = aCase.AdditionalTroughLiquidRate;
            double Dens2 = aCase.LiquidDensityFromAbove;
            double aVal = 0;
            double grav = 32.174;
            double VRate1 = 0;
            double VRate2 = 0;

           
          

            if (MRate2 <= 0){MRate2 = 0;}
            if (MRate1 <= 0) { MRate1 = 0; }
            if (Dens1 <= 0) { Dens1 = 0; }
            if (Dens2 <= 0) { Dens2 = 0; }
            


            if (Dens1 > 0 || Dens2 > 0)
            {
                if (Dens1 > 0)
                {
                    VRate1 = MRate1 / (3600 * Dens1);
                }
                if (Dens2 > 0)
                {
                    VRate2 = MRate2 / (3600 * Dens2);
                }

                Vl = OpRange / 100 * (VRate1 + VRate2);
                aVal = Math.Pow(Vl / (aR * 0.707 * 0.98) , 2);
                aVal = aVal / 2 / grav * 12;
            }

            _rVal.Value = aVal;
            _rVal.UnitType = uppUnitTypes.SmallLength;
            _rVal.SubPart(this);
            return _rVal;
        }

        /// <summary>
        /// //^returns the the total trough hole are in ft^2
        /// </summary>
        /// <returns></returns>
        public uopProperty TroughHoleArea()
        {
            uopProperty _rVal = new uopProperty("TroughHoleArea","N/A");
            int cnt = PropValI("TroughHoleCount");
            double ID = PropUnitValue("TroughHoleSize", Enums.uppUnitFamilies.English);
            if (cnt <= 0 || ID <= 0) { return _rVal; }


            //hole area in ft^2
            double aR  = cnt * Math.PI * Math.Pow(0.5 * ID / 12, 2);
            if (aR < 0) { aR = 0; }
            _rVal.Value = aR;
            _rVal.UnitType = uppUnitTypes.BigArea;
            return _rVal;
        }

        public uopProperty TroughMargin(mdDistributorCase aCase, double OpRange = 100)
        {
            uopProperty _rVal = new uopProperty("TroughMargin_" + OpRange, "N/A");
                _rVal.SubPart(this);


            if (aCase == null)  return _rVal;

            if (OpRange <= 0) return _rVal;

         
            if (Troughs <= 0) return _rVal;

        
            dynamic aHt = null;

            aHt = TroughHeight;
            if (!mzUtils.IsNumeric(aHt)) return _rVal;

          
            if (Convert.ToDouble(aHt) <= 0) return _rVal;


            dynamic aHd = null;

            aHd = TroughHead(aCase, OpRange).Value;
            if (mzUtils.IsNumeric(aHd))
            {
                _rVal.UnitType = uppUnitTypes.SmallLength;
                _rVal.SetValue(Convert.ToDouble(aHt) - Convert.ToDouble(aHd), 0);
            }
            return _rVal;
        }

        public override void UpdatePartProperties() { DescriptiveName = $"Distributors({ SpanName })"; }

        public override void UpdatePartWeight() =>  base.Weight = 0;
    

        /// <summary>
        /// get the Mechanical Properties
        /// </summary>
        /// <param name="aCaseIndex"></param>
        /// <returns></returns>
        public uopProperties MechanicalProperties(int aCaseIndex)
        {
            uopProperties _rVal = new uopProperties();
            mdDistributorCase aCase = (mdDistributorCase)Cases.Item(aCaseIndex);
            _rVal.Add("NozzleArea", "N/A");
            _rVal.Add( "NozzleVelocity", "N/A");
            _rVal.Add("NozzleRhoVSqr", "N/A");
            _rVal.Add("PipeArea", "N/A");
            _rVal.Add("PipeVelocity", "N/A");
            _rVal.Add("PipeRhoVSqr", "N/A");
            _rVal.Add("PipeHoleArea", "N/A");
            _rVal.Add("PipeHoleVelocity", "N/A");
            _rVal.Add("PipeHoleRhoVSqr", "N/A");
            _rVal.Add("PipeHoleVelocityAtTurnDown", "N/A");
            _rVal.Add("TroughHead100", "N/A");
            _rVal.Add("TroughHeadMin", "N/A");
            _rVal.Add("TroughHeadMax", "N/A");
            _rVal.Add("TroughMargin100", "N/A");
            _rVal.Add("TroughMarginMin", "N/A");
            _rVal.Add("TroughMarginMax", "N/A");
            _rVal.Add("PipeHoleAreaRatio", "N/A");
            if (aCase != null)
            {
                uopPipe aPipe = NozzleObject;
                if (aPipe != null) _rVal.ReplaceProperty("NozzleArea", NozzleArea(aPipe));
                if (NozzleCount > 0 && aPipe != null)
                {
                    _rVal.ReplaceProperty( "NozzleVelocity", NozzleVelocity(aCase, aPipe));
                    _rVal.ReplaceProperty("NozzleRhoVSqr", NozzleRhoVsqr(aCase, aPipe));
                }
                aPipe = PipeObject;
                if (aPipe != null) _rVal.ReplaceProperty("PipeArea", PipeArea(aPipe));
                if (NozzleCount > 0 && aPipe != null)
                {
                    _rVal.ReplaceProperty("PipeVelocity", PipeVelocity(aCase, aPipe));
                    _rVal.ReplaceProperty("PipeRhoVSqr", PipeRhoVsqr(aCase, aPipe));
                }
                if (PropValI("PipeHoleCount") > 0 && PropValD("PipeHoleSize") > 0)
                {
                    _rVal.ReplaceProperty("PipeHoleArea", PipeHoleArea());
                    _rVal.ReplaceProperty("PipeHoleVelocityAtTurnDown", PipeHoleVelocity(aCase, true));
                    _rVal.ReplaceProperty("PipeHoleVelocity", PipeHoleVelocity(aCase));
                    _rVal.ReplaceProperty("PipeHoleRhoVSqr", PipeHoleRhoVSqr(aCase));
                    _rVal.ReplaceProperty("PipeHoleAreaRatio", PipeHoleAreaRatio(aPipe));
                }
                _rVal.ReplaceProperty("TroughHead100", TroughHead(aCase, 100));
                _rVal.ReplaceProperty("TroughHeadMin", TroughHead(aCase, aCase.MinimumOperatingRange));
                _rVal.ReplaceProperty("TroughHeadMax", TroughHead(aCase, aCase.MaximumOperatingRange));
                _rVal.ReplaceProperty("TroughMargin100", TroughMargin(aCase, 100));
                _rVal.ReplaceProperty("TroughMarginMin", TroughMargin(aCase, aCase.MinimumOperatingRange));
                _rVal.ReplaceProperty("TroughMarginMax", TroughMargin(aCase, aCase.MaximumOperatingRange));
            }
            _rVal.SubPart((uopPart)aCase, "MechanicalProperties");
            return _rVal;
        }

        public uopDocuments GenerateWarnings(uopProject aProject, string aCategory = null, uopDocuments aCollector = null, bool bJustOne = false)
        {

            uopDocuments _rVal = aCollector ?? new uopDocuments();
            if (bJustOne && _rVal.Count > 0) return _rVal;
            mdProject myProj;

            if (aProject == null) { myProj = GetMDProject(); } else { myProj = (aProject.ProjectFamily == uppProjectFamilies.uopFamMD) ? (mdProject)aProject : GetMDProject(); }
            if (myProj == null) return _rVal;


            string sname = DescriptiveName;
            myProj.ReadStatus($"Generating { sname } Warnings", 1);


            _rVal.NewMemberCategory = sname;
            _rVal.NewMemberSubCategory = string.Empty;
            if (myProj.ProjectType == uppProjectTypes.MDSpout)
            {
                foreach (uopPart item in Cases)
                {
                    mdDistributorCase mycase = (mdDistributorCase)item;
                    mycase.UpdatePartProperties();
                    if (mycase.DesignRate <= 0)
                    {
                        _rVal.AddWarning(this, "Invalid Distributor Load Case", $" Distributor '{Description} - {mycase.Description}' Design Rate Rate is Zero.", aOwnerName: $"Distributors({Index}).Cases({mycase.Index})", aCategory: aCategory);
                        if (bJustOne) break;
                    }
                    if (bJustOne && _rVal.Count > 0) return _rVal;

                    if (mycase.MinimumOperatingRange <= 0)
                    {
                        _rVal.AddWarning(this, "Invalid Distributor Load Case", $" Chimney Tray '{Description} - {mycase.Description}' Minimum Operating Range is Zero.", aOwnerName: $"Distributors({Index}).Cases({mycase.Index})", aCategory: aCategory);
                        if (bJustOne) break;
                    }
                    if (bJustOne && _rVal.Count > 0) return _rVal;
                    if (mycase.MaximumOperatingRange <= 0)
                    {
                        _rVal.AddWarning(this, "Invalid Distributor Load Case", $" Chimney Tray '{Description} - {mycase.Description}' Maximum Operating Range is Zero.", aOwnerName: $"Distributors({Index}).Cases({mycase.Index})", aCategory: aCategory);
                        if (bJustOne) break;
                    }
                    if (bJustOne && _rVal.Count > 0) return _rVal;
                }

            }
            return _rVal;
        }
        #endregion
    }
}
