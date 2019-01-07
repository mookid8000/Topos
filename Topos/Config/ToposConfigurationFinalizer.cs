namespace Topos.Config
{
    public static class ToposConfigurationFinalizer
    {
        public static IToposProducer Create(this ToposProducerConfigurer configurer)
        {
            var injectionist = StandardConfigurer<IToposProducer>.Open(configurer);

            var resolutionResult = injectionist.Get<IToposProducer>();

            return resolutionResult.Instance;
        }

        public static IToposConsumer Create(this ToposConsumerConfigurer configurer)
        {
            var injectionist = StandardConfigurer<IToposConsumer>.Open(configurer);

            var resolutionResult = injectionist.Get<IToposConsumer>();

            return resolutionResult.Instance;
        }
    }
}