using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using UOP.WinTray.Projects.Structures;

namespace UOP.WinTray.Projects.Utilities
{
    public class mzQuestions :List<mzQuestion>, IEnumerable<mzQuestion>,  IDisposable, ICloneable
    {
        #region Fields

        private bool disposedValue;

        #endregion Fields

        #region Constructors

        public mzQuestions(string aTitle) { base.Clear(); Title = aTitle != null ? aTitle : ""; }

        public mzQuestions() { base.Clear(); Title = string.Empty; }

        internal mzQuestions(TQUESTIONS aStructure) 
        {

            base.Clear();
            Title = aStructure.Title;
            for (int i = 1; i <= aStructure.Count; i++)
            {
                Add(new mzQuestion(aStructure.Item(i)));
            }
        }

        public mzQuestions(mzQuestions aQuestions) 
        {
            Title = string.Empty;
            if (aQuestions == null) return;
            Title = aQuestions.Title;
            foreach (mzQuestion aQuestion in aQuestions) { Add(new mzQuestion(aQuestion)); } 
          
        }

        #endregion Constructors

        #region Properties

        public string Title { get; set; }

        #endregion Properties

        #region Methods

        public mzQuestions Clone() => new mzQuestions(this);


        public mzQuestion Add(string aPrompt, uopQueryTypes aType, dynamic aInitialAnswer = null, string aChoices = "", bool bAnswerRequired = false,
                                       string aChoiceDelimiter = ",", string aChoiceSubDelimiter = "", string aHeaders = "", int aMinChoiceCount = -1, int aMaxLengthOrWholeDigits = 0,
                                       int aMaxDecis = 4, uopValueControls aValueControl = new uopValueControls(), double? aMaxValue = null,
                                       double? aMinValue = null, string aColumnWidths = "", string aSuffix = "", double? aDisplayMultiplier = null, bool bShowAllDigits = false,
                                       bool bAddMirrors = false, double aMinDifference = 0, string aTag = "", string aToolTip = "")
        {


            if (string.IsNullOrWhiteSpace(aPrompt))
                aPrompt = $"Question { Count + 1}";

            TQUESTION aQst = TQUESTION.Create(aPrompt, aType, aInitialAnswer, aChoices, bAnswerRequired, aChoiceDelimiter, aChoiceSubDelimiter, aHeaders, aMinChoiceCount, aMaxLengthOrWholeDigits, aMaxDecis, aValueControl, aMaxValue, aMinValue, aColumnWidths, aSuffix, aDisplayMultiplier, bShowAllDigits, bAddMirrors, aMinDifference, aTag, aToolTip);
            if (aQst.QType == uopQueryTypes.Undefined) return null;
            aQst.Index = Count + 1;

            return Add(new mzQuestion(aQst));
        }

        public new mzQuestion Add(mzQuestion aQuestion)
        {
            if (aQuestion == null) return null;
            if (aQuestion.QuestionType == uopQueryTypes.Undefined) return null;
            aQuestion.Index = Count + 1;
            base.Add(aQuestion);
            return aQuestion;
        }

        public mzQuestion Item(int aIndex) => (aIndex < 1 || aIndex > Count) ? null :base[aIndex - 1];

        public uopQueryTypes QuestionType(int aIndex) => (aIndex < 1 || aIndex > Count) ? uopQueryTypes.Undefined : base[aIndex - 1].QuestionType;



        public mzQuestion Item(string aPrompt)
        {
            return string.IsNullOrWhiteSpace(aPrompt) ? null : GetByPrompt(aPrompt);
        }


        public mzQuestion GetByPrompt(string aPrompt)
        {
            if (string.IsNullOrWhiteSpace(aPrompt)) return null;

            mzQuestion _rVal = Find((x) => string.Compare(x.Prompt,aPrompt,true) == 0);
            if (_rVal == null) return null;

            if ((object)_rVal.Answer == default) _rVal.Answer = null;

            if ((object)_rVal.LastAnswer == default) _rVal.LastAnswer = _rVal.Answer;

            if (_rVal.QuestionType == uopQueryTypes.YesNo)
            {
                if (_rVal.Answer is bool) _rVal.Answer = mzUtils.VarToBoolean(_rVal.Answer);

                if (_rVal.LastAnswer is bool) _rVal.Answer = mzUtils.VarToBoolean(_rVal.LastAnswer);

            }
            return _rVal;
        }

