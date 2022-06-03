using Microsoft.Extensions.DependencyInjection;

namespace DCA.DotNet.Extensions.BackgroundTask;

public static class BackgroundTaskDispatcherExtensions
{
    public static ValueTask DispatchAsync<TDependency>(
        this IBackgroundTaskDispatcher dispatcher,
        Func<TDependency, CancellationToken, ValueTask> workItem) where TDependency : notnull
    {
        return dispatcher.DispatchAsync(Constants.DefaultQueueName, workItem);
    }

    public static ValueTask DispatchAsync<TDependency>(
        this IBackgroundTaskDispatcher dispatcher,
        string name,
        Func<TDependency, CancellationToken, ValueTask> workItem) where TDependency : notnull
    {
        var workItem1 = (IServiceProvider sp, CancellationToken ct)
            => workItem(sp.GetRequiredService<TDependency>(), ct);
        return dispatcher.DispatchAsync(name, workItem1);
    }

    public static ValueTask DispatchAsync<TDependency>(
        this IBackgroundTaskDispatcher dispatcher,
        Func<TDependency, Task> workItem) where TDependency : notnull
    {
        return dispatcher.DispatchAsync(Constants.DefaultQueueName, workItem);
    }

    public static ValueTask DispatchAsync<TDependency>(
        this IBackgroundTaskDispatcher dispatcher,
        string name,
        Func<TDependency, Task> workItem) where TDependency : notnull
    {
        var workItem1 = (IServiceProvider sp, CancellationToken ct)
            => new ValueTask(workItem(sp.GetRequiredService<TDependency>()));
        return dispatcher.DispatchAsync(name, workItem1);
    }
}
