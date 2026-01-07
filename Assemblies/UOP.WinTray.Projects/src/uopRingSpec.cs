using System;
using UOP.WinTray.Projects.Enums;

namespace UOP.WinTray.Projects
{
    /// <summary>
    /// represents class uopRingSpec
    /// </summary>
    public class uopRingSpec : uopPart
    {
        public override uppPartTypes BasePartType => uppPartTypes.RingSpec;
        private string _Contractor;
        private double _ShellMin;
        private double _ShellMax;
        private double _RingWidth;
        private bool _Metric;


        #region Constructors

        public uopRingSpec() : base(uppPartTypes.RingSpec, uppProjectFamilies.uopFamMD, "", "", true)
        { }

        internal uopRingSpec(uopRingSpec aPartToCopy) : base(uppPartTypes.RingSpec, uppProjectFamilies.uopFamMD, "", "", true)
        {
            Copy(aPartToCopy);
            _Metric = aPartToCopy.Metric;
            _Contractor = aPartToCopy.Contractor;
            _ShellMin = aPartToCopy.ShellMin;
            _ShellMax = aPartToCopy.ShellMax;
            _RingWidth = aPartToCopy.RingWidth;


        }

        #endregion

        #region property
        public new double RingWidth { get=> _RingWidth; set => _RingWidth = value; }
        public double ShellMin { get => _ShellMin; set => _ShellMin = value; }
        public double ShellMax { get => _ShellMax; set=> _ShellMax = value; }
        public string Contractor { get => _Contractor; set => _Contractor = value; }
        public bool Metric { get => _Metric; set => _Metric = value; }
        /// <summary>
        ///  '^returns as string that containts the properties of the spec
        ///'~used for saving the spec to a file
        /// </summary>
        public string Descriptor => Contractor + ":" + ShellMin + ":" + ShellMax + ":" + RingWidth + ":" + Metric;

        public override uopPart Clone(bool aFlag = false) => (uopPart)this.Clone();
        
        #endregion

        #region Methods 
      

        /// <summary>
        /// ^sets the properties of the spec based on the passed string
        /// ~used for saving the spec to a file
        /// </summary>
        /// <param name="aDescriptor"></param>
        public void DefineByString(string aDescriptor)
        {
            aDescriptor = aDescriptor.Trim();
            if (string.IsNullOrEmpty(aDescriptor))
                return;
            if (aDescriptor.StartsWith(":"))//Verify and delete this line// if (Strings.InStr(1, aDescriptor, ":", Constants.vbTextCompare) == 0)
                return;

            string[] vals;

            vals = aDescriptor.Split(':');
            if (vals.Length >= 0)
                Contractor = vals[0];
            if (vals.Length >= 1)
                ShellMin = Convert.ToDouble(vals[1]);
            if (vals.Length >= 2)
                ShellMax = Convert.ToDouble(vals[2]);
            if (vals.Length >= 3)
                RingWidth = Convert.ToDouble(vals[3]);
            if (vals.Length >= 4)
                Metric = string.Compare(vals[4].ToUpper(), "TRUE") == 0;
        }

       
        public override bool IsEqual(uopPart aPart)
        { return false; }

        public override void UpdatePartWeight() { return; }


        public override void UpdatePartProperties()
        { return; }


      
        #endregion
    }
}
