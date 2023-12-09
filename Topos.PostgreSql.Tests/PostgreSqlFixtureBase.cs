using Topos.Tests;

namespace Topos.PostgreSql.Tests;

public abstract class PostgreSqlFixtureBase : ToposFixtureBase
{
    const string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=postgres";

    protected string ConnectionString => connectionString;

    protected void CleanDatabase() => PostgreSqlTestConfig.CleanDatabase(connectionString);
}