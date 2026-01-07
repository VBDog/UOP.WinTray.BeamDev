using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;


namespace UOP.WinTray.UI.Behaviors
{
    /// <summary>
    /// Class implementing useful behaviors for the ListView control.
    /// </summary>
    public static class ListViewBehaviors
    {

        /// <summary>
        /// Updates the column widths of a ListView to fit the contents.
        /// </summary>
        /// <param name="listView">ListView to update.</param>
        public static void UpdateColumnWidths(System.Windows.Controls.ListView listView, List<string> aSkipList = null)
        {
            if (null == listView) return;
            try { UpdateColumnWidths(listView.View as GridView, aSkipList); } catch { }
            
        }

        /// <summary>
        /// Updates the column widths of a GridView to fit the contents.
        /// </summary>
        /// <param name="gridView">GridView to update.</param>
        public static void UpdateColumnWidths(GridView gridView, List<string> aSkipList = null)
        {
            // Validate parameter
            if (null == gridView) return;
            if (null == aSkipList) aSkipList = new List<string>();
            // For each column...
            foreach (var column in gridView.Columns)
            {
                // If this is an "auto width" column...
                //if (double.IsNaN(column.Width))
                //{
                //    // Set its Width back to NaN so it will auto-size
                if(aSkipList.IndexOf(column.Header.ToString()) < 0)
                {
                    column.Width = 0;
                    column.Width = double.NaN;
                }

                    
                //}
            }
        }

        /// <summary>
        /// Represents the IsAutoUpdatingColumnWidths attached DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty IsAutoUpdatingColumnWidthsProperty =
            DependencyProperty.RegisterAttached(
                "IsAutoUpdatingColumnWidths",
                typeof(bool),
                typeof(ListViewBehaviors),
                new UIPropertyMetadata(false, OnIsAutoUpdatingColumnWidthsChanged));

        /// <summary>
        /// Gets the value of the IsAutoUpdatingColumnWidths property.
        /// </summary>
        /// <param name="listView">ListView for which to get the value.</param>
        /// <returns>Value of the property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "Only applies to ListView instances.")]
        public static bool GetIsAutoUpdatingColumnWidths(ListView listView)
        {
            return (bool)listView.GetValue(IsAutoUpdatingColumnWidthsProperty);
        }

        /// <summary>
        /// Sets the value of the IsAutoUpdatingColumnWidths property.
        /// </summary>
        /// <param name="listView">ListView for which to set the value.</param>
        /// <param name="value">Value of the property.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "Only applies to ListView instances.")]
        public static void SetIsAutoUpdatingColumnWidths(ListView listView, bool value)
        {
            listView.SetValue(IsAutoUpdatingColumnWidthsProperty, value);
        }

        /// <summary>
        /// Change handler for the IsAutoUpdatingColumnWidths property.
        /// </summary>
        /// <param name="o">Instance for which it changed.</param>
        /// <param name="e">Change details.</param>
        private static void OnIsAutoUpdatingColumnWidthsChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            // Get the ListView instance and new bool value
            var listView = o as ListView;
            Debug.Assert(!VirtualizingStackPanel.GetIsVirtualizing(listView),
                "VirtualizingStackPanel.IsVirtualizing should be False for " +
                "ListViewBehaviors.IsAutoUpdatingColumnWidths to work best.");
            if ((null != listView) && (e.NewValue is bool))
            {
                // Get a descriptor for the ListView's ItemsSource property
                var descriptor = DependencyPropertyDescriptor.FromProperty(ListView.ItemsSourceProperty, typeof(ListView));
                if ((bool)e.NewValue)
                {
                    // Enabling the feature, so add the change handler
                    descriptor.AddValueChanged(listView, ListViewItemsSourceValueChanged);

                    // Create a ListViewState instance for tracking
                    var listViewState = new ListViewState(listView);

                    // Hook the CollectionChanged event (if possible)
                    var iNotifyCollectionChanged = listView.ItemsSource as INotifyCollectionChanged;
                    if (null != iNotifyCollectionChanged)
                    {
                        iNotifyCollectionChanged.CollectionChanged +=
                            new NotifyCollectionChangedEventHandler(ListViewItemsSourceCollectionChanged);
                        listViewState.INotifyCollectionChanged = iNotifyCollectionChanged;
                    }

                    // Track the instance
                    _listViewStates.Add(listViewState);
                }
                else
                {
                    // Disabling the feature, so remove the change handler
                    descriptor.RemoveValueChanged(listView, ListViewItemsSourceValueChanged);

                    // Look up the corresponding ListViewState instance
                    var listViewState = _listViewStates
                        .Where(lvs => listView == lvs.ListView)
                        .FirstOrDefault();
                    if (null != listViewState)
                    {
                        // Unhook the CollectionChanged event (if present)
                        var iNotifyCollectionChanged = listViewState.INotifyCollectionChanged;
                        if (null != iNotifyCollectionChanged)
                        {
                            iNotifyCollectionChanged.CollectionChanged -=
                                new NotifyCollectionChangedEventHandler(ListViewItemsSourceCollectionChanged);
                            listViewState.INotifyCollectionChanged = null;
                        }
                    }

                    // Remove the ListViewState (and any other unnecessary ones)
                    foreach (var lvs in _listViewStates
                        .Where(lvs => (listView == lvs.ListView) || (null == lvs.ListView))
                        .ToArray()) // ToArray avoids modifying the current collection
                    {
                        _listViewStates.Remove(lvs);
                    }
                }
            }
        }

