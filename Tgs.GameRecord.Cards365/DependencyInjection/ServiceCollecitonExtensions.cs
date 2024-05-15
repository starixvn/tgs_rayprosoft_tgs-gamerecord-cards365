using Tgs.GameCommand.Client;
using Tgs.GameInterface.Cards365.Client;
using Tgs.GameRecord.Cards365;
using Tgs.GameRecord.Cards365.Core;
using Tgs.GameRecordSchedules.Client;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollecitonExtensions
{
	public static IServiceCollection AddGameRecord(
		this IServiceCollection services,
		Action<RedisOptions, IServiceProvider> redisDBconfigure,
		Action<GameRecordInfoOptions, IServiceProvider> gameRecordInfoOptions,
		Action<GameCommandClientOptions, IServiceProvider> gameCommandClientOptions,
		Action<GameInterfaceCards365ClientOptions, IServiceProvider> gameInterfaceCards365ClientOptions,
		Action<GameRecordSchedulesClientOptions, IServiceProvider> gameRecordSchedulesClientOptions)
		=> services
		.AddOptions<RedisOptions>()
		.Configure(redisDBconfigure)
		.Services
		.AddOptions<GameRecordInfoOptions>()
		.Configure(gameRecordInfoOptions)
		.Services
		.AddSingleton<RedisConnectionManager>()
		.AddGameRecordCore(
			gameCommandClientOptions: gameCommandClientOptions,
			gameInterfaceCards365ClientOptions: gameInterfaceCards365ClientOptions,
			gameRecordSchedulesClientOptions: gameRecordSchedulesClientOptions,
			redisDatabaseResolver: (sp) => sp.GetRequiredService<RedisConnectionManager>().Database);
}
