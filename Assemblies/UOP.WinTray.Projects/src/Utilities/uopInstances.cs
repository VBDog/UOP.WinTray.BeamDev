using System;
using System.Collections.Generic;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Structures;
using static System.Collections.Specialized.BitVector32;

namespace UOP.WinTray.Projects.Utilities
{



    public class uopInstances : List<uopInstance>, IEnumerable<uopInstance>, ICloneable
    {

        private WeakReference<uopPart> _OwnerPtr = null;
        // private TINSTANCES _Struc = new TINSTANCES("");
    


        #region Constructors
        public uopInstances()
        {
            Name = string.Empty;
            base.Clear();
            _OwnerPtr = null;
       
        }
        public uopInstances(string aName = "", uopPart aOwner = null)
        {
            _BasePt = new uopVector();
            Name = String.IsNullOrWhiteSpace(aName) ? string.Empty : Name = aName.Trim();
            base.Clear();
            _OwnerPtr = (aOwner == null) ? null : new WeakReference<uopPart>(aOwner);
            if (aOwner != null)
            {
                _BasePt.SetCoordinates(aOwner.X, aOwner.Y);
                _BasePt.PartIndex = aOwner.Index;
            }

        }

        internal uopInstances(TINSTANCES aStructure, uopPart aOwner = null, string aName = null)
        {
            base.Clear();
            BasePt = new uopVector(aStructure.BasePt);
            BaseRotation = aStructure.BaseRotation;
            Name = aStructure.Name;
            for (int i = 1; i <= aStructure.Count; i++)
            {
                base.Add(new uopInstance(aStructure.Item(i)));
            }

            if (!string.IsNullOrWhiteSpace(aName)) Name = aName;
            _OwnerPtr = (aOwner == null) ? null : new WeakReference<uopPart>(aOwner);
            if (aOwner != null)
            {
                _BasePt.SetCoordinates(aOwner.X, aOwner.Y);
                _BasePt.PartIndex = aOwner.Index;
            }
            //if (aOwner != null) BasePtDXF = aOwner.CenterDXF;
            PartIndex = aStructure.PartIndex;
        }
        public uopInstances(uopInstances aInstances, uopPart aOwner = null, string aName = null)
        {
            base.Clear();
            if (aInstances != null)
            {
            
                BasePt = aInstances.BasePt.Clone();
                for (int i = 1; i <= aInstances.Count; i++)
                {
                    base.Add(new uopInstance( aInstances.Item(i)));
                }
            }

            if (!string.IsNullOrWhiteSpace(aName)) Name = aName;
            _OwnerPtr = (aOwner == null) ? null : new WeakReference<uopPart>(aOwner);
            //if (aOwner != null) BasePtDXF = aOwner.CenterDXF;
       
        }

        #endregion Constructors

        #region Properties
        public string Name { get; set; }

        public int PartIndex { get => BasePt.PartIndex; set => BasePt.PartIndex = value; }
        public double BaseRotation { get => BasePt.Value; set { BasePt.Value = mzUtils.NormAng(value, ThreeSixtyEqZero: true); UpdateOwner(); } }
        private uopVector _BasePt;
        internal uopVector BasePt { get { _BasePt ??= new uopVector(); return _BasePt; } set { _BasePt ??= new uopVector(); value ??= new uopVector();  if(_BasePt.SetCoordinates( value.X,value.Y)) UpdateOwner(); } }

        public dxfVector BasePtDXF { get => BasePt.ToDXFVector(bValueAsRotation: true); set { BasePt = new uopVector(value); } } // if (value == null) { BasePt.SetCoordinates(0, 0); } else { BasePt.SetCoordinates(value.X, value.Y); } } }
       public uopPart Owner
        {
            get
            {

                if (_OwnerPtr == null) return null;
                if (!_OwnerPtr.TryGetTarget(out uopPart myOwner)) { _OwnerPtr = null; return null; }
                return myOwner;

            }
            set
            {
                if (value == null)
                {
                    _OwnerPtr = null;
                    return;
                }
                _OwnerPtr = new WeakReference<uopPart>(value);
                BasePt.PartIndex = value.Index;
            }
        }

