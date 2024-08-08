namespace Tgs.GameRecord.Cards365.Core.Models;
public record GameRecordScheduleInfo(
	string Website,
	string Extension1,
	DateTimeOffset NextTime,
	string NextExternalBetId,
	DateTimeOffset CreatedAt,
	DateTimeOffset UpdatedAt);
