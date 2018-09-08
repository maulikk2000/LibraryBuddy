using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LibraryBuddy.Services.Identity.API.Services
{
    public interface IPwnedPasswordService
    {
        Task<bool> IsPasswordPwned(string password, CancellationToken cancellationToken = default);
    }
}
