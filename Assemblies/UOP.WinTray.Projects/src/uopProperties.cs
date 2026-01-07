using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.Projects
{

    /// <summary>
    /// a collection of uopProperties
    /// a simple collection class with some extended adding and retrieving functionality
    /// </summary>
    public class uopProperties: List<uopProperty>,  ICloneable
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;
    

        #region Fields
        //private List<uopProperty> _Members;
        internal List<TPROPERTIES> _StoredVals;
        #endregion Fields

        #region Constructors

        public uopProperties() { Clear(); }
        
        public uopProperties(uopProperty aProp, uopProperty bProp = null, uopProperty cProp = null)
        {
            Clear();
            if (aProp != null) Add(aProp);
            if (bProp != null) Add(bProp);
            if (cProp != null) Add(cProp);
        }
     


        public uopProperties(string aCategory) { Clear();  Category = aCategory; }


        internal uopProperties(TPROPERTIES aProperties, string aHeading = null, uopPart aPart = null) 
        {
            Clear();
            RangeGUID = aProperties.RangeGUID;
            PartIndex = aProperties.PartIndex;
            PartType = aProperties.PartType;
            NodeName = aProperties.NodeName;
            ProjectHandle = aProperties.ProjectHandle;
            Category = aProperties.Category;
            PartName = aProperties.PartName;
         
            if (aPart != null) SubPart(aPart,Name);

            for (int i = 1; i <= aProperties.Count; i++)
            {
                uopProperty newmem = new uopProperty(aProperties.Item(i)) { DisplayUnits = DisplayUnits };
                if (aPart != null) newmem.SubPart(aPart);
                base.Add(newmem);
            }
            if (aHeading != null) SetHeadings(aHeading); 
        
        }

        public uopProperties(uopProperties aProperties, string aHeading = null)
        {
            Clear();
            if (aProperties == null) return;
            RangeGUID = aProperties.RangeGUID;
            PartIndex = aProperties.PartIndex;
            PartType = aProperties.PartType;
            NodeName = aProperties.NodeName;
            ProjectHandle = aProperties.ProjectHandle;
            Category = aProperties.Category;
            PartName = aProperties.PartName;
           
            for (int i = 1; i <= aProperties.Count; i++)
            {
                base.Add(new uopProperty(aProperties.Item(i)) { DisplayUnits = DisplayUnits });
            }
            if (aHeading != null) SetHeadings(aHeading);

        }

        public uopProperties(List<uopProperty> aProps,uopProperties aParent = null) 
        {
            Clear();
            if (aParent != null)
            {
                RangeGUID = aParent.RangeGUID;
                PartIndex = aParent.PartIndex;
                PartType = aParent.PartType;
                NodeName = aParent.NodeName;
                ProjectHandle = aParent.ProjectHandle;
                Category = aParent.Category;
                PartName = aParent.PartName;
            }

            if (aProps == null) return;
            foreach (var item in aProps)
            {
                if (item != null)
                {
                    uopProperty newmem = new uopProperty(item.Structure)
                    {
                        DisplayUnits = DisplayUnits
                    };
                    Add(newmem);
                }
            }

        }

        #endregion Constructors

        #region Properties
        private uppUnitFamilies _DisplayUnits = uppUnitFamilies.Undefined;
        public uppUnitFamilies DisplayUnits
        {
            get => _DisplayUnits;
            set
            {
                bool newval = _DisplayUnits != value;
                _DisplayUnits = value;
                if (newval)
                {
                    foreach (uopProperty item in this) { item.DisplayUnits = value; }

                }


            }
        }


        private uppUnitFamilies _UnitSystem = uppUnitFamilies.English;
        public uppUnitFamilies UnitSystem { get => _UnitSystem; set => _UnitSystem = value; }



        public List<uopProperty> ToList
        {
            get
            {
                List<uopProperty> _rVal = new List<uopProperty>();
                _rVal.AddRange(this);
                return _rVal;
            }
        }

        private string _Category;
        public string Category { get => _Category; set => _Category = value; }
     
        private string _PartName;
        public string PartName { get => _PartName; internal set => _PartName = value; }

        /// <summary>
        /// the path to the properties collection
        /// like Column(1).Range(1).TrayAssembly.Deck.Properties
        /// </summary>
        private string _PartPath;
        public string PartPath { get => _PartPath; internal set => _PartPath = value; }

        private int _PartIndex;
        public int PartIndex { get => _PartIndex; internal set => _PartIndex = value; }

        /// <summary>
        /// assignable part type
        /// typically used to provide a the type name of the owning object of the property.
        //  is always stored in upper case.
        /// </summary>
        private uppPartTypes _PartType;
        public uppPartTypes PartType { get => _PartType; set => _PartType = value; }

        /// <summary>
        /// the handle of the project that owns this object
        /// </summary>
        private string _ProjectHandle;
        public string ProjectHandle { get => _ProjectHandle; set => _ProjectHandle = value; }

        private string _RangeGUID;
        public string RangeGUID { get => _RangeGUID; set => _RangeGUID = value; }

        private string _NodeName;
        public string NodeName { get => _NodeName; set => _NodeName = value; }

        private string _Name = string.Empty;
        public string Name { get => string.IsNullOrWhiteSpace(_Name) ? Category : _Name; set => _Name = value; } 

       
        /// <summary>
        /// returns all the names of all the properties
        /// </summary>
        public List<string> Names
        {
            get
            {
                List<string> _rVal = new List<string>();
                for (int i = 1; i <= Count; i++) { _rVal.Add(Item(i).Name); }
                return _rVal;
            }
        }

        /// <summary>
        /// returns all the names of all the properties
        /// </summary>
        public List<string> Descriptions
        {
            get
            {
                List<string> _rVal = new List<string>();
                for (int i = 1; i <= Count; i++) { _rVal.Add(Item(i).ToString()); }
                return _rVal;
            }
        }

        /// <summary>
        /// returns all the display names of all the properties
        /// </summary>
        public List<string> DisplayNames
        {
            get
            {
                List<string> _rVal = new List<string>();
                for (int i = 1; i <= Count; i++) { _rVal.Add(Item(i).DisplayName); }
                return _rVal;
            }
        }
        public int StoredValueCount => (_StoredVals == null) ? 0 : _StoredVals.Count;
   
      /// <summary>
        /// returns the last property in the collection
        /// </summary>
        public uopProperty LastProperty => Count <= 0 ? null : base[Count - 1];

        #endregion Properties

        #region Methods

   
        public void PrintToConsole(string aHeading = null, bool bIndexed = false, bool bPrintHeadings = false, bool? bHiddenVal = null)
        {

            string outs = string.Empty;
            string cHeading = string.Empty;
            string hd1 = string.Empty;
            bool testhidden = bHiddenVal.HasValue;
            bool thidden =testhidden && bHiddenVal.Value;
            bool doit = true;
            int idx = 0;

            if (!string.IsNullOrWhiteSpace(aHeading)) System.Diagnostics.Debug.WriteLine(aHeading);
            uopProperty mem;
            for (int i = 1; i <= Count; i++)
            {
                mem = Item(i);
               doit = !testhidden || mem.Hidden == thidden;
                if (doit)
                {
                    if (bPrintHeadings)
                    {

                        hd1 = mem.Heading.Trim();
                        if (hd1 !=  string.Empty)
                        {
                            if (i == 1)
                            {
                                cHeading = hd1;
                                System.Diagnostics.Debug.WriteLine("[" + cHeading + "]");
                            }
                            else
                            {
                                if (hd1 != cHeading)
                                {
                                    cHeading = hd1;
                                    System.Diagnostics.Debug.WriteLine("[" + cHeading + "]");
                                }

                            }
                        }
                    }
                    idx ++;
                    outs = bIndexed ? idx + " - " + mem.ToString() : mem.ToString();
                    System.Diagnostics.Debug.WriteLine(outs);
                }
               
            }
        }

     
        /// <summary>
        /// returns True if a property with the pased name exists in the array
        /// </summary>
        /// <param name="aName">the name to search for</param>
        /// <param name="rIndex">2returns the index of the first member with a matching name</param>
        /// <returns></returns>
        public bool HasMember(string aName, out int rIndex)
        {
            rIndex = 0;
            if (!TryGet(aName, out uopProperty mem)) return false;
            rIndex = mem.Index;
            return true;

        }

        public override string ToString()
        {
            string _rVal = $"uopProperties [{ Count }]";
            string name = Name;
            if (!string.IsNullOrWhiteSpace(name)) _rVal += $" {name}";
            return _rVal;
        }

        /// <summary>
        ///   // Linq version of the same function.
        /// returns True if a property with the pased name exists in the array
        /// </summary>
        /// <param name="aName">the name to search for</param>
        /// <param name="rIndex">2returns the index of the first member with a matching name</param>
        /// <returns></returns>
        public bool HasMember(string aName) => HasMember(aName, out int IDX);

        public bool AddUnique(uopProperty aProp, bool bDontCheck = false, dynamic aCategory = null, dynamic aHeading = null)
        {
            if (aProp == null) return false;
            bool _rVal = false;
            aProp.Name = aProp.Name.Trim();
            if (aProp.Name ==  string.Empty) { aProp.Name =$"Prop{ Count + 1}"; }

            if (aCategory != null) { aProp.Category = Convert.ToString(aCategory); }

            if (aHeading != null) { aProp.Heading = Convert.ToString(aHeading); }

            if (TryGet(aProp.Name, out uopProperty prop))
            {
                _rVal = string.Compare(prop.Value, aProp.Value, true) != 0;

                prop.Value = aProp.Value;
                prop.Category = aProp.Category;
                prop.Heading = aProp.Heading;
              
                return _rVal;
            }
            else
            {
                _rVal = true;
                Add(aProp);
                aProp.Index = Count;
                return _rVal;
            }

        }

        /// <summary>
        /// shorthand method for adding a property to the collection
        /// won't add a property with no name (no error raised).
        /// </summary>
        /// <param name="aPropName">the name of the property to add</param>
        /// <param name="aPart">the part to get the property from</param>
        /// <param name="aCaption">the caption assigned to the property</param>
        /// <param name="aChoices">an optional part type to assign to the property</param>
        public uopProperty AddPartProp(uopPart aPart, string aPropName, string aDisplayName = null,  string aChoices = null, string aColor = null, dynamic aOverrideValue = null, bool bSuppressNullProps = false)
        {
            try
            {
                if (aPart == null) throw new ArgumentException();
                if (string.IsNullOrEmpty(aPropName)) throw new ArgumentException();

                aPropName = aPropName.Trim();


                uopProperty aProp = aPart.CurrentProperty(aPropName, bSupressNotFoundError:true);
                if (aProp == null) throw new ArgumentException();

                if (aChoices != null) aProp.Choices = aChoices;
                if (aDisplayName != null) aProp.DisplayName = aDisplayName;
                if (aColor != null) aProp.DisplayColor = aColor;
                if (aOverrideValue != null) aProp.SetValue(aOverrideValue);

                if (bSuppressNullProps == true && aProp.HasNullValue)
                {
                    if (aProp.IsNullValue)
                        return null;
                }

                uopProperty _rVal = Add(aProp.Structure);
                return _rVal;
               
            }
            catch (Exception exception)
            {
                LoggerManager log = new LoggerManager();
                log.LogError(exception.Message);
                //Err.Raise Err.Number, Err.Source, Err.Description
            }
            return null;
        }


        /// <summary>
        /// shorthand method for adding a property to the collection
        /// won't add a property with no name (no error raised).
        /// </summary>
        /// <param name="aPropName">the name of the property to add</param>
        /// <param name="aPropVal">the value to assign to the new propert</param>
        /// <param name="aUnitType">the index of the units object to assign to the property</param>
        /// <param name="bIsHidden">flag to mark the new property as hidden</param>
        /// <param name="aHeading">the heading to assign to the new property</param>
        /// <param name="aCaption">the caption assigned to the property</param>
        /// <param name="aDecodeString"></param>
        /// <param name="bProtected"></param>
        /// <param name="aCategory"></param>
        /// <param name="aPartType"></param>
        /// <param name="aPrecision"></param>
        /// <param name="aChoices"></param>
        /// <param name="aPreviousVal"></param>
        public uopProperty Add(string aName, dynamic aPropVal,
            uppUnitTypes aUnitType = uppUnitTypes.Undefined,
            bool bIsHidden = false,
            string aHeading = "",
            string aCaption = "",
            string aDecodeString = "",
            bool bProtected = false,
            string aCategory = "",
            uppPartTypes aPartType = uppPartTypes.Undefined,
            int aPrecision = 0,
            string aChoices = "",
            dynamic aPreviousVal = null,
            string aColor = null,
            string aUnitCaption = null,
            string aDisplayName = "",
            bool bIsShared = false,
             bool bSetDefault = false,
             dynamic aNullVal = null
            )
        {
            if (string.IsNullOrWhiteSpace(aName) || aPropVal == null) return null;
            aName = aName.Trim();
            if (string.IsNullOrWhiteSpace(aName) || aPropVal == null) return null;
            TPROPERTY _rVal = new TPROPERTY()
            {
                Name = aName.Trim(),
                Heading = aHeading,
                Choices = aChoices?.Trim(),
                Units = new TUNIT(aUnitType),
                Hidden = bIsHidden,
                IsShared = bIsShared,
                DecodeString = aDecodeString,
                Protected = bProtected,
                Category = aCategory,
                PartType = aPartType,
                Precision = aPrecision,
                DisplayName = aDisplayName?.Trim()
            };
            //Caption = aCaption?.Trim(),

            _rVal.SetValue(aPropVal);
            _rVal.Color = (!string.IsNullOrWhiteSpace(aColor)) ? aColor : "Black";

            if (!string.IsNullOrWhiteSpace(aUnitCaption)) { _rVal.SetUnitCaption(aUnitCaption); }
            if (aNullVal != null) { _rVal.NullValue = aNullVal; }

            if (aPreviousVal != null) _rVal.LastValue = aPreviousVal;
            if (bSetDefault && _rVal.HasValue) _rVal.DefaultValue = _rVal.Value;

            return (Add(_rVal) != null) ? Item(Count) : null;



            //System.Diagnostics.Debug.WriteLine(Count);
        }

        /// <summary>
        /// used to add an item to the collection
        /// won't add "Nothing" (no error raised).
        /// </summary>
        /// <param name="aProperty">the item to add to the collection</param>
        /// <param name="aCategory"></param>
        /// <param name="aHeading"></param>
        /// <returns></returns>
     

        public uopProperty Add(uopProperty aProp, bool bAddClone = false, string aColor = null, string aDisplayName = null, string aCategory = null, string aHeading  = null, uppPartTypes aPartType = uppPartTypes.Undefined)
        {
            if (aProp == null) return null;
            if (bAddClone) aProp = aProp.Clone();
            if(string.IsNullOrWhiteSpace(aProp.Name) && !bAddClone) return null;
            if (string.IsNullOrWhiteSpace(aProp.Name)) aProp.Name = $"Property { Count + 1}";
            int i = 1;
            string name = aProp.Name;
            while( TryGet(aProp.Name,out _ )){ aProp.Name = $"{name}_{ i})"; i++; }
            if (!string.IsNullOrWhiteSpace(aColor)) { aProp.DisplayColor = aColor; }
            if (!string.IsNullOrWhiteSpace(aDisplayName)) { aProp.DisplayName = aDisplayName; }
            if (!string.IsNullOrWhiteSpace(aCategory)) { aProp.Category = aCategory; }
            if (!string.IsNullOrWhiteSpace(aHeading)) { aProp.Heading = aHeading; }
            if(aPartType != uppPartTypes.Undefined) { aProp.PartType = aPartType; }
            aProp.DisplayUnits = DisplayUnits;
            base.Add(aProp);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, aProp));
            return Item(Count);
        }

        internal uopProperty Add(TPROPERTY aProp)
        {
          
           
            if (string.IsNullOrWhiteSpace(aProp.Name)) return null;
            if (string.IsNullOrWhiteSpace(aProp.Name)) aProp.Name = $"Property { Count + 1}";
            int i = 1;
            string nm = aProp.Name;
           
            while (TryGet(aProp.Name, out _)) 
            { 
                aProp.Name = $"{nm}_{ i}"; i++; 
            }

            uopProperty newmem = new uopProperty(aProp)
            {
                DisplayUnits = DisplayUnits
            };
            base.Add(newmem);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newmem));
            return base[Count - 1];
        }

        internal uopProperty Add(TPROPERTY aProp, string aName, string aColor = null)
        {
            if (!string.IsNullOrWhiteSpace(aName)) aProp.Name = aName.Trim();
            if(!string.IsNullOrWhiteSpace(aColor))  aProp.Color = aColor.Trim();
            return Add(aProp);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newprops">the properties to add to the collection</param>
        /// <param name="aCategory"></param>
        public void Append(uopProperties newprops, string aCategory = null, string aHeading = null, bool bAddClones = true, uppPartTypes aPartType = uppPartTypes.Undefined)
        {
            if (newprops == null) return;
            foreach (uopProperty item in newprops)
            {
                Add(item, bAddClone: bAddClones, aCategory: aCategory, aHeading: aHeading, aPartType: aPartType);
            }
           
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newprops">the properties to add to the collection</param>
        internal void Append(TPROPERTIES newprops)
        {
            if (newprops.Count <= 0) return;

            for (int i = 1; i <= newprops.Count; i++)
            {
                Add(newprops.Item(i));
            }
        }

        public void SetRowCol(int aIndex, dynamic aRow = null, dynamic aCol = null)
        {
            if (aIndex < 1 || aIndex > Count) return;
            if (aRow != null) base[aIndex - 1].Row = mzUtils.VarToInteger(aRow);
            if (aCol != null) base[aIndex - 1].Col = mzUtils.VarToInteger(aCol);
        }

        public void SortByName()
        {
            List<Tuple<string, uopProperty>> srt = new List<Tuple<string, uopProperty>>();
            foreach (uopProperty mem in this)
            {
                srt.Add(new Tuple<string, uopProperty>(mem.Name, mem));
            }

            srt = srt.OrderBy(t => t.Item1).ToList();
            Clear();
            foreach (Tuple<string, uopProperty> item in srt)
            {
                base.Add(item.Item2);
            }

          
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, null));
        }

        /// <summary>
        /// removes all the properties from the collection
        /// </summary>
        public new void Clear()
        {
            base.Clear();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, null));
        }

        /// <summary>
        /// returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        public uopProperties Clone() => new uopProperties(this);

        public uopProperties Clone(bool bReturnEmpty)
        {
            TPROPERTIES struc = new TPROPERTIES(this);
            if(bReturnEmpty) struc.Clear();
            return new uopProperties(struc);
            
        }

        object ICloneable.Clone() => (object)this.Clone();


        /// <summary>
        /// returns the properties from the collection whose names match the on in the passed list
        /// returns True if any of the property names are founf
        /// </summary>
        /// <param name="aPropNames">the property names to search for</param>
        /// <param name="aDelimitor">the deliminator of the  passed string</param>
        /// <param name="aProps"></param>
        /// <returns></returns>
        public List<uopProperty> ContainsProperties(string aPropNames, string aDelimitor)
        {
            List<uopProperty> rProps = new List<uopProperty>();
           
            try
            {

                if (string.IsNullOrWhiteSpace(aPropNames)) return rProps;

                aPropNames = aPropNames.Trim();
                aDelimitor = aDelimitor.Trim();
                List<string> names = new List<string>() { aPropNames };
                if (!string.IsNullOrWhiteSpace(aDelimitor))
                {
                    names = mzUtils.StringsFromDelimitedList(aPropNames, aDelimitor);
                }
                string pname = string.Empty;
                uopProperty aMem;
               
                for (int i = 1; i <= names.Count; i++)
                {
                    pname = names[i - 1];
                    if (TryGet(pname, out aMem)) { rProps.Add(aMem); }

                }
              
            }
            catch (Exception exception)
            {
                LoggerManager log = new LoggerManager();
                log.LogError(exception.Message);
            }
            return rProps;
        }

        /// <summary>
        /// returns the properties from the collection whose names match the on in the passed list
        /// returns True if any of the property names are founf
        /// </summary>
        /// <param name="aPropNames">the property names to search for</param>
        /// <param name="aDelimitor">the deliminator of the  passed string</param>
        /// <param name="aProps"></param>
        /// <returns></returns>
        public int ContainsProperties(string aPropNames, string aDelimitor, out List<uopProperty> rProps)
        {
            rProps = ContainsProperties(aPropNames, aDelimitor);
            return rProps.Count;
        }

        public void ConvertUnits(uppUnitFamilies aFromUnits, uppUnitFamilies aToUnits)
        {

            TPROPERTY p;
            foreach (var prop in this)
            {
                p = prop.Structure;
                p.ConvertUnits(aFromUnits, aToUnits);
                prop.Structure = p;

            }

        }

        public bool Contains(string aPropertyName) => !string.IsNullOrWhiteSpace(aPropertyName) && Count > 0 && FindIndex(x => x != null && string.Compare(x.Name, aPropertyName, true) == 0) > 0;
        /// <summary>
        // Searchs this collection for properties that match the members of the passed collection.  If a match is found the value of the passed property is copied to the
        // matching member of this collection.
        // if there are members with the same name only the first member found receives the new value.
        // If there is not a matching member for a member of the passed collection and the second argument is true
        // a clone of the non-member property is added.
        // returns a list of the changed names
        /// </summary>
        /// <param name="aPropCol">the collection to copy</param>
        /// <param name="bCopyNonMembers">flag to copy any properties in the passed collection that are not already members of this collection</param>
        /// <returns></returns>
        public string CopyValues(uopProperties aPropCol, bool bCopyNonMembers = false)
        {
            if (aPropCol == null || aPropCol.Count <= 0) return "";
            return CopyValuesV(new TPROPERTIES(aPropCol), bCopyNonMembers);
        }

        /// <summary>
        // Searchs this collection for properties that match the members of the passed collection.  If a match is found the value of the passed property is copied to the
        // matching member of this collection.
        // if there are members with the same name only the first member found receives the new value.
        // If there is not a matching member for a member of the passed collection and the second argument is true
        // a clone of the non-member property is added.
        /// </summary>
        /// <param name="aPropCol">the collection to copy</param>
        /// <param name="bCopyNonMembers">flag to copy any properties in the passed collection that are not already members of this collection</param>
        /// <returns></returns>
        internal string CopyValuesV(TPROPERTIES aPropCol, bool bCopyNonMembers = false)
        {

            string rChangeNames = string.Empty;
            if (aPropCol.Count <= 0) return "";

            try
            {
                TPROPERTY aProp;
                uopProperty myProp;

                bool aFlag = false;
                for (int i = 1; i <= aPropCol.Count; i++)
                {
                    aProp = aPropCol.Item(i);
                    if (TryGet(aProp.Name, out myProp))
                    {
                        aFlag = myProp.SetValue(aProp.Value);
                        if (aFlag)
                        {
                            if (!string.IsNullOrEmpty(rChangeNames)) rChangeNames += ",";

                            rChangeNames += aProp.Name;
                        }
                    }
                    else
                    {
                        if (bCopyNonMembers)
                        {
                            Add(aProp);
                            if (!string.IsNullOrEmpty(rChangeNames)) rChangeNames += ",";

                            rChangeNames += aProp.Name;

                        }
                    }
                }
            }
            catch (Exception exception)
            {
                LoggerManager log = new LoggerManager();
                log.LogError(exception.Message);
            }
            return rChangeNames;
        }


        /// <summary>
        /// returns the value of the item from the collection at the requested index or the passed name
        /// </summary>
        /// <param name="aNameOrIndex"></param>
        /// <returns></returns>
        public dynamic DefaultValue(string aName)
        {
            if (!TryGet(aName, out uopProperty aMem)) return null;
            return aMem.DefaultValue;
        }

        /// <summary>
        /// Populates the collection with the properties defined in the passed string
        /// string like "Color=Red¸Count=240" etc.  Numerics are assumed to be doubles all others are strings.
        /// </summary>
        /// <param name="aPropString">a delimited string of property signatures</param>
        /// <param name="aDelimitor">the delimiter to look for</param>
        public void DefineByString(string aPropString, string aDelimitor = "¸", bool bClearExisting = false)
        {
            if (bClearExisting) Clear();
            TVALUES pVals = TVALUES.FromDelimitedList(aPropString, aDelimitor);
            string Sig = string.Empty;
            TVALUES nv;

            for (int i = 1; i <= pVals.Count; i++)
            {
                Sig = Convert.ToString(pVals.Item(i));
                nv = TVALUES.FromDelimitedList(Sig, "=", true, true);
                if (nv.Count > 2) Add(aName: Convert.ToString(nv.Item(1)),aPropVal: Convert.ToString(nv.Item(2)));
            }
        }

        /// <summary>
        /// returns the values of properties in the collection in a comma (or other deliminator) string
        /// </summary>
        /// <param name="aDelimitor">an optional delimator</param>
        /// <param name="aStartIndex">the index to begin the procedure (default = 1)</param>
        /// <param name="aEndIndex">the index to end the procedure (default = count)</param>
        /// <param name="bIncludePropertyNames">flag to return the properties complete signatures in the returned string</param>
        /// <param name="aWrapper">an optional string to put before anf after the values in the returned string</param>
        /// <param name="bShowDecodedValue"></param>
        /// <returns></returns>
        public string DeliminatedString(string aDelimitor = ",", int aStartIndex = 0,
            int aEndIndex = 0, bool bIncludePropertyNames = false,
            string aWrapper = "", bool bShowDecodedValue = false)
        {
            string _rVal = string.Empty;

            if (Count <= 0) return _rVal;
            mzUtils.LoopLimits(aStartIndex, aEndIndex, 1, Count, out int si, out int ei);

            uopProperty aProp;
            string dlm = string.Empty;
            string sVal;


            dlm = aDelimitor.Trim();
            if (dlm ==  string.Empty) dlm = ",";

            for (int i = si; i <= ei; i++)
            {
                aProp = Item(i);

                sVal = bShowDecodedValue ? aProp.DecodedValue : aProp.ValueS;

                if (!bIncludePropertyNames)
                {
                    if (aWrapper ==  string.Empty)
                    {
                        if (sVal.IndexOf(aDelimitor, StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            sVal = (char)34 + sVal + (char)34;
                        }
                        _rVal += sVal;
                    }
                    else
                    {
                        _rVal = _rVal + aWrapper + sVal + aWrapper;
                    }
                }
                else
                {
                    _rVal = _rVal + aWrapper + aProp.Name + "=" + sVal + aWrapper;
                }
                if (i < ei) _rVal += dlm;


            }

            return _rVal;
        }

    
        /// <summary>
        /// returns all the properties in the collection that have a handle matching the search value
        /// </summary>
        /// <param name="aCategory">the property handle to search for</param>
        /// <param name="bVisibleOnly">an optional collection of properties to search other that this one</param>
        /// <param name="bRemove"></param>
        /// <returns></returns>
        public List<uopProperty> GetByCategory(string aCategory, bool bVisibleOnly = false, bool bRemove = false)
        {
            if (aCategory == null) return null;
            List<uopProperty> _rVal = FindAll(x => string.Compare(x.Category, aCategory, ignoreCase: true) == 0);
            if (_rVal.Count == 0) return null;
            if (bVisibleOnly) _rVal = _rVal.FindAll(x => x.Hidden == false);
            if (bRemove) RemoveRange(_rVal);
            return _rVal;
        }

        /// <summary>
        /// returns all the properties in the collection that have a handle matching the search valu
        /// </summary>
        /// <param name="aCategory">the property handle to search for</param>
        /// <param name="bVisibleOnly">an optional collection of properties to search other that this one</param>
        /// <returns></returns>
        public List<uopProperty> RemoveByCategory(string aCategory, bool bVisibleOnly) => GetByCategory(aCategory, bVisibleOnly, true);

        /// <summary>
        /// returns the properties from the collection that have a heading property matching the passed string
        /// search is not case sensitive
        /// </summary>
        /// <param name="aHeading">1the heading to search form</param>
        /// <returns></returns>
        public List<uopProperty> GetByHeading(string aHeading, bool bVisibleOnly= false, bool bRemove = false)
        {
            if (aHeading == null) return null;
            List<uopProperty> _rVal = FindAll(x => string.Compare(x.Heading, aHeading, ignoreCase: true) == 0);
            if (_rVal.Count == 0) return _rVal;
            if (bVisibleOnly) _rVal = _rVal.FindAll(x => x.Hidden == false);
            if (bRemove) RemoveRange(_rVal);
            return _rVal;
        }

        /// <summary>
        /// returns the properties from the collection that have a hidden property matching the passed boolean
        /// </summary>
        /// <param name="bHiddenVal">the hidden value to search for</param>
        /// <returns></returns>
        public List<uopProperty> GetByHidden(bool bHiddenVal)
        {
            
            List<uopProperty> _rVal = FindAll(x => x.Hidden == bHiddenVal);
            return _rVal;
        }

        /// <summary>
        /// returns the first property in the collection whose part path matches the passed string
        /// </summary>
        /// <param name="aPartPath"></param>
        /// <param name="aIndex"></param>
        /// <returns></returns>
        public List<uopProperty> GetByPartPath(string aPartPath, bool bVisibleOnly = false, bool bRemove = false)
        {
            if (aPartPath == null) return null;
            List<uopProperty> _rVal = FindAll(x => string.Compare(x.PartPath, aPartPath, ignoreCase: true) == 0);
            if (_rVal.Count == 0) return _rVal;
            if (bVisibleOnly) _rVal = _rVal.FindAll(x => x.Hidden == false);
            if (bRemove) RemoveRange(_rVal);
            return _rVal;
        }

        public List<uopProperty> GetByProtected(bool aProtectedValue)
        {

            List<uopProperty> _rVal = FindAll(x => x.Protected == aProtectedValue);
            return _rVal;
        }


        public List<uopProperty> GetMembers(string aNamesList, string aDelimitor = ",")
        {
            List<uopProperty> _rVal = new List<uopProperty>();
            List<string> nms = mzUtils.StringsFromDelimitedList(aNamesList, aDelimitor);
            uopProperty aMem = null;

            foreach (var item in nms)
            {
                if (TryGet(item, out aMem)) _rVal.Add(aMem);
            }
         
            return _rVal;
        }

        /// <summary>
        /// returns all the properties in the collection that have a value matching the search valu
        /// </summary>
        /// <param name="aSearchVal">the property value to search for</param>
        /// <returns></returns>
        public List<uopProperty> GetByValue(dynamic aSearchVal)
        {
            List<uopProperty> _rVal = new List<uopProperty>();

            TPROPERTY aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i).Structure;
                if (aMem.Value == aSearchVal)
                { _rVal.Add(Item(i)); }
            }
            return _rVal;
        }

        /// <summary>
        ///returns all the properties from the collection whose values are not currently equal to their set default  values
        /// </summary>
        /// <param name="aStatus">the default status to search for</param>
        /// <returns></returns>
        public List<uopProperty> GetByDefaultStatus(bool aStatus, string aNameList = "", uopProperty aEmptyProp = null, int aStartIndex = 0, int aEndIndex = 0)
        {
            return GetByDefaultStatus(aStatus, aStartIndex, aEndIndex, aNameList, aEmptyProp: aEmptyProp, out string NMS);
        }

        /// <summary>
        ///returns all the properties from the collection whose values are not currently equal to their set default  values
        /// </summary>
        /// <param name="aStatus">the default status to search for</param>
        /// <param name="aStartIndex"></param>
        /// <param name="aEndIndex"></param>
        /// <param name="aNameList"></param>
        /// <param name="rNameList"></param>
        /// <returns></returns>
        public List<uopProperty> GetByDefaultStatus(bool aStatus, int aStartIndex, int aEndIndex, string aNameList, uopProperty aEmptyProp, out string rNames)
        {
            rNames = string.Empty;
            List < uopProperty > _rVal = new List<uopProperty>();
            if (Count <= 0) return _rVal;
            uopProperty aMem;
            bool bTestName = !string.IsNullOrWhiteSpace(aNameList);
           
            aNameList = aNameList.Trim();
            if (bTestName) { aNameList = "," + aNameList + ","; }


            mzUtils.LoopLimits(aStartIndex, aEndIndex, 1, Count, out int si, out int ei);

            bool bKeep = false;

            for (int i = si; i <= ei; i++)
            {
                aMem = Item(i);
                bKeep = false;
                if (!bTestName)
                {
                    bKeep = aMem.IsDefault == aStatus;
                }
                else
                {
                    if (aNameList.IndexOf("," + aMem.Name + ",", StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        bKeep = aMem.IsDefault == aStatus;
                    }
                }

                if (bKeep)
                {
                    _rVal.Add(aMem);
                    if (rNames !=  string.Empty) { rNames += ","; }
                    rNames += aMem.Name;
                }
            }
            if (_rVal.Count <= 0 && aEmptyProp != null) _rVal.Add(new uopProperty( aEmptyProp.Structure));
            return _rVal;
        }

        public uopProperties GetHiddenProperties(bool bHiddenValue) => new uopProperties(GetByHidden(bHiddenValue));

        /// <summary>
        /// used to compare two property collections and deterime how they differ from each other
        /// </summary>
        /// <param name="aPropCol">the collection to compare to</param>
        /// <param name="rDifferences">returns a collection of properties from the passed collection that have the same name but different values</param>
        /// <param name="rLikes">returns a collection of properties from the passed collection that have the same and equal values</param>
        /// <param name="rNotMembers">returns a collection of properties from the passed collection that are not found in the current collection</param>
        /// <param name="aPrecis"></param>
        /// <param name="bBailOnFistDifference"></param>
        public void GetIntersections(uopProperties aPropCol, out uopProperties rDifferences,
            out uopProperties rLikes, out uopProperties rNotMembers,
            int aPrecis = 5, bool bBailOnFistDifference = false)
        {
            rDifferences = new uopProperties();
            rLikes = new uopProperties();
            rNotMembers = new uopProperties();
            if (aPropCol == null) return;


            TPROPERTIES P1 = new TPROPERTIES();
            TPROPERTIES P2 = new TPROPERTIES();
            TPROPERTIES p3 = new TPROPERTIES();
            TPROPERTIES.GetIntersections(new TPROPERTIES(this), new TPROPERTIES(aPropCol), out P1, out P2, out p3, aPrecis, bBailOnFistDifference);

            rDifferences = new uopProperties( P1);
            rLikes = new uopProperties(P2);
            rNotMembers = new uopProperties( p3);
        }

        /// <summary>
        /// used to compare two property collections and deterime how they differ from each other
        /// </summary>
        /// <param name="aProps">the first properties to compare to</param>
        /// <param name="bProps">the second properties to compare to</param>
        /// <param name="rDifferences">returns a collection of properties from the passed collection that have the same name but different values</param>
        /// <param name="rLikes">returns a collection of properties from the passed collection that have the same name and equal values</param>
        /// <param name="rNotMembers">returns a collection of properties from the passed collection that are not found in the current collection</param>
        /// <param name="aPrecis"></param>
        /// <param name="bBailOnFistDifference"></param>
        public static void GetIntersections(List<uopProperty> aProps, List<uopProperty> bProps, out List<uopProperty> rDifferences, out List<uopProperty> rLikes, out List<uopProperty> rNotMembers, int aPrecis = 5, bool bBailOnFistDifference = false)
        {
            rDifferences = new List<uopProperty>();
            rLikes = new List<uopProperty>();
            rNotMembers = new List<uopProperty>(); ;

            if (bProps.Count <= 0)  return; 



            for (int i = 1; i <= bProps.Count; i++)
            {
                uopProperty hisProp = bProps[i-1];
                uopProperty myProp = aProps.Find(x => string.Compare(x.Name, hisProp.Name, true) == 0);
                if (myProp != null)
                {
                    if (uopProperty.Compare(myProp, hisProp, aPrecis))
                    { rLikes.Add(hisProp); }
                    else
                    {
                        rDifferences.Add(hisProp);
                        if (bBailOnFistDifference)
                        { break; }
                    }
                }
                else
                {
                    rNotMembers.Add(hisProp);
                }
            }
        }

        /// <summary>
        /// ^returns a collection of properties from the passed collection that have the same name but different values
        /// </summary>
        /// <param name="aPropCol"></param>
        /// <param name="aPrecis">the properties to compare to</param>
        /// <param name="bBailOnFistDifference"></param>
        /// <param name="rDifIndices"></param>
        /// <returns></returns>
        public uopProperties GetDifferences(List<uopProperty> aPropCol, int aPrecis = 5, bool bBailOnFistDifference = false, string rDifIndices = "")
        {
            uopProperties _rVal = new uopProperties(); 
            if (aPropCol == null) return _rVal;

            
            uopProperties.GetIntersections(this, aPropCol, out List<uopProperty> diffs , out List<uopProperty> likes , out _, aPrecis, bBailOnFistDifference);
            _rVal.AddRange(diffs);
            return _rVal;
        }


        /// <summary>
        /// returns all the properties from the collection whose values are not currently equal to their set default  values
        /// </summary>
        /// <param name="aStartIndex">the index to begin the procedure (default = 1)</param>
        /// <param name="aEndIndex">the index to end the procedure (default = count)</param>
        /// <returns></returns>
        public uopProperties GetNonDefaults(int aStartIndex = 0, int aEndIndex = 0) => new uopProperties(GetByDefaultStatus(false, aStartIndex: aStartIndex, aEndIndex: aEndIndex));


        /// <summary>
        /// returns the first property in the collection whose  name matches the passed string
        /// </summary>
        /// <param name="aNameOrIndex"></param>
        /// <param name="aIndex"></param>
        /// <returns></returns>
        public uopProperty GetProperty(dynamic aNameOrIndex) => GetProperty(aNameOrIndex, out int IDX);

        /// <summary>
        /// returns the first property in the collection whose  name matches the passed string
        /// </summary>
        /// <param name="aNameOrIndex"></param>
        /// <param name="aIndex"></param>
        /// <returns></returns>
        public uopProperty GetProperty(dynamic aNameOrIndex, out int rIndex)
        {
            TPROPERTY aProp = Member(aNameOrIndex);
            rIndex = aProp.Index;
            return (rIndex > 0) ? Item(rIndex) : null;

        }

        /// <summary>
        /// returns all the headings of all the properties
        /// </summary>
        public List<string> Headings => GetHeadings(false);

        /// <summary>
        /// returns a collection containg all of the unique property headings in the collection
        /// </summary>
        /// <returns></returns>
        public List<string> GetHeadings(bool bUnique = true)
        {
            bool HaveIt;
            uopProperty aMem;
            string src;
            List<string> _rVal = new List<string>();
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                src = aMem.Heading;
                src ??= string.Empty;
                if (!bUnique)
                {
                    _rVal.Add(src);
                }
                else
                {
                    HaveIt = true;

                    if (!string.IsNullOrWhiteSpace(src))
                    {
                        HaveIt = _rVal.FindIndex(x => string.Compare(x, src, ignoreCase: true) == 0) >= 0;
                    }
                    if (!HaveIt) _rVal.Add(src);

                }

            }
            return _rVal;
        }

        /// <summary>
        /// ^returns True if the passed properties collection contains the same number and values of properties as the current properties collection
        /// </summary>
        /// <param name="aProperties">the properties collection to compare to</param>
        /// <returns></returns>
        public bool IsEqual(uopProperties aProperties, int aPrecis = 3)
        {
            if (aProperties == null) return false;

            if (aProperties.Count != Count) return false;
            GetIntersections(aProperties, out uopProperties p1, out uopProperties P2, out uopProperties p3, aPrecis, false);

            return p1.Count == 0 && p3.Count == 0;
        }
  
        public bool IsHidden(string aName)
        {

            if (!TryGet(aName, out uopProperty aMem)) return false;
            return aMem.Hidden;
        }
        public bool IsNullValue(string aName,bool bReturnTrueForZeroOrLess = false)
        {
   
            if (!TryGet(aName, out uopProperty aMem)) return false;
            bool _rVal = aMem.IsNullValue;
            if (bReturnTrueForZeroOrLess)
            {
                if (aMem.ValueD <= 0) _rVal =true;
            }
            return _rVal;
        }

        public bool IsHidden(int aIndex)
        {

            if (aIndex < 1 || aIndex > Count) return false;
            return Item(aIndex).Hidden;
        }

        public bool IsLocked(string aName)
        {
            uopProperty aMem = null;
            if (!TryGet(aName, out aMem)) return false;
            return aMem.Locked;
        }

        public bool IsLocked(int aIndex)
        {
          
            if (aIndex <1 || aIndex > Count) return false;
            return Item(aIndex).Locked;
        }

        public bool TryGet(dynamic aNameOrIndex, out uopProperty aProperty, Dictionary<string, string> EqualNames = null)
        {
            aProperty = null;
            if (aNameOrIndex == null) return false;
            if (Count <= 0) return false;
            int idx = 0;
            if (aNameOrIndex is string)
            {
                string aPropertyName = (string)aNameOrIndex;
                idx = this.FindIndex(x => string.Compare(x.Name, aPropertyName, true) == 0) + 1;
                if (idx < 1) idx = FindIndex(x => string.Compare(x.DisplayName, aPropertyName, true) == 0) + 1;
                if(idx < 1 && EqualNames != null)
                {
                    if(EqualNames.TryGetValue(aPropertyName, out string alias))
                    {
                        idx = FindIndex(x => string.Compare(x.Name, alias, true) == 0) + 1;
                    }
                }

                if (idx < 1 && mzUtils.IsNumeric(aPropertyName)) idx = mzUtils.VarToInteger(aPropertyName);
            }
            if (idx < 1) 
            {
                if (mzUtils.IsNumeric(aNameOrIndex)) idx = mzUtils.VarToInteger(aNameOrIndex);
            }


            if (idx < 1 || idx > Count) return false;
            aProperty = Item(idx);
            return true;
        }

        /// <summary>
        /// returns the item from the collection at the requested index or the passed name
        /// </summary>
        /// <param name="aIndex"></param>

        /// <returns></returns>
        public uopProperty Item(int aIndex) 
        {
            if (aIndex <= 0 || aIndex > Count) throw new IndexOutOfRangeException();
            base[aIndex - 1].DisplayUnits = DisplayUnits;
            uopProperty _rVal = base[aIndex - 1];
            _rVal.Index = aIndex;
            return _rVal;
        }

        /// <summary>
        /// returns the item from the collection at the requested index or the passed name
        /// </summary>
        /// <param name="aNameOrIndex"></param>
        /// <param name="bSupressNotFoundError"></param>
        /// <param name="aIndex"></param>
        /// <returns></returns>
        public uopProperty Item(string aName,bool bSupressNotFoundError = false )
        {
            if (TryGet(aName, out uopProperty aMem)) return aMem;
            if (!bSupressNotFoundError) throw new Exception($"The Requested Property [{ aName }] Does Not Exist");
            return null;
        }

        public void LockTypes()
        {
            uopProperty aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                aMem.TypeLocked = true;
                if (aMem.Value != null && aMem.VariableTypeName != null)
                {
                    var str = aMem.Value.GetType();
                    aMem.VariableTypeName = str.Name;
                }
                //if (bSetDefaults)
                //{
                //    aMem.DefaultValue = aMem.Value;
                //}
                //SetItem(aMem.Index, aMem);
            }

        }


        internal TPROPERTY Member(dynamic aNameOrIndex)
        {
            TPROPERTY _rVal = new TPROPERTY();
            uopProperty mem = null;
            if(aNameOrIndex is string)
            {
                TryGet((string)aNameOrIndex, out mem);
            }
            else
            {
                int idx = mzUtils.VarToInteger(aNameOrIndex);
                if (idx >= 1 && idx <= Count) mem = Item(idx);
            }
            if (mem != null) _rVal = mem.Structure;
            return _rVal;
        }

        /// <summary>
        /// returns all the names of all the properties
        /// </summary>
        /// <param name="bUnique"></param>
        /// <param name="aDelimiter"></param>
        /// <returns></returns>
        public string NameList(bool bUnique = false, string aDelimiter = "")
        {
            string _rVal = string.Empty;
            for (int i = 1; i <= Count; i++) { mzUtils.ListAdd(ref _rVal, Item(i).Name); }
            return _rVal;
        }

        public void Notify(uopProperty aProperty) 
        { 
            
        }


        /// <summary>
        /// sets the property in the collection with the passed name or index to the passed value
        /// returns the property if the property value actually changes.
        /// </summary>
        /// <param name="aNameOrIndex">the name or index of the property to set</param>
        /// <param name="aPropVal">the value to set the property value to</param>
        /// <param name="rPropName">returns the name of the requeste proeprty if it exists</param>
        /// <param name="bSuppressErrs"></param>
        /// <returns></returns>
        public uopProperty PropValSet(dynamic aNameOrIndex, dynamic aPropVal) => PropValSet(aNameOrIndex, aPropVal, out string PNAME, false);


        /// <summary>
        /// sets the property in the collection with the passed name or index to the passed value
        /// returns the property if the property value actually changes.
        /// </summary>
        /// <param name="aNameOrIndex">the name or index of the property to set</param>
        /// <param name="aPropVal">the value to set the property value to</param>
        /// <param name="rPropName">returns the name of the requeste proeprty if it exists</param>
        /// <param name="bSuppressErrs"></param>
        /// <returns></returns>
        public uopProperty PropValSet(dynamic aNameOrIndex,
            dynamic aPropVal, out string rPropName, bool bSuppressErrs = false)
        {

            uopProperty _rVal = null;
            rPropName = string.Empty;
          
            bool bChanged = false;

            if (TryGet(aNameOrIndex, out uopProperty aMem))
            {
                bChanged = aMem.SetValue(aPropVal);
                if (!bChanged) return null;

                _rVal = aMem;

                rPropName = aMem.Name;

               
                Notify(_rVal);
            }
            else
            {
                if (!bSuppressErrs)
                {
                    throw new Exception("The Requested Property [" + aNameOrIndex + "] Does Not Exist");
                }
            }
            return _rVal;
        }


        /// <summary>
        /// resets the indexs of the properties in the collection
        /// </summary>
        private void ReIndex() { for (int i = 1; i <= Count; i++) { base[i - 1].Index = i; } }

        public int ReadFromINIFile(string aFileSpec, string aFileSection, bool bRaiseNotFoundError = false)
        {

            int _rVal = 0;
            try
            {
                if (string.IsNullOrWhiteSpace(aFileSpec)) { throw new Exception("The Passed File Name Is Invalid"); }
                if (aFileSection ==  string.Empty) { throw new Exception("The Passed FileSectionIs Invalid"); }
                if (!System.IO.File.Exists(aFileSpec)) { throw new Exception("file Not Found '" + aFileSpec + "'"); }

                foreach (uopProperty mem in this)
                {
                    if (mem.ReadFromINIFile( aFileSpec, aFileSection))
                    {
                        _rVal += 1;
                    
                    }

                }

              
                return _rVal;
            }
            catch (Exception exception)
            {
                LoggerManager log = new LoggerManager();
                log.LogError(exception.Message);
                //Err.Raise(Err().Number, "props_ReadFromFile", sErr);
                return _rVal;
            }
           
        }

        public int RemoveRange ( List<uopProperty> aRange)
        {
            if (aRange == null) return 0;
            int _rVal = 0;
            int idx;
            List<uopProperty> newmems = new List<uopProperty>();
            foreach (uopProperty item in aRange)
            {
                idx = IndexOf(item);
                if (idx >= 0)
                {
                    _rVal++;
                    Remove(item);
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, idx));

                }
                
            }
            return _rVal;
        }

        public new uopProperty Remove(uopProperty aMember)
        {
            if (aMember == null) return aMember;

            int idx = IndexOf(aMember);
            if (idx < 0) return aMember;
            base.Remove(aMember);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, aMember, aMember.Index - 1));
            return aMember;
        }

        public uopProperty Remove(string aName)
        {

            if (!TryGet(aName, out uopProperty mem)) return null;
            Remove(mem);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, mem, mem.Index - 1));
            return mem;
        }

        public uopProperty Remove(int aIndex) 
        { 
            uopProperty mem = (aIndex >= 1 && aIndex <=Count)? Item(aIndex) : null;
            if (mem == null) return null;
            Remove(mem);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, mem, aIndex - 1));

            return mem;
        }

        public List<uopProperty> RemoveByName(string aNamesList, char aDelimitor = ',')
        {
            List<uopProperty> _rVal = new List<uopProperty>();

            if (string.IsNullOrWhiteSpace(aNamesList) || Count <= 0) return _rVal;
            string[] pNames = aNamesList.Split(aDelimitor);
            uopProperty mem;
            bool keep = true;
           
            List<uopProperty> removers = new List<uopProperty>();
            for (int j = 1; j <= Count; j++)
            {
                mem = Item(j);
                keep = true;
                for (int i = 0; i < pNames.Length; i++)
                {
                    if (string.Compare(pNames[i], mem.Name, ignoreCase: true) == 0)
                    {
                        keep = false;
                        break;
                    }
                }

                if (!keep) removers.Add(mem);
              
             
            }
            foreach (uopProperty item in removers)
            {
                Remove(item);
            }
         

            return removers;
        }

        public List<uopProperty> RemoveByPartType(uppPartTypes aPartType, uppDocumentTypes aDocumentType)
        {

            List < uopProperty > _rVal  = new List<uopProperty>();
            uopProperty aMem;
            bool bKeep = false;
           


            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                bKeep = true;
                if (aMem.PartType == aPartType)
                {
                    if (aDocumentType == uppDocumentTypes.Undefined || aMem.DocumentType == aDocumentType)
                    { bKeep = false; }
                }
                if (!bKeep) _rVal.Add(aMem);

            }

            RemoveRange(_rVal);
            return _rVal;
        }

        public void RemoveLast() { if (Count > 0) Remove(Count); }


        public new List<uopProperty> RemoveRange(int aStartID, int aEndID)
        {
            List<uopProperty> _rVal = new List<uopProperty>();
            if (Count <= 0) return _rVal;
            mzUtils.LoopLimits(aStartID, aEndID, 1, Count, out int sid, out int eid);
            List<uopProperty> removers = new List<uopProperty>();
            for (int i = 1; i <= Count; i++)
            {
                if (i >= sid && i <= eid)  removers.Add(Item(i)); 
            }
            foreach (uopProperty item in removers)
            {
                Remove(item);
            }

            return removers;
        }
        /// <summary>
        /// returns true if a new value is stored
        /// </summary>
        /// <param name="aNameOrIndex">the name or index of the property to update</param>
        /// <param name="aReplacement">the property to caopy data from</param>
        /// <param name="bRename">flag to rename the original property to the name of the replacement</param>
        /// <param name="aIndex">updates the indicated member with the data from the passed property</param>
        /// <returns></returns>
        public bool ReplaceProperty(string aName, uopProperty aReplacement,  bool bRename = false)
        {


            if (aReplacement == null) return false;
            uopProperty aMem = null;
            if (!TryGet(aName, out aMem)) return false;
            TPROPERTY bMem = new TPROPERTY(aReplacement);
            TPROPERTY astruc = aMem.Structure;

            if (bRename)
            {
                if (string.Compare(aName, aReplacement.Name) != 0) bMem.Name = aName;
            }
            aMem.Structure = bMem;
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, aMem, aMem.Index - 1));

            Notify(aMem);
            return true;
        }

        /// <summary>
        /// used to reset all or some of the member properties back to their current default values
        /// </summary>
        /// <param name="aStartIndex">the index to begin the procedure (default = 1)</param>
        /// <param name="aEndIndex">the index to end the procedure (default = count)</param>
        public void ResetToDefaults(int aStartIndex = 0, int aEndIndex = 0)
        {
            if (Count <= 0) return;
            mzUtils.LoopLimits(aStartIndex, aEndIndex, 1, Count, out int si, out int ei);
            uopProperty aMem;


            for (int i = si; i <= ei; i++)
            {
                aMem = Item(i);
                if (aMem.HasDefaultValue)
                {
                    aMem.Value = aMem.DefaultValue;
                  
                }
            }

        }

        public void ResetValueChange(int aStartIndex = 0, int aEndIndex = 0)
        {
            if (Count <= 0) return;
            mzUtils.LoopLimits(aStartIndex, aEndIndex, 1, Count, out int si, out int ei);
            uopProperty aMem;


            for (int i = si; i <= ei; i++)
            {
                aMem = Item(i);
                aMem.ValueChanged = false;
           

            }

        }

        public void RestoreValues(uopProperties aChangeBacks = null)
        {
          
            try
            {

                if (_StoredVals == null) _StoredVals = new List<TPROPERTIES>();
                int cnt = _StoredVals.Count();
                if (cnt <= 0) return;

                TPROPERTIES copies = _StoredVals[cnt - 1];
                _StoredVals.RemoveAt(cnt - 1);

                if (copies.Count != Count) return;
                TPROPERTY copy;
                uopProperty myProp;
                for (int i = 1; i <= copies.Count; i++)
                {
                    copy = copies.Item(i);
                    myProp = Item(i);
                    if (string.Compare(copy.Name, myProp.Name, ignoreCase: true) == 0)
                    {
                        if (myProp.SetValue(copy.Value))
                        {
                            Notify(myProp);
                            if (aChangeBacks != null) aChangeBacks.Add(myProp);
                        }
                    }
                }


            }
            catch (Exception exception)
            {
                LoggerManager log = new LoggerManager();
                log.LogError(exception.Message);
            }

        }



        /// <summary>
        /// assigns the passed variant to the Handle property of all the properties in the collection
        /// </summary>
        /// <param name="aCategory"></param>
        /// <param name="aStartIndex"></param>
        /// <param name="aEndIndex"></param>
        public void SetCategories(string aCategory, int aStartIndex = 0, int aEndIndex = 0, uppPartTypes aPartType = uppPartTypes.Undefined)
        {
            if (Count <= 0) return;
            mzUtils.LoopLimits(aStartIndex, aEndIndex, 1, Count, out int si, out int ei);
            uopProperty prop;
            for (int i = si; i <= ei; i++)
            {
                prop = Item(i);
                prop.Category = aCategory;
                if (aPartType != uppPartTypes.Undefined) prop.PartType = aPartType;
            }
        }

        /// <summary>
        /// ^assigns the passed string to the Format string to the indicated properties
        /// </summary>
        /// <param name="aFormatString"></param>
        /// <param name="aStartIndex"></param>
        /// <param name="aEndIndex"></param>
        public void SetFormatString(string aFormatString, int aStartIndex = 0, int aEndIndex = 0)
        {
            if (Count <= 0) return;
            mzUtils.LoopLimits(aStartIndex, aEndIndex, 1, Count, out int si, out int ei);
            uopProperty prop;

            for (int i = si; i <= ei; i++)
            {
                prop = Item(i);
                prop.FormatString = aFormatString;
             
            }

        }
        /// <summary>
        /// ^assigns the passed string to the heading property of all the properties in the collection
        /// </summary>
        /// <param name="aHeadingString"></param>
        /// <param name="aStartIndex"></param>
        /// <param name="aEndIndex"></param>
        public void SetHeadings(string aHeadingString, int aStartIndex = 0, int aEndIndex = 0)
        {
            if (Count <= 0) return;
            mzUtils.LoopLimits(aStartIndex, aEndIndex, 1, Count, out int si, out int ei);
            uopProperty prop;
            for (int i = si; i <= ei; i++)
            {
                prop = Item(i);
                prop.Heading = aHeadingString;
            
            }

        }

        public uopProperty SetHidden(dynamic aNameOrIndex, bool bHidden)
        {
            if (Count <= 0) return null;
            uopProperty aProp;

            if (TryGet(aNameOrIndex, out aProp))
            {
                aProp.Hidden = bHidden;
            
            }
            return aProp;
        }

        /// <summary>
        /// sets the hidden property of the indicated members to the passed boolean value
        /// </summary>
        /// <param name="aHiddenValue">the value to apply to the properties dxfLinetypes.Hidden property</param>
        /// <param name="aStartIndex">the index to begin the procedure (default = 1)</param>
        /// <param name="aEndIndex">the index to end the procedure (default = count)</param>
        public void SetHidden(bool aHiddenValue, int aStartIndex = 0, int aEndIndex = 0)
        {
            if (Count <= 0) return;
            mzUtils.LoopLimits(aStartIndex, aEndIndex, 1, Count, out int si, out int ei);
            uopProperty prop;
            for (int i = si; i <= ei; i++)
            {
                prop = Item(i);
                prop.Hidden = aHiddenValue;
            }
        }

        /// <summary>
        /// assigns the passed string to the heading property of all the properties in the collection
        /// </summary>
        /// <param name="aPartType"></param>
        /// <param name="aStartIndex"></param>
        /// <param name="aEndIndex"></param>
        public void SetPartTypes(uppPartTypes aPartType, int aStartIndex = 0, int aEndIndex = 0)
        {
            if (Count <= 0) return;
            mzUtils.LoopLimits(aStartIndex, aEndIndex, 1, Count, out int si, out int ei);
            uopProperty prop;
            for (int i = si; i <= ei; i++)
            {
                prop = Item(i);
                prop.PartType = aPartType;
            }
        }
        /// <summary>
        /// returns True if any of the properties actual change.
        /// </summary>
        /// <param name="aNameOrIndex">the property to set</param>
        /// <param name="aPropVal">the value to set the property value to</param>
        /// <param name="bMakeDefault">flag to set the default value of the property to the passed value</param>
        /// <param name="bFirstOne">flag indicating that the property only occurs onces</param>
        /// <returns></returns>
        public bool SetProperty(dynamic aNameOrIndex, dynamic aPropVal, bool bMakeDefault = false)
        { return SetProperty(aNameOrIndex, aPropVal, out int IDX, bMakeDefault, 0); }

        /// <summary>
        /// returns True if any of the properties actual change.
        /// </summary>
        /// <param name="aNameOrIndex">the property to set</param>
        /// <param name="aPropVal">the value to set the property value to</param>
        /// <param name="rIndex">sets all the properties in the collection with the passed name to the passed value</param>
        /// <param name="bMakeDefault">flag to set the default value of the property to the passed value</param>
        /// <param name="bFirstOne">flag indicating that the property only occurs onces</param>
        /// <param name="aOccur"></param>
        /// <returns></returns>
        public bool SetProperty(dynamic aNameOrIndex, dynamic aPropVal, out int rIndex, bool bMakeDefault = false, int aOccur = 0)
        {
            rIndex = 0;
         
            bool _rVal = false;
            if (!TryGet(aNameOrIndex, out uopProperty aMem)) return false;
            rIndex = aMem.Index;

            aMem.ValueChanged = false;
            if (bMakeDefault)
            { aMem.SetValue(aPropVal, aDefaultValue: aPropVal); }
            else
            { aMem.SetValue(aPropVal); }
            _rVal = aMem.ValueChanged;
           

            if (_rVal) Notify(aMem);


            return _rVal;
        }


        /// <summary>
        /// assigns the passed string to the heading property of all the properties in the collection
        /// </summary>
        /// <param name="aProtectedValue"></param>
        /// <param name="aStartIndex"></param>
        /// <param name="aEndIndex"></param>
        public void SetProtected(bool aProtectedValue, int aStartIndex = 0, int aEndIndex = 0)
        {
            if (Count <= 0) return;
            mzUtils.LoopLimits(aStartIndex, aEndIndex, 1, Count, out int si, out int ei);
            uopProperty prop;

            for (int i = si; i <= ei; i++)
            {
                prop = Item(i);
                prop.Protected = aProtectedValue;
            
            }

        }


        /// <summary>
        /// assigns all the properties in the collection with the passed name to their default value if one is define
        /// returns True if any of the properties values actual change.
        /// </summary>
        /// <param name="aPropName">the property to set</param>
        /// <param name="bFirstOne">flag indicating that the property only occurs onces</param>
        /// <returns></returns>
        public bool SetToDefault(string aPropName,
            bool bFirstOne = true)
        {

            bool _rVal = false;
            uopProperty aMem;

            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                aMem.ValueChanged = false;

                if (string.Compare(aMem.Name, aPropName, StringComparison.OrdinalIgnoreCase) == 0)
                {

                    if (aMem.HasDefaultValue)
                    {
                        if (aMem.SetValue(aMem.DefaultValue))
                        {

                            _rVal = true;
                            
                            if (aMem.ValueChanged) Notify(aMem);
                            if (bFirstOne) break;
                        }
                    }
                }
            }
            return _rVal;
        }

        /// <summary>
        ///returns the properties from the collection that have a ValueChanged value matching the passed value
        /// </summary>
        /// <param name="aValue">the value to search form</param>
        /// <returns></returns>
        public List<uopProperty> GetByValueChange(bool aValue, bool bGetClones = false)
        {
            List<uopProperty> _rVal = new List<uopProperty>();

            uopProperty aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (aMem.ValueChanged == aValue) _rVal.Add(aMem);
            }
            return _rVal;
        }
        /// <summary>
        /// assigns the passed string to the unit string to the indicated properties
        /// </summary>
        /// <param name="aUnitCaption"></param>
        /// <param name="aStartIndex"></param>
        /// <param name="aEndIndex"></param>
        public void SetUnitCaption(string aUnitCaption, int aStartIndex = 0, int aEndIndex = 0)
        {
            if (Count <= 0) return;
            mzUtils.LoopLimits(aStartIndex, aEndIndex, 1, Count, out int si, out int ei);
            uopProperty prop;

            for (int i = si; i <= ei; i++)
            {
                prop = Item(i);
                prop.SetUnitCaption(aUnitCaption);
              
            }
        }

        /// <summary>
        /// sets the values of the members to the values in the passed list
        /// if the list includes names like "Color=Red,Size=3.2" then then names are used to define the values otherwise
        /// the valus are assigned as the occur in then current list like "Red,3.2" the first memebr will be set to "Red"
        /// </summary>
        /// <param name="aValueString">string of values</param>
        /// <param name="bNamedList">flag indicating that the property names are in the passed list</param>
        /// <param name="aDelimitor">the delimitor that seperates the members of the list</param>
        /// <returns></returns>

          public List<uopProperty> SetValuesByString(string aValueString, bool bNamedList = false, string aDelimitor = ",")
        {

            aValueString = aValueString.Trim();
            List<uopProperty> _rVal = new List<uopProperty>();

            uopProperty aMem;
            string[] sVals;
            string sVal = string.Empty;
            string aName = string.Empty;
            int idx = 0;
            if (aDelimitor !=  string.Empty)
            {
                if (aValueString.IndexOf(aDelimitor, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    sVals = aValueString.Split(aDelimitor.ToCharArray());
                }
                else
                {
                    sVals = new string[1];
                    sVals[0] = aValueString;
                }
            }
            else
            {
                sVals = new string[1];
                sVals[0] = aValueString;
            }
            for (int i = 0; i < sVals.Length; i++)
            {
                sVal = sVals[i].Trim();
                if (bNamedList)
                {
                    mzUtils.SplitString(sVal, "=", out aName, out sVal);
                    if (TryGet(aName, out aMem)) { idx = aMem.Index; } else { idx = 0; }
                }
                else
                {
                    idx = i + 1;
                }

                if (idx > 0 & idx <= Count)
                {
                    aMem = Item(idx);
                    if (aMem.SetValue(sVal))
                    {
                        Notify(aMem);
                        _rVal.Add(aMem);
                    }
                   
                }

            }
            return _rVal;
        }

        public double UnitValue(dynamic aNameOrIndex, bool bReturnMetric, int aPrecis = -1)
        {
            double _rVal = TryGet(aNameOrIndex, out uopProperty mem) ? mem.ValueD : 0;
            if (!mem.Units.IsDefined) return _rVal;
            if (bReturnMetric)
            {
                _rVal = mem.UnitVal(uppUnitFamilies.Metric);
                if (aPrecis < 0) aPrecis = uopUnits.UnitPrecision(mem.UnitType, uppUnitFamilies.Metric);

            }
            else
            {
                mem.UnitVal(uppUnitFamilies.English);
                if (aPrecis < 0) aPrecis = uopUnits.UnitPrecision(mem.UnitType, uppUnitFamilies.English);
            }


            if (aPrecis >= 0) _rVal = Math.Round(_rVal, mzUtils.LimitedValue(aPrecis, 0, 15));
            return _rVal;
        }

        public bool SetUnitValue(dynamic aNameOrIndex, double aValue, bool bMetricPassed)
        {
            if (!TryGet(aNameOrIndex, out uopProperty mem)) return false;
            double multi = 1;
            uopUnit units = mem.Units;
            if (units.IsDefined)
            {
                if (bMetricPassed)
                {
                    multi =  uopUnits.UnitFactor(units.UnitType, uppUnitFamilies.Metric);
                    if (multi != 0) multi = 1 / multi;
                }
            }
            bool _rVal = mem.SetValue(aValue * multi);
            if (_rVal == true) Notify(mem);
            return _rVal;

        }

        public bool SetValue(dynamic aNameOrIndex, dynamic aValue)
        {
           
            if (TryGet(aNameOrIndex, out uopProperty mem) == true)
            {
                bool _rVal = mem.SetValue(aValue);
                if (_rVal ) Notify(mem);
                return _rVal;
            }

            return false;
        }

        public bool SetDefaultValue(dynamic aNameOrIndex, dynamic aValue, bool bSetNullVal = false)
        {

            if (TryGet(aNameOrIndex, out uopProperty mem) == true)
            {
                bool _rVal = mem.DefaultValue != mem.DefaultValue;
                mem.SetValue(mem.Value, aValue, bSetNullVal ? aValue: null);
                return _rVal;
            }

            return false;
        }

        public bool SetValueD(dynamic aNameOrIndex, double aValue, dynamic aMultiplier = null)
        {

            if (!TryGet(aNameOrIndex, out uopProperty mem)) return false;
            if (aMultiplier != null) aValue *= mzUtils.VarToDouble(aMultiplier);
            bool _rVal = mem.SetValue(aValue);
            if (_rVal ) Notify(mem);
            return _rVal;

        }

        public string Signatures(string aDelimitor = ",", bool bUnitSignatures = false,
            uppUnitFamilies aUnitFamily = uppUnitFamilies.Default)
        {

            uopProperty aProp;
            string aStr = string.Empty;
            string rStr = string.Empty;
            rStr = string.Empty;
            for (int i = 1; i <= Count; i++)
            {
                aProp = Item(i);
                if (rStr !=  string.Empty) rStr += aDelimitor;

                aStr = bUnitSignatures ? aProp.UnitSignature(aUnitFamily, true) : aProp.Signature(true, true);

                rStr += aStr;

            }
            return rStr;
        }

        public string StoreValues(bool bClearExisting)
        {
            if (Count <= 0) return "";
            string _rVal = StringVals(uopGlobals.Delim, bIncludePropertyNames: true);
            if (_StoredVals == null) _StoredVals = new List<TPROPERTIES>();
            if (bClearExisting) _StoredVals.Clear();
            _StoredVals.Add(new TPROPERTIES(this));

            return _rVal;
        }
        public uopProperties StoreValuesGet(int aIndex = 1, bool bRemove = false)
        {
            if (_StoredVals == null) return Clone(true);
            int idx = _StoredVals.Count() - aIndex;
            if(idx < 0 ) return Clone(true);

            TPROPERTIES copies = _StoredVals[idx];
            if(bRemove)_StoredVals.RemoveAt(idx);
             return new uopProperties(copies);

        }

        public bool StoredValuesClear() { bool _rVal = (_StoredVals != null) && _StoredVals.Count() > 0; _StoredVals = new List<TPROPERTIES>(); return _rVal; }


        /// <summary>
        /// 5an optional string to put before anf after the values in the returned string
        /// returns the values of properties in the collection in a comma (or other deliminator) string
        /// </summary>
        /// <param name="aDeliminator">1an optional delimator</param>
        /// <param name="aStartIndex">the index to begin the procedure (default = 1)</param>
        /// <param name="aEndIndex">the index to end the procedure (default = count)</param>
        /// <param name="bIncludePropertyNames">flag to return the properties complete signatures in the returned string</param>
        /// <param name="aWrapper">an optional string to put before anf after the values in the returned string</param>
        /// <param name="bShowDecodedValue"></param>
        /// <returns></returns>
        public string StringVals(string aDeliminator = ",", int aStartIndex = 0, int aEndIndex = 0, bool bIncludePropertyNames = false, string aWrapper = "", bool bShowDecodedValue = false)
        {
            string _rVal = string.Empty;
            if (Count <= 0) return _rVal;
            mzUtils.LoopLimits(aStartIndex, aEndIndex, 1, Count, out int si, out int ei);

            uopProperty aProp;
            string dlm = string.Empty;
            dynamic aVal = null;
            string sVal = string.Empty;
            dlm = aDeliminator.Trim();
            if (dlm ==  string.Empty) dlm = ",";

            for (int i = si; i <= ei; i++)
            {
                aProp = Item(i);
                aVal = aProp.Value;
                sVal = bShowDecodedValue ? aProp.DecodedValue : Convert.ToString(aVal);

                if (bIncludePropertyNames) sVal = aProp.Name + "=" + sVal;

                if (aWrapper !=  string.Empty)
                {
                    if (sVal.IndexOf(aWrapper, StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        sVal = sVal.Replace(aWrapper, "");
                    }
                    sVal = aWrapper + sVal + aWrapper;
                }
                mzUtils.ListAdd(ref _rVal, sVal, dlm, true, ",", bAllowNulls: true);
            }

            return _rVal;
        }



        public void SubPart(uopPart aPart, string aPropertiesName)
        {
            if (aPart == null) return;

            ProjectHandle = aPart.ProjectHandle;
            _RangeGUID = aPart.RangeGUID;
            PartType = aPart.PartType;
            _PartName = aPart.PartName;
            _PartPath = aPart.GetPartPath();
            _PartIndex = aPart.Index;

            foreach (var item in this)
            {
                item.SubPart(this);

            }
        }

        public List<uopProperty> SubSet(int aStartID, int aEndID)
        {
            List<uopProperty> _rVal = new List<uopProperty>();
            int aSID = aStartID;
            int aEID = aEndID;
            int idx = 0;
            mzUtils.SortTwoValues(true, ref aSID, ref aEID);
            for (int i = 1; i <= Count; i++)
            {
                idx = i + 1;
                if (idx >= aSID && idx <= aEID)
                { _rVal.Add(Item(i)); }
            }
            return _rVal;
        }


        /// <summary>
        /// sets the value of the indicated members to the new value is it's current value is equal to the passed value
        /// returns true if the value changes
        /// </summary>
        /// <param name="aNameOrIndex">the name or index of the subject propert</param>
        /// <param name="aValue">test value</param>
        /// <param name="aNewValue">the new value</param>
        /// <param name="bStringCompare">flag to test by a non-casesensitive string comparison</param>
        /// <param name="aPrecis"> numerical precisions to apply to</param>
        /// <param name="aElseValue">value to apply if the current value does NOT match the test value</param>
        /// <param name="rIndex"></param>
        /// <returns></returns>
        public bool ThisThenThis(dynamic aNameOrIndex, dynamic aValue,
            dynamic aNewValue, bool bStringCompare,
            int aPrecis, dynamic aElseValue, out int rIndex)
        {
           
            rIndex = 0;
            if (Count <= 0) return false; 
            uopProperty aProp;
            if (!TryGet(aNameOrIndex, out aProp))  return false; 
            rIndex = aProp.Index;
            return aProp.ThisThenThis(aValue, aNewValue, bStringCompare, aPrecis, aElseValue);
            
       
        }
        /// <summary>
        /// ^returns all the unit captions of of all the properties
        /// </summary>
        /// <param name="aUnits"></param>
        /// <returns></returns>
        public List<string> UnitCaptions(uppUnitFamilies aUnits = uppUnitFamilies.Default)
        {
            List<string> _rVal = new List<string>();
            for (int i = 1; i <= Count; i++)
            {
                _rVal.Add(Item(i).UnitCaption(aUnits));
            }
            return _rVal;
        }

        /// <summary>
        /// returns the string value of the item from the collection at the requested index or the passed name
        /// </summary>
        /// <param name="aNameOrIndex"></param>
        /// <param name="bRaiseNotFoundError"></param>
        /// <returns></returns>
        public string StringValue(dynamic aNameOrIndex)
        {
        
            if (!TryGet(aNameOrIndex, out uopProperty aMem)) return "";
            if (!aMem.HasValue) return "";
            return Convert.ToString(aMem.Value);
        }

        /// <summary>
        /// returns the value of the item from the collection at the requested index or the passed name
        /// </summary>
        /// <param name="aNameOrIndex"></param>
        /// <param name="bSupressNotFoundError"></param>
        /// <param name="rFound"></param>
        /// <returns></returns>
        public dynamic Value(dynamic aNameOrIndex) => TryGet(aNameOrIndex, out uopProperty aMem) ? aMem.Value : null;

        public dynamic DisplayValue(dynamic aNameOrIndex) => TryGet(aNameOrIndex, out uopProperty aMem) ? aMem.DisplayValue : null;

        public double DisplayUnitValue(dynamic aNameOrIndex) => TryGet(aNameOrIndex, out uopProperty aMem) ? aMem.DisplayUnitValue : 0;
        public string DisplayValueString(dynamic aNameOrIndex, bool bZeroAsNullString = false, bool bAbsVal = false, dynamic aZeroValue = null) => TryGet(aNameOrIndex, out uopProperty aMem) ? aMem.GetDisplayValueString(bZeroAsNullString, bAbsVal, aZeroValue) : "";

        public bool SetDisplayUnitValue(dynamic aNameOrIndex, dynamic aUnitValue)
        {
            if (!TryGet(aNameOrIndex, out uopProperty aMem)) return false;
            uopUnit units = aMem.Units;
            bool _rVal = units.IsDefined ? _rVal = aMem.SetValue(units.ConvertValue(aUnitValue, aMem.DisplayUnits)) : aMem.SetValue(aUnitValue);
            
            if (_rVal) Notify(aMem);
            return _rVal;
        }
        /// <summary>
        ///returns the 'double' value of the item from the collection at the requested index or the passed name
        /// </summary>
        /// <param name="aNameOrIndex"></param>
        /// <param name="aDefault"></param>
        /// <returns></returns>
        public double ValueD(dynamic aNameOrIndex, double aDefault = 0, dynamic aMultiplier = null, int aPrecis = -1)
        {
           
            double _rVal = TryGet(aNameOrIndex, out uopProperty mem) ? mem.ValueD : aDefault;
            if (aMultiplier != null && aMultiplier != 0) _rVal *= mzUtils.VarToDouble(aMultiplier);
            if (aPrecis >= 0) _rVal = Math.Round(_rVal, mzUtils.LimitedValue(aPrecis, 0, 15));
            return _rVal;
        }
        /// <summary>
        ///returns the 'integer' value of the item from the collection at the requested index or the passed name
        /// </summary>
        /// <param name="aNameOrIndex"></param>
        /// <param name="aDefault"></param>
        /// <returns></returns>
        public int ValueI(dynamic aNameOrIndex, int aDefault = 0) => TryGet(aNameOrIndex, out uopProperty mem) ? mem.ValueI : aDefault;

        /// <summary>
        ///returns the 'bool' value of the item from the collection at the requested index or the passed name
        /// </summary>
        /// <param name="aNameOrIndex"></param>
        /// <param name="aDefault"></param>
        /// <returns></returns>
        public bool ValueB(dynamic aNameOrIndex, bool aDefault = false) => TryGet(aNameOrIndex, out uopProperty mem) ? mem.ValueB : aDefault;

        /// <summary>
        ///returns the 'string' value of the item from the collection at the requested index or the passed name
        /// </summary>
        /// <param name="aNameOrIndex"></param>
        /// <param name="aDefault"></param>
        /// <returns></returns>
        public string ValueS(dynamic aNameOrIndex, string aDefault = "", bool formatted = false)
        {

            string _rVal = string.IsNullOrEmpty(aDefault) ? string.Empty : aDefault;
            if (!TryGet(aNameOrIndex, out uopProperty mem)) return _rVal;

            if (mem.HasValue)
            { 
                if(mem.Value is string)
                {
                    _rVal = mem.Value as string;

                }
                else
                {
                    _rVal = (!formatted) ? mem.ValueS : mem.FormatedString;
                }
            }
            
            return _rVal;
        

        }


        /// <summary>
        /// writes the signatures of the properties to the VB debug window
        /// </summary>
        /// <param name="aHeader">optional Heading text to write before the properties</param>
        /// <param name="bShowHeadings"></param>
        /// <param name="bShowDecodedValue"></param>
        public void WriteToDebug(string aHeader = "",
            bool bShowHeadings = false, bool bShowDecodedValue = false) => TPROPERTIES.WriteToDebug(new TPROPERTIES(this), aHeader, bShowHeadings, bShowDecodedValue);

        /// <summary>
        /// writes the signatures of the properties to the passed text file
        /// </summary>
        /// <param name="aTStream"><the text stream to wite the properties to/param>
        /// <param name="aHeader">optional Heading text to write to the file before the properties</param>
        /// <param name="bShowDecodedValue"></param>
        public void WriteToFile(System.IO.TextWriter aTStream, string aHeader = "", bool bShowDecodedValue = false)
        {
            if (aTStream == null) return;
            aHeader = string.IsNullOrWhiteSpace(aHeader) ? this.NodeName : aHeader.Trim();
            if (string.IsNullOrWhiteSpace(aHeader)) aHeader = "PROPERTIES";
            aTStream.WriteLine(aHeader);
            uopProperty aMem;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                aTStream.WriteLine(aMem.Signature(bShowDecodedValue));
            }

        }

        /// <summary>
        /// writes the collection of properties to an INI file formated text file
        /// </summary>
        /// <param name="aFileSpec">the file name to create</param>
        /// <param name="bSuppressHiddenProperties">causes any property marked as hidden to be ommited from the file</param>
        /// <param name="aDefaultHeading">optional default section heading text to write properties without headings to</param>
        /// <param name="bForceToDefault">flag to force all properties to be written to the default heading</param>
        public void WriteToINIFile(string aFileSpec, bool bSuppressHiddenProperties = true,
             string aDefaultHeading = "DATA", bool bForceToDefault = false, bool bEnumsToInts = true) => uopProperties.WriteToINIFile(this, aFileSpec, bSuppressHiddenProperties, aDefaultHeading, bForceToDefault, false, bEnumsToInts);

        //public IEnumerator<uopProperty> GetEnumerator()
        //{
        //    return ((IEnumerable<uopProperty>)_Members).GetEnumerator();
        //}

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    return ((IEnumerable)_Members).GetEnumerator();
        //}

        #endregion Methods


        #region Shared Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aProps">the subject properties</param>
        /// <param name="aFileSpec">the file name to create</param>
        /// <param name="bSuppressHiddenProperties">causes any property marked as hidden to be ommited from the file</param>
        /// <param name="aDefaultHeading">optional default section heading text to write properties without headings to</param>
        /// <param name="bForceToDefault">flag to force all properties to be written to the default heading</param>
        /// <param name="bSuppressErrors">writes the collection of properties to an INI file formated text file</param>
        public static void WriteToINIFile(uopProperties aProps, string aFileSpec, bool bSuppressHiddenProperties = true, string aDefaultHeading = "DATA", bool bForceToDefault = false, bool bSuppressErrors = false, bool bEnumsToInts = true)
        {

            if (aProps == null) return;
            if (aProps.Count <= 0) return;

            uopProperty aMem;
            string dhead;
            string phead;
            dynamic pval;
            string propcolname = aProps.Name;
            propcolname ??= string.Empty;
            bool doneit = false;
            aDefaultHeading ??= string.Empty;
            dhead = aDefaultHeading.Trim();
            if (dhead ==  string.Empty) dhead = "DATA";
           
            int z = 0;
            for (int i = 1; i <= aProps.Count; i++)
            {
                aMem = aProps.Item(i);
                pval = aMem.Value;

                if (!bSuppressHiddenProperties || (bSuppressHiddenProperties && !aMem.Hidden))
                {
                    if (!bForceToDefault)
                    {
                        phead = aMem.Heading;
                        if (string.IsNullOrWhiteSpace(phead)) phead = propcolname;
                        if (string.IsNullOrWhiteSpace(phead )) phead = dhead;
                        
                        phead = phead.ToUpper();
                    }
                    else
                    {
                        phead = dhead;
                    }
                    if (pval is Enum && bEnumsToInts)
                    {
                        pval = (int)aMem.Value;
                    }


                    string value = Convert.ToString(pval);

                    doneit = uopUtils.WriteINIString(aFileSpec, phead.ToUpper(), aMem.Name, !string.IsNullOrEmpty(value) ? value : string.Empty);
                    z++;
                    if (!doneit && !bSuppressErrors)
                    {
                        throw new Exception("[TPROPERTIES.WriteToINIFile] INI Write failed.");
                    }
                }
            }
        }
        #endregion Shared Methods
    }

    public class uopPropertyArray: IEnumerable<uopProperties>
    {
        #region Constructors
        public uopPropertyArray() {Clear(); }
        public uopPropertyArray(List<uopProperties> aProps) 
        { 
            _Members = new List<uopProperties>();
            AddRange(aProps);
        }

        public uopPropertyArray(uopProperties aProps, string aName = null, string aHeading = null, bool bAddClone = false) { _Members = new List<uopProperties>(); Add(aProps, aName,aHeading,bAddClone); }

        #endregion Constructors

        #region Properties
        public int Count => Members.Count;
        public string Name { get; set; }


        private List<uopProperties> _Members;
        public List<uopProperties> Members { get { _Members ??= new List<uopProperties>();return  _Members; } set { _Members ??= new List<uopProperties>(); if (value == null) { _Members.Clear(); } else { _Members = value; } } }

        public List<string> Headings
        {
            get
            {
                List<string> _rVal = new List<string>();
                if (Count <= 0) return _rVal;
                foreach (uopProperties item in Members) { _rVal.Add(item.Name); }
                return _rVal;
            }
        }

        public List<uopProperties> ToList => Members;
        #endregion Properties


        #region Methods

        public uopProperties Item(int aIndex)
        {
            if (aIndex < 0 || aIndex > Count) throw new IndexOutOfRangeException();
            return Members[aIndex - 1];
        }

        public uopProperties Item(string aName)
        {
            if (string.IsNullOrWhiteSpace(aName) || Count <= 0) return null;
            return Find(x => string.Compare(x.Name, aName, true) == 0);
        }

        public bool SetValue(string aHeading,string aPropertyName, dynamic aValue)
        {
            uopProperties member = Item(aHeading);
            return member == null ? false : member.SetProperty(aPropertyName, aValue);
        }
        public dynamic Value(string aHeading, string aPropertyName, out bool rFound, dynamic aDefault = null)
        {
            rFound = false;
            dynamic _rVal = aDefault;
            uopProperties member = Item(aHeading);
            rFound = member != null;
            if (!rFound) return _rVal;
            if(member.TryGet(aPropertyName,out uopProperty prop))
            {
                rFound = true;
                _rVal = prop.Value;
            }
            return _rVal;
        }
        
        public dynamic Value(string aHeading, string aPropertyName, dynamic aDefault = null)
        {
            uopProperties member = Item(aHeading);
            return member == null ? aDefault : member.Value(aPropertyName);
        }

        public string ValueS(string aHeading, string aPropertyName, out bool rFound, string aDefault = "")
        {
          
            string _rVal = aDefault;
            uopProperties member = Item(aHeading);
            rFound = member != null;
            if (!rFound) return _rVal;
            rFound = false;
            if (member.TryGet(aPropertyName, out uopProperty prop))
            {
                rFound = true;
                _rVal = prop.ValueS;
            }
            
            return _rVal;
        }

        public string ValueS(string aHeading, string aPropertyName, string aDefault = "") 
        {
            uopProperties member = Item(aHeading);
            return member == null ? aDefault : member.ValueS(aPropertyName,aDefault);
        }

        public int ValueI(string aHeading, string aPropertyName, out bool rFound, int aDefault = 0)
        {
            rFound = false;
            int _rVal = aDefault;
            uopProperties member = Item(aHeading);
            rFound = member != null;
            if (!rFound) return _rVal;
            if (member.TryGet(aPropertyName, out uopProperty prop))
            {
                rFound = true;
                _rVal = prop.ValueI;
            }
            return _rVal;
        }

        public int ValueI(string aHeading, string aPropertyName, int aDefault = 0)
        {
            uopProperties member = Item(aHeading);
            return member == null ? aDefault : member.ValueI(aPropertyName, aDefault);
        }
        public double ValueD(string aHeading, string aPropertyName, out bool rFound, double aDefault = 0d)
        {
            rFound = false;
            double _rVal = aDefault;
            uopProperties member = Item(aHeading);
            rFound = member != null;
            if (!rFound) return _rVal;
            if (member.TryGet(aPropertyName, out uopProperty prop))
            {
                rFound = true;
                _rVal = prop.ValueD;
            }
            return _rVal;
        }

        public double ValueD(string aHeading, string aPropertyName, double aDefault = 0)
        {
            uopProperties member = Item(aHeading);
            return member == null ? aDefault : member.ValueD(aPropertyName, aDefault);
        }

        public bool ValueB(string aHeading, string aPropertyName, out bool rFound, bool aDefault = false)
        {
            rFound = false;
            bool _rVal = aDefault;
            uopProperties member = Item(aHeading);
            rFound = member != null;
            if (!rFound) return _rVal;
            if (member.TryGet(aPropertyName, out uopProperty prop))
            {
                rFound = true;
                _rVal = prop.ValueB;
            }
            return _rVal;
        }

        public bool ValueB(string aHeading, string aPropertyName, bool aDefault = false)
        {
            uopProperties member = Item(aHeading);
            return member == null ? aDefault : member.ValueB(aPropertyName, aDefault);
        }

        public void Add(uopProperty aProperty, string aMemberName, string aHeading = null, bool bAddClone = false, bool bAddIfNotFound = true)
        {
            if (aProperty == null||  string.IsNullOrWhiteSpace(aMemberName) ) return;
            if (bAddClone) aProperty = aProperty.Clone();
            

            uopProperties member = Item(aMemberName);
            if (member == null)
            {
                if (!bAddIfNotFound) return;
                member = new uopProperties() { Name = aMemberName };
                Add(member);

            }
           aProperty = member.Add(aProperty);
            if (!String.IsNullOrWhiteSpace(aHeading))
                aProperty.Heading = aHeading;
            else
                if (string.IsNullOrWhiteSpace(aProperty.Heading)) aProperty.Heading = aMemberName;
        }
        public void Append(uopProperties aProperties, string aMemberName, string aHeading = null, bool bAddClone = false, bool bAddIfNotFound = true)
        {
            if (string.IsNullOrWhiteSpace(aMemberName) && aProperties != null) aMemberName = aProperties.Name;
            if (aProperties == null || string.IsNullOrWhiteSpace(aMemberName)) return;
            if (bAddClone) aProperties = aProperties.Clone();
            foreach (var item in aProperties)
            {
                Add(item, aMemberName, aHeading, false, bAddIfNotFound);
            }

        }

        public void Add(uopProperties aMember, string aName = null, string aHeading = null, bool bAddClone = false, bool bAppendToExisting = true)
        {
            if (aMember == null) return;
            if (Members.IndexOf(aMember) >= 0) bAddClone = true;
            if (bAddClone) aMember = aMember.Clone();

                if (string.IsNullOrWhiteSpace(aName)) aName = aMember.Name;
            aMember.Name = aName;
            if (string.IsNullOrWhiteSpace(aMember.Name))
            {
                int i = Count + 1;
                string name = $"PROPERTIES_{i}";
                while (Item(name) != null)
                {
                    i++;
                    name = $"PROPERTIES_{i}";
                }
                aMember.Name = name;
                

            }
            if (!string.IsNullOrWhiteSpace(aHeading))
            {
                aMember.SetHeadings(aHeading);
            }
            if (bAppendToExisting)
            {
                uopProperties mymember = Item(aMember.Name);
                if(mymember != null)
                {
                    mymember.Append(aMember);
                    return;
                }
            }

            Members.Add(aMember);
        }

        public  uopPropertyArray PropertiesStartingWith(string aStartsWith)
        {

            uopPropertyArray _rVal = new uopPropertyArray() { Name = Name };
            if (string.IsNullOrWhiteSpace(aStartsWith)) return new uopPropertyArray() { Name = Name }; 
            return new uopPropertyArray(FindAll(x => x != null && x.Name.ToUpper().StartsWith(aStartsWith.ToUpper()))) { Name = Name };

        }
        public uopProperties SubPropertiesStartingWith(string aHeading, string aStartsWith)
        {

            uopProperties member = Item(aHeading);
            if (member == null) return new uopProperties();
            if (string.IsNullOrWhiteSpace(aStartsWith)) return new uopProperties();
            List<uopProperty> mems = member.FindAll(x => x.Name.ToUpper().StartsWith(aStartsWith.ToUpper()));
            return new uopProperties(mems, member);
        }

        public void AddRange(List<uopProperties> aRange, bool bAddClones = false)
        {
            if (aRange == null) return;
            foreach (uopProperties item in aRange)
            {
                if (item != null) Add(item, bAddClone: bAddClones);
         
            }
        }

        public void Append(uopPropertyArray aPropArray, bool bAddClones = false)
        {
            if (aPropArray == null) return;
            foreach (uopProperties item in aPropArray)
            {
                Add(item, bAddClone: bAddClones);
            }


        }
        public uopProperties Find(Predicate<uopProperties> match) => Members.Find(match);

        public int FindIndex(Predicate<uopProperties> match) => Members.FindIndex(match) + 1;
        public List<uopProperties> FindAll(Predicate<uopProperties> match) => Members.FindAll(match);

        public bool TryGet(string aHeading,out uopProperties rMember)
        {
            rMember = Item(aHeading);
            return rMember != null;
        }

        public bool TryGetValue(string aHeading, string aPropertyName, out dynamic rValue)
        {
            rValue = string.Empty;
            if (!TryGet(aHeading, out uopProperties aMember)) return false;
            if (!aMember.TryGet(aPropertyName, out uopProperty prop)) return false;
            rValue = prop.Value;
            return true;
        }

        public bool Contains(string aHeading) => !string.IsNullOrWhiteSpace(aHeading) && Count > 0 && FindIndex(x => x != null && string.Compare(x.Name, aHeading, true) == 0) > 0;

        public bool Contains(string aHeading, string aPropertyName) => Contains(aHeading) && Find(x => x! == null && string.Compare(x.Name, aHeading, true) == 0).Contains(aPropertyName) ;

        public void Clear() => Members.Clear();
        public void ReadFromINIFile(string aFileSpec, bool bClear = true, bool bRemoveQuotes = false)
        {
            if (bClear) 
                Clear();
            if (string.IsNullOrWhiteSpace(aFileSpec)) return;
            if (!File.Exists(aFileSpec)) return;
            List<string> lines = File.ReadAllLines(aFileSpec, encoding: System.Text.Encoding.UTF7).ToList();
            List<string> headings = new List<string>();
            List<string> rlines = new List<string>();
            string line;
            string heading = string.Empty;
            foreach (var item in lines)
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    line = item.Trim();
                    if(line.StartsWith("[") && line.EndsWith("]"))
                    {
                        heading = line.Substring(1, line.Length - 2).Trim();
                        if (!string.IsNullOrWhiteSpace(heading))
                        {

                            uopProperties member = Item(heading);
                            if(member == null)
                            {
                                member = new uopProperties(heading) { Name = heading };
                                Add(member);
                                
                            }
                            headings.Add(member.Name);
                        }
                      

                    }
                    else
                    {
                        int i = line.IndexOf('=');
                        if (i > 0 && !string.IsNullOrWhiteSpace(heading))
                        {
                            uopProperties member = Item(heading);
                            if (member != null)
                            {
                                string pname = line.Substring(0, i ).Trim();
                                
                                if(!string.IsNullOrWhiteSpace(pname))
                                {
                                    string value = line.Substring(i + 1, line.Length - i - 1).Trim();
                                    char[] chars = value.ToCharArray();
                                    if (value.Length > 0 && bRemoveQuotes)
                                    {
                                      
                                        if ((int)chars[0] == 34) 
                                            value = value.Substring(1, value.Length - 1);
                                        if ((int)chars[chars.Length - 1] == 34) 
                                            value = value.Substring(0, value.Length - 1);


                                    }

                                    uopProperty prop = new uopProperty(pname, value);
                                    member.Add(prop);
                                
                                }
                            }
                        }
                    }
                    rlines.Add(item.Trim());
                }
            }

        }
        
        public override string ToString()
        {
            string _rVal = $"uopPropertyArray [{ Count }]";
            if (!string.IsNullOrWhiteSpace(Name)) _rVal += $" - { Name }";
            return _rVal;
        }



        public void WriteToINIFile(string aFileSpec, bool bSuppressHiddenProperties = true,
            string aDefaultHeading = "DATA", bool bForceToDefault = false, bool bEnumsToInts = true)
        {

            foreach (uopProperties item in Members)
            {
                uopProperties.WriteToINIFile( item, aFileSpec, bSuppressHiddenProperties, aDefaultHeading, bForceToDefault, bEnumsToInts);
             }

        }


        public IEnumerator<uopProperties> GetEnumerator()
        {
            return ((IEnumerable<uopProperties>)_Members).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_Members).GetEnumerator();
        }
        #endregion Methods
    }
}

