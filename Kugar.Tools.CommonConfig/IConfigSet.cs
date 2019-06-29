using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Kugar.Tools.CommonConfig
{
    public interface IConfigSet
    {
        int GetInt(string key, int defaultValue = default);

        string GetString(string key, string defaultValue = default);

        bool GetBool(string key,  bool defaultValue = false);

        decimal GetDecimal(string key, decimal defaultValue = default);

        int? GetIntNullable(string key,int? defaultValue = default);

        bool? GetBoolNullable(string key, bool? defaultValue = default);

        JObject GetJson(string key, JObject defaultValue = default);

        (string key, object value)[] GetByKeys(string[] keys)
    }

    public interface IAsyncConfigSet
    {
        Task<int> GetIntAsync(string key, int defaultValue = default);

        Task<string> GetStringAsync(string key, string defaultValue = default);

        Task<bool> GetBoolAsync(string key, bool defaultValue = false);

        Task<decimal> GetDecimalAsync(string key, decimal defaultValue = default);

        Task<int?> GetIntNullableAsync(string key, int? defaultValue = default);

        Task<bool?> GetBoolNullableAsync(string key, bool? defaultValue = default);

        Task<JObject> GetJsonAsync(string key, JObject defaultValue = default);

        Task<(string key, object value)[]> GetByKeysAsync(string[] keys);
    }

    
}