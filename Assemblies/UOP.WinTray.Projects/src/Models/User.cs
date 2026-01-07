using System;

namespace UOP.WinTray.Projects.Models
{
    /// <summary>
    /// User class model
    /// </summary>
    public class AppUser
    {
        #region Constants

        private const string MD = "MD";
        private const string XF = "XF";
        private const string MDCAD = "MDCAD";

        #endregion

        #region Private Variables

        private string _ObjectPath = string.Empty;
        private string _Initials = string.Empty;
        private bool _IsGlobalUser = false;
        private bool _IsDesigner = false;
        private bool _IsEngineer = false;

        #endregion

        #region Public Properties

        //^if true the user can use all modules
        public bool GlobalUser {  get => _IsGlobalUser; set => _IsGlobalUser = value; }

        //^the users intials
        public string Initials
        {
            get
            {
                if (string.IsNullOrEmpty(_Initials) && NetworkName.Length >= 3) return NetworkName.Substring(0, 3).ToUpper();
                return _Initials;
            }
            set => _Initials = value.Trim().Length >= 3 ? value.Trim().Substring(0, 3).ToUpper() : value.Trim().ToUpper();
            
        }
        //^if true the user can use all cad designer modules
        public bool IsDesigner { get=> _IsDesigner || _IsGlobalUser; set => _IsDesigner = value; }

        //^if true the user can use all functional engineer modules
        public bool IsEngineer { get => _IsEngineer || _IsGlobalUser; set => _IsEngineer = value; }

        //^the network user name of the user
        public virtual string NetworkName => Environment.UserName;

        //^returns the long name if it is define or the network anme if it is not
        public string NiceName => (!string.IsNullOrEmpty(_ObjectPath)) ?  _ObjectPath : Environment.UserName;

        //^the name of the user
        //~like "Johny Rotten"
        public string ObjectPath { get=> _ObjectPath; set => _ObjectPath = value.Trim(); }
        public string Permissions
        {
            get
            {
                //^returns a string that contains the permissions of the user
                //~like MD,XF
                string _rVal = string.Empty;

                if (IsEngineer) _rVal = MD;
                if (IsDesigner)
                {
                    if (_rVal != string.Empty) _rVal += ",";
                    _rVal += $"{XF},{ MDCAD}";
                }
                return _rVal;
            }
        }

        public override string ToString()
        {
            return $"{ NiceName }[{ NetworkName }] ({ Permissions })";
        }

        #endregion
    }
}
