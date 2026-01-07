using UOP.WinTray.UI.BusinessLogic;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.Projects.Parts;
using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using UOP.WinTray.UI.Messages;
using UOP.WinTray.UI.Commands;

using UOP.DXFGraphics;
using System.ComponentModel;
using UOP.WinTray.Projects.Generators;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;

namespace UOP.WinTray.UI.ViewModels
{
    public class Edit_MDDowncomer_ViewModel : MDProjectViewModelBase, IModalDialogViewModel
    {




        #region Constructor

      
            public Edit_MDDowncomer_ViewModel(mdProject project, int dcindex, string fieldname = "") : base()
        {

            MDProject = project;
            Title = "Edit Downcomer Properties";


            IsOkBtnEnable = true;
            FocusTarget = fieldname;
            if (MDProject == null) return;
            if (MDAssy == null) return;
            IsGridClicked = dcindex > 0;
            MechanicalInput = IsGridClicked;
            GlobalInput = !MechanicalInput;
            if (MechanicalInput && dcindex > MDAssy.Downcomers.Count) dcindex = MDAssy.Downcomers.SelectedIndex;

             try
                {
                uopProperties eprops;
                Downcomer = MechanicalInput ? MDAssy.Downcomers.Item(dcindex) : MDAssy.Downcomer();
             
                if (MechanicalInput)
                {
                    VisibilitySpaceOverride = Visibility.Collapsed;
                    GlobalProjectTitle = $"{MDRange.Name(true)}.Downcomer({Downcomer.Index })";
                    VisibilityTriangularEP = (Downcomer.X ==0) ? Visibility.Collapsed : Visibility.Visible;
                    eprops = Downcomer.CurrentProperties();
                    eprops.SetCategories("DOWNCOMER");
                    uopProperty prop = eprops.Item("OverrideClipClearance");
                    if(prop != null)
                    {
                        double dval = MDAssy.Downcomers.DefaultRingClipClearance;
                        if (prop.ValueD <= 0) prop.SetValue(dval);
                    }
                    eprops.Item("EndplateInset");
                    if (prop != null)
                    {
                        if (prop.ValueD <= 0) prop.SetValue(Default_Inset);
                    }
                    //eprops.Remove("InsideHeight");
                    //eprops.Remove("Width");
                    //eprops.Remove("Asp");

                }
                else
                {

                    GlobalProjectTitle = $"{MDRange.Name(true)}.Global Downcomer";
                    VisibilityTriangularEP = Visibility.Collapsed;
                    Downcomer = MDAssy.Downcomer();
                    eprops = Downcomer.CurrentProperties();
                    eprops.SetCategories("DOWNCOMER");
                    uopProperty prop = MDAssy.CurrentProperties().Item("OverrideSpacing");
                    if (prop.ValueD <= 0) prop.Value = 0;
                    eprops.Add(prop, aCategory:"ASSY",aHeading:"");
                    eprops.Remove("EndplateInset");
                    eprops.Remove("OverrideClipClearance");
                    eprops.Remove("FoldOverHeight");
                    eprops.Remove("SupplementalDeflectorHeight");
                    eprops.Remove("HasTriangularEndPlate");
                    eprops.Remove("GussetedEndPlates");
                    eprops.Remove("BoltOnEndPlates");

                    Default_Space = MDAssy.Downcomers.OptimumSpacing;
                    VisibilitySpaceOverride = MDRange.DesignFamily.IsStandardDesignFamily() ? Visibility.Visible : Visibility.Collapsed ;
                }
                eprops.DisplayUnits = DisplayUnits;
               
                EditProps = eprops;

                
                DisplayUnits = MDProject.DisplayUnits;
               
               
            }
            finally
            {
                Validation_DefineLimits();
                this.IsEnabled = true;
            }
        }

        #endregion  Constructor

        #region Properties

        private colDXFEntities View_Entities { get; set; }
      

        private bool _MechanicalInput;
        public bool MechanicalInput { get => _MechanicalInput; set { _MechanicalInput = value; NotifyPropertyChanged("MechanicalInput"); VisibilityMechanical = value ? Visibility.Visible : Visibility.Collapsed; } }

        private bool _GlobalInput;
        public bool GlobalInput { get => _GlobalInput; set { _GlobalInput = value; NotifyPropertyChanged("GlobalInput"); VisibilityGlobal = value ? Visibility.Visible : Visibility.Collapsed; } }

        private Visibility _VisibilityMechanical;
        public Visibility VisibilityMechanical { get => _VisibilityMechanical; set { _VisibilityMechanical = value; NotifyPropertyChanged("VisibilityMechanical"); } }

        private Visibility _VisibilityGlobal;
        public Visibility VisibilityGlobal { get => _VisibilityGlobal; set { _VisibilityGlobal = value; NotifyPropertyChanged("VisibilityGlobal"); } }

        
        private Visibility _VisibilitySpaceOverride;
        public Visibility VisibilitySpaceOverride { get => _VisibilitySpaceOverride; set { _VisibilitySpaceOverride = value; NotifyPropertyChanged("VisibilitySpaceOverride"); } }

        private Visibility _VisibilityTriangularEP;
        public Visibility VisibilityTriangularEP { get => _VisibilityTriangularEP; set { _VisibilityTriangularEP = value; NotifyPropertyChanged("VisibilityTriangularEP"); } }

        public mdDowncomer Downcomer { get; set; }

        public bool IsGridClicked { get; set; }

        public string InsideHeight
        {
            get=> EditProps.DisplayValueString("InsideHeight");
            set { EditProps.SetDisplayUnitValue("InsideHeight", value); NotifyPropertyChanged("InsideHeight"); }
        }

        public string InsideWidth
        {
            get => EditProps.DisplayValueString("Width");
            set { bool newval =  EditProps.SetDisplayUnitValue("Width", value);   NotifyPropertyChanged("InsideWidth"); if (newval) UpdateOptimumSpace(); }
        }

        public string How
        {
            get => EditProps.DisplayValueString("How");
            set     { EditProps.SetDisplayUnitValue("How", value); NotifyPropertyChanged("How"); }
        }

