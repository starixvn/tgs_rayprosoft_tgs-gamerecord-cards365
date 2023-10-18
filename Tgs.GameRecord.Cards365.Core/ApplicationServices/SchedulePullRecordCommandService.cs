using Microsoft.Extensions.Logging;
using Tgs.GameInterface.Cards365.Client;
using Tgs.GameInterface.Cards365.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.Cards365.Core.ApplicationServices;

internal class SchedulePullRecordCommandService : ICommandService<SchedulePullRecordCommand>
{
    private readonly IGameInterfaceCards365Client _client;
    private readonly IGameRecordScheduleService _gameRecordScheduleService;
    private readonly ILogger<SchedulePullRecordCommandService> _logger;
    private readonly ISchedulePullRecordLock _schedulePullRecordLock;
    private readonly ICommandService<HandleRecordCommand> _service;
    private readonly IUtensil _utensil;

    public SchedulePullRecordCommandService(
        ILogger<SchedulePullRecordCommandService> logger,
        ISchedulePullRecordLock schedulePullRecordLock,
        IUtensil utensil,
        IGameRecordScheduleService gameRecordScheduleService,
        IGameInterfaceCards365Client client,
        ICommandService<HandleRecordCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _schedulePullRecordLock = schedulePullRecordLock ?? throw new ArgumentNullException(nameof(schedulePullRecordLock));
        _utensil = utensil ?? throw new ArgumentNullException(nameof(utensil));
        _gameRecordScheduleService = gameRecordScheduleService ?? throw new ArgumentNullException(nameof(gameRecordScheduleService));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask ExecuteAsync(SchedulePullRecordCommand command, CancellationToken cancellationToken = default)
    {
        if (!await _schedulePullRecordLock
            .LockingAsync(
                command.Extension1)
            .ConfigureAwait(false))
        {
            _logger.LogWarning("LogOn:{logOn} Message:This data type ({extension1}) pull record has been locked.",
                DateTime.Now.ToString("o"),
                command.Extension1);
            return;
        }

        try
        {
            var gameProviderId = await _utensil
                .GetGameProviderIdAsync(
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            var gameRecordSchedule =
                await _gameRecordScheduleService
                .GetAsync(
                    extension1: command.Extension1,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false) ??
                await _gameRecordScheduleService
                .InitAsync(
                    gameProviderId: gameProviderId,
                    extension1: command.Extension1,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false)
                ?? throw new Exception($"Failed to get Game Record Schedule data.({command.Extension1})");

            var config = _gameRecordScheduleService.GetConfig(gameRecordSchedule);

            var nextTime = config.NextTime;
            var nextExternalRoundId = config.NextExternalRoundId;

            var gameWagerInfos = Enumerable.Empty<GameWagerInfo>();
            do
            {
                var lastupdatedate = nextTime.ToUnixTimeMilliseconds();

                if (nextTime == DateTimeOffset.UnixEpoch)
                {
                    lastupdatedate = 0;
                }

                gameWagerInfos = await _client.GameWagerByLastUpdateAsync(
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
                        .ThenBy(x => x.ExternalRoundId)
                        .Select(x => new
                        {
                            x.ExternalRoundId,
                            x.UpdateTime
                        })
                        .LastOrDefault();

                    if (last is null)
                    {
                        break;
                    }

                    if (DateTimeOffset.Compare(nextTime, last.UpdateTime.ToDateTimeOffset()) == 0 &&
                        nextExternalRoundId.Equals(last.ExternalRoundId, StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }
                    else
                    {
                        nextTime = last.UpdateTime.ToDateTimeOffset();
                        nextExternalRoundId = last.ExternalRoundId;
                    }

                    _logger.LogDebug("Extension1:{extension1} | NextTime:{nextTime} | NextExternalRoundId:{nextExternalRoundId}", command.Extension1, nextTime.ToString("O"), nextExternalRoundId);

                    foreach (var gameWagerInfo in gameWagerInfos)
                    {
                        _logger.LogDebug("LogOn:{logOn} Message: Settlement. GameRecords:{gameRecord}", DateTime.Now.ToString("o"), gameWagerInfo);

                        try
                        {
                            await _service
                                .ExecuteAsync(
                                    command: new(gameWagerInfo),
                                    cancellationToken: cancellationToken)
                                .ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "LogOn:{logOn} Message: An exception occurred in this game record.({gameWagerInfo})", DateTime.Now.ToString("o"), gameWagerInfo);
                        }
                    }
                }
            } while (gameWagerInfos.Any());

            _ = await _gameRecordScheduleService
                .UpdateAsync(
                    gameRecordSchedule: gameRecordSchedule,
                    extension1: command.Extension1,
                    nextTime: nextTime,
                    nextExternalRoundId: nextExternalRoundId,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LogOn:{logOn} Message: Exception.", DateTime.Now.ToString("o"));
        }
        finally
        {
            _ = await _schedulePullRecordLock
                .UnlockAsync(
                    command.Extension1)
                .ConfigureAwait(false);
        }
    }
}