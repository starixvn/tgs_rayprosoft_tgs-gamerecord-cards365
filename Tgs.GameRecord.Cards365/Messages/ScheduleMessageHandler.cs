using System.Text;
using System.Text.Json;
using Tgs.GameRecord.Cards365.Common;
using Tgs.GameRecord.Cards365.Core;
using Tgs.GameRecord.Cards365.Core.ApplicationServices;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.Cards365.Messages;

internal class ScheduleMessageHandler(
	ILogger<ScheduleMessageHandler> logger,
	ISchedulePullRecordDispatcher dispatcher) : IMessageHandler<ReadOnlyMemory<byte>>
{
	public ValueTask HandleAsync(string subject, ReadOnlyMemory<byte> data, IEnumerable<MessageHeaderValue>? headerValues, CancellationToken cancellationToken = default)
	{
		var json = Encoding.UTF8.GetString(data.Span);
		var message = JsonSerializer.Deserialize<Cards365GameRecordScheduleMessage>(json)
			?? throw new NullReferenceException("The request data resolves to a null value");

		logger.LogInformation("{logInformation}",
			JsonSerializer.Serialize(new
			{
				LogOn = DateTime.Now.ToString("u"),
				Subject = subject,
				Data = new
				{
					message.ApiUrl,
					message.Website,
					message.Cert,
					message.Extension1
				},
				Message = "This receive message handler."
			}));

		return dispatcher.PushCommandAsync(
			command: new SchedulePullRecordCommand(
				ApiUrl: message.ApiUrl,
				Website: message.Website,
				Cert: message.Cert,
				Extension1: message.Extension1),
			cancellationToken: cancellationToken);
	}
}
