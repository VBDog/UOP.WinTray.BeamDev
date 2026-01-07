using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using  media = System.Windows.Media;

namespace UOP.WinTray.Projects
{
    /// <summary>
    /// used to represent a property in any components Properties collection
    /// </summary>
    public class uopProperty :  ICloneable, INotifyPropertyChanged

    {
        #region Variables
        private TPROPERTY _Struc = new TPROPERTY();

        #endregion Variables

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Constructors

        public uopProperty() {
            _Struc = TPROPERTY.Null;
            _ForeColor = null;


        }
        internal uopProperty(TPROPERTY aStructure) { _Struc.Copy(aStructure); if(string.IsNullOrWhiteSpace(_Struc.Color)) _Struc.Color = "Black"; _ForeColor = null; }
        public uopProperty(uopProperty aProperty) 
        { 
            _Struc= new TPROPERTY(aProperty); 
            if (string.IsNullOrWhiteSpace(_Struc.Color)) _Struc.Color = "Black";
            IsGlobal = false;
            _ForeColor = null;

            if (aProperty == null) return;

            IsGlobal = aProperty.IsGlobal; 
            _ForeColor = aProperty._ForeColor;
           
       
        }

        public uopProperty(string aName = "", dynamic aValue = null, uppUnitTypes aUnits = uppUnitTypes.Undefined, uppPartTypes aPartType = uppPartTypes.Undefined, dynamic aLastValue = null, dynamic aDefaultValue = null)
        {
          
            _Struc = new TPROPERTY(aName, aValue, aUnits, aPartType, aLastValue, aDefaultValue);
            _ForeColor = null;
        }

        #endregion Constructors

        #region Properties

        public bool IsGlobal { get; set; }

        private uppUnitFamilies _DisplayUnits = uppUnitFamilies.English;
        public uppUnitFamilies DisplayUnits
        {
            get => _DisplayUnits;
            set
            {
                _DisplayUnits = value;

                OnPropertyChanged("DisplayUnits");
               RaiseDisplayPropertyChanges(); 
            }
        }
  
        public bool Optional { get => _Struc.Optional; set => _Struc.Optional = value; }


        public string DisplayName { get => _Struc.DisplayName; set { _Struc.DisplayName = value; OnPropertyChanged("DisplayName"); } }

        public dynamic DisplayValue
        {
            get => _Struc.HasUnits ? _Struc.UnitValue(DisplayUnits) : Value;
            set
            {

                if (_Struc.Units.IsDefined)
                {

                    if (!mzUtils.IsNumeric(value)) return;
                    SetValue((double)_Struc.Units.ConvertValue(value, DisplayUnits, uppUnitFamilies.English));
                }
                else
                {
                    SetValue(value);
                }
            }
        }
        public int DisplayUnitPrecision { get => HasUnits ? Units.Precision(DisplayUnits) : 4; set { _Struc.Units.SetUnitPrecision(DisplayUnits, value); RaiseDisplayPropertyChanges(); } }
        public double DisplayUnitValue
        {
            get => _Struc.HasUnits ? _Struc.UnitValue(DisplayUnits) : mzUtils.VarToDouble(Value);
            set
            {

                if (_Struc.Units.IsDefined)
                {

                    SetValue(_Struc.Units.ConvertValue(value, DisplayUnits, uppUnitFamilies.English));

                }
                else
                { SetValue(value); }
            }
        }

        public string DisplayValueString
        {
            get => _Struc.UnitValueString(DisplayUnits,bYesNoForBool: true);
            set
            {

                if (_Struc.Units.IsDefined)
                {

                    if (!mzUtils.IsNumeric(value)) return;
                    SetValue((double)_Struc.Units.ConvertValue(value, DisplayUnits, uppUnitFamilies.English));
                }
                else
                {
                    SetValue(value);
                }
                
            }
        }

