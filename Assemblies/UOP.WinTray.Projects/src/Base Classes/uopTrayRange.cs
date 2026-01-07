using UOP.DXFGraphics;
using System;
using System.Collections.Generic;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Events;
using UOP.WinTray.Projects.Materials;
using UOP.WinTray.Projects.Parts;

using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.Projects
{
    public abstract class uopTrayRange : uopPart , IEventSubscriber<Message_RangeRequest>
    {

        public uopTrayRange(uppProjectFamilies aFamily, string aProjectHandle = "") : base(uppPartTypes.TrayRange, aFamily, aProjectHandle) 
        {
            uopEvents.Aggregator.Subscribe(this);
            _Elevation = new uopElevation();
            ColumnLetter = string.Empty;

        }


        private uopElevation _Elevation;
        public uopElevation Elevation { get => _Elevation; set => _Elevation = value ?? new uopElevation(); }
        
        public abstract uopTrayAssembly Assembly { get; }

        public abstract double RingThk { get; set; }

        public abstract double BoltBarThk { get; set; }

        public override uopTrayRange TrayRange => this;

        public abstract new  uopSheetMetal DeckMaterial { get; set; }
        public abstract uopSheetMetal BeamMaterial { get; set; }

        public override string ColumnLetter { get; set; }


        public override uopHardwareMaterial HardwareMaterial
        {
            get => Assembly.HardwareMaterial;

            set { Assembly.HardwareMaterial = value; base.HardwareMaterial = Assembly.HardwareMaterial; }
        }

        /// <summary>
        /// ^the controls the which tray is the top tray in the columns Ranges
        /// </summary>
        private uppTraySortOrders _TraySortOrder = uppTraySortOrders.TopToBottom;
        public uppTraySortOrders TraySortOrder { get => _TraySortOrder; internal set => _TraySortOrder = value; }

        public override uopRingRange RingRange => new uopRingRange(this);

        public abstract  mdDowncomer Downcomer { get; }

        private string _GUID;
        public  string GUID  { get => _GUID; internal set { _GUID = value; base.RangeGUID = value; }  }

        public override string RangeGUID { get => GUID; set => GUID = value; }


        public override int Index { get => base.Index; set { base.Index = value;  } }

    public abstract double OverrideRingClearance { get; set; }

        public abstract double RingClipRadius { get; }

        public abstract colUOPParts Rings { get; }

        public virtual bool SinglePass { get => true; }

        public override string DisplayName { get => TrayName(true); set => base.DisplayName = TrayName(true); }

        public override string UniqueID { get => GUID; set => base.UniqueID = GUID; }

        public abstract List<int> StageIndices { get; }

        public virtual bool HasAntiPenetrationPans { get => false; }
        public bool ContainsRingIndex(int testRing, bool bIncludeOddEven = true)
        {

            return uopUtils.SpanContainsIndex(testRing, RingStart, RingEnd, StackPattern, bIncludeOddEven);
         

        }

        public string SpanLabel => SpanName(true,  TrayCount > 1 ? "Trays " : "Tray ");

        public abstract string TrayTypeName { get; }

        public override string SelectName { get => Name(true); }

        public abstract string WarningPath { get; }

 
        internal override TTRAYRANGE RangeStructure()
        {
            TTRAYRANGE _rVal = new TTRAYRANGE()
            {
                GUID = _GUID,
                Properties = ActiveProps
            };


            return _rVal;

        }

        public bool IsSymmetric
        {
            get 
            { 
                uopTrayAssembly assy = Assembly;
                if(ProjectFamily== uppProjectFamilies.uopFamMD)
                {
                    if(DesignFamily == uppMDDesigns.MDDesign || DesignFamily == uppMDDesigns.ECMDDesign)
                    {
                        mdTrayAssembly mdassy = (mdTrayAssembly)assy;
                        return mdassy.OddDowncomers;
                    }
                    return false;
                }
                else
                {
                    throw new NotImplementedException();
              
                }
              
            }
        }

        public abstract dxfRectangle Rectangle(double aScaler = 0);

        public abstract void Alert(uopProperty aProperty);



        public abstract void ImportProperties(uopProject aProject, string aFileSpec, string aFileSection, int inRingStart, int inRingEnd, ref uopDocuments ioWarnings);

        public new abstract string Name(bool bIncludeDesignIndicator = false);

    
        public abstract void Notify(uopProperty aProperty);

     
        public abstract bool ReadyToDraw(out string ErrString);


        public abstract void ResetParts();

        public abstract colUOPParts SpliceAngles(bool bExcludeManwaySplices = false, bool bUpdateQuantities = false, colUOPParts aCollector = null); 

        public abstract uopDocuments GenerateWarnings(uopProject aProject, string aCategory = null, uopDocuments aCollector = null, bool bJustOne = false);

        /// <summary>
        /// make sure all persistent sub parts are up to date
        /// </summary>
        public abstract void UpdatePersistentSubParts(uopProject aProject, bool bForceRegen = false);

        void IEventSubscriber<Message_RangeRequest>.OnAggregateEvent(Message_RangeRequest message)
        {
            if (string.Compare(message.RangeGUID, GUID, ignoreCase: true) == 0)
                message.Range = this;
        }

           
    }
}
