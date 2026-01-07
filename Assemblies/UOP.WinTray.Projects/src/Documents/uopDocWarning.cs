using System;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Structures;

namespace UOP.WinTray.Projects.Documents
{
    /// <summary>
    ///  //^carries various warning information
    /// </summary>
    public class uopDocWarning : uopDocument, ICloneable
    {
        /// <summary>
        /// returns the text string of the warning concantonated to the owner string
        /// </summary>

        #region Constructors

        public uopDocWarning() : base(uppDocumentTypes.Warning) { }

        internal uopDocWarning(uopDocWarning aDocToCopy) : base(uppDocumentTypes.Drawing)
        {
            base.Copy(aDocToCopy);
            if (aDocToCopy == null) return;

        }


        #endregion


        public uopDocWarning Clone() => new uopDocWarning(this);


        /// <summary>
        /// returns the Clone of the object
        /// </summary>
        public override uopDocument Clone(bool aFlag = false) => (uopDocument)this.Clone();

        /// <summary>
        /// returns the Clone of the object
        /// </summary>
        object ICloneable.Clone() => (object)this.Clone();


        /// <summary>
        /// Abridged Text
        /// </summary>
        public string AbridgedText
        {
            get
            {
                string _rVal = string.Empty;
                if ( PartType != uppPartTypes.Undefined)
                {
                    _rVal = TrayName;
                }
                if (string.IsNullOrEmpty(_rVal))
                {
                    if (!string.IsNullOrEmpty( RangeName))
                    { _rVal = RangeName.ToUpper(); }
                    else
                    { _rVal = Structure.String3.ToUpper(); }
                }
                if (!string.IsNullOrEmpty(Structure.String1))
                {
                    if (!string.IsNullOrEmpty(_rVal)) _rVal +=  " - ";
                    
                    _rVal +=  Structure.String1;
                }

                return _rVal;
            }
        }
        /// <summary>
        ///  //^a short descriptor string for the warning

        /// </summary>
        public string Brief { get => Structure.String2; set { TDOCUMENT str = Structure; str.String2 = value; Structure = str; } } 
        
            /// returns the text string of the warning concantonated to the owner string
        /// </summary>
        public string FullText
        {
            get
            {
                string _rVal;
                _rVal = RangeName;
                if (_rVal !=  string.Empty)
                {
                    if (Structure.String3 !=  string.Empty) _rVal += " (" + Structure.String3 + ")";
                    
                }
                else
                { _rVal = Structure.String3; }

                if (Structure.String2 !=  string.Empty)
                {
                    if (_rVal !=  string.Empty) _rVal += " - ";
                    
                    _rVal += Structure.String2;
                }
                if (Structure.String1 !=  string.Empty)
                {
                    if (_rVal !=  string.Empty) _rVal += " - ";
                    
                    _rVal += Structure.String1;
                }

                return _rVal;
            }
        }
        /// <summary>
        ///   //^the object that is the source of this warning

        /// </summary>
        public string Owner
        {
            get => string.IsNullOrWhiteSpace( Structure.String3 ) ? PartName : Structure.String3;
            set { TDOCUMENT str = Structure; str.String3 = value; Structure = str; }
        }

        public override string PartPath
        {
            get
            {
                //^the part path of the documents source part
                //~like Column(1).Range(3)
                return $"PROJECT.WARNINGS.RANGE({ RangeIndex} - {Index}";
            }
        }

        /// <summary>
        /// the tray range that is the source of this warning
        /// </summary>
        public override  string RangeName
        {
            get
            {
                string _rVal = base.RangeName;
                if (string.IsNullOrEmpty(_rVal))
                {
                    uopTrayRange aRange = Range;
                    if (aRange != null) _rVal = $"Trays {aRange.SpanName()}";
                }
                if (string.IsNullOrEmpty(_rVal)) _rVal = Owner;
                    return _rVal;
            }
            set
            {
                value ??= string.Empty; 
                RangeName = value.Trim();
            }
        }

        //^the string of text that describes the warning
        public string TextString { get => Structure.String1; set { TDOCUMENT str = Structure; str.String1 = value; Structure = str; } } 
        
        /// <summary>
        ///   //^the type assigned to the warning
        /// </summary>
        public uppWarningTypes WarningType { get => (uppWarningTypes)SubType; set => SubType = (int)value; }
       
    }

}
