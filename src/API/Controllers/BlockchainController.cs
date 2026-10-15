using ICMarkets.Application.Abstractions;
using ICMarkets.Application.Queries.Request;
using ICMarkets.Application.Queries.Response;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ICMarkets.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlockchainController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IReferenceDataCache _referenceData;

        public BlockchainController(
            IMediator mediator,
            IReferenceDataCache referenceData)
        {
            _mediator = mediator;
            _referenceData = referenceData;
        }

        [HttpGet("{coin}/{chain}")]
        public async Task<ActionResult> GetCoinChainHistory(
            [FromRoute] string coin, 
            [FromRoute] string chain, 
            [FromQuery] DateTime? latest,
            [FromQuery] int? pageSize,
            CancellationToken ct) =>
            Ok(await _mediator.Send(new GetCoinChainHistoryQuery(coin, chain, latest, pageSize), ct));
        
        [HttpGet("coins")]
        public ActionResult<IEnumerable<NamedEntity<int>>> GetAwaibleCoins() =>
            Ok(_referenceData.Coins.Select(c => new NamedEntity<int>(c.Id, c.Name)));

        [HttpGet("chain")]
        public ActionResult<IEnumerable<NamedEntity<int>>> GetAwaibleChains() =>
            Ok(_referenceData.Chains.Select(c => new NamedEntity<int>(c.Id, c.Name)));

        [HttpGet("pairs")]
        public ActionResult<IEnumerable<CoinChainResponse>> GetAvailablePairs() =>
            Ok(_referenceData.CoinChains.Select(c => c.ToResponse()));
    }
}
