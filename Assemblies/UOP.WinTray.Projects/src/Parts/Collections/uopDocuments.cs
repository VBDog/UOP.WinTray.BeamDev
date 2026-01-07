using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UOP.WinTray.Projects.Constants;
using UOP.WinTray.Projects.Documents;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.Projects
{
    /// <summary>
    /// a collection of iuopDocuments
    /// </summary>
    public class uopDocuments : List<uopDocument>, IEnumerable<uopDocument>, IDisposable
    {
        // Option Explicit

        private bool _InvalidWhenEmpty = false;


        public uopDocuments()
        {
            _InvalidWhenEmpty = true;
        }
        public uopDocuments(List<uopDocWarning> aDocumentsList)
        {
            _InvalidWhenEmpty = true;
            if (aDocumentsList != null)
            {
                foreach (var item in aDocumentsList)
                {
                    Add(item);
                }
            }
        }

        /// <summary>
        /// ^used to add an item to the collection
        ///  ~won't add "Nothing" (no error raised).
        /// </summary>
        /// <param name="aDocument">the item to add to the collection</param>
        /// <param name="bAddClone"></param>
        /// <returns></returns>
        public uopDocument Add(uopDocument aDocument, bool bAddClone = false)
        {

            if (aDocument == null) return null;
            uopDocument _rVal = bAddClone ? aDocument.Clone() : aDocument;

            if (!string.IsNullOrWhiteSpace(NewMemberCategory) && string.IsNullOrWhiteSpace(_rVal.Category)) _rVal.Category = NewMemberCategory;
            if (!string.IsNullOrWhiteSpace(NewMemberSubCategory) && string.IsNullOrWhiteSpace(_rVal.SubCategory)) _rVal.SubCategory = NewMemberSubCategory;
            if (!string.IsNullOrWhiteSpace(NewMemberRangeGUID)) _rVal.RangeGUID = NewMemberRangeGUID;

            _rVal.Index = Count + 1;
            base.Add(_rVal);
            return _rVal;
        }

        /// <summary>
        /// ^used to add an item to the collection
        ///  ~won't add "Nothing" (no error raised).
        /// </summary>
        /// <param name="aDocument">the item to add to the collection</param>
        /// <param name="aSheetIndex"></param>
        /// <param name="bAddClone"></param>
        /// <returns></returns>
        public uopDocument Add(uopDocument aDocument, ref int aSheetIndex, bool bAddClone = false)
        {
            uopDocument _rVal = Add(aDocument, bAddClone);

            if (_rVal != null)
            {
                aSheetIndex++;
                _rVal.SheetNumber = aSheetIndex;

            }
            return _rVal;
        }
        /// <summary>
        ///#1the part that the calculation is generated from
        ///#2the name to assign to the calculation
        ///#3the string used to select the calculation from a list
        ///#4the type of calculation to add
        ///^shorthand way to add a calculation without the code to create one and add it
        ///~bad input like empty required strings will cause the calculation not to be added to the collection
        ///~but no error is raised.
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="aCalcName"></param>
        /// <param name="aSelectText"></param>
        /// <param name="aCalcType"></param>
        /// <param name="aCategory"></param>
        /// <param name="aSubCategory"></param>
        /// <param name="bQuestionsAreTrayPrompt"></param>
        /// <param name="aQuestions"></param>
        /// <returns></returns>
        public uopDocCalculation AddCalculation(uopPart aPart, string aCalcName, string aSelectText, uppCalculationType aCalcType, string aCategory = "", string aSubCategory = "", bool bQuestionsAreTrayPrompt = false, mzQuestions aQuestions = null)
        {

            uopDocCalculation _rVal = new uopDocCalculation(aCalcType)
            {
                Part = aPart,
                Name = aCalcName,
                SelectText = aSelectText,
                Category = aCategory,
                SubCategory = aSubCategory
            };

            if (aQuestions != null)
            {
                if (!bQuestionsAreTrayPrompt)
                { _rVal.Options = aQuestions.Clone(); }
                else
                { _rVal.TrayQuery = aQuestions.Clone(); }

            }

            string wuz = NewMemberSubCategory;
            if (!string.IsNullOrWhiteSpace(aSubCategory)) NewMemberSubCategory = aSubCategory;

            _rVal = (uopDocCalculation)Add((uopDocument)_rVal);
            NewMemberSubCategory = wuz;

            return _rVal;

        }

        /// <summary>
        /// //#1the name to assign to the drawing
        ///#2the string used to select the drawing from a list
        ///#3the type of drawing to add
        ///#4flag to add the new drawing to the beginning of the collection
        ///#5the part index to assign to the drawing
        ///^shorthand way to add a drawing without the code to create one and addit
        ///~bad input like empty required strings will cause the drawing not to be added to the collection
        ///~but no error is raised.
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="aDrawingName"></param>
        /// <param name="aSelectText"></param>
        /// <param name="aDrawingType"></param>
        /// <param name="aBorderSize"></param>
        /// <param name="aPartIndex"></param>
        /// <param name="bHidden"></param>
        /// <param name="aCategory"></param>
        /// <param name="bCancelable"></param>
        /// <param name="aPartType"></param>
        /// <param name="aFlag"></param>
        /// <param name="bOppositeHand"></param>
        /// <param name="bQuestionsAreTrayPrompt"></param>
        /// <param name="aQuestions"></param>
        /// <returns></returns>
        public uopDocDrawing AddDrawing(uppDrawingFamily aDrawingFamily, uopPart aPart, string aDrawingName, string aSelectText, uppDrawingTypes aDrawingType,
                uppBorderSizes aBorderSize = uppBorderSizes.Undefined, int? aPartIndex = null, bool bHidden = false,
                string aCategory = "", bool bCancelable = false, uppPartTypes aPartType = uppPartTypes.Undefined,
                string aFlag = "", bool bOppositeHand = false, bool bQuestionsAreTrayPrompt = false, mzQuestions aQuestions = null,
                bool bProjectWide = false, int aSheetIndex = 0, bool bZoomExents = true, uppUnitFamilies aUnits = uppUnitFamilies.Undefined,
                uopTrayAssembly aAssy = null, uopTrayRange aRange = null, string aSubCat = "")
        {


            if ((int)aDrawingType < 1 && !uopUtils.RunningInIDE) return null;



            uopDocDrawing _rVal = new uopDocDrawing(aDrawingFamily, aDrawingType, aDrawingName, aSelectText, aPart, aBorderSize, aPartIndex, bHidden, aCategory, bCancelable, aPartType, aFlag, bOppositeHand, bQuestionsAreTrayPrompt, aQuestions, bProjectWide, aSheetIndex, bZoomExents, aUnits, aAssy, aRange, aSubCat);



            string wuz = NewMemberCategory;
            if (!string.IsNullOrWhiteSpace(aCategory)) NewMemberCategory = string.Empty;

            _rVal = (uopDocDrawing)Add(_rVal);
            NewMemberCategory = wuz;

            return _rVal;

        }

        /// <summary>
        /// used to add a uopDocReport to the passed collection
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="wType"></param>
        /// <param name="aReportName"></param>
        /// <param name="bIsRequested"></param>
        /// <param name="aUnits"></param>
        /// <param name="aFileName"></param>
        /// <param name="FolderName_UNUSED"></param>
        /// <param name="bUnitsLocked"></param>
        /// <param name="aFileNameLocked"></param>
        /// <param name="bFolderNameLocked"></param>
        /// <param name="bProtectSheets"></param>
        /// <param name="bAllRangesOnly"></param>
        /// <param name="bAllPagesOnly"></param>
        /// <param name="bMaintainRevisionHistory"></param>
        /// <param name="aCategory"></param>
        /// <param name="aSubCategory"></param>
        /// <param name="bLookForMechTemplate"></param>
        /// <param name="aTemplateName"></param>
        /// <returns></returns>
        public uopDocReport AddReport(uopPart aPart, uppReportTypes wType = uppReportTypes.ReportPlaceHolder, string aReportName = "", bool bIsRequested = false, uppUnitFamilies aUnits = uppUnitFamilies.English, string aFileName = "", string FolderName = "", bool bUnitsLocked = false, bool aFileNameLocked = false, bool bFolderNameLocked = false, bool bProtectSheets = false, bool bAllRangesOnly = false, bool bAllPagesOnly = false, bool bMaintainRevisionHistory = false, string aCategory = "", string aSubCategory = "", bool bLookForMechTemplate = false, dynamic aTemplateName = null)
        {



            uopDocReport _rVal = new uopDocReport()
            {
                Part = aPart,
                ReportName = aReportName,
                TemplateName = aReportName,
                ReportType = wType,
                Units = aUnits,
                IsRequested = bIsRequested,
                FileName = aFileName,
                UnitsLocked = bUnitsLocked,
                FileNameLocked = aFileNameLocked,
                ProtectSheets = bProtectSheets,
                FolderName = FolderName,
                FolderNameLocked = bFolderNameLocked,
                AllRangesOnly = bAllRangesOnly,
                AllPagesOnly = bAllPagesOnly,
                MaintainRevisionHistory = bMaintainRevisionHistory,
                MechanicalTemplate = bLookForMechTemplate
            };

            _rVal.Category = string.IsNullOrWhiteSpace(aCategory) ? NewMemberCategory : aCategory;
            _rVal.SubCategory = string.IsNullOrWhiteSpace(aSubCategory) ? NewMemberSubCategory : aSubCategory;

            if (!string.IsNullOrEmpty(aTemplateName)) _rVal.TemplateName = aTemplateName.Trim();

            _rVal = (uopDocReport)Add(_rVal);



            return _rVal;
        }

        /// <summary>
        /// used to add a uopDocWarning to the passed collection
        /// </summary>
        /// <param name="aPart"></param>
        /// <param name="aBrief"></param>
        /// <param name="aTextString"></param>
        /// <param name="wType"></param>
        /// <param name="aOwnerName"></param>
        /// <param name="aCategory"></param>
        /// <param name="aSubCategory"></param>
        /// <returns></returns>
        public uopDocWarning AddWarning(uopPart aPart, string aBrief = "", string aTextString = "", uppWarningTypes wType = uppWarningTypes.General, string aOwnerName = "", string aCategory = "", string aSubCategory = "")
        {


            if (string.IsNullOrWhiteSpace(aTextString)) return null;


            aTextString = aTextString.Trim();
            aOwnerName = string.IsNullOrWhiteSpace(aOwnerName) ? "" : aOwnerName.Trim();
            if (string.IsNullOrWhiteSpace(aOwnerName) && aPart != null)
                aOwnerName = aPart.PartPath(true);

            aBrief = string.IsNullOrWhiteSpace(aBrief) ? string.Empty : aBrief.Trim();
            if (!string.IsNullOrWhiteSpace(NewMemberOwnerName)) aOwnerName = NewMemberOwnerName;

            uopDocWarning _rVal = new uopDocWarning()
            {
                Owner = aOwnerName,
                Brief = aBrief,
                TextString = aTextString,
                WarningType = wType,
                Category = aCategory,
                SubCategory = aSubCategory,
                Part = aPart

            };

            string wuz = NewMemberSubCategory;
            if (!string.IsNullOrWhiteSpace(aSubCategory)) NewMemberSubCategory = aSubCategory;

            Add((uopDocument)_rVal);
            NewMemberSubCategory = wuz;

            return _rVal;
        }

        public uopDocWarning AddWarning(Exception aException, System.Reflection.MethodBase aMethod = null, string aBrief = null, string aTextSuffix = "")
        {
            if (aException == null) return null;
            return AddWarning(null, aBrief == null ? "Unexpected Exception" : aBrief, $"ERROR : {aException}{aTextSuffix}", uppWarningTypes.Exception, aMethod == null ? string.Empty : aMethod.Name);
        }
        public virtual void Append(IEnumerable<uopDocument> aDocuments, bool bAddClones = false, uppDocumentTypes? aFilter = null)
        {
            if (aDocuments == null) return;
            foreach (uopDocument doc in aDocuments)
            {
                if (doc == null) continue;
                if (aFilter.HasValue && aFilter.Value != doc.DocumentType) continue;
                uopDocument mem = bAddClones ? doc.Clone() : doc;
                Add(mem);
            }
        }


        /// <summary>
        /// the calculations in the collection
        /// </summary>
        public uopDocuments Calculations => GetByDocumentType(uppDocumentTypes.Calculation);



        /// <summary>
        /// returns a collection of the categories of the member parts
        /// </summary>
        /// <param name="bUniqueValues"></param>
        /// <param name="bIncludeNullString"></param>
        /// <returns></returns>
        public List<string> Categories(bool bUniqueValues = true, bool bIncludeNullString = false, List<uopDocument> aSearchList = null, bool bReturnUpperCase = false)
        {
            List<string> _rVal = new List<string>();
            aSearchList ??= this;
            for (int i = 0; i < aSearchList.Count; i++)
            {
                uopDocument aMem = aSearchList[i];
                string aCat = aMem.Category;
                aCat ??= string.Empty;
                aCat = aCat.Trim();
                if (bReturnUpperCase) aCat = aCat.ToUpper();
                if (!bIncludeNullString && aCat == string.Empty) continue;
                if (bUniqueValues)
                {
                    if (_rVal.FindIndex(x => string.Compare(x, aCat, true) == 0) >= 0) continue;
                }
                _rVal.Add(aCat);

            }
            return _rVal;
        }

        /// <summary>
        /// returns a collection of the categories of the member parts
        /// </summary>
        /// <param name="bUniqueValues"></param>
        /// <param name="bIncludeNullString"></param>
        /// <returns></returns>
        public List<string> SubCategories(bool bUniqueValues = true, bool bIncludeNullString = false, List<uopDocument> aSearchList = null)
        {
            List<string> _rVal = new List<string>();
            aSearchList ??= this;
            for (int i = 0; i < aSearchList.Count; i++)
            {
                uopDocument aMem = aSearchList[i];
                string aCat = aMem.SubCategory;
                aCat ??= string.Empty;
                aCat = aCat.Trim();
                if (!bIncludeNullString && aCat == string.Empty) continue;
                if (bUniqueValues)
                {
                    if (_rVal.FindIndex(x => string.Compare(x, aCat, true) == 0) >= 0) continue;
                }
                _rVal.Add(aCat);

            }
            return _rVal;
        }


        public uopDocument GetDocument(uppDocumentTypes aDocType, string aRangeGUID, string aCategory, string aSubCategory, string aSelectName)
        {
            uopDocument _rVal = null;

            uopDocument aMem;

            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (aMem.DocumentType == aDocType)
                {
                    if (aMem.RangeGUID == aRangeGUID)
                    {
                        if (string.Compare(aMem.Category, aCategory, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            if (string.Compare(aMem.SubCategory, aSubCategory, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                if (string.Compare(aMem.SelectName, aSelectName, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    _rVal = aMem;
                                    break;
                                }
                            }
                        }
                    }
                }

            }


            return _rVal;
        }

        public uopDocument GetDrawing(string aRangeGUID, string aCategory, string aSubCategory, string aSelectName)
        {
            uopDocument _rVal = null;
            uopDocument aMem = null;
            uopDocDrawing aDWG = null;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (aMem.DocumentType == uppDocumentTypes.Drawing)
                {
                    aDWG = (uopDocDrawing)aMem;
                    if (aMem.RangeGUID == aRangeGUID || aDWG.RequiresSelections)
                    {
                        if (string.Compare(aMem.Category, aCategory, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            if (string.Compare(aMem.SubCategory, aSubCategory, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                if (string.Compare(aMem.SelectName, aSelectName, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    _rVal = aMem;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return _rVal;
        }
        public uopDocument GetCalulation(string aRangeGUID, string aCategory, string aSubCategory, string aSelectName)
        {
            uopDocument _rVal = null;
            uopDocument aMem = null;
            uopDocCalculation aCalc = null;
            for (int i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (aMem.DocumentType == uppDocumentTypes.Calculation)
                {
                    aCalc = (uopDocCalculation)aMem;
                    if (aMem.RangeGUID == aRangeGUID || aCalc.RequiresSelections)
                    {
                        if (string.Compare(aMem.Category, aCategory, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            if (string.Compare(aMem.SubCategory, aSubCategory, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                if (string.Compare(aMem.SelectName, aSelectName, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    _rVal = aMem;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return _rVal;
        }


        public Collection<uopDocument> ToCollection
        {
            get
            {
                Collection<uopDocument> _rVal = new Collection<uopDocument>();
                for (int i = 1; i <= Count; i++) { _rVal.Add(Item(i)); }
                return _rVal;
            }
        }

        public List<object> SortByCategories(uppDocumentTypes aDocType, bool bReturnPlaceHolders)
        {
            List<object> SortByCategories = null;
            string aCat = string.Empty;
            string bCat = string.Empty;
            List<uopDocument> aCatsDocs = null;
            string catLst = string.Empty;
            uopDocument aMem = null;
            uopDocument bMem = null;
            int i = 0;
            int j = 0;
            SortByCategories = new List<object>();
            aCatsDocs = new List<uopDocument>();

            //first get the ones with no category set
            for (i = 0; i < Count; i++)
            {
                aMem = this[i];
                if (aDocType == uppDocumentTypes.Undefined || aMem.DocumentType == aDocType)
                {
                    if (bReturnPlaceHolders || (!bReturnPlaceHolders && !aMem.IsPlaceHolder))
                    {
                        if (aMem.Category == string.Empty)
                        {
                            aCatsDocs.Add(aMem);
                        }
                    }
                }
            }
            if (aCatsDocs.Count > 0)
            {
                SortByCategories.Add(aCatsDocs);
            }

            for (i = 1; i <= Count; i++)
            {
                aMem = Item(i);
                if (aDocType == uppDocumentTypes.Undefined || aMem.DocumentType == aDocType)
                {
                    if (bReturnPlaceHolders || (!bReturnPlaceHolders && !aMem.IsPlaceHolder))
                    {
                        if (aMem.Category != string.Empty)
                        {
                            aCat = aMem.Category + aMem.SubCategory;

                            //TODO
                            catLst = catLst.Replace(uopGlobals.subDelim, string.Empty);
                            if (!mzUtils.ListContains(aCat, catLst, uopGlobals.Delim))
                            {
                                mzUtils.ListAdd(ref catLst, aCat);
                                aCatsDocs = new List<uopDocument>
                                {
                                    aMem
                                };
                                for (j = i + 1; j < Count; j++)
                                {
                                    bMem = this[j];
                                    if (aDocType == uppDocumentTypes.Undefined || bMem.DocumentType == aDocType)
                                    {
                                        if (bReturnPlaceHolders || (!bReturnPlaceHolders && !bMem.IsPlaceHolder))
                                        {
                                            bCat = bMem.Category + uopGlobals.subDelim + bMem.SubCategory;
                                            if (string.Compare(bCat, aCat, StringComparison.OrdinalIgnoreCase) == 0)
                                            {
                                                aCatsDocs.Add(bMem);
                                            }
                                        }
                                    }
                                }
                                SortByCategories.Add(aCatsDocs);
                            }

                        }
                    }

                }

            }


            return SortByCategories;
        }

        public uopDocuments Clone(bool bReturnEmpty = false)
        {

            uopDocuments _rVal = new uopDocuments
            {
                InvalidWhenEmpty = _InvalidWhenEmpty,
                NewMemberCategory = NewMemberCategory,
                NewMemberSubCategory = NewMemberSubCategory,
                NewMemberOwnerName = NewMemberOwnerName,
                NewMemberRangeGUID = NewMemberRangeGUID,
                Invalid = _Invalid
            };
            if (bReturnEmpty) return _rVal;
            for (int i = 1; i <= Count; i++)
            {
                _rVal.Add(Item(i), true);
            }
            return _rVal;

        }
        //the drawings in the collection
        public List<uopDocDrawing> Drawings => base.FindAll((x) => x.DocumentType == uppDocumentTypes.Drawing).OfType<uopDocDrawing>().ToList();



        public List<uopDocument> GetByCategory(string aCategory, string aSubCategory = null, uppDocumentTypes? aDocType = null, bool? aHiddenFilters = null, bool bRemove = false, List<uopDocument> aSearchList = null)
        {
            if (aCategory == null) return new List<uopDocument>();
            aSearchList ??= this;
            List<uopDocument> _rVal = aDocType.HasValue ? aSearchList.FindAll(x => x.DocumentType == aDocType.Value && string.Compare(x.Category, aCategory, true) == 0) : aSearchList.FindAll(x => string.Compare(x.Category, aCategory, true) == 0);


            if (aSubCategory != null) _rVal = _rVal.FindAll(x => string.Compare(aSubCategory, x.SubCategory, true) == 0);
            if (aHiddenFilters.HasValue) _rVal = _rVal.FindAll(x => x.Hidden == aHiddenFilters.Value);

            if (bRemove)
            {
                foreach (uopDocument item in _rVal)
                {
                    aSearchList.Remove(item);
                }


            }
            return _rVal;
        }

        public List<uopDocument> GetBySubCategory(string aSubCategory, string aCategory = null, uppDocumentTypes? aDocType = null, bool? aHiddenFilters = null, bool bRemove = false, List<uopDocument> aSearchList = null)
        {
            aSearchList ??= this;
            if (aSubCategory == null) return new List<uopDocument>();
            List<uopDocument> _rVal = aDocType.HasValue ? aSearchList.FindAll(x => x.DocumentType == aDocType.Value && string.Compare(x.SubCategory, aSubCategory, true) == 0) : aSearchList.FindAll(x => string.Compare(x.SubCategory, aSubCategory, true) == 0);


            if (aCategory != null) _rVal = _rVal.FindAll(x => string.Compare(aCategory, x.Category, true) == 0);
            if (aHiddenFilters.HasValue) _rVal = _rVal.FindAll(x => x.Hidden == aHiddenFilters.Value);

            if (bRemove)
            {
                foreach (uopDocument item in _rVal) { aSearchList.Remove(item); }

            }
            return _rVal;
        }



        public uopDocuments GetByDocumentType(uppDocumentTypes aDocType, bool bReturnPlaceHolders = true, string aRangeGUID = "", bool bReturnHidden = false, bool bReturnClones = false, bool bRemove = false)
        {
            uopDocuments _rVal = new uopDocuments();
            uopDocument aMem;
            List<int> ids = new List<int>();
            for (int i = 0; i < Count; i++)
            {
                aMem = this[i];
                if (aMem != null)
                {
                    if (aMem.DocumentType == aDocType)
                    {
                        if (bReturnPlaceHolders || (!bReturnPlaceHolders && !aMem.IsPlaceHolder))
                        {
                            if (aRangeGUID == string.Empty || (aRangeGUID != string.Empty & aMem.RangeGUID == aRangeGUID))
                            {
                                if (bReturnHidden || (!bReturnHidden && !aMem.Hidden))
                                {
                                    _rVal.Add(aMem, bAddClone: bReturnClones);
                                    ids.Add(i);
                                }

                            }

                        }
                    }
                }

            }

            if (bRemove && ids.Count > 0)
            {
                for (int i = ids.Count; i >= 1; i--)
                {
                    Remove(ids[i - 1]);
                }
            }
            return _rVal;
        }


        public uopDocuments GetByRangeGUID(string aRangeGUID, uppDocumentTypes aDocType = uppDocumentTypes.Undefined)
        {
            uopDocuments getByRangeGUID = null;
            getByRangeGUID = new uopDocuments();
            uopDocument aMem = null;
            for (int i = 0; i < Count; i++)
            {
                aMem = this[i];
                if (aMem.RangeGUID == aRangeGUID)
                {
                    if (aDocType == uppDocumentTypes.Undefined || aMem.DocumentType == aDocType)
                    {
                        getByRangeGUID.Add(aMem);
                    }
                }
            }
            return getByRangeGUID;
        }

        public uopDocument GetBySelectName(string aSelectName, uppDocumentTypes aDocType = uppDocumentTypes.Undefined)
        {
            uopDocument getBySelectName = null;
            uopDocument aMem = null;
            for (int i = 0; i < Count; i++)
            {
                aMem = this[i];
                if (string.Compare(aMem.SelectName, aSelectName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (aDocType == uppDocumentTypes.Undefined)
                    {
                        getBySelectName = aMem;
                        break;
                    }
                    else
                    {
                        if (aMem.DocumentType == aDocType)
                        {
                            getBySelectName = aMem;
                            break;
                        }
                    }
                }
            }
            return getBySelectName;
        }

        public uopDocDrawing GetDrawingByType(uppDrawingTypes aDrawingType, bool bReturnClone = false, dynamic aPasteAddress = null)
        {
            uopDocDrawing _rVal = null;
            uopDocument aMem = null;
            uopDocDrawing aDWG = null;
            for (int i = 0; i < Count; i++)
            {
                aMem = this[i];
                if (aMem.DocumentType == uppDocumentTypes.Drawing)
                {
                    aDWG = (uopDocDrawing)aMem;
                    if (aDWG.DrawingType == aDrawingType)
                    {
                        _rVal = (!bReturnClone) ? aDWG : aDWG.Clone();

                        if (aPasteAddress != null)
                        {
                            _rVal.PasteAddress = aPasteAddress.toString();
                        }
                        break;
                    }
                }
            }
            return _rVal;
        }

        public uopDocument GetSelectedDocument(uppDocumentTypes aDocType = uppDocumentTypes.Undefined)
        {
            uopDocument getSelectedDocument = null;
            uopDocument aMem = null;
            for (int i = 0; i < Count; i++)
            {
                aMem = this[i];
                if (aMem.Selected)
                {
                    if (aDocType == uppDocumentTypes.Undefined)
                    {
                        getSelectedDocument = aMem;
                        break;
                    }
                    else
                    {
                        if (aMem.DocumentType == aDocType)
                        {
                            getSelectedDocument = aMem;
                            break;
                        }
                    }
                }
            }

            if (getSelectedDocument == null && Count > 0)
            {
                getSelectedDocument = SetSelectedDocument(1, aDocType); //TODO first argument of SetSelectedDocument
            }
            return getSelectedDocument;
        }


        private bool _Invalid = false;
        public bool Invalid
        {
            get => _Invalid || (_InvalidWhenEmpty && Count == 0);

            set
            {
                uopDocument doc;
                if (_Invalid != value)
                {
                    _Invalid = value;
                    for (int i = 1; i <= Count; i++)
                    {
                        doc = this[i - 1];
                        if (doc != null) doc.Invalid = value;
                    }
                }
            }
        }
        public bool InvalidWhenEmpty { get => _InvalidWhenEmpty; set => _InvalidWhenEmpty = value; }

        /// <summary>
        ///#1 then index or part path of the requested part
        ///^returns the item from the collection at the requested index
        /// </summary>
        /// <param name="Index"></param>
        /// <param name="aDocumentType"></param>
        /// <returns></returns>
        public uopDocument Item(dynamic Index, uppDocumentTypes aDocumentType = uppDocumentTypes.Undefined)
        {
            uopDocument _rVal = null;

            if (mzUtils.IsNumeric(Index))
            {
                int idx = mzUtils.VarToInteger(Index);
                if (aDocumentType == uppDocumentTypes.Undefined)
                {
                    if (idx >= 1 & idx <= Count)
                    {
                        this[idx - 1].Index = idx;
                        _rVal = this[idx - 1];
                    }
                    else
                    {
                        throw new IndexOutOfRangeException();
                    }
                }
                else
                {
                    int cnt = 0;
                    for (int i = 0; i < Count; i++)
                    {
                        uopDocument aMem = this[i];
                        aMem.Index = i + 1;
                        if (aMem.DocumentType == aDocumentType)
                        {
                            cnt += 1;
                            if (cnt == idx)
                            {
                                _rVal = aMem;
                                break;
                            }
                        }
                    }
                }
                return _rVal;
            }

            //string aPth  = Convert.ToString(Index);
            //for (int i = 0; i < Count; i++)
            //{
            //    this[i].Index = i + 1;
            //    uopDocument aMem = this[i];
            //    if (string.Compare(aMem.PartPath, aPth, true) == 0)
            //    {
            //        _rVal = aMem;
            //        break;
            //    }
            //}

            return _rVal;
        }
        public uopDocument LastDocument => (Count > 0) ? Item(Count) : null;

        public string NewMemberCategory { get; set; } = string.Empty;
        public string NewMemberSubCategory { get; set; } = string.Empty;
        public string NewMemberRangeGUID { get; set; } = string.Empty;
        public string NewMemberOwnerName { get; set; } = string.Empty;

        private uppUnitFamilies _NewDrawingUnits = uppUnitFamilies.Undefined;
        private bool disposedValue;

        public uppUnitFamilies NewDrawingUnits
        {
            get { return _NewDrawingUnits; }
            set { _NewDrawingUnits = value; }
        }


        public List<string> NodePaths
        {
            get
            {

                List<string> _rVal = new List<string>();
                uopDocument aMem = null;
                for (int i = 1; i <= Count; i++)
                {
                    aMem = Item(i);
                    _rVal.Add(aMem.NodePath);
                }
                return _rVal;
            }
        }

        public uopDocuments PlaceHolders(bool PlaceHolderValue)
        {
            uopDocuments _rVal = new uopDocuments();
            uopDocument aDocument;
            for (int i = 1; i <= Count; i++)
            {
                aDocument = Item(i);
                if (aDocument.IsPlaceHolder == PlaceHolderValue) _rVal.Add(aDocument);

            }
            return _rVal;
        }

        /// <summary>
        /// removes the item from the collection at the requested index
        /// </summary>
        /// <param name="Index"></param>
        public void Remove(int aIndex)
        {
            uopDocument aDoc = null;

            if (aIndex > 0 & aIndex <= Count)
            {
                aDoc = Item(aIndex);
                this.RemoveAt(aIndex - 1);
            }
        }

        public void RemoveByType(uppDocumentTypes aDocType)
        {

            uopDocument aDoc = null;
            for (int i = Count; i >= 1; i--)
            {
                aDoc = Item(i);
                if (aDoc.DocumentType == aDocType)
                {
                    Remove(i);
                }
            }
        }


        /// <summary>
        /// the reports in the collection
        /// </summary>
        public uopDocuments Reports
        {
            get
            {
                return GetByDocumentType(uppDocumentTypes.Report);
            }
        }

        public void SetCategory(string aCategory, dynamic aStartAt = null, dynamic aEndAt = null)
        {
            int iStart = 1;
            int iEnd = Count;
            mzUtils.GetLoopIndices(1, Count, aStartAt, aEndAt, out iStart, out iEnd);

            for (int i = iStart; i <= iEnd; i++)
            {
                Item(i).Category = aCategory;
            }
        }

        public void SetSubCategory(string aSubCategory, dynamic aStartAt = null, dynamic aEndAt = null)
        {
            int iStart = 1;
            int iEnd = Count;
            mzUtils.GetLoopIndices(1, Count, aStartAt, aEndAt, out iStart, out iEnd);

            for (int i = iStart; i <= iEnd; i++)
            {
                Item(i).SubCategory = aSubCategory;
            }
        }

        /// <summary>
        /// sets and returns the currently selected part
        /// </summary>
        /// <param name="Index"></param>
        /// <param name="aDocumentType"></param>
        /// <returns></returns>
        public uopDocument SetSelectedDocument(dynamic Index, uppDocumentTypes aDocumentType = uppDocumentTypes.Undefined)
        {
            uopDocument SetSelectedDocument = null;
            SetSelectedDocument = Item(Index, aDocumentType);
            if (SetSelectedDocument == null)
            {
                return SetSelectedDocument;
            }
            uopDocument aDocument = null;
            for (int i = 1; i <= Count; i++)
            {
                aDocument = Item(i);
                aDocument.Selected = aDocument == SetSelectedDocument;
            }
            return SetSelectedDocument;
        }

        /// <summary>
        /// the warnings in the collection
        /// </summary>
        public uopDocuments Warnings
        {
            get
            {
                return GetByDocumentType(uppDocumentTypes.Warning);
            }
        }

        internal virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    uopDocument doc;
                    for (int i = 1; i <= Count; i++)
                    {
                        doc = this[i - 1];
                        if (doc != null) doc.Dispose(true);
                    }
                    Clear();
                }


                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }



        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
