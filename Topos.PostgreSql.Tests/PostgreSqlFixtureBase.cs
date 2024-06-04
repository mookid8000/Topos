using Topos.Tests;

namespace Topos.PostgreSql.Tests;

public abstract class PostgreSqlFixtureBase : ToposFixtureBase
{
    protected string ConnectionString => PostgreSqlTestConfig.ConnectionString;
}