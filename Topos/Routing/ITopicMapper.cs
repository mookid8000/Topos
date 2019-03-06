namespace Topos.Routing
{
    public interface ITopicMapper
    {
        string GetTopic(object message);
    }
}