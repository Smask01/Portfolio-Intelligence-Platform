namespace PortfolioIntelligencePlatform.Application;

public static class SectorNormalizer
{
    public static string Normalize(string sector)
    {
        return sector.Trim().ToUpperInvariant() switch
        {
            "TECHNOLOGY" => "Information Technology",
            "INFORMATION TECHNOLOGY" => "Information Technology",

            "HEALTH CARE" => "Healthcare",
            "HEALTHCARE" => "Healthcare",

            "CONSUMER CYCLICAL" => "Consumer Discretionary",
            "CONSUMER DISCRETIONARY" => "Consumer Discretionary",

            _ => sector.Trim()
        };
    }
}