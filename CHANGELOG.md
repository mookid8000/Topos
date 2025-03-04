﻿# Changelog

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

## 0.6.0
* Insert pause before re-initializing Kafka consumer due to revocation

## 0.7.0
* Update FASTER to 2.0.16
* Update Azure Blobs to 12.13.1

## 0.8.0
* Make pause before re-initializing Kafka consumer due to revocation configurable

## 0.9.0
* Log Kafka consumer group name on assignment/revocation
* Change logging API to avoid allocations

## 0.10.0
* Add Azure storage-based device to FASTER-based event store, enabling the use of Azure page blobs as an in-process event store

## 0.11.0
* Update lots of dependencies

## 0.12.0
* Update lots of dependencies

## 0.13.0
* Log some singleton management info

## 0.14.0
* Fix minor things

## 0.15.0
* Try experimental read-only flag on Azure log device

## 0.16.0
* Remove read-only flag again

## 0.17.0
* Add logging adapter to more FASTER things

## 0.18.0
* Finally FIX Azure blobs-based FASTER event broker implementation by adding a blobs-based checkpoint manager

## 0.19.0
* Remove directory name option from Azure Blobs-based FASTER event broker and use sanitized topic name as directory instead

## 0.20.0
* Tweak various things to cater for producer/consumer startup ordering problems, singleton pooling and reference counting, etc.

## 0.21.0
* Update Confluent.Kafka to 2.1.1
* Update Microsoft.FASTER.Core to 2.5.3
* Update Microsoft.FASTER.Devices.AzureStorage to 2.5.3
* Add better error reporting in FASTER compaction task

## 0.22.0
* Update Microsoft.FASTER.Core to 2.5.11
* Update Microsoft.FASTER.Devices.AzureStorage to 2.5.11
* Update MongoDB.Driver to 2.19.2
* Update Serilog to 3.0.0

## 0.24.0
* Add package readmes
 
## 0.25.0
* Update dependencies

## 0.26.0
* Update dependencies

## 0.27.0
* Target .NET 6, 7, and 8 explicitly in addition to .NET Standard 2.0.

## 0.28.0
* LOGO!

## 0.29.0
* Update Confluent.Kafka to 2.3.0
* Update MongoDb.Driver to 2.24.0
* Update Microsoft.FASTER.Core to 2.6.3
* Update Microsoft.FASTER.Devices.AzureStorage to 2.6.3
* Update Npgsql to 8.0.2
* Update System.Text.Json to 8.0.3

## 0.30.0
* Update deps

## 0.31.0
* Update deps

## 0.32.0
* Update deps

## 0.33.0
* Update deps

## 0.34.0
* Update deps

## 0.35.0
* Remove .NET Standard 2.0 as target framework for Topos.MongoDb

## 0.36.0
* Update deps

## 0.37.0
* Fix compaction when using the Azure Blobs-based FASTER storage device

## 0.38.0
* .NET 9 as compile target and updated dependencies throughout

## 0.39.0
* Update all the deps

---

[runeanielsen]: https://github.com/runeanielsen
