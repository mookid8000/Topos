using System.Threading.Tasks;
using NUnit.Framework;

namespace Topos.Kafka.Tests
{
    [TestFixture]
    public class ProdConCatchUpTest : KafkaFixtureBase
    {
        string _topic;

        protected override void SetUp()
        {
            _topic = GetNewTopic();
        }

        [Test]
        public async Task ThuisMustWork()
        {
            
        }
    }
}