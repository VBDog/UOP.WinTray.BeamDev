using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.UI.ViewModels.UserControls.CADfx.Questions;
using UOP.WinTray.UI.Views.UserControls.CADfx.Questions;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace UOP.WinTray.UI.Views.Windows.CADfx.Questions
{
    /// <summary>
    /// Interaction logic for QuestionsView.xaml
    /// </summary>
    public partial class QuestionsView : Window
    {
        public enum QuestionWindowStatus
        {
            INVALID,
            Valid
        }

        private List<UserControl> _AddedUserControls;
        private mzQuestions _Questions;
        private QuestionWindowStatus? _QuestionWindowStatus;

        public mzQuestions Questions
        {
            get
            {
                if (Validate() == QuestionWindowStatus.Valid)
                {
                    return _Questions;
                }
                else
                {
                    return null;
                }
            }

            set
            {
                _Questions = value;

                PopulateUIComponents();
            }
        }

        private bool _DisAllowCancel;
        public bool DisAllowCancel {
            get => _DisAllowCancel; 
            set
            {
                _DisAllowCancel = value;
                leftButton.Visibility = _DisAllowCancel ? Visibility.Collapsed : Visibility.Visible;

            } 
        }
        public bool IsCanceled { get; private set; }

        private QuestionOptions options;

        public QuestionsView(QuestionOptions options)
        {
            InitializeComponent();
          
            CustomInit(options);
        }

        public QuestionWindowStatus Validate()
        {
            if (!_QuestionWindowStatus.HasValue)
            {
                if (GetUserControlWithInvalidInput() == null)
                {
                    _QuestionWindowStatus = QuestionWindowStatus.Valid;
                }
                else
                {
                    _QuestionWindowStatus = QuestionWindowStatus.INVALID;
                }
            }
            return _QuestionWindowStatus.Value;
        }

        private void CustomInit(QuestionOptions options)
        {
            _Activated = false;
            _AddedUserControls = new List<UserControl>();
            IsCanceled = true;
            this.options = options;
            TextBlock prompt = Prompt;
            prompt.Visibility = Visibility.Collapsed;
            if (options == null) return;

            DisAllowCancel = !options.AllowCancel;
            
            if (!string.IsNullOrWhiteSpace(options.Title))
            {
                prompt.Text = options.Title;
                prompt.Visibility = Visibility.Visible;
                Title = "Questions";
            }
            else
            {
                prompt.Visibility = Visibility.Collapsed;
                Title = options.Title;
            }

            if (!string.IsNullOrWhiteSpace(options.SaveButtonText))
            {
                rightButton.Content = options.SaveButtonText;
            }

        }

        private void AddUserControl(UserControl userControl)
        {
            if (userControl == null) return;
            _AddedUserControls.Add(userControl);

            mainStackPanel.Children.Add(userControl);
            //mainScoller.ActualHeight += userControl.ActualHeight;
        }

        private void PopulateUIComponents()
        {
            if (_Questions == null || _Questions.Count == 0)
            {
                return;
            }
            foreach (mzQuestion question in _Questions)
            {
                switch (question.QuestionType)
                {
                    case uopQueryTypes.YesNo:
                        AddQuestion<YesNoQuestionViewModel, YesNoQuestionView>(question);
                        break;
                    case uopQueryTypes.SingleSelect:
                        AddQuestion<SingleSelectQuestionViewModel, SingleSelectQuestionView>(question);
                        break;
                    case uopQueryTypes.MultiSelect:
                        AddQuestion<MultiSelectQuestionViewModel, MultiSelectQuestionView>(question);
                        break;
                    case uopQueryTypes.StringValue:
                        AddQuestion<StringValueQuestionViewModel, StringValueQuestionView>(question);
                        break;
                    case uopQueryTypes.NumericValue:
                        AddQuestion<NumericValueQuestionViewModel, NumericValueQuestionView>(question);
                        break;
                    case uopQueryTypes.StringChoice:
                        AddQuestion<StringChoiceQuestionViewModel, StringChoiceQuestionView>(question);
                        break;
                    case uopQueryTypes.CheckVal:
                        AddQuestion<CheckValQuestionViewModel, CheckValQuestionView>(question);
                        break;
                    case uopQueryTypes.NumericList:
                        AddQuestion<NumericListQuestionViewModel, NumericListQuestionView>(question);
                        break;
                    case uopQueryTypes.Folder:
                        break;
                    case uopQueryTypes.DualStringChoice:
                        AddQuestion<DualStringChoiceQuestionViewModel, DualStringChoiceQuestionView>(question);
                        break;
                    default:
                        break;
                }

                

            }
        
        }

        private U AddQuestion<V,U>(mzQuestion question) 
            where V:IQuestionViewModel, new()
            where U:UserControl, new()
        {
            U userControl = new();
            V viewModel = new();
            viewModel.ConfigureDataContext(question, userControl, options);
            userControl.DataContext = viewModel;
            question.ViewModel = viewModel;
            AddUserControl(userControl);

            return userControl;
        }

        private void leftButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void rightButton_Click(object sender, RoutedEventArgs e)
        {
            UserControl invalidUC = GetUserControlWithInvalidInput();
            if (invalidUC == null)
            {
                SetAnswers();
                _QuestionWindowStatus = QuestionWindowStatus.Valid;
                IsCanceled = false;
                Close();
            }
            else
            {
                (invalidUC.DataContext as IQuestionViewModel).InformAboutInvalidInput();
            }
        }

        private UserControl GetUserControlWithInvalidInput()
        {
            UserControl invalid = null;
            string errorMessage;
            foreach (var uc in _AddedUserControls)
            {
                errorMessage = (uc.DataContext as IQuestionViewModel).ValidateInput();
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    invalid = uc;
                    break;
                }
            }
            return invalid;
        }

        // This method obtains the answers from usercontrols and set them in input mzQuestions object
        private void SetAnswers()
        {
            mzQuestion answerFromUC;
            for (int i = 0; i < _AddedUserControls.Count; i++)
            {
                answerFromUC = (_AddedUserControls[i].DataContext as IQuestionViewModel).ReturnQuestion();
                _Questions.SetAnswer(i + 1, answerFromUC.Answer); // what is the difference with set choices
            }
        }

        private bool _Activated;
        private void Window_Activated(object sender, System.EventArgs e)
        {
            
            if (_Questions == null | _Activated) return;
            if (_Questions.Count <= 0) return;
            _Activated = true;
            mzQuestion q1 = _Questions[0];
            QuestionViewModelBase qvm = (QuestionViewModelBase)q1.ViewModel;
            if(qvm != null)
            {
                qvm.SetFocus();
            }

        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            if (_Questions == null ) return;
            foreach (mzQuestion q in _Questions)
            {
                QuestionViewModelBase qvm = (QuestionViewModelBase)q.ViewModel;
                if (qvm != null) qvm.FocusElement = null;
            }
        }
    }

    public class QuestionOptions
    {
        public string Title { get; set; }
        public bool ShowNA { get; set; }
        public string SaveButtonText { get; set; }
        public bool AllowCancel{ get; set; }
        public bool HideAddMirrors { get; set; }
    }

    public static class QuestionsExtension
    {

        /// <summary>
        /// Returns true if the user does not cancel the input session
        /// </summary>
        /// 
        /// <param name="_Questions"></param>
        /// <param name="aTitle">Text to display as a prompt above the questions </param>
        /// <param name="aAllowCancel">If false the Cancel, button will be hidden</param>
        /// <param name="bShowNAs"></param>
        /// <param name="owner">the window that is calling for the questions to be answered</param>
        /// <param name="aSaveButtonText">The text to show on the Save button. Default is 'Save' </param>
        /// <param name="bReturnFalseIfNoChange">if true and the user does not cancel the view, true will be return only if the answers were not altered during the sesion  </param>
        /// <returns></returns>
        public static bool PromptForAnswers(this mzQuestions _Questions,   string aTitle, bool aAllowCancel = true, bool bShowNAs = false,  Window owner = null, string aSaveButtonText = "Save", bool bReturnFalseIfNoChange = false, bool hideAddMirrors = false)
        {
            owner ??= (Window)System.Windows.Application.Current.Windows.OfType<System.Windows.Window>().SingleOrDefault(x => x.IsActive);

            if (string.IsNullOrWhiteSpace(aTitle))
                aTitle = _Questions.Title;

            aTitle ??= "";
            aTitle = aTitle.Trim();
            if (string.IsNullOrWhiteSpace(aSaveButtonText)) aSaveButtonText = "Save";
            QuestionOptions options = new()
            {
                Title = aTitle,
                ShowNA = bShowNAs,
                SaveButtonText = aSaveButtonText,
                AllowCancel = aAllowCancel,
                HideAddMirrors = hideAddMirrors
            };

            QuestionsView questionsView = new QuestionsView(options);
            questionsView.Questions = _Questions;
            if (owner != null)
                questionsView.Owner = owner;

            List<string> initAnswers = _Questions.CurrentAnswers();

            questionsView.ShowDialog();
            bool _rVal = !questionsView.IsCanceled;
            if (bReturnFalseIfNoChange && ! _rVal)
            {
                List<string> curAnswers = _Questions.CurrentAnswers();
                for(int i = 0; i < curAnswers.Count; i++)
                {
                    if (curAnswers[i] != initAnswers[i])
                    {
                        _rVal = true;
                    }
                }

            }
            return _rVal;
          

        }
    }
}
