using FASTER.core;

namespace Topos.Faster;

/// <summary>
/// Abstracts away management of FasterLog logs
/// </summary>
public interface IDeviceManager
{
    /// <summary>
    /// Gets a WRITER log for the given topic
    /// </summary>
    FasterLog GetWriter(string topic);

    /// <summary>
    /// Gets a READER log for the given topic
    /// </summary>
    FasterLog GetReader(string topic);
}