using System;
using NUnit.Framework;
using Testcontainers.PostgreSql;
using Testy.Files;
using Testy.General;
using Topos.Helpers;

namespace Topos.PostgreSql.Tests;

public class PostgreSqlTestConfig
{
    static readonly Disposables disposables = new();

    static readonly Lazy<PostgreSqlContainer> PostgresContainer = new(() =>
    {
        var temporaryTestDirectory = new TemporaryTestDirectory();

        disposables.Add(temporaryTestDirectory);

        var postgres = new PostgreSqlBuilder().Build();

        postgres.StartAsync().GetAwaiter().GetResult();

        disposables.Add(new DisposableCallback(() => postgres.StopAsync().GetAwaiter().GetResult()));

        return postgres;
    });

    public static string ConnectionString => PostgresContainer.Value.GetConnectionString();

    [OneTimeTearDown]
    public void CleanUp() => disposables.Dispose();
}