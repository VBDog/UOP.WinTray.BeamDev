using UOP.WinTray.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UOP.WinTray.UI.Model;

namespace UOP.WinTray.UI.Views
{
    /// <summary>
    /// Interaction logic for UOPPropertyTreeView.xaml
    /// </summary>
    public partial class UOPPropertyTreeView : UserControl 
    {
        //private readonly Style treeViewStyle;
        public UOPPropertyTreeView()
        {
            InitializeComponent();
            //TreeViewNodes = new ObservableCollection<TreeViewNode>();
            //treeViewStyle = FindResource("TreeViewStyle") as Style;
            tvProperties.ItemContainerStyle = FindResource("DefaultTreeViewStyle") as Style;
        }

        



        //public ObservableCollection<TreeViewNode> TreeViewNodes
        //{
        //    get { return (ObservableCollection<TreeViewNode>)GetValue(TreeViewNodesProperty); }
        //    set { SetValue(TreeViewNodesProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for TreeViewNodes.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty TreeViewNodesProperty =
        //    DependencyProperty.Register("TreeViewNodes", typeof(ObservableCollection<TreeViewNode>), typeof(UOPPropertyTreeView));

        

        private void TreeNodeButton_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender == null) return;
            UOPPropertyTreeViewModel VM = DataContext as UOPPropertyTreeViewModel;

            Button btn = sender as Button;
            TreeViewNode node = btn.DataContext as TreeViewNode;
            VM.RespondToNodeDoubleClick(node);

        }

        private void TreeNodeButton_GotFocus(object sender, RoutedEventArgs e)
        {

            if (sender == null) return;
            UOPPropertyTreeViewModel VM = DataContext as UOPPropertyTreeViewModel;
            Button btn = sender as Button;
            TreeViewNode node = btn.DataContext as TreeViewNode;

            VM.RespondToNodeGotFocus(node);
        }
    }
}
