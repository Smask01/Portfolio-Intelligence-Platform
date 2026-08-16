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

    public PortfolioController(IPortfolioAnalyzer portfolioAnalyzer, EtfOverlapCalculator overlapCalculator)
    {
        _portfolioAnalyzer = portfolioAnalyzer;
        _overlapCalculator = overlapCalculator;
    }
    
    [HttpPost("analyze")]
    public ActionResult<AnalyzePortfolioResponse> Analyze(
        AnalyzePortfolioRequest request)
    {
        var positions = request.Positions
            .Select(position => new PortfolioPosition
            {
                EtfTicker = position.Ticker,
                AmountInvested = position.AmountInvested
            })
            .ToList();

        // Temporary sample ETF data for now
        var etfs = new List<Etf>
        {
            new()
            {
                Ticker = "EFIV",
                Name = "EFIV",
                Holdings =
                [
                    new EtfHolding
                    {
                        Symbol = "AAPL",
                        CompanyName = "Apple Inc.",
                        Sector = "Technology",
                        Weight = 0.10m
                    }
                ]
            }
        };

        var holdingExposures = _portfolioAnalyzer.CalculateExposure(positions, etfs);

        var sectorExposures = _portfolioAnalyzer.CalculateSectorExposure(positions, etfs);

        var overlaps =
            _overlapCalculator.CalculateAllOverlaps(etfs);

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