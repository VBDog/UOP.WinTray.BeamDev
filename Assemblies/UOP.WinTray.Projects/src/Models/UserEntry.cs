using System.Collections.Generic;
using System.Linq;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.Projects.Models
{
    public class UserEntry
    {
       
        #region Private Variables

        private string _Entry = string.Empty;
        private string _DefString = string.Empty;
        private List<string> _Vals = new List<string>();

        #endregion


        #region Constructors

        public UserEntry(string aEntry, string aKey) { Define(aEntry, aKey); }
        
        #endregion

        #region Public Properties

        public string DefString => _DefString;
            
        public string DriveSerialNumber => (_Vals?.Count > 0) ? _Vals[0] : "";

        //^the line of the security file that defined this entry
        public string Entry => _Entry;
            

        public string FullName => (_Vals?.Count > 1) ? _Vals[1] : "";
        
        public string Initials => (_Vals?.Count > 2) ? _Vals[2] : "";
        
        public string NetworkName => (_Vals?.Count > 3) ? _Vals[3] : "";
       
        public List<bool> Permissions
        {
            get
            {
                List<bool> _rVal = new List<bool>();
                for (int i = 4; i < _Vals.Count; i++)
                {
                    string txt = _Vals[i];
                    _rVal.Add(txt == "1");
                }

                return _rVal;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// defines the properties of the entry with the passed string
        /// </summary>
        /// <param name="aEntry">the string to define the enry properties with</param>
        /// <param name="aKey">the cryption key to decode the string with</param>
        public void Define(string aEntry, string aKey)
        {
            _Entry = aEntry;
            _Vals = aEntry.Split(',').ToList();
            _DefString = string.Empty;
            for (int i = 0; i < _Vals.Count; i++)
            {
                string txt = _Vals[i];
                if (i <= 3)
                {
                    txt = uopCryption.Decrypt(txt, aKey);
                }

                _Vals[i] = txt;
                _DefString += txt;

                if (i < _Vals.Count)
                {
                    _DefString += ",";
                }
            }


        }

        public override string ToString()
        {
            return $"{FullName} [{ NetworkName }]";
        }
        #endregion
    }
}
