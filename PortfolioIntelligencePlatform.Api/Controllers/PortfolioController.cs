using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PortfolioIntelligencePlatform.Api.Dtos;
using PortfolioIntelligencePlatform.Application;
using PortfolioIntelligencePlatform.Domain;
using PortfolioIntelligencePlatform.Infrastructure;

namespace PortfolioIntelligencePlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PortfolioController : ControllerBase
{
    private readonly IPortfolioAnalyzer _portfolioAnalyzer;
    private readonly EtfOverlapCalculator _overlapCalculator;
    private readonly IEtfDataProvider _etfDataProvider;
    private readonly IStockDataProvider _stockDataProvider;

    public PortfolioController(IPortfolioAnalyzer portfolioAnalyzer, EtfOverlapCalculator overlapCalculator, IEtfDataProvider etfDataProvider, IStockDataProvider stockDataProvider)
    {
        _portfolioAnalyzer = portfolioAnalyzer;
        _overlapCalculator = overlapCalculator;
        _etfDataProvider = etfDataProvider;
        _stockDataProvider = stockDataProvider;
    }
    
    [HttpPost("analyze")]
    public async Task<ActionResult<AnalyzePortfolioResponse>> Analyze(AnalyzePortfolioRequest request, CancellationToken cancellationToken)
    {
        var positions = request.Positions
            .Select(position => new PortfolioPosition
            {
                Symbol = position.Ticker,
                AmountInvested = position.AmountInvested
            })
            .ToList();

        var etfs = new List<Etf>();
        var stocks = new List<Stock>();
        
        foreach (var position in request.Positions)
        {
            switch (position.AssetType)
            {
                case AssetType.Etf:
                {
                    var etf = await _etfDataProvider.GetEtfAsync(position.Ticker, cancellationToken);

                    if (etf is null) return NotFound($"No ETF data was found for {position.Ticker}.");

                    etfs.Add(etf);
                    break;
                }

                case AssetType.Stock:
                {
                    var stock = await _stockDataProvider.GetStockAsync(position.Ticker, cancellationToken);

                    if (stock is null) return NotFound($"No stock data was found for {position.Ticker}.");

                    stocks.Add(stock);
                    break;
                }

                default: return BadRequest($"Unsupported asset type: {position.AssetType}.");
            }
        }

        var holdingExposures = _portfolioAnalyzer.CalculateExposure(positions, etfs, stocks);
        var sectorExposures = _portfolioAnalyzer.CalculateSectorExposure(positions, etfs, stocks);
        var overlaps = _overlapCalculator.CalculateAllOverlaps(etfs);

        var response = new AnalyzePortfolioResponse
        {
            HoldingExposures = holdingExposures.Select(x =>
                new HoldingExposureResponse
                {
                    Symbol = x.Symbol,
                    CompanyName = x.CompanyName,
                    AmountExposed = x.AmountExposed,
                    PortfolioPercentage = x.PortfolioPercentage
                }).ToList(),

            SectorExposures = sectorExposures.Select(x =>
                new SectorExposureResponse
                {
                    Sector = x.Sector,
                    AmountExposed = x.AmountExposed,
                    PortfolioPercentage = x.PortfolioPercentage
                }).ToList(),

            Overlaps = overlaps.Select(x =>
                new EtfOverlapResponse
                {
                    FirstTicker = x.FirstTicker,
                    SecondTicker = x.SecondTicker,
                    Overlap = x.Overlap
                }).ToList()
        };

        return Ok(response);
    }
    
    [HttpGet("test-alpha")]
    public async Task<IActionResult> TestAlpha([FromServices] IHttpClientFactory httpClientFactory, [FromServices] IOptions<AlphaVantageOptions> options)
    {
        var config = options.Value;
        var client = httpClientFactory.CreateClient();

        var url =
            $"{config.BaseUrl}/query" +
            $"?function=ETF_PROFILE" +
            $"&symbol=VOO" +
            $"&apikey={config.ApiKey}";

        var response = await client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();

        return Content(content, "application/json");
    }
}