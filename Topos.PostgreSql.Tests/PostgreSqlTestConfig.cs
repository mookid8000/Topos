using Npgsql;

namespace Topos.PostgreSql.Tests;

public static class PostgreSqlTestConfig
{
    public static void CleanDatabase(string connectionString)
    {
        using var connection = new NpgsqlConnection(connectionString);

        connection.Open();

        var clean = "DROP TABLE IF EXISTS topos.position_manager; DROP SCHEMA IF EXISTS topos;";

        using var cmd = new NpgsqlCommand(clean, connection);

        cmd.ExecuteNonQuery();
    }
}