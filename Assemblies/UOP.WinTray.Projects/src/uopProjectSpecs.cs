using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Materials;
using UOP.WinTray.Projects.Structures;


namespace UOP.WinTray.Projects
{
    public class uopProjectSpecs
    {
        private TPROJECTSPECS tStruc = new TPROJECTSPECS();

        private const int iSheet = 1;
        private const int iPlate = 2;
        private const int iPipe = 3;
        private const int iFlange = 4;
        private const int iFitting = 5;
        private const int iGasket = 6;
        private const int iTube = 7;
        private const int iBolt = 8;
        private const int iNut = 9;
        private const int iLockwasher = 10;
        private const int iFlatwasher = 11;
        private const int iStud = 12;
        private const int iSetScrew = 13;
        private const int iCarbon = 1;
        private const int iStainless = 2;
        private const int iMetricCarbon = 3;
        private const int iMetricStainless = 4;

        public string ProjectHandle { get => tStruc.ProjectHandle; set => tStruc.ProjectHandle = value; }

        internal TMATERIALSPEC GetSpecV(uppSpecTypes aType, bool bStainless, bool bMetric)
        { return GetSpecV(aType, bStainless, bMetric, out bool FND); }


            internal TMATERIALSPEC GetSpecV(uppSpecTypes aType, bool bStainless,bool bMetric,out bool rFound)
        {
            int idx1 = 0;
            int idx2 = 0;
            rFound = GetIndices(aType, bStainless, bMetric, out idx1, out idx2);
            return rFound ? tStruc.Item(idx1).Item(idx2) : new TMATERIALSPEC();
        }

        internal bool SetSpec(uopMaterialSpec aSpec) { string NMS = string.Empty; return SetSpec(aSpec, out NMS); }

        internal bool SetSpec(uopMaterialSpec aSpec, out string rLastName)
        {
            rLastName = string.Empty;
            if (aSpec == null) return false;
            return SetSpecV(aSpec.Structure, out rLastName);
        }

        internal bool SetSpecV(TMATERIALSPEC aSpec) { string NMS = string.Empty; return SetSpecV(aSpec, out NMS); }
        
            internal bool SetSpecV(TMATERIALSPEC aSpec, out string rLastName)
        {
            bool _rVal = false;
            int idx1 = 0;

            int idx2 = 0;

            rLastName = string.Empty;
            if (GetIndices(aSpec.Type, aSpec.Stainless, aSpec.Metric, out idx1, out idx2))
            {
                TSPECSET aSet = tStruc.Item(idx1);

                rLastName = aSet.Item(idx2).Name;
                _rVal = string.Compare(rLastName, aSpec.Name) != 0;
                aSet.SetItem(idx2, aSpec);
                tStruc.SetItem(idx1, aSet);
                _rVal = true;
            }
            return _rVal;
        }

        bool GetIndices(uppSpecTypes aSpecType, bool bStainless, bool bMetric, out int rSetIndex, out int rSpecIndex)
        {
            rSpecIndex = bStainless ? iStainless : iCarbon;
            rSetIndex = 0;


            
            if ((int)aSpecType >= 100)
            {
                if (bMetric) rSpecIndex = bStainless ? iMetricStainless : iMetricCarbon;
                 
            }


            switch (aSpecType)
            {
                case uppSpecTypes.SheetMetal:
                    rSetIndex = iSheet;
                    break;
                case uppSpecTypes.Plate:
                    rSetIndex = iPlate;
                    break;
                case uppSpecTypes.Pipe:
                    rSetIndex = iPipe;
                    break;
                case uppSpecTypes.Flange:
                    rSetIndex = iSheet;
                    break;
                case uppSpecTypes.Fitting:
                    rSetIndex = iFitting;
                    break;
                case uppSpecTypes.Bolt:
                    rSetIndex = iBolt;
                    break;
                case uppSpecTypes.Nut:
                    rSetIndex = iNut;
                    break;
                case uppSpecTypes.LockWasher:
                    rSetIndex = iLockwasher;
                    break;
                case uppSpecTypes.FlatWasher:
                    rSetIndex = iFlatwasher;
                    break;
                case uppSpecTypes.ThreadedStud:
                    rSetIndex = iStud;
                    break;
                case uppSpecTypes.Gasket:
                    rSetIndex = iGasket;
                    rSpecIndex = 1;
                    break;
                case uppSpecTypes.SetScrew:
                    rSetIndex = iSetScrew;
                    break;
                case uppSpecTypes.Tube:
                    rSetIndex = iTube;
                    break;
                default:
                    break;
            }



            return rSetIndex > 0;
        }
    }
}