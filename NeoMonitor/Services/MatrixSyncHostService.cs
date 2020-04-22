using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NeoMonitor.Abstractions.Caches;
using NeoMonitor.DbContexts;
using NeoMonitor.Shared.EntityFrameworkCore;

namespace NeoMonitor.Services
{
    public sealed class MatrixSyncHostService : BackgroundService
    {
        private readonly ILogger<MatrixSyncHostService> _logger;

        private readonly IMatrixDataCache _matrixDataCache;
        private readonly ScopedDbContextFactory _dbContextFactory;

        public MatrixSyncHostService(
            ILogger<MatrixSyncHostService> logger,
            IMatrixDataCache matrixDataCache,
            ScopedDbContextFactory dbContextFactory
            )
        {
            _logger = logger;
            _matrixDataCache = matrixDataCache;
            _dbContextFactory = dbContextFactory;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken cancelToken)
        {
            _logger.LogDebug("[Service]--> {0} Executing.", nameof(MatrixSyncHostService));
            using var dbCtxWrapper = _dbContextFactory.CreateDbContextScopedWrapper<NeoMatrixDbContext>();
            var dbCtx = dbCtxWrapper.Context;
            while (!cancelToken.IsCancellationRequested)
            {
                long recentGroupId = await dbCtx.MatrixItems
                    .AsNoTracking()
                    .OrderByDescending(p => p.Id)
                    .Select(p => p.GroupId)
                    .FirstOrDefaultAsync(cancelToken);
                if (recentGroupId > 0)
                {
                    var items = await dbCtx.MatrixItems
                        .AsNoTracking()
                        .Where(p => p.GroupId == recentGroupId)
                        .ToArrayAsync(cancelToken);
                    await _matrixDataCache.UpdateRecentNeoMatrixItemsAsync(items);
                }
                await Task.Delay(5 * 60 * 1000, cancelToken);
            }
        }
    }
}