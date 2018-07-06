using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryBuddy.Services.Identity.API.Helper
{
    public static class LibraryBuddyConstants
    {
        public static class StandardScopes
        {
            public const string Cart = "cart";
            public const string LoanBook = "loanbook";
            public const string Location = "location";
            public const string LateReturnFee = "latereturnfee";
            public const string MobileBorrowAgg = "mobileborrowagg";
            public const string WebBorrowAgg = "webborrowagg";
            public const string Payment = "payment";

        }
    }
}