        public mzQuestion AddStringChoice(string aPrompt, string aChoices, object aInitialAnswer = null, bool bAnswerRequired = true, string aChoiceDelimiter = ",", string aInputWidthPct = "", string aToolTip = "")
        {
            return Add(aPrompt, uopQueryTypes.StringChoice, aInitialAnswer, aChoices, bAnswerRequired, aChoiceDelimiter, aColumnWidths: aInputWidthPct, aToolTip:aToolTip);
        }

        public mzQuestion AddStringChoice(string aPrompt, List<string> aChoices, object aInitialAnswer = null, bool bAnswerRequired = true, string aChoiceDelimiter = ",", string aInputWidthPct = "", string aToolTip = "")
        {
            return AddStringChoice(aPrompt,  string.Join(aChoiceDelimiter,aChoices) , aInitialAnswer, bAnswerRequired, aChoiceDelimiter, aInputWidthPct, aToolTip);
        }


        public mzQuestion AddDualStringChoice(string aPrompt, string aChoices, object aInitialAnswer = null, bool bAnswerRequired = true, string aChoiceDelimiter = ",", string aSubChoiceDelimiter = "|", string aInputWidthPct = "", string aToolTip = "")
        {
            return Add(aPrompt, uopQueryTypes.DualStringChoice, aInitialAnswer, aChoices, bAnswerRequired, aChoiceDelimiter, aSubChoiceDelimiter, aColumnWidths: aInputWidthPct, aToolTip:aToolTip);
        }

        public mzQuestion AddDualStringChoice(string aPrompt, List<string> aChoices, object aInitialAnswer = null, bool bAnswerRequired = true, string aChoiceDelimiter = ",", string aSubChoiceDelimiter = "|", string aInputWidthPct = "", string aToolTip = "")
        {
            return Add(aPrompt, uopQueryTypes.DualStringChoice, aInitialAnswer, mzUtils.ListToString(aChoices, aChoiceDelimiter), bAnswerRequired, aChoiceDelimiter, aSubChoiceDelimiter, aColumnWidths: aInputWidthPct, aToolTip: aToolTip);
        }


        public mzQuestion AddYesNo(string aPrompt, object aInitialAnswer = null, bool bAnswerRequired = true, string aToolTip = "")
        {
            return Add(aPrompt, uopQueryTypes.YesNo, aInitialAnswer, bAnswerRequired: bAnswerRequired, aToolTip: aToolTip);
        }

        public mzQuestion AddCheckVal(string aPrompt, bool aInitialAnswer = true, string aToolTip = "")
        {
            return Add(aPrompt, uopQueryTypes.CheckVal, aInitialAnswer, aToolTip: aToolTip);
        }

        public mzQuestion AddString(string aPrompt, object aInitialAnswer = null, bool bAnswerRequired = true, int aMaxLength = int.MaxValue, string aInputWidthPct = "", string aToolTip  = "")
        {
            return Add(aPrompt, uopQueryTypes.StringValue, aInitialAnswer, bAnswerRequired: bAnswerRequired, aMaxLengthOrWholeDigits: aMaxLength, aColumnWidths: aInputWidthPct, aToolTip: aToolTip);
        }

        public mzQuestion AddFolder(string aPrompt, object aInitialAnswer = null, bool bAnswerRequired = true, string aInputWidthPct = "", string aTag = "", string aToolTip = "")
        {
            return Add(aPrompt, uopQueryTypes.Folder, aInitialAnswer, bAnswerRequired: bAnswerRequired, aColumnWidths: aInputWidthPct, aTag: aTag, aToolTip:aToolTip);
        }

