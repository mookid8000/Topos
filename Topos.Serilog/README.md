# Topos.Serilog

Provides a Serilog-based logger factory implementation for Topos.

Configure it by going

```csharp
using var producer = Configure
    .Producer(...)
    .Logging(s => s.UseSerilog())
    .(...)
    .Create();
```

or

```csharp
using var consumer = Configure
    .Consumer(...)
    .Logging(s => s.UseSerilog())
    .(...)
    .Start();
```

