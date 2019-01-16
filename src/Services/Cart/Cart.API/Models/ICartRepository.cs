using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryBuddy.Services.Cart.API.Models
{
    public interface ICartRepository
    {
        Task<BorrowerCart> GetCartAsync(string borrowerId);
        IEnumerable<string> GetUsers();
        Task<BorrowerCart> UpdateCartAsync(BorrowerCart Cart);
        Task<bool> DeleteCartAsync(string id);
    }
}
