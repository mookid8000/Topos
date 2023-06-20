# Topos.SystemTextJson

Provides a System.Text.Json-based message serializer implementation for Topos.

Configure it by going

```csharp
using var producer = Configure
    .(...)
    .Serialization(s => s.UseSystemTextJson())
    .Create();
```

or

```csharp
using var consumer = Configure
    .(...)
    .Serialization(s => s.UseSystemTextJson())
    .Start();
```

