# Topos.MongoDb

Provides a MongoDB-based positions manager implementation for Topos.

Configure it by going

```csharp
using var consumer = Configure
    .Consumer(...)
    .Positions(p => p.StoreInMongoDb(database, "positionsCollection"))
    .Start();
```

