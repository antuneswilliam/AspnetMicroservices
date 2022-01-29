using Basket.API.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Basket.API.Repositories
{
    public class BasketRepository : IBasketRepository
    {
        private readonly IDistributedCache _redisCache;

        public BasketRepository(IDistributedCache redisCache)
        {
            this._redisCache = redisCache ?? throw new ArgumentNullException(nameof(redisCache));
        }

        public async Task<ShoppingCart> GetBasket(string userName)
        {
            var basket = await _redisCache.GetStringAsync(userName);

            if (string.IsNullOrEmpty(basket))
                return null;

            //return JsonSerializer.Deserialize<ShoppingCart>(basket);

            return JsonConvert.DeserializeObject<ShoppingCart>(basket);

        }

        public async  Task<ShoppingCart> UpdateBasket(ShoppingCart basket)
        {
            var json = JsonConvert.SerializeObject(basket);

            await _redisCache.SetStringAsync(basket.UserName, json);

            return await GetBasket(basket.UserName);
        }

        public async Task DeleteBasket(string userName)
        {
            await _redisCache.RemoveAsync(userName);
        }
    }
}
