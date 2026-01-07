using System;
using System.Collections.Generic;
using System.Linq;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Parts;



namespace UOP.WinTray.Projects
{
    public class uopPartArray :List<colUOPParts>
    {
        #region Constructors

        public uopPartArray() { base.Clear(); OwnerGUID = string.Empty; PartType = uppPartTypes.Undefined; }

        public uopPartArray(uppPartTypes aPartType, string aOwnerGUID, colUOPParts aParts  = null) 
        {
            base.Clear(); 
            OwnerGUID = !string.IsNullOrWhiteSpace(aOwnerGUID) ? aOwnerGUID.Trim() : ""; 
            PartType = aPartType;
            if (aParts != null) Add(aParts);
        }

        #endregion Constructors


        #region Properties

        public uppPartTypes PartType { get; set; }

        public string OwnerGUID { get; set; }

        public bool InvalidWhenEmpty { get; set; }

        private bool _Invalid = false;
        public bool Invalid
        {
            get => (InvalidWhenEmpty && Count <= 0) || _Invalid;
            set => _Invalid = value;
        }
        #endregion Properties


        #region Methods
     
        public colUOPParts Item(int aIndex)
        {
            if (aIndex < 1 || aIndex > Count) throw new IndexOutOfRangeException();
            return base[aIndex - 1];
        }

        public bool TryGetParts(uppPartTypes aPartType, out colUOPParts rParts)
        {
            rParts = Item(aPartType);
            if(rParts != null)
            {
                if (rParts.Count <= 0) rParts = null;
            }
            return rParts != null;

        }
        public bool TryGetEqualPart(uopPart aPart, out uopPart rPart)
        {
            rPart = null;
            if (aPart == null) return false;
            if (!TryGetParts(aPart.PartType, out colUOPParts parts)) return false;
            rPart = parts.Find(x => x.IsEqual(aPart));
            return rPart != null;

        }
        public colUOPParts Item(uppPartTypes aPartType, bool bAddIfNotFound = false)
        {
            colUOPParts _rVal = base.Find(x => x.PartType == aPartType);
            if(bAddIfNotFound && _rVal == null)
            {
                _rVal = Add( new colUOPParts(aPartType, new List<uopPart>(), uppProjectFamilies.uopFamMD));
            }
            return _rVal;
        }
        public uopPart GetPart(uppPartTypes aPartType, int aPartIndex)
        {
            colUOPParts parts = Item(aPartType);
            if (parts == null) return null;
            return (aPartIndex > 0 && aPartIndex <= parts.Count) ? parts.Item(aPartIndex) : null;
        }

        

        public new colUOPParts Add(colUOPParts aParts)
        {
            if (aParts == null) return null;
            //if (aParts.Count <= 0) return;
            aParts.MaintainIndices = true;
            base.Add(aParts);
            return aParts;   
            
        }
        public void Replace(colUOPParts aMember, colUOPParts aReplacement)
        {
            if (aMember == null || aReplacement == null || Count <= 0) return;
            int idx = base.IndexOf(aMember);
            if (idx < 0) return;
            base[idx] = aReplacement;
        }

        public new int IndexOf(colUOPParts aMember) => base.IndexOf(aMember) + 1;
        
        public void Append(colUOPParts aParts,bool bAddClone = false)
        {
            if (aParts == null) return;
            if (aParts.Count <= 0) return;

            foreach (var item in aParts)
            {
                colUOPParts member = Item(item.PartType);
                if(member == null)
                {
                    member = new colUOPParts(bBaseOne: true, bMaintainIndices: true) { PartType = item.PartType };
                    base.Add(member);

                }
                member.Add(item, bAddClone);
            }
        }


        #endregion Methods

        #region Shared Methods

        #endregion Shared Methods
    }

  
}
