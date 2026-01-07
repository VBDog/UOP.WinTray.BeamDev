
using Ardalis.SmartEnum;
using System;
using System.Globalization;
using System.Reflection;
using UOP.DXFGraphics;
using UOP.WinTray.Projects.Structures;
using UOP.WinTray.Projects.Utilities;
using static System.Net.Mime.MediaTypeNames;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

/// <summary>
/// this class contains all of the DLL defined enums
/// never needs to be instantiated
/// </summary>
namespace UOP.WinTray.Projects.Enums
{


    public sealed class upnUnitTypes : SmartEnum<upnUnitTypes>
    {
        public static readonly upnUnitTypes uniUndefined = new upnUnitTypes("uniUndefined", 0, "Undefined");
        public static readonly upnUnitTypes uniSmallLength = new upnUnitTypes("uniSmallLength", 1, "Small Length");
        public static readonly upnUnitTypes uniBigLength = new upnUnitTypes("uniBigLength", 2, "Big Length");
        public static readonly upnUnitTypes uniSmallArea = new upnUnitTypes("uniSmallArea", 3, "Small Area");
        public static readonly upnUnitTypes uniBigArea = new upnUnitTypes("uniBigArea", 4, "Big Area");

        public string Description { get; }
        public upnUnitTypes(string name, int value, string description) : base(name, value) { Description = description; }
        //[Description("Undefined")]
        //uniUndefined = 0,
        //[Description("Small Length")]
        //uniSmallLength = 1,
        //[Description("Big Length")]
        //uniBigLength = 2,
        //[Description("Small Area")]
        //uniSmallArea = 3,
        //[Description("Big Area")]
        //uniBigArea = 4,
        //[Description("Percentage")]
        //uniPercentage = 5,
        //[Description("Big Percentage")]
        //uniBigPercentage = 6,
        //[Description("Volume Percentage")]
        //uniVolumePercentage = 7,
        //[Description("Mass Rate")]
        //uniMassRate = 8,
        //[Description("Big Mass Rate")]
        //uniBigMassRate = 9,
        //[Description("Density")]
        //uniDensity = 10,
        //[Description("Volumn Rate")]
        //uniVolumeRate = 11,
        //[Description("Viscosity")]
        //uniViscosity = 12,
        //[Description("Surface Tension")]
        //uniSurfaceTension = 13,
        //[Description("Seconds")]
        //uniSeconds = 14,
        //[Description("ρV²")]
        //uniRhoVSqr = 15,
        //[Description("Velocity")]
        //uniVelocity = 16,
        //[Description("Weight")]
        //uniWeight = 17

    }
    public static class uopEnums
    {
        public static uppStartupSpoutConfigurations SpoutConfigurationFromString(string aConfigurationName, out string rError)
        {
            rError = string.Empty;
            if (string.IsNullOrWhiteSpace(aConfigurationName))
            {
                rError = "Null string  detected";
                return uppStartupSpoutConfigurations.Undefined;
            }

            aConfigurationName = aConfigurationName.Trim().ToUpper();
            switch (aConfigurationName)
            {
                case "0":
                case "NONE":
                    return uppStartupSpoutConfigurations.None;

                case "1":
                case "TWOBYTWO":
                case "2X2":
                case "2 X 2":
                    return uppStartupSpoutConfigurations.TwoByTwo;
                case "2":
                case "TWOBYFOUR":
                case "2X4":
                case "2 X 4":
                    return uppStartupSpoutConfigurations.TwoByFour;
                case "3":
                case "FOURBYFOUR":
                case "4X4":
                case "4 X 4":
                    return uppStartupSpoutConfigurations.FourByFour;
                case "5":
                case "BYDEFAULT":
                case "BY DEFAULT":
                    return uppStartupSpoutConfigurations.ByDefault;
                case "-1":
                case "UNDEFINED":
                    return uppStartupSpoutConfigurations.Undefined;
                default:
                    {
                        rError = $"The passed string '{aConfigurationName}' does not describe a spout pattern";
                        return uppStartupSpoutConfigurations.Undefined;
                    }



            }

        }

        public static uppSpoutPatterns SpoutPatternFromString(string aPatternName,out string rError)
        {
            rError = string.Empty;
            if (string.IsNullOrWhiteSpace(aPatternName))
            {
                rError = "Null string  detected";
                return uppSpoutPatterns.Undefined;
            } 

            aPatternName = aPatternName.Trim().ToUpper();
            if(aPatternName.Length >2) aPatternName = aPatternName.Substring(0,2);
            switch (aPatternName)
            {
                case "A":
                        return uppSpoutPatterns.A;
                case "B":
                    return uppSpoutPatterns.B;
                case "C":
                    return uppSpoutPatterns.C;
                case "D":
                    return uppSpoutPatterns.D;
                case "A*":
                    return uppSpoutPatterns.Astar;
                case "S1":
                    return uppSpoutPatterns.S1;
                case "S2":
                    return uppSpoutPatterns.S2;
                case "S3":
                    return uppSpoutPatterns.S3;
                case "S*":
                    return uppSpoutPatterns.SStar;

                default:
                    {
                        rError = $"The passed string '{aPatternName}' does not describe a spout pattern";
                        return uppSpoutPatterns.Undefined;
                    }
                    


            }

        }

        /// <summary>
        /// Common method to convert Enum to specific type
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="val"></param>
        /// <returns></returns>
        /// 
        public static TEnum GetEnumValue<TEnum>(dynamic val)
        {
            string enumToConv = string.Empty;
            if (mzUtils.IsNumeric(val))
            {
                enumToConv = Enum.GetName(typeof(TEnum), mzUtils.VarToInteger(val));
            }
            else
            {
                enumToConv = val.ToString();
            }

            return (TEnum)Enum.Parse(typeof(TEnum), enumToConv.ToString());
        }




        public static string GetEnumDescription(System.Enum input)
        {
            Type type = input.GetType();
            MemberInfo[] memInfo = type.GetMember(input.ToString());

            if (memInfo != null && memInfo.Length > 0)
            {
                MemberInfo minfo = memInfo[0];
                try
                {
                    object[] attrs = (object[])minfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                    if (attrs.Length > 0)
                    {
                        return ((DescriptionAttribute)attrs[0]).Description;
                    }

                }
                catch
                {
                    return input.ToString();
                }

            }

            return input.ToString();
        }

