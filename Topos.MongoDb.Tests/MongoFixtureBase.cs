using MongoDB.Driver;
using Topos.Tests;

namespace Topos.MongoDb.Tests
{
    public abstract class MongoFixtureBase : ToposFixtureBase
    {
        protected IMongoDatabase GetCleanTestDatabase() => MongoTestConfig.GetCleanTestDatabase();
    }
}