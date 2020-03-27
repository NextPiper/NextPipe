using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Schema;
using k8s;
using k8s.Models;
using Microsoft.AspNetCore.Http;
using NextPipe.Core.Documents;
using NextPipe.Utilities.Documents.Responses;

namespace NextPipe.Core.Kubernetes
{
    public interface IKubectlHelper
    {
        V1StatefulSet GetStatefulset(string statefulsetName, string nameSpace = "default");
        Task<V1Service> GetService(string serviceName, string nameSpace = "default");
        Task<V1Deployment> GetDeployment(string deploymentName, string nameSpace = "default");
        bool ValidateStatefulsetIsRunning(string statefulsetName, string nameSpace = "default");
        int GetNumberOfStatefulsetReadyReplicas(string statefulsetName, string nameSpace = "default");
        Task<IEnumerable<V1Pod>> GetPodsByCustomNameFilter(string podName, Func<string, string, bool> podFilter,
            string nameSpace = "default");
        Task<IEnumerable<V1PersistentVolumeClaim>> GetPVCsByCustomerNameFilter(string pvcName,
            Func<string, string, bool> pvcFilter, string nameSpace = "default");
        Task DeletePVCList(IEnumerable<V1PersistentVolumeClaim> pvcList, string nameSpace = "default");
        Task InstallService(V1Service service, string nameSpace = "default");
        Task<string> DeleteService(string name, string nameSpace = "default");
        Task<Response> InstallModule(V1Deployment moduleDeployment, string nameSpace = "default");
        

    }
    
    public class KubectlHelper : IKubectlHelper
    {
        private readonly IKubernetes _client;

        public KubectlHelper(IKubernetes Client)
        {
            _client = Client;
        }
        
        public V1StatefulSet GetStatefulset(string statefulsetName, string nameSpace = "default")
        {
            return _client.ListNamespacedStatefulSet(nameSpace).Items
                .FirstOrDefault(item => item.Metadata.Name == statefulsetName);
        }

        public async Task<V1Service> GetService(string serviceName, string nameSpace = "default")
        {
            var result = await _client.ListNamespacedServiceWithHttpMessagesAsync(nameSpace);

            return result.Body.Items.SingleOrDefault(t => t.Metadata.Name == serviceName);
        }

        public async Task<V1Deployment> GetDeployment(string deploymentName, string nameSpace = "default")
        {
            var result = await _client.ListNamespacedDeploymentWithHttpMessagesAsync(nameSpace);

            return result.Body.Items.SingleOrDefault(t => t.Metadata.Name == deploymentName);
        }

        public bool ValidateStatefulsetIsRunning(string statefulsetName, string nameSpace = "default")
        {
            return GetStatefulset(statefulsetName, nameSpace) != null;
        }

        public int GetNumberOfStatefulsetReadyReplicas(string statefulsetName, string nameSpace = "default")
        {
            var statefulset = GetStatefulset(statefulsetName, nameSpace);

            if (statefulset == null)
            {
                throw new KubeConnectionException($"Trying to fetch ready replicas of statefulset: {statefulsetName} under namespace: {nameSpace}. Statefulset could not be found");
            }

            return statefulset.Status.ReadyReplicas.GetValueOrDefault();
        }

        public async Task<IEnumerable<V1Pod>> GetPodsByCustomNameFilter(string podName, Func<string, string, bool> podFilter, string nameSpace = "default")
        {
            var podList = await _client.ListNamespacedPodWithHttpMessagesAsync(nameSpace);
            return podList.Body.Items.Where(item => podFilter(item.Metadata.Name, podName));
        }

        public async Task<IEnumerable<V1PersistentVolumeClaim>> GetPVCsByCustomerNameFilter(string pvcName, Func<string, string, bool> pvcFilter, string nameSpace = "default")
        {
            var pvcList = await _client.ListPersistentVolumeClaimForAllNamespacesWithHttpMessagesAsync();
            return pvcList.Body.Items.Where(item => pvcFilter(item.Metadata.Name, pvcName));
        }

