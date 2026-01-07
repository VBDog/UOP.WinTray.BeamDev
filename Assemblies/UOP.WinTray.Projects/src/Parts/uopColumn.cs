using System;
using System.Collections.Generic;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Events;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using static System.Math;

namespace UOP.WinTray.Projects.Parts
{
    /// <summary>
    // represents a distillation column in a Cross Flow project
    // A project can have multiple columns defined.
    // Projects columns are accessed through its Columns collection property.
    // Columns have collections of Tray ranges defined.
    /// </summary>
    public class uopColumn : uopPart, IEventSubscriber<Message_ColumnRequest>
    {

        public override uppPartTypes BasePartType => uppPartTypes.Column;
      

       
        //@raised when the column configuration data changes
        public delegate void ColumnPropertyChange(uopProperty aProperty);
        public event ColumnPropertyChange eventColumnPropertyChange;
        public delegate void RangeCountChangeHandler(int NewCount);
        public event RangeCountChangeHandler eventRangeCountChange;

        #region Constructors 

        public uopColumn() : base(uppPartTypes.Column, uppProjectFamilies.uopFamMD, "","",false) =>  InitializeProperties();
            

        public uopColumn(uopProject aProject) : base(uppPartTypes.Column, uppProjectFamilies.uopFamMD, "", "", false)
        {

            
            InitializeProperties();
            SubPart(aProject);

        }

        internal uopColumn(uopColumn aPartToCopy ) : base(uppPartTypes.Column, uppProjectFamilies.uopFamMD, "", "", false) => InitializeProperties(aPartToCopy);


        private void InitializeProperties(uopColumn aPartToCopy = null)
        {
           if(base.ActiveProps.Count <= 0 || _TrayRanges == null)
            {
  
                Index = 1;
                base.ActiveProps.Clear();
                AddProperty("ManholeID", 0!, uppUnitTypes.SmallLength);
                AddProperty("RingThickness", 0!, uppUnitTypes.SmallLength);
                AddProperty("ColumnLetter", "");
                AddProperty("Name", "");
                _TrayRanges = new colUOPTrayRanges();

                _Handle = mzUtils.CreateGUID();

                _TrayRanges.eventRangeAdded += _TrayRanges_RangeAdded;
                _TrayRanges.eventRangeRemoved += _TrayRanges_RangeRemoved;
                uopEvents.Aggregator.Subscribe(this);
                _TrayRanges.SubPart(this);
            }
            if (aPartToCopy != null)
            {
                TraySortOrder = aPartToCopy.TraySortOrder;
                Copy(aPartToCopy);
                for (int i = 1; i <= aPartToCopy.TrayRanges.Count; i++)
                {
                    _TrayRanges.Add((uopTrayRange)aPartToCopy.TrayRanges.Item(i).Clone());
                }
            }
        }

        #endregion Constructors 


        #region Properties

        private string _Handle;
        public string Handle { get => _Handle ; set { _Handle = value;  } }

        public override string ColumnHandle { get => Handle; set => Handle = value; }

        public override int ColumnIndex { get => Index; set => Index = value; }

        /// <summary>
        /// design letter(s) to distinguish a particular column (for part numbering)
        /// </summary>
        public override string ColumnLetter { get => PropValS("ColumnLetter"); set => Notify(PropValSet("ColumnLetter", value)); }

        /// <summary>
        /// a descriptive name assigned to the column
        /// </summary>
        public override string Name { get  => PropValS("Name"); set => Notify(PropValSet("Name", value)); }

      

        public override uopColumn Column => this;

    

        /// <summary>
        /// ^returns True if any of the tray ranges in the column have anti-penetration pans defined
        /// </summary>
        public bool HasAntiPenetrationPans => TrayRanges.HasAntiPenetrationPans;

        public override string INIPath => $"COLUMN({ColumnIndex})";

        /// <summary>
        /// ^the controls the which tray is the top tray in the columns Ranges
        /// </summary>
        private uppTraySortOrders _TraySortOrder = uppTraySortOrders.TopToBottom;
        public uppTraySortOrders TraySortOrder { get => _TraySortOrder; set { if (_TraySortOrder == value) return; _TraySortOrder = value; TrayRanges.TraySortOrder = value; } }

        /// <summary>
        /// ^the ring thickness for the column
        /// </summary>
        public new double RingThickness { get => PropValD("RingThickness");  set => Notify(PropValSet("RingThickness", Math.Abs(value))); }

        /// <summary>
        /// ^the manhole ID for the column
        /// </summary>
        public override double ManholeID { get => PropValD("ManholeID"); set => Notify(PropValSet("ManholeID", Math.Abs(value))); }

        
        /// <summary>


