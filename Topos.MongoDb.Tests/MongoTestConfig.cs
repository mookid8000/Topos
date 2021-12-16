using System;
using MongoDB.Driver;

namespace Topos.MongoDb.Tests;

public static class MongoTestConfig
{
    public static MongoUrl MongoUrl => new("mongodb://localhost/topos-test");

    public static IMongoDatabase GetCleanTestDatabase()
    {
        var databaseName = MongoUrl.DatabaseName 
                           ?? throw new ArgumentException($"Can't use MongoDB connection string {MongoUrl}, because it doesn't contain a database name. Please provide one with a database name.");

        var mongoClient = new MongoClient(MongoUrl);

        mongoClient.DropDatabase(databaseName);
            
        var database = mongoClient.GetDatabase(databaseName);

        return database;
    }
}