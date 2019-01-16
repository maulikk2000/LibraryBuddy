using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryBuddy.Services.Cart.API.Models
{
    public class BorrowerCart
    {
        public string Id { get; set; }

        public List<CartItem> Books { get; set; }

        public BorrowerCart(string id)
        {
            Id = id;
            Books = new List<CartItem>();
        }
    }
}
