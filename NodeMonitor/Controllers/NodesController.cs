using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using NeoMonitor.Data.Models;
using Newtonsoft.Json.Linq;
using NodeMonitor.Controllers.Base;
using NodeMonitor.Hubs;
using NodeMonitor.Infrastructure;
using NodeMonitor.ViewModels;

namespace NodeMonitor.Controllers
{
	public class NodesController : BaseApiController
	{
		private readonly IConfiguration _configuration;
		private readonly NodeSynchronizer _nodeSynchronizer;

		private readonly List<NodeViewModel> _nodes;
		private readonly List<NodeException> _nodeExceptions;

		public NodesController(IConfiguration configuration,
			IHubContext<NodeHub> nodeHub,
			NodeSynchronizer nodeSynchronizer)
		{
			_configuration = configuration;
			_nodeSynchronizer = nodeSynchronizer;

			_nodes = _nodeSynchronizer.GetCachedNodesAs<NodeViewModel>();
			_nodeExceptions = _nodeSynchronizer.GetCachedNodeExceptionsAs<NodeException>();
		}

		[HttpGet]
		public IActionResult Get()
		{
			try
			{
				_nodes.ForEach(node =>
				{
					node.ExceptionCount = _nodeExceptions.Count(ex => ex.Url == node.Url);
				});
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
			return Ok(_nodes);
		}

		// GET api/nodes/5
		[HttpGet("{id}")]
		public ActionResult<JArray> Get(int id)
		{
			if (_nodeExceptions.Count < 1)
			{
				return new JArray();
			}
			var node = _nodes.Find(n => n.Id == id);
			if (node is null)
			{
				return new JArray();
			}
			string nodeUrl = node.Url;
			var nodeItems = _nodeExceptions.Where(ex => ex.Url == nodeUrl).Select(ex => new JObject
			{
				{ "id", ex.Id },
				{ "nodeName", ex.Url },
				{ "exceptionHeight", ex.ExceptionHeight },
				{ "exceptionTime", ex.GenTime },
				{ "intervals", ex.Intervals }
			});
			var result = new JArray();
			foreach (var nodeItem in nodeItems)
			{
				result.Add(nodeItem);
			}
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