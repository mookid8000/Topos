﻿using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Topos.Consumer;

namespace Topos.MongoDb;

public class MongoDbPositionManager : IPositionManager
{
    readonly IMongoCollection<BsonDocument> _positions;

    public MongoDbPositionManager(IMongoDatabase database, string collectionName)
    {
        if (database == null) throw new ArgumentNullException(nameof(database));
        if (collectionName == null) throw new ArgumentNullException(nameof(collectionName));
        _positions = database.GetCollection<BsonDocument>(collectionName);
    }

    public async Task SetAsync(Position position)
    {
        var criteria = new BsonDocumentFilterDefinition<BsonDocument>(new BsonDocument
        {
            {"_id", position.Topic}
        });

        var update = new BsonDocumentUpdateDefinition<BsonDocument>(new BsonDocument
        {
            {"$set", new BsonDocument{{position.Partition.ToString(), position.Offset}}}
        });

        await _positions.UpdateOneAsync(criteria, update, new UpdateOptions { IsUpsert = true });
    }

    public async Task<Position> GetAsync(string topic, int partition)
    {
        var query = new BsonDocumentFilterDefinition<BsonDocument>(new BsonDocument
        {
            {"_id", topic}
        });

        var document = await _positions.Find(query).FirstOrDefaultAsync();
        if (document == null) return Position.Default(topic, partition);

        var fieldName = partition.ToString();

        return document.Contains(fieldName)
            ? new Position(topic, partition, document[fieldName].AsInt64)
            : Position.Default(topic, partition);
    }
}