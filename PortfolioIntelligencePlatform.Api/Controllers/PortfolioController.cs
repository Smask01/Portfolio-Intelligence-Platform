using Microsoft.AspNetCore.Mvc;
using PortfolioIntelligencePlatform.Api.Dtos;
using PortfolioIntelligencePlatform.Application;
using PortfolioIntelligencePlatform.Domain;

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
            var etf = await _etfDataProvider.GetEtfAsync(position.Ticker, cancellationToken);
            
            if (etf is not null)
            {
                etfs.Add(etf);
                continue;
            }

            var stock = await _stockDataProvider.GetStockAsync(position.Ticker, cancellationToken);

            if (stock is not null)
            {
                stocks.Add(stock);
                continue;
            }

            return NotFound($"No ETF or stock data was found for symbol {position.Ticker}.");
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
}