        public async Task DeletePVCList(IEnumerable<V1PersistentVolumeClaim> pvcList, string nameSpace = "default")
        {
            foreach (var pvc in pvcList)
            {
                await _client.DeleteNamespacedPersistentVolumeClaimWithHttpMessagesAsync(pvc.Metadata.Name, nameSpace);
            }
        }

        public async Task InstallService(V1Service service, string nameSpace = "default")
        {
            var runningService = await GetService("rabbitmq-service", nameSpace);
            
            if (runningService == null)
            {
                await _client.CreateNamespacedServiceWithHttpMessagesAsync(service, nameSpace);
            }
        }

        public async Task<string> DeleteService(string name, string nameSpace = "default")
        {
            try
            {
                var result = await _client.DeleteNamespacedServiceAsync(name, nameSpace);
                return result.Message;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static V1Deployment CreateModuleDeployment(string imageName, string moduleName, int moduleReplicas)
        {
            return new V1Deployment()
            {
                ApiVersion = "apps/v1",
                Kind = "Deployment",
                Metadata = new V1ObjectMeta()
                {
                    Name = moduleName,
                    NamespaceProperty = null,
                    Labels = new Dictionary<string, string>()
                    {
                        { "app", moduleName }
                    }
                },
                Spec = new V1DeploymentSpec
                {
                    Replicas = moduleReplicas,
                    Selector = new V1LabelSelector()
                    {
                        MatchLabels = new Dictionary<string, string>
                        {
                            { "app", moduleName }
                        }
                    },
                    Template = new V1PodTemplateSpec()
                    {
                        Metadata = new V1ObjectMeta()
                        {
                            CreationTimestamp = null,
                            Labels = new Dictionary<string, string>
                            {
                                { "app", moduleName }
                            }
                        },
                        Spec = new V1PodSpec
                        {
                            Containers = new List<V1Container>()
                            {
                                new V1Container()
                                {
                                    Name = moduleName,
                                    Image = imageName,
                                    ImagePullPolicy = "Always",
                                    Ports = new List<V1ContainerPort> { new V1ContainerPort(80) }
                                }
                            }
                        }
                    }
                },
                Status = new V1DeploymentStatus()
                {
                    Replicas = 1
                }
            };
        }


        public async Task<Response> InstallModule(V1Deployment moduleDeployment, string nameSpace = "default")
        {
            // First check if the moduleDeployment is already deployed in the system
            var modules = await GetDeployment(moduleDeployment.Metadata.Name);

            if (modules != null)
            {
                // There is already a module running under the specified deploymentName
                return Response.Unsuccessful($"There is already a module running under the specified deploymentName: {moduleDeployment}");
            }
            try
            {
                await _client.CreateNamespacedDeploymentWithHttpMessagesAsync(moduleDeployment, nameSpace);
            }
            catch (Exception e)
            {
                return Response.Unsuccessful(e.Message);
            }
            
            return Response.Success();
        }
        
        public static string KubectlApplyRabbitService()
        {
            return "cd wwwroot && kubectl apply -f rabbitmq-service.yml";
        }

        public static string KubectlDeleteRabbitService()
        {
            return "cd wwwroot && kubectl delete -f rabbitmq-service.yml";
        }

        public static V1Service GetRabbitMQService()
        {
            return new V1Service(
                "v1",
                "Service",
                new V1ObjectMeta(
                    name: "rabbitmq-service",
                    labels: new Dictionary<string, string> { {"app", "rabbitmq-service"}},
                    namespaceProperty: "default"),
                new V1ServiceSpec(
                    ports: new List<V1ServicePort>
                    {
                        new V1ServicePort(15672,"http", protocol: "TCP", targetPort: 15672),
                        new V1ServicePort(5672,"amqp", protocol: "TCP", targetPort:"amqp"),
                        new V1ServicePort(4369,"empd", protocol: "TCP", targetPort: "empd")
                    },
                    selector: new Dictionary<string, string> { {"app", "rabbitmq"}},
                    type: "LoadBalancer"
                    ));
        }
    }
}
