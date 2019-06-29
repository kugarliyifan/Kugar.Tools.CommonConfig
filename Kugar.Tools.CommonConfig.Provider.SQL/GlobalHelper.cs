using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;

namespace Kugar.Tools.CommonConfig.Provider.SQL
{
    public static class GlobalHelper
    {
        public static IServiceCollection AddCachedDbCommonConfig(this IServiceCollection services,int expireTime=20000)
        {
            services.AddScoped(x =>
            {
                return new CachedDbConfigProvider((ISqlSugarClient) x.GetService(typeof(ISqlSugarClient)), expireTime);
            });

            return services;
        }

        public static IServiceCollection AddReadtimeDbCommonConfigManager(this ServiceCollection services)
        {
            services.AddScoped(x =>
            {
                return new RealtimeDbConfigProvider((ISqlSugarClient)x.GetService(typeof(ISqlSugarClient)));
            });

            return services;
        }
    }
}
