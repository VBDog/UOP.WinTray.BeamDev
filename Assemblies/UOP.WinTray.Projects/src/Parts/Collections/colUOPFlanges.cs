using System;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;


namespace UOP.WinTray.Projects
{
    public class colUOPFlanges :ICloneable
    {
          private TFLANGES tStruc;

        #region Constructors
        
        public colUOPFlanges() { tStruc = new TFLANGES(""); }
        
        public colUOPFlanges(string aName) { tStruc = new TFLANGES(aName); }

        internal colUOPFlanges(TFLANGES aStructure) { tStruc = aStructure; }

        #endregion

        public colUOPFlanges Clone() => new colUOPFlanges(tStruc.Clone());

        object ICloneable.Clone() => (object)this.Clone();

        public int Count => tStruc.Count;
        
        internal TFLANGES Structure { get => tStruc; set => tStruc = value; }


        public uopFlange GetByDescriptor(string aDescriptor, out int rIndex)
        {
            rIndex = 0;
            TFLANGE aMem = tStruc.GetByDescriptor(aDescriptor, out rIndex);
            return (rIndex <= 0) ? new uopFlange(aMem) : null;
        }

        private uopFlange flng_Object(TFLANGE aFLANGE)
        {
            uopFlange flngObject = new uopFlange
            {
                Structure = aFLANGE
            };
            return flngObject;
        }

        TFLANGE_ARRAY SubSets()
        {
            TFLANGE_ARRAY SubSets = new TFLANGE_ARRAY();
            SubSets = flngs_Parse(ref tStruc);
            return SubSets;
        }

        //todo uopPiping.cs - Not required forcurrent release 
        private TFLANGE_ARRAY flngs_Parse(ref TFLANGES tStruc)
        {
            throw new NotImplementedException();
        }


        //^returns the item from the collection at the requested index
        public uopFlange Item(dynamic aIndex)
        {
            TFLANGE mem = new TFLANGE();
            int idx = 0;

            if (mzUtils.IsNumeric(aIndex))
            {
                mem = tStruc.Item(mzUtils.VarToInteger(aIndex));
                return (mem.Index <= 0) ? null : new uopFlange(mem);

            } else { return GetByDescriptor(Convert.ToString(aIndex), out idx); }

         
        }
    }
}
