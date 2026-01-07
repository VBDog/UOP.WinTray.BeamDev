using System;
using System.Collections.Generic;
using System.Diagnostics;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Materials;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Structures;

namespace UOP.WinTray.Projects
{
    public abstract class uopTrayAssembly :uopPart

    {

        public delegate void StatusChangeHandler(string StatusString, string aSubStatus = null);
        public event StatusChangeHandler eventStatusChange;

        public uopTrayAssembly(uppProjectTypes aType,string aProjectHandle = "", string aRangeGUID = "")  :base(uppPartTypes.TrayAssembly,uppProjectFamilies.Undefined, aProjectHandle:  aProjectHandle, aRangeGUID: aRangeGUID, bIsSheetMetal:false)
        {
            _Elevation = new uopElevation();
        
            ColumnLetter = string.Empty;
        }

        
        public override string ColumnLetter { get; set; }

        private uopElevation _Elevation;
        public uopElevation Elevation { get => _Elevation; set => _Elevation = value ?? new uopElevation(); }

        public abstract uopDocuments GenerateWarnings(mdProject aProject, uopDocuments aCollector = null, string aCategory = "", bool bJustOne = false);
        public abstract uopSheetMetal BeamMaterial { get; }

        public abstract List<uopDeckSplice>Splices { get; } 

        public override double ManholeID { get => base.ManholeID; set => base.ManholeID = value; }
        public override abstract uopSheetMetal DeckMaterial { get; }
        //the hardware material assigned to the part
        private new uopHardwareMaterial _HardwareMaterial;
        public override uopHardwareMaterial HardwareMaterial
        {
            get {
                if(_HardwareMaterial == null)
                    _HardwareMaterial = new uopHardwareMaterial(TMATERIAL.DefaultHardware(), this); 
               
                base.HardwareMaterial = _HardwareMaterial;
                //Console.WriteLine(_HardwareMaterial.Descriptor);
                return _HardwareMaterial; }
        

            set
            {
                if (value == null) value = new uopHardwareMaterial(TMATERIAL.DefaultHardware(), this);
                _HardwareMaterial = value;
                base.HardwareMaterial = value;
            }
        }

        public abstract uppSpliceStyles SpliceStyle { get; }

        public abstract double JoggleAngleHeight { get; }


        public virtual bool HasAlternateDeckParts { get; }
        public override uopRingRange RingRange => new uopRingRange(this);

      
        public override uopTrayRange TrayRange
        {
            get => base.TrayRange;
            set => SubPart(value);
        }

        /// <summary>
        /// the deck object of the assembly
        /// </summary>
        public abstract uopPart DeckObj { get; }

        /// <summary>
        /// the downcomer object of the assembly
        /// </summary>
        public abstract uopPart DowncomerObj { get; }

        /// <summary>
        /// the downcomer object of the assembly
        /// </summary>
        public abstract uopPart RecieveingPanObj { get; }

        /// <summary>
        /// the deck object of the assembly
        /// </summary>
        public abstract uopParts DeckPanelsObj { get; }



        /// <summary>
        /// the deck beam of the assembly (if there is one defined)
        /// </summary>
        //todo
        public virtual uopPart DeckBeamObj { get; }

        /// <summary>
        /// the deck beam collection of the assembly (if there is one defined)
        /// </summary>
        ///todo
        public virtual uopParts DeckBeams { get; }

     
        public override abstract bool DoubleNuts { get; }
      

        /// <summary>
        /// the tray object of the assembly (if there is one defined)
        /// </summary>
        public abstract uopPart DesignOptionsObj { get; }

        

        /// <summary>
        /// a string describing the dewsign family of the assembly
        /// </summary>
        public abstract string FamilyName { get; }

        /// <summary>
        /// flag indicating if the assembly has anti-pen pans
        /// </summary>
        public abstract bool HasAntiPenetrationPans { get; }


        /// <summary>
        /// the integral beam of the assembly (if there is one defined)
        /// </summary>
        public virtual uopPart IntegralBeam { get; }

        /// <summary>
        /// the number of manways in the assembly
        /// </summary>
        public abstract int ManwayCount { get; }




        /// <summary>
        /// ^the controls the which tray is the top tray in the columns Ranges
        /// </summary>
        private uppTraySortOrders _TraySortOrder = uppTraySortOrders.TopToBottom;
        public uppTraySortOrders TraySortOrder { get => _TraySortOrder; internal set => _TraySortOrder = value; }


        /// <summary>
        /// the tray object of the assembly (if there is one defined)
        /// </summary>
        public virtual uopPart Tray { get; }

        /// <summary>
        /// the weir object of the assembly (if there is one defined)
        /// </summary>
        public virtual uopPart Weir { get; }


        private uppStackPatterns _StackPattern = uppStackPatterns.Continuous;
        public override uppStackPatterns StackPattern { get => _StackPattern; set { _StackPattern = value; base.StackPattern = value; } }
        /// <summary>
        /// used by objects above this object in the object model to alert a sub object that a property above it in the object model has changed.
        /// </summary>
        /// <param name="aProperty"></param>
        public abstract void Alert(uopProperty aProperty);

        public virtual void RaiseStatusChangeEvent(string aStatus, string aSubStatus = null, bool? bBegin = null)
        {
            eventStatusChange?.Invoke(aStatus,aSubStatus);
            //SetPartGenerationStatus(aStatus, bBegin);
            if (Reading) SetReadStatus(aStatus);
        }

        /// <summary>
        ///returns the tray assebly of the range below this one in the stack
        /// </summary>
        /// <returns></returns>
        public virtual uopTrayAssembly GetTrayBelow() { return null; }

        /// <summary>
        /// used by an object to respond to changes to its own properties and of properties of objects below it in the object model.
        /// </summary>
        /// <param name="aProperty"></param>
        public abstract void Notify(uopProperty aProperty);
       

        /// <summary>
        /// used to terminate the sub-objects of the assembly so they will be regenerated on the next request
        /// </summary>
        public abstract void ResetComponents();

        public abstract double PanelWidth();

        public abstract double JoggleBoltSpacing { get;  }

        public abstract double TabSpacing(bool bVertical, out double rWidth, out int rCount);

        public override void SubPart(uopTrayRange aRange, string aCategory = null, bool? bHidden = null)
        {
            
            try
            {
                base.SubPart(aRange);
                if (aRange == null) return;
                DesignFamily = aRange.DesignFamily;
                TraySortOrder = aRange.TraySortOrder;
                Elevation = new uopElevation(aRange.Elevation);
                ManholeID = aRange.ManholeID;
                ColumnLetter = aRange.ColumnLetter;
            }
            catch (Exception e)
            {
                Debug.Fail(e.Message);
            }
           
        }

    }
}