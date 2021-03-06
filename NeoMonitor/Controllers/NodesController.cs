﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NeoMonitor.Abstractions.Caches;
using NeoMonitor.Abstractions.Models;
using NeoMonitor.Abstractions.ViewModels;

namespace NeoMonitor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NodesController : ControllerBase
    {
        private readonly IMapper _mapper;

        private readonly INodeDataCache _nodeDataCache;
        private readonly IRawMemPoolDataCache _rawMemPoolDataCache;

        public NodesController(
            IMapper mapper,
            INodeDataCache nodeDataCache,
            IRawMemPoolDataCache rawMemPoolDataCache
            )
        {
            _mapper = mapper;
            _nodeDataCache = nodeDataCache;
            _rawMemPoolDataCache = rawMemPoolDataCache;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NodeViewModel>>> Get()
        {
            NodeException[] nodeExps = await _nodeDataCache.GetNodeExceptionsAsync();
            Node[] sourceNodes = await _nodeDataCache.GetNodesAsync();
            NodeViewModel[] nodes = _mapper.Map<NodeViewModel[]>(sourceNodes);
            if (nodeExps.Length > 0)
            {
                foreach (var node in nodes)
                {
                    node.ExceptionCount = nodeExps.Count(ex => ex.Url == node.Url);
                }
            }
            else
            {
                foreach (var node in nodes)
                {
                    node.ExceptionCount = 0;
                }
            }
            return Ok(nodes);
        }

        // GET api/nodes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<NodeExceptionViewModel>>> Get(int id)
        {
            var nodeExps = await _nodeDataCache.GetNodeExceptionsAsync();
            if (nodeExps.Length < 1)
            {
                return Ok(Array.Empty<NodeExceptionViewModel>());
            }
            var nodes = await _nodeDataCache.GetNodesAsync();
            if (nodes.Length < 1)
            {
                return Ok(Array.Empty<NodeExceptionViewModel>());
            }
            var node = Array.Find(nodes, n => n.Id == id);
            if (node is null)
            {
                return Ok(Array.Empty<NodeExceptionViewModel>());
            }
            var result = _mapper.Map<IEnumerable<NodeExceptionViewModel>>(nodeExps.Where(ex => ex.Url == node.Url));
            return Ok(result);
        }

        [HttpGet("rawmempool")]
        public async Task<ActionResult<IEnumerable<RawMemPoolSizeModel>>> GetMemPoolList()
        {
            var result = await _rawMemPoolDataCache.GetSizeArrayAsync();
            return Ok(result);
        }

        [HttpGet("rawmempool/{nodeId:int}")]
        public async Task<ActionResult<IList<string>>> GetMemPoolById(int nodeId)
        {
            var ok = await _rawMemPoolDataCache.TryGetAsync(nodeId, out var items);
            if (ok)
            {
                return Ok(items);
            }
            return Ok(Array.Empty<string>());
        }
    }
}