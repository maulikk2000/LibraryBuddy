using Cart.FunctionalTests.Base;
using LibraryBuddy.Services.Cart.API.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cart.FunctionalTests
{
    public class CartScenarios : CartScenarioBase
    {

        [Fact]
        public async Task Post_cart_and_response_ok_status_code()
        {
            using(var server = CreateServer())
            {
                var content = new StringContent(BuildCart(), UTF8Encoding.UTF8, "application/json");

                var response = await server.CreateClient().PostAsync(PostCartUrl.Cart, content);

                response.EnsureSuccessStatusCode();
            }
        }

        [Fact]
        public async Task Get_basket_and_response_ok_status_code()
        {
            using(var server = CreateServer())
            {
                var response = await server.CreateClient().GetAsync(GetCartUrlwithId.GetCart(1));

                response.EnsureSuccessStatusCode();
            }
        }

        [Fact]
        public async Task Send_checkout_basket_and_response_ok_status_code()
        {
            using(var server = CreateServer())
            {
                var cartContent = new StringContent(BuildCart(), UTF8Encoding.UTF8, "application/json");

                await server.CreateClient().PostAsync(PostCartUrl.Cart, cartContent);

                var checkoutContent = new StringContent(BuildCheckout(), UTF8Encoding.UTF8, "application/json");
                var response = await server.CreateIdempotentClient().PostAsync(PostCartUrl.CheckoutOrder, checkoutContent);

                response.EnsureSuccessStatusCode();
            }
        }

        private string BuildCheckout()
        {
            return ""; 
        }

        private string BuildCart()
        {
            var order = new BorrowerBasket(AutoAuthorizeMiddleware.IDENTITY_ID);

            order.Items.Add(new CartItem
            {

            });

            return JsonConvert.SerializeObject(order);
        }
    }
}
