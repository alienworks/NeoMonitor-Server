using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NeoMonitor.App.Abstractions.Models;
using NeoMonitor.App.Abstractions.Services.Data;
using NeoMonitor.DbContexts;

namespace NeoMonitor.Hubs
{
    public class DefaultRawMemPoolDataLoader : IRawMemPoolDataLoader
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DefaultRawMemPoolDataLoader(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<RawMemPoolSizeModel[]> LoadAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            using var dbContext = scope.ServiceProvider.GetRequiredService<NeoMonitorContext>();
            return await dbContext.Nodes
                .AsNoTracking()
                .Select(n => new RawMemPoolSizeModel() { Id = n.Id, MemoryPool = n.MemoryPool })
                .ToArrayAsync();
        }
    }
}