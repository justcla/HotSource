using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using System;
using System.Windows.Forms;
using OLEConstants = Microsoft.VisualStudio.OLE.Interop.Constants;
using OleInterop = Microsoft.VisualStudio.OLE.Interop;

namespace HotSource
{
    internal sealed class HotSourceCommandFilter : IOleCommandTarget
    {
        private readonly IWpfTextView textView;
        private readonly IClassifier classifier;
        private readonly SVsServiceProvider globalServiceProvider;
        private IEditorOperations editorOperations;
        private SVsServiceProvider serviceProvider;

        public HotSourceCommandFilter(SVsServiceProvider serviceProvider, IWpfTextView textView, IClassifierAggregatorService aggregatorFactory,
            SVsServiceProvider globalServiceProvider, IEditorOperationsFactoryService editorOperationsFactory)
        {
            this.serviceProvider = serviceProvider;
            this.textView = textView;
            classifier = aggregatorFactory.GetClassifier(textView.TextBuffer);
            this.globalServiceProvider = globalServiceProvider;
            editorOperations = editorOperationsFactory.GetEditorOperations(textView);
        }

        public IOleCommandTarget Next { get; internal set; }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            // Command handling registration
            if (pguidCmdGroup == CommandIDs.HotSourceCommandSetGuid && cCmds == 1)
            {
                switch (prgCmds[0].cmdID)
                {
                    case CommandIDs.UndoCmdId:
                    case CommandIDs.ShowFileHistoryCmdId:
                    case CommandIDs.ShowFileDiffCmdId:
                    case CommandIDs.ShowBlameCmdId:
                        prgCmds[0].cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;
                        return VSConstants.S_OK;
                }
            }

            if (Next != null)
            {
                //ThreadHelper.ThrowIfNotOnUIThread();
                return Next.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
            }
            return (int)OLEConstants.OLECMDERR_E_UNKNOWNGROUP;
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            // Command handling
            if (pguidCmdGroup == CommandIDs.HotSourceCommandSetGuid)
            {
                // Dispatch to the correct command handler
                switch (nCmdID)
                {
                    case CommandIDs.UndoCmdId:
                        return HandleUndo();
                    case CommandIDs.ShowFileHistoryCmdId:
                        return HandleViewFileHistory();
                    case CommandIDs.ShowFileDiffCmdId:
                        return HandleFileDiff();
                    case CommandIDs.ShowBlameCmdId:
                        return HandleFileBlame();
                }
            }

            // No commands called. Pass to next command handler.
            if (Next != null)
            {
                return Next.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            }
            return (int)OLEConstants.OLECMDERR_E_UNKNOWNGROUP;
        }

        private int HandleUndo()
        {
            ExecuteDTECommand("Team.Git.Undo");
            return VSConstants.S_OK;
        }

        private int HandleViewFileHistory()
        {
            ExecuteDTECommand("Team.Git.ViewHistory");
            return VSConstants.S_OK;
        }

        private int HandleFileDiff()
        {
            ExecuteDTECommand("Team.Git.CompareWithUnmodified");
            return VSConstants.S_OK;
        }

        private int HandleFileBlame()
        {
            ExecuteDTECommand("Team.Git.Annotate");
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Get the SUIHostCommandDispatcher from the global service provider.
        /// </summary>
        private IOleCommandTarget GetShellCommandDispatcher()
        {
            return globalServiceProvider.GetService(typeof(SUIHostCommandDispatcher)) as IOleCommandTarget;
        }

        public int HandleCommand(string commandName)
        {
            MessageBox.Show($"Handle File Operation: {commandName}");

            //ExecuteDTECommand(commandName, commandParameters);

            //Guid cmdGroup = VSConstants.VSStd2K;
            //uint cmdID = (uint)VSConstants.VSStd2KCmdID.COMMENT_BLOCK;
            //ExecuteVSCommand(cmdGroup, cmdID);

            return VSConstants.S_OK;
        }

        private void ExecuteDTECommand(string commandName, string commandParameters = "")
        {
            DTE dte = (DTE)serviceProvider.GetService(typeof(DTE));
            dte.ExecuteCommand(commandName, commandParameters);
        }

        private void ExecuteVSCommand(Guid cmdGroup, uint cmdID)
        {
            var commandTarget = GetShellCommandDispatcher();
            // Execute VS command
            int hr = commandTarget.Exec(ref cmdGroup, cmdID, (uint)OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, IntPtr.Zero, IntPtr.Zero);
        }
    }
}
