using UOP.DXFGraphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Materials;
using UOP.WinTray.Projects.Models;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using UOP.DXFGraphics.Utilities;

namespace UOP.WinTray.Projects.Constants
{
    public class uopGlobals
    {
        #region Variables
        static readonly string filename = @"\Help\Wintray.chm";

        private static uopObject _GlobalVars;
        private static uopGlobals _GlobalsObj;
        static uopMaterials _SheetMetalOptions;


        #endregion

        #region Constants

        
        //public const double PI = 3.14159265358979;//@application global value for pi
        public const string Delim = "¸";
        public const string subDelim = "¤";
        //public const int gMax_Int = 32766;
        //public const double MillimetersPerInch = 25.4;
        public const char DELIMITOR_COMMA = ',';
        public static string gsAppIni = System.Configuration.ConfigurationManager.AppSettings.Get("ApplicationIniFile");
        public static double gsEndAngleCompare = 0;
        private colUOPPipes _Pipes;
        #endregion



        public static uopEventHandler goEvents;
       
        public static string gstrDetailChoices;
        private static readonly Object syncLock = new Object();
        public static dxfUtils goDXFUtils = new dxfUtils();
        
        
        
        public dxfPrimatives _Primatives;
        public dxfPrimatives Primatives { get => _Primatives; set => _Primatives = value; }
        /// <summary>
        /// @the application global collection of defined hardware material
        /// </summary>

        private static uopMaterialSpecs _Specs;

        internal static uopMaterialSpecs goSpecs
        {
            get
            {
                if (_Specs.Count(uppSpecTypes.Undefined) <= 0) _Specs = null;
                if (_Specs == null)
                {
                    _Specs = new uopMaterialSpecs();
                    _Specs.ReadFromFile(uopGlobals.gsAppIni);
                    
                }
                return _Specs;
            }
        }

        internal static TUNITS gsUnits = new TUNITS();

        static uopGlobals()
        {
            //gsAppIni = System.Configuration.ConfigurationManager.AppSettings.Get("ApplicationIniFile");
        }
        public uopGlobals()
        {
            goEvents = new uopEventHandler();
            _Primatives = new dxfPrimatives();
            _Pipes = new colUOPPipes();
            //_Specs = new uopMaterialSpecs();
            //_Specs.ReadFromFile(uopGlobals.gsAppIni);
        }

        public static uopGlobals UopGlobals
        {
            get
            {
                _GlobalsObj ??= new uopGlobals();
                return _GlobalsObj;
            }
        }

        public colUOPPipes Pipes{get =>_Pipes; set => _Pipes = value;}



        /// <summary>
        /// Get the application version from assembly file
        /// </summary>
        public static string ApplicationVersion
        {
            get
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location);
                return fileVersion.FileVersion;
            }
        }
        public static uopObject goGlobalVars()
        {
            _GlobalVars ??= new uopObject();
            return _GlobalVars;
        }

        public static uopMaterials goSheetMetalOptions()
        {
            if (_SheetMetalOptions == null) _SheetMetalOptions = new uopMaterials(TMATERIALS.DefautSheetMetals());
            return _SheetMetalOptions;
        }

        public static int glProjCount { get; internal set; }
     
        public static string FunctionalPath => uopUtils.ReadINI_String(gsAppIni, "PATHS", "FunctionalPath");
     

        public static string LastOpenedFile { get; set; }

      
        private static uopMaterials _HardwareMaterialOptions;
        public static uopMaterials goHardwareMaterialOptions()
        {

            _HardwareMaterialOptions ??= new uopMaterials(TMATERIALS.DefaultHardware());
            return _HardwareMaterialOptions;
        }

        /// <summary>
        /// Show range help file
        /// </summary>
        public static void ShowHelp()
        {
            string filepath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + filename;
            Process.Start(filepath);
        }

        public static double RingClipSizeChangeLimit => 156;


    }
}
