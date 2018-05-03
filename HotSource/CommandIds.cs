using System;

namespace HotSource
{
    public class CommandIds
    {
            // Package GUID
        public static readonly Guid HotSourcePackageGuid = new Guid("f4b9354d-dc03-4b3e-be0c-49661569dc3b");
        /// Command Set GUID
        public static readonly Guid HotSourceCommandSetGuid = new Guid("366327c7-6c0c-471e-be19-0e9f1760529b");

        // Command IDs
        public const int FetchCmdId = 0x0110;
        public const int PullCmdId = 0x0115;
        public const int CommitCmdId = 0x0120;
        public const int PushCmdId = 0x0125;
        public const int SyncCmdId = 0x0127;
        public const int UndoCmdId = 0x0130;
        public const int NewBranchCmdId = 0x0135;
        public const int ManageBranchesCmdId = 0x0140;
        public const int ShowBranchHistoryCmdId = 0x0145;
        public const int ShowFileHistoryCmdId = 0x0150;
        public const int ShowFileDiffCmdId = 0x0155;
        public const int ShowBlameCmdId = 0x0160;
        public const int PendingChangesCmdId = 0x0165;
        public const int UnpushedCommitsCmdId = 0x0170;
    }
}
