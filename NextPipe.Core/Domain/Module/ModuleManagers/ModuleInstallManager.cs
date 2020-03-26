using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NextPipe.Core.Commands.Commands.ModuleCommands;
using NextPipe.Core.Helpers;
using NextPipe.Core.Kubernetes;

namespace NextPipe.Core.Domain.Module.ModuleManagers
{
    public interface IModuleInstallManager
    {
        Task DeployModule(IModuleInstallManagerConfig config, bool verboseLogging);
        void SetVerboseLogging(bool verboseLogging);
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
        public async Task DeployModule(IModuleInstallManagerConfig config, bool verboseLogging)
        {
            await _logHandler.WriteCmd($"{nameof(ModuleInstallManager)}.{nameof(DeployModule)}", verboseLogging);
            var moduleDeployment =
                KubectlHelper.CreateModuleDeployment(config.ImageName, config.ModuleName, config.ModuleReplicas);
            await _kubectlHelper.InstallModule(moduleDeployment);
            
        }

        public void SetVerboseLogging(bool verboseLogging)
        {
            this.verboseLogging = verboseLogging;
        }
    }
}
