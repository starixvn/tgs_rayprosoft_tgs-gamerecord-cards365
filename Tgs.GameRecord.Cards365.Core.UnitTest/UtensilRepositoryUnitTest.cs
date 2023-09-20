using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.GameCommand.Client;

namespace Tgs.GameRecord.Cards365.Core.UnitTest;

public class UtensilRepositoryUnitTest
{
    [Fact]
    public async Task Get_GameProvider_Id()
    {
        var gameRecordInfo = Options.Create(new GameRecordInfoOptions
        {
            GameProviderCode = "365TF",
            LockMinute = 10
        });
        var fakeClient = Substitute.For<IGameCommandClient>();
        var fakeMemoryCache = Substitute.For<IMemoryCache>();

        var key = $"gamerecord:{gameRecordInfo.Value.GameProviderCode}";

        _ = fakeMemoryCache
            .TryGetValue(
                key: Arg.Is(key),
                value: out _)
             .Returns(false);

        var gameProviderId = Guid.NewGuid();
        _ = fakeClient
            .GameProviderGetGameProviderIdAsync(
                gameProviderCode: Arg.Is(gameRecordInfo.Value.GameProviderCode),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(gameProviderId);

        var repository = new UtensilRepository(
            gameRecordInfo,
            fakeClient,
            fakeMemoryCache);

        var actual = await repository
            .GetGameProviderIdAsync();

        Assert.Equal(gameProviderId, actual);

        _ = fakeMemoryCache
            .Received(1)
            .Set(
                key: key,
                gameProviderId);
    }
}