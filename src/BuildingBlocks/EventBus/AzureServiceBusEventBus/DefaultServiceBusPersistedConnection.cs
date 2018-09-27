using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;

namespace LibraryBuddy.BuildingBlocks.AzureServiceBusEventBus
{
    public class DefaultServiceBusPersistedConnection : IServiceBusPersistedConnection
    {
        private readonly ILogger<DefaultServiceBusPersistedConnection> _logger;
        private readonly ServiceBusConnectionStringBuilder _serviceBusConnectionStringBuilder;
        private TopicClient _topicClient;
        private bool _disposed;

        public ServiceBusConnectionStringBuilder ServiceBusConnectionStringBuilder => _serviceBusConnectionStringBuilder;

        public DefaultServiceBusPersistedConnection(ServiceBusConnectionStringBuilder serviceBusConnectionStringBuilder, 
            ILogger<DefaultServiceBusPersistedConnection> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _serviceBusConnectionStringBuilder = serviceBusConnectionStringBuilder ??
                throw new ArgumentNullException(nameof(serviceBusConnectionStringBuilder));
            _topicClient = new TopicClient(_serviceBusConnectionStringBuilder, RetryPolicy.Default);
        }
        public ITopicClient CreateModel()
        {
            if (_topicClient.IsClosedOrClosing)
            {
                _topicClient = new TopicClient(_serviceBusConnectionStringBuilder, RetryPolicy.Default);
            }
            return _topicClient;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
        }
    }
}
