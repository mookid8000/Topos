using System;
using Topos.Config;

namespace Topos.Routing;

public class SubscriptionsRegistrar(Topics topics)
{
    public SubscriptionsRegistrar Subscribe(string topic)
    {
        topics.AddRange([topic]);
        return this;
    }
}