        public double X { get => BasePt.X; set => BasePt.Y = value; }

        public double Y { get => BasePt.Y; set => BasePt.Y = value; }
        
        #endregion Properties

        #region Methods

        public override string ToString() => $"uopInstances[{Count}]";

        object ICloneable.Clone() => (object)this.Clone();
        public uopInstances Clone() => new uopInstances(this);

        public uopInstance Item(int aIndex)
        {
            if (aIndex < 1 || aIndex > Count) return null;
            base[aIndex - 1].BasePt = new UVECTOR(BasePt);
            return base[aIndex - 1];
        }

        public bool Update(int aIndex, uopInstance aMember)
        {
            if (aIndex < 1 || aIndex > Count) return false;
            if (aMember == null) return false;
            bool _rVal = Update(aIndex, aMember);
            UpdateOwner();
            return _rVal;
        }

        public new void Add(uopInstance aInstance)
        {
            if (aInstance == null) return;
            aInstance.BasePt = new UVECTOR(BasePt);
            base.Add(aInstance);
            UpdateOwner();
        }

        public void Add(uopInstance aInstance, uopPart aOwner)
        {
            if (aInstance == null) return;
            aInstance.BasePt = new UVECTOR(BasePt);
            base.Add(aInstance);
            UpdateOwner(aOwner);
        }
        public colDXFVectors MemberPointsDXF(dxfVector aBasePt = null, bool aReturnBasePt = true, bool aCoordinatesOnly = false, bool bAltRing = false, double aRotationAdder = 0)
        {

            colDXFVectors _rVal = new colDXFVectors();
            try
            {
                dxfVector bpt = aBasePt ?? BasePtDXF;
                dxfVector ip = new dxfVector(bpt) { Rotation = bpt.Rotation + aRotationAdder };
                if (aReturnBasePt) _rVal.Add(ip);

                for (int i = 1; i <= Count; i++)
                {
                    uopInstance mem = Item(i);

                    ip = MemberPointDXF(i, bpt, aCoordinatesOnly, aRotationAdder);

                   if((!bAltRing && mem.ExistsOnRing1) || (bAltRing && mem.ExistsOnRing2)) _rVal.Add(ip);
                }

            }
            catch
            {

            }

            return _rVal;


        }


        public List<uopInstance> GetAtOrdinates(List<double> aOrdinates, dxxOrdinateDescriptors aOrdType = dxxOrdinateDescriptors.X, int aPrecis = 3, bool bRemove = false, uopPart aOwner = null)
        {
            List<uopInstance> _rVal = new List<uopInstance>();

            if (aOrdinates == null) return _rVal;
            aPrecis = mzUtils.LimitedValue(aPrecis, 0, 15);
            for(int i = 1; i <= Count; i++)
            {
                uopVector mp = MemberPoint(i);
                if(aOrdType == dxxOrdinateDescriptors.X)
                {
                    if(aOrdinates.FindIndex((x) => Math.Round(x,aPrecis) == Math.Round(mp.X,aPrecis)) >= 0)
                    {
                        _rVal.Add(Item(i));
                    }
                }
                else if (aOrdType == dxxOrdinateDescriptors.Y)
                {
                    if (aOrdinates.FindIndex((x) => Math.Round(x, aPrecis) == Math.Round(mp.Y, aPrecis)) >= 0)
                    {
                        _rVal.Add(Item(i));
                    }

                }

            }
            if (bRemove)
            {
                foreach (uopInstance item in _rVal)
                {
                    Remove(item);
                }
                if (_rVal.Count > 0) UpdateOwner(aOwner);
            }
            return _rVal;
        }
        public List<uopInstance> GetByPartIndex(int aPartIndex, bool bRemove = false, uopPart aOwner = null)
        {
            List<uopInstance> _rVal = FindAll((x) => x.PartIndex == aPartIndex);

            if (bRemove && _rVal.Count > 0)
            {
                foreach (uopInstance item in _rVal)
                {
                    Remove(item);
                }
               UpdateOwner(aOwner);
            }
            return _rVal;
        }

