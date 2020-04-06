using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NeoMonitor.App.Abstractions.Services;
using NeoMonitor.DbContexts;

namespace NeoMonitor
{
    public sealed class NodeSeedsStartupFilter : IStartupFilter
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly INodeSeedsLoaderFactory _dataLoader;

        public NodeSeedsStartupFilter(IServiceScopeFactory scopeFactory, INodeSeedsLoaderFactory dataLoader)
        {
            _scopeFactory = scopeFactory;
            _dataLoader = dataLoader;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            using var scope = _scopeFactory.CreateScope();
            using var dbContext = scope.ServiceProvider.GetRequiredService<NeoMonitorContext>();
            if (!dbContext.Nodes.Any())
            {
                var loader = _dataLoader.Build();
                var seeds = loader.Load();
                dbContext.AddRange(seeds);
                dbContext.SaveChanges();
            }
            return next;
        }
    }
}