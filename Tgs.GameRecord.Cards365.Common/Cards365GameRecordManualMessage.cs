using System.ComponentModel.DataAnnotations;

namespace Tgs.GameRecord.Cards365.Common;

public record Cards365GameRecordManualMessage
{
	[Required]
	[DataType(DataType.Url)]
	public Uri ApiUrl { get; init; } = default!;
	[Required]
	public string Website { get; init; } = default!;
	[Required]
	public string Cert { get; init; } = default!;
	[Required]
	public string Extension1 { get; init; } = default!;
	[Required]
	[DataType(DataType.DateTime)]
	public DateTimeOffset StartTime { get; init; } = default!;
	[Required]
	[DataType(DataType.DateTime)]
	public DateTimeOffset EndTime { get; init; } = default!;
}