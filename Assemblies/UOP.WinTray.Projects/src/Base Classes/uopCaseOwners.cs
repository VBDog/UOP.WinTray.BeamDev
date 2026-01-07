using System;
using System.Collections.Generic;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Interfaces;
using UOP.WinTray.Projects.Parts;

using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;
namespace UOP.WinTray.Projects
{
    public abstract class uopCaseOwners : uopParts
    {
        public uopCaseOwners(uppCaseOwnerOwnerTypes aCaseOwnerType = uppCaseOwnerOwnerTypes.Distributor) : base(aCaseOwnerType.PartType(), uppProjectFamilies.uopFamMD, bBaseOne: true, bMaintainIndices: true, bInvalidWhenEmpty: false)
        { }
        public uopCaseOwners(uppCaseOwnerOwnerTypes aCaseOwnerType = uppCaseOwnerOwnerTypes.Distributor, uopCaseOwners aPartToCopy = null, uopPart aParent = null, bool bDontCopyMembers = false) : base(aCaseOwnerType.PartType(), uppProjectFamilies.uopFamMD, bBaseOne: true, bMaintainIndices: true, bInvalidWhenEmpty: false)
        { }

        public abstract uppCaseOwnerOwnerTypes OwnerType { get; }
        private int _CaseCount = 1;
        public virtual int CaseCount
        {
            get => _CaseCount;

            set
            {
                if (value > 0)
                {
                    if (_CaseCount != value)
                    {
                        _CaseCount = value;
                        EnforceCaseCount();
                    }
                }
            }
        }
        public  List<string> CaseDescriptions
        {
            get
            {
                List<string> _rVal = new List<string>();
                if (Count <= 0) return _rVal;

                CaseCount = MaxCaseCount;
                iCaseOwner aOwner = (iCaseOwner)Item(1);
                for (int i = 1; i <= _CaseCount; i++)
                {
                    _rVal.Add(aOwner.GetCase(i)?.Description);
                }
                return _rVal;
            }
            set
            {
                if (value == null) return;
                iCaseOwner owner;
                for(int i = 1; i <= Count; i++)
                {
                    owner = Item(i);
                    colUOPParts cases = owner.Cases;
                    for (int j = 1; j <= cases.Count; j++)
                    {
                        if(j <= value.Count)
                        {
                            if(!string.IsNullOrWhiteSpace(value[j - 1]))
                            {
                                iCase ownercase = (iCase)cases.Item(j);
                                ownercase.Description = value[j - 1];
                            }
                          
                        }
                    }
                }
               
            }
        }

        public iCase GetCase(string aHandle, List<iCase> rAllCases = null)
        {

            iCase aCase = null;
            if (rAllCases == null) rAllCases = AllCases();

            for (int i = 0; i < rAllCases.Count; i++)
            {
                aCase = rAllCases[i];
                if (string.Compare(aCase.Handle, aHandle, true) == 0) return aCase;
            }
            return null;
        }

