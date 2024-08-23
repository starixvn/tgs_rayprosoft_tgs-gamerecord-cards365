using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Tgs.GameRecord.Cards365.Common;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.Cards365.Controllers;

[Route("[controller]")]
[ApiController]
public class Cards365Controller : ControllerBase
{
	/// <summary>
	/// Games the record asynchronous.
	/// </summary>
	/// <param name="messageSender">The message sender.</param>
	/// <param name="source">The source.</param>
	/// <returns></returns>
	[HttpPost("manual")]
	public ValueTask GameRecordAsync(
		[FromServices] IMessageSender messageSender,
		[FromBody] Cards365GameRecordManualMessage source)
		=> messageSender.PublishAsync(
			subject: Cards365NatsConst.ManualPublish,
			data: JsonSerializer.SerializeToUtf8Bytes(source),
			cancellationToken: HttpContext.RequestAborted);

	/// <summary>
	/// Games the record schedule asynchronous.
	/// </summary>
	/// <param name="messageSender">The message sender.</param>
	/// <param name="source">The source.</param>
	/// <returns></returns>
	[HttpPost("schedule")]
	public ValueTask GameRecordScheduleAsync(
		[FromServices] IMessageSender messageSender,
		Cards365GameRecordScheduleMessage source)
		=> messageSender.PublishAsync(
			subject: Cards365NatsConst.SchedulePublish,
			data: JsonSerializer.SerializeToUtf8Bytes(source),
			cancellationToken: HttpContext.RequestAborted);
}
