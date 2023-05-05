using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Tgs.GameRecord.Cards365.Core.ApplicationServices;
using Tgs.GameRounds.Schedule.Client;
using Tgs.GameRounds.Schedule.Common.Protos;
using Tgs.GameRounds.Schedule.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.Cards365.Core.UnitTest;

public class ScheduleFormDispatcherUnitTest
{
    [Fact]
    public async Task Exception_Occurs()
    {
        var fakeLogger = NullLogger<ScheduleFormDispatcher>.Instance;
        var fakeClient = Substitute.For<IGameRoundsScheduleClient>();
        var fakeScheduleFormLock = Substitute.For<IScheduleFormLock>();
        var fakeSchedulePullRecord = Substitute.For<ICommandService<SchedulePullRecordCommand>>();
        var fakeManualPullRecord = Substitute.For<ICommandService<ManualPullRecordCommand>>();

        var gameProviderCode = "365TF";
        var id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var parameter = new Cards365Parameter
        {
            ApiUrl = "http://localhost/",
            Website = "website",
            Cert = "cert",
            Extension1 = "extension1"
        };
        var scheduleFormReply = new ScheduleFormReply
        {
            Id = id,
            GameProviderId = Guid.NewGuid(),
            GameProviderCode = gameProviderCode,
            Name = "test",
            Subject = "gp.gamerecord.scheduleform.365tf.queued",
            Source = ScheduleFormSource.Schedule,
            Parameters = JsonSerializer.Serialize(parameter),
            State = ScheduleFormState.Queued,
            Status = $"{ScheduleFormState.Queued}..."
        };
        _ = fakeClient
            .ScheduleFormGetAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(scheduleFormReply);

        _ = fakeScheduleFormLock
            .LockingAsync(
                id: Arg.Is(id))
            .Returns(true);

        var scheduleCommand = new SchedulePullRecordCommand(
            ApiUrl: new Uri(parameter.ApiUrl),
            Website: parameter.Website,
            Cert: parameter.Cert,
            Extension1: parameter.Extension1);

        _ = fakeSchedulePullRecord
           .ExecuteAsync(
               command: Arg.Is(scheduleCommand),
               cancellationToken: Arg.Any<CancellationToken>())
           .Throws<Exception>();

        var dispatcher = new ScheduleFormDispatcher(
            fakeLogger,
            fakeClient,
            fakeScheduleFormLock,
            fakeSchedulePullRecord,
            fakeManualPullRecord);

        await dispatcher
            .ExecuteAsync(
            gameProviderCode: gameProviderCode,
                id: id)
            .ConfigureAwait(false);

        await fakeClient
            .Received(1)
            .ScheduleFormProcessingAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        await fakeClient
            .Received(0)
            .ScheduleFormCompleteAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        await fakeClient
            .Received(1)
            .ScheduleFormFaileAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                status: Arg.Any<string>(),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        _ = await fakeScheduleFormLock
            .Received(1)
            .UnlockAsync(
                id: Arg.Is(id))
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task Normal_Process_Manual()
    {
        var fakeLogger = NullLogger<ScheduleFormDispatcher>.Instance;
        var fakeClient = Substitute.For<IGameRoundsScheduleClient>();
        var fakeScheduleFormLock = Substitute.For<IScheduleFormLock>();
        var fakeSchedulePullRecord = Substitute.For<ICommandService<SchedulePullRecordCommand>>();
        var fakeManualPullRecord = Substitute.For<ICommandService<ManualPullRecordCommand>>();

        var gameProviderCode = "365TF";
        var id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var parameter = new Cards365ManualParameter
        {
            ApiUrl = "http://localhost/",
            Website = "website",
            Cert = "cert",
            Extension1 = "extension1",
            StartTime = DateTimeOffset.UtcNow.AddMinutes(-10).ToTimestamp(),
            EndTime = DateTimeOffset.UtcNow.ToTimestamp(),
        };
        var scheduleFormReply = new ScheduleFormReply
        {
            Id = id,
            GameProviderId = Guid.NewGuid(),
            GameProviderCode = gameProviderCode,
            Name = "test",
            Subject = "gp.gamerecord.scheduleform.365TF.queued",
            Source = ScheduleFormSource.Manual,
            Parameters = JsonSerializer.Serialize(parameter),
            State = ScheduleFormState.Queued,
            Status = $"{ScheduleFormState.Queued}..."
        };
        _ = fakeClient
            .ScheduleFormGetAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(scheduleFormReply);

        _ = fakeScheduleFormLock
            .LockingAsync(
                id: Arg.Is(id))
            .Returns(true);

        var dispatcher = new ScheduleFormDispatcher(
            fakeLogger,
            fakeClient,
            fakeScheduleFormLock,
            fakeSchedulePullRecord,
            fakeManualPullRecord);

        await dispatcher
            .ExecuteAsync(
                 gameProviderCode: gameProviderCode,
                id: id)
            .ConfigureAwait(false);

        var scheduleCommand = new SchedulePullRecordCommand(
            ApiUrl: new Uri(parameter.ApiUrl),
            Website: parameter.Website,
            Cert: parameter.Cert,
            Extension1: parameter.Extension1);

        await fakeSchedulePullRecord
            .Received(0)
            .ExecuteAsync(
                command: Arg.Is(scheduleCommand),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        var manualCommand = new ManualPullRecordCommand(
            ApiUrl: new Uri(parameter.ApiUrl),
            Website: parameter.Website,
            Cert: parameter.Cert,
            Extension1: parameter.Extension1,
            StartTime: parameter.StartTime.ToDateTimeOffset(),
            EndTime: parameter.EndTime.ToDateTimeOffset());

        await fakeManualPullRecord
            .Received(1)
            .ExecuteAsync(
                command: Arg.Is(manualCommand),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        await fakeClient
            .Received(1)
            .ScheduleFormProcessingAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        await fakeClient
            .Received(1)
            .ScheduleFormCompleteAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        _ = await fakeScheduleFormLock
            .Received(1)
            .UnlockAsync(
                id: Arg.Is(id))
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task Normal_Process_Schedule()
    {
        var fakeLogger = NullLogger<ScheduleFormDispatcher>.Instance;
        var fakeClient = Substitute.For<IGameRoundsScheduleClient>();
        var fakeScheduleFormLock = Substitute.For<IScheduleFormLock>();
        var fakeSchedulePullRecord = Substitute.For<ICommandService<SchedulePullRecordCommand>>();
        var fakeManualPullRecord = Substitute.For<ICommandService<ManualPullRecordCommand>>();

        var gameProviderCode = "365TF";
        var id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var parameter = new Cards365Parameter
        {
            ApiUrl = "http://localhost/",
            Website = "website",
            Cert = "cert",
            Extension1 = "extension1"
        };
        var scheduleFormReply = new ScheduleFormReply
        {
            Id = id,
            GameProviderId = Guid.NewGuid(),
            GameProviderCode = gameProviderCode,
            Name = "test",
            Subject = "gp.gamerecord.scheduleform.365TF.queued",
            Source = ScheduleFormSource.Schedule,
            Parameters = JsonSerializer.Serialize(parameter),
            State = ScheduleFormState.Queued,
            Status = $"{ScheduleFormState.Queued}..."
        };
        _ = fakeClient
            .ScheduleFormGetAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(scheduleFormReply);

        _ = fakeScheduleFormLock
            .LockingAsync(
                id: Arg.Is(id))
            .Returns(true);

        var dispatcher = new ScheduleFormDispatcher(
            fakeLogger,
            fakeClient,
            fakeScheduleFormLock,
            fakeSchedulePullRecord,
            fakeManualPullRecord);

        await dispatcher
            .ExecuteAsync(
                 gameProviderCode: gameProviderCode,
                id: id)
            .ConfigureAwait(false);

        var scheduleCommand = new SchedulePullRecordCommand(
            ApiUrl: new Uri(parameter.ApiUrl),
            Website: parameter.Website,
            Cert: parameter.Cert,
            Extension1: parameter.Extension1);

        await fakeSchedulePullRecord
            .Received(1)
            .ExecuteAsync(
                command: Arg.Is(scheduleCommand),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        var manualCommand = new ManualPullRecordCommand(
            ApiUrl: new Uri(parameter.ApiUrl),
            Website: parameter.Website,
            Cert: parameter.Cert,
            Extension1: parameter.Extension1,
            StartTime: Arg.Any<DateTimeOffset>(),
            EndTime: Arg.Any<DateTimeOffset>());

        await fakeManualPullRecord
            .Received(0)
            .ExecuteAsync(
                command: Arg.Is(manualCommand),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        await fakeClient
            .Received(1)
            .ScheduleFormProcessingAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        await fakeClient
            .Received(1)
            .ScheduleFormCompleteAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        _ = await fakeScheduleFormLock
            .Received(1)
            .UnlockAsync(
                id: Arg.Is(id))
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task Schedule_Form_State_Is_Not_Queued()
    {
        var fakeLogger = NullLogger<ScheduleFormDispatcher>.Instance;
        var fakeClient = Substitute.For<IGameRoundsScheduleClient>();
        var fakeScheduleFormLock = Substitute.For<IScheduleFormLock>();
        var fakeSchedulePullRecord = Substitute.For<ICommandService<SchedulePullRecordCommand>>();
        var fakeManualPullRecord = Substitute.For<ICommandService<ManualPullRecordCommand>>();

        var gameProviderCode = "365TF";
        var id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var parameter = new Cards365Parameter
        {
            ApiUrl = "http://localhost/",
            Website = "website",
            Cert = "cert",
            Extension1 = "extension1"
        };
        var scheduleFormReply = new ScheduleFormReply
        {
            Id = id,
            GameProviderId = Guid.NewGuid(),
            GameProviderCode = gameProviderCode,
            Name = "test",
            Subject = "gp.gamerecord.scheduleform.365TF.queued",
            Source = ScheduleFormSource.Schedule,
            Parameters = JsonSerializer.Serialize(parameter),
            State = ScheduleFormState.Processing,
            Status = $"{ScheduleFormState.Processing}..."
        };
        _ = fakeClient
            .ScheduleFormGetAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(scheduleFormReply);

        var dispatcher = new ScheduleFormDispatcher(
            fakeLogger,
            fakeClient,
            fakeScheduleFormLock,
            fakeSchedulePullRecord,
            fakeManualPullRecord);

        await dispatcher
            .ExecuteAsync(
                 gameProviderCode: gameProviderCode,
                id: id)
            .ConfigureAwait(false);

        _ = await fakeScheduleFormLock
            .Received(0)
            .LockingAsync(
                id: Arg.Is(id))
            .ConfigureAwait(false);

        var scheduleCommand = new SchedulePullRecordCommand(
            ApiUrl: new Uri(parameter.ApiUrl),
            Website: parameter.Website,
            Cert: parameter.Cert,
            Extension1: parameter.Extension1);

        await fakeSchedulePullRecord
            .Received(0)
            .ExecuteAsync(
                command: Arg.Is(scheduleCommand),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        var manualCommand = new ManualPullRecordCommand(
            ApiUrl: new Uri(parameter.ApiUrl),
            Website: parameter.Website,
            Cert: parameter.Cert,
            Extension1: parameter.Extension1,
            StartTime: Arg.Any<DateTimeOffset>(),
            EndTime: Arg.Any<DateTimeOffset>());

        await fakeManualPullRecord
            .Received(0)
            .ExecuteAsync(
                command: Arg.Is(manualCommand),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        await fakeClient
            .Received(0)
            .ScheduleFormProcessingAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        await fakeClient
            .Received(0)
            .ScheduleFormCompleteAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        _ = await fakeScheduleFormLock
            .Received(0)
            .UnlockAsync(
                id: Arg.Is(id))
            .ConfigureAwait(false);
    }
}