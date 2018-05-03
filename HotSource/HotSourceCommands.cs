using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace HotSource
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class HotSourceCommands
    {
        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

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

        private void AddGlobalCommandHandlers(OleMenuCommandService commandService)
        {
            // Add command handlers for global commands
            commandService.AddCommand(CreateMenuCommand(CommandIds.CommitCmdId));
            commandService.AddCommand(CreateMenuCommand(CommandIds.PushCmdId));
            commandService.AddCommand(CreateMenuCommand(CommandIds.PullCmdId));
            commandService.AddCommand(CreateMenuCommand(CommandIds.FetchCmdId));
            commandService.AddCommand(CreateMenuCommand(CommandIds.SyncCmdId));
            commandService.AddCommand(CreateMenuCommand(CommandIds.PendingChangesCmdId));
            commandService.AddCommand(CreateMenuCommand(CommandIds.UnpushedCommitsCmdId));
            commandService.AddCommand(CreateMenuCommand(CommandIds.NewBranchCmdId));
            commandService.AddCommand(CreateMenuCommand(CommandIds.ManageBranchesCmdId));
            commandService.AddCommand(CreateMenuCommand(CommandIds.ShowBranchHistoryCmdId));
        }

        private MenuCommand CreateMenuCommand(int commandId)
        {
            return new MenuCommand(this.Execute, new CommandID(CommandIds.HotSourceCommandSetGuid, commandId));
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static HotSourceCommands Instance { get; private set; }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider { get { return this.package; } }

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

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            string title = "Hot Source";

            // Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox( this.package, message, title,
                OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}
