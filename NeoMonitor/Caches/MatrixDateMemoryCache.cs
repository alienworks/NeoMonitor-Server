using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NeoMonitor.Abstractions.Caches;
using NeoMonitor.Abstractions.Models;

namespace NeoMonitor.Caches
{
    public sealed class MatrixDateMemoryCache : IMatrixDataCache
    {
        public NeoMatrixItemEntity[] _items = Array.Empty<NeoMatrixItemEntity>();

        public Task<NeoMatrixItemEntity[]> GetRecentNeoMatrixItemsAsync()
        {
            return Task.FromResult(_items);
        }

        public Task<bool> UpdateRecentNeoMatrixItemsAsync(IEnumerable<NeoMatrixItemEntity> items)
        {
            if (items is null)
            {
                return Task.FromResult(false);
            }
            Interlocked.Exchange(ref _items, items.ToArray());
            return Task.FromResult(true);
        }
    }
}