using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LibraryBuddy.BuildingBlocks.EventBus.Abstractions;
using LibraryBuddy.Services.Basket.API.Models;
using LibraryBuddy.Services.Basket.API.Services;
using LibraryBuddy.Services.Basket.API.IntegrationEvents.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibraryBuddy.Services.Basket.API.Controllers
{
    [Route("api/v1/[controller]")]
    [Authorize]
    public class BasketController : Controller
    {
        private readonly IBasketRepository _repository;
        private readonly IIdentityService _service;
        private readonly IEventBus _eventBus;

        public BasketController(IBasketRepository repository, IIdentityService service, IEventBus eventBus)
        {
            _repository = repository;
            _service = service;
            _eventBus = eventBus;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BorrowerBasket), (int)HttpStatusCode.OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(string id)
        {
            var basket = await _repository.GetBasketAsync(id);

            if (basket is null)
                return NotFound();

            return Ok(basket);
        }

        [HttpPost]
        [ProducesResponseType(typeof(BorrowerBasket), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Post([FromBody]BorrowerBasket basket)
        {
            var newBasket = await _repository.UpdateBasketAsync(basket);
            return Ok(newBasket);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> Delete(string id)
        {
            await _repository.DeleteBasketAsync(id);
            return NoContent();
        }

        [Route("checkout")]
        [HttpPost]        
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Checkout([FromBody]BasketCheckout basketCheckout, 
            [FromHeader(Name ="x-requestid")] string requestId)
        {
            var borrowerId = _service.GetUserIdentity();

            basketCheckout.RequestId = (Guid.TryParse(requestId, out Guid guid) && guid != Guid.Empty) ?
                guid : basketCheckout.RequestId;

            var basket = await _repository.GetBasketAsync(borrowerId);

            if (basket is null)
            {
                return BadRequest();
            }

            var eventMessage = new UserCheckoutAcceptedIntegrationEvent();

            _eventBus.Publish(eventMessage);

            return Accepted();
        }

    }
}