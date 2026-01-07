using UOP.DXFGraphics;
using System;
using System.Collections.Generic;
using UOP.WinTray.Projects.Structures;
using System.Runtime.Remoting.Messaging;
using UOP.WinTray.Projects.Enums;


namespace UOP.WinTray.Projects
{
    /// <summary>
    /// Get or set the Shape
    /// </summary>
    public class uopShapes : List<uopShape>, ICloneable
    {

        #region Constructors

        public uopShapes() { }

        public uopShapes(string aName) { Init(); Name = aName; }
        public uopShapes(uppSubShapeTypes aType) { Init(); Name = aType.GetDescription(); ShapeType = aType; }

        internal uopShapes(USHAPES aShapes)
        {
            Init();
            Name = aShapes.Name;
            for (int i = 1; i <= aShapes.Count; i++)
            {
                Add(new uopShape(aShapes.Item(i)));
            }
        }


        internal uopShapes(UARCRECS aShapes, string aName = "")
        {
            Init();
            Name = (aName != null) ? aName : "";
            for(int i = 1; i <= aShapes.Count; i++ )
            {
                UARCREC aShape = aShapes.Item(i);   
                Add(new uopShape(aShape));
            }


        }

        internal uopShapes(IEnumerable<USHAPE> aShapes, string aName = "")
        {
            Init();
            Name = (aName != null) ? aName : "";
            foreach (USHAPE aShape in aShapes)
            {
                Add(new uopShape(aShape));
            }


        }

        public uopShapes(IEnumerable<uopShape> aShapes, bool bCloneMembers = true )
        {
            Init();
            if (aShapes == null) return;
            if(aShapes.GetType() == typeof(uopShapes))
            {
                uopShapes uop = (uopShapes)aShapes;
                Name = uop.Name;
                ShapeType = uop.ShapeType;
            }
            
            foreach (uopShape aShape in aShapes) { Add(bCloneMembers ? aShape.Clone() : aShape); }
          }

        private void Init()
        {
            Clear();
            Name = string.Empty;
            ShapeType = uppSubShapeTypes.Undefined;
        }
        #endregion Constructors

        #region Properties

        public  uppSubShapeTypes ShapeType { get; set; }

        public string Name { get; set; }

        public double Area
        {
            get
            {
                double _rVal = 0;
                foreach (uopShape item in this)
                {
                    _rVal += item.Area;
                }
                return _rVal;
            }

        }

        #endregion Properties

        #region Methods

        public uopArcRecs ArcRecs(bool? aSuppressedValue = null )
        {
            uopArcRecs _rVal = new uopArcRecs();
            foreach(var mem in this)
            {
                if (aSuppressedValue.HasValue && mem.Suppressed != aSuppressedValue) continue;
                if (mem.IsCircle)
                {
                    _rVal.Add (new uopArc(mem.Radius, mem.Center) { Tag = mem.Tag, Flag = mem.Flag });
                } 
                else if (mem.IsRectangular()) 
                {
                    _rVal.Add(new uopRectangle(mem));
                }
            }
            return _rVal;
        }
        public int TotalOccurances( bool? bVirtualValue = null) => uopShapes.GetTotalOccrances(this, bVirtualValue);
        public double TotalArea(bool bIncludeOccuranceFactor = false, bool? bVirtualValue = null) => uopShapes.GetTotalArea(this, bIncludeOccuranceFactor,bVirtualValue);
        public double TotalLinePairLength(bool bIncludeOccuranceFactor = false, bool? bVirtualValue = null) => uopShapes.GetTotalLinePairLength(this, bIncludeOccuranceFactor, bVirtualValue);

        public colDXFEntities ToDXFEntities(string aLayerName = "", dxxColors aColor = dxxColors.Undefined, string aLinetype = "")
        {
            colDXFEntities _rVal = new colDXFEntities();
            foreach (uopShape item in this)
            {
                _rVal.Add(item.ToDXFEntity(aLayerName, aColor, aLinetype));
            }
            return _rVal;

        }


        /// <summary>
        /// Add's a shape
        /// </summary>
        /// <param name="aShape"></param>
        public new void Add(uopShape aShape) { if (aShape != null) base.Add(aShape); }

        /// <summary>
        /// Add's a shape
        /// </summary>
        /// <param name="aShape"></param>
        internal void Add(USHAPE aShape) { base.Add(new uopShape(aShape)); }

