# Changelog

## 0.0.32
* Confluent Kafka 1.2.0
* Kafkaesque-based broker implementation (for the file system)
* Ability to destructure objects, Serilog style
* Can use Azure Event Hubs connection string w. Kafka transport (automatically parses it & sets the correct parameters)
* Add hooks for partition assignment/revocation with Kafka