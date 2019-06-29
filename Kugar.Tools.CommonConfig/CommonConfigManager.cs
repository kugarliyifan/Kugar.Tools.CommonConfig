using System;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace Kugar.Tools.CommonConfig
{
    /// <summary>
    /// 提供基于数据库的通用配置项功能,使用时候,需基于DI,先注册 IConfigProvider 接口
    /// </summary>
    public class DbConfigManager
    {
        private static DbConfigManager _default=new DbConfigManager();
        //private IServiceCollection _services = null;

        public DbConfigManager Default => _default;

        /// <summary>
        /// 默认的容器,基于DI获取IConfigProvider
        /// </summary>
        public IServiceProvider Services { set; get; }

        /// <summary>
        /// 返回基于authType配置集合,使用authType区分不同的类型,如前台/后台,也可以传空字符串
        /// </summary>
        /// <param name="authType"></param>
        /// <returns></returns>
        public IAsyncConfigSet this[string authType]
        {
            get { return GetAsyncSet(authType); }
        }

        public IConfigSet GetSet(string authType)
        {
            return new DefaultConfigSet((IConfigProvider)Services.GetService(typeof(IConfigProvider)),authType);
        }

        public IAsyncConfigSet GetAsyncSet(string authType)
        {
            return new DefaultConfigSet((IConfigProvider)Services.GetService(typeof(IConfigProvider)), authType);
        }

        public ResultReturn SetValue<TValue>(string authType, string key, TValue value)
        {
            var provider = (IConfigProvider) Services.GetService(typeof(IConfigProvider));

            return provider.SetValue(authType, key, value);
        }

        public ResultReturn SetValue<TValue>(string authType, (string key, TValue value)[] items)
        {
            var provider = (IConfigProvider)Services.GetService(typeof(IConfigProvider));

            return provider.SetValue(authType, items);
        }

        private class DefaultConfigSet : IConfigSet, IAsyncConfigSet
        {
            private IConfigProvider _factory = null;
            private string _authType = "";

            public DefaultConfigSet(IConfigProvider provider,string authType)
            {
                _factory = provider;
                _authType = authType;
            }

            public int GetInt(string key, int defaultValue = default)
            {
                try
                {
                    var v = _factory.GetValue(_authType, key);

                    return v.ToInt(defaultValue);
                }
                catch (Exception e)
                {
                    return defaultValue;
                }
            }

            public string GetString(string key, string defaultValue = default)
            {
                try
                {
                    var v = _factory.GetValue(_authType, key);

                    return v.ToStringEx(defaultValue);
                }
                catch (Exception e)
                {
                    return defaultValue;
                }
            }

            public bool GetBool(string key, bool defaultValue = false)
            {
                try
                {
                    var v = _factory.GetValue(_authType, key);

                    return v.ToBool(defaultValue);
                }
                catch (Exception e)
                {
                    return defaultValue;
                }
            }

            public decimal GetDecimal(string key, decimal defaultValue = default)
            {
                try
                {
                    var v = _factory.GetValue(_authType, key);

                    return v.ToDecimal(defaultValue);
                }
                catch (Exception e)
                {
                    return defaultValue;
                }
            }

            public int? GetIntNullable(string key, int? defaultValue = default)
            {
                try
                {
                    var v = _factory.GetValue(_authType, key);

                    return v.ToIntNullable(defaultValue);
                }
                catch (Exception e)
                {
                    return defaultValue;
                }
            }

            public bool? GetBoolNullable(string key, bool? defaultValue = default)
            {
                try
                {
                    var v = _factory.GetValue(_authType, key);

                    return v.ToBoolNullable(defaultValue);
                }
                catch (Exception e)
                {
                    return defaultValue;
                }
            }

            public JObject GetJson(string key, JObject defaultValue = default)
            {
                try
                {
                    var v = _factory.GetValue(_authType, key);

                    var jsonStr = v.ToStringEx();

                    return JObject.Parse(jsonStr);
                }
                catch (Exception e)
                {
                    return defaultValue;
                }
            }

            public (string key, object value)[] GetByKeys(string[] keys)
            {
                return _factory.GetValue(_authType, keys);
            }

            public async Task<int> GetIntAsync(string key, int defaultValue = default)
            {
                try
                {
                    var v =await _factory.GetValueAsync(_authType, key);

                    return v.ToInt(defaultValue);
                }
                catch (Exception e)
                {
                    return defaultValue;
                }
            }

            public async Task<string> GetStringAsync(string key, string defaultValue = default)
            {
                try
                {
                    var v = await _factory.GetValueAsync(_authType, key);

                    return v.ToStringEx(defaultValue);
                }
                catch (Exception e)
                {
                    return defaultValue;
                }
            }

            public async Task<bool> GetBoolAsync(string key, bool defaultValue = false)
            {
                try
                {
                    var v = await _factory.GetValueAsync(_authType, key);

                    return v.ToBool(defaultValue);
                }
                catch (Exception e)
                {
                    return defaultValue;
                }
            }

            public async Task<decimal> GetDecimalAsync(string key, decimal defaultValue = default)
            {
                try
                {
                    var v = await _factory.GetValueAsync(_authType, key);

                    return v.ToDecimal(defaultValue);
                }
                catch (Exception e)
                {
                    return defaultValue;
                }
            }

            public async Task<int?> GetIntNullableAsync(string key, int? defaultValue = default)
            {
                try
                {
                    var v = await _factory.GetValueAsync(_authType, key);

                    return v.ToIntNullable(defaultValue);
                }
                catch (Exception e)
                {
                    return defaultValue;
                }
            }

            public async Task<bool?> GetBoolNullableAsync(string key, bool? defaultValue = default)
            {
                try
                {
                    var v = await _factory.GetValueAsync(_authType, key);

                    return v.ToBoolNullable(defaultValue);
                }
                catch (Exception e)
                {
                    return defaultValue;
                }
            }

            public async Task<JObject> GetJsonAsync(string key, JObject defaultValue = default)
            {
                try
                {
                    var v =await _factory.GetValueAsync(_authType, key);

                    var jsonStr = v.ToStringEx();

                    return JObject.Parse(jsonStr);
                }
                catch (Exception e)
                {
                    return defaultValue;
                }
            }

            public async Task<(string key, object value)[]> GetByKeysAsync(string[] keys)
            {
                return await _factory.GetValueAsync(_authType, keys);
            }
        }
    }
}