using UOP.WinTray.UI.ViewModels;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.UI.Views
{
    /// <summary>
    /// Interaction logic for MDDowncomersDataGridView.xaml
    /// </summary>
    [ExcludeFromCodeCoverage]
    public partial class MDDowncomersDataGridView : UserControl
    {
        private bool isLoad = true;
        public MDDowncomersDataGridView()
        {
            InitializeComponent();

       
        }


      
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var dtContext = this.DataContext as MDDowncomersDataGridViewModel;
            dtContext.DowncomerChanged -= DtContext_downcomerChanged;
            dtContext.DowncomerChanged += DtContext_downcomerChanged;
        }
        /// <summary>
        /// Downcomer change event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DtContext_downcomerChanged(object sender, EventArgs e)
        {
            isLoad = false;
            int index = Convert.ToInt32(sender);
            bool found = false;
            for (int i = 0; i < downcomerGrid.Items.Count; i++)
            {
                DataGridRow row = (DataGridRow)downcomerGrid.ItemContainerGenerator.ContainerFromIndex(i);
                if (row != null)
                {
                    UOP.WinTray.UI.Model.Downcomers dc = (UOP.WinTray.UI.Model.Downcomers)row.Item;
                    string dcname = dc.No != null ? dc.No.Value.ToString() : dc.PN.Value.ToString();
                    if (dc.No != null)
                    {
                        int idx = !found ? mzUtils.LeadingInteger(dcname) : 0;

                        if (idx == index && !found)
                        {
                            found = true;
                            downcomerGrid.SelectedItem = downcomerGrid.Items[i];
                        }
                        else row.Background = Brushes.White;
                    }
                    else
                    {
                        int idx = !found ? !string.IsNullOrWhiteSpace(dcname) ? mzUtils.VarToInteger(dcname.Substring(0,1)) :0 : 0;

                        if (idx == index && !found)
                        {
                            found = true;
                            downcomerGrid.SelectedItem = downcomerGrid.Items[i];
                        }
                        else row.Background = Brushes.White;
                    }
                    
                }

            }
            isLoad = true;
        }
        

        /// <summary>
        /// downcomer selection changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MDDowncomersDataGridViewModel VM = this.DataContext as MDDowncomersDataGridViewModel;
            var datagrid = sender as DataGrid;
            if (datagrid == null) return;
            UOP.WinTray.UI.Model.Downcomers selection = (UOP.WinTray.UI.Model.Downcomers) datagrid.SelectedItem;
            if (selection == null) return;
            int idx = 0;
            if(selection.No != null)
            {
                string dcname = selection.No.Value.ToString();
                 idx = mzUtils.LeadingInteger(dcname);

            } else if(selection.PN != null)
            {
                string dcname = selection.PN.Value.ToString();
                idx = !string.IsNullOrWhiteSpace(dcname) ? mzUtils.VarToInteger(dcname.Substring(0,1)) : 0;

            }
            if (idx <= 0) return;
            if (isLoad)
            {
                VM.SelectionChanged(idx);
            }

        }
    }
}
