using System.Collections.ObjectModel;

namespace DCA.DotNet.Extensions.BackgroundTask;

public interface IBackgroundTaskDispatcher
{
    ValueTask DispatchAsync(BackgroundWorkItem workItem);
    ValueTask DispatchAsync(string name, BackgroundWorkItem workItem);
}

public class BackgroundTaskDispatcher : IBackgroundTaskDispatcher
{
    private readonly ReadOnlyDictionary<string, IBackgroundTaskQueue> _queues;

    public BackgroundTaskDispatcher(ReadOnlyDictionary<string, IBackgroundTaskQueue> queues)
    {
        _queues = queues;
    }

    public async ValueTask DispatchAsync(BackgroundWorkItem workItem)
    {
        var t = DispatchAsync(Constants.DefaultQueueName, workItem);
        if (!t.IsCompletedSuccessfully)
        {
            await t;
        }
        Metrics.CounterDispathedWorkItems.Add(1);
    }

    public ValueTask DispatchAsync(string name, BackgroundWorkItem workItem)
    {
        var queue = _queues[name];
        return queue.QueueBackgroundWorkItemAsync(workItem);
    }
}
