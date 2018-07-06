using Microsoft.Extensions.Configuration;
using IdentityServer4.EntityFramework.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryBuddy.Services.Identity.API.Configuration;
using IdentityServer4.EntityFramework.Mappers;

namespace LibraryBuddy.Services.Identity.API.Data
{
    public class ConfigurationDbContextSeed
    {
        public async Task SeedAsync(ConfigurationDbContext context, IConfiguration configuration)
        {

            var clientUrls = new Dictionary<string, string>
            {
                { "MVC", configuration.GetValue<string>("MvcUrl") },
                { "Xamarin", configuration.GetValue<string>("XamarinUrl") },
                { "CartApi", configuration.GetValue<string>("CartApiClient") },
                //TODO: DO WE NEED BELOW TWO???
                //clientUrls.Add("LibraryCatalogApi", configuration.GetValue<string>("LibraryCatalogApiClient"));
                //clientUrls.Add("LibraryServicesApi", configuration.GetValue<string>("LibraryServicesApiClient"));
                { "LoanBookApi", configuration.GetValue<string>("LoanApiClient") },
                { "LateReturnApi", configuration.GetValue<string>("LateReturnApiClient") },
                { "LocationApi", configuration.GetValue<string>("LocationApiClient") },
                { "MobileBorrowAgg", configuration.GetValue<string>("MobileBorrowAggClient") },
                { "WebBorrowAgg", configuration.GetValue<string>("WebBorrowAggClient") }
            };

            //add clients
            if (!context.Clients.Any())
            {
                foreach (var client in Config.GetClients(clientUrls))
                {
                    context.Clients.Add(client.ToEntity());
                }

                await context.SaveChangesAsync();
            }
            //this block is to make sure if any change in the config entry, it is reflected properly
            else
            {
                //we dont have any ATM
            }

            //add identity resource
            if (!context.IdentityResources.Any())
            {
                foreach (var ir in Config.GetResources())
                {
                    await context.IdentityResources.AddAsync(ir.ToEntity());
                }

                await context.SaveChangesAsync();
            }

            //add API resouce
            if (!context.ApiResources.Any())
            {
                foreach (var api in Config.GetApis())
                {
                    await context.ApiResources.AddAsync(api.ToEntity());
                }

                await context.SaveChangesAsync();
            }

        }
    }
}
