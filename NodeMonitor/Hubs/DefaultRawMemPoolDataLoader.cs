using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NeoMonitor.Data;
using NodeMonitor.Web.Abstraction.DataLoaders;
using NodeMonitor.Web.Abstraction.Models;

namespace NodeMonitor.Hubs
{
    public class DefaultRawMemPoolDataLoader : IRawMemPoolDataLoader
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DefaultRawMemPoolDataLoader(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<RawMemPoolData[]> LoadAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            using var dbContext = scope.ServiceProvider.GetRequiredService<NeoMonitorContext>();
            return await dbContext.Nodes
                .AsNoTracking()
                .Select(n => new RawMemPoolData() { Id = n.Id, MemoryPool = n.MemoryPool })
                .ToArrayAsync();
        }
    }
}