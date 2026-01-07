using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UOP.WinTray.Projects
{
    public class uopShapeArray : List<uopShapes>, IEnumerable<uopShapes>, ICloneable
    {

        #region Constructors

        public uopShapeArray() => Init();
        public uopShapeArray(IEnumerable<uopShapes> aShapes) => Init(aShapes); 
        private void Init(IEnumerable<uopShapes> aShapes = null)
        {
            if(aShapes != null) Copy(aShapes);
        }
        #endregion Constructors

        #region Methods

        public void Copy(IEnumerable<uopShapes> aShapes)
        {
            Clear();
            if (aShapes == null) return;

            if(aShapes.GetType() != typeof(uopShapeArray)) 
            { 
            }

            foreach(var item in aShapes)
               if (item != null)  Add(new uopShapes(item), item.Name);
            
        }

        public bool TryGet(string aName, out uopShapes rShapes)
        {
            rShapes = Item(aName);
            return rShapes != null;
        }

        public uopShapes Item(int aIndex)
        {
            if(aIndex <1 || aIndex > Count) throw new ArgumentOutOfRangeException("index");
            return base[aIndex - 1];

        }

        public uopShapes Item(string aName)
        {
            if(string.IsNullOrWhiteSpace(aName)) return null;
            return  Find(x => string.Compare(aName, x.Name,true) == 0);

        }

        public void Add(IEnumerable<uopShape> aShapes, string aName = null, int? aOccuranceFactor = null, bool bAddClones = false)
        {
            if (aShapes == null) return;
            uopShapes ushapes = null;
            if (aShapes.GetType() == typeof(uopShapes))
            {
                ushapes = (uopShapes) aShapes;
                if (string.IsNullOrEmpty(ushapes.Name)) ushapes.Name = $"MEMBER_{Count + 1}";
               
            }
            else
            {
                ushapes = new uopShapes($"MEMBER_{Count + 1}");
                foreach (var item in aShapes) if (item != null) ushapes.Add(item);
            }
            
            if (ushapes != null)
            {
                if (!string.IsNullOrWhiteSpace(aName)) ushapes.Name = aName;
                if (aOccuranceFactor.HasValue)
                {
                    foreach(var item in aShapes) item.OccuranceFactor = aOccuranceFactor.Value;
                }
                base.Add(!bAddClones ? ushapes : new uopShapes(ushapes));
            }
        }

        public uopShapeArray Clone() => new uopShapeArray(this);

        object ICloneable.Clone() => new uopShapeArray(this);

        #endregion Methods

        #region Shared Methods

        public static uopShapeArray CloneCopy(uopShapeArray aShapeArray) => aShapeArray == null ? null : new uopShapeArray(aShapeArray);

        #endregion Shared Methods
    }
}
