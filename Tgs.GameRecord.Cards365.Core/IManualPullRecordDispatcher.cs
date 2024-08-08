using Tgs.GameRecord.Cards365.Core.ApplicationServices;

namespace Tgs.GameRecord.Cards365.Core;

public interface IManualPullRecordDispatcher
{
	ValueTask PushCommandAsync(ManualPullRecordCommand command, CancellationToken cancellationToken = default);
}
