using UOP.DXFGraphics;
using System;
using UOP.WinTray.Projects.Structures;


namespace UOP.WinTray.Projects
{
    public class uopDetail : ICloneable
    {

        private TDETAIL tStruc;
        public string Name
        {
            get => tStruc.Name;
            set
            {
                tStruc.Name = value;
            }
        }

        public string GIFFile_English
        {
            get => tStruc.GIFFile_English;
            set
            {
                tStruc.GIFFile_English = value;
            }
        }
        public string GIFFile_Metric
        {
            get => tStruc.GIFFile_Metric;

            set
            {
                tStruc.GIFFile_Metric = value;
            }
        }

        public string Title
        {
            get => tStruc.Title;
            set
            {
                tStruc.Title = value;
            }
        }
        public string BlockName
        {
            get => tStruc.BlockName;
            set
            {
                tStruc.BlockName = value;
            }
        }
        public string FileName
        {
            get => tStruc.FileName;
            set
            {
                tStruc.FileName = value;
            }
        }
        public string Description
        {
            get => tStruc.Description;
            set => tStruc.Description = value;
            
        }

        internal void SaveAttribute(TATTRIBUTE aAttribute)
        {
            tStruc.Attributes.Add(aAttribute);
        }

        public dxfAttributes Attributes(bool aMetricValues = false)
        {
            dxfAttributes Attributes = new dxfAttributes();
            if (tStruc.Attributes.Count > 0)
            {
                for (int i = 1; i <= tStruc.Attributes.Count - 1; i++)
                {
                    if (aMetricValues)
                    {
                        Attributes.AddTagValue(tStruc.Attributes.Item(i).Tag, tStruc.Attributes.Item(i).MetricValue, tStruc.Attributes.Item(i).Tag);
                    }
                    else
                    {
                        Attributes.AddTagValue(tStruc.Attributes.Item(i).Tag, tStruc.Attributes.Item(i).EnglishValue, tStruc.Attributes.Item(i).Tag);
                    }
                }
            }
            return Attributes;
        }
        TDETAIL Structure
        {
            get => tStruc;
            set
            {
                tStruc = value;
            }
        }
        /// <summary>
        /// returns an new object with the same properties as the cloned object
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            uopDetail uopDetailObj = new uopDetail
            {
                Structure = Force.DeepCloner.DeepClonerExtensions.DeepClone<TDETAIL>(tStruc)
            };
            return uopDetailObj;
        }
    }
}
