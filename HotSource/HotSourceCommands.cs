using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
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
            commandService.AddCommand(CreateMenuCommand(CommandIDs.PushCmdId, this.GoToSyncWindow));
            commandService.AddCommand(CreateMenuCommand(CommandIDs.PullCmdId, this.GoToSyncWindow));
            commandService.AddCommand(CreateMenuCommand(CommandIDs.FetchCmdId, this.GoToSyncWindow));
            commandService.AddCommand(CreateMenuCommand(CommandIDs.SyncCmdId, this.GoToSyncWindow));
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
            ExecuteDTECommand("View.PendingChanges");
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
            ExecuteDTECommand("Vew.UnpublishedCommits");
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
