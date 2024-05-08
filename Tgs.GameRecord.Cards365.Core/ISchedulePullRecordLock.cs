namespace Tgs.GameRecord.Cards365.Core;

public interface ISchedulePullRecordLock
{
	public Task<bool> LockingAsync(string agentName);

	public Task<bool> UnlockAsync(string agentName);
}