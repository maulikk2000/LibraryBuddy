using Identity.API.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LibraryBuddy.Services.Identity.API.Services
{
    public class PwnedPasswordService : IPwnedPasswordService
    {
        public const string DefaultName = "PwnedPasswordsService";
        private readonly HttpClient _client;

        public PwnedPasswordService(HttpClient client)
        {
            _client = client;
        }
        public async Task<bool> IsPasswordPwned(string password, CancellationToken cancellationToken = default) 
        {
            //to call the API, we need to pass the first 5 characters of SHA1 password

            //compute SHA1 hash from helper class 
            var sha1pwd = SHA1Helper.ComputeSHA1Hash(password);
            var sha1Prefix = sha1pwd.Substring(0, 5);
            var sha1Suffix = sha1pwd.Substring(5);

            //https://haveibeenpwned.com/API/v2#SearchingPwnedPasswordsByRange
            //search pwd by range

            try
            {
                var response = await _client.GetAsync("range/" + sha1Prefix, cancellationToken);


                if (response.IsSuccessStatusCode)
                {
                    var result = await Contains(response.Content, sha1Suffix);

                    if (result.isPwned)
                    {

                    }
                    else
                    {

                    }

                    return result.isPwned;
                }
            }
            catch (Exception)
            {

                throw;
            }

            return false;
            
        }

        internal static async Task<(bool isPwned, int frequency)> Contains(HttpContent content, string sha1Suffix)
        {
            using (var streamReader = new StreamReader(await content.ReadAsStreamAsync()))
            {
                while (!streamReader.EndOfStream)
                {
                    var line = await streamReader.ReadLineAsync();
                    var segments = line.Split(':');
                    if (segments.Length == 2
                        && string.Equals(segments[0], sha1Suffix, StringComparison.OrdinalIgnoreCase)
                        && int.TryParse(segments[1], out var count))
                    {
                        return (true, count);
                    }
                }
            }

            return (false, 0);

        }
    }
}
