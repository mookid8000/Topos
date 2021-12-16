using Topos.Tests;

namespace Topos.PostgreSql.Tests;

public abstract class PostgreSqlFixtureBase : ToposFixtureBase
{
    protected void CleanDatabase(string connectionString) => PostgreSqlTestConfig.CleanDatabase(connectionString);
}