        public static string GetDescription<T>(this T e, string aPrefixToRemove = null) where T : IConvertible
        {
            string _rVal = string.Empty;
            bool found = false;
            if (e is Enum)
            {

                _rVal = e.ToString();
                Type type = e.GetType();
                Array values = System.Enum.GetValues(type);
                DescriptionAttribute descriptionAttribute = null;
                foreach (int val in values)
                {
                    if (val == e.ToInt32(CultureInfo.InvariantCulture))
                    {
                        var memInfo = type.GetMember(type.GetEnumName(val));
                        try
                        {
                            descriptionAttribute = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false)[0] as DescriptionAttribute;
                        }
                        catch { descriptionAttribute = null; }


                        if (descriptionAttribute != null)
                        {
                            found = true;
                            _rVal = descriptionAttribute.Description;
                            break;
                        }
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(aPrefixToRemove))
            {
                if (_rVal.StartsWith(aPrefixToRemove)) _rVal = _rVal.Substring(aPrefixToRemove.Length);
            }
            if (!found)
            {
                _rVal = mzUtils.UnPascalCase(_rVal);
            }
            return _rVal;
        }

        public static string Description(Enum value, string aPrefix = "")
        {
            string _rVal;
            string description = GetEnumDescription(value);
            try
            {
                _rVal = GetEnumDescription(value);

            }
            catch (System.Exception ex)
            {
                _rVal = "#ERR#";
                aPrefix = string.Empty;
                throw ex;
            }

            return _rVal;


        }

        public static string GetValueNameList(Type aType)
        {
            try
            {
                Array ary = System.Enum.GetValues(aType);
                string[] names = System.Enum.GetNames(aType);
                string rVal = string.Empty;
                int i = 0;
                dynamic oval;
                foreach (object o in ary)
                {
                    oval = o;
                    i++;
                    int ival = mzUtils.VarToInteger(oval);
                    if (i > 1) rVal += ",";

                    rVal += $"{ival}={names[i - 1]}";
                }

                return rVal;
            }
            catch
            {
                return "";
            }

        }
        public static string GetNameList(Type aType, Char aDelim = ',')
        {
            try
            {
                Array ary = System.Enum.GetValues(aType);
                string[] names = System.Enum.GetNames(aType);
                string rVal = string.Empty;
                int i = 0;
                dynamic oval;
                foreach (object o in ary)
                {
                    oval = o;
                    i++;
                    int ival = mzUtils.VarToInteger(oval);
                    if (i > 1) rVal += aDelim;

                    rVal += $"{names[i - 1]}";
                }

                return rVal;
            }
            catch
            {
                return "";
            }

        }

        //Public Shared Function NameList(aType As Type, Optional aDelimitor As Char = ",") As String
        //    Try
        //        Dim ary As Array = System.Enum.GetValues(aType)
        //        Dim nms() As String = System.Enum.GetNames(aType)
        //        Dim rVal As String = ""
        //        Dim i As Integer = 0
        //        For Each o As Object In ary
        //            i += 1
        //            If i > 1 Then rVal += aDelimitor
        //            rVal += $"{ nms(i - 1)}"
        //        Next

        //        Return rVal
        //    Catch ex As Exception
        //        Return ""
        //    End Try
        //End Function
    }

    //@recognized project families for the application
    public enum uppProjectFamilies
    {
        [Description("Undefined")] Undefined = -1,
        [Description("XF")] uopFamXF = 0,
        [Description("MD")] uopFamMD = 1
    }

    public enum uppSubShapeTypes
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Blocked Areas")]
        BlockedAreas = 1,
        [Description("Slot Zone Blocked Areas")]
        SlotBlockedAreas = 2,
        [Description("Free Bubbling Area")]
        FreeBubblingArea = 3,

    }

