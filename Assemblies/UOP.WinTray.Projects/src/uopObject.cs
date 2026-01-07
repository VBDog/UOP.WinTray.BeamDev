using System;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Structures;

namespace UOP.WinTray.Projects
{
    public class uopObject : ICloneable
    {

        TOBJECT tStruc;

        public uopObject() { tStruc = new TOBJECT(""); }

        internal uopObject(TOBJECT aStructure) { tStruc = aStructure; }


        public void AddProperty(string aName, dynamic aValue, uppUnitTypes aUnitType = uppUnitTypes.Undefined, bool bIsHidden = false, string aHeading = "", string aCaption = "", string aDecodeString = "",
                                bool bProtected = false, string aCategory = "", uppPartTypes aPartType = uppPartTypes.Undefined, bool bShared = false, int aPrecision = -1, string aChoices = "")
        {
            TPROPERTY aMem;
            if (!tStruc.Properties.TryGet(aName, out aMem))
            {
                var tempProperties = tStruc.Properties;
                tempProperties.Add(aName, aValue, aUnitType, bIsHidden, aHeading, aCaption, aDecodeString, bProtected, aCategory, aPartType, bShared, aPrecision, aChoices);
                tStruc.Properties = tempProperties;
            }
            else
            {
                aMem.SetValue(aValue);
                tStruc.Properties.SetItem(aMem.Index, aMem);
            }
        }

        internal TPROPERTIES Properties { get => tStruc.Properties; set => tStruc.Properties = value; }

        public uopObject Clone() => new uopObject(tStruc.Clone());

        object ICloneable.Clone() => (object)this.Clone();
        
        public string Name => tStruc.Name;

        internal TOBJECT Structure { get => tStruc; set => tStruc = value; }

        public double ValueD(dynamic aNameOrIndex, double aDefault = 0, double aMultiplier = 0, int aPrecis = -1) //, int aPrecis = -1)
        => Properties.ValueD(aNameOrIndex, aDefault, aMultiplier, aPrecis);

        /// <summary>
        ///returns the 'integer' value of the item from the collection at the requested index or the passed name
        /// </summary>
        /// <param name="aNameOrIndex"></param>
        /// <param name="aDefault"></param>
        /// <returns></returns>

        public int ValueI(dynamic aNameOrIndex, int aDefault = 0)
        => Properties.ValueI(aNameOrIndex, aDefault);

        /// <summary>
        ///returns the 'bool' value of the item from the collection at the requested index or the passed name
        /// </summary>
        /// <param name="aNameOrIndex"></param>
        /// <param name="aDefault"></param>
        /// <returns></returns>
        public bool ValueB(dynamic aNameOrIndex, bool aDefault = false)
        => Properties.ValueB(aNameOrIndex, aDefault);


        /// <summary>
        ///returns the 'string' value of the item from the collection at the requested index or the passed name
        /// </summary>
        /// <param name="aNameOrIndex"></param>
        /// <param name="aDefault"></param>
        /// <returns></returns>
        public string ValueS(dynamic aNameOrIndex, string aDefault = "", bool formatted = false)
        => Properties.ValueS(aNameOrIndex, aDefault,formatted);



    }
}
