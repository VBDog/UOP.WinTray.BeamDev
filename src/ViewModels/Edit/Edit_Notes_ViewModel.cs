using MvvmDialogs;
using System.Collections.ObjectModel;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.UI.Commands;
using UOP.WinTray.UI.Model;

namespace UOP.WinTray.UI.ViewModels
{
    /// <summary>
    /// Edit Project Notes.
    /// </summary>
    public class Edit_Notes_ViewModel : MDProjectViewModelBase, IModalDialogViewModel
    {   
        #region Constructors

        internal Edit_Notes_ViewModel(uopProject project) : base()
        {
            Project = project;
            NotesCollection = new ObservableCollection<ProjectNote>();
            PartType = uppPartTypes.Project;
            uopProperties notes = Project.Notes;
            if(notes != null)
            {
                foreach (uopProperty note in notes)
                {
                    NotesCollection.Add(new ProjectNote() { NoteNo = note.Index, Note = note.ValueS });

                }

            }
        }

        #endregion Constructors


        #region Commands
        private DelegateCommand _CMD_Cancel;
        /// <summary>
        /// cancel command.
        /// </summary>
        public DelegateCommand Command_Cancel
        {
            get { _CMD_Cancel ??= new DelegateCommand(param => Execute_Cancel()); return _CMD_Cancel; }
        }

        private DelegateCommand _CMD_Save;
        /// <summary>
        /// save command.
        /// </summary>
        public DelegateCommand Command_Save
        {
            get { _CMD_Save ??= new DelegateCommand(param => Execute_Save()); return _CMD_Save; }
        }

        private DelegateCommand _CMD_Add;
        /// <summary>
        /// new Row initialize command.
        /// </summary>
        public DelegateCommand Command_Add
        {
            get { _CMD_Add ??= new DelegateCommand(param => Execute_AddRow()); return _CMD_Add; }
        }

        private DelegateCommand _CMD_Delete;
        /// <summary>
        /// delete command.
        /// </summary>
        public DelegateCommand Command_Delete
        {
            get { _CMD_Delete ??= new DelegateCommand(param => Execute_Delete()); return _CMD_Delete; }
        }

        #endregion Commands

        #region Properties

        public uppPartTypes PartType { get; set; }
      

        /// <summary>
        /// Dialog service result
        /// </summary>
        private bool? _DialogResult; 
        public bool? DialogResult { get => _DialogResult; private set { _DialogResult = value; NotifyPropertyChanged("DialogResult"); } }

        /// <summary>
        /// Selected Project note.
        /// </summary>
        private ProjectNote _SelectedItem;
        public ProjectNote SelectedItem { get => _SelectedItem; set { _SelectedItem = value; NotifyPropertyChanged("SelectedItem"); } }


        /// <summary>
        /// Notes Collection.
        /// </summary>
        public ObservableCollection<ProjectNote> NotesCollection { get; set; }

        #endregion Properties

      
        #region Methods

        /// <summary>
        /// Handle Cancel.
        /// </summary>
        private void Execute_Cancel() => DialogResult = false;
        

        /// <summary>
        /// Handle Delete.
        /// </summary>
        private void Execute_Delete()
        {
            if (SelectedItem != null)
            {
                NotesCollection.Remove(SelectedItem);
                int index = 1;
                foreach (ProjectNote item in NotesCollection)
                {
                    item.NoteNo = index;
                    index++;
                }
                NotifyPropertyChanged("NotesCollection");
            }
        }

        /// <summary>
        /// Handle Save.
        /// </summary>
        private void Execute_Save()
        {

            if(Project != null)
            {
                uopProperties notes = new();
                int index = 1;
                foreach (ProjectNote item in NotesCollection)
                {
                    if (!string.IsNullOrWhiteSpace(item.Note))
                    {
                        notes.Add($"Note {index}", item.Note, aPartType: PartType, aCategory: "NOTE");
                        index++;
                    }
                }


                DialogResult = Project.SetNotes(notes) != null;
            }
            else
            {
                DialogResult = false;
            }
  
        }

        /// <summary>
        /// Handle Add row.
        /// </summary>
        private void Execute_AddRow()
        {
            var projectNote = new ProjectNote() { NoteNo = this.NotesCollection.Count + 1 };
            this.NotesCollection.Add(projectNote);
        }

        #endregion Methods
    }
}
