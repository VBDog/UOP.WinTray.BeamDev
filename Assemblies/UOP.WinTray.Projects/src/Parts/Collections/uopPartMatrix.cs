using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Parts;


namespace UOP.WinTray.Projects
{
   public abstract class uopPartMatrix : List<uopPartArray>
    {
        #region Constructors


        public uopPartMatrix() { base.Clear(); OwnerGUID = String.Empty; }

        public uopPartMatrix(string aOwnerGUID ) { base.Clear(); OwnerGUID = string.IsNullOrWhiteSpace(aOwnerGUID) ?  String.Empty : aOwnerGUID.Trim(); }
        #endregion Constructors


        #region Properties
        public string OwnerGUID { get; set; }

        public bool InvalidWhenEmpty { get; set; }

        private bool _Invalid = false;
        public bool Invalid
        {
            get => (InvalidWhenEmpty && Count <= 0) || _Invalid;
            set => _Invalid = value;
        }
        #endregion Properties


        #region Methods

        public uopPartArray Item(int aIndex)
        {
            if (aIndex < 1 || aIndex > Count) throw new IndexOutOfRangeException();
            return base[aIndex - 1];
        }

        public uopPartArray Item(uppPartTypes aPartType)
        {

            return Members(aPartType).FirstOrDefault();
        }

        public bool TryGetParts(uppPartTypes aPartType, out uopPartArray rParts)
        {
            rParts = Item(aPartType);
            return rParts != null;
        }
        public List<uopPartArray> Members(uppPartTypes aPartType, string aOwnerGUID = null)
        {
            List<uopPartArray> members = FindAll((x) => x.PartType == aPartType);
            if (string.IsNullOrWhiteSpace(aOwnerGUID)) members = members.FindAll((x) => string.Compare(x.OwnerGUID, aOwnerGUID, true) == 0);
            return members;
        }

        public new void Add(uopPartArray aParts)
        {
            if (aParts == null) return;
            base.Add(aParts);

        }

        public void Add(colUOPParts aParts, bool bAddNewMember = false)
        {
            if (aParts == null) return;
            List<uopPartArray> members = Members(aParts.PartType);
            if (members.Count > 0)
            {
                members[0].Append(aParts);
            }
            else
            {
                uopPartArray newmember = new uopPartArray(aParts.PartType, OwnerGUID, aParts);
                base.Add(newmember);
            }


        }

        public new int IndexOf(uopPartArray aMember) => base.IndexOf(aMember) + 1;


        #endregion Methods

        #region Shared Methods

        #endregion Shared Methods
    }
}
