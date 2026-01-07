using System;
using System.Collections.Generic;


namespace UOP.WinTray.Projects.Utilities
{
    public class mzList
    {
        private readonly string _Delimitor = ",";
        private string _Value = string.Empty;
        private bool _AllowDupicates = true;
        private bool _AllowNulls = true;

        #region Constructors

        public mzList(string aDelimitor = ",", bool aAllowDupes = true, bool aAllowNulls = true, string aBaseValue = "")
        {
            _Value = string.IsNullOrWhiteSpace(aBaseValue) ? string.Empty : aBaseValue;
            _Delimitor = String.IsNullOrEmpty(aDelimitor) ? "," : aDelimitor;
            _AllowDupicates = aAllowDupes;
            _AllowNulls = aAllowNulls;

        }
        #endregion  Constructors

        #region Properties

        public string Delimitor { get => _Delimitor; }

        public string Value { get => _Value; }

        public bool AllowDupicates { get => _AllowDupicates; set => _AllowDupicates = value; }
        public bool AllowNulls { get => _AllowNulls; set => _AllowNulls = value; }

        public List<string> ToList  => mzUtils.StringsFromDelimitedList(_Value, Delimitor, bReturnNulls: AllowNulls); 



        #endregion Properties

        #region Methods

        public string Add(object aValue)
        {
            string newval = (aValue == null) ? string.Empty : aValue.ToString();
            if ((string.IsNullOrWhiteSpace(newval) && _AllowNulls) || !string.IsNullOrWhiteSpace(newval))
            {
                
                mzUtils.ListAdd(ref _Value, newval, bSuppressTest: _AllowDupicates,aDelimitor:_Delimitor, bAllowNulls:true);
                if (string.IsNullOrWhiteSpace(newval) && _AllowNulls)
                {
                    if (string.IsNullOrWhiteSpace(_Value)) _Value = _Delimitor;
                }

            }

            return _Value;
        }

        public void Clear() => _Value = string.Empty;

        public void Append(string aSublist, string aSublistDelimiter) 
        {
            if (aSublist == null) return;
            List<string> newvals = mzUtils.StringsFromDelimitedList(aSublist, aSublistDelimiter, _AllowNulls);
            foreach (var item in newvals)
            {
                Add(item);
            }


        } 
        public override string ToString()
        {
            return _Value;
        }
        #endregion Methods

    }
}