        /// <summary>
        /// renames the indicated case form all distributors
        /// </summary>
        /// <param name="aCaseIndex"></param>
        /// <param name="aName"></param>
        public void ReNameCase(int aCaseIndex, string aName)
        {
            try
            {
                iCaseOwner aOwner = null;
                CaseCount = MaxCaseCount;
                if (aCaseIndex < 0 || aCaseIndex >= _CaseCount) return;

                for (int i = 1; i <= Count; i++)
                {
                    aOwner = (iCaseOwner)Item(i);
                    aOwner.GetCase(aCaseIndex).Description = aName;
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
        /// <summary>
        /// removes the indicated case form all members
        /// </summary>
        /// <param name="aCaseIndex"></param>
        public bool RemoveCase(int aCaseIndex)
        {
            bool _rVal = false;
            try
            {
                iCaseOwner aOwner = null;
                CaseCount = MaxCaseCount;
                if (aCaseIndex < 0 || aCaseIndex > _CaseCount || CaseCount == 1) return false;
                

                for (int i = 1; i <= Count; i++)
                {
                    aOwner = (iCaseOwner)Item(i);
                    if( aOwner.Cases.Remove(aCaseIndex) != null) _rVal = true;
                }
                CaseCount = MaxCaseCount;
               
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return _rVal;
        }
        /// <summary>
        /// removes the indicated case form all members
        /// </summary>
        /// <param name="aCaseDescription"></param>
        public bool RemoveCase(string aCaseDescription)
        {
            bool _rVal = false;
            try
            {
                iCaseOwner aOwner = null;
                CaseCount = MaxCaseCount;
                if (string.IsNullOrWhiteSpace(aCaseDescription) || CaseCount == 1) return false;

                int idx;
                for (int i = 1; i <= Count; i++)
                {
                    aOwner = (iCaseOwner)Item(i);
                    idx = aOwner.Cases.FindIndex(x => string.Compare(x.Description, aCaseDescription, true) == 0) ;
                    if (aOwner.Cases.Remove(idx) != null) _rVal = true;
                }
                CaseCount = MaxCaseCount;

            }
            catch (Exception exception)
            {
                throw exception;
            }
            return _rVal;
        }
        /// <summary>
        /// removes the indicated case form all members
        /// </summary>
        /// <param name="aCaseDescription"></param>
        public bool RemoveCases(List<string> aCaseDescriptions)
        {
            bool _rVal = false;
            try
            {
             
                CaseCount = MaxCaseCount;
                if (aCaseDescriptions == null || CaseCount == 1) return false;
                foreach (var item in aCaseDescriptions)
                {
                    if (RemoveCase(item)) _rVal = true;
                }
                return _rVal;

            }
            catch (Exception exception)
            {
                throw exception;
            }
           
        }
        /// <summary>
        /// adds a clone of the indicated case to all the owners
        /// </summary>
        /// <param name="aCaseIndex"></param>
        public bool CloneCase(int aCaseIndex, string aNewCaseDescription)
        {
            try
            {
                iCaseOwner aOwner = null;
                CaseCount = MaxCaseCount;
                if (aCaseIndex < 0 || aCaseIndex > CaseCount || string.IsNullOrWhiteSpace(aNewCaseDescription)) return false;
                bool _rVal = false;
                for (int i = 1; i <= Count; i++)
                {
                    aOwner = (iCaseOwner)Item(i);
                    if(aCaseIndex <= aOwner.Cases.Count)
                    {
                        iCase acase = (iCase)aOwner.Cases.Item(aCaseIndex, bReturnClone: true);
                        if (acase != null)
                        { 
                            acase.Description = aNewCaseDescription;
                            if (aOwner.AddCase(acase)) _rVal = true;
                        }
                    }
                }

                CaseCount = MaxCaseCount;
                return _rVal;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
        public abstract iCaseOwner AddOwner(iCaseOwner aOwner, bool bAddClone = false);
        public  void EnforceCaseDescriptions(iCaseOwner aMember = null)
        {

            if (Reading)
                return;
            try
            {
                CaseCount = MaxCaseCount;
                List<iCase> dCases = AllCases();
                List<string> Descrips = CaseDescriptions;
                iCase aCase = null;
                for (int i = 0; i < dCases.Count; i++)
                {
                    aCase = dCases[i];
                    aCase.Description = Descrips[aCase.Index - 1];
                }
            }
            catch (Exception exception)
            {
                LoggerManager log = new LoggerManager();
                log.LogError(exception.Message);
            }
        }

        public virtual int MaxCaseCount {
            get
            {
                {
                    int _rVal = 1;
                    iCaseOwner mem;
                    for (int i = 1; i <= Count; i++)
                    {
                        mem = Item(i);
                        if (mem.Cases.Count > _rVal) 
                            _rVal = mem.Cases.Count;

                    }
                    return _rVal;
                }
            }
        }

        public iCaseOwner EnforceCaseCount(iCaseOwner aMember = null, int aCaseCount = 0)
        {

            if (Reading)
                return null;
            iCaseOwner aMem = null;
            uopParts dCases;
            iCase newcase;
            bool distribs = OwnerType == uppCaseOwnerOwnerTypes.Distributor;
            if (aCaseCount <= 0)
            {
                CaseCount = MaxCaseCount;
                aCaseCount = _CaseCount;
            }
            List<string> casenames = CaseDescriptions;

            if (casenames.Count <= 0)
            {
                if(distribs)
                    casenames.Add("Normal Feed");
                else
                    casenames.Add("Normal Feed");
            }

            if (aMember == null)
            {
                for (int i = 1; i <= Count; i++)
                {
                    aMem = (iCaseOwner)base.Item(i);
                    dCases = aMem.Cases;
                    if (dCases.Count > _CaseCount) dCases.ReduceTo(_CaseCount);
                    int j = 0;
                    while (dCases.Count < aCaseCount)
                    {
                        j++;
                        if (distribs)
                            newcase = new mdDistributorCase();
                        else
                            newcase = new mdChimneyTrayCase();

                        if (j <= casenames.Count)
                            newcase.Description = casenames[j - 1];
                        else
                            newcase.Description = $"Case {j}";

                        newcase.Index = aMem.Cases.Count + 1;
                        uopPart prt = (uopPart)newcase;
                        prt.SubPart((uopPart)aMem);

                        aMem.Cases.CollectionObj.Add(prt);

                    }
                }
            }
            else
            {
                aMem = aMember;
                dCases = aMem.Cases;
                if (dCases.Count > aCaseCount) dCases.ReduceTo(aCaseCount);
               
                int j = 0;
                while (dCases.Count < aCaseCount)
                {
                    j++;
                    if (distribs)
                        newcase = new mdDistributorCase();
                    else
                        newcase =new mdChimneyTrayCase();

                    
                    newcase.Index = aMem.Cases.Count + 1;
                    uopPart prt = (uopPart)newcase;
                    prt.SubPart((uopPart)aMem);
                    if (j <= casenames.Count)
                        newcase.Description = casenames[j - 1];
                    else
                        newcase.Description = $"Case {j}";

                    aMem.Cases.CollectionObj.Add(prt);

                }
            }
            if(aMem != null) aMem.Cases.Reindex();

            CaseCount = aCaseCount;
            return aMem;
        }

      

        public List<iCase> AllCases(bool bReturnClones = false)
        {
            List<iCase> _rVal = new List<iCase>();
            
            colUOPParts dCases;

            iCaseOwner aMem;
            iCase aCase;
            CaseCount = MaxCaseCount;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                aMem = EnforceCaseCount(aMem, _CaseCount);
                dCases = aMem.Cases;
                for (int j = 1; j <= dCases.Count; j++)
                {
                    aCase = (iCase)dCases.Item(j, bReturnClones);
                    aCase.Index = j;
                    if (aCase != null) _rVal.Add(aCase);

                }
            }
            return _rVal;
        }

        public string SelectedMemberName()
        {
            iCaseOwner mem = SelectedMember;
            return (mem != null) ? mem.Name : "";
        }

        /// <summary>
        ///returns the item from the collection at the requested index ! Base 1 !
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        /// 
        public new iCaseOwner Item(int aIndex)
        {
            return (iCaseOwner)base.Item(aIndex);
        }

        public new iCaseOwner SelectedMember
        {
            get
            {
                return (iCaseOwner)base.SelectedMember;
            }
        }

        public void SetSelectedCase(int aIndex)
        {
            List<iCaseOwner> owners = ToList();
            foreach (iCaseOwner item in owners)
            {
                item.Cases.SetSelected(aIndex);
            }

        }

        public List<string> CaseNames(List<string> rHandles = null)
        {
            List<string> _rVal = new List<string>();
            if (Count <= 0)
            {
                return _rVal;
            }
            CaseCount = MaxCaseCount;
            iCaseOwner aMem = null;
            iCase aCase = null;
            aMem = (iCaseOwner) Item(1);
            for (int i = 0; i < _CaseCount; i++)
            {
                aCase = aMem.GetCase(i);
                _rVal.Add(aCase.Name);
                if (rHandles != null)
                {
                    rHandles.Add(aCase.Handle);
                }
            }
            return _rVal;
        }
        public List<iCase> ReportCases
        {
            get
            {
                List<iCase> _rVal = new List<iCase>();
                if (Count <= 0) return _rVal;


                iCaseOwner aMem;
                List<iCase> CaseMems = null;
                CaseCount = MaxCaseCount;
                for (int i = 1; i <= _CaseCount; i++)
                {
                    CaseMems = new List<iCase>();
                    for (int j = 1; j <= Count; j++)
                    {
                        aMem = (iCaseOwner)Item(j);
                        CaseMems.Add(aMem.GetCase(i));
                    }
                    while (!(CaseMems.Count % 5 == 0))
                    {
                        CaseMems.Add(null);
                    }
                    for (int j = 0; j < CaseMems.Count; j++)
                    {
                        _rVal.Add(CaseMems[j]);
                    }
                }
                return _rVal;
            }
        }

        public void Populate(uopParts aParts)
        {
            
            Clear();
            uopPart prt;
            if (aParts == null) return;
            for (int i = 1; i <= aParts.Count; i++) 
            {
                prt = aParts.Item(i);
                if((prt.PartType == uppPartTypes.Distributor && OwnerType == uppCaseOwnerOwnerTypes.Distributor) || (prt.PartType == uppPartTypes.ChimneyTray && OwnerType == uppCaseOwnerOwnerTypes.ChimneyTray))
                {
                    base.Add(prt);
                }
                
            }
        }


        public List<iCase> GetCasesByIndex(int aIndex, ref List<iCase> ioAllCases , List<iCase> aCollector = null)
        {
            List<iCase> _rVal = aCollector ?? new List<iCase>();


            ioAllCases ??= AllCases();
            List<iCase> idxfcases = ioAllCases.FindAll(x => x.Index == aIndex);
            _rVal.AddRange(idxfcases);

           
            return _rVal;
        }

        public void Populate(uopCaseOwners aOwners)
        {

            Clear();
            iCaseOwner owner;
            if (aOwners == null) return;
            for (int i = 1; i <= aOwners.Count; i++)
            {
                owner = aOwners.Item(i);
                if (owner.OwnerType == OwnerType)
                {
                    base.Add((uopPart)owner);
                }

            }
        }

        public void Populate(List<iCaseOwner> aOwners)
        {

            Clear();
            iCaseOwner owner;
            if (aOwners == null) return;
            for (int i = 1; i <= aOwners.Count; i++)
            {
                owner = aOwners[i -1];
                if (owner.OwnerType == OwnerType)
                {
                    base.Add((uopPart)owner);
                }

            }
        }

        public new List<iCaseOwner> ToList(bool bGetClones = false)
        {

            List<iCaseOwner> _rVal = new List<iCaseOwner>();

            foreach (var part in CollectionObj)
            {
                if (!bGetClones)
                    _rVal.Add((iCaseOwner)part);
                else
                    _rVal.Add((iCaseOwner)part.Clone());
            }
            return _rVal;
        }


        public  void SetSelected(iCaseOwner aMember)
        {
            base.SetSelected((uopPart)aMember);

        }
        public iCaseOwner Add(iCaseOwner aMember) 
        {
            if (aMember == null) return null;
           
            iCaseOwner _rVal = (iCaseOwner)base.Add((uopPart)aMember);
            EnforceCaseCount(_rVal);
            EnforceCaseDescriptions();
            return _rVal;

        }

        public bool AddCase(iCase aCase)
        {
            if (aCase == null) return false;
            if (string.IsNullOrWhiteSpace(aCase.Description)) return false;
            bool _rVal = false;
            for (int i = 1; i <= Count; i++) { if(Item(i).AddCase(aCase.Clone())) _rVal = true; }
            EnforceCaseCount();
            EnforceCaseDescriptions();
            return _rVal;
        }

        /// <summary>
        ///removes the item to the collection at the requested description !
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        /// 
        public iCaseOwner Remove(string aDescription)
        {
            if (string.IsNullOrWhiteSpace(aDescription)) return null;
            int idx = FindIndex(x => string.Compare(x.Description, aDescription, true) == 0);
            if (idx <= 0) return null;
            return (iCaseOwner)Remove(idx);
           
        }

        public bool RenameCase(string aCurrentName, string aNewName)
        {
            if (Count <= 0 || MaxCaseCount <= 0) return false;
            bool _rVal = false;
            for (int i = 1; i <= Count; i++) { if (Item(i).ReNameCase(aCurrentName, aNewName)) _rVal = true; }
            return _rVal;
        }

        public List<string> Names
        {
            get
            {
                List<string> _rVal = new List<string>();
                for (int i = 1; i <= Count; i++) { _rVal.Add(Item(i).Name); }
                return _rVal;
            }
        }

        public List<string> Descriptions
        {
            get
            {
                List<string> _rVal = new List<string>();
                for (int i = 1; i <= Count; i++) { _rVal.Add(Item(i).Description); }
                return _rVal;
            }
        }

    }
}
