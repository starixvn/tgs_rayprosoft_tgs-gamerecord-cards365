namespace Tgs.GameRecord.Cards365.Core;

public interface IUtensil
{
    ValueTask<Guid> GetGameProviderIdAsync(
        CancellationToken cancellationToken = default);
}