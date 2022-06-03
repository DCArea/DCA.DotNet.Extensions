using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DCA.DotNet.Extensions.BackgroundTask;

public class BackgroundTaskWorkerPool : IHostedService
{
    private readonly ReadOnlyCollection<IBackgroundTaskQueue> _queues;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BackgroundTaskWorkerPool> _logger;
    private readonly BackgroundTaskWorkerOptions _options;
    private readonly List<Task> _workers = new();

    public BackgroundTaskWorkerPool(
        IServiceProvider serviceProvider,
        ILogger<BackgroundTaskWorkerPool> logger,
        IOptions<BackgroudTaskOptions> options,
        ReadOnlyDictionary<string, IBackgroundTaskQueue> queues)
    {
        _queues = queues.Values.OrderBy(x => x.Priority).ToList().AsReadOnly();
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value.Worker;

    }
    private CancellationTokenSource? _stoppingCts;
    private Task? _executeTask;
    public Task? ExecuteTask => _executeTask;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _logger.LogInformation("Starting workers.");

        for (int i = 0; i < _options.WorkerCount; i++)
            _workers.Add(BackgroundProcessing(_stoppingCts.Token));
        _executeTask = Task.WhenAll(_workers);

        if (_executeTask.IsCompleted)
        {
            return _executeTask;
        }

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping workers.");
        if (_executeTask == null)
        {
            return;
        }

        try
        {
            _stoppingCts!.Cancel();
        }
        finally
        {
            await Task.WhenAny(_executeTask, Task.Delay(Timeout.Infinite, cancellationToken)).ConfigureAwait(false);
        }
        _logger.LogInformation("Stopped workers.");
    }

    private async Task BackgroundProcessing(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var workItem = GetWorkItem();
            if (workItem is null)
            {
                workItem = await WaitWorkItemAsync(stoppingToken).ConfigureAwait(false);
            }
            if (workItem is null)
            {
                break;
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                try
                {
                    var task = workItem(scope.ServiceProvider, stoppingToken);
                    if (!task.IsCompletedSuccessfully)
                    {
                        await task.ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error occurred executing {WorkItem}.", nameof(workItem));
                }
                finally
                {
                    Metrics.CounterProcessedWorkItems.Add(1);
                }
            }
        }
    }

    private BackgroundWorkItem? GetWorkItem()
    {
        foreach (var queue in _queues)
        {
            if (queue.Reader.TryRead(out BackgroundWorkItem? workItem))
            {
                return workItem;
            }
        }
        return default;
    }

    private async Task<BackgroundWorkItem?> WaitWorkItemAsync(CancellationToken cancellationToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(2));
        while (!cancellationToken.IsCancellationRequested && await timer.WaitForNextTickAsync(default))
        {
            var workItem = GetWorkItem();
            if (workItem is not null) return workItem;
        }
        return default;
    }
}
