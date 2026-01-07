using UOP.WinTray.UI.Interfaces;
using UOP.WinTray.UI.Views.Windows;
using MvvmDialogs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Documents;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Parts;

using UOP.WinTray.UI.Messages;
using UOP.WinTray.UI.Utilities.Constants;
using System;
using DocumentFormat.OpenXml.Office2016.Excel;
using UOP.WinTray.Projects.Utilities.ExtensionMethods;

namespace UOP.WinTray.UI.Model
{
    /// <summary>
    /// project teee model class
    /// </summary>
    public class ProjectTreeModel : BindingObject, IProjectTreeModel
    {
        #region Constants

       
        private const string STAGE = "STAGE({0})";
       
        #endregion

        #region Variables




        private readonly IDialogService _DialogService;

        #endregion Variables

        #region Constructors


        public ProjectTreeModel(uopProject project, IDialogService dialogService)
        {
            Project = project;
            _DialogService = dialogService;
        }
        public ProjectTreeModel(IDialogService dialogService, uopProject aProject)
        {
            Project = aProject;
            _DialogService = dialogService;
        }
        #endregion Constructors

        #region Properties

        private Message_Refresh Refresh { get; set; }


        private string _DisplayName;
        public string DisplayName { get => _DisplayName; set { _DisplayName = value; NotifyPropertyChanged("DisplayName"); } }


        private ObservableCollection<TreeViewItemViewModel> _Children;
        public ObservableCollection<TreeViewItemViewModel> Children
        {
            get => _Children;
            set { _Children = value; NotifyPropertyChanged("Children"); }
        }


        public int WarningCount { get; set; }


        private ObservableCollection<TreeViewNode> _TreeViewNodes;
    public ObservableCollection<TreeViewNode> TreeViewNodes
        {
            get => _TreeViewNodes;
            set => _TreeViewNodes = value; 
        }

        private uopProject _Project;
        public uopProject Project
        {
            get => _Project;
            set => _Project = value;
        }

        public mdProject MDProject { get { if (_Project == null) return null; return (_Project.ProjectFamily == uppProjectFamilies.uopFamMD) ? (mdProject)_Project : null; } }
        #endregion Properties



        #region Methods


        /// <summary>
        /// Create the Stage node
        /// </summary>
        /// <param name="projectNode"></param>
        public TreeViewNode GetNodes_Stages(TreeViewNode projectNode, mdProject project)
        {
            TreeViewNode node = null;
            try
            {
                if (project == null)
                {
                    node = new TreeViewNode("STAGES", uppPartTypes.Stages, aCollector: _NodeCollector) { Path = "Project.Stages" }; 
                }
                else
                {
                    Refresh?.SetStatusMessages(null, "Adding Stage Nodes");
                    colMDStages stages = project.Stages;

                    node = new TreeViewNode("STAGES", stages, projectNode, aCollector: _NodeCollector) { Path = "Project.Stages" };
                  
                    for (int i = 1; i <= stages.Count; i++)
                    {
                        TreeViewNode stagenode = new TreeViewNode(string.Format(STAGE, i), stages.Item(i), node, i, aCollector: _NodeCollector) { Path = $"Project.Stages.Item({i})" };
                    }
                }
                //projectNode.Members.Add(node);

            }
            catch (Exception ex)
            {
                Refresh?.AddWarning(null, $"{System.Reflection.MethodBase.GetCurrentMethod()}", ex.Message);
            }


            return node;

        }