        public string EndplateInset
        {
            get => EditProps.DisplayValueString("EndplateInset",true,true,aZeroValue: Default_Inset);
            set { EditProps.SetDisplayUnitValue("EndplateInset", value); NotifyPropertyChanges(); }
        }

        public double Default_Inset => 1.0;

        private Brush _ForegroundColor_Spacing = Brushes.Black;
        public Brush ForegroundColor_Spacing
        {
            get
            {
                if (Activated)
                {
                    double dVal = Default_Space;
                    double aVal = (EditProps != null) ? EditProps.ValueD("OverrideSpacing", 0) : 0;
                    _ForegroundColor_Spacing = (aVal != dVal && aVal != 0) ? Brushes.Blue : Brushes.Black;

                }
                return _ForegroundColor_Spacing;
            }
        }

        private Brush _ForegroundColor_Inset = Brushes.Black;
        public Brush ForegroundColor_Inset
        {
            get
            {
                if (Activated)
                {
                    double dVal = Default_Inset;
                    double aVal = (EditProps != null) ? EditProps.ValueD("EndplateInset", 0) : 0;
                    _ForegroundColor_Inset = (aVal != dVal && aVal != 0) ? Brushes.Blue : Brushes.Black;

                }
                return _ForegroundColor_Inset;
            }
        }

        private Brush _ForegroundColor_Clearance = Brushes.Black;
        public Brush ForegroundColor_Clearance
        {
            get
            {
                if (Activated)
                {
                    double dVal =Default_ClipClearance;
                    double aVal = (EditProps != null) ? EditProps.ValueD("OverrideClipClearance", 0) : 0;
                    _ForegroundColor_Clearance = (aVal != dVal && aVal != 0) ? Brushes.Blue : Brushes.Black;

                }
                return _ForegroundColor_Clearance;
            }
        }

        public string ToolTip_Inset => $"Default Inset = {Units_Linear.UnitValueString(Default_Inset, DisplayUnits)}";
        public string ToolTip_Clearance => $"Default Inset = {Units_Linear.UnitValueString(Default_ClipClearance, DisplayUnits)}";
        public string ToolTip_Spacing => $"Optimum Space = {Units_Linear.UnitValueString(Default_Space, DisplayUnits)}";

        public int Count
        {
            get => EditProps.ValueI("Count");
            set { bool newval = EditProps.SetProperty("Count", value); NotifyPropertyChanged("Count"); if(newval)UpdateOptimumSpace(); }
        }

        private double _Default_Space;
        public double Default_Space { get => _Default_Space; set { _Default_Space = value; NotifyPropertyChanges(); } }

        public double Asp
        {
            get => EditProps.DisplayUnitValue("Asp");
            set { EditProps.SetDisplayUnitValue("Asp", value); NotifyPropertyChanged("Asp"); }
        }

       
        public string OptimumSpaceString => !GlobalInput ? "" : $"Optimized Space : {Units_Linear.UnitValueString(Default_Space,DisplayUnits)}"; 
     
        public string OverrideClipClearance
        {
            get => EditProps.DisplayValueString("OverrideClipClearance", true,aZeroValue: Default_ClipClearance);
                
            set
            {
                double finalValue = 0.0;

                if (HasTriangularEndPlate)
                {
                    finalValue = Units_Linear.ConvertValue(value, DisplayUnits, uppUnitFamilies.English);
                    if (finalValue == MDAssy.Downcomers.DefaultRingClipClearance) finalValue = 0;
                    
                }
                EditProps.SetValue ("OverrideClipClearance", finalValue); NotifyPropertyChanges();
            }
        }

        public double Default_ClipClearance => (MDAssy != null) ? MDAssy.DefaultRingClipClearance : 1.375;

        public string OverrideSpacing
        {
            get => EditProps.DisplayValueString("OverrideSpacing", true, aZeroValue:Default_Space);
            set { if (string.IsNullOrWhiteSpace(value)) value = "0"; EditProps.SetDisplayUnitValue("OverrideSpacing", value); NotifyPropertyChanges(); }
        }

        public string FoldOverHeight
        {
            get => EditProps.DisplayValueString( "FoldOverHeight", true);
            set { EditProps.SetDisplayUnitValue("FoldOverHeight", value); NotifyPropertyChanged("FoldOverHeight");  }
        }

        public string SupplementalDeflectorHeight
        {
            get => EditProps.DisplayValueString("SupplementalDeflectorHeight", true);
            set { EditProps.SetDisplayUnitValue("SupplementalDeflectorHeight", value); NotifyPropertyChanged("SupplementalDeflectorHeight"); }
        }


        public bool HasTriangularEndPlate
        {
            get => EditProps.ValueB("HasTriangularEndPlate");
            set
            {
                
                EditProps.SetValue("HasTriangularEndPlate", value);
                NotifyPropertyChanged("HasTriangularEndPlate");
              
                DrawObjects();
            }
        }

        public bool GussetedEndplates
        {
            get => EditProps.ValueB("GussetedEndplates");
            set  {EditProps.SetValue("GussetedEndplates", value); NotifyPropertyChanged("GussetedEndplates"); }
        }

        public bool BoltOnEndplates
        {
            get => EditProps.ValueB("BoltOnEndplates");
            set { EditProps.SetValue("BoltOnEndplates", value); NotifyPropertyChanged("BoltOnEndplates"); }
        }

        /// <summary>
        /// Dialogservice result
        /// </summary>
        private bool? _DialogResult;
        public bool? DialogResult
        {
            get => _DialogResult;
            private set { _DialogResult = value; NotifyPropertyChanged("DialogResult");
            }
        }


        #endregion Properties

        #region Commands


        private DelegateCommand _CMD_Cancel;
        public ICommand Command_Cancel
        {
            get
            {
                if (_CMD_Cancel == null) _CMD_Cancel = new DelegateCommand(param => Execute_Cancel());
                return _CMD_Cancel;
            }
        }

        private DelegateCommand _CMD_OK;
        public ICommand Command_Ok
        {
            get
            {
                if (_CMD_OK == null) _CMD_OK = new DelegateCommand(param => Execute_Save());
                return _CMD_OK;
            }
        }

      

        #endregion Commands

