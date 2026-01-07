using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Structures;

namespace UOP.WinTray.Projects
{
    public abstract class mdBoxSubPart : uopPart
    {
        #region Constructors

        public mdBoxSubPart() : base(uppPartTypes.Undefined, uppProjectFamilies.uopFamMD) { Init();  }
        public mdBoxSubPart(uppPartTypes aPartType) : base(aPartType, uppProjectFamilies.uopFamMD) { Init(); }

        private void Init() 
        {
            BoxCntr = UVECTOR.Zero;
            _DCBRef = null;

        }
        #endregion Constructors

        #region Properties
        /// <summary>
        /// the index of the box that this downcomer subpart belongs to
        /// </summary>
        public int BoxIndex { get; set; }

        private double DeckThickness { get; set; }

        private WeakReference<mdDowncomerBox> _DCBRef;
        public mdDowncomerBox DowncomerBox
        {
            get
            {
                mdDowncomerBox _rVal = null;
                if (_DCBRef != null)
                {
                    if (!_DCBRef.TryGetTarget(out _rVal)) _DCBRef = null;

                }
                if (_rVal == null && BoxIndex > 0 && DowncomerIndex > 0)
                {
                    mdTrayAssembly assy = GetMDTrayAssembly();
                    if (assy != null)
                    {
                        mdDowncomer dc = assy.Downcomers.Item(DowncomerIndex, bSuppressIndexError: true);
                        if (dc != null)
                        {
                            if (BoxIndex <= dc.Boxes.Count)
                            {
                                _rVal = dc.Boxes[BoxIndex - 1];
                                Initialize(null, _rVal);
                            }

                        }
                        else
                        {
                            DowncomerIndex = 0;
                        }
                    }
                }

                return _rVal;

            }

            set
            {
                if (value == null)
                {
                    _DCBRef = null;
                    return;
                }

                if (value != DowncomerBox || _DCBRef == null)
                {
                    Initialize(null, value);


                }
            }
        }

        public override mdDowncomer MDDowncomer
        {
            get
            {
                return DowncomerBox?.Downcomer;
            }

            set
            {
                // We leave it empty for now!
            }

        }

        
        internal UVECTOR BoxCntr { get; set; }

        public double BoxY{ get => BoxCntr.Y; }
        public double BoxX { get => BoxCntr.X; }

        #endregion Properties

        #region Methods
        internal abstract void Initialize(mdBoxSubPart aPartToCopy, mdDowncomerBox aBox);

        /// <summary>
        /// the parent downcomer of this spout group
        /// </summary>
        /// <param name="aAssy"></param>
        /// <returns></returns>
        public mdDowncomer Downcomer(mdTrayAssembly aAssy)
        {
            mdDowncomer _rVal = MDDowncomer;
            if(_rVal == null)
               _rVal = base.GetMDDowncomer(aAssy, null, DowncomerIndex);

            return _rVal;
        }

        public virtual  void SubPart(mdDowncomerBox aBox, string aCategory = null, bool? bHidden = null)
        {
            if (aBox == null) return;
            try
            {
                base.SubPart(aBox, aCategory, bHidden);
                _DCBRef = new WeakReference<mdDowncomerBox>(aBox);
                BoxIndex = aBox.Index;
                _DCBRef = new WeakReference<mdDowncomerBox>(aBox);
                SheetMetalStructure = aBox.SheetMetalStructure.Clone();
                DowncomerIndex = aBox.DowncomerIndex;
                AssociateToRange(aBox.RangeGUID);
                Row = aBox.Row;
                Col = aBox.Col;
                Category = "Sub Parts";
                AssociateToParent(aBox);
                base.PartIndex = aBox.Index;
                BoxCntr = new UVECTOR(aBox.Center);

            }
            catch (Exception e)
            {
                Debug.Fail(e.Message);
            }
            finally
            {


            }

        }

        #endregion Methods
    }
}

