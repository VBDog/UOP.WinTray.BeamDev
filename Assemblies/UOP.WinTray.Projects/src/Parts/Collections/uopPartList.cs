using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UOP.WinTray.Projects
{
    public class uopPartList<T> : List<T> where T : uopPart
    {
        #region Constructors
        public  uopPartList(IEnumerable<T> aParts)
            {
            _Invalid = false;
              foreach(var prt in aParts)
            {
                this.Add(prt);
            }
        }
        #endregion Constructors

        #region Properties
        private bool _Invalid;

        public bool Invalid { get => _Invalid || (InvalidWhenEmpty && Count ==0); set => _Invalid = value; }
        public bool InvalidWhenEmpty { get; set; }
        #endregion Properties

        #region Methods
        public void AssociateToRange(uopTrayRange aRange)
        {
            if (aRange == null) return;
            foreach (var item in this)
            {
                uopPart part = item as uopPart;
                part.AssociateToRange(aRange);
            }

        }
        public void AssociateToRange(string aRangeGUID)
        {
            if (string.IsNullOrWhiteSpace(aRangeGUID)) return;
            foreach (var item in this)
            {
                uopPart part = item as uopPart;
                part.AssociateToRange(aRangeGUID);
            }

        }

        public List<uopPart> ToList() => this.OfType<uopPart>().ToList();

        #endregion Methods

        #region Shared Methods

  

        #endregion Shared Methods
    }
}