        #region Validation of fields 

        private void UpdateOptimumSpace()
        {
            if (!GlobalInput || MDAssy == null) return;
            Default_Space = mdSpacingSolutions.OptimizedSpace(MDAssy, EditProps.ValueD("Width"), EditProps.ValueI("Count"));
                ValueLimit limits = EditPropertyValueLimits?.Find(x => string.Compare(x.PropertyName, "OverrideSpacing", true) == 0);
            if (limits != null)
            {
                limits.Min = Default_Space - 1.5;
                limits.Max = Default_Space + 1.5;
            }

                EditProps.SetValueD("OverrideSpacing", 0);
      
            NotifyPropertyChanges();
        

        }
        private void Validation_DefineLimits()
        {
           

            List<ValueLimit> Limits = new() { };
            Limits.Add(new ValueLimit("How", MDAssy.StartupDiameter + 0.375, (double)12, " (Startup Diameter + 0.375'')"));
            Limits.Add(new ValueLimit("Asp", (double)(0.01 * MDAssy.TotalFreeBubblingArea ) , (double)10000, " (1% of Free Bubbling Area)"));
            //Limits.Add(new ValueLimit("InsideHeight", (double)3.5, MDRange.RingSpacing - 2, "", " (Ring Spacing - 2'')"));
            Limits.Add(new ValueLimit("Width", (double)3, (double)24, "", ""));
            Limits.Add(new ValueLimit("OverrideClipClearance", (double)0.75, (double)2, bAllowZero:true ));
            Limits.Add(new ValueLimit("EndplateInset", (double)0.75, (double)12, bAllowZero: true, bAbsValue: true));
            Limits.Add(new ValueLimit("Count", (int)1, null, "", ""));
            Limits.Add(new ValueLimit("FoldOverHeight", (double)0.25, null, bAllowZero: true));
            Limits.Add(new ValueLimit("SupplementalDeflectorHeight", (double)1.75, MDAssy.Downcomer().MaxSupplementalDeflectorHeight, "", " (Stiffener Height - 0.25'')", bAllowZero: true));

            if (GlobalInput)
            {
                if(MDAssy.IsStandardDesign)
                     Limits.Add(new ValueLimit("OverrideSpacing", Default_Space-1.5, Default_Space + 1.5, "Optimimum Spacing - 1.5''", "Optimimum Spacing + 1.5''", bAllowZero: true));

            }

            EditPropertyValueLimits = Limits;
        }

        protected override string GetError(string aPropertyName)
        {
            if (!Activated) return "";
            string result = null;
            uopProperty prop = EditProps.Item(aPropertyName,true);
            if (prop == null) return "";

            //check for limits
            ValueLimit limits = EditPropertyValueLimits?.Find(x => string.Compare(x.PropertyName, aPropertyName, true) == 0);
            if(limits != null)
            {
                result = limits.ValidateProperty(prop, (prop.Units.UnitType == uppUnitTypes.SmallLength) ? Units_Linear : Units_Area, DisplayUnits);
            }

                //check inter-dependant values
                if (result == null)
            {
                switch (aPropertyName.ToUpper())
                {
                    case "OVERRIDESPACING":
                        if (!GlobalInput) break;
                        result = Validate_Spacing();
                        break;
                    case "COUNT":
                        if (!GlobalInput) break;
                        result = Validate_Spacing();
                        break;

                    case "WIDTH":
                        if (!GlobalInput) break;
                        result = Validate_Spacing();
                        break;
                    case "HOW":
                        if (!MechanicalInput) break;
                        result = Validate_FoldOver();
                        break;
                    case "FOLDOVERHEIGHT":
                        if (!MechanicalInput) break;
                        result = Validate_FoldOver();
                        break;
                    case "HASTRIANGULARENDLATE":
                
                        if (EditProps.ValueB("GussetedEndplates") && prop.ValueB)
                        {
                            result = "Downcomers With Triangular End Plates Cannot Have Gussets Applied!";
                        }
                        break;
                    case "GUSSETEDENDPLATES":

                        if (EditProps.ValueB("HasTriangularEndPlate") && prop.ValueB)
                        {
                             result = "Downcomers With Triangular End Plates Cannot Have Gussets Applied!";
                        }
                        break;
                }
            }

            
            int idx = ErrorCollection.FindIndex(x => string.Compare(x.Item1, aPropertyName, true) == 0);
            if (idx >= 0) ErrorCollection.RemoveAt(idx);
            if (!string.IsNullOrWhiteSpace(result)) ErrorCollection.Add(new Tuple<string, string>(aPropertyName, result));
            
            NotifyPropertyChanged("ErrorCollection");
            Validate_ShowErrors();
            return result;
        }

        private void Validate_ShowErrors()
        {

            if (ErrorCollection == null) { ErrorMessage = ""; return; }
            if (ErrorCollection.Count <= 0) { ErrorMessage = ""; return; }
            string msg = ErrorCollection[0].Item2;
            ErrorMessage = msg ;
        }


         private string Validate_FoldOver()
        {
            if (!MechanicalInput) return "";
            double dval = EditProps.ValueD("FoldOverHeight");
            if (dval <= 0) return "";
            double how = EditProps.ValueD("How");
            if(how < 1.75)
            {
                return  $"Only Downcomers With a Weir Height Greater Than Or Equal to {Units_Linear.UnitValueString(1.75,DisplayUnits) } Can Have Foldovers";
            }


            double maxlim = how - 1.25;
            if (dval > maxlim)
            {
                ErrorString = $"Max Fold Over Height = {Units_Linear.UnitValueString(maxlim, DisplayUnits) }";
            }
            return "";
        }

        private string Validate_Spacing()
        {

            if (!GlobalInput) return "";
            double dval =  EditProps.ValueD("OverrideSpacing");
            bool orride =dval > 0 && dval != Default_Space;
            if (dval <= 0) dval = Default_Space;
            int count = EditProps.ValueI("Count");
       
            if (count > mdUtils.MaxDowcomerCount(MDAssy.RingID, dval, count, EditProps.ValueD("Width")))
            {

                return orride ?
                    "Override Spacing Is Too Large Too Acommodate the Requested Number of Downcomers"
                    :
                    "Downcomers Width or Count Is Too Large Too Fit With The Current Spacing Value"
                    ;

            }
            return "";
        }

