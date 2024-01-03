using System.Text;
using System.Text.Json;
using Tgs.GameRecord.Cards365.Common;
using Tgs.GameRecord.Cards365.Core.ApplicationServices;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.Cards365.Messages;

internal class GameRecordScheduleMessageHandler(
    ILogger<GameRecordScheduleMessageHandler> logger,
    ICommandService<SchedulePullRecordCommand> service) : IMessageHandler
{
    public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var json = Encoding.UTF8.GetString(data.Span);
        var message = JsonSerializer.Deserialize<Cards365GameRecordScheduleMessage>(json)
            ?? throw new NullReferenceException("The request data resolves to a null value");

        logger.LogInformation("LogOn:{logOn} Information Request:{message}", DateTime.Now.ToString("o"), message);

        var command = new SchedulePullRecordCommand(
            ApiUrl: message.ApiUrl,
            Website: message.Website,
            Cert: message.Cert,
            Extension1: message.Extension1);

        return service.ExecuteAsync(
            command: command,
            cancellationToken: cancellationToken);
    }
}