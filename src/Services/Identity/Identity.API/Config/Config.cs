using IdentityServer4;
using IdentityServer4.Models;
using LibraryBuddy.Services.Identity.API.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryBuddy.Services.Identity.API.Configuration
{
    public class Config
    {
        public static IEnumerable<ApiResource> GetApis()
        {
            return new List<ApiResource>
            {
                new ApiResource("cart","Cart Service"),
                new ApiResource("loanbook","Loan Book Service"),
                new ApiResource("location","Location Service"),
                new ApiResource("latereturnfee","Late Return Fee Service"),
                new ApiResource("mobileborrowagg","Mobile Borrow Aggregator"),
                new ApiResource("webborrowagg","Web Borrow Aggregator"),
                new ApiResource("payment", "Payment Service")
            };
        }

        public static IEnumerable<IdentityResource> GetResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email()
            };
        }
        public static IEnumerable<Client> GetClients(Dictionary<string, string> clientUrls)
        {
            return new List<Client>
            {
                //TODO: COME BACK AND MORE IF REQUIRED

                //xamarin client
                new Client
                {
                    ClientId = "xamarin",
                    ClientName = "Lib Bud Xamarin Client",
                    AllowedGrantTypes = GrantTypes.Hybrid,
                    ClientSecrets =
                    {
                        new Secret("xamarinsecret".Sha512())
                    },
                    RedirectUris = { clientUrls["Xamarin"] },
                    RequireConsent = false,
                    RequirePkce = true,
                    PostLogoutRedirectUris = { $"{clientUrls["Xamarin"]}/Account/Redirecting" },
                    AllowedCorsOrigins = { "" }, //TODO: leave to empty string, come back to it later 
                    AllowOfflineAccess = true, //this allows requesting refresh tokens for long lived API access http://docs.identityserver.io/en/release/quickstarts/5_hybrid_and_api_access.html
                    AllowAccessTokensViaBrowser = true,
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        LibraryBuddyConstants.StandardScopes.LoanBook,
                        LibraryBuddyConstants.StandardScopes.Cart,
                        LibraryBuddyConstants.StandardScopes.Location,
                        LibraryBuddyConstants.StandardScopes.MobileBorrowAgg
                    }                    
                },
                //MVC Client
                new Client
                {
                    ClientId = "mvc",
                    ClientName = "MVC Client",
                    ClientSecrets =
                    {
                        new Secret("mvcsecret".Sha512())
                    },
                    ClientUri = $"{clientUrls["MVC"]}",
                    AllowedGrantTypes = GrantTypes.Hybrid,
                    RequireConsent = false,
                    AllowOfflineAccess = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    RedirectUris =
                    {
                        $"{clientUrls["MVC"]}/sigin-oidc"
                    },
                    PostLogoutRedirectUris =
                    {
                        $"{clientUrls["MVC"]}/signout-callback-oidc"
                    },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        LibraryBuddyConstants.StandardScopes.LoanBook,
                        LibraryBuddyConstants.StandardScopes.Cart,
                        LibraryBuddyConstants.StandardScopes.Location,
                        LibraryBuddyConstants.StandardScopes.WebBorrowAgg
                    }
                },
                //MVC test Client
                new Client
                {
                    ClientId = "mvc test",
                    ClientName = "MVC test Client",
                    ClientSecrets =
                    {
                        new Secret("mvctestsecret".Sha512())
                    },
                    ClientUri = $"{clientUrls["MVC"]}",
                    AllowedGrantTypes = GrantTypes.Hybrid,
                    RequireConsent = false,
                    AllowOfflineAccess = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    RedirectUris =
                    {
                        $"{clientUrls["MVC"]}/sigin-oidc"
                    },
                    PostLogoutRedirectUris =
                    {
                        $"{clientUrls["MVC"]}/signout-callback-oidc"
                    },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        LibraryBuddyConstants.StandardScopes.LoanBook,
                        LibraryBuddyConstants.StandardScopes.Cart,
                        LibraryBuddyConstants.StandardScopes.Location,
                        LibraryBuddyConstants.StandardScopes.WebBorrowAgg
                    }
                },
                //location swagger UI
                new Client
                {
                    ClientId = "locationswaggerui",
                    ClientName = "Location Swagger UI",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RedirectUris =
                    {
                        $"{clientUrls["LocationApi"]}/swagger/oauth2-redirect.html"
                    },
                    PostLogoutRedirectUris =
                    {
                        $"{clientUrls["LocationApi"]}/swagger/"
                    },
                    AllowedScopes =
                    {
                        LibraryBuddyConstants.StandardScopes.Location
                    }
                },
                //Cart Swagger UI
                new Client
                {
                    ClientId = "cartswaggerui",
                    ClientName = "Cart Swagger UI",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RedirectUris =
                    {
                        $"{clientUrls["CartApi"]}/swagger/oauth2-redirect.html"
                    },
                    PostLogoutRedirectUris =
                    {
                        $"{clientUrls["CartApi"]}/swagger/"
                    },
                    AllowedScopes =
                    {
                        LibraryBuddyConstants.StandardScopes.Cart
                    }
                },
                //Loan Book Swagger UI
                new Client
                {
                    ClientId = "loanbookswaggerui",
                    ClientName = "Loan Book Swagger UI",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RedirectUris =
                    {
                        $"{clientUrls["LoanBookApi"]}/swagger/oauth2-redirect.html"
                    },
                    PostLogoutRedirectUris =
                    {
                        $"{clientUrls["LoanBookApi"]}/swagger/"
                    },
                    AllowedScopes =
                    {
                        LibraryBuddyConstants.StandardScopes.LoanBook
                    }
                },
                //Late Return Swagger UI
                new Client
                {
                    ClientId = "latereturnswaggerui",
                    ClientName = "Late Return Swagger UI",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RedirectUris =
                    {
                        $"{clientUrls["LateReturnApi"]}/swagger/oauth2-redirect.html"
                    },
                    PostLogoutRedirectUris =
                    {
                        $"{clientUrls["LateReturnApi"]}/swagger/"
                    },
                    AllowedScopes =
                    {
                        LibraryBuddyConstants.StandardScopes.LoanBook
                    }
                }
            };
        }


    }
}
