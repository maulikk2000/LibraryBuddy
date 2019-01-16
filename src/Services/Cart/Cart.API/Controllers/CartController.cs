using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LibraryBuddy.BuildingBlocks.EventBus.Abstractions;
using LibraryBuddy.Services.Cart.API.Models;
using LibraryBuddy.Services.Cart.API.Services;
using LibraryBuddy.Services.Cart.API.IntegrationEvents.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibraryBuddy.Services.Cart.API.Controllers
{
    [Route("api/v1/[controller]")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartRepository _repository;
        private readonly IIdentityService _service;
        private readonly IEventBus _eventBus;

        public CartController(ICartRepository repository, IIdentityService service, IEventBus eventBus)
        {
            _repository = repository;
            _service = service;
            _eventBus = eventBus;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BorrowerCart), (int)HttpStatusCode.OK)]
        
        public async Task<ActionResult<BorrowerCart>> Get(string id)
        {
            var Cart = await _repository.GetCartAsync(id);

            if (Cart is null)
                return new BorrowerCart(id);

            return Ok(Cart);
        }

        [HttpPost]
        [ProducesResponseType(typeof(BorrowerCart), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Update([FromBody]BorrowerCart Cart)
        {
            var newCart = await _repository.UpdateCartAsync(Cart);
            return Ok(newCart);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> Delete(string id)
        {
            await _repository.DeleteCartAsync(id);
            return NoContent();
        }

        [Route("checkout")]
        [HttpPost]        
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Checkout([FromBody]CartCheckout CartCheckout, 
            [FromHeader(Name ="x-requestid")] string requestId)
        {
            var borrowerId = _service.GetUserIdentity();

            CartCheckout.RequestId = (Guid.TryParse(requestId, out Guid guid) && guid != Guid.Empty) ?
                guid : CartCheckout.RequestId;

            var Cart = await _repository.GetCartAsync(borrowerId);

            if (Cart is null)
            {
                return BadRequest();
            }

            var eventMessage = new UserCheckoutAcceptedIntegrationEvent();

            _eventBus.Publish(eventMessage);

            return Accepted();
        }

    }
}