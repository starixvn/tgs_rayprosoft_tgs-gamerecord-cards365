using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Eventing.Reader;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Tgs.GameRecord.Cards365.Core;
using Tgs.GameRecord.Cards365.Core.Models;
using Tgs.GameRecord.Cards365.Repositories.Extensions;
using Tgs.GameRecord.Cards365.Repositories.Models;

namespace Tgs.GameRecord.Cards365.Repositories;

internal class GameRecordScheduleRepository(
	ILogger<GameRecordScheduleRepository> logger,
	IOptions<GameRecordInfoOptions> gameRecordInfoOptions,
	IMongoClient client) : IGameRecordScheduleRepository
{
	private readonly GameRecordInfoOptions _gameRecordInfoOptions = gameRecordInfoOptions.Value;

	public async ValueTask<GameRecordScheduleInfo> AddAsync(string website, string extension1, CancellationToken cancellationToken = default)
	{
		var collection = GetCollection();
		var document = new GameRecordSchedule()
		{
			Website = website,
			Extension1 = extension1,
			NextTime = DateTimeOffset.UnixEpoch.UtcDateTime,
			NextExternalBetId = string.Empty,
			CreatedAt = DateTimeOffset.UtcNow.UtcDateTime,
			UpdatedAt = DateTimeOffset.UtcNow.UtcDateTime
		};

		await collection.InsertOneAsync(
			document: document,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);

		return new GameRecordScheduleInfo(
			Website: document.Website,
			Extension1: document.Extension1,
			NextTime: document.NextTime,
			NextExternalBetId: document.NextExternalBetId,
			CreatedAt: document.CreatedAt,
			UpdatedAt: document.UpdatedAt);
	}

	public async ValueTask<GameRecordScheduleInfo?> GetAsync(string website, string extension1, CancellationToken cancellationToken = default)
	{
		var collection = GetCollection();
		var builder = Builders<GameRecordSchedule>.Filter;
		var filter = builder.Eq(x => x.Website, website)
			& builder.Eq(x => x.Extension1, extension1);

		var gameRecordSchedule = (await collection.FindAsync<GameRecordSchedule>(
			filter: filter,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false))
			.FirstOrDefault(cancellationToken: cancellationToken);

		return gameRecordSchedule is null
			? null
			: new GameRecordScheduleInfo(
				Website: gameRecordSchedule.Website,
				Extension1: gameRecordSchedule.Extension1,
				NextTime: gameRecordSchedule.NextTime,
				NextExternalBetId: gameRecordSchedule.NextExternalBetId,
				CreatedAt: gameRecordSchedule.CreatedAt,
				UpdatedAt: gameRecordSchedule.UpdatedAt);
	}

	public async ValueTask InitializationAsync(CancellationToken cancellationToken = default)
	{
		var collection = GetCollection();

		var tasks = new Task[]{
			Task.Run(async () =>
			{
				try
				{
					_ = await Index_UniqueAsync( collection: collection,   cancellationToken: cancellationToken).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "LogOn:{logOn} Message:This index creation failed.{index}",  DateTime.Now.ToString("u"),"Index_Unique");
				}
			}, cancellationToken: cancellationToken),
			Task.Run(async () =>
			{
				try
				{
					 _ = await Index_UpdatedAtAsync( collection: collection,   cancellationToken: cancellationToken).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "LogOn:{logOn} Message:This index creation failed.{index}",  DateTime.Now.ToString("u"),"Index_ExpireOn");
				}
			}, cancellationToken: cancellationToken)
		};

		await Task.WhenAll(tasks).ConfigureAwait(false);
	}

	public async ValueTask UpdateAsync(string website, string extension1, DateTimeOffset nextTime, string nextExternalBetId, CancellationToken cancellationToken = default)
	{
		var collection = GetCollection();
		var builder = Builders<GameRecordSchedule>.Filter;
		var filter = builder.Eq(x => x.Website, website)
			& builder.Eq(x => x.Extension1, extension1);

		var update = Builders<GameRecordSchedule>.Update
			.Set(s => s.NextTime, nextTime.UtcDateTime)
			.Set(s => s.NextExternalBetId, nextExternalBetId)
			.Set(s => s.UpdatedAt, DateTimeOffset.UtcNow.UtcDateTime);

		_ = await collection.UpdateOneAsync(
			filter: filter,
			update: update,
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
	}

	private static Task<string> Index_UpdatedAtAsync(IMongoCollection<GameRecordSchedule> collection, CancellationToken cancellationToken = default)
	{
		var index = Builders<GameRecordSchedule>.IndexKeys
			.Ascending(m => m.UpdatedAt);

		var createIndexOptions = new CreateIndexOptions
		{
			ExpireAfter = TimeSpan.FromDays(7),
			Name = "idx_updated_at"
		};
		var createIndexModel = new CreateIndexModel<GameRecordSchedule>(index, createIndexOptions);

		return collection.Indexes.CreateOneAsync(
			model: createIndexModel,
			cancellationToken: cancellationToken);
	}

	private static Task<string> Index_UniqueAsync(IMongoCollection<GameRecordSchedule> collection, CancellationToken cancellationToken = default)
	{
		var index = Builders<GameRecordSchedule>.IndexKeys
			.Ascending(m => m.Website)
			.Ascending(m => m.Extension1);

		var createIndexOptions = new CreateIndexOptions
		{
			Unique = true,
			Name = $"uni_{collection.CollectionNamespace.CollectionName}"
		};
		var createIndexModel = new CreateIndexModel<GameRecordSchedule>(index, createIndexOptions);

		return collection.Indexes.CreateOneAsync(
			model: createIndexModel,
			cancellationToken: cancellationToken);
	}

	private IMongoCollection<GameRecordSchedule> GetCollection()
		=> client
		.GetDatabase($"game_provider_{_gameRecordInfoOptions.GameProviderCode}_master".ToLower())
		.GetCollection<GameRecordSchedule>(typeof(GameRecordSchedule).GetTableName());
}