        /// <summary>
        /// ^returns a comma deliminated string containing the ring numbers that are currently
        /// ^ocupied by a tray range within the column
        /// ~like 1,2,3,5,7,9,4,6,8, used to determine if a ring is in use
        /// </summary>
        public string OccupiedRings => TrayRanges.GetRingString();

        /// <summary>
        /// ^the number of rings in the column
        /// ~Not currently used for anything.
        /// ~Potentially used to make sure all rings have a tray defined.
        /// </summary>
        public int RingCount
        {
            get
            { 
                if (_TrayRanges.Count == 0) return 0;
                int _rVal = 0;
                for (int i = 1; i <= _TrayRanges.Count; i++)
                { _rVal += _TrayRanges.Item(i).TrayCount; }
                return _rVal;
            }
        }

        private colUOPTrayRanges _TrayRanges = null;

        /// <summary>
        /// ^the defined tray ranges for the column
        /// </summary>
        public new colUOPTrayRanges TrayRanges 
        { 
            get 
            {
                if (_TrayRanges == null) 
                    InitializeProperties();
                _TrayRanges.SubPart(this);
                return _TrayRanges;
            } 
        }
        #endregion Properties


        #region Methods

        internal override TCOLUMN ColumnStructure() { return new TCOLUMN() { Properties = ActiveProps }; }

        public uopColumn Clone() => new uopColumn(this);

        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();
        /// <summary>
        ///^returns the objects properties in a collection
        ///~signatures like "COLOR=RED"
        /// </summary>
        public override uopProperties CurrentProperties() { UpdatePartProperties(); return base.CurrentProperties(); }
        public override void UpdatePartProperties() { base.DescriptiveName = "COLUMN"; }

        public override void Destroy()
        {
            base.Destroy();

            _TrayRanges.eventRangeAdded -= _TrayRanges_RangeAdded;
            _TrayRanges.eventRangeRemoved -= _TrayRanges_RangeRemoved;
            _TrayRanges.Destroy();
            _TrayRanges = null;

        }

        public List<mdTrayRange> GetMDTrays(uppMDDesigns aDesignType = uppMDDesigns.Undefined)
        {
            List < mdTrayRange > _rVal  = new List<mdTrayRange>();
            foreach (uopTrayRange item in TrayRanges)
            {
                if(item.ProjectFamily == uppProjectFamilies.uopFamMD)
                {
                    if (aDesignType == uppMDDesigns.Undefined || item.DesignFamily == aDesignType)
                    {
                        _rVal.Add((mdTrayRange)item);
                    }

                }
            }

            return _rVal;
        }


        public bool SetManholeID(double aManholeID, bool bApplyToAllRanges = true, List<string> GUIDSToSkip = null, bool bSuppressEvents = false)
        {
            colUOPTrayRanges ranges = TrayRanges;
            if (ranges.Count <= 0 || aManholeID < 0) return false;


            uopTrayRange mem;
            bool doit = GUIDSToSkip == null;
            bool memchng;
            uopProperty prop = PropValSet("ManholeID", aManholeID,  bSuppressEvnts: bSuppressEvents);
            bool _rVal = prop != null;
            if (bApplyToAllRanges)
            {
                for (int i = 1; i <= ranges.Count; i++)
                {
                    mem = ranges.Item(i);
                    if (GUIDSToSkip != null)
                    {
                        doit = true;
                        for (int j = 1; j <= GUIDSToSkip.Count; j++)
                        {
                            if (string.Compare(mem.GUID, GUIDSToSkip[j - 1], ignoreCase: true) == 0)
                            {
                                doit = false;
                                break;
                            }
                        }
                    }
                    if (doit)
                    {
                       prop = mem.PropValSet("ManholeID", 0, bSuppressEvnts: bSuppressEvents);
                        memchng = prop != null;
                        if (memchng) _rVal = true;
                    }
                }
            }
            return _rVal;

        }

        public bool SetRingThickness(double aRingThk, bool bApplyToAllRanges = true, List<string> GUIDSToSkip = null, bool bSuppressEvents = false)
        {
            colUOPTrayRanges ranges = TrayRanges;
            if (ranges.Count <= 0 || aRingThk < 0) return false;


            uopTrayRange mem;
            bool doit = GUIDSToSkip == null;
            bool memchng;
            uopProperty prop  =PropValSet("RingThickness", aRingThk, bSuppressEvnts: bSuppressEvents);
            bool _rVal = prop != null;
            if (bApplyToAllRanges)
            {
                for (int i = 1; i <= ranges.Count; i++)
                {
                    mem = ranges.Item(i);
                    if (GUIDSToSkip != null)
                    {
                        doit = true;
                        for (int j = 1; j <= GUIDSToSkip.Count; j++)
                        {
                            if (string.Compare(mem.GUID, GUIDSToSkip[j - 1], ignoreCase: true) == 0)
                            {
                                doit = false;
                                break;
                            }
                        }
                    }
                    if (doit)
                    {
                        prop = mem.PropValSet("RingThk", 0,  bSuppressEvnts: bSuppressEvents);
                        memchng = prop != null;
                        if (memchng) _rVal = true;
                    }
                }
            }
            return _rVal;

        }
        /// <summary>
        ///returns and compiles all the warnings from all the tray ranges in the column
        /// </summary>
        public override uopDocuments Warnings() => TrayRanges.GenerateWarnings(Project,"");
       
