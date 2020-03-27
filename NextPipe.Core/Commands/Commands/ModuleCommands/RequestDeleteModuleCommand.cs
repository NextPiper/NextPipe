using System;
using NextPipe.Core.Domain.SharedValueObjects;
using NextPipe.Utilities.Documents.Responses;
using SimpleSoft.Mediator;

namespace NextPipe.Core.Commands.Commands.ModuleCommands
{
    public class RequestDeleteModuleCommand : BaseCommand
    {
        public Id Id { get; }
        
        public RequestDeleteModuleCommand(Guid moduleId)
        {
            Id = new Id(moduleId);
        }
    }
}