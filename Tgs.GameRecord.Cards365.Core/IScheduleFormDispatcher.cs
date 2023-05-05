namespace Tgs.GameRecord.Cards365.Core;

public interface IScheduleFormDispatcher
{
    ValueTask ExecuteAsync(
        string gameProviderCode,
        long id,
        CancellationToken cancellationToken = default);
}