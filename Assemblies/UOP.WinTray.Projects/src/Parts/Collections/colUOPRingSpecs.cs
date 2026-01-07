using System;
using System.IO;
using UOP.WinTray.Projects.Documents;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.Projects.Parts
{
    /// <summary> 
    /// a collection of colUOPRingSpecs
    /// </summary>
    public class colUOPRingSpecs : uopParts
    {
        public override uppPartTypes BasePartType => uppPartTypes.RingSpecs;

        #region Constructor
        public colUOPRingSpecs() :base(uppPartTypes.RingSpecs, uppProjectFamilies.uopFamMD)
        {
            Class_Initialize();
        }

        public colUOPRingSpecs(colUOPRingSpecs aPartToCopy, bool bDontCopyMembers = false,uopPart aParent = null) : base(aPartToCopy,bDontCopyMembers,aParent)
        {
          
        }

        private void Class_Initialize()
        {
            base.PartFilter = uppPartTypes.RingSpec;
            //english ring defaults  UOP: LOWER BOUND : UPPER BOUND : METRIC =  FLAG
            AddByString("UOP:0:36:1.5:FALSE");
            AddByString("UOP:36:84:2:FALSE");
            AddByString("UOP:84:156:2.5:FALSE");
            AddByString("UOP:156:240:3:FALSE");
            AddByString("UOP:240:300:3.5:FALSE");
            AddByString("UOP:300:420:4:FALSE");
            AddByString("UOP:420:480:4.5:FALSE");
            AddByString("UOP:480:552:5.0:FALSE");
            AddByString("UOP:552:600:5.5:FALSE");

            //metric ring defaults  UOP: LOWER BOUND : UPPER BOUND : METRIC =  TRUE
            AddByString("UOP:0:915:38:TRUE");
            AddByString("UOP:915:2130:50:TRUE");
            AddByString("UOP:2130:3960:65:TRUE");
            AddByString("UOP:3960:6100:75:TRUE");
            AddByString("UOP:6100:7620:90:TRUE");
            AddByString("UOP:7620:10670:100:TRUE");
            AddByString("UOP:10670:12190:115:TRUE");
            AddByString("UOP:12190:14020:125:TRUE");
            AddByString("UOP:14020:15240:140:TRUE");

            //english  ring defaults  ABB LUMMUS: LOWER BOUND : UPPER BOUND : METRIC =  FLAG
            AddByString("ABB LUMMUS:0:54:2:FALSE");
            AddByString("ABB LUMMUS:54:102:2.5:FALSE");
            AddByString("ABB LUMMUS:102:162:3:FALSE");
            AddByString("ABB LUMMUS:162:192:3.5:FALSE");
            AddByString("ABB LUMMUS:192:100000:4:FALSE");

            //metric  ring defaults  ABB LUMMUS: LOWER BOUND : UPPER BOUND : METRIC =  TRUE
            AddByString("ABB LUMMUS:0:1375:50:TRUE");
            AddByString("ABB LUMMUS:1375:2575:65:TRUE");
            AddByString("ABB LUMMUS:2575:4100:75:TRUE");
            AddByString("ABB LUMMUS:4100:4900:90:TRUE");
            AddByString("ABB LUMMUS:4900:10000:100:TRUE");
     
        }
        #endregion

        public colUOPRingSpecs Clone() => new colUOPRingSpecs(this);

        public override uopPart Clone(bool aFlag = false) => (uopParts)this.Clone();

        #region Methods
        /// <summary>
        ///  // #1the shell id to determine a ring with for
        /// #2the project requesting the calc
        /// ^returns the suggested ring with for the passed shell id
        /// </summary>
        /// <param name="ShellID"></param>
        /// <param name="Project"></param>
        /// <returns></returns>
        public double GetRingWidth(double shellID, uopProject project = null)
        {
            double _rVal = 0;
            uopRingSpec aMem;
            string uop = "UOP";
            bool bMetric;
            uopRingSpec spec = new uopRingSpec();
            double shid = 0;
            bMetric = false;

            if (project != null)
            {
                bMetric = project.MetricRings;
                if (bMetric)
                    shid = shellID * 25.4;
                else
                    shid = shellID;

                if (!string.IsNullOrEmpty(project.Contractor) && project.Contractor.StartsWith("Lummus"))
                {
                    uop = "ABB Lummus";
                }
                if (!string.IsNullOrEmpty(project.ProcessLicensor) && uop == "UOP" && project.ProcessLicensor.StartsWith("Lummus"))
                {
                    uop = "ABB Lummus";
                }
            }

            // find the applicable aMem

            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (bMetric == aMem.Metric && string.Compare(uop, aMem.Contractor, StringComparison.OrdinalIgnoreCase) == 0 && shid >= aMem.ShellMin && shid < aMem.ShellMax)
                {
                    spec = aMem;
                    break;
                }
            }

            if (spec != null)
            {
                if (bMetric)
                    _rVal = spec.RingWidth / 25.4;
                else
                    _rVal = spec.RingWidth;
            }
            return _rVal;
        }

        /// <summary>
        /// '#1the item to add to the collection
        ///'^used to add an item to the collection
        ///'~won't add "Nothing" (no error raised).
        /// </summary>
        /// <param name="spec"></param> 
        public void Add(uopRingSpec spec)
        {
            if ( spec != null) base.Add(spec);
            
        }
        /// <summary>
        /// ^shorthand method for adding a spec to the collection
        /// </summary>
        /// <param name="SpecDescriptor"></param>
        public void AddByString(string specDescriptor)
        {
            if (string.IsNullOrWhiteSpace(specDescriptor)) return;
            specDescriptor = specDescriptor.Trim();
           if (specDescriptor.StartsWith(":")) return;

            uopRingSpec newSpec = new uopRingSpec();
            newSpec.DefineByString(specDescriptor);
            Add(newSpec);
        }
        /// <summary>
        /// shorthand method for adding a spec to the collection
        /// </summary>
        /// <param name="Owner"></param>
        /// <param name="ShellMin"></param>
        /// <param name="ShellMax"></param>
        /// <param name="RingWidth"></param>
        /// <param name="IsMetric"></param>
        public void AddSpec(string owner, double shellMin, double shellMax, double ringWidth, bool isMetric)
        {
            uopRingSpec newSpec = new uopRingSpec
            {
                Contractor = owner,
                ShellMin = shellMin,
                ShellMax = shellMax,
                RingWidth = ringWidth,
                Metric = isMetric
            };
            Add(newSpec);
        }
        /// <summary>
        /// add the defaults
        /// </summary>

        /// <summary>
        ///returns the item from the collection at the requested index
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        public new uopRingSpec Item(int aIndex) => (uopRingSpec)base.Item(aIndex);

        /// <summary>
        /// Read From INI File
        /// </summary>
        /// <param name="FileSpec"></param>
        public void ReadFromINIFile(string fileSpec)
        {
            if (string.IsNullOrEmpty(fileSpec))
                return;
            if (!File.Exists(fileSpec))
                return;

            bool found = false;
            string stringSpec;
            uopRingSpec spec;

            stringSpec = uopUtils.ReadINI_String(fileSpec, "RING SPECS", "SPEC" + "1", "", out found);
            if (stringSpec ==  string.Empty | !found) return;
            spec = new uopRingSpec();
            spec.DefineByString(stringSpec);
            Add(spec);
            for (int i = 2; i <= 10000; i++)
            {
                stringSpec = uopUtils.ReadINI_String(fileSpec, "RING SPECS", "SPEC" + i, "",  out found);
                if (string.IsNullOrEmpty(stringSpec) | !found)
                    break;
                spec = new uopRingSpec();
                spec.DefineByString(stringSpec);
                Add(spec);
            }
        }
        /// <summary>
        /// Save To INI File
        /// </summary>
        /// <param name="fileSpec"></param>
        public void SaveToINIFile(string fileSpec)
        {
            if (string.IsNullOrEmpty(fileSpec))
                return;
            if (!File.Exists(fileSpec))
                return;
            uopRingSpec spec;
            for (int i = 0; i <= this.Count; i++)
            {
                spec = this.Item(i);
                uopUtils.WriteINIString(fileSpec, "RING SPECS", $"SPEC { i + 1}", spec.Descriptor);
            }
        }
        /// <summary>
        /// Test Ring Width
        /// </summary>
        /// <param name="shellID"></param>
        /// <param name="ringWidth"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        public uopDocWarning TestRingWidth(double shellID, double ringWidth, uopProject project)
        {
            uopRingSpec mem;
            string stringObject;
            bool metric;
            uopRingSpec spec = null;
            uopDocWarning _rVal = null;
            double shid = 0;
            double rwd;
            double srwd;

            stringObject = "UOP";
            metric = false;

            if (project != null)
            {
                metric = project.MetricRings;
                if (metric)
                    shid = shellID * 25.4;
                else
                    shid = shellID;

                if (project.Contractor.StartsWith("Lummus") | project.ProcessLicensor.StartsWith("Lummus"))
                    stringObject = "ABB Lummus";
            }

            // find the applicable aMem

            for (int i = 1; i <= Count; i++)
            {
                mem = Item(i);
                if (string.Compare(stringObject, mem.Contractor) == 0)
                {
                    if (mem.Metric == metric)
                    {
                        if (shid >= mem.ShellMin & shid < mem.ShellMax)
                        {
                            spec = mem;
                            break;
                        }
                    }
                }
            }

            if (spec != null)
            {
                if (spec.Metric)
                {
                    srwd = spec.RingWidth;
                    rwd = Math.Round(ringWidth * 25.4, 0);
                    if (rwd < srwd)
                    {
                        _rVal = new uopDocWarning
                        {
                            Brief = "Ring Width Warning",
                            TextString = "Ring Width of " + (ringWidth * 25.4).ToString("0") + " mm Is Less Than The Suggested " + spec.Contractor + " Ring aMem of " + spec.RingWidth + " mm"
                        };
                    }
                }
                else
                {
                    srwd = Math.Round(spec.RingWidth, 2);
                    rwd = Math.Round(ringWidth, 2);
                    if (rwd < srwd)
                    {
                        _rVal = new uopDocWarning
                        {
                            Brief = "Ring Width Warning",
                            TextString = "Ring Width of " + ringWidth.ToString("0.000") + " in. Is Less Than The " + spec.Contractor + " Ring aMem of " + spec.RingWidth + " in."
                        };
                    }
                }
            }

            return _rVal;
        }

        #endregion
    }
}