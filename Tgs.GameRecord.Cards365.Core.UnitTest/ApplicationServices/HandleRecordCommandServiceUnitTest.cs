using System;
using System.Threading;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.GameInterface.Cards365.Protos;
using Tgs.GameRecord.Cards365.Core.ApplicationServices;
using Tgs.GameRecord.Handle.Client;
using Tgs.GameRecord.Handle.Common.Protos;

namespace Tgs.GameRecord.Cards365.Core.UnitTest.ApplicationServices;

public class HandleRecordCommandServiceUnitTest
{
    [Fact]
    public async void Loss()
    {
        var gameRecordInfo = Options.Create(new GameRecordInfoOptions
        {
            GameProviderCode = "365TF",
            LockMinute = 10
        });
        var fakeClient = Substitute.For<IGameRecordHandleClient>();

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

        var service = new HandleRecordCommandService(
            gameRecordInfo,
            fakeClient);

        await service
            .ExecuteAsync(
                command)
            .ConfigureAwait(false);

        await fakeClient
            .Received(1)
            .MessageAsync(
            gameProviderCode: Arg.Is(gameRecordInfo.Value.GameProviderCode),
            externalRoundId: Arg.Is(command.Info.ExternalRoundId),
            externalGameId: Arg.Is(command.Info.ExternalGameId),
            playerCode: Arg.Is(command.Info.PlayerCode),
            status: Arg.Is(GameRecordStatus.Closed),
            clientStatus: Arg.Is(GameRecordClientStatus.Loss),
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
        var gameRecordInfo = Options.Create(new GameRecordInfoOptions
        {
            GameProviderCode = "365TF",
            LockMinute = 10
        });
        var fakeClient = Substitute.For<IGameRecordHandleClient>();

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

        var service = new HandleRecordCommandService(
            gameRecordInfo,
            fakeClient);

        await service
            .ExecuteAsync(
                command)
            .ConfigureAwait(false);

        await fakeClient
            .Received(1)
            .MessageAsync(
            gameProviderCode: Arg.Is(gameRecordInfo.Value.GameProviderCode),
            externalRoundId: Arg.Is(command.Info.ExternalRoundId),
            externalGameId: Arg.Is(command.Info.ExternalGameId),
            playerCode: Arg.Is(command.Info.PlayerCode),
            status: Arg.Is(GameRecordStatus.Closed),
            clientStatus: Arg.Is(GameRecordClientStatus.Win),
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
        var gameRecordInfo = Options.Create(new GameRecordInfoOptions
        {
            GameProviderCode = "365TF",
            LockMinute = 10
        });
        var fakeClient = Substitute.For<IGameRecordHandleClient>();

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

        var service = new HandleRecordCommandService(
            gameRecordInfo,
            fakeClient);

        await service
            .ExecuteAsync(
                command)
            .ConfigureAwait(false);

        await fakeClient
            .Received(1)
            .MessageAsync(
            gameProviderCode: Arg.Is(gameRecordInfo.Value.GameProviderCode),
            externalRoundId: Arg.Is(command.Info.ExternalRoundId),
            externalGameId: Arg.Is(command.Info.ExternalGameId),
            playerCode: Arg.Is(command.Info.PlayerCode),
            status: Arg.Is(GameRecordStatus.Open),
            clientStatus: Arg.Is(GameRecordClientStatus.Open),
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