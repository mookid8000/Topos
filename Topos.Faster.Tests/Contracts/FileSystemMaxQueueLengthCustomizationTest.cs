﻿using NUnit.Framework;
using Serilog;
using Topos.Tests.Contracts.Broker;

namespace Topos.Faster.Tests.Contracts;

[TestFixture]
public class FileSystemMaxQueueLengthCustomizationTest : MaxQueueLengthCustomizationTest<FasterLogBrokerFactory>
{
    public FileSystemMaxQueueLengthCustomizationTest()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();
    }
}