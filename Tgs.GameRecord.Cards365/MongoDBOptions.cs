namespace Tgs.GameRecord.Cards365;

public record MongoDBOptions
{
	public string ConnectionString { get; set; } = "mongodb://localhost";
}
