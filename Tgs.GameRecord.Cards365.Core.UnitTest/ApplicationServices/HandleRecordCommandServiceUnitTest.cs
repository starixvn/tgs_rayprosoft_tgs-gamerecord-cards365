using System;
using System.Threading;
using Google.Protobuf.WellKnownTypes;
using NSubstitute;
using Tgs.GameCommand.Common;
using Tgs.GameInterface.Cards365.Protos;
using Tgs.GameRecord.Cards365.Core.ApplicationServices;
using Tgs.GameRounds.Work.Client;
using Tgs.GameRounds.Work.Common.Protos;

namespace Tgs.GameRecord.Cards365.Core.UnitTest.ApplicationServices;

public class HandleRecordCommandServiceUnitTest
{
    [Fact]
    public async void Loss()
    {
        var fakeUtensil = Substitute.For<IUtensil>();
        var fakeClient = Substitute.For<IGameRoundsWorkClient>();

        var command = new HandleRecordCommand(
            Info: new GameRecordInfo
            {
                ExternalRoundId = "87032f02ea3d11ed853f5254002237ea",
                ExternalGameId = "549",
                PlayerCode = "4ucmqplxb029",
                IsClosed = true,
                IsVoid = false,
                UpdateTime = DateTimeOffset.Parse("2023-05-04T13:36:16.185+08:00").ToTimestamp(),
                ExternalCreatedOn = DateTimeOffset.Parse("2023-05-04T13:35:54.886+08:00").ToTimestamp(),
                ExternalClosedOn = DateTimeOffset.Parse("2023-05-04T13:36:16.092+08:00").ToTimestamp(),
                Bet = 33.0m,
                ValidBet = 33.0m,
                Settle = 0,
                Winloss = -33.0m,
                Commission = 0,
                AdditionalData = "{\"txId\":\"87032f02ea3d11ed853f5254002237ea\"}"
            });


        var gameProviderId = Guid.NewGuid();
        _ = fakeUtensil
            .GetGameProviderIdAsync(
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(gameProviderId);

        var gameProviderCode = "365TF";
        _ = fakeUtensil
            .GetGameProviderCode()
            .Returns(gameProviderCode);

        var player = new PlayerInfo(
            ResellerId: Guid.NewGuid(),
            ResellerCode: "TGS",
            LicenseeId: Guid.NewGuid(),
            LicenseeCode: "TGS",
            BrandId: Guid.NewGuid(),
            BrandCode: "PB2",
            PlayerCode: command.Info.PlayerCode,
            Username: "test",
            DefaultWalletId: Guid.NewGuid(),
            DefaultWalletCurrencyCode: "PHP"
            );
        _ = fakeUtensil
        .GetPlayerAsync(
                playerCode: Arg.Is(command.Info.PlayerCode),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(player);

        var gameId = 1;
        _ = fakeUtensil
            .GetGameIdAsync(
        gameProviderId: Arg.Is(gameProviderId),
                gameExternalId: Arg.Is(command.Info.ExternalGameId.ToString()),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(gameId);

        var gameProviderConfigurationId = Guid.NewGuid();
        _ = fakeUtensil
            .GetBrandGameProviderConfigurationIdAsync(
                brandId: Arg.Is(player.BrandId),
                gameProviderId: Arg.Is(gameProviderId),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(gameProviderConfigurationId);

        var service = new HandleRecordCommandService(
            fakeUtensil,
            fakeClient);

        await service
            .ExecuteAsync(
                command)
            .ConfigureAwait(false);

        await fakeClient
            .Received(1)
            .RoundBidding(
                gameProviderId: Arg.Is(gameProviderId),
                gameProviderCode: Arg.Is(gameProviderCode),
                externalRoundId: Arg.Is(command.Info.ExternalRoundId),
                gameId: Arg.Is(gameId),
                externalGameId: Arg.Is(command.Info.ExternalGameId),
                playerCode: Arg.Is(player.PlayerCode),
                walletId: Arg.Is(player.DefaultWalletId),
                currencyCode: Arg.Is(player.DefaultWalletCurrencyCode),
                brandId: Arg.Is(player.BrandId),
                gameProviderConfigurationId: Arg.Is(gameProviderConfigurationId),
                status: Arg.Is(RoundStatus.Closed),
                clientStatus: Arg.Is(RoundClientStatus.Loss),
                externalCreatedOn: Arg.Is(command.Info.ExternalCreatedOn.ToDateTimeOffset()),
                externalClosedOn: Arg.Is(command.Info.ExternalClosedOn.ToDateTimeOffset()),
                bet: Arg.Is(command.Info.Bet.ToDecimal().GetValueOrDefault(0)),
                validBet: Arg.Is(command.Info.ValidBet.ToDecimal().GetValueOrDefault(0)),
                settle: Arg.Is(command.Info.Settle.ToDecimal().GetValueOrDefault(0)),
                commission: Arg.Is(command.Info.Commission.ToDecimal().GetValueOrDefault(0)),
                turnover: Arg.Is(command.Info.ValidBet.ToDecimal().GetValueOrDefault(0)),
                playerWinLoss: Arg.Is(command.Info.Winloss.ToDecimal().GetValueOrDefault(0)),
                additionalData: Arg.Is(command.Info.AdditionalData),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);
    }

    [Fact]
    public async void Win()
    {
        var fakeUtensil = Substitute.For<IUtensil>();
        var fakeClient = Substitute.For<IGameRoundsWorkClient>();

        var command = new HandleRecordCommand(
            Info: new GameRecordInfo
            {
                ExternalRoundId = "b0d2caf0ee1511eda82a5254002237ea",
                ExternalGameId = "449",
                PlayerCode = "4uglyyqu4nae",
                IsClosed = true,
                IsVoid = false,
                UpdateTime = DateTimeOffset.Parse("2023-05-09T11:01:03.449+08:00").ToTimestamp(),
                ExternalCreatedOn = DateTimeOffset.Parse("2023-05-09T11:00:50.221+08:00").ToTimestamp(),
                ExternalClosedOn = DateTimeOffset.Parse("2023-05-09T11:01:03.354+08:00").ToTimestamp(),
                Bet = 36,
                ValidBet = 34.92m,
                Settle = 69.84m,
                Winloss = 34.92m,
                Commission = 1.08m,
                AdditionalData = "{\"txId\":\"b0d2caf0ee1511eda82a5254002237ea\",\"gameNumber\":\"449-1683601245-866371-1\",\"userId\":\"4uglyyqu4nae\",\"extension1\":\"cardp101\",\"status\":\"Pay\",\"tiles\":\"[a1, a1, a1, a2, a3, a4, a6, a7, a7, a8, a8, a9, d9, d9]\",\"gameId\":449,\"chair\":0,\"playId\":1,\"commission\":1.08,\"banker\":0,\"profit\":34.92,\"validbet\":34.92,\"bonus\":0,\"reward\":0,\"outMoney\":0,\"currency\":\"CNY\",\"updateTime\":\"05/09/2023 11:01:03.449\",\"createTime\":\"05/09/2023 11:00:50.320\",\"betTransTime\":\"05/09/2023 11:00:50.221\",\"payTransTime\":\"05/09/2023 11:01:03.354\",\"betAmount\":2087,\"realBetAmount\":36,\"payAmount\":2121.92,\"realPayAmount\":36}"
            });


        var gameProviderId = Guid.NewGuid();
        _ = fakeUtensil
            .GetGameProviderIdAsync(
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(gameProviderId);

        var gameProviderCode = "365TF";
        _ = fakeUtensil
            .GetGameProviderCode()
            .Returns(gameProviderCode);

        var player = new PlayerInfo(
            ResellerId: Guid.NewGuid(),
            ResellerCode: "TGS",
            LicenseeId: Guid.NewGuid(),
            LicenseeCode: "TGS",
            BrandId: Guid.NewGuid(),
            BrandCode: "PB2",
            PlayerCode: command.Info.PlayerCode,
            Username: "test",
            DefaultWalletId: Guid.NewGuid(),
            DefaultWalletCurrencyCode: "PHP"
            );
        _ = fakeUtensil
        .GetPlayerAsync(
                playerCode: Arg.Is(command.Info.PlayerCode),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(player);

        var gameId = 1;
        _ = fakeUtensil
            .GetGameIdAsync(
        gameProviderId: Arg.Is(gameProviderId),
                gameExternalId: Arg.Is(command.Info.ExternalGameId.ToString()),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(gameId);

        var gameProviderConfigurationId = Guid.NewGuid();
        _ = fakeUtensil
            .GetBrandGameProviderConfigurationIdAsync(
                brandId: Arg.Is(player.BrandId),
                gameProviderId: Arg.Is(gameProviderId),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(gameProviderConfigurationId);

        var service = new HandleRecordCommandService(
            fakeUtensil,
            fakeClient);

        await service
            .ExecuteAsync(
                command)
            .ConfigureAwait(false);

        await fakeClient
            .Received(1)
            .RoundBidding(
                gameProviderId: Arg.Is(gameProviderId),
                gameProviderCode: Arg.Is(gameProviderCode),
                externalRoundId: Arg.Is(command.Info.ExternalRoundId),
                gameId: Arg.Is(gameId),
                externalGameId: Arg.Is(command.Info.ExternalGameId),
                playerCode: Arg.Is(player.PlayerCode),
                walletId: Arg.Is(player.DefaultWalletId),
                currencyCode: Arg.Is(player.DefaultWalletCurrencyCode),
                brandId: Arg.Is(player.BrandId),
                gameProviderConfigurationId: Arg.Is(gameProviderConfigurationId),
                status: Arg.Is(RoundStatus.Closed),
                clientStatus: Arg.Is(RoundClientStatus.Win),
                externalCreatedOn: Arg.Is(command.Info.ExternalCreatedOn.ToDateTimeOffset()),
                externalClosedOn: Arg.Is(command.Info.ExternalClosedOn.ToDateTimeOffset()),
                bet: Arg.Is(command.Info.Bet.ToDecimal().GetValueOrDefault(0)),
                validBet: Arg.Is(command.Info.ValidBet.ToDecimal().GetValueOrDefault(0)),
                settle: Arg.Is(command.Info.Settle.ToDecimal().GetValueOrDefault(0)),
                commission: Arg.Is(command.Info.Commission.ToDecimal().GetValueOrDefault(0)),
                turnover: Arg.Is(command.Info.ValidBet.ToDecimal().GetValueOrDefault(0)),
                playerWinLoss: Arg.Is(command.Info.Winloss.ToDecimal().GetValueOrDefault(0)),
                additionalData: Arg.Is(command.Info.AdditionalData),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);
    }

    [Fact]
    public async void Unsettlement()
    {
        var fakeUtensil = Substitute.For<IUtensil>();
        var fakeClient = Substitute.For<IGameRoundsWorkClient>();

        var command = new HandleRecordCommand(
            Info: new GameRecordInfo
            {
                ExternalRoundId = "b0d2caf0ee1511eda82a5254002237ea",
                ExternalGameId = "449",
                PlayerCode = "4uglyyqu4nae",
                IsClosed = false,
                IsVoid = false,
                UpdateTime = DateTimeOffset.Parse("2023-05-09T11:01:03.449+08:00").ToTimestamp(),
                ExternalCreatedOn = DateTimeOffset.Parse("2023-05-09T11:00:50.221+08:00").ToTimestamp(),
                ExternalClosedOn = DateTimeOffset.UnixEpoch.ToTimestamp(),
                Bet = 36,
                ValidBet = 34.92m,
                Settle = 0.0m,
                Winloss = 0.0m,
                Commission = 0.0m,
                AdditionalData = "{\"recordID\": \"643583c37c18c573b57bbb2c\"}"
            });

        var gameProviderId = Guid.NewGuid();
        _ = fakeUtensil
            .GetGameProviderIdAsync(
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(gameProviderId);

        var gameProviderCode = "365TF";
        _ = fakeUtensil
            .GetGameProviderCode()
            .Returns(gameProviderCode);

        var player = new PlayerInfo(
            ResellerId: Guid.NewGuid(),
            ResellerCode: "TGS",
            LicenseeId: Guid.NewGuid(),
            LicenseeCode: "TGS",
            BrandId: Guid.NewGuid(),
            BrandCode: "PB2",
            PlayerCode: command.Info.PlayerCode,
            Username: "test",
            DefaultWalletId: Guid.NewGuid(),
            DefaultWalletCurrencyCode: "PHP"
            );
        _ = fakeUtensil
        .GetPlayerAsync(
                playerCode: Arg.Is(command.Info.PlayerCode),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(player);

        var gameId = 1;
        _ = fakeUtensil
            .GetGameIdAsync(
        gameProviderId: Arg.Is(gameProviderId),
                gameExternalId: Arg.Is(command.Info.ExternalGameId.ToString()),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(gameId);

        var gameProviderConfigurationId = Guid.NewGuid();
        _ = fakeUtensil
            .GetBrandGameProviderConfigurationIdAsync(
                brandId: Arg.Is(player.BrandId),
                gameProviderId: Arg.Is(gameProviderId),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(gameProviderConfigurationId);

        var service = new HandleRecordCommandService(
            fakeUtensil,
            fakeClient);

        await service
            .ExecuteAsync(
                command)
            .ConfigureAwait(false);

        await fakeClient
            .Received(1)
            .RoundBidding(
                gameProviderId: Arg.Is(gameProviderId),
                gameProviderCode: Arg.Is(gameProviderCode),
                externalRoundId: Arg.Is(command.Info.ExternalRoundId),
                gameId: Arg.Is(gameId),
                externalGameId: Arg.Is(command.Info.ExternalGameId),
                playerCode: Arg.Is(player.PlayerCode),
                walletId: Arg.Is(player.DefaultWalletId),
                currencyCode: Arg.Is(player.DefaultWalletCurrencyCode),
                brandId: Arg.Is(player.BrandId),
                gameProviderConfigurationId: Arg.Is(gameProviderConfigurationId),
                status: Arg.Is(RoundStatus.Open),
                clientStatus: Arg.Is(RoundClientStatus.Open),
                externalCreatedOn: Arg.Is(command.Info.ExternalCreatedOn.ToDateTimeOffset()),
                externalClosedOn: Arg.Is((DateTimeOffset?)null),
                bet: Arg.Is(command.Info.Bet.ToDecimal().GetValueOrDefault(0)),
                validBet: Arg.Is(command.Info.ValidBet.ToDecimal().GetValueOrDefault(0)),
                settle: Arg.Is(0.0m),
                commission: Arg.Is(0.0m),
                turnover: Arg.Is(0.0m),
                playerWinLoss: Arg.Is(0.0m),
                additionalData: Arg.Is(command.Info.AdditionalData),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);
    }
}