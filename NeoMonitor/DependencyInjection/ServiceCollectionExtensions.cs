using System;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NeoMonitor;
using NeoMonitor.App.Abstractions.Services;
using NeoMonitor.App.Abstractions.Services.Data;
using NeoMonitor.App.Profiles;
using NeoMonitor.App.Services;
using NeoMonitor.Basics;
using NeoMonitor.Common.IP;
using NeoMonitor.Common.IP.Services;
using NeoMonitor.DbContexts;
using NeoMonitor.Hubs;
using NeoMonitor.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static INeoMonitorModuleBuilder AddBasicServices(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddCors(options =>
            {
                options.AddPolicy("DEV",
                    builder =>
                    {
                        builder
                            .WithOrigins("http://localhost:8111", "http://localhost:4200", "http://localhost:9876")
                            .AllowCredentials()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });
            services.AddControllers();
            services.AddSignalR()
                .AddMessagePackProtocol();
            return new NeoMonitorModuleBuilder(services);
        }

        public static INeoMonitorModuleBuilder AddThirdPartyServices(this INeoMonitorModuleBuilder builder)
        {
            var services = builder.Services;
            services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);
            services.AddSwaggerDocument(config =>
            {
                config.PostProcess = document =>
                {
                    document.Info.Version = "v1";
                    document.Info.Title = "NeoMonitor APIs";
                    document.Info.Description = "APIs of NeoMonitor-Server";
                    document.Info.TermsOfService = "None";
                    document.Info.Contact = new NSwag.OpenApiContact
                    {
                        Name = "Github Repository",
                        Email = string.Empty,
                        Url = "https://github.com/alienworks/NeoMonitor-Server"
                    };
                    //document.Info.License = new NSwag.OpenApiLicense
                    //{
                    //};
                };
            });
            return builder;
        }

        public static INeoMonitorModuleBuilder AddInternalServices(this INeoMonitorModuleBuilder builder, IConfiguration configuration)
        {
            var services = builder.Services;

            services.AddHttpClient<ILocateIpService, IpStackService>();

            services.AddNeoRpcHttpClient(c => c.ApiVersion = new Version(2, 0))
               .AddNeoJsonRpcAPIs();

            services.AddDbContext<NeoMonitorContext>(options =>
            {
                options.UseMySql(configuration.GetConnectionString("DefaultConnection"));
            }, ServiceLifetime.Scoped, ServiceLifetime.Scoped);

            services.AddSingleton<NodeSynchronizer>();
            services.AddSingleton<NodeTicker>();
            services.AddSingleton<IHostedService, NotificationHostService>();

            services.AddTransient<INodeSeedsLoaderFactory, NodeSeedsLoaderFactory>();
            services.AddTransient<IRawMemPoolDataLoader, DefaultRawMemPoolDataLoader>();
            services.AddTransient<IStartupFilter, NodeSeedsStartupFilter>();

            return builder;
        }

        public static INeoMonitorModuleBuilder AddAnalysisServices(this INeoMonitorModuleBuilder builder, IConfiguration configuration)
        {
            var services = builder.Services;

            services.AddAnalysisWebModule(dbContextOptionsAction: options =>
            {
                options.UseMySql(configuration.GetConnectionString("AnalysisDevConnection"));
            });
            return builder;
        }
    }
}