using LibraryBuddy.Identity.API;
using LibraryBuddy.Services.Identity.API.Helper;
using LibraryBuddy.Services.Identity.API.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryBuddy.Services.Identity.API.Data
{
    public class ApplicationDbContextSeed
    {
        public async Task SeedAsync(ApplicationDbContext context, 
                                    IHostingEnvironment env, 
                                    ILogger<ApplicationDbContext> logger, 
                                    IOptions<AppSettings> settings, int? retry = 0)
        {

            if (!context.Users.Any())
            {
                context.Users.AddRange(GetDefaultUser());

                await context.SaveChangesAsync();
            }

        }

        private IEnumerable<ApplicationUser> GetDefaultUser()
        {

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = "Admin",
                LastName = "AdminLastName",
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@gmail.com",
                NormalizedEmail = "ADMIN@GMAIL.COM",
                StreetAdress = new Address
                {
                    Street = "Line 1",
                    City = "Sydney",
                    State = "NSW",
                    Country = "Australia",
                    ZipCode = "2000"
                },
                DOB = "01/01/1990",
                LibraryCardId = "A123456",
                //https://msdn.microsoft.com/en-us/library/97af8hh4(v=vs.110).aspx
                //D 32 digits separated by hyphens: 00000000-0000-0000-0000-000000000000
                SecurityStamp = Guid.NewGuid().ToString("D")
            };
            SCryptPasswordHasher<ApplicationUser> hasher = new SCryptPasswordHasher<ApplicationUser>();

            user.PasswordHash = hasher.HashPassword(user, "Password1@3$");
            return new List<ApplicationUser>
            {
                user
            };
        }
    }
}
