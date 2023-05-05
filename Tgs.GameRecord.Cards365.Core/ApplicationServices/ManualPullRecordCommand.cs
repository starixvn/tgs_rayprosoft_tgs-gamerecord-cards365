namespace Tgs.GameRecord.Cards365.Core.ApplicationServices;

public record ManualPullRecordCommand(
    Uri ApiUrl,
    string Website,
    string Cert,
    string Extension1,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime);