using FASTER.core;

namespace Topos.Faster
{
    /// <summary>
    /// Abstracts away management of FasterLog logs
    /// </summary>
    public interface IDeviceManager
    {
        /// <summary>
        /// Gets a log for the given topic
        /// </summary>
        FasterLog GetLog(string topic);
    }
}