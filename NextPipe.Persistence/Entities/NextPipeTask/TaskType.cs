namespace NextPipe.Persistence.Entities
{
    public enum TaskType
    {
        RabbitInfrastructureDeploy,
        RabbitInfrastructureUninstall,
        ModuleInstall,
        ModuleUninstall,
        ModuleScale
        
    }
}