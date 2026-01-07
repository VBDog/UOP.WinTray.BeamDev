using UOP.DXFGraphics;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Enums;

namespace UOP.WinTray.UI.BusinessLogic
{
    public class appDrawSettings
    {

        public appDrawSettings()
        {
            InitializeProperties();
        }

        



        private uopProperties _Properties;
        public uopProperties Properties => _Properties;
        public int LinearPrecision(uppUnitFamilies uppUnitFamilies)
        {
            return (uppUnitFamilies == uppUnitFamilies.English) ? EnglishPrecision :  MetricPrecision;
            
        }

        public int EnglishPrecision
        {
            get { return _Properties.ValueI("EnglishPrecision"); }
            set { _Properties.SetProperty("iEnglishPrecision", value); }
        }

        public int MetricPrecision
        {
            get { return _Properties.ValueI("MetricPrecision"); }
            set { _Properties.SetProperty("MetricPrecision", value); }
        }

        public int AngularPrecision
        {
            get { return _Properties.ValueI("AngularPrecision"); }
            set { _Properties.SetProperty("AngularPrecision", value); }
        }


        public bool SuppressLeadingZeros
        {
            get { return _Properties.ValueB("SuppressLeadingZeros"); }
            set { _Properties.SetProperty("SuppressLeadingZeros", value); }
        }

        public bool SuppressTrailingZeros
        {
            get { return _Properties.ValueB("SuppressTrailingZeros"); }
            set { _Properties.SetProperty("SuppressTrailingZeros", value); }
        }

        public dxxColors CenterLineColor
        {
            get { return (dxxColors)_Properties.ValueI("CenterLineColor"); }
            set { _Properties.SetProperty("CenterLineColor", value); }
        }

        public dxxColors HiddenLineColor
        {
            get { return (dxxColors)_Properties.ValueI("HiddenLineColor"); }
            set { _Properties.SetProperty("HiddenLineColor", value); }
        }

        public dxxColors DottedLineColor
        {
            get { return (dxxColors)_Properties.ValueI("DottedLineColor"); }
            set { _Properties.SetProperty("DottedLineColor", value); }
        }

        public dxxColors DimensionLineColor
        {
            get { return (dxxColors)_Properties.ValueI("DimensionLineColor"); }
            set { _Properties.SetProperty("DimensionLineColor", value); }
        }

        public dxxColors TextColor
        {
            get { return (dxxColors)_Properties.ValueI("TextColor"); }
            set { _Properties.SetProperty("TextColor", value); }
        }

        public dxxColors DrawingBackColor
        {
            get { return (dxxColors)_Properties.ValueI("DrawingBackColor"); }
            set { _Properties.SetProperty("DrawingBackColor", value); }
        }



        #region Functions
        private void InitializeProperties()
        {
            //On Error Resume Next
            _Properties = new uopProperties
            {
                { "DimensionLineColor", dxxColors.Yellow },
                { "CenterLineColor", dxxColors.Red },
                { "SolidLineColor", dxxColors.ByLayer },
                { "DottedLineColor", dxxColors.Green },
                { "TextColor", dxxColors.Cyan },
                { "ArrowColor", dxxColors.Yellow },
                { "HiddenLineColor", dxxColors.Green },
                { "TextWidthFactor", 1 },
                { "EnglishPrecision", 3 },
                { "MetricPrecision", 1 },
                { "AngularPrecision", 1 },
                { "FontName", "RomanS.shx" },
                { "SuppressLeadingZeros", true },
                { "SuppressTrailingZeros", true },
                { "DrawingBackColor", dxxColors.Undefined }
            };
        }
        #endregion
    }
}
