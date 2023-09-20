using Microsoft.Extensions.Logging;
using Tgs.GameRounds.Schedule.Client;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.Cards365.Core.JobApplicationServices;

internal class JobExecuteCommandService : ICommandService<JobExecuteCommand>
{
    private readonly IGameRoundsScheduleClient _client;
    private readonly ILogger<JobExecuteCommandService> _logger;

    public JobExecuteCommandService(
        ILogger<JobExecuteCommandService> logger,
        IGameRoundsScheduleClient client)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public ValueTask ExecuteAsync(JobExecuteCommand command, CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            command.GameProviderCode,
            command.Name,
            command.ApiUrl,
            command.Website,
            command.Cert,
            command.Extension1
        };

        _logger.LogDebug("LogOn:{logOn} Parameters:{parameters}", DateTime.Now.ToString("u"), parameters);

        return _client
            .Cards365_ScheduleFormQueuedAsync(
            gameProviderCode: command.GameProviderCode,
            name: command.Name,
            subject: "gp.gamerecord.scheduleform.365tf.queued",
            apiUrl: parameters.ApiUrl,
            website: parameters.Website,
            cert: parameters.Cert,
            extension1: parameters.Extension1,
            cancellationToken: cancellationToken);
    }
}