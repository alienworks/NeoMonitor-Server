using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NeoMonitor.Abstractions.Caches;
using NeoMonitor.Abstractions.ViewModels;

namespace NeoMonitor.Caches
{
    internal sealed class RawMemPoolDataMemoryCache : IRawMemPoolDataCache
    {
        private readonly ConcurrentDictionary<int, RawMemPoolModel> _rawMemPoolDatas = new ConcurrentDictionary<int, RawMemPoolModel>();

        public IReadOnlyDictionary<int, RawMemPoolModel> RawMemPoolDatas => _rawMemPoolDatas;

        public Task<bool> TryGetAsync(int key, out List<string> items)
        {
            if (_rawMemPoolDatas.TryGetValue(key, out var data))
            {
                items = data.Items;
                return Task.FromResult(true);
            }
            items = null;
            return Task.FromResult(false);
        }

        public Task<RawMemPoolModel[]> GetArrayAsync(Func<RawMemPoolModel, bool> filter = null)
        {
            RawMemPoolModel[] result = filter is null
                ? _rawMemPoolDatas.Values.ToArray()
                : _rawMemPoolDatas.Values.Where(filter).ToArray();
            return Task.FromResult(result);
        }

        public Task<RawMemPoolModel> UpdateAsync(int key, List<string> items)
        {
            if (items is null)
            {
                return Task.FromResult<RawMemPoolModel>(null);
            }
            var result = _rawMemPoolDatas.AddOrUpdate(key, new RawMemPoolModel() { NodeId = key, Items = items }, (k, v) => new RawMemPoolModel() { NodeId = key, Items = items });
            return Task.FromResult(result);
        }

        public Task<RawMemPoolSizeModel[]> GetSizeArrayAsync(Func<RawMemPoolModel, bool> filter = null)
        {
            var result = filter is null
                ? _rawMemPoolDatas.Values.Select(r => new RawMemPoolSizeModel() { Id = r.NodeId, MemoryPool = r.Items.Count }).ToArray()
                : _rawMemPoolDatas.Values.Where(filter).Select(r => new RawMemPoolSizeModel() { Id = r.NodeId, MemoryPool = r.Items.Count }).ToArray();
            return Task.FromResult(result);
        }
    }
}