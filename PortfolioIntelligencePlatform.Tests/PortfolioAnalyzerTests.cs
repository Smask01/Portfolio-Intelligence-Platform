using PortfolioIntelligencePlatform.Application;
using PortfolioIntelligencePlatform.Domain;

namespace PortfolioIntelligencePlatform.Tests;

[TestFixture]
public class PortfolioAnalyzerTests
{
    [Test]
    public void CalculateExposure_ReturnsCorrectExposure_ForEfivHolding()
    {
        // Arrange
        var efiv = new Etf
        {
            Ticker = "EFIV",
            Name = "SPDR S&P 500 ESG ETF",
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
        };

        var positions = new List<PortfolioPosition>
        {
            new()
            {
                EtfTicker = "EFIV",
                AmountInvested = 1_000m
            }
        };

        var analyzer = new PortfolioAnalyzer();

        // Act
        var result = analyzer.CalculateExposure(positions, [efiv]);

        // Assert
        var appleExposure = result.Single();

        Assert.That(appleExposure.Symbol, Is.EqualTo("AAPL"));
        Assert.That(appleExposure.AmountExposed, Is.EqualTo(100m));
        Assert.That(appleExposure.PortfolioPercentage, Is.EqualTo(10m));
    }
}