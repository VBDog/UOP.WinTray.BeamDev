using UOP.DXFGraphicsControl;
using UOP.DXFGraphics;
using UOP.WinTray.UI.BusinessLogic;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Documents;
using UOP.WinTray.UI.Events.EventAggregator;
using UOP.WinTray.UI.Interfaces;
using UOP.WinTray.UI.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


namespace UOP.WinTray.UI.ViewModels.CADfx.Windows
{
    public class DrawingsBatchProcessViewModel :  INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        #region Notifying Properties
        public ObservableCollection<SelectableWrapper> DrawingWrappers { get; set; } // Will contain uopDocDrawing objects

        public uopProject Project { get; set; }

        private string saveToPath;
        public string SaveToPath
        {
            get => saveToPath;
            set
            {
                if (saveToPath != value)
                {
                    saveToPath = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool generalUIComponentIsEnabled;
        public bool GeneralUIComponentIsEnabled
        {
            get => generalUIComponentIsEnabled;
            set
            {
                if (generalUIComponentIsEnabled != value)
                {
                    generalUIComponentIsEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool processButtonIsEnabled;
        public bool ProcessButtonIsEnabled
        {
            get => processButtonIsEnabled;
            set
            {
                if (processButtonIsEnabled != value)
                {
                    processButtonIsEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool cancelButtonIsEnabled;
        public bool CancelButtonIsEnabled
        {
            get => cancelButtonIsEnabled;
            set
            {
                if (cancelButtonIsEnabled != value)
                {
                    cancelButtonIsEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private string cancelPromptText;
        public string CancelPromptText
        {
            get => cancelPromptText;
            set
            {
                if (cancelPromptText != value)
                {
                    cancelPromptText = value;
                    OnPropertyChanged();
                }
            }
        }

        private string currentDrawingProgressText;
        public string CurrentDrawingProgressText
        {
            get => currentDrawingProgressText;
            set
            {
                if (currentDrawingProgressText != value)
                {
                    currentDrawingProgressText = value;
                    OnPropertyChanged();
                }
            }
        }

        private Visibility progressVisibility;
        public Visibility ProgressVisibility
        {
            get => progressVisibility;
            set
            {
                if (progressVisibility != value)
                {
                    progressVisibility = value;
                    OnPropertyChanged();
                }
            }
        }

        private double progressCurrentValue;
        public double ProgressCurrentValue
        {
            get => progressCurrentValue;
            set
            {
                if (progressCurrentValue != value)
                {
                    progressCurrentValue = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsEnglishSelected;
        public bool IsEnglishSelected
        {
            get => _IsEnglishSelected;
            set
            {
                _IsEnglishSelected = value;
                OnPropertyChanged();
            }
        }

        private string _Heading = "Drawings";
        public string Heading
        {
            get => _Heading;
            set
            {
                value ??= "";
                _Heading = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Ordinary Fields and Properties
        private bool busyProcessing;
        private bool shouldBeClosed;
        private const string CANCEL_PROMPT_TEXT = "Canceling the process. Please wait ...";
        private CancellationTokenSource cancellationTokenSource;

        public Action CloseForm { get; set; }
        #endregion

        #region Commands
        DelegateCommand drawingsSetSelectedForAllDelegateCommand;
        public ICommand DrawingsSetSelectedForAllCommand
        {
            get
            {
                if (drawingsSetSelectedForAllDelegateCommand == null)
                {
                    Action<object> executeAction = (object param) =>
                    {
                        bool selected = bool.Parse(param as string);
                        SetSelectedForAll(DrawingWrappers, selected);
                    };

                    drawingsSetSelectedForAllDelegateCommand = new DelegateCommand(executeAction, () => true);
                }

                return drawingsSetSelectedForAllDelegateCommand;
            }
        }

        DelegateCommand showFolderBrowserDialogDelegateCommand;
        public ICommand ShowFolderBrowserDialogCommand
        {
            get
            {
                if (showFolderBrowserDialogDelegateCommand == null)
                {
                    Action<object> executeAction = (object param) =>
                    {
                        string selectedPath = ShowFolderBrowserDialog();
                        if (Directory.Exists(selectedPath))
                        {
                            SaveToPath = selectedPath;
                        }
                        else
                        {
                            //SaveToPath = "";
                            //MessageBox.Show("The path does not exist. Please select a valid path.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    };

                    showFolderBrowserDialogDelegateCommand = new DelegateCommand(executeAction, () => true);
                }

                return showFolderBrowserDialogDelegateCommand;
            }
        }

        DelegateCommand closeDelegateCommand;
        public ICommand CloseCommand
        {
            get
            {
                if (closeDelegateCommand == null)
                {
                    Action<object> executeAction = (object param) =>
                    {
                        CloseForm?.Invoke();
                    };

                    closeDelegateCommand = new DelegateCommand(executeAction, () => true);
                }

                return closeDelegateCommand;
            }
        }
        #endregion

        #region Constructors
        public DrawingsBatchProcessViewModel(IEnumerable<uopDocDrawing> drawings, string category,  uopProject project , string saveTo = "")
        {
            // Initialize Drawings collection
            DrawingWrappers = new ObservableCollection<SelectableWrapper>();
            IsEnglishSelected = true;
           
            Heading = string.IsNullOrWhiteSpace(category) ? "Drawings" : category;
            if (project != null) 
            {
                Project = project;
                List<uopTrayRange> allTrayRanges = project.TrayRanges.CollectionObj.OfType<uopTrayRange>().ToList(); // new List<mdTrayRange>();

                uppUnitFamilies units = uppUnitFamilies.English;

                if (string.Compare(category, "FUNCTIONAL", true) == 0) units = uppUnitFamilies.English;
                if (string.Compare(category, "INSTALLATION", true) == 0) units = project.CustomerDrawingUnits;
                if (string.Compare(category, "MANUFACTURING", true) == 0) units = project.ManufacturingDrawingUnits;
                if (string.Compare(category, "TRAY VIEWS", true) == 0) units = project.CustomerDrawingUnits;

                IsEnglishSelected = units == uppUnitFamilies.English;

                foreach (var drawing in drawings)
                {

                    drawing.DrawingUnits = units;
                    drawing.Project = project;
                    if (drawing.RequiresTraySelection)
                    {
                        foreach (var trayRange in allTrayRanges)
                        {
                            DrawingWrappers.Add(new SelectableWrapper()
                            {
                                Drawing = drawing,
                                TrayRange = trayRange,
                                Project = project,
                                Selected = false
                            });
                        }
                    }
                    else
                    {
                        DrawingWrappers.Add(new SelectableWrapper()
                        {
                            Drawing = drawing,
                            TrayRange = drawing.Range,
                            Project = project,
                            Selected = false
                        });
                    }
                }

                saveTo ??= "";
                if (!string.IsNullOrWhiteSpace(saveTo))
                {
                    if (!Directory.Exists(saveTo)) saveTo = project.ProjectFolder;
                }
                SaveToPath = saveTo;

            }

            SetupUIForFreshStart();
        }
        #endregion

        #region Methods

        private void SetupUIForFreshStart()
        {
            GeneralUIComponentIsEnabled = true;
            ProcessButtonIsEnabled = true;
            CancelButtonIsEnabled = false;

            UpdateProgress("", 0);
            CancelPromptText = "";

            busyProcessing = false;
            shouldBeClosed = false;
        }

        private void SetupUIForStartingTheProcess()
        {
            GeneralUIComponentIsEnabled = false;
            ProcessButtonIsEnabled = false;
            CancelButtonIsEnabled = true;

            busyProcessing = true;

            UpdateProgress("", 0);
        }

        private void SetupUIForCancelingTheProcess()
        {
            CancelPromptText = CANCEL_PROMPT_TEXT;

            GeneralUIComponentIsEnabled = false;
            ProcessButtonIsEnabled = false;
            CancelButtonIsEnabled = false; // To prevent the user from clicking the cancel button multiple times

            UpdateProgress("", 0);
        }

        private void SetSelectedForAll(ObservableCollection<SelectableWrapper> selectableWrappers, bool selected)
        {
            foreach (var item in selectableWrappers)
            {
                item.Selected = selected;
            }
        }

        private string ShowFolderBrowserDialog()
        {
            string selectedPath = string.Empty;

            //using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            //{
            //    dialog.SelectedPath = SaveToPath;

            //    System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            //    if (result == System.Windows.Forms.DialogResult.OK)
            //    {
            //        selectedPath = dialog.SelectedPath;
            //    }
            //}
            using (var dlg = new Ookii.Dialogs.WinForms.VistaFolderBrowserDialog())
            {
                dlg.SelectedPath = SaveToPath;
                dlg.Description = "Select Drawing Output Folder";
                if (dlg.ShowDialog().ToString() == "OK")
                {
                    SaveToPath = dlg.SelectedPath;
                   
                }
            }
            return selectedPath;
        }

        public async Task ProcessInputsAsync()
        {
            if (!Directory.Exists(SaveToPath))
            {
                MessageBox.Show("The path does not exist. Please select a valid path.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SetupUIForStartingTheProcess();

            cancellationTokenSource = new CancellationTokenSource();

            var processInputs = ExtractBatchProcessInputs();

            Dictionary<string, string> failedDrawings = await ProcessInputs(processInputs, cancellationTokenSource.Token);
            InformUserAboutResults(processInputs, failedDrawings);

            if (shouldBeClosed) // Note! "SetupUIForFreshStart" sets the value of the "shouldBeClosed"
            {
                SetupUIForFreshStart();
                CloseForm?.Invoke();
            }
            else
            {
                SetupUIForFreshStart();
            }
        }

        private BatchProcessInputs ExtractBatchProcessInputs()
        {
            var result = new BatchProcessInputs() { SelectedDrawings = new List<SelectableWrapper>() };
            
            // Set the selected drawings
            
            foreach (var drawingWrapper in DrawingWrappers)
            {
                if (drawingWrapper.Selected)
                {
                    result.SelectedDrawings.Add(drawingWrapper);
                }
            }

            // Set the selected path
            result.SelectedPath = SaveToPath;

            // Set the canceled
            result.Canceled = false;

            return result;
        }

        private async Task<Dictionary<string, string>> ProcessInputs(BatchProcessInputs inputs, CancellationToken cancellationToken)
        {
            string errorMessage = string.Empty;
            Dictionary<string, string> failedDrawings = new(); // This keeps the failed drawings and the reason for their failure

            if (inputs.SelectedDrawings == null || inputs.SelectedDrawings.Count == 0)
            {
                return failedDrawings;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                inputs.Canceled = true;
                return failedDrawings;
            }

            dxfImage image;
            var projectType = WinTrayMainViewModel.WinTrayMainViewModelObj.ProjectType;
            string progressText = "";
            double progressPercentage = 0;
            SelectableWrapper drawingWrapper;

            for (int i = 0; i < inputs.SelectedDrawings.Count; i++)
            {
                drawingWrapper = inputs.SelectedDrawings[i];

                // Updating the status
                if (cancellationToken.IsCancellationRequested)
                {
                    inputs.Canceled = true;
                    return failedDrawings;
                }
                progressText = GetCurrentDrawingProgressText(drawingWrapper.ItemName, i + 1, inputs.SelectedDrawings.Count);
                progressPercentage = (double)i / inputs.SelectedDrawings.Count * 100;
                UpdateProgress(progressText, progressPercentage);
                // Updating the status

                // Creating the proper app drawing source
                if (cancellationToken.IsCancellationRequested)
                {
                    inputs.Canceled = true;
                    return failedDrawings;
                }

                // Creating the proper app drawing source
                IappDrawingSource appDrawingSource = GetAppDrawingSource(projectType);
                if (appDrawingSource == null)
                {
                    errorMessage = $"Unable to get app drawing source for project type \"{projectType}\".";
                    failedDrawings.Add(drawingWrapper.ItemName, errorMessage);
                    continue;
                }
              
                // Generating the image
                if (cancellationToken.IsCancellationRequested)
                {
                    inputs.Canceled = true;
                    return failedDrawings;
                }
                (image, errorMessage) = await GenerateDxfImageFromDrawing(drawingWrapper.Drawing, drawingWrapper.TrayRange, drawingWrapper.Project, appDrawingSource);
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = $"Unable to generate image for drawing \"{drawingWrapper.ItemName}\":\r\nError: {errorMessage}";
                    failedDrawings.Add(drawingWrapper.ItemName, errorMessage);
                    continue;
                }
                // Generating the image

                // Saving the generated image to a file
                if (cancellationToken.IsCancellationRequested)
                {
                    inputs.Canceled = true;
                    return failedDrawings;
                }
                errorMessage = SaveDxfImageAs(image, inputs.SelectedPath, drawingWrapper.ItemName, inputs.SelectedFileExtension, appDrawingSource, projectType, drawingWrapper.Drawing);
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = $"Unable to save drawing \"{drawingWrapper.ItemName}\":\r\nError: {errorMessage}";
                    failedDrawings.Add(drawingWrapper.ItemName, errorMessage);
                    continue;
                }

                image.Dispose();
                image = null;
                // Saving the generated image to a file
            }

            // Make the progress bar show full bar
            UpdateProgress(progressText, 100);
            // Make the progress bar show full bar

            return failedDrawings;
        }

        private async Task<(dxfImage, string)> GenerateDxfImageFromDrawing(uopDocDrawing drawing, uopTrayRange trayRange, uopProject project, IappDrawingSource appDrawingSource)
        {
            return await Task.Run<(dxfImage, string)>(() =>
            {
                string errorMessage = string.Empty;
                dxfImage image = null;

                try

                {
                    appDrawingSource.Project = project;
                    drawing.DeviceSize = new System.Drawing.Size(1201, 841);
                    drawing.DrawingUnits = IsEnglishSelected ? uppUnitFamilies.English : uppUnitFamilies.Metric;
                    image = appDrawingSource.GenerateImage(drawing, preSelectedTrayRange: trayRange, bSuppressIDEEffects:true);

                }
                catch (Exception e)
                {
                    image = null;
                    errorMessage = $"Exception:\r\n{e}";
                }

                return (image, errorMessage);
            });
        }

        private string SaveDxfImageAs(dxfImage image, string path, string drawingName, string fileExtension, IappDrawingSource appDrawingSource, uppProjectTypes projectType, uopDocDrawing doc)
        {
            string errorMessage = string.Empty;

            DXFViewer viewer = new();

            try
            {
                string fileName = $"{drawingName}.{fileExtension}";
                string fullFilePath = Path.Combine(path, fileName);

                viewer.SetImage(image);

                switch (projectType)
                {
                    case uppProjectTypes.MDSpout:
                        AppDrawingMDSHelper mdsHelper = appDrawingSource as AppDrawingMDSHelper;
                        mdsHelper?.Tool?.TitleBlockHelper?.Insert(viewer);
                        break;
                    case uppProjectTypes.MDDraw:
                        AppDrawingMDDHelper mddHelper = appDrawingSource as AppDrawingMDDHelper;
                        mddHelper?.Tool?.TitleBlockHelper?.Insert(viewer);
                        break;
                    default:
                        errorMessage = $"Project type \"{projectType}\" is not supported.";
                        return errorMessage;
                }

                switch (fileExtension.ToLower())
                {
                    case "dwg":
                        viewer.SaveDwg(fullFilePath, out errorMessage, doc.DrawingUnits == uppUnitFamilies.Metric);
                        break;
                    case "dxf":
                        viewer.SaveDxf(fullFilePath, out errorMessage, doc.DrawingUnits == uppUnitFamilies.Metric );
                        break;
                    case "pdf":
                        viewer.SavePdf(fullFilePath, out errorMessage);
                        break;
                    default:
                        errorMessage = $"Unknown file extension: {fileExtension}";
                        return errorMessage;
                }
            }
            catch (Exception e)
            {
                errorMessage = $"Exception:\r\n{e}";
            }
            finally
            {
                viewer?.Clear();
                viewer = null;
            }

            return errorMessage;
        }

        private void UpdateProgress(string statusString, double progressPercentage)
        {
            ProgressVisibility = string.IsNullOrWhiteSpace(statusString) ? Visibility.Hidden : Visibility.Visible;
            ProgressCurrentValue = progressPercentage;
            CurrentDrawingProgressText = statusString;
        }

        private string GetCurrentDrawingProgressText(string drawingName, int index, int totalNumberOfDrawings)
        {
            string status = $"[{index}/{totalNumberOfDrawings}] {drawingName}";
            return status;
        }

        public void CancelProcess(bool shouldBeClosed)
        {
            if (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                SetupUIForCancelingTheProcess();
                cancellationTokenSource.Cancel();
            }

            this.shouldBeClosed = shouldBeClosed;
        }

        public bool IsBusyProcessing()
        {
            return busyProcessing;
        }

        private IappDrawingSource GetAppDrawingSource(uppProjectTypes projectType)
        {
            IappDrawingSource appDrawingSource;
            switch (projectType)
            {
                case uppProjectTypes.MDSpout:
                    IEventAggregator eventAggregator = ApplicationModule.Instance.Resolve<IEventAggregator>();

                    appDrawingSource = new AppDrawingMDSHelper() {  Project = Project }  ;
                    break;
                case uppProjectTypes.MDDraw:
                    appDrawingSource = new AppDrawingMDDHelper() { Project = Project };
                    break;
                default:
                    appDrawingSource = null;
                    break;
            }

            return appDrawingSource;
        }

        private void InformUserAboutResults(BatchProcessInputs processInputs, Dictionary<string, string> failedDrawings)
        {
            string logFilePrompt = failedDrawings.Count > 0 ? "Process failed for some of the drawings. Please check the log file in the selected path." : "";

            if (processInputs.Canceled)
            {
                MessageBox.Show($"Process has been cancelled. {logFilePrompt}", "Batch Process Result", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                if (failedDrawings.Count == 0)
                {
                    MessageBox.Show("Drawings saved successfully.", "Batch Process Result", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(logFilePrompt, "Batch Process Result", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            if (failedDrawings.Count > 0)
            {
                ProcessDrawingErrors(failedDrawings, processInputs.SelectedPath);
            }
        }

        private void ProcessDrawingErrors(Dictionary<string, string> failedDrawings, string savePath)
        {
            StringBuilder sb = new();

            foreach (var kv in failedDrawings)
            {
                sb.Append($"Drawing: {kv.Key}\r\n\r\nFailure Reason: {kv.Value}\r\n**************************************************\r\n");
            }

            string filePath = Path.Combine(savePath, "Log.txt");
            bool successful = SaveLogFile(filePath, sb.ToString());
            if (successful)
            {
                successful = OpenLogFileInDefaultTextEditor(filePath);
                if (!successful)
                {
                    MessageBox.Show("Error opening the log file.", "Batch Process Result", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Error saving the log file.", "Batch Process Result", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool SaveLogFile(string path, string content)
        {
            bool successful = true;

            try
            {
                File.WriteAllText(path, content);
            }
            catch (Exception)
            {
                successful = false;
            }

            return successful;
        }

        private bool OpenLogFileInDefaultTextEditor(string filePath)
        {
            bool successful = true;

            try
            {
                using Process process = new();

                process.StartInfo.FileName = "explorer";
                process.StartInfo.Arguments = "\"" + filePath + "\"";
                process.Start();
            }
            catch (Exception)
            {
                successful = false;
            }

            return successful;
        }
        #endregion

    }

    public class BatchProcessInputs
    {
        public List<SelectableWrapper> SelectedDrawings { get; set; }
        public string SelectedPath { get; set; }
        public bool Canceled { get; set; }
        public string SelectedFileExtension { get; set; } = "dwg";
    }

    public class SelectableWrapper : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        public uopDocDrawing Drawing { get; set; }

        public uopTrayRange TrayRange { get; set; }

        public uopProject Project {get; set;}

        private bool _selected;
        public bool Selected
        {
            get => _selected;
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ItemName
        {
            get
            {
                if (TrayRange == null || !Drawing.RequiresTraySelection)
                {
                    return Drawing.DrawingName;
                }
                else
                {
                    return $"{Drawing.DrawingName} ({TrayRange})";
                }
            }
        }
    }
}
