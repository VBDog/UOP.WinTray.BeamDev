using System;
using System.Collections.Generic;
using UOP.WinTray.Projects.Structures;

namespace UOP.WinTray.Projects.Utilities
{
    public class mzQuestion: ICloneable 
    {
        private TQUESTION _Struc;

        #region Constructors

        public mzQuestion() { _Struc = new TQUESTION(); }
        public mzQuestion(mzQuestion aQuestion)  {  _Struc = aQuestion == null? new TQUESTION() : aQuestion.Structure; }


        public mzQuestion(uopQueryTypes aType, string aPrompt,  dynamic aInitialAnswer = null, string aChoices = "", bool bAnswerRequired = false,
                                      string aChoiceDelimiter = ",", string aChoiceSubDelimiter = "", string aHeaders = "", int aMinChoiceCount = -1, int aMaxLengthOrWholeDigits = 0,
                                      int aMaxDecis = 4, uopValueControls aValueControl = new uopValueControls(), double? aMaxValue = null,
                                      double? aMinValue = null, string aColumnWidths = "", string aSuffix = "", double? aDisplayMultiplier = null, bool bShowAllDigits = false,
                                      bool bAddMirrors = false, double aMinDifference = 0, string aTag = "", string aToolTip = "")
        { 
        
        

            if (string.IsNullOrWhiteSpace(aPrompt))
                aPrompt = $"Question";

            _Struc = TQUESTION.Create(aPrompt, aType, aInitialAnswer, aChoices, bAnswerRequired, aChoiceDelimiter, aChoiceSubDelimiter, aHeaders, aMinChoiceCount, aMaxLengthOrWholeDigits, aMaxDecis, aValueControl, aMaxValue, aMinValue, aColumnWidths, aSuffix, aDisplayMultiplier, bShowAllDigits, bAddMirrors, aMinDifference, aTag,aToolTip);

        }

        internal mzQuestion(TQUESTION aStructure) { _Struc = aStructure; }

        #endregion Constructors

        #region Properties

        private WeakReference<object> _ViewModel_Ref;
        public  object ViewModel 
        {
            get 
            { 
                if(_ViewModel_Ref == null) return null;
                if(!_ViewModel_Ref.TryGetTarget(out object _rVal)) { _ViewModel_Ref = null; return null; }
                return _rVal;
            }
            set => _ViewModel_Ref = value == null ? null : new WeakReference<object>(value);
            
          }

        public double AnswerD=> (_Struc.Answer == null || (QuestionType != uopQueryTypes.NumericValue && QuestionType != uopQueryTypes.StringValue)) ? 0 : mzUtils.VarToDouble(_Struc.Answer);

        public string AnswerS => (_Struc.Answer == null) ? string.Empty : _Struc.Answer.ToString();
        public bool AnswerB => (_Struc.Answer == null || (QuestionType != uopQueryTypes.YesNo && QuestionType != uopQueryTypes.CheckVal) ) ? false : mzUtils.VarToBoolean( _Struc.Answer);

        public dynamic Answer { get => _Struc.Answer;  set => SetAnswer(value); }

        public int AnswerI => (_Struc.Answer == null || (QuestionType != uopQueryTypes.NumericValue && QuestionType != uopQueryTypes.StringValue)) ? 0 : mzUtils.VarToInteger(_Struc.Answer);


        public List<string> UnacceptableAnswers { get; set; }
        public string UnacceptableAnswerMessage { get; set; }

        public double DisplayMultiplier { get => _Struc.DisplayMultiplier; set { _Struc.DisplayMultiplier = (Math.Abs(value) > 0) ? Math.Abs(value) : 1; } }

        public bool AnswerRequired => _Struc.AnswerRequired; 

        public mzValues Answers
        {
            get
            {
                var ret = new mzValues();
                ret.AddByString( AnswerS ,bNoDupes: true,aDelimitor: _Struc.ChoiceDelimiter,bReturnNulls: false,bTrim: true, bNumbersOnly: _Struc.QType == uopQueryTypes.NumericValue || _Struc.QType == uopQueryTypes.NumericList );
                return ret;
            }
        }
        public string ChoiceSubDelimeter => _Struc.ChoiceSubDelimiter; 

        public mzValues Headers  => new mzValues(_Struc.Headers);
        

        public mzValues ColumnWidths => new mzValues(_Struc.ColumnWidths);
        

        public int MinChoiceCount { get => _Struc.MinChoiceCount; set => _Struc.MinChoiceCount = value; }
        public int MaxChoiceCount { get => _Struc.MaxChoiceCount; set => _Struc.MaxChoiceCount = value; }
        public int MaxChars { get => _Struc.MaxChars; set { value = Math.Abs(value); if (value > 500) value = 500;  _Struc.MaxChars = value; } }
        public int MaxWhole { get => _Struc.MaxWhole; set  => _Struc.MaxWhole = mzUtils.LimitedValue(value, 0, 15, _Struc.MaxWhole); }

