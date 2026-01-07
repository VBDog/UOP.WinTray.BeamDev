using UOP.WinTray.UI.ViewModels;
using System.Collections.ObjectModel;

namespace UOP.WinTray.UI.Views.Windows
{
    /// <summary>
    /// Base class for all tree view view model
    /// </summary>
    public class TreeViewItemViewModel : ViewModel_Base
    {


        public TreeViewItemViewModel()
        {
            children = new ObservableCollection<TreeViewItemViewModel>();
        }

        private ObservableCollection<TreeViewItemViewModel> children;
        public ObservableCollection<TreeViewItemViewModel> Children
        {
            get => children;
            set { children = value; NotifyPropertyChanged("Children"); }
        }
    }
}
