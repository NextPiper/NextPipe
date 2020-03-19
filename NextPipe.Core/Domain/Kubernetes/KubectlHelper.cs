using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using NextPipe.Core.Documents;

namespace NextPipe.Core.Kubernetes
{
    public interface IKubectlHelper
    {
        V1StatefulSet GetStatefulset(string statefulsetName, string nameSpace = "default");
        bool ValidateStatefulsetIsRunning(string statefulsetName, string nameSpace = "default");
        int GetNumberOfStatefulsetReadyReplicas(string statefulsetName, string nameSpace = "default");
        Task<IEnumerable<V1Pod>> GetPodByCustomNameFilter(string podName, Func<string, string, bool> podFilter,
            string nameSpace = "default");
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

        public async Task<IEnumerable<V1Pod>> GetPodByCustomNameFilter(string podName, Func<string, string, bool> podFilter, string nameSpace = "default")
        {
            var podList = await _client.ListNamespacedPodWithHttpMessagesAsync(nameSpace);
            return podList.Body.Items.Where(item => podFilter(item.Metadata.Name, podName));
        }
    }
}