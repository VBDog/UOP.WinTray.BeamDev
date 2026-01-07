using System;
using System.Collections.Generic;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Structures;

namespace UOP.WinTray.Projects.Materials
{
    public class uopMaterials
    {
        private TMATERIALS tStruc ;

        #region Constructors
        
        public uopMaterials() { tStruc = new TMATERIALS(); }
        
        internal uopMaterials(TMATERIALS aStructure) { tStruc = aStructure; }

        #endregion

        public uopMaterials Clone(bool bReturnEmpty = false) => new uopMaterials(tStruc.Clone(bReturnEmpty));
     

        internal TMATERIALS Structure { get=> tStruc; set => tStruc = value; }


        public int Count => tStruc.Count;

        /// <summary>
        ///  //#2the name to search for
        ///^returns the first material in the collection whose material name matches the passed value
        /// </summary>
        /// <param name="aType"></param>
        /// <param name="aSearchName"></param>
        /// <param name="aIndex"></param>
        /// <returns></returns>
        public uopMaterial GetByMaterialName(uppMaterialTypes aType, string aSearchName)
        {
       
            TMATERIAL aMem = tStruc.GetByMaterialName(aType, aSearchName);
            return (aMem.Index > 0) ? TMATERIAL.FromStructure(aMem): null;
        }

        /// <summary>
        ///  //#2the name to search for
        //^returns the first material in the collection whose material name matches the passed value

        /// </summary>
        /// <param name="aType"></param>
        /// <param name="aSearchName"></param>
        /// <param name="aIndex"></param>
        /// <returns></returns>
        public uopMaterial GetByFamilyName(uppMaterialTypes aType, string aSearchName)
        {
          
            TMATERIAL aMem = tStruc.GetByFamilyName(aType, aSearchName);
            return (aMem.Index > 0) ? TMATERIAL.FromStructure(aMem) : null;
        }

        /// <summary>
        ///   returns the item from the collection at the requested index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public uopMaterial Item(int aIndex)
        {
            if (aIndex < 1 || aIndex > tStruc.Count) throw new IndexOutOfRangeException();
            return uopMaterial.Create(tStruc.Item(aIndex));
        }

        /// <summary>
        ///#1the friendly name to search for
        ///^returns the metal from the collection whose friendly name matches the passed values
        /// </summary>
        /// <param name="aType"></param>
        /// <param name="aFriendlyName"></param>
        /// <param name="aIndex"></param>
        /// <returns></returns>
        public uopMaterial GetByFriendlyName(uppMaterialTypes aType, string aFriendlyName)
        {
           
            TMATERIAL aMem = tStruc.GetByFriendlyName(aType, aFriendlyName);
            return (aMem.Index <= 0) ? null : uopMaterial.Create(aMem);
        }

        /// <summary>
        /// Returns the metal from the collection whose friendly name matches the passed values
        /// </summary>
        /// <param name="aSelectName"></param>
        /// <param name="aType"></param>
        /// <returns></returns>
        public uopMaterial GetBySelectName(string aSelectName, uppMaterialTypes aType = uppMaterialTypes.Undefined)
        {
            TMATERIAL aMem;
            uopMaterials _rVal = new uopMaterials(tStruc.Clone(true));
            aMem = tStruc.GetBySelectName(aSelectName, aType);
            
            return (aMem.Index <= 0) ? null : uopMaterial.Create(aMem);
        }

        /// <summary>
        /// item to add to the collection. used to add an item to the collection
        ///won't add "Nothing"(no error raised).
        ///won't accept a material with no name or no gage value or no thickness value.
        ///won't add doubles.
        /// </summary>
        /// <param name="aMaterial"></param>
        /// <returns></returns>
        internal TMATERIAL AddV(TMATERIAL aMaterial)
        {
             if (aMaterial.Type <= uppMaterialTypes.Undefined) return new TMATERIAL(uppMaterialTypes.Undefined);
          
            return tStruc.Add( aMaterial);
            
        }

        /// <summary>
        ///  The item to add to the collection used to add an item to the collection
        /// won't add "Nothing" (no error raised).
        /// won't accept a material with no name or no gage value or no thickness value.
        ///won't add doubles.
        /// </summary>
        /// <param name="aMaterial"></param>
        public uopMaterial Add(uopMaterial aMaterial)
        {
            if (null == aMaterial) return null;
            
            TMATERIAL aMem = AddV(aMaterial.Structure);
            return (aMem.Index > 0) ? TMATERIAL.FromStructure(aMem) : null;
        }
        /// <summary>
        /// #1the thickness to search for
        /// ^returns the material in the collection whose thickness most closely matches the passed value
        /// </summary>
        /// <param name="type"></param>
        /// <param name="searchThk"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public uopMaterial GetByNearestThickness(uppMaterialTypes type, double searchThk)
        {

            TMATERIAL aMem = tStruc.GetByNearestThickness(type, searchThk);
            return (aMem.Index > 0) ? TMATERIAL.FromStructure(aMem) : null;
        }

