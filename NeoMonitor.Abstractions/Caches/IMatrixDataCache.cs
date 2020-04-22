using System.Collections.Generic;
using System.Threading.Tasks;
using NeoMonitor.Abstractions.Models;

namespace NeoMonitor.Abstractions.Caches
{
    public interface IMatrixDataCache
    {
        Task<NeoMatrixItemEntity[]> GetRecentNeoMatrixItemsAsync();

        Task<bool> UpdateRecentNeoMatrixItemsAsync(IEnumerable<NeoMatrixItemEntity> items);
    }
}