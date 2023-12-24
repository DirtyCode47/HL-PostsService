using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PostsService.Entities;
using StackExchange.Redis;

namespace PostsService.Cache
{
    public class CacheService
    {
        private readonly IConnectionMultiplexer _redisConnection;

        public CacheService(IConnectionMultiplexer redisConnection)
        {
            _redisConnection = redisConnection ?? throw new ArgumentNullException(nameof(redisConnection));
        }

        public void AddOrUpdateCache<T>(string key, T data)
        {
            var database = _redisConnection.GetDatabase();
            var serializedData = JsonConvert.SerializeObject(data);
            database.StringSet(key, serializedData);
        }

        public T GetFromCache<T>(string key)
        {
            var database = _redisConnection.GetDatabase();
            var serializedData = database.StringGet(key);
            if (serializedData.HasValue)
            {
                return JsonConvert.DeserializeObject<T>(serializedData);
            }
            return default;
        }

        public void ClearCache(string key)
        {
            var database = _redisConnection.GetDatabase();
            database.KeyDelete(key);
        }

        public void ClearAll()
        {
            var server = _redisConnection.GetServer(_redisConnection.GetEndPoints()[0]);
            server.FlushAllDatabases();
        }

        public void InitializeCache(List<Posts> posts)
        {
            var database = _redisConnection.GetDatabase();

            // Очищаем все данные в кэше перед инициализацией
            ClearAll();

            foreach (var post in posts)
            {
                var cacheKey = $"post:{post.Id}";
                var serializedPost = JsonConvert.SerializeObject(post);
                database.StringSet(cacheKey, serializedPost);
            }
        }
    }
}
