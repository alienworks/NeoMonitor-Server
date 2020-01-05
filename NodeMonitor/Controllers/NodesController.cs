using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NeoMonitor.Data.Models;
using NodeMonitor.Controllers.Base;
using NodeMonitor.Infrastructure;
using NodeMonitor.ViewModels;

namespace NodeMonitor.Controllers
{
    public class NodesController : BaseApiController
    {
        //private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        private readonly NodeSynchronizer _nodeSynchronizer;
        private readonly List<NodeViewModel> _nodes;
        private readonly List<NodeException> _nodeExceptions;

        public NodesController(IMapper mapper,
            NodeSynchronizer nodeSynchronizer
            //IConfiguration configuration,
            //IHubContext<NodeHub> nodeHub
            )
        {
            //_configuration = configuration;
            _mapper = mapper;

            _nodeSynchronizer = nodeSynchronizer;
            _nodes = _nodeSynchronizer.GetCachedNodesAs<NodeViewModel>() ?? new List<NodeViewModel>();
            _nodeExceptions = _nodeSynchronizer.GetCachedNodeExceptionsAs<NodeException>() ?? new List<NodeException>();
        }

        [HttpGet]
        public IActionResult Get()
        {
            if (_nodeExceptions.Count > 0)
            {
                _nodes.ForEach(node =>
                {
                    node.ExceptionCount = _nodeExceptions.Count(ex => ex.Url == node.Url);
                });
            }
            else
            {
                foreach (var node in _nodes)
                {
                    node.ExceptionCount = 0;
                }
            }
            return Ok(_nodes);
        }

        // GET api/nodes/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            if (_nodeExceptions.Count < 1)
            {
                return "[]";
            }
            var node = _nodes.Find(n => n.Id == id);
            if (node is null)
            {
                return "[]";
            }
            string nodeUrl = node.Url;
            var nodeExps = _nodeExceptions.Where(ex => ex.Url == nodeUrl).Select(ex => _mapper.Map<NodeExceptionViewModel>(ex));
            string result = JsonSerializer.Serialize(nodeExps);
            return result;
        }

        // GET api/nodes/5
        //[HttpGet("{id}")]
        //public async Task<IActionResult> Get(int id)
        //{
        //    var end = DateTime.Now;
        //    var start = end.AddMonths(-3);

        //    var result = new JArray();
        //    try
        //    {
        //        string nodeUrl = _ctx.Nodes.Single(n => n.Id == id).Url;
        //        await _ctx.NodeExceptionList.Where(ex => ex.GenTime > start && ex.GenTime < end && ex.Url == nodeUrl && ex.Intervals > exceptionFilter).ForEachAsync(ex =>
        //        {
        //            var jNode = new JObject {
        //                { "id", ex.Id },
        //                { "NodeName", ex.Url },
        //                { "ExceptionHeight", ex.ExceptionHeight },
        //                { "ExceptionTime", ex.GenTime },
        //                { "Intervals", ex.Intervals }
        //            };
        //            result.Add(jNode);
        //        });
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
    }
}