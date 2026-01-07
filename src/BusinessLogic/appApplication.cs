using UOP.DXFGraphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Documents;
using UOP.WinTray.Projects.Models;
using UOP.WinTray.Projects.Utilities;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Drawing;

namespace UOP.WinTray.UI.BusinessLogic
{
    public class appApplication
    {
        #region Private Variables
        public static readonly List<string> TemplateExtensions = new() { ".xltx", ".xlsx", ".xltm", ".xlsm" };  // order matters!
       
        private static readonly appDrawSettings _DrawSettings;
        private static readonly dxfBlockSource _BlockSource;
        #endregion  Private Variables

        #region Constructors

        static appApplication()
        {

            _Properties = new uopPropertyArray();

            _DrawSettings = new appDrawSettings();
            
            uopProperties props = new() { Name = "APPLICATION" };
            props.Add("HighlightColor", -256); // This is a completely opaque yellow. It was "8388607" originally which is completely transparent cyan!
            props.Add("DrawingBackColor",UOP.WinTray.UI.Properties.Settings.Default.SketchColor.ToArgb() );// 'light grey/green
            props.Add("LastGlobalSelect", "");
            props.Add("LocalFilePath", @"C:\Docs\UOP\Wintray");
            _Properties.Add(props);
            //all the properties in the network INI file are read into the property array;
            _Properties.ReadFromINIFile(uopGlobals.gsAppIni, false, true);
            props = _Properties.Item("PATHS");
            
            // define relative paths
            if(props != null)
            {
                foreach (uopProperty prop in props)
                {
                    string vals = prop.ValueS.Trim();
                    if (vals.StartsWith("$("))
                    {
                        int i = vals.IndexOf(")");
                        if (i > 0)
                        {

                            string pname = vals.Substring(2, i - 2);

                            if (props.TryGet(pname, out uopProperty subprop))
                            {
                                string suffix = vals.Substring(i + 1, vals.Length - (pname.Length + 3));
                                vals = $"{subprop.ValueS.Trim()}{suffix}";
                                prop.SetValue(vals);
                            }


                        }

                    }

                }
            }
            
                    

            SetGlobalVars();
            _BlockSource = new dxfBlockSource();
            _BlockSource.LoadFolderFiles(appApplication.DXFFolder);

        }

        #endregion Constructors



        #region Public Properties

        
        private static AppUser _User;
        public static AppUser User 
        { 
            get
            {
                _User ??= new AppUser();
                return _User;
            }
            set => _User = value;

        }

        public static List<string> Contractors
        {
            get
            {
                if (_Contractors == null) ReadContractors();
                return new List<string>(_Contractors);

            }
        }
        public static List<string> Customers
        {
            get
            {
                if (_Customers == null) ReadCustomers();
                return new List<string>(_Customers);

            }
        }

        public static List<string> Liscensors
        {
            get
            {
                if (_Licencors == null) ReadLicensors();
                return new List<string>(_Licencors);

            }
        }

        public static List<string> Services
        {
            get
            {
                if (_Services == null) ReadServices();
                return new List<string>(_Services);

            }
        }

        public static List<string> TrayVendors
        {
            get
            {
                if (_TrayVendors == null) ReadTrayVendors();
                return new List<string>(_TrayVendors);

            }
        }


        private static readonly uopPropertyArray _Properties;
        public static Color SketchColor
        {
            get =>  Color.FromArgb(_Properties.ValueI("APPLICATION", "DrawingBackColor"));
            set { _Properties.SetValue("APPLICATION", "DrawingBackColor", value.ToArgb()); UOP.WinTray.UI.Properties.Settings.Default.SketchColor = value; }
        }

        public static Color HighlightColor
        {
            get => Color.FromArgb(_Properties.ValueI("APPLICATION", "HighlightColor"));
            set => _Properties.SetValue("APPLICATION", "HighlightColor", value.ToArgb());
        }
        public static string LocalFilePath
        {
            get =>_Properties.ValueS("APPLICATION", "LocalFilePath");
            set => _Properties.SetValue("APPLICATION", "LocalFilePath", value); 
        }
        public static appDrawSettings DrawSettings => _DrawSettings;

        public static dxfBlockSource BlockSource => _BlockSource;
        public static uopPropertyArray PropertyArray => _Properties;
        /// <summary>
        /// the path to the mechanical groups spreadsheet folder
        /// </summary>
        public static string MechanicalSpreadSheetFolder => _Properties.ValueS("PATHS", "MechanicalSpreadSheetFolder");
     

        public static string SecurityFolder => _Properties.ValueS("PATHS", "SecurityFolder"); 
        
        public static string SecurityINIFile => System.IO.Path.Combine( SecurityFolder ,"UOPSECURITY.INI");

      
        public static string TemplatesFolder  => _Properties.ValueS("PATHS", "TemplatesFolder");

        public static string FunctionalPath => _Properties.ValueS("PATHS", "FunctionalPath");
        public static string IPLFunctionalPath => _Properties.ValueS("PATHS", "IPLFunctionalPath");

        public static string MechanicalPath => _Properties.ValueS("PATHS", "MechanicalPath");

        