        /// <summary>
        /// Create the Distribute note
        /// </summary>
        /// <param name="project"></param>
        public TreeViewNode GetNodes_MDDistributors(TreeViewNode projectNode, mdProject project)
        {
            TreeViewNode node = null;
            try
            {
                if (project == null)
                {
                    node = new TreeViewNode("DISTRIBUTORS", aPartType: uppPartTypes.Distributors, aCollector: _NodeCollector) { Path = "Project.Distributors" };

                }
                else
                {
                    Refresh?.SetStatusMessages(null, "Adding Distributor Nodes");
                    colMDDistributors distributors = project.Distributors;
                    node = new TreeViewNode("DISTRIBUTORS", distributors, projectNode, aCollector: _NodeCollector) { Path = "Project.Distributors" };
                    int cnt = (distributors != null) ? distributors.Count : 0;
                    for (int i = 1; i <= cnt; i++)
                    {
                        mdDistributor aMember = distributors.Item(i);
                        var childNode = new TreeViewNode(aMember.Description.ToUpper(), aMember, node, i, aCollector: _NodeCollector) { Path = $"Project.Distributors.Item({i})" };
                        for (int j = 1; j <= aMember.Cases.Count; j++)
                        {

                            var casenode = new TreeViewNode($"CASE({j})", aMember.Cases.Item(j), childNode, j) { Path = $"{childNode.Path}.Case({j})" };
                        }
                        //node.Members.Add(childNode);
                    }
                }

            }
            catch (Exception ex)
            {
                Refresh?.AddWarning(null, $"{System.Reflection.MethodBase.GetCurrentMethod()}", ex.Message);
            }
          
           
            //projectNode.Members.Add(node);
            return node;
        }