        public string DisplayValueStringFormatted
        {
            get 
            { 
                string _rVal = _Struc.UnitValueString(DisplayUnits, bIncludeThousandsSeps: true, bIncludeUnitString: true,bYesNoForBool:true );
                return _rVal;
            }

        }


        public string DisplayValueStringFormattedNoUnitsLabel
        {
            get
            {
                string _rVal = _Struc.UnitValueString(DisplayUnits, bIncludeThousandsSeps: true, bIncludeUnitString: false, bYesNoForBool: true);
                return _rVal;
            }

        }


        public string DisplayUnitLabel => _Struc.UnitString(DisplayUnits);

        public string DisplaySignature => _Struc.UnitSignature(DisplayUnits);

        public string DisplayColor { get => _Struc.Color; set { _Struc.Color = value; OnPropertyChanged("DisplayColor"); }  }

      
        public double ValueD => _Struc.ValueD;

        public int ValueI => _Struc.ValueI;

        public bool ValueB => _Struc.ValueB;

        public string ValueS => _Struc.ValueS;

        public bool ValueChanged { get => _Struc.ValueChanged; internal set => _Struc.ValueChanged = value; }

        /// <summary>
        /// /
        /// </summary>
        public string VariableTypeName
        {
            get => _Struc.VariableTypeName;
            internal set => _Struc.VariableTypeName = value;
        }

          /// <summary>
        /// provided for user to assign any sort of value to the property
        /// </summary>
        public string Category { get => _Struc.Category; set => _Struc.Category = value;  }

        /// <summary>
        /// 
        /// </summary>
        public string Choices { get => _Struc.Choices; set => _Struc.Choices = value; }

        public bool SelectOnly { get => _Struc.SelectOnly; set => _Struc.SelectOnly = value; }

        /// <summary>
        /// a string used to convert a numeric value to a string in the properties signature
        /// </summary>
        public string DecodeString { get => _Struc.DecodeString; set => _Struc.DecodeString = value; }

        public string DecodedLastValue => uopProperty.DecodeValue(this, LastValue);

        public string DecodedValue => uopProperty.DecodeValue(this);

        /// <summary>
        /// the default value of the property
        /// this value is set to the first value of the property by default
        /// </summary>

        public dynamic DefaultValue { get => _Struc.DefaultValue; set => _Struc.DefaultValue = value; }

        public string ForegroundColor { get => DisplayColor; set => DisplayColor = value; }

        private System.Drawing.Color? _ForeColor;
        public System.Drawing.Color? ForeColor { get => _ForeColor; set => _ForeColor = value; }

        public string BrushColorString
        {
            get
            {
                if (ForeColor.HasValue)
                {
                    var brush = new media::SolidColorBrush(media::Color.FromArgb(255, (byte)ForeColor.Value.R, (byte)ForeColor.Value.G, (byte)ForeColor.Value.B));
                  
                    return brush.ToString();
                  
                }
                else
                {
                    return  !string.IsNullOrWhiteSpace(DisplayColor) ?  DisplayColor : "Black" ;
                }
            }
        }
        /// <summary>
        /// the format string applied to the signature of numeric properties
        /// </summary>
        public string FormatString { get => _Struc.FormatString; set => _Struc.FormatString = value; }


        /// <summary>
        /// a string that shows the property value with the current format string applied
        /// </summary>

        public string FormatedString { get => _Struc.FormatedString; }

        public bool HasNullValue => _Struc.HasNullValue;

        public bool HasDefaultValue => _Struc.HasDefaultValue;

        public bool HasValue => _Struc.HasValue;

        /// <summary>
        /// the heading string written to or looked for in a file when the properties are written to a file
        /// ~properties inherit the heading of the properties collection they come from
        /// </summary>
        public string Heading { get => _Struc.Heading; set => _Struc.Heading = value; }


        /// <summary>
        /// flag indicating that this is a hidden property
        /// </summary>
        public bool Hidden { get => _Struc.Hidden; set => _Struc.Hidden = value; }

        /// <summary>
        /// the index of the property when it is return from a collection of properties
        /// </summary>