        #endregion Validation of fields


        #region Methods

        /// <summary>
        /// Save Downcomer
        /// </summary>
        /// <returns></returns>
        private void Execute_Save()
        {
            try
            {
                IsEnabled = false;
                Canceled = true;
                IsEnabled = false;
                IsOkBtnEnable = false;


                var validationResult = ValidateInput();
                ErrorMessage = validationResult;
                if (string.IsNullOrEmpty(ErrorMessage))
                {
                    Save();
                }
            }
            finally
            {
                IsEnabled = true;
                IsEnabled = true;
                IsOkBtnEnable = true;
            }
        }


        public override void Activate(Window myWindow)
        {
            if (Activated || MDAssy == null || Viewer == null) return;
            base.Activate(myWindow);
            DrawObjects();
            if (!string.IsNullOrWhiteSpace(FocusTarget)) SetFocus(FocusTarget);
        }

        private void DrawObjects()
        {

            if (Viewer == null) return;
            IsEnabled = false;

            if (Image != null) Image.Dispose();
            Viewer.SetImage(null);
    
            Image = new dxfImage(appApplication.SketchColor,Viewer.Size );
            Image.Header.UCSMode = dxxUCSIconModes.None;

            dxoDrawingTool draw = Image.Draw;
            dxoDimTool dims = Image.DimTool;
            bool tria = HasTriangularEndPlate;
            dxeDimension dim;
            
           
            bool global = GlobalInput;
            //draw the side view
            dxfRectangle rec1 = new(70, 40);
            dxfRectangle rec2 = new(rec1.Width, 30);
            rec2.Y = rec1.Y - 1.5 * rec1.Height - rec2.Height / 6;
     
            Image.Layers.Add("SQUARE", dxxColors.BlackWhite, bVisible:!tria);
            Image.Layers.Add("TRIA", dxxColors.BlackWhite, bVisible:tria );
            Image.DimStyle().SetColors(dxxColors.Blue, dxxColors.BlackWhite);
            Image.DimStyle().ArrowSize = 0.07;
            Image.DimStyle().ExtLineExtend = 0.03;
            //View_Entities = null;
            if (View_Entities == null) View_Entities = Draw_Entities(Image, rec1, rec2, -0.15 * rec1.Width);
            colDXFEntities ents = View_Entities.Clone();
            string suf = tria ? "TRIA" : "SQUARE";
            //ents.GetByDisplayVariableValue(dxxDisplayProperties.LayerName, suf, bRemove: true, bReturnInverse: true);
            if (!tria)
            {
                ents.GetByDisplayVariableValue(dxxDisplayProperties.LayerName, "TRIA", bRemove: true);

            }
            else
            {
                ents.GetByDisplayVariableValue(dxxDisplayProperties.LayerName, "SQUARE", bRemove: true);
            }



            colDXFEntities subents = ents.GetByTag("DIM-THICKNESS");
            if (subents.Count > 0)
            {
                dim = (dxeDimension)subents.Item(1);
                dim.OverideText = Units_Linear.UnitValueString(Downcomer.Thickness, DisplayUnits, bAddLabel: false);
            }
          
            if (!global)
            {
                subents = ents.GetByTag("DIM-HEIGHT");
                if(subents.Count > 0)
                {
                    dim = (dxeDimension)subents.Item(1);
                    dim.OverideText = Units_Linear.UnitValueString(Downcomer.InsideHeight, DisplayUnits, bAddLabel: false);
                }

                subents = ents.GetByTag($"DIM-WIDTH-{suf}");
                if (subents.Count > 0)
                {
                    dim = (dxeDimension)subents.Item(1);
                    dim.OverideText = Units_Linear.UnitValueString(Downcomer.Width, DisplayUnits, bAddLabel: false);
                }

                subents = ents.GetByTag("DIM-HOW");
                if (subents.Count > 0)
                {
                    dim = (dxeDimension)subents.Item(1);
                    dim.OverideText = Units_Linear.UnitValueString(Downcomer.How, DisplayUnits, bAddLabel: false);
                }

               
                
                
              
                //View_Entities = ents.Clone();

                //Image.Entities.Append(View_Entities);
            }
            Image.Entities.Append(ents);

            ////Image.Header.LineTypeScale = 5;

            //ents.Add(draw.aPolyline(new colDXFVectors(rec1.TopLeft, rec1.TopRight, rec1.BottomRight, rec1.BottomLeft)));


            //colDXFVectors verts = new colDXFVectors(rec2.TopLeft, rec2.TopRight, rec2.TopRight.Moved(0, -thk), rec2.TopLeft.Moved(0, -thk));
            //ents.Add( draw.aPolyline(verts));
            //verts = new colDXFVectors(rec2.BottomLeft, rec2.BottomRight, rec2.BottomRight.Moved(0, thk), rec2.BottomLeft.Moved(0, thk));
            //if (tria) { verts.Move(oset); }
            //    ents.Add(draw.aPolyline(verts));

            //Image.Display.ZoomExtents(1.5, bSetFeatureScale: true);

            //double pscale = Image.Display.PaperScale;

            //Image.DimStyle().SetColors( dxxColors.Blue, dxxColors.BlackWhite);
            //Image.DimStyle().ArrowSize = 0.07;
            //Image.DimStyle().ExtLineExtend = 0.03;
            //dxfVector v1 = rec1.BottomLeft.Moved(aYChange: thk);
            //dxfVector v2 = rec1.BottomRight.Moved(aYChange: thk);

            //dxeLine l1 = new dxeLine(v1, v2) { Linetype = dxfLinetypes.Hidden };
            //ents.Add(Image.Entities.Add(l1));

            //v1.Move(0.0625 * rec1.Width, 0.75 * rec1.Height);
            //v2.Move(-0.125 * rec1.Width, 0.75 * rec1.Height);
            //dxfVector v3 = v2.Moved(0, -0.25 * rec1.Height);
            //dxfVector v4 = v1.Moved(0, -0.25 * rec1.Height);
            //uopProperties props = EditProps;

            //ents.Add(draw.aPolyline(new colDXFVectors(v1, v2, v3, v4)));
            //ents.Add(draw.aLine(v1.Moved(0, -thk), v2.Moved(0, -thk)));
            //txt = global ? "Height" : Units_Linear.UnitValueString(Downcomer.InsideHeight, DisplayUnits, bAddLabel: false);
            //dxeDimension dim = dims.Vertical(l1.StartPt, rec1.TopLeft, -0.008 * pscale, aOverideText:txt);
            //ents.Add(dim);
            //string err = "";
            //ents.Add(dims.TickLine(dim, 1, -thk, null, "", dxxColors.Undefined,"",false,ref err));

            //txt =  $"{Units_Linear.UnitValueString(Downcomer.Thickness, uppUnitFamilies.English, bAddLabel: false)}'' [{Units_Linear.UnitValueString(Downcomer.Thickness, uppUnitFamilies.Metric, bAddLabel: false)}]";
            //dim = dims.Vertical(l1.EndPt, rec1.BottomRight, 0.008 * pscale, aOverideText: txt);
            //ents.Add(dim);
            //txt = global ? "How" : Units_Linear.UnitValueString(Downcomer.How, DisplayUnits, bAddLabel: false);
            //ents.Add(dims.Vertical(v2, rec1.TopRight, 0.008 * pscale, aOverideText: txt));

            //txt = global ? "Width" : Units_Linear.UnitValueString(Downcomer.Width, DisplayUnits, bAddLabel: false);
            //dim = dims.Vertical(rec2.BottomLeft.Moved(oset,thk), rec2.TopLeft.Moved(0,-thk), -0.008 * pscale + (oset/ pscale), aOverideText: txt);
            //ents.Add(dim);
            //ents.Add(dims.TickLine(dim, 2, -thk, null, "", dxxColors.Undefined, "", false, ref err));
            //ents.Add(dims.TickLine(dim, 1, -thk, null, "", dxxColors.Undefined, "", false, ref err));


            //double rad = 2.5;
            //d1 = 2.5 * rad;
            //v1 = rec2.Center.Moved(0.0625 * rec2.Width);
            //dxeArc spout = new dxeArc(v1, rad);
            //spout.Instances.Add($"0,{d1},0,{-d1},{-d1},0,{-d1},{d1},{-d1},{-d1},{-2 * d1},0,{-2 * d1},{d1},{-2 * d1},{-d1}");

            ////spout.Instances.Add(0, 2.5 * rad);
            ////spout.Instances.Add(0, -2.5 * rad);
            ////spout.Instances.Add(-2.5 * rad, 0);
            ////spout.Instances.Add(-2.5 * rad, 2.5 * rad);
            ////spout.Instances.Add(-2.5 * rad, -2.5 * rad);
            ////spout.Instances.Add(-5 * rad, 0);
            ////spout.Instances.Add(-5 * rad, 2.5 * rad);
            ////spout.Instances.Add(-5 * rad, -2.5 * rad); 

            //ents.Add(Image.Entities.Add(spout));

            //v1 = rec2.TopRight.Moved(-0.15 * rec2.Width, -thk);
            //v2 = rec2.BottomRight.Moved(-0.15 * rec2.Width , thk);
            //v3 = v1.Clone();
            //v4 = v2.Clone();
            //v3.X = rec2.Right + 0.15 * rec2.Width ;
            //v4.X = v3.X + oset;

            //verts = new colDXFVectors(v3,v1,v2,v4);

            //verts.Add(v4.Moved(0, thk));
            //verts.Add(v2.Moved(thk, thk));
            //verts.Add(v1.Moved(thk, -thk));
            //verts.Add(v3.Moved(0, -thk));


            //if (tria) {verts.Item(3).Move( oset- thk); verts.Item(6).Move(oset); }

            //l1 = draw.aLine(verts.Item(5), verts.Item(8));
            //l1.TFVSet("ENDPLATE", "ENDLINE");
            //ents.Add(l1);
            //ents.Add(draw.aPolyline(verts,true));
            ////txt = global ? "Inset" : Units_Linear.UnitValueString(Math.Abs( Downcomer.EndplateInset), DisplayUnits, bAddLabel: false);
            //if (!global)
            //{
            //    txt = "Inset";
            //    v1 = rec2.BottomRight.Moved(oset);
            //    ents.Add(dims.Horizontal(v2, v1, -0.008 * pscale, aOverideText: txt));

            //}

            //spout = (dxeArc)spout.Clone();
            //spout.Instances.Clear();
            //l1 = (dxeLine)l1.Clone();
            //l1.MoveOrthogonal(3 * spout.Radius);
            //spout.Radius = 0.5 * spout.Radius;
            //spout.Center = l1.MidPt; spout.Y = rec2.Y;
            //ents.Add(Image.Entities.Add(spout, bAddClone: true));

            ////if (tria)
            ////{
            //    l1 = new dxeLine(verts.Item(6), verts.Item(7));

            //    v1 = spout.Center.ProjectedToLine(l1,null,ref d1);
            //    ents.Add(dims.Aligned(v1, spout.Center, 0.02 * pscale, aOverideText: "Ring Clip\\PClearance"));
            ////}


            Viewer.SetImage(Image,true,true);
            Viewer.ZoomExtents(1.1);

            Image.Dispose();
            Image = null;

            IsEnabled = true;


        }
        private colDXFEntities Draw_Entities(dxfImage image, dxfRectangle rec1, dxfRectangle rec2, double oset)
        {

          
            dxoDimTool dims = image.DimTool;
            colDXFVectors verts = rec1.Corners();
            verts.Append(rec2.Corners());
            string txt;
            double d1 = 0;
            double thk = 2;
            bool global = GlobalInput;
            dxfRectangle bounds = verts.BoundingRectangle();
            dxfDisplaySettings dsp = new();
            //draw the side view

            bounds.Rescale(1.5);
            image.Display.SetDisplayRectangle(bounds, bSetFeatureScales: true);

            colDXFEntities ents = new();

            //image.Header.LineTypeScale = 5;


            //draw the side view rectangle
            verts = new colDXFVectors(rec1.TopLeft, rec1.TopRight, rec1.BottomRight, rec1.BottomLeft);
            ents.Add(new dxePolyline(verts, bClosed: false, aDisplaySettings: dsp), aTag: "Side View");

            //draw the hidden bootm thickness line
            dxfVector v1 = rec1.BottomLeft.Moved(aYChange: thk);
            dxfVector v2 = rec1.BottomRight.Moved(aYChange: thk);
            dsp.Linetype = dxfLinetypes.Hidden;
            dxeLine l1 = new(v1, v2) { Linetype = dxfLinetypes.Hidden};
            ents.Add(l1, aTag: "Bottom Thickness");

            // draw the support angle

            dsp.LayerName = "0";
            v1.Move(0.0625 * rec1.Width, 0.75 * rec1.Height);
            v2.Move(-0.125 * rec1.Width, 0.75 * rec1.Height);
            dxfVector v3 = v2.Moved(0, -0.25 * rec1.Height);
            dxfVector v4 = v1.Moved(0, -0.25 * rec1.Height);

            ents.Add(new dxePolyline(new colDXFVectors(v1, v2, v3, v4)), aTag: "Support Angle");
            ents.Add(new dxeLine(v1.Moved(0, -thk), v2.Moved(0, -thk)), aTag: "Support Angle");

            //draw the top view downcomer edges
            //top
            dsp.Linetype = "CONTINUOUS";
            verts = new colDXFVectors(rec2.TopLeft, rec2.TopRight, rec2.TopRight.Moved(0, -thk), rec2.TopLeft.Moved(0, -thk));
            dxePolyline edge = new(verts, bClosed: false, aDisplaySettings: dsp);
            ents.Add(edge, aTag: "Top Edge");

            //draw the top view downcomer edges
            //bottom square
           
            dsp.LayerName = "SQUARE";
            verts = new colDXFVectors(rec2.BottomLeft, rec2.BottomRight, rec2.BottomRight.Moved(0, thk), rec2.BottomLeft.Moved(0, thk));
            dxePolyline edge1 = new(verts.Clone(), bClosed: false, aDisplaySettings: dsp);
            ents.Add(edge1, aTag: "Bottom Edge - Square");

            //bottom triangular
            dsp.LayerName = "TRIA";
            verts.Move(oset);
            dxePolyline edge2 = new(verts.Clone(), bClosed: false, aDisplaySettings: dsp);
            ents.Add(edge2, aTag: "Bottom Edge - Triangular");

            

            double pscale = image.Display.PaperScale;
          
            txt = global ? "Height" : Units_Linear.UnitValueString(Downcomer.InsideHeight, DisplayUnits, bAddLabel: false);
            dxeDimension dim = dims.Vertical(l1.StartPt, rec1.TopLeft, -0.008 * pscale, aOverideText: txt, bCreateOnly:true);
            ents.Add(dim, aTag: "DIM-HEIGHT");
           
            ents.Add(dims.TickLine(dim, 1, -thk, null, "", dxxColors.Undefined, "", true)) ;

            txt = Units_Linear.UnitValueString(Downcomer.Thickness, DisplayUnits, bAddLabel: false);
            dim = dims.Vertical(l1.EndPt, rec1.BottomRight, 0.008 * pscale, aOverideText: txt, bCreateOnly: true);
            ents.Add(dim, aTag: "DIM-THICKNESS");

            txt = global ? "How" : Units_Linear.UnitValueString(Downcomer.How, DisplayUnits, bAddLabel: false);
            dim = dims.Vertical(v2, rec1.TopRight, 0.008 * pscale, aOverideText: txt, bCreateOnly: true);
            ents.Add(dim, aTag: "DIM-HOW");

            dxeLine tick;
            dsp.LayerName = "SQUARE";
            txt = global ? "Width" : Units_Linear.UnitValueString(Downcomer.Width, DisplayUnits, bAddLabel: false);
            dim = dims.Vertical(edge1.Vertex(4), rec2.TopLeft.Moved(0, -thk), -0.008 * pscale + (oset / pscale), aOverideText: txt, aDisplaySettings: dsp, bCreateOnly: true);
            ents.Add(dim, aTag: "DIM-WIDTH-SQUARE");
            tick = dims.TickLine(dim, 2, -thk, null, "", dxxColors.Undefined, "", true);
            ents.Add(tick, aTag: "TICK_TOP");
            tick = dims.TickLine(dim, 1, -thk, null, "", dxxColors.Undefined, "", true);
            ents.Add(tick, aTag: "TICK_BOT");

            dsp.LayerName = "TRIA";
            txt = global ? "Width" : Units_Linear.UnitValueString(Downcomer.Width, DisplayUnits, bAddLabel: false);
            dim = dims.Vertical(edge2.Vertex(4), rec2.TopLeft.Moved(0, -thk), -0.008 * pscale + (oset / pscale), aOverideText: txt, aDisplaySettings: dsp, bCreateOnly: true);
            ents.Add(dim, aTag: "DIM-WIDTH-TRIA");
            tick = dims.TickLine(dim, 2, -thk, null, "", dxxColors.Undefined, "", true);
            ents.Add(tick, aTag: "TICK_TOP");
            tick = dims.TickLine(dim, 1, -thk, null, "", dxxColors.Undefined, "", true);
            ents.Add(tick, aTag: "TICK_BOT");



            // draw the spout group
            double rad = 2.5;
            d1 = 2.5 * rad;
            v1 = rec2.Center.Moved(0.0625 * rec2.Width);
            dxeArc spout = new(v1, rad);
            spout.Instances.Add($"0,{d1},0,{-d1},{-d1},0,{-d1},{d1},{-d1},{-d1},{-2 * d1},0,{-2 * d1},{d1},{-2 * d1},{-d1}");
            ents.Add(spout, aTag: "SPOUTS");

            // draw the end plate
            double iset = 0.15 * rec2.Width;
            v1 = edge.Vertex(3).Moved(iset);
            v2 = edge.Vertex(3).Moved(-iset);
            v3 = edge1.Vertex(3).Moved(-iset);
            v4 = edge1.Vertex(3).Moved(iset);

            verts = new colDXFVectors(v1, v2, v3, v4, v4.Moved(0, thk), v3.Moved(thk, thk), v2.Moved(thk, -thk), v1.Moved(0, -thk));


            //draw the square edplate
            dsp.LayerName = "SQUARE";
            dxePolyline ep1 = new(verts.Clone(), true, dsp);
            ents.Add(ep1, aTag: "ENDPLATE_SQUARE");
            l1 = new dxeLine(verts.Item(5), verts.Item(8), dsp);
            ents.Add(l1, aTag: "ENDPLATE_LINE_SQUARE");

            dxeArc rchole1 = (dxeArc)spout.Clone();
            rchole1.LayerName = "SQUARE";
            rchole1.Instances.Clear();
            l1 =l1.Clone();
            l1.MoveOrthogonal(3 * rchole1.Radius);
            rchole1.Radius = 0.5 * rchole1.Radius;
            rchole1.Center = l1.MidPt; rchole1.Y = rec2.Y;
            ents.Add(rchole1, aTag: "RINGCLIP HOLE_SQUARE");

            if (!global)
            {
                txt = "Inset";
                v1 = edge1.Vertex(2, true);
                v2 = ep1.Vertex(3, true);
                dim = dims.Horizontal(v2, v1, -0.008 * pscale, aOverideText: txt, aDisplaySettings: dsp, bCreateOnly: true);
                ents.Add(dim, aTag: "DIM-INSET");

            }

            //draw the triangular edplate
            dsp.LayerName = "TRIA";
            v1 = edge.Vertex(3).Moved(iset);
            v2 = edge.Vertex(3).Moved(-iset);
            v3 = edge2.Vertex(3).Moved(-iset);
            v4 = edge2.Vertex(3).Moved(iset);

            verts = new colDXFVectors(v1, v2, v3, v4, v4.Moved(0, thk), v3.Moved(1.8 * thk, thk), v2.Moved(thk, -thk), v1.Moved(0, -thk));

            dxePolyline ep2 = new(verts.Clone(), true, dsp);
            ents.Add(ep2, aTag: "ENDPLATE_TRIA");
            dxeLine l2 = new(verts.Item(5), verts.Item(8),dsp);
            ents.Add(l2, aTag: "ENDPLATE_LINE_TRIA");

            l1 = (dxeLine)l2.Clone();
            dxeArc rchole2 = (dxeArc)rchole1.Clone();
            rchole2.LayerName = "TRIA";
            l1.MoveOrthogonal(5 * rchole2.Radius);
            rchole2.Center = l1.MidPt; rchole2.Y = rec2.Y;
            ents.Add(rchole2, aTag: "RINGCLIP HOLE_TRIA");

            if (!global)
            {
                txt = "Inset";
                v1 = edge2.Vertex(2, true);
                v2 = ep2.Vertex(3, true);
                dim = dims.Horizontal(v2, v1, -0.008 * pscale, aOverideText: txt,aDisplaySettings:dsp, bCreateOnly: true);
                ents.Add(dim, aTag:"DIM-INSET-TRIA");

            }
         
            if (!global)
            {
                l1 = new dxeLine(ep1.Vertex(6), ep1.Vertex(7));

                v1 = rchole1.Center.ProjectedToLine(l1, null, ref d1);
                dsp.LayerName = "SQUARE";
                v3 = edge.Vertex(3).Moved(0, 0.4 * pscale);

                dim = dims.Horizontal(v1, rchole1.Center, v3.Y, bAbsolutePlacement:true, aOverideText: "Ring Clip\\PClearance", aDisplaySettings: dsp, bCreateOnly: true);
                ents.Add(dim, aTag: "DIM-RCCLEARANCE-SQUARE");

                l1 = new dxeLine(ep2.Vertex(6), ep2.Vertex(7));

                v1 = rchole2.Center.ProjectedToLine(l1, null, ref d1);
                dsp.LayerName = "TRIA";
                dim = dims.Aligned(v1, rchole2.Center, 0.03 * pscale, aOverideText: "Ring Clip\\PClearance", aDisplaySettings: dsp, bCreateOnly: true);
                ents.Add(dim, aTag: "DIM-RCCLEARANCE-TRIA");


            }


            return ents;


        }


