# Topos.AzureBlobs

Provides an Azure Blobs-based positions manager implementation for Topos.

Configure it by going

```csharp
using var consumer = Configure
    .Consumer(...)
    .Positions(p => p.StoreInAzureBlobs(connectionString, "positions_container"))
    .Start();
```