        public int Index { get => _Struc.Index; internal set => _Struc.Index = value; }


        /// <summary>
        /// ^returns True if the current value is equal to the currently set default value
        /// </summary>
        public bool IsDefault => _Struc.IsDefault;

        public bool IsEnum => _Struc.IsEnum;

        /// <summary>
        /// returns True if the current property value contains a single or a double data type
        /// </summary>
        public bool IsFloatingPoint
        {
            get
            {
               
                bool isDouble = double.TryParse(Convert.ToString(_Struc.Value), out double doubleValue);
                    bool isSingle = Single.TryParse(Convert.ToString(_Struc.Value), out Single singleValue);

                return isDouble || isSingle;
            }
        }


        public bool IsString => _Struc.IsString;

        public bool HasUnits => _Struc.HasUnits;

        public bool IsNullValue { get => _Struc.IsNullValue; }


        public bool IsShared { get => _Struc.IsShared; set => _Struc.IsShared = value; }

        private bool? _Numeric = null;
    
        public bool Numeric
        {
            get
            {
                if (HasUnits) return true;
                if (!HasValue) return false;
                return _Numeric ?? mzUtils.IsNumeric(ValueS);
            }
            set => _Numeric = value;
                            
        }

        public virtual string CellAddress
        {
            get => _Struc.CellAddress;
            set => _Struc.CellAddress = value;
        }

        /// <summary>
        /// the value of the property before it was changed last
        /// </summary>
        public dynamic LastValue { get => _Struc.LastValue; set => _Struc.LastValue = value; }

        public uppDocumentTypes DocumentType { get => _Struc.DocumentType; set => _Struc.DocumentType = value; }

        public virtual bool Locked { get => _Struc.Locked; set => _Struc.Locked = value; }

        /// <summary>
        /// a value that can be assigned to a property that is the desired maximum for the property
        /// doesn't affect the value of the property but is provided for use by clients
        /// </summary>
        public dynamic MaxValue { get => _Struc.MaxValue; set => _Struc.MaxValue = value; }

        /// <summary>
        /// a value that can be assigned to a property that is the desired minimum for the property
        /// doesn't affect the value of the property but is provided for use by clients.
        /// </summary>
        public dynamic MinValue { get => _Struc.MinValue; set => _Struc.MinValue = value; }

        /// <summary>
        /// a value that can be assigned to a property that is the desired increment step for the property value
        /// doesn't affect the value of the property but is provided for use by clients.
        /// </summary>
        public double Increment { get => _Struc.Increment == null ? 0.0 : (double)_Struc.Increment; set => _Struc.Increment = value; }

        /// <summary>
        /// the name of the property
        /// </summary>
        public virtual string Name { get => _Struc.Name; set { if (!string.IsNullOrWhiteSpace(value)) { _Struc.Name = value.Trim(); OnPropertyChanged("DisplayName"); } } } 

        /// <summary>
        /// the value which the property is considered to be null
        /// </summary>
        public dynamic NullValue { get => _Struc.NullValue; set => _Struc.NullValue = value; }

        /// <summary>
        /// the index of the part whose source is this property
        /// </summary>
        public int PartIndex { get => _Struc.PartIndex; set => _Struc.PartIndex = value; }

        public string PartPath { get => _Struc.PartPath; internal set => _Struc.PartPath = value; }

        public string PartName { get => _Struc.PartName; internal set => _Struc.PartName = value; }

        public uppPartTypes PartType { get => _Struc.PartType; set => _Struc.PartType = value; }

        /// <summary>
        /// he handle of the project that owns this object
        /// </summary>
        public string ProjectHandle { get => _Struc.ProjectHandle; set => _Struc.ProjectHandle = value; }

        /// <summary>
        /// True if the property is marked as protected
        /// </summary>
        public bool Protected { get => _Struc.Protected; set => _Struc.Protected = value; }

        /// <summary>
        /// the guid of the tray range that owns this object
        /// </summary>
        public string RangeGUID { get => _Struc.RangeGUID; set => _Struc.RangeGUID = value; }

