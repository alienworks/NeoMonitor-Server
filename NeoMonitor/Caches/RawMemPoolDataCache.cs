using System.Collections.Concurrent;
using System.Collections.Generic;
using NeoMonitor.App.Abstractions.ViewModels;

namespace NeoMonitor.Caches
{
    public sealed class RawMemPoolDataCache
    {
        private readonly ConcurrentDictionary<long, RawMemPoolModel> _rawMemPoolDatas = new ConcurrentDictionary<long, RawMemPoolModel>();

        public IReadOnlyDictionary<long, RawMemPoolModel> RawMemPoolDatas => _rawMemPoolDatas;

        public RawMemPoolModel UpdateRawMemPoolData(long key, List<string> items)
        {
            if (items is null)
            {
                return null;
            }
            return _rawMemPoolDatas.AddOrUpdate(key, new RawMemPoolModel() { NodeId = key, Items = items }, (k, v) => new RawMemPoolModel() { NodeId = key, Items = items });
        }

        public bool TryGetRawMemPoolItems(long key, out List<string> items)
        {
            if (_rawMemPoolDatas.TryGetValue(key, out var data))
            {
                items = data.Items;
                return true;
            }
            items = null;
            return false;
        }
    }
}