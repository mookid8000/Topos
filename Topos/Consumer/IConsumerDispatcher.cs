﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Topos.Serialization;

namespace Topos.Consumer;

public interface IConsumerDispatcher
{
    void Dispatch(ReceivedTransportMessage transportMessage);
    Task RevokeAsync(string topic, IEnumerable<int> partitions);
}