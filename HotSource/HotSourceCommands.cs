using EnvDTE;
using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.Controls.WPF.TeamExplorer;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using Task = System.Threading.Tasks.Task;

namespace HotSource
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class HotSourceCommands
    {
        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static HotSourceCommands Instance { get; private set; }

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider { get { return this.package; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="HotSourceCommands"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private HotSourceCommands(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            AddGlobalCommandHandlers(commandService);
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Verify the current thread is the UI thread - the call to AddCommand in HotSource's constructor requires
            // the UI thread.
            ThreadHelper.ThrowIfNotOnUIThread();

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new HotSourceCommands(package, commandService);
        }

        private void AddGlobalCommandHandlers(OleMenuCommandService commandService)
        {
            // Add command handlers for global commands
            commandService.AddCommand(CreateMenuCommand(CommandIDs.CommitCmdId, this.ExecCommit));
            commandService.AddCommand(CreateMenuCommand(CommandIDs.PushCmdId, this.ExecPush));
            commandService.AddCommand(CreateMenuCommand(CommandIDs.PullCmdId, this.ExecPull));
            commandService.AddCommand(CreateMenuCommand(CommandIDs.FetchCmdId, this.ExecFetch));
            commandService.AddCommand(CreateMenuCommand(CommandIDs.SyncCmdId, this.ExecSync));
            commandService.AddCommand(CreateMenuCommand(CommandIDs.PendingChangesCmdId, this.GoToPendingChanges));
            commandService.AddCommand(CreateMenuCommand(CommandIDs.UnpushedCommitsCmdId, this.GoToSyncWindow));
            commandService.AddCommand(CreateMenuCommand(CommandIDs.NewBranchCmdId, this.ExecCreateBranch));
            commandService.AddCommand(CreateMenuCommand(CommandIDs.ManageBranchesCmdId, this.GoToBranchesWindow));
            commandService.AddCommand(CreateMenuCommand(CommandIDs.ShowBranchHistoryCmdId, this.ExecViewBranchHistory));
        }

        private MenuCommand CreateMenuCommand(int commandId, EventHandler eventHandler)
        {
            return new MenuCommand(eventHandler, new CommandID(CommandIDs.HotSourceCommandSetGuid, commandId));
        }

        private void ExecCommit(object sender, EventArgs e)
        {
            PutCaretInCommitBox();
        }

        private void ExecSync(object sender, EventArgs e)
        {
            ExecLinkInSynchronizeWindow("syncLink", "Sync");
        }

        private void ExecFetch(object sender, EventArgs e)
        {
            ExecLinkInSynchronizeWindow("thisTextLink", "Fetch");
        }

        private void ExecPull(object sender, EventArgs e)
        {
            ExecLinkInSynchronizeWindow("thisTextLink", "Pull");
        }

        private void ExecPush(object sender, EventArgs e)
        {
            ExecLinkInSynchronizeWindow("thisTextLink", "Push");
        }

        private void GoToPendingChanges(object sender, EventArgs e)
        {
            ExecuteDTECommand("View.PendingChanges");
        }

        private void ExecViewBranchHistory(object sender, EventArgs e)
        {
            //DoViewBranchHistory();
            ExecuteDTECommand("Team.Git.ViewHistory");
        }

        private void GoToSyncWindow(object sender, EventArgs e)
        {
            ExecuteDTECommand("View.UnpublishedCommits");
        }

        private void ExecCreateBranch(object sender, EventArgs e)
        {
            ExecuteDTECommand("Team.Git.CreateBranch");
        }

        private void GoToBranchesWindow(object sender, EventArgs e)
        {
            ExecuteDTECommand("Team.Git.ManageBranches");
        }

        //---------Helper methods  ------------------------------------

        private async Task ExecuteDTECommand(string commandName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            DTE dte = await package.GetServiceAsync((typeof(DTE))) as DTE;
            dte.ExecuteCommand(commandName);
        }

        private async Task PutCaretInCommitBox()
        {
            await ExecuteDTECommand("View.PendingChanges");
            // TODO: Ensure window is loaded
            SendKeys.SendWait("%{HOME}");   // Alt+Home moves focus to top of window.
            SendKeys.SendWait("{TAB 3}");
            //SendKeys.SendWait("Message goes here...");
        }

        private async Task ExecLinkInSynchronizeWindow(string textLinkName, string linkText)
        {
            //ShowGitCommitsPageInformation();

            await ExecuteDTECommand("View.UnpublishedCommits");
            //// TODO: Ensure window is loaded
            //DTE2 dte = await package.GetServiceAsync((typeof(DTE))) as DTE2;
            //var window = dte.ActiveDocument.ActiveWindow;
            //Microsoft.TeamFoundation.Controls.WPF.TeamExplorer.Framework.TeamExplorerFrame teamExplorerFrame = 
            //    package.FindToolWindow(typeof(ITeamExplorerPage), 0, true);

            //ToolWindowPane toolWindow = package.FindToolWindow(TeamExplorerWindow, 0, true);
            //WindowPane windowPane = package.FindWindowPane(typeof(ITeamExplorerPage), 0, true);

            //VisualTreeHelper.GetChild(windowPane, 0);

            DependencyObject dd = VisualTreeHelper.GetChild(System.Windows.Application.Current.MainWindow, 0);
            //DependencyObject ddd = VisualTreeHelper.GetChild(dd, 0);

            TextLink textLink = FindChildTextLink(dd, textLinkName, linkText);
            if (textLink?.Command != null && textLink.Command.CanExecute(null))
            {
                MessageBoxResult result = System.Windows.MessageBox.Show($"Perform operation: {linkText}?", "Hot Source", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK);
                if (result != MessageBoxResult.OK)
                    return;

                textLink.Command.Execute(null);
            }
            else
            {
                System.Windows.MessageBox.Show($"Unable to perform operation: {linkText}");
            }

            //ITeamExplorer teamExplorer = package.GetServiceAsync(typeof(ITeamExplorer)) as ITeamExplorer;
            //teamExplorer.NavigateToPage()
            //ITeamExplorerPage teamExplorerPage;

            //teamExplorerPage = teamExplorer.CurrentPage;

            //ICommitDetailsExt commitDetailsExt = teamExplorerPage.GetExtensibilityService(
            //   typeof(Microsoft.TeamFoundation.Git.Controls.Extensibility.ICommitDetailsExt))
            //   as Microsoft.TeamFoundation.Git.Controls.Extensibility.ICommitDetailsExt;

            //for (int i = 0; i < VisualTreeHelper.GetChildrenCount(ddd); i++)
            //{
            //    DependencyObject o = VisualTreeHelper.GetChild(ddd, i);
            //}

            //string linkToActivate = "Fetch";
            // Walk the Visual Tree looking for the TextLink object we're interested in.
            //object linkObject = FindTextLink(linkToActivate);

            //SendKeys.SendWait("%{HOME}");   // Alt+Home moves focus to top of window.
            //SendKeys.SendWait("{TAB " + tabHits + "}");
            //System.Windows.IInputElement element = Keyboard.FocusedElement;
            //if (element is UIElement foo)
            //{
            //    var isEnabled = foo.IsEnabled;
            //}
            //DTE dte = await package.GetServiceAsync((typeof(DTE))) as DTE;
            //var selection = dte.ActiveWindow.Selection;
            //if (selection != null)
            //{
            // Do stuff
            //SendKeys.SendWait("{ENTER}");
            //}
        }

        private async void ShowGitCommitsPageInformation()
        {
            string pageGuid = TeamExplorerPageIds.GitCommits;

            ITeamExplorer teamExplorer = await package.GetServiceAsync(typeof(ITeamExplorer)) as ITeamExplorer;
            ITeamExplorerPage teamExplorerPage = teamExplorer.NavigateToPage(new Guid(pageGuid), null);
            var fe = (FrameworkElement)teamExplorerPage;
            var syncTextLink = fe.FindName("syncTextLink");

            //if (teamExplorerPage.GetExtensibilityService(typeof(ICommitsExt)) is ICommitsExt commitsExt)
            //{
            //    System.Collections.Generic.IReadOnlyList<ICommitsCommitItem> asda = commitsExt.IncomingCommits;
            //    //ShowCommits("IncomingCommits", commitsExt.IncomingCommits);
            //    //ShowCommits("OutgoingCommits", commitsExt.OutgoingCommits);
            //    //ShowCommits("SelectedIncomingCommits", commitsExt.SelectedIncomingCommits);
            //    //ShowCommits("SelectedOutgoingCommits", commitsExt.SelectedOutgoingCommits);
            //}
        }

        private object FindTextLink(string linkToActivate)
        {
            return GetStatusBarDockPanel();

        }

        private static object GetStatusBarDockPanel()
        {
            DependencyObject dd = VisualTreeHelper.GetChild(System.Windows.Application.Current.MainWindow, 0);
            DependencyObject ddd = VisualTreeHelper.GetChild(dd, 0);

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(ddd); i++)
            {
                DependencyObject o = VisualTreeHelper.GetChild(ddd, i);
                if (o != null) // && o is DockPanel)
                {
                    if (o.GetType().FullName.ToLower().Contains("link"))
                    {
                        return o;
                    }
                    if (o.GetType().FullName.ToLower().Contains("textlink"))
                    {
                        return o;
                    }
                    if (o.GetType().FullName.ToLower().Contains("teamexplorer"))
                    {
                        return o;
                    }
                    //DockPanel dockPanel = o as DockPanel;
                    //if (dockPanel.Name == "StatusBarPanel")
                    //{
                        //return dockPanel;
                    //}
                }
            }
            return null;
        }

        public static TextLink FindChildTextLink(DependencyObject depObj, string childName, string linkText)
        {
            // Confirm obj is valid. 
            if (depObj == null) return null;

            // success case
            if (depObj is TextLink 
                //&& depObj is FrameworkElement 
                && ((FrameworkElement)depObj).Name == childName)
            {
                string depObjDesc = depObj.ToString();
                if (depObjDesc.Equals($"Microsoft.TeamFoundation.Controls.WPF.TeamExplorer.TextLink: {linkText}"))
                    return depObj as TextLink;
            }

            int childrenCount = VisualTreeHelper.GetChildrenCount(depObj);
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);

                //DFS
                TextLink obj = FindChildTextLink(child, childName, linkText);

                if (obj != null)
                    return obj;
            }

            return null;
        }

        private static DependencyObject FindChildWithText(DependencyObject depObj, string linkText)
        {
            // success case
            int childCnt = VisualTreeHelper.GetChildrenCount(depObj);
            for (int i = 0; i < childCnt; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);

                //DFS
                var obj = FindChildWithText(child, linkText);

                if (obj != null)
                    return obj;
            }

            return null;
        }

        public static T FindChild<T>(DependencyObject depObj, string childName) where T : DependencyObject
        {
            // Confirm obj is valid. 
            if (depObj == null) return null;

            // success case
            if (depObj is T && depObj is FrameworkElement && ((FrameworkElement)depObj).Name == childName)
                return depObj as T;

            int childrenCount = VisualTreeHelper.GetChildrenCount(depObj);
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);

                //DFS
                T obj = FindChild<T>(child, childName);

                if (obj != null)
                    return obj;
            }

            return null;
        }

        private void ShowStandardMessageBox()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            string title = "Hot Source";

            // Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox(this.package, message, title,
                OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}
