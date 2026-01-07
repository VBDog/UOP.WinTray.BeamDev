using ClosedXML.Excel;
using UOP.DXFGraphics;
using UOP.WinTray.UI.ViewModels;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.UI.Events.EventAggregator;
using UOP.WinTray.UI.Messages;

namespace UOP.WinTray.UI.Views
{
    /// <summary>
    /// Interaction logic for MDProjectView.xaml
    /// </summary>
    public partial class MDProjectView : UserControl
    {

        private const string IS_COMPARE_RESULTS_EXCEL_NEEDED = "IsCompareResultsExcelNeeded";
        private const string COMPARE_FILE_PATH = "CompareFilePath";
        private const string FILENOTFOUND_MESSAGE = "Please provide VB generated files at 'C:WintrayTest'to compare results.";
        private const string FILENOTFOUND_CAPTION = "Compare Results";
        private const string INCH = "in";

        public MDProjectView()
        {
            InitializeComponent();
        }
        [ExcludeFromCodeCoverage]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var VM = DataContext as MDProjectViewModel;
            btnExcel.Visibility = Visibility.Collapsed;
            string isExcelCompareNeeded = System.Configuration.ConfigurationManager.AppSettings.Get(IS_COMPARE_RESULTS_EXCEL_NEEDED);
            if (!string.IsNullOrEmpty(isExcelCompareNeeded) && Boolean.TryParse(isExcelCompareNeeded, out bool isComparedNeeded) && isComparedNeeded)
            {
                btnExcel.Visibility = Visibility.Visible;
            }

            VM.DrawingTab_Index = CADfxGetTabIndex("Drawings");
            VM.Viewer = CadFxViewer_InputSketch;
            VM.Activate(this);
        
        }

        public void UdateViewerImage(dxfImage aImage)
        {
            UOP.DXFGraphicsControl.DXFViewer viewer = CadFxViewer_InputSketch;
            if (aImage != null)
            {
                viewer.SetImage(aImage);
                if (aImage.Display.HasLimits)
                { viewer.ZoomWindow(aImage.Display.LimitsRectangle); }
                else
                { viewer.ZoomExtents(); }
                viewer.RefreshDisplay();
            }
            else { viewer.Clear(); }

        }

        // Added by CADfx
        private int CADfxGetTabIndex(string tabHeader)
        {
            int index = 0;
            for (int i = 0; i < MainTabControl.Items.Count; i++)
            {
                TabItem tabItem = MainTabControl.Items[i] as TabItem;
                if (tabItem != null)
                {
                    if (tabItem.Header.ToString() == tabHeader)
                    {
                        index = i;
                        break;
                    }
                }
            }
            return index;
        }
        // Added by CADfx

        [ExcludeFromCodeCoverage]
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            int Row = 0, Col = 0, CRow = 0, VBRow = 4;
            dynamic deckPanelsObj = Grid_DeckPanels.DataContext;
            dynamic downcomersObj = Grid_Downcomers.DataContext;
            dynamic sproutsObj = Grid_Spouts.DataContext;
            string compareFileName = ConfigurationManager.AppSettings[COMPARE_FILE_PATH];
            if (!string.IsNullOrEmpty(compareFileName))
            {
                var PathName = Path.GetDirectoryName(compareFileName);
                if (!Directory.Exists(PathName))
                    Directory.CreateDirectory(PathName);
            }
            if (!File.Exists(compareFileName))
            {
                MessageBox.Show(FILENOTFOUND_MESSAGE, FILENOTFOUND_CAPTION);
                return;
            }
            XLWorkbook workbook = new(compareFileName);
            IXLWorksheet sheet = workbook.Worksheet(1);
            IXLRow rng = sheet.LastRowUsed();

            Row = rng.RowNumber() + 5;
            sheet.Cell(Row, 1).Value = "C# Data";
            Row++;
            CRow = Row;
            sheet.Cell(Row, 1).Value = "No.";
            sheet.Cell(Row, 2).Value = "Spout Count";
            sheet.Cell(Row, 3).Value = "Pat. Type";
            sheet.Cell(Row, 4).Value = "Spout Length";
            sheet.Cell(Row, 5).Value = "Spouts / Row";
            sheet.Cell(Row, 6).Value = "Vert. Pitch";
            sheet.Cell(Row, 7).Value = "Pat. Length";
            sheet.Cell(Row, 8).Value = "Actual Margin";
            sheet.Cell(Row, 9).Value = "Ideal Area";
            sheet.Cell(Row, 10).Value = "Actual Area";
            sheet.Cell(Row, 11).Value = "Err. % ";
            bool isInNeeded = false;//for file 26780 - make it true
            Row++;