        public List<uopInstance> GetByPartIndex(List<int> aPartIndexes, bool bRemove = false, uopPart aOwner = null)
        {
            if (aPartIndexes == null) return new List<uopInstance>();

            List<uopInstance> _rVal = FindAll((x) => aPartIndexes.Contains(x.PartIndex));

            if (bRemove && _rVal.Count > 0)
            {
                foreach (uopInstance item in _rVal)
                {
                    Remove(item);
                }
                UpdateOwner(aOwner);
            }
            return _rVal;
        }


        public uopVectors MemberPoints(iVector aBasePt = null, bool aReturnBasePt = true, bool aCoordinatesOnly = false, List<uopInstance> aSubSet = null, uppAlternateRingTypes? aAltRing = null, bool? bVirtual = null, int? aPartIndex = null)
        {

            uopVectors _rVal = new uopVectors();
            try
            {
                aBasePt ??= BasePt;
                uopVector bpt = new uopVector( aBasePt);
                aSubSet ??= this;
                if (aReturnBasePt)
                {
                    bpt.PartIndex = PartIndex;
                    if(!aPartIndex.HasValue || aPartIndex.Value == bpt.PartIndex)
                        _rVal.Add(bpt);
                }
                for (int i = 1; i <= aSubSet.Count; i++)
                {
                    uopInstance mem = aSubSet[i - 1];
                    if (bVirtual.HasValue && mem.Virtual != bVirtual.Value) continue; // skip virtual if not matching
                    if (aAltRing.HasValue &&  mem.AltRingType != aAltRing.Value) continue; // skip if alternate ring and not on ring 2
                    if (aPartIndex.HasValue  && mem.PartIndex != aPartIndex.Value) continue;

                    uopVector ip = MemberPoint(IndexOf(mem), bpt, aCoordinatesOnly);
                    _rVal.Add(ip);
                }

            }
            catch
            {

            }

            return _rVal;


        }

        public new int IndexOf(uopInstance aMember)
        {
            if (aMember == null) return 0;
            return base.IndexOf(aMember) + 1;
        }

        public bool AddByVector(iVector aVector, iVector aBaseVector, bool bUseZAsRotation = false, bool bSuppressRotations = false, bool bUseZAsScaleFactor = false, bool bUseInvertedFlag = false)
        {

            bool _rVal = false;

            if (aVector == null) return false;

            dxfVector bv;
            dxfVector v1 = new dxfVector(aVector);
            dxfVector dif;
            uopInstance aMem;
            uopPart myOwner = Owner;

            if (aBaseVector == null) { bv = BasePtDXF; } else { bv = new dxfVector(aBaseVector); }
            dif = v1 - bv;
            if (bUseZAsRotation) bUseZAsScaleFactor = false;
            if (bUseZAsRotation || bUseZAsScaleFactor) dif.Z = 0;

            if (!dif.IsZero(4))
            {

                _rVal = true;
                aMem = new uopInstance(dif.X, dif.Y);
                if (bUseInvertedFlag) aMem.Inverted = v1.Inverted;
                if (!bSuppressRotations) aMem.Rotation = mzUtils.NormAng(v1.Rotation, false, true, true);
                if (bUseZAsScaleFactor) {

                    if (v1.Z != 0) { aMem.ScaleFactor = Math.Abs(v1.Z); } else { aMem.ScaleFactor = 1; }
                }

                if (bUseZAsRotation) {
                    aMem.Rotation = mzUtils.NormAng(v1.Z, false, true, true);
                }

                Add(aMem, myOwner);
              
            }
            return _rVal;

        }
        public dxfVector MemberPointDXF(int aIndex, dxfVector aBasePt = null, bool aCoordinatesOnly = false, double aRotationAdder = 0)
        {

        

            if (aIndex < 1 || aIndex > Count) return null;

            try
            {

                uopInstance aMem;
                uopVector v1 = aBasePt == null ? BasePt.Clone() : new uopVector(aBasePt);
                aMem = Item(aIndex);

                v1 += new uopVector(aMem.DX, aMem.DY) { Tag = aMem.Tag };

                dxfVector _rVal = v1.ToDXFVector();
                if (!aCoordinatesOnly)
                {
                    _rVal.Inverted = aMem.Inverted;
                    _rVal.Rotation = aMem.Rotation + BaseRotation + aRotationAdder;

                }
                
                return _rVal;
            }
          

            catch
            {
                return null;
            }


          
        }
        public uopVector MemberPoint(int aIndex, uopVector aBasePt = null, bool aCoordinatesOnly = false)
        {
            uopVector _rVal = null;
            if (aIndex < 1 || aIndex > Count) return null;

            try
            {

                
                _rVal = aBasePt  == null ? new uopVector(BasePt) : new uopVector(aBasePt) ;
                uopInstance aMem = Item(aIndex);

                _rVal.X += aMem.DX;
                _rVal.Y += aMem.DY;
                _rVal.Tag = aMem.Tag;
                _rVal.Virtual = aMem.Virtual;

                if (!aCoordinatesOnly)
                {
                 
                    _rVal.Value = aMem.Rotation + BaseRotation;

                }
                _rVal.Tag = aMem.Tag;
                _rVal.PartIndex = aMem.PartIndex;
            }

            catch
            {
                _rVal = null;
            }


            return _rVal;
        }

