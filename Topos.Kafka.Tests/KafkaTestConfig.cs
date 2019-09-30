using System;
using System.IO;
using System.Linq;

namespace Topos.Kafka.Tests
{
    public static class KafkaTestConfig
    {
        static KafkaTestConfig()
        {
            var connectionStringFilePath = Path.Combine(AppContext.BaseDirectory, "connection_string.secret.txt");

            if (!File.Exists(connectionStringFilePath))
            {
                throw new FileNotFoundException($@"Could not locate connection string file here:

    {connectionStringFilePath}

Please create this file and add a Kafka connection string to it");
            }

            var firstLine = File.ReadAllLines(connectionStringFilePath).First();

            Address = firstLine;
        }

        public static string Address { get; }

        //public static string Address => "127.0.0.1:9092";
        //public static string Address => "10.200.236.139:9092";
        //public static string Address => "192.168.1.98:9092";
    }
}