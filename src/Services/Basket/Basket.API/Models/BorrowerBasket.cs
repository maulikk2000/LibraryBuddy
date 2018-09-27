using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryBuddy.Services.Basket.API.Models
{
    public class BorrowerBasket
    {
        public string BorrowerId { get; set; }

        public List<BasketItem> Items { get; set; }

        public BorrowerBasket(string borrowerId)
        {
            BorrowerId = borrowerId;
            Items = new List<BasketItem>();
        }
    }
}
