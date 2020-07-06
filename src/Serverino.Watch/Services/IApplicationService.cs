using System;
using Serverino.Watch.Models;

namespace Serverino.Watch.Services
{
    public interface IApplicationService : IDisposable
    {
        Application[] GetNotHostedApplications();
        Application[] GetUpdatedApplications();
        Application[] GetRemovedApplications();
        void PersistHostedApplications(params Application[] applications);
        void RemoveHostedApplication(string appName);
    }
}