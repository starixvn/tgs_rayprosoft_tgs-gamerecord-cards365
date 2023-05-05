using Tgs.GameInterface.Cards365.Protos;
using Tgs.GameRecord.Cards365.Core.Models;

namespace Tgs.GameRecord.Cards365.Core;

public interface IGameRecordUtensil
{
    public ValueTask<RecordInfo> SettlementConvertRecordAsync(
        GameRecordInfo source,
        CancellationToken cancellationToken = default);
}