using Microsoft.AspNetCore.Mvc;

namespace NodeMonitor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalysisController : ControllerBase
    {
        [HttpGet("totalVisitTimes")]
        public ActionResult<int> GetTotalVisitTimes()
        {
            return 1;
        }

        [HttpGet("totalIpCount")]
        public ActionResult<int> GetTotalIpCount()
        {
            return 1;
        }
    }
}