        /// <summary>
        /// Save downcomer
        /// </summary>
        private void Save()
        {
          
            Message_Refresh refresh = null;
            try
            {
                uopProperties changes = GetEditedProperties(); // EditProps.GetByValueChange(true);
                refresh = new Message_Refresh(bSuppressTree: true, aPartTypeList: new List<uppPartTypes>() { uppPartTypes.Downcomer }, bCloseDocuments: true);
                bool requirychanges = false;
                
                if (changes.Count > 0)
                {

                    //massage some properties to replace zeros for defaults on overrides
          
                    if (changes.TryGet("OverrideSpacing", out uopProperty prop))
                    {
                        double dval = prop.ValueD;
                        if (dval <= 0 || dval == mdSpacingSolutions.OptimizedSpace(MDAssy, EditProps.ValueD("Width"), EditProps.ValueI("Count")))
                        {
                            EditProps.SetValue(prop.Name, 0); requirychanges = true;
                        }
                    }
                    if (changes.TryGet("OverrideClipClearance", out prop))
                    {
                        double dval = prop.ValueD;
                        if (dval <= 0 || dval == MDAssy.DefaultRingClipClearance)
                        {
                            EditProps.SetValue(prop.Name, 0); requirychanges = true;
                        }
                    }
                    if (changes.TryGet("EndplateInset", out prop))
                    {
                        double dval = prop.ValueD;
                        if (dval <= 0 || dval == 1)
                        {
                            EditProps.SetValue(prop.Name, -1); requirychanges = true;
                        }
                      
                    }

                }

                if(requirychanges) 
                    changes = GetEditedProperties();

                if (changes.Count > 0)
                {
                    Canceled = false;
                    mdDowncomer downcomer = Downcomer;
                    bool dcproperty;
                    bool invalidateall = false;
                    string pname;
                    bool reoptimize = false;
                    foreach (uopProperty change in changes)
                    {

                        pname = change.Name.ToUpper();
                        dcproperty = true;
                        uopProperty propchange = null;
                        switch (pname)
                        {
                            case "OVERRIDESPACING":
                                refresh.SuppressImage = false;
                                refresh.SuppressDataGrids = false;
                                //if (change.ValueD > 0)
                                //{
                                reoptimize = true;
                              propchange =   MDAssy.PropValSet(change.Name, change.Value,bSuppressEvnts:true);
                                if (propchange != null)
                                    MDAssy.Notify(propchange);
                                invalidateall = true;
                                //

                                dcproperty = false;
                                break;
                            case "ASP":
                            case "ENDPLATEINSET":
                            case "HASTRIANGULARENDPLATE":
                            case "COUNT":
                            case "WIDTH":
                                refresh.SuppressDataGrids = false;
                                refresh.SuppressImage = false;
                                invalidateall = true;
                                reoptimize = true;
                                break;

                            case "SUPPLEMENTALDEFLECTORHEIGHT":
                                refresh.SuppressDataGrids = false;
                                refresh.SuppressDocumentClosure = false;
                                refresh.SuppressTree = false;
                                refresh.SuppressImage = false;
                              
                                break;
                            case "FOLDOVERHEIGHT":
                            case "GUSSETEDENDPLATES":
                            case "BOLTONENDPLATES":
                                refresh.SuppressDataGrids = false;
                                refresh.SuppressDocumentClosure = false;
                                refresh.SuppressTree = false;
                                if (pname == "FOLDOVERHEIGHT" ) refresh.SuppressImage = false;
                                break;
                            case "HOW":
                                refresh.SuppressDataGrids = false;
                                refresh.PartTypeList.Add(uppPartTypes.Deck);

                                break;
                            default:
                                break;

                        }

                       
                        if (dcproperty)
                        {
                            propchange = downcomer.PropValSet(change.Name, change.Value, bSuppressEvnts: true);
                        }
                        if (propchange != null)
                        {
                            MDAssy.Notify(propchange);
                            refresh.Changes.Add(propchange);
                        }
                            

                    }
                
                if (invalidateall)
                    {
                        refresh.PartTypeList.Clear();
                        refresh.SuppressImage = false;
                       

                       // if (reoptimize)
                       //     MDAssy.OptimizeSpacing();
                       //else
                            MDAssy.ResetComponents(reoptimize);

                        MDProject.HasChanged = true;
                    }
                }
                else
                {
                    Canceled = true;
                }

                //if (!Canceled)
                //{
                //    MDProject.HasChanged = true;
                //    WinTrayMainViewModel mainVM = WinTrayMainViewModel.WinTrayMainViewModelObj;
                //    if (mainVM != null) mainVM.RefreshDisplay(refresh);
                   
                //}
              
               
            }
            catch { }
            finally
            {
                RefreshMessage = (!Canceled) ?refresh: null; 
               
                DialogResult = !Canceled;
             }
        }

