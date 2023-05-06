using Tgs.GameRecordSchedules.Protos;

namespace Tgs.GameRecord.Cards365.Core;

public interface IGameRecordScheduleService
{
    ValueTask<GameRecordScheduleReply?> GetAsync(
       string extension1,
       CancellationToken cancellationToken = default);

    ScheduleConfig GetConfig(GameRecordScheduleReply gameRecordSchedule);

    ValueTask<GameRecordScheduleReply?> InitAsync(
      Guid gameProviderId,
      string extension1,
      CancellationToken cancellationToken = default);

    ValueTask<GameRecordScheduleReply?> UpdateAsync(
      GameRecordScheduleReply gameRecordSchedule,
      string extension1,
      DateTimeOffset nextTime,
      string nextExternalRoundId,
      CancellationToken cancellationToken = default);
}