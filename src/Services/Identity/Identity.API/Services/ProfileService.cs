
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Services;
using LibraryBuddy.Services.Identity.API.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace LibraryBuddy.Services.Identity.API.Services
{
    public class ProfileService : IProfileService
    {

        //http://docs.identityserver.io/en/release/reference/profileservice.html?highlight=IProfileService
        //By default, identityserver only has the claims in the authentication cookie to draw upon for this identity data
        //It is impractical to put all of the possible claims needed for users into the cookie, so IdentityServer defines an
        //extensibility point for allowing claims to be dynamically loaded as needed for a user. This extensibility point is the IProfileService.


        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var subject = context.Subject ?? throw new ArgumentNullException(nameof(context.Subject));

            var id = subject.Claims.Where(c => c.Type == "sub").FirstOrDefault().Value;

            var user = await _userManager.FindByIdAsync(id);

            if (user == null) throw new ArgumentException("Invalid subject identifier");

            var claims = GetClaimsFromUser(user);
            context.IssuedClaims = claims.ToList();
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var subject = context.Subject ?? throw new ArgumentNullException(nameof(context.Subject));

            var id = subject.Claims.Where(c => c.Type == "sub").FirstOrDefault().Value;
            var user = await _userManager.FindByIdAsync(id);

            // in this method we need to make sure whether the user is allowed to obtain claims.
            // we need to make our own checks to make sure that user is allowed to obtain claims

            context.IsActive = false;

            if(user != null)
            {
                if (_userManager.SupportsUserSecurityStamp)
                {
                    var security_stamp = subject.Claims.Where(c => c.Type == "security_stamp").Select(c => c.Value).SingleOrDefault();

                    if(security_stamp != null)
                    {
                        var db_security = await _userManager.GetSecurityStampAsync(user);
                        if (db_security != security_stamp)
                            return;
                    }
                }

                context.IsActive = !user.LockoutEnabled ||
                                    !user.LockoutEnd.HasValue ||
                                    user.LockoutEnd <= DateTime.Now;
            }

        }

        private IEnumerable<Claim> GetClaimsFromUser(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, user.Id),
                new Claim(JwtClaimTypes.PreferredUserName, user.FirstName + " " + user.LastName),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)
            };

            if (!string.IsNullOrEmpty(user.FirstName))
                claims.Add(new Claim(JwtClaimTypes.Name, user.FirstName));
            if (!string.IsNullOrEmpty(user.LastName))
                claims.Add(new Claim(JwtClaimTypes.FamilyName, user.LastName));
            if (user.StreetAdress != null)
            {
                if (!string.IsNullOrEmpty(user.StreetAdress.City))
                    claims.Add(new Claim("city", user.StreetAdress.City));

                if (!string.IsNullOrEmpty(user.StreetAdress.Country))
                    claims.Add(new Claim("country", user.StreetAdress.Country));

                if (!string.IsNullOrEmpty(user.StreetAdress.State))
                    claims.Add(new Claim("state", user.StreetAdress.State));

                if (!string.IsNullOrEmpty(user.StreetAdress.Street))
                    claims.Add(new Claim("street", user.StreetAdress.Street));

                if (!string.IsNullOrEmpty(user.StreetAdress.ZipCode))
                    claims.Add(new Claim("zip_code", user.StreetAdress.ZipCode));

                if (!string.IsNullOrEmpty(user.StreetAdress.City))
                    claims.Add(new Claim("city", user.StreetAdress.City));
            }

            if (user.DOB != null)
                claims.Add(new Claim(JwtClaimTypes.BirthDate, user.DOB));

            if (!string.IsNullOrEmpty(user.Email))
                claims.Add(new Claim(JwtClaimTypes.Email, user.Email));

            if (!string.IsNullOrEmpty(user.PhoneNumber))
                claims.Add(new Claim(JwtClaimTypes.PhoneNumber, user.PhoneNumber));

            if (!string.IsNullOrEmpty(user.LibraryCardId))
                claims.Add(new Claim("libcardid", user.LibraryCardId));
            return claims;
                
                
        }
    }
}
