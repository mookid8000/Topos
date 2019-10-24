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