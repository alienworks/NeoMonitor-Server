using System;
using Microsoft.EntityFrameworkCore;
using NeoMonitor.Analysis;
using NeoMonitor.Analysis.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNeoAnalysisWebModule(this IServiceCollection services, Action<DbContextOptionsBuilder> dbContextOptionsAction = null)
        {
            services.AddDbContext<AnalysisDbContext>(dbContextOptionsAction, ServiceLifetime.Scoped, ServiceLifetime.Scoped);

            services
                .AddSingleton<IpVisitorService>()
                .AddHostedService<IpVisitorHostService>();
            return services;
        }
    }
}