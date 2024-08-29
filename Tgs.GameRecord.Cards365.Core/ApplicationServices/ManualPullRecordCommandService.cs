using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.GameInterface.Cards365.Client;
using Tgs.GameInterface.Cards365.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.Cards365.Core.ApplicationServices;

internal class ManualPullRecordCommandService(
	ILogger<ManualPullRecordCommandService> logger,
	IGameInterfaceCards365Client client,
	ICommandService<HandleRecordCommand> service) : ICommandService<ManualPullRecordCommand>
{
	private readonly IGameInterfaceCards365Client _client = client ?? throw new ArgumentNullException(nameof(client));
	private readonly ILogger<ManualPullRecordCommandService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
	private readonly ICommandService<HandleRecordCommand> _service = service ?? throw new ArgumentNullException(nameof(service));

	public async ValueTask ExecuteAsync(ManualPullRecordCommand command, CancellationToken cancellationToken = default)
	{
		var betGameWagerInfos = await _client.GameWagerByBetTimeAsync(
			apiUrl: command.ApiUrl,
			website: command.Website,
			cert: command.Cert,
			extension1: command.Extension1,
			startTime: command.StartTime,
			endTime: command.EndTime,
			cancellationToken: cancellationToken)
			.ToArrayAsync(
			cancellationToken: cancellationToken)
			.ConfigureAwait(false)
			?? Enumerable.Empty<GameWagerInfo>();

		if (betGameWagerInfos.Any())
		{
			foreach (var betGameWagerInfo in betGameWagerInfos)
			{
				try
				{
					await _service.ExecuteAsync(
						command: new(betGameWagerInfo),
						cancellationToken: cancellationToken)
						.ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "{logError}",
						JsonSerializer.Serialize(new
						{
							LogOn = DateTime.Now.ToString("u"),
							Data = new
							{
								betGameWagerInfo.PlayerCode,
								betGameWagerInfo.ExternalRoundId,
								betGameWagerInfo.ExternalWagerId,
								betGameWagerInfo.ExternalGameId,
								ExternalCreatedOn = betGameWagerInfo.ExternalCreatedOn.ToDateTimeOffset().ToString("u"),
								ExternalClosedOn = betGameWagerInfo.ExternalClosedOn.ToDateTimeOffset().ToString("u")
							},
							Message = "An exception occurred in this game wager."
						}));
				}
			}
		}

		var payGameWagerInfos = await _client.GameWagerByPayTimeAsync(
			apiUrl: command.ApiUrl,
			website: command.Website,
			cert: command.Cert,
			extension1: command.Extension1,
			startTime: command.StartTime,
			endTime: command.EndTime,
			cancellationToken: cancellationToken)
			.ToArrayAsync(
			cancellationToken: cancellationToken)
			.ConfigureAwait(false)
			?? Enumerable.Empty<GameWagerInfo>();

		if (payGameWagerInfos.Any())
		{
			foreach (var payGameWagerInfo in payGameWagerInfos)
			{
				try
				{
					await _service
						.ExecuteAsync(
							command: new(payGameWagerInfo),
							cancellationToken: cancellationToken)
						.ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "{logError}",
						JsonSerializer.Serialize(new
						{
							LogOn = DateTime.Now.ToString("u"),
							Data = new
							{
								payGameWagerInfo.PlayerCode,
								payGameWagerInfo.ExternalRoundId,
								payGameWagerInfo.ExternalWagerId,
								payGameWagerInfo.ExternalGameId,
								ExternalCreatedOn = payGameWagerInfo.ExternalCreatedOn.ToDateTimeOffset().ToString("u"),
								ExternalClosedOn = payGameWagerInfo.ExternalClosedOn.ToDateTimeOffset().ToString("u")
							},
							Message = "An exception occurred in this game wager."
						}));
				}
			}
		}
	}
}
