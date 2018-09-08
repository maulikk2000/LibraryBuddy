using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryBuddy.BuildingBlocks.AzureServiceBusEventBus
{
    public interface IServiceBusPersistedConnection : IDisposable
    {
        ServiceBusConnectionStringBuilder ServiceBusConnectionStringBuilder { get; }

        ITopicClient CreateModel();
    }
}
