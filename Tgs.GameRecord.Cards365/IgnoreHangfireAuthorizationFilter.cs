using Hangfire.Dashboard;

namespace Tgs.GameRecord.Cards365;

public class IgnoreHangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
	public bool Authorize(DashboardContext context) => true;
}