        /// <summary>
        /// the row associated to the property
        /// </summary>
        public int Row { get => _Struc.Row; set => _Struc.Row = value; }

        /// <summary>
        /// the column associated to the property
        /// </summary>
        public int Col { get => _Struc.Col; set => _Struc.Col = value; }

        internal TPROPERTY Structure { get => _Struc; set => _Struc = value; }

        public bool TypeLocked { get => _Struc.TypeLocked; internal set => _Struc.TypeLocked = value; }

        /// <summary>
        /// the valueOrig of the property
        /// </summary>
        public dynamic Value { get => _Struc.Value; set => SetValue(value); }
        /// <summary>
        /// the type of units associated to this property
        /// </summary>
        public uppUnitTypes UnitType { get => _Struc.UnitType; set => _Struc.UnitType = value; }


        public uopUnit Units => new uopUnit(_Struc.Units);

        /// <summary>
        /// the current system of units
        /// </summary>
        public uppUnitFamilies UnitSystem { get => _Struc.Units.UnitSystem; set => _Struc.Units.UnitSystem = value; }


        #endregion Properties

        #region Methods

        public string GetDisplayValueString(bool bZeroAsNullString = false, bool bAbsVal = false, dynamic aZeroValue = null)
        {
            return _Struc.UnitValueString(DisplayUnits, bZerosAsNullString: bZeroAsNullString, bAbsVal: bAbsVal, aZeroValue: aZeroValue);
        }

        /// <summary>
        /// a html string that shows the name, units and value of the property
        /// ~like 'Length: 25 m'
        /// </summary>
        public string GetCaptionUnitSignature([Optional] bool bBoldLeftSide, uppUnitFamilies aUnitFamily = Enums.uppUnitFamilies.Default)
      => _Struc.CaptionUnitSignature(aUnitFamily, bBoldLeftSide);

        public override string ToString() => _Struc.ToString();


        public bool IsEqual(uopProperty aProp, int aPrecis = 5) => (aProp != null) && TPROPERTY.Compare(_Struc, aProp.Structure, aPrecis);


        /// <summary>
        /// returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        public uopProperty Clone() =>  new uopProperty(this) ;

        object ICloneable.Clone() => (object)this.Clone();

        /// <summary>
        /// Convert Units.
        /// </summary>
        /// <param name="aFromUnits"></param>
        /// <param name="aToUnits"></param>
        /// <param name="aValue"></param>
        /// <returns></returns>
        public double ConvertUnits(uppUnitFamilies aFromUnits, uppUnitFamilies aToUnits, dynamic aValue)
        => _Struc.ConvertUnits(aFromUnits, aToUnits, aValue);

        /// <summary>
        /// the property string used to set this properties name and value
        /// used to set the name and value of the property with a signature string
        /// strings like "Color=Red". defining properties this way will cause the property
        /// to be a string only.
        /// </summary>
        /// <param name="aPropSignature"></param>
        public void DefineByString(string aPropSignature)
        {

          
            if (!mzUtils.SplitString(aPropSignature, "=", out string sName, out string sVal)) return;
            if (!string.IsNullOrWhiteSpace(sName)) Name = sName;
            SetValue(sVal);
        }

        public void Copy(uopProperty aProperty, bool bCloneIndex = true, bool bCloneRowCol = true)
        {
            if (aProperty == null) return;
            _Struc.Copy(aProperty.Structure, bCloneIndex, bCloneRowCol);
            IsGlobal = aProperty.IsGlobal;
            _ForeColor = aProperty._ForeColor;
        }
        public virtual string GetCodedValue(string aString) => TPROPERTY.GetCodedValue(_Struc, aString);

        public bool IsNamed(string aNameList) => _Struc.IsNamed(aNameList);

        public void LockType() => _Struc.LockType();

        /// <summary>
        /// True if the property cannot be changed from it's current value
        /// </summary>