        public mzQuestion AddNumeric(string aPrompt, object aInitialAnswer, string aSuffix = "", double? aDisplayMultiplier = null, double? aMaxValue = null, double? aMinValue = null, uopValueControls aValueControl = uopValueControls.None, int aMaxWholeDigits = 10, int aMaxDecimals = 4, bool bShowAllDigits = false, string aInputWidthPct = "", string aToolTip = "")
        {
            return Add(aPrompt, uopQueryTypes.NumericValue, aInitialAnswer, bAnswerRequired: aValueControl == uopValueControls.NonZero, aMaxLengthOrWholeDigits: aMaxWholeDigits, aMaxDecis: aMaxDecimals, aValueControl: aValueControl, aMaxValue: aMaxValue, aMinValue: aMinValue, aColumnWidths: aInputWidthPct, aSuffix: aSuffix, aDisplayMultiplier: aDisplayMultiplier, bShowAllDigits: bShowAllDigits, aToolTip: aToolTip);
        }

        public mzQuestion AddNumericList(string aPrompt, object aInitialAnswer, string aSuffix = "", double? aDisplayMultiplier = null, int aMinAnswers = -1, double? aMaxValue = null, double? aMinValue = null, double aMinDifference = 0.0, uopValueControls aValueControl = uopValueControls.None, int aMaxWholeDigits = 10, int aMaxDecimals = 4, bool bAddMirrors = false, bool bShowAllDigits = false, string aInputWidthPct = "", string aToolTip = "")
        {
            return Add(aPrompt, uopQueryTypes.NumericList, aInitialAnswer, bAnswerRequired: aValueControl == uopValueControls.NonZero, aChoiceDelimiter: ",", aMinChoiceCount: aMinAnswers, aMaxLengthOrWholeDigits: aMaxWholeDigits, aMaxDecis: aMaxDecimals, aValueControl: aValueControl, aMaxValue: aMaxValue, aMinValue: aMinValue, aColumnWidths: aInputWidthPct, aSuffix: aSuffix, aDisplayMultiplier: aDisplayMultiplier, bShowAllDigits: bShowAllDigits, bAddMirrors: bAddMirrors, aMinDifference: aMinDifference, aToolTip: aToolTip);
        }

        public mzQuestion AddSingleSelect(string aPrompt, string aChoices, string aInitialAnswer = "", bool bAnswerRequired = false, string aChoiceDelimiter = ",", string aInputWidthPct = "", string aToolTip = "")
        {
            return Add(aPrompt, uopQueryTypes.SingleSelect, aInitialAnswer, aChoices, bAnswerRequired, aChoiceDelimiter, aColumnWidths: aInputWidthPct, aToolTip: aToolTip);
        }

        public mzQuestion AddSingleSelect(string aPrompt, List<string> aChoices, string aInitialAnswer = "", bool bAnswerRequired = false, string aChoiceDelimiter = ",", string aInputWidthPct = "", string aToolTip = "")
        {
            if (aChoices == null) return null;
            return Add(aPrompt, uopQueryTypes.SingleSelect, aInitialAnswer, mzUtils.ListToString( aChoices,aChoiceDelimiter), bAnswerRequired, aChoiceDelimiter, aColumnWidths: aInputWidthPct, aToolTip: aToolTip);
        }


        public mzQuestion AddMultiSelect(string aPrompt, string aChoices, string aInitialAnswer = "", int aMinChoiceCount = -1, string aHeaders = "", bool bAnswerRequired = false, string aChoiceDelimiter = ",", string aChoiceSubDelimiter = "", string aColumnWidths = "", string aToolTip = "")
        {
            return Add(aPrompt, uopQueryTypes.MultiSelect, aInitialAnswer, aChoices, bAnswerRequired, aChoiceDelimiter, aChoiceSubDelimiter, aHeaders, aMinChoiceCount, aColumnWidths: aColumnWidths, aToolTip: aToolTip);
        }

