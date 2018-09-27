using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryBuddy.Services.Basket.API
{
    public class BasketSettings
    {
        public string ConnectionString { get; set; }
        public string EventBusConnection { get; set; }
    }
}
