using UOP.WinTray.Projects.Tables;
using UOP.WinTray.UI.ViewModels;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UOP.WinTray.UI.Views
{
    /// <summary>
    /// Interaction logic for UOPPropertyGridView.xaml
    /// </summary>
    [ExcludeFromCodeCoverage]
    public partial class UOPPropertyGridView : UserControl
    {

        private bool isLoad = true;
        public UOPPropertyGridView()
        {
            InitializeComponent();
            Table = new uopTable(10, 10);
        }



        public uopTable Table
        {
            get { return (uopTable)GetValue(TableProperty); }
            set { SetValue(TableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Table.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TableProperty =
            DependencyProperty.Register("Table", typeof(uopTable), typeof(UOPPropertyGridViewModel), new PropertyMetadata(new uopTable(10, 10),propertyChangedCallback: TableChangedCallback));
        private static void TableChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UOPPropertyGridView usercntrl = (UOPPropertyGridView)d;
            usercntrl.Table = e.NewValue as uopTable;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var VM = DataContext as UOPPropertyGridViewModel;
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
            for (int i = 0; i < downcomerGrid.Items.Count; i++)
            {
                DataGridRow row = (DataGridRow)downcomerGrid.ItemContainerGenerator.ContainerFromIndex(i);
                if (row != null)
                {
                    if (i == index - 1)
                    {
                        downcomerGrid.SelectedItem = downcomerGrid.Items[i];
                    }
                    else row.Background = Brushes.White;
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
            UOPPropertyGridViewModel VM = this.DataContext as UOPPropertyGridViewModel;
            var datagrid = sender as DataGrid;
            if (datagrid == null) return;
            var index = datagrid.SelectedIndex + 1;
            if (isLoad)
            {
                //VM.SelectionChanged(index);
            }

        }
    }
}