        public mzQuestion AddMultiSelect(string aPrompt, List<string> aChoices, string aInitialAnswer = "", int aMinChoiceCount = -1, string aHeaders = "", bool bAnswerRequired = false, string aChoiceDelimiter = ",", string aChoiceSubDelimiter = "", string aColumnWidths = "", string aToolTip = "")
        {
        
            return Add(aPrompt, uopQueryTypes.MultiSelect, aInitialAnswer, mzUtils.ListToString( aChoices,aDelimitor: aChoiceDelimiter), bAnswerRequired, aChoiceDelimiter, aChoiceSubDelimiter, aHeaders, aMinChoiceCount, aColumnWidths: aColumnWidths, aToolTip: aToolTip);
        }

        public dynamic Answer(int aIndex, dynamic aDefaultAnswer, out dynamic rLastAnswer)
        {
            rLastAnswer = null;

            mzQuestion mem = Item(aIndex);
            dynamic _rVal = aDefaultAnswer;
            if (mem != null)
            {
                rLastAnswer = mem.LastAnswer;
                if (mem.Answer != null) _rVal = mem.Answer;
            }
            return _rVal;

        }

        public dynamic Answer(int aIndex, dynamic aDefaultAnswer)
        {


            mzQuestion mem = Item(aIndex);
            dynamic _rVal = aDefaultAnswer;
            if (mem != null)
            {
                if (mem.Answer != null) _rVal = mem.Answer;
            }
            return _rVal;

        }

        public dynamic Answer(string aPrompt, dynamic aDefaultAnswer, out dynamic rLastAnswer)
        {
            rLastAnswer = null;

            mzQuestion mem = Item(aPrompt);
            dynamic _rVal = aDefaultAnswer;
            if (mem != null)
            {
                rLastAnswer = mem.LastAnswer;
                if (mem.Answer != null) _rVal = mem.Answer;
            }
            return _rVal;

        }

        public dynamic Answer(string aPrompt, dynamic aDefaultAnswer)
        {
            mzQuestion mem = Item(aPrompt);
            dynamic _rVal = aDefaultAnswer;
            if (mem != null)
            {
                if (mem.Answer != null) _rVal = mem.Answer;
            }
            return _rVal;

        }
        public bool AnswerB(string aPrompt, bool aDefaultAnswer)
        {
            mzQuestion mem = Item(aPrompt);
            bool _rVal = aDefaultAnswer;
            if (mem != null)
            {
                if (mem.Answer != null) _rVal = mem.AnswerB;
            }
            return _rVal;

        }
        public string AnswerS(string aPrompt, string aDefaultAnswer)
        {
            mzQuestion mem = Item(aPrompt);
            string _rVal = aDefaultAnswer;
            if (mem != null)
            {
                if (mem.Answer != null) _rVal = mem.AnswerS;
            }
            return _rVal;

        }
        public double AnswerD(string aPrompt, double aDefaultAnswer)
        {
            mzQuestion mem = Item(aPrompt);
            double _rVal = aDefaultAnswer;
            if (mem != null)
            {
                if (mem.Answer != null) _rVal = mem.AnswerD;
            }
            return _rVal;

        }
        public int AnswerI(string aPrompt, int aDefaultAnswer)
        {
            mzQuestion mem = Item(aPrompt);
            int _rVal = aDefaultAnswer;
            if (mem != null)
            {
                if (mem.Answer != null) _rVal = mem.AnswerI;
            }
            return _rVal;

        }

        public bool AnswerB(int aIndex, bool aDefaultAnswer)
        {
            mzQuestion mem = Item(aIndex);
            bool _rVal = aDefaultAnswer;
            if (mem != null)
            {
                if (mem.Answer != null) _rVal = mem.AnswerB;
            }
            return _rVal;

        }
        public string AnswerS(int aIndex, string aDefaultAnswer)
        {
            mzQuestion mem = Item(aIndex);
            string _rVal = aDefaultAnswer;
            if (mem != null)
            {
                if (mem.Answer != null) _rVal = mem.AnswerS;
            }
            return _rVal;

        }
        public double AnswerD(int aIndex, double aDefaultAnswer)
        {
            mzQuestion mem = Item(aIndex);
            double _rVal = aDefaultAnswer;
            if (mem != null)
            {
                if (mem.Answer != null) _rVal = mem.AnswerD;
            }
            return _rVal;

        }
        public int AnswerI(int aIndex, int aDefaultAnswer)
        {
            mzQuestion mem = Item(aIndex);
            int _rVal = aDefaultAnswer;
            if (mem != null)
            {
                if (mem.Answer != null) _rVal = mem.AnswerI;
            }
            return _rVal;

        }

