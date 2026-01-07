using UOP.WinTray.Projects;
using UOP.WinTray.UI.Model;
using UOP.WinTray.UI.Views.Windows;
using MvvmDialogs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UOP.WinTray.UI.Messages;
using UOP.WinTray.UI.Interfaces;

namespace UOP.WinTray.UI.ViewModels
{
    /// <summary>
    /// Class for Tree view 
    /// </summary>
    public class ProjectTreeViewModel : TreeViewItemViewModel
    {
        #region Variables


        private readonly IDialogService _DialogService;
        private readonly IProjectTreeModel _ProjectTreeModel;
        #endregion
        #region Constructors

        public ProjectTreeViewModel()
        {
            _ProjectTreeModel = new ProjectTreeModel(null, _DialogService);
        }


        /// <summary>
        /// Constractor with storageservice
        /// </summary>
        /// <param name="project"></param>
        public ProjectTreeViewModel(uopProject project, IDialogService dialogService)
        {

            _DialogService = dialogService;
            _ProjectTreeModel = new ProjectTreeModel(project, _DialogService);
        }





        #endregion Constructors

        #region Properties




        public string DisplayName
        {
            get => _ProjectTreeModel.DisplayName;
            set { _ProjectTreeModel.DisplayName = value; NotifyPropertyChanged("DisplayName"); }
        }


        private ObservableCollection<TreeViewNode> _TreeViewNodes;
        public ObservableCollection<TreeViewNode> TreeViewNodes
        {
            get => _TreeViewNodes;
            set { _TreeViewNodes = value; NotifyPropertyChanged("TreeViewNodes"); }
        }


        public new uopProject Project
        {
            get => _ProjectTreeModel.Project;
            set => _ProjectTreeModel.Project = value;
        }
        #endregion Properties



        #region Method



        /// <summary>
        /// Get the tree view data
        /// </summary>
        /// <param name="aProject"></param>
        public ObservableCollection<TreeViewNode> GetTreeViewNodes(uopProject aProject, Message_Refresh refresh)
        {

            TreeViewNodes = (refresh != null) ? _ProjectTreeModel.GetTreeViewNodes(aProject, refresh, out List<TreeViewNode> _) : new ObservableCollection<TreeViewNode>(); ;
            return TreeViewNodes;
        }

        public int WarningCount { get { return (_ProjectTreeModel == null) ? 0 : _ProjectTreeModel.WarningCount; } }


        #endregion Methods


    }
}
