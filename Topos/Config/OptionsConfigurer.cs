using System;
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
    }
}