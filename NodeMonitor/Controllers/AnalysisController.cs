using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NeoMonitor.Analysis.Web.Services;

namespace NodeMonitor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public sealed class AnalysisController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly IpVisitorService _ipVisitorService;

        public AnalysisController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
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

        [HttpGet("currentIpVisitTimesToday")]
        public ActionResult<int> GetCurrentIpVisitTimesToday()
        {
            string myIp = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            if (string.IsNullOrEmpty(myIp))
            {
                return 0;
            }
            return _ipVisitorService.GetVisitTimesByIP(myIp);
        }

        [HttpGet("totalVisitTimes")]
        public ActionResult<long> GetTotalVisitTimes()
        {
            return _ipVisitorService.TotalVisitTimes;
        }

        [HttpGet("totalIpCount")]
        public ActionResult<long> GetTotalIpCount()
        {
            return _ipVisitorService.TotalIpCount;
        }
    }
}