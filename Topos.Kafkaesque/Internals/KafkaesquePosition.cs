namespace Topos.Internals
{
    struct KafkaesquePosition
    {
        public int FileNumber { get; }
        public int BytePosition { get; }

        public KafkaesquePosition(int fileNumber, int bytePosition)
        {
            FileNumber = fileNumber;
            BytePosition = bytePosition;
        }

        public override string ToString() => $"({FileNumber}, {BytePosition})";

        public void Deconstruct(out int fileNumber, out int bytePosition)
        {
            fileNumber = FileNumber;
            bytePosition = BytePosition;
        }
    }
}