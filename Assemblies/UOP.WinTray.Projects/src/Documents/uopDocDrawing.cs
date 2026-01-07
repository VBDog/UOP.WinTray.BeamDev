using UOP.DXFGraphics;
using System;
using System.Drawing;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using System.Runtime.Remoting.Contexts;

namespace UOP.WinTray.Projects.Documents
{
    /// <summary>
    /// represents a UOP Drawing drawing
    /// </summary>
    public class uopDocDrawing : uopDocument, ICloneable
    {
        
        
        
        public delegate void CancelDrawingHandler(ref bool rRecieved);
        public event CancelDrawingHandler eventCancelDrawing;
      
       
    /// <summary>
    /// UOP Doc Drawing Model
    /// </summary>

        #region Constructors

        public uopDocDrawing(uppDrawingFamily design) : base(uppDocumentTypes.Drawing) { Grid = null; }

        public uopDocDrawing(uppDrawingFamily aFamily, string aName, 
            uppDrawingTypes aType, 
            uopTrayRange aRange = null, 
            uopPart aPart = null, 
            bool bZoomExtents = true, 
            bool bNoText = false) : base(uppDocumentTypes.Drawing) 
        {
            Grid = null;
            Family = aFamily;
            Name = aName;
            DrawingType = aType;
            Range = aRange;
            Part = aPart;
            ZoomExtents = bZoomExtents;
            NoText = bNoText;
        }

     
       

        internal uopDocDrawing(uopDocDrawing aDocToCopy) : base(uppDocumentTypes.Drawing)
        {
            Grid = null;
            base.Copy(aDocToCopy);
            if (aDocToCopy == null) return;
            DeviceSize = aDocToCopy.DeviceSize;
            Family = aDocToCopy.Family;
            if (aDocToCopy.Grid != null) Grid = aDocToCopy.Grid.Clone();
        }

        internal uopDocDrawing(TDOCUMENT aDoc, mzQuestions aOptions = null, mzQuestions aTrayQuery = null) : base(uppDocumentTypes.Drawing)
            {
                aDoc.DocumentType = uppDocumentTypes.Drawing;
                Structure = aDoc.Clone();
                base.TrayQuery = aTrayQuery;
                base.Options = aOptions;

            }

         public uopDocDrawing(uppDrawingFamily aDrawingFamily, uppDrawingTypes aDrawingType, string aDrawingName,  string aSelectText = "", uopPart aPart = null, 
                uppBorderSizes aBorderSize = uppBorderSizes.Undefined, int? aPartIndex = null, bool bHidden = false,
                string aCategory = "", bool bCancelable = false, uppPartTypes aPartType = uppPartTypes.Undefined,
                string aFlag = "", bool bOppositeHand = false, bool bQuestionsAreTrayPrompt = false, mzQuestions aQuestions = null,
                bool bProjectWide = false, int aSheetIndex = 0, bool bZoomExents = true, uppUnitFamilies aUnits = uppUnitFamilies.Undefined,
                uopTrayAssembly aAssy = null, uopTrayRange aRange = null, string aSubCat = "") : base(uppDocumentTypes.Drawing)
        {

            Grid = null;
            if (aPartType == uppPartTypes.Undefined && aPart != null) aPartType = aPart.PartType;
            if ((int)aUnits < (int)uppUnitFamilies.English) aUnits =  uppUnitFamilies.English;
            aDrawingName =string.IsNullOrWhiteSpace(aDrawingName) ? string.Empty : aDrawingName.Trim(); // aDrawingType.GetDescription();
            if (string.IsNullOrWhiteSpace(aSelectText)) aSelectText = aDrawingName;

            if (aPart != null)
            {

                if (aPart is uopTrayRange)
                    aRange = aPart as uopTrayRange;
                if (aPart is uopTrayAssembly)
                    aAssy = aPart as uopTrayAssembly;
            }
            if (aRange == null && aAssy != null) aRange = aAssy.TrayRange;
            Family = aDrawingFamily;
            Name = aDrawingName;
            DrawingType = aDrawingType;
            Range = aRange;
            Part = aPart;
            ZoomExtents = bZoomExents;
            Cancelable = bCancelable;
            BorderSize = aBorderSize;
            OppositeHand = bOppositeHand;
            SelectText = aSelectText;
            Flag = aFlag;
            Category = aCategory;
            Hidden = bHidden;
            ProjectWide = bProjectWide;
            SheetNumber = aSheetIndex;
            DrawingUnits = aUnits;
        SubCategory = string.IsNullOrWhiteSpace(aSubCat) ? string.Empty : aSubCat.Trim();


            if (PartType == uppPartTypes.Project) ProjectWide = true;

            if (aPartIndex.HasValue) PartIndex = aPartIndex.Value;

            if (IsPlaceHolder)
            { NodeValue = SelectText.ToUpper(); }
            else
            { NodeValue = SelectText; }

            if (aQuestions != null)
            {
                if (!bQuestionsAreTrayPrompt)
                { Options = aQuestions.Clone(); }
                else
                {
                    TrayQuery = aQuestions.Clone();

                    mzQuestion units = TrayQuery.Item("Drawing Units");
                    if (units != null)
                    {
                        units.Answer = DrawingUnits.GetDescription();
                    }
                }


            }

      
        }

