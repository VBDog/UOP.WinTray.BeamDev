using System;
using System.Collections.Generic;
using UOP.WinTray.Projects.Enums;

namespace UOP.WinTray.Projects.Parts
{
    public class colUOPParts : uopParts

    {
        public override uppPartTypes BasePartType => uppPartTypes.Parts;

        public delegate void PartsInvalidatedHHandler();
        public event PartsInvalidatedHHandler EventPartsInvalidated;

        //public delegate void PartsClearedHandler();
        //public event PartsClearedHandler EventPartsCleared;

        public delegate void PartAddedHandler(uopPart NewPart);
        public event PartAddedHandler EventPartAdded;

        public delegate void PartRemovedHandler(uopPart RemovedPart);
        public event PartRemovedHandler EventPartRemoved;



        #region Constructors

        public colUOPParts(bool bBaseOne = true, uppProjectFamilies aProjectFamily = uppProjectFamilies.uopFamMD, bool bMaintainIndices = true, bool bInvalidWhenEmpty = false) : base(uppPartTypes.Parts, aProjectFamily, bBaseOne: bBaseOne, bMaintainIndices: bMaintainIndices, bInvalidWhenEmpty: bInvalidWhenEmpty)
        { }

        public colUOPParts() : base(uppPartTypes.Parts, uppProjectFamilies.Undefined, bBaseOne: true, bMaintainIndices: false)
        { }

        public colUOPParts(IEnumerable<uopPart> aParts, uopPart aParent) : base(uppPartTypes.Parts, uppProjectFamilies.uopFamMD, bBaseOne: true, bMaintainIndices: false)
        {
            if (aParts != null) Append(aParts);
            if (aParent != null) SubPart(aParent);
        }

        public colUOPParts(IEnumerable<uopPart> aParts, IEnumerable<uopPart> bParts, uopPart aParentPart) : base(uppPartTypes.Parts, uppProjectFamilies.uopFamMD, bBaseOne: true, bMaintainIndices: false)
        {
            if (aParts != null) Append(aParts);
            if (bParts != null) Append(bParts);
            if (aParentPart != null) SubPart(aParentPart);
        }

        public colUOPParts(uopPart aParentPart) : base(uppPartTypes.Parts, uppProjectFamilies.uopFamMD, bBaseOne: true, bMaintainIndices: false)
        { if (aParentPart != null) SubPart(aParentPart); }

        public colUOPParts(uopPart aBasePart, uopPart aParentPart) : base(uppPartTypes.Parts, uppProjectFamilies.uopFamMD, bBaseOne: true, bMaintainIndices: false)
        {
            if (aBasePart != null) Add(aBasePart);
            if (aParentPart != null) SubPart(aParentPart);
        }

        public colUOPParts(uppPartTypes aPartType, IEnumerable<uopPart> aParts, uppProjectFamilies aProjectFamily = uppProjectFamilies.uopFamMD, bool bBaseOne = false, bool bMaintainIndices = false, bool bInvalidWhenEmpty = false, uopPart aParentPart = null) : base(aParts, false)
        {
            PartType = aPartType;
            MaintainIndices = bMaintainIndices;
            BaseOne = bBaseOne;
            
            InvalidWhenEmpty = bInvalidWhenEmpty;
            if (aParentPart != null) SubPart(aParentPart);
        }


        public colUOPParts(uopPart aBasePart, uopPart bBasePart, uopPart aParentPart) : base(uppPartTypes.Parts, uppProjectFamilies.uopFamMD, bBaseOne: true, bMaintainIndices: false)
        {
            if (aBasePart != null) Add(aBasePart);
            if (bBasePart != null) Add(bBasePart);
            if (aParentPart != null) SubPart(aParentPart);
        }


