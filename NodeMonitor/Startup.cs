using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
			AutoMapperConfig.Init();

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
			services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, NotificationService>();

			services.AddCors();
			services.AddSignalR();

			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env, SeedData seeder)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			app.UseCors(builder =>
			{
				builder
					.WithOrigins("http://localhost:8111", "http://localhost:4200", "http://localhost:9876")
					.AllowAnyMethod()
					.AllowAnyHeader()
					.AllowCredentials();
			});
			app.UseStaticFiles();
			app.UseSignalR(routes =>
			{
				routes.MapHub<NodeHub>("/hubs/node");
			});

			seeder.Init();

			app.UseMvc();
		}
	}
}