        public bool CompareAnswers(mzQuestions aQuestions, out string rNotEqualIndexs)
        {
            rNotEqualIndexs = string.Empty;
            if (aQuestions == null) return false;

            if (aQuestions.Count != Count) return false;

             mzQuestion myQ;
            mzQuestion herQ;

            for (int i = 1; i <= Count; i++)
            {
                myQ = base[i-1];
                herQ = aQuestions[i -1];
                if (string.Compare(myQ.Answer.ToString(), herQ.Answer.ToString(), true) == 0)
                {
                    mzUtils.ListAdd(ref rNotEqualIndexs, i);
                }
            }
            return rNotEqualIndexs == string.Empty;
        }

        /// <summary>
        /// Clear questions
        /// </summary>
     
        public int GetIndexByPrompt(string aPrompt)
        {
            mzQuestion mem = Item(aPrompt);
            return (mem == null) ? 0 : mem.Index;
        }

        public string SetChoices(int aIndex, mzValues aChoices, string aChoiceDelimiter = "", string aChoiceSubDelimiter = "")
        {
            string _rVal = string.Empty;
            if (aIndex < 1 || aIndex > Count) return _rVal;
            mzQuestion question = Item(aIndex);

            TVALUES aVals = TVALUES.FromDelimitedList(question.Answer, question.ChoiceDelimiter);

            if (aChoices != null)
            {
                question.Choices = aChoices;
            }

            if (!string.IsNullOrWhiteSpace(aChoiceDelimiter))
            {
                question.ChoiceDelimiter = aChoiceDelimiter.Trim();
            }

            if (!string.IsNullOrWhiteSpace(aChoiceSubDelimiter))
            {
                question.ChoiceSubDelimiter = aChoiceSubDelimiter.Trim();
            }

            _rVal = aVals.ToDelimitedList(question.ChoiceDelimiter, true);
            question.Answer = _rVal;


            return _rVal;
        }

        public string SetChoices(string aPrompt, mzValues aChoices, string aChoiceDelimiter = "", string aChoiceSubDelimiter = "")
        {
            string _rVal = string.Empty;
            mzQuestion question = Item(aPrompt);
            if (question == null) return _rVal;
            TVALUES aVals = TVALUES.FromDelimitedList(question.Answer, question.ChoiceDelimiter);

            if (aChoices != null)
            {
                question.Choices = aChoices;
            }

            if (!string.IsNullOrWhiteSpace(aChoiceDelimiter))
            {
                question.ChoiceDelimiter = aChoiceDelimiter.Trim();
            }

            if (!string.IsNullOrWhiteSpace(aChoiceSubDelimiter))
            {
                question.ChoiceSubDelimiter = aChoiceSubDelimiter.Trim();
            }

            _rVal = aVals.ToDelimitedList(question.ChoiceDelimiter, true);
            question.Answer = _rVal;


            return _rVal;
        }

        public bool SetAnswer(int aIndex, dynamic aAnswer, out dynamic rLastAnswer)
        {
            rLastAnswer = null;
            if (aIndex < 1 || aIndex > Count) return false;

            mzQuestion question = Item(aIndex);
            rLastAnswer = question.Answer;

            if (question.QuestionType == uopQueryTypes.MultiSelect)
            {
                TVALUES tvalues = TVALUES.FromDelimitedList(aAnswer, question.ChoiceDelimiter, false, true, true);
                aAnswer = tvalues.ToDelimitedList(question.ChoiceDelimiter, true);
            }
            else
            {
                if (question.QuestionType == uopQueryTypes.YesNo)
                {
                    aAnswer = mzUtils.VarToBoolean(aAnswer);
                }
            }
            question.LastAnswer = question.Answer;
            bool _rVal = question.Answer != aAnswer;
            question.Answer = aAnswer;

            return _rVal;

        }

