using UOP.WinTray.UI.Views.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UOP.WinTray.Projects;
using UOP.WinTray.UI.Messages;
using UOP.WinTray.UI.Model;

namespace UOP.WinTray.UI.Interfaces
{
    public interface IProjectTreeModel
    {
        
     
     
        string DisplayName { get; set; }

        ObservableCollection<TreeViewItemViewModel> Children { get; set; }
        ObservableCollection<TreeViewNode> TreeViewNodes { get; set; }
        ObservableCollection<TreeViewNode> GetTreeViewNodes(uopProject aProject, Message_Refresh refresh, out List<TreeViewNode> rAllNodes);

    
        uopProject Project { get; set; }
        int WarningCount { get; set; }
    }
}
