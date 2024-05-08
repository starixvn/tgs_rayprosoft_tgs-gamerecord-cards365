using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.GameInterface.Cards365.Protos;
using Tgs.GameRecord.Cards365.Core.ApplicationServices;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.Cards365.Core.UnitTest.ApplicationServices;

public class HandleRecordCommandServiceUnitTest
{
	[Fact]
	public async Task BetState_Loss()
	{
		var faceLogger = NullLogger<HandleRecordCommandService>.Instance;
		var gameRecordInfo = Options.Create(new GameRecordInfoOptions
		{
			GameProviderCode = "365TF",
			LockMinute = 10
		});
		var fakeMessageSender = Substitute.For<IMessageSender>();

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

		var sut = new HandleRecordCommandService(
			faceLogger,
			gameRecordInfo,
			fakeMessageSender);

		await sut.ExecuteAsync(command);

		_ = fakeMessageSender
			.Received(1)
			.PublishAsync(
			subject: Arg.Is(Handle.Common.GameRecordHandleNatsConst.GameRecordHandleRequest),
			data: Arg.Is(new Handle.Common.Protos.GameRecordHandleRequest
			{
				GameProviderCode = gameRecordInfo.Value.GameProviderCode,
				PlayerCode = command.Info.PlayerCode,
				ExternalRoundId = command.Info.ExternalRoundId,
				ExternalBetId = command.Info.ExternalWagerId,
				ExternalGameId = command.Info.ExternalGameId,
				ExternalCreatedOn = command.Info.ExternalCreatedOn,
				ExternalClosedOn = command.Info.ExternalClosedOn,
				ExternalPayoffOn = command.Info.ExternalClosedOn,
				ExternalResettleOn = DateTimeOffset.UnixEpoch.ToTimestamp(),
				BetState = Handle.Common.Protos.BetState.Loss,
				BetType = Handle.Common.Protos.BetType.Normal,
				FreeGameState = Handle.Common.Protos.FreeGameState.None,
				GambleState = Handle.Common.Protos.GambleState.None,
				CashoutState = Handle.Common.Protos.CashoutState.None,
				ResettleState = Handle.Common.Protos.ResettleState.None,
				JackpotState = Handle.Common.Protos.JackpotState.None,
				Bet = 28.6m,
				ValidBet = 28.6m,
				Settle = 0.0M,
				Commission = 0.0m,
				Turnover = 28.6m,
				WinLoss = -28.6m,
				Jackpot = 0.0M,
				AdditionalData = command.Info.AdditionalData
			}),
			cancellationToken: Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task BetState_Win()
	{
		var faceLogger = NullLogger<HandleRecordCommandService>.Instance;
		var gameRecordInfo = Options.Create(new GameRecordInfoOptions
		{
			GameProviderCode = "365TF",
			LockMinute = 10
		});
		var fakeMessageSender = Substitute.For<IMessageSender>();

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

		var sut = new HandleRecordCommandService(
			faceLogger,
			gameRecordInfo,
			fakeMessageSender);

		await sut.ExecuteAsync(command);

		_ = fakeMessageSender
			.Received(1)
			.PublishAsync(
			subject: Arg.Is(Handle.Common.GameRecordHandleNatsConst.GameRecordHandleRequest),
			data: Arg.Is(new Handle.Common.Protos.GameRecordHandleRequest
			{
				GameProviderCode = gameRecordInfo.Value.GameProviderCode,
				PlayerCode = command.Info.PlayerCode,
				ExternalRoundId = command.Info.ExternalRoundId,
				ExternalBetId = command.Info.ExternalWagerId,
				ExternalGameId = command.Info.ExternalGameId,
				ExternalCreatedOn = command.Info.ExternalCreatedOn,
				ExternalClosedOn = command.Info.ExternalClosedOn,
				ExternalPayoffOn = command.Info.ExternalClosedOn,
				ExternalResettleOn = DateTimeOffset.UnixEpoch.ToTimestamp(),
				BetState = Handle.Common.Protos.BetState.Win,
				BetType = Handle.Common.Protos.BetType.Normal,
				FreeGameState = Handle.Common.Protos.FreeGameState.None,
				GambleState = Handle.Common.Protos.GambleState.None,
				CashoutState = Handle.Common.Protos.CashoutState.None,
				ResettleState = Handle.Common.Protos.ResettleState.None,
				JackpotState = Handle.Common.Protos.JackpotState.None,
				Bet = 60.82m,
				ValidBet = 62.7m,
				Settle = 121.64m,
				Commission = 1.88m,
				Turnover = 62.7m,
				WinLoss = 60.82m,
				Jackpot = 0.0M,
				AdditionalData = command.Info.AdditionalData
			}),
			cancellationToken: Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task BetState_Running()
	{
		var faceLogger = NullLogger<HandleRecordCommandService>.Instance;
		var gameRecordInfo = Options.Create(new GameRecordInfoOptions
		{
			GameProviderCode = "365TF",
			LockMinute = 10
		});
		var fakeMessageSender = Substitute.For<IMessageSender>();

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

		var sut = new HandleRecordCommandService(
			faceLogger,
			gameRecordInfo,
			fakeMessageSender);

		await sut.ExecuteAsync(command);

		_ = fakeMessageSender
			.Received(1)
			.PublishAsync(
			subject: Arg.Is(Handle.Common.GameRecordHandleNatsConst.GameRecordHandleRequest),
			data: Arg.Is(new Handle.Common.Protos.GameRecordHandleRequest
			{
				GameProviderCode = gameRecordInfo.Value.GameProviderCode,
				PlayerCode = command.Info.PlayerCode,
				ExternalRoundId = command.Info.ExternalRoundId,
				ExternalBetId = command.Info.ExternalWagerId,
				ExternalGameId = command.Info.ExternalGameId,
				ExternalCreatedOn = command.Info.ExternalCreatedOn,
				ExternalClosedOn = DateTimeOffset.UnixEpoch.ToTimestamp(),
				ExternalPayoffOn = DateTimeOffset.UnixEpoch.ToTimestamp(),
				ExternalResettleOn = DateTimeOffset.UnixEpoch.ToTimestamp(),
				BetState = Handle.Common.Protos.BetState.Running,
				BetType = Handle.Common.Protos.BetType.Normal,
				FreeGameState = Handle.Common.Protos.FreeGameState.None,
				GambleState = Handle.Common.Protos.GambleState.None,
				CashoutState = Handle.Common.Protos.CashoutState.None,
				ResettleState = Handle.Common.Protos.ResettleState.None,
				JackpotState = Handle.Common.Protos.JackpotState.None,
				Bet = 60.82m,
				ValidBet = 62.7m,
				Settle = 0.0M,
				Commission = 0.0M,
				Turnover = 0.0M,
				WinLoss = 0.0M,
				Jackpot = 0.0M,
				AdditionalData = command.Info.AdditionalData
			}),
			cancellationToken: Arg.Any<CancellationToken>());
	}
}