        public new  bool Clear()
        {
            bool _rVal = Count > 0;
            base.Clear();
            if (_rVal)
            {
                UpdateOwner();
                return true;
            }
            else { return false; }
        }
       public void Add(iVector aDisplacement, double aRotation = 0, bool bInverted = false, uppAlternateRingTypes altRing = uppAlternateRingTypes.AllRings, int aPartIndex = 0,bool bVirtual = false)
        {
            if (aDisplacement == null) return;
            if (aDisplacement.X != 0 || aDisplacement.Y != 0) Add(new uopInstance(aDisplacement.X, aDisplacement.Y, aRotation, bInverted, aPartIndex: aPartIndex, altRing: altRing,bVirtual:bVirtual), Owner);
        }
        public void Append(uopInstances aInstances, bool bSwapInvertedY = false ,bool? bVirtual = null)
        {
            if (aInstances == null) return;
            if (aInstances.Count <= 0) return;
            uopVector u0 = BasePt;
            int cnt = Count;
            uopVector u1 = aInstances.BasePt;
            double dx = Math.Round(u1.X - u0.X, 4);
            double dy = Math.Round(u1.Y - u0.Y, 4);
            uopInstance newmem = new uopInstance(dx, dy);
            if(bVirtual.HasValue) newmem.Virtual = bVirtual.Value;
            else newmem.Virtual = aInstances.Item(1).Virtual;
            if (bSwapInvertedY)
            {
                if((u1.Y > 0 && u0.Y < 0) || (u1.Y < 0 && u0.Y> 0))
                {
                    newmem.Inverted = true;
                }
            }
            if (dx != 0 || dy != 0)
            {
                newmem.BasePt = new UVECTOR(BasePt);
                base.Add(newmem);
            }
                
            

            foreach (uopInstance item in aInstances)
            {
                u1 = aInstances.BasePt + new uopVector(item.DX, item.DY);
                dx = Math.Round(u1.X - u0.X, 4);
                dy = Math.Round(u1.Y - u0.Y, 4);
                if(dx != 0 || dy != 0)
                {
                    newmem = item.Clone();
                    newmem.DX = dx;
                    newmem.DY = dy;
                    newmem.BasePt =  new UVECTOR(BasePt);
                    base.Add(newmem);

                    if (bSwapInvertedY)
                    {
                        
                         newmem.Inverted = !newmem.Inverted;
                        
                    }

                }

            }
            if (cnt != Count) 
                UpdateOwner();

        }