        #endregion

        /// <summary>
        /// returns the Clone of the object
        /// </summary>
        public uopDocDrawing Clone() => new uopDocDrawing(this);

            /// <summary>
            /// returns the Clone of the object
            /// </summary>
            public override uopDocument Clone(bool aFlag = false) => (uopDocument)this.Clone();

            /// <summary>
            /// returns the Clone of the object
            /// </summary>
            object ICloneable.Clone() => (object)this.Clone();


        //================== DWG SPECIFIC PROPERTIES and METHODS FOLLOW =============

        public override string ToString()
        {
            return $"uopDocDrawing[{  Name }]";
        }

        public Size DeviceSize { 
            get; 
            set;
        }


       public uopGrid Grid { get; set; }

        public override bool Hidden { get { base.Hidden = DrawingType < 0; return base.Hidden; } set =>  value = DrawingType < 0; }


        public override string Name { get => !string.IsNullOrWhiteSpace( base.Name) ? base.Name : DrawingType.GetDescription(); set => base.Name = value; }
        public uppDrawingFamily Family { get; set; }

        public uppDrawingTypes DrawingType { get => (uppDrawingTypes)base.SubType; set { if (value < 0) base.Hidden = true; base.SubType = (int)value; } }
        /// <summary>
        /// returns whether the drawing is for a particualr part or range a general tabular or informational drawing
        /// </summary>
        public bool ProjectWide { get => Props.ValueB("ProjectWide"); set => SetProp("ProjectWide", value); }
        /// <summary>
        /// the name of the drawing being requested
        /// </summary>
        public string DrawingName { get => Name; set { TDOCUMENT str = Structure; str.String3 = value; Name = value;  Structure = str; } }
        /// <summary>
        /// returns the Border Scale
        /// </summary>
        public string BorderScale { get => Props.ValueS("BorderScale"); set => SetProp("BorderScale", value); }
        /// <summary>
        /// returns the Cancelable
        /// </summary>
        public bool Cancelable { get => Props.ValueB("Cancelable"); set => SetProp("Cancelable", value); }
        /// <summary>
        /// returns the NoText
        /// </summary>
        public bool NoText { get => Props.ValueB("NoText"); set => SetProp("NoText", value); }
        /// <summary>
        /// returns whether it is opposite hand or not
        /// </summary>
        public bool OppositeHand { get => Props.ValueB("OppositeHand"); set => SetProp("OppositeHand", value); }
       
         /// <summary>
        /// return tag
        /// </summary>
        public string Tag { get => Props.ValueS("Tag"); set => SetProp("Tag", value); }
        /// <summary>
        /// returns Drawtime
        /// </summary>
        public string DrawTime { get => Props.ValueS("DrawTime"); set => SetProp("DrawTime", value); }
        /// <summary>
        /// returns writetime
        /// </summary>
        public string WriteTime { get => Props.ValueS("WriteTime"); set => SetProp("WriteTime", value); }
        /// <summary>
        /// returns border size
        /// </summary>
        public uppBorderSizes BorderSize { get => (uppBorderSizes)Props.ValueI("BorderSize"); set => SetProp("BorderSize",value); }
        /// <summary>
        /// returns tag address
        /// </summary>
        public string TagAddress { get => Props.ValueS("WriteTime"); set => SetProp("WriteTime", value); }
        /// <summary>
        /// the index of the drawing in a sub collection of drawings
        /// </summary>
        public int DrawingIndex { get => base.Index; set => base.Index = value; }
        /// <summary>
        /// returns the Drawing number
        /// </summary>
        public string DrawingNumber { get => Props.ValueS("DrawingNumber"); set => SetProp("DrawingNumber", value); }
        /// <summary>
        /// the units to display the drawing dimension in (English or Metric)
        /// </summary>
        public uppUnitFamilies DrawingUnits { 
            get => base.DisplayUnits; 
            set => base.DisplayUnits = value; 
        }

