namespace Topos
{
    /// <summary>
    /// Implement this to create a consumer for a concrete transport.
    /// </summary>
    public interface IConsumerImplementation
    {
        /// <summary>
        /// Start the consumer
        /// </summary>
        void Start();
    }
}