        public void Append(IEnumerable<uopShape> aShapes, bool bAddClones = false, int? aOccuranceFactor = null)
        {
            if (aShapes == null) return;
            foreach (uopShape aShape in aShapes)
            {
                if (aShape == null) continue;
                uopShape newmem = !bAddClones ? aShape : new uopShape(aShape);
                if (aOccuranceFactor.HasValue) newmem.OccuranceFactor = aOccuranceFactor.Value;
               Add(newmem);
            }
        }

        public void Populate(IEnumerable<uopShape> aShapes, bool bAddClones = true, int? aOccuranceFactor = null)
        {
            Clear();
            if (aShapes == null) return;

            foreach (uopShape aShape in aShapes)
            {
                if (aShape == null) continue;
                uopShape newmem = !bAddClones ? aShape : new uopShape(aShape);
                if (aOccuranceFactor.HasValue) newmem.OccuranceFactor = aOccuranceFactor.Value;
                Add(newmem);
            }
        }

        internal void Append(USHAPES aShapes)
        {
            for (int i = 1; i <= aShapes.Count; i++)
            {
                Add(new uopShape(aShapes.Item(i)));
            }
        }
        /// <summary>
        /// Get's Shape member
        /// </summary>
        /// <param name="aIndex"></param>
        /// <returns></returns>

        public uopShape Item(int aIndex)
        {
            if (aIndex < 0 || aIndex > Count) throw new IndexOutOfRangeException();
            return this[aIndex - 1];
        }

        /// <summary>
        /// the tags of the shapes in the array
        /// </summary>
        /// <param name="bUnique"></param>
        /// <returns></returns>
        public List<string> Tags(bool bUnique)
        {
            List<string> _rVal = new List<string>();
            string sval;
            foreach (uopShape item in this)
            {
                sval = item.Tag;
                if (!bUnique)
                {
                    _rVal.Add(sval);
                }
                else
                {
                    if (_rVal.FindIndex(x => string.Compare(x, sval, true) == 0) < 0) _rVal.Add(sval);
                }

            }
            return _rVal;
        }

        /// <summary>
        /// Add shapes with name
        /// </summary>
        /// <param name="bUnique"></param>
        /// <returns></returns>
        public List<string> Names(bool bUnique)
        {
            List<string> _rVal = new List<string>();
            string sval;
            foreach (uopShape item in this)
            {
                sval = item.Name;
                if (!bUnique)
                {
                    _rVal.Add(sval);
                }
                else
                {
                    if (_rVal.FindIndex(x => string.Compare(x, sval, true) == 0) < 0) _rVal.Add(sval);
                }

            }
            return _rVal;
        }
        public uopShapes Clone() => new uopShapes(this,bCloneMembers:true);

        object ICloneable.Clone() => new uopShapes(this, bCloneMembers: true);

        #endregion Methods

        #region Shared Methods
        public static double GetTotalArea(IEnumerable<uopShape> aShapes, bool bIncludeOccuranceFactor = false, bool? bVirtualValue = null)
        {
            if (aShapes == null ) return 0;
            double _rVal = 0;
            foreach (uopShape item in aShapes)
            {
                if (item == null) continue;
                if (bVirtualValue.HasValue && item.IsVirtual != bVirtualValue.Value) continue;
                _rVal += !bIncludeOccuranceFactor ? item.Area : item.Area * item.OccuranceFactor;
            }
            return _rVal;
        }

        public static int GetTotalOccrances(IEnumerable<uopShape> aShapes, bool? bVirtualValue = null)
        {
            if (aShapes == null) return 0;
            int _rVal = 0;
            foreach (uopShape item in aShapes)
            {
                if (item == null) continue;
                if (bVirtualValue.HasValue && item.IsVirtual != bVirtualValue.Value) continue;
                _rVal += item.OccuranceFactor;
            }
            return _rVal;
        }

        public static double GetTotalLinePairLength(IEnumerable<uopShape> aShapes, bool bIncludeOccuranceFactor = false, bool? bVirtualValue = null)
        {
            if (aShapes == null) return 0;
            double _rVal = 0;
            foreach (uopShape item in aShapes)
            {
                if (item == null) continue;
                if (bVirtualValue.HasValue && item.IsVirtual != bVirtualValue.Value) continue;
                double adder = item.LinePair.TotalLength;
                _rVal += !bIncludeOccuranceFactor ? adder : adder * item.OccuranceFactor;
            }
            return _rVal;
        }

      
        #endregion Shared Methods
    }
}