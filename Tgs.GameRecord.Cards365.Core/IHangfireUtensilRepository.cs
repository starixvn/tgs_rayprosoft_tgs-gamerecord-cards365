using Tgs.GameRecord.Cards365.Core.JobApplicationServices;

namespace Tgs.GameRecord.Cards365.Core;

public interface IHangfireUtensilRepository
{
	public Task JobCheckAsync();

	public Task JobExecuteAsync(JobExecuteCommand source);

	public Task JobInitAsync();
}
