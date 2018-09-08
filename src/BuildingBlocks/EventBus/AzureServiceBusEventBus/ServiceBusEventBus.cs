using Autofac;
using LibraryBuddy.BuildingBlocks.EventBus;
using LibraryBuddy.BuildingBlocks.EventBus.Abstractions;
using LibraryBuddy.BuildingBlocks.EventBus.Events;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LibraryBuddy.BuildingBlocks.AzureServiceBusEventBus
{
    public class ServiceBusEventBus : IEventBus
    {
        private readonly IServiceBusPersistedConnection _serviceBusPersistedConnection;
        private readonly ILogger<ServiceBusEventBus> _logger;
        private readonly IEventBusSubscriptionManager _subscriptionManager;
        private readonly ILifetimeScope _autofacScope;
        private readonly SubscriptionClient _subscriptionClient;

        private const string INTEGRATION_EVENT_SUFIX = "IntegrationEvent";
        private readonly string AUTOFAC_SCOPE_NAME = "library_event_bus";

        //https://github.com/Azure/azure-service-bus/tree/master/samples/DotNet/GettingStarted/Microsoft.Azure.ServiceBus/TopicSubscriptionWithRuleOperationsSample
        public ServiceBusEventBus(IServiceBusPersistedConnection serviceBusPersistedConnection,
                                    ILogger<ServiceBusEventBus> logger,
                                    IEventBusSubscriptionManager subscriptionManager,
                                    string subscriptionClientName,
                                    ILifetimeScope autofacScope)
        {
            _serviceBusPersistedConnection = serviceBusPersistedConnection;
            _logger = logger;
            _subscriptionManager = subscriptionManager ?? new InMemoryEventBusSubscriptionsManager();
            _autofacScope = autofacScope;

            _subscriptionClient = new SubscriptionClient(serviceBusPersistedConnection.ServiceBusConnectionStringBuilder, 
                subscriptionClientName);
            RemoveDefaultRule();
            RegisterSubscriptionClientMessageHandler();

        }

        //private void RegisterSubscriptionClientMessageHandler()
        //{
        //    _subscriptionClient.RegisterMessageHandler(

        //       async (message, token) =>
        //       {
        //           var eventName = $"{message.Label}{INTEGRATION_EVENT_SUFIX}";
        //           var messageData = Encoding.UTF8.GetString(message.Body);
        //           await ProcessEvent(eventName, messageData);

        //           await _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
        //       },
        //       //https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.servicebus.messagehandleroptions?view=azure-dotnet
        //       new MessageHandlerOptions(ExceptionReceivedHandler) { MaxConcurrentCalls = 19, AutoComplete = false });
        //}


        private void RegisterSubscriptionClientMessageHandler()
        {
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                //https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.servicebus.messagehandleroptions?view=azure-dotnet
                // Maximum number of Concurrent calls to the callback `ProcessMessagesAsync`, set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 10,

                // Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
                // False below indicates the Complete will be handled by the User Callback as in `ProcessMessagesAsync` below.
                AutoComplete = false
            };

            _subscriptionClient.RegisterMessageHandler(ProcessMessageAsync, messageHandlerOptions);
        }

        private async Task ProcessMessageAsync(Message message, CancellationToken token)
        {
            var eventName = $"{message.Label}{INTEGRATION_EVENT_SUFIX}";
            var messageData = Encoding.UTF8.GetString(message.Body);

            //process the message
            await ProcessEvent(eventName, messageData);

            // Complete the message so that it is not received again.
            // This can be done only if the subscriptionClient is opened in ReceiveMode.PeekLock mode (which is default).
            await _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            if (!_subscriptionManager.HasSubscriptionForEvent(eventName))
            {
                using (var scope = _autofacScope.BeginLifetimeScope(AUTOFAC_SCOPE_NAME))
                {
                    var subscriptions = _subscriptionManager.GetHandlersForEvent(eventName);

                    foreach (var sub in subscriptions)
                    {
                        var handler = scope.ResolveOptional(sub.HandlerType);
                        if (sub.IsDynamic)
                        {
                            var dynamicHandler = handler as IDynamicIntegrationEventHandler;
                            dynamic eventData = JObject.Parse(message);
                            await dynamicHandler.Handle(eventData);
                        }
                        else
                        {
                            var eventType = _subscriptionManager.GetEventTypeByName(eventName);
                            var integrationEvent = JsonConvert.DeserializeObject(message, eventType);
                            var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                            await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { integrationEvent });
                        }
                    }
                }
            }
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            //log the exception info..
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}");

            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine($"Action: {context.Action}");
            Console.WriteLine($"ClientId: {context.ClientId}");
            Console.WriteLine($"Endpoint: {context.Endpoint}");
            Console.WriteLine($"EntityPath: {context.EntityPath}");
            
            return Task.CompletedTask;
        }

        private void RemoveDefaultRule()
        {
            try
            {
                _subscriptionClient.RemoveRuleAsync(RuleDescription.DefaultRuleName)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (MessagingEntityNotFoundException)
            {
                _logger.LogInformation($"The messaging entity { RuleDescription.DefaultRuleName } could not be found!!");
            }
        }

        public void Publish(IntegrationEvent @event)
        {
            var eventName = @event.GetType().Name.Replace(INTEGRATION_EVENT_SUFIX, "");
            var jsonMessage = JsonConvert.SerializeObject(@event);
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            var message = new Message
            {
                MessageId = new Guid().ToString(),
                Body = body,
                Label = eventName
            };

            var topicClient = _serviceBusPersistedConnection.CreateModel();
            topicClient.SendAsync(message).GetAwaiter().GetResult();
        }

        public void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = typeof(T).Name.Replace(INTEGRATION_EVENT_SUFIX, "");

            var containsKey = _subscriptionManager.HasSubscriptionForEvent<T>();
            if (!containsKey)
            {
                try
                {
                    _subscriptionClient.AddRuleAsync(new RuleDescription
                    {
                        //CorrelationFilter - Holds a set of conditions that is evaluated in the ServiceBus service against the arriving messages' 
                        //user -defined properties and system properties. A match exists when an arriving message's value for a 
                        //property is equal to the value specified in the correlation filter.
                        Filter = new CorrelationFilter { Label = eventName },
                        Name = eventName
                    }).GetAwaiter().GetResult();
                }
                catch (ServiceBusException)
                {
                    _logger.LogInformation($"This messaging entity { eventName } already exist!!");
                }
            }
            _subscriptionManager.AddSubscription<T, TH>();
        }

        public void SubscribeDynamic<TH>(string eventName) where TH : IDynamicIntegrationEventHandler
        {
            _subscriptionManager.AddDynamicSubscription<TH>(eventName);
        }

        public void Unsubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = typeof(T).Name.Replace(INTEGRATION_EVENT_SUFIX, "");

            try
            {
                _subscriptionClient.RemoveRuleAsync(eventName)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (MessagingEntityNotFoundException)
            {
                _logger.LogInformation($"The messaging entity { eventName } could not be found!! ");
            }

            _subscriptionManager.RemoveSubscription<T, TH>();
        }

        public void UnsubscribeDynamic<TH>(string eventName) where TH : IDynamicIntegrationEventHandler
        {
            _subscriptionManager.RemoveDynamicSubscription<TH>(eventName);
        }

        public void Dispose()
        {
            _subscriptionManager.Clear();
        }
    }
}
