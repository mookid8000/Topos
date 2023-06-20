# Topos.Faster

Provides an Microsoft FASTER-based "broker" implementation for Topos.

Configure it by going

```csharp
using var producer = Configure
    .Producer(c => c.UseFileSystem(@"C:\data\my-event-data"))
    .(...)
    .Create();
```

or

```csharp
using var consumer = Configure
    .Consumer("default-group", c => c.UseFileSystem(@"C:\data\my-event-data"))
    .(...)
    .Start();
```

