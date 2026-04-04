namespace VisaBot.Humanization;

/// <summary>
/// Rastgele, insansı bekleme aralıkları (anti-bot ve doğal davranış için).
/// </summary>
public static class HumanDelay
{
    public const string ChromeLikeUserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36";

    public static async Task WaitMsAsync(int minMs, int maxMs, CancellationToken cancellationToken = default)
    {
        if (maxMs < minMs)
            (minMs, maxMs) = (maxMs, minMs);

        var ms = Random.Shared.Next(minMs, maxMs + 1);
        await Task.Delay(ms, cancellationToken);
    }

    /// <summary>Tarayıcıyı başlatmadan önce ek gecikme (başlatma öncesi).</summary>
    public static Task BeforeBrowserLaunchAsync(CancellationToken cancellationToken = default) =>
        WaitMsAsync(1_200, 4_200, cancellationToken);

    /// <summary>Process / context açılışları arasında kısa ara.</summary>
    public static Task BetweenStepsAsync(CancellationToken cancellationToken = default) =>
        WaitMsAsync(450, 2_200, cancellationToken);

    /// <summary>Sayfa yüklendikten sonra "okuma" hissi.</summary>
    public static Task AfterNavigationAsync(CancellationToken cancellationToken = default) =>
        WaitMsAsync(800, 2_800, cancellationToken);
}
