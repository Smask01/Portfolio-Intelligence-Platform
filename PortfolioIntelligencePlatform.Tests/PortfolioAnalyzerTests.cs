using PortfolioIntelligencePlatform.Application;
using PortfolioIntelligencePlatform.Domain;

namespace PortfolioIntelligencePlatform.Tests;

[TestFixture]
public class PortfolioAnalyzerTests
{
    [Test]
    public void CalculateExposure_ReturnsCorrectExposure_ForEfivHolding()
    {
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
                Symbol = "EFIV",
                AmountInvested = 1_000m
            }
        };

        var analyzer = new PortfolioAnalyzer();
        
        var result = analyzer.CalculateExposure(positions, [efiv], []);
        
        var appleExposure = result.Single();

        Assert.That(appleExposure.Symbol, Is.EqualTo("AAPL"));
        Assert.That(appleExposure.AmountExposed, Is.EqualTo(100m));
        Assert.That(appleExposure.PortfolioPercentage, Is.EqualTo(10m));
    }
    
    [Test]
    public void CalculateExposure_CombinesSameHoldingAcrossMultipleEtfs()
    {
        var efiv = new Etf
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
        };

        var secondEtf = new Etf
        {
            Ticker = "TEST",
            Name = "Test ETF",
            Holdings =
            [
                new EtfHolding
                {
                    Symbol = "AAPL",
                    CompanyName = "Apple Inc.",
                    Sector = "Technology",
                    Weight = 0.20m
                }
            ]
        };

        var positions = new List<PortfolioPosition>
        {
            new() { Symbol = "EFIV", AmountInvested = 1_000m },
            new() { Symbol = "TEST", AmountInvested = 500m }
        };

        var analyzer = new PortfolioAnalyzer();

        var result = analyzer.CalculateExposure(positions, [efiv, secondEtf], []);

        var appleExposure = result.Single();

        Assert.That(appleExposure.AmountExposed, Is.EqualTo(200m));
    }
    
    [Test]
    public void CalculateExposure_ReturnsCorrectExposure_ForMultipleHoldings()
    {
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
                },
                new EtfHolding
                {
                    Symbol = "MSFT",
                    CompanyName = "Microsoft Corp.",
                    Sector = "Technology",
                    Weight = 0.05m
                }
            ]
        };

        var positions = new List<PortfolioPosition>
        {
            new()
            {
                Symbol = "EFIV",
                AmountInvested = 1_000m
            }
        };

        var analyzer = new PortfolioAnalyzer();

        var result = analyzer.CalculateExposure(positions, [efiv], []);

        var apple = result.Single(x => x.Symbol == "AAPL");
        var microsoft = result.Single(x => x.Symbol == "MSFT");

        Assert.That(apple.AmountExposed, Is.EqualTo(100m));
        Assert.That(apple.PortfolioPercentage, Is.EqualTo(10m));

        Assert.That(microsoft.AmountExposed, Is.EqualTo(50m));
        Assert.That(microsoft.PortfolioPercentage, Is.EqualTo(5m));
    }
    
    [Test]
    public void CalculateExposure_ThrowsException_WhenSymbolIsNotFound()
    {
        var positions = new List<PortfolioPosition>
        {
            new()
            {
                Symbol = "UNKNOWN",
                AmountInvested = 1_000m
            }
        };

        var analyzer = new PortfolioAnalyzer();

        Assert.Throws<InvalidOperationException>(() => analyzer.CalculateExposure(positions, [], []));
    }
    
    [Test]
    public void CalculateExposure_ThrowsException_WhenPortfolioIsEmpty()
    {
        var analyzer = new PortfolioAnalyzer();

        Assert.Throws<ArgumentException>(() => analyzer.CalculateExposure([],[], []));
    }
    
    [TestCase(0)]
    [TestCase(-100)]
    public void CalculateExposure_ThrowsException_WhenAmountInvestedIsNotPositive(decimal amountInvested)
    {
        var efiv = new Etf
        {
            Ticker = "EFIV",
            Name = "SPDR S&P 500 ESG ETF",
            Holdings = []
        };

        var positions = new List<PortfolioPosition>
        {
            new()
            {
                Symbol = "EFIV",
                AmountInvested = amountInvested
            }
        };

        var analyzer = new PortfolioAnalyzer();

        Assert.Throws<ArgumentException>(() => analyzer.CalculateExposure(positions, [efiv], []));
    }

    [Test]
    public void CalculateSectorExposure_ReturnsCorrectSectorTotals()
    {
        var efiv = new Etf
        {
            Ticker = "EFIV",
            Name = "EFIV",
            SectorAllocations =
            [
                new SectorAllocation
                {
                    Sector = "Technology",
                    Weight = 0.15m
                },
                new SectorAllocation
                {
                    Sector = "Financials",
                    Weight = 0.08m
                }
            ]
        };

        var positions = new List<PortfolioPosition>
        {
            new()
            {
                Symbol = "EFIV",
                AmountInvested = 1_000m
            }
        };

        var analyzer = new PortfolioAnalyzer();

        var result = analyzer.CalculateSectorExposure(positions, [efiv], []);

        var technology = result.Single(x => x.Sector == "Technology");
        var financials = result.Single(x => x.Sector == "Financials");

        Assert.That(technology.AmountExposed, Is.EqualTo(150m));
        Assert.That(technology.PortfolioPercentage, Is.EqualTo(15m));

        Assert.That(financials.AmountExposed, Is.EqualTo(80m));
        Assert.That(financials.PortfolioPercentage, Is.EqualTo(8m));
    }
    
    [Test]
    public void CalculateExposure_CombinesDirectStockWithEtfExposure()
    {
        var voo = new Etf
        {
            Ticker = "VOO",
            Name = "Vanguard S&P 500 ETF",
            Holdings =
            [
                new EtfHolding
                {
                    Symbol = "NVDA",
                    CompanyName = "NVIDIA Corp.",
                    Sector = "Technology",
                    Weight = 0.10m
                }
            ]
        };

        var nvda = new Stock
        {
            Symbol = "NVDA",
            Name = "NVIDIA Corp.",
            Sector = "Technology"
        };

        var positions = new List<PortfolioPosition>
        {
            new() { Symbol = "VOO", AmountInvested = 1_000m },
            new() { Symbol = "NVDA", AmountInvested = 500m }
        };

        var analyzer = new PortfolioAnalyzer();
        var result = analyzer.CalculateExposure(positions, [voo], [nvda]);
        var nvdaExposure = result.Single(x => x.Symbol == "NVDA");

        Assert.That(nvdaExposure.AmountExposed, Is.EqualTo(600m));
        Assert.That(nvdaExposure.PortfolioPercentage, Is.EqualTo(40m));
    }
    
    [Test]
    public void CalculateSectorExposure_IncludesDirectStockSector()
    {
        var nvda = new Stock
        {
            Symbol = "NVDA",
            Name = "NVIDIA Corp.",
            Sector = "Technology"
        };

        var positions = new List<PortfolioPosition>
        {
            new() { Symbol = "NVDA", AmountInvested = 500m }
        };

        var analyzer = new PortfolioAnalyzer();

        var result = analyzer.CalculateSectorExposure(positions, [], [nvda]);

        var technology = result.Single();

        Assert.That(technology.Sector, Is.EqualTo("Technology"));
        Assert.That(technology.AmountExposed, Is.EqualTo(500m));
        Assert.That(technology.PortfolioPercentage, Is.EqualTo(100m));
    }
}