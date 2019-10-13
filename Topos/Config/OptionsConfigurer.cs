using System;
using Topos.Consumer;

namespace Topos.Config
{
    public class OptionsConfigurer
    {
        readonly Options _options;

        internal OptionsConfigurer(Options options) => _options = options ?? throw new ArgumentNullException(nameof(options));

        public void SetMaximumBatchSize(int maximumBatchSize) => _options.Set(MessageHandler.MaximumBatchSizeOptionsKey, maximumBatchSize);
    }
}