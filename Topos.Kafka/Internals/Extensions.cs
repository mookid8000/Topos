using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using Topos.Consumer;
// ReSharper disable ArgumentsStyleNamedExpression
// ReSharper disable ArgumentsStyleOther

namespace Topos.Internals
{
    static class Extensions
    {
        public static async Task<IReadOnlyList<T>> ToListAsync<T>(this IEnumerable<Task<T>> tasks)
        {
            var list = tasks.ToList();
            await Task.WhenAll(list);
            return list.Select(t => t.Result).ToList();
        }

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

        public static bool IsSpecial(this TopicPartitionOffset tpo) => tpo.Offset.IsSpecial;

        public static Position ToPosition(this TopicPartitionOffset tpo)
        {
            return tpo.IsSpecial()
                ? Position.Default(tpo.Topic, tpo.Partition.Value)
                : new Position(
                    topic: tpo.Topic,
                    partition: tpo.Partition.Value,
                    offset: tpo.Offset.Value
                );
        }

        public static TopicPartitionOffset WithOffset(this TopicPartition topicPartition, Offset offset) => new TopicPartitionOffset(topicPartition, offset);
    }
}