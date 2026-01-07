using System;
using System.Collections.Generic;
using UOP.WinTray.Projects.Documents;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Events;
using UOP.WinTray.Projects.Parts;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.Projects
{
    public abstract class uopDocument : IDisposable
    {
        private TDOCUMENT _Struc;
        private mzQuestions _Options;
        private mzQuestions _TrayQuery;



        #region Constructors

        public uopDocument() { _Struc = new TDOCUMENT(uppDocumentTypes.Undefined);_Options = new mzQuestions(); _TrayQuery = new mzQuestions(); Warnings = new List<uopDocWarning>(); }

        public uopDocument(uppDocumentTypes aType) { _Struc = new TDOCUMENT(aType);  _Options = new mzQuestions(); _TrayQuery = new mzQuestions(); Warnings = new List<uopDocWarning>(); }

        #endregion


        public string TabName { get => string.IsNullOrWhiteSpace(_Struc.TabName) ? Name : _Struc.TabName; set => _Struc.TabName = value; }
        /// <summary>
        /// returns whether it reqiure selection or not
        /// </summary>
        public bool RequiresSelections { get => TrayQuery.Count > 0; }

       

        private WeakReference<uopPart> _PartRef;
        /// <summary>
        /// returns the part that the document is associated to
        /// </summary>
        public uopPart Part
        {
            get
            {
                if (_PartRef == null) return null;
                if (!_PartRef.TryGetTarget(out uopPart _rVal))
                    _PartRef = null;
                return _rVal;
            }
            set
            {
                if (value == null)
                {
                    PartName = string.Empty;
                    PartType = uppPartTypes.Undefined;
                    PartIndex = 0;
                    _PartRef = null;
                    return;
                }
                if (value is uopProject)
                    Project = value as uopProject;

                if (value is uopTrayRange)
                    Range = value as uopTrayRange;

                PartName = value.PartName;
                PartType = value.PartType;
                PartIndex = value.PartIndex;
                if (!string.IsNullOrWhiteSpace(value.ProjectHandle)) ProjectHandle = value.ProjectHandle;
                _PartRef = new WeakReference<uopPart>(value);
            }
        }

        public virtual int SheetNumber { get => _Struc.SheetNumber; set => _Struc.SheetNumber = value; }
        public virtual string PartName{get; set;}
        internal TVALUES Data { get => _Struc.Data; set => _Struc.Data = value; }
        internal TDOCUMENT Structure { get => _Struc; set => _Struc = value; }

        public double X { get => _Struc.Center.X; set => _Struc.Center.X = value; }

        public double Y { get => _Struc.Center.Y; set => _Struc.Center.Y = value; }
        public int SketchCount { get => _Struc.SketchCount; set => _Struc.SketchCount = value; }

        public string ToolTip { get => string.IsNullOrWhiteSpace(_Struc.ToolTip) ? TabName : _Struc.ToolTip; set => _Struc.ToolTip = value; }
        /// <summary>
        /// object carrie for the document
        /// </summary>
        private object _ReferenceObject;
        private bool disposedValue;

        public object ReferenceObject { get => _ReferenceObject; set => _ReferenceObject = value; }

        /// <summary>
        /// returns the options
        /// </summary>
        public mzQuestions Options { get => _Options; set => _Options = value ?? new mzQuestions(); }

        /// <summary>
        /// returns trayquery
        /// </summary>
        public mzQuestions TrayQuery { get => _TrayQuery; set => _TrayQuery = value ?? new mzQuestions(); }
       
        /// <summary>
        /// returns true is the drawing needs a tray range selected before it's image is generated
        /// </summary>
        public bool RequiresTraySelection
        {
            get
            {
                if (_TrayQuery == null) return false;
                return _TrayQuery.Count > 0;
            }
        }
        /// <summary>
        /// returns true is the drawing needs a requires some options defined before it's image is generated
        /// </summary>
        public bool RequiresOptionSelection
        {
            get
            {
                if (_Options == null) return false;
                return _Options.Count > 0;
            }
        }

        /// <summary>
        /// returns true is the drawing needs a tray range and downcomer selected before it's image is generated
        /// </summary>
        public bool RequiresDowncomerSelection
        {
            get
            {
                if (!RequiresTraySelection) return false;
                return _TrayQuery.QuestionType(1) == uopQueryTypes.DualStringChoice;
            }
        }

        private List<uopDocWarning> _Warnings;
        /// returns warnings
        /// </summary>
        public List<uopDocWarning> Warnings { get => _Warnings; set => _Warnings = value ?? new List<uopDocWarning>(); }

        public void CopyWarnings(List<uopDocWarning> aWarnings, uppWarningTypes? aWarninType = null)
        {
            Warnings.Clear();
            if (aWarnings == null) return;
            foreach (uopDocWarning item in aWarnings)
            {
                if (!aWarninType.HasValue)
                {
                    Warnings.Add(item.Clone());
                }
                else
                {
                    if(aWarninType.Value == item.WarningType) Warnings.Add(item.Clone());
                }
            }
         
        }

        internal TPROPERTIES Props => _Struc.Properties;

        public uopProperties Properties => new uopProperties(_Struc.Properties);

        public string TrayName { get; set; }
        internal bool SetProp(string aName, dynamic aValue) => _Struc.Properties.SetValue(aName, aValue);
        public virtual string Category { get => _Struc.Category; set => _Struc.Category = value; }
        public virtual string SubCategory { get => _Struc.SubCategory; set => _Struc.SubCategory = value; }
        public virtual bool StandardFormat { get => _Struc.StandardFormat; set => _Struc.StandardFormat = value; }

        public abstract uopDocument Clone(bool aFlag = false);
        public uppDocumentTypes DocumentType { get => _Struc.DocumentType; }
        public virtual bool Hidden { get => _Struc.Hidden; set => _Struc.Hidden = value; }
        public int Index { get => _Struc.Index; set => _Struc.Index = value; }
        public int SubPage { get => _Struc.SubPage; set => _Struc.SubPage = value; }
        public virtual bool Invalid { get => _Struc.Invalid; set => _Struc.Invalid = value; }
        public virtual bool IsPlaceHolder { get => _Struc.IsPlaceHolder; set => _Struc.IsPlaceHolder = value; }
        public virtual string NodePath { get => _Struc.NodePath; set => _Struc.NodePath = value; }
        public virtual string NodeValue { get => _Struc.NodeValue; set => _Struc.NodeValue = value; }
        public virtual string PartPath  => GetPartPath(); 
        public uppPartTypes PartType { get; set; }
        public virtual int PartIndex { get; set; }
      
        public virtual string RangeGUID { get; set; }
        public virtual int RangeIndex { get; set; }

        public virtual string SelectName { get => _Struc.SelectText; set => _Struc.SelectText = value; }
        public virtual bool Selected { get => _Struc.Selected; set => _Struc.Selected = value; }

        public virtual int SubType { get => _Struc.SubType; set => _Struc.SubType = value; }
        public virtual bool Requested { get => !Hidden && _Struc.Requested; set => _Struc.Requested = value; }
        public virtual bool Required { get => _Struc.Required; set => _Struc.Required = value; }
        public virtual bool Protected { get => _Struc.Protected; set => _Struc.Protected = value; }
        public virtual uppUnitFamilies DisplayUnits { get => _Struc.DisplayUnits; set => _Struc.DisplayUnits = value; }
        public virtual string SelectText { get => _Struc.SelectText; set => _Struc.SelectText = value; }
        public virtual string FileName { get => _Struc.FileName; set => _Struc.FileName = value; }

        public string FileSpec { get; set; }

        public virtual string RangeName { get => _Struc.RangeName; set => _Struc.RangeName = value; }
        public virtual string Name { get => _Struc.Name; set => _Struc.Name = value; }
        public virtual string Password { get => _Struc.Password; set => _Struc.Password = value; }

        public bool SuppressRevisionChecks { get; set; }


        /// <summary>
        /// the handle of the project that owns this object
        /// </summary>
        public string ProjectHandle { get; set; }

        public uppProjectTypes ProjectType { get; set; }

    /// <summary>
    /// returns the project type name of the project that the drawing is associated to
    /// </summary>
    public string ProjectTypeName => uopEnums.Description(ProjectType);

        private WeakReference<uopProject> _ProjectRef;
        /// <summary>
        /// returns the project that the document is associated to
        /// </summary>
        public uopProject Project {
            get
            {

                uopProject _rVal = null;
                if(_ProjectRef != null)
                {
                    if (_ProjectRef.TryGetTarget(out _rVal))
                    {
                        ProjectHandle = _rVal.Handle;
                        return _rVal;
                    }
                    else
                    {
                        _ProjectRef = null;
                    }
                }
                if(_rVal == null)
                {
                    _rVal = uopEvents.RetrieveProject(ProjectHandle);
                    if (_rVal != null) _ProjectRef = new WeakReference<uopProject>(_rVal);
                }
                return _rVal;
            }
            set 
            { 
                ProjectHandle = (value == null)? string.Empty : ProjectHandle = value.Handle;
                if(value != null) ProjectType = value.ProjectType;

                if (value != null)
                    _ProjectRef = new WeakReference<uopProject>(value);
                else
                    _ProjectRef = null;
            }
        }

        public mdProject MDProject
        {
            get
            {
                uopProject project = Project;
                if (project == null) return null;
                return project.ProjectFamily == uppProjectFamilies.uopFamMD ? (mdProject)project : null;
            }

            set => Project = value;
            
        }
       
        private WeakReference<uopTrayRange> _RangeRef;
        /// <summary>
        /// returns the tray range that the document is associated to
        /// </summary>
        public uopTrayRange Range
        {
            get
            {
                uopTrayRange _rVal;
                if(_RangeRef != null)
                {
                    if(_RangeRef.TryGetTarget(out _rVal))
                    {
                        RangeGUID = _rVal.GUID;
                        return _rVal;
                    }
                }
                _rVal = uopEvents.RetrieveRange(RangeGUID);
                _RangeRef = _rVal == null ? null : new WeakReference<uopTrayRange>(_rVal);
                return _rVal;
            }
            set
            {
                if (value == null)
                {
                    RangeGUID = string.Empty;
                    RangeIndex = 0;
                    _RangeRef = null;
                    return;
                }
                ProjectHandle = value.ProjectHandle;
                ProjectType = value.ProjectType;
                RangeGUID = value.GUID;
                RangeIndex = value.Index;
                _Struc.RangeName = value.SpanName();
                _RangeRef = new WeakReference<uopTrayRange>(value);
                TrayName = value.TrayName(true);
            }
        }

        public mdTrayRange MDRange
        {
            get
            {
                uopTrayRange range = Range;
                if (range == null) return null;
                return range.ProjectFamily == uppProjectFamilies.uopFamMD ? (mdTrayRange)range: null;
            }
            set
            {
                Range = value;
            }
        }

        public mdTrayAssembly MDAssy
        {
            get
            {
                mdTrayRange range = MDRange;
                return Range == null ? null : range.TrayAssembly;
            }
        }

        /// <summary>
        /// used to add a uopDocWarning to the passed collection
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="aBrief"></param>
        /// <param name="aTextString"></param>
        /// <param name="wType"></param>
        /// <param name="aOwnerName"></param>
        /// <param name="aCategory"></param>
        /// <param name="aSubCategory"></param>
        /// <returns></returns>
        public uopDocWarning AddWarning(uopPart aPart, string aBrief = "", string aTextString = "", uppWarningTypes wType = uppWarningTypes.General, string aOwnerName = "", string aCategory = "", string aSubCategory = "")
        {


            if (string.IsNullOrWhiteSpace(aTextString)) return null;
         
            aTextString = aTextString.Trim();
            aOwnerName = aOwnerName.Trim();
            aBrief = aBrief.Trim();

            uopDocWarning _rVal = new uopDocWarning()
            {
                Owner = aOwnerName,
                Brief = aBrief,
                TextString = aTextString,
                WarningType = wType,
                Category = aCategory,
                SubCategory = aSubCategory,
                Part = aPart

            };



            Warnings.Add(_rVal);

            return _rVal;
        }

        #region Shared Methods
        internal uopDocument FromStructure(TDOCUMENT aDoc, mzQuestions aOptions = null, mzQuestions aTrayQuery = null)
        {
            uopDocument _rVal = null;
            switch(aDoc.DocumentType)
            {

            case uppDocumentTypes.Calculation:
                _rVal = new uopDocCalculation(aDoc,aOptions,aTrayQuery);
                break;

            }

            return _rVal;
        }

        internal virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _ReferenceObject = null;
                    _Options?.Dispose(true);
                    _TrayQuery?.Dispose(true);
                    _Warnings?.Clear();
                    _Warnings = null;
                    _Options = null;
                    _TrayQuery = null;
                    _RangeRef = null;
                    _PartRef = null;
                    _ProjectRef = null;

                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        public string GetPartPath(string aSuffix = null) 
        {
            string rVal = "PROJECT";
            uopTrayRange range = Range;
            if (range != null)
            {
                rVal += $".RANGE({range.Index})";
            }
            uopPart part = Part;
            if (part != null)
            {
                rVal += $".{part}";

            }
            rVal += $".{SelectName}";

            if (!string.IsNullOrEmpty(aSuffix)) { rVal += $".{ aSuffix}"; }

            return rVal;

        }

        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void Copy(uopDocument aDoc)
        {
            if (aDoc == null) return;
            Structure = aDoc.Structure.Clone();
            TrayQuery = aDoc.TrayQuery.Clone();
            Options = aDoc.Options.Clone();
            CopyWarnings(aDoc.Warnings);
            Part = aDoc.Part;
            Range = aDoc.Range;
            Project = aDoc.Project;
        }

        #endregion

    }
}

