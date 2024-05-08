using System.ComponentModel.DataAnnotations;

namespace Tgs.GameRecord.Cards365.Common;

public record Cards365GameRecordScheduleMessage
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
}