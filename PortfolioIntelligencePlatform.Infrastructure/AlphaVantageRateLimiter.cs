namespace PortfolioIntelligencePlatform.Infrastructure;

public sealed class AlphaVantageRateLimiter
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private DateTime _lastRequestTime = DateTime.MinValue;

    public async Task WaitAsync(CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            var minimumDelay = TimeSpan.FromMilliseconds(1200);

            var timeSinceLastRequest = DateTime.UtcNow - _lastRequestTime;

            if (timeSinceLastRequest < minimumDelay)
            {
                await Task.Delay(minimumDelay - timeSinceLastRequest, cancellationToken);
            }

            _lastRequestTime = DateTime.UtcNow;
        }
        finally
        {
            _lock.Release();
        }
    }
}