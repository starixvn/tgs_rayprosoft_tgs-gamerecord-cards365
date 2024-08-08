using Microsoft.Extensions.Logging;
using Tgs.GameInterface.Cards365.Client;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.Cards365.Core.ApplicationServices;

internal class SchedulePullRecordCommandService(
	ILogger<SchedulePullRecordCommandService> logger,
	IGameRecordScheduleRepository scheduleRepository,
	IGameInterfaceCards365Client client,
	ICommandService<HandleRecordCommand> service) : ICommandService<SchedulePullRecordCommand>
{
	public async ValueTask ExecuteAsync(SchedulePullRecordCommand command, CancellationToken cancellationToken = default)
	{
		var gameRecordSchedule = await scheduleRepository.GetAsync(
			website: command.Website,
			extension1: command.Extension1,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false) ??
			await scheduleRepository.AddAsync(
				website: command.Website,
				extension1: command.Extension1,
				cancellationToken: cancellationToken)
			.ConfigureAwait(false)
			?? throw new Exception($"Failed to get Game Record Schedule data.({command.Extension1})");

		var nextTime = gameRecordSchedule.NextTime;
		var nextExternalBetId = gameRecordSchedule.NextExternalBetId;

		var gameWagerInfos = Enumerable.Empty<GameInterface.Cards365.Protos.GameWagerInfo>();
		do
		{
			var lastupdatedate = nextTime.ToUnixTimeMilliseconds();

			if (nextTime == DateTimeOffset.UnixEpoch)
			{
				lastupdatedate = 0;
			}

			gameWagerInfos = await client.GameWagerByLastUpdateAsync(
			   apiUrl: command.ApiUrl,
			   website: command.Website,
			   cert: command.Cert,
			   extension1: command.Extension1,
			   lastupdatedate: lastupdatedate,
			   cancellationToken: cancellationToken)
			   .ToArrayAsync(
			   cancellationToken: cancellationToken)
			   .ConfigureAwait(false)
			   ?? Enumerable.Empty<GameInterface.Cards365.Protos.GameWagerInfo>();

			if (gameWagerInfos.Any())
			{
				var last = gameWagerInfos
					.OrderBy(x => x.UpdateTime)
					.ThenBy(x => x.ExternalWagerId)
					.Select(x => new
					{
						x.ExternalWagerId,
						x.UpdateTime
					})
					.LastOrDefault();

				if (last is null)
				{
					break;
				}

				if (DateTimeOffset.Compare(nextTime, last.UpdateTime.ToDateTimeOffset()) == 0 &&
					nextExternalBetId.Equals(last.ExternalWagerId, StringComparison.OrdinalIgnoreCase))
				{
					break;
				}
				else
				{
					nextTime = last.UpdateTime.ToDateTimeOffset();
					nextExternalBetId = last.ExternalWagerId;
				}

				logger.LogDebug("Extension1:{extension1} | NextTime:{nextTime} | NextExternalWagerId:{nextExternalWagerId}", command.Extension1, nextTime.ToString("O"), nextExternalBetId);

				foreach (var gameWagerInfo in gameWagerInfos)
				{
					logger.LogDebug("LogOn:{logOn} Message: Settlement. GameRecords:{gameRecord}", DateTime.Now.ToString("o"), gameWagerInfo);

					try
					{
						await service
							.ExecuteAsync(
								command: new(gameWagerInfo),
								cancellationToken: cancellationToken)
							.ConfigureAwait(false);
					}
					catch (Exception ex)
					{
						logger.LogError(ex, "LogOn:{logOn} Message: An exception occurred in this game record.({gameWagerInfo})", DateTime.Now.ToString("o"), gameWagerInfo);
					}
				}
			}
		} while (gameWagerInfos.Any());

		await scheduleRepository.UpdateAsync(
			website: command.Website,
			extension1: command.Extension1,
			nextTime: nextTime,
			nextExternalBetId: nextExternalBetId,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
	}
}
