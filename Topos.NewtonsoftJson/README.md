# Topos.NewtonsoftJson

Provides a Newtonsoft JSON.NET-based message serializer implementation for Topos.

Configure it by going

```csharp
using var producer = Configure
    .(...)
    .Serialization(s => s.UseNewtonsoftJson())
    .Create();
```

or

```csharp
using var consumer = Configure
    .(...)
    .Serialization(s => s.UseNewtonsoftJson())
    .Start();
```

