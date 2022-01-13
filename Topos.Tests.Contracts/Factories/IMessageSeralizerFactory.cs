using Topos.Serialization;

namespace Topos.Tests.Contracts.Factories;

public interface IMessageSeralizerFactory
{
    IMessageSerializer Create();
}