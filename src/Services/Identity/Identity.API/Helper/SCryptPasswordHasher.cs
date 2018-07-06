using LibraryBuddy.Services.Identity.API.Models;
using Microsoft.AspNetCore.Identity;
using Scrypt;

namespace LibraryBuddy.Services.Identity.API.Helper
{
    public class SCryptPasswordHasher<T> : IPasswordHasher<ApplicationUser>
    {
        private readonly ScryptEncoder _encoder;

        public SCryptPasswordHasher()
        {
            _encoder = new ScryptEncoder(16384, 8, 1);
        }
        public string HashPassword(ApplicationUser user, string password)
        {
            return _encoder.Encode(password);
        }

        public PasswordVerificationResult VerifyHashedPassword(ApplicationUser user, string hashedPassword, string providedPassword)
        {
            if(_encoder.Compare(providedPassword, hashedPassword))
            {
                return PasswordVerificationResult.Success;
            }
            return PasswordVerificationResult.Failed;
        }
    }
}
