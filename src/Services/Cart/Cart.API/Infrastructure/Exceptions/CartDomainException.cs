using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryBuddy.Services.Cart.API.Infrastructure.Exceptions
{
    public class CartDomainException : Exception
    {
        public CartDomainException()
        {

        }

        public CartDomainException(string message) : base(message)
        { }

        public CartDomainException(string message, Exception innerException)
            : base(message, innerException)
        {}
    }
}