        public static string Version { get; set; }

        public static string SupportFolder => _Properties.ValueS("PATHS", "SupportFolder");
        public static string DXFFolder => _Properties.ValueS("PATHS", "DXFFolder");
        public static string AppName => mzUtils.GetLastString(Assembly.GetExecutingAssembly().GetName().Name.ToString());

        public static string AppVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public static string LastGlobalSelect
        {
            get => _Properties.ValueS("APPLICATION", "LastGlobalSelect");
            set => _Properties.SetValue("APPLICATION", "LastGlobalSelect", value);
        }

        public static string BordersFolder => _Properties.ValueS("PATHS", "Borders");

   
        public static string DBorder => _Properties.ValueS("PATHS", "DBorderFileName");
   
        public static string BBorder => _Properties.ValueS("PATHS", "BBorderFileName");

        private static List<string> _Contractors;
        private static List<string> _Services;
        private static List<string> _Customers;
        private static List<string> _Licencors;
        private static List<string> _TrayVendors;
        #endregion

        #region Public Methods



        /// <summary>
        /// Find the folders with help of ID and keynumber
        /// </summary>
        /// <param name="MDProject"></param>

        /// <returns></returns>
        public static string GetInquiryFolder(uopProject project = null)
        {

            if (project == null) return string.Empty;


            string aID = project.IDNumber.Trim();

            if (!mzUtils.SplitIntegers(aID, out int year, out int index)) return string.Empty;
            string _rVal = "";
            AppUser user = appApplication.User;
            List<DirectoryInfo> fldrs;
            for (int i = 1; i <= 2; i++)
            {

                string root = i == 1 ? appApplication.FunctionalPath : appApplication.IPLFunctionalPath;
                if (string.IsNullOrWhiteSpace(root)) continue;
                string searchroot = $"{root}{"\\20"}{ year}";
                if (!Directory.Exists(searchroot))
                    continue;


                fldrs = new DirectoryInfo(searchroot).GetDirectories().ToList();
                List<DirectoryInfo> projectsfolders = fldrs.FindAll((x) => x.Name.StartsWith($"{ aID} "));
                if (projectsfolders.Count <= 0) continue;
                _rVal = projectsfolders.FirstOrDefault().FullName;
                break;
            }

            if (_rVal == "") return _rVal;
            if (Directory.Exists($"{_rVal}\\Trays"))
                _rVal = $"{_rVal}\\Trays";

            fldrs = new DirectoryInfo(_rVal).GetDirectories().ToList();
            List<DirectoryInfo> keyfolders = fldrs.FindAll((x) => x.Name.StartsWith($"{ project.KeyNumber} ", comparisonType: StringComparison.OrdinalIgnoreCase));
            if (keyfolders.Count <= 0) return _rVal;
            _rVal = keyfolders.FirstOrDefault().FullName;

            return _rVal;
        }

    

    /// <summary>
    /// Get Report template Path
    /// </summary>
    /// <param name="aReport"></param>
    /// <param name="arFileName"></param>
    /// <param name="rFound"></param>
    /// <returns></returns>
    public static string GetReportTemplatePath(uopDocReport aReport, ref string arFileName, out bool rFound, out string rExtenstion)
        {
            string _rVal = string.Empty;
            rFound = false;
            rExtenstion = "";
            if (string.IsNullOrWhiteSpace(arFileName) && aReport != null) arFileName = aReport.TemplateName;
            if (aReport == null || string.IsNullOrWhiteSpace(arFileName) ) return _rVal;
            aReport.TemplatePath = "";
            arFileName = arFileName.Trim();
            rExtenstion = System.IO.Path.GetExtension(arFileName).ToLower();
            string fspec = string.Empty;
            if(!string.IsNullOrWhiteSpace( rExtenstion))
            {
                arFileName = arFileName.Substring(0, arFileName.Length - rExtenstion.Length);
                rExtenstion = "";
            }

            if (aReport.MechanicalTemplate)
            {
                fspec = MechanicalSpreadSheetFolder;

                if (fspec != string.Empty && !Directory.Exists(fspec)) 
                    fspec = string.Empty;
               
            }

            if (string.IsNullOrEmpty(fspec)) 
                fspec = TemplatesFolder;
            

            foreach (var item in TemplateExtensions)
            {
                string path = System.IO.Path.Combine(fspec, arFileName + item);
                if (File.Exists(path))
                {
                    rExtenstion = item.ToLower();
                    rFound = true; 
                    //arFileName += item;
                    _rVal = path;
                    break;
                }
            }
           
            if (!string.IsNullOrEmpty(_rVal))
            {
                rFound = File.Exists(_rVal);
            }
            aReport.TemplatePath = _rVal;
            return _rVal;
        }