    public enum uppDisplayTableTypes
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Project Properties")]
        ProjectProperties = 1,
        [Description("Range Properties")]
        RangeProperties = 2,
        [Description("Downcomer Properties")]
        DowncomerProperties = 3,
        [Description("Deck Properties")]
        DeckProperties = 4,
        [Description("Design Options Properties")]
        DesignOptions = 5,
        [Description("Materials")]
        Materials = 6,
        [Description("Startup Spout Properties")]
        StartupSpouts = 7,
        [Description("Deck Panels Properties")]
        DeckPanelsProperties = 8,
        [Description("Downcomers Properties")]
        DowncomersProperties = 9,
        [Description("Spout Groups Properties")]
        SpoutGroupsProperties = 10,
        [Description("Beam Properties")]
        BeamProperties = 11

    }

    public enum uppFlangeTypes
    {
        uop_UndefinedFlange = 0,
        uop_SlipOnFlange = 1,
        uop_WeldNeckFlange = 2,
        uop_PlateFlange = 3
    }

    public enum uppCaseOwnerOwnerTypes  // must match part types!
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Distributor")]
        Distributor = 12102,
        [Description("Chimney Tray")]
        ChimneyTray = 12002
    }

    public enum uppSpecTypes
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Sheet Metal")]
        SheetMetal = 1,
        [Description("Plate")]
        Plate = 2,
        [Description("Pipe")]
        Pipe = 3,
        [Description("Flange")]
        Flange = 4,
        [Description("Fitting")]
        Fitting = 5,
        [Description("Gasket")]
        Gasket = 6,
        [Description("Bolt")]
        Bolt = 100,
        [Description("Nut")]
        Nut = 101,
        [Description("Lock Washer")]
        LockWasher = 102,
        [Description("Flatwasher")]
        FlatWasher = 103,
        [Description("Threaded Stud")]
        ThreadedStud = 104,
        [Description("Set Screw")]
        SetScrew = 106,
        [Description("Tube")]
        Tube = 107
    }

    public enum uppSpliceFastenerTypes
    {
        [Description("Slot & Tab")]
        SlotAndTab = 0,
        [Description("Bolts")]
        Bolts = 1
    }

    public enum uppConstraintApplications
    {
        uopApplyToNone = 0,
        uopApplyToAll = 1,
        uopApplyToGrouped = 2,
        uopApplyToEndGroups = 3,
        uopApplyToFieldGroups = 4,
        uopApplyToDowncomerGroups = 5,
        uopApplyToPanelGroups = 6
    }

    public enum uppSpliceAngleTypes
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Splice Angle")]
        SpliceAngle = 423,
        [Description("Manway Angle")]
        ManwayAngle = 428,
        [Description("Manway Splice Plate")]
        ManwaySplicePlate = 429,
    }
    
    public enum uppPartTypes
    {
        [Description("Undefined")]
        Undefined = 0,
        //hardware parts
        [Description("Hex Bolt")]
        HexBolt = -1,
        [Description("Hex Nut")]
        HexNut = -2,
        [Description("Lockwasher")]
        LockWasher = -3,
        [Description("Stud")]
        Stud = -4,
        [Description("Flat Washer")]
        FlatWasher = -5,
        [Description("Carriage Bolt")]
        CarriageBolt = -6,
        [Description("Shaved Stud")]
        ShavedStud = -7,
        [Description("Set Screw")]
        SetScrew = -8,

        // object parts

        [Description("Document")]
        Document = 10100,
        [Description("Material Spec.")]
        MaterialSpec = 10200,
        [Description("Materials")]
        Materials = 10201,
        [Description("Customer")]
        Customer = 10300,
        [Description("Feed Zone")]
        FeedZone = 10400,
        [Description("Flow Slot Zone")]
        FlowSlotZone = 10500,
        [Description("Flow Slot")]
        FlowSlot = 10600,
        [Description("MD Stage")]
        Stage = 10700,
        [Description("MD Stages")]
        Stages = 10701,
        [Description("Bubble Promoter")]
        BubblePromoter = 10800,
        [Description("Chimney Tray")]
        ChimneyTray = 12000,
        [Description("Chimney Tray Case")]
        ChimneyTrayCase = 12001,
        [Description("Chimney Trays")]
        ChimneyTrays = 12002,
        [Description("Distributor")]
        Distributor = 12100,
        [Description("Distributor Case")]
        DistributorCase = 12101,
        [Description("Distributors")]
        Distributors = 12102,
        [Description("Design Options")]
        DesignOptions = 12200,
        [Description("Bubble Promoter")]
        MDBubblePromoter = 1230,
        [Description("Constraint")]
        Constraint = 12400,
        [Description("Constraints")]
        Constraints = 12401,
        [Description("Spout Group")]
        SpoutGroup = 12500,
        [Description("Spout Groups")]
        SpoutGroups = 12501,
        [Description("Deck Splice")]
        DeckSplice = 12600,
        [Description("Deck Splices")]
        DeckSplices = 12601,
        [Description("Integral Beam")]
        IntegralBeam = 12700,
        [Description("Tray Layout")]
        TrayLayout = 12900,
        [Description("Deck")]
        Deck = 13000,
        [Description("Startup Spout")]
        StartupSpout = 14000,
        [Description("Startup Spouts")]
        StartupSpouts = 14001,
        [Description("Startup Line")]
        StartupLine = 14002,
        [Description("Project")]
        Project = 100,
        [Description("Columns")]
        Columns = 101,
        [Description("Column")]
        Column = 200,
        [Description("Tray Support Ring")]
        Ring = 201,
        [Description("Tray Ranges")]
        TrayRanges = 202,
        [Description("Tray Range")]
        TrayRange = 300,
        [Description("Slot File")]
        SlotFile = 301,
        [Description("Tray Assembly")]
        TrayAssembly = 400,
        [Description("Seal Plate")]
        SealPlate = 401,
        [Description("Hold Down Washer")]
        HoldDownWasher = 402,
        [Description("Manway Clamp")]
        ManwayClamp = 403,
        [Description("Manway Clip")]
        ManwayClip = 404,
        [Description("Manway Washer")]
        ManwayWasher = 405,
        [Description("Ring Clip")]
        RingClip = 406,
        [Description("Hold Down Clip")]
        HoldDownClip = 409,
        [Description("Downcomers")]
        Downcomers = 1409,
        [Description("Downcomer")]
        Downcomer = 410,
        [Description("Shelf Angle")]
        ShelfAngle = 411,
        [Description("End Angle")]
        EndAngle = 412,
        [Description("Downcomer Beam")]
        DowncomerBeam = 413,
        [Description("Downcomer Extension")]
        DowncomerExtension = 414,
        [Description("Downcomer Box")]
        DowncomerBox = 415,
        [Description("End Plate")]
        EndPlate = 416,
        [Description("End Support")]
        EndSupport = 417,
        [Description("Deck Panel")]
        DeckPanel = 418,
        [Description("Deck Section")]
        DeckSection = 419,
        [Description("Baffle")]
        Baffle = 420,
        [Description("Splice Angle")]
        SpliceAngle = 423,
        [Description("Deflector Plate")]
        Deflector = 422,
        [Description("Finger Clip")]
        FingerClip = 425,
        [Description("Stiffener")]
        Stiffener = 426,
        [Description("Cross Brace")]
        CrossBrace = 427,
        [Description("Manway Angle")]
        ManwayAngle = 428,
        [Description("Manway Splice Plate")]
        ManwaySplicePlate = 429,
        [Description("AP Pan")]
        APPan = 430,
        [Description("Deck Beam")]
        DeckBeam = 431,
        [Description("Supplemental Deflector")]
        SupplementalDeflector = 432,
        [Description("Beam Support")]
        BeamSupport = 434,

        [Description("Support Clip")]
        SupportClip = 436,
        [Description("Weir Angle")]
        WeirAngle = 437,
        [Description("Bubble Promoter")]
        XFBubblePromoter = 438,
        [Description("Receiving Pan")]
        ReceivingPan = 480,
        [Description("Seal Tab")]
        SealTab = 481,
        [Description("Spacer Tube")]
        SpacerTube = 482,
        [Description("Spacer Plate")]
        SpacerPlate = 483,
        [Description("Ring Spec.")]
        RingSpec = 484,
        [Description("Deck Panels")]
        DeckPanels = 485,
        [Description("Deck Sections")]
        DeckSections = 486,
        [Description("Ring Specs.")]
        RingSpecs = 487,
        [Description("Parts")]
        Parts = 488,
        [Description("Spout Areas")]
        SpoutAreas = 499,
        [Description("Tray Support Beam")]
        TraySupportBeam = 500,
        [Description("Divider Wall")]
        DividerWall = 600,

    }
    public enum uppTrayGenerationEventTypes
    {
        Downcomers = 1,
        DeckPanels = 2,
        DeckSections = 3,
        SlotZones = 4,
        FlowSlots = 5,
        SpoutGroups = 6,
        SpoutGroup = 7,
        StartupSpouts = 8,
        DowncomerSpacing = 9,
        Splices = 10,
        UniqueDeckSections = 11
    }
    public enum uppPartEventTypes
    {
        SheetMetalChange1 = 1,
        SheetMetalChange2 = 2,
        PartInvalidated = 4,
        DisplayUnitChange = 5,
        DrawingUnitChange = 6,
        PropertyChange = 7,
        HoleUpdateRequest = 10,
        PartTypeChange = 11,
        RangeRequest = 20,

        UpdateBaseProperties = -1,
        UpdateBaseWeight = -2
    }
    public enum uppBorderSizes
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("B Size")]
        BSize_Landscape = 1,
        [Description("D Size")]
        DSize_Landscape = 2
    }

    public enum uppDrawingFamily
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Functional")]
        Functional = 1,
        [Description("Attachment")]
        Attachment = 2,
        [Description("General Assembly")]
        GA = 3,
        [Description("Installation")]
        Installation = 4,
        [Description("Manufacturing")]
        Manufacturing = 5,
        [Description("Design")]
        Design = 6,
        [Description("Sketch")]
        Sketch = 7,
        [Description("Tray View")]
        TrayView = 8,

    }
    //@the various type of distributor pipe configurations
    public enum uppPipeConfigurations
    {
        I = 0,
        T = 1,
        H = 2
    }
    //@the various type of documents
    public enum uppDocumentTypes
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Warning")]
        Warning = 1,
        [Description("Calculation")]
        Calculation = 2,
        [Description("Report")]
        Report = 3,
        [Description("Drawing")]
        Drawing = 4,
        [Description("Report Page")]
        ReportPage = 5
    }

    //@the various type of panel splices
    public enum uppSpliceTypes
    {
        Undefined = -1,
        SpliceWithTabs = 0,
        SpliceWithAngle = 1,
        SpliceWithJoggle = 2,
        SpliceManwayCenter = 3
    }
    //@the various styles for splices
    public enum uppSpliceStyles
    {
        [Description("Slot & Tab")]
        Tabs = 0,
        [Description("Angles")]
        Angle = 1,
        [Description("Joggles")]
        Joggle = 2,
        [Description("Undefined")]
        Undefined = -1
    }
    public enum uppSpliceIndicators
    {
        Undefined = -2,
        ToRing = 0,
        ToDowncomer = -1,
        TabMale = 1,
        TabFemale = 2,
        JoggleMale = 11,
        JoggleFemale = 12,
        AngleMale = 21,
        AngleFemale = 22,
        ManwayAngleMale = 31,
        ManwayAngleFemale = 32
    }
    //@defined uop sheet metal gages
    public enum uppSheetGages
    {
        [Description("Unknown")]
        GageUnknown = -1,
        [Description("10 ga.")]
        Gage10 = 0,
        [Description("12 ga.")]
        Gage12 = 1,
        [Description("14 ga.")]
        Gage14 = 2,
        [Description("2 mm")]
        Gage2mm = 3,
        [Description("2.5 mm")]
        Gage2pt5mm = 4,
        [Description("2.7 mm")]
        Gage2pt7mm = 5,
        [Description("3 mm")]
        Gage3mm = 6,
        [Description("3.5 mm")]
        Gage3pt5mm = 7,
        [Description("4 mm")]
        Gage4mm = 8
    }

    public enum uppDefinitionLineTypes
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Limit Line")]
        LimitLine = 1,
        [Description("Box Line")]
        BoxLine = 2,
        [Description("Box Weir Line")]
        BoxWeirLine = 3,
                  [Description("Panel Weir Line")]
        PanelWeirLine = 4
    }

    public enum uppArcRecTypes
    {
        [Description("Undefined")]
        Undefined = -1,
        [Description("Arc")]
            Arc = 1,
        [Description("Rectangle")]
        Rectangle = 2,
        [Description("Slot")]
        Slot = 3

    }

    //@determines if a part exists on all rings or on alternate rings
    public enum uppAlternateRingTypes
    {
        [Description("All Rings")]
        AllRings = 0,
        [Description("Alternate Ring 1")]
        AtlernateRing1 = 1,
        [Description("Alternate Ring 2")]
        AtlernateRing2 = 2
    }


    //@defined uop metal material families
    public enum uppMetalFamilies
    {
        [Description("Unknown")]
        Unknown = -1,
        [Description("CS")]
        CarbonSteel = 0,
        [Description("316 SS")]
        Stainless_316 = 1,
        [Description("304 SS")]
        Stainless_304 = 2,
        [Description("410 SS")]
        Stainless_410 = 3,
        [Description("KILLED CS")]
        CarbonSteel_Killed = 4,
        [Description("18-8 SS")]
        Stainless_188 = 5,
        [Description("304L SS")]
        Stainless_304L = 6,
        [Description("316L SS")]
        Stainless_316L = 7
    }
    //@defined report types
    public enum uppReportTypes
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Spouting Data Report")]
        MDSpoutReport = 1,
        [Description("DC Spacing Optimization Report")]
        MDSpacingReport = 2,
        [Description("Feed Zone Report")]
        MDFeedZoneReport = 3,
        [Description("Hardware Report")]
        HardwareReport = 4,
        [Description("EDM Report")]
        MDEDMReport = 5,
        [Description("Downcomer Stress Report")]
        MDDCStressReport = 6,
        [Description("--")]
        ReportPlaceHolder = 7,
        [Description("Angle Stress Report")]
        AngleStressReport = 8,
        [Description("Distributor Report")]
        MDDistributorReport = 9
    }
    //@defined page types
    public enum uppReportPageTypes
    {
        PageUndefined = -999,
        [Description("Test Page")]
        TestPage = -1,
        [Description("Project Summary")]
        ProjectSummaryPage = 1,
        [Description("Column Sketch")]
        ColumnSketchPage = 2,
        [Description("Tray Design Summary")]
        TraySummaryPage = 3,
        [Description("Distributors")]
        MDDistributorPage = 4,
        [Description("Chimney Trays")]
        MDChimneyTrayPage = 5,
        [Description("Tray Sketch")]
        TraySketchPage = 6,
        [Description("DC Spacing Optimization")]
        MDOptimizationPage = 7,
        [Description("Spouting Details")]
        MDSpoutDetailPage = 8,
        [Description("Spout Group Sketches")]
        MDSpoutSketchPage = 9,
        [Description("Warnings")]
        WarningPage = 10,
        [Description("Spout Group Constraints")]
        MDSpoutConstraintPage = 11,
        [Description("Feed Zones")]
        MDFeedZonePage = 12,
        [Description("Tray Hardware Totals")]
        HardwareTotals = 13,
        [Description("Tray General And Process Information")]
        MDEDMPage1 = 14,
        [Description("Tray Mechanical Summary")]
        MDEDMPage2 = 15,
        [Description("Tray Hydraulic Parameters")]
        MDEDMPage3 = 16,
        [Description("Tray Hardware")]
        TrayHardwarePage = 17,
        [Description("DC Spacing Optimization")]
        DCSpacingOptimizationPage = 18,
        [Description("Downcomer Stress Calculations")]
        DCStressPage = 19
    }
    //@defined warning types
    public enum uppWarningTypes
    {
        General = 0,
        ReportFatal = 1,
        DrawingFatal = 2,
        PlaceHolder = 3,
        Exception = 4

    }
    //@MD startup spout configurations
    public enum uppStartupSpoutConfigurations
    {
        [Description("Undefined")]
        Undefined = -1,
        [Description("None")]
        None = 0,
        [Description("2 x 2")]
        TwoByTwo = 1,
        [Description("2 x 4")]
        TwoByFour = 2,
        [Description("4 x 4")]
        FourByFour = 3,
        [Description("By Default")]
        ByDefault = 5
    }
    //@represents an objects material type when it exends uopMaterial
    public enum uppMaterialTypes
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Sheet Metal")]
        SheetMetal = 1,
        [Description("Hardware")]
        Hardware = 2,
        [Description("Tubing")]
        Tubing = 3,
        [Description("Gasket")]
        Gasket = 4,
        [Description("Plate")]
        Plate = 5,
        [Description("Pipe")]
        Pipe = 6,
        [Description("Fitting")]
        Fitting = 7,
        [Description("Flange")]
        Flange = 8
    }
    //@MD downcomer spacing methods
    public enum uppMDSpacingMethods
    {
        [Description("Standard")]
        NonWeighted = 1,
        [Description("Weighted")]
        Weighted = 0
    }
    //@the types of available UOP reports
    public enum uppCalculationType
    {
        DowncomerProperties = 1,
        MDSpacing = 2,
        MDSpoutArea = 3,
        MDSpoutLayout = 4,
        Constraints = 5,
        Warnings = 6,
        FeedZones = 7,
        PlaceHolder = 8,
        Weights = 9
    }
    //@the available drawings  'negative types are invisble to users of the compiled program
    public enum uppDrawingTypes
    {
        
        [Description("Spout Areas")]
        SpoutAreas = -10,
        [Description("Definition Lines")]
        DefinitionLines = -9,
        [Description("Stiffener Edit")]
        StiffenerEdit = -8,
        [Description("Section Shapes")]
        SectionShapes = -7,
        [Description("Spout Group Input Sketch")]
        SGInputSketch = -6,
        [Description("Input Sketch")]
        InputSketch = -5,
        [Description("Ring Clip Segments")]
        RingClipSegments = -3,
        [Description("Startup Lines")]
        StartUpLines = -2,
        [Description("Test Drawing")]
        TestDrawing = -1,
        [Description("Undefined")]
        Undefined = 0,
        [Description("Attachments")]
        Attachment = 1,
        [Description("Layout")]
        Layout = 2,
        [Description("Downcomer Beam")]
        DowncomerBeam = 3,
        [Description("Downcomer Beam Assembly")]
        DowncomerBeamASSY = 4,
        [Description("Downcomer Extention")]
        DowncomerExtension = 5,
        [Description("Beam Support Clip")]
        DCBeamSupportClip = 6,
        [Description("I-Beam Support Clip")]
        IBeamSupportClip = 9,
        [Description("Receiving Pan")]
        ReceivingPan = 10,
        [Description("Receiving Pan Assembly")]
        ReceivingPanASSY = 11,
        [Description("Bubble Promoter")]
        BubblePromoter = 12,
        [Description("Bubble Promoter Assy")]
        BubblePromoterASSY = 13,
        [Description("Receiving Pan Section")]
        ReceivingPanSection = 14,

        [Description("Deck Beam")]
        DeckBeam = 15,
        [Description("Manway Angle")]
        ManwayAngle = 16,
        [Description("Cross Brace")]
        CrossBrace = 17,
        [Description("Weir Angle")]
        WeirAngle = 18,
        [Description("Deck Panel")]
        DeckPanel = 19,
        [Description("Deck Panel")]
        SealTab = 20,
        [Description("Seal Tab")]
        SealPlate = 21,
        [Description("Seal Plate")]
        RingClip = 22,
        [Description("Ring Clip (3 Inch)")]
        Installation = 23,
        [Description("Installation")]
        SpliceAngle = 24,
        [Description("Hold Down Clip")]
        HoldDownClip = 25,
        [Description("Slot Layout")]
        SlotLayout = 26,
        [Description("Deck Panels")]
        PanelsOnly = 27,
        [Description("Spacer Tube")]
        SpacerTube = 28,
        [Description("Tray Sketch")]
        TraySketch = 29,
        [Description("Hold Down Washer")]
        HoldDownWasher = 30,
        [Description("Attachment Details")]
        AttachmentDetailSheet = 31,
        [Description("Splice Details")]
        SpliceDetailSheet = 32,
        [Description("Sheet Index")]
        SheetIndex = 33,
        [Description("Sheet 2")]
        Sheet2 = 34,
        [Description("Sheet 3")]
        Sheet3 = 35,
        [Description("Hardware List")]
        HardwareBreakdown = 36,
        [Description("Manway Clamp")]
        ManClamp = 37,
        [Description("Receiving Pan Clip")]
        ReceivingPanClip = 38,
        [Description("Slot Layout")]
        DeckPanelSlotLayout = 39,

        [Description("Spacer Plate")]
        SpacerPlate = 40,
        [Description("Stack Detail")]
        StackDetail = 41,
        [Description("Plan View")]
        LayoutPlan = 42,
        [Description("Elevation View")]
        LayoutElevation = 43,
        [Description("Sectional Elevation View")]
        LayoutSectionalElevation = 44,
        [Description("Feed Zones")]
        FeedZones = 45,
        [Description("Spout Group Sketch")]
        SpoutGroupSketch = 46,
        [Description("Section Sketch")]
        SectionSketch = 47,
        [Description("Panels and Slots")]
        PanelsAndSlots = 48,
        [Description("Support Detail")]
        SupportDetails = 49,
        [Description("Receiving Pan Section Assembly")]
        ReceivingPanSectionASSY = 50,
        [Description("Blocked Areas")]
        BlockedAreas = 51,
        [Description("End Support")]
        EndSupport = 52,
        [Description("End Plate")]
        EndPlate = 53,
        [Description("End Angle")]
        EndAngle = 54,
        [Description("Stiffener")]
        Stiffener = 55,
        [Description("Deflector Plate")]
        DeflectorPlate = 56,
        [Description("Downcomer Box")]
        DowncomerBox = 57,
        [Description("Functional")]
        Functional = 58,
        [Description("Finger Clip")]
        FingerClip = 59,
        [Description("Support Clip")]
        SupportClip = 60,
        [Description("")]
        PlaceHolder = 61,
        [Description("Ring Clip (4 inch)")]
        RingClip4in = 62,
        [Description("Downcomers")]
        DowncomersOnly = 63,
        [Description("Manway Clip")]
        ManwayClip = 64,
        [Description("Equal Deck Sections")]
        EqualSections = 65,
        [Description("Manway Splice Plate")]
        ManwaySplicePlate = 66,
        [Description("Splice Angles")]
        SpliceAngles = 67,
        [Description("Manway Angles")]
        ManwayAngles = 68,
        [Description("Manway Splice Plates")]
        ManwaySplicePlates = 69,
        [Description("AP Pan")]
        APPan = 70,
        [Description("DC Ring Clip (4 inch)")]
        RingClipDC4in = 71,
        [Description("Downcomer Design")]
        DowncomerDesign = 72,
        [Description("Sheet 4")]
        Sheet4 = 73,
        [Description("Sheet 5")]
        Sheet5 = 74,
        [Description("Assembly Details")]
        AssemblyDetails = 75,
        [Description("Support Detail")]
        SupportDetail = 76,
        [Description("Sheet 6")]
        Sheet6 = 78,
        [Description("Downcomer Assembly")]
        DCAssembly = 79,
        [Description("Baffle Layout")]
        Baffles = 80,

        [Description("Supplemental Deflector")]
        SupplDeflector = 81,
        [Description("Sheet 7")]
        Sheet7 = 82,
        [Description("DC Manhole Fit")]
        DowncomerManholeFit = 83,
        [Description("Ring Clip (3 inch)")]
        RingClip3in = 84,
        [Description("Supplemental Deflectors")]
        SupplDeflectors = 85,
        [Description("End Angles")]
        EndAngles = 86,
        [Description("Free Bubbling Area")]
        FreeBubbleAreas = 87,
        [Description("Beam Details")]
        BeamDetails = 88,
        [Description("Installation")]
        BeamAttachments = 89,
        [Description("Functional Active Areas")]
        FunctionalActiveAreas = 90,
    }
    //@graph types that match excel ChartType enums
    public enum uppGraphTypes
    {
        ThreeDArea = -4098,
        ThreeDAreaStacked = 78,
        ThreeDAreaStacked100 = 79,
        ThreeDBarClustered = 60,
        ThreeDBarStacked = 61,
        ThreeDBarStacked100 = 62,
        ThreeDColumn = -4100,
        ThreeDColumnClustered = 54,
        ThreeDColumnStacked = 55,
        ThreeDColumnStacked100 = 56,
        ThreeDLine = -4101,
        ThreeDPie = -4102,
        ThreeDPieExploded = 70,
        Area = 1,
        AreaStacked = 76,
        AreaStacked100 = 77,
        BarClustered = 57,
        BarOfPie = 71,
        BarStacked = 58,
        BarStacked100 = 59,
        Bubble = 15,
        Bubble3DEffect = 87,
        ColumnClustered = 51,
        ColumnStacked = 52,
        ColumnStacked100 = 53,
        ConeBarClustered = 102,
        ConeBarStacked = 103,
        ConeBarStacked100 = 104,
        ConeCol = 105,
        ConeColClustered = 99,
        ConeColStacked = 100,
        ConeColStacked100 = 101,
        CylinderBarClustered = 95,
        CylinderBarStacked = 96,
        CylinderBarStacked100 = 97,
        CylinderCol = 98,
        CylinderColClustered = 92,
        CylinderColStacked = 93,
        CylinderColStacked100 = 94,
        Doughnut = -4120,
        DoughnutExploded = 80,
        Line = 4,
        LineMarkers = 65,
        LineMarkersStacked = 66,
        LineMarkersStacked100 = 67,
        LineStacked = 63,
        LineStacked100 = 64,
        Pie = 5,
        PieExploded = 69,
        PieOfPie = 68,
        PyramidBarClustered = 109,
        PyramidBarStacked = 110,
        PyramidBarStacked100 = 111,
        PyramidCol = 112,
        PyramidColClustered = 106,
        PyramidColStacked = 107,
        PyramidColStacked100 = 108,
        Radar = -4151,
        RadarFilled = 82,
        RadarMarkers = 81,
        StockHLC = 88,
        StockOHLC = 89,
        StockVHLC = 90,
        StockVOHLC = 91,
        Surface = 83,
        SurfaceTopView = 85,
        SurfaceTopViewWireframe = 86,
        SurfaceWireframe = 84,
        XYScatter = -4169,
        XYScatterLines,
        catterLines = 74,
        XYScatterLinesNoMarkers = 75,
        XYScatterSmooth = 72,
        XYScatterSmoothNoMarkers = 73
    }

    //@standard spout pattern types
    public enum uppSpoutPatterns
    {
        [Description("By Default")]
        Undefined = 0,

        [Description("D")]
        D = 1,

        [Description("C")]
        C = 2,

        [Description("B")]
        B = 3,

        [Description("A")]
        A = 4,

        [Description("*A*")]
        Astar = 5,

        [Description("S3")]
        S3 = 6,

        [Description("S2")]
        S2 = 7,

        [Description("S1")]
        S1 = 8,

        [Description("*S*")]
        SStar = 9
    }

    //@used to align gruds
    public enum uppGridAlignments
    {
        Undefined = 0,
        TopLeft = 1,
        TopCenter = 2,
        TopRight = 3,
        MiddleLeft = 4,
        MiddleCenter = 5,
        MiddleRight = 6,
        BottomLeft = 7,
        BottomCenter = 8,
        BottomRight = 9
    }
    public enum uppHorizontalAlignments
    {
        Undefined = 0,
        Left = 1,
        Center = 2,
        Right = 3,

    }
    public enum uppVerticalAlignments
    {
        Undefined = 0,
        Top = 1,
        Center = 2,
        Bottom = 3,

    }

    //@represents MD tray design families
    public enum uppMDDesigns
    {
        [Description("Undefined")]
        Undefined = -1,
        [Description("MD")]
        MDDesign = 0,
        [Description("ECMD")]
        ECMDDesign = 1,
        [Description("MD Beam")]
        MDBeamDesign = 100,
        [Description("ECMD Beam")]
        ECMDBeamDesign = 101,
        [Description("MD Divided Wall")]
        MDDividedWall = 200,
    }
    //@represents beam types
    public enum uppBeamTypes
    {
        Undefined = 0,
        Inlet = 1,
        Deck = 2,
        Downcomer = 3,
        IntegralPan = 4,
        IntegralDeck = 5,
        TraySupport = 6
    }

    public enum uppMDRoundToLimits
    {
        Sixteenth = 0,
        Millimeter = 1,
        None = 2,
    }


    //@represents the punch direction for openings in sheet metal parts
    public enum uppPunchDirections
    {
        [Description("Either")]
        Either = 0,
        [Description("From Above")]
        FromAbove = 1,
        [Description("From Below")]
        FromBelow = 2
    }
    //@represents the configuration of a tray assembly
    public enum uppTrayConfigurations
    {
        [Description("S2S")]
        SideToSide = 0,
        [Description("C2S")]
        CenterToSide = 1,
        [Description("S2C")]
        SideToCenter = 2,
        [Description("MD")]
        MultipleDowncomer = 3
    }
    //@represents the stack pattern in a tray range
    public enum uppStackPatterns
    {
        [Description("Undefined")]
        Undefined = -1,
        [Description("Continuous")]
        Continuous = 0,
        [Description("Odd")]
        Odd = 1,
        [Description("Even")]
        Even = 2
    }
    //@represents the sort order for tray ranges in a column
    public enum uppTraySortOrders
    {
        [Description("Top To Bottom")]
        TopToBottom = 0,
        [Description("Bottom To Top")]
        BottomToTop = 1,

    }

    //@flow slot zone types
    public enum uppFlowSlotTypes
    {
        [Description("Full C")]
        FullC = 1,

        [Description("Half C")]
        HalfC = 2,


    }

    //@Downcomer flow devices
    public enum uppFlowDevices
    {
        [Description("None")]
        None = 0,


        [Description("AP Pans")]
        APPans = 1,

        [Description("FED")]
        FED = 2,


    }

    //@defined views
    public enum uppViews
    {
        Plan = 0,
        Side = 1,
        AttachPlan = 2,
        AttachElevation = 3,
        LayoutPlan = 5,
        LayoutElevation = 6,
        LayoutSectionalElevation = 7,
        Top = 8,
        End = 9,
        Bottom = 10,
        InstallationPlan = 11,
        InstallationElevation = 12,
        InstallationSectionalElevation = 13,
        SlotZoneLayout = 14,
        PanelsView = 15,
        ZoneView = 16,
        FunctionalDesign = 17,
        DesignView = 18,
        InputSketch = 19
    }
    public enum uppPartViews
    {
        Top = 8,
        Bottom = 10,
        Left = 2,
        Right = 3
    }
    //@tube material size constants
    public enum uppTubeSizes
    {
        OneEighth = 0,
        OneQuarter = 1,
        OneThreeEighths = 2,
        TubeOneHalf = 3,
        ThreeQuarters = 4,
        One = 5
    }
    //@tube schedue constants
    public enum uppPipeSchedules
    {
        [Description("Sch. 5S")]
        uop5_S = 0,
        [Description("Sch. 10S")]
        uop10_S = 1,
        [Description("Sch. 10")]
        uop10 = 2,
        [Description("Sch. 20")]
        uop20 = 3,
        [Description("Sch. 30")]
        uop30 = 4,
        [Description("Sch. STD")]
        uopStandard = 5,
        [Description("Sch. 40")]
        uop40 = 6,
        [Description("Sch. XStrong")]
        uopXStrong = 7,
        [Description("Sch. 80")]
        uop80 = 8,
        [Description("Sch. 100")]
        uop100 = 9,
        [Description("Sch. 120")]
        uop120 = 10,
        [Description("Sch. 140")]
        uop140 = 11,
        [Description("Sch. 160")]
        uop160 = 12,
        [Description("Sch. XXStrong")]
        uopXXStrong = 13
    }

    //@deck or integral beam end style
    public enum uppDeckBeamEndStyle
    {
        ToBeam = 0,
        ToRingSupport = 1,
        ToReceivingPan = 2,
        FreeFloating = 3
    }
    //@available units
    public enum uppUnitFamilies
    {
        [Description("Undefined")]
        Undefined = -2,
        [Description("Default")]
        Default = -1,
        [Description("English")]
        English = 0,
        [Description("Metric")]
        Metric = 1,
        [Description("SI")]
        SI = 2
    }

    public enum uppUnitTypes
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Small Length")]
        SmallLength = 1,
        [Description("Big Length")]
        BigLength = 2,
        [Description("Small Area")]
        SmallArea = 3,
        [Description("Big Area")]
        BigArea = 4,
        [Description("Percentage")]
        Percentage = 5,
        [Description("Big Percentage")]
        BigPercentage = 6,
        [Description("Volume Percentage")]
        VolumePercentage = 7,
        [Description("Mass Rate")]
        MassRate = 8,
        [Description("Big Mass Rate")]
        BigMassRate = 9,
        [Description("Density")]
        Density = 10,
        [Description("Volumn Rate")]
        VolumeRate = 11,
        [Description("Viscosity")]
        Viscosity = 12,
        [Description("Surface Tension")]
        SurfaceTension = 13,
        [Description("Seconds")]
        Seconds = 14,
        [Description("ρV²")]
        RhoVSqr = 15,
        [Description("Velocity")]
        Velocity = 16,
        [Description("Weight")]
        Weight = 17,
        [Description("Temperature")]
        Temperature = 18,
        [Description("Pressure")]
        Pressure = 19

    }
    //@recognized project InstallationTypes
    public enum uppInstallationTypes
    {
        [Description("Grassroots")]
        GrassRoots = 0,
        [Description("Revamp")]
        Revamp = 1
    }

    public enum uppSegmentTypes
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Arc")]
        Arc = 1,
        [Description("Line")]
        Line = 2
    }

    public enum uppSegmentPoints
    {
        [Description("Start Pt")]
        StartPt = 0,
        [Description("End Pt")]
        EndPt = 1,
        [Description("Mid Pt")]
        MidPt = 2
    }

    public enum uppIntersectionTypes
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("To Ring")]
        ToRing = 1,
        [Description("To Divider")]
        ToDivider = 2,
        [Description("Straddles Ring To Beam")]
        StraddlesRingToDivider = 3,

    }

    public enum uppRevampStrategies
    {

        [Description("1-for-1")]
        OneForOne = 0,
        [Description("2-for-1")]
        TwoForOne = 1,
        [Description("4-for-3")]
        FourForThree = 2,
        [Description("5-for-4")]
        FiveForFour = 3
    }

    public enum uppHoleTypes
    {
        Any = 0,
        Hole = 20,
        Slot = 21
    }

    //@recognized project types for the application
    public enum uppProjectTypes
    {
        [Description("Undefined")]
        Undefined = -1,
        [Description("Cross Flow")]
        CrossFlow = 0,
        [Description("MD Spout")]
        MDSpout = 1,
        [Description("MD Draw")]
        MDDraw = 2
    }
    //@hardware size constants
    public enum uppHardwareTypes

    {
        [Description("Undefined")]
        Undefined = 0,
        //hardware parts
        [Description("Hex Bolt")]
        HexBolt = -1,
        [Description("Hex Nut")]
        HexNut = -2,
        [Description("Lockwasher")]
        LockWasher = -3,
        [Description("Stud")]
        Stud = -4,
        [Description("Flat Washer")]
        FlatWasher = -5,
        [Description("Carriage Bolt")]
        CarriageBolt = -6,
        [Description("Shaved Stud")]
        ShavedStud = -7,
        [Description("Set Screw")]
        SetScrew = -8,
        [Description("Hold Down Washer")]
        LargeODWasher = -9,
    }


    //@hardware size constants
    public enum uppHardwareSizes
    {
        ThreeEights = 1,
        OneHalf = 3,
        M10 = 100,
        M12 = 101
    }
    //@washer series constants
    public enum uppWasherSeries
    {
        Narrow = 0,
        Regular = 1,
        Wide = 2
    }
    //@washer type constants
    public enum uppWasherTypes
    {
        [Description("A")]
        TypeA = 0,
        [Description("B")]
        TypeB = 1
    }
    //@ring clip sizes
    public enum uppRingClipSizes
    {
        [Description("3'' Ring Clip")]
        ThreeInchRC = 0,
        [Description("4'' Ring Clip")]
        FourInchRC = 1,
        [Description("4'' Downcomer Ring Clip")]
        FourInchRC_DC = 2,
        Unknown = -2
    }
    public enum uppSides
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Left")]
        Left = 1,
        [Description("Right")]
        Right = 2,
        [Description("Bottom")]
        Bottom = 4,
        [Description("Top")]
        Top = 8
    }

    public enum uppElevationTypes
    {
        [Description("Undefined")]
        Undefined = -1,
        [Description("Above Tangent")]
        AboveTangent = 0,
        [Description("Above Ring")]
        AboveRing = 1,
        [Description("Below Ring")]
        BelowRing = 2,
        [Description("Below Tangent")]
        BelowTangent = 3,
    }

    public enum uppTrayDividerTypes
    {
        Undefined = 0,
        Beam = 1,
        Wall = 2
    }

    public enum uppSpoutAreaMatrixDataTypes
    {
        [Description("Undefined")]
        Undefined = 0,
        [Description("Available Areas")]
        AvailableArea = 1,
        [Description("Area Locks")]
        LockValue = 2,
        [Description("Group Indices")]
        GroupIndex = 3,
               [Description("Ideal Spout Areas")]
        IdealSpoutArea = 4,
                    [Description("DC Ideals")]
        DowncomerIdeals = 5,
                    [Description("DP Ideals")]
        DeckPanelIdeals = 6
    }
}
