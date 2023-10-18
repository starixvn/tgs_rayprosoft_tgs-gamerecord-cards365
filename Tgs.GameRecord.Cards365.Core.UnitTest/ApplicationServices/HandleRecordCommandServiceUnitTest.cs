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
            Info: new GameWagerInfo
            {
                ExternalRoundId = "449-1697559479-532463-1",
                ExternalWagerId = "bd606f6c6d0811eebfe04cd98f535a89",
                ExternalGameId = "449",
                PlayerCode = "4ujhc2phm6bd",
                IsClosed = true,
                IsVoid = false,
                UpdateTime = DateTimeOffset.Parse("2023-10-18T00:18:30.157+08:00").ToTimestamp(),
                ExternalCreatedOn = DateTimeOffset.Parse("2023-10-18T00:18:06.892+08:00").ToTimestamp(),
                ExternalClosedOn = DateTimeOffset.Parse("2023-10-18T00:18:30.072+08:00").ToTimestamp(),
                Bet = 28.6m,
                ValidBet = 28.6m,
                Settle = 0,
                Winloss = -28.6m,
                Commission = 0,
                AdditionalData = "{\"txId\":\"bd606f6c6d0811eebfe04cd98f535a89\",\"gameNumber\":\"449-1697559479-532463-1\",\"userId\":\"4ujhc2phm6bd\",\"extension1\":\"cardp121\",\"status\":\"Pay\",\"tiles\":\"[a1, a3, a4, a5, a6, a7, a8, a8, a9, d5, d5, e4, e4]\",\"gameId\":449,\"chair\":0,\"playId\":1,\"commission\":0,\"banker\":0,\"profit\":-28.6,\"validbet\":28.6,\"bonus\":0,\"reward\":0,\"outMoney\":0,\"currency\":\"HKD\",\"updateTime\":\"10/18/2023 00:18:30.157\",\"createTime\":\"10/18/2023 00:18:06.952\",\"betTransTime\":\"10/18/2023 00:18:06.892\",\"payTransTime\":\"10/18/2023 00:18:30.072\",\"betAmount\":1310.99,\"realBetAmount\":28.6,\"payAmount\":1282.39,\"realPayAmount\":0}"
            });

        var service = new HandleRecordCommandService(
            gameRecordInfo,
            fakeClient);

        await service.ExecuteAsync(command);

        await fakeClient
            .Received(1)
            .MessageAsync(
            gameProviderCode: Arg.Is(gameRecordInfo.Value.GameProviderCode),
            playerCode: Arg.Is(command.Info.PlayerCode),
            wagerState: Arg.Is(WagerStates.Lose),
            wagerType: Arg.Is(WagerTypes.Normal),
            freeGameState: Arg.Is(FreeGameStates.None),
            gambleState: Arg.Is(GambleStates.None),
            cashoutState: Arg.Is(CashoutStates.None),
            resettleState: Arg.Is(ResettleStates.None),
            externalRoundId: Arg.Is(command.Info.ExternalRoundId),
            externalWagerId: Arg.Is(command.Info.ExternalWagerId),
            externalGameId: Arg.Is(command.Info.ExternalGameId),
            externalCreatedOn: Arg.Is(command.Info.ExternalCreatedOn.ToDateTimeOffset()),
            externalClosedOn: Arg.Is(command.Info.ExternalClosedOn.ToDateTimeOffset()),
            externalPayoffOn: Arg.Is(command.Info.ExternalClosedOn.ToDateTimeOffset()),
            externalResettleOn: Arg.Is(DateTimeOffset.UnixEpoch),
            bet: Arg.Is(command.Info.Bet.ToDecimal().GetValueOrDefault(0)),
            validBet: Arg.Is(command.Info.ValidBet.ToDecimal().GetValueOrDefault(0)),
            settle: Arg.Is(command.Info.Settle.ToDecimal().GetValueOrDefault(0)),
            commission: Arg.Is(command.Info.Commission.ToDecimal().GetValueOrDefault(0)),
            turnover: Arg.Is(command.Info.ValidBet.ToDecimal().GetValueOrDefault(0)),
            winLoss: Arg.Is(command.Info.Winloss.ToDecimal().GetValueOrDefault(0)),
            additionalData: Arg.Is(command.Info.AdditionalData),
            cancellationToken: Arg.Any<CancellationToken>());
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
            Info: new GameWagerInfo
            {
                ExternalRoundId = "449-1697559511-395695-1",
                ExternalWagerId = "d048f1806d0811eebfe04cd98f535a89",
                ExternalGameId = "449",
                PlayerCode = "4ujhc2phm6bd",
                IsClosed = true,
                IsVoid = false,
                UpdateTime = DateTimeOffset.Parse("2023-10-18T00:19:40.650+08:00").ToTimestamp(),
                ExternalCreatedOn = DateTimeOffset.Parse("2023-10-18T00:18:38.090+08:00").ToTimestamp(),
                ExternalClosedOn = DateTimeOffset.Parse("2023-10-18T00:19:40.568+08:00").ToTimestamp(),
                Bet = 60.82m,
                ValidBet = 62.7m,
                Settle = 121.64m,
                Winloss = 60.82m,
                Commission = 1.88m,
                AdditionalData = "{\"txId\":\"d048f1806d0811eebfe04cd98f535a89\",\"gameNumber\":\"449-1697559511-395695-1\",\"userId\":\"4ujhc2phm6bd\",\"extension1\":\"cardp121\",\"status\":\"Pay\",\"tiles\":\"[a8, a8, a9, a9, d1, d1, d5, d5, d9, d9, e4, e4, e6, e6]\",\"gameId\":449,\"chair\":0,\"playId\":1,\"commission\":1.88,\"banker\":1,\"profit\":60.82,\"validbet\":60.82,\"bonus\":0,\"reward\":0,\"outMoney\":0,\"currency\":\"HKD\",\"updateTime\":\"10/18/2023 00:19:40.650\",\"createTime\":\"10/18/2023 00:18:38.150\",\"betTransTime\":\"10/18/2023 00:18:38.090\",\"payTransTime\":\"10/18/2023 00:19:40.568\",\"betAmount\":1282.39,\"realBetAmount\":62.7,\"payAmount\":1343.21,\"realPayAmount\":62.7}"
            });

        var service = new HandleRecordCommandService(
            gameRecordInfo,
            fakeClient);

        await service.ExecuteAsync(command);

        await fakeClient
            .Received(1)
            .MessageAsync(
            gameProviderCode: Arg.Is(gameRecordInfo.Value.GameProviderCode),
            playerCode: Arg.Is(command.Info.PlayerCode),
            wagerState: Arg.Is(WagerStates.Win),
            wagerType: Arg.Is(WagerTypes.Normal),
            freeGameState: Arg.Is(FreeGameStates.None),
            gambleState: Arg.Is(GambleStates.None),
            cashoutState: Arg.Is(CashoutStates.None),
            resettleState: Arg.Is(ResettleStates.None),
            externalRoundId: Arg.Is(command.Info.ExternalRoundId),
            externalWagerId: Arg.Is(command.Info.ExternalWagerId),
            externalGameId: Arg.Is(command.Info.ExternalGameId),
            externalCreatedOn: Arg.Is(command.Info.ExternalCreatedOn.ToDateTimeOffset()),
            externalClosedOn: Arg.Is(command.Info.ExternalClosedOn.ToDateTimeOffset()),
            externalPayoffOn: Arg.Is(command.Info.ExternalClosedOn.ToDateTimeOffset()),
            externalResettleOn: Arg.Is(DateTimeOffset.UnixEpoch),
            bet: Arg.Is(command.Info.Bet.ToDecimal().GetValueOrDefault(0)),
            validBet: Arg.Is(command.Info.ValidBet.ToDecimal().GetValueOrDefault(0)),
            settle: Arg.Is(command.Info.Settle.ToDecimal().GetValueOrDefault(0)),
            commission: Arg.Is(command.Info.Commission.ToDecimal().GetValueOrDefault(0)),
            turnover: Arg.Is(command.Info.ValidBet.ToDecimal().GetValueOrDefault(0)),
            winLoss: Arg.Is(command.Info.Winloss.ToDecimal().GetValueOrDefault(0)),
            additionalData: Arg.Is(command.Info.AdditionalData),
            cancellationToken: Arg.Any<CancellationToken>());
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
            Info: new GameWagerInfo
            {
                ExternalRoundId = "449-1697559511-395695-1",
                ExternalWagerId = "d048f1806d0811eebfe04cd98f535a89",
                ExternalGameId = "449",
                PlayerCode = "4ujhc2phm6bd",
                IsClosed = false,
                IsVoid = false,
                UpdateTime = DateTimeOffset.Parse("2023-10-18T00:19:40.650+08:00").ToTimestamp(),
                ExternalCreatedOn = DateTimeOffset.Parse("2023-10-18T00:18:38.090+08:00").ToTimestamp(),
                ExternalClosedOn = DateTimeOffset.UnixEpoch.ToTimestamp(),
                Bet = 60.82m,
                ValidBet = 62.7m,
                Settle = 0.0m,
                Winloss = 0.0m,
                Commission = 0.0m,
                AdditionalData = "{\"txId\":\"d048f1806d0811eebfe04cd98f535a89\",\"gameNumber\":\"449-1697559511-395695-1\"}"
            });

        var service = new HandleRecordCommandService(
            gameRecordInfo,
            fakeClient);

        await service.ExecuteAsync(command);

        await fakeClient
            .Received(1)
            .MessageAsync(
            gameProviderCode: Arg.Is(gameRecordInfo.Value.GameProviderCode),
            playerCode: Arg.Is(command.Info.PlayerCode),
            wagerState: Arg.Is(WagerStates.Running),
            wagerType: Arg.Is(WagerTypes.Normal),
            freeGameState: Arg.Is(FreeGameStates.None),
            gambleState: Arg.Is(GambleStates.None),
            cashoutState: Arg.Is(CashoutStates.None),
            resettleState: Arg.Is(ResettleStates.None),
            externalRoundId: Arg.Is(command.Info.ExternalRoundId),
            externalWagerId: Arg.Is(command.Info.ExternalWagerId),
            externalGameId: Arg.Is(command.Info.ExternalGameId),
            externalCreatedOn: Arg.Is(command.Info.ExternalCreatedOn.ToDateTimeOffset()),
            externalClosedOn: Arg.Is(DateTimeOffset.UnixEpoch),
            externalPayoffOn: Arg.Is(DateTimeOffset.UnixEpoch),
            externalResettleOn: Arg.Is(DateTimeOffset.UnixEpoch),
            bet: Arg.Is(command.Info.Bet.ToDecimal().GetValueOrDefault(0)),
            validBet: Arg.Is(command.Info.ValidBet.ToDecimal().GetValueOrDefault(0)),
            settle: Arg.Is(0.0m),
            commission: Arg.Is(0.0m),
            turnover: Arg.Is(0.0m),
            winLoss: Arg.Is(0.0m),
            additionalData: Arg.Is(command.Info.AdditionalData),
            cancellationToken: Arg.Any<CancellationToken>());
    }
}