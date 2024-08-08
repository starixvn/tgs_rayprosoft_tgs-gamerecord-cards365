using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Tgs.GameRecord.Cards365.Common;

public record Cards365GameRecordManualMessage
{
	[Required]
	[DataType(DataType.Url)]
	[DefaultValue(typeof(Uri), "https://g5ky3i.365gaming.cc/")]
	public Uri ApiUrl { get; init; } = default!;
	[Required]
	[DefaultValue(typeof(string), "card365pega001")]
	public string Website { get; init; } = default!;
	[Required]
	[DefaultValue(typeof(string), "40D237D0A995DFEAE0590F99552D3F6F")]
	public string Cert { get; init; } = default!;
	[Required]
	[DefaultValue(typeof(string), "cardp101")]
	public string Extension1 { get; init; } = default!;
	[Required]
	[DataType(DataType.DateTime)]
	[DefaultValue(typeof(DateTimeOffset), "2023-05-04T00:00:00+08:00")]
	public DateTimeOffset StartTime { get; init; } = default!;
	[Required]
	[DataType(DataType.DateTime)]
	[DefaultValue(typeof(DateTimeOffset), "2023-05-05T00:00:00+08:00")]
	public DateTimeOffset EndTime { get; init; } = default!;
}