        /// <summary>
        /// Get report template path from local machine
        /// </summary>
        /// <param name="serverTemplatePath"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetLocalReportTemplatePath(string serverTemplatePath, string fileName)
        {
            string localpath = LocalFilePath;
            if (string.IsNullOrWhiteSpace(localpath)) return serverTemplatePath;
            
            if (!Directory.Exists(localpath))
            {
                try { Directory.CreateDirectory(localpath); } catch { return serverTemplatePath; }
                   
            }
            localpath = System.IO.Path.Combine(localpath, "Templates");
            if (!Directory.Exists(localpath))
            {
                try { Directory.CreateDirectory(localpath); } catch { return serverTemplatePath; }

            }

            foreach (var item in TemplateExtensions)
            {

                //string localTemplatePath = (item.ToUpper() == ".XLTM") ? Path.Combine(localpath, fileName + ".xlsm") : Path.Combine(localpath, fileName + ".xlsx") ;
                string localTemplatePath = System.IO.Path.Combine(localpath, fileName + item);
                if (File.Exists(serverTemplatePath) && !File.Exists(localTemplatePath))
                {
                    if(System.IO.Path.GetExtension(serverTemplatePath) == System.IO.Path.GetExtension(localTemplatePath)) 
                    {
                        try { File.Copy(serverTemplatePath, localTemplatePath, true); } catch { return serverTemplatePath; }
                    }
                   
                    
                }

                if (File.Exists(serverTemplatePath) && File.Exists(localTemplatePath)) 
                { 
                    if ( File.GetLastWriteTimeUtc(localTemplatePath) < File.GetLastWriteTimeUtc(serverTemplatePath) )
                    {
                        try
                        {
                            File.Copy(serverTemplatePath, localTemplatePath, true);
                        }
                        catch
                        {
                            return serverTemplatePath;
                        }

                        return localTemplatePath;
                    }
                    else { return localTemplatePath; }
                }
            }
             
                
          
            return serverTemplatePath;
        }

        public static void DoEvents()
        {
            if (Application.Current == null) return;
            try { Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { })); } catch { };
        }
        #endregion

        #region Private Methods



        private static void SetGlobalVars()
        {
            
            uopGlobals.goGlobalVars().AddProperty("BendFactor", _Properties.ValueD("GLOBALVARIABLES", "BendFactor",1.7));
            uopGlobals.goGlobalVars().AddProperty("ToleranceRefDocument", _Properties.ValueS( "GLOBALVARIABLES", "ToleranceRefDocument", "XXXXX"));
            uopGlobals.goGlobalVars().AddProperty("DomesticPackingRefDocument", _Properties.ValueS("GLOBALVARIABLES", "DomesticPackingRefDocument", "XXXXX"));
            uopGlobals.goGlobalVars().AddProperty("ExportPackingRefDocument", _Properties.ValueS("GLOBALVARIABLES", "ExportPackingRefDocument", "XXXXX"));
            uopGlobals.goGlobalVars().AddProperty("WeldingRefDoc", _Properties.ValueS("GLOBALVARIABLES", "WeldingRefDoc", "XXXXX"));

            for (int i = 1; i <= 8; i++)
            {
                uopGlobals.goGlobalVars().AddProperty($"ManufacturingNote{i}", _Properties.ValueS("GLOBALVARIABLES", $"ManufacturingNote{i}", ""));
            }
            ReadCustomers();
            ReadLicensors();
            ReadServices();
            ReadContractors();
            ReadTrayVendors();

        }


        /// <summary>
        /// Read Customers
        /// </summary>
        /// <returns></returns>
        private static void ReadCustomers()
        {
            _Customers = new List<string>();
            string fname = System.IO.Path.Combine( appApplication.SecurityFolder, "Customer.txt");
            if (File.Exists(fname))
            {
                _Customers.AddRange(File.ReadAllLines(fname));
            }

        }
        /// <summary>
        /// Read Licensors
        /// </summary>
        /// <returns></returns>
        private static void ReadLicensors()
        {
            _Licencors = new List<string>();
            string fname = System.IO.Path.Combine(appApplication.SecurityFolder, "Licensor.txt");
            if (File.Exists(fname))
            {
                _Licencors.AddRange(File.ReadAllLines(fname));
            }

        }
        /// <summary>
        /// Read Services
        /// </summary>
        /// <returns></returns>
        private static void ReadServices()
        {
            _Services = new List<string>();
            string fname = System.IO.Path.Combine(appApplication.SecurityFolder, "Service.txt");
            if (File.Exists(fname))
            {
                _Services.AddRange(File.ReadAllLines(fname));
            }
        }
        /// <summary>
        /// read Contractors
        /// </summary>
        /// <returns></returns>
        private static void ReadContractors()
        {
            _Contractors = new List<string>();
            string fname = System.IO.Path.Combine(appApplication.SecurityFolder, "Contractor.txt");

            //  string fname = Path.Combine(_CurrentPath, contractorPath);
            if (File.Exists(fname))
            {
                _Contractors.AddRange(File.ReadAllLines(fname));
            }
        }
        /// <summary>
        /// Read Vendors
        /// </summary>
        /// <returns></returns>
        private static void ReadTrayVendors()
        {
            _TrayVendors = new List<string>();
            string fname = System.IO.Path.Combine(appApplication.SecurityFolder, "TrayVendors.txt");
            if (File.Exists(fname))
            {
                _TrayVendors.AddRange(File.ReadAllLines(fname));
            }


        }
        #endregion
    }
}