        /// <summary>
        /// Validate input fields entered by user
        /// </summary>
        /// <returns></returns>
        private string ValidateInput()
        {
            if(EditProps == null) return "";
            string err = "";
            uopProperty item = null;
            for(int i = 1; i <= EditProps.Count; i++)
            {
                item = EditProps.Item(i);
                err = GetError(item.Name);
               
            }
            Validate_ShowErrors();
            return ErrorMessage;
        }

      
        public override uppUnitFamilies DisplayUnits
        {
            get => base.DisplayUnits;
            set
            {
                base.DisplayUnits = value;

                
              NotifyPropertyChanges();
              DrawObjects();
            }
        }

        
        /// <summary>
        /// Close the Edit Downcomersforms
        /// </summary>
        private void Execute_Cancel()
        {
            DialogResult = false;
        }

        public override void SetFocus(string aControlName)
        {
            if (string.IsNullOrWhiteSpace(aControlName)) aControlName = FocusTarget;
            if (string.IsNullOrWhiteSpace(aControlName)) return;


            string pname = aControlName.Trim();
          
     
            if (!EditProps.TryGet(pname, out uopProperty prop))
            {
                EditProps.TryGet("RingStart", out prop);
            }
            if (prop != null) base.SetFocus(prop.Name);


        }



