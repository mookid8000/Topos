using System;
using System.Threading;
using System.Threading.Tasks;
using Topos.Config;
using Topos.Consumer;
using Topos.Logging;
#pragma warning disable 1998

namespace Topos.InMem;

class InMemConsumerImplementation : IConsumerImplementation, IDisposable
{
    readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    readonly InMemEventBroker _eventBroker;
    readonly ILogger _logger;

    Task _worker;

    public InMemConsumerImplementation(InMemEventBroker eventBroker, ILoggerFactory loggerFactory, Topics topics,
        IConsumerDispatcher consumerDispatcher, ConsumerContext consumerContext)
    {
        if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
        _eventBroker = eventBroker ?? throw new ArgumentNullException(nameof(eventBroker));
        _logger = loggerFactory.GetLogger(typeof(InMemConsumerImplementation));
    }

    public void Start()
    {
        _logger.Info("Starting in-mem consumer");
        _worker = Task.Run(PumpEvents);
    }

    async Task PumpEvents()
    {
        var token = _cancellationTokenSource.Token;

        _logger.Info("In-mem consumer started");

        while (!token.IsCancellationRequested)
        {
            try
            {

            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                // it's ok
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Unhandled error in in-mem consumer");
            }
        }

        _logger.Info("In-mem consumer stopped");
    }

    public void Dispose()
    {
        _logger.Info("Stopping in-mem consumer");
        _cancellationTokenSource.Cancel();


    }
}