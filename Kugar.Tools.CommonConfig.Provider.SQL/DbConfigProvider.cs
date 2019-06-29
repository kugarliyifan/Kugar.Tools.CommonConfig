using System;
using System.Collections.Concurrent;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Kugar.Core.ExtMethod;
using Microsoft.Extensions.Caching.Memory;
using SqlSugar;
using DbType = SqlSugar.DbType;

namespace Kugar.Tools.CommonConfig.Provider.SQL
{
    /// <summary>
    /// 实时配置项sql提供器,本地不缓存配置项数据
    /// </summary>
    public class RealtimeDbConfigProvider:IConfigProvider
    {
        private ISqlSugarClient _client = null;

        public RealtimeDbConfigProvider(SqlSugar.ISqlSugarClient client)
        {
            _client = client;
        }

        public async Task<object> GetValueAsync(string authType, string key, object defaultValue = default)
        {
            var value = await _client.Queryable<sys_plus_Config>().Where(x => x.AuthType == authType && x.Key == key)
                .Select(x => x.Value).SingleAsync();

            return value?? defaultValue;
        }

        public async Task<(string key,string value)[]> GetValueAsync(string authType, params string[] keys)
        {
            var values = await _client.Queryable<sys_plus_Config>()
                .Where(x => x.AuthType == authType && keys.Contains(x.Key))
                .Select(x => new
                {
                    x.Key,
                    x.Value
                }).ToListAsync();

            return values.Select(x=>(x.Key,x.Value)).ToArrayEx();
        }

        public object GetValue(string authType, string key, object defaultValue = default)
        {
            var value = _client.Queryable<sys_plus_Config>().Where(x => x.AuthType == authType && x.Key == key)
                .Select(x => x.Value).Single();

            return value?? defaultValue;
        }

        public (string key, string value)[] GetValue(string authType, params string[] keys)
        {
            var values = _client.Queryable<sys_plus_Config>()
                .Where(x => x.AuthType == authType && keys.Contains(x.Key))
                .Select(x => new
                {
                    x.Key,
                    x.Value
                }).ToList();

            return values.Select(x => (x.Key, x.Value)).ToArrayEx();
        }

        public ResultReturn SetValue<TValue>(string authType, string key, TValue value)
        {
            if (_client.CurrentConnectionConfig.DbType== DbType.SqlServer)
            {
                var sql = $@"declare @result int

                        MERGE sys_plus_Config
                        USING ( 
                            VALUES (@authType, @key)
                        ) AS foo (authType1,key1) 
                        ON sys_plus_Config.authType = foo.authType1 and  sys_plus_Config.key = foo.key1
                        WHEN MATCHED THEN UPDATE SET value = @value 
                        WHEN NOT MATCHED THEN INSERT  (authType,key,Value,LastUpdateDt) VALUES (@authType, @key,@value,getdate());
              
                        ";

                try
                {
                    var ret = _client.Ado.ExecuteCommand(sql, new[]
                    {
                        new SugarParameter("@authType", authType),
                        new SugarParameter("@key", key),
                        new SugarParameter("@value", value),
                    });

                    return ResultReturn.Create(ret>0);
                }
                catch (Exception e)
                {
                    return new FailResultReturn(e);
                }
            }
            else
            {
                return new FailResultReturn("尚未支持该数据库");
            }
        }

        public ResultReturn SetValue<TValue>(string authType, (string key, TValue value)[] items)
        {
            if (_client.CurrentConnectionConfig.DbType == DbType.SqlServer)
            {
                var sql = $@"
                        MERGE sys_plus_Config
                        USING ( 
                            VALUES (@authType, @key)
                        ) AS foo (authType1,key1) 
                        ON sys_plus_Config.authType = foo.authType1 and  sys_plus_Config.key = foo.key1
                        WHEN MATCHED THEN UPDATE SET value = @value 
                        WHEN NOT MATCHED THEN INSERT  (authType,key,Value,LastUpdateDt) VALUES (@authType, @key,@value,getdate());
              
                        ";

                _client.BeginTran();

                try
                {
                    foreach (var item in items)
                    {
                        var ret = _client.Ado.ExecuteCommand(sql, new[]
                        {
                            new SugarParameter("@authType", authType),
                            new SugarParameter("@key", item.key),
                            new SugarParameter("@value", item.value),
                        });

                        if (ret<=0)
                        {
                            _client.RollbackTran();

                            return new FailResultReturn($"key={item.key}修改出错,");
                        }
                    } 

                    _client.CommitTran();

                    return SuccessResultReturn.Default;
                }
                catch (Exception e)
                {
                    _client.RollbackTran();
                    return new FailResultReturn(e);
                }
            }
            else
            {
                return new FailResultReturn("尚未支持该数据库");
            }
        }