        /// <summary>
        ///returns the properties required to save the column to file
        /// signatures like "COLOR=RED"
        /// </summary>
        public override  uopPropertyArray SaveProperties(string aHeading = null)
        {

            aHeading = string.IsNullOrWhiteSpace(aHeading) ? INIPath : aHeading.Trim();

            uopProperties _rVal = base.SaveProperties(aHeading).Item(1);
            _rVal.Add("RangeCount", TrayRanges.Count, aHeading: INIPath);
            _rVal.Add("SelectedRange", _TrayRanges.SelectedRangeIndex, aHeading: INIPath);
            return new uopPropertyArray( _rVal, aName: aHeading, aHeading: aHeading);
        }

    
        public uopTrayRange GetRangeByRingNumber(int RingNumber)
        {
            uopTrayRange _rVal = null;
            //#1the ring number to search for
            //^returns the column tray range that includes the passed ring number

            RingNumber = Abs(RingNumber);
            if (RingNumber == 0) return null;
            

            uopTrayRange Range ;

            colUOPTrayRanges Ranges = TrayRanges;

            for (int i = 1; i <= Ranges.Count; i++)
            {
                Range = Ranges.Item(i);
                if (RingNumber >= Range.RingStart && RingNumber <= Range.RingEnd)
                {
                    if (Range.StackPattern == uppStackPatterns.Continuous)
                    {
                        _rVal = Range;
                        break;
                    }
                    else
                    {
                        if (Range.StackPattern == uppStackPatterns.Even)
                        {
                            if (uopUtils.IsEven(RingNumber))
                            {
                                _rVal = Range;
                                break;
                            }
                        }
                        else
                        {
                            if (uopUtils.IsOdd(RingNumber))
                            {
                                _rVal = Range;
                                break;
                            }
                        }
                    }
                }

              
            }
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
               


                uopPropertyArray myprops = aFileProps.PropertiesStartingWith(aFileSection);
                if (myprops.Count <= 0)
                {
                    ioWarnings?.AddWarning(this, $"{PartName} Data Not Found", $"File '{ aFileProps.Name }' Does Not Contain {aFileSection} Info!");
                    return;
                }

                base.ReadProperties(aProject, myprops, ref ioWarnings, aFileVersion, aFileSection, bIgnoreNotFound);
                Reading = true;


                double rngThk = 0;
                uopTrayRange aRng;
                double thk;
                ioWarnings ??= new uopDocuments();
                _TrayRanges.eventRangeAdded -= _TrayRanges_RangeAdded;
                _TrayRanges.eventRangeRemoved -= _TrayRanges_RangeRemoved;

                int tcount = aFileProps.ValueI(aFileSection, "TrayCount", out bool found);
                _TrayRanges.Clear();
                _TrayRanges.SubPart(this);
                _TrayRanges.ColumnIndex = Index;
  
                
                SetNotes(aFileProps.SubPropertiesStartingWith(aFileSection, "NOTE"));

                string FSec = $"{aFileSection}";

                aProject?.ReadStatus($"Extracting { aFileSection } Tray Ranges");

                _TrayRanges.ReadProperties(aProject, myprops, ref ioWarnings, aFileVersion, FSec, bIgnoreNotFound);
         
                _TrayRanges.SetSelected(myprops.ValueI(aFileSection, "SelectedRange", 1));
                if (rngThk <= 0)
                {
                    for (int i = 1; i <= _TrayRanges.Count; i++)
                    {
                        aRng = _TrayRanges.Item(i);
                        thk = aRng.RingThk;
                        if (thk > 0)
                        {
                            rngThk = thk;
                            PropValSet("RingThickness", thk, bSuppressEvnts: true);
                            break;
                        }
                    }
                    if (rngThk > 0)
                    {
                        for (int i = 1; i <= _TrayRanges.Count; i++)
                        {
                            aRng = _TrayRanges.Item(i);
                            thk = aRng.RingThk;
                            if (thk <= 0)
                            {
                                aRng.PropValSet("RingThk", rngThk, bSuppressEvnts: true);
                                break;
                            }
                        }
                    }
                }

                

                //return _rVal;

            }
            catch (Exception e) 
            {

                ioWarnings?.AddWarning(this, "Read Properties Error", e.Message);

            }
            finally 
            {
                _TrayRanges.SortByRingStart();
                _TrayRanges.eventRangeAdded += _TrayRanges_RangeAdded;
                _TrayRanges.eventRangeRemoved += _TrayRanges_RangeRemoved;

                Reading = false; 
            }
        }
        
