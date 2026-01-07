using System.Collections.Generic;
using UOP.WinTray.Projects;

namespace UOP.WinTray.UI.Model
{
    public class PropertyListEntry
    {
        public string Name { get; set; }
        public string UnitValue { get; set; }

        public static List<PropertyListEntry> FromUOPProperties(uopProperties aProperties)
        {
            List<PropertyListEntry> _rVal = new();
            if (aProperties == null) return _rVal;
            foreach (var item in aProperties)
            {
                var entry = new PropertyListEntry
                {
                    Name = item.DisplayName,
                    UnitValue = item.UnitValueString(aProperties.DisplayUnits, bIncludeUnitString: true, bYesNoForBool: true)
                };

                _rVal.Add(entry);
            }
            return _rVal;
        }
    }
}
