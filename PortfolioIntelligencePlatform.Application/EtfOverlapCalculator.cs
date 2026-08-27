using PortfolioIntelligencePlatform.Domain;

namespace PortfolioIntelligencePlatform.Application;

public class EtfOverlapCalculator
{
    public decimal CalculateOverlap(Etf firstEtf, Etf secondEtf)
    {
        var secondHoldings = secondEtf.Holdings
            .Where(holding => !string.IsNullOrWhiteSpace(holding.Symbol))
            .GroupBy(
                holding => holding.Symbol,
                StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(holding => holding.Weight),
                StringComparer.OrdinalIgnoreCase);

        return firstEtf.Holdings
            .Where(holding => !string.IsNullOrWhiteSpace(holding.Symbol))
            .Sum(firstHolding =>
                secondHoldings.TryGetValue(firstHolding.Symbol, out var secondWeight)
                    ? Math.Min(firstHolding.Weight, secondWeight)
                    : 0m);
    }
    
    public IReadOnlyCollection<EtfOverlapResult> CalculateAllOverlaps(IReadOnlyCollection<Etf> etfs)
    {
        var etfList = etfs.ToList();
        var results = new List<EtfOverlapResult>();

        for (var i = 0; i < etfList.Count; i++)
        {
            for (var j = i + 1; j < etfList.Count; j++)
            {
                var firstEtf = etfList[i];
                var secondEtf = etfList[j];

                results.Add(new EtfOverlapResult
                {
                    FirstTicker = firstEtf.Ticker,
                    SecondTicker = secondEtf.Ticker,
                    Overlap = CalculateOverlap(firstEtf, secondEtf)
                });
            }
        }

        return results;
    }
}