namespace Sample.Console;

using System.Diagnostics;
using Contracts;
using MassTransit;
using Microsoft.Extensions.Options;


public class DispatchRequestWorker :
    BackgroundService
{
    readonly ILogger<DispatchRequestWorker> _logger;
    readonly ConsoleOptions _options;
    readonly IServiceScopeFactory _scopeFactory;

    public DispatchRequestWorker(ILogger<DispatchRequestWorker> logger, IServiceScopeFactory scopeFactory, IOptions<ConsoleOptions> options)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var waitTime = _options.Wait ?? 1000;
        var delayTime = _options.Delay ?? 1000;
        var clientCount = _options.Clients ?? 1;

        var scope = _scopeFactory.CreateScope();
        try
        {
            var requestClient = scope.ServiceProvider.GetRequiredService<IRequestClient<DispatchRequest>>();
            var responseClient = scope.ServiceProvider.GetRequiredService<IRequestClient<DispatchResponse>>();

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.WhenAll(Enumerable.Range(0, clientCount).Select(async _ =>
                {
                    var dispatchRequest = new DispatchRequest
                    {
                        RequestTimestamp = DateTime.UtcNow,
                        Body = "HELLO AGAIN",
                        TransactionId = NewId.NextGuid().ToString(),
                        RoutingKey = "FIRSTNATL"
                    };

                    var requestTimer = Stopwatch.StartNew();

                    Response<DispatchRequestCompleted> response = await requestClient.GetResponse<DispatchRequestCompleted>(
                        dispatchRequest, stoppingToken);

                    requestTimer.Stop();

                    await Task.Delay(waitTime, stoppingToken);

                    var responseTimer = Stopwatch.StartNew();

                    Response<DispatchResponseCompleted> responseResponse = await responseClient.GetResponse<DispatchResponseCompleted>(new DispatchResponse
                    {
                        TransactionId = dispatchRequest.TransactionId,
                        Body = "HELLO RESPONSE",
                        ResponseTimestamp = DateTime.UtcNow,
                    }, stoppingToken);

                    responseTimer.Stop();

                    _logger.LogInformation("Response received: {Id} {Request}/{Response}", response.Message.TransactionId, requestTimer.ElapsedMilliseconds,
                        responseTimer.ElapsedMilliseconds);
                }));

                await Task.Delay(delayTime, stoppingToken);
            }
        }
        finally
        {
            if (scope is IAsyncDisposable disposable)
                await disposable.DisposeAsync();
            else
                scope.Dispose();
        }
    }
}