        //public colUOPParts(IEnumerable<uopPart> aParts, uppProjectFamilies aProjectFamily = uppProjectFamilies.uopFamMD, bool bCloneMembers = false, bool bBaseOne = true, bool bMaintainIndices = false, bool bInvalidWhenEmpty = false) : base(aParts, !bCloneMembers)
        //{
        //    MaintainIndices = bMaintainIndices;
        //    BaseOne = bBaseOne;
        //    ProjectFamily = aProjectFamily;
        //    InvalidWhenEmpty = bInvalidWhenEmpty;
        // }
        public colUOPParts(IEnumerable<uopPart> aPartTopCopy) : base(aPartTopCopy, false)
        { }


        public colUOPParts(IEnumerable<uopPart> aPartTopCopy, bool bDontCopyMembers, bool bMaintainIndices = false) : base(aPartTopCopy, bDontCopyMembers) 
        {
            MaintainIndices = bMaintainIndices;
        }

       


        #endregion Constructors

        /// <summary>
        /// Clones current object - Deep copy
        /// </summary>
        /// <param name="bNoItems"></param>
        /// <returns></returns>

        public colUOPParts Clone() => new colUOPParts(this, false);

        public override uopPart Clone(bool aFlag = false) => (uopPart)Clone(aFlag);

        /// <summary>
        /// used to add an _rVal to the collection
        /// won't add "Nothing" (no error raised)
        /// </summary>
        /// <param name="aMem">the _rVal to add to the collection</param>
        /// <param name="bAddClone"></param>
        /// <param name="aCategory"></param>
        /// <param name="aBeforeIndex"></param>
        /// <returns></returns>
        public override uopPart Add(uopPart aMem, bool bAddClone = false,
             string aCategory = null, int aBeforeIndex = -1)
        {
            if (aMem == null) return null;
            if (PartType != uppPartTypes.Undefined && PartType != uppPartTypes.Parts && aMem.PartType != PartType) throw new InvalidCastException();
            uopPart _rVal = base.Add(aMem, bAddClone, aCategory, aBeforeIndex);
             if (_rVal != null) EventPartAdded?.Invoke(_rVal);
            return _rVal;
           
        }


        public override bool Invalid
        {
            get => base.Invalid || (InvalidWhenEmpty && Count <= 0);
            set
            {
                if (base.Invalid != value)
                {
                    base.Invalid = value;
                    if (value) EventPartsInvalidated?.Invoke();
                    
                }
            }
        }

        private uppPartTypes _PartType = uppPartTypes.Undefined;
        /// <summary>
        ///if set, only parts of this parttype can be added
        /// </summary>
        /// <param name="aPartType"></param>
        /// <returns></returns>
        public override uppPartTypes PartType { get => _PartType; internal set => _PartType = value; }

        /// <summary>
        ///returns all the members whose part path is left equal to the past path
        /// </summary>
        /// <param name="aPartType"></param>
        /// <returns></returns>
        public override List<uopPart> RemoveByType(uppPartTypes aPartType)
        {
            List<uopPart> _rVal = base.RemoveByType(aPartType);
            uopPart aMem;
            for (int i = 0; i < _rVal.Count; i++)
            {
                aMem = _rVal[i];
                EventPartRemoved?.Invoke(aMem);
            }
            return _rVal;
        }

