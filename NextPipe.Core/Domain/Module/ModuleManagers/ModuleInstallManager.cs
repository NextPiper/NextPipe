using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NextPipe.Core.Commands.Commands.ModuleCommands;
using NextPipe.Core.Domain.SharedValueObjects;
using NextPipe.Core.Helpers;
using NextPipe.Core.Kubernetes;

namespace NextPipe.Core.Domain.Module.ModuleManagers
{
    public interface IModuleInstallManager
    {
        Task DeployModule(IModuleInstallManagerConfig config);
        void SetVerboseLogging(bool verboseLogging);
        void AttachTaskIdAndUpdateHandler(Id taskId, Func<Id, ILogHandler, Task> updateHandler);
        void AttachPreviousLogs(string logs);
    }
    public class ModuleInstallManager : IModuleInstallManager
    {
        private readonly IKubectlHelper _kubectlHelper;
        private readonly ILogHandler _logHandler;
        private bool verboseLogging;

        public ModuleInstallManager(IKubectlHelper kubectlHelper)
        {
            _kubectlHelper = kubectlHelper;
            _logHandler = new LogHandler();
        }
        
        
        public async Task DeployModule(IModuleInstallManagerConfig config)
        {
            _logHandler.AttachTaskIdAndUpdateHandler(config.TaskId, config.UpdateCallback);
            
            await _logHandler.WriteCmd($"{nameof(ModuleInstallManager)}.{nameof(DeployModule)}", verboseLogging);
            var response = await _kubectlHelper.InstallModule(KubectlHelper.CreateModuleDeployment(
                config.ImageName,
                config.ModuleName,
                config.ModuleReplicas));

            if (response.IsSuccessful)
            {
                await config.SuccessCallback(config.TaskId, _logHandler);
            }
            else
            {
                await config.FailureCallback(config.TaskId, _logHandler);
            }
        }

        public async Task DeleteModule()
        {
            
        }
        
        public void AttachTaskIdAndUpdateHandler(Id taskId, Func<Id, ILogHandler, Task> updateHandler)
        {
            _logHandler.AttachTaskIdAndUpdateHandler(taskId, updateHandler);
        }

        public void AttachPreviousLogs(string logs)
        {
            _logHandler.AttachPreviousLogs(logs);
        }

        public void SetVerboseLogging(bool verboseLogging)
        {
            this.verboseLogging = verboseLogging;
        }
    }
}
