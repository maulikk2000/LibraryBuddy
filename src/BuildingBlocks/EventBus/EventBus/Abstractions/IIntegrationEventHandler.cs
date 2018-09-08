using LibraryBuddy.BuildingBlocks.EventBus.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LibraryBuddy.BuildingBlocks.EventBus.Abstractions
{
    public interface IIntegrationEventHandler<in TIntegrationEvent> : IIntegrationEventHandler
        where TIntegrationEvent: IntegrationEvent
    {
        Task Handler(TIntegrationEvent @event);
    }

    public interface IIntegrationEventHandler
    {
    }
}
