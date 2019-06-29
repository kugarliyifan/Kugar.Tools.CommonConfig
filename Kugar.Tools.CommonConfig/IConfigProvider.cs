using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Microsoft.Extensions.Configuration;

namespace Kugar.Tools.CommonConfig
{
    public interface IConfigProvider
    {
        Task<object> GetValueAsync(string authType,  string key, object defaultValue = default);

        Task<(string key, string value)[]> GetValueAsync(string authType, params string[] keys);

        object GetValue(string authType, string key, object defaultValue = default);

        (string key, string value)[] GetValue(string authType, params string[] keys);

        ResultReturn SetValue<TValue>(string authType, string key, TValue value);

        ResultReturn SetValue<TValue>(string authType, (string key, TValue value)[] items);

        void Reload();
    }
}
