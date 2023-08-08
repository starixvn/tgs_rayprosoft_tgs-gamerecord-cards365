using Tgs.GameRounds.Work.Client;
using Tgs.GameRounds.Work.Common.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.Cards365.Core.ApplicationServices;

internal class HandleRecordCommandService : ICommandService<HandleRecordCommand>
{
    private readonly IUtensil _utensil;
    private readonly IGameRoundsWorkClient _client;

    public HandleRecordCommandService(
        IUtensil utensil,
        IGameRoundsWorkClient client)
    {
        _utensil = utensil ?? throw new ArgumentNullException(nameof(utensil));
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async ValueTask ExecuteAsync(HandleRecordCommand command, CancellationToken cancellationToken = default)
    {
        var gameProviderId = await _utensil
            .GetGameProviderIdAsync(
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var gameProviderCode = _utensil
            .GetGameProviderCode();

        var player = await _utensil
            .GetPlayerAsync(
            playerCode: command.Info.PlayerCode,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var gameId = await _utensil
            .GetGameIdAsync(
            gameProviderId: gameProviderId,
            gameExternalId: command.Info.ExternalGameId,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var gameProviderConfigurationId = await _utensil
            .GetBrandGameProviderConfigurationIdAsync(
            brandId: player.BrandId,
            gameProviderId: gameProviderId,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var status = RoundStatus.Open;
        var clientStatus = RoundClientStatus.Open;
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
            status = RoundStatus.Closed;
            settle = command.Info.Settle.ToDecimal().GetValueOrDefault(0);
            commission = command.Info.Commission.ToDecimal().GetValueOrDefault(0);
            turnover = command.Info.ValidBet.ToDecimal().GetValueOrDefault(0);
            winLoss = command.Info.Winloss.ToDecimal().GetValueOrDefault(0);
            clientStatus = command.Info.IsVoid ? RoundClientStatus.Void : winLoss <= 0 ? RoundClientStatus.Loss : RoundClientStatus.Win;
        }

        await _client
        .RoundBidding(
            gameProviderId: gameProviderId,
            gameProviderCode: gameProviderCode,
            externalRoundId: command.Info.ExternalRoundId,
            gameId: gameId,
            externalGameId: command.Info.ExternalGameId,
            playerCode: player.PlayerCode,
            walletId: player.DefaultWalletId,
            currencyCode: player.DefaultWalletCurrencyCode,
            brandId: player.BrandId,
            gameProviderConfigurationId: gameProviderConfigurationId,
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