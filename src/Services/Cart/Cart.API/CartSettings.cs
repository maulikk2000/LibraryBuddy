﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryBuddy.Services.Cart.API
{
    public class CartSettings
    {
        public string ConnectionString { get; set; }
        public string EventBusConnection { get; set; }
    }
}
