namespace PortfolioIntelligencePlatform.Domain;

public record EtfHolding
{
    public required string Symbol { get; set; }
    public required string CompanyName { get; set; }
    public required string Sector { get; set; }
    public decimal Weight { get; set; }
}