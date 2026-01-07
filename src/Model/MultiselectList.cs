namespace UOP.WinTray.UI.Model
{
    /// <summary>
    /// Model for storing multiselect list values
    /// </summary>
    public class MultiselectList : BindingObject
    {
        private string text;
        private bool? isSelected;

        public string Text
        {
            get { return text; }
            set { text = value; NotifyPropertyChanged("Text"); }
        }

        public bool? IsSelected
        {
            get { return isSelected; }
            set { isSelected = value; NotifyPropertyChanged("IsSelected"); }
        }
    }
}
