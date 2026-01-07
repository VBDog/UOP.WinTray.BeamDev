using UOP.WinTray.UI.ViewModels;
using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using UOP.WinTray.Projects.Tables;
using UOP.WinTray.UI.Utilities;
using System.Reflection;
using UOP.WinTray.Projects;
using System.Collections.Generic;
using UOP.WinTray.Projects.Utilities;


namespace UOP.WinTray.UI.Views
{
    /// <summary>
    /// Interaction logic for Edit_SpoutAreas_View.xaml
    /// </summary>
    [ExcludeFromCodeCoverage]
    public partial class Edit_SpoutAreas_View : Window
    {
        private static readonly FieldInfo _menuDropAlignmentField;

        static Edit_SpoutAreas_View()
        {
            // This static constructor has been used to make the submenus of the context menu appear on the right side of the parent menu item.
            _menuDropAlignmentField = typeof(SystemParameters).GetField("_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static);

            EnsureStandardPopupAlignment();
            SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;
        }

        private static void SystemParameters_StaticPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // This event handler has been used to make the submenus of the context menu appear on the right side of the parent menu item.
            EnsureStandardPopupAlignment();
        }

        private static void EnsureStandardPopupAlignment()
        {
            // This method has been used to make the submenus of the context menu appear on the right side of the parent menu item.
            if (SystemParameters.MenuDropAlignment && _menuDropAlignmentField != null)
            {
                _menuDropAlignmentField.SetValue(null, false);
            }
        }

        public Edit_SpoutAreas_View()
        {
            InitializeComponent();

        }



        public DataTable DataTableCollection { get; set; }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SysMenuUtils.DisableCloseButton(this);
            var VM = DataContext as Edit_SpoutAreas_ViewModel;
            //VM.Host = editLayout;
            //VM.EditSpoutAreas(editLayout);
            //editLayout.SpoutAreaPropertiesHelper = VM;

            VM.Viewer = cadfxViewer;
            VM.Event_ColorChanged -= Edit_SpoutAreas_ViewModel_colorChanged;
            VM.Event_ColorChanged += Edit_SpoutAreas_ViewModel_colorChanged;
            VM.Event_IndexChanged -= Edit_SpoutAreas_ViewModel_indexChanged;
            VM.Event_IndexChanged += Edit_SpoutAreas_ViewModel_indexChanged;
            VM.Activate(this);
            SysMenuUtils.EnableCloseButton(this);



        }

        private void Edit_SpoutAreas_ViewModel_indexChanged(object sender, EventArgs e)
        {
            Edit_SpoutAreas_ViewModel VM = (Edit_SpoutAreas_ViewModel)sender;
            List<mdSpoutArea> areas = VM.SelectedAreas;
            int iNd = VM.CurrentZone.DowncomerCount;
            int iNp = iNd + 1;
            //foreach (mdSpoutArea area in areas)
            //{
            //    DataGridCell cell = GetCell(area.PanelIndex, area.DowncomerIndex + 1);
            //    if(cell != null)cell.Background = Brushes.Yellow;
            //}
            for (int i = 0; i < GridData.Items.Count; i++)
            {
                for (int j = 0; j < GridData.Columns.Count; j++)
                {
                    DataGridCell cell = GetCell(i, j);
                    cell.Background = Brushes.White;
                    cell.IsSelected = false;
                }
            }
        
            foreach(mdSpoutArea area in areas)
            {
                int r = area.PanelIndex;
                int c = uopUtils.OpposingIndex( area.DowncomerIndex, iNd);
                DataGridCell cell = GetCell(r -1, c);
                if (cell != null)
                {
                    cell.Background = ! area.IsVirtual ? Brushes.Yellow : Brushes.LightSalmon;
                }
            }
        }

        private void Edit_SpoutAreas_ViewModel_colorChanged(object sender, EventArgs e)
        {
            var VM = (Edit_SpoutAreas_ViewModel)sender;
            //    AssginGridColor(VM.ColorCodeCollection);
            VM?.Execute_AssignGridColor(this);
        }

        public System.Windows.Media.SolidColorBrush ToMediaColor(System.Drawing.Color color)
        {
            return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
        }

        /// <summary>
        /// function for assiging grid foreground color
        /// </summary>

