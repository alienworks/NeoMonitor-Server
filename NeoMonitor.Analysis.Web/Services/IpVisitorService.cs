using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NeoMonitor.Analysis.Web.Models;

namespace NeoMonitor.Analysis.Web.Services
{
    public sealed class IpVisitorService : BackgroundService
    {
        private ConcurrentDictionary<string, int> _visitTimesCache = new ConcurrentDictionary<string, int>();

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<IpVisitorService> _logger;

        private DateTime _lastSyncTime;

        private DateTime _lastUpdateLocalFieldsTime;

        private long _totalVisitTimes;
        private long _totalIpCount;

        public long TotalVisitTimes => _totalVisitTimes;
        public long TotalIpCount => _totalIpCount;

        public IpVisitorService(IServiceScopeFactory scopeFactory, ILogger<IpVisitorService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public int OnVisited(string ip) => _visitTimesCache.AddOrUpdate(ip, 1, (k, v) => Interlocked.Increment(ref v));

        public int GetVisitTimesByIP(string ip) => _visitTimesCache.TryGetValue(ip, out int times) ? times : 1;

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);
            _lastSyncTime = _lastUpdateLocalFieldsTime = DateTime.UtcNow;
            _logger.LogDebug("[Service]--> {0} Started.", nameof(IpVisitorService));
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            await InsertAsync(DateTime.UtcNow);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug("[Service]--> {0} Executing.", nameof(IpVisitorService));
                DateTime now = DateTime.UtcNow;
                if (now.Minute != _lastUpdateLocalFieldsTime.Minute)
                {
                    _lastUpdateLocalFieldsTime = now;
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
                if (now.Day != _lastSyncTime.Day)
                {
                    _lastSyncTime = now;
                    await InsertAsync(now.AddDays(-1));
                }
                else
                {
                    double leftMins = (now.AddDays(1).Date - now).TotalMinutes;
                    if (leftMins > 1)
                    {
                        await Task.Delay(60 * 1000);
                    }
                    else
                    {
                        await Task.Delay(1000);
                    }
                }
            }
        }

        private async ValueTask InsertAsync(DateTime date, CancellationToken stoppingToken = default)
        {
            if (_visitTimesCache.IsEmpty)
            {
                return;
            }
            var dataOfYesterday = Interlocked.Exchange(ref _visitTimesCache, new ConcurrentDictionary<string, int>());
            using var scope = _scopeFactory.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<AnalysisDbContext>();
            await context.IpVisitors.AddRangeAsync(dataOfYesterday.Select(p => new IpVisitAnaData()
            {
                Ip = p.Key,
                Times = p.Value,
                Year = date.Year,
                Month = date.Month,
                Day = date.Day
            }), stoppingToken);
            await context.SaveChangesAsync(stoppingToken);
        }
    }
}