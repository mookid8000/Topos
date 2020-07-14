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

---

[runeanielsen]: https://github.com/runeanielsen