        private void NotifyPropertyChanges()
        {
            

          
            NotifyPropertyChanged("Count");
            NotifyPropertyChanged("Asp");
            NotifyPropertyChanged("OverrideSpacing");
            NotifyPropertyChanged("InsideHeight");
            NotifyPropertyChanged("InsideWidth");
            NotifyPropertyChanged("FoldOverHeight"); 
            NotifyPropertyChanged("SupplementalDeflectorHeight");  
            NotifyPropertyChanged("OptimumSpaceString");
            NotifyPropertyChanged("EndplateInset");
            NotifyPropertyChanged("How");
            NotifyPropertyChanged("ToolTip_Inset");
            NotifyPropertyChanged("OverrideClipClearance"); 
            NotifyPropertyChanged("ToolTip_Clearance");
            NotifyPropertyChanged("ToolTip_Spacing");
            NotifyPropertyChanged("Default_Space");
            NotifyPropertyChanged("ForegroundColor_Spacing");
            NotifyPropertyChanged("ForegroundColor_Clearance");
            NotifyPropertyChanged("VisibilityGlobal");
            NotifyPropertyChanged("VisibilityMechanical");
            NotifyPropertyChanged("ForegroundColor_Inset");
            NotifyPropertyChanged("GlobalProjectTitle");
            
            try
            {


                var props = this.GetType().GetProperties();
                foreach (var item in props)
                {
                    //System.Diagnostics.Debug.Print(item.Name);
                 
                    if (item.CanWrite)
                    {
                        if(item.Name != "IsGridClicked")
                        {
                            //System.Diagnostics.Debug.Print(item.Name);
                            NotifyPropertyChanged(item.Name);
                        }
                        //System.Diagnostics.Debug.Print(item.Name);
                       
                    }

                }
            }
            catch { }
        }


        internal void Window_Closing(object sender, CancelEventArgs e)
        {
           
            Image = null;
            Viewer = null;
         
        }
        #endregion Methods
    }
}