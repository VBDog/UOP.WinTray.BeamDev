using UOP.WinTray.UI.Commands;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Documents;
using UOP.WinTray.UI.Views.Windows;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.MessageBox;
using MvvmDialogs.FrameworkDialogs.SaveFile;
using System.Linq;
using System.Windows.Input;

namespace UOP.WinTray.UI.ViewModels
{
    /// <summary>
    /// View model for Warnings View
    /// </summary>
    public class WarningViewModel : TreeViewItemViewModel, IModalDialogViewModel
    {

        #region Constant Variables

        private const string PDFFILTER = "Pdf Files| *.pdf";
        private const string DISPLAYNAME = "WARNINGS";
        private const string FILESAVEDCAPTION = "File Saved Succesfully";
        private const string FILESAVEDTEXT = "File have been saved successfully at ";

        #endregion

        #region Variables

       
       
        private DelegateCommand _CMD_Print;
        private DelegateCommand _CMD_Close;
        private readonly IDialogService _DialogService;


        #endregion

        #region Properties
       

        private bool? _DialogResult; 
        public bool? DialogResult
        {
            get => _DialogResult;
            
            private set { _DialogResult = value; NotifyPropertyChanged("DialogResult"); }
        }

        public uopDocuments WarningDocuments { get; set; }

        private uopDocument _SelectedItem; 
        public uopDocument SelectedItem
        {
            get => _SelectedItem;
            
            set { _SelectedItem = value;
                if (_SelectedItem != null)
                {
                    SelectedWarning = (_SelectedItem as uopDocWarning).TextString;
                }
                NotifyPropertyChanged("SelectedItem");
                NotifyPropertyChanged("SelectedWarning");
            }
        }

        public string SelectedWarning { get; set; }

        public string DisplayName { get; set; }

        #endregion

        #region Commands

        public ICommand Command_Print
        {
            get
            {
                if (_CMD_Print == null)
                {
                    _CMD_Print = new DelegateCommand(param =>
                    {
                        SaveFileDialogSettings saveFileDialogSettings = new()
                        {
                            Filter = PDFFILTER
                        };
                        bool? result = _DialogService.ShowSaveFileDialog(this, saveFileDialogSettings);
                        if (result.HasValue && result.Value)
                        {
                            GeneratePDF(saveFileDialogSettings.FileName);
                        }
                    });
                }
                return _CMD_Print;
            }
        }

        public ICommand Command_Close
        {
            get
            {
                if (_CMD_Close == null)
                {
                    _CMD_Close = new DelegateCommand(param =>
                    {
                        DialogResult = true;
                    });
                }
                return _CMD_Close;
            }
        }

        #endregion

        #region Construtor

        public WarningViewModel(uopProject project, uopDocuments warnings, IDialogService dialogueService)
        {
            _DialogService = dialogueService;
            DisplayName = DISPLAYNAME;
           
            Project = project ?? WinTrayMainViewModel.WinTrayMainViewModelObj.Project;
            WarningDocuments = warnings ?? (Project?.Warnings());
            SelectedItem = WarningDocuments?.FirstOrDefault();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Update warning message 
        /// </summary>
        /// <param name="aWarnings"></param>
        public void UpdateWarningsMessage(uopDocuments aWarnings)
        {
            WarningDocuments = aWarnings ?? (Project?.Warnings());
            SelectedItem = WarningDocuments?.FirstOrDefault();
        }

        #endregion

        #region Private Methods

        private void GeneratePDF(string fileName)
        {
            PdfWriter writer = new(fileName);
            PdfDocument pdf = new(writer);
            Document document = new(pdf);
            Paragraph header = new($"{Project.ProjectTypeName} {Project.FriendlyFileName} Warnings");

            document.Add(header);
            int lineNumber = 1;
            foreach (uopDocument item in WarningDocuments)
            {
                uopDocWarning warning = item as uopDocWarning;
                Paragraph warningLine = new Paragraph($"{lineNumber}) {warning.RangeName} ({warning.Owner}) - {warning.Brief} - {warning.TextString}")
                    .SetFontSize(10);
                document.Add(warningLine);
                lineNumber++;
            }

            document.Close();
            _DialogService.ShowMessageBox(this, new MessageBoxSettings() { Button = System.Windows.MessageBoxButton.OK, Caption = FILESAVEDCAPTION, MessageBoxText = FILESAVEDTEXT + fileName });
        }

        #endregion
    }
}