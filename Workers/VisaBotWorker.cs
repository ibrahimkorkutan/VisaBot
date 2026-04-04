using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using Telegram.Bot;
using Telegram.Bot.Types;
using VisaBot.Config;
using VisaBot.Humanization;

namespace VisaBot.Workers;

public sealed class VisaBotWorker : BackgroundService
{
    private const string NoQuotaMessage = "Aktif Randevu Kotası Bulunmamaktadır";
    private const int QuotaMessageWaitTimeoutMs = 5_000;
    private const int AlarmLogRepeatCount = 25;

    private readonly ILogger<VisaBotWorker> _logger;
    private readonly ConfigSettings _settings;

    public VisaBotWorker(ILogger<VisaBotWorker> logger, IOptions<ConfigSettings> options)
    {
        _logger = logger;
        _settings = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AS Visa (Macaristan) otomasyon döngüsü başladı.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunVisitCycleAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tur sırasında beklenmeyen hata.");
            }

            var interval = TimeSpan.FromMinutes(Math.Max(1, _settings.IntervalMinutes));
            var jitterMax = Math.Max(0, _settings.JitterSecondsMax);
            var jitter = TimeSpan.FromSeconds(Random.Shared.Next(-jitterMax, jitterMax + 1));
            var nextWait = interval + jitter;
            if (nextWait < TimeSpan.FromMinutes(1))
                nextWait = TimeSpan.FromMinutes(1);

            _logger.LogInformation("Sonraki ziyaret yaklaşık {Delay} sonra.", nextWait);
            await Task.Delay(nextWait, stoppingToken);
        }

        _logger.LogInformation("Otomasyon döngüsü durduruldu.");
    }

    private async Task RunVisitCycleAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_settings.TargetUrl))
        {
            _logger.LogWarning("TargetUrl yapılandırması boş; tur atlanıyor.");
            return;
        }

        await HumanDelay.BeforeBrowserLaunchAsync(ct);

        using var playwright = await Playwright.CreateAsync();
        await HumanDelay.BetweenStepsAsync(ct);

        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = _settings.Headless,
            Args = new[] { "--ignore-certificate-errors" },
            SlowMo = _settings.Headless ? 0 : Random.Shared.Next(0, 80)
        });

        await HumanDelay.BetweenStepsAsync(ct);

        var contextOptions = new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1280, Height = 720 },
            Locale = "tr-TR",
            IgnoreHTTPSErrors = true
        };

        if (_settings.Headless)
            contextOptions.UserAgent = HumanDelay.ChromeLikeUserAgent;

        var context = await browser.NewContextAsync(contextOptions);
        var page = await context.NewPageAsync();

        await HumanDelay.BetweenStepsAsync(ct);

        _logger.LogInformation("Randevu sayfasına gidiliyor: {Url}", _settings.TargetUrl);
        await page.GotoAsync(_settings.TargetUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded,
            Timeout = 60_000
        });

        await HumanDelay.AfterNavigationAsync(ct);

        var quotaLocator = page.GetByText(NoQuotaMessage, new PageGetByTextOptions { Exact = false }).First;

        try
        {
            await quotaLocator.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = QuotaMessageWaitTimeoutMs
            });

            _logger.LogInformation("Kota yok, bir sonraki döngü bekleniyor...");
        }
        catch (TimeoutException)
        {
            await TriggerPossibleAppointmentAlarmAsync(ct);
        }
        catch (PlaywrightException)
        {
            await TriggerPossibleAppointmentAlarmAsync(ct);
        }
    }

    private async Task TriggerPossibleAppointmentAlarmAsync(CancellationToken ct)
    {
        _logger.LogWarning(
            "'{Message}' metni {TimeoutMs} ms içinde görünmedi; kota açılmış veya sayfa değişmiş olabilir.",
            NoQuotaMessage,
            QuotaMessageWaitTimeoutMs);

        await TrySendTelegramQuotaOpenedAlertAsync(ct);

        for (var i = 0; i < AlarmLogRepeatCount; i++)
        {
            ct.ThrowIfCancellationRequested();
            _logger.LogInformation("🚨 RANDEVU OLABİLİR 🚨");
            if (OperatingSystem.IsWindows())
                Console.Beep(frequency: 1000, duration: 350);
            await Task.Delay(120, ct);
        }
    }

    private async Task TrySendTelegramQuotaOpenedAlertAsync(CancellationToken ct)
    {
        var token = (_settings.TelegramBotToken ?? string.Empty).Trim();
        var chatIdRaw = (_settings.TelegramChatId ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(token)
            || string.IsNullOrWhiteSpace(chatIdRaw)
            || token.Equals("YOUR_TOKEN", StringComparison.OrdinalIgnoreCase)
            || chatIdRaw.Equals("YOUR_CHAT_ID", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Telegram token veya chat id yapılandırılmadı; anlık bildirim atlandı.");
            return;
        }

        try
        {
            var bot = new TelegramBotClient(token);
            var text =
                $"🚨 MACARİSTAN VİZE KOTASI AÇILDI! HEMEN SİTEYE GİR! 🚨 URL: {_settings.TargetUrl}";
            // Telegram.Bot v22+: metin için SendMessage (eski SendTextMessageAsync).
            await bot.SendMessage(new ChatId(chatIdRaw), text, cancellationToken: ct);
            _logger.LogInformation("Telegram uyarısı gönderildi.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Telegram uyarısı gönderilemedi.");
        }
    }
}
