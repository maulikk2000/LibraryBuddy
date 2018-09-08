using LibraryBuddy.BuildingBlocks.EventBus.Abstractions;
using LibraryBuddy.BuildingBlocks.EventBus.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibraryBuddy.BuildingBlocks.EventBus
{
    public class InMemoryEventBusSubscriptionsManager : IEventBusSubscriptionManager
    {
        private readonly Dictionary<string, List<SubscriptionInfo>> _handlers;
        private readonly List<Type> _eventTypes;

        public InMemoryEventBusSubscriptionsManager()
        {
            _handlers = new Dictionary<string, List<SubscriptionInfo>>();
            _eventTypes = new List<Type>();
        }
        public bool IsEmpty => _handlers.Keys.Any();
        public void Clear() => _handlers.Clear();
        public string GetEventKey<T>() => typeof(T).Name; //here we are returning the value but as we are using expression bodied members, we dont need to specify return

        public event EventHandler<string> OnEventRemoved;

        public void AddDynamicSubscription<TH>(string eventName) 
            where TH : IDynamicIntegrationEventHandler
        {
            AddSubscription(typeof(TH), eventName, isDynamic:true);
        }

        //the first argument is integration event to subscribe. 
        //The second is the integration event handler(callback method) to be executed when the receiver microservice gets that integration event.
        //in other words, second is the event to execute (method to call) 
        public void AddSubscription<T, TH>() 
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = GetEventKey<T>();
            AddSubscription(typeof(TH), eventName, isDynamic:false);
            _eventTypes.Add(typeof(T));
        }

        private void AddSubscription(Type handlerType, string eventName, bool isDynamic)
        {
            if (!HasSubscriptionForEvent(eventName))
            {
                _handlers.Add(eventName, new List<SubscriptionInfo>());
            }

            if(_handlers[eventName].Any(e => e.HandlerType == handlerType))
            {
                throw new ArgumentException($"Handler Type {handlerType} already registered for event ' {eventName} '", nameof(handlerType));
            }

            if (isDynamic)
            {
                _handlers[eventName].Add(SubscriptionInfo.Dynamic(handlerType));
            }
            else
            {
                _handlers[eventName].Add(SubscriptionInfo.Typed(handlerType));
            }
        }
        public void RemoveDynamicSubscription<TH>(string eventName) where TH : IDynamicIntegrationEventHandler
        {
            var handlerToRemove = FindDynamicSubscription<TH>(eventName);
        }

        public void RemoveSubscription<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var handlerToRemove = FindSubscription<T, TH>();
            var eventName = GetEventKey<T>();
            RemoveHandler(eventName, handlerToRemove);
        }

        private void RemoveHandler(string eventName, SubscriptionInfo subscriptionInfo)
        {
            if(subscriptionInfo != null)
            {
                _handlers[eventName].Remove(subscriptionInfo);

                if (_handlers[eventName].Any())
                {
                    _handlers.Remove(eventName);

                    var eventType = _eventTypes.SingleOrDefault(e => e.Name == eventName);
                    if(eventType != null)
                    {
                        _eventTypes.Remove(eventType);
                    }
                    RaiseOnEventRemoved(eventName);
                }
            }
        }

        private void RaiseOnEventRemoved(string eventName)
        {
            var handler = OnEventRemoved;
            if(handler != null)
            {
                OnEventRemoved(this, eventName);
            }
        }

        private SubscriptionInfo FindDynamicSubscription<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler
        {
            return FindSubscription(eventName, typeof(TH));
        }

        private SubscriptionInfo FindSubscription<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = GetEventKey<T>();
            return FindSubscription(eventName, typeof(TH));
        }

        private SubscriptionInfo FindSubscription(string eventName, Type handlerType)
        {
            if (!HasSubscriptionForEvent(eventName))
            {
                return null;
            }

            return _handlers[eventName].SingleOrDefault(s => s.HandlerType == handlerType);
        }

        public IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : IntegrationEvent
        {
            var key = GetEventKey<T>();
            return GetHandlersForEvent(key);
        }

        public IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName) => _handlers[eventName];

        public bool HasSubscriptionForEvent<T>() where T : IntegrationEvent
        {
            var eventName = GetEventKey<T>();
            return HasSubscriptionForEvent(eventName);
        }

        public bool HasSubscriptionForEvent(string eventName) => _handlers.ContainsKey(eventName);

        public Type GetEventTypeByName(string eventName) => _eventTypes.SingleOrDefault(s => s.Name == eventName);
       
    }
}
