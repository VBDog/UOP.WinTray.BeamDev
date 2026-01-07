using UOP.DXFGraphics;
using System;
using System.Collections;
using System.Collections.Generic;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Events;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using System.Linq;




namespace UOP.WinTray.Projects
{

    public class mdSlotZone :mdSlotGrid,  ICloneable
    {


        #region Constructors

        public mdSlotZone()  =>Init(); 
        public mdSlotZone(mdSlotZone aZone) => Init(aZone); 
          
        public mdSlotZone(uopSectionShape aBaseShape, mdDeckSection aDeckSection = null, mdTrayAssembly aAssy = null, double? aHPitch = null, double? aVPitch = null, bool bUpdateSlots = false)  => Init(null,aBaseShape, aDeckSection, aAssy, aHPitch, aVPitch, bUpdateSlots);


        private void Init(mdSlotZone aZoneToCopy = null, uopSectionShape aBaseShape = null, mdDeckSection aDeckSection = null, mdTrayAssembly aAssy = null, double? aHPitch = null, double? aVPitch = null, bool bUpdateSlots = false)
        {
            if (aDeckSection == null && aZoneToCopy != null) aDeckSection = aZoneToCopy.DeckSection;

            BaseShape = uopSectionShape.CloneCopy(aBaseShape);
            if(aDeckSection != null && BaseShape == null)  BaseShape = uopSectionShape.CloneCopy(aDeckSection.BaseShape);
     
            _DeckSectionRef = aDeckSection == null ? null : new WeakReference<mdDeckSection>(aDeckSection);
            TrayAssembly = aAssy;


            if (aZoneToCopy != null) Copy(aZoneToCopy);
            if(aHPitch.HasValue) HPitch = aHPitch.Value;
            if (aVPitch.HasValue) VPitch = aVPitch.Value;
            base.Zone = this;
            if (bUpdateSlots && HPitch !=0 && VPitch != 0 && aDeckSection != null && BaseShape != null) 
                this.UpdateSlotPoints(aAssy, false, true);

        }

        public void Copy(mdSlotZone aZone)
        {
            if (aZone == null) return;

            base.Copy(aZone); // to get the grid

            base.TrayAssembly = aZone.TrayAssembly;
            DeckSection = aZone.DeckSection;
            ProjectHandle = aZone.ProjectHandle;
            RangeGUID = aZone.RangeGUID;
         
            Index = aZone.Index;
        
       
            VisualHandles = null;
         
   
            if (aZone.VisualHandles != null)
            {
                VisualHandles = new List<ulong>();
                foreach (ulong item in aZone.VisualHandles)
                {
                    VisualHandles.Add(item);
                }
            }


            
        }

        #endregion Constructors

        #region Properties

     
        public uopVector GridOrigin { get =>  base.Origin; }
        
        public List<ulong> VisualHandles { get; set; }


        
        public List<double> SlotAngles  { get { UpdateSlotPoints(); return base.Angles; }}

  

        private WeakReference<mdDeckSection> _DeckSectionRef;
        public mdDeckSection DeckSection
        {
            get
            {
                if (_DeckSectionRef == null) return null;
                if (!_DeckSectionRef.TryGetTarget(out mdDeckSection _rVal)) _DeckSectionRef = null;
                return _rVal;
            }
            set
            {
                if (value == null)
                    _DeckSectionRef = null;
                else
                {
                    RangeGUID = value.RangeGUID;
                    _DeckSectionRef = new WeakReference<mdDeckSection>(value);
                }
                    
            }
        }

  
  
  

        /// <summary>
        /// returns the properties required to save the zone to file
        /// </summary>
        public uopProperties SaveProperties
        {
            get
            {
                uopProperties _rVal = new uopProperties
                {
                    { "XPitch", HPitch },
                    { "YPitch", VPitch },
                    { "PitchType", PitchType }
                };
                return _rVal;
            }
        }

        public string SectionHandle =>Handle;

        #endregion Properties

        #region Methods
        /// <summary>
        /// updates the current grid points if the need updating
        /// </summary>
        /// <param name="bRegen">flag to force a regen</param>
        /// <returns></returns>
        public override bool UpdateGridPoints(bool bRegen = false) => UpdateSlotPoints(bForceRegen: bRegen);





        internal uopVectors PerimeterPts(mdTrayAssembly aAssy)
        {

            uopVectors _rVal = uopVectors.Zero;
            UpdateSlotPoints(aAssy, true);

            foreach (uopVector item in _GridPts.FindAll(x => !x.Suppressed))
            {
                uopVector u1 = new uopVector(item);
                uopVector u2 = new uopVector(item) { X = item.X - 0.38 };
                u2.Rotate(u1, item.Value);
                _rVal.Add(u2);

            }

            return _rVal;
        }


