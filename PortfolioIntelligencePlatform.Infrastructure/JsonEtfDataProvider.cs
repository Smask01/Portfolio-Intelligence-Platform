using System.Text.Json;
using PortfolioIntelligencePlatform.Application;
using PortfolioIntelligencePlatform.Domain;

namespace PortfolioIntelligencePlatform.Infrastructure;

public class JsonEtfDataProvider : IEtfDataProvider
{
    private readonly string _dataDirectory;

    public JsonEtfDataProvider()
    {
        _dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");
    }

    public async Task<Etf?> GetEtfAsync(string ticker, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_dataDirectory, $"{ticker.ToUpperInvariant()}.json");

        if (!File.Exists(filePath))
        {
            return null;
        }

        await using var stream = File.OpenRead(filePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var etf = await JsonSerializer.DeserializeAsync<Etf>(stream, options, cancellationToken: cancellationToken);
        
        return etf;
    }
}