        public List<uopInstance> AppendMirrors(double? aX = null, double? aY  = null, bool bSwapInvertedY = false)
        {

            List<uopInstance> _rVal = new List<uopInstance>();
            if (!aX.HasValue && !aY.HasValue) return _rVal;

            uopPart owner = this.Owner;
            Owner = null;

            uopVector u0 = BasePt.Clone();
            uopVectors vecs = this.MemberPoints(u0, true);
            int cnt = Count;
            int vcnt = vecs.Count;
            int j = 0;
            if (aX.HasValue)
            {
                for(int i = 1; i <= vcnt; i ++)
                {

                    uopInstance inst = i -1 >= 1 && i-1 <= Count ? Item(i - 1) : new uopInstance(0,0,u0.Value);
                    uopVector u1 = vecs.Item(i);
                    uopVector u2 = u1.Mirrored(aX, null);
                    uopVector offset = u2 - u0;
                  
                    if (offset.X != 0)
                    {
                        Add(offset,inst.Rotation + 0, !inst.Inverted);
                        _rVal.Add(Item(Count));
                        j++;
                        vecs.Add(u2);
                    }
                }

            }

            if (aY.HasValue)
            {
                for (int i = 1; i <= vcnt + j; i++)
                {
                    uopInstance inst = i - 1 >= 1 && i - 1 <= Count ? Item(i - 1) : new uopInstance(0, 0, u0.Value);
                    uopVector u1 = vecs.Item(i);
                    uopVector u2 = u1.Mirrored(null, aY);
                    uopVector offset = u2 - u0;
                    if (offset.Y != 0)
                    {
                        Add(offset, inst.Rotation + 180, !inst.Inverted);
                        _rVal.Add(Item(Count));
                        j++;
                        vecs.Add(u2);
                    }
                }

            }


            this.Owner = owner;
          
            if (cnt != Count)
                UpdateOwner(owner);

            return _rVal;
        }

        public bool SetRotations(double aRotation, bool bAddIt = false, List<uopInstance> aSubset = null)
        {
            bool _rVal = false;
            if (aSubset == null) aSubset = this;

            double wuz;
            foreach (uopInstance mem in aSubset)
            {
                wuz = mem.Rotation;

                if (!bAddIt)
                {
                    mem.Rotation = aRotation;
                }
                else
                {
                    mem.Rotation += aRotation;
                }

                if (wuz != mem.Rotation) _rVal = true;
            }


            if (_rVal) UpdateOwner();
            return _rVal;

        }


        public List<uopInstance> AppendRotations(uopVector aCenter, double aRotation, List<uopInstance> aSubSet = null, bool bVirtual = false)
        {

            List<uopInstance> _rVal = new List<uopInstance>();
            aCenter ??= new uopVector();
          

             uopPart owner = this.Owner;
            Owner = null;

            aSubSet ??= this;
            uopVectors vecs = this.MemberPoints(BasePt, true,aSubSet: aSubSet);
      
            int cnt = Count;
            int vcnt = vecs.Count;
            uopVector bp = BasePt;
            for (int i = 1; i <= vcnt; i++)
            {

                uopInstance inst = i - 1 >= 1 && i - 1 <= Count ? Item(i - 1) : new uopInstance(0, 0, bp.Value);
                uopVector v1 = vecs.Item(i);
                v1.Rotate(aCenter, aRotation);
                uopVector offset = v1 - bp;

                if (offset.X != 0 || offset.Y != 0)
                {
                    Add(offset, inst.Rotation + aRotation,bVirtual: bVirtual);
                    _rVal.Add(Item(Count));
                  
                }
            }
            this.Owner = owner;

            if (cnt != Count  && owner != null)
                UpdateOwner(owner);

            return _rVal;
        }

