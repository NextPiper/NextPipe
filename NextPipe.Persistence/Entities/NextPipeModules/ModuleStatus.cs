namespace NextPipe.Persistence.Entities.NextPipeModules
{
    public enum ModuleStatus
    {
        Pending,
        Installing,
        Running,
        Uninstall,
        Uninstalling,
        Uninstalled,
        Failed,
        FailedUninstall
    }
}