        /// <summary>
        /// Handles changes to the ListView's ItemsSource and updates the column widths.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event args.</param>
        private static void ListViewItemsSourceValueChanged(object sender, EventArgs e)
        {
            // Get a reference to the ListView
            var listView = sender as ListView;
            if (null != listView)
            {
                // Look up the corresponding ListViewState instance
                var listViewState = _listViewStates
                    .Where(lvs => listView == lvs.ListView)
                    .FirstOrDefault();
                if (null != listViewState)
                {
                    // Unhook the CollectionChanged event (if present)
                    var oldINotifyCollectionChanged = listViewState.INotifyCollectionChanged;
                    if (null != oldINotifyCollectionChanged)
                    {
                        oldINotifyCollectionChanged.CollectionChanged -=
                            new NotifyCollectionChangedEventHandler(ListViewItemsSourceCollectionChanged);
                        listViewState.INotifyCollectionChanged = null;
                    }

                    // Hook the new CollectionChanged event (if possible)
                    var newINotifyCollectionChanged = listView.ItemsSource as INotifyCollectionChanged;
                    if (null != newINotifyCollectionChanged)
                    {
                        newINotifyCollectionChanged.CollectionChanged +=
                            new NotifyCollectionChangedEventHandler(ListViewItemsSourceCollectionChanged);
                        listViewState.INotifyCollectionChanged = newINotifyCollectionChanged;
                    }

                    // Get a reference to the ListView's GridView
                    var gridView = listView.View as GridView;
                    if (null != gridView)
                    {
                        // Update the ListView's column widths
                        UpdateColumnWidths(gridView);
                    }
                }
            }
        }

        /// <summary>
        /// Handles changes to the ListView's ItemsSource's CollectionChanged event and updates the column widths.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event args.</param>
        private static void ListViewItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Update the corresponding GridView by looking up the ListView by ItemsSource
            foreach (var gridView in _listViewStates
                    .Select(lvs => lvs.ListView)
                    .Where(lv => (null != lv) && (sender == lv.ItemsSource) && (lv.View is GridView))
                    .Select(lv => (GridView)lv.View))
            {
                // Update the ListView's column widths
                UpdateColumnWidths(gridView);
            }
        }

        /// <summary>
        /// Stores a collection of ListViewState instances.
        /// </summary>
        private static readonly List<ListViewState> _listViewStates = new();

        /// <summary>
        /// Stores state information about a ListView that allows ListViewItemsSourceCollectionChanged
        /// to map from a collection to the corresponding ListView owner.
        /// </summary>
        private class ListViewState
        {
            /// <summary>
            /// Weakly references the ListView.
            /// </summary>
            private readonly WeakReference _listViewReference = new(null);

            /// <summary>
            /// Gets or sets the ListView.
            /// </summary>
            public ListView ListView
            {
                get { return (ListView)_listViewReference.Target; }
                private set { _listViewReference.Target = value; }
            }

            /// <summary>
            /// Weakly references the INotifyCollectionChanged.
            /// </summary>
            private readonly WeakReference _iNotifyCollectionChangedReference = new(null);

            /// <summary>
            /// Gets or sets the INotifyCollectionChanged.
            /// </summary>
            public INotifyCollectionChanged INotifyCollectionChanged
            {
                get { return (INotifyCollectionChanged)_iNotifyCollectionChangedReference.Target; }
                set { _iNotifyCollectionChangedReference.Target = value; }
            }

            /// <summary>
            /// Creates a new instance of the ListViewState class.
            /// </summary>
            /// <param name="listView">Corresponding ListView.</param>
            public ListViewState(ListView listView)
            {
                ListView = listView;
            }
        }
    }
}
