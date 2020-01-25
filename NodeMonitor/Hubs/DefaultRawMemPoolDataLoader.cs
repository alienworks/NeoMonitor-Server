using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NeoMonitor.Data;
using NodeMonitor.Web.Abstraction.DataLoaders;
using NodeMonitor.Web.Abstraction.Models;

namespace NodeMonitor.Hubs
{
    public class DefaultRawMemPoolDataLoader : IRawMemPoolDataLoader
    {
        private readonly NeoMonitorContext _dbContext;

        public DefaultRawMemPoolDataLoader(NeoMonitorContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<RawMemPoolData[]> LoadAsync()
        {
            return _dbContext.Nodes
                .AsNoTracking()
                .Select(n => new RawMemPoolData() { Id = n.Id, MemoryPool = n.MemoryPool })
                .ToArrayAsync();
        }
    }
}