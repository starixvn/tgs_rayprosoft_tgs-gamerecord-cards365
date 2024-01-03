namespace Tgs.GameRecord.Cards365.Core;

public record GameRecordInfoOptions
{
    public string GameProviderCode { get; set; } = default!;
    public string GameProviderConfiguration { get; init; } = default!;
    public string HangfirePrefix { get; set; } = "gamerecord-cards365-hangfire:";
    public string HangfireLocking { get; set; } = "gamerecord-cards365-locking:";
    public double LockMinute { get; set; } = 8d;
}