using Microsoft.Extensions.Options;
using Tgs.GameRecord.Handle.Client;
using Tgs.GameRecord.Handle.Common.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.Cards365.Core.ApplicationServices;

internal class HandleRecordCommandService : ICommandService<HandleRecordCommand>
{
    private readonly GameRecordInfoOptions _gameRecordInfo;
    private readonly IGameRecordHandleClient _client;

    public HandleRecordCommandService(
        IOptions<GameRecordInfoOptions> options,
        IGameRecordHandleClient client)
    {
        _gameRecordInfo = options.Value;
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async ValueTask ExecuteAsync(HandleRecordCommand command, CancellationToken cancellationToken = default)
    {
        var status = GameRecordStatus.Open;
        var clientStatus = GameRecordClientStatus.Open;
        var externalCreatedOn = command.Info.ExternalCreatedOn.ToDateTimeOffset();
        var externalClosedOn = command.Info.IsClosed ? command.Info.ExternalClosedOn.ToDateTimeOffset() : (DateTimeOffset?)null;
        var bet = command.Info.Bet.ToDecimal().GetValueOrDefault(0);
        var validBet = command.Info.ValidBet.ToDecimal().GetValueOrDefault(0);
        var settle = 0.0m;
        var commission = 0.0m;
        var turnover = 0.0m;
        var winLoss = 0.0m;

        if (command.Info.IsClosed)
        {
            status = GameRecordStatus.Closed;
            settle = command.Info.Settle.ToDecimal().GetValueOrDefault(0);
            commission = command.Info.Commission.ToDecimal().GetValueOrDefault(0);
            turnover = command.Info.ValidBet.ToDecimal().GetValueOrDefault(0);
            winLoss = command.Info.Winloss.ToDecimal().GetValueOrDefault(0);
            clientStatus = command.Info.IsVoid ? GameRecordClientStatus.Void : winLoss <= 0 ? GameRecordClientStatus.Loss : GameRecordClientStatus.Win;
        }

        await _client
            .MessageAsync(
            gameProviderCode: _gameRecordInfo.GameProviderCode,
            externalRoundId: command.Info.ExternalRoundId,
            externalGameId: command.Info.ExternalGameId,
            playerCode: command.Info.PlayerCode,
            status: status,
            clientStatus: clientStatus,
            externalCreatedOn: externalCreatedOn,
            externalClosedOn: externalClosedOn,
            bet: bet,
            validBet: validBet,
            settle: settle,
            commission: commission,
            turnover: turnover,
            playerWinLoss: winLoss,
            additionalData: command.Info.AdditionalData,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }
}