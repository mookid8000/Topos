using System;
using Topos.Consumer;
using Topos.PostgreSql;

namespace Topos.Config;

public static class PostgreSqlPositionManagerConfigurationExtension
{
    public static void StoreInPostgreSql(
        this StandardConfigurer<IPositionManager> configurer,
        string connectionString,
        string consumerGroup)
    {
        if (configurer is null)
            throw new ArgumentNullException(nameof(configurer));
        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentException($"{nameof(connectionString)} cannot be null or empty.");
        if (string.IsNullOrEmpty(consumerGroup))
            throw new ArgumentException($"{nameof(consumerGroup)} cannot be null or empty.");

        var registrar = StandardConfigurer.Open(configurer);
        registrar.Register(c => new PostgreSqlPositionManager(connectionString, consumerGroup));
    }
}