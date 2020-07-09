using System;
using Serverino.Watch.Models;

namespace Serverino.Watch.Commands.Exceptions
{
    public class InvalidCommandExectutionException : Exception
    {
        public Application CurrentApplication { get; }
        public string CommandClassName { get; }

        public InvalidCommandExectutionException(string commandClassName, Application application,
            Exception innerException = null) : base($"Occuring an exception when call ExecuteAsync on {commandClassName}", innerException)
        {
            this.CurrentApplication = application;
            this.CommandClassName = commandClassName;
        }
    }
}