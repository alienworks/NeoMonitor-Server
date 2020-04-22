using System.Collections.Generic;
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
    public class MatrixController : ControllerBase
    {
        private readonly IMapper _mapper;

        private readonly IMatrixDataCache _matrixDataCache;

        public MatrixController(
            IMapper mapper,
            IMatrixDataCache matrixDataCache
            )
        {
            _mapper = mapper;
            _matrixDataCache = matrixDataCache;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NeoMatrixItemEntity>>> Get()
        {
            var entities = await _matrixDataCache.GetRecentNeoMatrixItemsAsync();
            var viewModels = _mapper.Map<NeoMatrixItemViewModel[]>(entities);
            return Ok(viewModels);
        }
    }
}