        public void AssignGridColor(Edit_SpoutAreas_ViewModel aVM )
        {
            if (aVM == null) return;
            uopTable aDataTable = aVM.DataTable;
            if (aDataTable == null) return;

            mdSpoutAreaMatrix matrix = aVM.CurrentMatrix;
            if (matrix == null) return;


            try
            {
                int iNd = matrix.Nd;
                int iNp = iNd + 1;

                for (int p = 1; p <= iNp; p++)
                {
                    for (int d = 1; d <= iNd; d++)
                    {
                        double dctotal = 0;
                        mdSpoutArea area = matrix.Area(p, d);
                        if (area == null) continue;
                        int r = p - 1;
                        int c = !aVM.InvertDowncomerColumns ? d : uopUtils.OpposingIndex(d, iNd)  ;
                        //c--;
                        System.Windows.Controls.DataGridCell dgcell = GetCell(r, c);
                        if (dgcell == null) continue;

                        dgcell.FontWeight = area.OverrideSpoutArea.HasValue ? System.Windows.FontWeights.Bold : System.Windows.FontWeights.Normal;
                        dgcell.Foreground = ToMediaColor(area.Color.Item2);

                        uopInstances insts = area.Instances;
                        foreach (var item in insts)
                        {
                            r = item.Row - 1;
                            c = !aVM.InvertDowncomerColumns ? item.Col : uopUtils.OpposingIndex(item.Col, iNd);
                            //c--;
                            System.Windows.Controls.DataGridCell dgcell1 = GetCell(r, c);
                            if (dgcell1 == null) continue;
                            dgcell1.FontWeight = dgcell.FontWeight;
                            dgcell1.Foreground = dgcell.Foreground;
                        }
                        if (area != null && dgcell != null && area.Color.Item1 != DXFGraphics.dxxColors.BlackWhite)
                        {
                            Console.Beep();
                            break;
                        }
                    }
                    break;
                }


                        for (int r = 0; r < GridData.Items.Count; r++)
                {
                    for (int c = 0; c < GridData.Columns.Count; c++)
                    {


                    //    System.Windows.Controls.DataGridCell dgcell = GetCell(r, c);
                    //    if (dgcell == null) continue;

                    //    System.Windows.FrameworkElement fe = (System.Windows.FrameworkElement)dgcell;
                    //    dgcell.Foreground = Brushes.Black;

                    //    System.Windows.FontStyle fstyle = dgcell.FontStyle;
                    //    uopTableCell cell = aDataTable.Cell(r + 2, c + 1);
                    //    if (cell == null) continue;
                    //    if (cell.FontColor == null) continue;


                    //    if (cell.FontColor != System.Drawing.Color.Black)
                    //    {
                    //        dgcell.Foreground = ToMediaColor(cell.FontColor);

                    //    }
                    //    if (cell.BoldText)
                    //    {
                    //        //Console.WriteLine(fe.ToString());

                    //        //Console.Beep();
                    //        //fstyle = new FontStyle( "Bold");
                    //        //dgcell.FontStyle = fstyle;
                    //    }

                    }
                }
            }
            catch
            {

            }


        }


        /// <summary>
        /// function for get cell
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public DataGridCell GetCell(int row, int column)
        {
            System.Windows.Controls.DataGridRow rowContainer = GetRow(row);
            System.Windows.Controls.DataGridCell cell = new();
            if (rowContainer != null)
            {
                System.Windows.Controls.Primitives.DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);

                if (presenter != null)
                {
                    cell = (System.Windows.Controls.DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                }
                if (cell == null)
                {
                    GridData.ScrollIntoView(rowContainer, GridData.Columns[column]);
                    cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                }
                return cell;
            }
            return null;
        }

        /// <summary>
        /// function for get row
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public DataGridRow GetRow(int index)
        {
            DataGridRow row = (DataGridRow)GridData.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null)
            {
                GridData.UpdateLayout();
                GridData.ScrollIntoView(GridData.Items[index]);
                row = (DataGridRow)GridData.ItemContainerGenerator.ContainerFromIndex(index);
            }
            return row;
        }
        /// <summary>
        /// function for get visual child
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }

        private void GridData_CurrentCellChanged(object sender, EventArgs e)
        {
            var VM = DataContext as Edit_SpoutAreas_ViewModel;
            if (VM == null || VM.CurrentZone == null || GridData.CurrentCell.Column== null) return;

            int coulmnIndex = VM.InvertDowncomerColumns ?  uopUtils.OpposingIndex(GridData.CurrentCell.Column.DisplayIndex, VM.CurrentZone.DowncomerCount) : GridData.CurrentCell.Column.DisplayIndex;
            var RowIndex = ((System.Data.DataRowView)GridData.CurrentCell.Item).Row.ItemArray[0];
            var index = RowIndex.ToString().Split(' ');
            if (index.Count() > 1)
            {
                var rowIndex = int.Parse(index[1]);
                VM.EnterCellEventOfDataGrid(coulmnIndex, rowIndex);
            }
           
        }

        private void cadfxViewer_Loaded(object sender, RoutedEventArgs e)
        {
            //Edit_SpoutAreas_ViewModel VM = DataContext as Edit_SpoutAreas_ViewModel;

            //VM.Viewer = cadfxViewer;

        }




        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var VM = (Edit_SpoutAreas_ViewModel)this.DataContext;
            VM.Window_Closing(sender, e);
        }
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel_Base VM = (ViewModel_Base)DataContext;
            if (VM != null)
            {
                VM.Viewer = null;
                VM.Dispose();
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Escape)
            {
                e.Handled = true;
               
            }
            var VM = (Edit_SpoutAreas_ViewModel)this.DataContext;
            VM?.Window_KeyDown(sender, e);
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var VM = (Edit_SpoutAreas_ViewModel)this.DataContext;
            VM?.Window_KeyUp(sender, e);
        }
    }
}





