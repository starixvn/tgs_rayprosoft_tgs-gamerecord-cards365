using Tgs.GameRecord.Cards365.Core.Models;

namespace Tgs.GameRecord.Cards365.Core;

public interface IGameRecordScheduleRepository
{
	ValueTask<GameRecordScheduleInfo?> GetAsync(string website, string extension1, CancellationToken cancellationToken = default);

	ValueTask<GameRecordScheduleInfo> AddAsync(string website, string extension1, CancellationToken cancellationToken = default);

	ValueTask InitializationAsync(CancellationToken cancellationToken = default);

	ValueTask UpdateAsync(string website, string extension1, DateTimeOffset nextTime, string nextExternalBetId, CancellationToken cancellationToken = default);
}
