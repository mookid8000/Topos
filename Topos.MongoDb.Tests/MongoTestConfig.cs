using System;
using MongoDB.Driver;
using NUnit.Framework;
using Testcontainers.MongoDb;
using Testy.Files;
using Testy.General;
using Topos.Helpers;

namespace Topos.MongoDb.Tests;

public class MongoTestConfig
{
    static readonly Disposables disposables = new();

    static readonly Lazy<MongoDbContainer> MongoDbContainer = new(() =>
    {
        var temporaryTestDirectory = new TemporaryTestDirectory();

        disposables.Add(temporaryTestDirectory);

        var mongo = new MongoDbBuilder().Build();

        mongo.StartAsync().GetAwaiter().GetResult();

        disposables.Add(new DisposableCallback(() => mongo.StopAsync().GetAwaiter().GetResult()));

        return mongo;
    });

    [OneTimeTearDown]
    public void CleanUp() => disposables.Dispose();

    public static IMongoDatabase GetCleanTestDatabase()
    {
        var databaseName = Guid.NewGuid().ToString("n");
        var mongoClient = new MongoClient(MongoDbContainer.Value.GetConnectionString());

        mongoClient.DropDatabase(databaseName);

        return mongoClient.GetDatabase(databaseName);
    }
}