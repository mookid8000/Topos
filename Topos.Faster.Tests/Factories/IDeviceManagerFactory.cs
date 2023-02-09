using System;

namespace Topos.Faster.Tests.Factories;

public interface IDeviceManagerFactory : IDisposable
{
    IDeviceManager Create();
}