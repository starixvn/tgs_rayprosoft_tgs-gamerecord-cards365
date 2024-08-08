using Microsoft.Extensions.Hosting;
using Tgs.GameRecord.Cards365.Core;

namespace Tgs.GameRecord.Cards365.Repositories;

public sealed class GameRecordScheduleInitStartup(IGameRecordScheduleRepository gameRecordScheduleRepository) : IHostedService
{
	public async Task StartAsync(CancellationToken cancellationToken)
	{
		await gameRecordScheduleRepository.InitializationAsync(
			cancellationToken)
			.ConfigureAwait(false);
	}

	public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
