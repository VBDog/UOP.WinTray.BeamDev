using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace UOP.WinTray.Projects.Utilities
{
    public enum mzBorderStyles
    {
        None = -4142,
        Double = -4119,
        Dot = -4118,
        Dash = -4115,
        Undefined = 0,
        Continous = 1,
        DashDot = 4,
        DashDotDot = 5,
        SlantDashDot = 13
    }

    public enum mzBorderWeights
    {
        Medium = -4138,
        Undefined = 0,
        Hairline = 1,
        Thin = 2,
        Thick = 4
    }

    public enum mzValueTypes
    {
        Undefined = 0,
        Boolean = 1,
        String = 2,
        Single = 3,
        Double = 4,
        Integer = 5,
        Long = 6,
    }

    public enum uopValueControls
    {
        None = 0,
        Positive = 1,
        Negative = 2,
        NonZero = 4,
        PositiveNonZero = Positive | NonZero,
        NegativeNonZero = Negative | NonZero,
    }

    public enum uopQueryTypes
    {
        [Description("Undefined")]
        Undefined = -1,
        [Description("Yes or No")]
        YesNo = 0,
        [Description("Single Select")]
        SingleSelect = 1,
        [Description("Multi-Select")]
        MultiSelect = 2,
        [Description("String Input")]
        StringValue = 3,
        [Description("Numeric Input")]
        NumericValue = 4,
        [Description("String Choice")]
        StringChoice = 5,
        [Description("Check Value")]
        CheckVal = 6,
        [Description("Numeric List")]
        NumericList = 7,
        [Description("Select Folder")]
        Folder = 8,
        [Description("Dual String Choice")]
        DualStringChoice = 9
    }

    public enum mzTextFormats
    {
        [Description("None")]
        None  = 0,
        [Description("Bold")]
        Bold = 1,
        [Description("Italic")]
        Italic = 2,
        [Description("Underline")]
        Underlined = 4
    }

    public enum mzSides
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
        Top = 8,
        [Description("Outer")]
        Outer = 16,
        [Description("Inner")]
        Inner = 32,
        [Description("None")]
        None = 64
    }

    public enum uopAlignments
    {
        Undefined = -1 * 2,
        General = -1 * 1,
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

    public enum mzRegistryRoots
    {
        HKEY_LOCAL_MACHINE = -1 * 2147483646, //&H80000002
        HKEY_CURRENT_USER = -1 * 2147483647, //&H80000001
        HKEY_CLASSES_ROOT = -2147483648, //&H80000000
        HKEY_CURRENT_CONFIG = -1 * 2147483643, //&H80000005
        HKEY_DYN_DATA = -1 * 2147483642, //&H80000006
        HKEY_PERFORMANCE_DATA = -1 * 2147483644, //&H80000004
        HKEY_USERS = -1 * 2147483645 //&H80000003
    }

    public enum mzSortOrders
    {
        None = 0,
        LowToHigh = 1,
        HighToLow = 2
    }
}
