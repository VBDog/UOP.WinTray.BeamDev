using UOP.DXFGraphics;
using System;
using System.Collections;
using System.Collections.Generic;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.Projects
{
    public class uopHoleArray : List<uopHoles>, ICloneable, IEnumerable<uopHoles>
    {
     
        public uopHoleArray() {Invalid = false; }

        internal uopHoleArray(UHOLEARRAY aStructure) { Structure_Set( aStructure); }

        public uopHoleArray(uopHoleArray aArrayToCopy)
        {
            Invalid = false;
            if (aArrayToCopy == null) return;
            foreach (var item in aArrayToCopy)
            {

                Add(item.Clone());
            }
        }


        #region Properties


        public string MemberNamesList
        {
            get
            {
                string _rVal = string.Empty;
                for (int i = 1; i <= Count; i++)
                {
                    mzUtils.ListAdd(ref _rVal, Item(i).Name, bSuppressTest: true, aDelimitor: ", ", bAllowNulls: true);
                }
                return _rVal;
            }
        }
        public bool Invalid { get; set; }

        #endregion Properties

        public void AppendMirrors(double? aX, double? aY, string aNames = "")
        {
            if (Count <= 0 || (!aX.HasValue && !aY.HasValue)) return;
            aNames ??= string.Empty;
            foreach (uopHoles item in this)
            {
                if (mzUtils.ListContains(item.Name, aNames, bReturnTrueForNullList: true))
                    item.AppendMirrors(aX, aY);
            }

        }

        public uopHoles Add(uopHoles aHoles, string aName = null, bool bAppendToExisting = false)
        {
            if (aHoles == null) return null;


            if (!string.IsNullOrWhiteSpace(aName)) aHoles.Name = aName;
            if (string.IsNullOrWhiteSpace(aHoles.Name)) aHoles.Name = aHoles.Member.Tag;
            if (string.IsNullOrWhiteSpace(aHoles.Name)) aHoles.Name = $"ARRAY_{ Count + 1}";

            if (bAppendToExisting)
            {

                if (Contains(aName, out int idx))
                {
                    uopHoles mem = Item(idx);
                    mem.Centers.Append(aHoles.Centers);
                    return Item(idx);
                }
            }

            aHoles.Index = Count + 1;
            base.Add(aHoles);
            return Item(Count);
        }
        public bool Contains(string aMemberName, bool bNamesLike = false) => Contains(aMemberName, out int _ , bNamesLike);
        
        public bool Contains(string aMemberName, out int rIndex, bool bNamesLike = false)
        {
            rIndex = 0;
            if (string.IsNullOrWhiteSpace(aMemberName)) return false;

            if(!bNamesLike)
                rIndex = FindIndex(x =>  string.Compare(x.Name,aMemberName,true) == 0) + 1;
            else
                rIndex = FindIndex(x => x.Name.StartsWith(aMemberName,comparisonType: StringComparison.OrdinalIgnoreCase)) + 1;

            return rIndex > 0;
        }

        internal UHOLEARRAY Structure_Get()
        {
            UHOLEARRAY _rVal = new UHOLEARRAY() { Invalid = Invalid };
            foreach (uopHoles item in this)
            {
                _rVal.Add(new UHOLES( item));
            }

            return _rVal;
        }

        internal void Structure_Set(UHOLEARRAY aHoleArray) 
        {
            Clear();
            Invalid = aHoleArray.Invalid;
            for(int i = 1; i <= aHoleArray.Count; i++)
            {
                Add(new uopHoles(aHoleArray.Item(i)));
            }
        }

        public bool TryGet(string aArrayName, out uopHoles rMember, bool bNamesLike = false)
        {
            rMember = null;
            if (!Contains(aArrayName, out int idx, bNamesLike)) return false;
            rMember = Item(idx);
            return true;
            
        }
        public uopHoleArray Clone() => new uopHoleArray(this);

        object ICloneable.Clone() => (object)this.Clone();

        public colDXFEntities ToDXFEntities(string aMemberName, string aLayerName = "", dxxColors aColor = dxxColors.Undefined, string aLinetype = "", double aHClineScale = 0, double aVClineScale = 0, double aDClineScale = 0, dxfImage aImage = null, dxxLinetypeLayerFlag aLTLSetting = dxxLinetypeLayerFlag.Undefined,  bool bSuppressInstances = false, bool bSuppressCenterPoints = false)
        {
            uopHoles mem;
            colDXFEntities _rVal = new colDXFEntities();
            if (!string.IsNullOrWhiteSpace(aMemberName)) 
            {
                if (!TryGet(aMemberName, out mem)) return _rVal;
                    return mem.ToDXFEntities(aLayerName, aColor, aLinetype, aHClineScale, aVClineScale, aDClineScale, aImage, aLTLSetting, bSuppressInstances, bSuppressCenterPoints:bSuppressCenterPoints);
                                
            }
            
            for (int i = 1; i <= Count; i++)
            {
                mem = Item(i);
                _rVal.Append(mem.ToDXFEntities(aLayerName, aColor, aLinetype, aHClineScale, aVClineScale, aDClineScale, aImage, aLTLSetting, bSuppressInstances, bSuppressCenterPoints:bSuppressCenterPoints), bAddClones:false);
            }
            return _rVal;
        }

        public int MemberCount(string aArrayName, uppHoleTypes aHoleType = uppHoleTypes.Any, bool? aSuppressVal = null, bool bNamesLike = false)
        {

            if (!Contains(aArrayName, out int idx, bNamesLike)) { return 0; }

            int _rVal = 0;
            
         
            bool bSupTest = aSuppressVal.HasValue;
            bool bSup = false;
            if (bSupTest)  bSup = aSuppressVal.Value; 
            if (!bNamesLike)
            {
                for (int j = 1; j <= Count; j++)
                {
                    uopHoles aHoles = Item(j);

                    if (string.Compare(aHoles.Name, aArrayName, true) != 0) continue;
                    if (!bSupTest && aHoleType == uppHoleTypes.Any)
                    {
                        _rVal += aHoles.Count;
                    }
                    else
                    {
                        for (int i = 1; i <= aHoles.Count; i++)
                        {
                            uopHole aHl = aHoles.Item(i);

                            if (aHl.HoleType == aHoleType || aHoleType == uppHoleTypes.Any)
                            {

                                if (!bSupTest || (bSupTest && aHl.Suppressed == bSup))
                                {
                                    _rVal += 1;
                                }
                            }
                        }
                    }
                }
                return _rVal;
            }



            for (int j = 1; j <= Count; j++)
            {
                uopHoles aHoles = Item(j);
                bool bAdd = aHoles.Name.IndexOf(aArrayName, StringComparison.OrdinalIgnoreCase) >= 0;

                if (bAdd)
                {
                    if (!bSupTest && aHoleType == uppHoleTypes.Any)
                    {
                        _rVal += aHoles.Count;
                    }
                    else
                    {
                        for (int i = 1; i <= aHoles.Count; i++)
                        {
                            uopHole aHl = aHoles.Item(i);
                            if (aHl.HoleType == aHoleType || aHoleType == uppHoleTypes.Any)
                            {
                                if (!bSupTest || (bSupTest && aHl.Suppressed == bSup))
                                {
                                    _rVal += 1;
                                }
                            }
                        }
                    }
                }
            }
            return _rVal;
        }
        public int ItemCount(int aIndex, uppHoleTypes aHoleType = uppHoleTypes.Any, dynamic aSuppressVal = null)
        {
            if (aIndex < 1 || aIndex > Count || Count <= 0) { return 0; }

            int _rVal = 0;
            uopHoles aHoles  = Item(aIndex);

            if (aHoleType == uppHoleTypes.Any)
            {
                if (aSuppressVal == null)
                {
                    _rVal = aHoles.Centers.Count;
                }
                else
                {
                    _rVal = aHoles.Centers.SuppressedCount(mzUtils.VarToBoolean(aSuppressVal));
                }
            }
            else
            {
                uopHole aHl;
                for (int i = 1; i <= aHoles.Count; i++)
                {
                    aHl = aHoles.Item(i);
                    if (aHl.HoleType == aHoleType)
                    {
                        if (aSuppressVal == null)
                        { _rVal += 1; }
                        else
                        {
                            if (Convert.ToBoolean(aSuppressVal) == aHl.Suppressed)
                            {
                                _rVal += 1;
                            }
                        }
                    }
                }
            }
            return _rVal;
        }

        public uopHoles Member(dynamic aNameOrIndex, bool bRemove = false) => Member(aNameOrIndex, out int _, bRemove);


        public uopHoles Member(dynamic aNameOrIndex, out int rIndex, bool bRemove = false)
        {
            rIndex = 0;
            uopHoles _rVal = null;
            if (aNameOrIndex == null || Count <= 0) return _rVal;
            string tname = TVALUES.GetDynamicTypeName(aNameOrIndex);
         

            if (tname == "STRING")
            {
                string memname = Convert.ToString(aNameOrIndex);
                if (!Contains(memname, out rIndex)) return _rVal;
                _rVal = this[rIndex - 1];
              
            }
            if (rIndex == 0)
            {
                if (mzUtils.IsNumeric(aNameOrIndex))
                {
                    int idx = mzUtils.VarToInteger(aNameOrIndex);
                    if (idx < 0 || idx > Count) return _rVal;
                    _rVal = this[idx - 1];
                    _rVal.Index = idx;
                    rIndex = idx;
                }
            }

            if (rIndex > 0 && bRemove) { Remove(rIndex); }

            return _rVal;
        }
        public uopHoles Remove(int aIndex)
        {
            uopHoles _rVal = null;
            if (aIndex < 1 || aIndex > Count)  return _rVal;
            _rVal = this[aIndex - 1];
           RemoveAt(aIndex - 1);

            return _rVal;

        }



        public uopHole Hole(dynamic aArrayNameOrIndex, dynamic aMemberFlagOrIndex, out int rArrayIndex, out int rIndex)
        {
            rArrayIndex = 0;
            rIndex = 0;
           uopHole _rVal = null;
            uopHoles aHoles;
            
            uopHole aHl;
            bool bByFlag = aMemberFlagOrIndex.GetType().Name.ToString().ToUpper() == "STRING";
            
            aHoles = Member(aArrayNameOrIndex, out rArrayIndex);
            if (rArrayIndex <= 0) return _rVal;
            if (bByFlag)
            {
                for (int i = 1; i <= aHoles.Centers.Count; i++)
                {
                    aHl = aHoles.Item(i);
                    if (string.Compare(aHl.Flag, aMemberFlagOrIndex, true) == 0)
                    {
                        rIndex = i;
                        _rVal = aHl;
                        break;
                    }
                }
            }
            else
            {
                int i = mzUtils.VarToInteger(aMemberFlagOrIndex);
                if (i > 0 & i <= aHoles.Centers.Count)
                {
                    rIndex = i;
                    _rVal = aHoles.Item(i);
                }
            }
            return _rVal;
        }

        public uopHoles Item(int aIndex, bool bRemove = false)
        => Member( aIndex, out int rIndex, bRemove);

        public uopHoles Item(string aName, bool bRemove = false)
        => Member(aName, out int rIndex, bRemove);

        public uopHoles Item(string aName, out int rIndex, bool bRemove = false)
     => Member(aName, out rIndex, bRemove);

        public List<List<uopHole>> ToList
        {
            get
            {
                List<List<uopHole>> _rVal = new List<List<uopHole>>();

                for (int i =1; i <= Count; i++)
                {
                    _rVal.Add(Item(i).ToList);
                }

                return _rVal;
            }
        }

        public int MembersCountMemberCount(string aName, uppHoleTypes aHoleType = uppHoleTypes.Any, dynamic aSuppressVal = null, bool bNamesLike = false)
        {

            if (!Contains(aName, out int idx, bNamesLike)) { return 0; }

            int _rVal = 0;
            uopHoles aHoles;

            uopHole aHl;
            bool bSupTest = aSuppressVal != null;
            bool bSup = false;
            if (bSupTest) { bSup = mzUtils.VarToBoolean(aSuppressVal); }
            if (!bNamesLike)
            {
                aHoles = Item(idx);
                if (!bSupTest && aHoleType == uppHoleTypes.Any)
                {
                    _rVal += aHoles.Count;
                }
                else
                {
                    for (int i = 1; i <= aHoles.Count; i++)
                    {
                        aHl = aHoles.Item(i);

                        if (aHl.HoleType == aHoleType || aHoleType == uppHoleTypes.Any)
                        {

                            if (!bSupTest || (bSupTest && aHl.Suppressed == bSup))
                            {
                                _rVal += 1;
                            }
                        }
                    }
                }

                return _rVal;
            }

            bool bAdd = false;

            for (int j = 1; j <= Count; j++)
            {
                aHoles = Item(j);
                bAdd = aHoles.Name.IndexOf(aName, StringComparison.OrdinalIgnoreCase) >= 0;

                if (bAdd)
                {
                    if (!bSupTest && aHoleType == uppHoleTypes.Any)
                    {
                        _rVal += aHoles.Count;
                    }
                    else
                    {
                        for (int i = 1; i <= aHoles.Count; i++)
                        {
                            aHl = aHoles.Item(i);
                            if (aHl.HoleType == aHoleType || aHoleType == uppHoleTypes.Any)
                            {
                                if (!bSupTest || (bSupTest && aHl.Suppressed == bSup))
                                {
                                    _rVal += 1;
                                }
                            }
                        }
                    }
                }
            }
            return _rVal;
        }
        public string Names(string aDelimiter = ",")
        {
            aDelimiter ??= ",";
            string _rVal = string.Empty;
    
            for (int i = 1; i <= Count; i++)
            {
                mzUtils.ListAdd(ref _rVal, Item(i).Name, bSuppressTest: true, aDelimitor: aDelimiter, bAllowNulls: true);
            }
            return _rVal;
        }



     public uopHole Hole(dynamic aArrayNameOrIndex, dynamic aMemberFlagOrIndex) => Hole(aArrayNameOrIndex, aMemberFlagOrIndex, out int _, out int _);
      public double TotalArea(string aArrayName = "")
        {
            double _rVal = 0;
            foreach (uopHoles item in this)
            {
                if (mzUtils.ListContains(item.Name, aArrayName, ",", bReturnTrueForNullList: true)) { _rVal += item.Area(); }

            }
            
            return _rVal;
        }

        public int HoleCount(string aNamesList = "", uppHoleTypes aHoleType = uppHoleTypes.Any, dynamic aSuppressVal = null)
        {
            int _rVal = 0;
            uopHoles aHls;
           
            for (int i = 1; i <= Count; i++)
            {
                aHls = Item(i);
                if (mzUtils.ListContains(aHls.Name, aNamesList, bReturnTrueForNullList: true))
                {
                    _rVal += ItemCount(i, aHoleType, aSuppressVal);
                }
            }
            return _rVal;
        }

        public uopVectors Centers(string aNamesList = "", uppHoleTypes aHoleType = uppHoleTypes.Any, dynamic aSuppressVal = null)  
        {
            uopVectors _rVal = new uopVectors();

        
            bool bTestSup = aSuppressVal != null;
            bool aSup = false;
    
           
            if (bTestSup) { aSup = mzUtils.VarToBoolean(aSuppressVal); }

            for (int i = 1; i <= Count; i++)
            {
                uopHoles aHls = Item(i);
                if (mzUtils.ListContains(aHls.Name, aNamesList, bReturnTrueForNullList: true))
                {
                    for (int j = 1; j <= aHls.Centers.Count; j++)
                    {
                        uopHole aHl = aHls.Item(j);

                        bool bKeep = aHoleType == uppHoleTypes.Any || aHl.HoleType == aHoleType;
                        if (bTestSup)
                        {
                            if (aHl.Suppressed != aSup) { bKeep = false; }
                        }
                        if (bKeep)
                        {
                            _rVal.Add(new UVECTOR(aHl.Center));
                        }
                    }
                }
            }
            return _rVal;
        }
        public colDXFVectors DXFCenters(string aNamesList = "", uppHoleTypes aHoleType = uppHoleTypes.Any, dynamic aSuppressVal = null)
        {
            colDXFVectors _rVal = new colDXFVectors();

            uopHoles aHls;
            bool bTestSup = aSuppressVal != null;
            bool aSup = false;
            bool bKeep = false;
            uopHole aHl;
            if (bTestSup) { aSup = mzUtils.VarToBoolean(aSuppressVal); }

            for (int i = 1; i <= Count; i++)
            {
                aHls = Item(i);
                if (mzUtils.ListContains(aHls.Name, aNamesList, bReturnTrueForNullList: true))
                {
                    for (int j = 1; j <= aHls.Centers.Count; j++)
                    {
                        aHl = aHls.Item(j);

                        bKeep = aHoleType == uppHoleTypes.Any || aHl.HoleType == aHoleType;
                        if (bTestSup)
                        {
                            if (aHl.Suppressed != aSup) { bKeep = false; }
                        }
                        if (bKeep)
                        {
                            _rVal.Add(aHl.CenterDXF);
                        }
                    }
                }
            }
            return _rVal;
        }

        public List<double> GetOrdinates(dxxOrdinateDescriptors aOrdType, List<string> aMemberNames = null)
        {
            List<double> _rVal = new List<double>();
            foreach (uopHoles mems in this)
            {
                if (aMemberNames != null)
                {
                    if (aMemberNames.FindIndex((x) => string.Compare(x, mems.Name, true) == 0) < 0) continue;
                }
                for (int j = 1; j <= mems.Count; j++)
                {
                    uopVector u1 = mems.Item(j).Center;

                    double ord = aOrdType switch { dxxOrdinateDescriptors.X => u1.X, dxxOrdinateDescriptors.Y => u1.Y, dxxOrdinateDescriptors.Z => u1.Elevation.HasValue ? u1.Elevation.Value : 0, _ => 0 };
                    _rVal.Add(ord);
                }

            }
            return _rVal;
        }
        public override string ToString() => $"uopHoleArray[{ MemberNamesList }]";

    }
}