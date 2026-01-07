
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.Projects.Parts
{
    /// <summary>
    /// a simple object that holds spout group constraints added to the group by a user
    ///the constraints of a spout group control how the spout area is assigned to the group and how
    ///the spout pattern is generated.
    /// </summary>
    public class mdConstraint : mdBoxSubPart
    {

        public override uppPartTypes BasePartType => uppPartTypes.Constraint;
       
        //@the event raised when one of the properties of the constraint changes
        public delegate void MDConstraintPropertyChange(uopProperty aProperty);
        public event MDConstraintPropertyChange eventMDConstraintPropertyChange;


        #region Constructors
        public mdConstraint() : base(uppPartTypes.Constraint)  => Initialize();

        public mdConstraint(mdConstraint aPartToCopy,  int? aPanelIndex, int? aDowncomerIndex, int? aBoxIndex) : base(uppPartTypes.Constraint) => Initialize(aPartToCopy, null, aPanelIndex, aDowncomerIndex,  aBoxIndex);

        public mdConstraint(mdConstraint aPartToCopy, mdDowncomerBox aBox = null, int? aPanelIndex =null) : base(uppPartTypes.Constraint) => Initialize(aPartToCopy, aBox, aPanelIndex);
        internal override void Initialize(mdBoxSubPart aPartToCopy, mdDowncomerBox aBox )
        {
            mdConstraint copy = null;
            if (aPartToCopy != null && aPartToCopy.GetType() == typeof(mdConstraint)) copy = (mdConstraint)aPartToCopy;
            Initialize(copy, aBox);
        }

        private bool _Init = false;
        private void Initialize(mdConstraint aPartToCopy = null, mdDowncomerBox aBox = null, int? aPanelIndex = null, int? aDowncomerIndex = null, int? aBoxIndex = null)
        {

            try
            {
                if (!_Init) 
                {
                    TPROPERTIES aProps = new TPROPERTIES();

                    aProps.Add("PatternType", uppSpoutPatterns.Undefined, aDecodeString: "0=~UNDEFINED~,1=D,2=C,3=B,4=A,5=*A*,6=S3,7=S2,8=S1,9=*S*");
                    aProps.Add("VerticalPitch", 0!, uppUnitTypes.SmallLength, bSetNullValue: true);
                    aProps.Add("HorizontalPitch", 0!, uppUnitTypes.SmallLength, bSetNullValue: true);
                    aProps.Add("SpoutCount", -1, bSetNullValue: true);
                    aProps.Add("SpoutDiameter", 0!, uppUnitTypes.SmallLength, bSetNullValue: true);
                    aProps.Add("SpoutLength", 0!, uppUnitTypes.SmallLength, bSetNullValue: true);
                    aProps.Add("Clearance", 0!, uppUnitTypes.SmallLength, bSetNullValue: true);
                    aProps.Add("Margin", 0!, uppUnitTypes.SmallLength, bSetNullValue: true);
                    aProps.Add("TreatAsIdeal", false, bSetNullValue: true);
                    //aProps.Add("TreatAsGroup", false, bSetNullValue:true);
                    aProps.Add("OverrideSpoutArea", 0!, uppUnitTypes.SmallArea, bSetNullValue: true);
                    aProps.Add("YOffset", 0!, uppUnitTypes.SmallLength, bSetNullValue: true);
                    aProps.Add("EndPlateClearance", 0!, uppUnitTypes.SmallLength, bSetNullValue: true);
                    aProps.Add("AreaGroupIndex", 0, bSetNullValue: true);
                    aProps.SetMinMax(2, 0);
                    aProps.SetMinMax(3, 0);
                    aProps.SetMinMax(7, 0.0625, 24);
                    aProps.SetMinMax(12, 0.0625);
                    aProps.SetMinMax(10, 0);

                    aProps.SetMinMax(7, -1);
                    aProps.SetMinMax(5, 0.125, 1.125);
                    aProps.LockTypes(true);
                    ActiveProps = aProps;
                    PropsLockTypes(true);
                    _Init = true;
                }

                if (aPartToCopy != null) 
                {
                    base.Copy(aPartToCopy);
                    aBox ??= aPartToCopy.DowncomerBox;
                }


                    SubPart(aBox);
                if(aPanelIndex.HasValue) {PanelIndex = aPanelIndex.Value;}
                if (aDowncomerIndex.HasValue) { DowncomerIndex = aDowncomerIndex.Value; }
                if(aBoxIndex.HasValue)  BoxIndex = aBoxIndex.Value;
            }
            catch (Exception e) { throw e; }

        }

        #endregion Constructors


        public override string ToString() { return $"CONSTRAINT({ Handle })"; }

      
        /// <summary>
        /// the minumum clearance for the left and right bounds of the group and the inside wall of the downcomer
        /// Default = 2 * dc thickness + 1/16.  Minumum allowed value 0.0625
        /// </summary>
        public double Clearance
        {
            get => PropValD("Clearance");

            set
            {
                value = Math.Round(value, 5);
                if (value <= 0.0625) { value = 0.0625; }
                Notify(PropValSet("Clearance", value));
            }
        }
        /// <summary>
        ///returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        public mdConstraint Clone() => new mdConstraint(this);

        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();

        public string ConstrainedPropertyNames => ActiveProps.GetByDefaultStatus(false).NameList();


        /// <summary>
        ///returns the objects properties in a collection
        ///signatures like "COLOR=RED"
        /// </summary>

        public bool SetCurrentProperties(uopProperties aProperties, bool bSuppressEvents = false)
        {
            if (aProperties == null) return false;
            bool _rVal = false;
            TPROPERTIES aProps = new TPROPERTIES(aProperties);
            TPROPERTY aProp;
            uopProperty bProp;
            if (!SuppressEvents && !bSuppressEvents)
            {
                for (int i = 1; i <= aProps.Count; i++)
                {
                    aProp = aProps.Item(i);
                    bProp = PropValSet(aProp.Name, aProp.Value);
                    if(bProp != null)
                    {
                        _rVal = true;
                        Notify(bProp);
                    }
                    
                }
            }
            else
            {
                string sval = string.Empty;
                TPROPERTIES prtProps = ActiveProps;
                prtProps.CopyValues(aProps, out sval, bCopyNonMembers: false);
                if (!string.IsNullOrWhiteSpace(sval)) _rVal = true;
                ActiveProps =prtProps;
            }
            return _rVal;
        }

        public override uopProperties CurrentProperties() { UpdatePartProperties(); return base.CurrentProperties(); }
        public override uopProperty CurrentProperty(string aPropertyName, bool bSupressNotFoundError = false)
        {
            UpdatePartProperties();
            return base.CurrentProperty(aPropertyName, bSupressNotFoundError);
        }

        private WeakReference<mdSpoutGroup> _SpoutGroupRef;
        public void SetSpoutGroup(mdSpoutGroup aSpoutGroup)
        {
            if (aSpoutGroup == null)
            {
                _SpoutGroupRef = null;
                return;
            }
            _SpoutGroupRef = new WeakReference<mdSpoutGroup>(aSpoutGroup);
        }


        /// <summary>
        /// the parent deck panel of this constraint
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        //converting it method as it is having parameters
        public mdDeckPanel DeckPanel(mdTrayAssembly aAssy) => base.GetMDPanel(aAssy);

        /// <summary>
        /// used to set the properties of the spout group constraint based on the values in the passed comma delimated string
        ///see mdConstraint.Descriptor
        /// </summary>
        /// <param name="sDescriptor">the descriptor string of a spout group constraint to extract  properties from</param>
        /// <param name="SetFlags">flag to reset the ideal and group flags properties</param>
        /// <param name="bSuppressEvents"></param>
        public void DefineByString(string sDescriptor, bool SetFlags = true, bool bSuppressEvents = false)
        {

            try
            {
                if (String.IsNullOrWhiteSpace(sDescriptor)) return;

                string oldDesc = string.Empty;

                sDescriptor = sDescriptor.Trim();
                if (sDescriptor.IndexOf(",") == 0) return;

                oldDesc = Descriptor;

                if (string.Compare(sDescriptor, oldDesc, StringComparison.OrdinalIgnoreCase) == 0) return;

                bool wuz = SuppressEvents;
                SuppressEvents = bSuppressEvents;

                List<string > vals = mzUtils.StringsFromDelimitedList(sDescriptor);
                int vcnt = vals.Count;

                if (vcnt >= 1) PatternType = (uppSpoutPatterns)mzUtils.VarToInteger(vals[0]);

                if (vcnt >= 2) VerticalPitch = mzUtils.VarToDouble(vals[1]);

                if (vcnt >= 3) HorizontalPitch = mzUtils.VarToDouble(vals[2]);

                if (vcnt >= 4) SpoutCount = mzUtils.VarToInteger(vals[3]);

                if (vcnt >= 5) SpoutDiameter = mzUtils.VarToDouble(vals[4]);

                if (vcnt >= 6) SpoutLength = mzUtils.VarToDouble(vals[5]);

                if (vcnt >= 7) Clearance = mzUtils.VarToDouble(vals[6]);

                if (vcnt >= 8) Margin = mzUtils.VarToDouble(vals[7]);

                if (SetFlags)
                {
                    if (vcnt >= 9) TreatAsIdeal = mzUtils.VarToBoolean(vals[8]);

                    if (vcnt >= 10) { if (mzUtils.VarToBoolean(vals[9])){ AreaGroupIndex = 1; } };

                    if (vcnt >= 11) OverrideSpoutArea = mzUtils.VarToDouble(vals[10]);

                }
                if (vcnt >= 12) YOffset = mzUtils.VarToDouble(vals[11]);

                SuppressEvents = wuz;
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// defines the non-default properties of the constraint based on the passed string
        /// strings like VerticalPitch = 1.2233, HorizontalPitch = 2
        /// </summary>
        /// <param name="NDString">non default property string to parse and extract propert values from</param>
        /// <param name="Delimitor"></param>
        public void DefineNonDefaults(string NDString, string Delimitor)
        {
            string Val = string.Empty;
            Val = NDString.Trim();
            if (Val ==  string.Empty)
            {
                return;
            }
            string pStr = string.Empty;
            string pname = string.Empty;
            string pval = string.Empty;
            dynamic vVal = null;
            int eq = 0;
            try
            {
                SuppressEvents = true;
                var vals = Val.Split(Delimitor.ToCharArray());
                for (int i = 0; i < vals.Count(); i++)
                {
                    pStr = vals[i].Trim();
                    if (pStr !=  string.Empty)
                    {
                        //if 2 nd parameter for Left and right in vbcode is constant then no need to change parameter
                        if (mzUtils.Left(pStr, 1) == "(")
                        {
                            pStr = mzUtils.Right(pStr, pStr.Length - 1).Trim();
                        }
                        if (mzUtils.Right(pStr, 1) == ")")
                        {
                            pStr = mzUtils.Left(pStr, pStr.Length - 1).Trim();
                        }
                        eq = pStr.IndexOf("=");
                        //changing left functions parameter as it is using IndexOf funtion output as parameter
                        pname = mzUtils.Left(pStr, eq).Trim();
                        pval = mzUtils.Right(pStr, pStr.Length - (eq + 1)).Trim();

                        switch (pname.ToUpper())
                        {
                            case "PATTERNTYPE":
                                vVal = (uppSpoutPatterns)mzUtils.VarToInteger(pval);
                                break;
                            case "TREATASIDEAL":
                                vVal = mzUtils.VarToBoolean(pval);
                                break;
                         
                            case "AREAGROUPINDEX":
                                vVal = mzUtils.VarToInteger(pval,true,0);
                                break;
                            case "SPOUTCOUNT":
                                vVal = mzUtils.VarToInteger(pval);
                                break;
                            default:
                                vVal = mzUtils.VarToDouble(pval);
                                break;
                        }

                        PropValSet(pname, vVal, bSuppressEvnts: true );

                    }
                }

            }
            catch (Exception e)
            {
                SuppressEvents = false;
                throw e;
            }
        }


        /// <summary>
        /// used to set the properties of the spout group constraint based on the values in the passed comma delimated string
        /// strings like "VerticalPitch=3.00,Clearance=0.5"
        /// </summary>
        /// <param name="sDescriptor">the descriptor string of a spout group constraint to extract  properties from</param>
        /// <param name="bSuppressEvents">flag to raise change events</param>
        /// <param name="aFileVersion"></param>
        public void DefineValues(string sDescriptor, bool bSuppressEvents, double aFileVersion)
        {
            if (string.IsNullOrWhiteSpace(sDescriptor)) return;
            sDescriptor = sDescriptor.Trim();
            if (sDescriptor.IndexOf("=", StringComparison.OrdinalIgnoreCase) == -1) return;

            bool wuz = SuppressEvents;

            uopProperty aProp;
            dynamic aVal;
            bool bClrc = false;
            int? areaGroup = null;

            var sigs = sDescriptor.Split(uopGlobals.Delim.ToCharArray());
         
            SuppressEvents = bSuppressEvents;

            for (int i = 0; i < sigs.Count(); i++)
            {
                string sig = sigs[i];
                string[] sVals = sig.Split('=');
                string pname = sVals[0].Trim().ToUpper();
                dynamic pval = sVals[1];
                aVal = null;
                    switch (pname.ToUpper())
                {
                    case "PATTERNTYPE":
                        if (!mzUtils.IsNumeric(pval))
                        {
                            aProp = CurrentProperty(pname);
                            string sval = sVals[1].ToUpper();
                            uppSpoutPatterns ptype = uppSpoutPatterns.Undefined;
                            switch (sval)
                            {
                                case "A":
                                    {
                                        ptype = uppSpoutPatterns.A;
                                        break;
                                    }
                                case "B":
                                    {
                                        ptype = uppSpoutPatterns.B;
                                        break;
                                    }
                                case "C":
                                    {
                                        ptype = uppSpoutPatterns.C;
                                        break;
                                    }
                                case "D":
                                    {
                                        ptype = uppSpoutPatterns.D;
                                        break;
                                    }
                                case "S1":
                                    {
                                        ptype = uppSpoutPatterns.S1;
                                        break;
                                    }
                                case "S2":
                                    {
                                        ptype = uppSpoutPatterns.S2;
                                        break;
                                    }
                                case "S3":
                                    {
                                        ptype = uppSpoutPatterns.S3;
                                        break;
                                    }
                                case "*S*":
                                    {
                                        ptype = uppSpoutPatterns.SStar;
                                        break;
                                    }

                                    
                            }
                            aVal = ptype;

                            //pval = aProp.GetCodedValue((pval is string) ? pval : pval.toString());
                            //aVal = (uppSpoutPatterns)mzUtils.VarToInteger(pval);
                        }
                        else
                        { aVal = (uppSpoutPatterns)(mzUtils.VarToInteger(pval) + 1); }
                        break;
                    case "SPOUTCOUNT":
                        aVal = mzUtils.VarToInteger(pval);
                        break;
                    case "TREATASIDEAL":
                        aVal = mzUtils.VarToBoolean(pval);

                        break;
                    case "TREATASGROUP":
                            
                        pname = "";
                        if (!areaGroup.HasValue && !sDescriptor.ToUpper().Contains("AREAGROUPINDEX"))
                        {
                            if (mzUtils.VarToBoolean(pval)) areaGroup = 1;

                        }
                        break;


                    case "AREAGROUPINDEX":

                        pname = "";
                        areaGroup = mzUtils.VarToInteger(pval, aDefault:0);
                        if (areaGroup.Value < 0) areaGroup = 0;
                       
                        break;
                    case "CLEARANCE":
                        bClrc = true;
                        aVal = mzUtils.VarToDouble(pval);

                        break;
                    
                    default:
                        aVal = mzUtils.VarToDouble(pval);
                        break;
                }

                if (!string.IsNullOrWhiteSpace(pname)  && aVal != null)
                {
                    aProp = CurrentProperty(pname, bSupressNotFoundError: true);
                    if (aProp != null) Notify(PropValSet(pname, aVal));

                }


            }
            if (!areaGroup.HasValue) areaGroup = 0;
            aProp = CurrentProperty("AreaGroupIndex", bSupressNotFoundError: true);
            if (aProp != null) Notify(PropValSet("AreaGroupIndex", areaGroup.Value));


            if (!bClrc && aFileVersion < 2.32)
            {
                Notify(PropValSet("CLEARANCE", 0.2));
            }

            SuppressEvents = wuz;

        }

        /// <summary>
        /// a string that completely defines property values of the lock object
        /// </summary>
        public string Descriptor => PropString(",");

      

        /// an override of the standard 0.25 clearance from the endplate
        /// </summary>
        public double EndPlateClearance { get => PropValD("EndPlateClearance"); set { if (value < 0) { value = 0; } Notify(PropValSet("EndPlateClearance", value)); } }

        /// <summary>
        /// a string that identifies the constraints downcomer and panel uopPatternUndefined
        /// like 1,2 etc.
        /// </summary>
        public string Handle => $"{DowncomerIndex},{ PanelIndex}" ;

        public override string Name { get => Handle; set { } }

        /// <summary>
        /// returns True if TreatAsIdeal or TreatAsGroup = True
        /// </summary>
        public bool HasAreaConstraints => TreatAsIdeal || TreatAsGroup;

        /// <summary>
        /// used by test if a change has been made to the curent project
        /// </summary>
        public bool HasChanged { get => Mark; set => Mark = value; }

        public bool HasConstraints(out string rPropNames) => ActiveProps.GetByDefaultStatus(false,0,0,"", null,out rPropNames ).Count > 0;
        

        /// <summary>
        /// the distance between spouts in the X direction
        /// </summary>
        public double HorizontalPitch { get => PropValD("HorizontalPitch"); set => Notify(PropValSet("HorizontalPitch", value)); }

        public override string INIPath => "COLUMN(" + ColumnIndex + ").RANGE(" + RangeIndex + ").TRAYASSEMBLY.CONSTRAINTS";

        public bool IsEqual(mdConstraint aConstraint) => aConstraint != null && TPROPERTIES.Compare(ActiveProps, aConstraint.ActiveProps);

        /// <summary>
        ///the minumum clearance for the top and bottom bounds of the groups bounding perimeter
        ///Default = 0
        /// </summary>
        public double Margin { get => PropValD("Margin"); set => Notify(PropValSet("Margin", value)); }

        /// <summary>
        /// the collection that this constraint is a member of
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        private colMDConstraints MyCollection(mdTrayAssembly aAssy = null) { aAssy = base.GetMDTrayAssembly(aAssy); return aAssy?.Constraints; }

        /// <summary>
        ///returns a properties collection with a single property
        ///containing the descriptor string of the constraint if any
        ///of the current property values are other than default
        /// </summary>
        public uopProperties NonDefaultProperties => new uopProperties(ActiveProps.GetByDefaultStatus(false, aEmptyProp: new TPROPERTY("Constraints", "None")));


        /// <summary>
        /// used by an object to respond to property changes of itself and of objects below it in the object model.
        ///also raises the changed property to the object above it it the object model.
        /// </summary>
        /// <param name="aProperty"></param>
        public void Notify(uopProperty aProperty)
        {
            if (aProperty == null || Reading || RangeGUID ==  string.Empty) return;

            
            uppPartTypes pType = aProperty.PartType;
            colMDConstraints aCNCol;
            mdSpoutGroup mySpoutGroup = SpoutGroup(null);

            string pname = aProperty.Name.ToUpper();
                if (pname == "PATTERNTYPE")
            {
                if (aProperty.Value == uppSpoutPatterns.Undefined)
                {
                    PropValSet("VerticalPitch", 0, bSuppressEvnts: true);
                    PropValSet("HorizontalPitch", 0, bSuppressEvnts: true);
                }
            }

            if (pname != "OVERRIDESPOUTAREA")
            {
                this.eventMDConstraintPropertyChange?.Invoke(aProperty);
              
                if (!SuppressEvents)
                {
                    aCNCol = MyCollection();
                    if (aCNCol != null)
                    {
                        aCNCol.NotifyMemberChange(this, aProperty);
                    }
                }
                if (mySpoutGroup != null) { mySpoutGroup.Invalid = true; }
            }
        }

        /// <summary>
        /// the ideal spout area to force the spout group to achieve
        /// </summary>
        public double OverrideSpoutArea { get => PropValD("OverrideSpoutArea"); set => Notify(PropValSet("OverrideSpoutArea", value)); }

        /// <summary>
        /// the name of the spout pattern indicated bt the pattern type
        /// </summary>
        public string PatternName => uopEnums.Description(PatternType);

        /// <summary>
        /// the pattern type
        /// </summary>
        public uppSpoutPatterns PatternType { get => (uppSpoutPatterns)PropValI("PatternType"); set => PropValSet("PatternType", value, bSuppressEvnts: true); }

        /// <summary>
        ///executed internally to set all constraint properties back to the default settings
        /// </summary>
        /// <param name="ResetFlags">flag to alter the two spout area flags</param>
        public void ResetValues(bool ResetFlags = true)
        {
            Reading = true;
            PropsReset(4, 11);
            if (ResetFlags) PropsReset(12, 14);
            PropsReset(16, 17);
            Reading = false;
        }

        /// <summary>
        ///returns the properties required to save the constraint object to file
        /// </summary>
        public override  uopPropertyArray SaveProperties(string aHeading = null)
        {
            UpdatePartProperties();
            aHeading = string.IsNullOrWhiteSpace(aHeading) ? INIPath : aHeading;
            uopProperties props = new uopProperties();
            TPROPERTIES tprops = ActiveProps.GetByDefaultStatus(false);
            for(int i = 1; i <= tprops.Count; i++)
            {
                TPROPERTY prop = tprops.Item(i);
                if (prop.IsEnum)
                {
                    if(string.Compare(prop.VariableTypeName, "uppSpoutPatterns", true) == 0)
                    {
                        uppSpoutPatterns ptype = (uppSpoutPatterns)prop.Value;
                        props.Add(new uopProperty(prop.Name, ptype.GetDescription()));
                    }
                    else { props.Add(new uopProperty(prop)); }
                    
                }
                else
                {
                    props.Add(new uopProperty(prop));
                }
            } 
//            props = new uopProperties(ActiveProps.GetByDefaultStatus(false), aHeading);
            return new uopPropertyArray( props , aName: aHeading, aHeading :aHeading);
        }

        public bool SetProperty(string aPropertyName, dynamic aPropertyValue)
        {
            if (string.IsNullOrWhiteSpace(aPropertyName) || aPropertyValue == null) return false;
            string pname = aPropertyName.ToUpper().Trim();

            uopProperty aProp = PropValSet(aPropertyName, aPropertyValue);
            if (aProp == null) return false;
            Notify(aProp);

            return true;
        }
        public override uopProperty PropValSet(string aName, object aPropVal, int aOccur = 0, bool? bSuppressEvnts = null, bool? bHiddenVal = null)
        {
            uopProperty _rVal = base.PropValSet(aName, aPropVal,aOccur,bSuppressEvnts,bHiddenVal);

            if(_rVal == null ) return null;
            bool noevent = bSuppressEvnts.HasValue ? bSuppressEvnts.Value : SuppressEvents;
            if (!noevent) this.eventMDConstraintPropertyChange?.Invoke(_rVal);
            return _rVal;
        }
        public uopProperties SetPropertyLimits(mdTrayAssembly aAssy, mdSpoutGroup aSpoutGroup)
        {
            uopProperties _rVal = null;
            try
            {
               
                if (aSpoutGroup == null) aSpoutGroup = SpoutGroup(aAssy);
                if ( aSpoutGroup == null) return null;

                _rVal = mdUtils.SetConstraintPropertyLimits(ActiveProperties,  aSpoutGroup, aSpoutGroup.Grid);
                ActiveProps = new TPROPERTIES(_rVal);

            }
            catch
            { throw; }
            return _rVal;
        }

        //^used to lock the requested property and resetit to its default
        public uopProperty SetPropertyLock(ref string aPropertyName, bool aLockValue, bool bResetIt = false) => PropSetAttributes(aPropertyName, bProtected: aLockValue, bReset: bResetIt);

        /// <summary>
        /// the spout area group index to apply durring distribution calculatations.
        /// </summary>
        public int AreaGroupIndex { get => PropValI((dynamic)"AreaGroupIndex"); set => Notify(PropValSet("AreaGroupIndex", value)); }

        

        /// <summary>
        /// a fixed number of spouts to force upon the spout group
        /// </summary>
        public int SpoutCount { get => PropValI((dynamic)"SpoutCount"); set => Notify(PropValSet("SpoutCount", value)); }

        /// <summary>
        ///  the diameter of the spouts
        /// </summary>
        public double SpoutDiameter
        {
            get
            {
                double _rVal = PropValD("SpoutDiameter");
                if (_rVal < 0.125)
                {
                    PropValSet("SpoutDiameter", 0, bSuppressEvnts: true);
                    _rVal = 0;
                }

                return _rVal;
            }
            set
            {

                if (!Reading && RangeGUID !=  string.Empty)
                {
                    mdDowncomer DComer = Downcomer(null);
                    if (DComer != null && Math.Abs(value) != 0)
                    {
                        if (Math.Abs(value) == DComer.SpoutDiameter)
                        {
                            value = 0;
                        }
                    }

                }
                Notify(PropValSet("SpoutDiameter", Math.Abs(value)));

            }
        }

        /// <summary>
        /// the parent spout group of the constraint object
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public mdSpoutGroup SpoutGroup(mdTrayAssembly aAssy)
        {
            mdSpoutGroup _rVal = null;
            if (_SpoutGroupRef != null)
            {
                _SpoutGroupRef.TryGetTarget(out _rVal);
                if (_rVal == null) _SpoutGroupRef = null;
            }
            if (_rVal == null)
            { 
                aAssy = base.GetMDTrayAssembly(aAssy);
                _rVal = aAssy?.GetSpoutGroup(Handle);
                SetSpoutGroup(_rVal);
            }
            return _rVal;
        }


        /// <summary>
        /// the length of the spouts
        /// </summary>
        public double SpoutLength { get => PropValD("SpoutLength");  set => Notify(PropValSet("SpoutLength", Math.Abs(value))); } 


        /// <summary>
        ///flag indicating that the theoretical spout area of the parent spout group should be averaged with other members of its group during distribution calculations
        /// </summary>
        public bool TreatAsGroup { get => AreaGroupIndex > 0;  }

        /// <summary>
        ///flag indicating that the current actual spout area of the parent spout group should be used for its theoretical value during distribution calculations
        /// </summary>
        public bool TreatAsIdeal { get => PropValB("TreatAsIdeal"); set => Notify(PropValSet("TreatAsIdeal", value)); }

        public override void UpdatePartProperties()
        {
            TPROPERTIES myProps = ActiveProps;
            myProps.SetHiddenByDefault(true, true, 0, 0, true);
            ActiveProps =myProps;
        }

        /// <summary>
        /// the distance between spouts in the Y direction
        /// </summary>
        public double VerticalPitch { get => PropValD("VerticalPitch"); set => Notify(PropValSet("VerticalPitch", Math.Abs(value))); }

        public override void UpdatePartWeight() { base.Weight = 0; }

        /// <summary>
        /// a Y offset applied to the pattern
        ///Default = 0
        /// </summary>
        public double YOffset { get => PropValD("YOffset"); set => Notify(PropValSet("YOffset", value)); }

     
        public override bool IsEqual(uopPart aPart)
        {
            if (aPart == null) { return false; }
            if (aPart.PartType != PartType) { return false; }
            return IsEqual((mdConstraint)aPart);
        }

      
    }
}