        /// <summary>
        /// Create the Chimney Tray node
        /// </summary>
        /// <param name="projectNode"></param>
        public TreeViewNode GetNodes_MDChimneyTrays(TreeViewNode projectNode, mdProject project)
        {
            TreeViewNode node = null;
            try
            {
                if (project == null)
                {
                    node = new TreeViewNode("CHIMNEY TRAYS", aCollector: _NodeCollector) { Path = "Project.ChimneyTrays" };
                }
                else
                {
                    Refresh?.SetStatusMessages(null, "Adding Chimney Tray Nodes");

                    colMDChimneyTrays chimneyTrays = project.ChimneyTrays;
                    node = new TreeViewNode("CHIMNEY TRAYS", chimneyTrays, projectNode, aCollector: _NodeCollector) { Path = "Project.ChimneyTrays" };

                    for (int i = 1; i <= chimneyTrays.Count; i++)
                    {
                        mdChimneyTray aMember = chimneyTrays.Item(i);
                        var childNode = new TreeViewNode(aMember.Description.ToUpper(), aMember, node, i, aCollector: _NodeCollector) { Path = $"Project.ChimneyTrays.Item({i})" };
                        for (int j = 1; j <= aMember.Cases.Count; j++)
                        {

                            var casenode = new TreeViewNode($"CASE({j})", aMember.Cases.Item(j), childNode, j, aCollector: _NodeCollector) { Path = $"{childNode.Path}.Case({j})" };

                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Refresh?.AddWarning(null, $"{System.Reflection.MethodBase.GetCurrentMethod()}", ex.Message);
            }

            
            return node;
        }


        /// <summary>
        /// get the Tray Sections node
        /// </summary>
        /// <param name="parentNode"></param>
        public TreeViewNode GetNodes_MDTrays(TreeViewNode parentNode,mdProject project)
        {
            TreeViewNode node = null;
            try
            {
                if (project == null)
                {
                    node = new TreeViewNode("TRAY SECTIONS", aCollector: _NodeCollector) { Path = "Project.Column.TrayRanges" };
                }
                else
                {
                    Refresh?.SetStatusMessages(null, "Adding Tray Range Nodes");
                    colUOPTrayRanges TrayRanges = project.TrayRanges;
                    node = new TreeViewNode("TRAY SECTIONS", TrayRanges, parentNode, aCollector: _NodeCollector) { Path = "Project.Column.TrayRanges", IsExpanded = true };

                    for (int i = 1; i <= TrayRanges.Count; i++)
                    {

                        mdTrayRange aMember = (mdTrayRange)TrayRanges.Item(i);
                        var firstChildNode = new TreeViewNode(aMember.Name(true).ToUpper(), aMember, node, i, aCollector: _NodeCollector) { Path = $"Project.Column.TrayRanges.Item({i})" };
                        //if (TrayRanges.Item(i).Name(true) == TrayRanges.SelectedRange?.Name(true))
                        //{
                        //    firstChildNode.Colour = BLUE;
                        //}
                        var assy = aMember.TrayAssembly;

                        var secondChildNode = new TreeViewNode("TRAY ASSY.", assy, firstChildNode, i, aCollector: _NodeCollector) { Path = $"Project.Column.TrayRanges.Item({i}).TrayAssembly" };

                        var deckNode = new TreeViewNode("DECK", assy.Deck, secondChildNode, i, aCollector: _NodeCollector) { Path = $"Project.Column.TrayRanges.Item({i}).TrayAssembly.Deck" };
                        if (assy.DesignFamily.IsBeamDesignFamily())
                        {
                            var beamNode = new TreeViewNode("BEAM", assy.Beam, secondChildNode, i, aCollector: _NodeCollector) { Path = $"Project.Column.TrayRanges.Item({i}).TrayAssembly.Beam" };
                        }

                        var dpNode = new TreeViewNode("DESIGN OPTIONS", assy.DesignOptions, secondChildNode, i, aCollector: _NodeCollector) { Path = $"Project.Column.TrayRanges.Item({i}).TrayAssembly.DesignOptions" };

                        var downcomerNode = new TreeViewNode("DOWNCOMER", assy.Downcomer(), secondChildNode, i, aCollector: _NodeCollector) { Path = $"Project.Column.TrayRanges.Item({i}).TrayAssembly.Downcomer" };

                        var dcs = assy.Downcomers.GetByVirtual(false);

                        var downcomersNode = new TreeViewNode("DOWNCOMERS", null, secondChildNode, i, aCollector: _NodeCollector) { Path = $"Project.Column.TrayRanges.Item({i}).Downcomers" };
                        TreeViewNode thirdChildNode;

                        for (int j = 1; j <= dcs.Count; j++)
                        {

                            thirdChildNode = new TreeViewNode($"DOWNCOMER({j})", dcs[j - 1], downcomersNode, j, aCollector: _NodeCollector) { Path = $"Project.Column.TrayRanges.Item({i}).Downcomers({j})" };

                        }
                        var SGs = aMember.TrayAssembly.SpoutGroups.GetByVirtual(false);

                        var spoutGroupsNode = new TreeViewNode("SPOUT GROUPS", null, secondChildNode, aCollector: _NodeCollector) { Path = $"Project.Column.TrayRanges.Item({i}).SpoutGroups" };


                        for (int j = 1; j <= SGs.Count; j++)
                        {
                            var sG = SGs[j - 1];
                            thirdChildNode = new TreeViewNode($"SG({sG.Handle})", sG, spoutGroupsNode, j, aCollector: _NodeCollector) { Path = $"Project.Column.TrayRanges.Item({i}).SpoutGroups.Item({j})" };

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Refresh?.AddWarning(null, $"{System.Reflection.MethodBase.GetCurrentMethod()}", ex.Message);
            }
      


            

            return node;
        }

      
        private List<TreeViewNode> _NodeCollector;
        /// <summary>
        /// Generate  the tree view node
        /// </summary>
        /// <param name="aProject">the subject project</param>
        /// <param name="refresh"> a refresh message to use</param>
        /// <param name="rAllNodes"> return a list which comtains all of the created nodes</param>
        /// <param name="bForceDocumentRefresh"> flag to force the project to regenerate its documents collections</param>
        /// <returns></returns>
        public ObservableCollection<TreeViewNode> GetTreeViewNodes(uopProject aProject, Message_Refresh refresh, out List<TreeViewNode> rAllNodes)
        {
            _NodeCollector = new List<TreeViewNode>();
            rAllNodes = new List<TreeViewNode>();
            var _rVal = new ObservableCollection<TreeViewNode>();
            if (aProject == null || refresh != null) aProject = refresh.Project;
            if (aProject == null || refresh == null) return new ObservableCollection<TreeViewNode>();
            mdProject mdproj = null;
            Refresh = refresh == null ? new Message_Refresh() : refresh;
            if (Refresh.Clear) return new ObservableCollection<TreeViewNode>();
            bool listening = (Refresh.ListenToProject == null);
            try
            {
           
                if (listening) Refresh.ListenToProject = aProject;
                    

                Refresh.SetStatusMessages("Creating Project Tree");

                TreeViewNode projectNode = new(aProject.Name, aProject, null, aCollector: _NodeCollector);

                projectNode.Path = "Project";
                GetNodes_ProjectNotes(projectNode, aProject);

                GetNodes_Customer(projectNode, aProject);

                if (aProject.ProjectFamily == uppProjectFamilies.uopFamMD)
                {

                     GetNodes_Column(projectNode, aProject);
                    mdproj = (mdProject)aProject;

              

                    if (mdproj.ProjectType == uppProjectTypes.MDSpout)
                        GetNodes_Stages(projectNode, mdproj);
                    GetNodes_MDDistributors(projectNode, mdproj);

                    GetNodes_MDChimneyTrays(projectNode, mdproj);

                    GetNodes_MDTrays(projectNode, mdproj);
                  

                    //Modified by CADfx   //MTZ this only needs to be called once as it is time consuming


                    uopDocuments projectDocs = new ();
                    try
                    {
                        Refresh.SetStatusMessages("Generating Project Documents");
                        projectDocs = aProject.Documents(refresh.ForceDocumentRefresh);
                    }
                    catch (Exception ex)
                    {
                        Refresh?.AddWarning(null, $"{System.Reflection.MethodBase.GetCurrentMethod()}", ex.Message);
                    }

                    
                    //Modified by CADfx


                    GetNodes_Documents(projectNode, mdproj, uppDocumentTypes.Calculation, projectDocs);

                    GetNodes_Documents(projectNode, mdproj, uppDocumentTypes.Report, projectDocs);

                    GetNodes_Documents(projectNode, mdproj, uppDocumentTypes.Warning, projectDocs);

                    GetNodes_Documents(projectNode, mdproj, uppDocumentTypes.Drawing, projectDocs);
                    _rVal.Add(projectNode);

                    projectNode.IsExpanded = true;
               
                }

            }
            catch (Exception ex) { Refresh?.AddWarning(null, $"{System.Reflection.MethodBase.GetCurrentMethod()}", ex.Message); }
            finally 
            {
                rAllNodes = _NodeCollector;
                if (listening) Refresh.ListenToProject = null;
                Refresh = null;

            }

            return _rVal;
        }


        /// <summary>
        /// generate the Note node
        /// </summary>
        /// <returns></returns>
        public TreeViewNode GetNodes_ProjectNotes(TreeViewNode projectNode, uopProject aProject)
        {
            TreeViewNode node = null;
            try
            {

                if (aProject == null)
                {
                    node = new TreeViewNode("NOTES", uppPartTypes.Project, aCollector: _NodeCollector) { Path = $"{projectNode?.Path}.Notes" };
                }
                else
                {
                    Refresh?.SetStatusMessages(null, "Adding Notes Nodes");

                    uopProperties notes = aProject.Notes;
                    node = new TreeViewNode("NOTES", Project, projectNode, aCollector: _NodeCollector) { Path = $"{projectNode?.Path}.Notes" };




                    for (int i = 1; i <= notes.Count; i++)
                    {
                        var notenode = new TreeViewNode($"Note({i}) {notes.Item(i).ValueS}", aProject, node, i, aCollector: _NodeCollector) { Path = $"{node.Path}.Item({i})" };
                    }
                }
      

            }
            catch (Exception ex)
            {
                Refresh?.AddWarning(null, $"{System.Reflection.MethodBase.GetCurrentMethod()}", ex.Message);
            }

            return node;
        
        }

       

        /// <summary>
        /// generate the _Column node
        /// </summary>
        /// <returns></returns>
        public TreeViewNode GetNodes_Column(TreeViewNode projectNode,uopProject project)
        {
            TreeViewNode node = null;
            try
            {
                Refresh?.SetStatusMessages(null, "Adding Column Nodes");

                if (project == null) 
                    node = new TreeViewNode("COLUMN", uppPartTypes.Column, aCollector: _NodeCollector) { Path = "Project.Column" };
                else
                    node = new TreeViewNode("COLUMN", project.Column, projectNode, aCollector: _NodeCollector) { Path = "Project.Column" };

            }
            catch (Exception ex)
            {
                Refresh?.AddWarning(null, $"{System.Reflection.MethodBase.GetCurrentMethod()}", ex.Message);
            }
      
            return node;
        }

        /// <summary>
        /// generate the _Customer node
        /// </summary>
        /// <returns></returns>
        public TreeViewNode GetNodes_Customer(TreeViewNode projectNode, uopProject project)
        {
            TreeViewNode node = null;

            try
            {
                Refresh?.SetStatusMessages(null, "Adding Customer Nodes");
                if (project == null) 
                    node = new TreeViewNode("CUSTOMER", uppPartTypes.Customer, aCollector: _NodeCollector) { Path = "project.Customer" };
                else
                    node = new("CUSTOMER", project.Customer, projectNode, aCollector: _NodeCollector) { Path = "project.Customer" };
            }
            catch (Exception ex)
            {
                Refresh?.AddWarning(null, $"{System.Reflection.MethodBase.GetCurrentMethod()}", ex.Message);
            }

            
           
            //projectNode.Members.Add(node);
            return node;
        }


        /// <summary>
        /// generates the Documents node
        /// </summary>
        /// <param name="projectNode"></param>
        /// <param name="aProject"></param>
        /// <returns></returns>
        public TreeViewNode GetNodes_Documents(TreeViewNode projectNode, mdProject aProject, uppDocumentTypes types, uopDocuments projectDocuments = null)
        {
            TreeViewNode node = null;

            try
            {
                string key = types.GetDescription();
                string path = $"Project.Documents/{key}s";

                Refresh?.SetStatusMessages(null, $"Adding {key}s Nodes");
                if (projectDocuments == null && aProject != null) projectDocuments = aProject.Documents();

                if (projectDocuments == null)
                { return node; }
                else
                {
                    node = new($"{key.ToUpper()}S", aCollector: _NodeCollector) { Colour = CommonConstants.BLACK, Path = path };

                    uopDocuments aDocs = (types == uppDocumentTypes.Warning) ? (aProject != null) ? aProject.Warnings(bJustOne: true) : new uopDocuments() : aDocs = projectDocuments.GetByDocumentType(types, true, "", true);

                    if (types == uppDocumentTypes.Warning) WarningCount = aDocs.Count;

                    if (types == uppDocumentTypes.Warning && aDocs.Count > 0 || (types == uppDocumentTypes.Report && aDocs.Count <= 0))
                    {
                        node.Colour = CommonConstants.RED;
                        projectNode.Members.Add(node);
                        return node;
                    }

                    node.Members = types switch
                    {
                        uppDocumentTypes.Calculation => GetNodes_Docs_Calculations(aDocs, null),
                        uppDocumentTypes.Report => GetNodes_Docs_Reports(aDocs, null),
                        uppDocumentTypes.Drawing => GetNodes_Docs_Drawings(aDocs, null, projectNode),
                        _ => new List<TreeViewNode>()
                    };


                    projectNode.Members.Add(node);
                }

                
            }
            catch (Exception ex)
            {
                Refresh?.AddWarning(null, $"{System.Reflection.MethodBase.GetCurrentMethod()}", ex.Message);
            }

            
            return node;
        }

        /// <summary>
        /// get the sub nodes for Caculations
        /// </summary>
        /// <param name="aDocs"></param>
        /// <returns></returns>
        public List<TreeViewNode> GetNodes_Docs_Calculations(uopDocuments aDocs, TreeViewNode aParentNode)
        {
            List<TreeViewNode> nodes = new();
            if (aDocs != null) return nodes;
            string path = aParentNode == null ? $"Project.Documents/Calculations" : aParentNode.Path;
            for(int i = 1; i <= aDocs.Count; i ++)
            {
                uopDocument doc = aDocs.Item(i);

                if(doc.DocumentType == uppDocumentTypes.Calculation)
                {
                    uopDocCalculation item = (uopDocCalculation)doc;
                    var childNode = new TreeViewNode(item.SelectText, null, aParentNode, aCollector: _NodeCollector) { Colour = getColour(item.Hidden), PartType = item.PartType, DocumentType = uppDocumentTypes.Calculation };

                    childNode.Path = $"{path}/{childNode.NodeName}";
                    item.NodePath = childNode.Path;
                    nodes.Add(childNode);
                }
            }
                
            
            return nodes;
        }

        /// <summary>
        /// Get the sub notes for Reports
        /// </summary>
        /// <param name="aDocs"></param>
        /// <returns></returns>
        public List<TreeViewNode> GetNodes_Docs_Reports(uopDocuments aDocs, TreeViewNode aParentNode)
        {

            List<TreeViewNode> nodes = new();
            if (aDocs == null) return nodes;
            string path = aParentNode == null ? $"Project.Documents/Reports" : aParentNode.Path;
            for (int i = 1; i <= aDocs.Count; i++)
            {
                uopDocument doc = aDocs.Item(i);

                if (doc.DocumentType == uppDocumentTypes.Report)
                {
                    uopDocReport item = (uopDocReport)doc;

                    var childNode = new TreeViewNode(item.SelectName, null, aParentNode, aCollector: _NodeCollector) { Colour = getColour(item.Hidden), PartType = item.PartType, DocumentType = uppDocumentTypes.Report };
                    childNode.Path = $"{path}/{childNode.NodeName}";
                    item.NodePath = childNode.Path;
                    if (!item.Hidden)
                        nodes.Add(childNode);
                }
            }

            return nodes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isHidden"></param>
        /// <returns></returns>
        public string getColour(bool isHidden)
        {
            if (isHidden)
            {
                return CommonConstants.GRAY;
            }
            else
                return CommonConstants.BLACK;
        }

        /// <summary>
        /// get drawing sub nodes
        /// </summary>
        /// <param name="aDocs"></param>
        /// <returns></returns>
        public List<TreeViewNode> GetNodes_Docs_Drawings(uopDocuments aDocs, TreeViewNode aParentNode, TreeViewNode projectNode)
        {
            string path = aParentNode == null ? $"Project.Documents/Drawings" : aParentNode.Path;
            List<TreeViewNode> nodes = new();
            if (Project == null) return nodes;
            Dictionary<string, TreeViewNode> catnodes = new();
            Dictionary<string, TreeViewNode> subcatnodes = new();
            List<uopDocDrawing> drawings = aDocs.Drawings;

            List<string> categories = aDocs.Categories(bUniqueValues: true, bIncludeNullString: true);
            List<string> subcategories = aDocs.SubCategories(bUniqueValues: true, bIncludeNullString: true);
            TreeViewNode subcatNode;
            TreeViewNode catNode;
            List<uopDocDrawing> nocats;
            if (aParentNode == null )
            {
                aParentNode = _NodeCollector.Find((x) => x.NodeName == "DRAWINGS");
                if(aParentNode == null)
                {
                    aParentNode = new("DRAWINGS", null, projectNode, aCollector: _NodeCollector);
                    aParentNode.Path = $"Project.Documents/Drawings";
                    nodes.Add(aParentNode);
                }
              
            }
            
            // get the drawings that are going to be under the main DRAWINGS node
                nocats =  drawings.FindAll((x) => string.IsNullOrWhiteSpace(x.Category) && string.IsNullOrWhiteSpace(x.SubCategory));
            foreach (uopDocDrawing nocat in nocats)
            {
                drawings.Remove(nocat);
                TreeViewNode nocatNode = new(nocat.Name, null, aParentNode,  aCollector: _NodeCollector, aDocument: nocat) { Colour = getColour(nocat.Hidden), DocumentType = uppDocumentTypes.Drawing, DocumentName = nocat.Name };
                nocatNode.Path = $"{aParentNode.Path}/{nocatNode.NodeName}";
                nodes.Add(nocatNode);
                     
            }
            categories.RemoveAll((x) => string.IsNullOrWhiteSpace(x));
                       
            if (drawings.Count <= 0) return nodes;

            foreach (string cat in categories)
            {
                subcatNode = null;
                         
                catNode = new TreeViewNode(cat.ToUpper(), null, aParentNode, aCollector: _NodeCollector);
                catNode.Path = $"{aParentNode.Path}/{catNode.NodeName}";
                catnodes.Add(cat.ToUpper(), catNode);

                // get the drawings that are going to be under the main CATEGORY node
                nocats = drawings.FindAll((x) => (string.Compare(x.Category,cat,true) == 0) && string.IsNullOrWhiteSpace(x.SubCategory));
                foreach (uopDocDrawing nocat in nocats)
                {
                    drawings.Remove(nocat);
                    TreeViewNode nocatNode = new(nocat.Name, null, catNode, aCollector: _NodeCollector, aDocument: nocat) { Colour = getColour(nocat.Hidden), DocumentType = uppDocumentTypes.Drawing, DocumentName = nocat.Name };
                    nocatNode.Path = $"{catNode.Path}/{nocatNode.NodeName}";

                }


                List<uopDocDrawing> catdocs = drawings.FindAll((x) => string.Compare(x.Category, cat, true) == 0);


                //nodes.Add(catNode);

                foreach (string subcat in subcategories)
                {
                    List<uopDocDrawing> subcatdocs = catdocs.FindAll((x) => string.Compare(x.SubCategory, subcat, true) == 0); // aDocs.GetByCategory(cat, subcat, bRemove: true, aSearchList: catdocs);
                    string subcatname = string.IsNullOrWhiteSpace(cat)  ?  $"Project.Documents/Drawings" : $"{cat.ToUpper()}##{subcat.ToUpper()}";
                                
                    if (subcatdocs.Count > 0)
                    {

                        if (string.IsNullOrEmpty(subcat) || subcatname == $"Project.Documents/Drawings")
                        {
                            subcatNode = catNode;
                        }
                        else
                        {

                            if (!subcatnodes.TryGetValue(subcatname, out subcatNode))
                            {
                                subcatNode = new TreeViewNode(subcat, null, catNode, aCollector: _NodeCollector);
                                subcatNode.Path = $"{catNode.Path}/{subcatNode.NodeName}";
                                subcatnodes.Add(subcatname, subcatNode);
                                //savenode = true;


                            }
                        }

                        foreach (uopDocDrawing item in subcatdocs)
                        {
                            TreeViewNode docNode = new(item.SelectName, null, subcatNode, aCollector: _NodeCollector, aDocument: item) { Colour = getColour(item.Hidden), DocumentType = uppDocumentTypes.Drawing, DocumentName = item.Name };
                            docNode.Path = $"{subcatNode.Path}/{item.Name}";

                            docNode.ToolTip = item.ToolTip;
                        }
                 
                    }
                }


            }
            foreach (TreeViewNode item in catnodes.Values)
            {
                nodes.Add(item);
            }
            return nodes;
        }

   


        #endregion Methods

        #region Shared Methods

        public static TreeViewNode FindTreeNode(ObservableCollection<TreeViewNode> aNodes,string aNodePath)
        {
            if (aNodes == null) return null;
            TreeViewNode node = null;

        

            int level = 1;
            foreach (var item1 in aNodes)
            {
                node = item1.Members.Find(x => string.Compare(x.Path, aNodePath, true) == 0);
                if (node == null)
                {
                    level++;
                    foreach (var item2 in item1.Members)
                    {
                        node = item2.Members.Find(x => string.Compare(x.Path, aNodePath, true) == 0);
                        if (node == null)
                        {
                            level++;
                            foreach (var item3 in item2.Members)
                            {
                                node = item3.Members.Find(x => string.Compare(x.Path, aNodePath, true) == 0);
                                if (node == null)
                                {
                                    level++;
                                    foreach (var item4 in item3.Members)
                                    {
                                        node = item4.Members.Find(x => string.Compare(x.Path, aNodePath, true) == 0);

                                        if (node == null)
                                        {
                                            level++;
                                            foreach (var item5 in item4.Members)
                                            {
                                                node = item5.Members.Find(x => string.Compare(x.Path, aNodePath, true) == 0);
                                                if (node != null)
                                                    return node;
                                            }
                                        }
                                        else
                                        { return node; }
                                        if (node != null) return node;
                                    }
                                    if (node != null) return node;
                                }
                                else
                                { return node; }
                                if (node != null) return node;
                            }
                            if (node != null) return node;
                        }
                        else
                        { return node; }
                        if (node != null) return node;
                    }
                    if (node != null) return node;
                }
                else
                { return node; }
                if (node != null) return node;
            }
        
            return null;
        }

        #endregion Shared Methods

    }
}
