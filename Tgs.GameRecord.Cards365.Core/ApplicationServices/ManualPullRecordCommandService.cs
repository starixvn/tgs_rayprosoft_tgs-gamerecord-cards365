using Microsoft.Extensions.Logging;
using Tgs.GameInterface.Cards365.Client;
using Tgs.GameInterface.Cards365.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.Cards365.Core.ApplicationServices;

internal class ManualPullRecordCommandService : ICommandService<ManualPullRecordCommand>
{
    private readonly IGameInterfaceCards365Client _client;
    private readonly IGameRecordUtensil _gameRecordUtensil;
    private readonly ILogger<ManualPullRecordCommandService> _logger;
    private readonly ICommandService<ManualHandleRecordCommand> _service;

    public ManualPullRecordCommandService(
        ILogger<ManualPullRecordCommandService> logger,
        IGameInterfaceCards365Client client,
        IGameRecordUtensil gameRecordUtensil,
        ICommandService<ManualHandleRecordCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _gameRecordUtensil = gameRecordUtensil ?? throw new ArgumentNullException(nameof(gameRecordUtensil));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask ExecuteAsync(ManualPullRecordCommand command, CancellationToken cancellationToken = default)
    {
        var betGameRecords = await _client
            .GameRecordByBetTimeAsync(
                apiUrl: command.ApiUrl,
                website: command.Website,
                cert: command.Cert,
                extension1: command.Extension1,
                startTime: command.StartTime,
                endTime: command.EndTime,
                cancellationToken: cancellationToken)
            .ToArrayAsync(
                cancellationToken: cancellationToken)
            .ConfigureAwait(false)
            ?? Enumerable.Empty<GameRecordInfo>();

        _logger.LogDebug("LogOn:{logOn} Message: GameRecord Unsettlement. GameRecords:{gameRecords}", DateTime.Now.ToString("o"), betGameRecords);

        if (!betGameRecords.Any())
        {
            return;
        }

        foreach (var gameRecord in betGameRecords)
        {
            _logger.LogDebug("LogOn:{logOn} Message: GameRecord Unsettlement. GameRecords:{gameRecord}", DateTime.Now.ToString("o"), gameRecord);

            try
            {
                var record = await _gameRecordUtensil
                    .SettlementConvertRecordAsync(
                        gameRecord,
                        cancellationToken)
                    .ConfigureAwait(false);

                _logger.LogDebug("LogOn:{logOn} Message: GameRecord Unsettlement. Record:{record}", DateTime.Now.ToString("o"), record);

                await _service
                    .ExecuteAsync(
                        command: new(record),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LogOn:{logOn} Message: GameRecord Unsettlement Exception. GameRecord:{gameRecord}", DateTime.Now.ToString("o"), gameRecord);
            }
        }

        var payGameRecords = await _client
            .GameRecordByPayTimeAsync(
                apiUrl: command.ApiUrl,
                website: command.Website,
                cert: command.Cert,
                extension1: command.Extension1,
                startTime: command.StartTime,
                endTime: command.EndTime,
                cancellationToken: cancellationToken)
            .ToArrayAsync(
                cancellationToken: cancellationToken)
            .ConfigureAwait(false)
            ?? Enumerable.Empty<GameRecordInfo>();

        _logger.LogDebug("LogOn:{logOn} Message: GameRecord Settlement. GameRecords:{payGameRecords}", DateTime.Now.ToString("o"), payGameRecords);

        if (!payGameRecords.Any())
        {
            return;
        }

        foreach (var gameRecord in payGameRecords)
        {
            _logger.LogDebug("LogOn:{logOn} Message: GameRecord Settlement. GameRecords:{payGameRecords}", DateTime.Now.ToString("o"), payGameRecords);

            try
            {
                var record = await _gameRecordUtensil
                    .SettlementConvertRecordAsync(
                        gameRecord,
                        cancellationToken)
                    .ConfigureAwait(false);

                _logger.LogDebug("LogOn:{logOn} Message: GameRecord Settlement. Record:{record}", DateTime.Now.ToString("o"), record);

                await _service
                    .ExecuteAsync(
                        command: new(record),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LogOn:{logOn} Message: GameRecord Settlement Exception. GameRecord:{gameRecord}", DateTime.Now.ToString("o"), gameRecord);
            }
        }
    }
}