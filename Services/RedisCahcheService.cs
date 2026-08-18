using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using StackExchange.Redis;
namespace ShopApi
{
    public interface ICacheService
    {
        Task<T> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? exception = null);
        Task RemoveAsync(string key);
        Task RemoveByPatternAsync(string pattern);
    }

    public class RedisCacheServices : ICacheService
    {
        private readonly IDistributedCache _cache;
        private ILogger<RedisCacheServices> _logger;
        private readonly IConnectionMultiplexer _redis;

        public RedisCacheServices(IDistributedCache cache, ILogger<RedisCacheServices> logger, IConnectionMultiplexer redis)
        {
            _cache = cache;
            _logger = logger;
            _redis = redis;
        }
        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var data = await _cache.GetStringAsync(key);
                if(string.IsNullOrEmpty(data))
                    return default;

                return JsonSerializer.Deserialize<T>(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения данных из кеша: {Key}", key);
                return default;
            }
        }
        public async Task SetAsync<T>(string key, T value, TimeSpan? exception = null)
        {
            try
            {
               var options = new DistributedCacheEntryOptions
               {
                   AbsoluteExpirationRelativeToNow = exception ?? TimeSpan.FromMinutes(10)
               };

               var json = JsonSerializer.Serialize(value);
               await _cache.SetStringAsync(key, json, options); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка сохранения в кеш: {Key}", key);
            }
        }
        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления из кеша: {Key}", key);
            }
        }
        public async Task RemoveByPatternAsync(string pattern)
        {
            try
            {
                var endpoints = _redis.GetEndPoints();
                if (!endpoints.Any())
                {
                    _logger.LogWarning("Нет доступных Redis-эндпоинтов");
                    return;
                }
                
                var server = _redis.GetServer(endpoints.First());

                var keys = server.Keys(pattern: $"{pattern}").ToArray();

                if (keys.Any())
                {
                    _logger.LogInformation("Найдено {Count} ключей для удаления", keys.Length);

                    foreach(var key in keys)
                    {
                        await _cache.RemoveAsync(key.ToString());
                    }

                    _logger.LogInformation("Удаленно  {Count} ключей по паттерну: {Pattern}", keys.Length, pattern);
                }
                else
                {
                    _logger.LogInformation("Ключи по паттерну не найдены: {Pattern}", pattern);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления по паттерну: {Pattern}", pattern);
            }
        }
    }
}