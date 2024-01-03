using Microsoft.Extensions.Logging;
using Tgs.GameInterface.Cards365.Client;
using Tgs.GameInterface.Cards365.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.Cards365.Core.ApplicationServices;

internal class SchedulePullRecordCommandService(
    ILogger<SchedulePullRecordCommandService> logger,
    ISchedulePullRecordLock schedulePullRecordLock,
    IUtensil utensil,
    IGameRecordScheduleService gameRecordScheduleService,
    IGameInterfaceCards365Client client,
    ICommandService<HandleRecordCommand> service) : ICommandService<SchedulePullRecordCommand>
{
    public async ValueTask ExecuteAsync(SchedulePullRecordCommand command, CancellationToken cancellationToken = default)
    {
        if (!await schedulePullRecordLock
            .LockingAsync(
                command.Extension1)
            .ConfigureAwait(false))
        {
            logger.LogWarning("LogOn:{logOn} Message:This data type ({extension1}) pull record has been locked.",
                DateTime.Now.ToString("o"),
                command.Extension1);
            return;
        }

        try
        {
            var gameProviderId = await utensil.GetGameProviderIdAsync(
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            var gameRecordSchedule = await gameRecordScheduleService.GetAsync(
                extension1: command.Extension1,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false) ??
                await gameRecordScheduleService.InitAsync(
                    gameProviderId: gameProviderId,
                    extension1: command.Extension1,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false)
                ?? throw new Exception($"Failed to get Game Record Schedule data.({command.Extension1})");

            var config = gameRecordScheduleService.GetConfig(gameRecordSchedule);

            var nextTime = config.NextTime;
            var nextExternalWagerId = config.NextExternalWagerId;

            var gameWagerInfos = Enumerable.Empty<GameWagerInfo>();
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
                   ?? Enumerable.Empty<GameWagerInfo>();

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
                        nextExternalWagerId.Equals(last.ExternalWagerId, StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }
                    else
                    {
                        nextTime = last.UpdateTime.ToDateTimeOffset();
                        nextExternalWagerId = last.ExternalWagerId;
                    }

                    logger.LogDebug("Extension1:{extension1} | NextTime:{nextTime} | NextExternalWagerId:{nextExternalWagerId}", command.Extension1, nextTime.ToString("O"), nextExternalWagerId);

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

            _ = await gameRecordScheduleService
                .UpdateAsync(
                    gameRecordSchedule: gameRecordSchedule,
                    extension1: command.Extension1,
                    nextTime: nextTime,
                    nextExternalWagerId: nextExternalWagerId,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "LogOn:{logOn} Message: Exception.", DateTime.Now.ToString("o"));
        }
        finally
        {
            _ = await schedulePullRecordLock.UnlockAsync(
                agentName: command.Extension1)
                .ConfigureAwait(false);
        }
    }
}