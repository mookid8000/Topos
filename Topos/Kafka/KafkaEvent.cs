using System.Collections.Generic;

namespace Topos.Kafka
{
    public class KafkaEvent
    {
        static readonly Dictionary<string, string> NoHeaders = new Dictionary<string, string>();

        public Dictionary<string, string> Headers { get; }
        public string Key { get; }
        public string Body { get; }

        public KafkaEvent(string key, string body, Dictionary<string, string> headers = null)
        {
            Headers = headers ?? NoHeaders;
            Key = key;
            Body = body;
        }
    }
}