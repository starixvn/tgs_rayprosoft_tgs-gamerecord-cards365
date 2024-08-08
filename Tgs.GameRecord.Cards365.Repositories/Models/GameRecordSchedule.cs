using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Tgs.GameRecord.Cards365.Repositories.Models;
[Table("game_record_schedules")]
public record GameRecordSchedule
{
	/// <summary>
	/// The identifier
	/// </summary>
	[BsonId]
	public ObjectId Id { get; set; } = default!;
	/// <summary>
	/// The website
	/// </summary>
	[BsonElement("web_site")]
	public string Website { get; set; } = default!;
	/// <summary>
	/// The extension1
	/// </summary>
	[BsonElement("extension_1")]
	public string Extension1 { get; set; } = default!;
	/// <summary>
	/// The next time
	/// </summary>
	[BsonElement("next_time")]
	public DateTime NextTime { get; set; } = default!;
	/// <summary>
	/// The next external bet identifier
	/// </summary>
	[BsonElement("next_external_bet_id")]
	public string NextExternalBetId { get; set; } = default!;
	/// <summary>
	/// The created at
	/// </summary>
	[BsonElement("created_at")]
	public DateTime CreatedAt { get; init; } = default!;
	/// <summary>
	/// The updated at
	/// </summary>
	[BsonElement("updated_at")]
	public DateTime UpdatedAt { get; init; } = default!;
}
