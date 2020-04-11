using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoMonitor.Abstractions.ViewModels;

namespace NeoMonitor.Abstractions.Caches
{
    public interface IRawMemPoolDataCache
    {
        Task<bool> ContainsAsync(int key);

        Task<bool> TryGetAsync(int key, out List<string> items);

        Task<RawMemPoolModel[]> GetArrayAsync(Func<RawMemPoolModel, bool> filter = null);

        Task<RawMemPoolModel> UpdateAsync(int key, List<string> items);

        Task<RawMemPoolSizeModel[]> GetSizeArrayAsync(Func<RawMemPoolModel, bool> filter = null);
    }
}