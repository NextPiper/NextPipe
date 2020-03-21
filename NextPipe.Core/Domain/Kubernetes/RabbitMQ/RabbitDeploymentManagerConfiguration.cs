using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Xml.Schema;
using NextPipe.Core.Domain.SharedValueObjects;
using NextPipe.Core.Helpers;
using NextPipe.Core.ValueObjects;
using NextPipe.Utilities.Resources;

namespace NextPipe.Core
{
    public interface IRabbitDeploymentManagerConfiguration
    {
        Id TaskId { get; }
        int LowerBoundaryReadyReplicas { get; }
        int ReplicaFailureThreshold { get; }
        int ReplicaDelaySeconds { get; }
        Func<Id, ILogHandler, Task> SuccessCallback { get; }
        Func<Id, ILogHandler, Task> FailureCallback { get; }
        Func<Id, ILogHandler, Task> UpdateCallback { get; }
    }
    
    public class RabbitDeploymentManagerConfiguration : IRabbitDeploymentManagerConfiguration
    {
        public Id TaskId { get; }
        public Func<Id, ILogHandler, Task> SuccessCallback { get; }
        public Func<Id, ILogHandler, Task> FailureCallback { get; }
        public Func<Id, ILogHandler, Task> UpdateCallback { get; }
        private readonly LowerBoundaryReadyReplicas _lowerBoundaryReadyReplicas;
        private readonly ReplicaFailureThreshold _replicaFailureThreshold;
        private readonly ReplicaDelaySeconds _replicaDelaySeconds;
        


        public RabbitDeploymentManagerConfiguration(Id taskId, LowerBoundaryReadyReplicas lowerBoundaryReadyReplicas, ReplicaFailureThreshold replicaFailureThreshold, ReplicaDelaySeconds replicaDelaySeconds, Func<Id, ILogHandler, Task> successCallback, Func<Id, ILogHandler, Task> failureCallback, Func<Id, ILogHandler, Task> updateCallback)
        {
            TaskId = taskId;
            SuccessCallback = successCallback;
            FailureCallback = failureCallback;
            UpdateCallback = updateCallback;
            _lowerBoundaryReadyReplicas = lowerBoundaryReadyReplicas;
            _replicaFailureThreshold = replicaFailureThreshold;
            _replicaDelaySeconds = replicaDelaySeconds;
        }

        public int LowerBoundaryReadyReplicas => _lowerBoundaryReadyReplicas.Value;
        public int ReplicaFailureThreshold => _replicaFailureThreshold.Value;
        public int ReplicaDelaySeconds => _replicaDelaySeconds.Value;
    }
}