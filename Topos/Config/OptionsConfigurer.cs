using System;
using System.Collections.Generic;
using Topos.Consumer;

namespace Topos.Config
{
    public class OptionsConfigurer
    {
        readonly Options _options;

        internal OptionsConfigurer(Options options) => _options = options ?? throw new ArgumentNullException(nameof(options));

        /// <summary>
        /// Sets the MIN batch size to dispatch to the message handler. The default is <see cref="MessageHandler.DefaultMinimumBatchSize"/>, which
        /// means that the handler will be called as long as there's events to dispatch. If you set this value higher than that, then the message
        /// handler will wait and buffer events until the MIN batch size is reached.
        /// </summary>
        public void SetMinimumBatchSize(int minimumBatchSize) => _options.Set(MessageHandler.MinimumBatchSizeOptionsKey, minimumBatchSize);
        
        /// <summary>
        /// Sets the MAX batch size to dispatch to the message handler. Bigger batch sizes generally yield higher throughput
        /// at the expense of an increased memory consumption.
        /// The default MAX batch size is <see cref="MessageHandler.DefaultMaximumBatchSize"/>.
        /// </summary>
        public void SetMaximumBatchSize(int maximumBatchSize) => _options.Set(MessageHandler.MaximumBatchSizeOptionsKey, maximumBatchSize);

        /// <summary>
        /// Sets the MAX prefetch queue length.
        /// This is how many messages we tolerate accepting from the underlying driver, before we stop receiving any more messages.
        /// The default MAX prefetch queue length is <see cref="MessageHandler.DefaultMaxPrefetchQueueLength"/>.
        /// </summary>
        public void SetMaximumPrefetchQueueLength(int maximumPrefetchQueueLength) => _options.Set(MessageHandler.MaximumPrefetchQueueLengthOptionsKey, maximumPrefetchQueueLength);

        /// <summary>
        /// Adds a function to be called when the <see cref="ConsumerContext"/> is initialized, making it possible to inject dependencies into the consumer message handler
        /// </summary>
        public void AddContextInitializer(Action<ConsumerContext> customizer)
        {
            if (customizer == null) throw new ArgumentNullException(nameof(customizer));

            var customizers = _options.GetOrAdd(ConsumerContext.ConsumerContextInitializersKey, () => new List<Action<ConsumerContext>>());

            customizers.Add(customizer);
        }
    }
}