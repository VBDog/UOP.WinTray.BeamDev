using UOP.WinTray.UI.Events.EventAggregator;
using UOP.WinTray.UI.Messages;
using UOP.WinTray.UI.Model;
using MvvmDialogs;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace UOP.WinTray.UI.ViewModels
{
    public class UOPPropertyTreeViewModel : ViewModel_Base, IEventSubscriber<Message_RefreshControls>
    {

        public delegate void NodeDoubleClick(TreeViewNode aNode);
        public event NodeDoubleClick NodeDoubleClickEvent;

        public delegate void NodeGotFocus(TreeViewNode aNode);
        public event NodeGotFocus NodeGotFocusEvent;

        internal UOPPropertyTreeViewModel(IEventAggregator eventAggregator, IDialogService dialogService) : base( eventAggregator, dialogService)
        {

        }

        internal UOPPropertyTreeViewModel() : base()
        {

        }

        public TreeViewNode CurrentNode { get; set; }

        private ObservableCollection<TreeViewNode> _TreeViewNodes = new();
        public ObservableCollection<TreeViewNode> TreeViewNodes
        {
            get => _TreeViewNodes;
            set { _TreeViewNodes = value; NotifyPropertyChanged("TreeViewNodes"); _AllNodes = null; }
        }
        private List<TreeViewNode> _AllNodes;
        public List<TreeViewNode> AllNodes
        {
            get 
            {
                if(_AllNodes == null) GetAllNodes(); 
                return _AllNodes;
            }

            set 
            { 
                _AllNodes = value;
            NotifyPropertyChanged("AllNodes");
            }
        }

        internal void RespondToNodeDoubleClick(string aNodePath)
        {
            TreeViewNode node = AllNodes.Find(x => string.Compare(x.Path, aNodePath, true) == 0);
            if(node != null) NodeDoubleClickEvent?.Invoke(node);

        }

        internal void RespondToNodeDoubleClick(TreeViewNode aNode)
        {
        
            if (aNode != null) NodeDoubleClickEvent?.Invoke(aNode);

        }

        internal void RespondToNodeGotFocus(string aNodePath)
        {
            TreeViewNode node = AllNodes.Find(x => string.Compare(x.Path, aNodePath, true) == 0);
            if (node != null) NodeGotFocusEvent?.Invoke(node);

        }

        internal void RespondToNodeGotFocus(TreeViewNode aNode)
        {
            CurrentNode = aNode;
            if (aNode != null) NodeGotFocusEvent?.Invoke(aNode);

        }
        /// <summary>
        ///  Event handler to update the column widths
        /// </summary>
        /// <param name="message"></param>
        public void OnAggregateEvent(Message_RefreshControls message) { }

        private void GetAllNodes()
        {
            if(_TreeViewNodes == null)
            {
                _AllNodes = null;
                return;
            }
            _AllNodes = new List<TreeViewNode>();
            foreach (var item in _TreeViewNodes)
            {
                _AllNodes.Add(item);
                foreach (var item1 in item.Members)
                {
                    _AllNodes.Add(item1);
                    foreach (var item2 in item1.Members)
                    {
                        _AllNodes.Add(item2);
                        foreach (var item3 in item2.Members)
                        {
                            _AllNodes.Add(item3);
                            foreach (var item4 in item3.Members)
                            {
                                _AllNodes.Add(item4);
                                foreach (var item5 in item4.Members)
                                {
                                    _AllNodes.Add(item5);
                                    foreach (var item6 in item5.Members)
                                    {
                                        _AllNodes.Add(item6);
                                        foreach (var item7 in item6.Members)
                                        {
                                            _AllNodes.Add(item7);
                                            foreach (var item8 in item7.Members)
                                            {
                                                _AllNodes.Add(item8);

                                            }

                                        }
                                       

                                    }

                                }
                            }
                        }

                    }
                }
            }
        }
    }
    
}
