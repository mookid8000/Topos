namespace Topos.Config
{
    public static class Configure
    {
        public static ToposProducerConfigurer Producer() => new ToposProducerConfigurer();
        
        public static ToposConsumerConfigurer Consumer() => new ToposConsumerConfigurer();
    }
}
