using System;
using System.Threading.Tasks;
using Topos.Consumer;
using Npgsql;
using System.Collections.Generic;

namespace Topos.PostgreSql;

public class PostgreSqlPositionManager : IPositionManager
{
    readonly string _connectionString;
    readonly string _consumerGroup;

    public PostgreSqlPositionManager(string connectionString, string consumerGroup)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _consumerGroup = consumerGroup ?? throw new ArgumentNullException(nameof(consumerGroup));

        if (!SchemaExist().Result)
        {
            InitSchema().Wait();
        }
    }

    public async Task Set(Position position)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        await connection.OpenAsync();

        const string query = @"
              INSERT INTO topos.position_manager (
                consumer_group,
                topic,
                partition,
                position)
              VALUES (
                @consumer_group,
                @topic,
                @partition,
                @position)
              ON CONFLICT(consumer_group, topic, partition) DO
              UPDATE SET
                  position = @position
              WHERE
                  topos.position_manager.consumer_group = @consumer_group AND
                  topos.position_manager.topic = @topic AND
                  topos.position_manager.partition = @partition
            ";

        await using var cmd = new NpgsqlCommand(query, connection);

        cmd.Parameters.AddWithValue("@consumer_group", _consumerGroup);
        cmd.Parameters.AddWithValue("@topic", position.Topic);
        cmd.Parameters.AddWithValue("@partition", position.Partition);
        cmd.Parameters.AddWithValue("@position", position.Offset);

        var result = await cmd.ExecuteNonQueryAsync();

        if (result == 0)
        {
            throw new Exception($@"Consumergroup: '{_consumerGroup}' with topic: '{position.Topic}'
                                       and partition: '{position.Partition}' did not get updated.");
        }
    }

    public async Task<Position> Get(string topic, int partition)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        await connection.OpenAsync();

        var getPositionQuery = @"
              SELECT position
              FROM topos.position_manager
              WHERE
                    consumer_group = @consumer_group AND
                    topic = @topic AND
                    partition = @partition
            ";

        await using var cmd = new NpgsqlCommand(getPositionQuery, connection);

        cmd.Parameters.AddWithValue("@consumer_group", _consumerGroup);
        cmd.Parameters.AddWithValue("@topic", topic);
        cmd.Parameters.AddWithValue("@partition", partition);

        var result = await cmd.ExecuteScalarAsync();

        long position = default(long);
        if (result is not null)
            position = (long)result;

        return EqualityComparer<long>.Default.Equals(position, default(long))
            ? Position.Default(topic, partition)
            : new Position(topic, partition, position);
    }

    async Task InitSchema()
    {
        const string schemaSetup = @"
                    CREATE SCHEMA topos;
                    CREATE TABLE topos.position_manager (
                        consumer_group varchar(255),
                        topic varchar(255),
                        partition integer,
                        position bigint,
                        PRIMARY KEY(consumer_group, topic, partition)
                     );";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var transaction = await connection.BeginTransactionAsync();
        await using var cmd = new NpgsqlCommand(schemaSetup, connection, transaction);

        var result = await cmd.ExecuteNonQueryAsync();
        if (result == 0)
        {
            await transaction.RollbackAsync();
            throw new Exception("Failed setting up schema and table");
        }

        await transaction.CommitAsync();
    }

    async Task<bool> SchemaExist()
    {
        var schemaExistsQuery =
            "SELECT schema_name FROM information_schema.schemata WHERE schema_name = 'topos'";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var cmd = new NpgsqlCommand(schemaExistsQuery, connection);

        var result = await cmd.ExecuteScalarAsync();

        return result is not null;
    }
}