        public uopMaterials GetByFamily(uppMaterialTypes aType, uppMetalFamilies aSearchFamily)
        {
            return new uopMaterials(tStruc.GetByFamily(aType, aSearchFamily));
        }

        internal uopSheetMetal GetByFamilyAndThickness(uppMetalFamilies aMaterialFamily, double aThickness)
        {
            TMATERIAL aMem = tStruc.GetByFamilyAndThickness(uppMaterialTypes.SheetMetal, aMaterialFamily,aThickness);
            return (aMem.Index > 0) ? (uopSheetMetal)TMATERIAL.FromStructure(aMem) : new uopSheetMetal();
        }


        /// <summary>
        /// returns the metal from the collection whose family and gage matches the passed values
        /// #1the family name to search for
        /// #2the sheet gage to search for
        /// </summary>
        /// <param name="aFamily"></param>
        /// <param name="aSheetGage"></param>
        /// <param name="aIndex"></param>
        /// <returns></returns>
        public uopSheetMetal GetSheetMetalByStringVals(string aFamily, string aSheetGage)
        {
            TMATERIAL aMem = tStruc.GetSheetMetalByStringVals(aFamily,aSheetGage);
            return (aMem.Index > 0 && aMem.Type == uppMaterialTypes.SheetMetal) ? (uopSheetMetal)TMATERIAL.FromStructure(aMem) : null;
        }


        /// <summary>
        /// returns the metal from the collection whose family and gage matches the pased values
        /// #1the family name to search for
        /// #2the sheet gage to search for
        /// </summary>
        /// <param name="aFamily"></param>
        /// <param name="aSheetGage"></param>
        /// <param name="aIndex"></param>
        /// <returns></returns>
        public uopSheetMetal GetByFamilyAndGauge(uopMaterialFamily aFamily, uopMaterialGage aSheetGage)
        {
            TMATERIAL aMem = tStruc.GetByFamilyAndGauge( aFamily != null ? aFamily.Family : uppMetalFamilies.CarbonSteel , aSheetGage != null ? aSheetGage.GageType : uppSheetGages.Gage12 ) ;
            return (aMem.Index > 0 && aMem.Type == uppMaterialTypes.SheetMetal) ? (uopSheetMetal)TMATERIAL.FromStructure(aMem) : null;

        }

        /// <summary>
        /// returns the metal from the collection whose family and gage matches the pased values
        /// #1the family name to search for
        /// #2the sheet gage to search for
        /// </summary>
        /// <param name="aFamily"></param>
        /// <param name="aSheetGage"></param>
        /// <param name="aIndex"></param>
        /// <returns></returns>
        public uopSheetMetal GetByFamilyAndGauge(uppMetalFamilies aFamily, uppSheetGages aSheetGage)
        {
            TMATERIAL aMem = tStruc.GetByFamilyAndGauge(aFamily, aSheetGage);
            return (aMem.Index > 0 && aMem.Type == uppMaterialTypes.SheetMetal) ? (uopSheetMetal)TMATERIAL.FromStructure(aMem) : null;

        }

        public uopSheetMetal GetByDescriptor(uppMaterialTypes aType, string aDescriptor)
        {
            TMATERIAL aMem = tStruc.GetByDescriptor(aDescriptor);
            return (aMem.Index > 0 && aMem.Type == uppMaterialTypes.SheetMetal) ? (uopSheetMetal)TMATERIAL.FromStructure(aMem) : null;
        }
     
        
        ///^returns the metal from the collection whose family and gage matches the passed values
        ///~if the material can't be found then the descriptor is used to create it and addit to the collection.
        ///~if the material can't be defined with the descriptor the material with the default family and gage is returned.
        ///~"Nothing" is never returned. 
        /// </summary>
        /// <param name="aFamily"></param>
        /// <param name="aSheetGage"></param>
        /// <param name="aDescriptor"></param>
        /// <param name="aDefaultFamily"></param>
        /// <param name="aDefaultGage"></param>
        /// <param name="aIndex"></param>
        /// <returns></returns>
        public uopMaterial Retrieve(string aFamily, string aSheetGage, string aDescriptor = "", uppMetalFamilies aDefaultFamily = uppMetalFamilies.CarbonSteel, uppSheetGages aDefaultGage = uppSheetGages.Gage12)
        {
            
            TMATERIAL aMem = tStruc.Retrieve(aFamily, aSheetGage, aDescriptor, aDefaultFamily, aDefaultGage);

            return (aMem.Index > 0) ? TMATERIAL.FromStructure(aMem) : null;
        }

