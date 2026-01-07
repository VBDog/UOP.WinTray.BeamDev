using System.ComponentModel;

namespace UOP.WinTray.UI.Model
{
    /// <summary>
    /// Type holds Project Notes.
    /// </summary>
    public class ProjectNote : INotifyPropertyChanged
    {

        #region Properties

        /// <summary>
        /// Note No.
        /// </summary>
        private int _NoteNo;
        public int NoteNo { get => _NoteNo; set { _NoteNo = value; OnPropertyRaised("NoteNo"); } }

        /// <summary>
        /// Note.
        /// </summary>
        private string _Note;
        public string Note { get=> _Note; set { _Note = value; OnPropertyRaised("Note"); } }
        #endregion

        #region Property Change Handler

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyRaised(string propertyname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
        }

        #endregion  Property Change Handler

    }
}