        public override void SubPart(uopProject aProject, string aCategory = null, bool? bHidden = null) 
        {
            base.SubPart(aProject, aCategory,bHidden);
            if (aProject != null)
            {
                TraySortOrder = aProject.ReverseSort ? uppTraySortOrders.BottomToTop : uppTraySortOrders.TopToBottom;
             
                
            } 
              
            _TrayRanges?.SubPart(aProject);
        }

        public override void UpdatePartWeight() { TrayRanges.UpdatePartWeight();  base.Weight = TrayRanges.Weight ; }

        public override bool IsEqual(uopPart aPart)
        {
            if (aPart == null) return false;
            if (aPart.PartType !=  PartType) return false;

            return IsEqual((uopColumn)aPart);
        }


        public bool IsEqual(uopColumn aPart)
        {
            if (aPart == null) return false;
            if (aPart.PartType !=  PartType) return false;
            if (!aPart.Properties.IsEqual(Properties)) return false;
            colUOPTrayRanges rngs = TrayRanges;
            colUOPTrayRanges cngs = aPart.TrayRanges;

            if (cngs.Count != rngs.Count) return false;
            for (int i = 1; i <= rngs.Count; i++)
            {
                if (!rngs.Item(i).IsEqual(cngs.Item(i))) return false;
            }
            return true;
        }

    
        public void _TrayRanges_RangeAdded()
        {
            if (! SuppressEvents)
            {
                Notify(uopProperty.Quick("RangeCount", _TrayRanges.Count, _TrayRanges.Count - 1, this));
                eventRangeCountChange?.Invoke(_TrayRanges.Count);
            }
        }

        /// <summary>
        /// Creates new TPROPERTY
        /// </summary>
        /// <param name="aName"></param>
        /// <param name="aValue"></param>
        /// <param name="aLastValue"></param>
        /// <returns></returns>        
        public void _TrayRanges_RangeRemoved(uopTrayRange aRange)
        {
            string RangeCount = string.Empty;
            if (! SuppressEvents)
            {
                Notify(uopProperty.Quick("RangeCount", _TrayRanges.Count, _TrayRanges.Count + 1, this));
                eventRangeCountChange?.Invoke(_TrayRanges.Count);
            }
        }
        /// <summary>
        /// used by objects above this object in the object model to alert a sub object that a property above it in the object model has changed.
        /// alerts the objects below it of the change
        /// </summary>
        /// <param name="aProperty"></param>
        public void Alert(uopProperty aProperty)
        {
            if (aProperty == null) return;
            TrayRanges.Alert(aProperty);
        }

        public void Notify(uopProperty aProperty)
        {
            //used by an object to respond to property changes of itself and of objects below it in the object model.
            //also raises the changed property to the object above it the object model.
            if (null == aProperty)
                return;

            aProperty.ProjectHandle =  ProjectHandle;

            string pname = aProperty.Name.ToUpper();
            uppPartTypes pType = aProperty.PartType;
            uopProject Proj = Project;

           if (null != Proj) Proj.Notify(aProperty);

            eventColumnPropertyChange?.Invoke(aProperty);

            if (pType == uppPartTypes.Column)
                TrayRanges.Alert(aProperty);
        }

        public override uopProperty CurrentProperty(string aPropertyName, bool bSupressNotFoundError = false) { UpdatePartProperties(); return base.CurrentProperty(aPropertyName, bSupressNotFoundError); }

        /// <summary>
        /// make sure all persistent sub parts are up to date
        /// </summary>
        public  void UpdatePersistentSubParts(uopProject aProject,  bool bForceRegen = false, string aRangeGUID = null)
        {
            aProject ??= Project;
            TrayRanges.UpdatePersistentSubParts(aProject, bForceRegen, aRangeGUID);

        }

        #endregion Methods
   
        #region Messages
        void IEventSubscriber<Message_ColumnRequest>.OnAggregateEvent(Message_ColumnRequest message)
        {
            if (string.Compare(message.ColumnHandle, Handle, ignoreCase: true) == 0)
                message.Column = this;
        }
        #endregion Messages

    }
}




