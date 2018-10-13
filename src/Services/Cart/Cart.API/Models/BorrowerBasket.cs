using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryBuddy.Services.Cart.API.Models
{
    public class BorrowerBasket
    {
        public string BorrowerId { get; set; }

        public List<CartItem> Items { get; set; }

        public BorrowerBasket(string borrowerId)
        {
            BorrowerId = borrowerId;
            Items = new List<CartItem>();
        }
    }
}