        public override List<uopPart> RemoveByPartPath(string aPartPath, bool RemoveExactMatch, out int rExactMatchIndex)
        {
            rExactMatchIndex = 0;
            List<uopPart> _rVal = base.RemoveByPartPath(aPartPath,RemoveExactMatch,out rExactMatchIndex);
            uopPart aMem;
            for (int i = 0; i < _rVal.Count; i++)
            {
                aMem = _rVal[i];
                EventPartRemoved?.Invoke(aMem);
            }
            return _rVal;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="aMemberPath"></param>
        public override List<uopPart> RemoveMemberAndChildren(string aMemberPath)
        {

            List<uopPart> _rVal = base.RemoveMemberAndChildren(aMemberPath);
            uopPart aMem;
            for (int i = 0; i < _rVal.Count; i++)
            {
                aMem = _rVal[i];
                EventPartRemoved?.Invoke(aMem);
            }
            return _rVal;
        }

        /// <summary>
        /// ^returns all the members whose part path is left equal to the passed path
        /// </summary>
        /// <param name="aPath"></param>
        /// <param name="rCollectionIndex"></param>
        /// <returns></returns>
        public override List<uopPart> RemoveSubMembers(string aPath, out int rCollectionIndex)
        {
            rCollectionIndex = 0;
            List<uopPart> _rVal = base.RemoveSubMembers(aPath, out rCollectionIndex);
            uopPart aMem;
            for (int i = 0; i < _rVal.Count; i++)
            {
                aMem = _rVal[i];
                EventPartRemoved?.Invoke(aMem);
            }
            return _rVal;
        }

        // This method originally resides in "uopProjects.colUOPCommonParts" (in VB code)
        public List<uopPart> GetByPartType(uppPartTypes aPartType, out bool SameMaterial, int? aSubType = null)
        {
            // ^returns the members with a part type matching then passed value
            // ~returns True if all the returned mems have the same material
            List<uopPart> _rVal = new List<uopPart>();
            SameMaterial = false;

            uopMaterial aMAT = null; // It was "iuopMaterial" in VB
            uopPart aMem; // It was "uopCommonPart" in VB
            bool bCheckSubType = false;
            int iST = 0; // I am not sure if this value can be considered as a legitimate sub type or not!
            bool bKeep;

            try
            {
                if (Count <= 0) return _rVal;
                

                if (aSubType.HasValue)
                {       
                    bCheckSubType = true;
                    iST = aSubType.Value;
                }

                uopParts mycoll = Collection();
                for (int i = 1; i <= mycoll.Count; i++)
                {
                    aMem = mycoll.Item(i);

                    if (aMem.PartType == aPartType)
                    {
                        bKeep = true;
                        if (bCheckSubType)
                        {
                            if (aMem.SubType != iST) 
                            {
                                bKeep = false;
                            }
                        }

                        if (bKeep)
                        {
                            _rVal.Add(aMem);
                            if (aMAT == null)
                            {
                                aMAT = aMem.Material; // It was "aMem.SourcePart.Part.Material" in VB
                                SameMaterial = true;
                            }
                            else
                            {
                                if (!aMAT.IsEqual(aMem.Material)) // It was "aMem.SourcePart.Part.Material" in VB
                                {
                                    SameMaterial = false;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                _rVal = null;
            }

            return _rVal;
        }

        #region Shared Methods


        public static colUOPParts FromPartsList(List<mdDowncomerBox> aParts, bool bGetClones = false)
        {
            colUOPParts _rVal = new colUOPParts() { PartType = uppPartTypes.DowncomerBox };
            if (aParts == null) return _rVal;
            foreach (uopPart item in aParts) { _rVal.Add(item, bGetClones); }
            return _rVal;
        }

        public static colUOPParts FromPartsList(List<mdStiffener> aParts, bool bGetClones = false)
        {
            colUOPParts _rVal = new colUOPParts() { PartType = uppPartTypes.Stiffener };
            if (aParts == null) return _rVal;
            foreach (uopPart item in aParts) { _rVal.Add(item, bGetClones); }
            return _rVal;
        }
       
       
        public static colUOPParts FromPartsList(List<mdEndPlate> aParts, bool bGetClones = false)
        {
            colUOPParts _rVal = new colUOPParts() { PartType = uppPartTypes.EndPlate };
            if (aParts == null) return _rVal;
            foreach (uopPart item in aParts) { _rVal.Add(item, bGetClones); }
            return _rVal;
        }
        public static colUOPParts FromPartsList(List<mdEndSupport> aParts, bool bGetClones = false)
        {
            colUOPParts _rVal = new colUOPParts() { PartType = uppPartTypes.EndSupport };
            if (aParts == null) return _rVal;
            foreach (uopPart item in aParts) { _rVal.Add(item, bGetClones); }
            return _rVal;
        }
        #endregion Shared Methods
    }
}
