using UOP.WinTray.UI.Views.Windows.CADfx.Questions;
using UOP.WinTray.Projects.Utilities;

using System.ComponentModel;
using System.Windows.Controls;
using System.Runtime.CompilerServices;

namespace UOP.WinTray.UI.ViewModels.UserControls.CADfx.Questions
{
    public interface IQuestionViewModel
    {
        public void ConfigureDataContext(mzQuestion question, UserControl userControl, QuestionOptions options);
        public string ValidateInput();
        public mzQuestion ReturnQuestion();
        public void InformAboutInvalidInput();
        
    }

    public class MultiSelectItem : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;


        public MultiSelectItem(string aText,bool bIsChecked = false)
        {
            IsChecked = bIsChecked;
            Text = !string.IsNullOrWhiteSpace(aText) ? aText : "";
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion



        private bool _IsChecked;
        public bool IsChecked
        {
            get => _IsChecked;
            set { _IsChecked = value; OnPropertyChanged("IsChecked"); }
        }

        private string _Text;
        public string Text
        {
            get => _Text;
            set {  _Text = value; OnPropertyChanged("Text"); }
        }
    }

}
