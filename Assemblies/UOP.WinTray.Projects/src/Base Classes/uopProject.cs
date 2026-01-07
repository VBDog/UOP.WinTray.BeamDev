using System;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Materials;
using UOP.WinTray.Projects.Parts;

using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Tables;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.Projects
{
    public abstract class uopProject : uopPart
    {
        #region Events

        
        public delegate void ReadStatusChangeHandler(string aStatusString, int aIndex);
        public event ReadStatusChangeHandler eventReadStatusChange;

        public delegate void PartGenerationHandler(string aStatusString, bool? bBegin = null);
        public event PartGenerationHandler eventPartGeneration;

        #endregion Events

        #region Constructors
        public uopProject(uppProjectTypes aType) : base(uppPartTypes.Project, uppProjectFamilies.Undefined) 
        {
            ProjectType = aType;

            GUID = mzUtils.CreateGUID(); 
        
           
        }
        #endregion Constructors

        private double _DLLVersion;
        public override double DLLVersion
        {
            get
            {
                if ( _DLLVersion == 0)
                {
                    Version App = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

                    _DLLVersion = mzUtils.VarToDouble($"{App.Major}.{ App.Minor }{ App.Revision}");
                }

                return _DLLVersion;
            }
            set => _DLLVersion = value;
        }
        private uopProperties _DrawingNumbers;

        /// <summary>
        /// the collection that holds drawing number information
        /// </summary>
        public virtual  uopProperties DrawingNumbers
        { 
            get
            {
                if(_DrawingNumbers == null)
                {
                    _DrawingNumbers = new uopProperties();
                    _DrawingNumbers.Add(new uopProperty("Functional", "", aPartType: uppPartTypes.Document));
                    _DrawingNumbers.Add(new uopProperty("Installation", "", aPartType: uppPartTypes.Document));
                    _DrawingNumbers.Add(new uopProperty("Manufacturing", "", aPartType: uppPartTypes.Document));
                    _DrawingNumbers.Add(new uopProperty("Attachment", "", aPartType: uppPartTypes.Document));

                }
                return _DrawingNumbers;
            }

           
        }

        public string GUID { get; set; }
        public abstract uppUnitFamilies CustomerDrawingUnits { get; set; }
        public abstract uppUnitFamilies ManufacturingDrawingUnits { get; set; }

        /// <summary>
        /// the project tray range that is currently marked as selected
        /// </summary>
        public virtual uopTrayRange SelectedRange => TrayRanges.SelectedRange;

        public virtual int SelectedRangeIndex
        {
            get => TrayRanges.SelectedIndex;
            set => TrayRanges.SelectedIndex = value;
        }

        public virtual string SelectedRangeGUID
        {
            get => TrayRanges.SelectedRangeGUID;
            set => TrayRanges.SelectedRangeGUID = value;
        }

        public virtual string SelectedRangeName
        {
            get => TrayRanges.SelectedRangeName;
            set => TrayRanges.SelectedRangeName = value;
        }
        public override int TrayCount => TrayRanges.TrayCount();

        public string AppName { get; set; }

        public string AppVersion { get; set; }

        internal override TPROJECT ProjectStructure() 
        {
            TPROJECT struc = new TPROJECT(ProjectType, ProjectHandle)
            {
                Properties = ActiveProps,
                Family = ProjectFamily,
                Handle = ProjectHandle,
                DLLVersion = DLLVersion,
                AppVersion = AppVersion,
                AppName = AppName,
                DrawingNumbers = new TPROPERTIES( DrawingNumbers),
            };
            return struc;
            
        }

        public override uopProject Project => this;
        
        public abstract uopSheetMetal FirstDowncomerMaterial { get; }
       
        public abstract uopSheetMetal FirstDeckMaterial { get; }

        //the distributors collection of the parent project
        public abstract colMDDistributors Distributors { get; }

        //the chimney trays collection of the parent project
        public abstract colMDChimneyTrays ChimneyTrays { get; }

        public  string FileExtension  => base.ProjectFileExtension;
     
        public abstract new uopDocuments Calculations();
        
        public abstract uopProjectSpecs MaterialSpecs { get; }

        public abstract uopDocuments Warnings(bool bJustOne = false);

        public abstract string TrayVendor { get; set; }
        
        public abstract bool SingleColumnProject { get; }

        /// <summary>
        /// a collection of sheet metal material objects
        /// </summary>
        public uopMaterials SheetMetalOptions => uopGlobals.goSheetMetalOptions();
        
        public abstract string SelectedCalculation { get; set; }
     
        public new abstract string SelectName { get; }
        
        public abstract string SAPNumber { get; set; }
        
        public abstract colUOPRingSpecs RingSpecs { get; set; }
        public abstract int Revision { get; set; }
        public abstract uopDocuments Reports { get; }
     
        public abstract string ProcessLicensor { get; set; }
    
        
        public abstract bool MetricRings { get; set; }
        
        public abstract new double ManholeID { get; set; }
     
        public abstract string KeyNumber { get; set; }
     
        public abstract uppInstallationTypes InstallationType { get; set; }
        
        public abstract string ImportFileName { get; set; }
        public abstract string DataFileName { get; set; }

        public abstract string IDNumber { get; set; }
        
        public abstract bool HasTriangularEndPlates { get; }
        
        public abstract bool HasReport { get; }

        public abstract bool HasChanged { get; set; }
        
        public abstract string Filter { get; }
        
        public abstract string FriendlyFileName { get; }

        public abstract new uopDocuments Drawings();
       
       
        
        public abstract string DefaultFileName { get; }
        
        public abstract string Designer { get; set; }
       
        
        public abstract uopCustomer Customer { get; set; }

        public abstract string Contractor { get; set; }


        /// <summary>
        /// the bolting setting of the project (metric or english)
        /// </summary>
        public abstract new uppUnitFamilies Bolting { get; set; }

        /// <summary>
        /// the first column in the projects columns collection
        /// </summary>
        public new abstract  uopColumn Column { get; }

      
       
        public abstract bool Locked { get; set; }

        /// <summary>
        ///returns the deck material of the first tray range in the project
        /// </summary>
        public abstract uopSheetMetal DeckMateral { get; }

        /// <summary>
        ///returns the downcomer material of the first tray range in the project
        /// </summary>
        public abstract override  uopSheetMetal DowncomerMaterial { get; }

        /// <summary>
        /// assigned to track all the properties created under this project
        /// </summary>
        private string _Handle = string.Empty;
        public virtual string Handle{ get => _Handle; internal set { _Handle = value; base.ProjectHandle = value; } }

        /// <summary>
        /// ^returns the file filter string used with a common dialogue when importing a wintray project file
        /// </summary>
        public abstract string ImportFileFilter { get; }

        private string _ReadStatus = string.Empty;
        public virtual string ReadStatusString => _ReadStatus;
        private string _PartGenStatus = string.Empty;
        public virtual string PartGenStatusString => _PartGenStatus;
        /// <summary>
        /// the path to the main folder for project files
        /// </summary>
        public abstract string OutputFolder { get; }


        
        /// <summary>
        /// True causes the trays to be listed with the highest ring numbers first
        /// </summary>
        public abstract bool ReverseSort { get; set; }

        /// <summary>
        ///returns the filename or the import filename
        /// </summary>
        public abstract string SourceFileName { get; }

        /// <summary>
        /// the percentage of extra hardware required when the trays are shipped
        /// ~default = 5
        /// </summary>
        public override double SparePercentage { get; set; }

      
        /// <summary>
        /// the total number of trays
        /// </summary>
        public new abstract int TotalTrayCount { get; }

        /// <summary>
        /// the version of the DLL
        /// like "1.0.2"
        /// </summary>
        public abstract string Version { get; set; }

        public override void Destroy()
        {
            base.Destroy();
        }

       public virtual uopProperties Notes { get => base.GetNotes(); set => base.SetNotes(value); }

        public new abstract colUOPTrayRanges TrayRanges { get; }

      


        /// <summary>
        /// 
        /// </summary>
        /// <param name="RHS"></param>
        /// <returns></returns>
        public abstract uopTrayAssembly DefaultTray(uopTrayRange RHS);


        /// <summary>
        /// get document Collection
        /// </summary>
        /// <param name="bRefresh"></param>
        /// <returns></returns>
        public abstract uopDocuments Documents(bool bRefresh = false);




        /// <summary>
        /// make sure all persistent sub parts are up to date
        /// </summary>
        public abstract void UpdatePersistentSubParts(bool bForceRegen = false, string aRangeGUID = null);

        /// <summary>
        /// get the tables
        /// </summary>
        /// <param name="aTableName"></param>
        /// <param name="aList"></param>
        /// <returns></returns>
        public abstract uopTable GetTable(string aTableName, string aList = "");

        /// <summary>
        /// the property that is notifying me (the project) that it has changes
        /// used by an event handler to notify the project that a property associated to it has changed
        /// </summary>
        /// <param name="RHS"></param>
        public abstract void Notify(uopProperty RHS);

        /// <summary>
        /// ^used to read a projects properties from a text file
        /// </summary>
        /// <param name="aFileSpec">the file name to read</param>
        /// <param name="aAppName">the name of the requesting application</param>
        /// <param name="aAppVersion">the version of the requesting application</param>
        /// <param name="aWarnings">4a collection to return warning strings</param>
        public abstract void ReadFromFile(string aFileSpec, string aAppName, string aAppVersion, out uopDocuments rWarnings);

       
        /// <summary>
        /// get the read status
        /// </summary>
        /// <param name="RHS"></param>
        /// <param name="aIndex"></param>
        /// <returns></returns>
        public virtual void ReadStatus(string RHS, int aIndex = 1)
        {
            _ReadStatus = RHS;
            eventReadStatusChange?.Invoke(RHS, aIndex);
        }

        public virtual void PartGenenerationStatus(string StatusString, bool? Begin = null)
        {
            _PartGenStatus = StatusString;
                eventPartGeneration?.Invoke(StatusString, Begin);
          
        }

        /// <summary>
        /// save the file
        /// </summary>
        /// <param name="FileSpec"></param>
        /// <param name="AppName"></param>
        /// <param name="AppVersion"></param>
        public abstract void SaveToFile(string FileSpec, string AppName, string AppVersion, string aFinalDestination = null);

        public abstract bool UpdateParts(bool bUpdateAll = false);

        public abstract string SelectedDrawing { get; set; }

        /// <summary>
        /// the collection of stages defined for the column in the project
        /// </summary>
        public virtual colMDStages Stages { get; set; }

        
      
    }
}
