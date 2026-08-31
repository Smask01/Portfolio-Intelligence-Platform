namespace PortfolioIntelligencePlatform.Api.Dtos;

public class PortfolioPositionRequest
{
    public string Ticker { get; set; } = string.Empty;
    public decimal AmountInvested { get; set; }
    public AssetType AssetType { get; set; }
}