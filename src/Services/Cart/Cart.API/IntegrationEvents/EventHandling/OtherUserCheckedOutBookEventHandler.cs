using LibraryBuddy.BuildingBlocks.EventBus.Abstractions;
using LibraryBuddy.Services.Cart.API.IntegrationEvents.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryBuddy.Services.Cart.API.IntegrationEvents.EventHandling
{
    public class OtherUserCheckedOutBookEventHandler : IIntegrationEventHandler<OtherUserCheckedOutBookEvent>
    {
        public Task Handle(OtherUserCheckedOutBookEvent @event)
        {
            throw new NotImplementedException();
        }
    }
}
