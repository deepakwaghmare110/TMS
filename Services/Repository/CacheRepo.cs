using StackExchange.Redis;
using System.Text.Json;
using TMS.Services.Interfaces;

namespace TMS.Services.Repository
{
    public class CacheRepo : ICache
    {

        private readonly IDatabase _Cachedatabase;

        public CacheRepo() {
            var redis = ConnectionMultiplexer.Connect("localhost:6379");
            _Cachedatabase = redis.GetDatabase();
        }
        public T GetData<T>(string key)
        {
            var value = _Cachedatabase.StringGet(key);
            if (!string.IsNullOrEmpty(value))
            {
                return JsonSerializer.Deserialize<T>(value!)!;
            }
            else {
                return default!;
            }
        }

        public object RemoveData(string key)
        {
            var exist = _Cachedatabase.KeyExists(key);
            if (exist)
            {
                return _Cachedatabase.KeyDelete(key);
            }
            return false;
        }

        public bool SetData<T>(string key, T value, DateTimeOffset expiry)
        {
            var expiryTime = expiry.DateTime.Subtract(DateTime.Now);
            return _Cachedatabase.StringSet(key, JsonSerializer.Serialize(value), expiryTime);
        }
    }
}
