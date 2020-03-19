using System;
using NextPipe.Core.Domain.SharedValueObjects;
using NextPipe.Utilities.Documents.Responses;
using SimpleSoft.Mediator;

namespace NextPipe.Core.Commands.Commands.StartupCommands
{
    public class RequestInitializeInfrastructure : Command<TaskRequestResponse>
    {
        public int LowerBoundaryReadyReplicas { get; }

        public RequestInitializeInfrastructure(int lowerBoundaryReadyReplicas)
        {
            LowerBoundaryReadyReplicas = lowerBoundaryReadyReplicas;
            var id = new NonNullValueObject<string>(null);
        }
    }
}