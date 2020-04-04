using System;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NeoMonitor.Data;
using NeoMonitor.Data.Seed;
using NeoMonitor.Infrastructure.Mapping;
using NodeMonitor.Hubs;
using NodeMonitor.Infrastructure;
using NodeMonitor.Services;
using NodeMonitor.Web.Abstraction.DataLoaders;

namespace NodeMonitor
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Basic Modules
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
            // Third-Party Models
            services.AddAutoMapper(AutoMapperConfig.InitMap, typeof(AutoMapperConfig));
            services.AddSwaggerDocument(config =>
            {
                config.PostProcess = document =>
                {
                    document.Info.Version = "v1";
                    document.Info.Title = "Neo-Monitor API";
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
            // Internal Modules
            services.AddNeoRpcHttpClient(c => c.ApiVersion = new Version(2, 0))
               .AddNeoJsonRpcAPIs();
            services.AddHttpClient<ILocateIpService, IpStackService>();
            services.AddDbContext<NeoMonitorContext>(options =>
                {
                    options.UseMySql(Configuration.GetConnectionString("DefaultConnection"));
                }, ServiceLifetime.Scoped, ServiceLifetime.Scoped);

            services.AddTransient<SeedData>();
            services.AddSingleton<RPCNodeCaller>();
            services.AddSingleton<LocationCaller>();
            services.AddSingleton<NodeSynchronizer>();

            services.AddTransient<IRawMemPoolDataLoader, DefaultRawMemPoolDataLoader>();
            services.AddSingleton<NodeTicker>();

            services.AddSingleton<IHostedService, NotificationHostService>();
            // Analysis Modules
            services.AddAnalysisWebModule(dbContextOptionsAction: options =>
            {
                options.UseMySql(Configuration.GetConnectionString("AnalysisDevConnection"));
            });
        }

        public void Configure(IApplicationBuilder appBuilder, IWebHostEnvironment env, SeedData seeder)
        {
            if (env.IsDevelopment())
            {
                appBuilder.UseDeveloperExceptionPage();
            }
            appBuilder.UseCors("DEV");
            appBuilder.UseStaticFiles();
            appBuilder.UseRouting()
                .UseEndpoints(routeBuilder =>
                {
                    routeBuilder.MapControllers();
                    routeBuilder.MapHub<NodeHub>("/hubs/node");
                });

            seeder.Initialize();

            appBuilder.UseOpenApi();
            appBuilder.UseSwaggerUi3();
        }
    }
}