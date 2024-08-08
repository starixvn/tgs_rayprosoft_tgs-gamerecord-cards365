using Tgs.GameRecord.Cards365.Core.ApplicationServices;

namespace Tgs.GameRecord.Cards365.Core;

public interface ISchedulePullRecordDispatcher
{
	ValueTask PushCommandAsync(SchedulePullRecordCommand command, CancellationToken cancellationToken = default);
}
