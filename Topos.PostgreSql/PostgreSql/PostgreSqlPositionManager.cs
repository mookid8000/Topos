using System;
using System.Threading.Tasks;
using Topos.Consumer;
using Npgsql;
using System.Collections.Generic;

namespace Topos.PostgreSql
{
    public class PostgreSqlPositionManager : IPositionManager
    {
        private readonly string _connectionString;
        private readonly string _consumerGroup;

        public PostgreSqlPositionManager(
            string connectionString,
            string consumerGroup)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException($"{nameof(connectionString)} cannot be null or empty.");
            if (string.IsNullOrEmpty(consumerGroup))
                throw new ArgumentException($"{nameof(consumerGroup)} cannot be null or empty.");

            _connectionString = connectionString;
            _consumerGroup = consumerGroup;

            if (!SchemaExist().Result)
                InitSchema().Wait();
        }

        public async Task Set(Position position)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
              INSERT INTO topos.kafka_position (
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
                  topos.kafka_position.consumer_group = @consumer_group AND
                  topos.kafka_position.topic = @topic AND
                  topos.kafka_position.partition = @partition
            ";

            using var cmd = new NpgsqlCommand(query, connection);

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
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var getPositionQuery = @"
              SELECT position
              FROM topos.kafka_position
              WHERE
                    consumer_group = @consumer_group AND
                    topic = @topic AND
                    partition = @partition
            ";

            using var cmd = new NpgsqlCommand(getPositionQuery, connection);

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

        private async Task InitSchema()
        {
            var schemaSetup = @"
                    CREATE SCHEMA topos;
                    CREATE TABLE topos.kafka_position (
                        consumer_group varchar(255),
                        topic varchar(255),
                        partition integer,
                        position bigint,
                        PRIMARY KEY(consumer_group, topic, partition)
                     );";

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();

            using var cmd = new NpgsqlCommand(schemaSetup, connection, transaction);

            var result = await cmd.ExecuteNonQueryAsync();
            if (result == 0)
            {
                await transaction.RollbackAsync();
                throw new Exception("Failed setting up schema and table");
            }
            else
            {
                await transaction.CommitAsync();
            }
        }

        private async Task<bool> SchemaExist()
        {
            var schemaExistsQuery =
                "SELECT schema_name FROM information_schema.schemata WHERE schema_name = 'topos'";

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new NpgsqlCommand(schemaExistsQuery, connection);

            var result = await cmd.ExecuteScalarAsync();

            return result is not null;
        }
    }
}
