using PortfolioIntelligencePlatform.Application;
using PortfolioIntelligencePlatform.Domain;

namespace PortfolioIntelligencePlatform.Tests;

[TestFixture]
public class EtfOverlapCalculatorTests
{
    [Test]
    public void CalculateOverlap_ReturnsCorrectWeightedOverlap()
    {
        var firstEtf = new Etf
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
                },
                new EtfHolding
                {
                    Symbol = "MSFT",
                    CompanyName = "Microsoft Corp.",
                    Sector = "Technology",
                    Weight = 0.06m
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
                    Weight = 0.08m
                },
                new EtfHolding
                {
                    Symbol = "MSFT",
                    CompanyName = "Microsoft Corp.",
                    Sector = "Technology",
                    Weight = 0.04m
                }
            ]
        };

        var calculator = new EtfOverlapCalculator();

        var result = calculator.CalculateOverlap(firstEtf, secondEtf);

        Assert.That(result, Is.EqualTo(0.12m));
    }
    
    [Test]
    public void CalculateAllOverlaps_ReturnsAllUniquePairs()
    {
        var etf1 = new Etf
        {
            Ticker = "EFIV",
            Name = "EFIV",
            Holdings = []
        };

        var etf2 = new Etf
        {
            Ticker = "VOO",
            Name = "VOO",
            Holdings = []
        };

        var etf3 = new Etf
        {
            Ticker = "QQQ",
            Name = "QQQ",
            Holdings = []
        };

        var calculator = new EtfOverlapCalculator();
        var result = calculator.CalculateAllOverlaps([etf1, etf2, etf3]);

        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That((bool)result.Any(x => x.FirstTicker == "EFIV" && x.SecondTicker == "VOO"));
        Assert.That((bool)result.Any(x => x.FirstTicker == "EFIV" && x.SecondTicker == "QQQ"));
        Assert.That((bool)result.Any(x => x.FirstTicker == "VOO" && x.SecondTicker == "QQQ"));
    }
    
    [Test]
    public void CalculateAllOverlaps_ReturnsCorrectOverlapValues()
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

        var voo = new Etf
        {
            Ticker = "VOO",
            Name = "VOO",
            Holdings =
            [
                new EtfHolding
                {
                    Symbol = "AAPL",
                    CompanyName = "Apple Inc.",
                    Sector = "Technology",
                    Weight = 0.08m
                }
            ]
        };

        var qqq = new Etf
        {
            Ticker = "QQQ",
            Name = "QQQ",
            Holdings =
            [
                new EtfHolding
                {
                    Symbol = "AAPL",
                    CompanyName = "Apple Inc.",
                    Sector = "Technology",
                    Weight = 0.05m
                }
            ]
        };

        var calculator = new EtfOverlapCalculator();

        var result = calculator.CalculateAllOverlaps([efiv, voo, qqq]);

        Assert.That(result.Single(x => x.FirstTicker == "EFIV" && x.SecondTicker == "VOO").Overlap, Is.EqualTo(0.08m));
        Assert.That(result.Single(x => x.FirstTicker == "EFIV" && x.SecondTicker == "QQQ").Overlap, Is.EqualTo(0.05m));
        Assert.That(result.Single(x => x.FirstTicker == "VOO" && x.SecondTicker == "QQQ").Overlap, Is.EqualTo(0.05m));
    }
}