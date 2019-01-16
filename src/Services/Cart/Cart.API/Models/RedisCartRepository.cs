using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryBuddy.Services.Cart.API.Models
{
    public class RedisCartRepository : ICartRepository
    {
        private readonly ILogger<RedisCartRepository> _logger;
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;

        public RedisCartRepository(ILogger<RedisCartRepository> logger, ConnectionMultiplexer redis)
        {
            _logger = logger;
            _redis = redis;
            _database = redis.GetDatabase();
        }
        public async Task<bool> DeleteCartAsync(string id)
        {
            return await _database.KeyDeleteAsync(id);
        }

        public async Task<BorrowerCart> GetCartAsync(string borrowerId)
        {
            var Cart = await _database.StringGetAsync(borrowerId);
            if(Cart.IsNullOrEmpty)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<BorrowerCart>(Cart);
        }

        public IEnumerable<string> GetUsers()
        {
            var server = GetServer();
            var data = server.Keys();
            return data?.Select(d => d.ToString());
        }

        private IServer GetServer()
        {
            var endpoint = _redis.GetEndPoints();
            return _redis.GetServer(endpoint.First());
        }

        public async Task<BorrowerCart> UpdateCartAsync(BorrowerCart Cart)
        {
            var CartCreated = await _database.StringSetAsync(Cart.Id, JsonConvert.SerializeObject(Cart));

            if (!CartCreated)
            {
                _logger.LogInformation("Problem persisting Cart item.");
                return null;
            }
            return await GetCartAsync(Cart.Id);
        }
    }
}