        public void SubPart(uopPart aPart) { if (aPart == null) return;  _Struc.SubPart(aPart); IsGlobal = aPart.IsGlobal;  }

        public void SubPart(uopProperties aProps)
        {
            if (aProps == null) return;

            ProjectHandle = aProps.ProjectHandle;
            RangeGUID = aProps.RangeGUID;
            PartType = aProps.PartType;
            PartPath = aProps.PartPath;
            PartIndex = aProps.PartIndex;
            PartName = aProps.PartName;

        }
        /// <summary>
        /// a unit caption string assigned to the property
        /// </summary>
        public string UnitCaption(uppUnitFamilies aUnits = uppUnitFamilies.Default) => _Struc.UnitCaption(aUnits);


        public string UnitString(uppUnitFamilies aUnitFamily = uppUnitFamilies.Default) => _Struc.UnitString(aUnitFamily);

        /// <summary>
        /// a string that shows the name, units and value of the property
        /// </summary>
        /// <param name="aUnitFamily"></param>
        /// <param name="bShowNulls"></param>
        /// <returns></returns>
        public string UnitSignature(uppUnitFamilies aUnitFamily = Enums.uppUnitFamilies.Default,
            bool bShowNulls = false) =>_Struc.UnitSignature( aUnitFamily, bShowNulls);


        public uopProperty UnitStringClone(uppUnitFamilies aUnits, bool bIncludeUnitString, string aNewName = null, string aCategory = null, bool bYesNoForBool = true)
        {
            uopProperty _rVal = new uopProperty(string.IsNullOrWhiteSpace(aNewName) ? Name : aNewName, UnitValueString(aUnits, bIncludeUnitString: bIncludeUnitString, bYesNoForBool: bYesNoForBool));
            if (!string.IsNullOrWhiteSpace(aNewName)) { _rVal.Category = aCategory; }
            return _rVal;

        }


        /// <summary>
        /// returns the property converted to the requested units
        /// </summary>
        /// <param name="aValue"></param>
        /// <param name="aUnitFamily"></param>
        /// <param name="bZerosAsNullString"></param>
        /// <param name="bDefaultAsNullString"></param>
        /// <param name="aOverridePrecision"></param>
        /// <param name="bIncludeThousandsSeps"></param>
        /// <param name="bIncludeUnitString"></param>
        /// <param name="aFormatString"></param>
        /// <returns></returns>
        public string UnitValue(uppUnitFamilies aUnitFamily = Enums.uppUnitFamilies.Default,
            bool bZerosAsNullString = false, bool bDefaultAsNullString = false,
            int aOverridePrecision = -1, bool bIncludeThousandsSeps = true,
            bool bIncludeUnitString = false, string aFormatString = "",
            dynamic aValue = null,
            bool bYesNoForBool =false)
        => _Struc.UnitValueString( aUnitFamily, bZerosAsNullString, bDefaultAsNullString, aOverridePrecision, bIncludeThousandsSeps, bIncludeUnitString, aFormatString, aValue, bYesNoForBool, bSuppressTrailingZeros:false);

        public string UnitValueString(uppUnitFamilies aUnitFamily = Enums.uppUnitFamilies.Default,
            bool bZerosAsNullString = false, bool bDefaultAsNullString = false,
            int aOverridePrecision = -1, bool bIncludeThousandsSeps = true,
            bool bIncludeUnitString = false, string aFormatString = "",
            dynamic aValue = null,
            bool bYesNoForBool = false)
        => _Struc.UnitValueString(aUnitFamily, bZerosAsNullString, bDefaultAsNullString, aOverridePrecision, bIncludeThousandsSeps, bIncludeUnitString, aFormatString, aValue, bYesNoForBool);


