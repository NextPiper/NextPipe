using System;
using System.Collections.Generic;
using NextPipe.Core.Domain.Module.KubernetesModule;
using NextPipe.Core.Domain.Module.ValueObjects;
using NextPipe.Utilities.Documents.Responses;
using SimpleSoft.Mediator;

namespace NextPipe.Core.Commands.Commands.ModuleCommands
{
    public class RequestInstallModule : Command<TaskRequestResponse>
    {
        public LoadBalancerConfig LoadBalancerConfig { get; }
        public ImageName ImageName { get; }
        public ModuleReplicas ModuleReplicas { get; }
        public ModuleName ModuleName { get; }
        
        public RequestInstallModule(string imageName, int moduleReplicas, string moduleName, LoadBalancerConfig loadBalancerConfig)
        {
            if (loadBalancerConfig == null)
            {
                throw new Exception("loadBalancerConfig may not be null");
            }
            
            ImageName = new ImageName(imageName);
            ModuleReplicas = new ModuleReplicas(moduleReplicas);
            ModuleName = new ModuleName(moduleName);
            LoadBalancerConfig = loadBalancerConfig;
        }
    }
}