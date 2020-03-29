using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NextPipe.Core.Commands.Commands.ModuleCommands;
using NextPipe.Core.Domain.SharedValueObjects;
using NextPipe.Core.Helpers;
using NextPipe.Core.Kubernetes;

namespace NextPipe.Core.Domain.Module.ModuleManagers
{
    public interface IModuleManager
    {
        Task DeployModule(IModuleManagerConfig config);

        Task UninstallModule(IModuleManagerConfig config);
        void SetVerboseLogging(bool verboseLogging);
        void AttachTaskIdAndUpdateHandler(Id taskId, Func<Id, ILogHandler, Task> updateHandler);
        void AttachPreviousLogs(string logs);
    }
    public class ModuleManager : IModuleManager
    {
        private readonly IKubectlHelper _kubectlHelper;
        private readonly ILogHandler _logHandler;
        private bool verboseLogging;

        public ModuleManager(IKubectlHelper kubectlHelper)
        {
            _kubectlHelper = kubectlHelper;
            _logHandler = new LogHandler();
        }
        
        public async Task DeployModule(IModuleManagerConfig config)
        {
            _logHandler.AttachTaskIdAndUpdateHandler(config.TaskId, config.UpdateCallback);
            
            await _logHandler.WriteCmd($"{nameof(ModuleManager)}.{nameof(DeployModule)}", verboseLogging);
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
                await _logHandler.WriteLine(response.Message);
                await config.FailureCallback(config.TaskId, _logHandler);
            }
        }

        public async Task UninstallModule(IModuleManagerConfig config)
        {
            _logHandler.AttachTaskIdAndUpdateHandler(config.TaskId, config.UpdateCallback);
            
            await _logHandler.WriteCmd($"{nameof(ModuleManager)}.{nameof(UninstallModule)}", verboseLogging);
            
            // check if the deployment exists, if not just reply with a response.success seeing as the module uninstall can go through
            var result = await _kubectlHelper.GetDeployment(config.ModuleName);
            if (result == null)
            {
                await config.SuccessCallback(config.TaskId, _logHandler);
                return;
            }
            
            var response = await _kubectlHelper.UninstallModule(config.ModuleName);

            if (response.IsSuccessful)
            {
                await config.SuccessCallback(config.TaskId, _logHandler);
            }
            else
            {
                await _logHandler.WriteLine(response.Message);
                await config.FailureCallback(config.TaskId, _logHandler);
            }
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
