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

            services
                .AddDbContext<NeoMonitorContext>(options =>
                {
                    options.UseMySql(Configuration.GetConnectionString("DefaultConnection"));
                }, ServiceLifetime.Scoped)
                .AddEntityFrameworkMySql();

            services.AddTransient<SeedData>();
            services.AddSingleton<RPCNodeCaller>();
            services.AddSingleton<LocationCaller>();
            services.AddSingleton<NodeSynchronizer>();

            services.AddSingleton<IHostedService, NotificationService>();

            services.AddCors();
            services.AddSignalR();

            services.AddMvc(p => p.EnableEndpointRouting = false);
        }

        public void Configure(IApplicationBuilder appBuilder, IWebHostEnvironment env, SeedData seeder)
        {
            if (env.IsDevelopment())
            {
                appBuilder.UseDeveloperExceptionPage();
            }
            appBuilder.UseCors(builder =>
            {
                builder
                    .WithOrigins("http://localhost:8111", "http://localhost:4200", "http://localhost:9876")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
            appBuilder.UseStaticFiles();
            appBuilder.UseRouting()
                .UseEndpoints(routeBuilder =>
                {
                    routeBuilder.MapHub<NodeHub>("/hubs/node");
                });

            seeder.Initialize();

            appBuilder.UseMvc();
        }
    }
}