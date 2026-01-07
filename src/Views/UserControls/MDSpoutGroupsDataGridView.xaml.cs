using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.UI.ViewModels;

namespace UOP.WinTray.UI.Views
{
    /// <summary>
    /// Interaction logic for MDSpoutGroupsView.xaml
    /// </summary>
    [ExcludeFromCodeCoverage]
    public partial class MDSpoutGroupsDataGridView : UserControl
    {
        private bool isLoad = true;
        public MDSpoutGroupsDataGridView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var dtContext = this.DataContext as MDSpoutGroupsDataGridViewModel;
            dtContext.DowncomerChanged -= DtContext_downcomerChanged;
            dtContext.DowncomerChanged += DtContext_downcomerChanged;
        }
        /// <summary>
        /// Downcomer selection changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DtContext_downcomerChanged(object sender, EventArgs e)
        {
            isLoad = false;
            int index = Convert.ToInt32(sender);
            for (int i = 0; i < SpoutGrid.Items.Count; i++)
            {
                DataGridRow row = (DataGridRow)SpoutGrid.ItemContainerGenerator.ContainerFromIndex(i);
                if (row != null)
                {
                    var val = row.Background;

                    if (i == index - 1)
                    {
                        SpoutGrid.SelectedItem = SpoutGrid.Items[i];
                       
                     }
                    else row.Background = Brushes.White;
                }

            }
            isLoad = true;
        }

        /// <summary>
        /// Grid row selection completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        
        private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MDSpoutGroupsDataGridViewModel VM = DataContext as MDSpoutGroupsDataGridViewModel;
            var datagrid = sender as DataGrid;
            if (datagrid == null) return;
         

            UOP.WinTray.UI.Model.SpoutGroup selection = (UOP.WinTray.UI.Model.SpoutGroup)datagrid.SelectedItem;
            if (selection == null) return;
            string sgname = selection.No.Value.ToString();
            if (string.IsNullOrWhiteSpace(sgname)) return;

            if (isLoad)
            {
                VM.SelectionChanged(sgname);
            }
        }
    }
}
