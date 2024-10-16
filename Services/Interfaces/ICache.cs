namespace TMS.Services.Interfaces
{
    public interface ICache
    {
        T GetData<T>(string key);
        bool SetData<T>(string key, T value, DateTimeOffset expiry);
        object RemoveData(string key);
    }
}
