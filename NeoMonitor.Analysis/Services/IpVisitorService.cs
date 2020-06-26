using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NeoMonitor.Analysis.Models;
using NeoMonitor.Shared.EntityFrameworkCore;

namespace NeoMonitor.Analysis.Services
{
    public sealed class IpVisitorService
    {
        private readonly DailyCache _dailyCache = new DailyCache();
        private readonly HourlyCache _hourlyCache = new HourlyCache();

        private readonly ScopedDbContextFactory _scopeFactory;

        public IpVisitorService(ScopedDbContextFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public long TotalDailyVisitTimes => _dailyCache.TotalVisitTimes;
        public long TotalDailyIpCount => _dailyCache.TotalIpCount;
        public long TotalHourlyVisitTimes => _hourlyCache.TotalVisitTimes;
        public long TotalHourlyIpCount => _hourlyCache.TotalIpCount;
        public IReadOnlyDictionary<string, int> DailyVisitTimesCache => _dailyCache.VisitTimesCache;
        public IReadOnlyDictionary<string, int> HourlyVisitTimesCache => _hourlyCache.VisitTimesCache;

        public int OnVisited(string ip)
        {
            _hourlyCache.OnVisited(ip);
            return _dailyCache.OnVisited(ip);
        }

        public int GetDailyVisitTimesByIP(string ip) => _dailyCache.GetVisitTimesByIP(ip);

        public int GetHourlyVisitTimesByIP(string ip) => _hourlyCache.GetVisitTimesByIP(ip);

        internal async Task OnDailyUpdateAsync()
        {
            _dailyCache.OnUpdate();
            await EnsureDatabaseCreatedAsync();
        }

        internal async Task OnHourlyUpdateAsync(DateTime date)
        {
            await _hourlyCache.OnUpdateAsync(_scopeFactory, date);
        }

        internal void OnMinutelyUpdate()
        {
            _dailyCache.ResetSimpleValues();
            _hourlyCache.ResetSimpleValues();
        }

        internal async Task EnsureDatabaseCreatedAsync()
        {
            using var wrapper = _scopeFactory.CreateDbContextScopedWrapper<AnalysisDbContext>();
            await wrapper.Context.Database.EnsureCreatedAsync();
        }

        private sealed class DailyCache : CacheBase
        {
            public void OnUpdate()
            {
                ReplaceCacheAndReturnOld();
            }
        }

        private sealed class HourlyCache : CacheBase
        {
            public async ValueTask OnUpdateAsync(ScopedDbContextFactory factory, DateTime date)
            {
                if (IsCacheEmpty)
                {
                    return;
                }
                var dataOfYesterday = ReplaceCacheAndReturnOld();
                using var wrapper = factory.CreateDbContextScopedWrapper<AnalysisDbContext>();
                var context = wrapper.Context;
                await context.IpVisitors.AddRangeAsync(dataOfYesterday.Select(p => new IpVisitData()
                {
                    Ip = p.Key,
                    Times = p.Value,
                    Day = date.Day,
                    Hour = date.Hour
                }));
                await context.SaveChangesAsync();
            }
        }

        private abstract class CacheBase
        {
            private ConcurrentDictionary<string, int> _visitTimesCache = new ConcurrentDictionary<string, int>();

            public ConcurrentDictionary<string, int> VisitTimesCache => _visitTimesCache;

            public bool IsCacheEmpty => _visitTimesCache.IsEmpty;

            private long _totalVisitTimes;
            private long _totalIpCount;

            public long TotalVisitTimes
            {
                get => _totalVisitTimes;
                internal set
                {
                    Interlocked.Exchange(ref _totalVisitTimes, value);
                }
            }

            public long TotalIpCount
            {
                get => _totalIpCount;
                internal set
                {
                    Interlocked.Exchange(ref _totalIpCount, value);
                }
            }

            public int GetVisitTimesByIP(string ip) => _visitTimesCache.TryGetValue(ip, out int times) ? times : 1;

            public int OnVisited(string ip) => _visitTimesCache.AddOrUpdate(ip, 1, (k, v) => Interlocked.Increment(ref v));

            public void ResetSimpleValues()
            {
                var cache = _visitTimesCache;
                if (cache.IsEmpty)
                {
                    Interlocked.Exchange(ref _totalVisitTimes, 1);
                    Interlocked.Exchange(ref _totalIpCount, 1);
                }
                else
                {
                    long vt = cache.Values.Sum(p => (long)p);
                    long ic = cache.Count;
                    Interlocked.Exchange(ref _totalVisitTimes, vt);
                    Interlocked.Exchange(ref _totalIpCount, ic);
                }
            }

            protected ConcurrentDictionary<string, int> ReplaceCacheAndReturnOld() => Interlocked.Exchange(ref _visitTimesCache, new ConcurrentDictionary<string, int>());
        }
    }
}