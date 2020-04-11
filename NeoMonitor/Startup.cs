using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NeoMonitor.Hubs;

namespace NeoMonitor
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
            services
                .AddBasicServices()
                .AddThirdPartyServices()
                .AddInternalServices(Configuration)
                .AddOtherModules(Configuration);
        }

        public void Configure(IApplicationBuilder appBuilder, IWebHostEnvironment env)
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

            appBuilder.UseOpenApi();
            appBuilder.UseSwaggerUi3();
        }
    }
}