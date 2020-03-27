using System.Collections.Generic;
using System.Linq;
using Confluent.Kafka;
using Topos.Consumer;
// ReSharper disable ArgumentsStyleNamedExpression
// ReSharper disable ArgumentsStyleOther

namespace Topos.Internals
{
    static class Extensions
    {
        public static IEnumerable<IReadOnlyCollection<T>> Batch<T>(this IEnumerable<T> items, int batchSize)
        {
            var list = new List<T>(batchSize);

            foreach (var item in items)
            {
                list.Add(item);

                if (list.Count < batchSize) continue;

                yield return list;

                list = new List<T>(batchSize);
            }

            if (list.Any())
            {
                yield return list.ToArray();
            }
        }

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