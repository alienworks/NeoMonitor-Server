using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NeoMonitor.Analysis.Web.Services
{
    public sealed class IpVisitorHostService : BackgroundService
    {
        private readonly IpVisitorService _service;

        private readonly ILogger<IpVisitorHostService> _logger;

        private DateTime _lastCheckTime;

        public IpVisitorHostService(IpVisitorService service, ILogger<IpVisitorHostService> logger)
        {
            _service = service;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _lastCheckTime = DateTime.UtcNow;
            await _service.EnsureDatabaseCreatedAsync();
            _logger.LogDebug("[Service]--> {0} Executing.", nameof(IpVisitorHostService));
            while (!stoppingToken.IsCancellationRequested)
            {
                DateTime utcNow = DateTime.UtcNow;
                if (utcNow.Minute != _lastCheckTime.Minute)
                {
                    _service.OnMinutelyUpdate();
                    if (utcNow.Hour != _lastCheckTime.Hour)
                    {
                        if (utcNow.Day != _lastCheckTime.Day)
                        {
                            await _service.OnDailyUpdateAsync();
                        }
                        await _service.OnHourlyUpdateAsync(_lastCheckTime);
                    }
                    _lastCheckTime = utcNow;
                }
                await Task.Delay(10 * 1000);
            }
            _logger.LogDebug("[Service]--> {0} Executed.", nameof(IpVisitorHostService));
        }
    }
}