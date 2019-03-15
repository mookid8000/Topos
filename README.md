# Topos

It's something with topics.

## Producing messages

Could e.g. be Apache Kafka, where we send a JSON-serialized message:
```csharp
var producer = Configure
    .Producer(c => c.UseKafka("kafkahost01:9092", "kafkahost02:9092))
    .Serialization(s => s.UseNewtonsoftJson())
    .Topics(m => m.Map<SomeEvent>("someevents"))
    .Create();

// keep producer instance for the entire life of your app,
// remembering to dispose it when we shut down
Using(producer);

// send events like this:;
await producer.Send(new SomeEvent("This is just a message"), partitionKey: "customer-004");
```

Let's go through the different configuration parts:
```csharp
// Topos configurations start with 'Configure.', no matter what you want to configure
var producer = Configure

	// we configure a producer that uses Kafka, seeding it with a couple of brokers
    .Producer(c => c.UseKafka("kafkahost01:9092", "kafkahost02:9092"))

	// tell Topos to JSON-serialize messages
    .Serialization(s => s.UseNewtonsoftJson())

	// map .NET types of type SomeEvent to the 'someevents' topic
    .Topics(m => m.Map<SomeEvent>("someevents"))

	// creates the producer
    .Create();
```

## Consuming messages

Let's also use Kafka to consume messages... the configuration is probably not that surprising to you, it's
just `Configure.` and then let the fluent API guide you.

Check this out - here we set up a corresponding consumer that just prints out the contents from the received messages:
```csharp
var consumer = Configure
    .Consumer("default-group", c => c.UseKafka("kafkahost01:9092", "kafkahost02:9092"))
    .Serialization(s => s.UseNewtonsoftJson())
    .Subscribe("someevents")
    .Handle(async (messages, token) =>
    {
        foreach (var message in messages)
        {
            switch (message.Body)
            {
                case SomeEvent someEvent:
                    Console.WriteLine($"Got some event: {someEvent}");
                    break;
            }
        }
    })
    .Start();

// dispose consumer when you want to stop consuming messages
Using(consumer);
```

Let's go through the configuration again:
```csharp
// start with 'Configure.'...
var consumer = Configure

	// configure a consumer instance as part of the group 'default-group', and use Kafka
    .Consumer("default-group", c => c.UseKafka("kafkahost01:9092", "kafkahost02:9092"))

	// use JSON
    .Serialization(s => s.UseNewtonsoftJson())

	// subscribe to 'someevents'
    .Subscribe("someevents")

	// handle messages
    .Handle(async (messages, token) =>
    {
        foreach (var message in messages)
        {
            switch (message.Body)
            {
                case SomeEvent someEvent:
                    Console.WriteLine($"Got some event: {someEvent}");
                    break;
            }
        }
    })
    .Start();
```