        public int MaxDecimals { get => _Struc.MaxDecimals; set => _Struc.MaxDecimals = mzUtils.LimitedValue(value, 0, 10, _Struc.MaxDecimals); }

        public bool ShowAllDigits => _Struc.ShowAllDigits;  
        
        public string  Tag => _Struc.Tag;
        
        public uopValueControls ValueControl => _Struc.ValueControl;
        
        public object MaxAnswer => _Struc.MaxAnswer; 
        
        public object MinAnswer => _Struc.MinAnswer; 

        public string ChoiceDelimiter { get => _Struc.ChoiceDelimiter; set => _Struc.ChoiceSubDelimiter = value; }

        public string ChoiceSubDelimiter { get => _Struc.ChoiceSubDelimiter; set => _Struc.ChoiceSubDelimiter = value; }

        public mzValues Choices { get => new mzValues(_Struc.Choices); set => _Struc.Choices = (value != null) ? new TVALUES(value.Structure) : new TVALUES(""); } 
        
        public int Index { get => _Struc.Index; internal set => _Struc.Index = value; } 
        
        public object LastAnswer { get => _Struc.LastAnswer; internal set => _Struc.LastAnswer = value; }

        public string Prompt => _Struc.Prompt; 

        public string Suffix { get => _Struc.Suffix; set => _Struc.Suffix = value; }

        public string ToolTip { get => _Struc.ToolTip; set => _Struc.ToolTip = value; }

        public bool AddMirrors { get => _Struc.AddMirrors; set => _Struc.AddMirrors = value; }

        public int ColCount { get => _Struc.ColCount; set => _Struc.ColCount = value; }

        public uopQueryTypes QuestionType => _Struc.QType; 

        internal TQUESTION Structure { get => _Struc;  set => _Struc = value; }

        #endregion Properties

        #region Methods

        public bool ValidateNumber(out string rError, dynamic aNumber = null, dynamic aNumericList = null) =>  _Struc.ValidateNumber(out rError, aNumber, aNumericList);

        public List<string> AnswersList(mzSortOrders Sort = mzSortOrders.None) => Answers.ToStringList(Sort);
        
        public List<double> AnswersNumericList(int aPrecis = -1, mzSortOrders Sort = mzSortOrders.None) => Answers.ToNumericList(aPrecis, Sort);


        public void SetFormats(object aDisplayMutiplier, object aMaxWhole, object aMaxDecimals, object aSuffix)
        {
            if (aDisplayMutiplier != null && aDisplayMutiplier is Single) DisplayMultiplier = Convert.ToSingle( aDisplayMutiplier );
            if (aMaxWhole != null && aMaxWhole is int) MaxWhole = Convert.ToInt32( aMaxWhole );
            if (aMaxDecimals != null && aMaxDecimals is int) MaxDecimals = Convert.ToInt32( aMaxDecimals );
            if (aSuffix != null && aSuffix is string) aSuffix = ((string)aSuffix).Trim();
        }