        public uopProperties GridPointProperties(int aPrecis = 5, uopProperties aCollector = null, int aMaxLength = 0)
        {
            uopProperties _rVal;
         
            int j = 0;
            string aStr = string.Empty;
            string pname = string.Empty;
            UpdateSlotPoints();
            aPrecis = mzUtils.LimitedValue(aPrecis, 1, 8);
            _rVal = aCollector ?? new uopProperties();
            
            j = 0;
            pname = $"ZONE({ SectionHandle }).GRIDPOINTS";
            foreach (var item in GridPts)
            {
                mzUtils.ListAdd(ref aStr, $"({ Math.Round(item.X, aPrecis) },{ Math.Round(item.Y, aPrecis) },{ Math.Round(item.Value, 1) },{ Convert.ToInt32(item.Suppressed) })", null, true, uopGlobals.Delim);
                if (aMaxLength > 0 && aStr.Length > aMaxLength)
                {
                    _rVal.Add(pname, aStr);
                    aStr = string.Empty;
                    j += 1;
                    pname = $"ZONE({ SectionHandle }).GRIDPOINTS({ j })";
                }
            }
          
            if (aStr.Length > 0) _rVal.Add(pname, aStr);
            return _rVal;
        }


        
       /// <summary>
        /// returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        public new mdSlotZone Clone() => new mdSlotZone(this);

        object ICloneable.Clone() => new mdSlotZone(this);

        public bool IsEqual(mdSlotZone aZone, bool bComparePoints = false)
        {
           
            if (aZone == null || aZone.PitchType != PitchType || Math.Round(aZone.HPitch, 5) != Math.Round(HPitch, 5) || Math.Round(aZone.VPitch, 5) != Math.Round(VPitch, 5))
                return false;
            if (!bComparePoints) return true;


            uopVectors aGPs = base.GridPoints();
            uopVectors bGPs = base.Zone.GridPoints();
            if (aGPs.Count != bGPs.Count) return false;
              
            dxfPlane aPl = new dxfPlane(new dxfVector(X,Y));
            dxfPlane bPl = new dxfPlane(new dxfVector(aZone.X, aZone.Y)); ;
             
            for (int i = 1; i <= aGPs.Count; i++)
            {
                uopVector u1 = aGPs[i].WithRespectToPlane(aPl);
                uopVector u2 = bGPs[i].WithRespectToPlane(bPl);
                if (Math.Round(u1.X, 4) != Math.Round(u2.X, 4))
                    return false;
                if (Math.Round(u1.Y, 4) != Math.Round(u2.Y, 4))
                    return false;

            }
            return true;
        }

    

         

        public bool UpdateSlotPoints(mdTrayAssembly aAssy = null, bool bMaintainSuppressed = true, bool bForceRegen = false)
        {
            //, List<double> rAngles = null, mdDeckSection aDeckSection = null//
            if (RegenSlots || bForceRegen || _GridPts == null || base.RectangleCollector == null)
            {

                if (aAssy != null) 
                    aAssy.RaiseStatusChangeEvent($"Generating {aAssy.TrayName()} Deck Section {Handle} ECMD Slot Centers");
                if (!bMaintainSuppressed) Clear();
                Generate();
                return true;
            }
            return false;
        }

  public override string ToString() => string.IsNullOrWhiteSpace(Name) ? $"mdSlotZone[{Vertices.Count}]" : $"mdSlotZone[{Vertices.Count}] '{Name}'";

        /// <summary>
        /// reads the properties of the zone member from a text file in INI file format
        /// </summary>
        /// <param name="aFileData">the file data to read the member zone properties from</param>
        /// <param name="aFileSection">the section in the file to read the data from</param>
        /// <returns></returns>
        public bool ReadProperties(uopPropertyArray aFileData, string aFileSection = "", mdTrayAssembly aAssy = null)
        {
            Invalid = true;
            TPROPERTIES aProps = new TPROPERTIES();
            if (aFileData == null)
            {
                RegenSlots = true;
                return false;
            }

            string aStr = $"ZONE({ SectionHandle })";
            aStr = aFileData.ValueS( aFileSection, aStr, "");

            if (aStr !=  string.Empty)
            {
                
                aProps = aProps.AddByString(aStr, ",", true);
                HPitch = aProps.ValueD("XPITCH",HPitch);
                VPitch = aProps.ValueD("YPITCH", VPitch);
                //PitchType = (dxxPitchTypes)aProps.ValueI( "PitchType", (int)_Struc.PitchType);
                this.RegenSlots = true;
                UpdateSlotPoints(aAssy, bForceRegen:true);
                return true;
            }else
            { return false; }

        }



        #endregion Methods


        #region Shared Methods

        public static mdSlotZone CloneCopy(mdSlotZone aZone) => aZone == null ? null : new mdSlotZone( aZone);
        #endregion Shared Methods
    }

}