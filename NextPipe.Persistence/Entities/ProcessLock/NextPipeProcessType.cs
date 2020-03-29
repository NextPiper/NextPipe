namespace NextPipe.Persistence.Entities.ProcessLock
{
    public enum NextPipeProcessType
    {
        CleanUpHangingTasks,
        InstallPendingModulesTask,
        CleanModulesReadyForUninstallTask,
        ArchiveModules,
        ArchiveCompletedTasks
    }
}