using Topos.Internals;
using Topos.Logging;
using Topos.Logging.Console;
using Topos.Serialization;

namespace Topos.Config
{
    class ToposConfigurerHelpers
    {
        public static void RegisterCommonServices(Injectionist injectionist)
        {
            injectionist.PossiblyRegisterDefault<ILoggerFactory>(c => new ConsoleLoggerFactory(minimumLogLevel: LogLevel.Debug));
            injectionist.PossiblyRegisterDefault<IMessageSerializer>(c => new Utf8StringEncoder());
        }
    }
}