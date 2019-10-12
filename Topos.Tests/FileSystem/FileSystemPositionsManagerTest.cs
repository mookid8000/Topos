using System;
using System.Collections.Concurrent;
using System.IO;
using NUnit.Framework;
using Topos.Consumer;
using Topos.FileSystem;
using Topos.Tests.Contracts.Factories;
using Topos.Tests.Contracts.Positions;
// ReSharper disable ArgumentsStyleLiteral

namespace Topos.Tests.FileSystem
{
    [TestFixture]
    public class FileSystemPositionsManagerTest : PositionsManagerTest<FileSystemPositionsManagerTest.FileSystemPositionsManagerFactory>
    {
        public class FileSystemPositionsManagerFactory : IPositionsManagerFactory
        {
            readonly ConcurrentBag<string> _directoriesToWipe = new ConcurrentBag<string>();

            public IPositionManager Create()
            {
                var path = Path.Combine(AppContext.BaseDirectory, Guid.NewGuid().ToString("N"));
                _directoriesToWipe.Add(path);
                return new FileSystemPositionsManager(path);
            }

            public void Dispose()
            {
                foreach (var path in _directoriesToWipe)
                {
                    Directory.Delete(path, recursive: true);
                }
            }
        }
    }
}