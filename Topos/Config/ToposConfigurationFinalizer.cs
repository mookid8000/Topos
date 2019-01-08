using Topos.Internals;
using Topos.Internals.Consumer;
using Topos.Internals.Producer;

namespace Topos.Config
{
    public static class ToposConfigurationFinalizer
    {
        public static IToposProducer Create(this ToposProducerConfigurer configurer)
        {
            var injectionist = StandardConfigurer<IToposProducer>.Open(configurer);

            RegisterCommonServices(injectionist);

            injectionist.Register<IToposProducer>(c => new ToposProducer());

            var resolutionResult = injectionist.Get<IToposProducer>();

            return resolutionResult.Instance;
        }

        public static IToposConsumer Create(this ToposConsumerConfigurer configurer)
        {
            var injectionist = StandardConfigurer<IToposConsumer>.Open(configurer);

            RegisterCommonServices(injectionist);

            injectionist.Register<IToposConsumer>(c => new ToposConsumer());

            var resolutionResult = injectionist.Get<IToposConsumer>();

            return resolutionResult.Instance;
        }

        static void RegisterCommonServices(Injectionist injectionist)
        {
        }
    }
}