using UOP.WinTray.UI.Logger;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Models;
using UOP.WinTray.Projects.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace UOP.WinTray.UI.BusinessLogic
{
    /// <summary>
    /// Class for security checks
    /// </summary>
    public class SecurityHelper
    {
        #region Constants

        private const string UNABLE_TO_CONNECT = "Unable To Connect to UOP Network";
        private const string LCR = "LCR";
        private const string SCR = "SCR";
        private const string NO_USER = "No User Passed";
        private const string UNABLE_TO_AUTHENTICATE = "Unable to Authenticate User";
        private const string NO_PERMISSION = "User doesn't have permissions";
        private const string NOT_REGISTERED = "'{0}'  Is Not Registered In The UOP Network Security File";
        private const string NOSECURITY_FILE = "\\NOSECURITY.TXT";
        private const string SECURITY_OVERRIDE = "** SECURITY OVERRIDE **";
        private const string VERSION_MESSAGE = "Security Issue Detedted. \n\nYour Currenly Installed Version Of WinTray ( {0} ) Has Been Superceded." +
                                                "\nWinTray Version {1} Must Be Installed Before Any New Projects Can Be Created.\n" + SECURITY_OVERRIDE;

        #endregion

        #region Private Variables

        private List<UserEntry> _Entries = null;
        private readonly string _FileSpec = string.Empty;
        private string _FileDate = string.Empty;
        private string _VersionMDTray = string.Empty;
        private string _VersionMDXF = string.Empty;
        private string _VersionXFlow = string.Empty;
        private string _LCRSerial = string.Empty;
        private string _SCRSerial = string.Empty;
        private static SecurityHelper _Instance;

        #endregion

        #region Properties

        public string AprovedVersion { get; set; }

        public bool IsConnectedToQdrive => File.Exists(_FileSpec);


        private List<UserEntry> Entries
        {
            get
            {
                if (_Entries == null) _Entries = new List<UserEntry>();
                return _Entries;
            }
        }

        public static SecurityHelper Instance
        {
            get
            {
                if (_Instance == null) _Instance = new SecurityHelper();
            
                return _Instance;
            }
        }

        #endregion

        #region Constructor

        private SecurityHelper()
        {
      
            _FileSpec = appApplication.SecurityINIFile;
            if ( IsConnectedToQdrive) Populate();
            
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Populate the list of valid users from security file
        /// </summary>
        /// <returns></returns>
        private void Populate()
        {
            _Entries = new List<UserEntry>();
            string[] fileContent = File.ReadAllLines(_FileSpec);

            for (int i = 0; i < fileContent.Length; i++)
            {
                string txt = fileContent[i].Trim();

                if (i == 0)
                {
                    _FileDate = DateTime.FromOADate(Convert.ToDouble(uopCryption.Decrypt(txt, GlobalConstants.NETWORKKEY))).ToString();
                }
                else if (i == 1)
                {
                    _VersionMDTray = uopCryption.Decrypt(txt, GlobalConstants.NETWORKKEY);
                }
                else if (i == 2)
                {
                    _VersionMDXF = uopCryption.Decrypt(txt, GlobalConstants.NETWORKKEY);
                }
                else if (i == 3)
                {
                    AprovedVersion = uopCryption.Decrypt(txt, GlobalConstants.NETWORKKEY);
                }
                else if (i == 4)
                {
                    _VersionXFlow = uopCryption.Decrypt(txt, GlobalConstants.NETWORKKEY);
                }
                else
                {
                    if (txt.Contains(","))
                    {
                        UserEntry aEntry = new(txt, GlobalConstants.NETWORKKEY);
                        _Entries.Add(aEntry);
                        if (aEntry.Initials == LCR)
                        {
                            _LCRSerial = aEntry.DriveSerialNumber;
                        }
                        if (aEntry.Initials == SCR)
                        {
                            _SCRSerial = aEntry.DriveSerialNumber;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Validate current user
        /// </summary>
        /// <param name="aUser"></param>
        /// <returns></returns>
        private string ValidateUser(AppUser aUser)
        {
            if (aUser == null)
            {
                return NO_USER;
            }

            string nname = aUser.NetworkName;
            Authenicate_User(Entries, nname, out bool bFail, out UserEntry aEntry);

            if (bFail) return UNABLE_TO_AUTHENTICATE;
            
            aUser.ObjectPath = aEntry.FullName;
            aUser.Initials = aEntry.Initials;

            List<bool> Permis = aEntry.Permissions;
            if (Permis != null && Permis.Count > 1)
            {
                aUser.IsEngineer = Permis[Permis.Count - 2];
                aUser.IsDesigner = Permis[Permis.Count - 1];
                aUser.GlobalUser = aUser.IsEngineer && aUser.IsDesigner;
            }
            if (!appApplication.User.IsDesigner && !appApplication.User.IsEngineer && !appApplication.User.GlobalUser)
            {
                return NO_PERMISSION;
            }
            return string.Empty;
        }

        /// <summary>
        /// Check if current user is available in the list of valid users
        /// </summary>
        /// <param name="aSecFileEntries"></param>
        /// <param name="irUser"></param>
        /// <param name="rFail"></param>
        /// <param name="aEntry"></param>
        private void Authenicate_User(List<UserEntry> aSecFileEntries, string irUser, out bool rFail, out UserEntry aEntry)
        {
            UserEntry oGoodEntry = null;
            rFail = true;
            irUser = irUser.Trim();
            aEntry = oGoodEntry;
            if (aSecFileEntries == null || aSecFileEntries.Count <= 0)
            {
                return;
            }
            _Entries = aSecFileEntries;

            if (!string.IsNullOrEmpty(irUser) && string.IsNullOrEmpty(Authenticate(irUser, out oGoodEntry)))
            {
                rFail = false;
                aEntry = oGoodEntry;
                return;
            }

            _Entries = null;
        }

        /// <summary>
        /// Check if current user is available in the list of valid users
        /// </summary>
        /// <param name="sEID"></param>
        /// <param name="oGoodEntry"></param>
        /// <returns></returns>
        private string Authenticate(string sEID, out UserEntry oGoodEntry)
        {
            string authenticate = string.Empty;
            oGoodEntry = null;

            for (int i = 0; i < _Entries.Count; i++)
            {
                UserEntry aEntry = _Entries[i];
                if (string.Compare(aEntry.NetworkName, sEID, true) == 0)
                {
                    oGoodEntry = aEntry;
                    break;
                }
            }

            if (oGoodEntry == null)
            {
                authenticate = string.Format(NOT_REGISTERED, sEID);
            }

            return authenticate;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Check if current user is valid
        /// </summary>
        /// <param name="rDontKill"></param>
        /// <returns></returns>
        public string CheckSecurity(out bool rDontKill)
        {
            rDontKill = false;

            string _rVal = !IsConnectedToQdrive ? UNABLE_TO_CONNECT : string.Empty;

            if (string.IsNullOrEmpty(_rVal)) _rVal = ValidateUser(appApplication.User);

            return CheckWintrayVersion(_rVal, appApplication.User.IsEngineer, File.Exists(AppDomain.CurrentDomain.BaseDirectory + NOSECURITY_FILE), out rDontKill);
        }
        /// <summary>
        /// Check the wintray version and get validation message 
        /// </summary>
        /// <param name="validationMessage"></param>
        /// <param name="IsEngineer"></param>
        /// <param name="baseDirectory"></param>
        /// <param name="rDontKill"></param>
        /// <returns></returns>
        public string CheckWintrayVersion(string validationMessage,bool IsEngineer, bool isFileExist, out bool rDontKill)
        {
            rDontKill = false;
            string _rVal = (!string.IsNullOrEmpty(validationMessage)) ? validationMessage.Trim() : "";
            //'check the wintray version
            if (_rVal == "" && IsEngineer)
            {
                _rVal = ValidateVersion();
                if (_rVal != "") rDontKill = true;
                
            }

            if (_rVal != "" && isFileExist)
            {
                _rVal += Environment.NewLine + SECURITY_OVERRIDE;
                rDontKill = true;
            }

            return _rVal;
        }
        /// <summary>
        /// Validates running version against approved version
        /// </summary>
        /// <param name="runningVersion"></param>
        /// <returns></returns>
        public string ValidateVersion()
        {
            string _rVal  = string.Empty;

            try
            {
                Version runningVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version; 

                if (!string.IsNullOrEmpty(AprovedVersion))
                {
                    var appVersion = new Version(AprovedVersion); 

                    if (runningVersion.CompareTo(appVersion) < 0)
                    {
                        _rVal = string.Format(VERSION_MESSAGE, runningVersion, AprovedVersion); 
                    }
                }
            }
            catch (Exception ex)
            {
                _rVal = string.Empty;
                ApplicationLogger.Instance.LogInfo(ex.Message);
            }

            return _rVal; 
        }

        #endregion
    }
}
