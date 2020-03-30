using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NeoMonitor.Data.Models;
using NodeMonitor.Controllers.Base;
using NodeMonitor.Infrastructure;
using NodeMonitor.ViewModels;
using NodeMonitor.Web.Abstraction.Models;

namespace NodeMonitor.Controllers
{
    public class NodesController : BaseApiController
    {
        //private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        //private readonly RPCNodeCaller _rPCNodeCaller;

        private readonly NodeSynchronizer _nodeSynchronizer;
        private readonly List<NodeViewModel> _nodes;
        private readonly List<NodeException> _nodeExceptions;

        // private readonly IHubContext<NodeHub> _nodeHub;

        public NodesController(IMapper mapper,
            //RPCNodeCaller rPCNodeCaller,
            NodeSynchronizer nodeSynchronizer
            //IConfiguration configuration,
            // IHubContext<NodeHub> nodeHub
            )
        {
            //_configuration = configuration;
            _mapper = mapper;

            //_rPCNodeCaller = rPCNodeCaller;
            _nodeSynchronizer = nodeSynchronizer;
            _nodes = _nodeSynchronizer.GetCachedNodesAs<NodeViewModel>() ?? new List<NodeViewModel>();
            _nodeExceptions = _nodeSynchronizer.GetCachedNodeExceptionsAs<NodeException>() ?? new List<NodeException>();

            // _nodeHub = nodeHub;
        }

        [HttpGet]
        public ActionResult<IEnumerable<NodeViewModel>> Get()
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
        public ActionResult<IEnumerable<NodeExceptionViewModel>> Get(int id)
        {
            if (_nodeExceptions.Count < 1)
            {
                return Ok("[]");
            }
            var node = _nodes.Find(n => n.Id == id);
            if (node is null)
            {
                return Ok("[]");
            }
            string nodeUrl = node.Url;
            var nodeExps = _nodeExceptions.Where(ex => ex.Url == nodeUrl).Select(ex => _mapper.Map<NodeExceptionViewModel>(ex));
            return Ok(nodeExps);
        }

        [HttpGet("rawmempool/list")]
        public ActionResult<IEnumerable<RawMemPoolData>> GetMemPoolList()
        {
            var result = _nodeSynchronizer.CachedDbNodes.Select(p => new RawMemPoolData() { Id = p.Id, MemoryPool = p.MemoryPool });
            return Ok(result);
        }
    }
}