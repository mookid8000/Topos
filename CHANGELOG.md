# Changelog

## 0.0.33
* Confluent Kafka 1.2.0
* Kafkaesque-based broker implementation (for the file system)
* Ability to destructure objects, Serilog style
* Can use Azure Event Hubs connection string w. Kafka transport (automatically parses it & sets the correct parameters)
* Add hooks for partition assignment/revocation with Kafka

## 0.0.34
* Add file system-based positions storage (and a little suite of contract tests to verify consistent behavior across implementations)

## 0.0.35
* Configuration extension for Azure Blobs-based positions manager

## 0.0.36
* Update Confluent.Kafka to 1.2.1

## 0.0.37
* Correct Azure Blobs positions manager configuration extension's name

## 0.0.38
* Make MAX dispatch batch size configurable

## 0.0.39
* Make MIN dispatch batch size configurable too

## 0.0.40
* Better names for Azure Blobs positions manager

## 0.0.41
* Fix idiotic bug introduced when it was made possible to configure min/max events per dispatch batch

## 0.0.42
* Remove topic mapper - topics are now specified when events are sent
* Introduce batch API

## 0.0.43
* Log critical errors with higher level n stuff

## 0.0.45
* Remember Newtonsoft

## 0.0.46
* Update Kafka driver to 1.3.0
* Add configuration helper for connecting to Confluent Cloud

## 0.0.51
* React quicker on closed connections in consumer
* Update Mongo driver dep

## 0.0.52
* React even quicker on closed connections in consumer

## 0.0.54
* Set max idle time for the connections

## 0.0.55
* Disable the max idle time configuration again, because the C driver crashes the process!! 😫

## 0.0.56
* Update MongoDb driver dependency to 2.10.2
* Update Azure blobs driver dependency to 11.1.3
* Update Serilog dependency to 2.9.0
* Update Kafka dependency to 1.4.0-rc1

## 0.0.57
* Remove null check to allow for Kafka's tombstone messages to pass through - thanks [runeanielsen]

## 0.0.58
* Small adjustments + don't accept deserialization failures

## 0.0.61
* Add ability to skip serialization of messages, treating all payloads as `byte[]`

## 0.0.62
* Avoid logging `TaskCancelledException` when shutting down in the middle of a long-running message handler that supports cancellation just fine

## 0.0.69-pre
* Minor tweaks
* Confluent.Kafka 1.3.0 again because RC5 did not work

## 0.0.69
* Update Confluent.Kafka to 1.4.2

## 0.0.70
* Make it possible to initialize the consumer context before starting the consumer

## 0.0.71
* Fiddle with positions management during revocation in Kafka consumer

## 0.0.72
* Update Kafka driver to 1.4.4

## 0.0.73
* Update Kafka driver to 1.5.0

## 0.0.75
* Update Kafka driver to 1.5.1
* Update Azure Blobs driver to 11.2.2

## 0.0.76
* Use Kafkaesque's batch API when Topos' batch API is used

## 0.0.77
* Add FASTER log broker implementation

## 0.0.78
* Update some packages

## 0.0.79
* Small adjustments

## 0.0.80
* Add ability to do log truncation based on time

## 0.0.81
* Pretty wild optimization of Faster producer (introduce semaphore to avoid unnecessary waiting)

## 0.0.82
* Update Faster to 1.6.3

## 0.0.83
* Enable calling `SetInitialPosition` when configuring positions manager, specifying either `StartFromPosition.Beginning` or `StartFromPosition.Now` - enables consuming only new events

## 0.0.84
* Reduce waiting time in message handler loops by using a semaphore to signal that there's work to do

## 0.0.85
* Increase FASTER Log page size to 23 bits (= 8 MB)
* Update Microsoft.FASTER.Core dep to 1.7.4
* Update Confluent.Kafka dep to 1.5.2

## 0.0.86
* Update MongoDB.Driver to 2.11.4
* Update Protobuf-net to 3.0.52
* Update Polly to 7.2.1

## 0.0.87
* Open for adding Kafka consumer/producer customizers from the outside

## 0.0.88
* Add license info to NuGet package of main lib

## 0.0.89
* Update Confluent.Kafka to 1.5.3
* Update Microsoft.FASTER.Core to 1.8.0
* Update MongoDB.Driver to 2.11.6
* Update protobuf-net to 3.0.73

## 0.0.90
* Smaaaal adjustments

## 0.0.91
* Handle one particular exception
* Update Kafka driver to 1.6.2
* Update FASTER to 1.8.2

## 0.0.92
* Update MongoDB driver to 2.12.0
* Update FASTER to 1.8.3

## 0.0.93
* Add System.Text.Json serializer package

## 0.0.94
* Minor tweak aroud handling positions

## 0.0.95
* Add PostgreSQL-based position manager - thanks [runeanielsen]
* Update Kafka driver to 1.7.0/

## 0.0.96
* Update some packages
* Update FASTER to 1.8.4

## 0.0.97
* Minor tweaks around disposal of things

## 0.0.98
* Update Confluent.Kafka to 1.8.1

## 0.0.99
* Update Confluent.Kafka to 1.8.2
* Update Microsoft.FASTER.Core to 1.9.9
* Update MongoDb.Driver to 2.14.1
* Update npgsql to 6.0.1

## 0.1.0
* Dispatch received logical messages in `IReadOnlyList` because we might as well allow indexing

## 0.1.1
* Use Nito.AsyncEx instead of `SemaphoreSlim` in places

## 0.1.2
* Update Microsoft.FASTER.Core to 1.9.10
* Update npgsql to 6.0.2

## 0.1.3
* Stop logging that silly Local_AllBrokersDown all the time and narrow the waiting time before trying to reconnect

## 0.1.4
* Update Microsoft.FASTER.Core to 1.9.16
* Update MongoDB.Driver to 2.15.0
* Update other packages too

## 0.2.0
* Update FASTER to 2.0.1

## 0.3.0
* Update FASTER to 2.0.10
* Update Confluent.Kafka to 1.9.0

## 0.4.0
* Update FASTER to 2.0.12
* Update MongoDB driver to 2.16.1

## 0.5.0
* Try full reconnect when Kafka partitions are revoked
* Update Confluent.Kafka to 1.9.2
* Update FASTER to 2.0.14
* Update Npgsql to 6.0.6
* Port Topos.AzureBlobs to azure.storage.blobs instead of the deprecated Microsoft.Azure.Blob nuggie
* Update other packages too

---

[runeanielsen]: https://github.com/runeanielsen
