namespace VisaBot.Config;

public sealed class ConfigSettings
{
    public const string SectionName = "VisaBot";

    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string TargetUrl { get; set; } = string.Empty;
    public int IntervalMinutes { get; set; } = 10;
    public int JitterSecondsMax { get; set; } = 120;
    public bool Headless { get; set; }
    public int PostObservationWaitMinSeconds { get; set; } = 45;
    public int PostObservationWaitMaxSeconds { get; set; } = 120;
    public string TelegramBotToken { get; set; } = string.Empty;
    public string TelegramChatId { get; set; } = string.Empty;
}
