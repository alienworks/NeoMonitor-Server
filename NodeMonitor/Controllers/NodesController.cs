using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using NeoMonitor.Data.Models;
using Newtonsoft.Json.Linq;
using NodeMonitor.Controllers.Base;
using NodeMonitor.Hubs;
using NodeMonitor.Infrastructure;
using NodeMonitor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NodeMonitor.Controllers
{
    public class NodesController : BaseApiController
    {
        private readonly IConfiguration _configuration;
        private readonly NodeSynchronizer _nodeSynchronizer;

        private List<NodeViewModel> nodes;
        private List<NodeException> exceptions;

        public NodesController(IConfiguration configuration, 
            IHubContext<NodeHub> nodeHub,
            NodeSynchronizer nodeSynchronizer)
        {
            _configuration = configuration;
            _nodeSynchronizer = nodeSynchronizer;

            nodes = _nodeSynchronizer.GetCachedNodesAs<NodeViewModel>().ToList();
            exceptions = _nodeSynchronizer.GetCachedNodeExceptionsAs<NodeException>().ToList();
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                nodes.ForEach(node =>
                {
                    node.ExceptionCount = exceptions.Where(ex => ex.Url == node.Url).ToList().Count();
                });
                return Ok(nodes);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET api/nodes/5        
        [HttpGet("{id}")]
        public async Task<JArray> Get(int id)
        {
            var result = new JArray();
            string nodeUrl = nodes.Single(n => n.Id == id).Url;
            exceptions.Where(ex => ex.Url == nodeUrl).ToList().ForEach(ex =>
            {
                var jNode = new JObject {
                        { "id", ex.Id },
                        { "nodeName", ex.Url },
                        { "exceptionHeight", ex.ExceptionHeight },
                        { "exceptionTime", ex.GenTime },
                        { "intervals", ex.Intervals }
                };
                result.Add(jNode);
            });
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