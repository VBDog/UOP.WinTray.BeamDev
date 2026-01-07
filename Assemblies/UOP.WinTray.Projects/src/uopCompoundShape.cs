using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Structures;

namespace UOP.WinTray.Projects
{
    /// <summary>
    /// a Shape that also carries a collection of other shapes (Islands or sub-Shapes) 
    /// </summary>
    public class uopCompoundShape : uopShape, ICloneable
    {

        #region Constructors
        public uopCompoundShape() => Init();

        public uopCompoundShape(string aName) => Init(aName: aName);
        public uopCompoundShape(uopShape aShape) => Init(aShape);

        private void Init(uopShape aShape = null,  IEnumerable<uopShape> aSubShapes = null,  string aName = null)
        {

            _SubShapes = new uopShapes();
            Copy(aShape, aSubShapes);
            if (aName != null) Name = aName;
        }
        public uopCompoundShape(IEnumerable<iVector> aVerts, string aName = "", string aTag = "", bool? bOpen = null) => Init(new USHAPE(aName, Tag), aVertices: aVerts, bOpen: bOpen);
        #endregion Constructors 

        #region Properties

        internal uopShapes _SubShapes;
        public uopShapes SubShapes { get { _SubShapes ??= new uopShapes(); return _SubShapes; } set => _SubShapes = value; }

        #endregion Properties

        #region Methods
        public new uopCompoundShape Clone() => new uopCompoundShape(this);
            
        object ICloneable.Clone() => new uopCompoundShape(this);
            
        public void Copy(uopShape aShape, IEnumerable<uopShape> aSupShapes = null)
        {
            if (aShape == null) return;
            base.Init(null, aShape);
            
            if (aShape is uopCompoundShape)
            {
                uopCompoundShape cshape = (uopCompoundShape)aShape;
                if(aSupShapes == null) aSupShapes = cshape._SubShapes;
            }
            
            if(aSupShapes != null)
                SubShapes.Populate(aSupShapes, bAddClones: true);
           
        }
        public override bool Mirror(double? aX, double? aY)
        {
            if (!aX.HasValue && !aY.HasValue) return false;
            base.Mirror(aX, aY);
            bool _rVal = base.Mirror(aX, aY);
            foreach (var subshape in SubShapes)
                  subshape.Mirror(aX, aY);
  
            if (_rVal) Update();
            return _rVal;

        }
        public override string ToString() => string.IsNullOrWhiteSpace(Name) ? $"uopCompoundShape[{Vertices.Count}]" : $"uopCompoundShape[{Vertices.Count}] '{Name}'";

        public override void Translate(uopVector aTranslation)
        {
            if (aTranslation == null || aTranslation.IsNull()) return;
            base.Translate(aTranslation);
            foreach (var subshape in SubShapes)
                subshape.Translate(aTranslation);
        }
        /// <summary>
        /// moves the shape per the passed distances
        /// </summary>
        /// <param name="dX"></param>
        /// <param name="dY"></param>
        public override void Move(double dX = 0, double dY = 0)
        {
            if (dX == 0 && dY == 0) return;
           base.Move(dX, dY);
            foreach (var subshape in SubShapes)
                subshape.Move(dX, dY);
            Update();
        }

        #endregion Methods
    }
}
