namespace Tgs.GameRecord.Cards365.Core;

public interface ISystemClock
{
	DateTimeOffset CurrentTime { get; }
}