        public void Reload()
        {
            
        }
    }

    /// <summary>
    /// 带缓存的提供器,适用于修改不太频繁的场景
    /// </summary>
    public class CachedDbConfigProvider : IConfigProvider
    {
        private ISqlSugarClient _client = null;
        private static ConcurrentDictionary<string,MemoryCache> _cacheDic = new ConcurrentDictionary<string, MemoryCache>();
        private static ConcurrentDictionary<string, ReaderWriterLockSlim> _lockers = new ConcurrentDictionary<string, ReaderWriterLockSlim>();
        private int _timeOut = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="cacheTimeout">配置项缓存时间</param>
        public CachedDbConfigProvider(SqlSugar.ISqlSugarClient client,int cacheTimeout=20000)
        {
            _client = client;

            

            _timeOut = cacheTimeout;
        }


        public async Task<object> GetValueAsync(string authType, string key, object defaultValue = default)
        {
            return getValue<string>(authType, key);
        }

        public async Task<(string key, string value)[]> GetValueAsync(string authType, params string[] keys)
        {
            return keys.Select(x => (x, getValue<string>(authType, x))).ToArrayEx();
        }

        public object GetValue(string authType, string key, object defaultValue = default)
        {
            return getValue<string>(authType, key);
        }

        public (string key, string value)[] GetValue(string authType, params string[] keys)
        {
            return keys.Select(x => (x, getValue<string>(authType, x))).ToArrayEx();
        }

        public ResultReturn SetValue<TValue>(string authType, string key, TValue value)
        {
            throw new NotImplementedException();
        }

        public ResultReturn SetValue<TValue>(string authType, (string key, TValue value)[] items)
        {
            throw new NotImplementedException();
        }

        private TValue getValue<TValue>(string authType, string key)
        {
            var cache = GetCache(authType);
            var lockerObject = getLocker(authType);

            lockerObject.EnterUpgradeableReadLock();

            try
            {
                if (cache.TryGetValue(key, out var value))
                {
                    if (value is TValue)
                    {
                        return (TValue) value;
                    }
                    else
                    {
                        var newValue = (TValue) value.ToStringEx().ConvertToPrimitive(typeof(TValue), default(TValue));

                        cache.Set(key, value, TimeSpan.FromMilliseconds(_timeOut));

                        return newValue;
                    }
                }
                else
                {
                    lockerObject.EnterWriteLock();

                    try
                    {
                        var newvValue = _client.Queryable<sys_plus_Config>().Where(x => x.AuthType == authType)
                            .Select(x => x.Value).Single();

                        var v = (TValue) newvValue.ConvertToPrimitive(typeof(TValue), default(TValue));

                        cache.Set(key, v, TimeSpan.FromMilliseconds(_timeOut));

                        return v;
                    }
                    finally
                    {
                        lockerObject.ExitWriteLock();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                lockerObject.ExitUpgradeableReadLock();
            }
            
        }

        private ReaderWriterLockSlim getLocker(string authType)
        {
            return _lockers.GetOrAdd(authType, x => new ReaderWriterLockSlim());
        }

        private IMemoryCache GetCache(string authType)
        {
            return _cacheDic.GetOrAdd(authType, x =>
            {
                var cache = new MemoryCache(new MemoryCacheOptions());

                var lst=_client.Queryable<sys_plus_Config>().Where(y => y.AuthType == authType)
                    .Select(y => new
                    {
                        y.Key, y.Value
                    }).ToList();

                foreach (var item in lst)
                {
                    cache.Set(item.Key, item.Value, TimeSpan.FromMilliseconds(_timeOut));
                }

                return cache;
            });
        }

        public void Reload()
        {
            throw new NotImplementedException();
        }
    }
}   
