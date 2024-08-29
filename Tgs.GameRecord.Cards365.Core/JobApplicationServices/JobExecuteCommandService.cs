using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tgs.GameRecord.Cards365.Common;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.Cards365.Core.JobApplicationServices;

internal class JobExecuteCommandService(
	ILogger<JobExecuteCommandService> logger,
	IMessageSender messageSender) : ICommandService<JobExecuteCommand>
{
	public ValueTask ExecuteAsync(JobExecuteCommand command, CancellationToken cancellationToken = default)
	{
		var message = new Cards365GameRecordScheduleMessage
		{
			ApiUrl = command.ApiUrl,
			Website = command.Website,
			Cert = command.Cert,
			Extension1 = command.Extension1
		};

		logger.LogInformation("{logInformation}",
			JsonSerializer.Serialize(new
			{
				LogOn = DateTime.Now.ToString("u"),
				Subject = Cards365NatsConst.SchedulePublish,
				Data = new
				{
					message.ApiUrl,
					message.Website,
					message.Cert,
					message.Extension1
				},
				Message = "This message publishing handler."
			}));

		return messageSender.PublishAsync(
			subject: Cards365NatsConst.SchedulePublish,
			data: JsonSerializer.SerializeToUtf8Bytes(message),
			cancellationToken: cancellationToken);
	}
}