        public void AppendWithPoints(uopVectors aPoints, uopVector aBasePt = null,bool? bVirtual = null)
        {
            if (aPoints == null) return;
            if (aPoints.Count <= 0) return;
           
            aBasePt ??= BasePt;

            foreach (uopVector item in aPoints)
            {
                uopInstance newmem = new uopInstance(item.X - aBasePt.X, item.Y - aBasePt.Y, item.Value, bVirtual: !bVirtual.HasValue ? item.Virtual: bVirtual.Value)
                {
                    BasePt = new UVECTOR(aBasePt)
                };
                base.Add(newmem);
                
            }
            UpdateOwner();
        }
        public void AppendWithPoints(colDXFVectors aPoints, dxfVector aBasePt = null, bool? bVirtual = null)
        {
            if (aPoints == null) return;
            if (aPoints.Count <= 0) return;
          
            aBasePt ??= BasePtDXF;

            foreach (dxfVector item in aPoints)
            {
                uopInstance newmem = new uopInstance(item.X - aBasePt.X, item.Y - aBasePt.Y, item.Rotation, item.Inverted)
                {
                    BasePt = new UVECTOR(BasePt)
                };

                if (bVirtual.HasValue) newmem.Virtual = bVirtual.Value;
                base.Add(newmem);

            }
            UpdateOwner();
        }


        public void Add(double aDX, double aDY, double aRotation = 0, bool bInverted = false, bool bLeftHanded = false, int aPartIndex = 0, bool bVirtual = false, int aRow = 0, int aCol  = 0)
        {
            uopInstance newmem = new uopInstance(aDX, aDY, aRotation, bInverted, bLeftHanded: bLeftHanded, aPartIndex: aPartIndex, bVirtual: bVirtual, aRow:  aRow, aCol: aCol);
            Add(newmem, Owner);
        }

        public dxoInstances ToDXFInstances()
        {
            dxoInstances _rVal = new dxoInstances
            {
                BasePlane = new dxfPlane( X,Y)
            };
            if (Count <= 0) return _rVal;
            foreach (uopInstance item in this)
            {
                _rVal.Add(item.DX, item.DY, item.ScaleFactor, item.Rotation, item.Inverted, item.LeftHanded);
            }
            
            return _rVal;
        }
        internal bool UpdateOwner(uopPart aOwner = null)
        {
            if (aOwner != null) { _OwnerPtr = new WeakReference<uopPart>(aOwner);  }
            if (_OwnerPtr == null) return false;
            if (!_OwnerPtr.TryGetTarget(out uopPart myOwner)) { _OwnerPtr = null; return false; }
            myOwner.Instances = this;
            return true;

        }

        internal  UVECTORS ApplyTo(UVECTORS aVectors, bool bReturnBaseVectors = false, double? aMirrorX = null, double? aMirrorY = null)
        {
            UVECTORS _rVal = bReturnBaseVectors ? new UVECTORS(aVectors) : UVECTORS.Zero;
            if ( Count == 0) return _rVal;
            bool mirror = aMirrorX.HasValue || aMirrorY.HasValue;
            for (int idx = 1; idx <= Count; idx++) 
            {
                uopInstance item = this[idx -1];
                for (int i = 1; i <= aVectors.Count; i++)
                {
                    UVECTOR u1 = new UVECTOR(aVectors.Item(i)) { Row = item.Row, Col = item.Col};
                    u1.X += item.DX;
                    u1.Y += item.DY;
                    
                    if(mirror) 
                        u1.Mirror(aMirrorX, aMirrorY);

                    _rVal.Add(u1);
                }
            }
            return _rVal;
        }


        public uopVectors ApplyTo(uopVectors aVectors, bool bReturnBaseVectors = false, double? aMirrorX = null, double? aMirrorY = null)
        {
            uopVectors _rVal = bReturnBaseVectors ? new uopVectors(aVectors) : uopVectors.Zero;
            if(aVectors == null || Count == 0) return _rVal;
            bool mirror = aMirrorX.HasValue || aMirrorY.HasValue;
            for (int idx = 1; idx <= Count; idx++)
            {
                uopInstance item = this[idx - 1];
                for (int i = 1; i <= aVectors.Count; i++)
                {
                   UVECTOR u1 = new UVECTOR(aVectors.Item(i)) { Row = item.Row, Col = item.Col }; ;
                    u1.X += item.DX;
                    u1.Y += item.DY;
                    if (mirror) u1.Mirror(aMirrorX, aMirrorY);
                    _rVal.Add(u1);
                }
            }
            return _rVal;
        }
        #endregion Methods