        /// <summary>
        /// returns the property converted to the requested units
        /// </summary>
        /// <param name="aValue"></param>
        /// <param name="aToUnits"></param>
        /// <param name="aFromUnits"></param>
        /// <returns></returns>
        public dynamic UnitVariant(uppUnitFamilies aToUnits = Enums.uppUnitFamilies.Default, dynamic aValue = null,
               uppUnitFamilies aFromUnits = uppUnitFamilies.Default, int aPrecis = -1)
        => _Struc.UnitVariant(aToUnits, aValue, aFromUnits, aPrecis);


        public void RaiseDisplayPropertyChanges()
        {
            try
            {
                OnPropertyChanged("DisplayValue");
                OnPropertyChanged("DisplayName");
                OnPropertyChanged("DisplayUnitLabel");
                OnPropertyChanged("DisplaySignature");
                OnPropertyChanged("DisplayColor");
                OnPropertyChanged("DisplayUnitValue");
                OnPropertyChanged("DisplayValueString");
                OnPropertyChanged("DisplayValueStringFormatted");
                OnPropertyChanged("DisplayValueStringFormattedNoUnitsLabel");
                OnPropertyChanged("DisplayUnitPrecision");
            }
            catch { }

        }

        public bool ReadFromINIFile( string aFileSpec,  string aFileSection) 
        {
            bool _rVal = false;
            TPROPERTY newStruc = new TPROPERTY(_Struc);
            try
            {
                if (string.IsNullOrWhiteSpace(aFileSpec)) return false;
                if (string.IsNullOrWhiteSpace(aFileSection)) return false;
                if (!System.IO.File.Exists(aFileSpec)) return false;
           
                newStruc = TPROPERTY.ReadFromINIFile(newStruc, aFileSpec, aFileSection);
                _rVal = SetValue(newStruc.Value);
            }
            catch (Exception ex)
            {
                throw ex;
               
            }
            return _rVal;
        }


        /// <summary>
        /// set the current property value back to its default value
        /// </summary>
        public bool ResetValue() { 
            bool _rVal = _Struc.ResetValue();
            if (_rVal) RaiseDisplayPropertyChanges();
            
            return _rVal;
        
        }

        public bool SetNameAndVal(string aName, dynamic aValue)
        {
            if (!string.IsNullOrWhiteSpace( aName)) Name = aName.Trim();
            return SetValue( aValue);
        }

        private void SetType(string aTypeName, dynamic newval)
        { Value = TPROPERTY.SetType(aTypeName, newval); }

        /// <summary>
        /// a unit string used ass the unit lable of the property when no unit object is defined
        /// </summary>
        /// <param name="aUString"></param>
        public void SetUnitString(string aUString) =>  _Struc.SetUnitCaption ( aUString);


        /// <summary>
        /// used to set value of the property and returns true if the value has changed
        /// returns False if SuppressEvents is set to False even if the value has changed.
        /// returns False if it is the initial value of the property
        /// </summary>
        /// <param name="newval"></param>
        /// <param name="aDefaultValue"></param>
        /// <param name="aNullValue"></param>
        /// <returns></returns>
        public virtual bool SetValue(dynamic newval, dynamic aDefaultValue = null, dynamic aNullValue = null)
        {
            TPROPERTY struc = _Struc;
            bool _rVal = struc.SetValue(newval, out bool FRTS, aDefaultValue, aNullValue);
            _Struc = struc;
            if (_rVal)
            {
                OnPropertyChanged("Value");
                RaiseDisplayPropertyChanges();
            }

            return _rVal;
        
        }


        public double UnitVal(uppUnitFamilies aToUnits = uppUnitFamilies.Default, dynamic aValue = null, uppUnitFamilies aFromUnits = uppUnitFamilies.Default) => _Struc.UnitValue(aToUnits, aValue, aFromUnits);
        public void SetUnitCaption(string aCaption) => _Struc.SetUnitCaption(aCaption);


        /// <summary>
        /// #1flag to used the decoded value of the property value if there is a decode string
        //  a string that shows the name and value of the property
        //  like "Size=0.3213
        /// </summary>
        /// <param name="bShowDecodedValue"></param>
        /// <param name="bShowNulls"></param>
        /// <returns></returns>
        public string Signature(bool bShowDecodedValue = true, bool bShowNulls = false) => _Struc.Signature(bShowDecodedValue, bShowNulls);


