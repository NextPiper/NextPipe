using System;
using System.Threading.Tasks;
using NextPipe.Core.Domain.Module.ValueObjects;
using NextPipe.Core.Domain.SharedValueObjects;
using NextPipe.Core.Helpers;

namespace NextPipe.Core.Domain.Module.ModuleManagers
{
    public interface IModuleInstallManagerConfig
    {
        Id Id { get; }
        int ModuleReplicas { get; }
        string ModuleName { get; }
        string ImageName { get; }
        Func<Id, ILogHandler, Task> SuccessCallback { get; }
        Func<Id, ILogHandler, Task> FailureCallback { get; }
        Func<Id, ILogHandler, Task> UpdateCallback { get; }
    }
    public class ModuleInstallManagerConfig : IModuleInstallManagerConfig
    {
        public Id Id { get; }
        private readonly ModuleReplicas _moduleReplicas;
        private readonly ImageName _imageName;
        private readonly ModuleName _moduleName;
       
        public Func<Id, ILogHandler, Task> SuccessCallback { get; }
        public Func<Id, ILogHandler, Task> FailureCallback { get; }
        public Func<Id, ILogHandler, Task> UpdateCallback { get; }

        public ModuleInstallManagerConfig(Id id, ModuleReplicas moduleReplicas, ModuleName moduleName, ImageName imageName, Func<Id, ILogHandler, Task> successCallback, Func<Id, ILogHandler, Task> failureCallback, Func<Id, ILogHandler, Task> updateCallback)
        {
            Id = id;
            _moduleReplicas = moduleReplicas;
            _moduleName = moduleName;
            _imageName = imageName;
            SuccessCallback = successCallback;
            FailureCallback = failureCallback;
            UpdateCallback = updateCallback;
            
        }

        public int ModuleReplicas => _moduleReplicas.Value;

        public string ImageName => _imageName.Value;

        public string ModuleName => _moduleName.Value;
    }
    
}