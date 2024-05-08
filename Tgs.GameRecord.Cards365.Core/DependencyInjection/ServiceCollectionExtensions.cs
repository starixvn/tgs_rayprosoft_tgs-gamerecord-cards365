using System.Reflection;
using StackExchange.Redis;
using Tgs.GameCommand.Client;
using Tgs.GameInterface.Cards365.Client;
using Tgs.GameRecord.Cards365.Core;
using Tgs.GameRecordSchedules.Client;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddGameRecordCore(
		this IServiceCollection services,
		Action<GameCommandClientOptions, IServiceProvider> gameCommandClientOptions,
		Action<GameInterfaceCards365ClientOptions, IServiceProvider> gameInterfaceCards365ClientOptions,
		Action<GameRecordSchedulesClientOptions, IServiceProvider> gameRecordSchedulesClientOptions,
		Func<IServiceProvider, IDatabase> redisDatabaseResolver)
		=> services
		.RegisterAllServiceUtilityTypes(
			Assembly.GetExecutingAssembly(),
			ServiceLifetime.Transient)
		.AddMemoryCache()
		.AddGameCommandClient(gameCommandClientOptions)
		.AddGameInterfaceCards365Client(gameInterfaceCards365ClientOptions)
		.AddGameRecordSchedulesClient(gameRecordSchedulesClientOptions)
		.AddTransient<IHangfireUtensil, HangfireUtensil>()
		 .AddTransient<IHangfireLock>(sp => ActivatorUtilities.CreateInstance<HangfireLock>(sp, redisDatabaseResolver(sp)))
		.AddSingleton<ISchedulePullRecordLock>(sp => ActivatorUtilities.CreateInstance<SchedulePullRecordLock>(sp, redisDatabaseResolver(sp)))
		.AddSingleton<IUtensil, UtensilRepository>()
		.AddSingleton<ISystemClock, SystemClock>()
		.AddTransient<IGameRecordScheduleService, GameRecordScheduleService>();
}