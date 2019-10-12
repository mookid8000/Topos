using System;
using Topos.Consumer;
using Topos.FileSystem;

namespace Topos.Config
{
    public static class FileSystemConfigurationExtensions
    {
        public static void StoreInFileSystem(this StandardConfigurer<IPositionManager> configurer, string directoryPath)
        {
            if (configurer == null) throw new ArgumentNullException(nameof(configurer));
            if (directoryPath == null) throw new ArgumentNullException(nameof(directoryPath));

            var registrar = StandardConfigurer.Open(configurer);

            registrar.Register(c => new FileSystemPositionsManager(directoryPath));
        }
    }
}