        public bool SetAnswer(dynamic aAnswer)
        {
            uopQueryTypes qtype = QuestionType;
            bool _rVal = false;
            switch (qtype)
            {
                case uopQueryTypes.CheckVal:
                case uopQueryTypes.YesNo:
                    bool oldval = mzUtils.VarToBoolean(_Struc.Answer);
                    _Struc.Answer = oldval;
                    bool newval = mzUtils.VarToBoolean(aAnswer, oldval);
                    _rVal = oldval != newval;
                    if (_rVal)
                    {
                        _Struc.LastAnswer = oldval;
                        _Struc.Answer = newval;
                    }

                    break;
                case uopQueryTypes.StringValue:
                case uopQueryTypes.Folder:
                case uopQueryTypes.SingleSelect:
                    string oldvals = (_Struc.Answer == null)? "": _Struc.Answer.ToString();
                    _Struc.Answer = oldvals;
                    string newvals = (aAnswer == null) ? string.Empty : aAnswer.ToString() ;
                    _rVal = string.Compare( oldvals ,newvals,ignoreCase:true) != 0;
                    if (_rVal)
                    {
                        _Struc.LastAnswer = oldvals;
                        _Struc.Answer = newvals;
                    }

                    break;
                //case uopQueryTypes.DualStringChoice:
                //    string oldvals1 = (_Struc.Answer == null) ? string.Empty : _Struc.Answer.ToString();
                //    _Struc.Answer = oldvals1;
                //    string newvals1 = (aAnswer = null) ? string.Empty : aAnswer.ToString();
                //    _rVal = string.Compare(oldvals1, newvals1, ignoreCase: true) != 0;
                //    if (_rVal)
                //    {
                //        _Struc.LastAnswer = oldvals1;
                //        _Struc.Answer = newvals1;
                //    }

                //    break;

                case uopQueryTypes.NumericValue:
                    double oldvald = mzUtils.VarToDouble(_Struc.Answer);
                    _Struc.Answer = oldvald;
                    double newvald = mzUtils.VarToDouble(aAnswer, aDefault: oldvald);
                    _rVal = oldvald != newvald;
                    if (_rVal)
                    {
                        _Struc.LastAnswer = oldvald;
                        _Struc.Answer = newvald;
                    }

                    break;

                case uopQueryTypes.NumericList:
                    TQUESTION struc = _Struc.Clone();
                    struc.Answer = (aAnswer == null) ? string.Empty : aAnswer.ToString().Trim();
                    if(!TQUESTION.ValidateNumbers(struc, out string err)) return false;
                    _Struc.LastAnswer = _Struc.Answer;
                    _Struc.Answer = struc.Answer;
                    break;
                case uopQueryTypes.DualStringChoice:
                case uopQueryTypes.MultiSelect:
                     TVALUES tvalues = TVALUES.FromDelimitedList(aAnswer, ChoiceDelimiter, false, true, true);
                     string newlist = tvalues.ToDelimitedList(ChoiceDelimiter, true);
                    _rVal = string.Compare(_Struc.Answer, newlist, ignoreCase: true) != 0;
                    if (_rVal)
                    {
                        _Struc.LastAnswer = _Struc.Answer;
                        _Struc.Answer = newlist;
                    }
                    break;
                case uopQueryTypes.StringChoice:
                    string oldvalue = (_Struc.Answer == null) ? string.Empty : _Struc.Answer.ToString();
                    _Struc.Answer = oldvalue;
                    string newvalue = (aAnswer == null) ? string.Empty : aAnswer.ToString();
                    _rVal = string.Compare(oldvalue, newvalue, ignoreCase: true) != 0;
                    if (_rVal)
                    {
                        _Struc.LastAnswer = oldvalue;
                        _Struc.Answer = newvalue;
                    }
                    break;
            }


            return _rVal;
        }

        public override string ToString() => _Struc.ToString().Replace("TQUESTION","mzQuestion");

        /// <summary>
        /// Add new Questions
        /// </summary>
        /// <param name="aPrompt"></param>
        /// <param name="aType"></param>
        /// <param name="aInitialAnswer"></param>
        /// <param name="aChoices"></param>
        /// <param name="bAnswerRequired"></param>
        /// <param name="aChoiceDelimiter"></param>
        /// <param name="aChoiceSubDelimiter"></param>
        /// <param name="aHeaders"></param>
        /// <param name="aMinChoiceCount"></param>
        /// <param name="aMaxLengthOrWholeDigits"></param>
        /// <param name="aMaxDecis"></param>
        /// <param name="aValueControl"></param>
        /// <param name="aMaxValue"></param>
        /// <param name="aMinValue"></param>
        /// <param name="aColumnWidths"></param>
        /// <param name="aSuffix"></param>
        /// <param name="aDisplayMultiplier"></param>
        /// <param name="bShowAllDigits"></param>
        /// <param name="bAddMirrors"></param>
        /// <param name="aMinDifference"></param>
        /// <param name="aTag"></param>
        public static mzQuestion Create(string aPrompt, uopQueryTypes aType, dynamic aInitialAnswer = null, string aChoices = "", bool bAnswerRequired = false,
                                        string aChoiceDelimiter = ",", string aChoiceSubDelimiter = "", string aHeaders = "", int aMinChoiceCount = -1, int aMaxLengthOrWholeDigits = 0,
                                        int aMaxDecis = 4, uopValueControls aValueControl = new uopValueControls(), double? aMaxValue = null,
                                        double? aMinValue = null, string aColumnWidths = "", string aSuffix = "", double? aDisplayMultiplier = null, bool bShowAllDigits = false,
                                        bool bAddMirrors = false, double aMinDifference = 0, string aTag = "", string aToolTip = "")
        {

            return new mzQuestion(TQUESTION.Create(aPrompt,aType,aInitialAnswer,aChoices,bAnswerRequired,aChoiceDelimiter,aChoiceSubDelimiter,aHeaders,aMinChoiceCount,aMaxLengthOrWholeDigits,aMaxDecis,aValueControl,aMaxValue,aMinValue,aColumnWidths,aSuffix,aDisplayMultiplier,bShowAllDigits,bAddMirrors,aMinDifference,aTag, aToolTip));
        }

        
        public mzQuestion Clone() => new mzQuestion(this);

        object ICloneable.Clone() => new mzQuestion(this);

    }

    #endregion Methods
}

