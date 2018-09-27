using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryBuddy.Services.Basket.API.Models
{
    public interface IBasketRepository
    {
        Task<BorrowerBasket> GetBasketAsync(string borrowerId);
        IEnumerable<string> GetUsers();
        Task<BorrowerBasket> UpdateBasketAsync(BorrowerBasket basket);
        Task<bool> DeleteBasketAsync(string id);
    }
}
