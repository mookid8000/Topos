using Confluent.Kafka;
using Topos.Consumer;

namespace Topos.Internals
{
    static class Extensions
    {
        public static TopicPartitionOffset ToTopicPartitionOffset(this Position position)
        {
            return new TopicPartitionOffset(
                topic: position.Topic,
                partition: new Partition(position.Partition),
                offset: position.IsDefault
                    ? Offset.Beginning
                    : new Offset(position.Offset)
            );
        }

        public static TopicPartitionOffset WithOffset(this TopicPartition topicPartition, Offset offset) => new TopicPartitionOffset(topicPartition, offset);
    }
}