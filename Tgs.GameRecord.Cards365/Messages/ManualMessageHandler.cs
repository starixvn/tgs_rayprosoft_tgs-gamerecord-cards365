using System.Text;
using System.Text.Json;
using Tgs.GameRecord.Cards365.Common;
using Tgs.GameRecord.Cards365.Core;
using Tgs.GameRecord.Cards365.Core.ApplicationServices;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.Cards365.Messages;

internal class ManualMessageHandler(
	ILogger<ManualMessageHandler> logger,
	IManualPullRecordDispatcher dispatcher) : IMessageHandler<ReadOnlyMemory<byte>>
{
	public ValueTask HandleAsync(string subject, ReadOnlyMemory<byte> data, IEnumerable<MessageHeaderValue>? headerValues, CancellationToken cancellationToken = default)
	{
		var json = Encoding.UTF8.GetString(data.Span);
		var message = JsonSerializer.Deserialize<Cards365GameRecordManualMessage>(json)
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
					message.Extension1,
					StartTime = message.StartTime.ToString("u"),
					EndTime = message.EndTime.ToString("u"),
				},
				Message = "This receive message handler."
			}));

		return dispatcher.PushCommandAsync(
			command: new ManualPullRecordCommand(
				ApiUrl: message.ApiUrl,
				Website: message.Website,
				Cert: message.Cert,
				Extension1: message.Extension1,
				StartTime: message.StartTime,
				EndTime: message.EndTime),
			cancellationToken: cancellationToken);
	}
}
