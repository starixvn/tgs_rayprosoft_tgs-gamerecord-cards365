namespace Tgs.GameRecord.Cards365.Core;

public record ScheduleConfig
{
    public DateTimeOffset NextTime { get; set; }
    public string NextExternalRoundId { get; set; } = default!;
}