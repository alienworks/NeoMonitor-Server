using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NeoMonitor.App.Abstractions.Models;
using NeoMonitor.App.ViewModels;
using NeoMonitor.Basics;
using NeoMonitor.Basics.Models;

namespace NeoMonitor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NodesController : ControllerBase
    {
        private readonly IMapper _mapper;

        private readonly NodeSynchronizer _nodeSynchronizer;
        private readonly List<NodeViewModel> _nodes;
        private readonly List<NodeException> _nodeExceptions;

        public NodesController(IMapper mapper,
            NodeSynchronizer nodeSynchronizer
            )
        {
            _mapper = mapper;

            _nodeSynchronizer = nodeSynchronizer;
            _nodes = _nodeSynchronizer.GetCachedNodesAs<NodeViewModel>() ?? new List<NodeViewModel>();
            _nodeExceptions = _nodeSynchronizer.GetCachedNodeExceptionsAs<NodeException>() ?? new List<NodeException>();
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
                return Ok(Array.Empty<NodeExceptionViewModel>());
            }
            var node = _nodes.Find(n => n.Id == id);
            if (node is null)
            {
                return Ok(Array.Empty<NodeExceptionViewModel>());
            }
            string nodeUrl = node.Url;
            var nodeExps = _nodeExceptions.Where(ex => ex.Url == nodeUrl).Select(ex => _mapper.Map<NodeExceptionViewModel>(ex));
            return Ok(nodeExps);
        }

        [HttpGet("rawmempool/list")]
        public ActionResult<IEnumerable<RawMemPoolSizeModel>> GetMemPoolList()
        {
            var result = _nodeSynchronizer.CachedDbNodes.Select(p => new RawMemPoolSizeModel() { Id = p.Id, MemoryPool = p.MemoryPool });
            return Ok(result);
        }
    }
}