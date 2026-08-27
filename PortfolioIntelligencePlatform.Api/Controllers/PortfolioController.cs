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

    public PortfolioController(IPortfolioAnalyzer portfolioAnalyzer, EtfOverlapCalculator overlapCalculator, IEtfDataProvider etfDataProvider)
    {
        _portfolioAnalyzer = portfolioAnalyzer;
        _overlapCalculator = overlapCalculator;
        _etfDataProvider = etfDataProvider;
    }
    
    [HttpPost("analyze")]
    public async Task<ActionResult<AnalyzePortfolioResponse>> Analyze(AnalyzePortfolioRequest request, CancellationToken cancellationToken)
    {
        var positions = request.Positions
            .Select(position => new PortfolioPosition
            {
                EtfTicker = position.Ticker,
                AmountInvested = position.AmountInvested
            })
            .ToList();

        var etfs = new List<Etf>();

        foreach (var position in request.Positions)
        {
            var etf = await _etfDataProvider.GetEtfAsync(position.Ticker, cancellationToken);

            if (etf is null)
            {
                return NotFound($"ETF data was not found for ticker {position.Ticker}.");
            }

            etfs.Add(etf);

            await Task.Delay(1100, cancellationToken);
        }

        var holdingExposures = _portfolioAnalyzer.CalculateExposure(positions, etfs);
        var sectorExposures = _portfolioAnalyzer.CalculateSectorExposure(positions, etfs);
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