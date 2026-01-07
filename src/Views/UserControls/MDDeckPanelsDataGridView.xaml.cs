using UOP.WinTray.UI.ViewModels;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UOP.WinTray.UI.Views
{
    /// <summary>
    /// Interaction logic for MDDeckPanelsDataGridView.xaml
    /// </summary>
    [ExcludeFromCodeCoverage]
    public partial class MDDeckPanelsDataGridView : UserControl
    {
        private bool isLoad = true;

        public MDDeckPanelsDataGridView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var dtContext = this.DataContext as MDDeckPanelsDataGridViewModel;
            dtContext.DeckPanelChanged -= DtContext_downcomerChanged;
            dtContext.DeckPanelChanged += DtContext_downcomerChanged;
        }
        /// <summary>
        /// Downcomer changed method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DtContext_downcomerChanged(object sender, EventArgs e)
        {
            isLoad = false;
            int index = Convert.ToInt32(sender);
            for (int i = 0; i < deckPanelGrid.Items.Count;i++)
            {
                DataGridRow row = (DataGridRow)deckPanelGrid.ItemContainerGenerator.ContainerFromIndex(i);
                if (row != null)
                {
                    if (i == index - 1)
                    {
                        deckPanelGrid.SelectedItem = deckPanelGrid.Items[i];
                    }
                    else row.Background = Brushes.White;
                }

            }
            isLoad = true;
        }
        
        /// <summary>
        /// Selection changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MDDeckPanelsDataGridViewModel dtContext = this.DataContext as MDDeckPanelsDataGridViewModel;
            var datagrid = sender as DataGrid;
            if (datagrid == null) return;
            var index = datagrid.SelectedIndex + 1;
            if (isLoad) dtContext.SelectionChanged(index);
        }
    }
}