        /// <summary>
        /// Fetches all bolting material types
        /// </summary>
        /// <returns></returns>
        public static List<string> GetBoltingMaterialNames()
        {
            uopHardwareMaterial hMat;
            uopMaterials hMats = uopGlobals.goHardwareMaterialOptions();
            string familyName = null;
            List<string> _rVal = new List<string>();

            for (int i = 1; i <= hMats.Count; i++)
            {
                hMat = hMats.Item(i) as uopHardwareMaterial;
                if (null != hMat)
                {
                    familyName = hMat.FamilySelectName;

                    if (!string.IsNullOrWhiteSpace(familyName)) _rVal.Add(familyName);
                }
            }

            return _rVal;
        }

        /// <summary>
        /// Fetches all bolting material types
        /// </summary>
        /// <returns></returns>
        public static List<uopHardwareMaterial> GetBoltingMaterials()
        {
            uopHardwareMaterial hMat;
            uopMaterials hMats = uopGlobals.goHardwareMaterialOptions();
            string familyName = null;
            List<uopHardwareMaterial> _rVal = new List<uopHardwareMaterial>();

            for (int i = 1; i <= hMats.Count; i++)
            {
                hMat = (uopHardwareMaterial)hMats.Item(i);
                if (null != hMat)
                {
                    familyName = hMat.FamilySelectName;

                    if (!string.IsNullOrWhiteSpace(familyName)) _rVal.Add(hMat);
                }
            }

            return _rVal;
        }

        /// <summary>
        /// Fetches all sheet metal families
        /// </summary>
        /// <returns></returns>
        public static List<uopMaterialFamily> GetSheetMetalFamilies()
        {
            uopSheetMetal sMat;
            uopMaterials sMats = uopGlobals.goSheetMetalOptions();
            string familyName = null;
            List<uopMaterialFamily> _rVal = new List<uopMaterialFamily>();
            int cnt = 0;
            for (int i = 1; i <= sMats.Count; i++)
            {
                sMat = (uopSheetMetal)sMats.Item(i);
                if (null != sMat)
                {
                    familyName = sMat.FamilyName;

                    if (!string.IsNullOrWhiteSpace(familyName))
                    {
                        if (_rVal.Find((x) => string.Compare(familyName, x.Name, ignoreCase: true) == 0) == null)
                        {
                            _rVal.Add(new uopMaterialFamily() { Name = familyName, Id = i - 1, Density = sMat.Density, IsStainlessSteel = sMat.IsStainless, Family = sMat.Family });
                            cnt++;
                        }

                    }
                        
                }
            }

            return _rVal;
        }
        /// <summary>
        /// Fetches all sheet metal families
        /// </summary>
        /// <returns></returns>
        public static List<uopMaterialGage> GetSheetMetalGages(uopMaterialFamily aFamily )
        {
            List<uopMaterialGage> _rVal = new List<uopMaterialGage>();
            if (aFamily == null) return _rVal;
            if (string.IsNullOrWhiteSpace( aFamily.Name)) return _rVal;
            string familyName = aFamily.Name.Trim();


            uopSheetMetal sMat;
            uopMaterials sMats = uopGlobals.goSheetMetalOptions();

            string gname = string.Empty;
            int cnt = 0;

            for (int i = 1; i <= sMats.Count; i++)
            {
                sMat = (uopSheetMetal)sMats.Item(i);
                if (null != sMat)
                {
                    if (string.Compare(familyName, sMat.FamilyName, ignoreCase: true) == 0 || string.Compare(familyName, sMat.FamilySelectName, ignoreCase: true) == 0)
                    {
                        gname = sMat.GageName;
                        if (string.IsNullOrWhiteSpace(gname))
                        {
                            gname = sMat.SheetGage.GetDescription();
                        }
                        if (!string.IsNullOrWhiteSpace(gname))
                        {
                            if (_rVal.Find((x) => string.Compare(gname, x.Gage, ignoreCase: true) == 0) == null)
                            {
                                _rVal.Add(new uopMaterialGage() { FamilyName = familyName, Gage = gname, Id = cnt, Thickness = sMat.Thickness, GageType = sMat.SheetGage });
                                cnt++;
                            }
                        }
                    }
                }
            }

            return _rVal;
        }
    }
}
