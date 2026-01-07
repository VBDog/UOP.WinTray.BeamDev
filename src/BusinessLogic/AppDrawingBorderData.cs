namespace UOP.WinTray.UI.BusinessLogic
{
    public class AppDrawingBorderData
    {
        public string PartName_Value { get; set; }
        public string PartNumber_Value { get; set; }
        public string ForParts_Value { get; set; }
        public string NumberRequired_Value { get; set; }
        public string ForMaterial_Value { get; set; } = "FOR MATERIAL SEE SHEET 2"; 
        public string Location_Value { get; set; }

        public string PartName_Tag { get; set; } = "PART_NAME";
        public string PartNumber_Tag { get; set; } = "PART_NUMBER";
        public string ForParts_Tag { get; set; } = "FOR_PARTS";
        public string NumberRequired_Tag { get; set; } = "NUMBER_REQ";
        public string ForMaterial_Tag { get; set; } = "FOR_MATERIAL";
        public string Location_Tag { get; set; } = "LOCATION";

        public string PartName_Prompt { get; set; } = "Part Name";
        public string PartNumber_Prompt { get; set; } = "Part Number";
        public string ForParts_Prompt { get; set; } = "For Parts";

        public string NumberRequired_Prompt { get; set; } = "Number Required";
        public string ForMaterial_Prompt { get; set; } = "For Material";
        public string Location_Prompt { get; set; } = "Location";
    }
}

