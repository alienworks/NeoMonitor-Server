using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NeoMonitor.Analysis.Web;
using NeoMonitor.Analysis.Web.Services;
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
            //services.Configure<NetSettings>(Configuration.GetSection("NetSettings"));

            services.AddAutoMapper(AutoMapperConfig.InitMap, typeof(AutoMapperConfig));

            services.AddHttpClient<ILocateIpService, IpStackService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddDbContext<NeoMonitorContext>(options =>
                {
                    options.UseMySql(Configuration.GetConnectionString("DefaultConnection"));
                }, ServiceLifetime.Scoped);
            services.AddDbContext<AnalysisDbContext>(options =>
                {
                    options.UseMySql(Configuration.GetConnectionString("AnalysisDevConnection"));
                }, ServiceLifetime.Scoped);

            services.AddTransient<SeedData>();
            services.AddSingleton<RPCNodeCaller>();
            services.AddSingleton<LocationCaller>();
            services.AddSingleton<NodeSynchronizer>();
            services.AddSingleton<IpVisitorService>();

            services.AddTransient<IRawMemPoolDataLoader, DefaultRawMemPoolDataLoader>();
            services.AddSingleton<NodeTicker>();

            services.AddSingleton<IHostedService, NotificationHostService>();
            services.AddSingleton<IHostedService, IpVisitorHostService>();

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
            services.AddSignalR()
                .AddMessagePackProtocol();
            services.AddControllers();
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
        }
    }
}