        public List<string> CurrentAnswers()
        {
            List<string> _rVal = new List<string>();
            foreach (var item in this)
            {
                _rVal.Add(item.AnswerS);
            }

            return _rVal;
        }

        public bool SetAnswer(int aIndex, dynamic aAnswer)
        {
            if (aIndex < 1 || aIndex > Count) return false;

            mzQuestion question = Item(aIndex);

            if (question.QuestionType == uopQueryTypes.MultiSelect)
            {
                TVALUES tvalues = TVALUES.FromDelimitedList(aAnswer, question.ChoiceDelimiter, false, true, true);
                aAnswer = tvalues.ToDelimitedList(question.ChoiceDelimiter, true);
            }
            else
            {
                if (question.QuestionType == uopQueryTypes.YesNo)
                {
                    aAnswer = mzUtils.VarToBoolean(aAnswer);
                }
            }
            question.LastAnswer = question.Answer;
            bool _rVal = question.SetAnswer(aAnswer);
           

            return _rVal;

        }


        public bool SetAnswer(string aPrompt, dynamic aAnswer, out dynamic rLastAnswer)
        {
            rLastAnswer = null;

            mzQuestion question = Item(aPrompt);
            if (question == null) return false;
            rLastAnswer = question.Answer;

            if (question.QuestionType == uopQueryTypes.MultiSelect)
            {
                TVALUES tvalues = TVALUES.FromDelimitedList(aAnswer, question.ChoiceDelimiter, false, true, true);
                aAnswer = tvalues.ToDelimitedList(question.ChoiceDelimiter, true);
            }
            else
            {
                if (question.QuestionType == uopQueryTypes.YesNo)
                {
                    aAnswer = mzUtils.VarToBoolean(aAnswer);
                }
            }
            question.LastAnswer = question.Answer;
            bool _rVal = question.Answer != aAnswer;
            question.Answer = aAnswer;

            return _rVal;

        }


        public bool SetAnswer(string aPrompt, dynamic aAnswer)
        {


            mzQuestion question = Item(aPrompt);
            if (question == null) return false;
            if (question.QuestionType == uopQueryTypes.MultiSelect)
            {
                TVALUES tvalues = TVALUES.FromDelimitedList(aAnswer, question.ChoiceDelimiter, false, true, true);
                aAnswer = tvalues.ToDelimitedList(question.ChoiceDelimiter, true);
            }
            else
            {
                if (question.QuestionType == uopQueryTypes.YesNo)
                {
                    aAnswer = mzUtils.VarToBoolean(aAnswer);
                }
            }
            question.LastAnswer = question.Answer;
            bool _rVal = question.Answer != aAnswer;
            question.Answer = aAnswer;

            return _rVal;

        }


        public void SetFormats(dynamic aIndexOrPrompt, dynamic aDisplayMutiplier = null, dynamic aMaxWhole = null, dynamic aMaxDecimals = null, dynamic aSuffix = null)
        {
            mzQuestion aQ = Item(aIndexOrPrompt);
            if (aQ == null) return;
            if (aDisplayMutiplier != null) aQ.DisplayMultiplier = mzUtils.VarToDouble(aDisplayMutiplier, aDefault: 1);
            if (aMaxWhole != null) aQ.MaxWhole = mzUtils.VarToInteger(aMaxWhole, bAbsoluteVal: true, aMinVal: 0, aMaxVal: 15);
            if (aMaxDecimals != null) aQ.MaxDecimals = mzUtils.VarToInteger(aMaxDecimals, bAbsoluteVal: true, aMinVal: 0, aMaxVal: 10);
            if (aSuffix != null) aQ.Suffix = aSuffix.Trim();
        }
        

        public override string ToString() => $"mzQuestions [{Count}]";

        #endregion Methods

        #region IDisposable Implementation
        internal virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    base.Clear();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~mzQuestions()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        object ICloneable.Clone() => new mzQuestions(this);

        #endregion IDisposable Implementation
    }


}
