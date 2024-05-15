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

		_logger.LogDebug("LogOn:{logOn} Message: GameRecord Unsettlement. GameRecords:{betGameWagerInfos}", DateTime.Now.ToString("o"), betGameWagerInfos);

		if (betGameWagerInfos.Any())
		{
			foreach (var betGameWagerInfo in betGameWagerInfos)
			{
				_logger.LogDebug("LogOn:{logOn} Message: GameRecord Unsettlement. GameRecords:{betGameWagerInfo}", DateTime.Now.ToString("o"), betGameWagerInfo);

				try
				{
					await _service
						.ExecuteAsync(
							command: new(betGameWagerInfo),
							cancellationToken: cancellationToken)
						.ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "LogOn:{logOn} Message: An exception occurred in this game record.({betGameWagerInfo})", DateTime.Now.ToString("o"), betGameWagerInfo);
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

		_logger.LogDebug("LogOn:{logOn} Message: GameRecord Settlement. GameRecords:{payGameWagerInfos}", DateTime.Now.ToString("o"), payGameWagerInfos);

		if (payGameWagerInfos.Any())
		{
			foreach (var payGameWagerInfo in payGameWagerInfos)
			{
				_logger.LogDebug("LogOn:{logOn} Message: GameRecord Settlement. GameRecords:{payGameWagerInfo}", DateTime.Now.ToString("o"), payGameWagerInfo);

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
					_logger.LogError(ex, "LogOn:{logOn} Message: An exception occurred in this game record.({payGameWagerInfo})", DateTime.Now.ToString("o"), payGameWagerInfo);
				}
			}
		}
	}
}