            foreach (var items in sproutsObj.SpoutGroups)
            {
                sheet.Cell(Row, 1).SetValue(XLCellValue.FromObject(items.No.Value));
                sheet.Cell(Row, 2).Value = items.SpoutCount.Value;
                sheet.Cell(Row, 3).Value = items.PatType.Value;
                sheet.Cell(Row, 4).Value = isInNeeded ? items.SpoutLength.Value + INCH : items.SpoutLength.Value;
                sheet.Cell(Row, 5).Value = items.SPR;
                sheet.Cell(Row, 6).Value = items.VertPitch.Value;
                sheet.Cell(Row, 7).Value = items.PatLength;
                sheet.Cell(Row, 8).Value = items.ActualMargin.Value;
                sheet.Cell(Row, 9).Value = items.IdealArea.Value;
                sheet.Cell(Row, 10).Value = items.ActualArea;
                sheet.Cell(Row, 11).Value = items.Err.Value;
                Row++;
            }
            int spoutGroupCount = sproutsObj.SpoutGroups.Count;
            ApplyCellColor(CRow + 1, VBRow, 1, 11, spoutGroupCount, sheet);

            Row = CRow;
            Col = 13;
            sheet.Cell(Row, Col + 0).Value = "No.";
            sheet.Cell(Row, Col + 1).Value = "Weir Length";
            sheet.Cell(Row, Col + 2).Value = "Ideal Area";
            sheet.Cell(Row, Col + 3).Value = "Actual Area";
            sheet.Cell(Row, Col + 4).Value = "Err. % ";
            Row++;
            foreach (var items in downcomersObj.Downcomers)
            {
                sheet.Cell(Row, Col + 0).Value = items.No.Value;
                sheet.Cell(Row, Col + 1).Value = items.WeirLength;
                sheet.Cell(Row, Col + 2).Value = items.IdealArea;
                sheet.Cell(Row, Col + 3).Value = items.ActualArea;
                sheet.Cell(Row, Col + 4).Value = items.Err.Value;
                Row++;
            }
            int downComerCount = downcomersObj.Downcomers.Count;
            ApplyCellColor(CRow + 1, 4, Col, Col + 5, downComerCount, sheet);

