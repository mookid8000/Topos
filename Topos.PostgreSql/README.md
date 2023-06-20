# Topos.PostgreSql

Provides a PostgreSQL-based positions manager implementation for Topos.

Configure it by going

```csharp
using var consumer = Configure
    .Consumer(...)
    .Positions(p => p.StoreInPostgreSql(connectionString, "consumerGroupName"))
    .Start();
```

