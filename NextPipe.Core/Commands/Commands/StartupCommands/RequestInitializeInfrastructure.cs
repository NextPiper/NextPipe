using System;
using NextPipe.Core.Domain.SharedValueObjects;
using NextPipe.Core.ValueObjects;
using NextPipe.Utilities.Documents.Responses;
using SimpleSoft.Mediator;

namespace NextPipe.Core.Commands.Commands.StartupCommands
{
    public class RequestInitializeInfrastructure : Command<TaskRequestResponse>
    {
        public LowerBoundaryReadyReplicas LowerBoundaryReadyReplicas { get; }

        public RequestInitializeInfrastructure(int lowerBoundaryReadyReplicas)
        {
            LowerBoundaryReadyReplicas = new LowerBoundaryReadyReplicas(lowerBoundaryReadyReplicas);
        }
    }
}