            Row = CRow;
            Col = 19;
            sheet.Cell(Row, Col + 0).Value = "No.";
            sheet.Cell(Row, Col + 1).Value = "FBA";
            sheet.Cell(Row, Col + 2).Value = "FBA/ WL";
            sheet.Cell(Row, Col + 3).Value = "V/L Err. % ";
            sheet.Cell(Row, Col + 4).Value = "Ideal Area";
            sheet.Cell(Row, Col + 5).Value = "Actual Area";
            sheet.Cell(Row, Col + 6).Value = "Err. % ";
            Row++;
            foreach (var items in deckPanelsObj.DeckPanels)
            {
                sheet.Cell(Row, Col + 0).Value = items.No.Value;
                sheet.Cell(Row, Col + 1).Value = items.FBA;
                sheet.Cell(Row, Col + 2).Value = items.WL;
                sheet.Cell(Row, Col + 3).Value = items.VolumeErr.Value;
                sheet.Cell(Row, Col + 4).Value = items.IdealArea;
                sheet.Cell(Row, Col + 5).Value = items.ActualArea;
                sheet.Cell(Row, Col + 6).Value = items.Err.Value;
                Row++;
            }
            int deckPanelCount = deckPanelsObj.DeckPanels.Count;
            ApplyCellColor(CRow + 1, 4, Col, Col + 7, deckPanelCount, sheet);
            workbook.Save();
            workbook.Dispose();
            Process p = new();
            Type officeType = Type.GetTypeFromProgID("Excel.Application");
            dynamic xlApp = Activator.CreateInstance(officeType);
            xlApp.Visible = false;
            p.StartInfo.FileName = xlApp.Path + @"\EXCEL.exe";
            p.StartInfo.Arguments = System.Configuration.ConfigurationManager.AppSettings[COMPARE_FILE_PATH];
            p.Start();
        }

        [ExcludeFromCodeCoverage]
        private static void ApplyCellColor(int CRow, int VBRow, int startCol, int endCol, int ObjListCount, IXLWorksheet sheet)
        {
            double cell1, cell2;
            for (int ind = CRow; ind <= (CRow + ObjListCount); ind++)
            {
                for (int j = startCol; j <= endCol; j++)
                {
                    if (Convert.ToString(sheet.Cell(ind, j).Value) != Convert.ToString(sheet.Cell(VBRow, j).Value))
                    {
                        if (double.TryParse(Convert.ToString(sheet.Cell(ind, j).Value), out cell1))
                        {
                            if (double.TryParse(Convert.ToString(sheet.Cell(VBRow, j).Value), out cell2))
                            {
                                if (j == 20)
                                {
                                    if (Math.Abs(Math.Round(cell1 - cell2, 2)) >= 0.01)
                                    {
                                        sheet.Cell(ind, j).Style.Fill.BackgroundColor = XLColor.Red;
                                        sheet.Cell(ind, j).CreateComment().AddText(Convert.ToString(sheet.Cell(VBRow, j).Value));
                                    }
                                }
                                else if (Math.Abs(Math.Round(cell1 - cell2, 3)) >= 0.001)
                                {
                                    sheet.Cell(ind, j).Style.Fill.BackgroundColor = XLColor.Red;
                                    sheet.Cell(ind, j).CreateComment().AddText(Convert.ToString(sheet.Cell(VBRow, j).Value));
                                }
                            }
                            else
                            {
                                sheet.Cell(ind, j).Style.Fill.BackgroundColor = XLColor.Red;
                                sheet.Cell(ind, j).CreateComment().AddText(Convert.ToString(sheet.Cell(VBRow, j).Value));
                            }
                        }
                        else
                        {
                            sheet.Cell(ind, j).Style.Fill.BackgroundColor = XLColor.Red;
                            sheet.Cell(ind, j).CreateComment().AddText(Convert.ToString(sheet.Cell(VBRow, j).Value));
                        }
                    }
                }
                VBRow++;
            }
        }

        // Added by CADfx
        private void cadfxViewer_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is UOP.DXFGraphicsControl.DXFViewer dxfViewer)
            {
                if (dxfViewer.DataContext is DXFViewerViewModel viewModel)
                {
                    DXFViewerViewModel.CADfxViewer = dxfViewer;
                    if (viewModel.Image != null)
                    {
                        if (dxfViewer.CADModels == null || dxfViewer.CADModels.GUID != viewModel.Image.GUID)
                        {

                            dxfViewer.SetImage(viewModel.Image);

                            if (viewModel.titleBlockHelper != null)
                                viewModel.titleBlockHelper.Insert(DXFViewerViewModel.CADfxViewer);

                            dxfViewer.ZoomExtents();
                            viewModel.CADModels = dxfViewer.CADModels.Clone();
                            viewModel.Image?.Dispose();
                            viewModel.Image = null;
                        }
                    }
                    else if (viewModel.CADModels != null)

                    {

                        dxfViewer.SetModel(viewModel.CADModels.Clone());

                    }
                }
            }
        }

        private void drawingsTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedTabItemModel = (sender as TabControl)?.SelectedItem as UOPDocumentTab;
            if (selectedTabItemModel != null)
            {
                ((MDProjectViewModel)DataContext).DrawingTab_Show(selectedTabItemModel.Header);
            }
        }

        private void cadfxTrayDxfViewer_Loaded(object sender, RoutedEventArgs e)
        {
            ((MDProjectViewModel)DataContext).Viewer = CadFxViewer_InputSketch;
        }



        // Added by CADfx



        //private void CadFxViewer_InputSketch_SizeChanged(object sender, SizeChangedEventArgs e)

        //{

        //    MDProjectViewModel VM = DataContext as MDProjectViewModel;

        //    if (VM.MDProject == null) return;

        //    VM.UpdateTraySketch(true,null);

        //}



        public void RefreshProjectControls()

        {



            InvalidateVisual();

            for (int i = 0; i < VisualChildrenCount; i++)

            {

                UIElement VC = (UIElement)GetVisualChild(i);

                VC.InvalidateVisual();

            }

            //appApplication.DoEvents();



        }


    


        private void Grid_Spouts_Loaded(object sender, RoutedEventArgs e)
        {
            MDProjectViewModel VM = DataContext as MDProjectViewModel;
            MDSpoutGroupsDataGridView spoutgrid = (MDSpoutGroupsDataGridView)sender;
            MDSpoutGroupsDataGridViewModel gridvm = (MDSpoutGroupsDataGridViewModel)spoutgrid.DataContext;
            gridvm.ParentVM = VM;
        }



        private void Grid_Downcomers_Loaded(object sender, RoutedEventArgs e)
        {
            MDProjectViewModel VM = DataContext as MDProjectViewModel;
            MDDowncomersDataGridView dcgrid = (MDDowncomersDataGridView)sender;
            MDDowncomersDataGridViewModel gridvm = (MDDowncomersDataGridViewModel)dcgrid.DataContext;
            gridvm.ParentVM = VM;

        }



        private void Grid_DeckPanels_Loaded(object sender, RoutedEventArgs e)

        {

            MDProjectViewModel VM = DataContext as MDProjectViewModel;

            MDDeckPanelsDataGridView panelgrid = (MDDeckPanelsDataGridView)sender;

            MDDeckPanelsDataGridViewModel gridvm = (MDDeckPanelsDataGridViewModel)panelgrid.DataContext;

            gridvm.ParentVM = VM;

        }


        private void PropertyList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            UOPPropertyListView propList = sender as UOPPropertyListView;
            if (propList == null) return;
            MDProjectViewModel VM = DataContext as MDProjectViewModel;
            VM?.PropertyList_DoubleClick(propList.SelectedProperty, propList.PartType);
        }

     

        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            MDProjectViewModel VM = DataContext as MDProjectViewModel;
            if (VM != null) { if (!VM.Activated) VM.Activate(this); }
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            MDProjectViewModel VM = DataContext as MDProjectViewModel;
            VM.Viewer = null;
        }
    }
}
