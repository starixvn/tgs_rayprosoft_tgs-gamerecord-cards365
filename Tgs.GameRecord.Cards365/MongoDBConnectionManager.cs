using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Tgs.GameRecord.Cards365;

namespace Tgs.GameRecord.Cards365;

internal class MongoDBConnectionManager
{
	public IMongoClient MongoClient { get; }

	public MongoDBConnectionManager(
		IOptions<MongoDBOptions> optionsAccessor)
	{
		var options = optionsAccessor.Value;
		var mongoUrl = new MongoUrl(options.ConnectionString);
		MongoClient = new MongoClient(mongoUrl);
	}
}
