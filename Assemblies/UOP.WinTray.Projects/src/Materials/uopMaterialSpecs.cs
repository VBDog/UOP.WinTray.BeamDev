using System;
using System.Collections.Generic;
using System.IO;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;


namespace UOP.WinTray.Projects.Materials
{
    public class uopMaterialSpecs : ICloneable
    {
        #region Variables

        private static TMATERIALSPECS tStruc = new TMATERIALSPECS();

        #endregion 

        #region Constrcutor

        public uopMaterialSpecs() { tStruc = new TMATERIALSPECS(); }

        internal uopMaterialSpecs(TMATERIALSPECS aStructure) { tStruc = aStructure; }

        public uopMaterialSpecs Clone() => new uopMaterialSpecs(tStruc.Clone());

        object ICloneable.Clone() => (object)this.Clone();

        #endregion Constrcutor

        #region Properties

        internal TMATERIALSPECS Structure { get => tStruc; set => tStruc = value; }


        public static uopProjectSpecs DefaultProjectSpecs
        {
            get
            {
                uopProjectSpecs _rVal = new uopProjectSpecs();
                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.SheetMetal, false, false));
                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.SheetMetal, true, false));
                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.Plate, false, false));
                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.Plate, true, false));
                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.Pipe, false, false));
                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.Pipe, true, false));
                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.Flange, false, false));
                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.Flange, true, false));
                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.Fitting, false, false));
                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.Fitting, true, false));
                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.Gasket, false, false));
                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.Bolt, false, false));
                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.Bolt, true, false));
                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.Bolt, false, true));
                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.Bolt, true, true));

                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.Nut, false, false));
                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.Nut, true, false));
                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.Nut, false, true));
                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.Nut, true, true));

                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.LockWasher, false, false));
                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.LockWasher, true, false));
                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.LockWasher, false, true));
                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.LockWasher, true, true));

                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.FlatWasher, false, false));
                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.FlatWasher, true, false));
                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.FlatWasher, false, true));
                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.FlatWasher, true, true));

                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.ThreadedStud, false, false));
                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.ThreadedStud, true, false));
                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.ThreadedStud, false, true));
                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.ThreadedStud, true, true));

                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.SetScrew, false, false));
                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.SetScrew, true, false));
                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.SetScrew, false, true));
                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.SetScrew, true, true));

                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.Tube, false, false));
                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.Tube, true, false));
                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.Tube, false, true));
                _rVal.SetSpecV(GetDefaultV(uppSpecTypes.Tube, true, true));


                return _rVal;
            }
        }

        #endregion

        #region Methods



        public int Count(uppSpecTypes aType) => (aType == uppSpecTypes.Undefined) ? tStruc.Count : SubSet(aType).Count;


        TMATERIALSPECS SubSet(uppSpecTypes aType, dynamic bStainless = null, dynamic bMetric = null)
        {
            TMATERIALSPECS _rVal = new TMATERIALSPECS();
            // TODO (not supported):     On Error Resume Next
            bool bChkStn = bStainless != null;
            bool bStns = false;
            bool bChkMetr = bMetric != null;
            bool bMetr = false;
            if (bChkStn) bStns = mzUtils.VarToBoolean(bStainless);

            if ((int)aType >= 100 && bChkMetr) bMetr = mzUtils.VarToBoolean(bMetric);

            TMATERIALSPEC aMem;
            bool bKeep = false;
            _rVal = new TMATERIALSPECS
            {
                TypeNames = uopEnums.Description(aType)
            };

            for (int i = 1; i <= tStruc.Count; i++)
            {
                aMem = tStruc.Item(i);
                bKeep = aMem.Type == aType;
                if (bChkStn && bKeep) bKeep = aMem.Stainless == bStns;

                if (bChkMetr && bKeep) bKeep = aMem.Metric == bMetr;

                if (bKeep) _rVal.Add(aMem);

            }
            return _rVal;
        }

        public static string FirstName(uppSpecTypes aType)
        {
            string FirstName = string.Empty;
            // TODO (not supported):     On Error Resume Next
            int i = 0;
            for (i = 1; i <= tStruc.Count; i++)
            {
                if (tStruc.Item(i).Type == aType)
                {
                    FirstName = tStruc.Item(i).Name;
                    if (FirstName != null)
                        break;
                    else
                        FirstName = string.Empty;
                }
            }
            return FirstName;
        }

        public bool ReadFromFile(string aFileSpec, string aSection = "MATERIALSPECS")
        {
            bool _rVal = false;
            tStruc.Clear();
            aFileSpec = aFileSpec.Trim();
            aSection = aSection.Trim();
            if (aFileSpec ==  string.Empty || aSection ==  string.Empty)
            {
                return _rVal;
            }
            if (!File.Exists(aFileSpec))
            {
                return _rVal;
            }
            int i = 0;
            int j = 0;
            string aStr = string.Empty;
            List<string> tNames = new List<string> { }; // TODO - Specified Minimum Array Boundary Not Supported:     Dim tNames() As String
            string tname = string.Empty;
            TMATERIALSPEC aMem;
            List<string> sVals = new List<string> { }; // TODO - Specified Minimum Array Boundary Not Supported:     Dim sVals() As String
            string tlist = string.Empty;
            uppSpecTypes sType;
            int cnt = 0;
            int vcnt = 0;
            tNames = uopUtils.ListValues("Sheet,Plate,Pipe,Flange,Fitting,Bolt,Nut,Lock Washer,Flat Washer,Stud,Gasket,Set Screw");
            cnt = tNames.Count;

            for (i = 1; i <= cnt; i++)
            {
                tname = tNames[i - 1].ToUpper().Trim().Replace(" ", "");
                sType = uopUtils.SpecType(tname);
                if (sType > uppSpecTypes.Undefined)
                {
                    for (j = 1; j <= 100; j++)
                    {
                        bool found;
                        aStr = uopUtils.ReadINI_String(aFileSpec, aSection, tname + j, "", out found);
                        if (aStr ==  string.Empty)
                        {
                            break;
                        }
                        else
                        {
                            sVals = uopUtils.ListValues(aStr, ",", true, true);
                            vcnt = sVals.Count;

                            if (vcnt > 0 &&  sVals[0] !=  string.Empty)
                            {
                                aMem = new TMATERIALSPEC
                                {
                                    Name = sVals[0]
                                };
                                if (vcnt >= 2) aMem.Comment = sVals[1];
                                
                                if (vcnt >= 3) aMem.Stainless = string.Compare(sVals[2], "True") == 0;
                                
                                if (sType >= uppSpecTypes.Bolt)
                                {
                                    if (vcnt >= 4) aMem.Metric = string.Compare(sVals[3], "True") == 0;
                                    
                                }
                                if (vcnt >= 5) aMem.Filter = sVals[4].Trim();
                                
                                aMem.Type = sType;
                                tStruc.Add(aMem);
                                if (j == 1)
                                {
                                    mzUtils.ListAdd(ref tlist, uopUtils.SpecTypeName(aMem.Type));
                                }
                            }
                        }
                    }
                }
            }
            tStruc.TypeNames = tlist;
            return _rVal;
        }

        public string TypeNames
        {
            get
            {
                string TypeNames;
                TypeNames = tStruc.TypeNames;

                return TypeNames;
            }
        }

        public string SpecList(uppSpecTypes aType, object bStainless = null, object bMetric = null)
        {
            string _rVal = string.Empty;
            TMATERIALSPECS aSet;
            aSet = SubSet(aType, bStainless, bMetric);
            for (int i = 1; i <= aSet.Count; i++)
            {
                mzUtils.ListAdd(ref _rVal, aSet.Item(i).Name);
            }
            return _rVal;
        }

        public uopMaterialSpec Item(dynamic aIndex)
        {
            int idx = mzUtils.VarToInteger(aIndex);
            return (idx > 0 & idx <= tStruc.Count) ? new uopMaterialSpec(tStruc.Item(idx)) : null;
        }

        public uopMaterialSpecs GetByTypeName(string aName, object bStainless = null, object bMetric = null)
        {
            uopMaterialSpecs GetByTypeName = null;
            GetByTypeName = new uopMaterialSpecs
            {
                Structure = SubSet(uopUtils.SpecType(aName), bStainless, bMetric)
            };
            return GetByTypeName;
        }

        public uopMaterialSpecs GetByType(uppSpecTypes aType, object bStainless = null, object bMetric = null)
        {
            uopMaterialSpecs GetByType = null;
            GetByType = new uopMaterialSpecs
            {
                Structure = SubSet(aType, bStainless, bMetric)
            };
            return GetByType;
        }

        public uopMaterialSpecs GetByMaterial(uopMaterial aMaterial)
        {
            uopMaterialSpecs GetByMaterial = null;
            GetByMaterial = new uopMaterialSpecs();
            if (aMaterial == null)
            {
                return GetByMaterial;

            }
            GetByMaterial.Structure = SubSet(aMaterial.SpecType, aMaterial.IsStainless, aMaterial.IsMetric);
            return GetByMaterial;
        }

        public static uopMaterialSpec GetByName(uppSpecTypes aType, string aName, bool bReturnNewIfNotFound = false)
        {
            TMATERIALSPEC aMem = tStruc.GetByName(aType, aName, bReturnNewIfNotFound);
            return (aMem.Index <= 0) ? null : new uopMaterialSpec(aMem);
        }

        public string NameList(uppSpecTypes aType, object bStainless = null, object bMetric = null, string aDelimiter_UNUSED = "")
        {
            string _rVal = string.Empty;
            TMATERIALSPECS aSet;
            aSet = SubSet(aType, bStainless, bMetric);
            for (int i = 1; i <= aSet.Count; i++)
            {
                mzUtils.ListAdd(ref _rVal, aSet.Item(i).Name);
            }
            return _rVal;
        }

        public static string GetMaterialDefault(uopMaterial aMaterial) => (aMaterial == null) ? string.Empty : GetMaterialDefaultV(aMaterial.Structure);


        internal static string GetMaterialDefaultV(TMATERIAL aMaterial)
        {
            TMATERIALSPEC aMem;
            uppSpecTypes sType;
            uppMaterialTypes mtype;
            bool bTest = false;
            sType = aMaterial.SpecType;
            mtype = aMaterial.Type;
            for (int i = 1; i <= tStruc.Count; i++)
            {
                aMem = tStruc.Item(i);
                bTest = aMem.Type == sType;
                if (bTest && sType != uppSpecTypes.Gasket) bTest = aMem.Stainless == aMaterial.IsStainless;

                if (bTest && mtype == uppMaterialTypes.Hardware) bTest = aMem.Metric == aMaterial.IsMetric;

                if (bTest)
                {
                    if (aMem.AppliesTo(aMaterial)) return aMem.Name;
                }
            }
            return "";
        }

        internal static TMATERIALSPEC GetMaterialDefaultStruc(TMATERIAL aMaterial)
        {
            TMATERIALSPEC aMem;
            uppSpecTypes sType;
            uppMaterialTypes mtype;
            bool bTest = false;


            sType = aMaterial.SpecType;
            mtype = aMaterial.Type;

            for (int i = 1; i <= tStruc.Count; i++)
            {
                aMem = tStruc.Item(i);
                bTest = aMem.Type == sType;
                if (bTest && sType != uppSpecTypes.Gasket) bTest = aMem.Stainless == aMaterial.IsStainless;

                if (bTest && mtype == uppMaterialTypes.Hardware) bTest = aMem.Metric == aMaterial.IsMetric;

                if (bTest)
                {
                    if (aMem.AppliesTo(aMaterial)) return aMem;
                }
            }
            return new TMATERIALSPEC();
        }

        internal static TMATERIALSPEC GetDefaultV(uppSpecTypes aType, bool bStainless, bool bMetric)
        { int IDX = 0; return GetDefaultV(aType, bStainless, bMetric, out IDX); }


            internal static TMATERIALSPEC GetDefaultV(uppSpecTypes aType, bool bStainless, bool bMetric, out int rIndex)
        {
            TMATERIALSPEC _rVal = new TMATERIALSPEC();
            TMATERIALSPEC aMem;
            bool bTest = false;
            rIndex = 0;
            for (int i = 1; i <= tStruc.Count; i++)
            {
                aMem = tStruc.Item(i);
                bTest = aMem.Type == aType;
                if (bTest && aType != uppSpecTypes.Gasket) bTest = aMem.Stainless == bStainless;
                
                if (bTest && (int)aType >= 100) bTest = aMem.Metric == bMetric;
                
                if (bTest)
                {
                    rIndex = i;
                    _rVal = aMem;
                    break;
                }
            }
            return _rVal;
        }

        uopMaterialSpec GetDefault(uppSpecTypes aType, bool bStainless, bool bMetric)
        {
            int i = 0;
            TMATERIALSPEC aMem;
            aMem = GetDefaultV(aType, bStainless, bMetric, out i);
            return (i > 0) ? new uopMaterialSpec(aMem) : null;
        }

        TMATERIALSPECS GetMaterialSpecsV(TMATERIAL aMaterial)
        {
            TMATERIALSPECS GetMaterialSpecsV = new TMATERIALSPECS();
           
            TMATERIALSPEC aMem;
           
            uppSpecTypes sType;
            uppMaterialTypes mtype;
            bool bTest = false;
          
            sType = aMaterial.SpecType;
            mtype = aMaterial.Type;
            for (int i = 1; i <= tStruc.Count; i++)
            {
                aMem = tStruc.Item(i);
                bTest = aMem.Type == sType;
                if (bTest && sType != uppSpecTypes.Gasket)
                {
                    bTest = aMem.Stainless == aMaterial.IsStainless;
                }
                if (bTest && mtype == uppMaterialTypes.Hardware)
                {
                    bTest = aMem.Metric == aMaterial.IsMetric;
                }
                if (bTest)
                {
                    if (aMem.AppliesTo(aMaterial))
                    {
                        GetMaterialSpecsV.Add(aMem);
                    }
                }
            }
            return GetMaterialSpecsV;
        }

        public uopMaterialSpecs GetMaterialSpecs(uopMaterial aMaterial)
        {
            uopMaterialSpecs GetMaterialSpecs = null;
            GetMaterialSpecs = new uopMaterialSpecs();
            if (aMaterial != null)
            {
                GetMaterialSpecs.Structure = GetMaterialSpecsV(aMaterial.Structure);
            }
            return GetMaterialSpecs;
        }

        #endregion Methods
    }
}
