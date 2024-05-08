using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Tgs.GameCommand.Client;

namespace Tgs.GameRecord.Cards365.Core;

internal class UtensilRepository(
	IOptions<GameRecordInfoOptions> options,
	IGameCommandClient client,
	IMemoryCache memoryCache) : IUtensil
{
	private readonly GameRecordInfoOptions _gameRecordInfo = options.Value;

	public async ValueTask<Guid> GetGameProviderIdAsync(
		CancellationToken cancellationToken = default)
	{
		var key = $"gamerecord:{_gameRecordInfo.GameProviderCode}";
		if (!memoryCache.TryGetValue(key, out Guid gameProviderId))
		{
			gameProviderId = await client
				.GameProviderGetGameProviderIdAsync(
					gameProviderCode: _gameRecordInfo.GameProviderCode,
					cancellationToken: cancellationToken)
				.ConfigureAwait(false)
				?? throw new Exception($"The identifier for this game provider does not exist.({_gameRecordInfo.GameProviderCode})");

			return memoryCache.Set(key, gameProviderId);
		}

		return gameProviderId;
	}
}