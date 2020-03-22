using System;
using NextPipe.Utilities.Documents.Responses;
using SimpleSoft.Mediator;

namespace NextPipe.Core.Commands.Commands.ModuleCommands
{
    public class RequestInstallModule : Command<TaskRequestResponse>
    {
        public RequestInstallModule(string imageName, int imageReplicas)
        {
            
        }
    }
}