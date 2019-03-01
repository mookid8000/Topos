namespace Topos.EventProcessing
{
    public struct Position
    {
        public string Topic { get; }
        public int Partition { get; }
        public long Offset { get; }

        public Position(string topic, int partition, long offset)
        {
            Topic = topic;
            Partition = partition;
            Offset = offset;
        }

        public override string ToString() => $"{Topic}: {Partition}/{Offset}";
    }
}