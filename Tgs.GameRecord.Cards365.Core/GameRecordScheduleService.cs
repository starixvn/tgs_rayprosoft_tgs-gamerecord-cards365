using System.Text.Json;
using Microsoft.Extensions.Options;
using Tgs.GameRecordSchedules.Client;
using Tgs.GameRecordSchedules.Protos;

namespace Tgs.GameRecord.Cards365.Core;

internal class GameRecordScheduleService(
	IGameRecordSchedulesClient client,
	IOptions<GameRecordInfoOptions> options) : IGameRecordScheduleService
{
	private readonly GameRecordInfoOptions _gameRecordInfo = options.Value;

	public ValueTask<GameRecordScheduleReply?> GetAsync(
		string extension1,
		CancellationToken cancellationToken = default)
	{
		var request = new GameRecordScheduleGetRequest
		{
			GameProviderCode = _gameRecordInfo.GameProviderCode,
			BrandCode = string.Empty,
			GameProviderAgentId = extension1,
			CurrencyCode = string.Empty
		};

		return client.GameRecordScheduleGetAsync(request, cancellationToken);
	}

	public ScheduleConfig GetConfig(GameRecordScheduleReply gameRecordSchedule)
		=> JsonSerializer.Deserialize<ScheduleConfig>(gameRecordSchedule.Config)!;

	public async ValueTask<GameRecordScheduleReply?> InitAsync(
		Guid gameProviderId,
		string extension1,
		CancellationToken cancellationToken = default)
	{
		var nextTime = DateTimeOffset.UnixEpoch;

		var config = new ScheduleConfig
		{
			NextTime = nextTime,
			NextExternalWagerId = string.Empty
		};

		var addRequest = new GameRecordScheduleAddRequest
		{
			GameProviderId = gameProviderId,
			GameProviderCode = _gameRecordInfo.GameProviderCode,
			BrandCode = string.Empty,
			GameProviderAgentId = extension1,
			CurrencyCode = string.Empty,
			Config = JsonSerializer.Serialize(config)
		};

		_ = await client.GameRecordScheduleAddAsync(addRequest, cancellationToken).ConfigureAwait(false);

		return await GetAsync(extension1, cancellationToken).ConfigureAwait(false);
	}

	public async ValueTask<GameRecordScheduleReply?> UpdateAsync(
		GameRecordScheduleReply gameRecordSchedule,
		string extension1,
		DateTimeOffset nextTime,
		string nextExternalWagerId,
		CancellationToken cancellationToken = default)
	{
		var config = GetConfig(gameRecordSchedule);

		config.NextTime = nextTime;
		config.NextExternalWagerId = nextExternalWagerId;

		gameRecordSchedule.Config = JsonSerializer.Serialize(config);

		var request = new GameRecordScheduleUpdateRequest
		{
			GameRecordScheduleId = gameRecordSchedule.GameRecordScheduleId,
			Config = JsonSerializer.Serialize(config)
		};

		var updateResult = await client
			.GameRecordScheduleUpdateAsync(request, cancellationToken).ConfigureAwait(false);

		return updateResult.Success
			? await GetAsync(extension1, cancellationToken).ConfigureAwait(false)
			: null;
	}
}
