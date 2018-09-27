using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryBuddy.Services.Basket.API.Models
{
    public class RedisBasketRepository : IBasketRepository
    {
        private readonly ILogger<RedisBasketRepository> _logger;
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;

        public RedisBasketRepository(ILogger<RedisBasketRepository> logger, ConnectionMultiplexer redis)
        {
            _logger = logger;
            _redis = redis;
            _database = redis.GetDatabase();
        }
        public async Task<bool> DeleteBasketAsync(string id)
        {
            return await _database.KeyDeleteAsync(id);
        }

        public async Task<BorrowerBasket> GetBasketAsync(string borrowerId)
        {
            var basket = await _database.StringGetAsync(borrowerId);
            if(basket.IsNullOrEmpty)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<BorrowerBasket>(basket);
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

        public async Task<BorrowerBasket> UpdateBasketAsync(BorrowerBasket basket)
        {
            var basketCreated = await _database.StringSetAsync(basket.BorrowerId, JsonConvert.SerializeObject(basket));

            if (!basketCreated)
            {
                _logger.LogInformation("Problem persisting basket item.");
                return null;
            }
            return await GetBasketAsync(basket.BorrowerId);
        }
    }
}
