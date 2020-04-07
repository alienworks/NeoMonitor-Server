using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using NeoMonitor.App.Abstractions.Services;
using NeoMonitor.DbContexts;

namespace NeoMonitor
{
    public sealed class NodeSeedsStartupFilter : IStartupFilter
    {
        private readonly ScopedDbContextFactory _dbcontextFactory;
        private readonly INodeSeedsLoaderFactory _dataLoader;

        public NodeSeedsStartupFilter(ScopedDbContextFactory dbcontextFactory, INodeSeedsLoaderFactory dataLoader)
        {
            _dbcontextFactory = dbcontextFactory;
            _dataLoader = dataLoader;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            using var wrapper = _dbcontextFactory.CreateDbContextScopedWrapper<NeoMonitorContext>();
            var dbContext = wrapper.Context;
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