        public dxxDrawingUnits DrawingUnitsDXF => DrawingUnits == uppUnitFamilies.English ? dxxDrawingUnits.English : dxxDrawingUnits.Metric; 


        /// <summary>
        /// extra property too carry additional information about the requested drawing
        /// </summary>
        public string Flag { get => Props.ValueS("Flag"); set => SetProp("Flag", value ?? "" ); }
        /// <summary>
        /// the default location to create the drawings DXF file in
        /// </summary>
        public string OutputFolder { get => Structure.String1; set { TDOCUMENT str = Structure; str.String1 = value; Structure = str; } }
        
       
            
        public string PasteAddress { get => Props.ValueS("PasteAddress"); set => SetProp("PasteAddress", value); }

        /// <summary>
        /// returns the span name
        /// </summary>
        public string SpanName { get => RangeName; set => RangeName  = value ; }
        

        /// <summary>
        /// the scale width to apply to the display
        /// </summary>
        public double ExtentWidth { get => Props.ValueD("ExtentWidth"); set => SetProp("ExtentWidth", value); }
       
        /// <summary>
        /// returns the sheet number
        /// </summary>
        public  override int SheetNumber { get => Props.ValueI("SheetNumber"); set => SetProp("SheetNumber", value); }

      
        /// <summary>
        /// flag to indicate if the drawing should be zoomed to extents after it is drawn
        /// </summary>
        public bool ZoomExtents { get => Props.ValueB("ZoomExtents"); set => SetProp("ZoomExtents", value); }
        
        /// <summary>
        /// flag to ommit the insertion of the drawing border if one is defined
        /// </summary>
        public bool SuppressBorder { get => Props.ValueB("SuppressBorder"); set => SetProp("SuppressBorder", value); }
    
        /// <summary>
        /// Cancels the drawing
        /// </summary>
        /// <param name="rRecieved">Gets wthether the argument is recieved</param>
        public dynamic CancelDrawing(bool rRecieved)
        {
            dynamic CancelDrawing = null;
            eventCancelDrawing.Invoke(ref rRecieved);
            return CancelDrawing;
        }
        /// <summary>
        /// the center point to zoom on
        /// </summary>
        public dxfVector ViewCenter 
        {
            get => new dxfVector(X, Y);
            
            set
            {
                X = 0;
                Y = 0;
                if (value != null)
                {
                    X = value.X;
                    Y = value.Y;
                }
            }
        }
        
        public void UpdateDrawingUnits(uopProject aProject = null, uppUnitFamilies? aOverride = null)
        {
            if (aProject == null)
                aProject = Project;
            else
                Project = aProject;
            if (aOverride.HasValue) 
            {
                DrawingUnits = aOverride.Value;
                return;
            }

            if (aProject == null) return;



            mzQuestion units = TrayQuery.Item("Drawing Units");
            if (units != null)
            {

               DrawingUnits = string.Compare(units.AnswerS, "Metric", true) == 0 ? uppUnitFamilies.Metric : uppUnitFamilies.English;
            }
            else
            {

                DrawingUnits = Family switch
                {
                    uppDrawingFamily.Functional => uppUnitFamilies.English,
                    uppDrawingFamily.Installation => aProject.CustomerDrawingUnits,
                    uppDrawingFamily.Attachment => aProject.CustomerDrawingUnits,
                    uppDrawingFamily.Manufacturing => aProject.ManufacturingDrawingUnits,
                    uppDrawingFamily.Design => uppUnitFamilies.English,
                    uppDrawingFamily.TrayView => uppUnitFamilies.English,
                    _ => uppUnitFamilies.English
                };
            }

        }

     
    }
}