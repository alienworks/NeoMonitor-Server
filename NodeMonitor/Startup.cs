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
            services.AddAutoMapper(typeof(AutoMapperConfig));

            //services.Configure<NetSettings>(Configuration.GetSection("NetSettings"));

            services.AddHttpClient<ILocateIpService, IpStackService>();

            services
                .AddDbContext<NeoMonitorContext>(options =>
                {
                    options.UseMySql(Configuration.GetConnectionString("DefaultConnection"));
                }, ServiceLifetime.Scoped)
                .AddEntityFrameworkMySql();

            services.AddSingleton<RPCNodeCaller>();
            services.AddSingleton<LocationCaller>();
            services.AddSingleton<NodeSynchronizer>();

            services.AddTransient<SeedData>();
            services.AddSingleton<IHostedService, NotificationService>();

            services.AddCors();
            services.AddSignalR();

            services.AddMvc();
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
            appBuilder.UseEndpoints(routes =>
            {
                routes.MapHub<NodeHub>("/hubs/node");
            });

            seeder.Init();
        }
    }
}