namespace Tgs.GameRecord.Cards365.Core;

public record GameRecordInfoOptions
{
    public string GameProviderCode { get; set; } = default!;
    public string GameProviderConfiguration { get; init; } = default!;
    public string GameProviderHangfire { get; init; } = default!;
    public double LockMinute { get; set; } = 8d;
}