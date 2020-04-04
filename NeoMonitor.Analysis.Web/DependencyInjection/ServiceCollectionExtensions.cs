using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using NeoMonitor.Analysis.Web;
using NeoMonitor.Analysis.Web.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAnalysisWebModule(this IServiceCollection services, Action<DbContextOptionsBuilder> dbContextOptionsAction = null)
        {
            services.AddDbContext<AnalysisDbContext>(dbContextOptionsAction, ServiceLifetime.Scoped, ServiceLifetime.Scoped);
            services.AddSingleton<IHostedService, IpVisitorHostService>();
            return services;
        }
    }
}