        #region Shared Methods

        public static uopInstances FromDXFInstances(dxoInstances aInstances)
        {
            uopInstances _rVal = new uopInstances();
            if (aInstances == null) return _rVal;
            _rVal.BasePt =  new uopVector(aInstances.BasePlane.X, aInstances.BasePlane.Y);

             if (aInstances.Count <= 0) return _rVal;

            dxoInstance inst;
            for (int i = 1; i <= aInstances.Count; i++)
            {
                inst = aInstances.Item(i);
                _rVal.Add(new uopInstance(aInstances.Item(i)),null);
            }
            _rVal.BasePtDXF = aInstances.BasePlane.Origin;
            return _rVal;
        }

        public static uopInstances FromVectors(colDXFVectors aVectors, dxfVector aBaseVector, bool bSuppressRotations = false, bool bUseZAsScaleFactor = false, bool bUseInvertedFlag = false)
        {
            dxoInstances dinst = dxoInstances.FromVectors(aVectors, aBaseVector, bSuppressRotations, bUseZAsScaleFactor, bUseInvertedFlag);
            uopInstances _rVal = FromDXFInstances(dinst);
            if (aBaseVector != null) _rVal.BasePt = new uopVector(aBaseVector);
            return _rVal;
        }
        public static uopInstances FromVectors(uopVectors aVectors, uopVector aBaseVector, bool bSuppressRotations = false, bool bUseZAsScaleFactor = false, bool bUseInvertedFlag = false)
        {
            colDXFVectors vs = aVectors == null ? new colDXFVectors() : aVectors.ToDXFVectors(bValueAsRotation: true);
            dxfVector bv = aBaseVector == null ? aVectors.Count > 0 ? aVectors.Item(1).ToDXFVector(bValueAsRotation: true) : null : aBaseVector.ToDXFVector(bValueAsRotation: true);
            uopInstances _rVal = uopInstances.FromVectors( vs, bv,  bSuppressRotations, bUseZAsScaleFactor, bUseInvertedFlag); 
            return _rVal;
        }

        internal static uopInstances FromVectors(UVECTORS aVectors, UVECTOR aBaseVector, bool bSuppressRotations = false, bool bUseZAsScaleFactor = false, bool bUseInvertedFlag = false)
        {
            colDXFVectors vs = aVectors.ToDXFVectors(bValueAsRotation: true);
            dxfVector bv = aBaseVector.ToDXFVector(bValueAsRotation: true);
            uopInstances _rVal = uopInstances.FromVectors(vs, bv, bSuppressRotations, bUseZAsScaleFactor, bUseInvertedFlag);
            return _rVal;
        }

        public static uopInstances CloneCopy(uopInstances aInstances) => aInstances == null ? null : new uopInstances(aInstances); 

        public static void Combine(List<uopInstance> aInstances, List<uopInstance> bInstances, uopVector aOffsetAdder = null )
        {
            if(aInstances == null || bInstances == null || bInstances.Count ==0)  return;
           
            if(aOffsetAdder == null)
            {
                if(aInstances is uopInstances)
                {
                    uopInstances uinsts1 = (uopInstances)aInstances;
                    if (bInstances is uopInstances)
                    {
                        uopInstances uinsts2 = (uopInstances)bInstances;
                        aOffsetAdder = uinsts2.BasePt - uinsts1.BasePt;
                    }
                }
                
            }

            foreach (var inst in bInstances) 
            {
                uopInstance newinst = new uopInstance(inst);
                if (aOffsetAdder != null) { newinst.DX += aOffsetAdder.X; newinst.DY += aOffsetAdder.Y;  }
                    aInstances.Add(newinst);

            }

        }
        #endregion Shared Methods

   

    }
}
