# Topos.Kafka

Provides a Kafka-based broker implementation for Topos.

Configure it by going

```csharp
using var producer = Configure
    .Producer(c => c.UseKafka("kafkahost01:9092"))
    .(...)
    .Create();
```

or

```csharp
using var consumer = Configure
    .Consumer("default-group", c => c.UseKafka("kafkahost01:9092"))
    .(...)
    .Start();
```

