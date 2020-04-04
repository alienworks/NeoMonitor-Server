using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NeoMonitor.Analysis.Web.Services;

namespace NodeMonitor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class AnalysisController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly IpVisitorService _ipVisitorService;

        public AnalysisController(IHttpContextAccessor httpContextAccessor, IpVisitorService ipVisitorService)
        {
            _httpContextAccessor = httpContextAccessor;
            _ipVisitorService = ipVisitorService;
        }

        [HttpGet("register")]
        public ActionResult<int> Register()
        {
            string myIp = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            if (string.IsNullOrEmpty(myIp))
            {
                return 1;
            }
            return _ipVisitorService.OnVisited(myIp);
        }

        #region Daily

        [HttpGet("currentDailyVisitTimes")]
        public ActionResult<int> GetCurrentDailyVisitTimes()
        {
            string myIp = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            if (string.IsNullOrEmpty(myIp))
            {
                return 0;
            }
            return _ipVisitorService.GetDailyVisitTimesByIP(myIp);
        }

        [HttpGet("totalDailyVisitTimes")]
        public ActionResult<long> GetTotalDailyVisitTimes()
        {
            return _ipVisitorService.TotalDailyVisitTimes;
        }

        [HttpGet("totalDailyIpCount")]
        public ActionResult<long> GetTotalDailyIpCount()
        {
            return _ipVisitorService.TotalDailyIpCount;
        }

        #endregion Daily

        #region Hourly

        [HttpGet("currentHourlyVisitTimes")]
        public ActionResult<int> GetCurrentHourlyVisitTimes()
        {
            string myIp = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            if (string.IsNullOrEmpty(myIp))
            {
                return 0;
            }
            return _ipVisitorService.GetHourlyVisitTimesByIP(myIp);
        }

        [HttpGet("totalHourlyVisitTimes")]
        public ActionResult<long> GetTotalHourlyVisitTimes()
        {
            return _ipVisitorService.TotalHourlyVisitTimes;
        }

        [HttpGet("totalHourlyIpCount")]
        public ActionResult<long> GetTotalHourlyIpCount()
        {
            return _ipVisitorService.TotalHourlyIpCount;
        }

        #endregion Hourly
    }
}