        /// <summary>
        ///#1test value
        // #2the new value
        // #3flag to test by a non-casesensitive string comparison
        // #4a numerical precisions to apply to
        // #5value to apply if the current value does NOT match the test value
        // sets the value of the property to the new value is it's current value is equal to the passed value
        //~returns true if the value changes
        /// </summary>
        /// <param name="aValue"></param>
        /// <param name="aNewValue"></param>
        /// <param name="bStringCompare"></param>
        /// <param name="aPrecis"></param>
        /// <param name="aElseValue"></param>
        /// <returns></returns>
        public bool ThisThenThis( dynamic aValue, dynamic aNewValue,
            bool bStringCompare,
            int aPrecis = -1, dynamic aElseValue = null)
        {
            bool _rVal = _Struc.ThisThenThis(aValue, aNewValue, bStringCompare, aPrecis, aElseValue);
            if (_rVal) RaiseDisplayPropertyChanges();
            return _rVal;
        }

        /// <summary>
        /// ^the format string applied to the signature of numeric properties
        /// </summary>
        /// <param name="aFamily"></param>
        /// <param name="aOverridePrecision"></param>
        /// <param name="bIncludeThousandsSeps"></param>
        /// <returns></returns>
        public string UnitFormatString(uppUnitFamilies aFamily,
            int aOverridePrecision = -1, bool bIncludeThousandsSeps = false)
        {
            return _Struc.UnitFormatString(aFamily, aOverridePrecision, bIncludeThousandsSeps);
        }



        #endregion Methods


        #region Shared Methods

        public static bool Compare(uopProperty aProp, uopProperty bProp, int aPrecis = 5, bool bCompareNames = false)
        {
            if(aProp == null || bProp == null) return false;
            aPrecis = mzUtils.LimitedValue(aPrecis, 1, 10);


            if (bCompareNames)
            {
                if (string.Compare(aProp.Name, bProp.Name, ignoreCase: true) != 0) return false;
            }

            bool numbr = mzUtils.IsNumeric(aProp.Value);

            if (numbr != mzUtils.IsNumeric(bProp.Value)) return false;


            return !numbr
                ? (string.Compare(aProp.Value.ToString(), bProp.Value.ToString(), ignoreCase: true) == 0)
                : (mzUtils.VarToDouble(aProp.Value, aPrecis: aPrecis) == mzUtils.VarToDouble(bProp.Value, aPrecis: aPrecis));


        }
        public static dynamic DecodeValue(uopProperty aProp, dynamic aValue = null)
        {
            if (aProp == null) return string.Empty;
            object val = aValue ?? aProp.Value;
            if (val == null) return string.Empty;

            string _rVal = val.GetType().IsEnum ? Convert.ToInt32(val).ToString() : Convert.ToString(val);



            if (string.IsNullOrWhiteSpace( aProp.DecodeString )) return _rVal;
            string comp1 = string.Empty;
            string comp2 = string.Empty;
            string dcdval = string.Empty;
            int j = 0;
            comp1 = _rVal;
            string[] dCodes = aProp.DecodeString?.Split(',');
            for (int i = 0; i < dCodes?.Length; i++)
            {
                comp2 = dCodes[i];
                j = comp2.IndexOf("=");
                if (j != -1)
                {
                    dcdval = comp2.Substring(j + 1);
                    comp2 = comp2.Substring(0, j);
                    if (string.Compare(comp1, comp2, true) == 0)
                    {
                        _rVal = dcdval;
                        break;
                    }
                }
            }
            return _rVal;


        }
        public static uopProperty Quick(string aName, dynamic aValue, dynamic aLastValue, uopPart aPart = null) => new uopProperty(TPROPERTY.Quick (aName, aValue, aLastValue: aLastValue, aPart: aPart));

        #endregion Shared Methods
    }
}